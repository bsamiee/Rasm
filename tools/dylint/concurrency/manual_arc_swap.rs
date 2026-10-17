use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    visitors::is_local_used,
};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Block, Expr, ExprKind, LetStmt, Pat, PatKind, Stmt, StmtKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use super::{PlaceUses, cloned_into_binding, stored_place};
use crate::{Access, Place, access, constructed, generated, impl_owner, method_call_on, path, pointee, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Method {
    Read,
    Guard,
    Replace,
    Modify,
}

#[derive(Clone, Copy, PartialEq, Eq)]
enum Holder {
    Lock,
    Watch,
}

// --- [MODELS] --------------------------------------------------------------------------

struct Usage {
    holder: Holder,
    written: Option<Span>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    methods: FxHashMap<DefId, Method>,
    holders: FxHashMap<DefId, Holder>,
    sender_new: Option<DefId>,
    places: PlaceUses<Usage>,
}

lint!(Pass; MANUAL_ARC_SWAP: "locks over an `Arc`, and `watch::Sender`s with no receiver, that every write replaces whole");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let held = resolve(
            tcx,
            PathNS::Value,
            &[
                ("std::sync::nonpoison::RwLock::read", (Method::Read, Holder::Lock)),
                ("std::sync::nonpoison::RwLock::with", (Method::Read, Holder::Lock)),
                ("std::sync::nonpoison::RwLock::write", (Method::Guard, Holder::Lock)),
                ("std::sync::nonpoison::RwLock::set", (Method::Replace, Holder::Lock)),
                ("std::sync::nonpoison::RwLock::replace", (Method::Replace, Holder::Lock)),
                ("std::sync::nonpoison::Mutex::lock", (Method::Guard, Holder::Lock)),
                ("std::sync::nonpoison::Mutex::set", (Method::Replace, Holder::Lock)),
                ("std::sync::nonpoison::Mutex::replace", (Method::Replace, Holder::Lock)),
                ("tokio::sync::watch::Sender::borrow", (Method::Read, Holder::Watch)),
                ("tokio::sync::watch::Sender::send_replace", (Method::Replace, Holder::Watch)),
                ("tokio::sync::watch::Sender::send_modify", (Method::Modify, Holder::Watch)),
            ],
        );
        let holders = held.iter().filter_map(|(&method, &(_, holder))| Some((impl_owner(tcx, method)?.opt_def_id()?, holder))).collect();
        let sender_new = path(tcx, PathNS::Value, "tokio::sync::watch::Sender::new");
        Self { methods: held.into_iter().map(|(method, (kind, _))| (method, kind)).collect(), holders, sender_new, places: PlaceUses::default() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn overwrites(cx: &LateContext<'_>, modify: &Expr<'_>) -> bool {
            let ExprKind::Closure(closure) = modify.kind else { return false };
            let body = cx.tcx.hir_body(closure.body);
            let [parameter] = body.params else { return false };
            let PatKind::Binding(_, value, ..) = parameter.pat.kind else { return false };
            let assigned = match peel_blocks(body.value).kind {
                ExprKind::Block(Block { stmts: [Stmt { kind: StmtKind::Semi(assigned) | StmtKind::Expr(assigned), .. }], expr: None, .. }, _) => assigned,
                _ => peel_blocks(body.value),
            };
            let ExprKind::Assign(Expr { kind: ExprKind::Unary(UnOp::Deref, place), .. }, next, _) = assigned.kind else { return false };
            place.res_local_id() == Some(value) && !is_local_used(cx, next, value)
        }

        if generated(cx, expr.hir_id) {
            return;
        }
        if let Some((_, fields)) = constructed(cx.typeck_results(), expr) {
            fields
                .into_iter()
                .filter_map(|(field, init)| Some((field.did.as_local()?, init)))
                .filter(|&(field, init)| {
                    whole_value_holder(cx, &self.holders, cx.tcx.type_of(field).instantiate_identity().skip_normalization()) == Some(Holder::Watch) && !fresh(cx, self.sender_new, init)
                })
                .for_each(|(field, _)| self.places.escape(Place::Field(field)));
        }
        if let Some((place, holder)) = stored_place(cx, expr).and_then(|(place, ty)| Some((place, whole_value_holder(cx, &self.holders, ty)?))) {
            let method = method_call_on(cx, expr).and_then(|(call, method)| Some((*self.methods.get(&method)?, call)));
            let written = match method {
                Some((Method::Read, _)) => Some(None),
                Some((Method::Replace, replaced)) => Some(Some(replaced.span)),
                Some((Method::Modify, modified)) => matches!(modified.kind, ExprKind::MethodCall(_, _, [modify], _) if overwrites(cx, modify)).then_some(Some(modified.span)),
                Some((Method::Guard, guard)) => match get_parent_expr(cx, guard)
                    .filter(|deref| matches!(deref.kind, ExprKind::Unary(UnOp::Deref, _)))
                    .map_or_else(|| (guard, access(cx, guard)), |deref| (deref, access(cx, deref)))
                {
                    (deref, Access::Assigned(value)) => Some(Some(deref.span.to(value.span))),
                    (_, Access::Copied | Access::Borrowed) => Some(None),
                    (_, Access::Mutated | Access::Moved) => None,
                },
                None => cloned_into_binding(cx, expr).then_some(None),
            };
            match written {
                Some(written) =>
                    if let Some(usage) = self.places.record(place, || Usage { holder, written: None }) {
                        usage.written = usage.written.or(written);
                    },
                None => self.places.escape(place),
            }
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        let shared = |init: &'tcx Expr<'tcx>| match init.kind {
            ExprKind::Call(_, [inner]) if cx.typeck_results().expr_ty(init).is_diag_item(cx, sym::Arc) => inner,
            _ => init,
        };
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && whole_value_holder(cx, &self.holders, pointee(cx, cx.typeck_results().node_type(binding))) == Some(Holder::Watch)
            && !local.init.is_some_and(|init| fresh(cx, self.sender_new, shared(init)))
        {
            self.places.escape(Place::Local(binding));
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.places.escape_bound(cx, pat, |held| whole_value_holder(cx, &self.holders, held).is_some()); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (place, holder, written) in self.places.unescaped(cx).filter_map(|(place, usage)| Some((place, usage.holder, usage.written?))) {
            let name = place.name(cx);
            let (message, help) = match holder {
                Holder::Lock =>
                    (format!("every write to `{name}` replaces its whole `Arc`"), "declare `arc_swap::ArcSwap`, read with `load_full()`, and write with `store(Arc::new(..))` or `rcu(..)`"),
                Holder::Watch => (
                    format!("every write to `{name}` replaces its whole value, and no receiver subscribes to it"),
                    "declare `arc_swap::ArcSwap`, read with `load()`, and write with `store(Arc::new(..))`",
                ),
            };
            span_lint_hir_and_then(cx, MANUAL_ARC_SWAP, place.hir_id(cx), place.span(cx), message, |diag| {
                diag.span_note(written, "replaced here");
                diag.help(help);
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn fresh(cx: &LateContext<'_>, sender_new: Option<DefId>, init: &Expr<'_>) -> bool {
    fn_def_id(cx, init).is_some_and(|built| sender_new == Some(built) || built.opt_parent(cx).is_diag_item(cx, sym::Default))
}

fn whole_value_holder<'tcx>(cx: &LateContext<'tcx>, holders: &FxHashMap<DefId, Holder>, held: Ty<'tcx>) -> Option<Holder> {
    let ty::Adt(holder, args) = held.kind() else { return None };
    holders.get(&holder.did()).copied().filter(|&holder| holder == Holder::Watch || args.type_at(0).is_diag_item(cx, sym::Arc))
}
