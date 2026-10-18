use clippy_utils::{fn_def_id, get_parent_expr, paths::PathNS, res::MaybeDef as _, sym};
use derive_where::derive_where;
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    CoroutineDesugaring, CoroutineKind, Expr, ExprKind, HirId, Node, Pat, PatKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, Binder, Ty, TyCtxt};

use crate::{Place, cloned, enclosing_trait_impl, expanded_externally, impl_owner, parameter_of, path, pattern_fields, place};

// --- [MODELS] --------------------------------------------------------------------------

#[derive_where(Default)]
struct PlaceUses<U> {
    places: FxIndexMap<Place, Option<U>>,
}

impl<U> PlaceUses<U> {
    fn record<F>(&mut self, place: Place, usage: F) -> Option<&mut U>
    where F: FnOnce() -> U {
        self.places.entry(place).or_insert_with(|| Some(usage())).as_mut()
    }

    fn escape(&mut self, place: Place) { self.places.insert(place, None); }

    fn escape_bound<'tcx, F>(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>, kept: F)
    where F: Fn(Ty<'tcx>) -> bool {
        self.places.extend(
            pattern_fields(cx.typeck_results(), pat)
                .filter(|(_, bound)| !matches!(bound.kind, PatKind::Wild))
                .filter_map(|(field, _)| field.as_local())
                .filter(|&field| kept(cx.tcx.type_of(field).instantiate_identity().skip_normalization()))
                .map(|field| (Place::Field(field), None)),
        );
    }

    fn unescaped<'uses>(&'uses self, cx: &LateContext<'_>) -> impl Iterator<Item = (Place, &'uses U)> {
        self.places.iter().filter_map(|(&place, usage)| Some((place, usage.as_ref()?))).filter(|&(place, _)| !place.exported(cx) && !expanded_externally(cx, place.span(cx)))
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

struct AsyncContext {
    scope_spawn: Option<DefId>,
}

impl AsyncContext {
    fn new(tcx: TyCtxt<'_>) -> Self { Self { scope_spawn: path(tcx, PathNS::Value, "std::thread::Scope::spawn") } }

    fn is_async(&self, cx: &LateContext<'_>, body: LocalDefId) -> bool {
        let offloaded = |closure: &Expr<'_>| {
            get_parent_expr(cx, closure).and_then(|call| parameter_of(cx, call, closure)).is_some_and(|(callee, input)| {
                self.scope_spawn == Some(callee)
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
            None => matches!(cx.tcx.hir_node_by_def_id(body), Node::Expr(closure) if !offloaded(closure) && self.is_async(cx, cx.tcx.hir_enclosing_body_owner(closure.hir_id))),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn stored_place<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<(Place, Ty<'tcx>)> {
    match place(cx, expr)? {
        stored @ (Place::Field(item) | Place::Static(item)) => Some((stored, cx.tcx.type_of(item).instantiate_identity().skip_normalization())),
        Place::Local(binding) if let Node::LetStmt(local) = cx.tcx.parent_hir_node(binding) => {
            let bound = cx.typeck_results().node_type(binding);
            if let ty::Adt(shared, args) = bound.kind()
                && shared.is_diag_item(cx, sym::Arc)
            {
                let init = local.init?;
                cloned(cx, init).map_or_else(
                    || {
                        fn_def_id(cx, init)
                            .filter(|&built| cx.tcx.item_name(built) == sym::new && impl_owner(cx.tcx, built).is_diag_item(cx, sym::Arc))
                            .map(|_| (Place::Local(binding), args.type_at(0)))
                    },
                    |source| stored_place(cx, source),
                )
            } else {
                Some((Place::Local(binding), bound))
            }
        }
        Place::Local(_) | Place::Parameter(_) => None,
    }
}

fn cloned_into_binding(cx: &LateContext<'_>, expr: &Expr<'_>) -> bool {
    get_parent_expr(cx, expr).and_then(|parent| if let ExprKind::AddrOf(..) = parent.kind { get_parent_expr(cx, parent) } else { Some(parent) }).is_some_and(|call| {
        cloned(cx, call).is_some_and(|source| source.hir_id == expr.hir_id)
            && matches!(cx.tcx.parent_hir_node(call.hir_id), Node::LetStmt(local) if matches!(local.pat.kind, PatKind::Binding(_, _, _, None)))
    })
}

fn in_drop_impl(cx: &LateContext<'_>, node: HirId) -> bool { enclosing_trait_impl(cx, node).map(|implementation| cx.tcx.impl_trait_id(implementation)).is_lang_item(cx, LangItem::Drop) }

fn boxed_future<'tcx>(cx: &LateContext<'tcx>, output: Ty<'tcx>) -> Option<bool> {
    if let ty::Adt(pin, pinned) = output.kind()
        && pin.is_lang_item(cx, LangItem::Pin)
        && let ty::Adt(boxed, boxed_args) = pinned.type_at(0).kind()
        && boxed.is_box()
        && let ty::Dynamic(predicates, ..) = boxed_args.type_at(0).kind()
        && predicates.principal_def_id().is_lang_item(cx, LangItem::Future)
    {
        Some(predicates.auto_traits().any(|auto| auto.is_diag_item(cx, sym::Send)))
    } else {
        None
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod async_lock_without_await;
pub(crate) mod blocking_call_in_async;
pub(crate) mod detached_task;
pub(crate) mod manual_abort_on_drop;
pub(crate) mod manual_arc_swap;
pub(crate) mod manual_cancellation_token;
pub(crate) mod manual_stream;
pub(crate) mod needless_boxed_future;
pub(crate) mod optional_channel_sender;
pub(crate) mod redundant_spawn;
pub(crate) mod sequential_await;
pub(crate) mod short_circuited_join;
pub(crate) mod unbounded_pending_map;
pub(crate) mod unending_join;
