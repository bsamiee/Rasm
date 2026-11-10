use core::iter;
use core::ops::ControlFlow;

use clippy_utils::eager_or_lazy::switch_to_eager_eval;
use clippy_utils::res::MaybeDef as _;
use clippy_utils::visitors::for_each_expr_without_closures;
use clippy_utils::{fn_def_id, sym};
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{Expr, ExprKind};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, AdtDef, GenericArgsRef, Ty};

use crate::associated_trait;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RESOLUTION]

fn iterator_method(cx: &LateContext<'_>, call: &Expr<'_>) -> bool {
    matches!(fn_def_id(cx, call).and_then(|method| associated_trait(cx.tcx, method)).opt_diag_name(cx), Some(sym::Iterator | sym::DoubleEndedIterator))
}

fn uses_global_allocator<'tcx>(cx: &LateContext<'tcx>, owner: AdtDef<'tcx>, arguments: GenericArgsRef<'tcx>) -> bool {
    iter::zip(&cx.tcx.generics_of(owner.did()).own_params, arguments).any(|(parameter, argument)| {
        if let ty::GenericParamDefKind::Type { has_default: true, .. } = parameter.kind {
            let declared = cx.tcx.type_of(parameter.def_id).instantiate_identity().skip_normalization();
            declared.is_lang_item(cx, LangItem::GlobalAlloc) && argument.expect_ty() == declared
        } else {
            false
        }
    })
}

fn collects_vec<'tcx>(cx: &LateContext<'tcx>, item: Ty<'tcx>, collected: Ty<'tcx>) -> bool {
    matches!(collected.kind(), ty::Adt(owner, arguments) if owner.is_diag_item(cx, sym::Vec)
        && uses_global_allocator(cx, *owner, arguments)
        && cx.tcx.erase_and_anonymize_regions(item) == cx.tcx.erase_and_anonymize_regions(arguments.type_at(0)))
}

// --- [EVALUATION]

fn repeatable<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> bool {
    for_each_expr_without_closures(value, |expression| match expression.kind {
        ExprKind::MethodCall(_, receiver, ..) | ExprKind::Call(_, [receiver, ..])
            if let held = cx.typeck_results().expr_ty(receiver).peel_refs()
                && (fn_def_id(cx, expression).and_then(|called| associated_trait(cx.tcx, called)).is_some()
                    || !(matches!(held.kind(), ty::Str | ty::Slice(_) | ty::Array(..))
                        || held.is_diag_item(cx, sym::Vec)
                        || held.is_diag_item(cx, sym::VecDeque)
                        || held.is_lang_item(cx, LangItem::String))) =>
            ControlFlow::Break(()),
        ExprKind::Call(_, []) => ControlFlow::Break(()),
        _ => ControlFlow::Continue(()),
    })
    .is_none()
        && switch_to_eager_eval(cx, value)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod manual_anonymous_impl;
pub(crate) mod manual_bool_toggle;
pub(crate) mod manual_default_field_values;
pub(crate) mod manual_deref_pattern;
pub(crate) mod manual_drop_guard;
pub(crate) mod manual_from_utf8_lossy_owned;
pub(crate) mod manual_partition;
pub(crate) mod manual_path_is_empty;
pub(crate) mod manual_retain_back;
pub(crate) mod manual_set_operator;
pub(crate) mod manual_slice_as_array;
pub(crate) mod manual_type_changing_struct_update;
pub(crate) mod manual_upcast;
pub(crate) mod manual_vec_from_fn;
