use core::iter;

use clippy_utils::{desugar_await, diagnostics::span_lint_hir_and_then, fn_def_id_with_node_args, peel_blocks, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{ClosureKind, CoroutineDesugaring, CoroutineKind, CoroutineSource, Expr, ExprKind, MatchSource, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::{self, Ty};
use rustc_span::Span;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    NESTED_TASK,
    Warn,
    "tasks spawned only to await a handle of the type their spawner returns"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] NestedTask => [NESTED_TASK]);

impl<'tcx> LateLintPass<'tcx> for NestedTask {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn awaited<'tcx>(cx: &LateContext<'tcx>, future: &'tcx Expr<'tcx>) -> Option<(Ty<'tcx>, Span)> {
            if let ExprKind::Closure(closure) = future.kind
                && closure.kind == ClosureKind::Coroutine(CoroutineKind::Desugared(CoroutineDesugaring::Async, CoroutineSource::Block))
            {
                let typeck = cx.tcx.typeck(closure.def_id);
                let value = peel_blocks(cx.tcx.hir_body(closure.body).value);
                let tried = if let ExprKind::Match(branch, _, MatchSource::TryDesugar(_)) = value.kind
                    && let ExprKind::Call(_, [inner]) = branch.kind
                {
                    inner
                } else {
                    value
                };
                let mapped = if let ExprKind::MethodCall(segment, receiver, [_], _) = tried.kind
                    && [sym::map, sym::map_err].contains(&segment.ident.name)
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
            let signature = tcx.fn_sig(spawner).instantiate_identity().skip_normalization().skip_binder();
            let index = signature.inputs().iter().position(|&input| {
                matches!(input.kind(), ty::Param(_))
                    && tcx.param_env(spawner).caller_bounds().any(|clause| clause.as_trait_clause().is_some_and(|bound| bound.def_id() == future && bound.self_ty().skip_binder() == input))
            })?;
            let parameter = *signature.inputs().get(index)?;
            let ty::Adt(handle, held) = signature.output().kind() else { return None };
            held.types()
                .any(|carried| matches!(carried.kind(), ty::Alias(_, alias) if alias.kind == ty::AliasTyKind::Projection { def_id: output } && alias.self_ty() == parameter))
                .then(|| (index, handle.did()))
        }

        let (ExprKind::Call(head, arguments) | ExprKind::MethodCall(_, head, arguments, _)) = expr.kind else { return };
        if !expr.span.from_expansion()
            && let Some((spawner, _)) = fn_def_id_with_node_args(cx, expr)
            && let Some((index, handle)) = spawned(cx, spawner)
            && let Some(future) = iter::once(head).filter(|_| matches!(expr.kind, ExprKind::MethodCall(..))).chain(arguments).nth(index)
            && let Some((awaited, at)) = awaited(cx, future)
            && awaited.peel_refs().ty_adt_def().is_some_and(|adt| adt.did() == handle)
        {
            span_lint_hir_and_then(cx, NESTED_TASK, expr.hir_id, expr.span, format!("spawned task only awaits a `{awaited}` it did not create"), |diag| {
                diag.span_note(at, format!("the task awaits the `{awaited}` here"));
                diag.help(format!("drop the task and await the `{awaited}` where its result is read"));
            });
        }
    }
}
