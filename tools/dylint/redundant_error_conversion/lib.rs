#![feature(rustc_private)]

extern crate rustc_errors;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use clippy_utils::{
    diagnostics::span_lint_and_sugg,
    is_from_proc_macro,
    res::MaybeDef as _,
    ty::{implements_trait, same_type_modulo_regions},
};
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, MatchSource};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_span::sym;

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::declare_late_lint! {
    pub REDUNDANT_ERROR_CONVERSION,
    Warn,
    "`map_err` repeats the `From` conversion `?` applies"
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for RedundantErrorConversion {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::Match(branch, arms, MatchSource::TryDesugar(_)) = expr.kind
            && let ExprKind::Call(_, [converted]) = branch.kind
            && let ExprKind::MethodCall(segment, source, [conversion], _) = converted.kind
            && segment.ident.name == sym::map_err
            && !converted.span.from_expansion()
            && let ty::FnDef(conversion_fn, _) = typeck.expr_ty(conversion).kind()
            && let Some(conversion_trait) = cx.tcx.trait_of_assoc(*conversion_fn)
            && (conversion_trait.is_diag_item(cx, sym::From) || conversion_trait.is_diag_item(cx, sym::Into))
            && let ty::Adt(source_result, source_args) = typeck.expr_ty(source).kind()
            && source_result.is_diag_item(cx, sym::Result)
            && let Some(source_error) = source_args.types().nth(1)
            && let Some(returned) = arms.iter().find_map(|arm| if let ExprKind::Ret(Some(returned)) | ExprKind::Break(_, Some(returned)) = arm.body.kind { Some(returned) } else { None })
            && let ty::Adt(target_result, target_args) = typeck.expr_ty(returned).kind()
            && target_result.is_diag_item(cx, sym::Result)
            && let Some(target_error) = target_args.types().nth(1)
            && !same_type_modulo_regions(source_error, target_error)
            && let ty::Adt(_, converted_args) = typeck.expr_ty(converted).kind()
            && converted_args.types().nth(1).is_some_and(|converted_error| same_type_modulo_regions(converted_error, target_error))
            && let Some(from) = cx.tcx.get_diagnostic_item(sym::From)
            && implements_trait(cx, target_error, from, &[source_error.into()])
            && !is_from_proc_macro(cx, converted)
        {
            span_lint_and_sugg(
                cx,
                REDUNDANT_ERROR_CONVERSION,
                converted.span.with_lo(source.span.hi()),
                REDUNDANT_ERROR_CONVERSION.desc,
                "remove the `map_err` call",
                String::new(),
                Applicability::MachineApplicable,
            );
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
