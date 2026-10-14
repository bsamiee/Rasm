#![feature(rustc_private)]

extern crate rustc_attr_ir;
extern crate rustc_data_structures;
extern crate rustc_hir;
extern crate rustc_middle;
extern crate rustc_span;

use std::mem;

use clippy_utils::{
    diagnostics::{span_lint_and_help, span_lint_and_then},
    is_entrypoint_fn, is_in_test,
    paths::{PathNS, lookup_path_str},
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, ty_from_hir_ty},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    self as hir, AmbigArg, FnRetTy, ImplItem, ImplItemKind, Item, TraitItem, TyKind,
    def::DefKind,
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Ty, TypeVisitableExt as _, Unnormalized};
use rustc_span::Span;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Untyped {
    Text,
    Unit,
    Boxed,
    Opaque,
    NoError(DefId),
}

impl Untyped {
    const fn message(self) -> &'static str {
        match self {
            Self::Text => "error type is text that callers cannot match",
            Self::Unit => "error type `()` names no failure",
            Self::Boxed => "error type is a boxed `dyn Error` that callers cannot match",
            Self::Opaque => "error type is an opaque report that callers cannot match",
            Self::NoError(_) => "error type renders through `Display` and implements no `std::error::Error`",
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

dylint_linting::impl_late_lint! {
    pub UNTYPED_ERROR,
    Warn,
    "`Result` error types that resolve to text, unit, a boxed or opaque error, or a local type rendered through `Display` without `Error`",
    UntypedError::default()
}

#[derive(Default)]
struct UntypedError {
    opaque: Vec<DefId>,
    local: FxIndexMap<DefId, Vec<Span>>,
}

struct Signature<'pass, 'tcx> {
    cx: &'pass LateContext<'tcx>,
    pass: &'pass mut UntypedError,
    inputs: Option<&'tcx [Ty<'tcx>]>,
    result_unit_err: Option<Span>,
}

impl<'pass, 'tcx> Signature<'pass, 'tcx> {
    fn new(cx: &'pass LateContext<'tcx>, pass: &'pass mut UntypedError, owner: LocalDefId) -> Self {
        let declaration = cx.tcx.hir_node_by_def_id(owner).fn_decl();
        Self {
            cx,
            pass,
            inputs: declaration.map(|_| cx.tcx.fn_sig(owner).instantiate_identity().skip_binder().inputs()),
            result_unit_err: declaration.and_then(|declaration| match declaration.output {
                FnRetTy::Return(output) => (cx.effective_visibilities.is_exported(owner) && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none()).then_some(output.span.source_callsite()),
                FnRetTy::DefaultReturn(_) => None,
            }),
        }
    }

    fn inspect(&mut self, span: Span, ty: Ty<'tcx>) {
        if let Some((ok, error)) = result_parts(self.cx, ty)
            && !self.inputs.is_some_and(|inputs| inputs.iter().any(|input| input.peel_refs() == error))
            && let Some(untyped) = classify(self.cx, &self.pass.opaque, ok, error)
            && !(matches!(untyped, Untyped::Unit) && self.result_unit_err == Some(span))
        {
            match untyped {
                Untyped::NoError(local) => self.pass.local.entry(local).or_default().push(span),
                Untyped::Text | Untyped::Unit | Untyped::Boxed | Untyped::Opaque => {
                    span_lint_and_help(self.cx, UNTYPED_ERROR, span, format!("{}: `{error}`", untyped.message()), None, "return a `thiserror` enum with a variant per failure and its typed source");
                }
            }
        }
    }
}

impl<'tcx> Visitor<'tcx> for Signature<'_, 'tcx> {
    fn visit_ty(&mut self, t: &'tcx hir::Ty<'tcx, AmbigArg>) {
        let hir_ty = t.as_unambig_ty();
        if matches!(hir_ty.kind, TyKind::Path(_))
            && !hir_ty.span.from_expansion()
            && hir_ty
                .basic_res()
                .opt_def_id()
                .filter(|alias| alias.is_local() && self.cx.tcx.def_kind(*alias) == DefKind::TyAlias)
                .and_then(|alias| result_parts(self.cx, self.cx.tcx.type_of(alias).instantiate_identity().skip_normalization()))
                .is_none_or(|(ok, error)| classify(self.cx, &self.pass.opaque, ok, error).is_none())
        {
            self.inspect(hir_ty.span, ty_from_hir_ty(self.cx, hir_ty));
        }
        intravisit::walk_ty(self, t);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn result_parts<'tcx>(cx: &LateContext<'tcx>, ty: Ty<'tcx>) -> Option<(Ty<'tcx>, Ty<'tcx>)> {
    if let ty::Adt(result, args) = ty.kind()
        && result.is_diag_item(cx, sym::Result)
    {
        Some((args.types().next()?, args.types().nth(1)?))
    } else {
        None
    }
}

fn returned<'tcx>(cx: &LateContext<'tcx>, function: DefId) -> Ty<'tcx> {
    let output = cx.tcx.fn_sig(function).instantiate_identity().skip_binder().output();
    cx.tcx.get_impl_future_output_ty(output).unwrap_or(output)
}

fn classify<'tcx>(cx: &LateContext<'tcx>, opaque: &[DefId], ok: Ty<'tcx>, error: Ty<'tcx>) -> Option<Untyped> {
    let error_trait = cx.tcx.get_diagnostic_item(sym::Error)?;
    let display = cx.tcx.get_diagnostic_item(sym::Display)?;
    if error.is_unit() {
        Some(Untyped::Unit)
    } else if error.is_lang_item(cx, LangItem::String)
        || error.peel_refs().is_str()
        || matches!(error.kind(), ty::Adt(owner, args) if (owner.is_box() || [sym::Arc, sym::Rc, sym::Cow].into_iter().any(|name| owner.is_diag_item(cx, name))) && args.types().any(Ty::is_str))
    {
        Some(Untyped::Text)
    } else if error.opt_def_id().is_some_and(|adt| opaque.contains(&adt)) {
        Some(Untyped::Opaque)
    } else if error.boxed_ty().is_some_and(|inner| matches!(inner.kind(), ty::Dynamic(predicates, ..) if predicates.principal_def_id() == Some(error_trait))) {
        Some(Untyped::Boxed)
    } else {
        error
            .opt_def_id()
            .filter(|local| local.is_local() && error != ok && !error.has_param() && implements_trait(cx, error, display, &[]) && !implements_trait(cx, error, error_trait, &[]))
            .map(Untyped::NoError)
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for UntypedError {
    fn check_crate(&mut self, cx: &LateContext<'tcx>) { self.opaque = ["anyhow::Error", "eyre::Report"].into_iter().flat_map(|path| lookup_path_str(cx.tcx, PathNS::Type, path)).collect(); }

    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let owner = item.owner_id.def_id;
        if matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::Struct | DefKind::Enum | DefKind::Union | DefKind::TyAlias)
            && !item.span.from_expansion()
            && !is_entrypoint_fn(cx, owner.to_def_id())
            && !is_in_test(cx.tcx, item.hir_id())
        {
            intravisit::walk_item(&mut Signature::new(cx, self, owner), item);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if !is_in_test(cx.tcx, item.hir_id()) {
            intravisit::walk_trait_item(&mut Signature::new(cx, self, item.owner_id.def_id), item);
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        let owner = item.owner_id.def_id;
        match (is_in_test(cx.tcx, item.hir_id()), cx.tcx.associated_item(owner).trait_item_def_id()) {
            (true, _) => {}
            (false, None) => intravisit::walk_impl_item(&mut Signature::new(cx, self, owner), item),
            (false, Some(declared)) => {
                if let ImplItemKind::Fn(signature, _) = item.kind
                    && let FnRetTy::Return(output) = signature.decl.output
                    && result_parts(cx, returned(cx, declared)).is_some_and(|(_, error)| error.has_aliases())
                {
                    let chosen = cx.tcx.normalize_erasing_regions(cx.typing_env(), Unnormalized::new_wip(returned(cx, owner.to_def_id())));
                    Signature::new(cx, self, owner).inspect(output.span.source_callsite(), chosen);
                }
            }
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (error, uses) in mem::take(&mut self.local) {
            span_lint_and_then(cx, UNTYPED_ERROR, cx.tcx.def_span(error), format!("{}: `{}`", Untyped::NoError(error).message(), cx.tcx.item_name(error)), |diag| {
                diag.span_note(uses, "returned as a `Result` error here");
                diag.help("derive `thiserror::Error` in place of the `Display` impl");
            });
        }
    }
}

// --- [TESTS] ---------------------------------------------------------------------------

#[cfg(test)]
mod tests {
    #[test]
    fn ui() { dylint_testing::ui_test_examples(env!("CARGO_PKG_NAME")); }
}
