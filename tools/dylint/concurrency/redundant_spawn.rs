use core::iter;

use clippy_utils::{desugar_await, diagnostics::span_lint_hir_and_then, fn_def_id, peel_blocks, res::MaybeDef as _, source::snippet, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{ClosureKind, CoroutineDesugaring, CoroutineKind, CoroutineSource, Expr, ExprKind, MatchSource, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty};
use rustc_span::Span;

use crate::{bounds_of, expanded_externally, item_path, signature};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_SPAWN: "tasks spawned only to await a handle of the type their spawn function returns");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn awaited<'tcx>(cx: &LateContext<'tcx>, future: &'tcx Expr<'tcx>) -> Option<(Ty<'tcx>, Span)> {
            if let ExprKind::Closure(closure) = future.kind
                && closure.kind == ClosureKind::Coroutine(CoroutineKind::Desugared(CoroutineDesugaring::Async, CoroutineSource::Block))
            {
                let typeck = cx.tcx.typeck(closure.def_id);
                let value = peel_blocks(&cx.tcx.hir_body(closure.body).value);
                let tried = if let ExprKind::Match(branch, _, MatchSource::TryDesugar(_)) = value.kind
                    && let ExprKind::Call(_, [inner]) = branch.kind
                {
                    inner
                } else {
                    value
                };
                let mapped = if let ExprKind::MethodCall(segment, receiver, [_], _) = tried.kind
                    && matches!(segment.ident.name, sym::map | sym::map_err)
                {
                    receiver
                } else {
                    tried
                };
                desugar_await(mapped).map(|operand| (typeck.expr_ty(operand), operand.span))
            } else {
                Some((cx.typeck_results().expr_ty(future), future.span))
            }
        }

        fn spawned(cx: &LateContext<'_>, spawner: DefId) -> Option<(usize, DefId)> {
            let tcx = cx.tcx;
            let (future, output) = (tcx.lang_items().future_trait()?, tcx.lang_items().get(LangItem::FutureOutput)?);
            let declared = signature(tcx, spawner);
            let index = declared.inputs().iter().position(|&input| matches!(input.kind(), ty::Param(_)) && bounds_of(tcx, spawner, input).any(|bound| bound.def_id() == future))?;
            let parameter = *declared.inputs().get(index)?;
            let ty::Adt(handle, held) = declared.output().kind() else { return None };
            held.types()
                .any(|carried| matches!(carried.kind(), ty::Alias(_, alias) if alias.kind == ty::AliasTyKind::Projection { def_id: output } && alias.self_ty() == parameter))
                .then(|| (index, handle.did()))
        }

        let (ExprKind::Call(head, arguments) | ExprKind::MethodCall(_, head, arguments, _)) = expr.kind else { return };
        if !expanded_externally(cx, expr.span)
            && let Some(spawner) = fn_def_id(cx, expr)
            && let Some((index, handle)) = spawned(cx, spawner)
            && let Some(future) = iter::once(head).filter(|_| matches!(expr.kind, ExprKind::MethodCall(..))).chain(arguments).nth(index)
            && let Some((awaited, at)) = awaited(cx, future)
            && awaited.peel_refs().opt_def_id() == Some(handle)
        {
            span_lint_hir_and_then(cx, REDUNDANT_SPAWN, expr.hir_id, expr.span, format!("`{}` spawns a task that only awaits `{awaited}`", item_path(cx, spawner)), |diag| {
                diag.span_note(at, "awaited here");
                diag.help(format!("delete the call and await `{}` where its result is read", snippet(cx, at, "..")));
            });
        }
    }
}
