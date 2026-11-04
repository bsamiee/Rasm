use clippy_utils::{diagnostics::span_lint_hir_and_then, expr_or_init, fn_def_id, paths::PathNS, peel_hir_expr_while, res::MaybeDef as _, source::snippet, sym, ty::same_type_modulo_regions};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::{Expr, ExprKind, MatchSource, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, impl_owner, item_path, paths, result_parts, signature};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    wrappers: FxHashMap<DefId, DefId>,
    serializers: FxHashSet<DefId>,
}

lint!(Pass; MANUAL_SERIALIZATION: "serialized text passed to a constructor with a counterpart that serializes the value itself");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        let direct = paths(tcx, PathNS::Value, &["rmcp::model::ContentBlock::json"]);
        Self {
            wrappers: paths(tcx, PathNS::Value, &["rmcp::model::ContentBlock::text"])
                .into_iter()
                .filter_map(|wrapper| Some((wrapper, *direct.iter().find(|direct| direct.krate == wrapper.krate)?)))
                .collect(),
            serializers: paths(tcx, PathNS::Value, &["serde_json::to_string"]),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn serialized<'tcx>(cx: &LateContext<'tcx>, argument: &'tcx Expr<'tcx>, serializers: &FxHashSet<DefId>) -> Option<(DefId, &'tcx Expr<'tcx>, &'tcx Expr<'tcx>)> {
            let typeck = cx.typeck_results();
            let value = peel_hir_expr_while(argument, |value| {
                let initialized = expr_or_init(cx, value.peel_borrows());
                match initialized.kind {
                    _ if initialized.hir_id != value.hir_id => Some(initialized),
                    ExprKind::Match(branch, _, MatchSource::TryDesugar(_)) if let ExprKind::Call(_, [tried]) = branch.kind => Some(tried),
                    ExprKind::MethodCall(_, receiver, ..)
                        if let Some(method) = fn_def_id(cx, initialized)
                            && impl_owner(cx.tcx, method).is_diag_item(cx, sym::Result)
                            && matches!(cx.tcx.item_name(method), sym::unwrap | sym::expect | sym::unwrap_or | sym::unwrap_or_default | sym::map_err)
                            && let Some((held, _)) = result_parts(cx, typeck.expr_ty(receiver))
                            && let produced = typeck.expr_ty(initialized)
                            && same_type_modulo_regions(result_parts(cx, produced).map_or(produced, |(ok, _)| ok), held) =>
                        Some(receiver),
                    _ => None,
                }
            });
            if let ExprKind::Call(_, [operand]) = value.kind
                && let Some(serializer) = fn_def_id(cx, value).filter(|serializer| serializers.contains(serializer))
            {
                Some((serializer, value, operand))
            } else {
                None
            }
        }

        if let ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(wrapper) = fn_def_id(cx, expr)
            && let Some(direct) = self.wrappers.get(&wrapper)
            && let Some((serializer, serialized, value)) = arguments.iter().find_map(|argument| serialized(cx, argument, &self.serializers))
        {
            let returned = signature(cx.tcx, *direct).output();
            let retyped = !same_type_modulo_regions(signature(cx.tcx, wrapper).output(), returned);
            let (wrapper, direct, returned) = with_forced_trimmed_paths!((item_path(cx, wrapper), item_path(cx, *direct), returned.to_string()));
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
