use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr,
    paths::PathNS,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::is_copy,
    visitors::is_local_used,
};
use rustc_data_structures::{
    fx::{FxHashMap, FxIndexMap, IndexEntry},
    unord::{UnordItems, UnordSet},
};
use rustc_hir::{
    Body, Expr, ExprKind, HirId, LetStmt, PatKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, TyCtxt};
use rustc_span::{Ident, Span};

use crate::{expanded_externally, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    operations: FxHashMap<DefId, bool>,
    locals: FxHashMap<LocalDefId, FxIndexMap<HirId, (HirId, Span, Ident)>>,
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
        {
            self.locals.entry(cx.tcx.hir_enclosing_body_owner(local.hir_id)).or_default().insert(binding, (local.hir_id, local.span, name));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(binding) = expr.res_local_id()
            && let Some(locals) = self.locals.get_mut(&cx.tcx.hir_enclosing_body_owner(expr.hir_id))
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
        let owner = cx.tcx.hir_body_owner_def_id(body.id());
        self.captures.remove(&owner);
        for (node, span, name) in self.locals.remove(&owner).into_iter().flat_map(FxIndexMap::into_values) {
            span_lint_hir_and_then(cx, NEEDLESS_CELL, node, span, format!("`{name}` uses `Cell` only to access and replace a copyable value"), |diag| {
                diag.help(format!("bind the value with `let mut {name}`, use reads and assignment, and use `core::mem::replace` or `core::mem::take` for returned old values"));
            });
        }
    }
}
