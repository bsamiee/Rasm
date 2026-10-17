use clippy_utils::{diagnostics::span_lint_hir_and_then, is_in_test, is_inside_always_const_context, macros::root_macro_call_first_node, paths::PathNS, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::TyCtxt;

use crate::paths;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    ASSERTION,
    Warn,
    "assertion macros outside test code and const contexts, checking the program's own code at run time"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Assertion {
    assertions: FxHashSet<DefId>,
}

impl_lint_pass!(Assertion => [ASSERTION]);

impl Assertion {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            assertions: [sym::assert_macro, sym::assert_eq_macro, sym::assert_ne_macro, sym::debug_assert_macro, sym::debug_assert_eq_macro, sym::debug_assert_ne_macro]
                .into_iter()
                .filter_map(|name| tcx.get_diagnostic_item(name))
                .chain(paths(tcx, PathNS::Macro, &["core::assert_matches", "core::debug_assert_matches"]))
                .collect(),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Assertion {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(call) = root_macro_call_first_node(cx, expr)
            && self.assertions.contains(&call.def_id)
            && !call.span.from_expansion()
            && !is_in_test(cx.tcx, expr.hir_id)
            && !is_inside_always_const_context(cx.tcx, expr.hir_id)
        {
            span_lint_hir_and_then(cx, ASSERTION, expr.hir_id, call.span, format!("`{}!` checks a fact of the program's own code at run time", cx.tcx.item_name(call.def_id)), |diag| {
                diag.help("return a typed error for a failure the input causes, state the invariant in the types, or check it in a test or a `const` block");
            });
        }
    }
}
