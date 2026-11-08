use core::{iter, mem};

use clippy_utils::{
    diagnostics::span_lint_hir,
    expr_or_init, fn_def_id, is_expr_temporary_value,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
};
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{Expr, ExprKind, HirId, LetStmt, PatKind, Stmt, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    TyCtxt,
    adjustment::{Adjust, Adjustment},
};
use rustc_span::{Span, sym};

use crate::{expanded_externally, impl_owner, paths, resolve, signature};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    handles: FxHashMap<DefId, &'static str>,
    trackers: FxHashSet<DefId>,
    tracked: FxHashSet<DefId>,
    spawns: FxHashSet<DefId>,
    detach_all: FxHashSet<DefId>,
    unread: FxIndexMap<HirId, Option<(Span, DefId)>>,
}

lint!(Pass; DETACHED_TASK: "task and thread handle drops that detach work, and `JoinSet::detach_all` calls");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let tracking = paths(tcx, PathNS::Value, &["tokio_util::task::TaskTracker::track_future"]);
        Self {
            handles: resolve(tcx, PathNS::Type, &[("tokio::task::JoinHandle", "task"), ("std::thread::JoinHandle", "thread")]),
            trackers: tracking.iter().filter_map(|&method| impl_owner(tcx, method).opt_def_id()).collect(),
            tracked: tracking.iter().filter_map(|&method| signature(tcx, method).output().opt_def_id()).collect(),
            spawns: paths(
                tcx,
                PathNS::Value,
                &["tokio::task::spawn", "tokio::task::spawn_local", "tokio::runtime::Runtime::spawn", "tokio::runtime::Handle::spawn", "tokio::task::LocalSet::spawn_local"],
            ),
            detach_all: paths(tcx, PathNS::Value, &["tokio::task::JoinSet::detach_all"]),
            unread: FxIndexMap::default(),
        }
    }

    fn detached(&self, cx: &LateContext<'_>, handle: &Expr<'_>) -> Option<(DefId, &'static str)> {
        let typeck = cx.typeck_results();
        let dropped = typeck.expr_ty(handle).opt_def_id()?;
        let &work = self.handles.get(&dropped)?;
        let origin = expr_or_init(cx, handle);
        let producer = fn_def_id(cx, origin);
        let owned = producer.and_then(|method| impl_owner(cx.tcx, method).opt_def_id()).is_some_and(|owner| self.trackers.contains(&owner))
            || producer.is_some_and(|producer| self.spawns.contains(&producer))
                && matches!(origin.kind, ExprKind::MethodCall(_, receiver, arguments, _) | ExprKind::Call(receiver, arguments)
                    if iter::chain([receiver], arguments).any(|operand| typeck.expr_ty_adjusted(operand).opt_def_id().is_some_and(|adt| self.tracked.contains(&adt))));
        (!owned).then_some((dropped, work))
    }

    fn report(cx: &LateContext<'_>, (dropped, work): (DefId, &'static str), node: HirId, span: Span) {
        span_lint_hir(cx, DETACHED_TASK, node, span, format!("dropping `{}` detaches its {work}", cx.tcx.def_path_str(dropped)));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_stmt(&mut self, cx: &LateContext<'tcx>, statement: &'tcx Stmt<'tcx>) {
        if let StmtKind::Semi(expr) = statement.kind
            && !expanded_externally(cx, statement.span)
            && let Some(handle) = self.detached(cx, expr)
        {
            Self::report(cx, handle, expr.hir_id, expr.span.source_callsite());
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let Some(init) = local.init
            && !expanded_externally(cx, local.span)
            && let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && let Some((handle, _)) = self.detached(cx, init)
        {
            self.unread.insert(binding, Some((local.span, handle)));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(binding) = expr.res_local_id()
            && let Some(unread) = self.unread.get_mut(&binding)
        {
            *unread = None;
        }
        if expanded_externally(cx, expr.span) {
            return;
        }
        if let ExprKind::Call(_, [argument]) = expr.kind
            && fn_def_id(cx, expr).is_diag_item(cx, sym::mem_drop)
            && let Some(handle) = self.detached(cx, argument)
        {
            Self::report(cx, handle, expr.hir_id, expr.span);
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && matches!(cx.typeck_results().expr_adjustments(receiver), [.., Adjustment { kind: Adjust::Borrow(_), .. }])
            && let Some(handle) = self.detached(cx, receiver)
            && is_expr_temporary_value(cx, receiver)
        {
            Self::report(cx, handle, receiver.hir_id, receiver.span.source_callsite());
        }
        if let ExprKind::MethodCall(segment, ..) = expr.kind
            && let Some(called) = fn_def_id(cx, expr).filter(|called| self.detach_all.contains(called))
            && let Some(set) = impl_owner(cx.tcx, called).opt_def_id()
        {
            span_lint_hir(cx, DETACHED_TASK, expr.hir_id, segment.ident.span.to(expr.span.shrink_to_hi()), format!("`{}::detach_all` detaches every task in the set", cx.tcx.def_path_str(set)));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let unread = mem::take(&mut self.unread).into_iter().filter_map(|(binding, unread)| unread.and_then(|(span, dropped)| Some((binding, span, (dropped, *self.handles.get(&dropped)?)))));
        for (binding, span, handle) in unread {
            Self::report(cx, handle, binding, span);
        }
    }
}
