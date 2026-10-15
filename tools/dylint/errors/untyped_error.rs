use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr, is_lint_allowed,
    res::{MaybeDef as _, MaybeResPath as _},
    ty::{same_type_modulo_regions, ty_from_hir_ty},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{
    AmbigArg, ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, FnRetTy, HirId, ImplItem, ImplItemKind, Item, ItemKind, TraitItem, TyKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::{self, GenericArg, Ty, print::with_forced_trimmed_paths};
use rustc_span::Span;

use super::{Untyped, classify, output};
use crate::{delivered, is_error, is_text, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    UNTYPED_ERROR,
    Warn,
    "`Result` errors and error variants holding text, unit, an erased error, a possibly empty error list, a local `Display` type lacking `Error`, or a report erasing first-party errors"
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] UntypedError => [UNTYPED_ERROR]);

struct Signature<'pass, 'tcx> {
    cx: &'pass LateContext<'tcx>,
    output: Option<Span>,
    exported: bool,
}

impl<'pass, 'tcx> Signature<'pass, 'tcx> {
    fn new(cx: &'pass LateContext<'tcx>, owner: LocalDefId) -> Self {
        let output = cx.tcx.hir_node_by_def_id(owner).fn_decl().and_then(|declaration| match declaration.output {
            FnRetTy::Return(output) => Some(output.span.source_callsite()),
            FnRetTy::DefaultReturn(_) => None,
        });
        Self { cx, output, exported: cx.effective_visibilities.is_exported(owner) && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none() }
    }
}

impl<'tcx> Visitor<'tcx> for Signature<'_, 'tcx> {
    fn visit_ty(&mut self, t: &'tcx rustc_hir::Ty<'tcx, AmbigArg>) {
        let hir_ty = t.as_unambig_ty();
        if matches!(hir_ty.kind, TyKind::Path(_)) && !hir_ty.span.from_expansion() {
            let output = self.output == Some(hir_ty.span);
            let held = ty_from_hir_ty(self.cx, hir_ty);
            match result_parts(self.cx, held) {
                Some((_, error)) =>
                    if let Some(untyped) = classify(self.cx, error)
                        && !(matches!(untyped, Untyped::Unit) && self.exported && output)
                        && (!matches!(untyped, Untyped::Report) || output)
                        && !matches!(*hir_ty.basic_res(), Res::Def(DefKind::TyAlias, alias) if alias.is_local()
                            && result_parts(self.cx, self.cx.tcx.type_of(alias).instantiate_identity().skip_normalization()).is_some_and(|(_, aliased)| classify(self.cx, aliased).is_some()))
                    {
                        report(self.cx, hir_ty.hir_id, hir_ty.span, error, untyped);
                    },
                None =>
                    if output && matches!(classify(self.cx, held), Some(Untyped::Many)) {
                        span_lint_hir_and_then(
                            self.cx,
                            UNTYPED_ERROR,
                            hir_ty.hir_id,
                            hir_ty.span,
                            with_forced_trimmed_paths!(format!("function returns `{held}`, where an empty collection stands for success")),
                            |diag| {
                                diag.help("return `Result<(), NEVec<E>>` built by collecting into `validated::Validated`");
                            },
                        );
                    },
            }
        }
        intravisit::walk_ty(self, t);
    }
}

impl<'tcx> LateLintPass<'tcx> for UntypedError {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let owner = item.owner_id.def_id;
        let checked = matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::Struct | DefKind::Enum | DefKind::Union | DefKind::TyAlias) && !skipped(cx, item.span, item.hir_id());
        if checked && matches!(item.kind, ItemKind::Struct(..) | ItemKind::Enum(..)) && is_error(cx, cx.tcx.type_of(owner).instantiate_identity().skip_normalization()) {
            let variants = cx.tcx.adt_def(owner).variants().iter().filter_map(|variant| {
                let mut fields = variant.fields.iter().map(|field| cx.tcx.type_of(field.did).instantiate_identity().skip_normalization());
                fields
                    .clone()
                    .filter_map(|field| classify(cx, field))
                    .find(|untyped| matches!(untyped, Untyped::Erased | Untyped::Many))
                    .or_else(|| (!variant.fields.is_empty() && fields.all(|field| is_text(cx, field))).then_some(Untyped::Text))
                    .zip(variant.def_id.as_local())
                    .map(|(untyped, local)| (variant, local, untyped))
            });
            for (variant, local, untyped) in variants {
                let help = if matches!(untyped, Untyped::Many) {
                    "hold the errors in a `nonempty_collections::NEVec`"
                } else {
                    "give the variant typed fields and the wrapped error's own type as its `#[source]`"
                };
                span_lint_hir_and_then(cx, UNTYPED_ERROR, cx.tcx.local_def_id_to_hir_id(local), cx.tcx.def_span(local), format!("error `{}` holds {}", variant.name, untyped.reason()), |diag| {
                    diag.help(help);
                });
            }
        }
        if checked {
            intravisit::walk_item(&mut Signature::new(cx, owner), item);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if !skipped(cx, item.span, item.hir_id()) {
            intravisit::walk_trait_item(&mut Signature::new(cx, item.owner_id.def_id), item);
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        fn error_alias(cx: &LateContext<'_>, declared: DefId) -> bool {
            cx.tcx.associated_items(cx.tcx.parent(declared)).in_definition_order().filter(|method| method.is_fn()).any(|method| {
                delivered(cx.tcx, method.def_id)
                    .walk()
                    .filter_map(GenericArg::as_type)
                    .any(|held| result_parts(cx, held).is_some_and(|(_, error)| matches!(error.kind(), ty::Alias(_, alias) if matches!(alias.kind, ty::Projection { def_id } if def_id == declared))))
            })
        }

        let owner = item.owner_id.def_id;
        if !skipped(cx, item.span, item.hir_id()) {
            match (cx.tcx.associated_item(owner).trait_item_def_id(), item.kind) {
                (None, _) => intravisit::walk_impl_item(&mut Signature::new(cx, owner), item),
                (Some(declared), ImplItemKind::Type(defined)) => {
                    intravisit::walk_impl_item(&mut Signature::new(cx, owner), item);
                    let error = cx.tcx.type_of(owner).instantiate_identity().skip_normalization();
                    if let Some(untyped) = classify(cx, error)
                        && error_alias(cx, declared)
                    {
                        report(cx, item.hir_id(), defined.span, error, untyped);
                    }
                }
                (Some(_), _) => {}
            }
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn mapped<'tcx>(cx: &LateContext<'tcx>, function: &Expr<'_>, returned: Ty<'tcx>) -> Option<Ty<'tcx>> {
            let typeck = cx.typeck_results();
            let call = get_parent_expr(cx, function)?;
            let ExprKind::MethodCall(_, receiver, ..) = call.kind else { return None };
            result_parts(cx, typeck.expr_ty(call))
                .map(|(_, error)| error)
                .filter(|error| same_type_modulo_regions(*error, returned) && !result_parts(cx, typeck.expr_ty(receiver).peel_refs()).is_some_and(|(_, held)| same_type_modulo_regions(held, *error)))
        }

        let typeck = cx.typeck_results();
        let raised = if expr.span.from_expansion() {
            None
        } else if let ExprKind::Closure(closure) = expr.kind
            && !matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn)))
        {
            output(cx, closure.def_id).and_then(|returned| result_parts(cx, returned).map(|(_, error)| error).or_else(|| mapped(cx, expr, returned))).map(|error| (closure.fn_decl_span, error))
        } else if let ExprKind::Path(_) = expr.kind
            && typeck.expr_ty(expr).is_fn()
        {
            mapped(cx, expr, typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder().output()).map(|error| (expr.span, error))
        } else if let ExprKind::Call(_, [_]) = expr.kind
            && fn_def_id(cx, expr).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr)
        {
            result_parts(cx, typeck.expr_ty(expr)).map(|(_, error)| (expr.span, error))
        } else if let ExprKind::Call(..) | ExprKind::MethodCall(..) = expr.kind
            && let Some(callee) = fn_def_id(cx, expr)
            && let produced = typeck.expr_ty(expr)
            && let error = result_parts(cx, produced).map_or(produced, |(_, error)| error)
            && error.ty_adt_def().is_some_and(|report| report.did().krate == callee.krate)
            && matches!(classify(cx, error), Some(Untyped::Report))
        {
            Some((expr.span, error))
        } else {
            None
        };
        if let Some((span, error)) = raised
            && let Some(untyped) = classify(cx, error)
            && !output(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)).and_then(|enclosing| result_parts(cx, enclosing)).is_some_and(|(_, enclosing)| same_type_modulo_regions(enclosing, error))
        {
            report(cx, expr.hir_id, span, error, untyped);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn skipped(cx: &LateContext<'_>, span: Span, node: HirId) -> bool { span.from_expansion() || is_lint_allowed(cx, UNTYPED_ERROR, node) }

fn report<'tcx>(cx: &LateContext<'tcx>, node: HirId, span: Span, error: Ty<'tcx>, untyped: Untyped) {
    let shown = if matches!(untyped, Untyped::Report) { error.to_string() } else { with_forced_trimmed_paths!(error.to_string()) };
    span_lint_hir_and_then(cx, UNTYPED_ERROR, node, span, format!("`Result` error `{shown}` is {}", untyped.reason()), |diag| match untyped {
        Untyped::NoError(declared) => {
            diag.span_note(cx.tcx.def_span(declared), "the type is declared here");
            diag.help("derive `thiserror::Error` on the type in place of its hand-written `Display`");
        }
        Untyped::Many => {
            diag.help("return `Result<T, NEVec<E>>` built by collecting into `validated::Validated`");
        }
        Untyped::Text | Untyped::Unit | Untyped::Erased | Untyped::Report => {
            diag.help("return a `thiserror` enum with a variant per failure and its typed source");
        }
    });
}
