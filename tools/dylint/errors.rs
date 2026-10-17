use clippy_utils::{res::MaybeDef as _, sym, ty::implements_trait};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, Ty, TypeVisitableExt as _, fast_reject::DeepRejectCtxt};

use crate::{bounds_of, is_error, is_text, pointee};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Untyped {
    Text,
    Unit,
    Erased,
    Report,
    Many,
    NoError(DefId),
}

impl Untyped {
    const fn reason(self) -> &'static str {
        match self {
            Self::Text => "text that callers cannot match",
            Self::Unit => "a unit that names no failure",
            Self::Erased => "an erased `dyn Error` that callers cannot match",
            Self::Report => "a report erasing each failure's type, which callers cannot match",
            Self::Many => "a collection of errors that can be empty, a failure holding no error",
            Self::NoError(_) => "a `Display` type that implements no `core::error::Error`",
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn classify<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> Option<Untyped> {
    let (error_trait, display) = (cx.tcx.get_diagnostic_item(sym::Error)?, cx.tcx.get_diagnostic_item(sym::Display)?);
    match error.kind() {
        _ if error.is_unit() => Some(Untyped::Unit),
        _ if is_text(cx, error) => Some(Untyped::Text),
        _ if matches!(pointee(cx, error).kind(), ty::Dynamic(predicates, ..) if predicates.principal_def_id() == Some(error_trait)) => Some(Untyped::Erased),
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
            Some(Untyped::Report),
        ty::Adt(collection, args) if [sym::Vec, sym::BTreeSet].into_iter().any(|name| collection.is_diag_item(cx, name)) && is_error(cx, args.type_at(0)) => Some(Untyped::Many),
        _ => error
            .opt_def_id()
            .filter(|declared| declared.is_local() && !error.has_param() && implements_trait(cx, error, display, &[]) && !implements_trait(cx, error, error_trait, &[]))
            .map(Untyped::NoError),
    }
}

fn output<'tcx>(cx: &LateContext<'tcx>, body: LocalDefId) -> Option<Ty<'tcx>> {
    let typeck = cx.typeck_results();
    let node = cx.tcx.local_def_id_to_hir_id(body);
    typeck
        .liberated_fn_sigs()
        .get(node)
        .map(|signature| signature.output())
        .or_else(|| if let ty::Coroutine(_, args) = typeck.node_type_opt(node)?.kind() { Some(args.as_coroutine().return_ty()) } else { None })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub(crate) mod contextless_io_error;
pub(crate) mod foreign_error_return;
pub(crate) mod manual_error_impl;
pub(crate) mod redundant_error_conversion;
pub(crate) mod rendered_error;
pub(crate) mod split_outcome;
pub(crate) mod untyped_error;
