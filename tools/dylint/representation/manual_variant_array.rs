use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::res::{MaybeDef as _, MaybeQPath as _};
use clippy_utils::ty::is_copy;
use rustc_hir::{Expr, ExprKind, Mutability};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;
use rustc_middle::ty::{self, VariantDef};

use super::fieldless_local_enum;
use crate::{expanded_externally, generated};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_VARIANT_ARRAY: "borrowed slices listing every variant of a fieldless local `Copy` enum in declaration order");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let ExprKind::Array(elements @ [first, ..]) = expr.kind else { return };
        let typeck = cx.typeck_results();
        if matches!(typeck.expr_ty_adjusted(expr).kind(), ty::Ref(_, held, Mutability::Not) if held.is_slice())
            && let Some(keyed) = fieldless_local_enum(typeck.expr_ty(first))
            && is_copy(cx, typeck.expr_ty(first))
            && !expanded_externally(cx, expr.span)
            && elements.iter().map(|element| element.res(typeck).opt_def_id()).eq(keyed.variants().iter().map(VariantDef::ctor_def_id))
            && !generated(cx, expr.hir_id)
        {
            let name = with_forced_trimmed_paths!(typeck.expr_ty(first).to_string());
            span_lint_hir_and_then(cx, MANUAL_VARIANT_ARRAY, expr.hir_id, expr.span, format!("borrowed slice lists every variant of `{name}` in declaration order"), |diag| {
                diag.help(format!("read `<{name} as strum::VariantArray>::VARIANTS`; add `#[derive(strum::VariantArray)]` where the trait is absent"));
            });
        }
    }
}
