use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::paths::PathNS;
use clippy_utils::res::MaybeDef as _;
use clippy_utils::{binop_traits, fn_def_id, sym};
use rustc_attr_ir::LangItem;
use rustc_hir::{Expr, ExprKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;
use rustc_middle::ty::print::with_forced_trimmed_paths;
use rustc_span::Symbol;

use crate::{expanded_externally, impl_owner, path};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(WRAPPING_ARITHMETIC: "`wrapping_*` and `overflowing_*` integer methods and arithmetic operators on `Wrapping`");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let arithmetic = |operation| binop_traits(operation).filter(|(item, _)| !matches!(item, LangItem::BitAnd | LangItem::BitOr | LangItem::BitXor)).map(|(item, _)| item.name());
        let operated = match expr.kind {
            ExprKind::Binary(operation, operand, _) => arithmetic(operation.node).map(|name| (operand, operation.node.as_str(), name)),
            ExprKind::AssignOp(operation, operand, _) => arithmetic(operation.node.into()).map(|name| (operand, operation.node.as_str(), name)),
            ExprKind::Unary(UnOp::Neg, operand) => Some((operand, "-", sym::neg)),
            _ => None,
        };
        let found = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::Call(..) | ExprKind::MethodCall(..)
                if let Some(method) = fn_def_id(cx, expr)
                    && let name = cx.tcx.item_name(method)
                    && let Some(operation) = ["wrapping_", "overflowing_"].iter().find_map(|prefix| name.as_str().strip_prefix(prefix))
                    && let Some(integer) = impl_owner(cx.tcx, method).filter(|integer| integer.is_integral()) =>
            {
                let span = match expr.kind {
                    ExprKind::MethodCall(segment, ..) => segment.ident.span.to(expr.span.shrink_to_hi()),
                    _ => expr.span,
                };
                Some((span, format!("`{integer}::{name}` wraps on overflow"), integer, Symbol::intern(operation), String::new()))
            }
            _ if let Some((operand, symbol, operation)) = operated
                && let wrapped = cx.typeck_results().expr_ty(operand).peel_refs()
                && let ty::Adt(wrapping, arguments) = wrapped.kind()
                && wrapping.is_diag_item(cx, sym::Wrapping) =>
            {
                let (integer, wrapped) = (arguments.type_at(0), with_forced_trimmed_paths!(wrapped.to_string()));
                Some((expr.span, format!("`{symbol}` on `{wrapped}` wraps on overflow"), integer, operation, format!("hold `{integer}` in place of `{wrapped}`, ")))
            }
            _ => None,
        };
        if let Some((span, message, integer, operation, unwrapped)) = found {
            let saturating = format!("saturating_{operation}");
            let clamped = if path(cx.tcx, PathNS::Value, &format!("{integer}::{saturating}")).is_some() { format!(", or `{saturating}` to clamp at the `{integer}` bounds") } else { String::new() };
            span_lint_hir_and_then(cx, WRAPPING_ARITHMETIC, expr.hir_id, span, message, |diag| {
                diag.help(format!("{unwrapped}call `checked_{operation}` and return its `None` as an error{clamped}"));
            });
        }
    }
}
