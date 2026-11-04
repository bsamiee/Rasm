use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_hir::{TraitItem, TraitItemKind};
use rustc_lint::{LateContext, LateLintPass};

use crate::{expanded_externally, upcast};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_UPCAST: "trait methods returning `self` as an object of a supertrait or of `Any`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(signature, _) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(upcast) = upcast(cx, item.owner_id.def_id)
        {
            let (method, subtrait) = (item.ident, cx.tcx.item_name(cx.tcx.parent(item.owner_id.to_def_id())));
            let supertrait = cx.tcx.item_name(upcast);
            let (message, help) = if cx.tcx.generics_require_sized_self(item.owner_id.to_def_id()) {
                (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which an unsizing coercion converts"),
                    format!("delete the method and coerce the receiver to `dyn {supertrait}` at each call site"),
                )
            } else {
                (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which trait upcasting converts from `dyn {subtrait}`"),
                    format!("delete the method and coerce `dyn {subtrait}` to `dyn {supertrait}` at each call site"),
                )
            };
            span_lint_hir_and_then(cx, MANUAL_UPCAST, item.hir_id(), signature.decl.output.span(), message, |diag| {
                diag.help(help);
            });
        }
    }
}
