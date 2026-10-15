use core::ops::ControlFlow;

use clippy_utils::{SpanlessHash, diagnostics::span_lint_hir_and_then, peel_blocks, res::MaybeQPath as _, visitors::for_each_expr};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_errors::listify;
use rustc_hir::{
    Expr, ExprKind, HirId, Node,
    def::{CtorOf, DefKind},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::{self, BottomUpFolder, Clause, FnSig, GenericArg, Interner as _, Ty, TyCtxt, TypeFoldable, Unnormalized, print::with_forced_trimmed_paths};
use rustc_span::kw;

use crate::{constant, constructed, datum, delivered, equal_bodies, is_error, item_path, upcast};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    DUPLICATE_BODY,
    Warn,
    "functions and methods repeating another's body and signature"
}

declare_lint! {
    PARAMETERIZABLE_BODY,
    Warn,
    "functions repeating another's body with other values or one other type"
}

// --- [MODELS] --------------------------------------------------------------------------

enum Varied<'tcx> {
    Value(Option<DefId>),
    Holder(Ty<'tcx>),
    Type { from: Ty<'tcx>, traits: Vec<DefId>, direct: Option<DefId> },
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] RepeatedBody => [DUPLICATE_BODY, PARAMETERIZABLE_BODY]);

impl<'tcx> LateLintPass<'tcx> for RepeatedBody {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn families<'tcx, F>(bucket: &[LocalDefId], varied: F) -> FxIndexMap<LocalDefId, (Vec<LocalDefId>, Varied<'tcx>)>
        where F: Fn(LocalDefId, LocalDefId) -> Option<Varied<'tcx>> {
            bucket.iter().enumerate().fold(FxIndexMap::<LocalDefId, (Vec<LocalDefId>, Varied<'tcx>)>::default(), |mut families, (index, &later)| {
                if let Some((earlier, form)) = bucket.iter().take(index).find_map(|&earlier| varied(earlier, later).map(|form| (earlier, form))) {
                    let root = families.iter().find_map(|(&root, (members, _))| members.contains(&earlier).then_some(root)).unwrap_or(earlier);
                    families.entry(root).or_insert_with(|| (Vec::new(), form)).0.push(later);
                }
                families
            })
        }

        fn parameterized<'tcx>(cx: &LateContext<'tcx>, root: LocalDefId, members: &[LocalDefId], form: &Varied<'tcx>) {
            let tcx = cx.tcx;
            let root_path = item_path(cx, root.to_def_id());
            let listed = listify(members, |&member| format!("`{}`", item_path(cx, member.to_def_id()))).unwrap_or_default();
            let repeat = if let [_] = members { "repeats" } else { "repeat" };
            let message = with_forced_trimmed_paths!(match form {
                Varied::Type { from, traits: _, direct: _ } => format!("{listed} {repeat} the body of `{root_path}` with another type in place of `{from}`"),
                Varied::Value(_) | Varied::Holder(_) => format!("{listed} {repeat} the body of `{root_path}` with other values"),
            });
            let help = with_forced_trimmed_paths!(match form {
                Varied::Value(Some(callee)) => format!("call `{}` with the differing value at each site of `{root_path}` and {listed}", item_path(cx, *callee)),
                Varied::Value(None) => format!("take the differing value as a parameter of one function in place of `{root_path}` and {listed}"),
                Varied::Holder(root_owner) => format!("replace `{root_owner}` and each other holder with one type holding the value and a `const` per former type"),
                Varied::Type { from: _, traits: _, direct: Some(method) } => format!("call `{}` directly in place of `{root_path}` and {listed}", item_path(cx, *method)),
                Varied::Type { from: _, traits, direct: None } =>
                    format!("make one function generic over `S: {}` in place of `{root_path}` and {listed}", traits.iter().map(|&implemented| tcx.def_path_str(implemented)).join(" + ")),
            });
            span_lint_hir_and_then(cx, PARAMETERIZABLE_BODY, tcx.local_def_id_to_hir_id(root), tcx.def_span(root), message, |diag| {
                diag.span_note(members.iter().map(|&member| tcx.def_span(member)).collect::<Vec<_>>(), "repeats the body here");
                diag.help(help);
            });
        }

        fn varied_type<'tcx>(cx: &LateContext<'tcx>, earlier: LocalDefId, later: LocalDefId) -> Option<Varied<'tcx>> {
            let tcx = cx.tcx;
            let ((earlier_signature, _), (later_signature, _)) = (signature(cx, earlier), signature(cx, later));
            let adts = |types: &[Ty<'tcx>]| types.iter().flat_map(|held| held.walk()).filter_map(GenericArg::as_type).filter(|held| held.is_adt()).collect::<Vec<_>>();
            let (from_types, to_types) = (adts(earlier_signature.inputs_and_output), adts(later_signature.inputs_and_output));
            let mut candidates = from_types.iter().copied().filter(|held| !to_types.contains(held)).collect::<Vec<_>>();
            candidates.sort_by_key(|held| held.walk().count());
            let (from, to) = candidates.into_iter().find_map(|from| {
                to_types
                    .iter()
                    .copied()
                    .filter(|held| !from_types.contains(held))
                    .find(|&to| from.ty_adt_def() != to.ty_adt_def() && replaced(tcx, later_signature, to, from) == earlier_signature)
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
            equal_bodies(cx, earlier, later, |left_typeck, left_expr, right_typeck, right_expr| {
                let (left_ty, right_ty) = (tcx.erase_and_anonymize_regions(left_typeck.expr_ty(left_expr)), tcx.erase_and_anonymize_regions(right_typeck.expr_ty(right_expr)));
                matches!((left_expr.kind, right_expr.kind), (ExprKind::Path(_), ExprKind::Path(_)))
                    && matches!((left_ty.kind(), right_ty.kind()), (ty::FnDef(..) | ty::Adt(..), ty::FnDef(..) | ty::Adt(..)))
                    && (replaced(tcx, right_ty, to, from) == left_ty || ctor_of(left_ty, from) && ctor_of(right_ty, to))
            })?;
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
                (!called.iter().any(|&(_, item)| {
                    tcx.inherent_impl_of_assoc(item).is_some_and(|implementation| tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def() == held.ty_adt_def())
                }))
                .then(|| called.iter().filter(|&&(node, _)| receives(node)).filter_map(|&(_, item)| tcx.trait_of_assoc(item)).unique().collect::<Vec<_>>())
            };
            let traits = generic_calls(earlier, from)?;
            let body = peel_blocks(tcx.hir_body_owned_by(earlier).value);
            let typeck = tcx.typeck(earlier);
            let direct = if let ExprKind::Call(callee, _) = body.kind
                && let ExprKind::Path(qpath) = callee.kind
            {
                typeck.qpath_res(&qpath, callee.hir_id).opt_def_id()
            } else if let ExprKind::MethodCall(..) = body.kind {
                typeck.type_dependent_def_id(body.hir_id)
            } else {
                None
            }
            .filter(|&called| tcx.trait_of_assoc(called).is_some_and(|implemented| traits.contains(&implemented)));
            (owner(cx, earlier) == owner(cx, later) && !traits.is_empty() && generic_calls(later, to).is_some()).then_some(Varied::Type { from, traits, direct })
        }

        fn varied_value<'tcx>(cx: &LateContext<'tcx>, earlier: LocalDefId, later: LocalDefId, differences: &[(HirId, HirId)]) -> Option<Varied<'tcx>> {
            let tcx = cx.tcx;
            let typeck = tcx.typeck(earlier);
            let body = peel_blocks(tcx.hir_body_owned_by(earlier).value);
            let fieldless = |function: LocalDefId| owner(cx, function).filter(|held| held.ty_adt_def().is_some_and(|adt| adt.is_struct() && adt.all_fields().next().is_none()) && !is_error(cx, *held));
            let direct = |arguments: &[Expr<'_>]| differences.iter().all(|&(differing, _)| arguments.iter().any(|argument| argument.hir_id == differing));
            let called = if let ExprKind::Call(callee, arguments) = body.kind
                && let ExprKind::Path(qpath) = callee.kind
                && direct(arguments)
            {
                typeck.qpath_res(&qpath, callee.hir_id).opt_def_id()
            } else if let ExprKind::MethodCall(_, _, arguments, _) = body.kind
                && direct(arguments)
            {
                typeck.type_dependent_def_id(body.hir_id)
            } else {
                None
            }
            .filter(|&callee| matches!(tcx.def_kind(callee), DefKind::Fn | DefKind::AssocFn));
            let compared = |differing: HirId| matches!(tcx.parent_hir_node(differing), Node::Expr(parent) if matches!(parent.kind, ExprKind::Binary(operator, ..) if operator.node.is_comparison()));
            let tabled = |differing: HirId| matches!(tcx.parent_hir_node(differing), Node::Arm(arm) if arm.body.hir_id == differing);
            if constant(typeck, body) || differences.iter().all(|&(differing, _)| tabled(differing)) {
                None
            } else if owner(cx, earlier) == owner(cx, later) && [earlier, later].into_iter().any(|function| tcx.trait_item_of(function).is_none()) {
                (called.is_some() || !differences.iter().all(|&(differing, _)| compared(differing))).then_some(Varied::Value(called))
            } else if let (Some(earlier_owner), Some(_)) = (fieldless(earlier), fieldless(later))
                && let Some(declaration) = tcx.trait_item_of(earlier)
                && tcx.trait_item_of(later) == Some(declaration)
                && [earlier, later].into_iter().all(|function| bound_to_trait(cx, function, tcx.parent(declaration)))
            {
                Some(Varied::Holder(earlier_owner))
            } else {
                None
            }
        }

        fn duplicated(cx: &LateContext<'_>, group: &[LocalDefId]) {
            let tcx = cx.tcx;
            let deletable = |function: LocalDefId| tcx.trait_impl_of_assoc(function.to_def_id()).is_none() && tcx.trait_of_assoc(function.to_def_id()).is_none();
            let shared = |left: LocalDefId, right: LocalDefId| owner(cx, left) == owner(cx, right);
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
                        diag.span_note(tcx.def_span(kept), format!("`{kept_path}` holds the same body"));
                        diag.help(format!("delete `{duplicate_path}` and call `{kept_path}` at each site"));
                    },
                );
            }
            if let [first, _, ..] = group
                && let Some(declaration) = tcx.trait_item_of(*first)
                && let Some(local) = declaration.as_local()
                && !tcx.defaultness(declaration).has_value()
                && let implemented = tcx.parent(declaration)
                && tcx.all_impls(implemented).count() == group.len()
                && upcast(cx, local).is_none()
                && group.iter().all(|&function| tcx.trait_item_of(function) == Some(declaration) && bound_to_trait(cx, function, implemented))
            {
                let (method, name) = (tcx.item_name(declaration), tcx.item_name(implemented));
                span_lint_hir_and_then(cx, DUPLICATE_BODY, tcx.local_def_id_to_hir_id(local), tcx.def_span(declaration), format!("every `{name}` impl repeats one body for `{method}`"), |diag| {
                    diag.span_note(group.iter().map(|&function| tcx.def_span(function)).collect::<Vec<_>>(), "each impl holds the same body");
                    diag.help(format!("move `{method}` into a default method of `{name}` and remove it from each impl"));
                });
            }
        }

        let tcx = cx.tcx;
        let functions = tcx
            .hir_body_owners()
            .filter(|&owner| {
                matches!(tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                    && !tcx.def_span(owner).from_expansion()
                    && !tcx.impl_of_assoc(owner.to_def_id()).is_some_and(|implementation| tcx.is_automatically_derived(implementation))
            })
            .map(|function| {
                let mut hasher = SpanlessHash::new(cx);
                hasher.hash_body(tcx.hir_body_owned_by(function).id());
                (function, hasher.finish(), signature(cx, function))
            })
            .collect::<Vec<_>>();
        let by_signature = functions.iter().fold(FxIndexMap::<_, Vec<LocalDefId>>::default(), |mut buckets, (function, _, signature)| {
            buckets.entry(signature).or_default().push(*function);
            buckets
        });
        let compared = by_signature
            .values()
            .flat_map(|bucket| bucket.iter().enumerate().flat_map(move |(index, &later)| bucket.iter().take(index).map(move |&earlier| (earlier, later))))
            .filter_map(|(earlier, later)| {
                let differences = equal_bodies(cx, earlier, later, |left_typeck, left_expr, right_typeck, right_expr| {
                    tcx.erase_and_anonymize_regions(left_typeck.expr_ty(left_expr)) == tcx.erase_and_anonymize_regions(right_typeck.expr_ty(right_expr))
                        && datum(left_typeck, left_expr)
                        && datum(right_typeck, right_expr)
                })?;
                Some(((earlier, later), differences))
            })
            .collect::<FxHashMap<_, _>>();
        let groups = by_signature.values().flat_map(|bucket| {
            bucket.iter().fold(Vec::<Vec<LocalDefId>>::new(), |mut groups, &function| {
                match groups.iter_mut().find(|group| group.first().is_some_and(|&kept| compared.get(&(kept, function)).is_some_and(Vec::is_empty))) {
                    Some(group) => group.push(function),
                    None => groups.push(vec![function]),
                }
                groups
            })
        });
        let (repeated, distinct) = groups.partition::<Vec<_>, _>(|group| group.len() > 1);
        let distinct = repeated.iter().filter_map(|group| group.first()).chain(distinct.iter().flatten()).copied().collect::<FxHashSet<_>>();
        for group in &repeated {
            duplicated(cx, group);
        }
        let valued = by_signature.values().map(|bucket| bucket.iter().copied().filter(|function| distinct.contains(function)).collect::<Vec<_>>());
        let typed = functions.iter().filter(|(function, ..)| distinct.contains(function)).fold(FxIndexMap::<u64, Vec<LocalDefId>>::default(), |mut buckets, &(function, hash, _)| {
            buckets.entry(hash).or_default().push(function);
            buckets
        });
        let families = valued
            .flat_map(|bucket| families(&bucket, |earlier, later| compared.get(&(earlier, later)).and_then(|differences| varied_value(cx, earlier, later, differences))))
            .chain(typed.values().flat_map(|bucket| families(bucket, |earlier, later| varied_type(cx, earlier, later))));
        for (root, (members, form)) in families {
            parameterized(cx, root, &members, &form);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn owner<'tcx>(cx: &LateContext<'tcx>, function: LocalDefId) -> Option<Ty<'tcx>> {
    let parent = cx.tcx.parent(function.to_def_id());
    if matches!(cx.tcx.def_kind(parent), DefKind::Impl { of_trait: _ }) {
        Some(cx.tcx.type_of(parent).instantiate_identity().skip_normalization())
    } else if cx.tcx.def_kind(parent) == DefKind::Trait {
        Some(cx.tcx.types.self_param)
    } else {
        None
    }
}

fn replaced<'tcx, T>(tcx: TyCtxt<'tcx>, value: T, from: Ty<'tcx>, to: Ty<'tcx>) -> T
where T: TypeFoldable<TyCtxt<'tcx>> {
    value.fold_with(&mut BottomUpFolder { tcx, ty_op: |ty| if ty == from { to } else { ty }, lt_op: |region| region, ct_op: |constant| constant })
}

fn signature<'tcx>(cx: &LateContext<'tcx>, function: LocalDefId) -> (FnSig<'tcx>, Vec<Clause<'tcx>>) {
    let tcx = cx.tcx;
    let mut canonical = BottomUpFolder {
        tcx,
        ty_op: |ty: Ty<'tcx>| if let ty::Param(param) = ty.kind() { Ty::new_param(tcx, param.index, kw::Underscore) } else { ty },
        lt_op: |region| region,
        ct_op: |constant| constant,
    };
    let declared = tcx.instantiate_bound_regions_with_erased(tcx.fn_sig(function).instantiate_identity().skip_normalization());
    let delivered = FnSig { inputs_and_output: tcx.mk_type_list_from_iter(declared.inputs().iter().copied().chain([delivered(tcx, function.to_def_id())])), ..declared };
    let clauses = tcx.clauses_of(function).instantiate_identity(tcx).clauses.into_iter().map(Unnormalized::skip_normalization).collect::<Vec<_>>();
    let erased = tcx.erase_and_anonymize_regions((delivered, clauses)).fold_with(&mut canonical);
    match owner(cx, function) {
        Some(owner) => replaced(tcx, erased, owner.fold_with(&mut canonical), Ty::new_param(tcx, u32::MAX, kw::SelfUpper)),
        None => erased,
    }
}

fn bound_to_trait(cx: &LateContext<'_>, function: LocalDefId, implemented: DefId) -> bool {
    let typeck = cx.tcx.typeck(function);
    let supertraits = cx.tcx.supertrait_def_ids(implemented).collect::<Vec<_>>();
    owner(cx, function).is_some_and(|owner| {
        let owned = |expr: &Expr<'_>| typeck.expr_ty_adjusted(expr).peel_refs() == owner;
        for_each_expr(cx.tcx, cx.tcx.hir_body_owned_by(function).value, |expr| {
            let escapes = matches!(expr.kind, ExprKind::Field(base, _) if owned(base))
                || matches!(expr.kind, ExprKind::MethodCall(_, receiver, ..)
                    if owned(receiver) && !typeck.type_dependent_def_id(expr.hir_id).and_then(|method| cx.tcx.trait_of_assoc(method)).is_some_and(|declared| supertraits.contains(&declared)))
                || expr
                    .res(typeck)
                    .opt_def_id()
                    .and_then(|item| cx.tcx.inherent_impl_of_assoc(item))
                    .is_some_and(|implementation| cx.tcx.type_of(implementation).instantiate_identity().skip_normalization() == owner)
                || owned(expr) && constructed(typeck, expr).is_some();
            if escapes { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }
        })
        .is_none()
    })
}
