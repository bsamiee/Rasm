use clippy_utils::{diagnostics::span_lint_hir_and_then, expr_or_init, fn_def_id, paths::PathNS, source::snippet, ty::same_type_modulo_regions};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, MatchSource, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, item_path, path, result_parts, signature};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    wrappers: FxHashMap<DefId, (DefId, DefId)>,
}

lint!(Pass; MANUAL_SERIALIZATION: "serialized text passed to a constructor with a counterpart that serializes the value itself");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let rows = [("rmcp::model::ContentBlock::text", "serde_json::to_string", "rmcp::model::ContentBlock::json")];
        let value = |named| path(tcx, PathNS::Value, named);
        Self { wrappers: rows.into_iter().filter_map(|(wrapper, serializer, direct)| Some((value(wrapper)?, (value(serializer)?, value(direct)?)))).collect() }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn serialized<'tcx>(cx: &LateContext<'tcx>, argument: &'tcx Expr<'tcx>, serializer: DefId) -> Option<(&'tcx Expr<'tcx>, &'tcx Expr<'tcx>)> {
            let typeck = cx.typeck_results();
            let value = expr_or_init(cx, argument.peel_borrows());
            match value.kind {
                ExprKind::Match(branch, _, MatchSource::TryDesugar(_)) if let ExprKind::Call(_, [tried]) = branch.kind => serialized(cx, tried, serializer),
                ExprKind::MethodCall(_, receiver, ..)
                    if let Some((held, _)) = result_parts(cx, typeck.expr_ty(receiver))
                        && let produced = typeck.expr_ty(value)
                        && same_type_modulo_regions(result_parts(cx, produced).map_or(produced, |(ok, _)| ok), held) =>
                    serialized(cx, receiver, serializer),
                ExprKind::Call(_, [operand]) if fn_def_id(cx, value) == Some(serializer) => Some((value, operand)),
                _ => None,
            }
        }

        if let ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(wrapper) = fn_def_id(cx, expr)
            && let Some(&(serializer, direct)) = self.wrappers.get(&wrapper)
            && let Some((serialized, value)) = arguments.iter().find_map(|argument| serialized(cx, argument, serializer))
        {
            let returned = signature(cx.tcx, direct).output();
            let retyped = !same_type_modulo_regions(signature(cx.tcx, wrapper).output(), returned);
            let (wrapper, direct, returned) = with_forced_trimmed_paths!((item_path(cx, wrapper), item_path(cx, direct), returned.to_string()));
            let value = snippet(cx, value.span, "..");
            span_lint_hir_and_then(cx, MANUAL_SERIALIZATION, expr.hir_id, expr.span, format!("`{wrapper}` receives text from `{}`", cx.tcx.def_path_str(serializer)), |diag| {
                diag.span_note(serialized.span, "serialized here");
                diag.help(format!("pass `{value}` to `{direct}`"));
                if retyped {
                    diag.note(format!("`{direct}` returns `{returned}`"));
                }
            });
        }
    }
}
