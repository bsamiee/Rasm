use core::{iter, ops::ControlFlow, slice};

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    peel_hir_expr_while,
    res::MaybeResPath as _,
    visitors::{for_each_expr_without_closures, is_local_used},
};
use rustc_hir::{Block, Expr, ExprField, ExprKind, HirId, MatchSource, PatKind, Stmt, StmtKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(INDEPENDENT_QUESTION_MARKS: "consecutive `let` bindings applying `?` where the second reads no value of the first and a later expression combines both");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_block(&mut self, cx: &LateContext<'tcx>, block: &'tcx Block<'tcx>) {
        let tried = |statement: &Stmt<'tcx>| {
            if let StmtKind::Let(local) = statement.kind
                && let PatKind::Binding(_, binding, ident, None) = local.pat.kind
                && let Some(init) = local.init
                && let ExprKind::Match(_, _, MatchSource::TryDesugar(_)) = init.kind
                && !expanded_externally(cx, local.span)
            {
                Some((local.hir_id, binding, ident, init))
            } else {
                None
            }
        };
        let read = |operand: &Expr<'_>| {
            peel_hir_expr_while(operand, |inner| match inner.kind {
                ExprKind::AddrOf(_, _, read) | ExprKind::Field(read, _) | ExprKind::Unary(UnOp::Deref, read) | ExprKind::MethodCall(_, read, ..) => Some(read),
                _ => None,
            })
            .res_local_id()
        };
        let joins = |expr: &Expr<'_>, first: HirId, second: HirId| {
            let (head, rest, fields): (Option<&Expr<'_>>, &[Expr<'_>], &[ExprField<'_>]) = match expr.kind {
                ExprKind::Binary(_, left, right) => (Some(left), slice::from_ref(right), &[]),
                ExprKind::Tup(items) | ExprKind::Array(items) | ExprKind::Call(_, items) => (None, items, &[]),
                ExprKind::MethodCall(_, receiver, arguments, _) => (Some(receiver), arguments, &[]),
                ExprKind::Struct(_, fields, _) => (None, &[], fields),
                _ => (None, &[], &[]),
            };
            iter::chain(head, rest).chain(fields.iter().map(|field| field.expr)).filter_map(read).fold([false; 2], |[left, right], local| [left || local == first, right || local == second])
                == [true; 2]
        };
        iter::successors(Some(block.stmts), |statements| statements.split_first().map(|(_, rest)| rest))
            .filter_map(|statements| {
                let [first, second, rest @ ..] = statements else { return None };
                let ((node, first_binding, first_name, _), (_, second_binding, second_name, second_init)) = (tried(first)?, tried(second)?);
                (!is_local_used(cx, second_init, first_binding)
                    && rest
                        .iter()
                        .filter_map(|statement| match statement.kind {
                            StmtKind::Let(local) => local.init,
                            StmtKind::Expr(expr) | StmtKind::Semi(expr) => Some(expr),
                            StmtKind::Item(_) => None,
                        })
                        .chain(block.expr)
                        .any(|expr| for_each_expr_without_closures(expr, |part| if joins(part, first_binding, second_binding) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }).is_some()))
                .then(|| (node, first.span.to(second.span), first_name, second_name))
            })
            .for_each(|(node, span, first_name, second_name)| {
                span_lint_hir_and_then(cx, INDEPENDENT_QUESTION_MARKS, node, span, format!("`?` on `{first_name}` returns before independent `{second_name}` is computed"), |diag| {
                    diag.help("evaluate both and accumulate their errors through `validated::Validated` before combining");
                });
            });
    }
}
