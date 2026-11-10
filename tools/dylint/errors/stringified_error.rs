use core::ops::ControlFlow;
use std::borrow::Cow;

use clippy_utils::diagnostics::span_lint_hir_and_then;
use clippy_utils::macros::{FormatArgsStorage, find_format_arg_expr, first_node_macro_backtrace, macro_backtrace};
use clippy_utils::paths::PathNS;
use clippy_utils::res::{MaybeDef as _, MaybeResPath as _};
use clippy_utils::{ExprUseNode, fn_def_id, fn_def_id_with_node_args, get_expr_use_site, get_parent_expr_for_hir, peel_blocks, peel_hir_expr_while, sym};
use itertools::{FoldWhile, Itertools as _};
use rustc_ast::{BinOpKind, FormatArgsPiece, UnOp};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::{FxHashMap, FxHashSet};
use rustc_hir::def_id::{DefId, LocalDefId};
use rustc_hir::intravisit::{self, Visitor};
use rustc_hir::{Body, Expr, ExprKind, HirId, HirIdSet, MatchSource, Node, Pat, PatKind};
use rustc_hir_typeck::expr_use_visitor::{Delegate, ExprUseVisitor, PlaceBase, PlaceWithHirId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::hir::nested_filter;
use rustc_middle::mir::FakeReadCause;
use rustc_middle::ty::{self, Ty, TyCtxt, TypeVisitableExt as _};
use rustc_trait_selection::regions::test_region_obligations;
use rustc_trait_selection::traits::{Obligation, ObligationCause, ObligationCtxt};

use super::{text_source, untyped_kind};
use crate::macros::{assertions, cargo_test_target};
use crate::{
    ReportedRenderings, associated_trait, constructed, expanded_externally, fn_item_value, impl_owner, in_test, is_error, is_text, parameter, path, paths, pointee, record_rendering, resolve,
    result_parts, signature,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy)]
enum Sink {
    Compared,
    Passed,
    Returned,
    Stored,
    Source,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Rendering {
    Produced(LocalDefId),
    Deferred(DefId),
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass<'tcx> {
    format_args: FormatArgsStorage,
    reported: ReportedRenderings,
    assertions: FxHashSet<DefId>,
    test_target: bool,
    comparisons: FxHashSet<DefId>,
    transfers: FxHashMap<DefId, usize>,
    applied: FxHashSet<DefId>,
    push: FxHashSet<DefId>,
    io_error: Option<DefId>,
    uses: FxHashMap<LocalDefId, Uses<'tcx>>,
}

lint!(Pass<'_>; STRINGIFIED_ERROR: "errors rendered to text that is stored, returned, or compared, and values rendered to text that is compared");

impl<'tcx> Pass<'tcx> {
    pub(crate) fn new(tcx: TyCtxt<'_>, format_args: FormatArgsStorage, reported: ReportedRenderings) -> Self {
        Self {
            format_args,
            reported,
            assertions: assertions(tcx),
            test_target: cargo_test_target(tcx),
            comparisons: paths(
                tcx,
                PathNS::Value,
                &[
                    "str::contains",
                    "str::starts_with",
                    "str::ends_with",
                    "str::eq_ignore_ascii_case",
                    "str::eq_ignore_case_unnormalized",
                    "std::collections::BTreeSet::contains",
                    "std::collections::HashSet::contains",
                    "std::collections::BTreeMap::contains_key",
                    "std::collections::HashMap::contains_key",
                ],
            ),
            transfers: resolve(tcx, PathNS::Value, &[("slice::join", 1), ("core::option::Option::unwrap_or_default", 0), ("core::option::Option::as_deref", 0), ("str::split_once", 0)]),
            applied: paths(tcx, PathNS::Value, &["core::option::Option::map", "core::result::Result::map", "core::result::Result::map_err"]),
            push: paths(tcx, PathNS::Value, &["alloc::vec::Vec::push"]),
            io_error: path(tcx, PathNS::Type, "std::io::Error"),
            uses: FxHashMap::default(),
        }
    }

    fn after(&mut self, cx: &LateContext<'tcx>, binding: HirId, written: HirId) -> impl DoubleEndedIterator<Item = &'tcx Expr<'tcx>> {
        let root = cx.tcx.typeck_root_def_id_local(cx.tcx.hir_enclosing_body_owner(written));
        self.uses.entry(root).or_insert_with(|| Uses::new(cx.tcx, root)).after(cx, binding, written)
    }
}

struct Uses<'tcx> {
    tcx: TyCtxt<'tcx>,
    ends: FxHashMap<HirId, usize>,
    locals: FxHashMap<HirId, Vec<(usize, &'tcx Expr<'tcx>)>>,
    mutations: FxHashMap<HirId, HirIdSet>,
    analyzed: FxHashSet<LocalDefId>,
    position: usize,
}

impl<'tcx> Uses<'tcx> {
    fn new(tcx: TyCtxt<'tcx>, owner: LocalDefId) -> Self {
        let mut uses = Self { tcx, ends: FxHashMap::default(), locals: FxHashMap::default(), mutations: FxHashMap::default(), analyzed: FxHashSet::default(), position: 0 };
        uses.visit_expr(tcx.hir_body_owned_by(owner).value);
        uses
    }

    fn after(&mut self, cx: &LateContext<'tcx>, binding: HirId, written: HirId) -> impl DoubleEndedIterator<Item = &'tcx Expr<'tcx>> {
        let owner = cx.tcx.hir_enclosing_body_owner(binding);
        if self.analyzed.insert(owner) {
            ExprUseVisitor::for_clippy(cx, owner, &mut *self).walk_expr(cx.tcx.hir_body_owned_by(owner).value).into_ok();
        }
        let mutations = self.mutations.get(&binding);
        let repeated = mutations.is_some()
            && cx
                .tcx
                .hir_parent_iter(written)
                .any(|(scope, node)| matches!(node, Node::Expr(Expr { kind: ExprKind::Loop(..), .. })) && !cx.tcx.hir_parent_id_iter(binding).any(|parent| parent == scope));
        let ends = &self.ends;
        ends.get(&written).filter(move |_| !repeated && mutations.is_none_or(|sites| sites.iter().all(|site| site.owner == written.owner))).zip(self.locals.get(&binding)).into_iter().flat_map(
            move |(&end, locals)| {
                let next_mutation = mutations.into_iter().flatten().map(|site| ends[site]).filter(|&position| position > end).min();
                let first = locals.partition_point(|&(position, _)| position < end);
                let last = next_mutation.map_or(locals.len(), |mutation| locals.partition_point(|&(position, _)| position < mutation));
                locals[first..last].iter().map(|&(_, used)| used)
            },
        )
    }
}

impl<'tcx> Delegate<'tcx> for Uses<'tcx> {
    fn consume(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn use_cloned(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn borrow(&mut self, place_with_id: &PlaceWithHirId<'tcx>, diag_expr_id: HirId, bk: ty::BorrowKind) {
        if bk == ty::BorrowKind::Mutable {
            self.mutate(place_with_id, diag_expr_id);
        }
    }

    fn mutate(&mut self, assignee_place: &PlaceWithHirId<'tcx>, diag_expr_id: HirId) {
        match assignee_place.place.base {
            PlaceBase::Local(binding) => {
                self.mutations.entry(binding).or_default().insert(diag_expr_id);
            }
            PlaceBase::Upvar(upvar) => {
                self.mutations.entry(upvar.var_path.hir_id).or_default().insert(diag_expr_id);
            }
            PlaceBase::Rvalue | PlaceBase::StaticItem => {}
        }
    }

    fn bind(&mut self, _: &PlaceWithHirId<'tcx>, _: HirId) {}

    fn fake_read(&mut self, _: &PlaceWithHirId<'tcx>, _: FakeReadCause, _: HirId) {}
}

impl<'tcx> Visitor<'tcx> for Uses<'tcx> {
    type NestedFilter = nested_filter::OnlyBodies;

    fn maybe_tcx(&mut self) -> Self::MaybeTyCtxt { self.tcx }

    fn visit_expr(&mut self, ex: &'tcx Expr<'tcx>) {
        if let Some(local) = ex.res_local_id() {
            self.locals.entry(local).or_default().push((self.position, ex));
            self.position = self.position.strict_add(1);
        }
        intravisit::walk_expr(self, ex);
        self.ends.insert(ex.hir_id, self.position);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn sink<'tcx>(cx: &LateContext<'tcx>, pass: &mut Pass<'tcx>, text: &'tcx Expr<'tcx>, source: Option<Ty<'tcx>>) -> Option<Sink> {
    fn asserted(cx: &LateContext<'_>, pass: &Pass<'_>, comparison: &Expr<'_>) -> bool {
        if !pass.test_target && !in_test(cx.tcx, comparison.hir_id) {
            return false;
        }
        if first_node_macro_backtrace(cx, comparison).any(|call| pass.assertions.contains(&call.def_id)) {
            return true;
        }
        if matches!(comparison.kind, ExprKind::Let(_))
            || matches!(comparison.kind, ExprKind::Match(..)) && !first_node_macro_backtrace(cx, comparison).any(|call| call.def_id.is_diag_item(cx, sym::matches_macro))
        {
            return false;
        }
        let mut child = comparison.hir_id;
        for (parent, node) in cx.tcx.hir_parent_iter(child) {
            if let Node::Expr(expr) = node
                && first_node_macro_backtrace(cx, expr).any(|call| pass.assertions.contains(&call.def_id))
            {
                return true;
            }
            let relayed = match node {
                Node::Expr(Expr { kind: ExprKind::Binary(operator, ..), .. }) => matches!(operator.node, BinOpKind::And | BinOpKind::Or),
                Node::Expr(Expr { kind: ExprKind::Unary(UnOp::Not, _) | ExprKind::DropTemps(_) | ExprKind::Block(..), .. }) => true,
                Node::Expr(expr) if matches!(expr.kind, ExprKind::Match(..)) => first_node_macro_backtrace(cx, expr).any(|call| call.def_id.is_diag_item(cx, sym::matches_macro)),
                Node::Expr(expr) if matches!(expr.kind, ExprKind::AddrOf(..) | ExprKind::Tup(_)) => macro_backtrace(expr.span).any(|call| pass.assertions.contains(&call.def_id)),
                Node::Block(block) => block.expr.is_some_and(|tail| tail.hir_id == child),
                Node::Arm(arm) => arm.guard.is_some_and(|guard| guard.hir_id == child),
                _ => false,
            };
            if !relayed {
                return false;
            }
            child = parent;
        }
        false
    }

    fn consumed<'tcx>(
        cx: &LateContext<'tcx>,
        pass: &Pass<'tcx>,
        call: &'tcx Expr<'tcx>,
        position: usize,
        rendering: Rendering,
        source: Option<Ty<'tcx>>,
    ) -> Option<ControlFlow<Sink, (&'tcx Expr<'tcx>, Rendering)>> {
        let (callee, arguments) = fn_def_id_with_node_args(cx, call)?;
        let typeck = cx.typeck_results();
        let produced = typeck.expr_ty(call);
        let constructor = callee.ctor_parent(cx);
        if callee.is_lang_item(cx, LangItem::TryTraitBranch) {
            get_parent_expr_for_hir(cx, call.hir_id)
                .filter(|parent| matches!(parent.kind, ExprKind::Match(_, _, MatchSource::TryDesugar(branch)) if branch == call.hir_id))
                .map(|parent| ControlFlow::Continue((parent, rendering)))
        } else if constructor.is_lang_item(cx, LangItem::ResultErr) && result_parts(cx, produced).is_some_and(|(_, error)| untyped_kind(cx, error).is_some()) {
            None
        } else if pass.comparisons.contains(&callee) || associated_trait(cx.tcx, callee).opt_diag_name(cx).is_some_and(|owner| matches!(owner, sym::PartialEq | sym::PartialOrd | sym::Ord)) {
            Some(ControlFlow::Break(Sink::Compared))
        } else if position == 0
            && callee.is_diag_item(cx, sym::iterator_collect_fn)
            && matches!(produced.kind(), ty::Adt(collection, args) if collection.is_diag_item(cx, sym::Vec) && is_text(cx, args.type_at(0)))
        {
            let collected = match rendering {
                Rendering::Produced(_) => rendering,
                Rendering::Deferred(_) => Rendering::Produced(cx.tcx.hir_enclosing_body_owner(call.hir_id)),
            };
            Some(ControlFlow::Continue((call, collected)))
        } else if position == 1 && pass.applied.contains(&callee) {
            let applied = match rendering {
                Rendering::Deferred(_) => Rendering::Produced(cx.tcx.hir_enclosing_body_owner(call.hir_id)),
                Rendering::Produced(_) => rendering,
            };
            Some(ControlFlow::Continue((call, applied)))
        } else if pass.transfers.get(&callee).is_some_and(|&last| position <= last)
            || position == 0 && text_source(cx, typeck, call).is_some()
            || position == 1 && callee.is_diag_item(cx, sym::IteratorMap)
            || constructor.is_lang_item(cx, LangItem::OptionSome)
            || constructor.is_lang_item(cx, LangItem::ResultOk)
        {
            Some(ControlFlow::Continue((call, rendering)))
        } else if produced.opt_def_id().is_some_and(|owner| Some(owner) == pass.io_error)
            && impl_owner(cx.tcx, callee).is_some_and(|owner| owner == produced)
            && source.is_some_and(|source| {
                if source.has_erased_regions() || source.has_escaping_bound_vars() {
                    return false;
                }
                let declaration = signature(cx.tcx, callee);
                let Some(&input) = declaration.inputs().get(position).filter(|input| matches!(input.kind(), ty::Param(_))) else {
                    return false;
                };
                let tcx = cx.tcx;
                let substituted = tcx.mk_args_from_iter(
                    arguments.iter().zip(ty::GenericArgs::identity_for_item(tcx, callee)).map(|(actual, parameter)| if parameter.as_type() == Some(input) { source.into() } else { actual }),
                );
                let owner = tcx.typeck_root_def_id_local(tcx.hir_enclosing_body_owner(call.hir_id));
                let param_env = tcx.param_env(owner);
                let known = tcx.assumed_wf_types(owner).iter().map(|&(held, _)| held).collect();
                test_region_obligations(tcx, owner, param_env, &known, |infcx| {
                    let obligations = ObligationCtxt::new(infcx);
                    let cause = ObligationCause::dummy();
                    obligations.register_obligations(
                        tcx.clauses_of(callee)
                            .instantiate(tcx, substituted)
                            .clauses
                            .into_iter()
                            .map(|clause| Obligation::new(tcx, cause.clone(), param_env, obligations.normalize(&cause, param_env, clause))),
                    );
                    obligations.evaluate_obligations_error_on_ambiguity().no_errors().ok_or(())
                })
            })
        {
            Some(ControlFlow::Break(Sink::Source))
        } else if position == 1 && pass.push.contains(&callee) || constructor.is_some() && !handed_off(cx, produced) {
            Some(ControlFlow::Break(Sink::Stored))
        } else {
            let local = callee.as_local()?;
            let body = cx.tcx.hir_maybe_body_owned_by(local)?;
            let constructor_typeck = cx.tcx.typeck(local);
            let Some((variant, mut fields)) = constructed(constructor_typeck, peel_blocks(body.value)) else {
                return signature(cx.tcx, callee).inputs().get(position).filter(|&&input| is_text(cx, input)).map(|_| ControlFlow::Break(Sink::Passed));
            };
            if variant.def_id.is_lang_item(cx, LangItem::ResultErr) && result_parts(cx, produced).is_some_and(|(_, error)| untyped_kind(cx, error).is_some()) || handed_off(cx, produced) {
                return None;
            }
            fields
                .any(|(_, value)| {
                    parameter(body, peel_blocks(peel_hir_expr_while(value, |part| text_source(cx, constructor_typeck, peel_blocks(part.peel_borrows()))).peel_borrows())) == Some(position)
                })
                .then_some(if matches!(cx.tcx.lang_items().from_def_id(variant.def_id), Some(LangItem::OptionSome | LangItem::ResultOk)) {
                    ControlFlow::Continue((call, rendering))
                } else {
                    ControlFlow::Break(Sink::Stored)
                })
        }
    }

    fn compares(cx: &LateContext<'_>, parent: &Expr<'_>, child: HirId) -> bool {
        let literal = |pattern: &Pat<'_>| !pattern.walk_short(|part| !(matches!(part.kind, PatKind::Expr(_)) && is_text(cx, cx.typeck_results().pat_ty(part))));
        match parent.kind {
            ExprKind::Binary(operator, ..) => operator.node.is_comparison(),
            ExprKind::Match(scrutinee, arms, _) => scrutinee.hir_id == child && arms.iter().any(|arm| literal(arm.pat)),
            ExprKind::Let(binding) => binding.init.hir_id == child && literal(binding.pat),
            _ => false,
        }
    }

    let mut pending = Vec::new();
    let mut visited = FxHashSet::default();
    let mut current = Some((text, fn_item_value(cx, text).map_or_else(|| Rendering::Produced(cx.tcx.hir_enclosing_body_owner(text.hir_id)), Rendering::Deferred)));
    while let Some((rendered, rendering)) = current.take().or_else(|| pending.pop()) {
        if visited.contains(&(rendered.hir_id, rendering)) {
            continue;
        }
        let site = get_expr_use_site(cx.tcx, cx.typeck_results(), rendered.span.ctxt(), rendered);
        let found = match (site.node, site.use_node(cx)) {
            (_, ExprUseNode::Field(field)) => get_parent_expr_for_hir(cx, field.hir_id).filter(|parent| !handed_off(cx, cx.typeck_results().expr_ty(parent))).map(|_| ControlFlow::Break(Sink::Stored)),
            (_, ExprUseNode::Return(owner)) if let Node::Expr(closure) = cx.tcx.hir_node_by_def_id(owner.def_id) => {
                let returned = match rendering {
                    Rendering::Produced(body) if body == owner.def_id => Rendering::Deferred(body.to_def_id()),
                    Rendering::Produced(_) | Rendering::Deferred(_) => rendering,
                };
                Some(ControlFlow::Continue((closure, returned)))
            }
            (_, ExprUseNode::Return(_)) => Some(ControlFlow::Break(Sink::Returned)),
            (_, ExprUseNode::LetStmt(local))
                if let PatKind::Binding(_, binding, _, None) = local.pat.kind
                    && let Some(init) = local.init =>
            {
                pending.extend(pass.after(cx, binding, init.hir_id).rev().map(|used| (used, rendering)));
                None
            }
            (Node::Expr(parent), _)
                if let ExprKind::Assign(target, value, _) | ExprKind::AssignOp(_, target, value) = parent.kind
                    && value.hir_id == site.child_id =>
                match target.res_local_id() {
                    Some(binding) => {
                        pending.extend(pass.after(cx, binding, parent.hir_id).rev().map(|used| (used, rendering)));
                        None
                    }
                    None => Some(ControlFlow::Break(Sink::Stored)),
                },
            (Node::Expr(parent), _) if compares(cx, parent, site.child_id) => Some(ControlFlow::Break(Sink::Compared)),
            (Node::Expr(parent), _) if matches!(parent.kind, ExprKind::Array(_) | ExprKind::Tup(_)) => Some(ControlFlow::Break(Sink::Stored)),
            (Node::Expr(parent), ExprUseNode::Callee) => {
                let called = match (rendering, cx.typeck_results().expr_ty(rendered).peel_refs().kind()) {
                    (Rendering::Deferred(body), ty::Closure(callee, _) | ty::FnDef(callee, _)) if body == *callee => Rendering::Produced(cx.tcx.hir_enclosing_body_owner(parent.hir_id)),
                    (Rendering::Produced(_) | Rendering::Deferred(_), _) => rendering,
                };
                Some(ControlFlow::Continue((parent, called)))
            }
            (Node::Expr(parent), ExprUseNode::AddrOf(..)) => Some(ControlFlow::Continue((parent, rendering))),
            (Node::Expr(parent), ExprUseNode::FnArg(_, position) | ExprUseNode::MethodArg(_, _, position)) => consumed(cx, pass, parent, position, rendering, source),
            _ => None,
        };
        match found {
            Some(ControlFlow::Break(sink)) =>
                if matches!(rendering, Rendering::Produced(_))
                    && (source.is_some() || matches!(sink, Sink::Compared))
                    && !(matches!(sink, Sink::Compared) && matches!(site.node, Node::Expr(comparison) if asserted(cx, pass, comparison)))
                {
                    return Some(sink);
                },
            Some(ControlFlow::Continue(next)) => current = Some(next),
            None => {}
        }
        if current.is_some() || !pending.is_empty() {
            visited.insert((rendered.hir_id, rendering));
        }
    }
    None
}

fn handed_off<'tcx>(cx: &LateContext<'tcx>, produced: Ty<'tcx>) -> bool { produced.ty_adt_def().is_some_and(|adt| !adt.did().is_local()) && is_error(cx, produced) }

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let rendered =
            match (expr.kind, expanded_externally(cx, expr.span)) {
                (ExprKind::MethodCall(_, operand, [], _) | ExprKind::Call(_, [operand]), false) if fn_def_id(cx, expr).is_diag_item(cx, sym::to_string_method) =>
                    Some((expr.span, typeck.expr_ty(operand))),
                (ExprKind::Path(_), false) if fn_item_value(cx, expr).is_diag_item(cx, sym::to_string_method) =>
                    typeck.expr_ty(expr).fn_sig(cx.tcx).skip_binder().inputs().first().map(|&input| (expr.span, input)),
                _ if let Some(call) =
                    first_node_macro_backtrace(cx, expr).find(|call| cx.tcx.is_diagnostic_item(sym::format_macro, call.def_id)).filter(|call| !expanded_externally(cx, call.span)) =>
                    self.format_args
                        .get(cx, expr, call.expn)
                        .and_then(|arguments| {
                            arguments
                                .template
                                .iter()
                                .filter_map(|piece| {
                                    let FormatArgsPiece::Placeholder(placeholder) = piece else { return None };
                                    let argument = arguments.arguments.all_args().get(placeholder.argument.index.ok()?)?;
                                    find_format_arg_expr(expr, argument).map(|value| typeck.expr_ty(value))
                                })
                                .fold_while(None, |first, operand| {
                                    if is_error(cx, operand) { FoldWhile::Done(Some(operand)) } else { FoldWhile::Continue(first.or_else(|| (!is_text(cx, operand)).then_some(operand))) }
                                })
                                .into_inner()
                        })
                        .map(|operand| (call.span, operand)),
                _ => None,
            };
        if let Some((span, operand)) = rendered.filter(|&(_, operand)| !is_text(cx, operand))
            && let error = is_error(cx, operand)
            && let Some(sink) = sink(cx, self, expr, error.then_some(operand))
        {
            let shown = pointee(cx, operand);
            let help = match sink {
                Sink::Compared if error => Cow::Borrowed("match the error's variant, `kind()`, or `downcast_ref` in place of its text"),
                Sink::Compared => Cow::Owned(format!("compare or match `{shown}` in place of its text")),
                Sink::Returned | Sink::Stored => Cow::Owned(format!("hold `{shown}` as `#[source]` of a `thiserror` variant")),
                Sink::Passed => Cow::Owned(format!("pass `{shown}` itself and render it where a person reads it")),
                Sink::Source => Cow::Owned(format!("preserve `{shown}` as the constructor's typed source")),
            };
            let effect = match sink {
                Sink::Compared => "compared",
                Sink::Passed => "passed as text",
                Sink::Returned => "returned",
                Sink::Stored | Sink::Source => "stored",
            };
            let message = if error { format!("text of error `{shown}` is {effect}") } else { format!("text of `{shown}` is {effect}") };
            span_lint_hir_and_then(cx, STRINGIFIED_ERROR, expr.hir_id, span, message, |diag| {
                diag.help(help);
                record_rendering(cx, &self.reported, expr.hir_id);
            });
        }
    }

    fn check_body_post(&mut self, cx: &LateContext<'tcx>, body: &Body<'tcx>) { self.uses.remove(&cx.tcx.hir_body_owner_def_id(body.id())); }
}
