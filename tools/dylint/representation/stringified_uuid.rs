use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id,
    macros::{FormatArgsStorage, find_format_arg_expr, root_macro_call_first_node},
    paths::PathNS,
    res::MaybeDef as _,
    sym,
};
use rustc_ast::{FormatArgsPiece, FormatOptions, FormatTrait};
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{Expr, ExprKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};

use crate::{Access, Place, ReportedRenderings, access, associated_trait, constructed, expanded_externally, is_text, paths, place, record_rendering};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    identifiers: FxHashSet<DefId>,
    format_args: FormatArgsStorage,
    renderings: ReportedRenderings,
}

lint!(Pass; STRINGIFIED_UUID: "text fields of local structs set from a `Uuid` rendered to text");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>, format_args: FormatArgsStorage, renderings: ReportedRenderings) -> Self {
        Self { identifiers: paths(tcx, PathNS::Type, &["uuid::Uuid", "uuid::fmt::Hyphenated", "uuid::fmt::Simple", "uuid::fmt::Urn", "uuid::fmt::Braced"]), format_args, renderings }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        if !matches!(expr.kind, ExprKind::Struct(..) | ExprKind::Call(..) | ExprKind::Field(..)) {
            return;
        }
        let typeck = cx.typeck_results();
        let identifier = |held: Ty<'_>| held.peel_refs().ty_adt_def().is_some_and(|adt| self.identifiers.contains(&adt.did()));
        let rendered = |value: &'tcx Expr<'tcx>| {
            let value = expr_or_init(cx, value);
            match value.kind {
                _ if let Some(call) = root_macro_call_first_node(cx, value).filter(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)) => {
                    let args = self.format_args.get(cx, value, call.expn)?;
                    (matches!((args.arguments.all_args(), &args.template), ([argument], [FormatArgsPiece::Placeholder(placeholder)])
                        if placeholder.format_trait == FormatTrait::Display && placeholder.format_options == FormatOptions::default()
                            && find_format_arg_expr(value, argument).is_some_and(|value| identifier(typeck.expr_ty(value)))))
                    .then_some((call.span, value.hir_id))
                }
                ExprKind::MethodCall(_, receiver, [], _) | ExprKind::Call(_, [receiver])
                    if fn_def_id(cx, value).and_then(|method| associated_trait(cx.tcx, method)).is_diag_item(cx, sym::ToString) && identifier(typeck.expr_ty(receiver)) =>
                    Some((value.span, value.hir_id)),
                _ => None,
            }
        };
        if expanded_externally(cx, expr.span) {
            return;
        }
        let built = constructed(typeck, expr)
            .into_iter()
            .flat_map(|(_, fields)| fields)
            .filter(|(field, _)| field.did.is_local() && is_text(cx, cx.tcx.type_of(field.did).instantiate_identity().skip_normalization()))
            .map(|(field, value)| (field.name, value));
        let assigned = match (place(cx, expr), access(cx, expr)) {
            (Some(Place::Field(field)), Access::Assigned(value)) => Some((cx.tcx.item_name(field), value)),
            (Some(Place::Field(_)), Access::Copied | Access::Borrowed | Access::Moved | Access::Mutated)
            | (Some(Place::Static(_) | Place::Local(_) | Place::Parameter(_)) | None, Access::Copied | Access::Borrowed | Access::Moved | Access::Mutated | Access::Assigned(_)) => None,
        };
        for (field, (span, origin)) in built.chain(assigned).filter_map(|(field, value)| Some((field, rendered(value)?))) {
            span_lint_hir_and_then(cx, STRINGIFIED_UUID, expr.hir_id, span, format!("field `{field}` holds a `Uuid` rendered to text"), |diag| {
                record_rendering(cx, &self.renderings, origin);
                diag.help(format!("declare `{field}` as `newtype_uuid::TypedUuid<K>` for an entity identifier, or as `uuid::Uuid`"));
            });
        }
    }
}
