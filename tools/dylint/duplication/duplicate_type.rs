use core::{cell::RefCell, cmp::Ordering, iter};
use std::{collections::VecDeque, rc::Rc};

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, is_lint_allowed, peel_blocks, peel_hir_expr_while, peel_hir_pat_refs,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::implements_trait_with_env_from_iter,
};
use itertools::Itertools as _;
use rustc_ast::tokenstream::TokenTree;
use rustc_attr_ir::{AttrArgs, AttrItem, Attribute};
use rustc_data_structures::{
    fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet},
    sorted_map::SortedIndexMultiMap,
};
use rustc_errors::listify;
use rustc_hir::{
    Arm, Expr, ExprKind, ImplItemImplKind, MatchSource, PatExpr, PatExprKind, PatKind,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId, ModId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{
    middle::privacy::Level,
    ty::{
        self, AdtDef, AssocKind, EarlyBinder, GenericArg, GenericArgKind, GenericArgs, GenericArgsRef, Ty, TyCtxt, TypeFoldable, TypeVisitableExt as _, TypeckResults, TypingEnv, Unnormalized,
        VariantDef,
    },
};
use rustc_span::{ExpnKind, MacroKind, Span, Symbol};
use rustc_trait_selection::{
    regions::test_region_obligations,
    traits::{Obligation, ObligationCause, ObligationCtxt},
};

use super::{Bodies, canonical, declaration, normalized, owner, substitute_types};
use crate::{accessed_field, associated_trait, constructed, expanded_externally, pattern_fields, signature, trait_impls};

// --- [MODELS] --------------------------------------------------------------------------

struct Case<'tcx> {
    variant: &'tcx VariantDef,
    fields: Vec<(&'tcx ty::FieldDef, Ty<'tcx>)>,
    discriminant: Option<u128>,
}

struct Shape<'tcx> {
    item: DefId,
    cases: Vec<Case<'tcx>>,
}

struct Behavior<'tcx> {
    implementations: Vec<(DefId, LocalDefId)>,
    inherent: SortedIndexMultiMap<u32, Symbol, LocalDefId>,
    attributes: Vec<&'tcx AttrItem>,
}

struct MatchedArm<'tcx> {
    target: AdtDef<'tcx>,
    source: DefId,
    built: DefId,
    copied_payload: Option<bool>,
}

struct Conversion {
    pairs: Option<FxHashMap<DefId, DefId>>,
    retyped: bool,
    spans: Vec<Span>,
}

enum Duplicate {
    Same(DefId, String),
    Generalized(DefId, String),
    Subset(DefId, String, String, Vec<Span>),
    Kind(DefId, String, Vec<Span>),
    Parallel(DefId, bool, Vec<Span>),
}

impl Shape<'_> {
    fn record(&self) -> bool { matches!(&self.cases, [case] if case.variant.def_id == self.item) }
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    bodies: Rc<Bodies<'tcx>>,
    matches: FxIndexMap<(DefId, DefId), Conversion>,
    copies: FxIndexMap<(DefId, DefId), Conversion>,
}

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(_: TyCtxt<'tcx>, bodies: Rc<Bodies<'tcx>>) -> Self { Self { bodies, matches: FxIndexMap::default(), copies: FxIndexMap::default() } }
}

lint!(Pass<'_>; DUPLICATE_TYPE: "local types and variants that repeat another type's fields or variants, and variants that repeat each other's fields");

// --- [OPERATIONS] ----------------------------------------------------------------------

fn converted<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> &'tcx Expr<'tcx> {
    let conversion = |item: Option<DefId>| matches!(item.and_then(|item| associated_trait(cx.tcx, item)).opt_diag_name(cx), Some(sym::Clone | sym::From | sym::Into | sym::ToOwned));
    peel_hir_expr_while(value, |inner| match inner.kind {
        ExprKind::MethodCall(_, receiver, [], _) if conversion(cx.typeck_results().type_dependent_def_id(inner.hir_id)) => Some(receiver),
        ExprKind::Call(_, [argument]) if conversion(fn_def_id(cx, inner)) => Some(argument),
        _ => None,
    })
}

fn fields<'tcx>(tcx: TyCtxt<'tcx>, owner: DefId, variant: &'tcx VariantDef) -> Option<Vec<(&'tcx ty::FieldDef, Ty<'tcx>)>> {
    let args = GenericArgs::identity_for_item(tcx, owner);
    let declared = tcx.mk_type_list_from_iter(variant.fields.iter().map(|field| field.ty(tcx, args).skip_normalization()));
    let held = normalized(tcx, owner, Unnormalized::new(declared))?;
    Some(iter::zip(variant.fields.iter(), held.iter()).collect())
}

fn current(tcx: TyCtxt<'_>, item: DefId) -> bool {
    tcx.lookup_deprecation(item).is_none() && tcx.lookup_stability(item).is_none_or(|stability| !stability.level.is_unstable() || tcx.features().enabled(stability.feature))
}

fn path(tcx: TyCtxt<'_>, item: DefId) -> Option<String> {
    let parents = tcx.visible_parent_map(());
    if item.is_local() {
        return Some(tcx.item_name(item).to_string());
    }
    let mut walked = FxHashSet::default();
    if iter::successors(parents.get(&item), |module| parents.get(module)).take_while(|&&module| walked.insert(module)).all(|&module| current(tcx, module)) {
        return Some(tcx.def_path_str(item));
    }
    let mut queue = VecDeque::from([(item.krate.as_def_id(), tcx.crate_name(item.krate).to_string())]);
    let mut seen = FxHashSet::default();
    while let Some((module, prefix)) = queue.pop_front() {
        for child in tcx.module_children(module).iter().filter(|child| child.vis.is_public()) {
            let named = format!("{prefix}::{}", child.ident);
            match child.res.opt_def_id() {
                Some(def) if def == item => return Some(named),
                Some(def) if tcx.def_kind(def) == DefKind::Mod && current(tcx, def) && seen.insert(def) => queue.push_back((def, named)),
                Some(_) | None => {}
            }
        }
    }
    None
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn matched<'tcx>(cx: &LateContext<'tcx>, arm: &'tcx Arm<'tcx>, source: AdtDef<'tcx>) -> Option<MatchedArm<'tcx>> {
            let typeck = cx.typeck_results();
            let pattern = peel_hir_pat_refs(arm.pat).0;
            let (qpath, node) = match pattern.kind {
                PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) => (qpath, pattern.hir_id),
                PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) => (qpath, hir_id),
                _ => return None,
            };
            let resolved = typeck.qpath_res(&qpath, node);
            let Res::Def(DefKind::Variant | DefKind::Ctor(..), _) = resolved else { return None };
            let variant = source.variant_of_res(resolved);
            let bound = pattern_fields(typeck, pattern)
                .filter_map(|(field, pattern)| match peel_hir_pat_refs(pattern).0.kind {
                    PatKind::Binding(_, binding, _, None) => Some((binding, field)),
                    _ => None,
                })
                .collect::<FxHashMap<_, _>>();
            let body = peel_blocks(arm.body);
            let (built, fields) = constructed(typeck, body)?;
            let target = typeck.expr_ty(body).ty_adt_def().filter(|adt| adt.is_enum())?;
            let reads = fields.map(|(field, value)| (field.did, (value, converted(cx, value)))).collect::<FxHashMap<_, _>>();
            (arm.guard.is_none() && reads.values().all(|&(_, read)| read.res_local_id().is_some_and(|binding| bound.contains_key(&binding)))).then(|| MatchedArm {
                target,
                source: variant.def_id,
                built: built.def_id,
                copied_payload: (built.fields.len() == variant.fields.len()
                    && reads.len() == built.fields.len()
                    && iter::zip(&built.fields, &variant.fields).all(|(target_field, source_field)| {
                        reads.get(&target_field.did).and_then(|&(_, read)| read.res_local_id()).and_then(|binding| bound.get(&binding)) == Some(&source_field.did)
                    }))
                .then(|| reads.values().any(|&(value, read)| typeck.expr_ty(value) != typeck.expr_ty(read))),
            })
        }

        let typeck = cx.typeck_results();
        let record = |conversion: &mut FxIndexMap<(DefId, DefId), Conversion>, key: (DefId, DefId), pairs: FxHashMap<DefId, DefId>, retyped: bool| {
            let held = conversion.entry(key).and_modify(|held| held.pairs = held.pairs.take().filter(|earlier| *earlier == pairs)).or_insert_with(|| Conversion {
                pairs: Some(pairs),
                retyped: false,
                spans: Vec::new(),
            });
            held.retyped |= retyped;
            held.spans.push(expr.span);
        };
        if expanded_externally(cx, expr.span) {
            return;
        }
        if let ExprKind::Match(scrutinee, arms, MatchSource::Normal) = expr.kind
            && let Some(source) = typeck.expr_ty_adjusted(scrutinee).peel_refs().ty_adt_def().filter(|adt| adt.is_enum())
            && arms.len() == source.variants().len()
            && let Some(mapped) = arms.iter().map(|arm| matched(cx, arm, source)).collect::<Option<Vec<_>>>()
            && let [first, rest @ ..] = &mapped
            && first.target.did() != source.did()
            && first.target.variants().len() == arms.len()
            && rest.iter().all(|other| other.target == first.target)
            && (first.target.all_fields().next().is_none() || mapped.iter().all(|arm| arm.copied_payload.is_some()))
            && mapped.iter().map(|arm| arm.source).all_unique()
            && mapped.iter().map(|arm| arm.built).all_unique()
        {
            let pairs = mapped.iter().map(|arm| (arm.built, arm.source)).collect();
            record(&mut self.matches, (source.did(), first.target.did()), pairs, mapped.iter().any(|arm| arm.copied_payload == Some(true)));
        }
        if let Some((variant, assigned)) = constructed(typeck, expr)
            && let Some(built) = typeck.expr_ty(expr).ty_adt_def().filter(|adt| adt.is_struct())
            && !variant.fields.is_empty()
            && assigned.clone().count() == variant.fields.len()
            && let Some(reads) = assigned
                .map(|(field, value)| {
                    let read = converted(cx, value);
                    let ExprKind::Field(base, _) = read.kind else { return None };
                    Some((field.did, accessed_field(typeck, read)?, base.res_local_id()?, typeck.expr_ty_adjusted(base).peel_refs(), typeck.expr_ty(value) != typeck.expr_ty(read)))
                })
                .collect::<Option<Vec<_>>>()
            && let [(_, _, binding, owner, _), rest @ ..] = &reads
            && rest.iter().all(|&(_, _, other_binding, other_owner, _)| other_binding == *binding && other_owner == *owner)
            && let Some(source) = owner.ty_adt_def().filter(|adt| adt.is_struct() && adt.did() != built.did())
        {
            let pairs = reads.iter().map(|&(target, read, ..)| (target, read)).collect();
            record(&mut self.copies, (built.did(), source.did()), pairs, reads.iter().any(|&(.., retyped)| retyped));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn nameable_visibility(tcx: TyCtxt<'_>, item: DefId) -> ty::Visibility<ModId> {
            let Some(local) = item.as_local() else { return tcx.visibility(item) };
            tcx.effective_visibilities(())
                .effective_vis(local)
                .map_or_else(|| ty::Visibility::Restricted(tcx.parent_module_from_def_id(local)), |visibility| *visibility.at_level(Level::Reexported))
                .to_mod_id()
        }

        struct ShapeUnifier<'tcx> {
            tcx: TyCtxt<'tcx>,
            pair: (LocalDefId, DefId),
            bound: FxHashMap<u32, GenericArg<'tcx>>,
            differences: Option<(FxIndexMap<Ty<'tcx>, Ty<'tcx>>, FxHashMap<Ty<'tcx>, Ty<'tcx>>)>,
            shared: bool,
        }

        fn substituted<'tcx, T, W>(tcx: TyCtxt<'tcx>, value: T, item: DefId, with: W) -> T
        where
            T: TypeFoldable<TyCtxt<'tcx>>,
            W: Fn(GenericArgsRef<'tcx>) -> Ty<'tcx>,
        {
            substitute_types(tcx, value, |held| {
                if let ty::Adt(adt, args) = held.kind()
                    && adt.did() == item
                {
                    Some(with(args))
                } else {
                    None
                }
            })
        }

        fn unit(shape: &Shape<'_>) -> bool { !shape.record() && shape.cases.iter().all(|case| case.fields.is_empty()) }

        fn shape(tcx: TyCtxt<'_>, item: DefId) -> Option<Shape<'_>> {
            let adt = tcx.adt_def(item);
            if adt.variant_list_has_applicable_non_exhaustive() || adt.variants().iter().any(VariantDef::field_list_has_applicable_non_exhaustive) {
                return None;
            }
            let mut cases = if adt.is_enum() && !adt.variants().is_empty() {
                adt.discriminants(tcx)
                    .map(|(index, discriminant)| {
                        let variant = adt.variant(index);
                        Some(Case { variant, fields: fields(tcx, item, variant)?, discriminant: Some(discriminant.val) })
                    })
                    .collect::<Option<Vec<_>>>()?
            } else if adt.is_struct()
                && let variant = adt.non_enum_variant()
                && variant.ctor.is_none()
                && variant.fields.len() > 1
                && (item.is_local() || variant.fields.iter().all(|field| field.vis.is_public()))
            {
                return Some(Shape { item, cases: vec![Case { variant, fields: fields(tcx, item, variant)?, discriminant: None }] });
            } else {
                return None;
            };
            cases.sort_by(|left, right| left.variant.name.as_str().cmp(right.variant.name.as_str()));
            Some(Shape { item, cases })
        }

        fn unifier(tcx: TyCtxt<'_>, pair: (LocalDefId, DefId), lenient: bool) -> ShapeUnifier<'_> {
            ShapeUnifier { tcx, pair, bound: FxHashMap::default(), differences: lenient.then(Default::default), shared: false }
        }

        fn unified_ty<'tcx>(unifier: &mut ShapeUnifier<'tcx>, pair: (Ty<'tcx>, Ty<'tcx>)) -> Option<bool> {
            let identity = |item: DefId| GenericArgs::identity_for_item(unifier.tcx, item);
            let mut pending = vec![(GenericArg::from(pair.0), GenericArg::from(pair.1))];
            let mut shared = true;
            while let Some((left, right)) = pending.pop() {
                let matched = match (left.kind(), right.kind()) {
                    _ if left == right && !right.has_param() => true,
                    (GenericArgKind::Type(_), GenericArgKind::Type(right_ty)) if let ty::Param(param) = right_ty.kind() => *unifier.bound.entry(param.index).or_insert(left) == left,
                    (GenericArgKind::Lifetime(_), GenericArgKind::Lifetime(right_region)) if let ty::ReEarlyParam(param) = right_region.kind() =>
                        *unifier.bound.entry(param.index).or_insert(left) == left,
                    (GenericArgKind::Const(_), GenericArgKind::Const(right_constant)) if let ty::ConstKind::Param(param) = right_constant.kind() =>
                        *unifier.bound.entry(param.index).or_insert(left) == left,
                    (GenericArgKind::Type(left_ty), GenericArgKind::Type(right_ty)) => match (left_ty.kind(), right_ty.kind()) {
                        (ty::Adt(left_adt, left_args), ty::Adt(right_adt, right_args)) if left_adt == right_adt => {
                            pending.extend(iter::zip(*left_args, *right_args).rev());
                            true
                        }
                        (ty::Adt(left_adt, left_args), ty::Adt(right_adt, right_args)) if (left_adt.did(), right_adt.did()) == (unifier.pair.0.to_def_id(), unifier.pair.1) =>
                            *left_args == identity(left_adt.did()) && *right_args == identity(right_adt.did()),
                        (&ty::Ref(left_region, left_inner, left_mutability), &ty::Ref(right_region, right_inner, right_mutability)) if left_mutability == right_mutability => {
                            pending.extend([(left_inner.into(), right_inner.into()), (left_region.into(), right_region.into())]);
                            true
                        }
                        (&ty::RawPtr(left_inner, left_mutability), &ty::RawPtr(right_inner, right_mutability)) if left_mutability == right_mutability => {
                            pending.push((left_inner.into(), right_inner.into()));
                            true
                        }
                        (&ty::Slice(left_inner), &ty::Slice(right_inner)) => {
                            pending.push((left_inner.into(), right_inner.into()));
                            true
                        }
                        (&ty::Array(left_inner, left_length), &ty::Array(right_inner, right_length)) => {
                            pending.extend([(left_inner.into(), right_inner.into()), (left_length.into(), right_length.into())]);
                            true
                        }
                        (ty::Tuple(left_items), ty::Tuple(right_items)) if left_items.len() == right_items.len() => {
                            pending.extend(left_items.iter().map(GenericArg::from).zip(right_items.iter().map(GenericArg::from)).rev());
                            true
                        }
                        _ => unifier.differences.as_mut().is_some_and(|(forward, backward)| {
                            shared = false;
                            *forward.entry(left_ty).or_insert(right_ty) == right_ty && *backward.entry(right_ty).or_insert(left_ty) == left_ty
                        }),
                    },
                    _ => left == right,
                };
                if !matched {
                    return None;
                }
            }
            Some(shared)
        }

        fn unified_field<'tcx>(unifier: &mut ShapeUnifier<'tcx>, (left_field, left_ty): (&ty::FieldDef, Ty<'tcx>), (right_field, right_ty): (&ty::FieldDef, Ty<'tcx>)) -> bool {
            let tcx = unifier.tcx;
            let defaulted = |left_default: Option<DefId>, right_default: Option<DefId>| match (left_default, right_default) {
                (None, None) => true,
                (Some(left_value), Some(right_value)) => matches!((tcx.const_eval_poly(left_value), tcx.const_eval_poly(right_value)), (Ok(left_value), Ok(right_value)) if left_value == right_value),
                (Some(_), None) | (None, Some(_)) => false,
            };
            right_field.vis.partial_cmp(left_field.vis, tcx).is_some_and(Ordering::is_ge)
                && left_field.safety == right_field.safety
                && left_field.mut_restriction == right_field.mut_restriction
                && defaulted(left_field.value, right_field.value)
                && unified_ty(unifier, (left_ty, right_ty)).is_some_and(|shared| {
                    unifier.shared |= shared;
                    true
                })
        }

        fn unified_fields<'tcx>(unifier: &mut ShapeUnifier<'tcx>, left: &[(&ty::FieldDef, Ty<'tcx>)], right: &[(&ty::FieldDef, Ty<'tcx>)]) -> bool {
            left.len() == right.len() && iter::zip(left, right).all(|(&left, &right)| left.0.name == right.0.name && unified_field(unifier, left, right))
        }

        fn unified_shapes<'tcx>(unifier: &mut ShapeUnifier<'tcx>, left: &Shape<'tcx>, right: &Shape<'tcx>) -> Option<Ty<'tcx>> {
            (left.record() == right.record()
                && nameable_visibility(unifier.tcx, right.item).partial_cmp(nameable_visibility(unifier.tcx, left.item), unifier.tcx).is_some_and(Ordering::is_ge)
                && unifier.tcx.adt_def(left.item).repr().equal_up_to_seed(&unifier.tcx.adt_def(right.item).repr())
                && unifier.tcx.adt_def(left.item).is_variant_list_non_exhaustive() == unifier.tcx.adt_def(right.item).is_variant_list_non_exhaustive()
                && left.cases.len() == right.cases.len()
                && iter::zip(&left.cases, &right.cases).all(|(left_case, right_case)| {
                    (left.record() || left_case.variant.name == right_case.variant.name)
                        && left_case.variant.ctor_kind() == right_case.variant.ctor_kind()
                        && left_case.variant.is_field_list_non_exhaustive() == right_case.variant.is_field_list_non_exhaustive()
                        && left_case.discriminant == right_case.discriminant
                        && unified_fields(unifier, &left_case.fields, &right_case.fields)
                }))
            .then(|| well_formed(unifier))?
        }

        fn well_formed<'tcx>(unifier: &ShapeUnifier<'tcx>) -> Option<Ty<'tcx>> {
            let tcx = unifier.tcx;
            let (source, target) = unifier.pair;
            let declared = tcx.generics_of(target);
            let args = declared.own_params.iter().try_fold(Vec::with_capacity(declared.count()), |mut args, param| {
                args.push(unifier.bound.get(&param.index).copied().or_else(|| param.default_value(tcx).map(|default| default.instantiate(tcx, args.as_slice()).skip_normalization()))?);
                Some(args)
            })?;
            let held = Ty::new_adt(tcx, tcx.adt_def(target), tcx.mk_args(&args));
            let param_env = tcx.param_env(source);
            let implied = tcx.assumed_wf_types(source).iter().map(|&(ty, _)| ty).collect();
            test_region_obligations(tcx, source, param_env, &implied, |infcx| {
                let ocx = ObligationCtxt::new(infcx);
                ocx.register_obligation(Obligation::new(tcx, ObligationCause::dummy(), param_env, ty::Binder::dummy(ty::ClauseKind::WellFormed(held.into()))));
                ocx.evaluate_obligations_error_on_ambiguity().no_errors().ok_or(())
            })
            .then_some(held)
        }

        fn payloads<'tcx>(cx: &LateContext<'tcx>, item: LocalDefId, shape: &Shape<'tcx>, index: &FxIndexMap<(DefKind, Vec<Symbol>), Vec<&Shape<'tcx>>>) {
            let tcx = cx.tcx;
            let held = shape.cases.iter().filter(|case| case.variant.ctor_kind().is_none() && case.fields.len() > 1).filter_map(|case| {
                let records = index.get(&(DefKind::Struct, case.fields.iter().map(|(field, _)| field.name).collect()))?;
                let (record, held) = records.iter().flat_map(|record| record.cases.iter().map(move |held| (record, held))).find_map(|(record, held)| {
                    let mut unifier = unifier(tcx, (item, record.item), false);
                    (nameable_visibility(tcx, record.item).partial_cmp(nameable_visibility(tcx, item.to_def_id()), tcx).is_some_and(Ordering::is_ge)
                        && !tcx.adt_def(record.item).has_dtor(tcx)
                        && case.variant.is_field_list_non_exhaustive() == held.variant.is_field_list_non_exhaustive()
                        && unified_fields(&mut unifier, &case.fields, &held.fields))
                    .then(|| well_formed(&unifier))?
                    .map(|held| (record, held.to_string()))
                })?;
                let variant = case.variant;
                Some((variant.def_id.as_local()?, variant.name, path(tcx, record.item)?, held))
            });
            for (variant, variant_name, record_name, record_type) in held {
                span_lint_hir_and_then(
                    cx,
                    DUPLICATE_TYPE,
                    tcx.local_def_id_to_hir_id(variant),
                    tcx.def_span(variant),
                    format!("variant `{variant_name}` repeats the fields of `{record_name}`"),
                    |diag| {
                        diag.help(format!("declare `{variant_name}({record_type})`"));
                    },
                );
            }
            let alike = |left: &Case<'tcx>, right: &Case<'tcx>| {
                left.variant.ctor_kind() == right.variant.ctor_kind()
                    && unified_fields(&mut unifier(tcx, (item, shape.item), false), &left.fields, &right.fields)
                    && unified_fields(&mut unifier(tcx, (item, shape.item), false), &right.fields, &left.fields)
            };
            let transparent = sym::transparent;
            let forwarding = |case: &Case<'tcx>| {
                case.variant
                    .def_id
                    .as_local()
                    .into_iter()
                    .flat_map(|variant| tcx.hir_attrs(tcx.local_def_id_to_hir_id(variant)))
                    .filter_map(|attribute| match attribute {
                        Attribute::DeriveHelper(helper)
                            if helper.path.segments.last().is_some_and(|segment| segment.as_str() == "error")
                                && let AttrArgs::Delimited(delimited) = &helper.args =>
                            Some(delimited.tokens.iter().collect::<Vec<_>>()),
                        Attribute::DeriveHelper(_) | Attribute::Tool(_) | Attribute::Parsed(_) => None,
                    })
                    .any(|tokens| matches!(&tokens, [TokenTree::Token(token, _)] if token.is_ident_named(transparent)))
            };
            let every = matches!(&shape.cases, [first, rest @ ..]
                if !rest.is_empty() && (first.variant.ctor_kind() != Some(CtorKind::Fn) && !first.fields.is_empty() || first.fields.len() > 1) && rest.iter().all(|case| alike(case, first)));
            if every {
                span_lint_hir_and_then(
                    cx,
                    DUPLICATE_TYPE,
                    tcx.local_def_id_to_hir_id(item),
                    tcx.def_span(item),
                    format!("every variant of `{}` repeats the same fields", tcx.item_name(item)),
                    |diag| {
                        diag.help(format!("declare `{}` as a struct of the shared fields and a fieldless enum of its variants", tcx.item_name(item)));
                    },
                );
            }
            if !every
                && tcx.get_diagnostic_item(sym::Error).is_some_and(|error| tcx.non_blanket_impls_for_ty(error, tcx.type_of(item).instantiate_identity().skip_normalization()).next().is_some())
                && let Some(group) = shape
                    .cases
                    .iter()
                    .filter(|case| !case.fields.is_empty() && !forwarding(case))
                    .map(|case| shape.cases.iter().filter(|other| !other.fields.is_empty() && !forwarding(other) && alike(other, case)).map(|other| other.variant).collect::<Vec<_>>())
                    .find(|group| group.len() > 1)
                && let Some(listed) = listify(&group, |variant| format!("`{}`", variant.name))
            {
                span_lint_hir_and_then(
                    cx,
                    DUPLICATE_TYPE,
                    tcx.local_def_id_to_hir_id(item),
                    tcx.def_span(item),
                    format!("error variants {listed} of `{}` repeat the same fields", tcx.item_name(item)),
                    |diag| {
                        diag.span_note(group.iter().map(|variant| tcx.def_span(variant.def_id)).collect::<Vec<_>>(), "repeated here");
                        diag.help(format!("replace {listed} with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages"));
                    },
                );
            }
        }

        fn report(cx: &LateContext<'_>, item: LocalDefId, duplicate: Duplicate, derive: Option<DefId>, mapping: Option<DefId>) {
            let tcx = cx.tcx;
            let name = tcx.item_name(item);
            let message = match &duplicate {
                Duplicate::Same(_, other) => format!("`{name}` duplicates `{other}`"),
                Duplicate::Generalized(other, varied) => format!("`{name}` repeats `{}` with {varied}", tcx.item_name(*other)),
                Duplicate::Subset(_, other, copied, _) => format!("`{name}` repeats `{other}` fields {copied}"),
                Duplicate::Kind(_, other, _) => format!("`{name}` repeats the variants of `{other}` without their fields"),
                Duplicate::Parallel(other, _, _) => format!("`{name}` repeats `{}` member by member", tcx.item_name(*other)),
            };
            span_lint_hir_and_then(cx, DUPLICATE_TYPE, tcx.local_def_id_to_hir_id(item), tcx.def_span(item), message, |diag| match duplicate {
                Duplicate::Same(other, text) => {
                    if other.is_local() {
                        diag.span_note(tcx.def_span(other), format!("`{}` declared here", tcx.item_name(other)));
                    }
                    diag.help(format!("use `{text}` in place of `{name}`"));
                }
                Duplicate::Generalized(other, _) => {
                    let other_name = tcx.item_name(other);
                    diag.span_note(tcx.def_span(other), format!("`{other_name}` declared here"));
                    diag.help(format!("use `{other_name}` generic over the differing field types in place of `{name}`"));
                }
                Duplicate::Subset(other, other_name, copied, spans) => {
                    diag.span_note(spans, format!("built from `{other_name}` fields here"));
                    diag.help(if other.is_local() { format!("hold `{name}` in `{other_name}` in place of {copied}") } else { format!("use `{other_name}` in place of `{name}`") });
                }
                Duplicate::Kind(other, other_name, spans) => {
                    diag.span_note(spans, format!("built from a match on `{other_name}` here"));
                    diag.help(match derive.filter(|_| other.is_local()).and_then(|derive| path(tcx, derive)) {
                        Some(derive) => format!("derive `{name}` from `{other_name}` with `{derive}`"),
                        None => format!("match `{other_name}` in place of `{name}`"),
                    });
                }
                Duplicate::Parallel(other, retyped, spans) => {
                    let other_name = tcx.item_name(other);
                    diag.span_note(spans, format!("converted from `{other_name}` here"));
                    diag.help(match (retyped, mapping.and_then(|mapping| path(tcx, mapping))) {
                        (false, _) => format!("use `{other_name}` in place of `{name}`"),
                        (true, Some(mapping)) => format!("derive the conversion of `{name}` from `{other_name}` with `{mapping}`"),
                        (true, None) => format!("use `{other_name}` generic over the differing member types in place of `{name}`"),
                    });
                }
            });
        }

        fn reached(tcx: TyCtxt<'_>) -> FxIndexSet<DefId> {
            let items = tcx.hir_crate_items(());
            let declared = items.definitions().flat_map(|item| {
                let kind = tcx.def_kind(item);
                let signed = kind.is_fn_like().then(|| signature(tcx, item.to_def_id()).inputs_and_output);
                let fields = kind.is_adt().then(|| tcx.adt_def(item).all_fields().map(|field| tcx.type_of(field.did).instantiate_identity().skip_normalization()));
                signed.into_iter().flatten().chain(fields.into_iter().flatten())
            });
            let bodies =
                tcx.hir_body_owners().filter(|&owner| !tcx.is_typeck_child(owner.to_def_id())).flat_map(|owner| tcx.typeck(owner).node_types().items_in_stable_order().into_iter().map(|(_, &ty)| ty));
            declared.chain(bodies).flat_map(Ty::walk).filter_map(|argument| argument.as_type()?.ty_adt_def().map(AdtDef::did)).collect()
        }

        let tcx = cx.tcx;
        let bodies = &self.bodies;
        let items = tcx.hir_crate_items(());
        let locals = items.definitions().filter(|&item| matches!(tcx.def_kind(item), DefKind::Struct | DefKind::Enum)).collect::<Vec<_>>();
        let unchecked = |item: LocalDefId| expanded_externally(cx, tcx.def_span(item)) || is_lint_allowed(cx, DUPLICATE_TYPE, tcx.local_def_id_to_hir_id(item));
        let checked = locals.iter().copied().filter(|&item| !unchecked(item)).collect::<Vec<_>>();
        if checked.is_empty() {
            return;
        }
        let shapes = locals
            .iter()
            .map(|item| item.to_def_id())
            .chain(
                reached(tcx)
                    .into_iter()
                    .filter(|&item| !item.is_local() && tcx.visibility(item).is_public() && tcx.visible_parent_map(()).contains_key(&item) && !tcx.is_doc_hidden(item) && current(tcx, item)),
            )
            .filter_map(|item| shape(tcx, item))
            .filter(|shape| shape.item.is_local() || path(tcx, shape.item).is_some())
            .collect::<Vec<_>>();
        let key = |shape: &Shape<'_>| {
            (
                tcx.def_kind(shape.item),
                if shape.record() { shape.cases.iter().flat_map(|case| &case.fields).map(|(field, _)| field.name).collect() } else { shape.cases.iter().map(|case| case.variant.name).collect() },
            )
        };
        let index = shapes.iter().fold(FxIndexMap::<(DefKind, Vec<Symbol>), Vec<&Shape<'tcx>>>::default(), |mut index, shape| {
            index.entry(key(shape)).or_default().push(shape);
            index
        });
        let owned = shapes.iter().filter(|shape| shape.item.is_local()).map(|shape| (shape.item, shape)).collect::<FxHashMap<_, _>>();
        let by_item = shapes.iter().map(|shape| (shape.item, shape)).collect::<FxHashMap<_, _>>();
        let position = locals.iter().enumerate().map(|(order, &item)| (item.to_def_id(), order)).collect::<FxHashMap<_, _>>();
        let impls = trait_impls(tcx);
        let projects = |source: DefId, kind: &Shape<'tcx>| {
            by_item.get(&source).is_some_and(|full| !full.record() && !unit(full) && full.cases.iter().map(|case| case.variant.name).eq(kind.cases.iter().map(|case| case.variant.name)))
        };
        let tool_attributes = |item: LocalDefId| {
            let adt = tcx.adt_def(item);
            iter::once(item.to_def_id())
                .chain(adt.variants().iter().map(|variant| variant.def_id).filter(|&variant| variant != item.to_def_id()))
                .chain(adt.all_fields().map(|field| field.did))
                .filter_map(DefId::as_local)
                .flat_map(|owner| tcx.hir_attrs(tcx.local_def_id_to_hir_id(owner)))
                .filter_map(|attribute| match attribute {
                    Attribute::Tool(unparsed) | Attribute::DeriveHelper(unparsed) => Some(&**unparsed),
                    Attribute::Parsed(_) => None,
                })
                .collect::<Vec<&AttrItem>>()
        };
        let inherent = items
            .impl_items()
            .map(|item| tcx.hir_impl_item(item))
            .filter(|item| matches!(item.impl_kind, ImplItemImplKind::Inherent { .. }))
            .filter_map(|item| {
                let implementation = tcx.local_parent(item.owner_id.def_id);
                let adt = tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def()?.did().as_local()?;
                Some((adt, (item.ident.name, item.owner_id.def_id)))
            })
            .fold(FxHashMap::<LocalDefId, Vec<(Symbol, LocalDefId)>>::default(), |mut inherent, (adt, member)| {
                inherent.entry(adt).or_default().push(member);
                inherent
            });
        let metadata = RefCell::new(FxHashMap::default());
        let behavior = |item: LocalDefId| {
            Rc::clone(metadata.borrow_mut().entry(item).or_insert_with(|| {
                Rc::new(Behavior {
                    implementations: impls.get(&item).into_iter().flatten().copied().filter(|&(_, implementation)| !tcx.is_builtin_derived(implementation.to_def_id())).collect(),
                    inherent: inherent.get(&item).into_iter().flatten().copied().collect(),
                    attributes: tool_attributes(item),
                })
            }))
        };
        let member_cache = RefCell::new(FxHashMap::default());
        let associated = |implementation: LocalDefId| {
            Rc::clone(member_cache.borrow_mut().entry(implementation).or_insert_with(|| {
                Rc::new(
                    tcx.hir_expect_item(implementation)
                        .expect_impl()
                        .items
                        .iter()
                        .map(|&item| tcx.hir_impl_item(item))
                        .map(|item| (item.ident.name, item.owner_id.def_id))
                        .collect::<SortedIndexMultiMap<u32, Symbol, LocalDefId>>(),
                )
            }))
        };
        let clause_cache = RefCell::new(FxHashMap::default());
        let clauses = |item: LocalDefId, owner: Option<Ty<'tcx>>| {
            Rc::clone(clause_cache.borrow_mut().entry((item, owner)).or_insert_with(|| {
                Rc::new(tcx.clauses_of(item).instantiate_identity(tcx).clauses.into_iter().map(|clause| declaration(tcx, clause.skip_normalization(), owner)).collect::<FxHashSet<_>>())
            }))
        };
        let equivalent = |first, second| first == second;
        let same_member = |first: LocalDefId, second: LocalDefId, first_owner: Option<Ty<'tcx>>, second_owner: Option<Ty<'tcx>>| {
            let [first_item, second_item] = [first, second].map(|item| tcx.associated_item(item));
            let differ = |first_typeck: &TypeckResults<'tcx>, first_expr: &Expr<'_>, second_typeck: &TypeckResults<'tcx>, second_expr: &Expr<'_>| {
                let (first_type, second_type) = (first_typeck.expr_ty(first_expr), second_typeck.expr_ty(second_expr));
                matches!((first_expr.kind, second_expr.kind), (ExprKind::Path(_), ExprKind::Path(_)))
                    && first_type != second_type
                    && canonical(tcx, first_type, first_owner) == canonical(tcx, second_type, second_owner)
            };
            first_item.tag() == second_item.tag()
                && tcx.visibility(first) == tcx.visibility(second)
                && first_item.defaultness(tcx) == second_item.defaultness(tcx)
                && clauses(first, first_owner) == clauses(second, second_owner)
                && declaration(tcx, tcx.type_of(tcx.local_parent(first)).instantiate_identity().skip_normalization(), first_owner)
                    == declaration(tcx, tcx.type_of(tcx.local_parent(second)).instantiate_identity().skip_normalization(), second_owner)
                && match first_item.kind {
                    AssocKind::Fn { .. } =>
                        declaration(tcx, first_owner, None) == declaration(tcx, owner(tcx, first), None)
                            && declaration(tcx, second_owner, None) == declaration(tcx, owner(tcx, second), None)
                            && bodies.callable(tcx, first).zip(bodies.callable(tcx, second)).is_some_and(|(first, second)| first == second)
                            && bodies.equal(cx, first, second, &differ, equivalent).is_some(),
                    AssocKind::Const { .. } | AssocKind::Type { .. } =>
                        declaration(tcx, tcx.type_of(first).instantiate_identity().skip_normalization(), first_owner)
                            == declaration(tcx, tcx.type_of(second).instantiate_identity().skip_normalization(), second_owner)
                            && (!matches!(first_item.kind, AssocKind::Const { .. }) || bodies.equal(cx, first, second, &differ, equivalent).is_some()),
                }
        };
        let comparisons = RefCell::new(FxHashMap::default());
        let distinct = |left: LocalDefId, right: LocalDefId| {
            *comparisons.borrow_mut().entry((left, right)).or_insert_with(|| {
                let [left_owner, right_owner] = [left, right].map(|item| Some(tcx.type_of(item).instantiate_identity().skip_normalization()));
                let (left_behavior, right_behavior) = (behavior(left), behavior(right));
                let (left_impls, right_impls) = (&left_behavior.implementations, &right_behavior.implementations);
                let (left_tools, right_tools) = (&left_behavior.attributes, &right_behavior.attributes);
                let same_tools = left_tools.len() == right_tools.len()
                    && iter::zip(left_tools, right_tools).all(|(left_tool, right_tool)| {
                        left_tool.path.segments == right_tool.path.segments
                            && match (&left_tool.args, &right_tool.args) {
                                (AttrArgs::Empty, AttrArgs::Empty) => true,
                                (AttrArgs::Delimited(left_args), AttrArgs::Delimited(right_args)) =>
                                    left_args.delim == right_args.delim
                                        && left_args.tokens.len() == right_args.tokens.len()
                                        && iter::zip(left_args.tokens.iter(), right_args.tokens.iter()).all(|(left_token, right_token)| left_token.eq_unspanned(right_token)),
                                (AttrArgs::Eq { expr: left_value, .. }, AttrArgs::Eq { expr: right_value, .. }) => left_value.kind == right_value.kind,
                                (AttrArgs::Empty | AttrArgs::Delimited(_) | AttrArgs::Eq { .. }, _) => false,
                            }
                    });
                let alike = |left_implementation: LocalDefId, right_implementation: LocalDefId| {
                    let [left_header, right_header] = [left_implementation, right_implementation].map(|item| tcx.impl_trait_header(item));
                    let (left_members, right_members) = (associated(left_implementation), associated(right_implementation));
                    declaration(tcx, left_header.trait_ref.instantiate_identity(), left_owner) == declaration(tcx, right_header.trait_ref.instantiate_identity(), right_owner)
                        && left_header.polarity == right_header.polarity
                        && left_header.safety == right_header.safety
                        && left_header.constness == right_header.constness
                        && tcx.defaultness(left_implementation) == tcx.defaultness(right_implementation)
                        && clauses(left_implementation, left_owner) == clauses(right_implementation, right_owner)
                        && left_members.len() == right_members.len()
                        && left_members.iter().all(|(&name, &member)| right_members.get_by_key(name).any(|&counterpart| same_member(member, counterpart, left_owner, right_owner)))
                };
                !same_tools
                    || left_impls.len() != right_impls.len()
                    || left_impls.iter().any(|&(implemented, implementation)| !right_impls.iter().any(|&(other, counterpart)| implemented == other && alike(implementation, counterpart)))
                    || left_behavior.inherent.len() != right_behavior.inherent.len()
                    || left_behavior.inherent.iter().any(|(&name, &member)| !right_behavior.inherent.get_by_key(name).any(|&counterpart| same_member(member, counterpart, left_owner, right_owner)))
            })
        };
        let duplicate = |item: LocalDefId, left: &Shape<'tcx>| {
            let has_no_own_behavior = |owner: Ty<'tcx>| {
                let behavior = behavior(item);
                behavior.attributes.is_empty()
                    && behavior.inherent.is_empty()
                    && behavior.implementations.is_empty()
                    && impls.get(&item).into_iter().flatten().all(|&(_, implementation)| {
                        let trait_ref = tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization();
                        let replaced = substituted(tcx, trait_ref, item.to_def_id(), |args| EarlyBinder::bind(tcx, owner).instantiate(tcx, args).skip_normalization());
                        implements_trait_with_env_from_iter(tcx, TypingEnv::non_body_analysis(tcx, implementation), replaced.self_ty(), replaced.def_id, None, replaced.args.iter().skip(1))
                    })
            };
            let candidates = || index.get(&key(left)).into_iter().flatten().filter(|right| right.item != left.item);
            let earlier = |right: &Shape<'_>| position.get(&right.item) < position.get(&left.item);
            let dependency = candidates().filter(|right| !right.item.is_local()).find_map(|right| {
                let mut unifier = unifier(tcx, (item, right.item), false);
                unified_shapes(&mut unifier, left, right).filter(|&held| has_no_own_behavior(held)).map(|held| Duplicate::Same(right.item, held.to_string()))
            });
            let local = || {
                candidates().filter_map(|right| Some((right, right.item.as_local()?))).find_map(|(right, other)| {
                    let mut forward = unifier(tcx, (item, right.item), false);
                    let held = unified_shapes(&mut forward, left, right).filter(|_| !distinct(item, other))?;
                    let general = unified_shapes(&mut unifier(tcx, (other, left.item), false), right, left).is_some();
                    (!general || earlier(right)).then(|| Duplicate::Same(right.item, held.to_string()))
                })
            };
            let generalized = || {
                candidates().filter(|right| right.item.is_local() && earlier(right)).find_map(|right| {
                    let mut unifier = unifier(tcx, (item, right.item), true);
                    (unified_shapes(&mut unifier, left, right).is_some() && unifier.shared && right.item.as_local().is_some_and(|other| !distinct(item, other)))
                        .then_some(unifier.differences)
                        .flatten()
                        .and_then(|(differences, _)| listify(&differences.into_iter().collect::<Vec<_>>(), |(from, to)| format!("`{from}` in place of `{to}`")))
                        .map(|varied| Duplicate::Generalized(right.item, varied))
                })
            };
            let subset = || {
                self.copies.iter().filter(|&(&(copy, _), conversion)| copy == left.item && left.record() && !conversion.retyped).find_map(|(&(_, source), conversion)| {
                    let pairs = conversion.pairs.as_ref()?;
                    let held = fields(tcx, source, tcx.adt_def(source).non_enum_variant())?;
                    let mut unifier = unifier(tcx, (item, source), false);
                    let copied = left.cases.iter().flat_map(|case| &case.fields).collect::<Vec<_>>();
                    let within =
                        copied.iter().all(|field| held.iter().find(|other| pairs.get(&field.0.did) == Some(&other.0.did)).is_some_and(|other| unified_fields(&mut unifier, &[**field], &[*other])));
                    (copied.len() < held.len() && within)
                        .then(|| Some(Duplicate::Subset(source, path(tcx, source)?, listify(&copied, |field| format!("`{}`", field.0.name))?, conversion.spans.clone())))?
                })
            };
            let kind = || {
                self.matches.iter().filter(|&(&(source, target), _)| target == left.item && unit(left) && projects(source, left)).find_map(|(&(source, _), conversion)| {
                    conversion
                        .pairs
                        .as_ref()?
                        .iter()
                        .all(|(&target_variant, &source_variant)| tcx.item_name(target_variant) == tcx.item_name(source_variant))
                        .then(|| Some(Duplicate::Kind(source, path(tcx, source)?, conversion.spans.clone())))?
                })
            };
            dependency.or_else(local).or_else(generalized).or_else(subset).or_else(kind)
        };
        let parallel = |item: LocalDefId| {
            let members = |owner: DefId| {
                let adt = tcx.adt_def(owner);
                if adt.is_enum() { adt.variants().iter().map(|variant| variant.def_id).collect::<FxHashSet<_>>() } else { adt.all_fields().map(|field| field.did).collect() }
            };
            let copies = self.copies.iter().map(|(&(target, source), conversion)| (target, source, conversion));
            copies.chain(self.matches.iter().map(|(&(source, target), conversion)| (target, source, conversion))).filter(|&(target, _, _)| target == item.to_def_id()).find_map(
                |(target, source, conversion)| {
                    let pairs = conversion.pairs.as_ref()?;
                    source.as_local().filter(|&local| !distinct(item, local))?;
                    let built = members(target);
                    let read = pairs.values().copied().collect::<FxHashSet<_>>();
                    if !(pairs.len() == built.len() && pairs.keys().all(|field| built.contains(field)) && read == members(source) && read.len() == pairs.len()) {
                        return None;
                    }
                    if conversion.retyped {
                        return Some(Duplicate::Parallel(source, true, conversion.spans.clone()));
                    }
                    let (target_adt, source_adt) = (tcx.adt_def(target), tcx.adt_def(source));
                    if !(target_adt.repr().equal_up_to_seed(&source_adt.repr())
                        && nameable_visibility(tcx, source).partial_cmp(nameable_visibility(tcx, target), tcx).is_some_and(Ordering::is_ge)
                        && target_adt.is_variant_list_non_exhaustive() == source_adt.is_variant_list_non_exhaustive())
                    {
                        return None;
                    }
                    let mut unifier = unifier(tcx, (item, source), false);
                    let mut corresponding = |target_variant: &'tcx VariantDef, source_variant: &'tcx VariantDef| {
                        target_variant.is_field_list_non_exhaustive() == source_variant.is_field_list_non_exhaustive()
                            && fields(tcx, target, target_variant)
                                .zip(fields(tcx, source, source_variant))
                                .is_some_and(|(left, right)| left.len() == right.len() && iter::zip(left, right).all(|(left, right)| unified_field(&mut unifier, left, right)))
                    };
                    let same_fields = if target_adt.is_enum() {
                        pairs
                            .iter()
                            .map(|(&target_case, &source_case)| (target_adt.variant_index_with_id(target_case), source_adt.variant_index_with_id(source_case)))
                            .map(|(left, right)| (target_adt.discriminant_for_variant(tcx, left), source_adt.discriminant_for_variant(tcx, right), target_adt.variant(left), source_adt.variant(right)))
                            .all(|(left, right, target_variant, source_variant)| (left.val, left.ty) == (right.val, right.ty) && corresponding(target_variant, source_variant))
                    } else {
                        let (target_variant, source_variant) = (target_adt.non_enum_variant(), source_adt.non_enum_variant());
                        target_variant.fields.iter().zip(source_variant.fields.iter()).all(|(left, right)| pairs.get(&left.did) == Some(&right.did)) && corresponding(target_variant, source_variant)
                    };
                    (same_fields && well_formed(&unifier).is_some()).then(|| Duplicate::Parallel(source, false, conversion.spans.clone()))
                },
            )
        };
        let kind = |item: DefId| owned.get(&item).is_some_and(|generated| unit(generated) && owned.values().any(|other| projects(other.item, generated)));
        let conversion = |item: DefId| {
            matches!(tcx.def_kind(item), DefKind::Impl { of_trait: true })
                && tcx.is_diagnostic_item(sym::From, tcx.impl_trait_id(item))
                && tcx.impl_trait_ref(item).instantiate_identity().skip_normalization().args.types().all(|converted| converted.ty_adt_def().is_some_and(|adt| adt.is_struct() && adt.did().is_local()))
        };
        let derived = |converts: bool| {
            items.definitions().filter(|&item| expanded_externally(cx, tcx.def_span(item))).find_map(|item| {
                let expansion = tcx.def_span(item).ctxt().outer_expn_data();
                let produced = if converts { conversion(item.to_def_id()) } else { kind(item.to_def_id()) };
                (matches!(expansion.kind, ExpnKind::Macro(MacroKind::Derive, _)) && produced).then_some(expansion.macro_def_id?)
            })
        };
        let (derive, mapping) = (derived(false), derived(true));
        let mut same = FxHashSet::default();
        for item in checked {
            let left = owned.get(&item.to_def_id()).copied().filter(|left| left.record() || tcx.adt_def(item).is_enum());
            if let Some(duplicate) = left.and_then(|left| duplicate(item, left)).or_else(|| parallel(item))
                && !matches!(duplicate, Duplicate::Generalized(other, _) if same.contains(&(other, item.to_def_id())))
            {
                if let Duplicate::Same(other, _) = duplicate {
                    same.insert((item.to_def_id(), other));
                }
                report(cx, item, duplicate, derive, mapping);
            }
            if let Some(left) = left.filter(|left| !left.record()) {
                payloads(cx, item, left, &index);
            }
        }
    }
}
