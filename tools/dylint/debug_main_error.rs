use clippy_utils::{diagnostics::span_lint_hir_and_then, sym};
use rustc_hir::FnRetTy;
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::print::with_forced_trimmed_paths;

use crate::{derives, is_error, result_parts, signature};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    DEBUG_MAIN_ERROR,
    Warn,
    "`main` returning a `Result` whose error `Termination` prints through a derived `Debug`"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] DebugMainError => [DEBUG_MAIN_ERROR]);

impl<'tcx> LateLintPass<'tcx> for DebugMainError {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        if let Some((entry, _)) = cx.tcx.entry_fn(())
            && let Some(main) = entry.as_local()
            && let Some(declaration) = cx.tcx.hir_node_by_def_id(main).fn_decl()
            && let FnRetTy::Return(output) = declaration.output
            && let Some((_, error)) = result_parts(cx, signature(cx.tcx, entry).output())
            && is_error(cx, error)
            && derives(cx, error, sym::Debug)
        {
            span_lint_hir_and_then(
                cx,
                DEBUG_MAIN_ERROR,
                cx.tcx.local_def_id_to_hir_id(main),
                output.span.source_callsite(),
                with_forced_trimmed_paths!(format!("`main` returns `{error}` to `Termination`, which prints its derived `Debug`")),
                |diag| {
                    diag.help(with_forced_trimmed_paths!(format!(
                        "return `Result<_, std::error::Report<{error}>>` under `#![feature(error_reporter)]`, its `Debug` prints the error's `Display` and `sources()` chain"
                    )));
                },
            );
        }
    }
}
