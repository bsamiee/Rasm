use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, TypeVisitableExt as _, print::with_forced_trimmed_paths};

use crate::{expanded_externally, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    deserialize: Option<DefId>,
}

lint!(Pass; DECODE_WITHOUT_PATH: "`Deserialize::deserialize` calls on a concrete deserializer that track no field path");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { deserialize: path(tcx, PathNS::Value, "serde::de::Deserialize::deserialize") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(callee, [deserializer]) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|called| self.deserialize == Some(called))
            && let source = cx.typeck_results().expr_ty(deserializer)
            && !source.has_param()
        {
            let source = with_forced_trimmed_paths!(source.to_string());
            span_lint_hir_and_then(cx, DECODE_WITHOUT_PATH, expr.hir_id, callee.span, format!("`Deserialize::deserialize` decodes `{source}` without tracking the field path"), |diag| {
                diag.help("decode through `serde_path_to_error::deserialize`");
            });
        }
    }
}
