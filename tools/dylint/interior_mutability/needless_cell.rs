use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::paths::PathNS;
use clippy_utils::res::{MaybeDef as _, MaybeResPath as _};
use clippy_utils::ty::is_copy;
use clippy_utils::visitors::is_local_used;
use clippy_utils::{fn_def_id, get_parent_expr, sym};
use rustc_data_structures::fx::{FxHashMap, FxIndexMap, IndexEntry};
use rustc_data_structures::unord::{UnordItems, UnordSet};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::{Body, BodyId, Expr, ExprKind, HirId, LetStmt, PatKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt};
use rustc_span::{Ident, Span};

use crate::{expanded_externally, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    operations: FxHashMap<DefId, bool>,
    locals: FxHashMap<BodyId, FxIndexMap<HirId, (HirId, Span, Ident)>>,
    captures: FxHashMap<LocalDefId, UnordSet<HirId>>,
}

lint!(Pass; NEEDLESS_CELL: "uncaptured local cells over copyable values used only for native value access and replacement");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            operations: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("core::cell::Cell::get", false),
                    ("core::cell::Cell::set", false),
                    ("core::cell::Cell::update", false),
                    ("core::cell::Cell::replace", true),
                    ("core::cell::Cell::take", false),
                    ("core::cell::Cell::into_inner", false),
                ],
            ),
            locals: FxHashMap::default(),
            captures: FxHashMap::default(),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, name, None) = local.pat.kind
            && !expanded_externally(cx, local.span)
            && let typeck = cx.typeck_results()
            && let ty::Adt(cell, arguments) = typeck.node_type(binding).kind()
            && cell.is_diag_item(cx, sym::Cell)
            && is_copy(cx, arguments.type_at(0))
            && !self
                .captures
                .entry(typeck.hir_owner.def_id)
                .or_insert_with(|| typeck.closure_min_captures.items().flat_map(|(_, captures)| UnordItems::new(captures.keys().copied())).collect())
                .contains(&binding)
            && let Some(body) = cx.enclosing_body
        {
            self.locals.entry(body).or_default().insert(binding, (local.hir_id, local.span, name));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(binding) = expr.res_local_id()
            && let Some(locals) = cx.enclosing_body.and_then(|body| self.locals.get_mut(&body))
            && let IndexEntry::Occupied(candidate) = locals.entry(binding)
            && !get_parent_expr(cx, expr).is_some_and(|call| {
                matches!(call.kind, ExprKind::MethodCall(_, receiver, arguments, _)
                if receiver.hir_id == expr.hir_id && !expanded_externally(cx, call.span)
                    && fn_def_id(cx, call).and_then(|operation| self.operations.get(&operation)).is_some_and(|&replacement| !replacement || !is_local_used(cx, arguments, binding)))
            })
        {
            candidate.swap_remove();
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) {
        self.captures.remove(&cx.tcx.hir_body_owner_def_id(body.id()));
        for (node, span, name) in self.locals.remove(&body.id()).into_iter().flat_map(FxIndexMap::into_values) {
            span_lint_hir_and_then(cx, NEEDLESS_CELL, node, span, format!("`{name}` uses `Cell` only to access and replace a copyable value"), |diag| {
                diag.help(format!("bind the value with `let mut {name}`, use reads and assignment, and use `core::mem::replace` or `core::mem::take` for returned old values"));
            });
        }
    }
}
