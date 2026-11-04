use core::{convert::identity, iter, ops::ControlFlow};

use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    expr_or_init, fn_def_id, is_refutable, is_wild,
    macros::{MacroCall, root_macro_call},
    paths::PathNS,
    peel_blocks, peel_hir_expr_refs, peel_hir_pat_refs, recurse_or_patterns,
    res::MaybeDef as _,
    sym,
    visitors::{Descend, for_each_expr_without_closures},
};
use itertools::{Either, Itertools as _};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashSet, FxIndexMap};
use rustc_hir::{
    Arm, BinOpKind, Expr, ExprKind, HirId, LetStmt, MatchSource, Pat, PatExpr, PatExprKind, PatKind, UnOp,
    def::{CtorOf, DefKind, Res},
    def_id::DefId,
    pat_util::EnumerateAndAdjustIterator as _,
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, Ty, TyCtxt, TypeckResults, VariantDef};
use rustc_span::{Span, Spanned, Symbol};

use crate::{constructed, expanded_externally, paths, workspace_def};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
struct Step {
    variant: Option<DefId>,
    field: usize,
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    contains: FxHashSet<DefId>,
}

lint!(Pass; WILDCARD_WORKSPACE_ENUM_MATCH_ARM: "`match` catch-all arms, `if .. else` variant tests, and `matches!` and `let .. else` patterns that send each workspace enum variant they leave unnamed to one branch");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self {
        Self { contains: paths(tcx, PathNS::Value, &["slice::contains", "alloc::collections::VecDeque::contains", "std::collections::HashSet::contains", "alloc::collections::BTreeSet::contains"]) }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn workspace_enum<'tcx>(cx: &LateContext<'tcx>, held: Ty<'tcx>) -> Option<AdtDef<'tcx>> {
    match held.peel_refs().kind() {
        ty::Adt(adt, _) if adt.is_enum() && !matches!(adt.opt_diag_name(cx), Some(sym::Option | sym::Result)) && workspace_def(cx, adt.did()) => Some(*adt),
        _ => None,
    }
}

fn covered<'tcx, P>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, patterns: P) -> FxHashSet<DefId>
where P: IntoIterator<Item = &'tcx Pat<'tcx>> {
    patterns.into_iter().fold(FxHashSet::default(), |mut covered, pat| {
        recurse_or_patterns(pat, |pat| {
            let declared = match peel_hir_pat_refs(pat).0.kind {
                PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) => typeck.qpath_res(&qpath, hir_id).opt_def_id(),
                PatKind::TupleStruct(qpath, fields, _) if fields.iter().all(|field| !is_refutable(cx, field)) => typeck.qpath_res(&qpath, pat.hir_id).opt_def_id(),
                PatKind::Struct(qpath, fields, _) if fields.iter().all(|field| !is_refutable(cx, field.pat)) => typeck.qpath_res(&qpath, pat.hir_id).opt_def_id(),
                _ => None,
            };
            covered.extend(declared);
        });
        covered
    })
}

fn unnamed<'tcx>(adt: AdtDef<'tcx>, covered: &FxHashSet<DefId>) -> impl Iterator<Item = &'tcx VariantDef> {
    adt.variants().iter().filter(|variant| !covered.contains(&variant.def_id) && variant.ctor_def_id().is_none_or(|ctor| !covered.contains(&ctor)))
}

fn matches_call(cx: &LateContext<'_>, tested: &Expr<'_>) -> Option<MacroCall> { root_macro_call(tested.span).filter(|call| cx.tcx.is_diagnostic_item(sym::matches_macro, call.def_id)) }

fn has_enum_constants(typeck: &TypeckResults<'_>, root: &Pat<'_>) -> bool {
    !root.walk_short(|pat| match pat.kind {
        PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) =>
            matches!(typeck.qpath_res(&qpath, hir_id), Res::Def(DefKind::Variant | DefKind::Ctor(CtorOf::Variant, _), _))
                || typeck.pat_ty(pat).peel_refs().ty_adt_def().is_none_or(|adt| !adt.is_enum()),
        _ => true,
    })
}

gen fn shape<'tcx, P>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, patterns: P) -> Either<(Vec<Step>, AdtDef<'tcx>), (Vec<Step>, Span)>
where P: IntoIterator<Item = &'tcx Pat<'tcx>> {
    for root in patterns {
        let mut pending = vec![(root, Vec::<Step>::new())];
        while let Some((pat, position)) = pending.pop() {
            let res = match pat.kind {
                PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) => Some(typeck.qpath_res(&qpath, hir_id)),
                PatKind::TupleStruct(qpath, ..) | PatKind::Struct(qpath, ..) => Some(typeck.qpath_res(&qpath, pat.hir_id)),
                _ => None,
            };
            let declared = res.and_then(|written| if let Res::Def(DefKind::Ctor(CtorOf::Variant, _), ctor) = written { Some(cx.tcx.parent(ctor)) } else { written.opt_def_id() });
            if matches!(res, Some(Res::Def(DefKind::Variant | DefKind::Ctor(CtorOf::Variant, _), _)))
                && let Some(adt) = workspace_enum(cx, typeck.pat_ty(pat))
            {
                yield Either::Left((position.clone(), adt));
            }
            match pat.kind {
                PatKind::Wild | PatKind::Binding(.., None) => yield Either::Right((position, pat.span)),
                PatKind::Binding(.., Some(inner)) | PatKind::Ref(inner, ..) | PatKind::Deref(inner) => pending.push((inner, position)),
                PatKind::Or(alternatives) => pending.extend(alternatives.iter().rev().map(|alternative| (alternative, position.clone()))),
                PatKind::TupleStruct(_, fields, rest) | PatKind::Tuple(fields, rest) => {
                    let arity = match (typeck.pat_ty(pat).peel_refs().kind(), res) {
                        (ty::Adt(adt, _), Some(written)) => adt.variant_of_res(written).fields.len(),
                        (ty::Tuple(parts), _) => parts.len(),
                        _ => fields.len(),
                    };
                    pending.extend(
                        fields
                            .iter()
                            .enumerate_and_adjust(arity, rest)
                            .map(|(index, field)| (field, iter::chain(position.iter().copied(), [Step { variant: declared, field: index }]).collect()))
                            .collect::<Vec<_>>()
                            .into_iter()
                            .rev(),
                    );
                }
                PatKind::Struct(_, fields, _) => pending.extend(
                    fields.iter().rev().map(|field| (field.pat, iter::chain(position.iter().copied(), [Step { variant: declared, field: typeck.field_index(field.hir_id).as_usize() }]).collect())),
                ),
                PatKind::Missing | PatKind::Never | PatKind::Expr(_) | PatKind::Guard(..) | PatKind::Range(..) | PatKind::Slice(..) | PatKind::Err(_) => {}
            }
        }
    }
}

fn named<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, pat: &'tcx Pat<'tcx>) -> Option<(Symbol, usize)> {
    if has_enum_constants(typeck, pat) {
        return None;
    }
    let counts = shape(cx, typeck, [pat]).filter_map(Either::left).try_fold(FxIndexMap::<_, usize>::default(), |mut counts, subject| {
        (!subject.1.variant_list_has_applicable_non_exhaustive()).then(|| {
            counts.entry(subject).and_modify(|count| *count = count.strict_add(1)).or_insert(1);
            counts
        })
    })?;
    let ((_, adt), count) = counts.into_iter().reduce(|earlier, later| if later.1 > earlier.1 { later } else { earlier })?;
    Some((cx.tcx.item_name(adt.did()), count))
}

fn report(cx: &LateContext<'_>, node: HirId, span: Span, message: String) {
    span_lint_hir_and_then(cx, WILDCARD_WORKSPACE_ENUM_MATCH_ARM, node, span, message, |diag| {
        diag.help("name each variant in a `match`, join variants that share a branch with an or-pattern, and keep a binding with `name @ (A | B)`");
    });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn reported_by_clippy<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, scrutinee: &Expr<'_>, arms: &'tcx [Arm<'tcx>]) -> Vec<Span> {
            let catch_all = |arm: &Arm<'_>| matches!(peel_hir_pat_refs(arm.pat).0.kind, PatKind::Binding(.., None)) || arm.guard.is_none() && is_wild(peel_hir_pat_refs(arm.pat).0);
            let mixed = arms.iter().filter(|arm| matches!(arm.pat.kind, PatKind::Or(alternatives) if alternatives.len() > 1 && alternatives.iter().any(is_wild)));
            let covered = covered(cx, typeck, arms.iter().filter(|arm| arm.guard.is_none()).map(|arm| arm.pat));
            let single = workspace_enum(cx, typeck.expr_ty(scrutinee))
                .zip(arms.iter().rfind(|arm| catch_all(arm)))
                .filter(|&(adt, _)| !arms.iter().all(catch_all) && unnamed(adt, &covered).count() == 1)
                .map(|(_, arm)| arm);
            mixed.chain(single).map(|arm| arm.pat.span).collect()
        }

        fn tested<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, contains: &FxHashSet<DefId>, condition: &'tcx Expr<'tcx>) -> Option<(Symbol, Span)> {
            let variant = |written: &'tcx Expr<'tcx>| {
                let adt = workspace_enum(cx, typeck.expr_ty(written)).filter(|adt| !adt.variant_list_has_applicable_non_exhaustive())?;
                constructed(typeck, written).map(|_| cx.tcx.item_name(adt.did()))
            };
            for_each_expr_without_closures(condition, |condition| {
                let found = match condition.kind {
                    ExprKind::DropTemps(_) | ExprKind::Unary(UnOp::Not, _) => return ControlFlow::Continue(Descend::Yes),
                    ExprKind::Binary(operator, ..) if matches!(operator.node, BinOpKind::And | BinOpKind::Or) => return ControlFlow::Continue(Descend::Yes),
                    ExprKind::Binary(operator, left, right) if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) => variant(right).or_else(|| variant(left)),
                    ExprKind::Let(bound) => named(cx, typeck, bound.pat).map(|(name, _)| name),
                    ExprKind::Match(_, [arm, ..], MatchSource::Normal)
                        if matches_call(cx, condition).is_some()
                            && let Some((name, 1)) = named(cx, typeck, arm.pat) =>
                        Some(name),
                    ExprKind::MethodCall(_, receiver, [argument], _) if fn_def_id(cx, condition).is_some_and(|called| contains.contains(&called)) =>
                        variant(peel_hir_expr_refs(argument).0).or_else(|| match expr_or_init(cx, peel_hir_expr_refs(receiver).0).kind {
                            ExprKind::Array(listed) => listed.iter().find_map(variant),
                            _ => None,
                        }),
                    _ => None,
                };
                found.map_or(ControlFlow::Continue(Descend::No), |name| ControlFlow::Break((name, condition.span.source_callsite())))
            })
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
                    && let Some((name, 2..)) = named(cx, typeck, arm.pat)
                    && workspace_enum(cx, typeck.pat_ty(arm.pat)).is_none_or(|adt| unnamed(adt, &covered(cx, typeck, [arm.pat])).next().is_some()) =>
                report(cx, expr.hir_id, call.span, format!("`matches!` sends each `{name}` variant it leaves unnamed to `false`")),
            ExprKind::Match(scrutinee, arms, MatchSource::Normal)
                if matches_call(cx, expr).is_none() && !expanded_externally(cx, expr.span) && !like_matches(arms) && arms.iter().all(|arm| !has_enum_constants(typeck, arm.pat)) =>
            {
                let (named, caught): (Vec<_>, Vec<_>) = shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_none()).map(|arm| arm.pat)).partition_map(identity);
                let named = iter::chain(named, shape(cx, typeck, arms.iter().filter(|arm| arm.guard.is_some()).map(|arm| arm.pat)).filter_map(Either::left)).collect::<Vec<_>>();
                let reported = reported_by_clippy(cx, typeck, scrutinee, arms);
                let caught = caught.into_iter().filter(|(position, span)| !(position.is_empty() && reported.iter().any(|arm| arm.contains(*span))));
                for (span, adt) in caught.filter_map(|(position, span)| {
                    let mut candidates = named.iter().filter(|(covered, _)| covered.starts_with(&position)).peekable();
                    let first = candidates.peek()?.1;
                    candidates.all(|(_, adt)| !adt.variant_list_has_applicable_non_exhaustive()).then_some((span, first))
                }) {
                    let name = cx.tcx.item_name(adt.did());
                    report(cx, expr.hir_id, span, format!("catch-all pattern covers each `{name}` variant the other arms leave unnamed"));
                }
            }
            ExprKind::If(condition, then, Some(otherwise))
                if !expanded_externally(cx, expr.span)
                    && !matches!((condition.peel_drop_temps().kind, literal(then).zip(literal(otherwise))), (ExprKind::Let(_), Some((kept, other))) if kept != other)
                    && let Some((name, span)) = tested(cx, typeck, &self.contains, condition) =>
                report(cx, expr.hir_id, span, format!("`if` over `{name}` sends each variant its condition leaves unnamed to one branch")),
            _ => {}
        }
    }

    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if local.els.is_some()
            && !expanded_externally(cx, local.span)
            && let typeck = cx.typeck_results()
            && let Some((name, 2..)) = named(cx, typeck, local.pat)
        {
            report(cx, local.hir_id, local.pat.span, format!("`let .. else` sends each `{name}` variant its pattern leaves unnamed to `else`"));
        }
    }
}
