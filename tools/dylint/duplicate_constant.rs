use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::AssocKind;

use crate::equal_bodies;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    DUPLICATE_CONSTANT,
    Warn,
    "trait constants every impl gives one value"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] DuplicateConstant => [DUPLICATE_CONSTANT]);

impl<'tcx> LateLintPass<'tcx> for DuplicateConstant {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let provided = tcx.hir_crate_items(()).definitions().filter(|&item| !tcx.def_span(item).from_expansion() && tcx.def_kind(item) == DefKind::AssocConst).fold(
            FxIndexMap::<LocalDefId, Vec<LocalDefId>>::default(),
            |mut provided, item| {
                if let Some(declared) = tcx.trait_item_of(item).and_then(DefId::as_local) {
                    provided.entry(declared).or_default().push(item);
                }
                provided
            },
        );
        let shared = provided.into_iter().filter(|(declared, constants)| {
            matches!(tcx.associated_item(*declared).kind, AssocKind::Const { name: _ })
                && !tcx.defaultness(*declared).has_value()
                && constants.len() > 1
                && tcx.all_impls(tcx.parent(declared.to_def_id())).count() == constants.len()
                && constants.array_windows().all(|&[earlier, later]| equal_bodies(cx, earlier, later, |_, _, _, _| false).is_some())
        });
        for (declared, constants) in shared {
            let (name, owner) = (tcx.item_name(declared), tcx.item_name(tcx.parent(declared.to_def_id())));
            span_lint_hir_and_then(cx, DUPLICATE_CONSTANT, tcx.local_def_id_to_hir_id(declared), tcx.def_span(declared), format!("every `{owner}` impl gives `{name}` the same value"), |diag| {
                diag.span_note(constants.iter().map(|&constant| tcx.def_span(constant)).collect::<Vec<_>>(), "each impl states the value here");
                diag.help(format!("state `{name}` once as the default value in `{owner}` and remove it from each impl"));
            });
        }
    }
}
