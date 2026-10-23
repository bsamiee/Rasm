use clippy_utils::{diagnostics::span_lint_hir_and_then, sym};
use rustc_hir::FnRetTy;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

use crate::{derives, is_error, result_parts, signature};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(DEBUG_MAIN_ERROR: "`main` functions returning a `Result` error that `Termination` prints through a derived `Debug`");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        if let Some((entry, _)) = cx.tcx.entry_fn(())
            && let Some(main) = entry.as_local()
            && let Some(declaration) = cx.tcx.hir_node_by_def_id(main).fn_decl()
            && let FnRetTy::Return(output) = declaration.output
            && let Some((ok, error)) = result_parts(cx, signature(cx.tcx, entry).output())
            && is_error(cx, error)
            && derives(cx, error, sym::Debug)
        {
            span_lint_hir_and_then(
                cx,
                DEBUG_MAIN_ERROR,
                cx.tcx.local_def_id_to_hir_id(main),
                output.span.source_callsite(),
                with_forced_trimmed_paths!(format!("`Termination` prints `{error}` from `main` through its derived `Debug`")),
                |diag| {
                    diag.help(with_forced_trimmed_paths!(format!("return `Result<{ok}, std::error::Report<{error}>>` under `#![feature(error_reporter)]`")));
                },
            );
        }
    }
}
