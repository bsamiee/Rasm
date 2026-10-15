use clippy_utils::{diagnostics::span_lint_hir_and_then, sym};
use rustc_hir::{TraitItem, TraitItemKind};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};

use crate::{Upcast, upcast};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_UPCAST,
    Warn,
    "trait methods returning `self` as an object of a supertrait or of `Any`"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] ManualUpcast => [MANUAL_UPCAST]);

impl<'tcx> LateLintPass<'tcx> for ManualUpcast {
    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(signature, _) = item.kind
            && !item.span.from_expansion()
            && let Some(upcast) = upcast(cx, item.owner_id.def_id)
        {
            let subtrait = cx.tcx.item_name(cx.tcx.parent(item.owner_id.to_def_id()));
            let (supertrait, condition) = match upcast {
                Upcast::Supertrait(supertrait) => (cx.tcx.item_name(supertrait), ""),
                Upcast::Any => (sym::Any, " once `Any` is a supertrait"),
            };
            span_lint_hir_and_then(
                cx,
                MANUAL_UPCAST,
                item.hir_id(),
                signature.decl.output.span(),
                format!("`{}` returns `self` as `dyn {supertrait}`, which trait upcasting converts from `dyn {subtrait}`{condition}", item.ident),
                |diag| {
                    diag.help(match upcast {
                        Upcast::Supertrait(_) => format!("delete the method and coerce `dyn {subtrait}` to `dyn {supertrait}` where it is called, trait upcasting converts the object"),
                        Upcast::Any => format!("add `Any` to the supertraits of `{subtrait}`, delete the method, and coerce `dyn {subtrait}` to `dyn Any` where it is called"),
                    });
                },
            );
        }
    }
}
