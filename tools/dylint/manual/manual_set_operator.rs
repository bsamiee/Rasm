use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    res::MaybeDef as _,
    sugg::{Sugg, make_binop},
    sym,
};
use itertools::Itertools as _;
use rustc_ast::BinOpKind;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::adjustment::Adjust;
use rustc_span::Symbol;

use super::iterator_method;
use crate::{associated_trait, expanded_externally, impl_owner, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_SET_OPERATOR: "`intersection`, `union`, `difference`, and `symmetric_difference` chains collected into the set their operator returns");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let tcx = cx.tcx;
        let typeck = cx.typeck_results();
        if let ExprKind::MethodCall(_, cloned, [], _) | ExprKind::Call(_, [cloned]) = expr.kind
            && let ExprKind::MethodCall(_, operation, [], _) | ExprKind::Call(_, [operation]) = cloned.kind
            && let ExprKind::MethodCall(_, set, [other], _) | ExprKind::Call(_, [set, other]) = operation.kind
            && let [Some(collect), Some(clone), Some(method)] = [expr, cloned, operation].map(|call| fn_def_id(cx, call))
            && tcx.item_name(collect) == sym::collect
            && matches!(tcx.item_name(clone), sym::copied | sym::cloned)
            && iterator_method(cx, expr)
            && iterator_method(cx, cloned)
            && associated_trait(tcx, method).is_none()
            && matches!(impl_owner(tcx, method).opt_diag_name(cx), Some(sym::BTreeSet | sym::HashSet))
            && let name = tcx.item_name(method)
            && let Some((_, operator)) = [("intersection", BinOpKind::BitAnd), ("union", BinOpKind::BitOr), ("difference", BinOpKind::Sub), ("symmetric_difference", BinOpKind::BitXor)]
                .into_iter()
                .find(|&(written, _)| name == Symbol::intern(written))
            && typeck.expr_ty(expr) == typeck.expr_ty_adjusted(set).peel_refs()
            && !expanded_externally(cx, expr.span)
        {
            let ctxt = expr.span.ctxt();
            let mut applicability = Applicability::MachineApplicable;
            let mut borrowed = |operand: &'tcx Expr<'tcx>| {
                let written = Sugg::hir_with_context(cx, operand, ctxt, "..", &mut applicability);
                let target = typeck.expr_ty_adjusted(operand);
                if typeck.expr_ty(operand) == target {
                    written
                } else {
                    typeck.expr_adjustments(operand).iter().take_while_inclusive(|step| step.target != target).fold(written, |written, step| match step.kind {
                        Adjust::Deref(_) => written.deref(),
                        Adjust::Borrow(_) => written.addr(),
                        _ => written,
                    })
                }
            };
            let (left, right) = (borrowed(set), borrowed(other));
            span_lint_hir_and_then(cx, MANUAL_SET_OPERATOR, expr.hir_id, expr.span, format!("`{name}` collects a set by hand"), |diag| {
                suggest(cx, expr.hir_id, diag, expr.span, "write the set operator", make_binop(operator, &left, &right), applicability);
            });
        }
    }
}
