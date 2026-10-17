use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS, res::MaybeDef as _, source::snippet, sym};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    maps: FxHashSet<DefId>,
}

lint!(Pass; MAP_INDEXING: "index expressions on a `BTreeMap`, `HashMap`, or `IndexMap`, which panic when the key is absent");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { maps: [sym::BTreeMap, sym::HashMap].into_iter().filter_map(|map| tcx.get_diagnostic_item(map)).chain(paths(tcx, PathNS::Type, &["indexmap::IndexMap"])).collect() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Index(map, key, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let held = cx.typeck_results().expr_ty_adjusted(map).peel_refs()
            && held.opt_def_id().is_some_and(|declared| self.maps.contains(&declared))
        {
            let (held, key) = (with_forced_trimmed_paths!(held.to_string()), snippet(cx, key.span, ".."));
            span_lint_hir_and_then(cx, MAP_INDEXING, expr.hir_id, expr.span, format!("indexing `{held}` by `{key}` panics when the key is absent"), |diag| {
                diag.help("call `get` and handle `None`");
            });
        }
    }
}
