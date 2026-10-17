use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, paths::PathNS, peel_blocks, res::MaybeResPath as _};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::TyCtxt;

use crate::{expanded_externally, fn_item_value, path, paths};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    partition: Option<DefId>,
    cases: FxHashSet<DefId>,
}

lint!(Pass; PARTITION_BY_RESULT: "`partition` calls splitting `Result` items with a named function that calls `is_ok` or `is_err`");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { partition: path(tcx, PathNS::Value, "core::iter::Iterator::partition"), cases: paths(tcx, PathNS::Value, &["core::result::Result::is_ok", "core::result::Result::is_err"]) }
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::MethodCall(segment, _, [predicate], _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && fn_def_id(cx, expr).is_some_and(|method| Some(method) == self.partition)
            && let Some(named) = fn_item_value(cx, predicate)
            && let Some(local) = named.as_local()
            && let Some(body) = cx.tcx.hir_maybe_body_owned_by(local)
            && let [parameter, ..] = body.params
            && let tested = peel_blocks(&body.value)
            && let ExprKind::MethodCall(_, receiver, [], _) = tested.kind
            && cx.tcx.typeck(local).type_dependent_def_id(tested.hir_id).is_some_and(|method| self.cases.contains(&method))
            && receiver.peel_borrows().res_local_id() == Some(parameter.pat.hir_id)
        {
            span_lint_hir_and_then(
                cx,
                PARTITION_BY_RESULT,
                expr.hir_id,
                segment.ident.span.to(expr.span.shrink_to_hi()),
                format!("`partition` splits results through `{}` and leaves each half wrapped in `Ok` or `Err`", cx.tcx.item_name(named)),
                |diag| {
                    diag.help("call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it");
                },
            );
        }
    }
}
