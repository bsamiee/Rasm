use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, is_default_equivalent,
    paths::PathNS,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _, MaybeTypeckRes as _},
    source::snippet_with_context,
    sugg::Sugg,
    sym,
    ty::{get_iterator_item_ty, implements_trait, option_arg_ty},
    visitors::is_const_evaluatable,
};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Applicability;
use rustc_hir::{
    Body, BorrowKind, Expr, ExprField, ExprKind, FnDecl, ImplItem, ImplItemKind, LetStmt, MatchSource, Mutability, StructTailExpr, UnOp,
    def_id::{DefId, LocalDefId},
    intravisit::FnKind,
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AdtDef, GenericArg, Ty, TyCtxt};
use rustc_span::Span;

use crate::{accessed_field, boxed_future, derives, path};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    MANUAL_LANGUAGE_FEATURE,
    Warn,
    "hand-written forms a nightly language feature states directly"
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct ManualLanguageFeature {
    views: FxHashSet<DefId>,
}

impl_lint_pass!(ManualLanguageFeature => [MANUAL_LANGUAGE_FEATURE]);

impl ManualLanguageFeature {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { views: [sym::string_as_str, sym::vec_as_slice].into_iter().filter_map(|name| tcx.get_diagnostic_item(name)).chain(path(tcx, PathNS::Value, "core::option::Option::as_deref")).collect() }
    }

    fn deref_scrutinee<'tcx>(&self, cx: &LateContext<'tcx>, scrutinee: &'tcx Expr<'tcx>) {
        fn derefs_purely<'tcx>(cx: &LateContext<'tcx>, owned: Ty<'tcx>) -> bool { cx.tcx.lang_items().deref_pure_trait().is_some_and(|pure| implements_trait(cx, owned, pure, &[])) }

        let typeck = cx.typeck_results();
        let viewed = match scrutinee.kind {
            ExprKind::MethodCall(view, receiver, [], _)
                if cx.ty_based_def(scrutinee).opt_def_id().is_some_and(|resolved| self.views.contains(&resolved))
                    && let received = typeck.expr_ty(receiver).peel_refs()
                    && derefs_purely(cx, option_arg_ty(cx, received).unwrap_or(received)) =>
                Some((receiver, format!("`{}`", view.ident))),
            ExprKind::AddrOf(BorrowKind::Ref, Mutability::Not, &Expr { kind: ExprKind::Unary(UnOp::Deref, owner), .. })
                if let owned = typeck.expr_ty(owner)
                    && !owned.is_ref()
                    && derefs_purely(cx, owned) =>
                Some((
                    match owner.kind {
                        ExprKind::Unary(UnOp::Deref, referenced) if typeck.expr_ty(referenced).is_ref() => referenced,
                        _ => owner,
                    },
                    "`&*`".to_owned(),
                )),
            _ => None,
        };
        if let Some((receiver, view)) = viewed {
            let mut applicability = Applicability::MaybeIncorrect;
            let borrowed = typeck.expr_ty(receiver).is_ref();
            let receiver = Sugg::hir_with_context(cx, receiver, scrutinee.span.ctxt(), "..", &mut applicability);
            let receiver = if borrowed { receiver } else { receiver.addr() };
            span_lint_hir_and_then(cx, MANUAL_LANGUAGE_FEATURE, scrutinee.hir_id, scrutinee.span, format!("{view} derefs the scrutinee for its patterns"), |diag| {
                diag.span_suggestion(scrutinee.span, "match a reference to the value under `#![feature(deref_patterns)]`", receiver.to_string(), applicability);
            });
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for ManualLanguageFeature {
    fn check_fn(&mut self, cx: &LateContext<'tcx>, kind: FnKind<'tcx>, _: &'tcx FnDecl<'tcx>, body: &'tcx Body<'tcx>, span: Span, function: LocalDefId) {
        if matches!(kind, FnKind::Method(..))
            && !span.from_expansion()
            && let Some(implementation) = cx.tcx.trait_impl_of_assoc(function.to_def_id())
            && cx.tcx.impl_trait_id(implementation).is_diag_item(cx, sym::Default)
            && !cx.tcx.is_automatically_derived(implementation)
            && let built = peel_blocks(body.value)
            && let ExprKind::Struct(_, fields, StructTailExpr::None) = built.kind
            && let ty::Adt(owner, _) = cx.typeck_results().expr_ty(built).kind()
            && owner.is_struct()
            && owner.did().is_local()
            && owner.non_enum_variant().fields.iter().all(|field| field.value.is_none())
            && let counts = cx.tcx.generics_of(owner.did()).own_counts()
            && counts.types == 0
            && counts.consts == 0
            && fields.iter().all(|field| is_const_evaluatable(cx.tcx, cx.typeck_results(), field.expr))
            && !fields.iter().all(|field| is_default_equivalent(cx, field.expr))
        {
            span_lint_hir_and_then(cx, MANUAL_LANGUAGE_FEATURE, built.hir_id, built.span, format!("`Default` for `{}` builds each field from a constant", cx.tcx.item_name(owner.did())), |diag| {
                diag.help("declare each value on its field as `field: T = value` and derive `Default` under `#![feature(default_field_values)]`");
            });
        }
    }

    fn check_impl_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx ImplItem<'tcx>) {
        if let ImplItemKind::Type(declared) = item.kind
            && !item.span.from_expansion()
        {
            let held = cx.tcx.type_of(item.owner_id).instantiate_identity();
            if cx.tcx.trait_impl_of_assoc(item.owner_id.to_def_id()).is_some()
                && !cx.effective_visibilities.is_exported(item.owner_id.def_id)
                && boxed_future(cx, cx.tcx.normalize_erasing_regions(cx.typing_env(), held)).is_some()
            {
                span_lint_hir_and_then(cx, MANUAL_LANGUAGE_FEATURE, item.hir_id(), declared.span, format!("associated type `{}` names a boxed `dyn Future`", item.ident), |diag| {
                    diag.help("declare the type as `impl Future` with the same bounds under `#![feature(impl_trait_in_assoc_type)]`");
                });
            }
            if let Some(iterator) = cx.tcx.get_diagnostic_item(sym::Iterator)
                && held.skip_normalization().walk().filter_map(GenericArg::as_type).any(|adapter| {
                    matches!(adapter.kind(), ty::Adt(_, arguments)
                        if arguments.types().any(Ty::is_fn_ptr)
                            && implements_trait(cx, adapter, iterator, &[])
                            && arguments.types().any(|argument| argument.is_fn_ptr() && get_iterator_item_ty(cx, adapter) != Some(argument)))
                })
            {
                span_lint_hir_and_then(
                    cx,
                    MANUAL_LANGUAGE_FEATURE,
                    item.hir_id(),
                    declared.span,
                    format!("`{}` names an iterator adapter through a `fn` pointer in place of its closure", item.ident),
                    |diag| {
                        diag.help("declare the type as `impl Iterator<Item = ..>` and build it with a closure under `#![feature(impl_trait_in_assoc_type)]`");
                    },
                );
            }
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let (Some(init), Some(_)) = (local.init, local.els)
            && !local.span.from_expansion()
        {
            self.deref_scrutinee(cx, init);
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn rebuilt<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
            fn copied<'tcx>(cx: &LateContext<'tcx>, built: Ty<'tcx>, field: &ExprField<'tcx>) -> Option<&'tcx Expr<'tcx>> {
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
            }

            if let ExprKind::Struct(path, fields, StructTailExpr::None) = expr.kind
                && let built = cx.typeck_results().expr_ty(expr)
                && built.ty_adt_def().is_some_and(AdtDef::is_struct)
                && let sources = fields.iter().map(|field| copied(cx, built, field)).collect::<Vec<_>>()
                && let Some(base) = sources.iter().flatten().next()
                && sources.iter().flatten().all(|other| other.res_local_id() == base.res_local_id())
            {
                let ctxt = expr.span.ctxt();
                let mut applicability = Applicability::MaybeIncorrect;
                let kept = iter::zip(fields, &sources).filter(|(_, source)| source.is_none()).map(|(field, _)| snippet_with_context(cx, field.span, ctxt, "..", &mut applicability).0).join(", ");
                let (named, _) = snippet_with_context(cx, path.span(), ctxt, "..", &mut applicability);
                let base = Sugg::hir_with_context(cx, base, ctxt, "..", &mut applicability);
                span_lint_hir_and_then(cx, MANUAL_LANGUAGE_FEATURE, expr.hir_id, expr.span, format!("`{named}` rebuilt field by field from `{base}` to change its type arguments"), |diag| {
                    diag.span_suggestion(expr.span, "write the changed fields and the base under `#![feature(type_changing_struct_update)]`", format!("{named} {{ {kept}, ..{base} }}"), applicability);
                });
            }
        }

        fn default_base<'tcx>(cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
            if let ExprKind::Struct(_, fields, StructTailExpr::Base(base)) = expr.kind
                && let ExprKind::Call(_, []) = base.kind
                && fn_def_id(cx, base).is_diag_item(cx, sym::default_fn)
                && let typeck = cx.typeck_results()
                && let built = typeck.expr_ty(expr)
                && let ty::Adt(owner, _) = built.kind()
                && owner.is_struct()
                && let written = fields.iter().filter_map(|field| typeck.opt_field_index(field.hir_id)).collect::<FxHashSet<_>>()
                && let declared = &owner.non_enum_variant().fields
                && written.len() < declared.len()
                && declared.iter_enumerated().all(|(index, field)| written.contains(&index) || field.value.is_some())
            {
                let applicability = if derives(cx, built, sym::Default) { Applicability::MachineApplicable } else { Applicability::MaybeIncorrect };
                span_lint_hir_and_then(cx, MANUAL_LANGUAGE_FEATURE, expr.hir_id, base.span, "`..Default::default()` fills fields that declare their own default values", |diag| {
                    diag.span_suggestion(base.span, "write `..` to take each field's declared default", "", applicability);
                });
            }
        }

        if !expr.span.from_expansion() {
            if let ExprKind::Match(scrutinee, _, MatchSource::Normal) = expr.kind {
                self.deref_scrutinee(cx, scrutinee);
            }
            if let ExprKind::Let(binding) = expr.kind {
                self.deref_scrutinee(cx, binding.init);
            }
            default_base(cx, expr);
            rebuilt(cx, expr);
        }
    }
}
