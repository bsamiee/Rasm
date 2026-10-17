use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, eq_expr_value, peel_blocks, res::MaybeDef as _, sym, ty::option_arg_ty};
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashMap, FxIndexMap};
use rustc_hir::{
    Expr, ExprKind, Mutability, Pat,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass, declare_lint, impl_lint_pass};
use rustc_middle::ty::{self, AdtDef};
use rustc_span::Symbol;

use crate::{Access, Place, access, constructed, impl_owner, is_error, pattern_fields, place};

// --- [CONSTANTS] -----------------------------------------------------------------------

declare_lint! {
    SPLIT_OUTCOME,
    Warn,
    "flags stating whether an `Option` field holds a value, and `Option` value and error fields every construction sets one of"
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum Pair {
    Flag { flag: LocalDefId, value: LocalDefId },
    Outcome { value: LocalDefId, error: LocalDefId },
}

#[derive(Clone, Copy, Default)]
struct Seen {
    states: [[bool; 2]; 2],
    written: bool,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct SplitOutcome {
    pairs: FxHashMap<DefId, Vec<Pair>>,
    seen: FxIndexMap<Pair, Seen>,
}

impl_lint_pass!(SplitOutcome => [SPLIT_OUTCOME]);

impl SplitOutcome {
    fn pairs<'cache>(cache: &'cache mut FxHashMap<DefId, Vec<Pair>>, cx: &LateContext<'_>, adt: AdtDef<'_>) -> &'cache [Pair] {
        cache.entry(adt.did()).or_insert_with(|| {
            let fields = if adt.is_struct() && adt.did().is_local() {
                adt.non_enum_variant().fields.iter().filter_map(|field| field.did.as_local()).filter(|&field| !cx.effective_visibilities.is_exported(field)).collect()
            } else {
                Vec::new()
            };
            let declared = |field: LocalDefId| cx.tcx.type_of(field).instantiate_identity().skip_normalization();
            let options = fields.iter().filter_map(|&field| Some((field, is_error(cx, option_arg_ty(cx, declared(field))?))));
            let flags = fields.iter().filter(|&&field| declared(field).is_bool()).flat_map(|&flag| options.clone().map(move |(value, _)| Pair::Flag { flag, value }));
            let (errors, values) = options.clone().partition::<Vec<_>, _>(|&(_, failing)| failing);
            let outcomes = values.iter().flat_map(|&(value, _)| errors.iter().map(move |&(error, _)| Pair::Outcome { value, error }));
            flags.chain(outcomes).collect()
        })
    }

    fn write(&mut self, cx: &LateContext<'_>, field: LocalDefId) {
        let touched = Self::pairs(&mut self.pairs, cx, cx.tcx.adt_def(cx.tcx.local_parent(field)))
            .iter()
            .filter(|pair| matches!(pair, Pair::Flag { flag: first, value: second } | Pair::Outcome { value: first, error: second } if *first == field || *second == field));
        for &pair in touched {
            self.seen.entry(pair).or_default().written = true;
        }
    }
}

impl<'tcx> LateLintPass<'tcx> for SplitOutcome {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        const fn observe(seen: &mut Seen, (first, second): (bool, bool)) {
            let [[neither, second_only], [first_only, both]] = &mut seen.states;
            *match (first, second) {
                (false, false) => neither,
                (false, true) => second_only,
                (true, false) => first_only,
                (true, true) => both,
            } = true;
        }

        fn option_state<'tcx>(cx: &LateContext<'tcx>, value: &'tcx Expr<'tcx>) -> Option<bool> {
            let (variant, _) = constructed(cx.typeck_results(), value)?;
            let items = cx.tcx.lang_items();
            if items.option_some_variant() == Some(variant.def_id) {
                Some(true)
            } else if items.option_none_variant() == Some(variant.def_id) {
                Some(false)
            } else {
                None
            }
        }

        let typeck = cx.typeck_results();
        if let Some((_, fields)) = constructed(typeck, expr)
            && let Some(adt) = typeck.expr_ty(expr).ty_adt_def()
            && adt.is_struct()
        {
            let held = |field: LocalDefId| fields.iter().find(|(declared, _)| declared.did == field.to_def_id()).map(|&(_, value)| peel_blocks(value));
            let called = |call: &Expr<'_>, owner: Symbol| {
                let method = typeck.type_dependent_def_id(call.hir_id)?;
                impl_owner(cx.tcx, method).filter(|implementor| implementor.is_diag_item(cx, owner))?;
                Some(cx.tcx.item_name(method))
            };
            for &pair in Self::pairs(&mut self.pairs, cx, adt) {
                let seen = self.seen.entry(pair).or_default();
                match pair {
                    Pair::Flag { flag, value } => match held(flag).zip(held(value)) {
                        Some((flag, value))
                            if let ExprKind::Lit(literal) = flag.kind
                                && let LitKind::Bool(set) = literal.node
                                && let Some(some) = option_state(cx, value) =>
                            observe(seen, (set, some)),
                        Some((tested, value))
                            if let ExprKind::MethodCall(_, receiver, [], _) = tested.kind
                                && let Some(some) = called(tested, sym::Option).and_then(|name| (name == sym::is_some).then_some(true).or_else(|| (name == sym::is_none).then_some(false)))
                                && eq_expr_value(cx, expr.span.ctxt(), receiver.peel_borrows(), value) =>
                        {
                            observe(seen, (true, some));
                            observe(seen, (false, !some));
                        }
                        Some(_) | None => seen.written = true,
                    },
                    Pair::Outcome { value, error } => match held(value).zip(held(error)) {
                        Some((value, error)) if let Some(both) = option_state(cx, value).zip(option_state(cx, error)) => observe(seen, both),
                        Some((value, error))
                            if let (ExprKind::MethodCall(_, succeeded, [], _), ExprKind::MethodCall(_, failed, [], _)) = (value.kind, error.kind)
                                && called(value, sym::Result) == Some(sym::ok)
                                && called(error, sym::Result) == Some(sym::err)
                                && eq_expr_value(cx, expr.span.ctxt(), succeeded, failed) =>
                        {
                            observe(seen, (true, false));
                            observe(seen, (false, true));
                        }
                        Some(_) | None => seen.written = true,
                    },
                }
            }
        }
        if let Some(Place::Field(field)) = place(cx, expr)
            && matches!(access(cx, expr), Access::Assigned(_) | Access::Mutated)
        {
            self.write(cx, field);
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let typeck = cx.typeck_results();
        let written =
            pattern_fields(typeck, pat).filter(|(_, bound)| matches!(typeck.pat_ty(bound).kind(), ty::Ref(_, _, Mutability::Mut))).filter_map(|(field, _)| field.as_local()).collect::<Vec<_>>();
        for field in written {
            self.write(cx, field);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let restated = mem::take(&mut self.seen).into_iter().filter(|(_, seen)| !seen.written).filter_map(|(pair, seen)| match (pair, seen.states) {
            (Pair::Flag { flag, value }, [[true, false], [false, true]]) => Some((flag, value, Some(true))),
            (Pair::Flag { flag, value }, [[false, true], [true, false]]) => Some((flag, value, Some(false))),
            (Pair::Outcome { value, error }, [[false, true], [true, false]]) => Some((error, value, None)),
            (Pair::Flag { flag: _, value: _ } | Pair::Outcome { value: _, error: _ }, _) => None,
        });
        for (reported, other, agrees) in restated {
            let (reported_name, other_name, owner) = (cx.tcx.item_name(reported), cx.tcx.item_name(other), cx.tcx.item_name(cx.tcx.local_parent(reported)));
            let (message, note, help) = match agrees.map(|agrees| if agrees { "is_some" } else { "is_none" }) {
                Some(read) => (
                    format!("`{owner}` holds flag `{reported_name}` restating `{other_name}.{read}()`"),
                    format!("every construction sets `{reported_name}` from whether `{other_name}` holds a value"),
                    format!("delete `{reported_name}` and read `{other_name}.{read}()` or match `{other_name}`"),
                ),
                None => (
                    format!("`{owner}` holds failure `{reported_name}` beside success `{other_name}`"),
                    format!("every construction sets exactly one of `{other_name}` and `{reported_name}`"),
                    format!("replace `{other_name}` and `{reported_name}` with one `Result`"),
                ),
            };
            span_lint_hir_and_then(cx, SPLIT_OUTCOME, cx.tcx.local_def_id_to_hir_id(reported), cx.tcx.def_span(reported), message, |diag| {
                diag.span_note(cx.tcx.def_span(other), note);
                diag.help(help);
            });
        }
    }
}
