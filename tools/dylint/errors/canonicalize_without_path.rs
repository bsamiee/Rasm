use clippy_utils::{diagnostics::span_lint_hir_and_then, paths::PathNS};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    canonicalize: Option<DefId>,
}

lint!(Pass; CANONICALIZE_WITHOUT_PATH: "`std::path::Path::canonicalize` uses, whose `io::Error` names no path and whose Windows result holds the verbatim prefix");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { canonicalize: path(tcx, PathNS::Value, "std::path::Path::canonicalize") } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let (called, span) = match &expr.kind {
            ExprKind::Path(qpath) => (cx.qpath_res(qpath, expr.hir_id).opt_def_id(), expr.span),
            ExprKind::MethodCall(segment, ..) => (cx.typeck_results().type_dependent_def_id(expr.hir_id), segment.ident.span),
            _ => return,
        };
        if let Some(banned) = self.canonicalize
            && called == Some(banned)
            && !expanded_externally(cx, expr.span)
        {
            span_lint_hir_and_then(
                cx,
                CANONICALIZE_WITHOUT_PATH,
                expr.hir_id,
                span,
                "`Path::canonicalize` returns an `io::Error` that names no path and a Windows path that holds the verbatim prefix",
                |diag| {
                    diag.help("use `dunce::simplified(&fs_err::canonicalize(..)?)`");
                },
            );
        }
    }
}
