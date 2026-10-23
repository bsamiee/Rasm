use core::{fmt::Display, iter, ops::ControlFlow, slice};

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    get_parent_expr, peel_hir_expr_while,
    res::MaybeResPath as _,
    source::snippet,
    visitors::{for_each_expr_without_closures, is_local_used},
};
use rustc_hir::{Block, Expr, ExprField, ExprKind, HirId, MatchSource, Mutability, PatKind, Stmt, StmtKind, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::adjustment::{Adjust, AutoBorrow, AutoBorrowMutability};
use rustc_span::Span;

use crate::{any_expr, expanded_externally, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(INDEPENDENT_QUESTION_MARKS: "`?` on independent results in consecutive `let` bindings a later expression combines, or in sibling operands of one expression");

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
        let joins = |expr: &'tcx Expr<'tcx>, first: HirId, second: HirId| {
            operands(expr).filter_map(read).fold([false; 2], |[left, right], local| [left || local == first, right || local == second]) == [true; 2]
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
            .for_each(|(node, span, first_name, second_name)| report(cx, node, span, first_name, second_name, combined(2)));
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let mutates = |inner: &Expr<'_>| {
            matches!(inner.kind, ExprKind::AddrOf(_, Mutability::Mut, _))
                || typeck.expr_adjustments(inner).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Borrow(AutoBorrow::Ref(AutoBorrowMutability::Mut { .. }))))
        };
        let question = |inner: &'tcx Expr<'tcx>| match inner.kind {
            ExprKind::Match(&Expr { kind: ExprKind::Call(_, [scrutinee]), .. }, _, MatchSource::TryDesugar(_))
                if scrutinee.res_local_id().is_none() && result_parts(cx, typeck.expr_ty(scrutinee)).is_some() =>
                ControlFlow::Break(scrutinee),
            _ => ControlFlow::Continue(()),
        };
        let tried = |combined: &'tcx Expr<'tcx>| {
            operands(combined).filter(|&operand| !any_expr(cx.tcx, operand, mutates)).filter_map(|operand| for_each_expr_without_closures(operand, question)).collect::<Vec<_>>()
        };
        if !expanded_externally(cx, expr.span)
            && let results = tried(expr)
            && let &[first, second, ..] = results.as_slice()
            && get_parent_expr(cx, expr).is_none_or(|parent| tried(parent).len() < 2)
        {
            let help = if let ExprKind::Array(_) = expr.kind { String::from("collect the results into `validated::Validated<Vec<_>, _>` and call `.ok()`") } else { combined(results.len()) };
            report(
                cx,
                expr.hir_id,
                first.span.source_callsite().to(second.span.source_callsite()),
                snippet(cx, first.span.source_callsite(), ".."),
                snippet(cx, second.span.source_callsite(), ".."),
                help,
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn operands<'tcx>(expr: &'tcx Expr<'tcx>) -> impl Iterator<Item = &'tcx Expr<'tcx>> {
    let (head, rest, fields): (Option<&Expr<'_>>, &[Expr<'_>], &[ExprField<'_>]) = match expr.kind {
        ExprKind::Binary(_, left, right) => (Some(left), slice::from_ref(right), &[]),
        ExprKind::Tup(items) | ExprKind::Array(items) | ExprKind::Call(_, items) => (None, items, &[]),
        ExprKind::MethodCall(_, receiver, arguments, _) => (Some(receiver), arguments, &[]),
        ExprKind::Struct(_, fields, _) => (None, &[], fields),
        _ => (None, &[], &[]),
    };
    iter::chain(head, rest).chain(fields.iter().map(|field| field.expr))
}

fn combined(results: usize) -> String {
    match results {
        2..=4 => format!("convert each result with `Validated::from`, combine them with `map{results}`, and call `.ok()`"),
        _ => String::from("convert each result with `Validated::from`, combine them with nested `map2`..`map4` calls, and call `.ok()`"),
    }
}

fn report(cx: &LateContext<'_>, node: HirId, span: Span, first: impl Display, second: impl Display, help: String) {
    span_lint_hir_and_then(cx, INDEPENDENT_QUESTION_MARKS, node, span, format!("`?` on `{first}` returns before independent `{second}` is computed"), |diag| {
        diag.help(help);
    });
}
