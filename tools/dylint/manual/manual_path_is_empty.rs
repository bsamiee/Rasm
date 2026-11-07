use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, std_or_core, sugg::Sugg};
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, path, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    view: Option<DefId>,
    empty: Option<DefId>,
}

lint!(Pass; MANUAL_PATH_IS_EMPTY: "`as_os_str().is_empty()` on a `Path` or `PathBuf`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { view: path(tcx, PathNS::Value, "std::path::Path::as_os_str"), empty: path(tcx, PathNS::Value, "std::ffi::OsStr::is_empty") } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(_, viewed, [], _) | ExprKind::Call(_, [viewed]) = expr.kind
            && let ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver]) = viewed.kind
            && self.empty.is_some_and(|empty| fn_def_id(cx, expr) == Some(empty))
            && self.view.is_some_and(|view| fn_def_id(cx, viewed) == Some(view))
            && !expanded_externally(cx, expr.span)
        {
            let mut applicability = Applicability::MachineApplicable;
            let argument = Sugg::hir_with_context(cx, receiver, expr.span.ctxt(), "..", &mut applicability);
            let argument = if cx.typeck_results().expr_ty(receiver).is_ref() { argument } else { argument.addr() };
            span_lint_hir_and_then(cx, MANUAL_PATH_IS_EMPTY, expr.hir_id, expr.span, "path emptiness is tested through `OsStr`", |diag| {
                let help = "call `Path::is_empty`";
                if std_or_core(cx) == Some("std") {
                    suggest(cx, expr.hir_id, diag, expr.span, help, format!("<::std::path::Path>::is_empty({argument})"), applicability);
                } else {
                    diag.help(help);
                }
            });
        }
    }
}
