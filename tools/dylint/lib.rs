#![feature(const_default, const_trait_impl, default_field_values, deref_patterns, gen_blocks, rustc_private, try_blocks)]

extern crate rustc_abi;
extern crate rustc_ast;
extern crate rustc_attr_ir;
extern crate rustc_crate_store;
extern crate rustc_data_structures;
extern crate rustc_errors;
extern crate rustc_expand;
extern crate rustc_hir;
extern crate rustc_hir_typeck;
extern crate rustc_infer;
extern crate rustc_lexer;
extern crate rustc_lint;
extern crate rustc_middle;
extern crate rustc_parse;
extern crate rustc_parse_format;
extern crate rustc_session;
extern crate rustc_span;
extern crate rustc_trait_selection;
extern crate rustc_type_ir;

use core::{
    cell::{LazyCell, RefCell},
    iter, mem,
    ops::ControlFlow,
};
use std::{path::Path, rc::Rc};

use camino::Utf8Path;
use clippy_utils::{
    consts::ConstEvalCtxt,
    fn_def_id, get_expr_use_site, get_parent_expr, is_from_proc_macro, is_in_cfg_test, is_in_test_function,
    macros::{FormatArgsStorage, macro_backtrace},
    paths::{PathNS, lookup_path_str},
    peel_blocks, peel_hir_expr_refs,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, is_copy},
    visitors::{Visitable, for_each_expr},
};
use itertools::{Either, Itertools as _};
use rustc_abi::FieldIdx;
use rustc_ast::{Crate, Expr as AstExpr, ExprKind as AstExprKind, FormatArgs};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxBuildHasher, FxHashMap, FxHashSet};
use rustc_errors::{Applicability, Diag, DiagMessage};
use rustc_hir::{
    Body, BodyId, Expr, ExprKind, HirId, HirIdMap, HirIdSet, Mutability, Node, Pat, PatExpr, PatKind, UnOp,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId, LocalDefIdMap},
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_infer::infer::TyCtxtInferExt as _;
use rustc_lint::{EarlyContext, EarlyLintPass, LateContext, LateLintPass, LintId, LintPass, LintVec};
use rustc_middle::ty::{
    self, Clause, ExistentialPredicateStableCmpExt as _, FieldDef, FnSig, Interner as _, PolyTraitClause, Ty, TyCtxt, TypeckResults, TypingEnv, Unnormalized, VariantDef,
    adjustment::{Adjust, AutoBorrow, DerefAdjustKind, PointerCoercion},
};
use rustc_span::{ExpnKind, FileName, MacroKind, RemapPathScopeComponents, Span, Symbol};
use rustc_trait_selection::traits::{Obligation, ObligationCause, ObligationCtxt};
use rustc_type_ir::elaborate::{elaborate, supertraits};

// --- [TYPES] ---------------------------------------------------------------------------

type ReportedRenderings = Option<Rc<RefCell<LocalDefIdMap<HirIdSet>>>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Place {
    Field(LocalDefId),
    Static(LocalDefId),
    Local(HirId),
    Parameter(HirId),
}

impl Place {
    fn hir_id(self, cx: &LateContext<'_>) -> HirId {
        match self {
            Self::Field(item) | Self::Static(item) => cx.tcx.local_def_id_to_hir_id(item),
            Self::Local(binding) | Self::Parameter(binding) => binding,
        }
    }

    fn span(self, cx: &LateContext<'_>) -> Span {
        match self {
            Self::Field(item) | Self::Static(item) => cx.tcx.def_span(item),
            Self::Local(binding) | Self::Parameter(binding) => cx.tcx.hir_span(binding),
        }
    }

    fn name(self, cx: &LateContext<'_>) -> Symbol { cx.tcx.hir_name(self.hir_id(cx)) }
}

#[derive(Clone, Copy)]
enum Access<'tcx> {
    Copied,
    Borrowed,
    Moved,
    Mutated,
    Assigned(&'tcx Expr<'tcx>),
}

impl Access<'_> {
    const fn writes(self) -> bool {
        match self {
            Self::Mutated | Self::Assigned(_) => true,
            Self::Copied | Self::Borrowed | Self::Moved => false,
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct FormatArgsCollector {
    format_args: FxHashMap<Span, FormatArgs>,
    storage: FormatArgsStorage,
}

struct Recorded {
    value: FxHashSet<DefId>,
    record: FxHashSet<DefId>,
    wrappers: FxHashSet<DefId>,
}

impl Recorded {
    fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            value: paths(tcx, PathNS::Type, &["tracing_core::field::Value"]),
            record: paths(tcx, PathNS::Value, &["tracing::span::Span::record"]),
            wrappers: paths(tcx, PathNS::Value, &["tracing_core::field::display", "tracing_core::field::debug"]),
        }
    }

    fn field<'tcx>(&self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(&'tcx Expr<'tcx>, Span)> {
        let typeck = cx.typeck_results();
        let recorded = match expr.kind {
            ExprKind::Cast(recorded, _)
                if let ty::Ref(_, object, _) = typeck.expr_ty(expr).kind()
                    && let ty::Dynamic(predicates, ..) = object.kind()
                    && predicates.principal_def_id().is_some_and(|principal| self.value.contains(&principal)) =>
                Some(recorded),
            ExprKind::Call(_, [_, _, recorded]) | ExprKind::MethodCall(_, _, [_, recorded], _) if fn_def_id(cx, expr).is_some_and(|method| self.record.contains(&method)) => Some(recorded),
            _ => None,
        }?;
        let call = expr.span.source_callsite();
        (!call.from_expansion() && !macro_backtrace(expr.span).any(|expansion| matches!(expansion.kind, MacroKind::Attr | MacroKind::Derive))).then(|| {
            let value = peel_hir_expr_refs(recorded).0;
            let shown = match value.kind {
                ExprKind::Call(_, [shown]) if fn_def_id(cx, value).is_some_and(|wrapper| self.wrappers.contains(&wrapper)) => peel_hir_expr_refs(shown).0,
                _ => value,
            };
            (shown, call)
        })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ORIGINS]

fn workspace_def(cx: &LateContext<'_>, item: DefId) -> bool {
    let source_map = cx.tcx.sess.source_map();
    item.is_local()
        || matches!(source_map.span_to_filename(cx.tcx.def_span(item)), FileName::Real(file)
            if source_map.working_dir().local_path().is_some_and(|root| file.embeddable_name(RemapPathScopeComponents::DIAGNOSTICS).1.starts_with(root)))
}

fn in_test(tcx: TyCtxt<'_>, node: HirId) -> bool { is_in_test_function(tcx, node) || is_in_cfg_test(tcx, node) }

fn expanded_externally(cx: &LateContext<'_>, span: Span) -> bool {
    span.macro_backtrace().any(|expansion| !matches!(expansion.kind, ExpnKind::Macro(MacroKind::Bang, _)) || !expansion.macro_def_id.is_some_and(|written| workspace_def(cx, written)))
}

fn derived(tcx: TyCtxt<'_>, item: DefId) -> bool {
    matches!(tcx.expn_that_defined(item).expn_data().kind, ExpnKind::Macro(MacroKind::Derive, _)) || macro_backtrace(tcx.def_span(item)).any(|call| call.kind == MacroKind::Derive)
}

fn derived_impl(tcx: TyCtxt<'_>, implementation: DefId) -> bool { tcx.is_automatically_derived(implementation) && derived(tcx, implementation) }

fn exported(cx: &LateContext<'_>, item: LocalDefId) -> bool { cx.tcx.entry_fn(()).is_none() && cx.effective_visibilities.is_exported(item) }

fn suggest<M, R>(cx: &LateContext<'_>, node: HirId, diag: &mut Diag<'_>, span: Span, help: M, replacement: R, applicability: Applicability)
where
    M: Into<DiagMessage>,
    R: ToString,
{
    let generated = match cx.tcx.hir_node(node) {
        Node::Expr(expr) => is_from_proc_macro(cx, expr),
        Node::Pat(pat) => is_from_proc_macro(cx, pat),
        Node::Item(item) => is_from_proc_macro(cx, item),
        Node::TraitItem(item) => is_from_proc_macro(cx, item),
        Node::ImplItem(item) => is_from_proc_macro(cx, item),
        Node::Field(field) => is_from_proc_macro(cx, field),
        Node::Variant(variant) => is_from_proc_macro(cx, variant),
        Node::Ty(ty) => is_from_proc_macro(cx, ty),
        _ => false,
    };
    if span.from_expansion() || generated {
        diag.span_help(span, help);
    } else {
        diag.span_suggestion(span, help, replacement, applicability);
    }
}

fn record_rendering(cx: &LateContext<'_>, reported: &ReportedRenderings, source: HirId) {
    if let Some(reported) = reported {
        let root = cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(source));
        reported.borrow_mut().entry(root).or_default().insert(source);
    }
}

// --- [PATHS]

fn resolve<T>(tcx: TyCtxt<'_>, namespace: PathNS, table: &[(&str, T)]) -> FxHashMap<DefId, T>
where T: Copy {
    table.iter().flat_map(|&(path, value)| lookup_path_str(tcx, namespace, path).into_iter().map(move |resolved| (resolved, value))).collect()
}

fn paths(tcx: TyCtxt<'_>, namespace: PathNS, listed: &[&str]) -> FxHashSet<DefId> { listed.iter().flat_map(|listed| lookup_path_str(tcx, namespace, listed)).collect() }

fn path(tcx: TyCtxt<'_>, namespace: PathNS, named: &str) -> Option<DefId> { lookup_path_str(tcx, namespace, named).into_iter().next() }

fn impl_owner(tcx: TyCtxt<'_>, item: DefId) -> Option<Ty<'_>> { tcx.impl_of_assoc(item).map(|implementation| tcx.type_of(implementation).instantiate_identity().skip_normalization()) }

fn associated_trait(tcx: TyCtxt<'_>, item: DefId) -> Option<DefId> { tcx.trait_of_assoc(tcx.opt_associated_item(item)?.trait_item_or_self().ok()?) }

// --- [TYPES]

fn result_parts<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>) -> Option<(Ty<'tcx>, Ty<'tcx>)> {
    if let ty::Adt(result, args) = ty.kind()
        && result.is_diag_item(cx, sym::Result)
    {
        Some((args.type_at(0), args.type_at(1)))
    } else {
        None
    }
}

fn pointee<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>) -> Ty<'tcx> {
    let referent = ty.peel_refs();
    if let ty::Adt(owner, args) = referent.kind()
        && (owner.is_box() || matches!(owner.opt_diag_name(cx), Some(sym::Arc | sym::Rc | sym::Cow)))
        && let Some(inner) = args.types().next()
    {
        inner
    } else {
        referent
    }
}

fn is_text<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>) -> bool {
    let text = pointee(cx, ty);
    text.is_str() || text.is_lang_item(cx, LangItem::String)
}

fn is_error<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>) -> bool { cx.tcx.get_diagnostic_item(sym::Error).is_some_and(|error| implements_trait(cx, pointee(cx, ty), error, &[])) }

fn derives<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>, derived: Symbol) -> bool {
    cx.tcx.get_diagnostic_item(derived).is_some_and(|implemented| cx.tcx.non_blanket_impls_for_ty(implemented, ty).any(|implementation| derived_impl(cx.tcx, implementation)))
}

fn signature(tcx: TyCtxt<'_>, item: DefId) -> FnSig<'_> {
    tcx.instantiate_bound_regions_with_erased(tcx.normalize_erasing_regions(TypingEnv::non_body_analysis(tcx, item), tcx.fn_sig(item).instantiate_identity()))
}

fn awaited_output(tcx: TyCtxt<'_>, item: DefId) -> Ty<'_> {
    let output = signature(tcx, item).output();
    tcx.get_impl_future_output_ty(output).map_or(output, |future| tcx.normalize_erasing_regions(TypingEnv::non_body_analysis(tcx, item), Unnormalized::new(future)))
}

fn bounds_of<'tcx>(tcx: TyCtxt<'tcx>, item: DefId, param: Ty<'tcx>) -> impl Iterator<Item = PolyTraitClause<'tcx>> {
    tcx.param_env(item).caller_bounds().filter_map(Clause::as_trait_clause).filter(move |bound| bound.self_ty().skip_binder() == param)
}

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

// --- [PLACES]

fn accessed_field(typeck: &TypeckResults<'_>, access: &Expr<'_>) -> Option<DefId> {
    let ExprKind::Field(base, _) = access.kind else { return None };
    Some(typeck.expr_ty_adjusted(base).peel_refs().ty_adt_def()?.non_enum_variant().fields[typeck.field_index(access.hir_id)].did)
}

fn place(cx: &LateContext<'_>, expr: &Expr<'_>) -> Option<Place> {
    match expr.kind {
        ExprKind::Field(..) => accessed_field(cx.typeck_results(), expr)?.as_local().map(Place::Field),
        ExprKind::Path(qpath) => match cx.qpath_res(&qpath, expr.hir_id) {
            Res::Local(binding) => Some(if matches!(cx.tcx.parent_hir_node(binding), Node::Param(_)) { Place::Parameter(binding) } else { Place::Local(binding) }),
            res => res.opt_def_id().filter(|&item| cx.tcx.is_static(item))?.as_local().map(Place::Static),
        },
        _ => None,
    }
}

fn access<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Access<'tcx> {
    let typeck = cx.typeck_results();
    let mut current = expr;
    let result = loop {
        let site = get_expr_use_site(cx.tcx, typeck, current.span.ctxt(), current);
        let used = if let Node::Expr(used) = site.node { Some(used) } else { None };
        let borrowed = site
            .adjustments
            .iter()
            .find_map(|adjustment| match adjustment.kind {
                Adjust::Borrow(AutoBorrow::Ref(borrow)) => Some(Mutability::from(borrow)),
                Adjust::Borrow(AutoBorrow::RawPtr(mutability) | AutoBorrow::Pin(mutability)) | Adjust::GenericReborrow(mutability) => Some(mutability),
                Adjust::Deref(DerefAdjustKind::Overloaded(deref)) => Some(deref.mutbl),
                Adjust::Deref(DerefAdjustKind::Builtin | DerefAdjustKind::Pin) | Adjust::NeverToAny | Adjust::Pointer(_) => None,
            })
            .or_else(|| if let ExprKind::AddrOf(_, mutability, _) = used?.kind { Some(mutability) } else { None });
        match (borrowed, used.map(|used| (used, used.kind))) {
            (Some(Mutability::Mut), _) => break Access::Mutated,
            (Some(Mutability::Not), _) => break Access::Borrowed,
            (None, Some((_, ExprKind::Assign(target, value, _)))) if target.hir_id == site.child_id => break Access::Assigned(value),
            (None, Some((_, ExprKind::AssignOp(_, target, _)))) if target.hir_id == site.child_id => break Access::Mutated,
            (None, Some((projection, ExprKind::Field(base, _) | ExprKind::Unary(UnOp::Deref, base) | ExprKind::Index(base, ..)))) if base.hir_id == site.child_id => current = projection,
            (None, _) if is_copy(cx, typeck.expr_ty(current)) => break Access::Copied,
            (None, _) => break Access::Moved,
        }
    };
    match (current.hir_id == expr.hir_id, result) {
        (true, result @ (Access::Copied | Access::Borrowed | Access::Moved | Access::Mutated | Access::Assigned(_))) => result,
        (false, Access::Assigned(_) | Access::Mutated) => Access::Mutated,
        (false, Access::Copied | Access::Borrowed) => Access::Borrowed,
        (false, Access::Moved) => Access::Moved,
    }
}

fn cloned<'tcx>(cx: &LateContext<'tcx>, call: &'tcx Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    let (ExprKind::Call(_, [source]) | ExprKind::MethodCall(_, source, [], _)) = call.kind else { return None };
    fn_def_id(cx, call).and_then(|method| associated_trait(cx.tcx, method)).is_diag_item(cx, sym::Clone).then_some(source.peel_borrows())
}

fn pattern_fields<'tcx>(typeck: &TypeckResults<'tcx>, pat: &'tcx Pat<'tcx>) -> impl Iterator<Item = (DefId, &'tcx Pat<'tcx>)> {
    let variant = |qpath| typeck.pat_ty(pat).ty_adt_def().map(|adt| adt.variant_of_res(typeck.qpath_res(qpath, pat.hir_id)));
    let named = if let PatKind::Struct(qpath, fields, _) = &pat.kind {
        variant(qpath).map(|variant| fields.iter().map(move |field| (variant.fields[typeck.field_index(field.hir_id)].did, field.pat)))
    } else {
        None
    };
    let positional = if let PatKind::TupleStruct(qpath, fields, rest) = &pat.kind {
        variant(qpath).map(|variant| fields.iter().enumerate_and_adjust(variant.fields.len(), *rest).map(move |(index, field)| (variant.fields[FieldIdx::from_usize(index)].did, field)))
    } else {
        None
    };
    named.into_iter().flatten().chain(positional.into_iter().flatten())
}

// --- [EXPRESSIONS]

fn constructed<'tcx>(typeck: &TypeckResults<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(&'tcx VariantDef, impl ExactSizeIterator<Item = (&'tcx FieldDef, &'tcx Expr<'tcx>)> + Clone)> {
    let (variant, fields) = match expr.kind {
        ExprKind::Struct(qpath, fields, _) => {
            let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(typeck.qpath_res(qpath, expr.hir_id));
            (variant, Either::Left(fields))
        }
        ExprKind::Call(&Expr { kind: ExprKind::Path(qpath), hir_id, .. }, arguments) if let ctor @ (Res::Def(DefKind::Ctor(..), _) | Res::SelfCtor(_)) = typeck.qpath_res(&qpath, hir_id) => {
            let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(ctor);
            (variant, Either::Right(arguments))
        }
        ExprKind::Path(qpath)
            if let ctor @ (Res::Def(DefKind::Ctor(_, CtorKind::Const), _) | Res::SelfCtor(_)) = typeck.qpath_res(&qpath, expr.hir_id)
                && let ty::Adt(adt, _) = typeck.expr_ty(expr).kind() =>
            (adt.variant_of_res(ctor), Either::Right(&[][..])),
        _ => return None,
    };
    Some((variant, fields.map_either(|fields| fields.iter().map(move |field| (&variant.fields[typeck.field_index(field.hir_id)], field.expr)), |arguments| iter::zip(&variant.fields, arguments))))
}

fn literal_or_const(typeck: &TypeckResults<'_>, expr: &Expr<'_>) -> bool {
    matches!(expr.kind, ExprKind::Lit(_))
        || matches!(expr.kind, ExprKind::Unary(UnOp::Neg, operand) if matches!(operand.kind, ExprKind::Lit(_)))
        || matches!(expr.kind, ExprKind::Path(qpath) if matches!(typeck.qpath_res(&qpath, expr.hir_id), Res::Def(DefKind::Const | DefKind::AssocConst | DefKind::Ctor(_, CtorKind::Const), _)))
}

fn any_expr<'tcx, N, F>(tcx: TyCtxt<'tcx>, node: N, mut found: F) -> bool
where
    N: Visitable<'tcx>,
    F: FnMut(&'tcx Expr<'tcx>) -> bool,
{
    for_each_expr(tcx, node, |expr| if found(expr) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }).is_some()
}

fn fn_item_value(cx: &LateContext<'_>, expr: &Expr<'_>) -> Option<DefId> {
    if let ExprKind::Path(qpath) = &expr.kind
        && let Res::Def(DefKind::Fn | DefKind::AssocFn, function) = cx.qpath_res(qpath, expr.hir_id)
        && !get_parent_expr(cx, expr).is_some_and(|call| matches!(call.kind, ExprKind::Call(callee, _) if callee.hir_id == expr.hir_id))
    {
        Some(function)
    } else {
        None
    }
}

fn parameter(body: &Body<'_>, expr: &Expr<'_>) -> Option<usize> {
    let local = expr.res_local_id()?;
    body.params.iter().position(|parameter| parameter.pat.hir_id == local)
}

fn parameter_of<'tcx>(cx: &LateContext<'tcx>, call: &Expr<'_>, argument: &Expr<'_>) -> Option<(DefId, Ty<'tcx>)> {
    let (ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _)) = call.kind else { return None };
    let position = arguments.iter().position(|candidate| candidate.hir_id == argument.hir_id)?;
    let callee = fn_def_id(cx, call)?;
    let inputs = signature(cx.tcx, callee).inputs();
    Some((callee, *inputs.get(inputs.len().checked_sub(arguments.len())?..)?.get(position)?))
}

// --- [ITEMS]

fn generated(cx: &LateContext<'_>, node: HirId) -> bool {
    iter::once(node.owner)
        .chain(cx.tcx.hir_parent_owner_iter(node).map(|(owner, _)| owner))
        .any(|owner| derived(cx.tcx, owner.to_def_id()) || macro_backtrace(cx.tcx.def_span(owner)).any(|call| call.kind == MacroKind::Attr))
}

fn trait_impls(tcx: TyCtxt<'_>) -> FxHashMap<LocalDefId, Vec<(DefId, LocalDefId)>> {
    tcx.all_local_trait_impls(())
        .iter()
        .flat_map(|(&implemented, implementations)| implementations.iter().map(move |&implementation| (implemented, implementation)))
        .filter_map(|implementation @ (_, local)| Some((tcx.type_of(local).instantiate_identity().skip_normalization().peel_refs().ty_adt_def()?.did().as_local()?, implementation)))
        .into_grouping_map_with_hasher(FxBuildHasher)
        .collect()
}

fn enclosing_trait_impl(cx: &LateContext<'_>, node: HirId) -> Option<DefId> { cx.tcx.trait_impl_of_assoc(cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(node).to_def_id())) }

fn item_path(cx: &LateContext<'_>, item: DefId) -> String {
    let tcx = cx.tcx;
    let owner = |implementation: DefId| tcx.type_of(implementation).instantiate_identity().skip_normalization().opt_def_id();
    match (tcx.inherent_impl_of_assoc(item), tcx.trait_impl_of_assoc(item).and_then(|implementation| owner(implementation).zip(tcx.impl_opt_trait_id(implementation)))) {
        (Some(implementation), _) => format!(
            "{}::{}",
            owner(implementation).map_or_else(|| format!("<{}>", tcx.type_of(implementation).instantiate_identity().skip_normalization()), |owner| tcx.def_path_str(owner)),
            tcx.item_name(item)
        ),
        (None, Some((owner, declared))) if declared.krate == item.krate && owner.krate != item.krate => format!("{}::{}", tcx.def_path_str(declared), tcx.item_name(item)),
        (None, Some((owner, declared))) => format!("<{} as {}>::{}", tcx.def_path_str(owner), tcx.def_path_str(declared), tcx.item_name(item)),
        (None, None) => tcx.def_path_str(item),
    }
}

// --- [BODIES]

fn parameters_match(tcx: TyCtxt<'_>, locals: &mut HirIdMap<HirId>, left_id: BodyId, right_id: BodyId) -> bool {
    let [left_body, right_body] = [left_id, right_id].map(|body| tcx.hir_body(body));
    if left_body.params.len() != right_body.params.len() {
        return false;
    }
    let [left_typeck, right_typeck] = [left_id, right_id].map(|body| tcx.typeck_body(body));
    let [left_eval, right_eval] =
        [(left_id, left_typeck), (right_id, right_typeck)].map(|(body, typeck)| ConstEvalCtxt::with_env(tcx, TypingEnv::non_body_analysis(tcx, tcx.hir_body_owner_def_id(body)), typeck));
    let equal_constant =
        |left: &PatExpr<'_>, right: &PatExpr<'_>| left_eval.eval_pat_expr(left).zip(right_eval.eval_pat_expr(right)).is_some_and(|(left_value, right_value)| left_value == right_value);
    let mut pending = iter::zip(left_body.params, right_body.params).map(|(left, right)| (left.pat, right.pat)).collect::<Vec<_>>();
    while let Some((left, right)) = pending.pop() {
        match (left.kind, right.kind) {
            (PatKind::Wild, PatKind::Wild) | (PatKind::Never, PatKind::Never) => (),
            (PatKind::Binding(left_mode, left_binding, _, left_inner), PatKind::Binding(right_mode, right_binding, _, right_inner))
                if left_mode == right_mode && left_inner.is_some() == right_inner.is_some() =>
            {
                if locals.insert(left_binding, right_binding).is_some_and(|mapped| mapped != right_binding) {
                    return false;
                }
                pending.extend(left_inner.zip(right_inner));
            }
            (PatKind::Tuple(left_items, left_rest), PatKind::Tuple(right_items, right_rest)) | (PatKind::TupleStruct(_, left_items, left_rest), PatKind::TupleStruct(_, right_items, right_rest))
                if left_rest == right_rest
                    && left_items.len() == right_items.len()
                    && match (left.kind, right.kind) {
                        (PatKind::TupleStruct(left_path, ..), PatKind::TupleStruct(right_path, ..)) =>
                            left_typeck.qpath_res(&left_path, left.hir_id) == right_typeck.qpath_res(&right_path, right.hir_id),
                        _ => true,
                    } =>
                pending.extend(iter::zip(left_items, right_items)),
            (PatKind::Struct(left_path, left_fields, _), PatKind::Struct(right_path, right_fields, _))
                if left_fields.len() == right_fields.len()
                    && left_typeck.qpath_res(&left_path, left.hir_id) == right_typeck.qpath_res(&right_path, right.hir_id)
                    && iter::zip(left_fields, right_fields).all(|(left_field, right_field)| left_typeck.field_index(left_field.hir_id) == right_typeck.field_index(right_field.hir_id)) =>
                pending.extend(iter::zip(left_fields, right_fields).map(|(left_field, right_field)| (left_field.pat, right_field.pat))),
            (PatKind::Or(left_items), PatKind::Or(right_items)) if left_items.len() == right_items.len() => pending.extend(iter::zip(left_items, right_items)),
            (PatKind::Deref(left_inner), PatKind::Deref(right_inner)) => pending.push((left_inner, right_inner)),
            (PatKind::Ref(left_inner, left_pinned, left_mutability), PatKind::Ref(right_inner, right_pinned, right_mutability))
                if left_pinned == right_pinned && left_mutability == right_mutability =>
                pending.push((left_inner, right_inner)),
            (PatKind::Expr(left_expr), PatKind::Expr(right_expr)) if equal_constant(left_expr, right_expr) => (),
            (PatKind::Guard(left_inner, left_guard), PatKind::Guard(right_inner, right_guard))
                if left_eval.eval(left_guard).zip(right_eval.eval(right_guard)).is_some_and(|(left_value, right_value)| left_value == right_value) =>
                pending.push((left_inner, right_inner)),
            (PatKind::Range(left_start, left_end, left_inclusive), PatKind::Range(right_start, right_end, right_inclusive))
                if left_inclusive == right_inclusive
                    && [(left_start, right_start), (left_end, right_end)].into_iter().all(|bounds| match bounds {
                        (None, None) => true,
                        (Some(left_bound), Some(right_bound)) => equal_constant(left_bound, right_bound),
                        (Some(_), None) | (None, Some(_)) => false,
                    }) =>
                (),
            (PatKind::Slice(left_start, left_middle, left_end), PatKind::Slice(right_start, right_middle, right_end))
                if left_start.len() == right_start.len() && left_end.len() == right_end.len() && left_middle.is_some() == right_middle.is_some() =>
                pending.extend(iter::zip(left_start, right_start).chain(left_middle.zip(right_middle)).chain(iter::zip(left_end, right_end))),
            _ => return false,
        }
    }
    true
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl LintPass for FormatArgsCollector {
    fn name(&self) -> &'static str { "FormatArgsCollector" }
    fn get_lints(&self) -> LintVec { LintVec::new() }
}

impl EarlyLintPass for FormatArgsCollector {
    fn check_expr(&mut self, _: &EarlyContext<'_>, expr: &AstExpr) {
        if let AstExprKind::FormatArgs(args) = &expr.kind {
            self.format_args.insert(expr.span.with_parent(None), (**args).clone());
        }
    }

    fn check_crate_post(&mut self, _: &EarlyContext<'_>, _: &Crate) { self.storage.set(mem::take(&mut self.format_args)); }
}

macro_rules! lint {
    ($($visibility:vis $lint:ident: $description:literal),+ $(,)?) => {
        lint!(Pass; $($visibility $lint: $description),+);
        pub(crate) struct Pass;
        impl Pass { pub(crate) const fn new(_: ::rustc_middle::ty::TyCtxt<'_>) -> Self { Self } }
    };
    (default $pass:ty; $($visibility:vis $lint:ident: $description:literal),+ $(,)?) => {
        lint!($pass; $($visibility $lint: $description),+);
        impl $pass { pub(crate) fn new(_: ::rustc_middle::ty::TyCtxt<'_>) -> Self { Self::default() } }
    };
    ($pass:ty; $($visibility:vis $lint:ident: $description:literal),+ $(,)?) => {
        $(::rustc_lint::declare_lint! { $visibility $lint, Warn, $description })+
        ::rustc_lint::impl_lint_pass!($pass => [$($lint),+]);
    };
}

macro_rules! lints {
    ($store:ident, $tcx:ident; {$($initialize:tt)*} $($folder:ident::$module:ident $(($($argument:expr),+))?),+ $(,)?) => {
        dylint_linting::paste::paste! {
            struct Pass<$([<$module:camel>]),+> {
                $($module: Option<[<$module:camel>]>),+
            }

            impl<$([<$module:camel>]),+> LintPass for Pass<$([<$module:camel>]),+> {
                fn name(&self) -> &'static str { env!("CARGO_PKG_NAME") }
                fn get_lints(&self) -> LintVec {
                    let mut lints = Vec::new();
                    $(lints.extend($folder::$module::Pass::lint_vec());)+
                    lints
                }
            }

            impl<'tcx, $([<$module:camel>]: LateLintPass<'tcx>),+> LateLintPass<'tcx> for Pass<$([<$module:camel>]),+> {
                ::rustc_lint::late_lint_methods!(lint_methods, [$($module),+]);
            }
        }

        $(let $module = $folder::$module::Pass::lint_vec();
        $store.register_lints(&$module);)+
        $store.register_late_lint_pass(Box::new(move |$tcx| {
            $($initialize)*
            Box::new(Pass {
                $($module: ::rustc_lint::is_lint_pass_required($tcx.skippable_lints(()), &$module)
                    .then(|| $folder::$module::Pass::new($tcx $(, $($argument),+)?))),+
            })
        }));
    };
}

macro_rules! lint_methods {
    ($passes:tt, [$(fn $name:ident($($parameter:ident: $argument:ty),*);)*]) => {
        $(fn $name(&mut self, context: &LateContext<'tcx>, $($parameter: $argument),*) {
            lint_methods!(@call $passes, self, $name, (context, $($parameter),*));
        })*
    };
    (@call [$($pass:ident),+], $owner:ident, $method:ident, $arguments:tt) => {
        $(if let Some(pass) = &mut $owner.$pass { pass.$method $arguments; })+
    };
}

dylint_linting::dylint_library!();

mod concurrency;
mod duplication;
mod errors;
mod expressions;
mod interior_mutability;
mod items;
mod macros;
mod manual;
mod patterns;
mod representation;

dylint_linting::__maybe_mangle! {
    pub extern "Rust" fn register_lints(session: &rustc_session::Session, store: &mut rustc_lint::LintStore) {
        if let Some(folder) = session.io.input.opt_path().and_then(Path::parent) {
            match Utf8Path::from_path(folder) {
                Some(folder) => session.file_depinfo.lock().extend([Symbol::intern(folder.as_str())]),
                None => _ = session.dcx().err(format!("`{}` is not UTF-8, so Cargo cannot track the modules in it", folder.display())),
            }
        }
        let format_args = FormatArgsStorage::default();
        let early = format_args.clone();
        store.register_early_lint_pass(Box::new(move || Box::new(FormatArgsCollector { format_args: FxHashMap::default(), storage: early.clone() })));
        lints! {
            store, tcx;
            {
                let bodies = LazyCell::new(|| Rc::new(duplication::Bodies::default()));
                let reported = (!tcx.skippable_lints(()).contains(&LintId::of(macros::formatting::DISPLAY_FORMAT))).then(|| Rc::new(RefCell::new(LocalDefIdMap::default())));
            }
            concurrency::blocking_call_in_async,
            concurrency::detached_task,
            concurrency::short_circuited_join,
            concurrency::unending_join,
            duplication::duplicate_body(Rc::clone(&bodies)),
            duplication::duplicate_trait_constant(Rc::clone(&bodies)),
            duplication::duplicate_type(Rc::clone(&bodies)),
            errors::canonicalize_without_path,
            errors::debug_main_error,
            errors::decode_without_path,
            errors::discarded_error,
            errors::discarded_error_source,
            errors::error_field_without_sources,
            errors::manual_error_impl,
            errors::partition_by_result,
            errors::redundant_error_conversion,
            errors::source_in_error_message,
            errors::stringified_error(format_args.clone(), reported.clone()),
            errors::untyped_error,
            expressions::absurd_nonzero_comparison,
            expressions::assertions_outside_tests,
            expressions::environment_override,
            expressions::explicit_iter_chain_zip,
            expressions::redundant_clone_in_closure,
            expressions::wrapping_arithmetic,
            interior_mutability::interior_mutable_static,
            interior_mutability::needless_cell,
            items::modules,
            items::needless_import_rename,
            macros::formatting(format_args.clone(), reported.clone()),
            macros::matching(format_args.clone()),
            macros::renamed_function_params_in_macro,
            macros::restriction_in_macro,
            manual::manual_anonymous_impl(Rc::clone(&bodies)),
            manual::manual_bool_toggle,
            manual::manual_default_field_values,
            manual::manual_deref_pattern,
            manual::manual_drop_guard,
            manual::manual_from_utf8_lossy_owned,
            manual::manual_partition,
            manual::manual_path_is_empty,
            manual::manual_retain_back,
            manual::manual_set_operator,
            manual::manual_slice_as_array,
            manual::manual_type_changing_struct_update,
            manual::manual_upcast,
            manual::manual_vec_from_fn,
            patterns::rest_pattern_workspace_field,
            patterns::wildcard_workspace_enum_match_arm,
            representation::formatted_tracing_value(reported.clone()),
            representation::manual_reference_impl,
            representation::manual_serialization,
            representation::manual_variant_array,
            representation::needless_reference_field,
            representation::needless_struct,
            representation::option_field_pairs,
            representation::restated_constant,
            representation::sentinel_default,
            representation::single_impl_trait,
            representation::stringified_uuid(format_args.clone(), reported.clone()),
            representation::stringly_typed_value,
            representation::untyped_json_access,
            representation::variant_constant_functions,
        }
    }
}
