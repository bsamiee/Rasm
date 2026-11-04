use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_errors::Applicability;
use rustc_hir::{
    ForeignItem, HirId, ImplItem, ImplItemImplKind, Item,
    def_id::{CRATE_DEF_ID, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{ExpnKind, MacroKind, Span};

use crate::{suggest, workspace_def};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(UNREACHABLE_PUBLIC_ITEM: "`pub` items unreachable from the crate root outside the output of an external macro");

// --- [OPERATIONS] ----------------------------------------------------------------------

fn report(cx: &LateContext<'_>, node: HirId, item: LocalDefId, visibility: Span) {
    if !cx.tcx.visibility(item).is_public() || cx.effective_visibilities.is_reachable(item) {
        return;
    }
    let defined = cx.tcx.expn_that_defined(item).expn_data();
    let generated = match defined.kind {
        ExpnKind::Macro(MacroKind::Bang, _) => !defined.macro_def_id.is_some_and(|written| workspace_def(cx, written)),
        ExpnKind::Macro(..) => true,
        _ => false,
    };
    if !generated {
        let descr = cx.tcx.def_descr(item.to_def_id());
        span_lint_hir_and_then(cx, UNREACHABLE_PUBLIC_ITEM, node, cx.tcx.def_span(item), format!("`pub` {descr} is unreachable from the crate root"), |diag| {
            if cx.tcx.parent_module_from_def_id(item).to_local_def_id() == CRATE_DEF_ID {
                suggest(
                    cx,
                    node,
                    diag,
                    cx.tcx.sess.source_map().span_until_non_whitespace(visibility),
                    "delete the visibility, a private crate root item is visible in every module of its crate",
                    "",
                    Applicability::MachineApplicable,
                );
            } else {
                suggest(cx, node, diag, visibility, "narrow the visibility to the crate", "pub(crate)", Applicability::MachineApplicable);
            }
        });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) { report(cx, item.hir_id(), item.owner_id.def_id, item.vis_span); }

    fn check_foreign_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ForeignItem<'tcx>) { report(cx, item.hir_id(), item.owner_id.def_id, item.vis_span); }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemImplKind::Inherent { vis_span } = item.impl_kind {
            report(cx, item.hir_id(), item.owner_id.def_id, vis_span);
        }
    }
}
