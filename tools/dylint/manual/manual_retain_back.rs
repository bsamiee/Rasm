use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    eq_expr_value, fn_def_id,
    higher::{ForLoop, Range, RangeTy, While},
    is_integer_literal, path_to_local_with_projections, peel_blocks,
    res::MaybeDef as _,
    sugg::Sugg,
    sym,
};
use rustc_ast::BinOpKind;
use rustc_errors::Applicability;
use rustc_hir::{Block, Expr, ExprKind, LetStmt, LoopSource, Node, Pat, PatKind, PathSegment, Stmt, StmtKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{Ident, Spanned, Symbol, SyntaxContext};

use crate::{expanded_externally, impl_owner, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_RETAIN_BACK: "`pop_front` loops and `drain(..len - n)` calls keeping the last `n` elements of a `VecDeque`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn deque_call<'hir>(cx: &LateContext<'_>, call: &'hir Expr<'hir>, name: Symbol) -> Option<(&'hir Expr<'hir>, &'hir [Expr<'hir>])> {
            if let ExprKind::MethodCall(_, deque, arguments, _) = call.kind
                && let Some(method) = fn_def_id(cx, call)
                && cx.tcx.item_name(method) == name
                && impl_owner(cx.tcx, method).is_diag_item(cx, sym::VecDeque)
            {
                Some((deque, arguments))
            } else {
                None
            }
        }

        fn trimmed_length<'hir>(cx: &LateContext<'_>, end: &'hir Expr<'hir>) -> Option<(&'hir Expr<'hir>, &'hir Expr<'hir>)> {
            match end.kind {
                ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::saturating_sub, .. }, .. }, measured, [kept], _)
                | ExprKind::Binary(Spanned { node: BinOpKind::Sub, .. }, measured, kept) => deque_call(cx, measured, sym::len).map(|(deque, _)| (deque, kept)),
                _ => None,
            }
        }

        fn bound<'hir>(cx: &LateContext<'_>, condition: &'hir Expr<'hir>, longer: BinOpKind) -> Option<(&'hir Expr<'hir>, &'hir Expr<'hir>)> {
            let shorter = if longer == BinOpKind::Gt { BinOpKind::Lt } else { BinOpKind::Ge };
            let ExprKind::Binary(operator, left, right) = condition.kind else { return None };
            match operator.node {
                operator if operator == longer => deque_call(cx, left, sym::len).map(|(deque, _)| (deque, right)),
                operator if operator == shorter => deque_call(cx, right, sym::len).map(|(deque, _)| (deque, left)),
                _ => None,
            }
        }

        fn popped_front(cx: &LateContext<'_>, ctxt: SyntaxContext, popped: &Expr<'_>, deque: &Expr<'_>) -> bool {
            let popped = match popped.kind {
                ExprKind::Call(_, [argument]) if fn_def_id(cx, popped).is_diag_item(cx, sym::mem_drop) => argument,
                _ => popped,
            };
            deque_call(cx, popped, sym::pop_front).is_some_and(|(popped_deque, _)| eq_expr_value(cx, ctxt, deque, popped_deque))
        }

        fn popping_statement(cx: &LateContext<'_>, ctxt: SyntaxContext, statement: &Stmt<'_>, deque: &Expr<'_>) -> bool {
            match statement.kind {
                StmtKind::Semi(popped) | StmtKind::Let(&LetStmt { pat: &Pat { kind: PatKind::Wild, .. }, init: Some(popped), els: None, .. }) =>
                    popped_front(cx, ctxt, popped, deque) || popping_body(cx, ctxt, popped, deque),
                StmtKind::Expr(_) | StmtKind::Let(_) | StmtKind::Item(_) => false,
            }
        }

        fn popping_body(cx: &LateContext<'_>, ctxt: SyntaxContext, body: &Expr<'_>, deque: &Expr<'_>) -> bool {
            matches!(body.kind, ExprKind::Block(Block { stmts: [statement], expr: None, .. }, _) if popping_statement(cx, ctxt, statement, deque))
        }

        let ctxt = expr.span.source_callsite().ctxt();
        let statement = matches!(cx.tcx.parent_hir_node(expr.hir_id), Node::Stmt(_) | Node::Block(_));
        let kept = if let Some(looped) = While::hir(expr)
            && statement
            && !expanded_externally(cx, looped.body.span)
        {
            match looped.condition.peel_drop_temps().kind {
                ExprKind::Binary(
                    Spanned { node: BinOpKind::And, .. },
                    measured,
                    &Expr { kind: ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::is_some, .. }, .. }, popped, [], _), .. },
                ) if matches!(looped.body.kind, ExprKind::Block(Block { stmts: [], expr: None, .. }, _)) =>
                    bound(cx, measured, BinOpKind::Gt).filter(|&(deque, _)| popped_front(cx, ctxt, popped, deque)),
                _ => bound(cx, looped.condition.peel_drop_temps(), BinOpKind::Gt).filter(|&(deque, _)| popping_body(cx, ctxt, looped.body, deque)),
            }
            .map(|(deque, kept)| (deque, kept, ";"))
        } else if let ExprKind::Loop(&Block { stmts: [guard, popped], expr: None, .. }, _, LoopSource::Loop, _) = expr.kind
            && statement
            && !expanded_externally(cx, expr.span)
            && let StmtKind::Semi(guard) | StmtKind::Expr(guard) = guard.kind
            && let ExprKind::If(condition, broken, None) = guard.kind
            && matches!(peel_blocks(broken).kind, ExprKind::Break(destination, None) if destination.label.is_none())
            && let Some((deque, kept)) = bound(cx, condition.peel_drop_temps(), BinOpKind::Le)
            && popping_statement(cx, ctxt, popped, deque)
        {
            Some((deque, kept, ";"))
        } else if let Some(looped) = ForLoop::hir(expr)
            && statement
            && matches!(looped.pat.kind, PatKind::Wild)
            && !expanded_externally(cx, looped.span)
            && let Some(Range { ty: RangeTy::OpsRange, start: Some(start), end: Some(end), .. }) = Range::hir(cx, looped.arg)
            && let Some((deque, kept)) = if is_integer_literal(start, 0) { trimmed_length(cx, end) } else { deque_call(cx, end, sym::len).map(|(deque, _)| (deque, start)) }
            && popping_body(cx, ctxt, looped.body, deque)
        {
            Some((deque, kept, ";"))
        } else if let Some((deque, [range])) = deque_call(cx, expr, sym::drain)
            && !expanded_externally(cx, expr.span)
            && matches!(cx.tcx.parent_hir_node(expr.hir_id), Node::Stmt(Stmt { kind: StmtKind::Semi(_), .. }))
            && let Some(Range { ty: RangeTy::OpsTo, start: None, end: Some(end), .. }) = Range::hir(cx, range)
            && let Some((measured, kept)) = trimmed_length(cx, end)
            && eq_expr_value(cx, ctxt, deque, measured)
        {
            Some((deque, kept, ""))
        } else {
            None
        };
        if let Some((deque, kept, terminator)) = kept {
            let mut applicability =
                if matches!(kept.kind, ExprKind::Lit(_)) || path_to_local_with_projections(kept).is_some() { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect };
            let (deque, kept) = (Sugg::hir_with_context(cx, deque, ctxt, "..", &mut applicability).maybe_paren(), Sugg::hir_with_context(cx, kept, ctxt, "..", &mut applicability));
            span_lint_hir_and_then(cx, MANUAL_RETAIN_BACK, expr.hir_id, expr.span, format!("`{deque}` drops front elements by hand to keep its last `{kept}`"), |diag| {
                suggest(cx, expr.hir_id, diag, expr.span, "call `VecDeque::retain_back`", format!("{deque}.retain_back({kept}){terminator}"), applicability);
            });
        }
    }
}
