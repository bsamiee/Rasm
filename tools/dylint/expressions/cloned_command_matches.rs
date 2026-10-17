use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, peel_hir_expr_refs, res::MaybeResPath as _, source::snippet};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, Node, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{cloned, expanded_externally, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    consuming: FxHashSet<DefId>,
}

lint!(Pass; CLONED_COMMAND_MATCHES: "`clap::Command` parameters and fields cloned for a consuming `get_matches` or `try_get_matches` call");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { consuming: paths(tcx, PathNS::Value, &["clap::Command::get_matches", "clap::Command::try_get_matches", "clap::Command::get_matches_from", "clap::Command::try_get_matches_from"]) }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, receiver, ..) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| self.consuming.contains(&method))
            && let Some(source) = cloned(cx, receiver).map(|source| peel_hir_expr_refs(source).0)
            && (matches!(source.kind, ExprKind::Field(..)) || source.res_local_id().is_some_and(|binding| matches!(cx.tcx.parent_hir_node(binding), Node::Param(_))))
        {
            let command = snippet(cx, source.span, "..");
            span_lint_hir_and_then(cx, CLONED_COMMAND_MATCHES, expr.hir_id, expr.span, format!("`{}` consumes a clone of `{command}`", segment.ident), |diag| {
                diag.help("take the command by `&mut` and call `get_matches_mut()`, or `try_get_matches_from_mut(<arguments>)` for the `_from` methods");
            });
        }
    }
}
