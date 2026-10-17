use core::{mem, ops::ControlFlow};

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id, get_parent_expr, is_lint_allowed,
    macros::root_macro_call_first_node,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{implements_trait, same_type_modulo_regions, ty_from_hir_ty},
    visitors::for_each_expr,
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxIndexMap;
use rustc_hir::{
    AmbigArg, ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, FnRetTy, HirId, ImplItem, ImplItemKind, Item, ItemKind, MatchSource, TraitItem, TyKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, GenericArg, Ty, TypeVisitableExt as _, layout::LayoutOf as _, print::with_forced_trimmed_paths};
use rustc_span::{Span, Symbol};

use super::{UntypedKind, body_output, untyped_kind};
use crate::{awaited_output, constructed, expanded_externally, exported, is_error, is_text, literal_or_const, result_parts};

// --- [CONSTANTS] -----------------------------------------------------------------------

const HELP: &str = "give the variant typed fields, with the wrapped error's own type as `#[source]`";
const WRITTEN: &str = "text written here";

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    written: FxIndexMap<LocalDefId, Span>,
}

lint! {
    default Pass;
    UNTYPED_ERROR:
        "`Result` error types callers cannot match, error variants holding erased, possibly empty, one-value, or written-text sources, and dependency errors built from text",
}

struct Signature<'pass, 'tcx> {
    cx: &'pass LateContext<'tcx>,
    output: Option<Span>,
    exported: bool,
}

impl<'pass, 'tcx> Signature<'pass, 'tcx> {
    fn new(cx: &'pass LateContext<'tcx>, owner: LocalDefId) -> Self {
        let output = cx.tcx.hir_node_by_def_id(owner).fn_decl().and_then(|declaration| match declaration.output {
            FnRetTy::Return(output) => Some(output.span.source_callsite()),
            FnRetTy::DefaultReturn(_) => None,
        });
        Self { cx, output, exported: exported(cx, owner) && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none() }
    }
}

impl<'tcx> Visitor<'tcx> for Signature<'_, 'tcx> {
    fn visit_ty(&mut self, t: &'tcx rustc_hir::Ty<'tcx, AmbigArg>) {
        let hir_ty = t.as_unambig_ty();
        if matches!(hir_ty.kind, TyKind::Path(_)) && !expanded_externally(self.cx, hir_ty.span) {
            let output = self.output == Some(hir_ty.span);
            let held = ty_from_hir_ty(self.cx, hir_ty);
            match result_parts(self.cx, held) {
                Some((_, error)) =>
                    if let Some(untyped) = untyped_kind(self.cx, error)
                        && !(matches!(untyped, UntypedKind::Unit) && self.exported && output)
                        && (!matches!(untyped, UntypedKind::Report) || output)
                        && !matches!(*hir_ty.basic_res(), Res::Def(DefKind::TyAlias, alias) if alias.is_local()
                            && result_parts(self.cx, self.cx.tcx.type_of(alias).instantiate_identity().skip_normalization()).is_some_and(|(_, aliased)| untyped_kind(self.cx, aliased).is_some()))
                    {
                        report(self.cx, hir_ty.hir_id, hir_ty.span, error, untyped);
                    },
                None =>
                    if output && matches!(untyped_kind(self.cx, held), Some(UntypedKind::Many)) {
                        span_lint_hir_and_then(
                            self.cx,
                            UNTYPED_ERROR,
                            hir_ty.hir_id,
                            hir_ty.span,
                            with_forced_trimmed_paths!(format!("function returns `{held}`, where an empty collection means success")),
                            |diag| {
                                diag.help("return `Result<(), NEVec<E>>` built by collecting into `validated::Validated`");
                            },
                        );
                    },
            }
        }
        intravisit::walk_ty(self, t);
    }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        fn singular<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> bool {
            !held.has_param()
                && is_error(cx, held)
                && held.ty_adt_def().is_some_and(|adt| !adt.is_variant_list_non_exhaustive() && adt.variants().iter().all(|variant| !variant.is_field_list_non_exhaustive()))
                && cx.layout_of(held).is_ok_and(|layout| layout.is_zst() && !layout.is_uninhabited())
        }

        let owner = item.owner_id.def_id;
        let checked = matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::Struct | DefKind::Enum | DefKind::Union | DefKind::TyAlias) && !skipped(cx, item.span, item.hir_id());
        if checked && matches!(item.kind, ItemKind::Struct(..) | ItemKind::Enum(..)) && is_error(cx, cx.tcx.type_of(owner).instantiate_identity().skip_normalization()) {
            let variants = cx.tcx.adt_def(owner).variants().iter().filter_map(|variant| {
                let (reason, help) = variant.fields.iter().find_map(|field| {
                    let held = cx.tcx.type_of(field.did).instantiate_identity().skip_normalization();
                    match untyped_kind(cx, held) {
                        Some(untyped @ (UntypedKind::Erased | UntypedKind::Report)) => Some((untyped.reason(), HELP)),
                        Some(untyped @ UntypedKind::Many) => Some((untyped.reason(), "hold the errors in a `nonempty_collections::NEVec`")),
                        Some(UntypedKind::Text | UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit | UntypedKind::Messages | UntypedKind::NoError(_)) | None =>
                            singular(cx, held).then_some((
                                "a one-value source error that adds nothing to the variant",
                                "delete the field, state the source's cause in the variant's message, and convert with a `map_err` closure matching the source's one value",
                            )),
                    }
                })?;
                Some((variant, variant.def_id.as_local()?, reason, help))
            });
            for (variant, local, reason, help) in variants {
                span_lint_hir_and_then(cx, UNTYPED_ERROR, cx.tcx.local_def_id_to_hir_id(local), cx.tcx.def_span(local), format!("error `{}` holds {reason}", variant.name), |diag| {
                    diag.help(help);
                });
            }
        }
        if checked {
            intravisit::walk_item(&mut Signature::new(cx, owner), item);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if !skipped(cx, item.span, item.hir_id()) {
            intravisit::walk_trait_item(&mut Signature::new(cx, item.owner_id.def_id), item);
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        fn error_alias(cx: &LateContext<'_>, declared: DefId) -> bool {
            cx.tcx.associated_items(cx.tcx.parent(declared)).in_definition_order().filter(|method| method.is_fn()).any(|method| {
                awaited_output(cx.tcx, method.def_id)
                    .walk()
                    .filter_map(GenericArg::as_type)
                    .any(|held| result_parts(cx, held).is_some_and(|(_, error)| matches!(error.kind(), ty::Alias(_, alias) if matches!(alias.kind, ty::Projection { def_id } if def_id == declared))))
            })
        }

        let owner = item.owner_id.def_id;
        if !skipped(cx, item.span, item.hir_id()) {
            match (cx.tcx.associated_item(owner).trait_item_def_id(), item.kind) {
                (None, _) => intravisit::walk_impl_item(&mut Signature::new(cx, owner), item),
                (Some(declared), ImplItemKind::Type(defined)) => {
                    intravisit::walk_impl_item(&mut Signature::new(cx, owner), item);
                    let error = cx.tcx.type_of(owner).instantiate_identity().skip_normalization();
                    if let Some(untyped) = untyped_kind(cx, error)
                        && error_alias(cx, declared)
                    {
                        report(cx, item.hir_id(), defined.span, error, untyped);
                    }
                }
                (Some(_), _) => {}
            }
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn mapped<'tcx>(cx: &LateContext<'tcx>, function: &Expr<'_>, returned: Ty<'tcx>) -> Option<Ty<'tcx>> {
            let typeck = cx.typeck_results();
            let call = get_parent_expr(cx, function)?;
            let ExprKind::MethodCall(_, receiver, ..) = call.kind else { return None };
            result_parts(cx, typeck.expr_ty(call))
                .map(|(_, error)| error)
                .filter(|error| same_type_modulo_regions(*error, returned) && !result_parts(cx, typeck.expr_ty(receiver).peel_refs()).is_some_and(|(_, held)| same_type_modulo_regions(held, *error)))
        }

        fn written(cx: &LateContext<'_>, value: &Expr<'_>) -> Option<Span> {
            let typeck = cx.typeck_results();
            let value = expr_or_init(cx, value);
            let text = |part: &Expr<'_>| is_text(cx, typeck.expr_ty(part));
            match value.kind {
                _ if let Some(call) = root_macro_call_first_node(cx, value).filter(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)) => Some(call.span),
                _ if literal_or_const(typeck, value) => Some(value.span),
                ExprKind::AddrOf(_, _, referent) => written(cx, referent),
                ExprKind::MethodCall(_, source, [], _) | ExprKind::Call(_, [source]) if text(value) && text(source) => written(cx, source),
                ExprKind::MethodCall(_, _, [], _) | ExprKind::Call(_, [_]) if text(value) => fn_def_id(cx, value).is_diag_item(cx, sym::to_string_method).then_some(value.span),
                _ => None,
            }
        }

        fn converted<'tcx>(cx: &LateContext<'tcx>, node: HirId, target: Ty<'tcx>) -> Option<Ty<'tcx>> {
            let tcx = cx.tcx;
            let root = tcx.typeck_root_def_id(tcx.hir_enclosing_body_owner(node).to_def_id()).as_local()?;
            let (typeck, from) = (tcx.typeck(root), tcx.get_diagnostic_item(sym::From)?);
            let declared = |method: Option<DefId>, owner: Symbol| method.opt_parent(cx).is_diag_item(cx, owner);
            for_each_expr(tcx, tcx.hir_body_owned_by(root).value, |part| {
                let source = match part.kind {
                    ExprKind::Match(&Expr { kind: ExprKind::Call(_, [tried]), .. }, _, MatchSource::TryDesugar(_)) => result_parts(cx, typeck.expr_ty(tried)).map(|(_, error)| error),
                    ExprKind::MethodCall(_, operand, [], _) if declared(typeck.type_dependent_def_id(part.hir_id), sym::Into) && typeck.expr_ty(part) == target => Some(typeck.expr_ty(operand)),
                    ExprKind::Path(qpath) if declared(typeck.qpath_res(&qpath, part.hir_id).opt_def_id(), sym::From) =>
                        Some(typeck.node_args(part.hir_id)).filter(|args| args.type_at(0) == target).map(|args| args.type_at(1)),
                    _ => None,
                };
                source
                    .filter(|&source| source.ty_adt_def().is_some_and(|adt| adt.did().is_local()) && is_error(cx, source) && implements_trait(cx, target, from, &[source.into()]))
                    .map_or(ControlFlow::Continue(()), ControlFlow::Break)
            })
        }

        let typeck = cx.typeck_results();
        if let Some((variant, fields)) = constructed(typeck, expr)
            && let Some(local) = variant.def_id.as_local()
            && !expanded_externally(cx, cx.tcx.def_span(local))
            && !variant.fields.is_empty()
            && variant.fields.iter().all(|field| is_text(cx, cx.tcx.type_of(field.did).instantiate_identity().skip_normalization()))
            && is_error(cx, typeck.expr_ty(expr))
            && let Some(site) = fields.iter().find_map(|&(_, value)| written(cx, value))
        {
            self.written.entry(local).or_insert(site);
        }
        if let ExprKind::Call(_, arguments) | ExprKind::MethodCall(_, _, arguments, _) = expr.kind
            && !expanded_externally(cx, expr.span)
            && let produced = typeck.expr_ty(expr)
            && let Some(foreign) = produced.ty_adt_def().filter(|adt| !adt.did().is_local())
            && fn_def_id(cx, expr).is_some_and(|callee| callee.krate == foreign.did().krate)
            && is_error(cx, produced)
            && let Some(site) = arguments.iter().filter(|argument| is_text(cx, typeck.expr_ty(argument))).find_map(|argument| written(cx, argument))
            && let Some(converted) = converted(cx, expr.hir_id, produced)
        {
            let (produced, converted) = with_forced_trimmed_paths!((produced.to_string(), converted.to_string()));
            span_lint_hir_and_then(cx, UNTYPED_ERROR, expr.hir_id, expr.span, format!("`{produced}` is built from written text while local `{converted}` converts into it"), |diag| {
                diag.span_note(site, WRITTEN);
                diag.help(format!("add a variant holding the subject to `{converted}` and convert it into `{produced}`"));
            });
        }
        let raised = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::Closure(closure) if !matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn))) => body_output(cx, closure.def_id)
                .and_then(|returned| result_parts(cx, returned).map(|(_, error)| error).or_else(|| mapped(cx, expr, returned)))
                .map(|error| (closure.fn_decl_span, error)),
            ExprKind::Path(_) if typeck.expr_ty(expr).is_fn() => mapped(cx, expr, typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder().output()).map(|error| (expr.span, error)),
            ExprKind::Call(_, [_]) if fn_def_id(cx, expr).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr) => result_parts(cx, typeck.expr_ty(expr)).map(|(_, error)| (expr.span, error)),
            ExprKind::Call(..) | ExprKind::MethodCall(..)
                if let Some(callee) = fn_def_id(cx, expr)
                    && let produced = typeck.expr_ty(expr)
                    && let error = result_parts(cx, produced).map_or(produced, |(_, error)| error)
                    && error.ty_adt_def().is_some_and(|report| report.did().krate == callee.krate)
                    && matches!(untyped_kind(cx, error), Some(UntypedKind::Report)) =>
                Some((expr.span, error)),
            _ => None,
        };
        if let Some((span, error)) = raised
            && let Some(untyped) = untyped_kind(cx, error)
            && !body_output(cx, cx.tcx.hir_enclosing_body_owner(expr.hir_id)).and_then(|enclosing| result_parts(cx, enclosing)).is_some_and(|(_, enclosing)| same_type_modulo_regions(enclosing, error))
        {
            report(cx, expr.hir_id, span, error, untyped);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (variant, site) in mem::take(&mut self.written) {
            span_lint_hir_and_then(
                cx,
                UNTYPED_ERROR,
                cx.tcx.local_def_id_to_hir_id(variant),
                cx.tcx.def_span(variant),
                format!("error `{}` holds {}", cx.tcx.item_name(variant), UntypedKind::Text.reason()),
                |diag| {
                    diag.span_note(site, WRITTEN);
                    diag.help(HELP);
                },
            );
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn skipped(cx: &LateContext<'_>, span: Span, node: HirId) -> bool { expanded_externally(cx, span) || is_lint_allowed(cx, UNTYPED_ERROR, node) }

fn report<'tcx>(cx: &LateContext<'tcx>, node: HirId, span: Span, error: Ty<'tcx>, untyped: UntypedKind) {
    const TYPED: &str = "return a `thiserror` enum with a variant per failure and its typed source";
    let trimmed = || with_forced_trimmed_paths!(error.to_string());
    let (shown, declared, help) = match untyped {
        UntypedKind::Report => (error.to_string(), None, TYPED),
        UntypedKind::Many => (trimmed(), None, "return `Result<T, NEVec<E>>` built by collecting into `validated::Validated`"),
        UntypedKind::NoError(declared) if declared.is_local() => (trimmed(), Some(declared), "derive `thiserror::Error` on the type in place of its hand-written `Display`"),
        UntypedKind::Text | UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit | UntypedKind::Erased | UntypedKind::Messages | UntypedKind::NoError(_) => (trimmed(), None, TYPED),
    };
    span_lint_hir_and_then(cx, UNTYPED_ERROR, node, span, format!("`Result` error `{shown}` is {}", untyped.reason()), |diag| {
        if let Some(declared) = declared {
            diag.span_note(cx.tcx.def_span(declared), "declared here");
        }
        diag.help(help);
    });
}
