use core::{cell::RefCell, iter, ops::ControlFlow};
use std::rc::Rc;

use clippy_utils::{SpanlessEq, visitors::for_each_expr_without_closures};
use rustc_data_structures::{
    fingerprint::Fingerprint,
    fx::FxHashMap,
    stable_hash::{StableHash as _, StableHasher},
};
use rustc_hir::{
    AnonConst, BodyId, Expr, ExprKind, HirId, ItemId,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor as _},
};
use rustc_infer::infer::TyCtxtInferExt as _;
use rustc_lint::LateContext;
use rustc_middle::ty::{
    self, Binder, BottomUpFolder, Clauses, FnSig, GenericArgKind, GenericArgsRef, PolyFnSig, Ty, TyCtxt, TypeFoldable, TypeFolder, TypeSuperFoldable as _, TypeVisitableExt as _, TypeckResults,
    TypingEnv, Unnormalized,
};
use rustc_span::{SyntaxContext, kw};
use rustc_trait_selection::traits::{ObligationCause, ObligationCtxt};

use crate::{awaited_output, impl_owner, parameters_match};

// --- [MODELS] --------------------------------------------------------------------------

enum Resolved<'tcx> {
    Item(DefId, GenericArgsRef<'tcx>),
    Type(Ty<'tcx>),
    Body(LocalDefId),
}

struct BodyFacts<'tcx> {
    output: Option<Ty<'tcx>>,
    operations: Vec<Resolved<'tcx>>,
}

#[derive(PartialEq, Eq, Hash)]
pub(crate) struct Callable<'tcx> {
    signature: PolyFnSig<'tcx>,
    clauses: Clauses<'tcx>,
    attributes: Fingerprint,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Bodies<'tcx> {
    facts: RefCell<FxHashMap<LocalDefId, Option<Rc<BodyFacts<'tcx>>>>>,
    callables: RefCell<FxHashMap<LocalDefId, Option<Rc<Callable<'tcx>>>>>,
}

impl<'tcx> Bodies<'tcx> {
    pub(crate) fn callable(&self, tcx: TyCtxt<'tcx>, function: LocalDefId) -> Option<Rc<Callable<'tcx>>> {
        self.callables
            .borrow_mut()
            .entry(function)
            .or_insert_with(|| {
                let declared = tcx.fn_sig(function).instantiate_identity().skip_normalization();
                let output = tcx.asyncness(function).is_async().then(|| tcx.get_impl_future_output_ty(declared.skip_binder().output())).flatten();
                let signature = output.map_or(declared, |output| {
                    declared.map_bound(|signature| FnSig { inputs_and_output: tcx.mk_type_list_from_iter(signature.inputs().iter().copied().chain([output])), ..signature })
                });
                let clauses = tcx.mk_clauses_from_iter(tcx.clauses_of(function).instantiate_identity(tcx).clauses.into_iter().map(Unnormalized::skip_normalization));
                let (signature, clauses) = declaration(tcx, normalized(tcx, function.to_def_id(), Unnormalized::new((signature, clauses)))?, owner(tcx, function));
                let attributes = tcx.codegen_fn_attrs(function);
                let exported = (attributes.contains_extern_indicator() || !attributes.foreign_item_symbol_aliases.is_empty()).then_some(function.to_def_id());
                let overridable = tcx.opt_associated_item(function.to_def_id()).is_some_and(|associated| associated.defaultness(tcx).is_default());
                let attributes = tcx.with_stable_hashing_context(|mut context| {
                    let mut hasher = StableHasher::new();
                    context.while_hashing_spans(false, |context| {
                        ((tcx.constness(function), tcx.asyncness(function), tcx.visibility(function), overridable), attributes, exported).stable_hash(context, &mut hasher);
                    });
                    hasher.finish()
                });
                Some(Rc::new(Callable { signature, clauses, attributes }))
            })
            .clone()
    }

    fn body(&self, tcx: TyCtxt<'tcx>, owner: LocalDefId) -> Option<Rc<BodyFacts<'tcx>>> {
        struct BodyScope;

        impl<'tcx> intravisit::Visitor<'tcx> for BodyScope {
            type Result = ControlFlow<()>;

            fn visit_expr(&mut self, ex: &'tcx Expr<'tcx>) -> Self::Result { if matches!(ex.kind, ExprKind::Binary(..)) { ControlFlow::Break(()) } else { intravisit::walk_expr(self, ex) } }

            fn visit_anon_const(&mut self, _: &'tcx AnonConst) -> Self::Result { ControlFlow::Break(()) }

            fn visit_nested_item(&mut self, _: ItemId) -> Self::Result { ControlFlow::Break(()) }
        }

        self.facts
            .borrow_mut()
            .entry(owner)
            .or_insert_with(|| {
                let body = tcx.hir_body_owned_by(owner);
                if BodyScope.visit_body(body).is_break() {
                    return None;
                }
                let typeck = tcx.typeck(tcx.typeck_root_def_id_local(owner));
                let mut operations = Vec::new();
                let _: Option<()> = for_each_expr_without_closures(body.value, |expr| {
                    let resolved = typeck.type_dependent_def_id(expr.hir_id).or_else(|| match expr.kind {
                        ExprKind::Path(qpath) => match typeck.qpath_res(&qpath, expr.hir_id) {
                            Res::Def(DefKind::Fn | DefKind::AssocFn | DefKind::AssocConst, called) => Some(called),
                            _ => None,
                        },
                        _ => None,
                    });
                    if let Some((declared, arguments)) = resolved.and_then(|called| resolved_item(tcx, called, typeck.node_args(expr.hir_id))) {
                        operations.push(Resolved::Item(declared, arguments));
                    }
                    let constructed = match expr.kind {
                        ExprKind::Struct(..) => true,
                        ExprKind::Path(qpath) =>
                            typeck.expr_ty(expr).is_adt() && matches!(typeck.qpath_res(&qpath, expr.hir_id), Res::Def(DefKind::Variant | DefKind::Ctor(_, CtorKind::Const), _) | Res::SelfCtor(_)),
                        ExprKind::Call(callee, _) if let ExprKind::Path(qpath) = callee.kind => matches!(typeck.qpath_res(&qpath, callee.hir_id), Res::Def(DefKind::Ctor(..), _) | Res::SelfCtor(_)),
                        _ => false,
                    };
                    if constructed {
                        operations.push(Resolved::Type(typeck.expr_ty(expr)));
                    }
                    let nested = match expr.kind {
                        ExprKind::Closure(closure) => Some(closure.def_id),
                        ExprKind::ConstBlock(constant) => Some(constant.def_id),
                        _ => None,
                    };
                    if let ExprKind::Closure(_) = expr.kind
                        && let Some(signature) = match typeck.expr_ty(expr).kind() {
                            ty::Closure(_, args) => Some(args.as_closure().sig_as_fn_ptr_ty()),
                            ty::CoroutineClosure(_, args) => Some(args.as_coroutine_closure().signature_parts_ty()),
                            ty::Coroutine(_, args) => {
                                let signature = args.as_coroutine().sig();
                                Some(Ty::new_tup(tcx, &[signature.resume_ty, signature.yield_ty, signature.return_ty]))
                            }
                            _ => None,
                        }
                    {
                        operations.push(Resolved::Type(signature));
                    }
                    if let Some(nested) = nested {
                        operations.push(Resolved::Body(nested));
                    }
                    ControlFlow::Continue(())
                });
                let output = match tcx.def_kind(owner) {
                    DefKind::Fn | DefKind::AssocFn => Some(awaited_output(tcx, owner.to_def_id())),
                    DefKind::Closure | DefKind::SyntheticCoroutineBody => None,
                    _ => Some(tcx.type_of(owner).instantiate_identity().skip_normalization()),
                };
                Some(Rc::new(BodyFacts { output, operations }))
            })
            .clone()
    }

    pub(crate) fn equal<F, E>(&self, cx: &LateContext<'tcx>, left_function: LocalDefId, right_function: LocalDefId, mut differ: F, equivalent: E) -> Option<Vec<(HirId, HirId)>>
    where
        F: FnMut(&TypeckResults<'tcx>, &Expr<'_>, &TypeckResults<'tcx>, &Expr<'_>) -> bool,
        E: Fn(Ty<'tcx>, Ty<'tcx>) -> bool,
    {
        let closures = RefCell::new(Vec::<(BodyId, BodyId)>::new());
        let differences = RefCell::new(Vec::new());
        let [left_owner, right_owner] = [left_function, right_function].map(|item| impl_owner(cx.tcx, item.to_def_id()));
        let same_type = |left_type, right_type| equivalent(canonical(cx.tcx, left_type, left_owner), canonical(cx.tcx, right_type, right_owner));
        let mut pending = vec![(left_function, right_function)];
        while let Some((left_body, right_body)) = pending.pop() {
            let (left_facts, right_facts) = (self.body(cx.tcx, left_body)?, self.body(cx.tcx, right_body)?);
            let output = match (left_facts.output, right_facts.output) {
                (Some(left_type), Some(right_type)) => same_type(left_type, right_type),
                (None, None) => true,
                _ => false,
            };
            if !output
                || left_facts.operations.len() != right_facts.operations.len()
                || !iter::zip(&left_facts.operations, &right_facts.operations).all(|(left_operation, right_operation)| match (left_operation, right_operation) {
                    (Resolved::Item(left_item, left_args), Resolved::Item(right_item, right_args)) =>
                        left_item == right_item
                            && left_args.len() == right_args.len()
                            && iter::zip(*left_args, *right_args).all(|(left_argument, right_argument)| match (left_argument.kind(), right_argument.kind()) {
                                (GenericArgKind::Type(left_type), GenericArgKind::Type(right_type)) => same_type(left_type, right_type),
                                (GenericArgKind::Lifetime(_), GenericArgKind::Lifetime(_)) => true,
                                (left_kind, right_kind) => left_kind == right_kind,
                            }),
                    (Resolved::Type(left_type), Resolved::Type(right_type)) => same_type(*left_type, *right_type),
                    (Resolved::Body(left_nested), Resolved::Body(right_nested)) => {
                        pending.push((*left_nested, *right_nested));
                        true
                    }
                    (Resolved::Item(..), Resolved::Type(_) | Resolved::Body(_))
                    | (Resolved::Type(_), Resolved::Item(..) | Resolved::Body(_))
                    | (Resolved::Body(_), Resolved::Item(..) | Resolved::Type(_)) => false,
                })
            {
                return None;
            }
        }
        let equal = {
            let mut spanless = SpanlessEq::new(cx).expr_fallback(|left_typeck, left_expr, right_typeck, right_expr| {
                if let (ExprKind::Closure(left_closure), ExprKind::Closure(right_closure)) = (left_expr.kind, right_expr.kind) {
                    closures.borrow_mut().push((left_closure.body, right_closure.body));
                    left_closure.capture_clause == right_closure.capture_clause && left_closure.constness == right_closure.constness && left_closure.kind == right_closure.kind
                } else if differ(left_typeck, left_expr, right_typeck, right_expr) {
                    differences.borrow_mut().push((left_expr.hir_id, right_expr.hir_id));
                    true
                } else {
                    false
                }
            });
            let mut inter = spanless.inter_expr(SyntaxContext::root());
            let mut compare = |left_body: BodyId, right_body: BodyId| parameters_match(cx.tcx, &mut inter.locals, left_body, right_body) && inter.eq_body(left_body, right_body);
            compare(cx.tcx.hir_body_owned_by(left_function).id(), cx.tcx.hir_body_owned_by(right_function).id())
                && iter::from_fn(|| closures.borrow_mut().pop()).all(|(left_body, right_body)| compare(left_body, right_body))
        };
        equal.then(|| differences.into_inner())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn owner(tcx: TyCtxt<'_>, function: LocalDefId) -> Option<Ty<'_>> {
    let parent = tcx.parent(function.to_def_id());
    if matches!(tcx.def_kind(parent), DefKind::Impl { .. }) {
        Some(tcx.type_of(parent).instantiate_identity().skip_normalization())
    } else if tcx.def_kind(parent) == DefKind::Trait {
        Some(tcx.types.self_param)
    } else {
        None
    }
}

fn resolved_item<'tcx>(tcx: TyCtxt<'tcx>, called: DefId, arguments: GenericArgsRef<'tcx>) -> Option<(DefId, GenericArgsRef<'tcx>)> {
    match tcx.opt_associated_item(called).map(|item| item.container) {
        Some(ty::AssocContainer::TraitImpl(declared)) => {
            let owner = tcx.parent(called);
            let implemented = tcx.impl_trait_header(owner).trait_ref.instantiate(tcx, arguments).skip_normalization();
            Some((declared.ok()?, arguments.rebase_onto(tcx, owner, implemented.args)))
        }
        Some(ty::AssocContainer::Trait | ty::AssocContainer::InherentImpl) | None => Some((called, arguments)),
    }
}

fn canonical<'tcx, T>(tcx: TyCtxt<'tcx>, value: T, owner: Option<Ty<'tcx>>) -> T
where T: TypeFoldable<TyCtxt<'tcx>> {
    let parameter = |held: Ty<'tcx>| if let ty::Param(param) = held.kind() { Some(Ty::new_param(tcx, param.index, kw::Underscore)) } else { None };
    let owner = owner.map(|owner| substitute_types(tcx, tcx.erase_and_anonymize_regions(owner), parameter));
    substitute_types(tcx, tcx.erase_and_anonymize_regions(value), |held| if Some(held) == owner { Some(tcx.types.self_param) } else { parameter(held) })
}

fn declaration<'tcx, T>(tcx: TyCtxt<'tcx>, value: T, owner: Option<Ty<'tcx>>) -> T
where T: TypeFoldable<TyCtxt<'tcx>> {
    struct Declaration<'tcx> {
        tcx: TyCtxt<'tcx>,
        owner: Option<Ty<'tcx>>,
    }
    impl<'tcx> TypeFolder<TyCtxt<'tcx>> for Declaration<'tcx> {
        fn cx(&self) -> TyCtxt<'tcx> { self.tcx }

        fn fold_ty(&mut self, t: Ty<'tcx>) -> Ty<'tcx> {
            let ty = t.super_fold_with(self);
            let ty = match ty.kind() {
                ty::Param(parameter) => Ty::new_param(self.tcx, parameter.index, kw::Underscore),
                _ => ty,
            };
            if Some(ty) == self.owner { self.tcx.types.self_param } else { ty }
        }

        fn fold_region(&mut self, r: ty::Region<'tcx>) -> ty::Region<'tcx> {
            match r.kind() {
                ty::ReEarlyParam(parameter) => ty::Region::new_early_param(self.tcx, ty::EarlyParamRegion { index: parameter.index, name: kw::UnderscoreLifetime }),
                _ => r,
            }
        }

        fn fold_const(&mut self, c: ty::Const<'tcx>) -> ty::Const<'tcx> {
            match c.kind() {
                ty::ConstKind::Param(parameter) => ty::Const::new_param(self.tcx, ty::ParamConst { index: parameter.index, name: kw::Underscore }),
                _ => c.super_fold_with(self),
            }
        }

        fn fold_binder<U>(&mut self, t: Binder<'tcx, U>) -> Binder<'tcx, U>
        where U: TypeFoldable<TyCtxt<'tcx>> {
            self.tcx.anonymize_bound_vars(t).super_fold_with(self)
        }
    }
    let mut folder = Declaration { tcx, owner: None };
    folder.owner = owner.map(|owner| owner.fold_with(&mut folder));
    value.fold_with(&mut folder)
}

fn normalized<'tcx, T>(tcx: TyCtxt<'tcx>, owner: DefId, value: Unnormalized<'tcx, T>) -> Option<T>
where T: TypeFoldable<TyCtxt<'tcx>> + Copy {
    if !value.skip_normalization().has_aliases() {
        return Some(value.skip_normalization());
    }
    let (infcx, param_env) = tcx.infer_ctxt().build_with_typing_env(TypingEnv::non_body_analysis(tcx, owner));
    infcx.probe(|snapshot| {
        let obligations = ObligationCtxt::new(&infcx);
        let value = obligations.normalize(&ObligationCause::dummy(), param_env, value);
        let errors = obligations.evaluate_obligations_error_on_ambiguity();
        let value = infcx.deeply_resolve_ignoring_regions(value);
        (errors.no_errors() && !infcx.region_constraints_added_in_snapshot(snapshot) && !value.has_infer()).then_some(value)
    })
}

fn substitute_types<'tcx, T, F>(tcx: TyCtxt<'tcx>, value: T, substitute: F) -> T
where
    T: TypeFoldable<TyCtxt<'tcx>>,
    F: Fn(Ty<'tcx>) -> Option<Ty<'tcx>>,
{
    value.fold_with(&mut BottomUpFolder { tcx, ty_op: |ty| substitute(ty).unwrap_or(ty), lt_op: |region| region, ct_op: |constant| constant })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod duplicate_body;
pub(crate) mod duplicate_trait_constant;
pub(crate) mod duplicate_type;
