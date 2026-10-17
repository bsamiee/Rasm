use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use rustc_hir::{Item, ItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_ERROR_IMPL: "hand-written `core::error::Error` impls");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(_) = item.kind
            && !expanded_externally(cx, item.span)
            && cx.tcx.impl_opt_trait_id(item.owner_id).is_diag_item(cx, sym::Error)
        {
            let implementor = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization();
            span_lint_hir_and_then(
                cx,
                MANUAL_ERROR_IMPL,
                item.hir_id(),
                cx.tcx.def_span(item.owner_id),
                with_forced_trimmed_paths!(format!("`{implementor}` implements `core::error::Error` by hand")),
                |diag| {
                    diag.help("derive `thiserror::Error` with an `#[error(..)]` message and `#[source]` on the wrapped error");
                },
            );
        }
    }
}
