use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    sym,
    ty::{get_iterator_item_ty, implements_trait},
};
use rustc_hir::{ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArg, Ty, print::with_forced_trimmed_paths};

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(FN_POINTER_ITERATOR_TYPE: "associated types naming an iterator adapter through a `fn` pointer in place of its closure");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Type(declared) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(iterator) = cx.tcx.get_diagnostic_item(sym::Iterator)
            && let Some((adapter, produced)) =
                cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization().walk().filter_map(GenericArg::as_type).find_map(|adapter| match adapter.kind() {
                    ty::Adt(_, arguments) if arguments.types().any(Ty::is_fn_ptr) && implements_trait(cx, adapter, iterator, &[]) => {
                        let produced = get_iterator_item_ty(cx, adapter)?;
                        arguments.types().any(|argument| argument.is_fn_ptr() && argument != produced).then_some((adapter, produced))
                    }
                    _ => None,
                })
        {
            let (name, adapter, produced) = (item.ident, with_forced_trimmed_paths!(adapter.to_string()), with_forced_trimmed_paths!(produced.to_string()));
            span_lint_hir_and_then(cx, FN_POINTER_ITERATOR_TYPE, item.hir_id(), declared.span, format!("`{name}` names `{adapter}` through a `fn` pointer in place of its closure"), |diag| {
                diag.help(format!("declare `type {name} = impl Iterator<Item = {produced}>` under `#![feature(impl_trait_in_assoc_type)]` and build the adapter with a closure"));
            });
        }
    }
}
