use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::mir::{PossibleBorrowerMap, visit_local_usage};
use clippy_utils::res::MaybeDef as _;
use clippy_utils::source::snippet;
use clippy_utils::visitors::local_used_once;
use clippy_utils::{fn_def_id, fn_has_unsatisfiable_clauses, get_parent_expr, sym};
use itertools::Itertools as _;
use rustc_attr_ir::LangItem;
use rustc_data_structures::fx::FxHashMap;
use rustc_errors::Applicability;
use rustc_hir::def_id::{LocalDefId, LocalDefIdMap};
use rustc_hir::{Body, CaptureBy, ClosureKind, ConstContext, Constness, CoroutineDesugaring, CoroutineKind, CoroutineSource, Expr, ExprKind, Node};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::mir::visit::{PlaceContext, Visitor as _};
use rustc_middle::mir::{self, AggregateKind, Location, Operand, Rvalue};
use rustc_middle::ty::{self, BorrowKind, TyCtxt, UpvarCapture};

use crate::{associated_trait, bounds_of, expanded_externally, parameter_of, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

struct Captures<'tcx> {
    tcx: TyCtxt<'tcx>,
    mir: &'tcx mir::Body<'tcx>,
    borrowed: FxHashMap<mir::Local, Option<mir::Local>>,
    closures: LocalDefIdMap<Vec<(Location, mir::Local, &'tcx [Operand<'tcx>])>>,
    borrowers: PossibleBorrowerMap<'tcx, 'tcx>,
}

impl<'tcx> Captures<'tcx> {
    fn new(cx: &LateContext<'tcx>, parent: LocalDefId) -> Option<Self> {
        if fn_has_unsatisfiable_clauses(cx, parent.to_def_id()) {
            return None;
        }
        let tcx = cx.tcx;
        let ctfe = match tcx.hir_body_const_context(parent) {
            Some(ConstContext::ConstFn) => matches!(tcx.constness(parent.to_def_id()), Constness::Const { always: true }),
            context => context.is_some(),
        };
        let mir = if ctfe { tcx.mir_for_ctfe(parent) } else { tcx.optimized_mir(parent) };
        let closures: LocalDefIdMap<_> = mir
            .basic_blocks
            .iter_enumerated()
            .flat_map(|(block, data)| data.statements.iter().enumerate().map(move |(statement_index, statement)| (Location { block, statement_index }, statement)))
            .filter_map(|(location, statement)| {
                if let Some((target, Rvalue::Aggregate(kind, operands))) = statement.kind.as_assign()
                    && let AggregateKind::Closure(built, _) | AggregateKind::Coroutine(built, _) = **kind
                    && let Some(built) = built.as_local()
                    && let Some(target) = target.as_local()
                {
                    Some((built, (location, target, operands.raw.as_slice())))
                } else {
                    None
                }
            })
            .into_group_map()
            .into_iter()
            .collect();
        let mut captures = Self { tcx, mir, borrowed: FxHashMap::default(), closures, borrowers: PossibleBorrowerMap::new(cx, mir) };
        captures.visit_body(mir);
        Some(captures)
    }
}

impl<'tcx> mir::visit::Visitor<'tcx> for Captures<'tcx> {
    fn visit_place(&mut self, place: &mir::Place<'tcx>, context: PlaceContext, location: Location) {
        if context.is_place_assignment()
            && let Some(target) = place.as_local()
            && place.ty(self.mir, self.tcx).ty.is_ref()
        {
            let source = self
                .mir
                .stmt_at(location)
                .left()
                .and_then(|statement| statement.kind.as_assign())
                .and_then(|(_, value)| if let Rvalue::Ref(_, mir::BorrowKind::Shared, source) = value { source.as_local() } else { None });
            self.borrowed.entry(target).and_modify(|previous| *previous = previous.filter(|&previous| Some(previous) == source)).or_insert(source);
        }
    }
}

#[derive(Default)]
pub(crate) struct Pass<'tcx> {
    bodies: FxHashMap<LocalDefId, Option<Captures<'tcx>>>,
}

lint!(default Pass<'_>; REDUNDANT_CLONE_IN_CLOSURE: "clones of a captured variable inside an `FnOnce` closure or async block with no later read");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn consumed_once(cx: &LateContext<'_>, closure: &Expr<'_>) -> bool {
            get_parent_expr(cx, closure).and_then(|call| parameter_of(cx, call, closure)).is_some_and(|(callee, parameter)| {
                matches!(parameter.kind(), ty::Param(_)) && bounds_of(cx.tcx, callee, parameter).filter_map(|bound| cx.tcx.fn_trait_kind_from_def_id(bound.def_id())).eq([ty::ClosureKind::FnOnce])
            })
        }

        fn cloning<'tcx>(cx: &LateContext<'tcx>, used: &'tcx Expr<'tcx>) -> Option<(&'tcx Expr<'tcx>, String)> {
            let call = get_parent_expr(cx, used).and_then(|parent| if let ExprKind::AddrOf(..) = parent.kind { get_parent_expr(cx, parent) } else { Some(parent) })?;
            let callee = fn_def_id(cx, call)?;
            let copies = associated_trait(cx.tcx, callee).is_diag_item(cx, sym::Clone)
                || matches!(cx.tcx.opt_associated_item(callee).and_then(|item| item.trait_item_or_self().ok()).opt_diag_name(cx), Some(sym::to_owned_method | sym::to_string_method));
            let direct = matches!(call.kind, ExprKind::MethodCall(_, receiver, [], _) if receiver.hir_id == used.hir_id)
                || matches!(call.kind, ExprKind::Call(_, [argument]) if matches!(argument.kind, ExprKind::AddrOf(_, _, operand) if operand.hir_id == used.hir_id));
            (copies && direct).then(|| (call, snippet(cx, used.span, "..").into_owned()))
        }

        let tcx = cx.tcx;
        let ExprKind::Closure(closure) = expr.kind else { return };
        if closure.capture_clause != CaptureBy::Ref || expanded_externally(cx, expr.span) {
            return;
        }
        let parent = tcx.hir_enclosing_body_owner(expr.hir_id);
        let once = matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(CoroutineDesugaring::Async, CoroutineSource::Block)));
        if tcx.coroutine_kind(parent).is_some() || !(once || matches!(closure.kind, ClosureKind::Closure) && consumed_once(cx, expr)) {
            return;
        }
        let (body, capturing) = (tcx.hir_body(closure.body).value, if once { "async block" } else { "closure" });
        let typeck = cx.typeck_results();
        let mut dead_after = |index| {
            let Some(facts) = self.bodies.entry(parent).or_insert_with(|| Captures::new(cx, parent)).as_mut() else { return false };
            let Some(captures) = facts.closures.get(&closure.def_id) else { return false };
            captures.iter().all(|(location, target, operands)| {
                if let Some(Operand::Move(reference)) = operands.get(index)
                    && let Some(reference) = reference.as_local()
                    && let Some(variable) = facts.borrowed.get(&reference).copied().flatten()
                {
                    facts.borrowers.bounded_borrowers(&[], &[reference, *target], variable, *location)
                        && visit_local_usage([variable], facts.mir, *location).is_some_and(|[usage]| usage.local_use_locs.is_empty())
                } else {
                    false
                }
            })
        };
        let cloned = tcx.closure_captures(closure.def_id).iter().enumerate().filter_map(|(index, captured)| {
            let variable = captured.get_root_variable();
            let held = typeck.node_type(variable);
            if captured.place.projections.is_empty()
                && matches!(captured.info.capture_kind, UpvarCapture::ByRef(BorrowKind::Immutable))
                && held.is_lang_item(cx, LangItem::String)
                && let Some(used) = local_used_once(cx, body, variable)
                && let Some((call, removal)) = cloning(cx, used)
                && dead_after(index)
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
            span_lint_hir_and_then(cx, REDUNDANT_CLONE_IN_CLOSURE, call.hir_id, call.span, format!("{capturing} clones `{name}` with no later read"), |diag| {
                suggest(cx, call.hir_id, diag, replaced, format!("move `{name}` into the {capturing}"), removal, Applicability::MachineApplicable);
            });
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) { self.bodies.remove(&cx.tcx.hir_body_owner_def_id(body.id())); }
}
