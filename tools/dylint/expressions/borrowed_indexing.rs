use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, source::snippet, sym, ty::get_adt_inherent_method};
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, print::with_forced_trimmed_paths};
use rustc_span::Symbol;

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(BORROWED_INDEXING: "index expressions through a reference to a type with an inherent `get` or `get_index` taking the key and returning `Option`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Index(base, key, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let indexed = cx.typeck_results().expr_ty(base)
            && indexed.is_ref()
            && let held = indexed.peel_refs()
            && let key_borrowed = cx.typeck_results().expr_ty(key).is_ref()
            && let Some(getter) = [sym::get, Symbol::intern("get_index")].into_iter().find_map(|name| {
                get_adt_inherent_method(cx, held, name).filter(|method| {
                    let signature = cx.tcx.fn_sig(method.def_id).skip_binder().skip_binder();
                    matches!(signature.output().kind(), ty::Adt(option, _) if option.is_diag_item(cx, sym::Option)) && signature.inputs().get(1).is_some_and(|parameter| parameter.is_ref() == key_borrowed)
                })
            })
        {
            let (held, receiver, key) = (with_forced_trimmed_paths!(held.to_string()), snippet(cx, base.span, ".."), snippet(cx, key.span, ".."));
            span_lint_hir_and_then(cx, BORROWED_INDEXING, expr.hir_id, expr.span, format!("indexing `{held}` by `{key}` panics when the entry is absent"), |diag| {
                diag.help(format!("call `{receiver}.{}({key})` and handle `None`", getter.name()));
            });
        }
    }
}
