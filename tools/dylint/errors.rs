use clippy_utils::{
    paths::{PathNS, lookup_path_str},
    res::MaybeDef as _,
    sym,
    ty::implements_trait,
};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_lint::LateContext;
use rustc_middle::ty::{self, Ty, TyCtxt, TypeVisitableExt as _};

use crate::{is_error, is_text, pointee};

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

fn io_error(tcx: TyCtxt<'_>) -> Option<DefId> { lookup_path_str(tcx, PathNS::Type, "std::io::Error").into_iter().next() }

fn classify<'tcx>(cx: &LateContext<'tcx>, error: Ty<'tcx>) -> Option<Untyped> {
    let (error_trait, display) = (cx.tcx.get_diagnostic_item(sym::Error)?, cx.tcx.get_diagnostic_item(sym::Display)?);
    if error.is_unit() {
        Some(Untyped::Unit)
    } else if is_text(cx, error) {
        Some(Untyped::Text)
    } else if matches!(pointee(cx, error).kind(), ty::Dynamic(predicates, ..) if predicates.principal_def_id() == Some(error_trait)) {
        Some(Untyped::Erased)
    } else if let ty::Adt(report, _) = error.kind()
        && !report.did().is_local()
        && !implements_trait(cx, error, error_trait, &[])
        && cx.tcx.get_diagnostic_item(sym::From).is_some_and(|from| {
            cx.tcx.non_blanket_impls_for_ty(from, error).any(|implementation| {
                let converted = cx.tcx.impl_trait_ref(implementation).instantiate_identity().skip_normalization();
                let source = converted.args.type_at(1);
                converted.self_ty() == error
                    && matches!(source.kind(), ty::Param(_))
                    && cx
                        .tcx
                        .clauses_of(implementation)
                        .clauses
                        .iter()
                        .filter_map(|(clause, _)| clause.as_trait_clause())
                        .any(|bound| bound.def_id() == error_trait && bound.skip_binder().self_ty() == source)
            })
        })
    {
        Some(Untyped::Report)
    } else if let ty::Adt(collection, args) = error.kind()
        && [sym::Vec, sym::BTreeSet].into_iter().any(|name| collection.is_diag_item(cx, name))
        && is_error(cx, args.type_at(0))
    {
        Some(Untyped::Many)
    } else {
        error
            .opt_def_id()
            .filter(|declared| declared.is_local() && !error.has_param() && implements_trait(cx, error, display, &[]) && !implements_trait(cx, error, error_trait, &[]))
            .map(Untyped::NoError)
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

mod contextless_io_error;
mod foreign_error_return;
mod manual_error_impl;
mod redundant_error_conversion;
mod rendered_error;
mod split_outcome;
mod untyped_error;

lints! {
    contextless_io_error::ContextlessIoError::new,
    foreign_error_return::ForeignErrorReturn::new,
    manual_error_impl::ManualErrorImpl,
    redundant_error_conversion::RedundantErrorConversion::new,
    rendered_error::RenderedError,
    split_outcome::SplitOutcome,
    untyped_error::UntypedError,
}
