use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id, get_enclosing_block, is_expr_temporary_value, local_is_initialized, path_to_local_with_projections,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    visitors::is_local_used,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, HirId, LetStmt, PatKind, Stmt, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{
    TyCtxt,
    adjustment::{Adjust, Adjustment},
};
use rustc_span::{Span, sym};

use crate::{Polling, enclosing_trait_impl, item_path, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Spawned {
    Task,
    Thread,
}

#[derive(Clone, Copy, PartialEq, Eq)]
enum Site {
    Body,
    Async,
    Drop,
}

#[derive(Clone, Copy)]
enum Loss {
    Dropped,
    Replaced,
}

#[derive(Clone, Copy)]
enum Release {
    AbortOnDropHandle,
    AbortOnDrop,
    DropGuard,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    DETACHED_TASK,
    Warn,
    "task and thread handles dropped with no owner joining their work, and `Drop` impls releasing held tasks by hand"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Role {
    Handle(Spawned),
    Tracker,
    Tracked,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct DetachedTask {
    roles: FxHashMap<DefId, Role>,
    releases: FxHashMap<DefId, Release>,
    polling: Polling,
}

impl_lint_pass!(DetachedTask => [DETACHED_TASK]);

impl DetachedTask {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            roles: resolve(
                tcx,
                PathNS::Type,
                &[
                    ("tokio::task::JoinHandle", Role::Handle(Spawned::Task)),
                    ("std::thread::JoinHandle", Role::Handle(Spawned::Thread)),
                    ("tokio_util::task::TaskTracker", Role::Tracker),
                    ("tokio_util::task::task_tracker::TrackedFuture", Role::Tracked),
                ],
            ),
            releases: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("tokio::task::JoinHandle::abort", Release::AbortOnDropHandle),
                    ("tokio::task::AbortHandle::abort", Release::AbortOnDrop),
                    ("tokio_util::sync::CancellationToken::cancel", Release::DropGuard),
                ],
            ),
            polling: Polling::new(tcx),
        }
    }

    fn detached(&self, cx: &LateContext<'_>, handle: &Expr<'_>) -> Option<(DefId, Spawned)> {
        let typeck = cx.typeck_results();
        let dropped = typeck.expr_ty(handle).opt_def_id()?;
        let Some(Role::Handle(spawned)) = self.roles.get(&dropped).copied() else { return None };
        let tracked = |operand: &Expr<'_>| matches!(typeck.expr_ty_adjusted(operand).peel_refs().opt_def_id().and_then(|adt| self.roles.get(&adt)), Some(Role::Tracker | Role::Tracked));
        let owned = if let ExprKind::MethodCall(_, receiver, arguments, _) | ExprKind::Call(receiver, arguments) = expr_or_init(cx, handle).kind {
            iter::chain([receiver], arguments).any(tracked)
        } else {
            false
        };
        (!owned).then_some((dropped, spawned))
    }

    fn site(&self, cx: &LateContext<'_>, node: HirId) -> Site {
        if enclosing_trait_impl(cx, node).is_some_and(|implementation| cx.tcx.impl_trait_id(implementation).is_lang_item(cx, LangItem::Drop)) {
            Site::Drop
        } else if self.polling.polled(cx, cx.tcx.hir_enclosing_body_owner(node)) {
            Site::Async
        } else {
            Site::Body
        }
    }

    fn report(&self, cx: &LateContext<'_>, (dropped, spawned): (DefId, Spawned), node: HirId, span: Span, loss: Loss) {
        let name = cx.tcx.def_path_str(dropped);
        let message = match loss {
            Loss::Dropped => format!("dropping `{name}` detaches its work"),
            Loss::Replaced => format!("assigning over `{name}` drops the previous handle and detaches its work"),
        };
        span_lint_hir_and_then(cx, DETACHED_TASK, node, span, message, |diag| {
            diag.help(match (spawned, self.site(cx, node)) {
                (Spawned::Task, Site::Body | Site::Async) => "spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown",
                (Spawned::Task, Site::Drop) => "spawn the release through `TaskTracker::spawn_on` with a `Handle` captured at construction, or hold a `DropGuard` whose token the release task awaits",
                (Spawned::Thread, Site::Async) => "run the work through `tokio::task::spawn_blocking` and await its handle",
                (Spawned::Thread, Site::Body | Site::Drop) => "spawn the thread inside `std::thread::scope`",
            });
        });
    }
}

impl<'tcx> LateLintPass<'tcx> for DetachedTask {
    fn check_stmt(&mut self, cx: &LateContext<'tcx>, statement: &'tcx Stmt<'tcx>) {
        if let StmtKind::Semi(expr) = statement.kind
            && !statement.span.from_expansion()
            && let Some(handle) = self.detached(cx, expr)
        {
            self.report(cx, handle, expr.hir_id, expr.span.source_callsite(), Loss::Dropped);
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && let Some(init) = local.init
            && !local.span.from_expansion()
            && let Some(handle) = self.detached(cx, init)
            && get_enclosing_block(cx, local.hir_id).is_some_and(|block| !is_local_used(cx, block, binding))
        {
            self.report(cx, handle, local.hir_id, local.span, Loss::Dropped);
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if expr.span.from_expansion() {
            return;
        }
        if let ExprKind::Call(_, [argument]) = expr.kind
            && fn_def_id(cx, expr).is_some_and(|callee| callee.is_diag_item(cx, sym::mem_drop))
            && let Some(handle) = self.detached(cx, argument)
        {
            self.report(cx, handle, expr.hir_id, expr.span, Loss::Dropped);
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && matches!(cx.typeck_results().expr_adjustments(receiver), [.., Adjustment { kind: Adjust::Borrow(_), .. }])
            && let Some(handle) = self.detached(cx, receiver)
            && is_expr_temporary_value(cx, receiver)
        {
            self.report(cx, handle, receiver.hir_id, receiver.span.source_callsite(), Loss::Dropped);
        }
        if let ExprKind::Assign(place, ..) = expr.kind
            && let Some(handle) = self.detached(cx, place)
            && place.res_local_id().is_none_or(|local| local_is_initialized(cx, local))
        {
            self.report(cx, handle, expr.hir_id, expr.span, Loss::Replaced);
        }
        if let ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver]) = expr.kind
            && let Some(method) = fn_def_id(cx, expr)
            && let Some(release) = self.releases.get(&method).copied()
            && self.site(cx, expr.hir_id) == Site::Drop
            && let place = receiver.peel_borrows()
            && matches!(place.kind, ExprKind::Field(..))
            && let Some(this) = cx.tcx.hir_body_owned_by(cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(place.hir_id))).params.first()
            && path_to_local_with_projections(place) == Some(this.pat.hir_id)
        {
            span_lint_hir_and_then(cx, DETACHED_TASK, expr.hir_id, expr.span, format!("`Drop::drop` calls `{}` by hand", item_path(cx, method)), |diag| {
                diag.help(match release {
                    Release::AbortOnDropHandle => "hold the task in a `tokio_util::task::AbortOnDropHandle`, which aborts it when dropped",
                    Release::AbortOnDrop => "hold the abort handle in a `tokio_util::task::AbortOnDrop`, which aborts its task when dropped",
                    Release::DropGuard => "hold the `DropGuard` from `CancellationToken::drop_guard`, which cancels its token when dropped",
                });
            });
        }
    }
}
