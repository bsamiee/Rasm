use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::res::MaybeDef as _;
use clippy_utils::sym;
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::def_id::DefId;
use rustc_hir::{Expr, ExprKind, HirId, ImplItem, ImplItemKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;

use crate::expanded_externally;

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    guards: Vec<(HirId, DefId, ty::ParamTy)>,
}

lint!(default Pass; MANUAL_DROP_GUARD: "one-field structs with a `Drop` impl that calls the generic closure they hold");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Fn(_, _) = item.kind
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
            && let ty::Param(parameter) = closure.kind()
            && !expanded_externally(cx, item.span)
        {
            self.guards.push((item.hir_id(), guard.did(), *parameter));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Call(callee, _) = expr.kind
            && let Some(&(function, guard, parameter)) = self.guards.last()
            && cx.tcx.hir_enclosing_body_owner(expr.hir_id) == function.owner.def_id
            && let typeck = cx.typeck_results()
            && let closure = typeck.expr_ty(callee)
            && matches!(closure.kind(), ty::Param(called) if *called == parameter)
        {
            self.guards.pop();
            let name = cx.tcx.item_name(guard);
            span_lint_hir_and_then(cx, MANUAL_DROP_GUARD, function, cx.tcx.def_span(guard), format!("`Drop` for `{name}` calls the closure `{closure}` it holds"), |diag| {
                diag.help(format!("replace `{name}` with `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where code disarms the guard"));
            });
        }
    }

    fn check_impl_item_post(&mut self, _: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) { self.guards.pop_if(|(function, _, _)| *function == item.hir_id()); }
}
