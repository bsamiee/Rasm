use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, is_default_equivalent, peel_blocks,
    res::{MaybeDef as _, MaybeQPath as _},
    source::snippet,
    sym,
    ty::implements_trait,
    visitors::is_const_evaluatable,
};
use itertools::Itertools as _;
use rustc_ast::LitKind;
use rustc_attr_ir::lang_items::LangItem;
use rustc_hir::{
    Body, Expr, ExprField, ExprKind, FnDecl, StructTailExpr,
    def_id::{DefId, LocalDefId},
    intravisit::FnKind,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{TypeckResults, print::with_forced_trimmed_paths};
use rustc_span::{Span, Spanned};

use crate::{expanded_externally, impl_owner};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(MANUAL_DEFAULT_FIELD_VALUES: "hand-written `Default` impls building each field of a local struct from a constant, directly or through a constructor");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, _: FnKind<'tcx>, _: &'tcx FnDecl<'tcx>, body: &'tcx Body<'tcx>, span: Span, function: LocalDefId) {
        fn default_value<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, value: &Expr<'_>) -> bool {
            let built = typeck.expr_ty(value);
            match value.kind {
                ExprKind::Lit(Spanned { node: LitKind::Int(number, _), .. }) => number.get() == 0,
                ExprKind::Lit(Spanned { node: LitKind::Bool(set), .. }) => !set,
                ExprKind::Lit(Spanned { node: LitKind::Str(text, _), .. }) => text.is_empty(),
                ExprKind::Call(callee, []) => callee.res(typeck).opt_def_id().is_some_and(|called| {
                    cx.tcx.item_name(called) == sym::new
                        && impl_owner(cx.tcx, called).is_some_and(|owner| owner.ty_adt_def() == built.ty_adt_def())
                        && cx.tcx.get_diagnostic_item(sym::Default).is_some_and(|default| implements_trait(cx, built, default, &[]))
                }),
                ExprKind::Path(_) => value.res(typeck).ctor_parent(cx).is_lang_item(cx, LangItem::OptionNone),
                _ => false,
            }
        }
        if !expanded_externally(cx, span)
            && let Some(implementation) = cx.tcx.trait_impl_of_assoc(function.to_def_id())
            && cx.tcx.impl_trait_id(implementation).is_diag_item(cx, sym::Default)
            && let value = peel_blocks(&body.value)
            && let Some((built, typeck, delegate)) = match value.kind {
                ExprKind::Call(_, []) | ExprKind::MethodCall(_, _, [], _) =>
                    fn_def_id(cx, value).and_then(DefId::as_local).and_then(|callee| Some((peel_blocks(&cx.tcx.hir_maybe_body_owned_by(callee)?.value), cx.tcx.typeck(callee), Some(callee)))),
                _ => Some((value, cx.typeck_results(), None)),
            }
            && let ExprKind::Struct(_, fields, StructTailExpr::None) = built.kind
            && let Some(owner) = typeck.expr_ty(built).ty_adt_def()
            && owner.is_struct()
            && (delegate.is_none() || owner.non_enum_variant().fields.iter().all(|field| field.value.is_none()))
            && let counts = cx.tcx.generics_of(owner.did()).own_counts()
            && counts.types == 0
            && counts.consts == 0
            && fields.iter().all(|field| is_const_evaluatable(cx.tcx, typeck, field.expr))
            && let defaults = (|field: &ExprField<'_>| if delegate.is_some() { default_value(cx, typeck, field.expr) } else { is_default_equivalent(cx, field.expr) })
            && let defaulted = fields.iter().all(defaults)
            && (delegate.is_some() || !defaulted)
        {
            let name = cx.tcx.item_name(owner.did());
            let declared = fields
                .iter()
                .filter(|&field| !defaults(field))
                .filter_map(|field| {
                    let declared = owner.non_enum_variant().fields.get(typeck.opt_field_index(field.hir_id)?)?;
                    let held = with_forced_trimmed_paths!(cx.tcx.type_of(declared.did).instantiate_identity().skip_normalization().to_string());
                    Some(format!("`{}: {held} = {}`", field.ident, snippet(cx, field.expr.span, "..")))
                })
                .join(", ");
            let (source, help) = if defaulted {
                ("its type's default", String::from("derive `Default` and delete the impl"))
            } else {
                ("a constant", format!("declare {declared} under `#![feature(default_field_values)]`, derive `Default`, and delete the impl"))
            };
            let message = match delegate {
                Some(callee) => format!("`Default` for `{name}` builds each field from {source} through `{}`", cx.tcx.item_name(callee)),
                None => format!("`Default` for `{name}` builds each field from {source}"),
            };
            span_lint_hir_and_then(cx, MANUAL_DEFAULT_FIELD_VALUES, value.hir_id, value.span, message, |diag| {
                if delegate.is_some() {
                    diag.span_note(built.span, "fields built here");
                }
                diag.help(help);
            });
        }
    }
}
