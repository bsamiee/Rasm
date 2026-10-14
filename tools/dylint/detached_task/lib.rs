#![feature(rustc_private)]

extern crate rustc_attr_ir;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use clippy_utils::{
    diagnostics::span_lint_and_help,
    fn_def_id, get_enclosing_block,
    paths::{PathNS, lookup_path_str},
    res::MaybeDef as _,
    visitors::is_local_used,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind, LetStmt, PatKind, Stmt, StmtKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::adjustment::{Adjust, Adjustment};
use rustc_span::{Span, sym};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Spawned {
    Task,
    Thread,
}

impl Spawned {
    const fn help(self) -> &'static str {
        match self {
            Self::Task => "spawn on the `TaskTracker` or `JoinSet` its owner waits on at shutdown",
            Self::Thread => "spawn the thread inside `std::thread::scope`",
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub DETACHED_TASK,
    Warn,
    "task and thread handles dropped with no owner joining their work",
    DetachedTask::default()
}

#[derive(Default)]
struct DetachedTask {
    handles: Vec<(DefId, Spawned)>,
    trackers: Vec<DefId>,
}

impl DetachedTask {
    fn detach(&self, cx: &LateContext<'_>, handle: &Expr<'_>, span: Span) {
        let typeck = cx.typeck_results();
        if !span.from_expansion()
            && let Some((dropped, spawned)) = typeck.expr_ty(handle).opt_def_id().and_then(|adt| self.handles.iter().copied().find(|(known, _)| *known == adt))
            && !matches!(handle.kind, ExprKind::MethodCall(_, receiver, ..) if typeck.expr_ty_adjusted(receiver).peel_refs().opt_def_id().is_some_and(|adt| self.trackers.contains(&adt)))
            && !cx
                .tcx
                .trait_impl_of_assoc(cx.tcx.typeck_root_def_id(cx.tcx.hir_enclosing_body_owner(handle.hir_id).to_def_id()))
                .is_some_and(|implementation| cx.tcx.is_lang_item(cx.tcx.impl_trait_id(implementation), LangItem::Drop))
        {
            span_lint_and_help(cx, DETACHED_TASK, span, format!("dropping `{}` detaches its work", cx.tcx.def_path_str(dropped)), None, spawned.help());
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for DetachedTask {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        self.handles = [("tokio::task::JoinHandle", Spawned::Task), ("std::thread::JoinHandle", Spawned::Thread)]
            .into_iter()
            .flat_map(|(path, spawned)| lookup_path_str(cx.tcx, PathNS::Type, path).into_iter().map(move |handle| (handle, spawned)))
            .collect();
        self.trackers = lookup_path_str(cx.tcx, PathNS::Type, "tokio_util::task::TaskTracker");
    }

    fn check_stmt(&mut self, cx: &LateContext<'tcx>, statement: &'tcx Stmt<'tcx>) {
        if let StmtKind::Semi(expr) = statement.kind {
            self.detach(cx, expr, expr.span);
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, _, None) = local.pat.kind
            && let Some(init) = local.init
            && get_enclosing_block(cx, local.hir_id).is_some_and(|block| !is_local_used(cx, block, binding))
        {
            self.detach(cx, init, local.span);
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(_, [argument]) = expr.kind
            && fn_def_id(cx, expr).is_some_and(|callee| callee.is_diag_item(cx, sym::mem_drop))
        {
            self.detach(cx, argument, expr.span);
        }
        if let ExprKind::MethodCall(_, receiver, ..) = expr.kind
            && !receiver.is_syntactic_place_expr()
            && matches!(cx.typeck_results().expr_adjustments(receiver), [.., Adjustment { kind: Adjust::Borrow(_), target: _ }])
        {
            self.detach(cx, receiver, receiver.span);
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
