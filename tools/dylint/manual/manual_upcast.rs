use core::iter;

use clippy_utils::diagnostics::span_lint_and_help;
use clippy_utils::{peel_blocks, sym};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxBuildHasher;
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::{TraitItem, TraitItemKind};
use rustc_infer::infer::TyCtxtInferExt as _;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::adjustment::{Adjust, PointerCoercion};
use rustc_middle::ty::{self, Clause, ExistentialPredicateStableCmpExt as _, Interner as _, Ty, TypingEnv, Unnormalized};
use rustc_trait_selection::traits::{Obligation, ObligationCause, ObligationCtxt};
use rustc_type_ir::elaborate::{elaborate, supertraits};

use crate::{expanded_externally, exported, parameter, pointee};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_UPCAST: "trait methods returning `self` as an object of a supertrait or of `Any`");

// --- [OPERATIONS] ----------------------------------------------------------------------

fn upcast(cx: &LateContext<'_>, method: LocalDefId) -> Option<DefId> {
    let tcx = cx.tcx;
    let trait_owner = tcx.local_parent(method);
    if exported(cx, trait_owner) {
        return None;
    }
    let subtrait = trait_owner.to_def_id();
    let method_id = method.to_def_id();
    let raw = tcx.fn_sig(method).instantiate_identity().skip_normalization();
    let signature = raw.skip_binder();
    if !matches!(signature.inputs(), [_]) || !matches!(pointee(cx, signature.output()).kind(), ty::Dynamic(..) | ty::Alias(..)) {
        return None;
    }
    let mut bodies = tcx
        .local_trait_impls(subtrait)
        .iter()
        .filter_map(|implementation| tcx.impl_item_implementor_ids(implementation.to_def_id()).get(&method_id)?.as_local())
        .chain(tcx.defaultness(method).has_value().then_some(method))
        .peekable();
    if bodies.peek().is_none()
        || !bodies.all(|owner| {
            tcx.hir_maybe_body_owned_by(owner).is_some_and(|body| {
                let value = peel_blocks(body.value);
                parameter(body, value) == Some(0) && tcx.typeck(owner).expr_adjustments(value).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Pointer(PointerCoercion::Unsize)))
            })
        })
    {
        return None;
    }
    let region = ty::Region::new_bound(tcx, ty::INNERMOST, ty::BoundRegion { var: ty::BoundVar::from_usize(raw.bound_vars().len()), kind: ty::BoundRegionKind::Anon });
    let (declared, region) = tcx.liberate_late_bound_regions(
        method_id,
        ty::Binder::bind_with_vars((signature, region), tcx.mk_bound_variable_kinds_from_iter(raw.bound_vars().iter().chain([ty::BoundVariableKind::Region(ty::BoundRegionKind::Anon)]))),
    );
    let (infcx, param_env) = tcx.infer_ctxt().build_with_typing_env(TypingEnv::non_body_analysis(tcx, method_id));
    let obligations = ObligationCtxt::new(&infcx);
    let cause = ObligationCause::dummy();
    let declared = obligations.normalize(&cause, param_env, Unnormalized::new(declared));
    let ty::Dynamic(bounds, ..) = pointee(cx, declared.output()).kind() else { return None };
    let supertrait = bounds.principal_def_id()?;
    let inherited = tcx.supertrait_def_ids(subtrait).any(|ancestor| ancestor == supertrait);
    let sized = tcx.generics_require_sized_self(method_id);
    if supertrait == subtrait || !(inherited || sized && tcx.is_diagnostic_item(sym::Any, supertrait)) {
        return None;
    }
    let coercion = (declared.inputs()[0], declared.output());
    let (receiver, output, param_env) = if sized {
        (coercion.0, coercion.1, param_env)
    } else {
        if !tcx.is_dyn_compatible(subtrait) {
            return None;
        }
        let principal = ty::TraitRef::identity(tcx, subtrait);
        let projections = supertraits(tcx, ty::Binder::dummy(principal))
            .flat_map(|bound| tcx.associated_items(bound.def_id()).in_definition_order().map(move |item| (bound, item)))
            .filter(|(_, item)| item.can_have_equality_constraint(tcx) && !item.is_impl_trait_in_trait() && !tcx.generics_require_sized_self(item.def_id))
            .map(|(bound, item)| {
                let kind = if item.is_type() { ty::AliasTermKind::ProjectionTy { def_id: item.def_id } } else { ty::AliasTermKind::ProjectionConst { def_id: item.def_id } };
                let alias = ty::AliasTerm::new_from_args(tcx, kind, bound.skip_binder().args);
                bound.rebind(ty::ExistentialPredicate::Projection(ty::ExistentialProjection::erase_self_ty(
                    tcx,
                    ty::ProjectionClause { projection_term: alias, term: alias.to_term(tcx, ty::IsRigid::No) },
                )))
            });
        let auto_traits = elaborate(tcx, param_env.caller_bounds())
            .filter_only_self()
            .filter_map(Clause::as_trait_clause)
            .filter(|bound| bound.skip_binder().polarity == ty::ClausePolarity::Positive && bound.self_ty().skip_binder().is_self_param() && tcx.trait_is_auto(bound.def_id()))
            .map(|bound| ty::Binder::dummy(ty::ExistentialPredicate::AutoTrait(bound.def_id())));
        let predicates = tcx.mk_poly_existential_predicates_from_iter(
            iter::once(ty::Binder::dummy(ty::ExistentialPredicate::Trait(ty::ExistentialTraitRef::erase_self_ty(tcx, principal))))
                .chain(projections)
                .chain(auto_traits)
                .unique_with_hasher(FxBuildHasher)
                .sorted_by(|left, right| left.skip_binder().stable_cmp(tcx, &right.skip_binder())),
        );
        let object = Ty::new_dynamic(tcx, predicates, region);
        let args = ty::GenericArgs::for_item(tcx, method_id, |parameter, _| {
            let argument = tcx.mk_param_from_def(parameter);
            if argument.as_type().is_some_and(Ty::is_self_param) { object.into() } else { argument }
        });
        let conditions = tcx.clauses_of(method).instantiate_identity(tcx).clauses;
        let original = conditions.iter().copied().map(Unnormalized::skip_normalization);
        let substituted = original
            .clone()
            .filter(|clause| {
                clause
                    .as_projection_clause()
                    .is_none_or(|bound| bound.skip_binder().projection_term.args.iter().flat_map(ty::GenericArg::walk).filter_map(ty::GenericArg::as_type).any(Ty::is_self_param))
            })
            .map(|clause| ty::EarlyBinder::bind(tcx, clause).instantiate(tcx, args).skip_normalization());
        let param_env = obligations.normalize(&cause, param_env, Unnormalized::new(ty::ParamEnv::new(tcx, elaborate(tcx, original.chain(substituted)))));
        let (receiver, output) = obligations.normalize(&cause, param_env, ty::EarlyBinder::bind(tcx, coercion).instantiate(tcx, args));
        (receiver, output, param_env)
    };
    obligations.register_obligation(Obligation::new(tcx, cause, param_env, ty::TraitRef::new(tcx, tcx.lang_items().coerce_unsized_trait()?, [receiver, output])));
    (obligations.evaluate_obligations_error_on_ambiguity().no_errors() && obligations.resolve_regions(method, param_env, [receiver]).is_empty()).then_some(supertrait)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(signature, _) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(upcast) = upcast(cx, item.owner_id.def_id)
        {
            let (method, subtrait) = (item.ident, cx.tcx.item_name(cx.tcx.parent(item.owner_id.to_def_id())));
            let supertrait = cx.tcx.item_name(upcast);
            let (message, help) = if cx.tcx.generics_require_sized_self(item.owner_id.to_def_id()) {
                (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which an unsizing coercion converts"),
                    format!("delete the method and coerce the receiver to `dyn {supertrait}` at each call site"),
                )
            } else {
                (
                    format!("`{method}` returns `self` as `dyn {supertrait}`, which trait upcasting converts from `dyn {subtrait}`"),
                    format!("delete the method and coerce `dyn {subtrait}` to `dyn {supertrait}` at each call site"),
                )
            };
            span_lint_and_help(cx, MANUAL_UPCAST, signature.decl.output.span(), message, None, help);
        }
    }
}
