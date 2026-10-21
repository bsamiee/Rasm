use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    fn_def_id, is_refutable, is_wild,
    macros::{MacroCall, root_macro_call},
    paths::PathNS,
    peel_blocks, peel_hir_expr_refs, peel_hir_expr_while, peel_hir_pat_refs, recurse_or_patterns,
    res::{MaybeDef as _, MaybeQPath as _},
    sym,
};
use itertools::Itertools as _;
use rustc_ast::LitKind;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{
    Arm, BinOpKind, Expr, ExprKind, HirId, LetStmt, MatchSource, Pat, PatExpr, PatExprKind, PatKind, UnOp,
    def::{CtorKind, CtorOf, DefKind, Res},
    def_id::DefId,
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, Ty, TyCtxt, TypeckResults};
use rustc_span::{Span, Spanned, Symbol};

use crate::{expanded_externally, paths, workspace_def};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
struct Step {
    variant: Option<DefId>,
    field: usize,
}

#[derive(Default)]
struct Shape {
    named: Vec<(Vec<Step>, Symbol)>,
    caught: Vec<(Vec<Step>, Span)>,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    contains: FxHashSet<DefId>,
}

lint!(Pass; WILDCARD_WORKSPACE_ENUM_MATCH_ARM: "`match` catch-all arms, `if .. else` variant tests, and `matches!` and `let .. else` patterns that send each workspace enum variant they leave unnamed to one branch");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { contains: paths(tcx, PathNS::Value, &["slice::contains", "alloc::collections::VecDeque::contains"]) } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn reported_by_clippy<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, scrutinee: &Expr<'_>, arms: &'tcx [Arm<'tcx>]) -> Vec<Span> {
            let catch_all = |arm: &Arm<'_>| matches!(peel_hir_pat_refs(arm.pat).0.kind, PatKind::Binding(.., None)) || arm.guard.is_none() && is_wild(peel_hir_pat_refs(arm.pat).0);
            let mixed = arms.iter().filter(|arm| matches!(arm.pat.kind, PatKind::Or(alternatives) if alternatives.len() > 1 && alternatives.iter().any(is_wild)));
            let mut alternatives = Vec::new();
            arms.iter().filter(|arm| arm.guard.is_none()).for_each(|arm| recurse_or_patterns(arm.pat, |pat| alternatives.push(pat)));
            let covered = alternatives
                .into_iter()
                .filter_map(|pat| match peel_hir_pat_refs(pat).0.kind {
                    PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) => typeck.qpath_res(&qpath, hir_id).opt_def_id(),
                    PatKind::TupleStruct(qpath, fields, _) if fields.iter().all(|field| !is_refutable(cx, field)) => typeck.qpath_res(&qpath, pat.hir_id).opt_def_id(),
                    PatKind::Struct(qpath, fields, _) if fields.iter().all(|field| !is_refutable(cx, field.pat)) => typeck.qpath_res(&qpath, pat.hir_id).opt_def_id(),
                    _ => None,
                })
                .collect::<FxHashSet<_>>();
            let single = workspace_enum(cx, typeck.expr_ty(scrutinee))
                .zip(arms.iter().rfind(|arm| catch_all(arm)))
                .filter(|&(adt, _)| {
                    !arms.iter().all(catch_all)
                        && adt.variants().iter().filter(|variant| !covered.contains(&variant.def_id) && variant.ctor_def_id().is_none_or(|ctor| !covered.contains(&ctor))).count() == 1
                })
                .map(|(_, arm)| arm);
            mixed.chain(single).map(|arm| arm.pat.span).collect()
        }

        let typeck = cx.typeck_results();
        let literal = |body: &Expr<'_>| if let ExprKind::Lit(Spanned { node: LitKind::Bool(value), .. }) = peel_blocks(body).kind { Some(value) } else { None };
        let like_matches = |arms: &[Arm<'_>]| {
            typeck.expr_ty(expr).is_bool()
                && matches!(arms, [first, middle @ .., last] if is_wild(last.pat) && literal(last.body).is_some_and(|tail| {
                    literal(first.body) == Some(!tail) && (middle.is_empty() || iter::once(first).chain(middle).all(|arm| arm.guard.is_none() && literal(arm.body) == Some(!tail)))
                }))
        };
        match expr.kind {
            ExprKind::Match(_, [arm, ..], MatchSource::Normal)
                if let Some(call) = matches_call(cx, expr)
                    && !expanded_externally(cx, call.span)
                    && let Some((name, 2..)) = named(cx, typeck, arm.pat) =>
                report(cx, expr.hir_id, call.span, format!("`matches!` sends each `{name}` variant it leaves unnamed to `false`")),
            ExprKind::Match(scrutinee, arms, MatchSource::Normal) if matches_call(cx, expr).is_none() && !expanded_externally(cx, expr.span) && !like_matches(arms) => {
                let Shape { named, caught } = shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_none()).map(|arm| arm.pat));
                let named = iter::chain(named, shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_some()).map(|arm| arm.pat)).named).collect::<Vec<_>>();
                let reported = reported_by_clippy(cx, typeck, scrutinee, arms);
                let caught = caught.into_iter().filter(|(position, span)| !(position.is_empty() && reported.iter().any(|arm| arm.contains(*span))));
                for (span, name) in caught.filter_map(|(position, span)| Some((span, named.iter().find(|(covered, _)| covered.starts_with(&position))?.1))) {
                    report(cx, expr.hir_id, span, format!("catch-all pattern covers each `{name}` variant the other arms leave unnamed"));
                }
            }
            ExprKind::If(condition, then, Some(otherwise))
                if !expanded_externally(cx, expr.span)
                    && !(matches!(condition.peel_drop_temps().kind, ExprKind::Let(_))
                        && typeck.expr_ty(expr).is_bool()
                        && literal(then).zip(literal(otherwise)).is_some_and(|(kept, other)| kept != other))
                    && let Some((name, span)) = tested(cx, typeck, &self.contains, condition) =>
                report(cx, expr.hir_id, span, format!("`if` over `{name}` sends each variant its condition leaves unnamed to `else`")),
            _ => {}
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if local.els.is_some()
            && !expanded_externally(cx, local.span)
            && let Some((name, 2..)) = named(cx, cx.typeck_results(), local.pat)
        {
            report(cx, local.hir_id, local.pat.span, format!("`let .. else` sends each `{name}` variant its pattern leaves unnamed to `else`"));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn workspace_enum<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> Option<AdtDef<'tcx>> {
    match held.peel_refs().kind() {
        ty::Adt(adt, _) if adt.is_enum() && !matches!(adt.opt_diag_name(cx), Some(sym::Option | sym::Result)) && workspace_def(cx, adt.did()) => Some(*adt),
        _ => None,
    }
}

fn matches_call(cx: &LateContext<'_>, tested: &Expr<'_>) -> Option<MacroCall> { root_macro_call(tested.span).filter(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id)) }

fn tested<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, contains: &FxHashSet<DefId>, condition: &'tcx Expr<'tcx>) -> Option<(Symbol, Span)> {
    let condition = peel_hir_expr_while(condition, |inner| if let ExprKind::DropTemps(inner) | ExprKind::Unary(UnOp::Not, inner) = inner.kind { Some(inner) } else { None });
    let variant = |written: &Expr<'_>| {
        let adt = workspace_enum(cx, typeck.expr_ty(written))?;
        matches!(written.res(typeck), Res::Def(DefKind::Ctor(CtorOf::Variant, CtorKind::Const), ctor) if cx.tcx.parent(cx.tcx.parent(ctor)) == adt.did()).then(|| cx.tcx.item_name(adt.did()))
    };
    let found = |name: Option<Symbol>| name.map(|name| (name, condition.span.source_callsite()));
    match condition.kind {
        ExprKind::Binary(operator, left, right) if matches!(operator.node, BinOpKind::And | BinOpKind::Or) => tested(cx, typeck, contains, left).or_else(|| tested(cx, typeck, contains, right)),
        ExprKind::Binary(operator, left, right) if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) => found(variant(right).or_else(|| variant(left))),
        ExprKind::Let(tested) => found(named(cx, typeck, tested.pat).map(|(name, _)| name)),
        ExprKind::Match(_, [arm, ..], MatchSource::Normal)
            if matches_call(cx, condition).is_some()
                && let Some((name, 1)) = named(cx, typeck, arm.pat) =>
            found(Some(name)),
        ExprKind::MethodCall(_, _, [argument], _) if fn_def_id(cx, condition).is_some_and(|called| contains.contains(&called)) => found(variant(peel_hir_expr_refs(argument).0)),
        _ => None,
    }
}

fn shape<'tcx, P>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, patterns: P) -> Shape
where P: IntoIterator<Item = &'tcx Pat<'tcx>> {
    fn walk<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, pat: &'tcx Pat<'tcx>, position: &[Step], shape: &mut Shape) {
        let held = typeck.pat_ty(pat);
        let res = match pat.kind {
            PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) => Some(typeck.qpath_res(&qpath, hir_id)),
            PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) => Some(typeck.qpath_res(&qpath, pat.hir_id)),
            _ => None,
        };
        let declared = res.and_then(|written| if let Res::Def(DefKind::Ctor(CtorOf::Variant, _), ctor) = written { Some(cx.tcx.parent(ctor)) } else { written.opt_def_id() });
        if let Some(adt) = workspace_enum(cx, held).filter(|_| declared.is_some()) {
            shape.named.push((position.to_vec(), cx.tcx.item_name(adt.did())));
        }
        match pat.kind {
            PatKind::Wild | PatKind::Binding(.., None) => shape.caught.push((position.to_vec(), pat.span)),
            PatKind::Binding(.., Some(inner)) | PatKind::Ref(inner, ..) | PatKind::Deref(inner) => walk(cx, typeck, inner, position, shape),
            PatKind::Or(alternatives) => alternatives.iter().for_each(|alternative| walk(cx, typeck, alternative, position, shape)),
            PatKind::TupleStruct(_, fields, rest) => {
                let arity = match (held.peel_refs().kind(), res) {
                    (ty::Adt(adt, _), Some(written)) => adt.variant_of_res(written).fields.len(),
                    _ => fields.len(),
                };
                for (index, field) in fields.iter().enumerate_and_adjust(arity, rest) {
                    walk(cx, typeck, field, &[position, &[Step { variant: declared, field: index }]].concat(), shape);
                }
            }
            PatKind::Struct(_, fields, _) =>
                for field in fields {
                    walk(cx, typeck, field.pat, &[position, &[Step { variant: declared, field: typeck.field_index(field.hir_id).as_usize() }]].concat(), shape);
                },
            PatKind::Tuple(fields, rest) => {
                let arity = if let ty::Tuple(parts) = held.peel_refs().kind() { parts.len() } else { fields.len() };
                for (index, field) in fields.iter().enumerate_and_adjust(arity, rest) {
                    walk(cx, typeck, field, &[position, &[Step { variant: None, field: index }]].concat(), shape);
                }
            }
            PatKind::Missing | PatKind::Never | PatKind::Expr(_) | PatKind::Guard(..) | PatKind::Range(..) | PatKind::Slice(..) | PatKind::Err(_) => {}
        }
    }

    patterns.into_iter().fold(Shape::default(), |mut shape, pat| {
        walk(cx, typeck, pat, &[], &mut shape);
        shape
    })
}

fn named<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, pat: &'tcx Pat<'tcx>) -> Option<(Symbol, usize)> {
    let named = shape(cx, typeck, [pat]).named;
    Some((named.first()?.1, named.iter().counts_by(|(position, _)| position).into_values().max()?))
}

fn report(cx: &LateContext<'_>, node: HirId, span: Span, message: String) {
    span_lint_hir_and_then(cx, WILDCARD_WORKSPACE_ENUM_MATCH_ARM, node, span, message, |diag| {
        diag.help("name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`");
    });
}
