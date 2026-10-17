use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    sym,
    ty::{get_iterator_item_ty, implements_trait},
};
use rustc_hir::{ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArg};

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(FN_POINTER_ITERATOR_TYPE: "associated types naming an iterator adapter through a `fn` pointer in place of its closure");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Type(declared) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(iterator) = cx.tcx.get_diagnostic_item(sym::Iterator)
            && let Some(adapter) = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization().walk().filter_map(GenericArg::as_type).find(|adapter| {
                matches!(adapter.kind(), ty::Adt(_, arguments)
                    if arguments.types().any(|argument| argument.is_fn_ptr() && implements_trait(cx, *adapter, iterator, &[]) && get_iterator_item_ty(cx, *adapter) != Some(argument)))
            })
        {
            span_lint_hir_and_then(cx, FN_POINTER_ITERATOR_TYPE, item.hir_id(), declared.span, format!("`{}` names `{adapter}` through a `fn` pointer in place of its closure", item.ident), |diag| {
                diag.help(format!("declare `type {} = impl Iterator<Item = ..>` and build it with a closure under `#![feature(impl_trait_in_assoc_type)]`", item.ident));
            });
        }
    }
}
