#![feature(rustc_private)]

extern crate rustc_hir;

use clippy_utils::{
    diagnostics::span_lint_and_help,
    expr_or_init, get_enclosing_block,
    paths::{PathNS, lookup_path_str},
    res::MaybeResPath as _,
    visitors::local_used_once,
};
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub DIR_ENTRY_PATH_FILE_NAME,
    Warn,
    "`DirEntry::path` builds a path to read the name `DirEntry::file_name` returns",
    DirEntryPathFileName::default()
}

#[derive(Default)]
struct DirEntryPathFileName {
    entry_paths: Vec<DefId>,
    file_names: Vec<DefId>,
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for DirEntryPathFileName {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) {
        self.entry_paths = ["std::fs::DirEntry::path", "tokio::fs::DirEntry::path", "fs_err::DirEntry::path", "fs_err::tokio::DirEntry::path"]
            .into_iter()
            .flat_map(|path| lookup_path_str(cx.tcx, PathNS::Value, path))
            .collect();
        self.file_names = lookup_path_str(cx.tcx, PathNS::Value, "std::path::Path::file_name");
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        if let ExprKind::MethodCall(_, receiver, [], _) = expr.kind
            && !expr.span.from_expansion()
            && typeck.type_dependent_def_id(expr.hir_id).is_some_and(|file_name| self.file_names.contains(&file_name))
            && receiver.res_local_id().is_none_or(|local| get_enclosing_block(cx, local).is_some_and(|block| local_used_once(cx, block, local).is_some()))
            && let path = expr_or_init(cx, receiver)
            && let ExprKind::MethodCall(_, _, [], _) = path.kind
            && typeck.type_dependent_def_id(path.hir_id).is_some_and(|entry_path| self.entry_paths.contains(&entry_path))
        {
            span_lint_and_help(cx, DIR_ENTRY_PATH_FILE_NAME, expr.span, DIR_ENTRY_PATH_FILE_NAME.desc, None, "call `file_name` on the entry for the name as an `OsString`");
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
