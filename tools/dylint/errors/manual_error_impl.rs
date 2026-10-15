use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use rustc_hir::{Item, ItemKind};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_ERROR_IMPL,
    Warn,
    "hand-written `core::error::Error` impls that `thiserror` derives"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] ManualErrorImpl => [MANUAL_ERROR_IMPL]);

impl<'tcx> LateLintPass<'tcx> for ManualErrorImpl {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(implementation) = item.kind
            && implementation.of_trait.is_some()
            && !item.span.from_expansion()
            && cx.tcx.impl_trait_id(item.owner_id).is_diag_item(cx, sym::Error)
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
