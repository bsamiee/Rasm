use core::iter;
use std::collections::VecDeque;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then, fn_def_id, is_lint_allowed, last_path_segment, peel_blocks, peel_hir_expr_while, peel_hir_pat_refs, res::MaybeResPath as _, str_utils::to_camel_case, sym,
    ty::implements_trait_with_env,
};
use itertools::Itertools as _;
use rustc_attr_ir::{AttrArgs, AttrItem, Attribute};
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet};
use rustc_hir::{
    Arm, Expr, ExprKind, MatchSource, PatExpr, PatExprKind, PatKind,
    def::{CtorKind, DefKind},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, Level, declare_lint, impl_lint_pass};
use rustc_middle::ty::{
    self, AdtDef, AssocKind, BottomUpFolder, EarlyBinder, GenericArgKind, GenericArgs, GenericArgsRef, Ty, TyCtxt, TypeFoldable, TypeVisitableExt as _, TypeckResults, TypingEnv, VariantDef,
    VariantDiscr,
};
use rustc_span::{ExpnKind, MacroKind, Span, Symbol};

use crate::{accessed_field, constructed, equal_bodies};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    RESTATED_TYPE,
    Warn,
    "local types, variants, and enums whose declared shape restates a type a dependency or the crate declares"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Field<'tcx> {
    name: Symbol,
    ty: Ty<'tcx>,
    default: Option<DefId>,
}

struct Case<'tcx> {
    key: Option<Symbol>,
    name: Option<Symbol>,
    ctor: Option<CtorKind>,
    fields: Vec<Field<'tcx>>,
    discriminant: Option<u128>,
}

struct Shape<'tcx> {
    item: DefId,
    explicit: bool,
    cases: Vec<Case<'tcx>>,
}

struct Mirrored<'tcx> {
    target: AdtDef<'tcx>,
    source: DefId,
    built: DefId,
    retyped: bool,
}

struct Evidence {
    pairs: Vec<(DefId, DefId)>,
    retyped: bool,
    spans: Vec<Span>,
}

#[derive(PartialEq, Eq, Hash)]
enum Key {
    Record(Vec<Symbol>),
    Cases(Vec<Option<Symbol>>),
}

enum Restated {
    Same(DefId, String),
    Generalized(DefId),
    Subset(DefId, String, Vec<Span>),
    Kind(DefId, String, Vec<Span>),
    Parallel(DefId, bool, Vec<Span>),
}

impl Shape<'_> {
    const fn record(&self) -> bool { matches!(self.cases.as_slice(), [case] if case.name.is_none() && case.ctor.is_none()) }
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct RestatedType {
    mirrors: FxIndexMap<(DefId, DefId), Evidence>,
    copies: FxIndexMap<(DefId, DefId), Evidence>,
}

impl_lint_pass!(RestatedType => [RESTATED_TYPE]);

impl<'tcx> LateLintPass<'tcx> for RestatedType {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn mirrored<'tcx>(cx: &LateContext<'tcx>, arm: &'tcx Arm<'tcx>, source: AdtDef<'tcx>) -> Option<Mirrored<'tcx>> {
            let typeck = cx.typeck_results();
            let pattern = peel_hir_pat_refs(arm.pat).0;
            let (PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) | PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id: _, span: _ })) = pattern.kind else { return None };
            let variant = source.variants().iter().find(|variant| variant.name == last_path_segment(&qpath).ident.name)?;
            let mut bound = FxHashSet::default();
            pattern.each_binding(|_, binding, _, _| {
                bound.insert(binding);
            });
            let body = peel_blocks(arm.body);
            let (built, fields) = constructed(typeck, body)?;
            let target = typeck.expr_ty(body).ty_adt_def().filter(|adt| adt.is_enum())?;
            let reads = fields.iter().map(|&(_, value)| (value, converted(cx, value))).collect::<Vec<_>>();
            (arm.guard.is_none() && reads.iter().all(|&(_, read)| read.res_local_id().is_some_and(|binding| bound.contains(&binding)))).then(|| Mirrored {
                target,
                source: variant.def_id,
                built: built.def_id,
                retyped: reads.iter().any(|&(value, read)| typeck.expr_ty(value) != typeck.expr_ty(read)),
            })
        }

        let typeck = cx.typeck_results();
        let record = |evidence: &mut FxIndexMap<(DefId, DefId), Evidence>, key: (DefId, DefId), pairs: Vec<(DefId, DefId)>, retyped: bool| {
            let held = evidence.entry(key).or_insert_with(|| Evidence { pairs, retyped: false, spans: Vec::new() });
            held.retyped |= retyped;
            held.spans.push(expr.span);
        };
        if expr.span.from_expansion() {
            return;
        }
        if let ExprKind::Match(scrutinee, arms, MatchSource::Normal) = expr.kind
            && let Some(source) = typeck.expr_ty_adjusted(scrutinee).peel_refs().ty_adt_def().filter(|adt| adt.is_enum())
            && arms.len() == source.variants().len()
            && let Some(mapped) = arms.iter().map(|arm| mirrored(cx, arm, source)).collect::<Option<Vec<_>>>()
            && let [first, rest @ ..] = &mapped
            && first.target.did() != source.did()
            && first.target.variants().len() == arms.len()
            && rest.iter().all(|other| other.target == first.target)
            && mapped.iter().map(|arm| arm.source).collect::<FxHashSet<_>>().len() == arms.len()
            && mapped.iter().map(|arm| arm.built).collect::<FxHashSet<_>>().len() == arms.len()
        {
            let pairs = mapped.iter().map(|arm| (arm.built, arm.source)).collect();
            record(&mut self.mirrors, (source.did(), first.target.did()), pairs, mapped.iter().any(|arm| arm.retyped));
        }
        if let Some((variant, assigned)) = constructed(typeck, expr)
            && let Some(built) = typeck.expr_ty(expr).ty_adt_def().filter(|adt| adt.is_struct())
            && !assigned.is_empty()
            && assigned.len() == variant.fields.len()
            && let Some(reads) = assigned
                .iter()
                .map(|&(field, value)| {
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
        struct Unifier<'tcx> {
            tcx: TyCtxt<'tcx>,
            pair: (DefId, DefId),
            bound: FxHashMap<u32, Ty<'tcx>>,
            differences: Option<Vec<(Ty<'tcx>, Ty<'tcx>)>>,
            shared: bool,
        }

        fn substituted<'tcx, T, W>(tcx: TyCtxt<'tcx>, value: T, item: DefId, with: W) -> T
        where
            T: TypeFoldable<TyCtxt<'tcx>>,
            W: Fn(GenericArgsRef<'tcx>) -> Ty<'tcx>,
        {
            value.fold_with(&mut BottomUpFolder {
                tcx,
                ty_op: |ty: Ty<'tcx>| {
                    if let ty::Adt(adt, args) = ty.kind()
                        && adt.did() == item
                    {
                        with(args)
                    } else {
                        ty
                    }
                },
                lt_op: |region| region,
                ct_op: |constant| constant,
            })
        }

        fn unit(shape: &Shape<'_>) -> bool { !shape.record() && shape.cases.iter().all(|case| case.fields.is_empty()) }

        fn shape(tcx: TyCtxt<'_>, item: DefId) -> Option<Shape<'_>> {
            fn count<'tcx>(tcx: TyCtxt<'tcx>, ty: Ty<'tcx>, open: &[DefId]) -> Option<u128> {
                if ty.is_bool() {
                    Some(2)
                } else if let ty::Tuple(items) = ty.kind() {
                    items.iter().try_fold(1_u128, |product, item| product.checked_mul(count(tcx, item, open)?))
                } else if let ty::Adt(adt, args) = ty.kind()
                    && !adt.is_union()
                    && !open.contains(&adt.did())
                {
                    let nested = [open, &[adt.did()]].concat();
                    adt.variants()
                        .iter()
                        .map(|variant| variant.fields.iter().try_fold(1_u128, |product, field| product.checked_mul(count(tcx, field.ty(tcx, args).skip_normalization(), &nested)?)))
                        .try_fold(0_u128, |sum, product| sum.checked_add(product?))
                } else {
                    None
                }
            }

            let adt = tcx.adt_def(item);
            let explicit = adt.variants().iter().any(|variant| matches!(variant.discr, VariantDiscr::Explicit(_)));
            let mut cases = if adt.is_enum() && !adt.variants().is_empty() {
                adt.discriminants(tcx)
                    .map(|(index, discriminant)| {
                        let variant = adt.variant(index);
                        Case { key: Some(variant.name), name: Some(variant.name), ctor: variant.ctor_kind(), fields: fields(tcx, item, variant), discriminant: Some(discriminant.val) }
                    })
                    .collect::<Vec<_>>()
            } else if adt.is_struct()
                && let variant = adt.non_enum_variant()
                && variant.ctor.is_none()
                && variant.fields.len() > 1
                && (item.is_local() || variant.fields.iter().all(|field| field.vis.is_public()))
            {
                return Some(Shape { item, explicit: false, cases: vec![Case { key: None, name: None, ctor: None, fields: fields(tcx, item, variant), discriminant: None }] });
            } else if adt.is_struct() {
                let own = tcx.type_of(item).instantiate_identity().skip_normalization();
                let constants = tcx
                    .inherent_impls(item)
                    .iter()
                    .flat_map(|&implementation| tcx.associated_items(implementation).in_definition_order())
                    .filter_map(|associated| if let AssocKind::Const { name } = associated.kind { Some((associated.def_id, name)) } else { None })
                    .filter(|&(constant, _)| tcx.visibility(constant).is_public() && tcx.type_of(constant).instantiate_identity().skip_normalization() == own)
                    .map(|(_, name)| Case { key: Some(Symbol::intern(&to_camel_case(&name.as_str().to_lowercase()))), name: None, ctor: Some(CtorKind::Const), fields: Vec::new(), discriminant: None })
                    .collect::<Vec<_>>();
                (!constants.is_empty() && count(tcx, own, &[]) == u128::try_from(constants.len()).ok()).then_some(constants)?
            } else {
                return None;
            };
            cases.sort_by(|left, right| left.key.as_ref().map(Symbol::as_str).cmp(&right.key.as_ref().map(Symbol::as_str)));
            Some(Shape { item, explicit, cases })
        }

        fn unifier(tcx: TyCtxt<'_>, pair: (DefId, DefId), lenient: bool) -> Unifier<'_> { Unifier { tcx, pair, bound: FxHashMap::default(), differences: lenient.then(Vec::new), shared: false } }

        fn unified_ty<'tcx>(unifier: &mut Unifier<'tcx>, left: Ty<'tcx>, right: Ty<'tcx>) -> bool {
            let identity = |item: DefId| GenericArgs::identity_for_item(unifier.tcx, item);
            match (left.kind(), right.kind()) {
                _ if left == right && !right.has_param() => true,
                (_, ty::Param(param)) => *unifier.bound.entry(param.index).or_insert(left) == left,
                (ty::Adt(left_adt, left_args), ty::Adt(right_adt, right_args)) if left_adt == right_adt =>
                    iter::zip(*left_args, *right_args).all(|(left_arg, right_arg)| match (left_arg.kind(), right_arg.kind()) {
                        (GenericArgKind::Type(left_inner), GenericArgKind::Type(right_inner)) => unified_ty(unifier, left_inner, right_inner),
                        (GenericArgKind::Lifetime(_), GenericArgKind::Lifetime(_)) => true,
                        (left_kind, right_kind) => left_kind == right_kind,
                    }),
                (ty::Adt(left_adt, left_args), ty::Adt(right_adt, right_args)) if (left_adt.did(), right_adt.did()) == unifier.pair =>
                    *left_args == identity(left_adt.did()) && *right_args == identity(right_adt.did()),
                (&ty::Ref(_, left_inner, left_mutability), &ty::Ref(_, right_inner, right_mutability)) | (&ty::RawPtr(left_inner, left_mutability), &ty::RawPtr(right_inner, right_mutability))
                    if left_mutability == right_mutability =>
                    unified_ty(unifier, left_inner, right_inner),
                (&ty::Slice(left_inner), &ty::Slice(right_inner)) => unified_ty(unifier, left_inner, right_inner),
                (&ty::Array(left_inner, left_length), &ty::Array(right_inner, right_length)) if left_length == right_length => unified_ty(unifier, left_inner, right_inner),
                (ty::Tuple(left_items), ty::Tuple(right_items)) if left_items.len() == right_items.len() =>
                    iter::zip(*left_items, *right_items).all(|(left_item, right_item)| unified_ty(unifier, left_item, right_item)),
                _ => unifier.differences.as_mut().is_some_and(|differences| {
                    let consistent = differences.iter().all(|&(from, to)| (from == left) == (to == right));
                    differences.push((left, right));
                    consistent
                }),
            }
        }

        fn unified_fields<'tcx>(unifier: &mut Unifier<'tcx>, left: &[Field<'tcx>], right: &[Field<'tcx>]) -> bool {
            let tcx = unifier.tcx;
            let defaulted = |left_default: Option<DefId>, right_default: Option<DefId>| match (left_default, right_default) {
                (None, None) => true,
                (Some(left_value), Some(right_value)) => matches!((tcx.const_eval_poly(left_value), tcx.const_eval_poly(right_value)), (Ok(left_value), Ok(right_value)) if left_value == right_value),
                (Some(_), None) | (None, Some(_)) => false,
            };
            left.len() == right.len()
                && iter::zip(left, right).all(|(left_field, right_field)| {
                    let before = unifier.differences.as_ref().map_or(0, Vec::len);
                    let matched = left_field.name == right_field.name && defaulted(left_field.default, right_field.default) && unified_ty(unifier, left_field.ty, right_field.ty);
                    unifier.shared |= matched && unifier.differences.as_ref().map_or(0, Vec::len) == before;
                    matched
                })
        }

        fn unified_shapes<'tcx>(unifier: &mut Unifier<'tcx>, left: &Shape<'tcx>, right: &Shape<'tcx>) -> bool {
            left.record() == right.record()
                && left.cases.len() == right.cases.len()
                && iter::zip(&left.cases, &right.cases).all(|(left_case, right_case)| {
                    left_case.key == right_case.key
                        && (left_case.name == right_case.name || left_case.name.is_none() || right_case.name.is_none())
                        && left_case.ctor == right_case.ctor
                        && (!left.explicit || left_case.discriminant.is_some() && left_case.discriminant == right_case.discriminant)
                        && unified_fields(unifier, &left_case.fields, &right_case.fields)
                })
        }

        fn instance<'tcx>(unifier: &Unifier<'tcx>, item: DefId) -> Ty<'tcx> {
            let tcx = unifier.tcx;
            let args = GenericArgs::for_item(tcx, item, |param, _| match (param.kind.is_ty_or_const(), unifier.bound.get(&param.index)) {
                (false, _) => tcx.lifetimes.re_erased.into(),
                (true, Some(&ty)) => ty.into(),
                (true, None) => tcx.mk_param_from_def(param),
            });
            Ty::new_adt(tcx, tcx.adt_def(item), args)
        }

        fn text<'tcx>(tcx: TyCtxt<'tcx>, instance: Ty<'tcx>) -> Option<String> {
            let ty::Adt(adt, args) = instance.kind() else { return None };
            let named = path(tcx, adt.did())?;
            Some(if args.types().next().is_none() { named } else { format!("{named}<{}>", args.types().join(", ")) })
        }

        fn payloads<'tcx>(cx: &LateContext<'tcx>, item: LocalDefId, shape: &Shape<'tcx>, index: &FxIndexMap<Key, Vec<&Shape<'tcx>>>) {
            let tcx = cx.tcx;
            let adt = tcx.adt_def(item);
            let held = shape.cases.iter().filter(|case| case.ctor.is_none() && case.fields.len() > 1).filter_map(|case| {
                let records = index.get(&Key::Record(case.fields.iter().map(|field| field.name).collect()))?;
                let record = records.iter().find(|record| unified_fields(&mut unifier(tcx, (shape.item, record.item), false), &case.fields, record.cases.first().map_or(&[], |held| &held.fields)))?;
                let variant = adt.variants().iter().find(|variant| Some(variant.name) == case.name)?;
                Some((variant.def_id.as_local()?, variant.name, path(tcx, record.item)?))
            });
            for (variant, variant_name, record_name) in held {
                span_lint_hir_and_then(cx, RESTATED_TYPE, tcx.local_def_id_to_hir_id(variant), tcx.def_span(variant), format!("variant `{variant_name}` restates `{record_name}`"), |diag| {
                    diag.help(format!("hold `{record_name}` in `{variant_name}` in place of its fields"));
                });
            }
            if let [first, rest @ ..] = &shape.cases
                && !rest.is_empty()
                && (first.ctor != Some(CtorKind::Fn) && !first.fields.is_empty() || first.fields.len() > 1)
                && rest.iter().all(|case| case.ctor == first.ctor && unified_fields(&mut unifier(tcx, (shape.item, shape.item), false), &case.fields, &first.fields))
            {
                span_lint_hir_and_then(
                    cx,
                    RESTATED_TYPE,
                    tcx.local_def_id_to_hir_id(item),
                    tcx.def_span(item),
                    format!("every variant of `{}` restates the same fields", tcx.item_name(item)),
                    |diag| {
                        diag.help("hold the shared fields once beside a fieldless kind enum");
                    },
                );
            }
        }

        fn report(cx: &LateContext<'_>, item: LocalDefId, restated: Restated, derive: Option<DefId>, mapping: Option<DefId>) {
            let tcx = cx.tcx;
            let name = tcx.item_name(item);
            let message = match &restated {
                Restated::Same(_, other) => format!("`{name}` restates `{other}`"),
                Restated::Generalized(other) => format!("`{name}` restates `{}` with other field types", tcx.item_name(*other)),
                Restated::Subset(_, other, _) => format!("`{name}` restates a subset of the fields of `{other}`"),
                Restated::Kind(_, other, _) => format!("`{name}` restates the variants of `{other}` without their fields"),
                Restated::Parallel(other, ..) => format!("`{name}` restates `{}` member by member", tcx.item_name(*other)),
            };
            span_lint_hir_and_then(cx, RESTATED_TYPE, tcx.local_def_id_to_hir_id(item), tcx.def_span(item), message, |diag| match restated {
                Restated::Same(other, text) => {
                    if other.is_local() {
                        diag.span_note(tcx.def_span(other), format!("`{}` declares the same shape here", tcx.item_name(other)));
                    }
                    diag.help(format!("use `{text}` in place of `{name}`"));
                }
                Restated::Generalized(other) => {
                    let other_name = tcx.item_name(other);
                    diag.span_note(tcx.def_span(other), format!("`{other_name}` declares the fields here"));
                    diag.help(format!("make `{other_name}` generic over the field types that differ and use it in place of `{name}`"));
                }
                Restated::Subset(other, other_name, spans) => {
                    diag.span_note(spans, format!("`{name}` is built from the fields of `{other_name}` here"));
                    diag.help(if other.is_local() { format!("hold `{name}` in `{other_name}` in place of its fields") } else { format!("hold the `{other_name}` in place of `{name}`") });
                }
                Restated::Kind(other, other_name, spans) => {
                    diag.span_note(spans, format!("`{name}` mirrors `{other_name}` here"));
                    diag.help(match derive.filter(|_| other.is_local()).and_then(|derive| path(tcx, derive)) {
                        Some(derive) => format!("derive `{name}` from `{other_name}` with `{derive}`"),
                        None => format!("match `{other_name}` in place of `{name}`"),
                    });
                }
                Restated::Parallel(other, retyped, spans) => {
                    let other_name = tcx.item_name(other);
                    diag.span_note(spans, format!("`{name}` is converted from `{other_name}` member by member here"));
                    diag.help(match (retyped, mapping.and_then(|mapping| path(tcx, mapping))) {
                        (false, _) => format!("use `{other_name}` in place of `{name}`"),
                        (true, Some(mapping)) => format!("derive the conversion of `{name}` from `{other_name}` with `{mapping}`"),
                        (true, None) => format!("make `{other_name}` generic over the member types that differ and use it in place of `{name}`"),
                    });
                }
            });
        }

        fn public(tcx: TyCtxt<'_>, adt: DefId) -> bool { tcx.visibility(adt).is_public() && tcx.visible_parent_map(()).contains_key(&adt) }

        fn reached(tcx: TyCtxt<'_>) -> FxIndexSet<DefId> {
            let items = tcx.hir_crate_items(());
            let declared = items.definitions().flat_map(|item| {
                let kind = tcx.def_kind(item);
                let signature = kind.is_fn_like().then(|| tcx.fn_sig(item).instantiate_identity().skip_normalization().skip_binder().inputs_and_output);
                let fields = kind.is_adt().then(|| tcx.adt_def(item).all_fields().map(|field| tcx.type_of(field.did).instantiate_identity().skip_normalization()));
                signature.into_iter().flatten().chain(fields.into_iter().flatten())
            });
            let bodies =
                tcx.hir_body_owners().filter(|&owner| !tcx.is_typeck_child(owner.to_def_id())).flat_map(|owner| tcx.typeck(owner).node_types().items_in_stable_order().into_iter().map(|(_, &ty)| ty));
            declared.chain(bodies).flat_map(Ty::walk).filter_map(|argument| argument.as_type()?.ty_adt_def().map(AdtDef::did)).collect()
        }

        let tcx = cx.tcx;
        let items = tcx.hir_crate_items(());
        let locals = items.definitions().filter(|&item| matches!(tcx.def_kind(item), DefKind::Struct | DefKind::Enum)).collect::<Vec<_>>();
        let unchecked = |item: LocalDefId| tcx.def_span(item).from_expansion() || is_lint_allowed(cx, RESTATED_TYPE, tcx.local_def_id_to_hir_id(item));
        let checked = locals.iter().copied().filter(|&item| !unchecked(item)).collect::<Vec<_>>();
        if checked.is_empty() {
            return;
        }
        let shapes = locals
            .iter()
            .map(|item| item.to_def_id())
            .chain(reached(tcx).into_iter().filter(|&item| !item.is_local() && public(tcx, item) && !tcx.is_doc_hidden(item) && current(tcx, item)))
            .filter_map(|item| shape(tcx, item))
            .filter(|shape| shape.item.is_local() || path(tcx, shape.item).is_some())
            .collect::<Vec<_>>();
        let key = |shape: &Shape<'_>| {
            if shape.record() { Key::Record(shape.cases.iter().flat_map(|case| &case.fields).map(|field| field.name).collect()) } else { Key::Cases(shape.cases.iter().map(|case| case.key).collect()) }
        };
        let index = shapes.iter().fold(FxIndexMap::<Key, Vec<&Shape<'tcx>>>::default(), |mut index, shape| {
            index.entry(key(shape)).or_default().push(shape);
            index
        });
        let owned = shapes.iter().filter(|shape| shape.item.is_local()).map(|shape| (shape.item, shape)).collect::<FxHashMap<_, _>>();
        let position = locals.iter().enumerate().map(|(order, &item)| (item.to_def_id(), order)).collect::<FxHashMap<_, _>>();
        let impls = items.definitions().filter(|&item| matches!(tcx.def_kind(item), DefKind::Impl { of_trait: true })).fold(FxIndexMap::<DefId, Vec<DefId>>::default(), |mut impls, implementation| {
            if let Some(adt) = tcx.type_of(implementation).instantiate_identity().skip_normalization().peel_refs().ty_adt_def() {
                impls.entry(adt.did()).or_default().push(implementation.to_def_id());
            }
            impls
        });
        let projects = |source: DefId, kind: &Shape<'tcx>| {
            shape(tcx, source).is_some_and(|full| !full.record() && !unit(&full) && full.cases.iter().map(|case| case.name).eq(kind.cases.iter().map(|case| case.name)))
        };
        let tooled = |item: LocalDefId| {
            let adt = tcx.adt_def(item);
            iter::once(item.to_def_id())
                .chain(adt.variants().iter().map(|variant| variant.def_id).filter(|&variant| variant != item.to_def_id()))
                .chain(adt.all_fields().map(|field| field.did))
                .filter_map(DefId::as_local)
                .flat_map(|owner| tcx.hir_attrs(tcx.local_def_id_to_hir_id(owner)))
                .filter_map(|attribute| {
                    if let Attribute::Unparsed(unparsed) = attribute
                        && Level::from_opt_symbol(attribute.name()).is_none()
                    {
                        Some(&**unparsed)
                    } else {
                        None
                    }
                })
                .collect::<Vec<&AttrItem>>()
        };
        let distinct = |left: LocalDefId, right: LocalDefId| {
            let by_hand = |item: LocalDefId| {
                let mut traits = impls
                    .get(&item.to_def_id())
                    .into_iter()
                    .flatten()
                    .filter(|&&implementation| !tcx.is_automatically_derived(implementation))
                    .map(|&implementation| (tcx.impl_trait_id(implementation), implementation))
                    .collect::<Vec<_>>();
                traits.sort_unstable_by_key(|&(implemented, _)| tcx.def_path_hash(implemented));
                traits
            };
            let (left_impls, right_impls) = (by_hand(left), by_hand(right));
            let (left_tools, right_tools) = (tooled(left), tooled(right));
            let same_tools = left_tools.len() == right_tools.len()
                && iter::zip(&left_tools, &right_tools).all(|(left_tool, right_tool)| {
                    left_tool.path.segments == right_tool.path.segments
                        && match (&left_tool.args, &right_tool.args) {
                            (AttrArgs::Empty, AttrArgs::Empty) => true,
                            (AttrArgs::Delimited(left_args), AttrArgs::Delimited(right_args)) =>
                                left_args.delim == right_args.delim
                                    && left_args.tokens.len() == right_args.tokens.len()
                                    && iter::zip(left_args.tokens.iter(), right_args.tokens.iter()).all(|(left_token, right_token)| left_token.eq_unspanned(right_token)),
                            (AttrArgs::Eq { eq_span: _, expr: left_value }, AttrArgs::Eq { eq_span: _, expr: right_value }) => left_value.kind == right_value.kind,
                            (AttrArgs::Empty | AttrArgs::Delimited(_) | AttrArgs::Eq { eq_span: _, expr: _ }, _) => false,
                        }
                });
            let differ = |left_typeck: &TypeckResults<'tcx>, left_expr: &Expr<'_>, right_typeck: &TypeckResults<'tcx>, right_expr: &Expr<'_>| {
                let (left_ty, right_ty) = (tcx.erase_and_anonymize_regions(left_typeck.expr_ty(left_expr)), tcx.erase_and_anonymize_regions(right_typeck.expr_ty(right_expr)));
                matches!((left_expr.kind, right_expr.kind), (ExprKind::Path(_), ExprKind::Path(_)))
                    && left_ty != right_ty
                    && substituted(tcx, right_ty, right.to_def_id(), |args| Ty::new_adt(tcx, tcx.adt_def(left), args)) == left_ty
            };
            let alike = |left_implementation: DefId, right_implementation: DefId| {
                let implemented = tcx.impl_item_implementor_ids(right_implementation);
                tcx.associated_items(left_implementation).in_definition_order().filter(|associated| associated.is_fn()).all(|associated| {
                    associated
                        .trait_item_def_id()
                        .and_then(|declared| implemented.get(&declared))
                        .and_then(|counterpart| counterpart.as_local().zip(associated.def_id.as_local()))
                        .is_some_and(|(right_function, left_function)| equal_bodies(cx, left_function, right_function, &differ).is_some())
                })
            };
            left_impls.iter().map(|&(implemented, _)| implemented).ne(right_impls.iter().map(|&(implemented, _)| implemented))
                || !same_tools
                || iter::zip(&left_impls, &right_impls).any(|(&(_, left_implementation), &(_, right_implementation))| !alike(left_implementation, right_implementation))
        };
        let restated = |item: LocalDefId, left: &Shape<'tcx>| {
            let behaves = |owner: Ty<'tcx>| {
                tooled(item).is_empty()
                    && tcx.inherent_impls(item).is_empty()
                    && impls.get(&item.to_def_id()).into_iter().flatten().all(|&implementation| {
                        let trait_ref = tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization();
                        let replaced = substituted(tcx, trait_ref, item.to_def_id(), |args| EarlyBinder::bind(tcx, owner).instantiate(tcx, args).skip_normalization());
                        trait_ref.def_id.is_local()
                            || replaced
                                .args
                                .get(1..)
                                .is_some_and(|rest| implements_trait_with_env(tcx, TypingEnv::non_body_analysis(tcx, implementation), replaced.self_ty(), replaced.def_id, None, rest))
                    })
            };
            let candidates = || index.get(&key(left)).into_iter().flatten().filter(|right| right.item != left.item);
            let earlier = |right: &Shape<'_>| position.get(&right.item) < position.get(&left.item);
            let dependency = candidates().filter(|right| !right.item.is_local()).find_map(|right| {
                let mut unifier = unifier(tcx, (left.item, right.item), false);
                (unified_shapes(&mut unifier, left, right) && behaves(instance(&unifier, right.item))).then(|| Some(Restated::Same(right.item, text(tcx, instance(&unifier, right.item))?)))?
            });
            let local = || {
                candidates().filter_map(|right| Some((right, right.item.as_local()?))).find_map(|(right, other)| {
                    let mut forward = unifier(tcx, (left.item, right.item), false);
                    let restates = unified_shapes(&mut forward, left, right) && !distinct(item, other);
                    let general = unified_shapes(&mut unifier(tcx, (right.item, left.item), false), right, left);
                    (restates && (!general || earlier(right))).then(|| Some(Restated::Same(right.item, text(tcx, instance(&forward, right.item))?)))?
                })
            };
            let generalized = || {
                candidates().filter(|right| right.item.is_local() && earlier(right)).find_map(|right| {
                    let mut unifier = unifier(tcx, (left.item, right.item), true);
                    (unified_shapes(&mut unifier, left, right)
                        && unifier.shared
                        && unifier.differences.as_ref().is_some_and(|differences| !differences.is_empty())
                        && right.item.as_local().is_some_and(|other| !distinct(item, other)))
                    .then_some(Restated::Generalized(right.item))
                })
            };
            let subset = || {
                self.copies.iter().filter(|&(&(copy, _), evidence)| copy == left.item && left.record() && !evidence.retyped).find_map(|(&(_, source), evidence)| {
                    let held = fields(tcx, source, tcx.adt_def(source).non_enum_variant());
                    let mut unifier = unifier(tcx, (left.item, source), false);
                    let copied = left.cases.iter().flat_map(|case| &case.fields).collect::<Vec<_>>();
                    let within = copied.iter().all(|field| held.iter().find(|other| other.name == field.name).is_some_and(|other| unified_fields(&mut unifier, &[**field], &[*other])));
                    (copied.len() < held.len() && within).then(|| Some(Restated::Subset(source, path(tcx, source)?, evidence.spans.clone())))?
                })
            };
            let kind = || {
                self.mirrors
                    .iter()
                    .filter(|&(&(source, target), _)| target == left.item && unit(left) && projects(source, left))
                    .find_map(|(&(source, _), evidence)| Some(Restated::Kind(source, path(tcx, source)?, evidence.spans.clone())))
            };
            dependency.or_else(local).or_else(generalized).or_else(subset).or_else(kind)
        };
        let parallel = |item: LocalDefId| {
            let members = |owner: DefId| {
                let adt = tcx.adt_def(owner);
                if adt.is_enum() { adt.variants().iter().map(|variant| variant.def_id).collect::<FxHashSet<_>>() } else { adt.all_fields().map(|field| field.did).collect() }
            };
            let copies = self.copies.iter().map(|(&(target, source), evidence)| (target, source, evidence));
            copies.chain(self.mirrors.iter().map(|(&(source, target), evidence)| (target, source, evidence))).find_map(|(target, source, evidence)| {
                let (built, read) = evidence.pairs.iter().copied().unzip::<_, _, FxHashSet<_>, FxHashSet<_>>();
                let local = source.as_local().filter(|&local| !distinct(item, local));
                (target == item.to_def_id() && local.is_some() && built == members(target) && read == members(source) && read.len() == evidence.pairs.len())
                    .then(|| Restated::Parallel(source, evidence.retyped, evidence.spans.clone()))
            })
        };
        let kind = |item: DefId| owned.get(&item).is_some_and(|generated| unit(generated) && owned.values().any(|other| projects(other.item, generated)));
        let conversion = |item: DefId| {
            matches!(tcx.def_kind(item), DefKind::Impl { of_trait: true })
                && tcx.is_diagnostic_item(sym::From, tcx.impl_trait_id(item))
                && tcx.impl_trait_ref(item).instantiate_identity().skip_normalization().args.types().all(|converted| converted.ty_adt_def().is_some_and(|adt| adt.is_struct() && adt.did().is_local()))
        };
        let derived = |converts: bool| {
            items.definitions().filter(|&item| tcx.def_span(item).from_expansion()).find_map(|item| {
                let expansion = tcx.def_span(item).ctxt().outer_expn_data();
                let produced = if converts { conversion(item.to_def_id()) } else { kind(item.to_def_id()) };
                (matches!(expansion.kind, ExpnKind::Macro(MacroKind::Derive, _)) && produced).then_some(expansion.macro_def_id?)
            })
        };
        let (derive, mapping) = (derived(false), derived(true));
        let mut same = FxHashSet::default();
        for item in checked {
            let left = owned.get(&item.to_def_id()).copied().filter(|left| left.record() || tcx.adt_def(item).is_enum());
            match left.and_then(|left| restated(item, left)).or_else(|| parallel(item)) {
                Some(Restated::Generalized(other)) if same.contains(&(other, item.to_def_id())) => {}
                Some(restated) => {
                    if let Restated::Same(other, _) = restated {
                        same.insert((item.to_def_id(), other));
                    }
                    report(cx, item, restated, derive, mapping);
                }
                None => {}
            }
            if let Some(left) = left.filter(|left| !left.record()) {
                payloads(cx, item, left, &index);
            }
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn converted<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> &'tcx Expr<'tcx> {
    let conversion = |item: Option<DefId>| {
        item.and_then(|item| cx.tcx.trait_of_assoc(item)).and_then(|declared| cx.tcx.get_diagnostic_name(declared)).is_some_and(|name| [sym::Clone, sym::From, sym::Into, sym::ToOwned].contains(&name))
    };
    peel_hir_expr_while(value, |inner| {
        if let ExprKind::MethodCall(_, receiver, [], _) = inner.kind
            && conversion(cx.typeck_results().type_dependent_def_id(inner.hir_id))
        {
            Some(receiver)
        } else if let ExprKind::Call(_, [argument]) = inner.kind
            && conversion(fn_def_id(cx, inner))
        {
            Some(argument)
        } else {
            None
        }
    })
}

fn fields<'tcx>(tcx: TyCtxt<'tcx>, owner: DefId, variant: &VariantDef) -> Vec<Field<'tcx>> {
    let (env, args) = (TypingEnv::non_body_analysis(tcx, owner), GenericArgs::identity_for_item(tcx, owner));
    let mut fields = variant.fields.iter().map(|field| Field { name: field.name, ty: tcx.normalize_erasing_regions(env, field.ty(tcx, args)), default: field.value }).collect::<Vec<_>>();
    fields.sort_by(|left, right| left.name.as_str().cmp(right.name.as_str()));
    fields
}

fn current(tcx: TyCtxt<'_>, item: DefId) -> bool {
    tcx.lookup_deprecation(item).is_none() && tcx.lookup_stability(item).is_none_or(|stability| !stability.level.is_unstable() || tcx.features().enabled(stability.feature))
}

fn path(tcx: TyCtxt<'_>, item: DefId) -> Option<String> {
    let parents = tcx.visible_parent_map(());
    if item.is_local() {
        return Some(tcx.item_name(item).to_string());
    }
    if iter::successors(parents.get(&item), |module| parents.get(module)).all(|&module| current(tcx, module)) {
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
