use clippy_utils::{as_some_pattern, diagnostics::span_lint_hir_and_then, fn_def_id, path_to_local_with_projections, paths::PathNS, res::MaybeResPath as _};
use rustc_data_structures::fx::FxHashMap;
use rustc_hir::{Expr, ExprKind, Node, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use super::in_drop_impl;
use crate::{expanded_externally, item_path, resolve};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    releases: FxHashMap<DefId, &'static str>,
}

lint!(Pass; MANUAL_ABORT_ON_DROP: "`Drop` impls that abort a task handle or `JoinSet`, or cancel a token, held in a field");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self {
            releases: resolve(
                tcx,
                PathNS::Value,
                &[
                    ("tokio::task::JoinHandle::abort", "store the handle as `tokio_util::task::AbortOnDropHandle` to abort its task on drop"),
                    ("tokio::task::AbortHandle::abort", "store the handle as `tokio_util::task::AbortOnDrop` to abort its task on drop"),
                    ("tokio::task::JoinSet::abort_all", "delete the call, dropping `JoinSet` aborts every task"),
                    ("tokio_util::sync::CancellationToken::cancel", "store the `DropGuard` from `CancellationToken::drop_guard` to cancel its token on drop"),
                ],
            ),
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver]) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let Some(method) = fn_def_id(cx, expr)
            && let Some(&help) = self.releases.get(&method)
            && in_drop_impl(cx, expr.hir_id)
            && let place = receiver
                .peel_borrows()
                .res_local_id()
                .and_then(|binding| cx.tcx.hir_parent_iter(binding).find(|(_, node)| !matches!(node, Node::Pat(_))))
                .and_then(|(_, node)| if let Node::Expr(Expr { kind: ExprKind::Let(test), .. }) = node { as_some_pattern(cx, test.pat).map(|_| test.init) } else { None })
                .unwrap_or(receiver)
                .peel_borrows()
            && let ExprKind::Field(_, field) = place.kind
            && let Some(this) = cx.tcx.hir_body_owned_by(cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(place.hir_id))).params.first()
            && path_to_local_with_projections(place) == Some(this.pat.hir_id)
        {
            span_lint_hir_and_then(cx, MANUAL_ABORT_ON_DROP, expr.hir_id, expr.span, format!("`Drop::drop` calls `{}` on field `{field}`", item_path(cx, method)), |diag| {
                diag.help(help);
            });
        }
    }
}
