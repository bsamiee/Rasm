use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, get_parent_expr, mir::visit_local_usage, res::MaybeDef as _, source::snippet, sym, ty::is_copy, visitors::local_used_once};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_errors::Applicability;
use rustc_hir::{CaptureBy, ClosureKind, CoroutineDesugaring, CoroutineKind, CoroutineSource, Expr, ExprKind, Node, def_id::LocalDefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::{
    mir::{self, AggregateKind, Location, Operand, Rvalue},
    ty::{self, BorrowKind, ClosureKind as Called, UpvarCapture},
};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    CAPTURED_CLONE,
    Warn,
    "clones of a variable a closure borrows only to clone, where the variable dies after the closure"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] CapturedClone => [CAPTURED_CLONE]);

impl<'tcx> LateLintPass<'tcx> for CapturedClone {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn consumed_once(cx: &LateContext<'_>, closure: &Expr<'_>) -> bool {
            let tcx = cx.tcx;
            if let Some(call) = get_parent_expr(cx, closure)
                && let ExprKind::MethodCall(_, _, arguments, _) | ExprKind::Call(_, arguments) = call.kind
                && let Some(callee) = fn_def_id(cx, call)
                && let Some(position) = arguments.iter().position(|argument| argument.hir_id == closure.hir_id)
                && let Some(&parameter) =
                    tcx.fn_sig(callee).instantiate_identity().skip_normalization().skip_binder().inputs().get(position.strict_add(usize::from(matches!(call.kind, ExprKind::MethodCall(..)))))
                && let ty::Param(_) = parameter.kind()
            {
                tcx.clauses_of(callee)
                    .instantiate_identity(tcx)
                    .clauses
                    .into_iter()
                    .filter_map(|clause| clause.skip_normalization().as_trait_clause())
                    .filter(|bound| bound.self_ty().skip_binder() == parameter)
                    .filter_map(|bound| tcx.fn_trait_kind_from_def_id(bound.def_id()))
                    .eq([Called::FnOnce])
            } else {
                false
            }
        }

        fn cloning<'tcx>(cx: &LateContext<'tcx>, used: &'tcx Expr<'tcx>, held: ty::Ty<'tcx>) -> Option<(&'tcx Expr<'tcx>, String)> {
            let call = get_parent_expr(cx, used).map(|parent| if let ExprKind::AddrOf(..) = parent.kind { get_parent_expr(cx, parent).unwrap_or(parent) } else { parent })?;
            let callee = fn_def_id(cx, call)?;
            let copies = callee.is_lang_item(cx, LangItem::CloneFn)
                || [sym::to_owned_method, sym::to_string_method].into_iter().any(|name| callee.is_diag_item(cx, name)) && cx.typeck_results().expr_ty(call) == held;
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
            && !expr.span.from_expansion()
            && let parent = tcx.hir_enclosing_body_owner(expr.hir_id)
            && tcx.coroutine_kind(parent).is_none()
            && let once = matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(CoroutineDesugaring::Async, CoroutineSource::Block)))
            && (once || matches!(closure.kind, ClosureKind::Closure) && consumed_once(cx, expr))
        {
            let body = tcx.hir_body(closure.body).value;
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
                span_lint_hir_and_then(cx, CAPTURED_CLONE, call.hir_id, call.span, format!("closure clones `{name}`, which nothing reads after the closure"), |diag| {
                    diag.span_suggestion(replaced, format!("move `{name}` into the closure"), removal, Applicability::MachineApplicable);
                });
            }
        }
    }
}
