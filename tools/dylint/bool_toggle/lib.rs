#![feature(rustc_private)]

extern crate rustc_ast;
extern crate rustc_errors;
extern crate rustc_hir;

use clippy_utils::{diagnostics::span_lint_and_sugg, eq_expr_value, sugg::Sugg};
use rustc_ast::LitKind;
use rustc_errors::Applicability;
use rustc_hir::{AssignOpKind, Expr, ExprKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::declare_late_lint! {
    pub BOOL_TOGGLE,
    Warn,
    "`bool` negated through assignment"
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for BoolToggle {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if !expr.span.from_expansion()
            && let Some(place) = if let ExprKind::Assign(place, value, _) = expr.kind
                && let ExprKind::Unary(UnOp::Not, negated) = value.kind
                && eq_expr_value(cx, expr.span.ctxt(), place, negated)
            {
                Some(place)
            } else if let ExprKind::AssignOp(operator, place, value) = expr.kind
                && operator.node == AssignOpKind::BitXorAssign
                && let ExprKind::Lit(literal) = value.kind
                && literal.node == LitKind::Bool(true)
            {
                Some(place)
            } else {
                None
            }
            && cx.typeck_results().expr_ty(place).is_bool()
        {
            let mut applicability = Applicability::MachineApplicable;
            let receiver = Sugg::hir_with_applicability(cx, if let ExprKind::Unary(UnOp::Deref, pointer) = place.kind { pointer } else { place }, "..", &mut applicability);
            span_lint_and_sugg(cx, BOOL_TOGGLE, expr.span, BOOL_TOGGLE.desc, "call `bool::toggle`", format!("{}.toggle()", receiver.maybe_paren()), applicability);
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
