use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::is_inside_always_const_context;
use clippy_utils::macros::first_node_macro_backtrace;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::Expr;
use rustc_hir::def_id::DefId;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::macros::{assertions, cargo_test_target};
use crate::{expanded_externally, in_test};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    assertions: FxHashSet<DefId>,
    test_target: bool,
}

lint!(Pass; ASSERTIONS_OUTSIDE_TESTS: "assertion macros outside test code and `const` contexts");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { assertions: assertions(tcx), test_target: cargo_test_target(tcx) } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if !self.test_target
            && let Some(call) = first_node_macro_backtrace(cx, expr).find(|call| self.assertions.contains(&call.def_id) && !expanded_externally(cx, call.span))
            && !in_test(cx.tcx, expr.hir_id)
            && !is_inside_always_const_context(cx.tcx, expr.hir_id)
        {
            span_lint_hir_and_then(cx, ASSERTIONS_OUTSIDE_TESTS, expr.hir_id, call.span, format!("`{}!` asserts at run time outside a test", cx.tcx.item_name(call.def_id)), |diag| {
                diag.help("return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block");
            });
        }
    }
}
