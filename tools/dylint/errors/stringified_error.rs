use core::ops::ControlFlow;

use clippy_utils::{
    ExprUseNode,
    diagnostics::span_lint_and_help,
    fn_def_id, get_expr_use_site, get_parent_expr_for_hir,
    macros::first_node_macro_backtrace,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    visitors::{for_each_expr_without_closures, for_each_local_use_after_expr},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind, HirId, Mutability, Node, Pat, PatKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArg, Ty};

use super::{body_output, untyped_kind};
use crate::{expanded_externally, fn_item_value, impl_owner, is_error, is_text, pointee, result_parts, signature};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Sink {
    Compared,
    Returned,
    Stored,
}

// --- [SERVICES] ------------------------------------------------------------------------

lint!(STRINGIFIED_ERROR: "errors rendered to text that is stored, returned, or compared, and values rendered to text that is compared");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let rendered = match (expr.kind, expanded_externally(cx, expr.span)) {
            (ExprKind::MethodCall(_, operand, [], _) | ExprKind::Call(_, [operand]), false) if fn_def_id(cx, expr).is_diag_item(cx, sym::to_string_method) =>
                Some((expr.span, typeck.expr_ty(operand))),
            (ExprKind::Path(_), false) if fn_item_value(cx, expr).is_diag_item(cx, sym::to_string_method) => Some((expr.span, typeck.node_args(expr.hir_id).type_at(0))),
            _ if let Some(call) = first_node_macro_backtrace(cx, expr).find(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)).filter(|call| !expanded_externally(cx, call.span)) =>
                for_each_expr_without_closures(expr, |part| {
                    if let ExprKind::Tup(arguments) = part.kind
                        && part.span.from_expansion()
                    {
                        ControlFlow::Break(arguments)
                    } else {
                        ControlFlow::Continue(())
                    }
                })
                .and_then(|arguments| {
                    let operands = || arguments.iter().filter_map(|argument| typeck.expr_ty(argument).builtin_deref(true));
                    operands().find(|&operand| is_error(cx, operand)).or_else(|| operands().find(|&operand| !is_text(cx, operand)))
                })
                .map(|operand| (call.span, operand)),
            _ => None,
        };
        if let Some((span, operand)) = rendered.filter(|&(_, operand)| !is_text(cx, operand))
            && let Some(sink) = sink(cx, expr)
        {
            let (error, shown) = (is_error(cx, operand), pointee(cx, operand));
            let help = match (error, sink) {
                (true, Sink::Compared) => String::from("match the error's variant, `kind()`, or `downcast_ref` in place of its text"),
                (true, Sink::Returned | Sink::Stored) => format!("hold `{shown}` as `#[source]` of a `thiserror` variant"),
                (false, Sink::Compared) => format!("compare or match `{shown}` in place of its text"),
                (false, Sink::Returned | Sink::Stored) => return,
            };
            let effect = match sink {
                Sink::Compared => "compared",
                Sink::Returned => "returned",
                Sink::Stored => "stored",
            };
            let message = if error { format!("text of error `{shown}` is {effect}") } else { format!("text of `{shown}` is {effect}") };
            span_lint_and_help(cx, STRINGIFIED_ERROR, span, message, None, help);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn sink<'tcx>(cx: &LateContext<'tcx>, text: &'tcx Expr<'tcx>) -> Option<Sink> {
    fn relayed(cx: &LateContext<'_>, binding: HirId, written: HirId) -> Option<Sink> {
        for_each_local_use_after_expr(cx, binding, written, |used| sink(cx, used).map_or(ControlFlow::Continue(()), ControlFlow::Break)).break_value()
    }

    fn consumed<'tcx>(cx: &LateContext<'tcx>, call: &'tcx Expr<'tcx>, text: &Expr<'_>) -> Option<Sink> {
        let callee = fn_def_id(cx, call)?;
        let produced = cx.typeck_results().expr_ty(call);
        let constructed = callee.ctor_parent(cx);
        if callee.is_lang_item(cx, LangItem::TryTraitBranch) {
            returned(cx, call.hir_id)
        } else if result_parts(cx, produced).is_some_and(|(_, error)| untyped_kind(cx, error).is_some()) {
            None
        } else if produced.walk().filter_map(GenericArg::as_type).any(|held| is_text(cx, held) || held.is_fn() && is_text(cx, held.fn_sig(cx.tcx).skip_binder().output())) {
            sink(cx, call)
        } else if let ExprKind::MethodCall(_, receiver, ..) = call.kind
            && receiver.hir_id != text.hir_id
            && !cx.typeck_results().expr_ty(text).is_ref()
            && signature(cx.tcx, callee).inputs().first().is_some_and(|holder| matches!(holder.kind(), ty::Ref(_, _, Mutability::Mut)))
        {
            Some(Sink::Stored)
        } else if produced.is_bool() && signature(cx.tcx, callee).inputs().len() > 1 {
            Some(Sink::Compared)
        } else {
            let owned = produced.ty_adt_def().is_some_and(|adt| impl_owner(cx.tcx, callee).is_some_and(|owner| owner.ty_adt_def() == Some(adt)));
            ((constructed.is_some() || callee.is_local() && (owned || is_error(cx, produced))) && !handed_off(cx, produced)).then_some(Sink::Stored)
        }
    }

    fn compares(cx: &LateContext<'_>, parent: &Expr<'_>, child: HirId) -> bool {
        let literal = |pattern: &Pat<'_>| !pattern.walk_short(|part| !(matches!(part.kind, PatKind::Expr(_)) && is_text(cx, cx.typeck_results().pat_ty(part))));
        match parent.kind {
            ExprKind::Binary(operator, ..) => operator.node.is_comparison(),
            ExprKind::Match(scrutinee, arms, _) => scrutinee.hir_id == child && arms.iter().any(|arm| literal(arm.pat)),
            ExprKind::Let(binding) => binding.init.hir_id == child && literal(binding.pat),
            _ => false,
        }
    }

    let site = get_expr_use_site(cx.tcx, cx.typeck_results(), text.span.ctxt(), text);
    match (site.node, site.use_node(cx)) {
        (_, ExprUseNode::Field(field)) => (!handed_off(cx, cx.typeck_results().expr_ty(get_parent_expr_for_hir(cx, field.hir_id)?))).then_some(Sink::Stored),
        (_, ExprUseNode::Return(owner)) if let Node::Expr(closure) = cx.tcx.hir_node_by_def_id(owner.def_id) => sink(cx, closure),
        (_, ExprUseNode::Return(_)) => returned(cx, text.hir_id),
        (_, ExprUseNode::LetStmt(local)) if let PatKind::Binding(_, binding, _, None) = local.pat.kind => relayed(cx, binding, local.init?.hir_id),
        (Node::Expr(parent), _)
            if let ExprKind::Assign(target, value, _) | ExprKind::AssignOp(_, target, value) = parent.kind
                && value.hir_id == site.child_id =>
            target.res_local_id().map_or(Some(Sink::Stored), |binding| relayed(cx, binding, parent.hir_id)),
        (Node::Expr(parent), _) if compares(cx, parent, site.child_id) => Some(Sink::Compared),
        (Node::Expr(parent), ExprUseNode::AddrOf(..)) => sink(cx, parent),
        (Node::Expr(parent), ExprUseNode::FnArg(..) | ExprUseNode::MethodArg(..)) => consumed(cx, parent, text),
        _ => None,
    }
}

fn returned(cx: &LateContext<'_>, node: HirId) -> Option<Sink> {
    body_output(cx, cx.tcx.hir_enclosing_body_owner(node)).and_then(|enclosing| result_parts(cx, enclosing)).is_none_or(|(_, error)| untyped_kind(cx, error).is_none()).then_some(Sink::Returned)
}

fn handed_off<'tcx>(cx: &LateContext<'tcx>, produced: Ty<'tcx>) -> bool { produced.ty_adt_def().is_some_and(|adt| !adt.did().is_local()) && is_error(cx, produced) }
