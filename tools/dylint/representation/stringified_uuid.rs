use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then, expr_or_init, fn_def_id, macros::root_macro_call_first_node, paths::PathNS, res::MaybeDef as _, sym, visitors::for_each_expr_without_closures,
};
use rustc_hir::{Expr, ExprKind, def_id::CrateNum};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};

use crate::{Access, Place, access, constructed, expanded_externally, is_text, path, place};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    uuid: Option<CrateNum>,
}

lint!(Pass; STRINGIFIED_UUID: "text fields of local structs set from a `Uuid` rendered to text");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { uuid: path(tcx, PathNS::Type, "uuid::Uuid").map(|uuid| uuid.krate) } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let identifier = |held: Ty<'_>| held.peel_refs().ty_adt_def().is_some_and(|adt| Some(adt.did().krate) == self.uuid);
        let rendered = |value: &'tcx Expr<'tcx>| {
            let value = expr_or_init(cx, value);
            match value.kind {
                _ if let Some(call) = root_macro_call_first_node(cx, value).filter(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)) =>
                    for_each_expr_without_closures(value, |part| if identifier(typeck.expr_ty(part)) { ControlFlow::Break(call.span) } else { ControlFlow::Continue(()) }),
                ExprKind::MethodCall(_, receiver, [], _) if fn_def_id(cx, value).is_diag_item(cx, sym::to_string_method) && identifier(typeck.expr_ty(receiver)) => Some(value.span),
                _ => None,
            }
        };
        if expanded_externally(cx, expr.span) {
            return;
        }
        let built = constructed(typeck, expr)
            .into_iter()
            .flat_map(|(_, fields)| fields)
            .filter(|(field, _)| field.did.is_local() && is_text(cx, cx.tcx.type_of(field.did).instantiate_identity().skip_normalization()))
            .map(|(field, value)| (field.name, value));
        let assigned = if let Some(Place::Field(field)) = place(cx, expr)
            && let Access::Assigned(value) = access(cx, expr)
        {
            Some((cx.tcx.item_name(field), value))
        } else {
            None
        };
        for (field, span) in built.chain(assigned).filter_map(|(field, value)| Some((field, rendered(value)?))) {
            span_lint_hir_and_then(cx, STRINGIFIED_UUID, expr.hir_id, span, format!("field `{field}` holds a `Uuid` rendered to text"), |diag| {
                diag.help(format!("declare `{field}` as `newtype_uuid::TypedUuid<<Kind>>` for an entity identifier or `uuid::Uuid` otherwise, `Display` and serde render either"));
            });
        }
    }
}
