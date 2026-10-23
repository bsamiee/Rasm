use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, res::MaybeDef as _, sym};
use rustc_hir::{BinOpKind, Expr, ExprKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, print::with_forced_trimmed_paths};

use crate::{expanded_externally, impl_owner, path};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(WRAPPING_ARITHMETIC: "`wrapping_*` and `overflowing_*` integer methods and arithmetic operators on `Wrapping`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let arithmetic = |operation: BinOpKind| match operation {
            BinOpKind::Add => Some("add"),
            BinOpKind::Sub => Some("sub"),
            BinOpKind::Mul => Some("mul"),
            BinOpKind::Div => Some("div"),
            BinOpKind::Rem => Some("rem"),
            BinOpKind::Shl => Some("shl"),
            BinOpKind::Shr => Some("shr"),
            _ => None,
        };
        let operated = match expr.kind {
            ExprKind::Binary(operation, operand, _) => arithmetic(operation.node).map(|name| (operand, operation.node.as_str(), name)),
            ExprKind::AssignOp(operation, operand, _) => arithmetic(operation.node.into()).map(|name| (operand, operation.node.as_str(), name)),
            ExprKind::Unary(UnOp::Neg, operand) => Some((operand, "-", "neg")),
            _ => None,
        };
        let found = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::MethodCall(segment, ..)
                if let Some(method) = fn_def_id(cx, expr)
                    && let name = cx.tcx.item_name(method)
                    && let Some(operation) = ["wrapping_", "overflowing_"].iter().find_map(|prefix| name.as_str().strip_prefix(prefix))
                    && let Some(integer) = impl_owner(cx.tcx, method).filter(|integer| integer.is_integral()) =>
                Some((segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{integer}::{}` wraps on overflow", segment.ident), integer, operation.to_owned(), String::new())),
            _ if let Some((operand, symbol, operation)) = operated
                && let wrapped = cx.typeck_results().expr_ty(operand)
                && let ty::Adt(wrapping, arguments) = wrapped.kind()
                && wrapping.is_diag_item(cx, sym::Wrapping) =>
            {
                let (integer, wrapped) = (arguments.type_at(0), with_forced_trimmed_paths!(wrapped.to_string()));
                Some((expr.span, format!("`{symbol}` on `{wrapped}` wraps on overflow"), integer, operation.to_owned(), format!("hold `{integer}` in place of `{wrapped}`, ")))
            }
            _ => None,
        };
        if let Some((span, message, integer, operation, unwrapped)) = found {
            let saturating = format!("saturating_{operation}");
            let clamped = path(cx.tcx, PathNS::Value, &format!("{integer}::{saturating}")).map_or_default(|_| format!(", or `{saturating}` to clamp at the `{integer}` bounds"));
            span_lint_hir_and_then(cx, WRAPPING_ARITHMETIC, expr.hir_id, span, message, |diag| {
                diag.help(format!("{unwrapped}call `checked_{operation}` and return its `None` as an error{clamped}"));
            });
        }
    }
}
