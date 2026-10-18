use clippy_utils::{diagnostics::span_lint_hir_and_then, sym};
use rustc_hir::{TraitItem, TraitItemKind};
use rustc_lint::{LateContext, LateLintPass};

use crate::{Upcast, expanded_externally, upcast};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_UPCAST: "trait methods returning `self` as an object of a supertrait or of `Any`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(signature, _) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(upcast) = upcast(cx, item.owner_id.def_id)
        {
            let (method, subtrait) = (item.ident, cx.tcx.item_name(cx.tcx.parent(item.owner_id.to_def_id())));
            let supertrait = match upcast {
                Upcast::Supertrait(supertrait) => cx.tcx.item_name(supertrait),
                Upcast::Any => sym::Any,
            };
            let (message, help) = match (cx.tcx.generics_require_sized_self(item.owner_id.to_def_id()), upcast) {
                (true, Upcast::Supertrait(_) | Upcast::Any) => (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which an unsizing coercion converts"),
                    format!("delete the method and coerce each receiver to `dyn {supertrait}` at each call site"),
                ),
                (false, Upcast::Supertrait(_)) => (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which trait upcasting converts from `dyn {subtrait}`"),
                    format!("delete the method and coerce `dyn {subtrait}` to `dyn {supertrait}` at each call site"),
                ),
                (false, Upcast::Any) => (
                    format!("`{method}` returns `self` as `dyn Any`, which trait upcasting converts from `dyn {subtrait}` once `Any` is a supertrait"),
                    format!("add `Any` to the supertraits of `{subtrait}`, delete the method, and coerce `dyn {subtrait}` to `dyn Any` at each call site"),
                ),
            };
            span_lint_hir_and_then(cx, MANUAL_UPCAST, item.hir_id(), signature.decl.output.span(), message, |diag| {
                diag.help(help);
            });
        }
    }
}
