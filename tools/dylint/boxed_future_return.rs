use core::{mem, ops::ControlFlow};

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, peel_blocks, visitors::for_each_expr};
use rustc_data_structures::fx::{FxHashSet, FxIndexMap};
use rustc_hir::{
    AmbigArg, Body, Expr, ExprKind, FnDecl, FnRetTy, MatchSource, TraitItem, TraitItemKind, TyKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::FnKind,
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::Ty;
use rustc_span::Span;

use crate::boxed_future;

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    BOXED_FUTURE_RETURN,
    Warn,
    "crate-private functions and trait methods returning `Pin<Box<dyn Future>>` that no function pointer or `dyn` use requires"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct BoxedFutureReturn {
    returns: FxIndexMap<LocalDefId, Span>,
    pointers: FxHashSet<LocalDefId>,
    declared: FxIndexMap<LocalDefId, Span>,
    objects: FxHashSet<DefId>,
}

impl_lint_pass!(BoxedFutureReturn => [BOXED_FUTURE_RETURN]);

impl<'tcx> LateLintPass<'tcx> for BoxedFutureReturn {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, kind: FnKind<'tcx>, declaration: &'tcx FnDecl<'tcx>, _: &'tcx Body<'tcx>, span: Span, item: LocalDefId) {
        if match kind {
            FnKind::ItemFn(..) => true,
            FnKind::Method(..) => cx.tcx.inherent_impl_of_assoc(item.to_def_id()).is_some(),
            FnKind::Closure => false,
        } && !span.from_expansion()
            && let FnRetTy::Return(output) = declaration.output
            && !cx.effective_visibilities.is_exported(item)
            && boxed_future(cx, returned(cx, item)).is_some()
        {
            self.returns.insert(item, output.span);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(signature, _) = item.kind
            && !item.span.from_expansion()
            && let FnRetTy::Return(output) = signature.decl.output
            && !cx.effective_visibilities.is_exported(item.owner_id.def_id)
            && boxed_future(cx, returned(cx, item.owner_id.def_id)).is_some()
        {
            self.declared.insert(item.owner_id.def_id, output.span);
        }
    }

    fn check_ty(&mut self, _: &LateContext<'tcx>, ty: &'tcx rustc_hir::Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(bounds, _) = ty.kind {
            self.objects.extend(bounds.iter().filter_map(|bound| bound.trait_ref.trait_def_id()));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Path(path) = &expr.kind
            && let Res::Def(DefKind::Fn | DefKind::AssocFn, function) = cx.qpath_res(path, expr.hir_id)
            && let Some(function) = function.as_local()
            && !get_parent_expr(cx, expr).is_some_and(|call| matches!(call.kind, ExprKind::Call(callee, _) if callee.hir_id == expr.hir_id))
        {
            self.pointers.insert(function);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (method, output) in mem::take(&mut self.declared).into_iter().filter(|(method, _)| !self.objects.contains(&cx.tcx.parent(method.to_def_id()))) {
            let declared = cx.tcx.parent(method.to_def_id());
            span_lint_hir_and_then(
                cx,
                BOXED_FUTURE_RETURN,
                cx.tcx.local_def_id_to_hir_id(method),
                output,
                format!("`{}` of trait `{}` returns a boxed `dyn Future`", cx.tcx.item_name(method), cx.tcx.item_name(declared)),
                |diag| {
                    diag.help(if boxed_future(cx, returned(cx, method)) == Some(true) {
                        format!("declare `async fn` and bound `Send` callers with `T: {}<{}(..): Send>`", cx.tcx.item_name(declared), cx.tcx.item_name(method))
                    } else {
                        "declare the method `async fn`, no `dyn` use of the trait needs a boxed future".to_owned()
                    });
                },
            );
        }
        for (function, output) in mem::take(&mut self.returns).into_iter().filter(|(function, _)| !self.pointers.contains(function)) {
            let body = cx.tcx.hir_body_owned_by(function).value;
            let typeck = cx.tcx.typeck(function);
            let recursive = for_each_expr(cx.tcx, body, |called| {
                let callee = if let ExprKind::Path(qpath) = &called.kind { typeck.qpath_res(qpath, called.hir_id).opt_def_id() } else { typeck.type_dependent_def_id(called.hir_id) };
                if callee == Some(function.to_def_id()) { ControlFlow::Break(()) } else { ControlFlow::Continue(()) }
            })
            .is_some();
            let help = if recursive {
                "declare `async fn` and box the recursive call"
            } else if matches!(peel_blocks(body).kind, ExprKind::If(..) | ExprKind::Match(_, _, MatchSource::Normal)) {
                "move the branch inside one `async move` block and return `impl Future`"
            } else {
                "return `impl Future` with the same bounds in place of `Pin<Box<dyn Future>>`"
            };
            span_lint_hir_and_then(cx, BOXED_FUTURE_RETURN, cx.tcx.local_def_id_to_hir_id(function), output, format!("`{}` returns a boxed `dyn Future`", cx.tcx.item_name(function)), |diag| {
                diag.help(help);
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn returned<'tcx>(cx: &LateContext<'tcx>, function: LocalDefId) -> Ty<'tcx> {
    cx.tcx.normalize_erasing_regions(cx.typing_env(), cx.tcx.fn_sig(function).instantiate_identity().map(|signature| cx.tcx.instantiate_bound_regions_with_erased(signature).output()))
}
