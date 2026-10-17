use core::iter;

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    macros::root_macro_call,
    peel_hir_expr_while,
    res::{MaybeDef as _, MaybeQPath as _},
    sym,
};
use itertools::Itertools as _;
use rustc_hir::{
    BinOpKind, Expr, ExprKind, HirId, LetStmt, MatchSource, Pat, PatExpr, PatExprKind, PatKind, UnOp,
    def::{CtorKind, CtorOf, DefKind, Res},
    def_id::DefId,
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, Ty, TypeckResults};
use rustc_span::{Span, Symbol};

use crate::{expanded_externally, workspace_def};

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

lint!(WILDCARD_WORKSPACE_ENUM_MATCH_ARM: "`match` catch-all arms, `if .. else` variant tests, and `matches!` and `let .. else` patterns that send each workspace enum variant they leave unnamed to one branch");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let matches_call = |tested: &Expr<'_>| root_macro_call(tested.span).filter(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id));
        match expr.kind {
            ExprKind::Match(_, [arm, ..], MatchSource::Normal)
                if let Some(call) = matches_call(expr)
                    && !expanded_externally(cx, call.span)
                    && let Some((name, 2..)) = named(cx, typeck, arm.pat) =>
                report(cx, expr.hir_id, call.span, format!("`matches!` sends each `{name}` variant it leaves unnamed to `false`")),
            ExprKind::Match(_, arms, MatchSource::Normal) if matches_call(expr).is_none() && !expanded_externally(cx, expr.span) => {
                let Shape { named, caught } = shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_none()).map(|arm| arm.pat));
                let named = iter::chain(named, shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_some()).map(|arm| arm.pat)).named).collect::<Vec<_>>();
                for (span, name) in caught.into_iter().filter_map(|(position, span)| Some((span, named.iter().find(|(covered, _)| covered.starts_with(&position))?.1))) {
                    report(cx, expr.hir_id, span, format!("catch-all pattern covers each `{name}` variant the other arms leave unnamed"));
                }
            }
            ExprKind::If(condition, _, Some(_)) if !expanded_externally(cx, expr.span) => {
                let condition = peel_hir_expr_while(condition, |inner| if let ExprKind::DropTemps(inner) | ExprKind::Unary(UnOp::Not, inner) = inner.kind { Some(inner) } else { None });
                let name = match condition.kind {
                    ExprKind::Let(tested) => named(cx, typeck, tested.pat).map(|(name, _)| name),
                    ExprKind::Match(_, [arm, ..], MatchSource::Normal)
                        if matches_call(condition).is_some()
                            && let Some((name, 1)) = named(cx, typeck, arm.pat) =>
                        Some(name),
                    ExprKind::Binary(operator, left, right) if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) => [(left, right), (right, left)].into_iter().find_map(|(compared, variant)| {
                        let adt = workspace_enum(cx, typeck.expr_ty(compared))?;
                        matches!(variant.res(typeck), Res::Def(DefKind::Ctor(CtorOf::Variant, CtorKind::Const), ctor) if cx.tcx.parent(cx.tcx.parent(ctor)) == adt.did())
                            .then(|| cx.tcx.item_name(adt.did()))
                    }),
                    _ => None,
                };
                if let Some(name) = name {
                    report(cx, expr.hir_id, condition.span.source_callsite(), format!("`if` over `{name}` sends each variant its condition leaves unnamed to `else`"));
                }
            }
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
