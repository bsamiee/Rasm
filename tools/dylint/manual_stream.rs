use core::ops::ControlFlow;

use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    fn_def_id,
    higher::WhileLet,
    paths::PathNS,
    res::MaybeResPath as _,
    source::snippet,
    usage::local_used_after_expr,
    visitors::{for_each_expr_without_closures, is_local_used},
};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, HirId, MatchSource, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::TyCtxt;

use crate::{Access, access, resolve};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_STREAM,
    Warn,
    "`while let` loops reading a tokio receiver, reader, listener, or task set one item at a time, which a tokio-stream wrapper states as a stream"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualStream {
    reads: FxHashMap<DefId, &'static str>,
}

impl_lint_pass!(ManualStream => [MANUAL_STREAM]);

impl ManualStream {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            reads: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("tokio::sync::mpsc::Receiver::recv", "ReceiverStream"),
                    ("tokio::sync::mpsc::UnboundedReceiver::recv", "UnboundedReceiverStream"),
                    ("tokio::sync::broadcast::Receiver::recv", "BroadcastStream"),
                    ("tokio::task::JoinSet::join_next", "JoinSetStream"),
                    ("tokio::io::Lines::next_line", "LinesStream"),
                    ("tokio::io::Split::next_segment", "SplitStream"),
                    ("tokio::fs::ReadDir::next_entry", "ReadDirStream"),
                    ("tokio::net::TcpListener::accept", "TcpListenerStream"),
                    ("tokio::net::UnixListener::accept", "UnixListenerStream"),
                    ("tokio::signal::unix::Signal::recv", "SignalStream"),
                    ("tokio::signal::windows::CtrlC::recv", "CtrlCStream"),
                    ("tokio::signal::windows::CtrlBreak::recv", "CtrlBreakStream"),
                ],
            ),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualStream {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn awaited_read<'tcx>(scrutinee: &'tcx Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
            desugar_await(scrutinee).or_else(|| match scrutinee.kind {
                ExprKind::MethodCall(_, operand, ..) | ExprKind::Match(&Expr { kind: ExprKind::Call(_, [operand]), .. }, _, MatchSource::TryDesugar(_)) => awaited_read(operand),
                _ => None,
            })
        }

        fn inside(cx: &LateContext<'_>, node: HirId, scope: &Expr<'_>) -> bool { cx.tcx.hir_parent_id_iter(node).any(|ancestor| ancestor == scope.hir_id) }

        fn confined<'tcx>(cx: &LateContext<'tcx>, looped: &Expr<'_>, body: &'tcx Expr<'tcx>) -> bool {
            let (mut awaits, mut mutates) = (false, false);
            let escapes = for_each_expr_without_closures(body, |part| match part.kind {
                ExprKind::Ret(_) | ExprKind::Become(_) => ControlFlow::Break(()),
                ExprKind::Break(destination, _) | ExprKind::Continue(destination) if !destination.target_id.is_ok_and(|target| inside(cx, target, body)) => ControlFlow::Break(()),
                _ => {
                    awaits |= matches!(part.kind, ExprKind::Match(_, _, MatchSource::AwaitDesugar));
                    mutates |=
                        !part.span.from_expansion() && part.res_local_id().is_some_and(|binding| !inside(cx, binding, looped)) && matches!(access(cx, part), Access::Mutated | Access::Assigned(_));
                    ControlFlow::Continue(())
                }
            });
            escapes.is_none() && !(awaits && mutates)
        }

        if let Some(looped) = WhileLet::hir(expr)
            && !expr.span.from_expansion()
            && let Some(read) = awaited_read(looped.let_expr)
            && let Some(method) = fn_def_id(cx, read)
            && let Some(&wrapper) = self.reads.get(&method)
            && let ExprKind::MethodCall(_, source, ..) = read.kind
            && let Some(local) = source.res_local_id()
            && !cx.typeck_results().expr_ty(source).is_ref()
            && !is_local_used(cx, looped.if_then, local)
            && !local_used_after_expr(cx, local, expr)
            && confined(cx, expr, looped.if_then)
        {
            span_lint_hir_and_then(
                cx,
                MANUAL_STREAM,
                expr.hir_id,
                looped.let_span,
                format!("`while let` reads `{}` through `{}` one item at a time", snippet(cx, source.span, ".."), cx.tcx.item_name(method)),
                |diag| {
                    diag.help(format!("wrap it in `tokio_stream::wrappers::{wrapper}` and state the loop with stream combinators"));
                },
            );
        }
    }
}
