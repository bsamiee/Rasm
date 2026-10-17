use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    visitors::is_local_used,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Block, Expr, ExprKind, LetStmt, Pat, PatKind, Stmt, StmtKind, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::Span;

use crate::{Access, Ledger, Place, access, constructed, generated, held, impl_owner, paths, pointee, rebound, receiving_call, resolve};

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

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_ARC_SWAP,
    Warn,
    "locks over an `Arc`, and `watch::Sender`s no receiver subscribes to, that every write replaces whole"
}

// --- [MODELS] --------------------------------------------------------------------------

struct Usage {
    holder: Holder,
    writes: Vec<Span>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualArcSwap {
    methods: FxHashMap<DefId, Method>,
    holders: FxHashMap<DefId, Holder>,
    fresh: FxHashSet<DefId>,
    snapshots: Ledger<Usage>,
}

impl_lint_pass!(ManualArcSwap => [MANUAL_ARC_SWAP]);

impl ManualArcSwap {
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
        let fresh = paths(tcx, PathNS::Value, &["tokio::sync::watch::Sender::new"]);
        Self { methods: held.into_iter().map(|(method, (kind, _))| (method, kind)).collect(), holders, fresh, snapshots: Ledger::default() }
    }

    fn fresh(&self, cx: &LateContext<'_>, init: &Expr<'_>) -> bool {
        fn_def_id(cx, init).is_some_and(|built| self.fresh.contains(&built) || cx.tcx.trait_of_assoc(built).is_some_and(|owner| cx.tcx.is_diagnostic_item(sym::Default, owner)))
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualArcSwap {
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
            let unfresh = fields
                .into_iter()
                .filter_map(|(field, init)| Some((field.did.as_local()?, init)))
                .filter(|&(field, init)| snapshot(cx, &self.holders, cx.tcx.type_of(field).instantiate_identity().skip_normalization()) == Some(Holder::Watch) && !self.fresh(cx, init))
                .map(|(field, _)| field)
                .collect::<Vec<_>>();
            for field in unfresh {
                self.snapshots.escape(Place::Field(field));
            }
        }
        if let Some((place, holder)) = held(cx, expr).filter(|&(place, _)| !place.exported(cx)).and_then(|(place, ty)| Some((place, snapshot(cx, &self.holders, ty)?))) {
            let method = receiving_call(cx, expr).and_then(|(call, method)| Some((*self.methods.get(&method)?, call)));
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
                None => rebound(cx, expr).then_some(None),
            };
            match written {
                Some(written) =>
                    if let Some(usage) = self.snapshots.record(place, || Usage { holder, writes: Vec::new() }) {
                        usage.writes.extend(written);
                    },
                None => self.snapshots.escape(place),
            }
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        let shared = |init: &'tcx Expr<'tcx>| match init.kind {
            ExprKind::Call(_, [inner]) if cx.typeck_results().expr_ty(init).is_diag_item(cx, sym::Arc) => inner,
            _ => init,
        };
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && snapshot(cx, &self.holders, pointee(cx, cx.typeck_results().node_type(binding))) == Some(Holder::Watch)
            && !local.init.is_some_and(|init| self.fresh(cx, shared(init)))
        {
            self.snapshots.escape(Place::Local(binding));
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) { self.snapshots.escape_bound(cx, pat, |held| snapshot(cx, &self.holders, held).is_some()); }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (place, holder, written) in self.snapshots.held(cx).filter_map(|(place, usage)| Some((place, usage.holder, *usage.writes.first()?))) {
            let (message, note, help) = match holder {
                Holder::Lock => (
                    format!("`{}` locks an `Arc` that every write replaces whole", place.name(cx)),
                    "the `Arc` is replaced here",
                    "hold an `arc_swap::ArcSwap`, read it with `load_full()`, and replace it with `store(Arc::new(..))` or `rcu(..)`",
                ),
                Holder::Watch => (
                    format!("`{}` sends through a `watch::Sender` that no receiver subscribes to and every write replaces whole", place.name(cx)),
                    "the watched value is replaced here",
                    "hold an `arc_swap::ArcSwap`, read it with `load()`, and replace it with `store(Arc::new(..))`",
                ),
            };
            span_lint_hir_and_then(cx, MANUAL_ARC_SWAP, place.hir_id(cx), place.span(cx), message, |diag| {
                diag.span_note(written, note);
                diag.help(help);
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn snapshot<'tcx>(cx: &LateContext<'tcx>, holders: &FxHashMap<DefId, Holder>, held: Ty<'tcx>) -> Option<Holder> {
    let ty::Adt(holder, args) = held.kind() else { return None };
    holders.get(&holder.did()).copied().filter(|&holder| holder == Holder::Watch || args.type_at(0).is_diag_item(cx, sym::Arc))
}
