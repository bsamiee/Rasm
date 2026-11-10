use core::num::IntErrorKind;
use core::ops::ControlFlow;

use clippy_utils::res::MaybeDef as _;
use clippy_utils::ty::implements_trait;
use clippy_utils::visitors::{for_each_expr_without_closures, is_local_used};
use clippy_utils::{fn_def_id, peel_blocks, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::{Expr, ExprKind, PatKind};
use rustc_lint::LateContext;
use rustc_middle::ty::fast_reject::DeepRejectCtxt;
use rustc_middle::ty::layout::LayoutOf as _;
use rustc_middle::ty::{self, GenericArg, Ty, TypeVisitableExt as _, TypeckResults};
use rustc_span::Symbol;

use crate::{associated_trait, bounds_of, derived_impl, impl_owner, is_error, is_text, pointee, result_parts, workspace_def};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, strum::IntoStaticStr)]
enum UntypedKind<'tcx> {
    #[strum(serialize = "text callers cannot match")]
    Text,
    #[strum(serialize = "a primitive that names no failure")]
    Code,
    #[strum(serialize = "a tuple that names no failure")]
    Tuple,
    #[strum(serialize = "a unit type that names no failure")]
    Unit,
    #[strum(serialize = "an erased `dyn Error` callers cannot match")]
    Erased,
    #[strum(serialize = "a report that erases each failure's type")]
    Report,
    #[strum(serialize = "a collection of errors that can be empty")]
    Many(Ty<'tcx>),
    #[strum(serialize = "a collection of text callers cannot match")]
    Messages,
    #[strum(serialize = "a `Display` type that implements no `core::error::Error`")]
    NoError(DefId),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn derives<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>, derived: Symbol) -> bool {
    cx.tcx.get_diagnostic_item(derived).is_some_and(|implemented| cx.tcx.non_blanket_impls_for_ty(implemented, ty).any(|implementation| derived_impl(cx.tcx, implementation)))
}

fn singular<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> bool {
    !error.has_param()
        && is_error(cx, error)
        && error.ty_adt_def().is_some_and(|adt| adt.variants().len() == 1 && !adt.is_variant_list_non_exhaustive() && adt.variants().iter().all(|variant| !variant.is_field_list_non_exhaustive()))
        && !error.needs_drop(cx.tcx, cx.typing_env())
        && cx.layout_of(error).is_ok_and(|layout| layout.is_zst() && !layout.is_uninhabited())
}

fn has_single_failure_cause<'tcx>(cx: &LateContext<'tcx>, conversion: &'tcx Expr<'tcx>) -> bool {
    let typeck = cx.typeck_results();
    let without_abrupt_exits = |root: &'tcx Expr<'tcx>| {
        for_each_expr_without_closures(root, |part| match part.kind {
            ExprKind::Ret(..) | ExprKind::Break(..) | ExprKind::Continue(..) => ControlFlow::Break(()),
            _ => ControlFlow::Continue(()),
        })
        .is_none()
        .then_some(root)
    };
    let proof: Option<_> = try {
        let mut pending = vec![without_abrupt_exits(conversion)?];
        let mut failure = None;
        while let Some(expr) = pending.pop() {
            let expr = peel_blocks(expr);
            let selected = if let ty::FnDef(callee, arguments) = *typeck.expr_ty(expr).kind() { Some((callee, arguments)) } else { None };
            let callee = selected.map(|(callee, _)| callee).or_else(|| fn_def_id(cx, expr))?;
            let owner = associated_trait(cx.tcx, callee);
            let array = owner.is_none() && impl_owner(cx.tcx, callee).is_lang_item(cx, LangItem::AllocLayout) && cx.tcx.item_name(callee) == sym::array;
            if array || matches!(owner.opt_diag_name(cx), Some(sym::TryFrom | sym::TryInto)) {
                let (source, target) = if let Some((_, arguments)) = selected {
                    let declared = cx.tcx.fn_sig(callee).instantiate(cx.tcx, arguments.no_bound_vars()?).skip_normalization().skip_binder();
                    (declared.inputs()[0], result_parts(cx, declared.output())?.0)
                } else {
                    let source = match expr.kind {
                        ExprKind::Call(_, [source]) | ExprKind::MethodCall(_, source, [], _) => typeck.expr_ty_adjusted(source),
                        _ => return false,
                    };
                    (source, result_parts(cx, typeck.expr_ty(expr))?.0)
                };
                let cause = if array {
                    None
                } else {
                    Some(match (source.kind(), target.kind()) {
                        (ty::Uint(_), ty::Int(_) | ty::Uint(_)) => IntErrorKind::PosOverflow,
                        (ty::Int(_), ty::Uint(_)) if source.int_size_and_signed(cx.tcx).0 <= target.int_size_and_signed(cx.tcx).0 => IntErrorKind::NegOverflow,
                        _ => return false,
                    })
                };
                if failure.replace(cause).is_some_and(|previous| previous != cause) {
                    return false;
                }
            } else if impl_owner(cx.tcx, callee).is_diag_item(cx, sym::Result) {
                let (ExprKind::MethodCall(_, receiver, [handler], _) | ExprKind::Call(_, [receiver, handler])) = expr.kind else { return false };
                match cx.tcx.item_name(callee) {
                    sym::map => pending.push(receiver),
                    sym::and_then => pending.extend([receiver, if let ExprKind::Closure(closure) = handler.kind { without_abrupt_exits(cx.tcx.hir_body(closure.body).value)? } else { handler }]),
                    _ => return false,
                }
            } else {
                return false;
            }
        }
        failure?
    };
    proof.is_some()
}

fn untyped_kind<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> Option<UntypedKind<'tcx>> {
    let (error_trait, display) = (cx.tcx.get_diagnostic_item(sym::Error)?, cx.tcx.get_diagnostic_item(sym::Display)?);
    match error.kind() {
        _ if error.is_unit() => Some(UntypedKind::Unit),
        _ if is_text(cx, error) => Some(UntypedKind::Text),
        ty::Int(_) | ty::Uint(_) | ty::Char | ty::Bool => Some(UntypedKind::Code),
        ty::Tuple(_) => Some(UntypedKind::Tuple),
        _ if matches!(pointee(cx, error).kind(), ty::Dynamic(predicates, ..) if predicates.principal_def_id() == Some(error_trait)) => Some(UntypedKind::Erased),
        ty::Adt(report, _)
            if !report.did().is_local()
                && !implements_trait(cx, error, error_trait, &[])
                && cx.tcx.get_diagnostic_item(sym::From).is_some_and(|from| {
                    cx.tcx.non_blanket_impls_for_ty(from, error).any(|implementation| {
                        let converted = cx.tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization();
                        let source = converted.args.type_at(1);
                        matches!(source.kind(), ty::Param(_))
                            && DeepRejectCtxt::relate_rigid_infer(cx.tcx).types_may_unify(error, converted.self_ty())
                            && !converted.self_ty().walk().any(|held| held == source.into())
                            && bounds_of(cx.tcx, implementation, source).any(|bound| bound.def_id() == error_trait)
                    })
                }) =>
            Some(UntypedKind::Report),
        ty::Adt(collection, args) if matches!(collection.opt_diag_name(cx), Some(sym::Vec | sym::BTreeSet)) =>
            is_error(cx, args.type_at(0)).then_some(UntypedKind::Many(args.type_at(0))).or_else(|| is_text(cx, args.type_at(0)).then_some(UntypedKind::Messages)),
        _ => error
            .opt_def_id()
            .filter(|_| {
                !error.has_param()
                    && implements_trait(cx, error, display, &[])
                    && !implements_trait(cx, error, error_trait, &[])
                    && !error.walk().skip(1).filter_map(GenericArg::as_type).any(|held| is_error(cx, held))
            })
            .map(UntypedKind::NoError),
    }
}

fn failing<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> bool {
    !error.is_privately_uninhabited(cx.tcx, cx.typing_env())
        && (is_error(cx, error)
            || error.ty_adt_def().is_some_and(|adt| workspace_def(cx, adt.did()))
            || match untyped_kind(cx, error) {
                Some(UntypedKind::Text | UntypedKind::Erased | UntypedKind::Report | UntypedKind::Many(_) | UntypedKind::Messages | UntypedKind::NoError(_)) => true,
                Some(UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit) | None => false,
            })
}

fn body_output<'tcx>(cx: &LateContext<'tcx>, body: LocalDefId) -> Option<Ty<'tcx>> {
    let typeck = cx.typeck_results();
    let node = cx.tcx.local_def_id_to_hir_id(body);
    typeck
        .liberated_fn_sigs()
        .get(node)
        .map(|signature| signature.output())
        .or_else(|| if let ty::Coroutine(_, args) = typeck.node_type_opt(node)?.kind() { Some(args.as_coroutine().return_ty()) } else { None })
}

fn text_source<'tcx, 'hir>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, call: &Expr<'hir>) -> Option<&'hir Expr<'hir>> {
    let (callee, source) = match call.kind {
        ExprKind::MethodCall(_, source, [], _) => (typeck.type_dependent_def_id(call.hir_id)?, source),
        ExprKind::Call(function, [source]) if let ty::FnDef(callee, _) = *typeck.expr_ty(function).kind() => (callee, source),
        _ => return None,
    };
    let canonical = cx.tcx.opt_associated_item(callee).and_then(|item| item.trait_item_or_self().ok());
    let owner = associated_trait(cx.tcx, callee);
    let text = is_text(cx, typeck.expr_ty_adjusted(source)) && is_text(cx, typeck.expr_ty(call));
    (matches!(callee.opt_diag_name(cx), Some(sym::string_as_str | sym::str_trim))
        || text && (matches!(canonical.opt_diag_name(cx), Some(sym::to_owned_method | sym::to_string_method)) || owner.is_diag_item(cx, sym::Clone))
        || text && matches!(owner.opt_diag_name(cx), Some(sym::From | sym::Into))
        || text && typeck.expr_ty(call).is_diag_item(cx, sym::Cow) && callee.ctor_parent(cx).is_some())
    .then_some(source)
}

fn unread<'tcx>(cx: &LateContext<'tcx>, handler: &Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    let ExprKind::Closure(closure) = handler.kind else { return None };
    let body = cx.tcx.hir_body(closure.body);
    match body.params {
        [] => true,
        [parameter] => match parameter.pat.kind {
            PatKind::Wild | PatKind::Struct(_, [], Some(_)) => true,
            PatKind::Binding(_, binding, _, None) => !is_local_used(cx, body.value, binding),
            _ => false,
        },
        _ => false,
    }
    .then(|| peel_blocks(body.value))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod canonicalize_without_path;
pub(crate) mod debug_main_error;
pub(crate) mod decode_without_path;
pub(crate) mod discarded_error;
pub(crate) mod discarded_error_source;
pub(crate) mod error_field_without_sources;
pub(crate) mod manual_error_impl;
pub(crate) mod partition_by_result;
pub(crate) mod redundant_error_conversion;
pub(crate) mod source_in_error_message;
pub(crate) mod stringified_error;
pub(crate) mod untyped_error;
