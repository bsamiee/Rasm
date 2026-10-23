use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    get_enclosing_block,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    visitors::for_each_expr,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, HirId, LetStmt, Node, Param, Pat, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use super::{PlaceUses, boxed_future, in_drop_impl, stored_place};
use crate::{impl_owner, method_call_on, paths, pointee, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Guard {
    Returned,
    Lent,
}

#[derive(Clone, Copy)]
enum Method {
    Insert,
    Evict,
}

// --- [MODELS] --------------------------------------------------------------------------

struct Usage<'tcx> {
    entry: Ty<'tcx>,
    inserted: Option<Span>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    guards: FxHashMap<DefId, Guard>,
    locks: FxHashSet<DefId>,
    maps: FxHashSet<DefId>,
    pending: FxHashSet<DefId>,
    methods: FxHashMap<DefId, Method>,
    usages: PlaceUses<Usage<'tcx>>,
}

lint!(Pass<'_>; UNBOUNDED_PENDING_MAP: "locked maps of pending senders, futures, or task handles that only a keyed `remove` deletes from");

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(tcx: TyCtxt<'tcx>) -> Self {
        let owner = |method: DefId| impl_owner(tcx, method).opt_def_id();
        let guards = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::sync::nonpoison::Mutex::lock", Guard::Returned),
                ("std::sync::nonpoison::RwLock::write", Guard::Returned),
                ("std::sync::nonpoison::RwLock::read", Guard::Returned),
                ("std::sync::nonpoison::Mutex::with_mut", Guard::Lent),
                ("std::sync::nonpoison::RwLock::with_mut", Guard::Lent),
                ("std::sync::nonpoison::RwLock::with", Guard::Lent),
            ],
        );
        let methods = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::collections::BTreeMap::insert", Method::Insert),
                ("std::collections::BTreeMap::try_insert", Method::Insert),
                ("std::collections::BTreeMap::entry", Method::Insert),
                ("std::collections::BTreeMap::retain", Method::Evict),
                ("std::collections::BTreeMap::extract_if", Method::Evict),
                ("std::collections::BTreeMap::split_off", Method::Evict),
                ("std::collections::BTreeMap::pop_first", Method::Evict),
                ("std::collections::BTreeMap::pop_last", Method::Evict),
                ("std::collections::BTreeMap::first_entry", Method::Evict),
                ("std::collections::BTreeMap::last_entry", Method::Evict),
                ("std::collections::BTreeMap::clear", Method::Evict),
                ("std::collections::HashMap::insert", Method::Insert),
                ("std::collections::HashMap::try_insert", Method::Insert),
                ("std::collections::HashMap::entry", Method::Insert),
                ("std::collections::HashMap::retain", Method::Evict),
                ("std::collections::HashMap::extract_if", Method::Evict),
                ("std::collections::HashMap::drain", Method::Evict),
                ("std::collections::HashMap::clear", Method::Evict),
                ("indexmap::IndexMap::insert", Method::Insert),
                ("indexmap::IndexMap::insert_full", Method::Insert),
                ("indexmap::IndexMap::insert_before", Method::Insert),
                ("indexmap::IndexMap::insert_sorted", Method::Insert),
                ("indexmap::IndexMap::shift_insert", Method::Insert),
                ("indexmap::IndexMap::entry", Method::Insert),
                ("indexmap::IndexMap::retain", Method::Evict),
                ("indexmap::IndexMap::extract_if", Method::Evict),
                ("indexmap::IndexMap::drain", Method::Evict),
                ("indexmap::IndexMap::split_off", Method::Evict),
                ("indexmap::IndexMap::truncate", Method::Evict),
                ("indexmap::IndexMap::pop", Method::Evict),
                ("indexmap::IndexMap::shift_remove_index", Method::Evict),
                ("indexmap::IndexMap::swap_remove_index", Method::Evict),
                ("indexmap::IndexMap::first_entry", Method::Evict),
                ("indexmap::IndexMap::last_entry", Method::Evict),
                ("indexmap::IndexMap::clear", Method::Evict),
            ],
        );
        let pending = paths(
            tcx,
            PathNS::Type,
            &[
                "tokio::sync::oneshot::Sender",
                "tokio::task::JoinHandle",
                "tokio::task::AbortHandle",
                "tokio_util::task::AbortOnDropHandle",
                "futures::channel::oneshot::Sender",
                "futures::future::AbortHandle",
                "std::thread::JoinHandle",
            ],
        );
        let locks = guards.keys().copied().filter_map(owner).collect();
        Self { guards, locks, maps: methods.keys().copied().filter_map(owner).collect(), pending, methods, usages: PlaceUses::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn holds<'tcx>(cx: &LateContext<'tcx>, pending: &FxHashSet<DefId>, held: Ty<'tcx>, seen: &mut FxHashSet<DefId>) -> bool {
            held.walk().filter_map(ty::GenericArg::as_type).any(|inner| {
                boxed_future(cx, inner).is_some()
                    || matches!(inner.kind(), ty::Adt(adt, args)
                        if pending.contains(&adt.did())
                            || (adt.did().is_local() && seen.insert(adt.did()) && adt.all_fields().any(|field| holds(cx, pending, field.ty(cx.tcx, args).skip_normalization(), seen))))
            })
        }

        fn released(cx: &LateContext<'_>, call: &Expr<'_>) -> bool {
            in_drop_impl(cx, call.hir_id)
                || cx.tcx.hir_parent_iter(call.hir_id).any(|(_, node)| {
                    matches!(node, Node::Expr(Expr { kind: ExprKind::Closure(closure), .. }) if matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Block))))
                })
        }

        let entry = |held: Ty<'tcx>| {
            let ty::Adt(lock, locked) = held.kind() else { return None };
            let ty::Adt(map, entries) = locked.types().next().filter(|_| self.locks.contains(&lock.did()))?.kind() else { return None };
            entries.types().nth(1).filter(|&entry| self.maps.contains(&map.did()) && holds(cx, &self.pending, entry, &mut FxHashSet::default()))
        };
        let inserted = |guard: &'tcx Expr<'tcx>| {
            let (call, method) = method_call_on(cx, guard).filter(|&(_, method)| impl_owner(cx.tcx, method).opt_def_id().is_some_and(|map| self.maps.contains(&map)))?;
            match self.methods.get(&method) {
                Some(Method::Insert) => Some(Some(call.span)),
                Some(Method::Evict) => None,
                None => (!released(cx, call)).then_some(None),
            }
        };
        let first_insert = |binding: HirId, scope: &'tcx Expr<'tcx>| {
            let mut first = None;
            for_each_expr(cx.tcx, scope, |used| match (used.res_local_id() == Some(binding)).then(|| inserted(used)) {
                Some(Some(call)) => {
                    first = first.or(call);
                    ControlFlow::Continue(())
                }
                Some(None) => ControlFlow::Break(()),
                None => ControlFlow::Continue(()),
            })
            .is_none()
            .then_some(first)
        };
        let Some((place, entry)) = stored_place(cx, expr).and_then(|(place, ty)| Some((place, entry(pointee(cx, ty))?))) else { return };
        let guarded = method_call_on(cx, expr).and_then(|(call, guard)| match (self.guards.get(&guard)?, cx.tcx.parent_hir_node(call.hir_id), call.kind) {
            (Guard::Returned, Node::LetStmt(&LetStmt { pat: &Pat { kind: PatKind::Binding(_, binding, _, None), .. }, hir_id, .. }), _)
                if let Node::Expr(scope) = cx.tcx.parent_hir_node(get_enclosing_block(cx, hir_id)?.hir_id) =>
                first_insert(binding, scope),
            (Guard::Returned, ..) => inserted(call),
            (Guard::Lent, _, ExprKind::MethodCall(_, _, [Expr { kind: ExprKind::Closure(closure), .. }], _))
                if let body = cx.tcx.hir_body(closure.body)
                    && let [Param { pat: Pat { kind: PatKind::Binding(_, binding, _, None), .. }, .. }] = body.params =>
                first_insert(*binding, &body.value),
            (Guard::Lent, ..) => None,
        });
        match guarded {
            Some(call) =>
                if let Some(usage) = self.usages.record(place, || Usage { entry, inserted: None }) {
                    usage.inserted = usage.inserted.or(call);
                },
            None => self.usages.escape(place),
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (place, entry, inserted) in self.usages.unescaped(cx).filter_map(|(place, usage)| Some((place, usage.entry, usage.inserted?))) {
            let name = place.name(cx);
            span_lint_hir_and_then(cx, UNBOUNDED_PENDING_MAP, place.hir_id(cx), place.span(cx), format!("`{name}` holds `{entry}` entries that only a keyed `remove` deletes"), |diag| {
                diag.span_note(inserted, "inserted here");
                diag.help(format!("return a guard from each insert that removes its key from `{name}` on drop"));
            });
        }
    }
}
