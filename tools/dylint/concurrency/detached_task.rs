use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id, get_enclosing_block, is_expr_temporary_value, local_is_initialized,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    visitors::is_local_used,
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, HirId, LetStmt, PatKind, Stmt, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    TyCtxt,
    adjustment::{Adjust, Adjustment},
};
use rustc_span::{Span, sym};

use super::{AsyncContext, in_drop_impl};
use crate::{expanded_externally, impl_owner, path, paths, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Spawned {
    Task,
    Thread,
}

#[derive(Clone, Copy)]
enum Detachment {
    Dropped,
    Replaced,
    Every,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    handles: FxHashMap<DefId, Spawned>,
    trackers: FxHashSet<DefId>,
    detach_all: Option<DefId>,
    async_context: AsyncContext,
}

lint!(Pass; DETACHED_TASK: "task and thread handles dropped with no owner joining them, and `JoinSet::detach_all` calls");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            handles: resolve(tcx, PathNS::Type, &[("tokio::task::JoinHandle", Spawned::Task), ("std::thread::JoinHandle", Spawned::Thread)]),
            trackers: paths(tcx, PathNS::Type, &["tokio_util::task::TaskTracker", "tokio_util::task::task_tracker::TrackedFuture"]),
            detach_all: path(tcx, PathNS::Value, "tokio::task::JoinSet::detach_all"),
            async_context: AsyncContext::new(tcx),
        }
    }

    fn detached(&self, cx: &LateContext<'_>, handle: &Expr<'_>) -> Option<(DefId, Spawned)> {
        let typeck = cx.typeck_results();
        let dropped = typeck.expr_ty(handle).opt_def_id()?;
        let &spawned = self.handles.get(&dropped)?;
        let owned = matches!(expr_or_init(cx, handle).kind, ExprKind::MethodCall(_, receiver, arguments, _) | ExprKind::Call(receiver, arguments)
            if iter::chain([receiver], arguments).any(|operand| typeck.expr_ty_adjusted(operand).peel_refs().opt_def_id().is_some_and(|adt| self.trackers.contains(&adt))));
        (!owned).then_some((dropped, spawned))
    }

    fn report(&self, cx: &LateContext<'_>, (dropped, spawned): (DefId, Spawned), node: HirId, span: Span, detachment: Detachment) {
        let name = cx.tcx.def_path_str(dropped);
        let work = match spawned {
            Spawned::Task => "task",
            Spawned::Thread => "thread",
        };
        let message = match detachment {
            Detachment::Dropped => format!("dropping `{name}` detaches its {work}"),
            Detachment::Replaced => format!("assignment drops the previous `{name}` and detaches its {work}"),
            Detachment::Every => format!("`{name}::detach_all` detaches every task in the set"),
        };
        let help = match (detachment, spawned, in_drop_impl(cx, node), self.async_context.is_async(cx, cx.tcx.hir_enclosing_body_owner(node))) {
            (Detachment::Every, ..) => "await `JoinSet::join_all`, or call `JoinSet::shutdown` where its owner stops",
            (Detachment::Dropped | Detachment::Replaced, Spawned::Task, false, _) => "spawn on a `TaskTracker` or `JoinSet` that its owner waits on at shutdown",
            (Detachment::Dropped | Detachment::Replaced, Spawned::Task, true, _) =>
                "spawn release work through `TaskTracker::spawn_on` with a `Handle` captured at construction, or hold a `DropGuard` with a token the release task awaits",
            (Detachment::Dropped | Detachment::Replaced, Spawned::Thread, false, true) => "run the work in `tokio::task::spawn_blocking` and await its handle",
            (Detachment::Dropped | Detachment::Replaced, Spawned::Thread, _, _) => "spawn the thread in `std::thread::scope`",
        };
        span_lint_hir_and_then(cx, DETACHED_TASK, node, span, message, |diag| {
            diag.help(help);
        });
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_stmt(&mut self, cx: &LateContext<'tcx>, statement: &'tcx Stmt<'tcx>) {
        if let StmtKind::Semi(expr) = statement.kind
            && !expanded_externally(cx, statement.span)
            && let Some(handle) = self.detached(cx, expr)
        {
            self.report(cx, handle, expr.hir_id, expr.span.source_callsite(), Detachment::Dropped);
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && let Some(init) = local.init
            && !expanded_externally(cx, local.span)
            && let Some(handle) = self.detached(cx, init)
            && get_enclosing_block(cx, local.hir_id).is_some_and(|block| !is_local_used(cx, block, binding))
        {
            self.report(cx, handle, local.hir_id, local.span, Detachment::Dropped);
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if expanded_externally(cx, expr.span) {
            return;
        }
        if let ExprKind::Call(_, [argument]) = expr.kind
            && fn_def_id(cx, expr).is_diag_item(cx, sym::mem_drop)
            && let Some(handle) = self.detached(cx, argument)
        {
            self.report(cx, handle, expr.hir_id, expr.span, Detachment::Dropped);
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && matches!(cx.typeck_results().expr_adjustments(receiver), [.., Adjustment { kind: Adjust::Borrow(_), .. }])
            && let Some(handle) = self.detached(cx, receiver)
            && is_expr_temporary_value(cx, receiver)
        {
            self.report(cx, handle, receiver.hir_id, receiver.span.source_callsite(), Detachment::Dropped);
        }
        if let ExprKind::Assign(place, ..) = expr.kind
            && let Some(handle) = self.detached(cx, place)
            && place.res_local_id().is_none_or(|local| local_is_initialized(cx, local))
        {
            self.report(cx, handle, expr.hir_id, expr.span, Detachment::Replaced);
        }
        if let ExprKind::MethodCall(segment, ..) = expr.kind
            && let Some(called) = fn_def_id(cx, expr).filter(|&called| Some(called) == self.detach_all)
            && let Some(set) = impl_owner(cx.tcx, called).opt_def_id()
        {
            self.report(cx, (set, Spawned::Task), expr.hir_id, segment.ident.span.to(expr.span.shrink_to_hi()), Detachment::Every);
        }
    }
}
