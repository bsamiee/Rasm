use clippy_utils::diagnostics::span_lint_hir_and_then;
use itertools::Itertools as _;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{AmbigArg, Ty, TyKind, def::DefKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{AssocKind, AssocTypeData, print::with_forced_trimmed_paths};
use rustc_span::Span;

use crate::{expanded_externally, exported};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    objects: FxIndexMap<DefId, Vec<Span>>,
}

lint!(default Pass; SINGLE_IMPL_TRAIT: "crate-private traits with one implementor");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_ty(&mut self, _: &LateContext<'tcx>, hir_ty: &'tcx Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(bounds, ..) = hir_ty.kind
            && let Some(principal) = bounds.iter().find_map(|bound| bound.trait_ref.trait_def_id().filter(|principal| principal.is_local()))
        {
            self.objects.entry(principal).or_default().push(hir_ty.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let singles = tcx.hir_crate_items(()).free_items().filter_map(|item| {
            let declared = item.owner_id.to_def_id();
            if tcx.def_kind(declared) == DefKind::Trait
                && !expanded_externally(cx, tcx.def_span(declared))
                && !exported(cx, item.owner_id.def_id)
                && !tcx.associated_item_def_ids(declared).is_empty()
                && !tcx.associated_items(declared).in_definition_order().any(|associated| matches!(associated.kind, AssocKind::Type { data: AssocTypeData::Normal(_) }))
                && let Some(single) = tcx.all_impls(declared).exactly_one().ok()
                && !(tcx.generics_of(single).requires_monomorphization(tcx) && self.objects.contains_key(&declared))
            {
                Some((item.hir_id(), declared, tcx.type_of(single).instantiate_identity().skip_normalization()))
            } else {
                None
            }
        });
        for (node, declared, implementor) in singles {
            let name = tcx.def_path_str(declared);
            let local = implementor.ty_adt_def().is_some_and(|adt| adt.did().is_local());
            span_lint_hir_and_then(cx, SINGLE_IMPL_TRAIT, node, tcx.def_span(declared), with_forced_trimmed_paths!(format!("trait `{name}` is implemented only for `{implementor}`")), |diag| {
                if let Some(objects) = self.objects.get(&declared).map(|objects| objects.iter().copied().filter(|&object| !expanded_externally(cx, object)).collect::<Vec<_>>())
                    && !objects.is_empty()
                {
                    diag.span_note(objects, with_forced_trimmed_paths!(format!("`dyn {name}` always holds `{implementor}`")));
                }
                diag.help(if local {
                    with_forced_trimmed_paths!(format!("replace the trait with an inherent `impl {implementor}` block"))
                } else {
                    with_forced_trimmed_paths!(format!("delete the trait and write each method's body at its call sites on `{implementor}`"))
                });
            });
        }
    }
}
