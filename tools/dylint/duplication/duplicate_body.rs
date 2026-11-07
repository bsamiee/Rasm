use core::{iter, ops::ControlFlow};
use std::rc::Rc;

use clippy_utils::{
    SpanlessHash,
    diagnostics::span_lint_hir_and_then,
    peel_blocks, peel_hir_expr_refs, peel_hir_expr_while,
    res::MaybeQPath as _,
    source::snippet,
    visitors::{for_each_expr, is_const_evaluatable},
};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap, FxIndexSet};
use rustc_errors::listify;
use rustc_hir::{
    Block, ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, HirId, Node,
    def::{CtorOf, DefKind},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    self, EarlyBinder, GenericArg, GenericArgs, GenericArgsRef, TraitRef, Ty, TyCtxt, TypeFoldable, TypeVisitableExt as _, TypeckResults,
    adjustment::{Adjust, DerefAdjustKind, PointerCoercion},
    print::{PrintTraitRefExt as _, with_forced_trimmed_paths},
};
use rustc_span::Symbol;
use rustc_trait_selection::{
    regions::test_region_obligations,
    traits::{Obligation, ObligationCause, ObligationCtxt},
};

use super::{Bodies, Callable, owner, substitute_types};
use crate::{constructed, derived_impl, expanded_externally, impl_owner, is_error, item_path, literal_or_const, parameter};

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPEATS: &str = "repeated here";

// --- [MODELS] --------------------------------------------------------------------------

enum Difference<'tcx> {
    Value(Option<DefId>, FxIndexSet<HirId>),
    ConstantPerType(DefId, FxIndexSet<HirId>),
    Type { from: Ty<'tcx>, traits: Vec<TraitRef<'tcx>>, direct: Option<DefId> },
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    bodies: Rc<Bodies<'tcx>>,
}

impl<'tcx> Pass<'tcx> {
    pub(crate) const fn new(_: TyCtxt<'tcx>, bodies: Rc<Bodies<'tcx>>) -> Self { Self { bodies } }
}

lint!(Pass<'_>;
    DUPLICATE_BODY: "functions and methods that repeat the body and signature of another function",
    PARAMETERIZABLE_BODY: "functions that repeat another function's body with other literals or constants, or with one other type"
);

// --- [OPERATIONS] ----------------------------------------------------------------------

fn value(tcx: TyCtxt<'_>, function: LocalDefId) -> &Expr<'_> {
    let value = peel_blocks(tcx.hir_body_owned_by(function).value);
    match value.kind {
        ExprKind::Closure(closure) if matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn))) => match tcx.hir_body(closure.body).value.kind {
            ExprKind::Block(Block { expr: Some(tail), .. }, _) =>
                peel_blocks(peel_hir_expr_while(tail, |inner| if let ExprKind::DropTemps(kept) = inner.kind { Some(peel_blocks(kept)) } else { None })),
            _ => value,
        },
        _ => value,
    }
}

fn substantive(tcx: TyCtxt<'_>, function: LocalDefId) -> bool {
    let mut operations = 0_usize;
    for_each_expr(tcx, value(tcx, function), |expr| {
        if !matches!(expr.kind, ExprKind::Lit(_) | ExprKind::Path(_) | ExprKind::Block(..) | ExprKind::DropTemps(_) | ExprKind::AddrOf(..) | ExprKind::Closure(_)) {
            operations = operations.saturating_add(1);
        }
        if operations > 1 { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }
    })
    .is_some()
}

fn bound_to_trait<'tcx>(cx: &LateContext<'tcx>, function: LocalDefId, implemented: DefId) -> bool {
    fn rebased<'tcx, T>(tcx: TyCtxt<'tcx>, value: T, arguments: GenericArgsRef<'tcx>, owner: Ty<'tcx>) -> T
    where T: TypeFoldable<TyCtxt<'tcx>> {
        substitute_types(tcx, EarlyBinder::bind(tcx, value).instantiate(tcx, arguments).skip_normalization(), |held| (held == owner).then_some(tcx.types.self_param))
    }

    let tcx = cx.tcx;
    let (Some(declaration), Some(original_owner)) = (tcx.trait_item_of(function), owner(tcx, function)) else { return false };
    if declaration.as_local().is_some_and(|declaration| tcx.parent_module_from_def_id(declaration) != tcx.parent_module_from_def_id(function)) {
        return false;
    }
    let implementation = tcx.impl_trait_ref(tcx.parent(function.to_def_id())).instantiate_identity().skip_normalization();
    let declared = GenericArgs::identity_for_item(tcx, declaration);
    let inherited = tcx.generics_of(function).parent_count;
    let arguments = GenericArgs::identity_for_item(tcx, function)
        .iter()
        .enumerate()
        .map(|(index, argument)| {
            if index >= inherited {
                declared.get(tcx.generics_of(declaration).parent_count.strict_add(index.strict_sub(inherited))).copied()
            } else {
                implementation.args.iter().zip(declared).find_map(|(held, target)| (held == argument).then_some(target))
            }
        })
        .collect::<Option<Vec<_>>>();
    let Some(arguments) = arguments else { return false };
    let arguments = tcx.mk_args(&arguments);
    let owner = EarlyBinder::bind(tcx, original_owner).instantiate(tcx, arguments).skip_normalization();
    if rebased(tcx, implementation, arguments, owner) != TraitRef::identity(tcx, implemented) {
        return false;
    }
    let typeck = tcx.typeck(function);
    let body_owner = tcx.erase_and_anonymize_regions(original_owner);
    let declared_owner = tcx.erase_and_anonymize_regions(owner);
    let param_env = tcx.param_env(declaration);
    let known = tcx.liberate_late_bound_regions(declaration, tcx.fn_sig(declaration).instantiate_identity().skip_normalization()).inputs_and_output.iter().collect();
    test_region_obligations(tcx, function, param_env, &known, |infcx| {
        let obligations = ObligationCtxt::new(infcx);
        let cause = ObligationCause::dummy();
        let clauses = tcx.mk_clauses_from_iter(tcx.clauses_of(function).instantiate_identity(tcx).clauses.into_iter().map(ty::Unnormalized::skip_normalization));
        obligations.register_obligations(rebased(tcx, clauses, arguments, owner).iter().map(|clause| Obligation::new(tcx, cause.clone(), param_env, clause)));
        let unavailable = for_each_expr(tcx, tcx.hir_body_owned_by(function).value, |expr| {
            let depends = |held: Ty<'tcx>| held.walk().filter_map(GenericArg::as_type).any(|held| held == body_owner);
            let adjustments = typeck.expr_adjustments(expr);
            let coerced = iter::once(typeck.expr_ty(expr)).chain(adjustments.iter().map(|adjustment| adjustment.target)).zip(adjustments).any(|(source, adjustment)| {
                matches!(adjustment.kind, Adjust::Pointer(PointerCoercion::Unsize) | Adjust::Deref(DerefAdjustKind::Overloaded(_) | DerefAdjustKind::Pin) | Adjust::GenericReborrow(_))
                    && (depends(source) || depends(adjustment.target))
            });
            let affected = iter::once(typeck.expr_ty(expr))
                .chain(adjustments.iter().map(|adjustment| adjustment.target))
                .filter(|&held| depends(held))
                .map(|held| (held, rebased(tcx, held.peel_refs(), arguments, declared_owner)))
                .collect::<Vec<_>>();
            let resolved = typeck.type_dependent_def_id(expr.hir_id).or_else(|| expr.res(typeck).opt_def_id());
            if matches!(expr.kind, ExprKind::Field(base, _) if typeck.expr_ty_adjusted(base).peel_refs() == body_owner)
                || resolved.and_then(|item| impl_owner(tcx, item)).is_some_and(|implementor| implementor == original_owner)
                || typeck.expr_ty_adjusted(expr).peel_refs() == body_owner && constructed(typeck, expr).is_some()
                || coerced
                || affected.iter().any(|(_, held)| held.has_erased_regions())
            {
                return ControlFlow::Break(());
            }
            for (source, held) in affected {
                obligations.register_obligation(Obligation::new(tcx, cause.clone(), param_env, ty::Binder::dummy(ty::ClauseKind::WellFormed(held.into()))));
                match (source.is_ref(), tcx.lang_items().sized_trait()) {
                    (true, _) => (),
                    (false, Some(sized)) => obligations.register_bound(cause.clone(), param_env, held, sized),
                    (false, None) => return ControlFlow::Break(()),
                }
            }
            if let Some(called) = resolved.filter(|&called| matches!(tcx.def_kind(called), DefKind::Fn | DefKind::AssocFn | DefKind::Const | DefKind::AssocConst | DefKind::Static { .. }))
                && let called_args = typeck.node_args(expr.hir_id)
                && called_args.iter().filter_map(GenericArg::as_type).any(depends)
            {
                let called_args = rebased(tcx, called_args, arguments, declared_owner);
                if called_args.has_erased_regions() {
                    return ControlFlow::Break(());
                }
                obligations.register_obligations(
                    tcx.clauses_of(called).instantiate(tcx, called_args).clauses.into_iter().map(|clause| Obligation::new(tcx, cause.clone(), param_env, clause.skip_normalization())),
                );
            }
            ControlFlow::Continue(())
        })
        .is_some();
        (!unavailable && obligations.evaluate_obligations_error_on_ambiguity().no_errors()).ok_or(())
    })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn families<'tcx, T, F>(bucket: &[(LocalDefId, T)], varied: F) -> FxIndexMap<LocalDefId, (Vec<LocalDefId>, Difference<'tcx>)>
        where
            T: Copy,
            F: Fn(LocalDefId, T, LocalDefId, T) -> Option<Difference<'tcx>>,
        {
            bucket
                .iter()
                .fold(FxIndexMap::<LocalDefId, (T, Option<(Vec<LocalDefId>, Difference<'tcx>)>)>::default(), |mut families, &(later, data)| {
                    let added = families.iter_mut().any(|(&root, (earlier, family))| match (family, varied(root, *earlier, later, data)) {
                        (family @ None, Some(form @ (Difference::Value(..) | Difference::ConstantPerType(..) | Difference::Type { .. }))) => {
                            *family = Some((vec![later], form));
                            true
                        }
                        (Some((members, Difference::Value(called, sites))), Some(Difference::Value(other, added))) => {
                            *called = called.filter(|&called| Some(called) == other);
                            sites.extend(added);
                            members.push(later);
                            true
                        }
                        (Some((members, Difference::ConstantPerType(implemented, sites))), Some(Difference::ConstantPerType(other, added))) if *implemented == other => {
                            sites.extend(added);
                            members.push(later);
                            true
                        }
                        (Some((members, Difference::Type { from, traits, direct })), Some(Difference::Type { from: other_from, traits: added, direct: other })) if *from == other_from => {
                            *direct = direct.filter(|&called| Some(called) == other);
                            *traits = traits.iter().copied().chain(added).unique().collect();
                            members.push(later);
                            true
                        }
                        (None | Some((_, Difference::Value(..) | Difference::ConstantPerType(..) | Difference::Type { .. })), None)
                        | (Some((_, Difference::Value(..))), Some(Difference::ConstantPerType(..) | Difference::Type { .. }))
                        | (Some((_, Difference::ConstantPerType(..))), Some(Difference::Value(..) | Difference::ConstantPerType(..) | Difference::Type { .. }))
                        | (Some((_, Difference::Type { .. })), Some(Difference::Value(..) | Difference::ConstantPerType(..) | Difference::Type { .. })) => false,
                    });
                    if !added {
                        families.insert(later, (data, None));
                    }
                    families
                })
                .into_iter()
                .filter_map(|(root, (_, family))| Some((root, family?)))
                .collect()
        }

        fn parameterized<'tcx>(cx: &LateContext<'tcx>, root: LocalDefId, members: &[LocalDefId], form: &Difference<'tcx>) {
            let tcx = cx.tcx;
            let root_path = item_path(cx, root.to_def_id());
            let functions = [&[root], members].concat();
            let (Some(listed), Some(every)) =
                (listify(members, |&member| format!("`{}`", item_path(cx, member.to_def_id()))), listify(&functions, |&function| format!("`{}`", item_path(cx, function.to_def_id()))))
            else {
                return;
            };
            let repeat = if let [_] = members { "repeats" } else { "repeat" };
            let values = |differing: &FxIndexSet<HirId>| {
                let written = differing.iter().map(|&value| snippet(cx, tcx.hir_span(value), "..")).unique().collect::<Vec<_>>();
                let other = if let [_] = &written { "another value" } else { "other values" };
                listify(&written, |value| format!("`{value}`")).map(|shown| format!("{other} in place of {shown}"))
            };
            let varied = with_forced_trimmed_paths!(match form {
                Difference::Type { from, traits: _, direct: _ } => Some(format!("another type in place of `{from}`")),
                Difference::Value(_, differing) | Difference::ConstantPerType(_, differing) => values(differing),
            });
            let help = with_forced_trimmed_paths!(match form {
                Difference::Value(Some(callee), _) => Some(format!("call `{}` directly at each call site of {every}", item_path(cx, *callee))),
                Difference::Value(None, differing) =>
                    Some(format!("replace {every} with one function that takes {}", if differing.len() == 1 { "the value as a parameter" } else { "the values as parameters" })),
                Difference::ConstantPerType(implemented, _) => listify(&functions.iter().filter_map(|&function| owner(tcx, function)).collect::<Vec<_>>(), |held| format!("`{held}`"))
                    .map(|owners| format!("replace {owners} with one struct holding the value, one `const` per replaced type, and one `{}` impl", tcx.item_name(*implemented))),
                Difference::Type { from: _, traits: _, direct: Some(method) } => Some(format!("call `{}` directly in place of {every}", item_path(cx, *method))),
                Difference::Type { from: _, traits, direct: None } =>
                    Some(format!("replace {every} with one function generic over `S: {}`", traits.iter().map(|implemented| implemented.print_only_trait_path().to_string()).join(" + "))),
            });
            let (Some(varied), Some(help)) = (varied, help) else { return };
            let message = format!("{listed} {repeat} the body of `{root_path}` with {varied}");
            span_lint_hir_and_then(cx, PARAMETERIZABLE_BODY, tcx.local_def_id_to_hir_id(root), tcx.def_span(root), message, |diag| {
                diag.span_note(members.iter().map(|&member| tcx.def_span(member)).collect::<Vec<_>>(), REPEATS);
                diag.help(help);
            });
        }

        fn varied_type<'tcx>(
            bodies: &Bodies<'tcx>,
            cx: &LateContext<'tcx>,
            earlier: LocalDefId,
            later: LocalDefId,
            earlier_contract: &Callable<'tcx>,
            later_contract: &Callable<'tcx>,
        ) -> Option<Difference<'tcx>> {
            let tcx = cx.tcx;
            let (earlier_signature, later_signature) = (earlier_contract.signature, later_contract.signature);
            if owner(tcx, earlier) != owner(tcx, later) || !substantive(tcx, earlier) {
                return None;
            }
            let adts = |types: &[Ty<'tcx>]| types.iter().flat_map(|held| held.walk()).filter_map(GenericArg::as_type).filter(|held| held.is_adt()).collect::<Vec<_>>();
            let (from_types, to_types) = (adts(earlier_signature.skip_binder().inputs_and_output), adts(later_signature.skip_binder().inputs_and_output));
            let mut candidates = from_types.iter().copied().filter(|held| !to_types.contains(held)).collect::<Vec<_>>();
            candidates.sort_by_key(|held| held.walk().count());
            let (from, to) = candidates.into_iter().find_map(|from| {
                to_types
                    .iter()
                    .copied()
                    .filter(|held| !from_types.contains(held))
                    .find(|&to| {
                        from.ty_adt_def() != to.ty_adt_def()
                            && substitute_types(tcx, (later_signature, later_contract.clauses), |held| (held == to).then_some(from)) == (earlier_signature, earlier_contract.clauses)
                    })
                    .map(|to| (from, to))
            })?;
            let ctor_of = |held: Ty<'tcx>, adt: Ty<'tcx>| {
                if let ty::FnDef(ctor, _) = *held.kind()
                    && let DefKind::Ctor(CtorOf::Struct, _) = tcx.def_kind(ctor)
                {
                    adt.ty_adt_def().is_some_and(|built| built.did() == tcx.parent(ctor))
                } else {
                    false
                }
            };
            bodies.equal(
                cx,
                earlier,
                later,
                |left_typeck, left_expr, right_typeck, right_expr| {
                    let (left_ty, right_ty) = (tcx.erase_and_anonymize_regions(left_typeck.expr_ty(left_expr)), tcx.erase_and_anonymize_regions(right_typeck.expr_ty(right_expr)));
                    matches!((left_expr.kind, right_expr.kind), (ExprKind::Path(_), ExprKind::Path(_)))
                        && matches!((left_ty.kind(), right_ty.kind()), (ty::FnDef(..) | ty::Adt(..), ty::FnDef(..) | ty::Adt(..)))
                        && (substitute_types(tcx, right_ty, |held| (held == to).then_some(from)) == left_ty || ctor_of(left_ty, from) && ctor_of(right_ty, to))
                },
                |left, right| substitute_types(tcx, right, |held| (held == to).then_some(from)) == left,
            )?;
            let generic_calls = |function: LocalDefId, held: Ty<'tcx>| {
                let typeck = tcx.typeck(function);
                let receives = |node: HirId| typeck.node_args_opt(node).and_then(|args| args.types().next()).is_some_and(|receiver| receiver.peel_refs() == held);
                let mut called = Vec::new();
                let _: Option<()> = for_each_expr(tcx, tcx.hir_body_owned_by(function).value, |expr| {
                    called.extend(
                        typeck.type_dependent_def_id(expr.hir_id).or_else(|| expr.res(typeck).opt_def_id().filter(|&item| tcx.def_kind(item) == DefKind::AssocFn)).map(|item| (expr.hir_id, item)),
                    );
                    ControlFlow::Continue(())
                });
                let bound = |node: HirId, implemented: DefId| {
                    let args = typeck.node_args(node).truncate_to(tcx, tcx.generics_of(implemented));
                    substitute_types(tcx, TraitRef::new_from_args(tcx, implemented, args), |written| (written == held).then(|| Ty::new_param(tcx, 0, Symbol::intern("S"))))
                };
                (!called.iter().any(|&(_, item)| impl_owner(tcx, item).is_some_and(|owner| owner.ty_adt_def() == held.ty_adt_def())))
                    .then(|| called.iter().filter(|&&(node, _)| receives(node)).filter_map(|&(node, item)| Some(bound(node, tcx.trait_of_assoc(item)?))).unique().collect::<Vec<_>>())
            };
            let traits = generic_calls(earlier, from)?;
            let owned = tcx.hir_body_owned_by(earlier);
            let body = peel_blocks(owned.value);
            let typeck = tcx.typeck(earlier);
            let passed = |argument: &Expr<'_>| parameter(owned, argument).is_some();
            let direct = match body.kind {
                ExprKind::Call(&Expr { kind: ExprKind::Path(qpath), hir_id, .. }, arguments) if arguments.iter().all(passed) => typeck.qpath_res(&qpath, hir_id).opt_def_id(),
                ExprKind::MethodCall(_, receiver, arguments, _) if passed(receiver) && arguments.iter().all(passed) => typeck.type_dependent_def_id(body.hir_id),
                _ => None,
            }
            .filter(|&called| tcx.trait_of_assoc(called).is_some_and(|implemented| traits.iter().any(|bound| bound.def_id == implemented)));
            (!traits.is_empty() && generic_calls(later, to).is_some()).then_some(Difference::Type { from, traits, direct })
        }

        fn varied_value<'tcx>(cx: &LateContext<'tcx>, earlier: LocalDefId, later: LocalDefId, differences: &[(HirId, HirId)]) -> Option<Difference<'tcx>> {
            let tcx = cx.tcx;
            let typeck = tcx.typeck(earlier);
            let body = peel_blocks(tcx.hir_body_owned_by(earlier).value);
            let fieldless =
                |function: LocalDefId| owner(tcx, function).filter(|held| held.ty_adt_def().is_some_and(|adt| adt.is_struct() && adt.all_fields().next().is_none()) && !is_error(cx, *held));
            let direct = |arguments: &[Expr<'_>]| differences.iter().all(|&(differing, _)| arguments.iter().any(|argument| argument.hir_id == differing));
            let called = match body.kind {
                ExprKind::Call(&Expr { kind: ExprKind::Path(qpath), hir_id, .. }, arguments) if direct(arguments) => typeck.qpath_res(&qpath, hir_id).opt_def_id(),
                ExprKind::MethodCall(_, _, arguments, _) if direct(arguments) => typeck.type_dependent_def_id(body.hir_id),
                _ => None,
            }
            .filter(|&callee| matches!(tcx.def_kind(callee), DefKind::Fn | DefKind::AssocFn));
            let compared = |differing: HirId| matches!(tcx.parent_hir_node(differing), Node::Expr(parent) if matches!(parent.kind, ExprKind::Binary(operator, ..) if operator.node.is_comparison()));
            let tabled = |differing: HirId| matches!(tcx.parent_hir_node(differing), Node::Arm(arm) if arm.body.hir_id == differing);
            let differing = differences.iter().map(|&(differing, _)| differing).collect::<Vec<_>>();
            if is_const_evaluatable(tcx, typeck, value(tcx, earlier)) || differing.iter().all(|&differing| tabled(differing)) {
                None
            } else if owner(tcx, earlier) == owner(tcx, later) && [earlier, later].into_iter().any(|function| tcx.trait_item_of(function).is_none()) {
                (substantive(tcx, earlier) && (called.is_some() || !differing.iter().all(|&differing| compared(differing)))).then(|| Difference::Value(called, differing.into_iter().collect()))
            } else if fieldless(earlier).is_some()
                && fieldless(later).is_some()
                && let Some(declaration) = tcx.trait_item_of(earlier)
                && tcx.trait_item_of(later) == Some(declaration)
                && [earlier, later].into_iter().all(|function| bound_to_trait(cx, function, tcx.parent(declaration)))
            {
                Some(Difference::ConstantPerType(tcx.parent(declaration), differing.into_iter().collect()))
            } else {
                None
            }
        }

        fn duplicated(cx: &LateContext<'_>, group: &[LocalDefId]) {
            let tcx = cx.tcx;
            if group.first().is_some_and(|&first| is_const_evaluatable(tcx, tcx.typeck(first), value(tcx, first))) {
                return;
            }
            let deletable = |function: LocalDefId| tcx.trait_impl_of_assoc(function.to_def_id()).is_none() && tcx.trait_of_assoc(function.to_def_id()).is_none();
            let shared = |left: LocalDefId, right: LocalDefId| owner(tcx, left) == owner(tcx, right);
            let pairs = group.iter().enumerate().filter(|&(_, &function)| deletable(function)).filter_map(|(index, &duplicate)| {
                let (earlier, later) = group.split_at_checked(index)?;
                earlier.iter().find(|&&kept| shared(kept, duplicate)).or_else(|| later.iter().skip(1).find(|&&kept| !deletable(kept) && shared(kept, duplicate))).map(|&kept| (duplicate, kept))
            });
            for (duplicate, kept) in pairs {
                let (duplicate_path, kept_path) = (item_path(cx, duplicate.to_def_id()), item_path(cx, kept.to_def_id()));
                span_lint_hir_and_then(
                    cx,
                    DUPLICATE_BODY,
                    tcx.local_def_id_to_hir_id(duplicate),
                    tcx.def_span(duplicate),
                    format!("`{duplicate_path}` repeats the body and signature of `{kept_path}`"),
                    |diag| {
                        diag.span_note(tcx.def_span(kept), format!("`{kept_path}` declared here"));
                        diag.help(format!("delete `{duplicate_path}` and call `{kept_path}` at each call site"));
                    },
                );
            }
            let forwards = |function: LocalDefId| {
                let read = peel_hir_expr_refs(value(tcx, function)).0;
                matches!(read.kind, ExprKind::Field(base, _) if parameter(tcx.hir_body_owned_by(function), base) == Some(0))
                    || matches!(read.kind, ExprKind::Call(callee, _)
                        if callee.res(tcx.typeck(function)).opt_def_id().is_some_and(|called| tcx.inherent_impl_of_assoc(called).is_some() && impl_owner(tcx, called) == owner(tcx, function)))
            };
            let [first, rest @ ..] = group else { return };
            if group.iter().all(|&function| forwards(function)) {
                return;
            }
            let Some(declaration) = tcx.trait_item_of(*first).filter(|&declaration| group.iter().all(|&function| tcx.trait_item_of(function) == Some(declaration))) else { return };
            let implemented = tcx.parent(declaration);
            let (method, name) = (tcx.item_name(declaration), tcx.item_name(implemented));
            let defaultable = declaration.as_local().filter(|_| !tcx.defaultness(declaration).has_value() && group.iter().all(|&function| bound_to_trait(cx, function, implemented)));
            let every = defaultable.filter(|_| tcx.local_trait_impls(implemented).len() == group.len());
            let adt = |function: LocalDefId| owner(tcx, function).and_then(Ty::ty_adt_def);
            let generic = adt(*first).filter(|common| !common.is_fundamental() && rest.iter().all(|&function| adt(function) == Some(*common)));
            let help = match (every, defaultable, implemented.is_local(), generic) {
                (Some(_), ..) | (None, None, true, _) => None,
                (None, Some(_), ..) => Some(format!("move the body into `{name}::{method}` as its default and delete each repeated `{method}`")),
                (None, None, false, Some(common)) => Some(format!("replace the impls with one `{name}` impl generic over `{}` type arguments", tcx.def_path_str(common.did()))),
                (None, None, false, None) => Some(format!("generate the `{name}` impls from one `macro_rules!` over the implementing type")),
            };
            match (every, help) {
                (Some(local), _) =>
                    span_lint_hir_and_then(cx, DUPLICATE_BODY, tcx.local_def_id_to_hir_id(local), tcx.def_span(declaration), format!("every `{name}` impl has the same `{method}` body"), |diag| {
                        diag.span_note(group.iter().map(|&function| tcx.def_span(function)).collect::<Vec<_>>(), REPEATS);
                        diag.help(format!("move the body into `{name}::{method}` as its default and delete `{method}` from each impl"));
                    }),
                (None, Some(help))
                    if let Some(owners) = listify(&group.iter().filter_map(|&function| owner(tcx, function)).collect::<Vec<_>>(), |held| with_forced_trimmed_paths!(format!("`{held}`"))) =>
                    span_lint_hir_and_then(cx, DUPLICATE_BODY, tcx.local_def_id_to_hir_id(*first), tcx.def_span(*first), format!("`{name}` impls for {owners} have the same `{method}` body"), |diag| {
                        diag.span_note(rest.iter().map(|&function| tcx.def_span(function)).collect::<Vec<_>>(), REPEATS);
                        diag.help(help);
                    }),
                (None, _) => {}
            }
        }

        let tcx = cx.tcx;
        let bodies = &self.bodies;
        let functions = tcx
            .hir_body_owners()
            .filter(|&owner| {
                matches!(tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                    && !expanded_externally(cx, tcx.def_span(owner))
                    && !tcx.impl_of_assoc(owner.to_def_id()).is_some_and(|implementation| derived_impl(tcx, implementation))
            })
            .filter_map(|function| Some((function, bodies.callable(tcx, function)?)))
            .collect::<Vec<_>>();
        let by_signature = functions.iter().fold(FxIndexMap::<_, Vec<LocalDefId>>::default(), |mut buckets, (function, signature)| {
            buckets.entry(Rc::clone(signature)).or_default().push(*function);
            buckets
        });
        let one_expansion = |earlier: LocalDefId, later: LocalDefId| {
            let written = |function: LocalDefId| tcx.def_span(function).macro_backtrace().last().and_then(|expansion| expansion.macro_def_id);
            written(earlier).is_some() && written(earlier) == written(later)
        };
        let literal_difference = |left_typeck: &TypeckResults<'tcx>, left_expr: &Expr<'_>, right_typeck: &TypeckResults<'tcx>, right_expr: &Expr<'_>| {
            literal_or_const(left_typeck, left_expr)
                && literal_or_const(right_typeck, right_expr)
                && tcx.erase_and_anonymize_regions(left_typeck.expr_ty(left_expr)) == tcx.erase_and_anonymize_regions(right_typeck.expr_ty(right_expr))
        };
        let mut compared = FxHashMap::default();
        let groups = by_signature.values().flat_map(|bucket| {
            bucket
                .iter()
                .fold(FxIndexMap::<LocalDefId, Vec<LocalDefId>>::default(), |mut groups, &function| {
                    let mut matching = groups
                        .iter_mut()
                        .filter(|&(&kept, _)| !one_expansion(kept, function))
                        .filter_map(|(&kept, members)| bodies.equal(cx, kept, function, literal_difference, |first, second| first == second).map(|differences| (kept, members, differences)))
                        .take_while_inclusive(|(_, _, differences)| !differences.is_empty())
                        .collect::<Vec<_>>();
                    let equal = matching.pop_if(|(_, _, differences)| differences.is_empty());
                    if let Some((_, group, _)) = equal {
                        group.push(function);
                    } else {
                        compared.extend(matching.into_iter().map(|(kept, _, differences)| ((kept, function), differences)));
                        groups.insert(function, Vec::new());
                    }
                    groups
                })
                .into_iter()
                .map(|(root, members)| iter::once(root).chain(members).collect::<Vec<_>>())
        });
        let (repeated, distinct) = groups.partition::<Vec<_>, _>(|group| group.len() > 1);
        let distinct = repeated.iter().filter_map(|group| group.first()).chain(distinct.iter().flatten()).copied().collect::<FxHashSet<_>>();
        for group in &repeated {
            duplicated(cx, group);
        }
        let valued = by_signature.values().map(|bucket| bucket.iter().copied().filter(|function| distinct.contains(function)).map(|function| (function, ())).collect::<Vec<_>>());
        let typed = functions.iter().filter(|(function, _)| distinct.contains(function)).fold(FxIndexMap::<_, Vec<(LocalDefId, &Callable<'tcx>)>>::default(), |mut buckets, (function, contract)| {
            let mut hasher = SpanlessHash::new(cx);
            hasher.hash_body(tcx.hir_body_owned_by(*function).id());
            buckets.entry((hasher.finish(), contract.attributes)).or_default().push((*function, contract.as_ref()));
            buckets
        });
        let families = valued
            .flat_map(|bucket| families(&bucket, |earlier, (), later, ()| compared.get(&(earlier, later)).and_then(|differences| varied_value(cx, earlier, later, differences))))
            .chain(typed.values().flat_map(|bucket| {
                families(
                    bucket,
                    |earlier, earlier_signature, later, later_signature| {
                        if one_expansion(earlier, later) { None } else { varied_type(bodies, cx, earlier, later, earlier_signature, later_signature) }
                    },
                )
            }));
        for (root, (members, form)) in families {
            parameterized(cx, root, &members, &form);
        }
    }
}
