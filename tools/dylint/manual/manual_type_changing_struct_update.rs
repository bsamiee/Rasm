use core::iter;

use clippy_utils::{diagnostics::span_lint_hir_and_then, res::MaybeResPath as _, source::snippet_with_context, sugg::Sugg};
use itertools::Itertools as _;
use rustc_errors::Applicability;
use rustc_hir::{Expr, ExprField, ExprKind, StructTailExpr};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, Ty};

use crate::{accessed_field, expanded_externally, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_TYPE_CHANGING_STRUCT_UPDATE: "struct literals copying each unchanged field from the same struct with other type arguments");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let copied = |built: Ty<'tcx>, field: &ExprField<'tcx>| {
            let typeck = cx.typeck_results();
            if let ExprKind::Field(base, _) = field.expr.kind
                && let (ty::Adt(owner, arguments), ty::Adt(source, source_arguments)) = (built.kind(), typeck.expr_ty(base).kind())
                && owner == source
                && arguments != source_arguments
                && base.res_local_id().is_some()
                && let Some(accessed) = accessed_field(typeck, field.expr)
                && typeck.opt_field_index(field.hir_id).and_then(|index| owner.non_enum_variant().fields.get(index)).is_some_and(|declared| declared.did == accessed)
            {
                Some(base)
            } else {
                None
            }
        };
        if let ExprKind::Struct(path, fields, StructTailExpr::None) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let built = cx.typeck_results().expr_ty(expr)
            && built.ty_adt_def().is_some_and(AdtDef::is_struct)
            && let sources = fields.iter().map(|field| copied(built, field)).collect::<Vec<_>>()
            && let Some(base) = sources.iter().flatten().next()
            && sources.iter().flatten().all(|other| other.res_local_id() == base.res_local_id())
        {
            let ctxt = expr.span.ctxt();
            let mut applicability = Applicability::MaybeIncorrect;
            let kept = iter::zip(fields, &sources).filter(|(_, source)| source.is_none()).map(|(field, _)| snippet_with_context(cx, field.span, ctxt, "..", &mut applicability).0).join(", ");
            let (named, _) = snippet_with_context(cx, path.span(), ctxt, "..", &mut applicability);
            let base = Sugg::hir_with_context(cx, base, ctxt, "..", &mut applicability);
            span_lint_hir_and_then(cx, MANUAL_TYPE_CHANGING_STRUCT_UPDATE, expr.hir_id, expr.span, format!("`{named}` copies each field of `{base}` to change its type arguments"), |diag| {
                suggest(
                    cx,
                    expr.hir_id,
                    diag,
                    expr.span,
                    "write the changed fields and `..base` under `#![feature(type_changing_struct_update)]`",
                    format!("{named} {{ {kept}, ..{base} }}"),
                    applicability,
                );
            });
        }
    }
}
