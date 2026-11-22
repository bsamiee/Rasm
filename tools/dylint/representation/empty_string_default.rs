use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::paths::PathNS;
use clippy_utils::res::MaybeDef as _;
use clippy_utils::{DefinedTy, fn_def_id, get_expr_use_site, is_default_equivalent, is_default_equivalent_call, peel_blocks, sym};
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::def_id::DefId;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;
use rustc_middle::ty::{self, Ty, TyCtxt};
use rustc_span::STDLIB_STABLE_CRATES;

use crate::{expanded_externally, is_text, paths, pointee, resolve};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Fallback {
    Value,
    Producer,
    Default,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    fallbacks: FxHashMap<DefId, Fallback>,
    paths: FxHashSet<DefId>,
}

lint!(Pass; EMPTY_STRING_DEFAULT: "`Option` and `Result` fallbacks producing an empty string, path, or OS string outside an argument or field of an external non-standard crate");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            fallbacks: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("core::option::Option::unwrap_or", Fallback::Value),
                    ("core::option::Option::map_or", Fallback::Value),
                    ("core::option::Option::unwrap_or_else", Fallback::Producer),
                    ("core::option::Option::map_or_else", Fallback::Producer),
                    ("core::option::Option::unwrap_or_default", Fallback::Default),
                    ("core::option::Option::map_or_default", Fallback::Default),
                    ("core::result::Result::unwrap_or", Fallback::Value),
                    ("core::result::Result::map_or", Fallback::Value),
                    ("core::result::Result::unwrap_or_else", Fallback::Producer),
                    ("core::result::Result::map_or_else", Fallback::Producer),
                    ("core::result::Result::unwrap_or_default", Fallback::Default),
                    ("core::result::Result::map_or_default", Fallback::Default),
                ],
            ),
            paths: paths(tcx, PathNS::Type, &["std::ffi::OsStr", "std::ffi::OsString", "std::path::Path", "std::path::PathBuf", "camino::Utf8Path", "camino::Utf8PathBuf"]),
        }
    }

    fn textual<'tcx>(&self, cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool {
        match held.peel_refs().kind() {
            ty::Tuple(items) => items.iter().any(|item| self.textual(cx, item)),
            _ => is_text(cx, held) || pointee(cx, held).ty_adt_def().is_some_and(|adt| self.paths.contains(&adt.did())),
        }
    }

    fn empty<'tcx>(&self, cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> bool {
        let value = peel_blocks(value);
        match value.kind {
            ExprKind::Tup(items) => items.iter().any(|item| self.textual(cx, cx.typeck_results().expr_ty(item)) && self.empty(cx, item)),
            _ => is_default_equivalent(cx, value),
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let ExprKind::MethodCall(segment, receiver, arguments, _) = expr.kind else { return };
        let Some(&fallback) = fn_def_id(cx, expr).and_then(|callee| self.fallbacks.get(&callee)) else { return };
        let typeck = cx.typeck_results();
        let held = typeck.expr_ty(expr);
        let emptied = match (fallback, arguments.first()) {
            (Fallback::Default, _) => true,
            (Fallback::Value, Some(value)) => self.empty(cx, value),
            (Fallback::Producer, Some(Expr { kind: ExprKind::Closure(closure), .. })) => self.empty(cx, cx.tcx.hir_body(closure.body).value),
            (Fallback::Producer, Some(producer)) => is_default_equivalent_call(cx, producer, None),
            (Fallback::Value | Fallback::Producer, None) => false,
        };
        if emptied
            && !expanded_externally(cx, expr.span)
            && self.textual(cx, held)
            && let site = get_expr_use_site(cx.tcx, typeck, expr.span.ctxt(), expr).use_node(cx)
            && (site.is_recv()
                || !matches!(site.defined_ty(cx), Some(DefinedTy::Mir { def_site_def_id: Some(owner), .. }) if !owner.is_local() && !STDLIB_STABLE_CRATES.contains(&cx.tcx.crate_name(owner.krate))))
        {
            let absent = if typeck.expr_ty(receiver).peel_refs().is_diag_item(cx, sym::Option) { "None" } else { "Err" };
            let (span, held) = (segment.ident.span.to(expr.span.shrink_to_hi()), with_forced_trimmed_paths!(held.to_string()));
            span_lint_hir_and_then(cx, EMPTY_STRING_DEFAULT, expr.hir_id, span, format!("`{}` stands an empty `{held}` in for `{absent}`", segment.ident), |diag| {
                diag.help("keep the `Option` through to its reader, or `match` `Some` and `None` to choose the foreign call");
            });
        }
    }
}
