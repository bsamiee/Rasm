#![feature(rustc_private)]

extern crate rustc_data_structures;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use clippy_utils::diagnostics::span_lint_and_then;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{AmbigArg, Ty, TyKind, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{AssocKind, AssocTypeData};
use rustc_span::Span;

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub SINGLE_IMPLEMENTOR_TRAIT,
    Warn,
    "crate-private traits with one implementor",
    SingleImplementorTrait::default()
}

#[derive(Default)]
struct SingleImplementorTrait {
    objects: FxIndexMap<DefId, Vec<Span>>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for SingleImplementorTrait {
    fn check_ty(&mut self, _: &LateContext<'tcx>, hir_ty: &'tcx Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(bounds, ..) = hir_ty.kind
            && !hir_ty.span.from_expansion()
            && let Some(principal) = bounds.iter().find_map(|bound| bound.trait_ref.trait_def_id().filter(|principal| principal.is_local()))
        {
            self.objects.entry(principal).or_default().push(hir_ty.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let singles = cx.tcx.hir_crate_items(()).definitions().filter_map(|candidate| {
            let declared = candidate.to_def_id();
            if cx.tcx.def_kind(declared) == DefKind::Trait
                && !cx.effective_visibilities.is_exported(candidate)
                && !cx.tcx.associated_item_def_ids(declared).is_empty()
                && !cx.tcx.associated_items(declared).in_definition_order().any(|item| matches!(item.kind, AssocKind::Type { data: AssocTypeData::Normal(_) }))
                && let [single] = cx.tcx.all_impls(declared).collect::<Vec<_>>().as_slice()
                && !(cx.tcx.generics_of(*single).count() > 0 && self.objects.contains_key(&declared))
                && let Some(implementor) = cx.tcx.type_of(*single).instantiate_identity().skip_normalization().ty_adt_def()
                && implementor.did().is_local()
            {
                Some((declared, cx.tcx.def_path_str(implementor.did())))
            } else {
                None
            }
        });
        for (declared, implementor) in singles {
            let name = cx.tcx.def_path_str(declared);
            span_lint_and_then(cx, SINGLE_IMPLEMENTOR_TRAIT, cx.tcx.def_span(declared), format!("trait `{name}` has one implementor, `{implementor}`"), |diag| {
                if let Some(objects) = self.objects.get(&declared) {
                    diag.span_note(objects.clone(), format!("`dyn {name}` holds a `{implementor}` at each use"));
                }
                diag.help(format!("replace the trait with an inherent `impl {implementor}` block"));
            });
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
