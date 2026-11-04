use clippy_utils::{desugar_await, diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, res::MaybeDef as _, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, path, paths, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    buffers: FxHashSet<DefId>,
    collects: FxHashSet<DefId>,
    join_all: FxHashSet<DefId>,
    try_collect: Option<DefId>,
}

lint!(Pass; SHORT_CIRCUITED_JOIN: "completed native future batches collected into a `Result` that keeps only the first error");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            buffers: paths(tcx, PathNS::Value, &["futures::stream::StreamExt::buffered", "futures::stream::StreamExt::buffer_unordered"]),
            collects: paths(tcx, PathNS::Value, &["futures::stream::StreamExt::collect", "tokio_stream::StreamExt::collect"]),
            join_all: paths(tcx, PathNS::Value, &["futures::future::join_all", "tokio::task::JoinSet::join_all"]),
            try_collect: path(tcx, PathNS::Value, "core::iter::Iterator::try_collect"),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(_, iterated, [], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && result_parts(cx, cx.typeck_results().expr_ty(expr)).is_some()
            && let Some(stop) = fn_def_id(cx, expr)
            && (cx.tcx.is_diagnostic_item(sym::iterator_collect_fn, stop) || self.try_collect == Some(stop))
            && fn_def_id(cx, iterated).and_then(|called| cx.tcx.opt_associated_item(called)?.trait_item_or_self().ok()).is_lang_item(cx, LangItem::IntoIterIntoIter)
            && let ExprKind::MethodCall(_, vector, [], _) | ExprKind::Call(_, [vector]) = iterated.kind
            && cx.typeck_results().expr_ty(vector).is_diag_item(cx, sym::Vec)
            && let Some(completed) = desugar_await(vector)
            && let Some(producer) = fn_def_id(cx, completed)
            && let Some(joined) = if self.join_all.contains(&producer) {
                Some(producer)
            } else if self.collects.contains(&producer)
                && let ExprKind::MethodCall(_, buffered, [], _) = completed.kind
            {
                fn_def_id(cx, buffered).filter(|buffered| self.buffers.contains(buffered))
            } else {
                None
            }
        {
            span_lint_hir_and_then(
                cx,
                SHORT_CIRCUITED_JOIN,
                expr.hir_id,
                expr.span,
                format!("`{}` over `{}` keeps only the first error of the completed futures", cx.tcx.item_name(stop), cx.tcx.item_name(joined)),
                |diag| {
                    diag.help("collect the results into `validated::Validated<_, _>` and call `.ok()`");
                },
            );
        }
    }
}
