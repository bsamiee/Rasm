use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, res::MaybeDef as _, sym, ty::approx_ty_size};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind, PatKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

use super::unread;
use crate::{expanded_externally, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(DISCARDED_ERROR_SOURCE: "`map_err` and `or_else` closures that replace a non-zero-sized error without reading it");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, [handler], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(method) = fn_def_id(cx, expr)
            && let name @ (sym::map_err | sym::or_else) = cx.tcx.item_name(method)
            && let Some((_, error)) = result_parts(cx, cx.typeck_results().expr_ty_adjusted(receiver))
            && approx_ty_size(cx, error) > 0
            && let Some((parameter, body)) = unread(cx, handler)
            && (name == sym::map_err && !matches!(parameter.kind, PatKind::Wild) || name == sym::or_else && fn_def_id(cx, body).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr))
        {
            let error = with_forced_trimmed_paths!(error.to_string());
            span_lint_hir_and_then(cx, DISCARDED_ERROR_SOURCE, expr.hir_id, segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{name}` replaces `{error}` without reading it"), |diag| {
                diag.help(format!("hold `{error}` as `#[source]` of the error the closure builds"));
            });
        }
    }
}
