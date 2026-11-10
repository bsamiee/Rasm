use core::iter;
use core::ops::ControlFlow;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::macros::{FormatArgsStorage, PanicCall, find_assert_args, matching_root_macro_call, root_macro_call_first_node};
use clippy_utils::res::MaybeDef as _;
use clippy_utils::source::snippet;
use clippy_utils::ty::{has_debug_impl, is_copy};
use clippy_utils::visitors::for_each_expr_without_closures;
use clippy_utils::{eq_expr_value, fn_def_id, get_parent_expr, sym};
use itertools::{Either, Itertools as _};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::def::Res;
use rustc_hir::{Arm, BinOpKind, ByRef, Expr, ExprKind, Pat, PatKind, QPath, UnOp};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_middle::ty::adjustment::{Adjust, DerefAdjustKind};

use super::has_temporary_drop;
use crate::{expanded_externally, impl_owner, in_test};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    format_args: FormatArgsStorage,
}

lint! {
    Pass;
    ASSERT_MATCHES: "asserting a pattern through a boolean when the subject supports native pattern assertion",
    CHAINED_MATCHES: "repeated native pattern tests over the same stable subject",
}

impl Pass {
    pub(crate) const fn new(_: TyCtxt<'_>, format_args: FormatArgsStorage) -> Self { Self { format_args } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn matched<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>, negated: bool) -> Option<(&'tcx Expr<'tcx>, &'tcx Arm<'tcx>)> {
            let expr = match (negated, expr.peel_drop_temps().kind) {
                (true, ExprKind::Unary(UnOp::Not, inner)) => inner.peel_drop_temps(),
                (false, _) => expr.peel_drop_temps(),
                (true, _) => return None,
            };
            if matching_root_macro_call(cx, expr.span, sym::matches_macro).is_some()
                && let ExprKind::Match(value, [accepted, _], _) = expr.kind
            {
                Some((value.peel_drop_temps(), accepted))
            } else {
                None
            }
        }

        fn stable<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> bool {
            let typeck = cx.typeck_results();
            for_each_expr_without_closures(expr, |part| {
                let native = match part.kind {
                    ExprKind::Path(QPath::Resolved(_, path)) => matches!(path.res, Res::Local(_)),
                    ExprKind::Field(..) | ExprKind::Index(..) | ExprKind::Lit(_) => true,
                    ExprKind::Unary(UnOp::Deref, inner) => typeck.expr_ty(inner).is_ref(),
                    _ => false,
                };
                if native
                    && typeck.type_dependent_def_id(part.hir_id).is_none()
                    && typeck.expr_adjustments(part).iter().all(|adjustment| matches!(adjustment.kind, Adjust::Deref(DerefAdjustKind::Builtin)))
                {
                    ControlFlow::Continue(())
                } else {
                    ControlFlow::Break(())
                }
            })
            .is_none()
        }

        let typeck = cx.typeck_results();
        if let Some(call) = root_macro_call_first_node(cx, expr)
            && let Some(assertion @ (sym::assert_macro | sym::debug_assert_macro)) = cx.tcx.get_diagnostic_name(call.def_id)
            && !expanded_externally(cx, call.span)
            && in_test(cx.tcx, expr.hir_id)
            && !cx.tcx.hir_is_inside_const_context(expr.hir_id)
            && let Some((condition, message)) = find_assert_args(cx, expr, call.expn)
            && match message {
                PanicCall::DefaultMessage => true,
                PanicCall::Format(message) => self.format_args.get(cx, message, call.expn).is_some_and(|arguments| arguments.arguments.all_args().is_empty()),
                PanicCall::Str2015(message) => matches!(message.kind, ExprKind::Lit(literal) if matches!(literal.node, rustc_ast::LitKind::Str(..))),
                PanicCall::Display(_) => false,
            }
            && let Some((value, pattern)) = matched(cx, condition, false).map(|(value, arm)| (value, Either::Left(arm))).or_else(|| {
                let (ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver])) = condition.kind else { return None };
                let method = fn_def_id(cx, condition)?;
                let name = cx.tcx.item_name(method);
                (matches!(name, sym::is_some | sym::is_none)
                    && cx.tcx.inherent_impl_of_assoc(method).is_some()
                    && impl_owner(cx.tcx, method).is_lang_item(cx, LangItem::Option)
                    && typeck.expr_ty(receiver).peel_refs().is_lang_item(cx, LangItem::Option))
                .then_some((receiver, Either::Right(name == sym::is_some)))
            })
            && has_debug_impl(cx, typeck.expr_ty(value))
            && for_each_expr_without_closures(value, |part| match &part.kind {
                ExprKind::Field(receiver, _) if typeck.expr_ty_adjusted(receiver).peel_refs().ty_adt_def().is_some_and(|owner| owner.repr().packed()) => ControlFlow::Break(()),
                ExprKind::Path(path) if cx.qpath_res(path, part.hir_id).opt_def_id().is_some_and(|item| cx.tcx.is_mutable_static(item)) => ControlFlow::Break(()),
                _ => ControlFlow::Continue(()),
            })
            .is_none()
            && !has_temporary_drop(cx, condition)
        {
            let target = if assertion == sym::debug_assert_macro { "core::debug_assert_matches" } else { "core::assert_matches" };
            span_lint_hir_and_then(cx, ASSERT_MATCHES, expr.hir_id, call.span, "assertion tests a pattern through a boolean", |diag| {
                let pattern = match pattern {
                    Either::Left(arm) => format!("{}{}", snippet(cx, arm.pat.span, "pattern"), arm.guard.map_or_default(|guard| format!(" if {}", snippet(cx, guard.span, "condition")))),
                    Either::Right(true) => String::from("core::option::Option::Some(_)"),
                    Either::Right(false) => String::from("core::option::Option::None"),
                };
                diag.help(format!("assert `{pattern}` through `{target}!`, retaining custom messages"));
            });
        }

        if let ExprKind::Binary(chain_operation, _, last_test) = expr.kind
            && let negated = match chain_operation.node {
                BinOpKind::Or | BinOpKind::BitOr => false,
                BinOpKind::And | BinOpKind::BitAnd => true,
                _ => return,
            }
            && !expanded_externally(cx, expr.span)
            && let Some((value, first)) = matched(cx, last_test, negated)
            && stable(cx, value)
        {
            let bindings = |pattern: &'tcx Pat<'tcx>| {
                let mut bindings = FxHashMap::default();
                let mut retained = true;
                pattern.each_binding_or_first(&mut |_, id, span, name| {
                    let mode = typeck.extract_binding_mode(cx.tcx.sess, id, span);
                    let held = typeck.node_type(id);
                    retained &= mode.0 != ByRef::No || is_copy(cx, held);
                    bindings.insert(name.name, (mode, held));
                });
                retained.then_some(bindings)
            };
            let Some(expected) = bindings(first.pat) else { return };
            let compatible = |candidate: &'tcx Expr<'tcx>| {
                let (subject, arm) = matched(cx, candidate, negated)?;
                (arm.guard.is_none()
                    && arm.pat.walk_short(|pattern| !matches!(pattern.kind, PatKind::Deref(_) | PatKind::Guard(..)))
                    && bindings(arm.pat).is_some_and(|bindings| bindings == expected)
                    && stable(cx, subject)
                    && eq_expr_value(cx, expr.span.ctxt(), value, subject))
                .then_some(arm.pat)
            };
            let operator = |node| matches!((negated, node), (false, BinOpKind::Or | BinOpKind::BitOr) | (true, BinOpKind::And | BinOpKind::BitAnd));
            if get_parent_expr(cx, expr).is_some_and(|parent| matches!(parent.kind, ExprKind::Binary(operation, _, right) if operator(operation.node) && compatible(right).is_some())) {
                return;
            }
            let patterns = iter::successors(Some(expr), |node| match node.kind {
                ExprKind::Binary(operation, left, _) if operator(operation.node) => Some(left),
                _ => None,
            })
            .map(|node| {
                compatible(match node.kind {
                    ExprKind::Binary(operation, _, right) if operator(operation.node) => right,
                    _ => node,
                })
            })
            .collect::<Option<Vec<_>>>();
            let Some(patterns) = patterns else { return };
            let polarity = if negated { "negated " } else { "" };
            span_lint_hir_and_then(cx, CHAINED_MATCHES, expr.hir_id, expr.span, "native pattern tests repeat the same stable subject", |diag| {
                let patterns = patterns.iter().rev().map(|pattern| snippet(cx, pattern.span, "pattern")).format(" | ");
                diag.help(format!("test `{patterns}` in one {polarity}`matches!` invocation"));
            });
        }
    }
}
