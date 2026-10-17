use core::{iter, ops::ControlFlow};

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
use rustc_hir::{Expr, ExprKind, Mutability, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    TyCtxt,
    adjustment::{Adjust, AutoBorrow, AutoBorrowMutability},
};

use crate::{expanded_externally, path, result_parts};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Built {
    Before,
    Inside,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    join_error: Option<DefId>,
}

lint!(Pass; SEQUENTIAL_AWAIT: "`for` loops awaiting one independent future per item in sequence");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { join_error: path(tcx, PathNS::Type, "tokio::task::JoinError") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(for_loop) = ForLoop::hir(expr)
            && !expanded_externally(cx, for_loop.span)
        {
            let typeck = cx.typeck_results();
            let bound = |operand: &Expr<'_>| {
                peel_hir_expr_while(operand, |place| if let ExprKind::Field(base, _) | ExprKind::AddrOf(_, _, base) = place.kind { Some(base) } else { None })
                    .res_local_id()
                    .is_some_and(|local| !for_loop.pat.walk_short(|pat| pat.hir_id != local))
            };
            let into_future = cx.tcx.get_diagnostic_item(sym::IntoFuture);
            let wrapped = |argument: &&'tcx Expr<'tcx>| bound(argument) && into_future.is_some_and(|into_future| implements_trait(cx, typeck.expr_ty(argument), into_future, &[]));
            let task = |future: &Expr<'_>| {
                into_future
                    .and_then(|into_future| make_normalized_projection(cx.tcx, cx.typing_env(), into_future, sym::Output, [typeck.expr_ty(future)]))
                    .and_then(|output| result_parts(cx, output))
                    .is_some_and(|(_, error)| error.opt_def_id().is_some_and(|error| Some(error) == self.join_error))
            };
            let shared = |operand: &Expr<'_>| {
                !typeck.expr_adjustments(operand).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Borrow(AutoBorrow::Ref(AutoBorrowMutability::Mut { .. }))))
                    && !matches!(operand.kind, ExprKind::AddrOf(_, Mutability::Mut, _))
            };
            let joined = |future: &'tcx Expr<'tcx>| {
                let (receiver, arguments) = match future.kind {
                    ExprKind::Call(_, arguments) => (None, arguments),
                    ExprKind::MethodCall(_, receiver, arguments, _) => (Some(receiver), arguments),
                    _ => (None, [].as_slice()),
                };
                let operands = || iter::chain(receiver, arguments);
                if bound(future) {
                    Some((future, Built::Before))
                } else if let Some(passed) = arguments.iter().find(wrapped) {
                    Some((passed, Built::Before))
                } else {
                    (operands().any(&bound) && operands().all(&shared)).then_some((future, Built::Inside))
                }
            };
            let exits = || {
                for_each_expr_without_closures(for_loop.body, |inner| match inner.kind {
                    ExprKind::Break(..) | ExprKind::Ret(_) if !inner.span.from_expansion() => ControlFlow::Break(()),
                    _ => ControlFlow::Continue(()),
                })
                .is_some()
            };
            if let Some((awaited, (future, built))) =
                for_each_expr_without_closures(for_loop.body, |inner| desugar_await(inner).and_then(joined).map_or(ControlFlow::Continue(()), |future| ControlFlow::Break((inner, future))))
                && match built {
                    Built::Before => !task(future),
                    Built::Inside => !exits(),
                }
            {
                let message = match built {
                    Built::Before => "`for` loop awaits futures built before it in sequence",
                    Built::Inside => "`for` loop builds and awaits one independent future per item in sequence",
                };
                span_lint_hir_and_then(cx, SEQUENTIAL_AWAIT, expr.hir_id, for_loop.span.with_hi(for_loop.arg.span.hi()), message, |diag| {
                    diag.span_note(awaited.span, "each future completes here before the next starts");
                    diag.help("await `futures::future::join_all` over the futures, or `futures::stream::iter(..).buffered(n)` to bound concurrency, both keep input order");
                });
            }
        }
    }
}
