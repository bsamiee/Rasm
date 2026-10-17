use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeDef as _, sym};
use itertools::Itertools as _;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprKind, MatchSource, Pat, PatKind};
use rustc_lint::{LateContext, LateLintPass, declare_lint, declare_lint_pass};
use rustc_middle::ty::{self, AdtDef, FieldDef, VariantDef};
use rustc_span::{FileName, RemapPathScopeComponents, Span};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    OWNED_CATCH_ALL,
    Warn,
    "`match` arms catching repository enum variants through `_`, a binding, or an or-pattern holding `_`, and `..` over accessible repository struct or variant fields, where additions go unhandled"
}

// --- [MODELS] --------------------------------------------------------------------------

enum Covered {
    Named(Span, String),
    Positional,
}

// --- [SERVICES] ------------------------------------------------------------------------

declare_lint_pass!(#[derive(Default)] OwnedCatchAll => [OWNED_CATCH_ALL]);

impl<'tcx> LateLintPass<'tcx> for OwnedCatchAll {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn catches(pat: &Pat<'_>) -> bool {
            match pat.kind {
                PatKind::Wild => true,
                PatKind::Binding(.., subpattern) => subpattern.is_none_or(catches),
                PatKind::Or(alternatives) => alternatives.iter().any(catches),
                PatKind::Ref(inner, ..) | PatKind::Deref(inner) => catches(inner),
                _ => false,
            }
        }

        if let ExprKind::Match(scrutinee, arms @ [_, _, ..], MatchSource::Normal) = expr.kind
            && !expr.span.from_expansion()
            && let ty::Adt(adt, _) = cx.typeck_results().expr_ty(scrutinee).peel_refs().kind()
            && adt.is_enum()
            && ![sym::Option, sym::Result].into_iter().any(|name| adt.is_diag_item(cx, name))
            && owned(cx, *adt)
        {
            for arm in arms.iter().filter(|arm| arm.guard.is_none() && catches(arm.pat)) {
                span_lint_hir_and_then(
                    cx,
                    OWNED_CATCH_ALL,
                    arm.hir_id,
                    arm.pat.span,
                    format!("catch-all arm covers every `{}` variant the arms above leave unnamed", cx.tcx.item_name(adt.did())),
                    |diag| {
                        diag.help("name each variant this arm covers in an or-pattern, `name @ (A | B)` keeps the binding");
                    },
                );
            }
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        fn covered(cx: &LateContext<'_>, pat: &Pat<'_>, variant: &VariantDef) -> Option<Covered> {
            let module = cx.tcx.parent_module(pat.hir_id);
            let accessible = |field: &FieldDef| field.vis.is_accessible_from(module, cx.tcx);
            match pat.kind {
                PatKind::Struct(_, fields, Some(rest)) => {
                    let unnamed =
                        variant.fields.iter().filter(|field| accessible(field) && fields.iter().all(|named| named.ident.name != field.name)).map(|field| format!("{}: _", field.name)).join(", ");
                    let whole = variant.fields.iter().all(accessible) && !variant.field_list_has_applicable_non_exhaustive();
                    (!unnamed.is_empty()).then(|| if whole { Covered::Named(rest, unnamed) } else { Covered::Named(rest.shrink_to_lo(), format!("{unnamed}, ")) })
                }
                PatKind::TupleStruct(_, fields, rest) => (rest.as_opt_usize().is_some() && fields.len() < variant.fields.len() && variant.fields.iter().all(accessible)).then_some(Covered::Positional),
                _ => None,
            }
        }

        if let PatKind::Struct(qpath, ..) | PatKind::TupleStruct(qpath, ..) = pat.kind
            && !pat.span.from_expansion()
            && let ty::Adt(adt, _) = cx.typeck_results().pat_ty(pat).kind()
            && owned(cx, *adt)
            && let variant = adt.variant_of_res(cx.qpath_res(&qpath, pat.hir_id))
            && let Some(covered) = covered(cx, pat, variant)
        {
            span_lint_hir_and_then(cx, OWNED_CATCH_ALL, pat.hir_id, pat.span, format!("`..` covers `{}` fields the pattern leaves unnamed", variant.name), |diag| match covered {
                Covered::Named(span, replacement) => {
                    diag.span_suggestion_verbose(span, "name each field `..` covers as `field: _`", replacement, Applicability::MachineApplicable);
                }
                Covered::Positional => {
                    diag.help("write `_` in place of `..` for each field it covers");
                }
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn owned(cx: &LateContext<'_>, adt: AdtDef<'_>) -> bool {
    let source_map = cx.tcx.sess.source_map();
    adt.did().is_local()
        || matches!(source_map.span_to_filename(cx.tcx.def_span(adt.did())), FileName::Real(file)
            if source_map.working_dir().local_path().is_some_and(|root| file.embeddable_name(RemapPathScopeComponents::DIAGNOSTICS).1.starts_with(root)))
}
