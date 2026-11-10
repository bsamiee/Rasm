use core::ops::ControlFlow;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::higher::{ForLoop, Range, RangeTy, While};
use clippy_utils::res::MaybeDef as _;
use clippy_utils::sugg::Sugg;
use clippy_utils::visitors::for_each_expr_without_closures;
use clippy_utils::{eq_expr_value, fn_def_id, is_integer_literal, peel_blocks, peel_hir_expr_while, std_or_core, sym};
use rustc_ast::BinOpKind;
use rustc_errors::Applicability;
use rustc_hir::{Block, Expr, ExprKind, LetStmt, LoopSource, Node, Pat, PatKind, PathSegment, Stmt, StmtKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{Ident, Spanned, Symbol, SyntaxContext};

use super::repeatable;
use crate::{associated_trait, expanded_externally, impl_owner, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_RETAIN_BACK: "`pop_front` loops and `drain` calls keeping the last `n` elements of a `VecDeque`");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn deque_call<'hir>(cx: &LateContext<'_>, call: &'hir Expr<'hir>, name: Symbol) -> Option<(&'hir Expr<'hir>, &'hir [Expr<'hir>])> {
            if let Some((deque, arguments)) = match call.kind {
                ExprKind::MethodCall(_, deque, arguments, _) | ExprKind::Call(_, [deque, arguments @ ..]) => Some((deque.peel_borrows(), arguments)),
                _ => None,
            } && let Some(method) = fn_def_id(cx, call)
                && cx.tcx.item_name(method) == name
                && associated_trait(cx.tcx, method).is_none()
                && impl_owner(cx.tcx, method).is_diag_item(cx, sym::VecDeque)
            {
                Some((deque, arguments))
            } else {
                None
            }
        }

        fn trimmed_length<'hir>(cx: &LateContext<'_>, end: &'hir Expr<'hir>) -> Option<(&'hir Expr<'hir>, &'hir Expr<'hir>)> {
            match end.kind {
                ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::saturating_sub, .. }, .. }, measured, [kept], _) => deque_call(cx, measured, sym::len).map(|(deque, _)| (deque, kept)),
                _ => None,
            }
        }

        fn bound<'hir>(cx: &LateContext<'_>, condition: &'hir Expr<'hir>, longer: BinOpKind) -> Option<(&'hir Expr<'hir>, &'hir Expr<'hir>)> {
            let ExprKind::Binary(operator, left, right) = condition.kind else { return None };
            match (longer, operator.node) {
                (BinOpKind::Gt, BinOpKind::Gt) | (BinOpKind::Le, BinOpKind::Le) => deque_call(cx, left, sym::len).map(|(deque, _)| (deque, right)),
                (BinOpKind::Gt, BinOpKind::Lt) | (BinOpKind::Le, BinOpKind::Ge) => deque_call(cx, right, sym::len).map(|(deque, _)| (deque, left)),
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
                StmtKind::Semi(popped) | StmtKind::Let(&LetStmt { pat: &Pat { kind: PatKind::Wild, .. }, init: Some(popped), els: None, .. }) => popped_front(
                    cx,
                    ctxt,
                    peel_hir_expr_while(popped, |popped| match popped.kind {
                        ExprKind::Block(
                            Block {
                                stmts: [Stmt { kind: StmtKind::Semi(inner) | StmtKind::Let(LetStmt { pat: Pat { kind: PatKind::Wild, .. }, init: Some(inner), els: None, .. }), .. }],
                                expr: None,
                                ..
                            },
                            _,
                        ) => Some(inner),
                        _ => None,
                    }),
                    deque,
                ),
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
                    tested @ &Expr { kind: ExprKind::MethodCall(&PathSegment { ident: Ident { name: sym::is_some, .. }, .. }, popped, [], _), .. },
                ) if matches!(looped.body.kind, ExprKind::Block(Block { stmts: [], expr: None, .. }, _))
                    && fn_def_id(cx, tested).is_some_and(|method| associated_trait(cx.tcx, method).is_none() && impl_owner(cx.tcx, method).is_diag_item(cx, sym::Option)) =>
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
        if let Some((deque, kept, terminator)) = kept
            && repeatable(cx, kept)
            && repeatable(cx, deque)
            && !cx.typeck_results().expr_ty(deque).peel_refs().has_significant_drop(cx.tcx, cx.typing_env())
            && for_each_expr_without_closures(kept, |part| if eq_expr_value(cx, ctxt, deque, part) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }).is_none()
        {
            let mut applicability = Applicability::MachineApplicable;
            let borrowed = cx.typeck_results().expr_ty(deque).is_ref();
            let (deque, kept) = (Sugg::hir_with_context(cx, deque, ctxt, "..", &mut applicability), Sugg::hir_with_context(cx, kept, ctxt, "..", &mut applicability));
            let message = format!("`{deque}` drops front elements by hand to keep its last `{kept}`");
            let argument = if borrowed { deque } else { deque.mut_addr() };
            span_lint_hir_and_then(cx, MANUAL_RETAIN_BACK, expr.hir_id, expr.span, message, |diag| {
                let help = "call `VecDeque::retain_back`";
                if std_or_core(cx) == Some("std") {
                    suggest(cx, expr.hir_id, diag, expr.span, help, format!("::std::collections::VecDeque::retain_back({argument}, {kept}){terminator}"), applicability);
                } else {
                    diag.help(help);
                }
            });
        }
    }
}
