use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, get_parent_expr, mir::visit_local_usage, res::MaybeDef as _, source::snippet, sym, ty::is_copy, visitors::local_used_once};
use rustc_data_structures::fx::FxHashMap;
use rustc_errors::Applicability;
use rustc_hir::{CaptureBy, ClosureKind, CoroutineDesugaring, CoroutineKind, CoroutineSource, Expr, ExprKind, Node, def_id::LocalDefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{
    mir::{self, AggregateKind, Location, Operand, Rvalue},
    ty::{self, BorrowKind, ClosureKind as Called, Ty, UpvarCapture},
};

use crate::{bounds_of, cloned, expanded_externally, parameter_of, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_CLONE_IN_CLOSURE: "clones of a captured variable inside an `FnOnce` closure or async block, where nothing reads the variable after the closure");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn consumed_once(cx: &LateContext<'_>, closure: &Expr<'_>) -> bool {
            get_parent_expr(cx, closure).and_then(|call| parameter_of(cx, call, closure)).is_some_and(|(callee, parameter)| {
                matches!(parameter.kind(), ty::Param(_)) && bounds_of(cx.tcx, callee, parameter).filter_map(|bound| cx.tcx.fn_trait_kind_from_def_id(bound.def_id())).eq([Called::FnOnce])
            })
        }

        fn cloning<'tcx>(cx: &LateContext<'tcx>, used: &'tcx Expr<'tcx>, held: Ty<'tcx>) -> Option<(&'tcx Expr<'tcx>, String)> {
            let call = get_parent_expr(cx, used).map(|parent| if let ExprKind::AddrOf(..) = parent.kind { get_parent_expr(cx, parent).unwrap_or(parent) } else { parent })?;
            let callee = fn_def_id(cx, call)?;
            let copies = cloned(cx, call).is_some() || matches!(callee.opt_diag_name(cx), Some(sym::to_owned_method | sym::to_string_method)) && cx.typeck_results().expr_ty(call) == held;
            let direct = matches!(call.kind, ExprKind::MethodCall(_, receiver, [], _) if receiver.hir_id == used.hir_id)
                || matches!(call.kind, ExprKind::Call(_, [argument]) if matches!(argument.kind, ExprKind::AddrOf(_, _, operand) if operand.hir_id == used.hir_id));
            (copies && direct).then(|| (call, snippet(cx, used.span, "..").into_owned()))
        }

        fn dead_after(mir: &mir::Body<'_>, closure: LocalDefId, index: usize) -> bool {
            let assignments = || {
                mir.basic_blocks.iter_enumerated().flat_map(|(block, data)| {
                    data.statements.iter().enumerate().filter_map(move |(statement_index, statement)| Some((Location { block, statement_index }, statement.kind.as_assign()?)))
                })
            };
            let borrowed = assignments()
                .filter_map(|(_, (target, value))| if let Rvalue::Ref(_, mir::BorrowKind::Shared, source) = value { Some((target.as_local()?, source.as_local()?)) } else { None })
                .collect::<FxHashMap<_, _>>();
            assignments().any(|(location, (_, value))| {
                if let Rvalue::Aggregate(kind, operands) = value
                    && let AggregateKind::Closure(built, _) | AggregateKind::Coroutine(built, _) = **kind
                    && built == closure.to_def_id()
                    && let Some(Operand::Move(reference)) = operands.iter().nth(index)
                    && let Some(&variable) = reference.as_local().and_then(|reference| borrowed.get(&reference))
                {
                    visit_local_usage([variable], mir, location.successor_within_block()).is_some_and(|[usage]| usage.local_use_locs.is_empty())
                } else {
                    false
                }
            })
        }

        let tcx = cx.tcx;
        if let ExprKind::Closure(closure) = expr.kind
            && closure.capture_clause == CaptureBy::Ref
            && !expanded_externally(cx, expr.span)
            && let parent = tcx.hir_enclosing_body_owner(expr.hir_id)
            && tcx.coroutine_kind(parent).is_none()
            && let once = matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(CoroutineDesugaring::Async, CoroutineSource::Block)))
            && (once || matches!(closure.kind, ClosureKind::Closure) && consumed_once(cx, expr))
        {
            let body = &tcx.hir_body(closure.body).value;
            let typeck = cx.typeck_results();
            let cloned = tcx.closure_captures(closure.def_id).iter().enumerate().filter_map(|(index, captured)| {
                let variable = captured.get_root_variable();
                let held = typeck.node_type(variable);
                if captured.place.projections.is_empty()
                    && matches!(captured.info.capture_kind, UpvarCapture::ByRef(BorrowKind::Immutable))
                    && !held.is_ref()
                    && !is_copy(cx, held)
                    && !held.has_significant_drop(tcx, cx.typing_env())
                    && let Some(used) = local_used_once(cx, body, variable)
                    && let Some((call, removal)) = cloning(cx, used, held)
                    && dead_after(tcx.optimized_mir(parent), closure.def_id, index)
                {
                    Some((tcx.hir_name(variable), call, removal))
                } else {
                    None
                }
            });
            for (name, call, removal) in cloned {
                let replaced = if let Node::ExprField(field) = tcx.parent_hir_node(call.hir_id)
                    && field.ident.name == name
                {
                    field.span
                } else {
                    call.span
                };
                span_lint_hir_and_then(cx, REDUNDANT_CLONE_IN_CLOSURE, call.hir_id, call.span, format!("closure clones `{name}`, which nothing reads after the closure"), |diag| {
                    suggest(cx, call.hir_id, diag, replaced, format!("move `{name}` into the closure"), removal, Applicability::MachineApplicable);
                });
            }
        }
    }
}
