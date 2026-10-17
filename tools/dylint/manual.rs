use clippy_utils::{
    res::{MaybeDef as _, MaybeTypeckRes as _},
    sym,
};
use rustc_hir::Expr;
use rustc_lint::LateContext;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn iterator_method(cx: &LateContext<'_>, call: &Expr<'_>) -> bool { matches!(cx.ty_based_def(call).opt_parent(cx).opt_diag_name(cx), Some(sym::Iterator | sym::DoubleEndedIterator)) }

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
pub(crate) mod manual_slice_as_array;
pub(crate) mod manual_type_changing_struct_update;
pub(crate) mod manual_upcast;
pub(crate) mod manual_vec_from_fn;
