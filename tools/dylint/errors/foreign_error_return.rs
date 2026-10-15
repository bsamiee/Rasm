use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    is_in_test,
    paths::{PathNS, lookup_path_str},
};
use rustc_hir::{
    FnRetTy, ImplItem, Item, TraitItem,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, LintContext as _, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, TyCtxt};

use crate::{delivered, is_error, pointee, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    FOREIGN_ERROR_RETURN,
    Warn,
    "exported functions returning an error type another crate defines"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ForeignErrorReturn {
    formatting: Option<DefId>,
}

impl_lint_pass!(ForeignErrorReturn => [FOREIGN_ERROR_RETURN]);

impl ForeignErrorReturn {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { formatting: lookup_path_str(tcx, PathNS::Type, "core::fmt::Error").into_iter().next() } }

    fn inspect(&self, cx: &LateContext<'_>, owner: LocalDefId) {
        if let Some(declaration) = cx.tcx.hir_node_by_def_id(owner).fn_decl()
            && let FnRetTy::Return(output) = declaration.output
            && !output.span.in_external_macro(cx.sess().source_map())
            && cx.effective_visibilities.is_exported(owner)
            && let Some((_, error)) = result_parts(cx, delivered(cx.tcx, owner.to_def_id()))
            && let ty::Adt(adt, _) = pointee(cx, error).kind()
            && !adt.did().is_local()
            && Some(adt.did()) != self.formatting
            && let node = cx.tcx.local_def_id_to_hir_id(owner)
            && !is_in_test(cx.tcx, node)
            && is_error(cx, error)
        {
            let foreign = cx.tcx.def_path_str(adt.did());
            span_lint_hir_and_then(
                cx,
                FOREIGN_ERROR_RETURN,
                node,
                output.span.source_callsite(),
                format!("exported `{}` returns `{foreign}`, an error type another crate defines", cx.tcx.item_name(owner)),
                |diag| {
                    diag.help(format!("return an error enum this crate defines, with a variant holding the `{foreign}` as its `#[source]`"));
                },
            );
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ForeignErrorReturn {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) { self.inspect(cx, item.owner_id.def_id); }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) { self.inspect(cx, item.owner_id.def_id); }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if cx.tcx.trait_impl_of_assoc(item.owner_id.to_def_id()).is_none() {
            self.inspect(cx, item.owner_id.def_id);
        }
    }
}
