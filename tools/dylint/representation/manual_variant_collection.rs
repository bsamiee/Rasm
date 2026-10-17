use clippy_utils::diagnostics::span_lint_hir_and_then;
use rustc_hir::Expr;
use rustc_lint::{LateContext, LateLintPass};

use super::{VariantCollection, variant_collection};
use crate::generated;

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_ENUM_MAP: "`BTreeMap`s built with one entry per variant of a fieldless local enum", MANUAL_VARIANT_ARRAY: "arrays listing every variant of a fieldless local enum in declaration order");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some((collection, keyed)) = variant_collection(cx, expr)
            && !generated(cx, expr.hir_id)
        {
            let name = cx.tcx.item_name(keyed.did());
            let (lint, message, help) = match collection {
                VariantCollection::Array =>
                    (MANUAL_VARIANT_ARRAY, format!("array lists every variant of `{name}` in declaration order"), format!("derive `strum::VariantArray` on `{name}` and read `{name}::VARIANTS`")),
                VariantCollection::Map => (
                    MANUAL_ENUM_MAP,
                    format!("`BTreeMap` built with one entry per variant of `{name}`"),
                    format!("build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `{name}`"),
                ),
            };
            span_lint_hir_and_then(cx, lint, expr.hir_id, expr.span, message, |diag| {
                diag.help(help);
            });
        }
    }
}
