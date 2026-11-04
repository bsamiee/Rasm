use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, get_parent_expr,
    macros::{FormatArgsStorage, find_format_arg_expr, first_node_in_macro, matching_root_macro_call, root_macro_call_first_node},
    res::{MaybeDef as _, MaybeResPath as _},
    sugg::Sugg,
    sym,
};
use rustc_ast::{FormatArgsPiece, FormatOptions, FormatTrait};
use rustc_attr_ir::lang_items::LangItem;
use rustc_errors::Applicability;
use rustc_hir::{Body, Expr, ExprKind, HirId, Node, PatKind, UnOp, def_id::LocalDefIdMap};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{AdtDef, TyCtxt};
use rustc_span::Span;

use super::has_temporary_drop;
use crate::{ReportedRenderings, associated_trait, expanded_externally, impl_owner, suggest};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    format_args: FormatArgsStorage,
    reported: ReportedRenderings,
    pending: LocalDefIdMap<Vec<(HirId, Span)>>,
}

lint! {
    Pass;
    pub(crate) DISPLAY_FORMAT: "`format!` wrapping one non-text `Display` value without formatting options",
    LITERAL_WRITE: "writing a literal through `Formatter::write_fmt` in place of `Formatter::write_str`",
}

impl Pass {
    pub(crate) const fn new(_: TyCtxt<'_>, format_args: FormatArgsStorage, reported: ReportedRenderings) -> Self { Self { format_args, reported, pending: LocalDefIdMap::default() } }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let enum_self_match = |source| {
            let owner = cx.tcx.hir_enclosing_body_owner(source);
            if let Some(implementation) = cx.tcx.inherent_impl_of_assoc(owner.to_def_id())
                && cx.tcx.associated_item(owner.to_def_id()).is_method()
                && cx.tcx.type_of(implementation).instantiate_identity().skip_normalization().ty_adt_def().is_some_and(AdtDef::is_enum)
                && let body = cx.tcx.hir_body_owned_by(owner)
                && let [receiver, ..] = body.params
                && let PatKind::Binding(_, binding, _, _) = receiver.pat.kind
            {
                cx.tcx.hir_parent_iter(source).any(|(_, node)| match node {
                    Node::Expr(Expr { kind: ExprKind::Match(value, _, _), .. }) =>
                        (match value.kind {
                            ExprKind::Unary(UnOp::Deref, inner) => inner,
                            _ => value,
                        })
                        .res_local_id()
                            == Some(binding),
                    _ => false,
                })
            } else {
                false
            }
        };

        if self.reported.is_some()
            && let Some(parent) = first_node_in_macro(cx, expr)
            && let Some(call) = matching_root_macro_call(cx, expr.span, sym::format_macro)
            && parent != call.expn
            && !expanded_externally(cx, call.span)
            && let Some(args) = self.format_args.get(cx, expr, call.expn)
            && let ([argument], [FormatArgsPiece::Placeholder(placeholder)]) = (args.arguments.all_args(), &args.template)
            && placeholder.format_trait == FormatTrait::Display
            && placeholder.format_options == FormatOptions::default()
            && let Some(value) = find_format_arg_expr(expr, argument)
            && let held = cx.typeck_results().expr_ty(value).peel_refs()
            && !held.is_str()
            && !held.is_lang_item(cx, LangItem::String)
            && let Some(borrowed) = get_parent_expr(cx, value)
            && matches!(borrowed.kind, ExprKind::AddrOf(..))
            && !has_temporary_drop(cx, borrowed)
        {
            let owner = cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(expr.hir_id));
            self.pending.entry(owner).or_default().push((expr.hir_id, call.span));
        }

        if let ExprKind::MethodCall(_, receiver, [_], _) = expr.kind
            && let Some(call) = root_macro_call_first_node(cx, expr)
            && matches!(cx.tcx.get_diagnostic_name(call.def_id), Some(sym::write_macro | sym::writeln_macro))
            && !expanded_externally(cx, call.span)
            && !associated_trait(cx.tcx, cx.tcx.hir_enclosing_body_owner(expr.hir_id).to_def_id()).is_diag_item(cx, sym::Display)
            && !enum_self_match(expr.hir_id)
            && cx.typeck_results().expr_ty(receiver).peel_refs().is_diag_item(cx, sym::Formatter)
            && fn_def_id(cx, expr).is_some_and(|method| cx.tcx.item_name(method) == sym::write_fmt && impl_owner(cx.tcx, method).is_diag_item(cx, sym::Formatter))
            && let Some(args) = self.format_args.get(cx, expr, call.expn)
            && args.arguments.all_args().is_empty()
        {
            let text: String = args
                .template
                .iter()
                .filter_map(|piece| match piece {
                    FormatArgsPiece::Literal(text) => Some(text.as_str()),
                    FormatArgsPiece::Placeholder(_) => None,
                })
                .collect();
            span_lint_hir_and_then(cx, LITERAL_WRITE, expr.hir_id, call.span, "formatter writes a literal through formatting arguments", |diag| {
                let mut applicability = Applicability::MachineApplicable;
                let receiver = Sugg::hir_with_context(cx, receiver, call.span.ctxt(), "formatter", &mut applicability);
                suggest(cx, expr.hir_id, diag, call.span, "write the literal directly", format!("{}.write_str({text:?})", receiver.maybe_paren()), applicability);
            });
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) {
        if self.reported.is_none() {
            return;
        }
        let owner = cx.tcx.hir_body_owner_def_id(body.id());
        if cx.tcx.typeck_root_def_id_local(owner) != owner {
            return;
        }
        let reported = self.reported.as_ref().and_then(|reported| reported.borrow_mut().remove(&owner)).unwrap_or_default();
        for (source, span) in self.pending.remove(&owner).into_iter().flatten().filter(|(source, _)| !reported.contains(source)) {
            span_lint_hir_and_then(cx, DISPLAY_FORMAT, source, span, "`format!` wraps one `Display` value without formatting options", |diag| {
                diag.help("call the `ToString::to_string` trait method on the formatted value");
            });
        }
    }
}
