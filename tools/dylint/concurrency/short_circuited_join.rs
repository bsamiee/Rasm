use core::{iter, ops::ControlFlow};

use clippy_utils::{desugar_await, diagnostics::span_lint_hir_and_then, fn_def_id, get_enclosing_block, paths::PathNS, res::MaybeResPath as _, sym, visitors::for_each_expr};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, HirId, LetStmt, MatchSource, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;
use rustc_span::Span;

use crate::{expanded_externally, item_path, path, paths, result_parts};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    joins: FxHashSet<DefId>,
    join_macros: FxHashSet<DefId>,
    buffers: FxHashSet<DefId>,
    stops: FxHashSet<DefId>,
    join_all: FxHashSet<DefId>,
    try_collect: Option<DefId>,
}

lint!(Pass; SHORT_CIRCUITED_JOIN: "joined or buffered futures that `?`, `try_*` adapters, or a `Result` collection stop at the first error");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            joins: paths(tcx, PathNS::Value, &["futures::future::join", "futures::future::join3", "futures::future::join4", "futures::future::join5"]),
            join_macros: paths(tcx, PathNS::Macro, &["tokio::join", "futures::join"]),
            buffers: paths(tcx, PathNS::Value, &["futures::stream::StreamExt::buffered", "futures::stream::StreamExt::buffer_unordered"]),
            stops: paths(
                tcx,
                PathNS::Value,
                &["futures::stream::TryStreamExt::try_collect", "futures::stream::TryStreamExt::try_next", "futures::stream::TryStreamExt::try_for_each", "futures::stream::TryStreamExt::try_fold"],
            ),
            join_all: paths(tcx, PathNS::Value, &["futures::future::join_all", "tokio::task::JoinSet::join_all"]),
            try_collect: path(tcx, PathNS::Value, "core::iter::Iterator::try_collect"),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        let typeck = cx.typeck_results();
        if let PatKind::Tuple(parts, _) = local.pat.kind
            && let Some(init) = local.init
            && !expanded_externally(cx, local.span.source_callsite())
            && let Some(join) = init
                .span
                .macro_backtrace()
                .find_map(|expansion| expansion.macro_def_id.filter(|written| self.join_macros.contains(written) && !expanded_externally(cx, expansion.call_site)))
                .map(|written| format!("{}!", cx.tcx.item_name(written)))
                .or_else(|| fn_def_id(cx, desugar_await(init)?).filter(|called| self.joins.contains(called)).map(|called| item_path(cx, called)))
            && let Some(block) = get_enclosing_block(cx, local.hir_id)
        {
            let fallible = parts
                .iter()
                .filter_map(|part| if let PatKind::Binding(_, binding, ..) = part.kind { Some(binding) } else { None })
                .filter(|&binding| result_parts(cx, typeck.node_type(binding)).is_some())
                .collect::<FxHashSet<_>>();
            let mut tried = Vec::new();
            let _: Option<()> = for_each_expr(cx.tcx, block, |expr| {
                if let ExprKind::Match(&Expr { kind: ExprKind::Call(_, [operand]), .. }, _, MatchSource::TryDesugar(_)) = expr.kind
                    && operand.res_local_id().is_some_and(|binding| fallible.contains(&binding))
                {
                    tried.push(operand.span);
                }
                ControlFlow::Continue(())
            });
            if tried.len() > 1 {
                let combine = match fallible.len() {
                    count @ 2..=4 => format!("`map{count}`"),
                    _ => String::from("`map2`..`map4`"),
                };
                report(
                    cx,
                    local.hir_id,
                    local.span,
                    format!("`?` on `{join}` results keeps only the first error"),
                    tried,
                    format!("convert each result with `validated::Validated::from`, combine them with {combine}, and call `.ok()`"),
                );
            }
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let receivers = || iter::successors(Some(expr), |step| if let ExprKind::MethodCall(_, receiver, ..) = step.kind { Some(receiver) } else { desugar_await(step) }).skip(1);
        let Some(stop) = fn_def_id(cx, expr) else { return };
        if expanded_externally(cx, expr.span) {
            return;
        }
        match self.stops.contains(&stop).then(|| receivers().find_map(|step| fn_def_id(cx, step).filter(|called| self.buffers.contains(called)))).flatten() {
            Some(buffer) => report(
                cx,
                expr.hir_id,
                expr.span,
                format!("`{}` after `{}` stops at the first error of the buffered futures", cx.tcx.item_name(stop), cx.tcx.item_name(buffer)),
                Vec::new(),
                String::from("collect the stream into a `Vec`, then its results into `validated::Validated<_, _>`, and call `.ok()`"),
            ),
            None if matches!(expr.kind, ExprKind::MethodCall(..))
                && (result_parts(cx, cx.typeck_results().expr_ty(expr)).is_some() && cx.tcx.is_diagnostic_item(sym::iterator_collect_fn, stop) || self.try_collect == Some(stop))
                && let Some(joined) = receivers().find_map(|step| fn_def_id(cx, step).filter(|called| self.join_all.contains(called) || self.buffers.contains(called))) =>
                report(
                    cx,
                    expr.hir_id,
                    expr.span,
                    format!("`{}` over `{}` keeps only the first error of the joined futures", cx.tcx.item_name(stop), cx.tcx.item_name(joined)),
                    Vec::new(),
                    String::from("collect the results into `validated::Validated<_, _>` and call `.ok()`"),
                ),
            None => {}
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn report(cx: &LateContext<'_>, node: HirId, span: Span, message: String, tried: Vec<Span>, help: String) {
    span_lint_hir_and_then(cx, SHORT_CIRCUITED_JOIN, node, span, message, |diag| {
        if !tried.is_empty() {
            diag.span_note(tried, "each `?` returns here before the remaining results are read");
        }
        diag.help(help);
    });
}
