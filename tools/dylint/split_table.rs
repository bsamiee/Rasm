use core::{iter, mem, slice};

use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks, sym};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashSet, FxIndexMap};
use rustc_errors::listify;
use rustc_hir::{
    BinOpKind, Expr, ExprKind, Pat, PatExpr, PatExprKind, PatKind,
    def::DefKind,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AdtDef, Ty, TypeckResults};

use crate::{constant, generated};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    SPLIT_TABLE,
    Warn,
    "functions each returning one column of a table keyed by a fieldless enum, and maps and arrays holding one entry per variant"
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct SplitTable {
    columns: FxIndexMap<DefId, Vec<LocalDefId>>,
    reads: FxHashSet<DefId>,
}

impl_lint_pass!(SplitTable => [SPLIT_TABLE]);

impl<'tcx> LateLintPass<'tcx> for SplitTable {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn variant_array(cx: &LateContext<'_>, typeck: &TypeckResults<'_>, expr: &Expr<'_>) -> Option<DefId> {
            if let ExprKind::Array(elements @ [first, ..]) = expr.kind
                && !expr.span.from_expansion()
                && let Some(keyed) = keyed(typeck.expr_ty(first))
                && keyed.variants().len() == elements.len()
                && iter::zip(elements, keyed.variants())
                    .all(|(element, variant)| matches!(&element.kind, ExprKind::Path(qpath) if typeck.qpath_res(qpath, element.hir_id).opt_def_id() == variant.ctor_def_id()))
            {
                let name = cx.tcx.item_name(keyed.did());
                span_lint_hir_and_then(cx, SPLIT_TABLE, expr.hir_id, expr.span, format!("array lists every variant of `{name}` in declaration order"), |diag| {
                    diag.help(format!("derive `strum::VariantArray` on `{name}` and read `{name}::VARIANTS`"));
                });
                Some(keyed.did())
            } else {
                None
            }
        }

        fn total_map<'tcx>(cx: &LateContext<'tcx>, typeck: &TypeckResults<'tcx>, expr: &'tcx Expr<'tcx>) -> Option<DefId> {
            let listed = match expr.kind {
                ExprKind::Call(_, [listed]) => Some(listed),
                ExprKind::MethodCall(collected, &Expr { kind: ExprKind::MethodCall(iterated, listed, [], _), .. }, [], _)
                    if collected.ident.name == sym::collect && iterated.ident.name == sym::into_iter =>
                    Some(listed),
                _ => None,
            };
            if let Some(&ExprKind::Array(entries)) = listed.map(|listed| &listed.kind)
                && !expr.span.from_expansion()
                && let ty::Adt(map, args) = typeck.expr_ty(expr).kind()
                && cx.tcx.is_diagnostic_item(sym::BTreeMap, map.did())
                && let Some(keyed) = keyed(args.type_at(0))
                && entries.len() == keyed.variants().len()
                && let Some(keys) = entries
                    .iter()
                    .map(|entry| {
                        if let ExprKind::Tup([key, _]) = entry.kind
                            && let ExprKind::Path(qpath) = key.kind
                        {
                            typeck.qpath_res(&qpath, key.hir_id).opt_def_id()
                        } else {
                            None
                        }
                    })
                    .try_collect::<FxHashSet<_>>()
                && keys.len() == entries.len()
            {
                let name = cx.tcx.item_name(keyed.did());
                span_lint_hir_and_then(cx, SPLIT_TABLE, expr.hir_id, expr.span, format!("`BTreeMap` holds one entry per variant of `{name}`"), |diag| {
                    diag.help(format!("build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `{name}`"));
                });
                Some(keyed.did())
            } else {
                None
            }
        }

        fn variants(typeck: &TypeckResults<'_>, pat: &Pat<'_>) -> Option<Vec<DefId>> {
            let alternatives = if let PatKind::Or(alternatives) = pat.kind { alternatives } else { slice::from_ref(pat) };
            alternatives
                .iter()
                .map(|alternative| if let PatKind::Expr(&PatExpr { kind: PatExprKind::Path(qpath), hir_id, .. }) = alternative.kind { typeck.qpath_res(&qpath, hir_id).opt_def_id() } else { None })
                .try_collect()
        }

        let typeck = cx.typeck_results();
        if generated(cx, expr.hir_id) {
            return;
        }
        let compared = match expr.kind {
            ExprKind::Binary(operator, left, _) if matches!(operator.node, BinOpKind::Eq | BinOpKind::Ne) => keyed(typeck.expr_ty_adjusted(left)),
            ExprKind::Let(binding) => keyed(typeck.expr_ty_adjusted(binding.init)),
            _ => None,
        };
        if let ExprKind::Match(scrutinee, arms, _) = expr.kind
            && let Some(keyed) = keyed(typeck.expr_ty_adjusted(scrutinee))
        {
            let owner = cx.tcx.hir_enclosing_body_owner(expr.hir_id);
            let column =
                !expr.span.from_expansion()
                    && matches!(cx.tcx.def_kind(owner), DefKind::Fn | DefKind::AssocFn)
                    && cx.tcx.trait_item_of(owner).is_none()
                    && peel_blocks(cx.tcx.hir_body_owned_by(owner).value).hir_id == expr.hir_id
                    && arms.iter().all(|arm| arm.guard.is_none() && constant(typeck, arm.body))
                    && arms.iter().map(|arm| variants(typeck, arm.pat)).try_collect::<Vec<_>>().is_some_and(|covered| {
                        covered.iter().map(Vec::len).sum::<usize>() == keyed.variants().len() && covered.iter().flatten().collect::<FxHashSet<_>>().len() == keyed.variants().len()
                    });
            let named = arms.iter().all(|arm| matches!(peel_blocks(arm.body).kind, ExprKind::Lit(literal) if matches!(literal.node, LitKind::Str(..))));
            match (column, named) {
                (true, false) => self.columns.entry(keyed.did()).or_default().push(owner),
                (true, true) | (false, _) => {
                    self.reads.insert(keyed.did());
                }
            }
        } else if let Some(keyed) = compared {
            self.reads.insert(keyed.did());
        } else {
            self.reads.extend(total_map(cx, typeck, expr).into_iter().chain(variant_array(cx, typeck, expr)));
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tables = mem::take(&mut self.columns)
            .into_iter()
            .filter(|(keyed, columns)| columns.len() > 1 && !cx.tcx.def_span(*keyed).from_expansion())
            .filter_map(|(keyed, columns)| Some((keyed.as_local()?, listify(&columns, |&column| format!("`{}`", cx.tcx.item_name(column)))?, columns)));
        for (local, listed, columns) in tables {
            let (keyed, name) = (local.to_def_id(), cx.tcx.item_name(local));
            let help = if self.reads.contains(&keyed) {
                format!("return every column from one match on `{name}`")
            } else {
                format!("replace `{name}` with a struct holding {listed} and one `const` per variant")
            };
            span_lint_hir_and_then(cx, SPLIT_TABLE, cx.tcx.local_def_id_to_hir_id(local), cx.tcx.def_span(keyed), format!("{listed} each return one column of a table keyed by `{name}`"), |diag| {
                diag.span_note(columns.iter().map(|&column| cx.tcx.def_span(column)).collect::<Vec<_>>(), "each column matches every variant here");
                diag.help(help);
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn keyed(held: Ty<'_>) -> Option<AdtDef<'_>> { held.peel_refs().ty_adt_def().filter(|adt| adt.is_enum() && adt.did().is_local() && adt.variants().len() > 1 && adt.all_fields().next().is_none()) }
