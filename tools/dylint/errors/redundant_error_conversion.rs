use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::res::MaybeDef as _;
use clippy_utils::source::walk_span_to_context;
use clippy_utils::ty::{implements_trait, same_type_modulo_regions};
use clippy_utils::{desugar_await, fn_def_id, is_from_proc_macro, peel_blocks, sym};
use itertools::Itertools as _;
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::def::{CtorKind, DefKind};
use rustc_hir::def_id::DefId;
use rustc_hir::{ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, FnRetTy, HirId, Item, ItemKind, MatchSource, Node};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::print::with_forced_trimmed_paths;
use rustc_middle::ty::{self, FieldDef, TypeVisitableExt as _, VariantDef};

use super::derives;
use crate::{associated_trait, awaited_output, constructed, derived_impl, expanded_externally, impl_owner, is_error, parameter, result_parts, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REDUNDANT_ERROR_CONVERSION: "`map_err` conversions that `?` applies, and hand-written `From` impls that a `#[from]` field replaces");

// --- [OPERATIONS] ----------------------------------------------------------------------

fn wrapped_variant<'tcx>(cx: &LateContext<'tcx>, implementation: DefId) -> Option<(&'tcx VariantDef, &'tcx FieldDef)> {
    if let [from_fn] = cx.tcx.associated_item_def_ids(implementation)
        && let Some(from_fn) = from_fn.as_local()
        && let body = cx.tcx.hir_body_owned_by(from_fn)
        && let Some((variant, fields)) = constructed(cx.tcx.typeck(from_fn), peel_blocks(body.value))
        && variant.fields.len() == 1
        && let Ok((field, value)) = fields.exactly_one()
        && parameter(body, value) == Some(0)
    {
        Some((variant, field))
    } else {
        None
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn inferred_return(cx: &LateContext<'_>, node: HirId) -> bool {
            matches!(cx.tcx.hir_node_by_def_id(cx.tcx.hir_enclosing_body_owner(node)), Node::Expr(owner)
                if matches!(owner.kind, ExprKind::Closure(closure)
                    if matches!(closure.fn_decl.output, FnRetTy::DefaultReturn(_)) && !matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn)))))
        }

        fn fixed_error(cx: &LateContext<'_>, source: &Expr<'_>) -> bool {
            fn_def_id(cx, desugar_await(source).unwrap_or(source)).and_then(|callee| result_parts(cx, awaited_output(cx.tcx, callee))).is_some_and(|(_, error)| !error.has_non_region_param())
        }

        let typeck = cx.typeck_results();
        let forwarding_call = |handler: &Expr<'_>| match handler.kind {
            ExprKind::Closure(closure)
                if let body = cx.tcx.hir_body(closure.body)
                    && matches!(body.params, [_])
                    && let value = peel_blocks(body.value)
                    && let ExprKind::Call(_, [argument]) | ExprKind::MethodCall(_, argument, [], _) = value.kind
                    && parameter(body, argument) == Some(0) =>
                fn_def_id(cx, value),
            _ if let ty::FnDef(function, _) = *typeck.expr_ty(handler).kind() => Some(function),
            _ => None,
        };
        if let ExprKind::Match(branch, arms, MatchSource::TryDesugar(_)) = expr.kind
            && let ExprKind::Call(_, [converted]) = branch.kind
            && let ExprKind::MethodCall(segment, source, [conversion], _) = converted.kind
            && segment.ident.name == sym::map_err
            && fn_def_id(cx, converted).and_then(|method| impl_owner(cx.tcx, method)).is_diag_item(cx, sym::Result)
            && !expanded_externally(cx, converted.span)
            && fixed_error(cx, source)
            && !inferred_return(cx, expr.hir_id)
            && let Some(conversion_fn) = forwarding_call(conversion)
            && let Some((_, source_error)) = result_parts(cx, typeck.expr_ty(source))
            && let Some(returned) = arms.iter().find_map(|arm| if let ExprKind::Ret(Some(returned)) | ExprKind::Break(_, Some(returned)) = arm.body.kind { Some(returned) } else { None })
            && let residual = fn_def_id(cx, returned)
            && let homogeneous = residual.is_lang_item(cx, LangItem::ResidualIntoTryType)
            && (homogeneous || residual.is_lang_item(cx, LangItem::TryTraitFromResidual))
            && let Some((_, target_error)) = result_parts(cx, typeck.expr_ty(returned))
            && !same_type_modulo_regions(source_error, target_error)
            && result_parts(cx, typeck.expr_ty(converted)).is_some_and(|(_, converted_error)| same_type_modulo_regions(converted_error, target_error))
            && let Some(from) = cx.tcx.get_diagnostic_item(sym::From)
            && implements_trait(cx, target_error, from, &[source_error.into()])
            && (matches!(associated_trait(cx.tcx, conversion_fn).opt_diag_name(cx), Some(sym::From | sym::Into))
                || matches!(cx.tcx.def_kind(conversion_fn), DefKind::Ctor(_, CtorKind::Fn))
                    && cx.tcx.non_blanket_impls_for_ty(from, target_error).any(|implementation| {
                        same_type_modulo_regions(cx.tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization().args.type_at(1), source_error)
                            && wrapped_variant(cx, implementation).is_some_and(|(variant, _)| variant.def_id == cx.tcx.parent(conversion_fn))
                    }))
            && let Some(receiver) = walk_span_to_context(source.span, converted.span.ctxt())
            && !is_from_proc_macro(cx, converted)
        {
            let removed = converted.span.with_lo(receiver.hi());
            if homogeneous {
                span_lint_hir_and_then(cx, REDUNDANT_ERROR_CONVERSION, converted.hir_id, removed, "`?` in a homogeneous `try` block applies no `From`, `map_err` converts by hand", |diag| {
                    diag.help("name the block's type with `try bikeshed <result> { .. }` and delete the `map_err` call, each `?` converting into it");
                });
            } else {
                span_lint_hir_and_then(cx, REDUNDANT_ERROR_CONVERSION, converted.hir_id, removed, "`map_err` repeats the `From` conversion `?` applies", |diag| {
                    suggest(cx, converted.hir_id, diag, removed, "delete the `map_err` call", "", Applicability::MachineApplicable);
                });
            }
        }
    }

    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let declared = item.owner_id.to_def_id();
        if matches!(item.kind, ItemKind::Impl(_))
            && !expanded_externally(cx, item.span)
            && !derived_impl(cx.tcx, declared)
            && cx.tcx.impl_opt_trait_id(declared).is_diag_item(cx, sym::From)
            && let conversion = cx.tcx.impl_trait_ref(declared).instantiate_identity().skip_normalization()
            && let (target, source) = (conversion.self_ty(), conversion.args.type_at(1))
            && !source.has_param()
            && is_error(cx, source)
            && derives(cx, target, sym::Error)
            && let Some((variant, field)) = wrapped_variant(cx, declared)
        {
            span_lint_hir_and_then(
                cx,
                REDUNDANT_ERROR_CONVERSION,
                item.hir_id(),
                cx.tcx.def_span(declared),
                with_forced_trimmed_paths!(format!("`impl From<{source}> for {target}` wraps the source in `{}` by hand", variant.name)),
                |diag| {
                    diag.span_note(cx.tcx.def_span(field.did), "source field");
                    diag.help("mark the field `#[from]` and delete the impl");
                },
            );
        }
    }
}
