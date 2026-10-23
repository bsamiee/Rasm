use core::iter;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use itertools::Itertools as _;
use rustc_errors::Applicability;
use rustc_hir::{Pat, PatKind};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, FieldDef, VariantDef};
use rustc_span::Span;

use crate::{expanded_externally, suggest, workspace_def};

// --- [MODELS] --------------------------------------------------------------------------

enum UnnamedFields {
    Named(Span, String),
    Positional(String),
}

// --- [SERVICES] ------------------------------------------------------------------------

lint!(REST_PATTERN_WORKSPACE_FIELD: "`..` in struct and tuple-struct patterns covering accessible fields of workspace types");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        fn unnamed_fields(cx: &LateContext<'_>, pat: &Pat<'_>, variant: &VariantDef) -> Option<UnnamedFields> {
            let module = cx.tcx.parent_module(pat.hir_id);
            let accessible = |field: &FieldDef| field.vis.is_accessible_from(module, cx.tcx);
            match pat.kind {
                PatKind::Struct(_, fields, Some(rest)) => {
                    let unnamed =
                        variant.fields.iter().filter(|field| accessible(field) && fields.iter().all(|named| named.ident.name != field.name)).map(|field| format!("{}: _", field.name)).join(", ");
                    let whole = variant.fields.iter().all(accessible) && !variant.field_list_has_applicable_non_exhaustive();
                    (!unnamed.is_empty()).then(|| if whole { UnnamedFields::Named(rest, unnamed) } else { UnnamedFields::Named(rest.shrink_to_lo(), format!("{unnamed}, ")) })
                }
                PatKind::TupleStruct(_, fields, rest) => (rest.as_opt_usize().is_some() && fields.len() < variant.fields.len() && variant.fields.iter().all(accessible))
                    .then(|| UnnamedFields::Positional(iter::repeat_n("_", variant.fields.len().strict_sub(fields.len())).join(", "))),
                _ => None,
            }
        }

        if let PatKind::Struct(qpath, ..) | PatKind::TupleStruct(qpath, ..) = pat.kind
            && !expanded_externally(cx, pat.span)
            && let ty::Adt(adt, _) = cx.typeck_results().pat_ty(pat).kind()
            && workspace_def(cx, adt.did())
            && let variant = adt.variant_of_res(cx.qpath_res(&qpath, pat.hir_id))
            && let Some(unnamed) = unnamed_fields(cx, pat, variant)
        {
            span_lint_hir_and_then(cx, REST_PATTERN_WORKSPACE_FIELD, pat.hir_id, pat.span, format!("`..` covers `{}` fields the pattern leaves unnamed", variant.name), |diag| match unnamed {
                UnnamedFields::Named(span, replacement) => {
                    suggest(cx, pat.hir_id, diag, span, "name each field `..` covers as `field: _`", replacement, Applicability::MachineApplicable);
                }
                UnnamedFields::Positional(replacement) => {
                    diag.help(format!("replace `..` with `{replacement}`"));
                }
            });
        }
    }
}
