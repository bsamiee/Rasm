use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS, res::MaybeDef as _, sym};
use rustc_hir::{Item, ItemKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    source: Option<DefId>,
}

lint!(Pass; MANUAL_ERROR_IMPL: "hand-written `core::error::Error` impls");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { source: path(tcx, PathNS::Value, "core::error::Error::source") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(_) = item.kind
            && !expanded_externally(cx, item.span)
            && cx.tcx.impl_opt_trait_id(item.owner_id).is_diag_item(cx, sym::Error)
        {
            let implementor = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization();
            let sourced = self.source.is_some_and(|source| cx.tcx.impl_item_implementor_ids(item.owner_id.to_def_id()).contains_key(&source));
            span_lint_hir_and_then(
                cx,
                MANUAL_ERROR_IMPL,
                item.hir_id(),
                cx.tcx.def_span(item.owner_id),
                with_forced_trimmed_paths!(format!("`{implementor}` implements `core::error::Error` by hand")),
                |diag| {
                    diag.help(if sourced {
                        "derive `thiserror::Error`, move the `Display` text into `#[error(..)]`, and mark the field that `source` returns `#[source]`"
                    } else {
                        "derive `thiserror::Error` and move the `Display` text into `#[error(..)]`"
                    });
                },
            );
        }
    }
}
