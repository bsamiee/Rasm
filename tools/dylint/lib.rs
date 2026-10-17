#![feature(deref_patterns, iterator_try_collect, rustc_private)]

extern crate rustc_abi;
extern crate rustc_ast;
extern crate rustc_attr_ir;
extern crate rustc_data_structures;
extern crate rustc_errors;
extern crate rustc_hir;
extern crate rustc_lexer;
extern crate rustc_lint;
extern crate rustc_middle;
extern crate rustc_parse_format;
extern crate rustc_session;
extern crate rustc_span;

use core::{iter, ops::ControlFlow};

use clippy_utils::{
    fn_def_id, get_expr_use_site, get_parent_expr, is_from_proc_macro,
    macros::macro_backtrace,
    paths::{PathNS, lookup_path_str},
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, is_copy},
    visitors::{Visitable, for_each_expr},
};
use rustc_abi::FieldIdx;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_errors::{Applicability, Diag, DiagMessage};
use rustc_hir::{
    Body, BodyId, Expr, ExprKind, HirId, Mutability, Node, Pat, PatKind, UnOp,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId},
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::LateContext;
use rustc_middle::ty::{
    self, AdtDef, Clause, FieldDef, FnSig, Interner as _, PolyTraitClause, Ty, TyCtxt, TypeckResults, TypingEnv, Unnormalized, VariantDef,
    adjustment::{Adjust, AutoBorrow, DerefAdjustKind, PointerCoercion},
};
use rustc_span::{ExpnKind, FileName, MacroKind, RemapPathScopeComponents, Span, Symbol};

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

    fn exported(self, cx: &LateContext<'_>) -> bool {
        match self {
            Self::Field(item) | Self::Static(item) => exported(cx, item),
            Self::Local(_) | Self::Parameter(_) => false,
        }
    }
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

#[derive(Clone, Copy)]
enum Upcast {
    Supertrait(DefId),
    Any,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [ORIGINS]
fn workspace_def(cx: &LateContext<'_>, item: DefId) -> bool {
    let source_map = cx.tcx.sess.source_map();
    item.is_local()
        || matches!(source_map.span_to_filename(cx.tcx.def_span(item)), FileName::Real(file)
            if source_map.working_dir().local_path().is_some_and(|root| file.embeddable_name(RemapPathScopeComponents::DIAGNOSTICS).1.starts_with(root)))
}

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

// --- [PATHS]
fn resolve<T>(tcx: TyCtxt<'_>, namespace: PathNS, table: &[(&str, T)]) -> FxHashMap<DefId, T>
where T: Copy {
    table.iter().flat_map(|&(path, value)| lookup_path_str(tcx, namespace, path).into_iter().map(move |resolved| (resolved, value))).collect()
}

fn paths(tcx: TyCtxt<'_>, namespace: PathNS, listed: &[&str]) -> FxHashSet<DefId> { listed.iter().flat_map(|listed| lookup_path_str(tcx, namespace, listed)).collect() }

fn path(tcx: TyCtxt<'_>, namespace: PathNS, named: &str) -> Option<DefId> { lookup_path_str(tcx, namespace, named).into_iter().next() }

fn impl_owner(tcx: TyCtxt<'_>, item: DefId) -> Option<Ty<'_>> { tcx.impl_of_assoc(item).map(|implementation| tcx.type_of(implementation).instantiate_identity().skip_normalization()) }

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
    tcx.get_impl_future_output_ty(output).and_then(|future| tcx.try_normalize_erasing_regions(TypingEnv::non_body_analysis(tcx, item), Unnormalized::new(future)).ok()).unwrap_or(output)
}

fn bounds_of<'tcx>(tcx: TyCtxt<'tcx>, item: DefId, param: Ty<'tcx>) -> impl Iterator<Item = PolyTraitClause<'tcx>> {
    tcx.param_env(item).caller_bounds().filter_map(Clause::as_trait_clause).filter(move |bound| bound.self_ty().skip_binder() == param)
}

fn upcast(cx: &LateContext<'_>, method: LocalDefId) -> Option<Upcast> {
    let tcx = cx.tcx;
    let subtrait = tcx.parent(method.to_def_id());
    let ty::Dynamic(bounds, ..) = pointee(cx, signature(tcx, method.to_def_id()).output()).kind() else { return None };
    let supertrait = bounds.principal_def_id()?;
    let inherited = tcx.supertrait_def_ids(subtrait).any(|ancestor| ancestor == supertrait);
    let bodies = tcx
        .local_trait_impls(subtrait)
        .iter()
        .filter_map(|implementation| tcx.impl_item_implementor_ids(implementation.to_def_id()).get(&method.to_def_id())?.as_local())
        .chain(tcx.defaultness(method).has_value().then_some(method))
        .collect::<Vec<_>>();
    (supertrait != subtrait
        && (inherited || tcx.is_diagnostic_item(sym::Any, supertrait))
        && !bodies.is_empty()
        && bodies.iter().all(|&owner| {
            tcx.hir_maybe_body_owned_by(owner).is_some_and(|body| {
                let value = peel_blocks(&body.value);
                parameter(body, value) == Some(0) && tcx.typeck(owner).expr_adjustments(value).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Pointer(PointerCoercion::Unsize)))
            })
        }))
    .then_some(if inherited { Upcast::Supertrait(supertrait) } else { Upcast::Any })
}

// --- [PLACES]
fn field_at(typeck: &TypeckResults<'_>, owner: Ty<'_>, node: HirId) -> Option<DefId> {
    Some(owner.peel_refs().ty_adt_def().filter(|adt| !adt.is_enum())?.non_enum_variant().fields.get(typeck.opt_field_index(node)?)?.did)
}

fn accessed_field(typeck: &TypeckResults<'_>, access: &Expr<'_>) -> Option<DefId> {
    if let ExprKind::Field(base, _) = access.kind { field_at(typeck, typeck.expr_ty_adjusted(base), access.hir_id) } else { None }
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
    let site = get_expr_use_site(cx.tcx, typeck, expr.span.ctxt(), expr);
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
        (Some(Mutability::Mut), _) => Access::Mutated,
        (Some(Mutability::Not), _) => Access::Borrowed,
        (None, Some((_, ExprKind::Assign(target, value, _)))) if target.hir_id == site.child_id => Access::Assigned(value),
        (None, Some((_, ExprKind::AssignOp(_, target, _)))) if target.hir_id == site.child_id => Access::Mutated,
        (None, Some((projection, ExprKind::Field(base, _) | ExprKind::Unary(UnOp::Deref, base) | ExprKind::Index(base, ..)))) if base.hir_id == site.child_id => match access(cx, projection) {
            Access::Assigned(_) | Access::Mutated => Access::Mutated,
            Access::Copied | Access::Borrowed => Access::Borrowed,
            Access::Moved => Access::Moved,
        },
        (None, _) if is_copy(cx, typeck.expr_ty(expr)) => Access::Copied,
        (None, _) => Access::Moved,
    }
}

fn cloned<'tcx>(cx: &LateContext<'tcx>, call: &'tcx Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    let (ExprKind::Call(_, &[Expr { kind: ExprKind::AddrOf(_, Mutability::Not, source), .. }]) | ExprKind::MethodCall(_, source, [], _)) = call.kind else { return None };
    fn_def_id(cx, call).opt_parent(cx).is_diag_item(cx, sym::Clone).then_some(source)
}

fn pattern_fields<'tcx>(typeck: &TypeckResults<'tcx>, pat: &'tcx Pat<'tcx>) -> impl Iterator<Item = (DefId, &'tcx Pat<'tcx>)> {
    let variant = || typeck.pat_ty(pat).ty_adt_def().filter(|adt| adt.is_struct()).map(AdtDef::non_enum_variant);
    let named = if let PatKind::Struct(_, fields, _) = pat.kind {
        variant().map(|variant| fields.iter().filter_map(move |field| Some((variant.fields.get(typeck.opt_field_index(field.hir_id)?)?.did, field.pat))))
    } else {
        None
    };
    let positional = if let PatKind::TupleStruct(_, fields, rest) = pat.kind {
        variant().map(|variant| fields.iter().enumerate_and_adjust(variant.fields.len(), rest).filter_map(move |(index, field)| Some((variant.fields.get(FieldIdx::from_usize(index))?.did, field))))
    } else {
        None
    };
    named.into_iter().flatten().chain(positional.into_iter().flatten())
}

// --- [EXPRESSIONS]
fn constructed<'tcx>(typeck: &TypeckResults<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(&'tcx VariantDef, Vec<(&'tcx FieldDef, &'tcx Expr<'tcx>)>)> {
    match expr.kind {
        ExprKind::Struct(qpath, fields, _) => {
            let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(typeck.qpath_res(qpath, expr.hir_id));
            Some((variant, fields.iter().filter_map(|field| Some((variant.fields.get(typeck.opt_field_index(field.hir_id)?)?, field.expr))).collect()))
        }
        ExprKind::Call(&Expr { kind: ExprKind::Path(qpath), hir_id, .. }, arguments) if let ctor @ (Res::Def(DefKind::Ctor(..), _) | Res::SelfCtor(_)) = typeck.qpath_res(&qpath, hir_id) => {
            let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(ctor);
            Some((variant, iter::zip(&variant.fields, arguments).collect()))
        }
        ExprKind::Path(qpath)
            if let ctor @ (Res::Def(DefKind::Ctor(_, CtorKind::Const), _) | Res::SelfCtor(_)) = typeck.qpath_res(&qpath, expr.hir_id)
                && let ty::Adt(adt, _) = typeck.expr_ty(expr).kind() =>
            Some((adt.variant_of_res(ctor), Vec::new())),
        _ => None,
    }
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

fn method_call_on<'tcx>(cx: &LateContext<'tcx>, receiver: &Expr<'_>) -> Option<(&'tcx Expr<'tcx>, DefId)> {
    get_parent_expr(cx, receiver)
        .filter(|call| matches!(call.kind, ExprKind::MethodCall(_, called, ..) if called.hir_id == receiver.hir_id))
        .and_then(|call| Some((call, cx.typeck_results().type_dependent_def_id(call.hir_id)?)))
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

fn trait_impls(tcx: TyCtxt<'_>) -> FxIndexMap<LocalDefId, Vec<(DefId, LocalDefId)>> {
    tcx.all_local_trait_impls(())
        .iter()
        .flat_map(|(&implemented, implementations)| implementations.iter().map(move |&implementation| (implemented, implementation)))
        .filter_map(|implementation @ (_, local)| Some((tcx.type_of(local).instantiate_identity().skip_normalization().peel_refs().ty_adt_def()?.did().as_local()?, implementation)))
        .fold(FxIndexMap::default(), |mut owners, (owner, implementation)| {
            owners.entry(owner).or_default().push(implementation);
            owners
        })
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
fn parameter_bindings(tcx: TyCtxt<'_>, body: BodyId) -> Vec<HirId> {
    tcx.hir_body(body).params.iter().fold(Vec::new(), |mut bound, parameter| {
        parameter.pat.each_binding(|_, binding, _, _| bound.push(binding));
        bound
    })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! lint {
    ($($lint:ident: $description:literal),+ $(,)?) => {
        lint!(Pass; $($lint: $description),+);
        pub(crate) struct Pass;
        impl Pass { pub(crate) const fn new(_: ::rustc_middle::ty::TyCtxt<'_>) -> Self { Self } }
    };
    (default $pass:ty; $($lint:ident: $description:literal),+ $(,)?) => {
        lint!($pass; $($lint: $description),+);
        impl $pass { pub(crate) fn new(_: ::rustc_middle::ty::TyCtxt<'_>) -> Self { Self::default() } }
    };
    ($pass:ty; $($lint:ident: $description:literal),+ $(,)?) => {
        $(::rustc_lint::declare_lint! { $lint, Warn, $description })+
        ::rustc_lint::impl_lint_pass!($pass => [$($lint),+]);
    };
}

macro_rules! lints {
    ($($folder:ident::$module:ident),+ $(,)?) => {
        dylint_linting::__maybe_mangle! {
            pub extern "Rust" fn register_lints(_: &rustc_session::Session, store: &mut rustc_lint::LintStore) {
                $(
                    store.register_lints(&$folder::$module::Pass::lint_vec());
                    store.register_late_lint_pass(Box::new(|tcx| Box::new($folder::$module::Pass::new(tcx))));
                )+
            }
        }
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

lints! {
    concurrency::async_lock_without_await,
    concurrency::blocking_call_in_async,
    concurrency::buffered_join,
    concurrency::detached_task,
    concurrency::manual_abort_on_drop,
    concurrency::manual_arc_swap,
    concurrency::manual_cancellation_token,
    concurrency::manual_stream,
    concurrency::needless_boxed_future,
    concurrency::optional_channel_sender,
    concurrency::redundant_spawn,
    concurrency::sequential_await,
    concurrency::short_circuited_join,
    concurrency::unbounded_pending_map,
    duplication::duplicate_body,
    duplication::duplicate_trait_constant,
    duplication::duplicate_type,
    errors::debug_main_error,
    errors::discarded_error,
    errors::discarded_error_source,
    errors::foreign_error_return,
    errors::independent_question_marks,
    errors::io_error_without_context,
    errors::manual_error_impl,
    errors::partition_by_result,
    errors::redundant_error_conversion,
    errors::source_in_error_message,
    errors::stringified_error,
    errors::untyped_error,
    expressions::absurd_nonzero_comparison,
    expressions::assertions_outside_tests,
    expressions::cloned_command_matches,
    expressions::environment_override,
    expressions::map_indexing,
    expressions::redundant_clone_in_closure,
    expressions::redundant_default_base,
    expressions::wrapping_arithmetic,
    interior_mutability::interior_mutable_static,
    interior_mutability::needless_cell,
    items::needless_import_rename,
    items::single_caller,
    macros::redundant_closure_call_in_macro,
    macros::renamed_function_params_in_macro,
    macros::restriction_in_macro,
    manual::manual_anonymous_impl,
    manual::manual_bool_toggle,
    manual::manual_default_field_values,
    manual::manual_deref_pattern,
    manual::manual_drop_guard,
    manual::manual_from_utf8_lossy_owned,
    manual::manual_partition,
    manual::manual_path_is_empty,
    manual::manual_retain_back,
    manual::manual_slice_as_array,
    manual::manual_type_changing_struct_update,
    manual::manual_upcast,
    manual::manual_vec_from_fn,
    patterns::rest_pattern_workspace_field,
    patterns::wildcard_workspace_enum_match_arm,
    representation::fn_pointer_iterator_type,
    representation::formatted_tracing_value,
    representation::forwarding_newtype,
    representation::manual_serialization,
    representation::manual_slot_map,
    representation::manual_variant_collection,
    representation::needless_reference_field,
    representation::needless_struct,
    representation::option_field_pairs,
    representation::primitive_domain_binding,
    representation::sentinel_default,
    representation::single_impl_trait,
    representation::stringified_uuid,
    representation::stringly_typed_value,
    representation::unchecked_construction,
    representation::unparsed_field,
    representation::untyped_json_access,
    representation::variant_constant_functions,
}
