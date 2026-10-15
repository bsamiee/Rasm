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
extern crate rustc_session;
extern crate rustc_span;

use core::{cell::RefCell, iter};

use clippy_utils::{
    SpanlessEq, fn_def_id, get_expr_use_site, get_parent_expr,
    paths::{PathNS, lookup_path_str},
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, is_copy},
};
use rustc_abi::FieldIdx;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{
    BodyId, Expr, ExprKind, HirId, Mutability, Node, Pat, PatKind, StructTailExpr, UnOp,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId},
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::LateContext;
use rustc_middle::ty::{
    self, AdtDef, FieldDef, Interner as _, Ty, TyCtxt, TypeckResults, TypingEnv, Unnormalized, VariantDef,
    adjustment::{Adjust, AutoBorrow, DerefAdjustKind, PointerCoercion},
};
use rustc_span::{Span, Symbol, SyntaxContext};

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

    fn name(self, cx: &LateContext<'_>) -> Symbol {
        match self {
            Self::Field(item) | Self::Static(item) => cx.tcx.item_name(item),
            Self::Local(binding) | Self::Parameter(binding) => cx.tcx.hir_name(binding),
        }
    }

    fn exported(self, cx: &LateContext<'_>) -> bool {
        match self {
            Self::Field(item) | Self::Static(item) => cx.effective_visibilities.is_exported(item),
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

#[derive(Clone, Copy)]
enum Upcast {
    Supertrait(DefId),
    Any,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn resolve<T>(tcx: TyCtxt<'_>, namespace: PathNS, table: &[(&str, T)]) -> FxHashMap<DefId, T>
where T: Copy {
    table.iter().flat_map(|&(path, value)| lookup_path_str(tcx, namespace, path).into_iter().map(move |resolved| (resolved, value))).collect()
}

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
        && (owner.is_box() || [sym::Arc, sym::Rc, sym::Cow].into_iter().any(|name| owner.is_diag_item(cx, name)))
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

fn delivered(tcx: TyCtxt<'_>, item: DefId) -> Ty<'_> {
    let env = TypingEnv::non_body_analysis(tcx, item);
    let output = tcx.instantiate_bound_regions_with_erased(tcx.normalize_erasing_regions(env, tcx.fn_sig(item).instantiate_identity())).output();
    tcx.get_impl_future_output_ty(output).and_then(|future| tcx.try_normalize_erasing_regions(env, Unnormalized::new(future)).ok()).unwrap_or(output)
}

fn boxed_future<'tcx>(cx: &LateContext<'tcx>, output: Ty<'tcx>) -> Option<bool> {
    if let ty::Adt(pin, pinned) = output.kind()
        && pin.is_lang_item(cx, LangItem::Pin)
        && let ty::Adt(boxed, boxed_args) = pinned.type_at(0).kind()
        && boxed.is_box()
        && let ty::Dynamic(predicates, ..) = boxed_args.type_at(0).kind()
        && predicates.principal_def_id().is_some_and(|principal| cx.tcx.is_lang_item(principal, LangItem::Future))
    {
        Some(predicates.auto_traits().any(|auto| cx.tcx.is_diagnostic_item(sym::Send, auto)))
    } else {
        None
    }
}

fn upcast(cx: &LateContext<'_>, method: LocalDefId) -> Option<Upcast> {
    let tcx = cx.tcx;
    let subtrait = tcx.parent(method.to_def_id());
    let ty::Dynamic(bounds, ..) = pointee(cx, tcx.fn_sig(method).instantiate_identity().skip_normalization().skip_binder().output()).kind() else { return None };
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
                let value = peel_blocks(body.value);
                body.params.first().is_some_and(|receiver| value.res_local_id() == Some(receiver.pat.hir_id))
                    && tcx.typeck(owner).expr_adjustments(value).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Pointer(PointerCoercion::Unsize)))
            })
        }))
    .then_some(if inherited { Upcast::Supertrait(supertrait) } else { Upcast::Any })
}

fn field_at(typeck: &TypeckResults<'_>, owner: Ty<'_>, node: HirId) -> Option<DefId> {
    Some(owner.peel_refs().ty_adt_def().filter(|adt| !adt.is_enum())?.non_enum_variant().fields.get(typeck.opt_field_index(node)?)?.did)
}

fn accessed_field(typeck: &TypeckResults<'_>, access: &Expr<'_>) -> Option<DefId> {
    if let ExprKind::Field(base, _) = access.kind { field_at(typeck, typeck.expr_ty_adjusted(base), access.hir_id) } else { None }
}

fn place(cx: &LateContext<'_>, expr: &Expr<'_>) -> Option<Place> {
    if let ExprKind::Field(..) = expr.kind {
        accessed_field(cx.typeck_results(), expr)?.as_local().map(Place::Field)
    } else if let ExprKind::Path(qpath) = &expr.kind {
        let res = cx.qpath_res(qpath, expr.hir_id);
        if let Res::Local(binding) = res {
            Some(if matches!(cx.tcx.parent_hir_node(binding), Node::Param(_)) { Place::Parameter(binding) } else { Place::Local(binding) })
        } else {
            res.opt_def_id().filter(|&item| cx.tcx.is_static(item))?.as_local().map(Place::Static)
        }
    } else {
        None
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
    let source = if let ExprKind::Call(_, [argument]) = call.kind
        && let ExprKind::AddrOf(_, Mutability::Not, source) = argument.kind
    {
        Some(source)
    } else if let ExprKind::MethodCall(_, source, [], _) = call.kind {
        Some(source)
    } else {
        None
    }?;
    fn_def_id(cx, call).and_then(|clone| cx.tcx.trait_of_assoc(clone)).is_some_and(|clone| clone.is_diag_item(cx, sym::Clone)).then_some(source)
}

fn held<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(Place, Ty<'tcx>)> {
    match place(cx, expr)? {
        held @ (Place::Field(item) | Place::Static(item)) => Some((held, cx.tcx.type_of(item).instantiate_identity().skip_normalization())),
        Place::Local(binding) if let Node::LetStmt(local) = cx.tcx.parent_hir_node(binding) => {
            let bound = cx.typeck_results().node_type(binding);
            if let ty::Adt(shared, args) = bound.kind()
                && shared.is_diag_item(cx, sym::Arc)
            {
                local.init.and_then(|init| cloned(cx, init)).map_or_else(
                    || {
                        local
                            .init
                            .and_then(|init| fn_def_id(cx, init))
                            .filter(|&built| {
                                cx.tcx.item_name(built) == sym::new
                                    && cx
                                        .tcx
                                        .inherent_impl_of_assoc(built)
                                        .is_some_and(|implementation| cx.tcx.type_of(implementation).instantiate_identity().skip_normalization().is_diag_item(cx, sym::Arc))
                            })
                            .map(|_| (Place::Local(binding), args.type_at(0)))
                    },
                    |source| held(cx, source),
                )
            } else {
                Some((Place::Local(binding), bound))
            }
        }
        Place::Local(_) | Place::Parameter(_) => None,
    }
}

fn rebound(cx: &LateContext<'_>, expr: &Expr<'_>) -> bool {
    get_parent_expr(cx, expr).and_then(|parent| if let ExprKind::AddrOf(..) = parent.kind { get_parent_expr(cx, parent) } else { Some(parent) }).is_some_and(|call| {
        cloned(cx, call).is_some_and(|source| source.hir_id == expr.hir_id)
            && matches!(cx.tcx.parent_hir_node(call.hir_id), Node::LetStmt(local) if matches!(local.pat.kind, PatKind::Binding(_, _, _, None)))
    })
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

fn constructed<'tcx>(typeck: &TypeckResults<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(&'tcx VariantDef, Vec<(&'tcx FieldDef, &'tcx Expr<'tcx>)>)> {
    if let ExprKind::Struct(qpath, fields, _) = expr.kind {
        let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(typeck.qpath_res(qpath, expr.hir_id));
        Some((variant, fields.iter().filter_map(|field| Some((variant.fields.get(typeck.opt_field_index(field.hir_id)?)?, field.expr))).collect()))
    } else if let ExprKind::Call(callee, arguments) = expr.kind
        && let ExprKind::Path(qpath) = &callee.kind
        && let ctor @ (Res::Def(DefKind::Ctor(..), _) | Res::SelfCtor(_)) = typeck.qpath_res(qpath, callee.hir_id)
    {
        let variant = typeck.expr_ty(expr).ty_adt_def()?.variant_of_res(ctor);
        Some((variant, iter::zip(&variant.fields, arguments).collect()))
    } else if let ExprKind::Path(qpath) = &expr.kind
        && let ctor @ (Res::Def(DefKind::Ctor(_, CtorKind::Const), _) | Res::SelfCtor(_)) = typeck.qpath_res(qpath, expr.hir_id)
        && let ty::Adt(adt, _) = typeck.expr_ty(expr).kind()
    {
        Some((adt.variant_of_res(ctor), Vec::new()))
    } else {
        None
    }
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

fn datum(typeck: &TypeckResults<'_>, expr: &Expr<'_>) -> bool {
    matches!(expr.kind, ExprKind::Lit(_))
        || matches!(expr.kind, ExprKind::Unary(UnOp::Neg, operand) if matches!(operand.kind, ExprKind::Lit(_)))
        || matches!(expr.kind, ExprKind::Path(qpath) if matches!(typeck.qpath_res(&qpath, expr.hir_id), Res::Def(DefKind::Const | DefKind::AssocConst | DefKind::Ctor(_, CtorKind::Const), _)))
}

fn constant(typeck: &TypeckResults<'_>, expr: &Expr<'_>) -> bool {
    let value = peel_blocks(expr);
    datum(typeck, value)
        || matches!(value.kind, ExprKind::Tup(items) | ExprKind::Array(items) if items.iter().all(|item| constant(typeck, item)))
        || matches!(value.kind, ExprKind::Call(callee, arguments)
            if matches!(callee.kind, ExprKind::Path(qpath) if matches!(typeck.qpath_res(&qpath, callee.hir_id), Res::Def(DefKind::Ctor(..), _)))
                && arguments.iter().all(|argument| constant(typeck, argument)))
        || matches!(value.kind, ExprKind::Struct(_, fields, StructTailExpr::None) if fields.iter().all(|field| constant(typeck, field.expr)))
        || matches!(value.kind, ExprKind::AddrOf(_, _, operand) if constant(typeck, operand))
}

fn equal_bodies<'tcx, F>(cx: &LateContext<'tcx>, left: LocalDefId, right: LocalDefId, mut differ: F) -> Option<Vec<(HirId, HirId)>>
where F: FnMut(&TypeckResults<'tcx>, &Expr<'_>, &TypeckResults<'tcx>, &Expr<'_>) -> bool {
    let closures = RefCell::new(Vec::<(BodyId, BodyId)>::new());
    let differences = RefCell::new(Vec::new());
    let bindings = |body: BodyId| {
        cx.tcx.hir_body(body).params.iter().fold(Vec::new(), |mut bound, parameter| {
            parameter.pat.each_binding(|_, binding, _, _| bound.push(binding));
            bound
        })
    };
    let equal = {
        let mut spanless = SpanlessEq::new(cx).expr_fallback(|left_typeck, left_expr, right_typeck, right_expr| {
            if let (ExprKind::Closure(left_closure), ExprKind::Closure(right_closure)) = (left_expr.kind, right_expr.kind) {
                closures.borrow_mut().push((left_closure.body, right_closure.body));
                left_closure.capture_clause == right_closure.capture_clause && left_closure.fn_decl.inputs.len() == right_closure.fn_decl.inputs.len()
            } else if differ(left_typeck, left_expr, right_typeck, right_expr) {
                differences.borrow_mut().push((left_expr.hir_id, right_expr.hir_id));
                true
            } else {
                false
            }
        });
        let mut inter = spanless.inter_expr(SyntaxContext::root());
        let mut compare = |left_body: BodyId, right_body: BodyId| {
            let (left_bindings, right_bindings) = (bindings(left_body), bindings(right_body));
            left_bindings.len() == right_bindings.len() && {
                inter.locals.extend(iter::zip(left_bindings, right_bindings));
                inter.eq_body(left_body, right_body)
            }
        };
        compare(cx.tcx.hir_body_owned_by(left).id(), cx.tcx.hir_body_owned_by(right).id())
            && iter::from_fn(|| closures.borrow_mut().pop()).all(|(left_body, right_body)| compare(left_body, right_body))
    };
    equal.then(|| differences.into_inner())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! lints {
    ($($module:ident::$pass:ident $(::$new:ident)?),+ $(,)?) => {
        pub(crate) fn register(store: &mut rustc_lint::LintStore) {
            $(
                store.register_lints(&$module::$pass::lint_vec());
                store.register_late_lint_pass(lints!(@pass $module::$pass $(::$new)?));
            )+
        }
    };
    (@pass $module:ident::$pass:ident) => {
        Box::new(|_| Box::new(<$module::$pass>::default()))
    };
    (@pass $module:ident::$pass:ident::$new:ident) => {
        Box::new(|tcx| Box::new($module::$pass::$new(tcx)))
    };
}

dylint_linting::dylint_library!();

dylint_linting::__maybe_mangle! {
    pub extern "Rust" fn register_lints(_: &rustc_session::Session, store: &mut rustc_lint::LintStore) {
        register(store);
        errors::register(store);
    }
}

mod assertion;
mod async_mutex_without_await;
mod blocking_call_in_async;
mod boxed_future_return;
mod captured_clone;
mod construction_invariant;
mod debug_main_error;
mod detached_task;
mod duplicate_constant;
mod errors;
mod formatted_event;
mod interior_mutable_static;
mod manual_arc_swap;
mod manual_cancellation_token;
mod manual_language_feature;
mod manual_std_method;
mod manual_upcast;
mod needless_import_rename;
mod nested_task;
mod redundant_indirection;
mod redundant_reference;
mod repeated_body;
mod restated_type;
mod sequential_join;
mod single_caller;
mod single_use_wrapper;
mod split_table;
mod string_tag;
mod unshared_cell;
mod untyped_document;

lints! {
    assertion::Assertion::new,
    async_mutex_without_await::AsyncMutexWithoutAwait::new,
    blocking_call_in_async::BlockingCallInAsync::new,
    boxed_future_return::BoxedFutureReturn,
    captured_clone::CapturedClone,
    construction_invariant::ConstructionInvariant,
    debug_main_error::DebugMainError,
    detached_task::DetachedTask::new,
    duplicate_constant::DuplicateConstant,
    formatted_event::FormattedEvent::new,
    interior_mutable_static::InteriorMutableStatic::new,
    manual_arc_swap::ManualArcSwap::new,
    manual_cancellation_token::ManualCancellationToken::new,
    manual_language_feature::ManualLanguageFeature::new,
    manual_std_method::ManualStdMethod::new,
    manual_upcast::ManualUpcast,
    needless_import_rename::NeedlessImportRename::new,
    nested_task::NestedTask,
    redundant_indirection::RedundantIndirection,
    redundant_reference::RedundantReference,
    repeated_body::RepeatedBody,
    restated_type::RestatedType,
    sequential_join::SequentialJoin::new,
    single_caller::SingleCaller,
    single_use_wrapper::SingleUseWrapper::new,
    split_table::SplitTable,
    string_tag::StringTag,
    unshared_cell::UnsharedCell,
    untyped_document::UntypedDocument::new,
}
