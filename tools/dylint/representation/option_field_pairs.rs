use core::mem;

use clippy_utils::{diagnostics::span_lint_hir_and_then, eq_expr_value, peel_blocks, res::MaybeDef as _, sym, ty::option_arg_ty};
use itertools::Itertools as _;
use rustc_ast::LitKind;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{
    Expr, ExprKind, Mutability, Pat,
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{self, AdtDef, print::with_forced_trimmed_paths};
use rustc_span::Spanned;

use crate::{Place, access, constructed, impl_owner, is_error, is_text, pattern_fields, place};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, Hash)]
enum FieldPair {
    Flag { flag: LocalDefId, value: LocalDefId },
    Outcome { value: LocalDefId, error: LocalDefId },
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Default)]
pub(crate) struct Pass {
    pairs: FxHashMap<DefId, Vec<FieldPair>>,
    observed: FxIndexMap<FieldPair, Option<FxHashSet<(bool, bool)>>>,
    written: FxHashSet<LocalDefId>,
}

lint! {
    default Pass;
    REDUNDANT_OPTION_FLAG: "`bool` fields that every construction sets from whether an `Option` field is `Some`",
    OPTION_FIELDS_AS_RESULT: "`Option` value and `Option` error field pairs that every construction sets exactly one of",
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        fn field_pairs(cx: &LateContext<'_>, adt: AdtDef<'_>) -> Vec<FieldPair> {
            let declared = |field: LocalDefId| cx.tcx.type_of(field).instantiate_identity().skip_normalization();
            let fields = adt.non_enum_variant().fields.iter().filter_map(|field| field.did.as_local());
            let options = fields.clone().filter_map(|field| Some((field, option_arg_ty(cx, declared(field))?))).collect::<Vec<_>>();
            let (errors, values) = options.iter().partition::<Vec<_>, _>(|&&(_, held)| is_error(cx, held) || is_text(cx, held));
            let flags = fields.filter(|&field| declared(field).is_bool()).cartesian_product(&options).map(|(flag, &(value, _))| FieldPair::Flag { flag, value });
            let outcomes = values.iter().cartesian_product(&errors).map(|(&&(value, _), &&(error, _))| FieldPair::Outcome { value, error });
            flags.chain(outcomes).collect()
        }

        let typeck = cx.typeck_results();
        if let Some((_, fields)) = constructed(typeck, expr)
            && let Some(adt) = typeck.expr_ty(expr).ty_adt_def().filter(|adt| adt.is_struct() && adt.did().is_local())
        {
            let value_of = |field: LocalDefId| fields.iter().find(|(declared, _)| declared.did == field.to_def_id()).map(|&(_, value)| peel_blocks(value));
            let present = |value: &'tcx Expr<'tcx>| constructed(typeck, value).map(|(variant, _)| Some(variant.def_id) == cx.tcx.lang_items().option_some_variant());
            let called = |call: &Expr<'_>| {
                typeck
                    .type_dependent_def_id(call.hir_id)
                    .filter(|&method| matches!(impl_owner(cx.tcx, method).opt_diag_name(cx), Some(sym::Option | sym::Result)))
                    .map(|method| cx.tcx.item_name(method))
            };
            let source = |value: &'tcx Expr<'tcx>| {
                if let ExprKind::MethodCall(_, source, [], _) = value.kind
                    && called(value) == Some(sym::ok)
                {
                    source.peel_borrows()
                } else {
                    value
                }
            };
            for &pair in &*self.pairs.entry(adt.did()).or_insert_with(|| field_pairs(cx, adt)) {
                let states = match pair {
                    FieldPair::Flag { flag, value } => value_of(flag).zip(value_of(value)).and_then(|(tested, value)| match tested.kind {
                        ExprKind::Lit(Spanned { node: LitKind::Bool(set), .. }) => present(value).map(|held| [(set, held); 2]),
                        ExprKind::MethodCall(_, receiver, [], _) => called(tested)
                            .and_then(|name| match name {
                                sym::is_some | sym::is_ok => Some(true),
                                sym::is_none | sym::is_err => Some(false),
                                _ => None,
                            })
                            .filter(|_| eq_expr_value(cx, expr.span.ctxt(), receiver.peel_borrows(), source(value)))
                            .map(|held| [(true, held), (false, !held)]),
                        _ => None,
                    }),
                    FieldPair::Outcome { value, error } => value_of(value).zip(value_of(error)).and_then(|(value, error)| match (value.kind, error.kind) {
                        (ExprKind::MethodCall(_, succeeded, [], _), ExprKind::MethodCall(_, failed, [], _)) if called(value) == Some(sym::ok) && called(error) == Some(sym::err) =>
                            eq_expr_value(cx, expr.span.ctxt(), succeeded, failed).then_some([(true, false), (false, true)]),
                        _ => present(value).zip(present(error)).map(|both| [both; 2]),
                    }),
                };
                let observed = self.observed.entry(pair).or_insert_with(|| Some(FxHashSet::default()));
                *observed = observed.take().zip(states).map(|(mut observed, states)| {
                    observed.extend(states);
                    observed
                });
            }
        }
        if let Some(Place::Field(field)) = place(cx, expr)
            && access(cx, expr).writes()
        {
            self.written.insert(field);
        }
    }

    fn check_pat(&mut self, cx: &LateContext<'tcx>, pat: &'tcx Pat<'tcx>) {
        let typeck = cx.typeck_results();
        self.written.extend(pattern_fields(typeck, pat).filter(|(_, bound)| matches!(typeck.pat_ty(bound).kind(), ty::Ref(_, _, Mutability::Mut))).filter_map(|(field, _)| field.as_local()));
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let written = mem::take(&mut self.written);
        let decided = mem::take(&mut self.observed).into_iter().filter_map(|(pair, states)| {
            let (FieldPair::Flag { flag: first, value: second } | FieldPair::Outcome { value: first, error: second }) = pair;
            let states = states.filter(|states| states.len() == 2 && !written.contains(&first) && !written.contains(&second))?;
            let mirrored = states.contains(&(true, true)) && states.contains(&(false, false));
            (mirrored || states.contains(&(true, false)) && states.contains(&(false, true))).then_some((pair, mirrored))
        });
        let held = |field: LocalDefId| option_arg_ty(cx, cx.tcx.type_of(field).instantiate_identity().skip_normalization());
        for (pair, mirrored) in decided {
            let name = |field: LocalDefId| cx.tcx.item_name(field);
            match pair {
                FieldPair::Flag { flag, value } => {
                    let (read, flag_name, value_name) = (if mirrored { "is_some" } else { "is_none" }, name(flag), name(value));
                    span_lint_hir_and_then(
                        cx,
                        REDUNDANT_OPTION_FLAG,
                        cx.tcx.local_def_id_to_hir_id(flag),
                        cx.tcx.def_span(flag),
                        format!("every construction sets `{}::{flag_name}` to `{value_name}.{read}()`", name(cx.tcx.local_parent(flag))),
                        |diag| {
                            diag.span_note(cx.tcx.def_span(value), format!("`{value_name}` declared here"));
                            diag.help(format!("delete `{flag_name}` and read `{value_name}.{read}()` or match `{value_name}`"));
                        },
                    );
                }
                FieldPair::Outcome { value, error } if !mirrored && let Some((succeeded, failed)) = held(value).zip(held(error)) => {
                    let (value_name, error_name, outcome) = (name(value), name(error), with_forced_trimmed_paths!(format!("Result<{succeeded}, {failed}>")));
                    span_lint_hir_and_then(
                        cx,
                        OPTION_FIELDS_AS_RESULT,
                        cx.tcx.local_def_id_to_hir_id(error),
                        cx.tcx.def_span(error),
                        format!("every construction of `{}` sets exactly one of success `{value_name}` and failure `{error_name}`", name(cx.tcx.local_parent(error))),
                        |diag| {
                            diag.span_note(cx.tcx.def_span(value), format!("`{value_name}` declared here"));
                            diag.help(format!("replace `{value_name}` and `{error_name}` with one `{outcome}` field"));
                        },
                    );
                }
                FieldPair::Outcome { value: _, error: _ } => {}
            }
        }
    }
}
