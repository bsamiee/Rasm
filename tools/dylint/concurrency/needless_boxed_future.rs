use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks, res::MaybeQPath as _};
use rustc_data_structures::fx::{FxHashSet, FxIndexMap};
use rustc_hir::{
    AmbigArg, Body, Expr, ExprKind, FnDecl, FnRetTy, ImplItem, ImplItemKind, MatchSource, TraitItem, TraitItemKind, TyKind,
    def_id::{DefId, LocalDefId},
    intravisit::FnKind,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::Span;

use super::boxed_future;
use crate::{any_expr, expanded_externally, exported, fn_item_value, signature};

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    returns: FxIndexMap<LocalDefId, Span>,
    pointers: FxHashSet<LocalDefId>,
    declared: FxIndexMap<LocalDefId, Span>,
    objects: FxHashSet<DefId>,
}

lint!(default Pass; NEEDLESS_BOXED_FUTURE: "crate-private functions, trait methods, and associated types naming `Pin<Box<dyn Future>>` that no function pointer or `dyn` use requires");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, kind: FnKind<'tcx>, declaration: &'tcx FnDecl<'tcx>, _: &'tcx Body<'tcx>, span: Span, item: LocalDefId) {
        if match kind {
            FnKind::ItemFn(..) => true,
            FnKind::Method(..) => cx.tcx.inherent_impl_of_assoc(item.to_def_id()).is_some(),
            FnKind::Closure => false,
        } && !expanded_externally(cx, span)
            && let FnRetTy::Return(output) = declaration.output
            && !exported(cx, item)
            && boxed_future(cx, signature(cx.tcx, item.to_def_id()).output()).is_some()
        {
            self.returns.insert(item, output.span);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if let TraitItemKind::Fn(declared, _) = item.kind
            && !expanded_externally(cx, item.span)
            && let FnRetTy::Return(output) = declared.decl.output
            && !exported(cx, item.owner_id.def_id)
            && boxed_future(cx, signature(cx.tcx, item.owner_id.to_def_id()).output()).is_some()
        {
            self.declared.insert(item.owner_id.def_id, output.span);
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Type(declared) = item.kind
            && !expanded_externally(cx, item.span)
            && !exported(cx, item.owner_id.def_id)
            && boxed_future(cx, cx.tcx.normalize_erasing_regions(cx.typing_env(), cx.tcx.type_of(item.owner_id).instantiate_identity())).is_some()
        {
            span_lint_hir_and_then(cx, NEEDLESS_BOXED_FUTURE, item.hir_id(), declared.span, format!("associated type `{}` is `Pin<Box<dyn Future>>`", item.ident), |diag| {
                diag.help(format!("declare `type {} = impl Future` with the same bounds under `#![feature(impl_trait_in_assoc_type)]`", item.ident));
            });
        }
    }

    fn check_ty(&mut self, _: &LateContext<'tcx>, ty: &'tcx rustc_hir::Ty<'tcx, AmbigArg>) {
        if let TyKind::TraitObject(bounds, _) = ty.kind {
            self.objects.extend(bounds.iter().filter_map(|bound| bound.trait_ref.trait_def_id()));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let Some(function) = fn_item_value(cx, expr).and_then(DefId::as_local) {
            self.pointers.insert(function);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (&method, &output) in self.declared.iter().filter(|(method, _)| !self.objects.contains(&cx.tcx.parent(method.to_def_id()))) {
            let (name, declared) = (cx.tcx.item_name(method), cx.tcx.item_name(cx.tcx.parent(method.to_def_id())));
            span_lint_hir_and_then(
                cx,
                NEEDLESS_BOXED_FUTURE,
                cx.tcx.local_def_id_to_hir_id(method),
                output,
                format!("`{declared}::{name}` returns `Pin<Box<dyn Future>>` with no `dyn {declared}` use"),
                |diag| {
                    diag.help(if boxed_future(cx, signature(cx.tcx, method.to_def_id()).output()) == Some(true) {
                        format!("declare `async fn {name}` and require `Send` at call sites with `T: {declared}<{name}(..): Send>`")
                    } else {
                        format!("declare `async fn {name}`")
                    });
                },
            );
        }
        for (&function, &output) in self.returns.iter().filter(|(function, _)| !self.pointers.contains(function)) {
            let body = &cx.tcx.hir_body_owned_by(function).value;
            let typeck = cx.tcx.typeck(function);
            let recursive = any_expr(cx.tcx, body, |called| called.res(typeck).opt_def_id().or_else(|| typeck.type_dependent_def_id(called.hir_id)) == Some(function.to_def_id()));
            let name = cx.tcx.item_name(function);
            let help = if recursive {
                "declare `async fn` and box the recursive call"
            } else if matches!(peel_blocks(body).kind, ExprKind::If(..) | ExprKind::Match(_, _, MatchSource::Normal)) {
                "move the branches into one `async move` block and return `impl Future`"
            } else {
                "return `impl Future` with the same bounds"
            };
            span_lint_hir_and_then(
                cx,
                NEEDLESS_BOXED_FUTURE,
                cx.tcx.local_def_id_to_hir_id(function),
                output,
                format!("`{name}` returns `Pin<Box<dyn Future>>` with no function pointer use"),
                |diag| {
                    diag.help(help);
                },
            );
        }
    }
}
