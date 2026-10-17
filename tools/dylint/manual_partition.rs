use core::{iter, ops::ControlFlow};

use clippy_utils::{SpanlessEq, diagnostics::span_lint_hir_and_then, fn_def_id, peel_blocks, res::MaybeResPath as _, source::snippet, sym, visitors::for_each_expr};
use rustc_ast::BinOpKind;
use rustc_hir::{
    Body, Expr, ExprKind, HirId, Mutability, Node, UnOp,
    def::{DefKind, Res},
    def_id::DefId,
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::adjustment::{Adjust, AutoBorrow};
use rustc_span::{Span, Symbol};

use crate::{iterator_method, parameter_bindings, receiving_call};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_PARTITION,
    Warn,
    "two iterator passes over one collection splitting it on a predicate and its negation, which `Iterator::partition` states in one pass"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Reach {
    Drained,
    Stopped,
    Unread,
}

enum Predicate<'tcx> {
    Closure(Vec<HirId>, &'tcx Expr<'tcx>),
    Path(DefId),
}

struct Split<'tcx> {
    site: &'tcx Expr<'tcx>,
    span: Span,
    adapter: Symbol,
    source: &'tcx Expr<'tcx>,
    negated: bool,
    predicate: Predicate<'tcx>,
    reach: Reach,
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] ManualPartition => [MANUAL_PARTITION]);

impl<'tcx> LateLintPass<'tcx> for ManualPartition {
    fn check_body(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) {
        fn split<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<Split<'tcx>> {
            const ADAPTERS: [Symbol; 11] = [sym::map, sym::cloned, sym::copied, sym::rev, sym::enumerate, sym::inspect, sym::filter, sym::filter_map, sym::flat_map, sym::flatten, sym::chain];
            const DRAINS: [Symbol; 16] = [
                sym::collect,
                sym::count,
                sym::for_each,
                sym::fold,
                sym::sum,
                sym::product,
                sym::unzip,
                sym::last,
                sym::max,
                sym::min,
                sym::max_by,
                sym::min_by,
                sym::max_by_key,
                sym::min_by_key,
                sym::partition,
                sym::reduce,
            ];
            const STOPS: [Symbol; 12] =
                [sym::next, sym::next_back, sym::nth, sym::any, sym::all, sym::find, sym::find_map, sym::position, sym::take, sym::take_while, sym::map_while, sym::try_for_each];
            let ExprKind::MethodCall(segment, source, [predicate], _) = expr.kind else { return None };
            let (adapter, typeck) = (segment.ident.name, cx.typeck_results());
            let root = iter::successors(Some(source), |step| if let ExprKind::MethodCall(_, receiver, ..) = step.kind { Some(receiver) } else { None }).last()?;
            let base = iter::successors(Some(root), |place| if let ExprKind::Field(base, _) = place.kind { Some(base) } else { None }).last()?;
            let stable = matches!(base.kind, ExprKind::Path(_))
                && !typeck.expr_adjustments(root).iter().any(|adjustment| matches!(adjustment.kind, Adjust::Borrow(AutoBorrow::Ref(borrow)) if Mutability::from(borrow).is_mut()))
                && typeck.expr_ty(root).peel_refs().is_freeze(cx.tcx, cx.typing_env());
            if expr.span.from_expansion() || ![sym::filter, sym::find].contains(&adapter) || !iterator_method(cx, expr) || !stable {
                return None;
            }
            let consumer = iter::successors(Some(expr), |&current| receiving_call(cx, current).map(|(call, _)| call).filter(|call| iterator_method(cx, call)))
                .skip(1)
                .filter_map(Expr::method_ident)
                .map(|step| step.name)
                .find(|name| !ADAPTERS.contains(name));
            let reach = match consumer {
                _ if adapter == sym::find => Reach::Stopped,
                Some(name) if DRAINS.contains(&name) => Reach::Drained,
                Some(name) if STOPS.contains(&name) => Reach::Stopped,
                Some(_) | None => Reach::Unread,
            };
            let (negated, predicate) = match predicate.kind {
                ExprKind::Closure(closure) => {
                    let value = peel_blocks(cx.tcx.hir_body(closure.body).value);
                    match value.kind {
                        ExprKind::Unary(UnOp::Not, kept) => (true, Predicate::Closure(parameter_bindings(cx.tcx, closure.body), kept)),
                        _ => (false, Predicate::Closure(parameter_bindings(cx.tcx, closure.body), value)),
                    }
                }
                ExprKind::Path(qpath) => match cx.qpath_res(&qpath, predicate.hir_id) {
                    Res::Def(DefKind::Fn | DefKind::AssocFn, function) => (false, Predicate::Path(function)),
                    _ => return None,
                },
                _ => return None,
            };
            Some(Split { site: expr, span: segment.ident.span.to(expr.span.shrink_to_hi()), adapter, source, negated, predicate, reach })
        }

        fn complementary<'tcx>(cx: &LateContext<'tcx>, left: &Split<'tcx>, right: &Split<'tcx>) -> bool {
            fn applied(cx: &LateContext<'_>, function: DefId, bindings: &[HirId], value: &Expr<'_>) -> bool {
                let operand = match value.kind {
                    ExprKind::MethodCall(_, receiver, [], _) => receiver,
                    ExprKind::Call(_, [argument]) => argument,
                    _ => return false,
                };
                let peeled = iter::successors(Some(operand), |&step| if let ExprKind::Unary(UnOp::Deref, inner) | ExprKind::AddrOf(_, _, inner) = step.kind { Some(inner) } else { None })
                    .last()
                    .unwrap_or(operand);
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
                && match (left.reach, right.reach) {
                    (Reach::Stopped, Reach::Stopped) => false,
                    (Reach::Stopped, other) | (other, Reach::Stopped) => other == Reach::Drained,
                    (Reach::Drained | Reach::Unread, Reach::Drained | Reach::Unread) => true,
                }
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
        let _: Option<()> = for_each_expr(cx.tcx, body.value, |expr| {
            splits.extend(split(cx, expr));
            ControlFlow::Continue(())
        });
        let pairs = splits.iter().enumerate().flat_map(|(index, left)| splits.iter().skip(index.strict_add(1)).map(move |right| (left, right)));
        for (left, right) in pairs.filter(|&(left, right)| complementary(cx, left, right)) {
            span_lint_hir_and_then(
                cx,
                MANUAL_PARTITION,
                right.site.hir_id,
                right.span,
                format!("`{}` and `{}` split `{}` on one predicate and its negation in two passes", left.adapter, right.adapter, snippet(cx, right.source.span, "..")),
                |diag| {
                    diag.span_note(left.span, "the other pass splits it here");
                    diag.help("split it once with `Iterator::partition` and read each side from its half");
                },
            );
        }
    }
}
