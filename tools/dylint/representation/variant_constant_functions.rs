use core::{mem, slice};

use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks, res::MaybeQPath as _, visitors::is_const_evaluatable};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashSet, FxIndexMap};
use rustc_errors::listify;
use rustc_hir::{
    BinOpKind, Expr, ExprKind, PatKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::AdtDef;

use super::{fieldless_local_enum, variant_collection};
use crate::{expanded_externally, generated};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    functions: FxIndexMap<DefId, Vec<LocalDefId>>,
    reads: FxHashSet<DefId>,
}

lint!(default Pass; VARIANT_CONSTANT_FUNCTIONS: "functions that each match every variant of a fieldless local enum to one constant");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if generated(cx, expr.hir_id) {
            return;
        }
        match expr.kind {
            ExprKind::Match(scrutinee, arms, _) if let Some(keyed) = fieldless_local_enum(typeck.expr_ty_adjusted(scrutinee)) => {
                let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id);
                let tabled = !expanded_externally(cx, expr.span)
                    && matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                    && cx.tcx.trait_item_of(owner).is_none()
                    && peel_blocks(&cx.tcx.hir_body_owned_by(owner).value).hir_id == expr.hir_id
                    && !arms.iter().all(|arm| matches!(peel_blocks(arm.body).kind, ExprKind::Lit(literal) if matches!(literal.node, LitKind::Str(..))))
                    && arms.iter().all(|arm| {
                        arm.guard.is_none()
                            && is_const_evaluatable(cx.tcx, typeck, arm.body)
                            && if let PatKind::Or(alternatives) = arm.pat.kind { alternatives } else { slice::from_ref(arm.pat) }
                                .iter()
                                .all(|alternative| matches!(alternative.res(typeck), Res::Def(..)))
                    });
                if tabled {
                    self.functions.entry(keyed.did()).or_default().push(owner);
                } else {
                    self.reads.insert(keyed.did());
                }
            }
            ExprKind::Binary(operator, compared, _) if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) =>
                self.reads.extend(fieldless_local_enum(typeck.expr_ty_adjusted(compared)).map(AdtDef::did)),
            ExprKind::Let(binding) => self.reads.extend(fieldless_local_enum(typeck.expr_ty_adjusted(binding.init)).map(AdtDef::did)),
            _ => self.reads.extend(variant_collection(cx, expr).map(|(_, keyed)| keyed.did())),
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tables = mem::take(&mut self.functions)
            .into_iter()
            .filter(|(keyed, functions)| functions.len() > 1 && !expanded_externally(cx, cx.tcx.def_span(*keyed)))
            .filter_map(|(keyed, functions)| Some((keyed.as_local()?, listify(&functions, |&function| format!("`{}`", cx.tcx.item_name(function)))?, functions)));
        for (local, listed, functions) in tables {
            let name = cx.tcx.item_name(local);
            let help = if self.reads.contains(&local.to_def_id()) {
                format!("return the values of {listed} from one match on `{name}`")
            } else {
                format!("replace `{name}` with a struct holding {listed} and one `const` per variant")
            };
            span_lint_hir_and_then(
                cx,
                VARIANT_CONSTANT_FUNCTIONS,
                cx.tcx.local_def_id_to_hir_id(local),
                cx.tcx.def_span(local),
                format!("{listed} each match every `{name}` variant to one constant"),
                |diag| {
                    diag.span_note(functions.iter().map(|&function| cx.tcx.def_span(function)).collect::<Vec<_>>(), "matched here");
                    diag.help(help);
                },
            );
        }
    }
}
