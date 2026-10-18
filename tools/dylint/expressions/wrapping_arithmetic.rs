use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, res::MaybeDef as _, sym};
use rustc_hir::{BinOpKind, Expr, ExprKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::Ty;

use crate::{expanded_externally, impl_owner};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(WRAPPING_ARITHMETIC: "`wrapping_*` and `overflowing_*` integer methods and `Wrapping` operators, which wrap on overflow");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let arithmetic = |operation: BinOpKind| matches!(operation, BinOpKind::Add | BinOpKind::Sub | BinOpKind::Mul | BinOpKind::Div | BinOpKind::Rem | BinOpKind::Shl | BinOpKind::Shr);
        let operand = match expr.kind {
            ExprKind::Binary(operation, operand, _) if arithmetic(operation.node) => Some(operand),
            ExprKind::AssignOp(operation, operand, _) if arithmetic(operation.node.into()) => Some(operand),
            ExprKind::Unary(UnOp::Neg, operand) => Some(operand),
            _ => None,
        };
        let message = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::MethodCall(segment, ..)
                if let Some(method) = fn_def_id(cx, expr)
                    && ["wrapping_", "overflowing_"].iter().any(|prefix| cx.tcx.item_name(method).as_str().starts_with(prefix))
                    && impl_owner(cx.tcx, method).is_some_and(Ty::is_integral) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{}` wraps on overflow", segment.ident))),
            _ if operand.is_some_and(|operand| cx.typeck_results().expr_ty(operand).is_diag_item(cx, sym::Wrapping)) => Some((expr.span, String::from("`Wrapping` arithmetic wraps on overflow"))),
            _ => None,
        };
        if let Some((span, message)) = message {
            span_lint_hir_and_then(cx, WRAPPING_ARITHMETIC, expr.hir_id, span, message, |diag| {
                diag.help("call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain");
            });
        }
    }
}
