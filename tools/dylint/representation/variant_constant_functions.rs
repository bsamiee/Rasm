use core::{mem, slice};

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::peel_blocks;
use clippy_utils::res::MaybeQPath as _;
use clippy_utils::visitors::is_const_evaluatable;
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxIndexMap;
use rustc_errors::listify;
use rustc_hir::def::{DefKind, Res};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::{Expr, ExprKind, PatKind};
use rustc_lint::{LateContext, LateLintPass};

use super::fieldless_local_enum;
use crate::{expanded_externally, generated};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    functions: FxIndexMap<DefId, Vec<LocalDefId>>,
}

lint!(default Pass; VARIANT_CONSTANT_FUNCTIONS: "functions that each match every variant of a fieldless local enum to one constant");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Match(scrutinee, arms, _) = expr.kind
            && let Some(keyed) = fieldless_local_enum(typeck.expr_ty_adjusted(scrutinee))
            && !generated(cx, expr.hir_id)
            && !expanded_externally(cx, expr.span)
            && let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id)
            && matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
            && cx.tcx.trait_item_of(owner).is_none()
            && peel_blocks(cx.tcx.hir_body_owned_by(owner).value).hir_id == expr.hir_id
            && !arms.iter().all(|arm| matches!(peel_blocks(arm.body).kind, ExprKind::Lit(literal) if matches!(literal.node, LitKind::Str(..))))
            && arms.iter().all(|arm| {
                arm.guard.is_none()
                    && is_const_evaluatable(cx.tcx, typeck, arm.body)
                    && if let PatKind::Or(alternatives) = arm.pat.kind { alternatives } else { slice::from_ref(arm.pat) }.iter().all(|alternative| matches!(alternative.res(typeck), Res::Def(..)))
            })
        {
            self.functions.entry(keyed.did()).or_default().push(owner);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tables = mem::take(&mut self.functions)
            .into_iter()
            .filter(|(keyed, functions)| functions.len() > 1 && !expanded_externally(cx, cx.tcx.def_span(*keyed)))
            .filter_map(|(keyed, functions)| Some((keyed.as_local()?, listify(&functions, |&function| format!("`{}`", cx.tcx.item_name(function)))?, functions)));
        for (local, listed, functions) in tables {
            let name = cx.tcx.item_name(local);
            span_lint_hir_and_then(
                cx,
                VARIANT_CONSTANT_FUNCTIONS,
                cx.tcx.local_def_id_to_hir_id(local),
                cx.tcx.def_span(local),
                format!("{listed} each match every `{name}` variant to one constant"),
                |diag| {
                    diag.span_note(functions.iter().map(|&function| cx.tcx.def_span(function)).collect::<Vec<_>>(), "matched here");
                    diag.help(format!("return the values of {listed} from one match on `{name}`"));
                },
            );
        }
    }
}
