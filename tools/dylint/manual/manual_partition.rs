use core::{iter, ops::ControlFlow};

use clippy_utils::{SpanlessEq, diagnostics::span_lint_hir_and_then, fn_def_id, peel_blocks, peel_hir_expr_while, res::MaybeResPath as _, source::snippet, sym, visitors::for_each_expr};
use itertools::Itertools as _;
use rustc_ast::BinOpKind;
use rustc_hir::{Body, Expr, ExprKind, HirId, Mutability, Node, UnOp, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::adjustment::{Adjust, AutoBorrow};
use rustc_span::Span;

use super::iterator_method;
use crate::{expanded_externally, fn_item_value, method_call_on, parameter_bindings};

// --- [MODELS] --------------------------------------------------------------------------

enum Predicate<'tcx> {
    Closure(Vec<HirId>, &'tcx Expr<'tcx>),
    Path(DefId),
}

struct Split<'tcx> {
    site: &'tcx Expr<'tcx>,
    span: Span,
    source: &'tcx Expr<'tcx>,
    negated: bool,
    predicate: Predicate<'tcx>,
}

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_PARTITION: "two `filter` passes collecting one collection split on a predicate and its negation");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_body(&mut self, cx: &LateContext<'tcx>, body: &'tcx Body<'tcx>) {
        fn split<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<Split<'tcx>> {
            let typeck = cx.typeck_results();
            if let ExprKind::MethodCall(segment, source, [predicate], _) = expr.kind
                && segment.ident.name == sym::filter
                && !expanded_externally(cx, expr.span)
                && iterator_method(cx, expr)
                && let root = peel_hir_expr_while(source, |step| if let ExprKind::MethodCall(_, receiver, ..) = step.kind { Some(receiver) } else { None })
                && let ExprKind::Path(_) = peel_hir_expr_while(root, |place| if let ExprKind::Field(base, _) = place.kind { Some(base) } else { None }).kind
                && !typeck.expr_adjustments(root).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Borrow(AutoBorrow::Ref(borrow)) if Mutability::from(borrow).is_mut()))
                && typeck.expr_ty(root).peel_refs().is_freeze(cx.tcx, cx.typing_env())
                && let Some((negated, predicate)) = match predicate.kind {
                    ExprKind::Closure(closure) => {
                        let value = peel_blocks(&cx.tcx.hir_body(closure.body).value);
                        let (negated, kept) = if let ExprKind::Unary(UnOp::Not, kept) = value.kind { (true, kept) } else { (false, value) };
                        Some((negated, Predicate::Closure(parameter_bindings(cx.tcx, closure.body), kept)))
                    }
                    _ => fn_item_value(cx, predicate).map(|function| (false, Predicate::Path(function))),
                }
                && iter::successors(Some(expr), |&current| method_call_on(cx, current).map(|(call, _)| call).filter(|call| iterator_method(cx, call)))
                    .skip(1)
                    .filter_map(Expr::method_ident)
                    .map(|step| step.name)
                    .find(|&name| {
                        !matches!(name, sym::map | sym::cloned | sym::copied | sym::rev | sym::enumerate | sym::inspect | sym::filter | sym::filter_map | sym::flat_map | sym::flatten | sym::chain)
                    })
                    == Some(sym::collect)
            {
                Some(Split { site: expr, span: segment.ident.span.to(expr.span.shrink_to_hi()), source, negated, predicate })
            } else {
                None
            }
        }

        fn complementary<'tcx>(cx: &LateContext<'tcx>, left: &Split<'tcx>, right: &Split<'tcx>) -> bool {
            fn applied(cx: &LateContext<'_>, function: DefId, bindings: &[HirId], value: &Expr<'_>) -> bool {
                let (ExprKind::MethodCall(_, operand, [], _) | ExprKind::Call(_, [operand])) = value.kind else { return false };
                let peeled = peel_hir_expr_while(operand, |step| if let ExprKind::Unary(UnOp::Deref, inner) | ExprKind::AddrOf(_, _, inner) = step.kind { Some(inner) } else { None });
                fn_def_id(cx, value) == Some(function) && matches!(bindings, [binding] if peeled.res_local_id() == Some(*binding))
            }

            fn together(cx: &LateContext<'_>, left: HirId, right: HirId) -> bool {
                let tcx = cx.tcx;
                let ancestors = |node: HirId| iter::once(node).chain(tcx.hir_parent_id_iter(node));
                let unconditional = |node: HirId, common: HirId| {
                    ancestors(node).take_while(|&step| step != common).all(|child| match tcx.parent_hir_node(child) {
                        Node::Arm(_) => false,
                        Node::Expr(&Expr { kind: ExprKind::If(condition, ..), .. }) => condition.hir_id == child,
                        Node::Expr(&Expr { kind: ExprKind::Binary(operator, _, rhs), .. }) if matches!(operator.node, BinOpKind::And | BinOpKind::Or) => rhs.hir_id != child,
                        Node::LetStmt(local) => local.els.is_none_or(|otherwise| otherwise.hir_id != child),
                        _ => true,
                    })
                };
                ancestors(left).find(|&ancestor| ancestors(right).any(|other| other == ancestor)).is_some_and(|common| unconditional(left, common) && unconditional(right, common))
            }

            let ctxt = left.site.span.ctxt();
            left.negated != right.negated
                && SpanlessEq::new(cx).eq_expr(ctxt, left.source, right.source)
                && match (&left.predicate, &right.predicate) {
                    (Predicate::Closure(left_bindings, left_value), Predicate::Closure(right_bindings, right_value)) =>
                        left_bindings.len() == right_bindings.len() && {
                            let mut spanless = SpanlessEq::new(cx);
                            let mut inter = spanless.inter_expr(ctxt);
                            inter.locals.extend(iter::zip(left_bindings.iter().copied(), right_bindings.iter().copied()));
                            inter.eq_expr(left_value, right_value)
                        },
                    (&Predicate::Path(function), Predicate::Closure(bindings, value)) | (Predicate::Closure(bindings, value), &Predicate::Path(function)) => applied(cx, function, bindings, value),
                    (Predicate::Path(_), Predicate::Path(_)) => false,
                }
                && together(cx, left.site.hir_id, right.site.hir_id)
        }

        if cx.tcx.is_typeck_child(cx.tcx.hir_body_owner_def_id(body.id()).to_def_id()) {
            return;
        }
        let mut splits = Vec::new();
        let _: Option<()> = for_each_expr(cx.tcx, &body.value, |expr| {
            splits.extend(split(cx, expr));
            ControlFlow::Continue(())
        });
        for [left, right] in splits.iter().array_combinations::<2>().filter(|&[left, right]| complementary(cx, left, right)) {
            span_lint_hir_and_then(
                cx,
                MANUAL_PARTITION,
                right.site.hir_id,
                right.span,
                format!("two `filter` passes split `{}` on one predicate and its negation", snippet(cx, right.source.span, "..")),
                |diag| {
                    diag.span_note(left.span, "other pass here");
                    diag.help("split once with `Iterator::partition` and read each side from its half");
                },
            );
        }
    }
}
