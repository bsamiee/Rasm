use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, ty::option_arg_ty};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TyCtxt, print::with_forced_trimmed_paths};

use crate::{expanded_externally, is_text, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    defaulted: Option<DefId>,
}

lint!(Pass; SENTINEL_DEFAULT: "`Option::unwrap_or_default` calls turning an absent text, number, `bool`, or `char` into a default that callers cannot tell from a present value");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { defaulted: path(tcx, PathNS::Value, "core::option::Option::unwrap_or_default") } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, [], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| Some(method) == self.defaulted)
            && let Some(held) = option_arg_ty(cx, cx.typeck_results().expr_ty_adjusted(receiver).peel_refs())
            && (is_text(cx, held) || held.is_numeric() || held.is_bool() || held.is_char())
        {
            let held = with_forced_trimmed_paths!(held.to_string());
            span_lint_hir_and_then(
                cx,
                SENTINEL_DEFAULT,
                expr.hir_id,
                segment.ident.span.to(expr.span.shrink_to_hi()),
                format!("`unwrap_or_default` turns an absent `{held}` into a default that callers cannot tell from a present value"),
                |diag| {
                    diag.help("keep the `Option` and match it where a consumer acts on absence");
                },
            );
        }
    }
}
