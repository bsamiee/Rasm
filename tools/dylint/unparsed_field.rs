use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, fn_def_id, get_parent_expr, paths::PathNS, sym, ty::same_type_modulo_regions};
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    Expr, ExprKind, HirId, Pat, PatKind, StructTailExpr,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{Ty, TyCtxt, print::with_forced_trimmed_paths};

use crate::{Access, access, accessed_field, generated, impl_owner, is_text, path, pattern_fields, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    UNPARSED_FIELD,
    Warn,
    "crate-private text fields of structs that every read parses into one type"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct UnparsedField {
    from_str: Option<DefId>,
    parses: FxIndexMap<LocalDefId, Option<Vec<HirId>>>,
}

impl_lint_pass!(UnparsedField => [UNPARSED_FIELD]);

impl UnparsedField {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { from_str: path(tcx, PathNS::Type, "core::str::FromStr"), parses: FxIndexMap::default() } }

    fn read(&mut self, cx: &LateContext<'_>, field: LocalDefId, parse: Option<HirId>) {
        if !cx.effective_visibilities.is_exported(field) && is_text(cx, cx.tcx.type_of(field).instantiate_identity().skip_normalization()) {
            let sites = self.parses.entry(field).or_insert_with(|| Some(Vec::new()));
            *sites = sites.take().zip(parse).map(|(mut sites, parse)| {
                sites.push(parse);
                sites
            });
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for UnparsedField {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn parse<'tcx>(cx: &LateContext<'tcx>, read: &'tcx Expr<'tcx>, from_str: DefId) -> Option<HirId> {
            let parent = get_parent_expr(cx, read)?;
            let callee = fn_def_id(cx, parent);
            match parent.kind {
                ExprKind::AddrOf(_, _, referent) if referent.hir_id == read.hir_id => parse(cx, parent, from_str),
                ExprKind::MethodCall(segment, receiver, [], _)
                    if receiver.hir_id == read.hir_id && segment.ident.name == sym::parse && callee.and_then(|method| impl_owner(cx.tcx, method)).is_some_and(Ty::is_str) =>
                    Some(parent.hir_id),
                ExprKind::Call(_, [argument]) if argument.hir_id == read.hir_id && callee.and_then(|function| cx.tcx.trait_of_assoc(function)) == Some(from_str) => Some(parent.hir_id),
                _ => None,
            }
        }

        let typeck = cx.typeck_results();
        match expr.kind {
            ExprKind::Field(..)
                if let Some(field) = accessed_field(typeck, expr).and_then(DefId::as_local)
                    && !matches!(access(cx, expr), Access::Assigned(_))
                    && !generated(cx, expr.hir_id) =>
            {
                let parse = self.from_str.filter(|_| !expr.span.from_expansion()).and_then(|from_str| parse(cx, expr, from_str));
                self.read(cx, field, parse);
            }
            ExprKind::Struct(_, fields, StructTailExpr::Base(_)) if let Some(adt) = typeck.expr_ty(expr).ty_adt_def().filter(|adt| adt.is_struct()) => {
                for field in adt.non_enum_variant().fields.iter().filter(|declared| fields.iter().all(|named| named.ident.name != declared.name)).filter_map(|declared| declared.did.as_local()) {
                    self.read(cx, field, None);
                }
            }
            _ => {}
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let derived = generated(cx, pat.hir_id);
        for field in pattern_fields(cx.typeck_results(), pat).filter(|(_, bound)| !derived && !matches!(bound.kind, PatKind::Wild)).filter_map(|(field, _)| field.as_local()) {
            self.read(cx, field, None);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let parsed = |site: HirId| result_parts(cx, cx.tcx.typeck(cx.tcx.hir_enclosing_body_owner(site)).node_type(site)).map(|(target, _)| target);
        let unparsed = mem::take(&mut self.parses).into_iter().filter_map(|(field, sites)| {
            let sites = sites?;
            let (&first, target) = sites.first().and_then(|first| Some((first, parsed(*first)?)))?;
            sites.iter().all(|&site| parsed(site).is_some_and(|other| same_type_modulo_regions(other, target))).then_some((field, first, target))
        });
        for (field, first, target) in unparsed {
            let shown = with_forced_trimmed_paths!(target.to_string());
            span_lint_hir_and_then(
                cx,
                UNPARSED_FIELD,
                cx.tcx.local_def_id_to_hir_id(field),
                cx.tcx.def_span(field),
                format!("field `{}` holds text every read parses into `{shown}`", cx.tcx.item_name(field)),
                |diag| {
                    diag.span_note(cx.tcx.hir_span(first), "the text is parsed here");
                    diag.help(format!("declare the field `{shown}` and parse the text where it enters, through clap's `value_parser` or serde's `DisplayFromStr`"));
                },
            );
        }
    }
}
