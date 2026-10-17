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

use core::{cell::RefCell, iter, ops::ControlFlow};

use clippy_utils::{
    SpanlessEq, fn_def_id, get_expr_use_site, get_parent_expr,
    macros::macro_backtrace,
    paths::{PathNS, lookup_path_str},
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _, MaybeTypeckRes as _},
    sym,
    ty::{implements_trait, is_copy},
    visitors::{Visitable, for_each_expr},
};
use rustc_abi::FieldIdx;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{
    Body, BodyId, CoroutineDesugaring, CoroutineKind, Expr, ExprKind, HirId, Mutability, Node, Pat, PatKind, StructTailExpr, UnOp,
    def::{CtorKind, DefKind, Res},
    def_id::{DefId, LocalDefId},
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::LateContext;
use rustc_middle::ty::{
    self, AdtDef, Binder, BottomUpFolder, Clause, FieldDef, FnSig, Interner as _, PolyTraitClause, Ty, TyCtxt, TypeFoldable, TypeckResults, TypingEnv, Unnormalized, VariantDef,
    adjustment::{Adjust, AutoBorrow, DerefAdjustKind, PointerCoercion},
};
use rustc_span::{MacroKind, Span, Symbol, SyntaxContext};

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

struct Ledger<U> {
    places: FxIndexMap<Place, Option<U>>,
}

impl<U> Default for Ledger<U> {
    fn default() -> Self { Self { places: FxIndexMap::default() } }
}

impl<U> Ledger<U> {
    fn record<F>(&mut self, place: Place, usage: F) -> Option<&mut U>
    where F: FnOnce() -> U {
        self.places.entry(place).or_insert_with(|| Some(usage())).as_mut()
    }

    fn escape(&mut self, place: Place) { self.places.insert(place, None); }

    fn escape_bound<'tcx, F>(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>, kept: F)
    where F: Fn(Ty<'tcx>) -> bool {
        let bound = pattern_fields(cx.typeck_results(), pat)
            .filter(|(_, bound)| !matches!(bound.kind, PatKind::Wild))
            .filter_map(|(field, _)| field.as_local())
            .filter(|&field| kept(cx.tcx.type_of(field).instantiate_identity().skip_normalization()));
        for field in bound {
            self.escape(Place::Field(field));
        }
    }

    fn held<'ledger>(&'ledger self, cx: &LateContext<'_>) -> impl Iterator<Item = (Place, &'ledger U)> {
        self.places.iter().filter_map(|(&place, usage)| Some((place, usage.as_ref()?))).filter(|&(place, _)| !place.exported(cx) && !place.span(cx).from_expansion())
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct Polling {
    offloads: FxHashSet<DefId>,
}

impl Polling {
    fn new(tcx: TyCtxt<'_>) -> Self { Self { offloads: paths(tcx, PathNS::Value, &["std::thread::Scope::spawn"]) } }

    fn polled(&self, cx: &LateContext<'_>, body: LocalDefId) -> bool {
        let offloaded = |closure: &Expr<'_>| {
            get_parent_expr(cx, closure).and_then(|call| parameter_of(cx, call, closure)).is_some_and(|(callee, input)| {
                self.offloads.contains(&callee)
                    || cx
                        .tcx
                        .clauses_of(callee)
                        .clauses
                        .iter()
                        .filter_map(|(clause, _)| clause.as_type_outlives_clause())
                        .map(Binder::skip_binder)
                        .any(|outlives| outlives.0 == input && outlives.1.is_static())
            })
        };
        match cx.tcx.coroutine_kind(body) {
            Some(CoroutineKind::Desugared(CoroutineDesugaring::Async | CoroutineDesugaring::AsyncGen, _)) => true,
            Some(_) => false,
            None => matches!(cx.tcx.hir_node_by_def_id(body), Node::Expr(closure) if !offloaded(closure) && self.polled(cx, cx.tcx.hir_enclosing_body_owner(closure.hir_id))),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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

fn derives<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>, derived: Symbol) -> bool {
    cx.tcx.get_diagnostic_item(derived).is_some_and(|implemented| cx.tcx.non_blanket_impls_for_ty(implemented, ty).any(|implementation| cx.tcx.is_automatically_derived(implementation)))
}

fn signature(tcx: TyCtxt<'_>, item: DefId) -> FnSig<'_> {
    tcx.instantiate_bound_regions_with_erased(tcx.normalize_erasing_regions(TypingEnv::non_body_analysis(tcx, item), tcx.fn_sig(item).instantiate_identity()))
}

fn delivered(tcx: TyCtxt<'_>, item: DefId) -> Ty<'_> {
    let output = signature(tcx, item).output();
    tcx.get_impl_future_output_ty(output).and_then(|future| tcx.try_normalize_erasing_regions(TypingEnv::non_body_analysis(tcx, item), Unnormalized::new(future)).ok()).unwrap_or(output)
}

fn bounds_of<'tcx>(tcx: TyCtxt<'tcx>, item: DefId, param: Ty<'tcx>) -> impl Iterator<Item = PolyTraitClause<'tcx>> {
    tcx.param_env(item).caller_bounds().filter_map(Clause::as_trait_clause).filter(move |bound| bound.self_ty().skip_binder() == param)
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
                let value = peel_blocks(body.value);
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
                            .filter(|&built| cx.tcx.item_name(built) == sym::new && impl_owner(cx.tcx, built).is_some_and(|owner| owner.is_diag_item(cx, sym::Arc)))
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

fn any_expr<'tcx, N, F>(tcx: TyCtxt<'tcx>, node: N, mut found: F) -> bool
where
    N: Visitable<'tcx>,
    F: FnMut(&'tcx Expr<'tcx>) -> bool,
{
    for_each_expr(tcx, node, |expr| if found(expr) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }).is_some()
}

fn receiving_call<'tcx>(cx: &LateContext<'tcx>, receiver: &Expr<'_>) -> Option<(&'tcx Expr<'tcx>, DefId)> {
    get_parent_expr(cx, receiver)
        .filter(|call| matches!(call.kind, ExprKind::MethodCall(_, called, ..) if called.hir_id == receiver.hir_id))
        .and_then(|call| Some((call, cx.typeck_results().type_dependent_def_id(call.hir_id)?)))
}

fn iterator_method(cx: &LateContext<'_>, call: &Expr<'_>) -> bool {
    cx.ty_based_def(call).opt_def_id().and_then(|method| cx.tcx.trait_of_assoc(method)).is_some_and(|owner| owner.is_diag_item(cx, sym::Iterator) || owner.is_diag_item(cx, sym::DoubleEndedIterator))
}

fn fn_value(cx: &LateContext<'_>, expr: &Expr<'_>) -> Option<DefId> {
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
        .any(|owner| macro_backtrace(cx.tcx.def_span(owner)).any(|call| matches!(call.kind, MacroKind::Attr | MacroKind::Derive)))
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

fn equal_bodies<'tcx, F>(cx: &LateContext<'tcx>, left: LocalDefId, right: LocalDefId, mut differ: F) -> Option<Vec<(HirId, HirId)>>
where F: FnMut(&TypeckResults<'tcx>, &Expr<'_>, &TypeckResults<'tcx>, &Expr<'_>) -> bool {
    let closures = RefCell::new(Vec::<(BodyId, BodyId)>::new());
    let differences = RefCell::new(Vec::new());
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
            let (left_bindings, right_bindings) = (parameter_bindings(cx.tcx, left_body), parameter_bindings(cx.tcx, right_body));
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

fn replaced<'tcx, T, F>(tcx: TyCtxt<'tcx>, value: T, substitute: F) -> T
where
    T: TypeFoldable<TyCtxt<'tcx>>,
    F: Fn(Ty<'tcx>) -> Option<Ty<'tcx>>,
{
    value.fold_with(&mut BottomUpFolder { tcx, ty_op: |ty| substitute(ty).unwrap_or(ty), lt_op: |region| region, ct_op: |constant| constant })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

macro_rules! lints {
    ($($head:ident $(::$segment:ident)+),+ $(,)?) => {
        fn register(store: &mut rustc_lint::LintStore) {
            $(lints!(@row store [$head] $($segment)+);)+
        }
    };
    (@row $store:ident [$($pass:ident)::+] new) => {
        $store.register_lints(&$($pass)::+::lint_vec());
        $store.register_late_lint_pass(Box::new(|tcx| Box::new($($pass)::+::new(tcx))));
    };
    (@row $store:ident [$($pass:ident)::+] $last:ident) => {
        $store.register_lints(&$($pass)::+::$last::lint_vec());
        $store.register_late_lint_pass(Box::new(|_| Box::new(<$($pass)::+::$last>::default())));
    };
    (@row $store:ident [$($module:ident)::+] $next:ident $($segment:ident)+) => {
        lints!(@row $store [$($module)::+::$next] $($segment)+)
    };
}

dylint_linting::dylint_library!();

dylint_linting::__maybe_mangle! {
    pub extern "Rust" fn register_lints(_: &rustc_session::Session, store: &mut rustc_lint::LintStore) {
        register(store);
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
mod environment_override;
mod errors;
mod formatted_event;
mod interior_mutable_static;
mod macro_closure_call;
mod manual_arc_swap;
mod manual_cancellation_token;
mod manual_language_feature;
mod manual_partition;
mod manual_serialization;
mod manual_std_method;
mod manual_stream;
mod manual_upcast;
mod needless_import_rename;
mod nested_task;
mod optional_sender;
mod owned_catch_all;
mod plain_record;
mod redundant_indirection;
mod redundant_reference;
mod renamed_macro_params;
mod repeated_body;
mod restated_type;
mod sequential_join;
mod single_caller;
mod single_use_wrapper;
mod split_table;
mod string_tag;
mod unbounded_pending_map;
mod unparsed_field;
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
    environment_override::EnvironmentOverride::new,
    errors::contextless_io_error::ContextlessIoError::new,
    errors::foreign_error_return::ForeignErrorReturn,
    errors::manual_error_impl::ManualErrorImpl,
    errors::redundant_error_conversion::RedundantErrorConversion::new,
    errors::rendered_error::RenderedError,
    errors::split_outcome::SplitOutcome,
    errors::untyped_error::UntypedError,
    formatted_event::FormattedEvent::new,
    interior_mutable_static::InteriorMutableStatic::new,
    macro_closure_call::MacroClosureCall,
    manual_arc_swap::ManualArcSwap::new,
    manual_cancellation_token::ManualCancellationToken::new,
    manual_language_feature::ManualLanguageFeature::new,
    manual_partition::ManualPartition,
    manual_serialization::ManualSerialization::new,
    manual_std_method::ManualStdMethod::new,
    manual_stream::ManualStream::new,
    manual_upcast::ManualUpcast,
    needless_import_rename::NeedlessImportRename::new,
    nested_task::NestedTask,
    optional_sender::OptionalSender::new,
    owned_catch_all::OwnedCatchAll,
    plain_record::PlainRecord,
    redundant_indirection::RedundantIndirection,
    redundant_reference::RedundantReference,
    renamed_macro_params::RenamedMacroParams,
    repeated_body::RepeatedBody,
    restated_type::RestatedType,
    sequential_join::SequentialJoin::new,
    single_caller::SingleCaller,
    single_use_wrapper::SingleUseWrapper::new,
    split_table::SplitTable,
    string_tag::StringTag::new,
    unbounded_pending_map::UnboundedPendingMap::new,
    unparsed_field::UnparsedField::new,
    unshared_cell::UnsharedCell,
    untyped_document::UntypedDocument::new,
}
