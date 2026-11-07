use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, is_from_proc_macro, peel_blocks, res::MaybeDef as _, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

use super::{UntypedKind, failing, has_single_failure_cause, singular, unread, untyped_kind};
use crate::{constructed, expanded_externally, impl_owner, is_error, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(DISCARDED_ERROR_SOURCE: "`map_err` and `or_else` closures that replace a failure without reading it");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, [handler], _) = expr.kind
            && let name @ (sym::map_err | sym::or_else) = segment.ident.name
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).and_then(|method| impl_owner(cx.tcx, method)).is_diag_item(cx, sym::Result)
            && let Some(body) = unread(cx, handler)
            && (name == sym::map_err || fn_def_id(cx, body).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr))
            && let built = if let (sym::or_else, ExprKind::Call(_, [wrapped])) = (name, body.kind) { peel_blocks(wrapped) } else { body }
            && let Some((_, error)) = result_parts(cx, cx.typeck_results().expr_ty_adjusted(receiver))
            && failing(cx, error)
            && let text = matches!(untyped_kind(cx, error), Some(UntypedKind::Text))
            && !(text && constructed(cx.typeck_results(), built).is_some_and(|(variant, _)| !variant.fields.is_empty()))
            && !((has_single_failure_cause(cx, receiver) || error.ty_adt_def().is_some_and(|adt| !adt.did().is_local()) && singular(cx, error))
                && result_parts(cx, cx.typeck_results().expr_ty(expr)).is_some_and(|(_, translated)| is_error(cx, translated) && untyped_kind(cx, translated).is_none()))
            && !is_from_proc_macro(cx, expr)
        {
            let error = with_forced_trimmed_paths!(error.to_string());
            let help =
                if text { String::from("hold the text or the subject it names as a field the message prints") } else { format!("hold `{error}` as `#[source]` of the error the closure builds") };
            span_lint_hir_and_then(cx, DISCARDED_ERROR_SOURCE, expr.hir_id, segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{name}` replaces `{error}` without reading it"), |diag| {
                diag.help(help);
            });
        }
    }
}
