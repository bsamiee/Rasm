use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, res::MaybeDef as _, source::snippet, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, StructTailExpr};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;

use crate::{derives, expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_DEFAULT_BASE: "`Default::default` struct bases that fill only fields with declared default values");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Struct(_, fields, StructTailExpr::Base(base)) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let ExprKind::Call(_, []) = base.kind
            && fn_def_id(cx, base).is_diag_item(cx, sym::default_fn)
            && let built = typeck.expr_ty(expr)
            && let ty::Adt(owner, _) = built.kind()
            && owner.is_struct()
            && let written = fields.iter().filter_map(|field| typeck.opt_field_index(field.hir_id)).collect::<FxHashSet<_>>()
            && let declared = &owner.non_enum_variant().fields
            && written.len() < declared.len()
            && declared.iter_enumerated().all(|(index, field)| written.contains(&index) || field.value.is_some())
        {
            let (name, derived) = (cx.tcx.item_name(owner.did()), derives(cx, built, sym::Default));
            let applicability = if derived { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect };
            span_lint_hir_and_then(
                cx,
                REDUNDANT_DEFAULT_BASE,
                expr.hir_id,
                base.span,
                format!("`..{}` fills only `{name}` fields with declared default values", snippet(cx, base.span, "..")),
                |diag| {
                    suggest(cx, base.hir_id, diag, base.span, "write `..` to take each field's declared default", "", applicability);
                    if !derived {
                        diag.note(format!("`..` skips the hand-written `Default` impl of `{name}`"));
                    }
                },
            );
        }
    }
}
