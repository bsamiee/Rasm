use core::ops::ControlFlow;

use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    higher::ForLoop,
    paths::PathNS,
    peel_hir_expr_while,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, make_normalized_projection},
    visitors::for_each_expr_without_closures,
};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::TyCtxt;

use crate::{path, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    SEQUENTIAL_JOIN,
    Warn,
    "`for` loops awaiting futures built before the loop one at a time"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct SequentialJoin {
    join_error: Option<DefId>,
}

impl_lint_pass!(SequentialJoin => [SEQUENTIAL_JOIN]);

impl SequentialJoin {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { join_error: path(tcx, PathNS::Type, "tokio::task::JoinError") } }
}

impl<'tcx> LateLintPass<'tcx> for SequentialJoin {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(for_loop) = ForLoop::hir(expr)
            && !for_loop.span.from_expansion()
        {
            let typeck = cx.typeck_results();
            let mut bindings = FxHashSet::default();
            for_loop.pat.each_binding(|_, binding, _, _| {
                bindings.insert(binding);
            });
            let bound = |operand: &Expr<'_>| {
                peel_hir_expr_while(operand, |place| if let ExprKind::Field(base, _) | ExprKind::AddrOf(_, _, base) = place.kind { Some(base) } else { None })
                    .res_local_id()
                    .is_some_and(|local| bindings.contains(&local))
            };
            let into_future = cx.tcx.get_diagnostic_item(sym::IntoFuture);
            let wrapped = |argument: &&'tcx Expr<'tcx>| bound(argument) && into_future.is_some_and(|into_future| implements_trait(cx, typeck.expr_ty(argument), into_future, &[]));
            let task = |future: &Expr<'_>| {
                into_future
                    .and_then(|into_future| make_normalized_projection(cx.tcx, cx.typing_env(), into_future, sym::Output, [typeck.expr_ty(future)]))
                    .and_then(|output| result_parts(cx, output))
                    .is_some_and(|(_, error)| error.opt_def_id().is_some_and(|error| Some(error) == self.join_error))
            };
            let joined = |future: &'tcx Expr<'tcx>| {
                if bound(future) {
                    Some(future)
                } else if let ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _) = future.kind {
                    arguments.iter().find(wrapped)
                } else {
                    None
                }
            };
            if let Some((awaited, future)) =
                for_each_expr_without_closures(for_loop.body, |inner| desugar_await(inner).and_then(joined).map_or(ControlFlow::Continue(()), |future| ControlFlow::Break((inner, future))))
                && !task(future)
            {
                span_lint_hir_and_then(cx, SEQUENTIAL_JOIN, expr.hir_id, for_loop.span.with_hi(for_loop.arg.span.hi()), "`for` loop awaits futures built before it one at a time", |diag| {
                    diag.span_note(awaited.span, "each future is awaited here before the next one");
                    diag.help("await `futures::future::join_all` over the futures, its results keep input order, or drive `futures::stream::iter(..).buffered(n)` to bound how many run at once");
                });
            }
        }
    }
}
