use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeResPath as _, source::snippet_with_context, sugg::Sugg};
use itertools::Itertools as _;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprField, ExprKind, StructTailExpr};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty;

use crate::{expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_TYPE_CHANGING_STRUCT_UPDATE: "struct literals copying each unchanged field from the same struct with other type arguments");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if let ExprKind::Struct(path, fields, StructTailExpr::None) = expr.kind
            && let typeck = cx.typeck_results()
            && let ty::Adt(owner, arguments) = typeck.expr_ty(expr).kind()
            && owner.is_struct()
            && !arguments.is_empty()
            && let copied = (|field: &ExprField<'tcx>| {
                if let ExprKind::Field(copied, _) = field.expr.kind
                    && let ty::Adt(source, source_arguments) = typeck.expr_ty(copied).kind()
                    && owner == source
                    && arguments != source_arguments
                    && copied.res_local_id().is_some()
                    && typeck.expr_adjustments(copied).is_empty()
                    && typeck.expr_adjustments(field.expr).is_empty()
                    && typeck.field_index(field.hir_id) == typeck.field_index(field.expr.hir_id)
                {
                    Some(copied)
                } else {
                    None
                }
            })
            && let mut remaining = fields.iter().enumerate()
            && let Some((written, base)) = remaining.by_ref().find_map(|(index, field)| copied(field).map(|base| (index, base)))
            && remaining.all(|(_, field)| copied(field).is_some_and(|other| other.res_local_id() == base.res_local_id()))
            && !expanded_externally(cx, expr.span)
        {
            let ctxt = expr.span.ctxt();
            let mut applicability = Applicability::MaybeIncorrect;
            let (named, _) = snippet_with_context(cx, path.span(), ctxt, "..", &mut applicability);
            let base = Sugg::hir_with_context(cx, base, ctxt, "..", &mut applicability);
            let written = fields.iter().take(written).map(|field| snippet_with_context(cx, field.span, ctxt, "..", &mut applicability).0).chain([format!("..{base}").into()]).join(", ");
            span_lint_hir_and_then(
                cx,
                MANUAL_TYPE_CHANGING_STRUCT_UPDATE,
                expr.hir_id,
                expr.span,
                format!("`{named}` literal copies each unchanged field of `{base}` to change its type arguments"),
                |diag| {
                    suggest(
                        cx,
                        expr.hir_id,
                        diag,
                        expr.span,
                        format!("write the changed fields and `..{base}` under `#![feature(type_changing_struct_update)]`"),
                        format!("{named} {{ {written} }}"),
                        applicability,
                    );
                },
            );
        }
    }
}
