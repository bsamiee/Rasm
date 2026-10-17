use clippy_utils::{diagnostics::span_lint_hir_and_then, eq_expr_value, sugg::Sugg};
use rustc_ast::{AssignOpKind, LitKind};
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::Spanned;

use crate::{expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_BOOL_TOGGLE: "`x = !x` and `x ^= true` assignments negating the `bool` they write");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let negated = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::Assign(place, &Expr { kind: ExprKind::Unary(UnOp::Not, negated), .. }, _) if eq_expr_value(cx, expr.span.ctxt(), place, negated) => Some(place),
            ExprKind::AssignOp(Spanned { node: AssignOpKind::BitXorAssign, .. }, place, &Expr { kind: ExprKind::Lit(Spanned { node: LitKind::Bool(true), .. }), .. }) => Some(place),
            _ => None,
        };
        if let Some(place) = negated.filter(|place| cx.typeck_results().expr_ty(place).is_bool()) {
            let mut applicability = Applicability::MachineApplicable;
            let receiver = if let ExprKind::Unary(UnOp::Deref, referent) = place.kind { referent } else { place };
            let toggled = Sugg::hir_with_context(cx, receiver, expr.span.ctxt(), "..", &mut applicability).maybe_paren();
            span_lint_hir_and_then(cx, MANUAL_BOOL_TOGGLE, expr.hir_id, expr.span, format!("assignment negates `{toggled}` by hand"), |diag| {
                suggest(cx, expr.hir_id, diag, expr.span, "call `bool::toggle`", format!("{toggled}.toggle()"), applicability);
            });
        }
    }
}
