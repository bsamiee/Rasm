use core::{iter, ops::ControlFlow};

use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    higher::ForLoop,
    paths::PathNS,
    peel_hir_expr_while,
    res::{MaybeDef as _, MaybeResPath as _},
    source::snippet,
    sym,
    ty::implements_trait,
    visitors::for_each_expr_without_closures,
};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, Mutability, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    TyCtxt,
    adjustment::{Adjust, AutoBorrow, AutoBorrowMutability},
};

use crate::{expanded_externally, paths};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Built {
    Iterated,
    Before,
    Inside,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    handles: FxHashSet<DefId>,
}

lint!(Pass; SEQUENTIAL_AWAIT: "`for` loops awaiting one independent future per item in sequence");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { handles: paths(tcx, PathNS::Type, &["tokio::task::JoinHandle", "tokio_util::task::AbortOnDropHandle", "tokio::sync::oneshot::Receiver", "futures::channel::oneshot::Receiver"]) }
    }
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
            let wrapped = |argument: &Expr<'tcx>| bound(argument) && into_future.is_some_and(|into_future| implements_trait(cx, typeck.expr_ty(argument), into_future, &[]));
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
                    typeck.expr_ty(future).opt_def_id().is_none_or(|held| !self.handles.contains(&held)).then(|| future.res_local_id().map_or(Built::Before, |_| Built::Iterated))
                } else if arguments.iter().any(wrapped) {
                    Some(Built::Before)
                } else {
                    (operands().any(&bound) && operands().all(&shared)).then_some(Built::Inside)
                }
            };
            let exits = || {
                for_each_expr_without_closures(for_loop.body, |inner| match inner.kind {
                    ExprKind::Break(..) | ExprKind::Ret(_) if !inner.span.from_expansion() => ControlFlow::Break(()),
                    _ => ControlFlow::Continue(()),
                })
                .is_some()
            };
            if let Some((awaited, built)) =
                for_each_expr_without_closures(for_loop.body, |inner| desugar_await(inner).and_then(joined).map_or(ControlFlow::Continue(()), |built| ControlFlow::Break((inner, built))))
                && (matches!(built, Built::Iterated | Built::Before) || !exits())
            {
                let items = snippet(cx, for_loop.arg.span, "..");
                let message = match built {
                    Built::Iterated | Built::Before => "`for` loop awaits futures built before it in sequence",
                    Built::Inside => "`for` loop builds and awaits one independent future per item in sequence",
                };
                let help = match built {
                    Built::Iterated => format!("await `futures::future::join_all({items})`"),
                    Built::Before => format!("await `futures::future::join_all({items}.into_iter().map(..))`"),
                    Built::Inside => format!("await `futures::stream::iter({items}).map(..).buffered(n).collect::<Vec<_>>()`"),
                };
                span_lint_hir_and_then(cx, SEQUENTIAL_AWAIT, expr.hir_id, for_loop.span.with_hi(for_loop.arg.span.hi()), message, |diag| {
                    diag.span_note(awaited.span, "each future completes here before the next starts");
                    diag.help(help);
                });
            }
        }
    }
}
