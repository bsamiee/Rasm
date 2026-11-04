use clippy_utils::{paths::PathNS, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, HirId, Node, def_id::DefId};
use rustc_hir_typeck::expr_use_visitor::{Delegate, ExprUseVisitor, PlaceBase, PlaceWithHirId};
use rustc_lint::LateContext;
use rustc_middle::{
    mir::FakeReadCause,
    ty::{self, TyCtxt},
};

use crate::paths;

// --- [SERVICES] ------------------------------------------------------------------------

struct TemporaryDrops<'cx, 'tcx> {
    cx: &'cx LateContext<'tcx>,
    found: bool,
}

impl<'tcx> TemporaryDrops<'_, 'tcx> {
    fn borrowed(&mut self, place: &PlaceWithHirId<'tcx>) { self.found = self.found || place.place.base == PlaceBase::Rvalue && place.place.base_ty.needs_drop(self.cx.tcx, self.cx.typing_env()); }
}

impl<'tcx> Delegate<'tcx> for TemporaryDrops<'_, 'tcx> {
    fn consume(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}
    fn use_cloned(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}
    fn borrow(&mut self, place_with_id: &PlaceWithHirId<'tcx>, _: HirId, _: ty::BorrowKind) { self.borrowed(place_with_id); }
    fn mutate(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}
    fn bind(&mut self, binding_place: &PlaceWithHirId<'tcx>, _: HirId) {
        self.found = self.found
            || binding_place.place.ty().needs_drop(self.cx.tcx, self.cx.typing_env())
                && self.cx.tcx.hir_parent_iter(binding_place.hir_id).find_map(|(_, node)| match node {
                    Node::LetStmt(local) => Some(local.super_.is_some()),
                    Node::Arm(_) | Node::Expr(_) => Some(false),
                    _ => None,
                }) == Some(true);
    }
    fn fake_read(&mut self, place_with_id: &PlaceWithHirId<'tcx>, _: FakeReadCause, _: HirId) { self.borrowed(place_with_id); }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn assertions(tcx: TyCtxt<'_>) -> FxHashSet<DefId> {
    [sym::assert_macro, sym::assert_eq_macro, sym::assert_ne_macro, sym::debug_assert_macro, sym::debug_assert_eq_macro, sym::debug_assert_ne_macro]
        .into_iter()
        .filter_map(|name| tcx.get_diagnostic_item(name))
        .chain(paths(tcx, PathNS::Macro, &["core::assert_matches", "core::debug_assert_matches"]))
        .collect()
}

pub(crate) fn cargo_test_target(tcx: TyCtxt<'_>) -> bool { tcx.sess.opts.test && std::env::var_os("CARGO_TARGET_TMPDIR").is_some() }

fn has_temporary_drop<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) -> bool {
    let mut drops = TemporaryDrops { cx, found: false };
    ExprUseVisitor::for_clippy(cx, expr.hir_id.owner.def_id, &mut drops).walk_expr(expr).into_ok();
    drops.found
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod formatting;
pub(crate) mod matching;
pub(crate) mod renamed_function_params_in_macro;
pub(crate) mod restriction_in_macro;
