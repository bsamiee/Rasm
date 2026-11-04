use clippy_utils::{fn_def_id, res::MaybeDef as _, sym};
use rustc_hir::{Expr, ExprKind, UnOp};
use rustc_lint::LateContext;
use rustc_middle::ty::{AdtDef, Ty};

use crate::{associated_trait, is_text};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn fieldless_local_enum(held: Ty<'_>) -> Option<AdtDef<'_>> {
    held.peel_refs().ty_adt_def().filter(|adt| adt.is_enum() && adt.did().is_local() && adt.variants().len() > 1 && adt.all_fields().next().is_none())
}

fn text_view<'tcx>(cx: &LateContext<'tcx>, inner: &'tcx Expr<'tcx>) -> Option<&'tcx Expr<'tcx>> {
    let typeck = cx.typeck_results();
    match inner.kind {
        ExprKind::AddrOf(_, _, operand) => Some(operand),
        ExprKind::Unary(UnOp::Deref, operand) if is_text(cx, typeck.expr_ty(operand)) => Some(operand),
        ExprKind::MethodCall(_, receiver, [], _)
            if is_text(cx, typeck.expr_ty(receiver))
                && is_text(cx, typeck.expr_ty(inner))
                && let Some(method) = fn_def_id(cx, inner)
                && (cx.tcx.is_diagnostic_item(sym::string_as_str, method)
                    || matches!(associated_trait(cx.tcx, method).opt_diag_name(cx), Some(sym::AsRef | sym::Borrow | sym::Deref | sym::ToOwned | sym::ToString | sym::From | sym::Into))) =>
            Some(receiver),
        ExprKind::Call(_, [argument])
            if is_text(cx, typeck.expr_ty(argument)) && matches!(fn_def_id(cx, inner).and_then(|method| associated_trait(cx.tcx, method)).opt_diag_name(cx), Some(sym::From | sym::Into)) =>
            Some(argument),
        _ => None,
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod formatted_tracing_value;
pub(crate) mod manual_reference_impl;
pub(crate) mod manual_serialization;
pub(crate) mod manual_variant_array;
pub(crate) mod needless_reference_field;
pub(crate) mod needless_struct;
pub(crate) mod option_field_pairs;
pub(crate) mod restated_constant;
pub(crate) mod sentinel_default;
pub(crate) mod single_impl_trait;
pub(crate) mod stringified_uuid;
pub(crate) mod stringly_typed_value;
pub(crate) mod untyped_json_access;
pub(crate) mod variant_constant_functions;
