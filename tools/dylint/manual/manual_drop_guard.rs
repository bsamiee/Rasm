use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{ExprKind, ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;

use crate::{any_expr, expanded_externally};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_DROP_GUARD: "one-field structs with a `Drop` impl that calls the generic closure they hold");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Fn(_, body) = item.kind
            && !expanded_externally(cx, item.span)
            && let Some(implementation) = cx.tcx.trait_impl_of_assoc(item.owner_id.to_def_id())
            && cx.tcx.impl_trait_id(implementation).is_lang_item(cx, LangItem::Drop)
            && let ty::Adt(guard, arguments) = cx.tcx.type_of(implementation).instantiate_identity().skip_normalization().kind()
            && guard.is_struct()
            && let [field] = &guard.non_enum_variant().fields.raw
            && let held = field.ty(cx.tcx, arguments).skip_normalization()
            && let closure = if let ty::Adt(wrapper, inner) = held.kind()
                && (wrapper.is_diag_item(cx, sym::Option) || wrapper.is_lang_item(cx, LangItem::ManuallyDrop))
            {
                inner.type_at(0)
            } else {
                held
            }
            && matches!(closure.kind(), ty::Param(_))
            && let typeck = cx.tcx.typeck_body(body)
            && any_expr(cx.tcx, &cx.tcx.hir_body(body).value, |called| matches!(called.kind, ExprKind::Call(callee, _) if typeck.expr_ty(callee) == closure))
        {
            let name = cx.tcx.item_name(guard.did());
            span_lint_hir_and_then(cx, MANUAL_DROP_GUARD, item.hir_id(), cx.tcx.def_span(guard.did()), format!("`Drop` for `{name}` calls the closure `{closure}` it holds"), |diag| {
                diag.help(format!("replace `{name}` with `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where code disarms the guard"));
            });
        }
    }
}
