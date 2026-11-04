use core::{iter, mem};

use clippy_utils::{diagnostics::span_lint_hir_and_then, source::snippet, ty::ty_from_hir_ty};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    AmbigArg, Item, ItemKind, Ty, TyKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AssocKind, AssocTypeData, print::with_forced_trimmed_paths};
use rustc_span::Span;

use crate::{expanded_externally, exported};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    traits: Vec<LocalDefId>,
    objects: FxIndexMap<LocalDefId, Vec<Span>>,
}

lint!(default Pass; SINGLE_IMPL_TRAIT: "crate-private traits with one implementor");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Trait { .. } = item.kind
            && !expanded_externally(cx, item.span)
            && !exported(cx, item.owner_id.def_id)
            && !cx.tcx.associated_item_def_ids(item.owner_id).is_empty()
            && !cx.tcx.associated_items(item.owner_id).in_definition_order().any(|associated| matches!(associated.kind, AssocKind::Type { data: AssocTypeData::Normal(_) }))
        {
            self.traits.push(item.owner_id.def_id);
        }
    }

    fn check_ty(&mut self, cx: &LateContext<'tcx>, hir_ty: &'tcx Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(..) = hir_ty.kind
            && let ty::Dynamic(bounds, ..) = ty_from_hir_ty(cx, hir_ty.as_unambig_ty()).kind()
            && let Some(principal) = bounds.principal_def_id().and_then(DefId::as_local)
        {
            self.objects.entry(principal).or_default().push(hir_ty.span);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let singles = mem::take(&mut self.traits).into_iter().filter_map(|declared| match tcx.local_trait_impls(declared) {
            [single] if tcx.impl_polarity(*single) == ty::ImplPolarity::Positive && !(tcx.generics_of(*single).requires_monomorphization(tcx) && self.objects.contains_key(&declared)) =>
                Some((declared, *single)),
            [] | [_] | [_, _, ..] => None,
        });
        for (declared, single) in singles {
            let implementor = tcx.type_of(single).instantiate_identity().skip_normalization();
            let name = tcx.def_path_str(declared);
            let local = implementor.ty_adt_def().is_some_and(|adt| adt.did().is_local());
            let mut bounds = if let ty::Param(param) = implementor.kind() {
                tcx.hir_get_generics(single)
                    .zip(tcx.generics_of(single).type_param(*param, tcx).def_id.as_local())
                    .map(|(generics, param)| generics.bounds_for_param(param).flat_map(|predicate| predicate.bounds))
            } else {
                None
            }
            .into_iter()
            .flatten();
            let bound = bounds.next().map(|first| iter::once(first).chain(bounds).map(|bound| snippet(cx, bound.span(), "..")).format(" + "));
            span_lint_hir_and_then(
                cx,
                SINGLE_IMPL_TRAIT,
                tcx.local_def_id_to_hir_id(declared),
                tcx.def_span(declared),
                with_forced_trimmed_paths!(format!("trait `{name}` is implemented only for `{implementor}`")),
                |diag| {
                    if let Some(objects) = self.objects.get(&declared).map(|objects| objects.iter().copied().filter(|&object| !expanded_externally(cx, object)).collect::<Vec<_>>())
                        && !objects.is_empty()
                    {
                        diag.span_note(objects, with_forced_trimmed_paths!(format!("`dyn {name}` always holds `{implementor}`")));
                    }
                    diag.help(match (local, bound) {
                        (true, _) => with_forced_trimmed_paths!(format!("replace `{name}` with an inherent `impl {implementor}` block")),
                        (false, Some(bound)) => format!("replace `{name}` with the `{bound}` bound and retain behavior in generic functions"),
                        (false, None) => with_forced_trimmed_paths!(format!("replace `{name}` with functions and constants for `{implementor}`")),
                    });
                },
            );
        }
    }
}
