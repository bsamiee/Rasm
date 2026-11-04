use core::ops::ControlFlow;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, fn_def_id_with_node_args, is_default_equivalent, is_default_equivalent_call, peel_blocks,
    res::MaybeDef as _,
    source::snippet,
    sym,
    visitors::{Descend, for_each_expr_without_closures, is_const_evaluatable},
};
use itertools::{Either, Itertools as _};
use rustc_hir::{
    Body, Expr, ExprField, ExprKind, FnDecl, HirIdSet, ItemKind, StructTailExpr,
    def::DefKind,
    def_id::{DefId, LocalDefId},
    intravisit::FnKind,
};
use rustc_infer::infer::TyCtxtInferExt as _;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{
    self, TypingEnv,
    adjustment::{Adjust, PointerCoercion},
};
use rustc_span::Span;
use rustc_trait_selection::traits::{Obligation, ObligationCause, ObligationCtxt};

use crate::{associated_trait, expanded_externally, impl_owner};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_DEFAULT_FIELD_VALUES: "hand-written `Default` impls building each field of a local struct from a constant, directly or through a constructor");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, _: FnKind<'tcx>, _: &'tcx FnDecl<'tcx>, body: &'tcx Body<'tcx>, span: Span, function: LocalDefId) {
        if let Some(implementation) = cx.tcx.trait_impl_of_assoc(function.to_def_id())
            && cx.tcx.impl_trait_id(implementation).is_diag_item(cx, sym::Default)
            && let value = peel_blocks(body.value)
            && let Some((construction, typeck, delegate)) = match value.kind {
                ExprKind::Call(&Expr { kind: ExprKind::Path(_), .. }, []) =>
                    fn_def_id(cx, value).and_then(DefId::as_local).and_then(|callee| Some((cx.tcx.hir_maybe_body_owned_by(callee)?, cx.tcx.typeck(callee), Some(callee)))),
                _ => Some((body, cx.typeck_results(), None)),
            }
            && let built = peel_blocks(construction.value)
            && let ExprKind::Struct(_, fields, StructTailExpr::None) = built.kind
            && let Some(owner) = typeck.expr_ty(built).ty_adt_def()
            && let Some(definition) = owner.did().as_local()
            && let ItemKind::Struct(_, _, declaration) = cx.tcx.hir_expect_item(definition).kind
            && let owner_ty = cx.tcx.type_of(owner.did()).instantiate_identity().skip_normalization()
            && let owner_env = cx.tcx.param_env(definition)
            && impl_owner(cx.tcx, function.to_def_id()) == Some(owner_ty)
            && cx.param_env == owner_env
            && !expanded_externally(cx, span)
            && !expanded_externally(cx, cx.tcx.def_span(definition))
            && declaration.fields().iter().all(|field| field.default.is_none())
            && let counts = cx.tcx.generics_of(owner.did()).own_counts()
            && counts.types == 0
            && counts.consts == 0
            && fields.iter().is_sorted_by_key(|field| typeck.field_index(field.hir_id))
            && let construction = (LateContext { enclosing_body: Some(construction.id()), typeck_results: Some(typeck), param_env: cx.tcx.param_env(built.hir_id.owner), ..*cx })
            && let (explicit, implicit) = fields.iter().partition_map::<Vec<&ExprField<'tcx>>, HirIdSet, _, _, _>(|field| {
                if for_each_expr_without_closures(field.expr, |part| {
                    let contract = fn_def_id(&construction, part).and_then(|called| associated_trait(cx.tcx, called));
                    if match part.kind {
                        ExprKind::Call(callee, []) => !is_default_equivalent_call(&construction, callee, None) || contract.is_some_and(|implemented| !implemented.is_diag_item(cx, sym::Default)),
                        ExprKind::Call(_, [_]) => !contract.is_diag_item(cx, sym::From),
                        _ => false,
                    } || typeck
                        .expr_adjustments(part)
                        .iter()
                        .filter(|adjustment| matches!(adjustment.kind, Adjust::Pointer(PointerCoercion::Unsize)))
                        .flat_map(|adjustment| adjustment.target.walk())
                        .filter_map(ty::GenericArg::as_type)
                        .any(|held| matches!(held.kind(), ty::Dynamic(..)))
                    {
                        ControlFlow::Break(())
                    } else {
                        ControlFlow::Continue(())
                    }
                })
                .is_some()
                    || !is_default_equivalent(&construction, field.expr)
                {
                    Either::Left(field)
                } else {
                    Either::Right(field.expr.hir_id)
                }
            })
            && explicit.iter().all(|field| is_const_evaluatable(cx.tcx, typeck, field.expr))
            && {
                let (infcx, param_env) = cx.tcx.infer_ctxt().build_with_typing_env(TypingEnv::non_body_analysis(cx.tcx, definition));
                let obligations = ObligationCtxt::new(&infcx);
                let cause = ObligationCause::dummy();
                let _: Option<()> = for_each_expr_without_closures(built, |part| {
                    if implicit.contains(&part.hir_id) {
                        return ControlFlow::Continue(Descend::No);
                    }
                    let Some((called, arguments)) = fn_def_id_with_node_args(&construction, part).filter(|&(called, _)| cx.tcx.is_conditionally_const(called)) else {
                        return ControlFlow::Continue(Descend::Yes);
                    };
                    obligations.register_obligations(cx.tcx.const_conditions(called).instantiate(cx.tcx, arguments).into_iter().map(|(bound, _)| {
                        let bound = obligations.normalize(&cause, param_env, bound);
                        Obligation::new(cx.tcx, cause.clone(), param_env, bound.to_host_effect_clause(cx.tcx, ty::BoundConstness::Const))
                    }));
                    ControlFlow::Continue(Descend::Yes)
                });
                obligations.evaluate_obligations_error_on_ambiguity().no_errors()
            }
            && (explicit.is_empty() || {
                let construction_owner = built.hir_id.owner.def_id;
                let parent = cx.tcx.parent(construction_owner.to_def_id());
                let scope = if matches!(cx.tcx.def_kind(parent), DefKind::Impl { .. }) { cx.tcx.parent(parent) } else { parent };
                let generics = cx.tcx.generics_of(construction_owner);
                scope == cx.tcx.parent(owner.did())
                    && matches!(cx.tcx.def_kind(scope), DefKind::Mod)
                    && (delegate.is_none() || associated_trait(cx.tcx, construction_owner.to_def_id()).is_none())
                    && generics.count() == cx.tcx.generics_of(owner.did()).count()
                    && generics.has_late_bound_regions.is_none()
                    && construction.param_env == owner_env
                    && typeck.expr_ty(built) == owner_ty
                    && impl_owner(cx.tcx, construction_owner.to_def_id()).is_none_or(|target| target == owner_ty)
            })
            && (delegate.is_some() || !explicit.is_empty())
        {
            let name = cx.tcx.item_name(owner.did());
            let derive = if cx.tcx.is_const_trait_impl(implementation) { "add `#[derive_const(Default)]` under `#![feature(derive_const)]`" } else { "derive `Default`" };
            let (source, help) = if explicit.is_empty() {
                ("its type's default", format!("{derive} and delete the impl"))
            } else {
                let declared = explicit
                    .iter()
                    .map(|field| {
                        let declared = &declaration.fields()[typeck.field_index(field.hir_id).as_usize()];
                        format!("`{} = {}`", snippet(cx, declared.span, ".."), snippet(cx, field.expr.span, ".."))
                    })
                    .join(", ");
                ("a constant", format!("declare {declared} under `#![feature(default_field_values)]`, {derive}, and delete the impl"))
            };
            let message = match delegate {
                Some(callee) => format!("`Default` for `{name}` builds each field from {source} through `{}`", cx.tcx.item_name(callee)),
                None => format!("`Default` for `{name}` builds each field from {source}"),
            };
            span_lint_hir_and_then(cx, MANUAL_DEFAULT_FIELD_VALUES, value.hir_id, value.span, message, |diag| {
                if delegate.is_some() {
                    diag.span_note(built.span, "fields built here");
                }
                diag.help(help);
            });
        }
    }
}
