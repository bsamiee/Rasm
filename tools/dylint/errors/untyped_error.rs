use core::{iter, mem, ops::ControlFlow};

use clippy_utils::{
    desugar_await,
    diagnostics::span_lint_hir_and_then,
    expr_or_init, find_binding_init, fn_def_id, get_expr_use_site, get_parent_expr, is_test_function,
    macros::root_macro_call_first_node,
    peel_blocks,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
    ty::{same_type_modulo_regions, ty_from_hir_ty},
    visitors::{for_each_expr, for_each_expr_without_closures},
};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    AmbigArg, Body, ClosureKind, CoroutineKind, CoroutineSource, Expr, ExprKind, FnRetTy, HirId, ImplItem, ImplItemKind, Item, ItemKind, MatchSource, Node, TraitItem, TyKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, Binder, Clause, GenericArg, Ty, TypeVisitableExt as _, print::with_forced_trimmed_paths};
use rustc_span::{DesugaringKind, Span, Symbol};

use super::{UntypedKind, body_output, text_source, untyped_kind};
use crate::{associated_trait, awaited_output, constructed, expanded_externally, exported, impl_owner, is_error, is_text, literal_or_const, parameter_of, result_parts, signature, workspace_def};

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITTEN: &str = "text written here";

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass<'tcx> {
    written: FxIndexMap<LocalDefId, Span>,
    conversions: FxHashMap<LocalDefId, FxHashMap<Ty<'tcx>, Ty<'tcx>>>,
    retained: FxHashMap<LocalDefId, bool>,
}

lint! {
    default Pass<'_>;
    UNTYPED_ERROR:
        "unmatchable `Result` error types, error variants holding erased, possibly empty, or text sources, and dependency errors built from text",
}

struct Signature<'pass, 'tcx> {
    pass: &'pass mut Pass<'tcx>,
    cx: &'pass LateContext<'tcx>,
    owner: LocalDefId,
    output: Option<Span>,
    exported: bool,
}

impl<'pass, 'tcx> Signature<'pass, 'tcx> {
    fn new(pass: &'pass mut Pass<'tcx>, cx: &'pass LateContext<'tcx>, owner: LocalDefId) -> Self {
        let output = cx.tcx.hir_node_by_def_id(owner).fn_decl().and_then(|declaration| match declaration.output {
            FnRetTy::Return(output) => Some(output.span.source_callsite()),
            FnRetTy::DefaultReturn(_) => None,
        });
        Self { pass, cx, owner, output, exported: exported(cx, owner) && cx.tcx.trait_impl_of_assoc(owner.to_def_id()).is_none() }
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
                        && match untyped {
                            UntypedKind::Unit => !self.exported || !output,
                            UntypedKind::Erased => !output || !test_entrypoint(self.cx, self.owner),
                            UntypedKind::Report => output && !test_entrypoint(self.cx, self.owner) && !self.pass.retained_report(self.cx, self.owner, error),
                            UntypedKind::Text | UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Many(_) | UntypedKind::Messages | UntypedKind::NoError(_) => true,
                        }
                        && !matches!(*hir_ty.basic_res(), Res::Def(DefKind::TyAlias, alias) if alias.is_local()
                            && result_parts(self.cx, self.cx.tcx.type_of(alias).instantiate_identity().skip_normalization()).is_some_and(|(_, aliased)| untyped_kind(self.cx, aliased).is_some()))
                    {
                        report(self.cx, hir_ty.hir_id, hir_ty.span, error, untyped);
                    },
                None =>
                    if output && let Some(UntypedKind::Many(item)) = untyped_kind(self.cx, held) {
                        span_lint_hir_and_then(self.cx, UNTYPED_ERROR, hir_ty.hir_id, hir_ty.span, with_forced_trimmed_paths!(format!("function signals success with an empty `{held}`")), |diag| {
                            diag.help(with_forced_trimmed_paths!(format!("collect the results into `validated::Validated` and return `Result<(), NEVec<{item}>>` from `.ok()`")));
                        });
                    },
            }
        }
        intravisit::walk_ty(self, t);
    }
}

impl<'tcx> Pass<'tcx> {
    fn retained_report(&mut self, cx: &LateContext<'tcx>, owner: LocalDefId, report: Ty<'tcx>) -> bool {
        let Some(body) = cx.tcx.hir_maybe_body_owned_by(owner) else { return false };
        let body = match body.value.kind {
            ExprKind::Closure(closure) if matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn))) => cx.tcx.hir_body(closure.body),
            _ => body,
        };
        *self.retained.entry(cx.tcx.hir_body_owner_def_id(body.id())).or_insert_with(|| retained_report(cx, body, report))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn test_entrypoint(cx: &LateContext<'_>, owner: LocalDefId) -> bool { matches!(cx.tcx.def_kind(cx.tcx.parent(owner.to_def_id())), DefKind::Mod) && is_test_function(cx.tcx, owner) }

fn retained_report<'tcx>(cx: &LateContext<'tcx>, body: &Body<'tcx>, report: Ty<'tcx>) -> bool {
    let body_owner = cx.tcx.hir_body_owner_def_id(body.id());
    let typeck = cx.tcx.typeck_body(body.id());
    let mut known = FxHashMap::default();
    let mut source = |mut value: &'tcx Expr<'tcx>| {
        let mut visited = Vec::new();
        let proof = (|| loop {
            if let Some(&proof) = known.get(&value.hir_id) {
                return proof;
            }
            visited.push(value.hir_id);
            value = peel_blocks(value);
            if let Some(local) = value.res_local_id()
                && let Some(init) = find_binding_init(cx, local).filter(|init| typeck.expr_adjustments(init).is_empty())
            {
                value = init;
                continue;
            }
            if let Some(awaited) = desugar_await(value) {
                value = awaited;
                continue;
            }
            let (callee, receiver, arguments) = match value.kind {
                ExprKind::Call(function, arguments) if let ty::FnDef(callee, _) = *typeck.expr_ty(function).kind() => (callee, None, arguments),
                ExprKind::MethodCall(_, receiver, arguments, _) => (typeck.type_dependent_def_id(value.hir_id)?, Some(receiver), arguments),
                _ => return None,
            };
            if callee.ctor_parent(cx).is_lang_item(cx, LangItem::ResultOk) || callee.is_lang_item(cx, LangItem::TryTraitFromOutput) && value.span.is_desugaring(DesugaringKind::TryBlock) {
                return Some(false);
            }
            let declared = awaited_output(cx.tcx, callee);
            let error = result_parts(cx, declared).map_or(declared, |(_, error)| error);
            if same_type_modulo_regions(error, report) && !error.has_non_region_param() && !callee.is_local() {
                let incoming = iter::chain(receiver, arguments).any(|input| {
                    let input = typeck.expr_ty(input);
                    same_type_modulo_regions(input, report) || result_parts(cx, input).is_some_and(|(_, error)| same_type_modulo_regions(error, report))
                });
                return (incoming || report.ty_adt_def().is_some_and(|report| report.did().krate != callee.krate)).then_some(true);
            }
            if let Some((_, error)) = result_parts(cx, declared)
                && let declaration = signature(cx.tcx, callee)
                && let Some(&input) = declaration.inputs().first()
                && result_parts(cx, input.peel_refs())
                    .is_some_and(|(ok, incoming)| same_type_modulo_regions(incoming, error) && !result_parts(cx, ok).is_some_and(|(_, nested)| same_type_modulo_regions(nested, error)))
                && impl_owner(cx.tcx, callee).is_diag_item(cx, sym::Result)
                && !declaration.inputs().iter().skip(1).any(|&input| result_parts(cx, input.peel_refs()).is_some_and(|(_, incoming)| same_type_modulo_regions(incoming, error)))
                && !cx
                    .tcx
                    .param_env(callee)
                    .caller_bounds()
                    .filter_map(Clause::as_projection_clause)
                    .map(Binder::skip_binder)
                    .filter(|bound| cx.tcx.is_lang_item(bound.trait_def_id(cx.tcx), LangItem::FnOnce))
                    .filter_map(|bound| bound.term.as_type())
                    .filter_map(|output| result_parts(cx, output))
                    .any(|(_, incoming)| same_type_modulo_regions(incoming, error))
                && let Some(input) = receiver.or_else(|| arguments.first())
            {
                value = input;
                continue;
            }
            return None;
        })();
        known.extend(visited.into_iter().map(|hir_id| (hir_id, proof)));
        proof
    };

    let returned = |part: &'tcx Expr<'tcx>| match get_expr_use_site(cx.tcx, typeck, part.span.ctxt(), part).node {
        Node::Item(item) => item.owner_id.def_id == body_owner,
        Node::ImplItem(item) => item.owner_id.def_id == body_owner,
        Node::Expr(Expr { kind: ExprKind::Closure(closure), .. }) => closure.def_id == body_owner,
        Node::Expr(Expr { hir_id, kind: ExprKind::Ret(_), .. }) => cx.tcx.hir_enclosing_body_owner(*hir_id) == body_owner,
        _ => false,
    };
    let mut retained = false;
    let unknown = for_each_expr_without_closures(body.value, |part| {
        let proof = match part.kind {
            ExprKind::Match(&Expr { kind: ExprKind::Call(_, [tried]), .. }, arms, MatchSource::TryDesugar(_))
                if arms.iter().any(|arm| match arm.body.kind {
                    ExprKind::Ret(Some(value)) | ExprKind::Break(_, Some(value)) =>
                        result_parts(cx, typeck.expr_ty(value)).is_some_and(|(_, error)| same_type_modulo_regions(error, report)) && returned(value),
                    _ => false,
                }) =>
                Some(source(tried)),
            ExprKind::Block(..) | ExprKind::If(..) | ExprKind::Loop(..) | ExprKind::Ret(..) | ExprKind::Break(..) | ExprKind::DropTemps(..) | ExprKind::Type(..) => None,
            ExprKind::Match(..) if desugar_await(part).is_none() => None,
            _ if !part.span.is_desugaring(DesugaringKind::QuestionMark)
                && (same_type_modulo_regions(typeck.expr_ty(part), report) || result_parts(cx, typeck.expr_ty(part)).is_some_and(|(_, error)| same_type_modulo_regions(error, report)))
                && returned(part) =>
                Some(source(part)),
            _ => None,
        };
        match proof {
            Some(Some(foreign)) => {
                retained |= foreign;
                ControlFlow::Continue(())
            }
            Some(None) => ControlFlow::Break(()),
            None => ControlFlow::Continue(()),
        }
    });
    retained && unknown.is_none()
}

fn fixed(cx: &LateContext<'_>, closure: &Expr<'_>) -> bool {
    let tcx = cx.tcx;
    get_parent_expr(cx, closure).and_then(|call| parameter_of(cx, call, closure)).filter(|&(callee, _)| !workspace_def(cx, callee)).is_some_and(|(callee, parameter)| {
        let inherited = tcx.generics_of(callee).parent_count;
        tcx.param_env(callee).caller_bounds().filter_map(Clause::as_projection_clause).map(Binder::skip_binder).any(|bound| {
            bound.self_ty() == parameter
                && tcx.is_lang_item(bound.trait_def_id(tcx), LangItem::FnOnce)
                && bound.term.as_type().and_then(|output| result_parts(cx, output)).is_some_and(|(_, error)| {
                    !error.walk().filter_map(GenericArg::as_type).any(|part| matches!(part.kind(), ty::Param(declared) if usize::try_from(declared.index).is_ok_and(|index| index >= inherited)))
                })
        })
    })
}

fn report<'tcx>(cx: &LateContext<'tcx>, node: HirId, span: Span, error: Ty<'tcx>, untyped: UntypedKind<'tcx>) {
    const TYPED: &str = "return the concrete source error, or add a domain error for context or distinct failures";
    let trimmed = || with_forced_trimmed_paths!(error.to_string());
    let (shown, declared, help) = match untyped {
        UntypedKind::Report => (error.to_string(), None, String::from(TYPED)),
        UntypedKind::Many(item) => (trimmed(), None, with_forced_trimmed_paths!(format!("collect the results into `validated::Validated` and return `Result<_, NEVec<{item}>>` from `.ok()`"))),
        UntypedKind::NoError(declared) if declared.is_local() => {
            let shown = trimmed();
            let help = format!("implement `core::error::Error` for `{shown}`");
            (shown, Some(declared), help)
        }
        UntypedKind::NoError(_) => return,
        UntypedKind::Text | UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit | UntypedKind::Erased | UntypedKind::Messages => (trimmed(), None, String::from(TYPED)),
    };
    span_lint_hir_and_then(cx, UNTYPED_ERROR, node, span, format!("error `{shown}` is {}", <&str>::from(untyped)), |diag| {
        if let Some(declared) = declared {
            diag.span_note(cx.tcx.def_span(declared), "declared here");
        }
        diag.help(help);
    });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        let owner = item.owner_id.def_id;
        let checked = matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::Struct | DefKind::Enum | DefKind::Union | DefKind::TyAlias) && !expanded_externally(cx, item.span);
        if checked && matches!(item.kind, ItemKind::Struct(..) | ItemKind::Enum(..)) && is_error(cx, cx.tcx.type_of(owner).instantiate_identity().skip_normalization()) {
            let variants = cx.tcx.adt_def(owner).variants().iter().filter_map(|variant| {
                let (reason, help) = variant.fields.iter().find_map(|field| {
                    let held = cx.tcx.type_of(field.did).instantiate_identity().skip_normalization();
                    match untyped_kind(cx, held) {
                        Some(untyped @ UntypedKind::Erased) => Some((<&str>::from(untyped), String::from("hold the wrapped error's concrete type as `#[source]`"))),
                        Some(untyped @ UntypedKind::Many(element)) => Some((<&str>::from(untyped), with_forced_trimmed_paths!(format!("hold the errors as `nonempty_collections::NEVec<{element}>`")))),
                        Some(UntypedKind::Report | UntypedKind::Text | UntypedKind::Code | UntypedKind::Tuple | UntypedKind::Unit | UntypedKind::Messages | UntypedKind::NoError(_)) | None => None,
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
            intravisit::walk_item(&mut Signature::new(self, cx, owner), item);
        }
    }

    fn check_trait_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx TraitItem<'tcx>) {
        if !expanded_externally(cx, item.span) {
            intravisit::walk_trait_item(&mut Signature::new(self, cx, item.owner_id.def_id), item);
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
        if !expanded_externally(cx, item.span) {
            match (cx.tcx.associated_item(owner).trait_item_def_id(), item.kind) {
                (None, _) => intravisit::walk_impl_item(&mut Signature::new(self, cx, owner), item),
                (Some(declared), ImplItemKind::Type(defined)) => {
                    intravisit::walk_impl_item(&mut Signature::new(self, cx, owner), item);
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
            let text = |part: &Expr<'_>| is_text(cx, typeck.expr_ty(part));
            iter::successors(Some(expr_or_init(cx, value)), |value| match value.kind {
                ExprKind::AddrOf(_, _, referent) => Some(expr_or_init(cx, referent)),
                _ => text_source(cx, typeck, value).map(|source| expr_or_init(cx, source)),
            })
            .find_map(|value| match value.kind {
                _ if let Some(call) = root_macro_call_first_node(cx, value).filter(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)) => Some(call.span),
                _ if literal_or_const(typeck, value) => Some(value.span),
                ExprKind::MethodCall(_, source, [], _) | ExprKind::Call(_, [source]) if text(value) && !text(source) =>
                    fn_def_id(cx, value).is_diag_item(cx, sym::to_string_method).then_some(value.span),
                _ => None,
            })
        }

        fn conversions<'tcx>(cx: &LateContext<'tcx>, root: LocalDefId) -> FxHashMap<Ty<'tcx>, Ty<'tcx>> {
            let tcx = cx.tcx;
            let typeck = tcx.typeck(root);
            let declared = |method: Option<DefId>, owner: Symbol| method.and_then(|method| associated_trait(tcx, method)).is_diag_item(cx, owner);
            let mut found = FxHashMap::default();
            for_each_expr(tcx, tcx.hir_body_owned_by(root).value, |part| {
                let conversion = match part.kind {
                    ExprKind::Match(&Expr { kind: ExprKind::Call(_, [tried]), .. }, arms, MatchSource::TryDesugar(_)) => arms
                        .iter()
                        .find_map(|arm| match arm.body.kind {
                            ExprKind::Ret(Some(returned)) | ExprKind::Break(_, Some(returned)) => result_parts(cx, typeck.expr_ty(returned)).map(|(_, target)| target),
                            _ => None,
                        })
                        .zip(result_parts(cx, typeck.expr_ty(tried)).map(|(_, source)| source)),
                    ExprKind::MethodCall(_, operand, [], _) if declared(typeck.type_dependent_def_id(part.hir_id), sym::Into) => Some((typeck.expr_ty(part), typeck.expr_ty(operand))),
                    ExprKind::Path(qpath)
                        if matches!(typeck.qpath_res(&qpath, part.hir_id).opt_def_id().and_then(|method| associated_trait(tcx, method)).opt_diag_name(cx), Some(sym::From | sym::Into)) =>
                    {
                        let declaration = typeck.expr_ty(part).fn_sig(tcx).skip_binder();
                        declaration.inputs().first().map(|&source| (declaration.output(), source))
                    }
                    _ => None,
                };
                if let Some((target, source)) = conversion
                    && source.ty_adt_def().is_some_and(|adt| adt.did().is_local())
                    && is_error(cx, source)
                {
                    found.entry(target).or_insert(source);
                }
                ControlFlow::<()>::Continue(())
            });
            found
        }

        fn handed<'tcx>(cx: &LateContext<'tcx>, call: &Expr<'_>, error: Ty<'tcx>) -> bool {
            let (receiver, arguments) = match call.kind {
                ExprKind::MethodCall(_, receiver, arguments, _) => (Some(receiver), arguments),
                ExprKind::Call(_, arguments) => (None, arguments),
                _ => (None, [].as_slice()),
            };
            iter::chain(receiver, arguments).any(|operand| {
                let operand = cx.typeck_results().expr_ty(operand).peel_refs();
                same_type_modulo_regions(operand, error) || result_parts(cx, operand).is_some_and(|(_, incoming)| same_type_modulo_regions(incoming, error))
            })
        }

        let typeck = cx.typeck_results();
        if let Some((variant, mut fields)) = constructed(typeck, expr)
            && let Some(local) = variant.def_id.as_local()
            && !expanded_externally(cx, cx.tcx.def_span(local))
            && !variant.fields.is_empty()
            && variant.fields.iter().all(|field| is_text(cx, cx.tcx.type_of(field.did).instantiate_identity().skip_normalization()))
            && is_error(cx, typeck.expr_ty(expr))
            && let Some(site) = fields.find_map(|(_, value)| written(cx, value))
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
            && let root = cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(expr.hir_id))
            && let Some(converted) = self.conversions.entry(root).or_insert_with(|| conversions(cx, root)).get(&produced).copied()
        {
            let (produced, converted) = with_forced_trimmed_paths!((produced.to_string(), converted.to_string()));
            span_lint_hir_and_then(cx, UNTYPED_ERROR, expr.hir_id, expr.span, format!("`{produced}` is built from written text while local `{converted}` converts into it"), |diag| {
                diag.span_note(site, WRITTEN);
                diag.help(format!("add a variant holding the subject to `{converted}` and convert it into `{produced}`"));
            });
        }
        let raised = match expr.kind {
            _ if expanded_externally(cx, expr.span) => None,
            ExprKind::Closure(closure) if !matches!(closure.kind, ClosureKind::Coroutine(CoroutineKind::Desugared(_, CoroutineSource::Fn))) && !fixed(cx, expr) => body_output(cx, closure.def_id)
                .and_then(|returned| result_parts(cx, returned).map(|(_, error)| error).or_else(|| mapped(cx, expr, returned)))
                .filter(|&error| !matches!(untyped_kind(cx, error), Some(UntypedKind::Report)) || !self.retained_report(cx, closure.def_id, error))
                .map(|error| (closure.fn_decl_span, error)),
            ExprKind::Path(_) if typeck.expr_ty(expr).is_fn() => {
                let declaration = typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder();
                mapped(cx, expr, declaration.output()).filter(|&error| !declaration.inputs().iter().any(|&input| same_type_modulo_regions(input.peel_refs(), error))).map(|error| (expr.span, error))
            }
            ExprKind::Call(_, [_]) if fn_def_id(cx, expr).ctor_parent(cx).is_lang_item(cx, LangItem::ResultErr) => result_parts(cx, typeck.expr_ty(expr)).map(|(_, error)| (expr.span, error)),
            ExprKind::Call(..) | ExprKind::MethodCall(..)
                if let Some(callee) = fn_def_id(cx, expr)
                    && let produced = typeck.expr_ty(expr)
                    && let error = result_parts(cx, produced).map_or(produced, |(_, error)| error)
                    && (error.ty_adt_def().is_some_and(|report| report.did().krate == callee.krate) || matches!(associated_trait(cx.tcx, callee).opt_diag_name(cx), Some(sym::From | sym::Into)))
                    && matches!(untyped_kind(cx, error), Some(UntypedKind::Report))
                    && !handed(cx, expr, error) =>
                Some((expr.span, error)),
            _ => None,
        };
        if let Some((span, error)) = raised
            && let Some(untyped) = untyped_kind(cx, error)
            && !(matches!(untyped, UntypedKind::Tuple) && error.tuple_fields().last().is_some_and(|&source| is_error(cx, source)))
            && !{
                let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id);
                body_output(cx, owner).and_then(|enclosing| result_parts(cx, enclosing)).is_some_and(|(_, enclosing)| {
                    same_type_modulo_regions(enclosing, error)
                        && (!matches!(untyped, UntypedKind::Report)
                            || !test_entrypoint(cx, owner)
                                && !expanded_externally(cx, cx.tcx.def_span(owner))
                                && !matches!(cx.tcx.hir_node_by_def_id(owner), Node::Expr(closure) if matches!(closure.kind, ExprKind::Closure(_)) && fixed(cx, closure))
                                && !self.retained_report(cx, owner, error))
                })
            }
        {
            report(cx, expr.hir_id, span, error, untyped);
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) {
        let owner = cx.tcx.hir_body_owner_def_id(body.id());
        self.conversions.remove(&owner);
        self.retained.remove(&owner);
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        for (variant, site) in mem::take(&mut self.written) {
            span_lint_hir_and_then(
                cx,
                UNTYPED_ERROR,
                cx.tcx.local_def_id_to_hir_id(variant),
                cx.tcx.def_span(variant),
                format!("error `{}` holds {}", cx.tcx.item_name(variant), <&str>::from(UntypedKind::Text)),
                |diag| {
                    diag.span_note(site, WRITTEN);
                    diag.help("move the text into `#[error(..)]` with each interpolated value as a typed field");
                },
            );
        }
    }
}
