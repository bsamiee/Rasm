use core::{cell::LazyCell, ops::ControlFlow};
use std::borrow::Cow;

use clippy_utils::{diagnostics::span_lint_hir_and_then, is_lang_item_or_ctor, paths::PathNS, peel_blocks, res::MaybeDef as _, sym};
use rustc_ast::{ItemKind as AstItemKind, MacCall, TyKind, visit::Visitor};
use rustc_attr_ir::lang_items::LangItem;
use rustc_data_structures::fx::FxHashSet;
use rustc_errors::Diag;
use rustc_hir::{ExprKind, Item, ItemKind, def_id::DefId};
use rustc_infer::infer::TyCtxtInferExt as _;
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{
    traits::EvaluationResult,
    ty::{self, TyCtxt, TypeVisitableExt as _, Upcast as _, print::with_forced_trimmed_paths},
};
use rustc_parse::{
    lexer::StripTokens,
    new_parser_from_source_str,
    parser::{AllowConstBlockItems, ForceCollect},
};
use rustc_span::FileName;
use rustc_trait_selection::traits::{Obligation, ObligationCause, query::evaluate_obligation::InferCtxtExt as _};
use rustc_type_ir::elaborate::elaborate;

use crate::{accessed_field, expanded_externally, parameter, path};

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    source: Option<DefId>,
}

lint!(Pass; MANUAL_ERROR_IMPL: "hand-written `core::error::Error` impls with default members or a field as their source");

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { source: path(tcx, PathNS::Value, "core::error::Error::source") } }
}

impl<'ast> Visitor<'ast> for Pass {
    type Result = ControlFlow<()>;

    fn visit_mac_call(&mut self, _: &'ast MacCall) -> Self::Result { ControlFlow::Break(()) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_item(&mut self, cx: &LateContext<'tcx>, item: &'tcx Item<'tcx>) {
        if let ItemKind::Impl(_) = item.kind
            && let Some(error_trait) = cx.tcx.impl_opt_trait_id(item.owner_id).filter(|&implemented| cx.tcx.is_diagnostic_item(sym::Error, implemented))
            && !expanded_externally(cx, item.span)
            && let implementor = cx.tcx.type_of(item.owner_id).instantiate_identity().skip_normalization()
            && let ty::Adt(adt, arguments) = implementor.kind()
            && let Some(local) = adt.did().as_local()
            && !adt.is_union()
            && *arguments == ty::GenericArgs::identity_for_item(cx.tcx, item.owner_id.to_def_id())
            && let implementations = cx.tcx.impl_item_implementor_ids(item.owner_id.to_def_id())
            && implementations.items().all(|(&declared, _)| Some(declared) == self.source)
            && let Some(sourced) = self.source.and_then(|source| implementations.get(&source)).map_or(Some(None), |&implementation| {
                let owner = implementation.as_local()?;
                let (body, typeck) = (cx.tcx.hir_body_owned_by(owner), cx.tcx.typeck(owner));
                let returned = peel_blocks(body.value);
                match returned.kind {
                    ExprKind::Path(qpath) if typeck.qpath_res(&qpath, returned.hir_id).opt_def_id().is_some_and(|called| is_lang_item_or_ctor(cx, called, LangItem::OptionNone)) => Some(None),
                    ExprKind::Call(callee, [value])
                        if let ty::FnDef(called, _) = typeck.expr_ty(callee).kind()
                            && is_lang_item_or_ctor(cx, *called, LangItem::OptionSome)
                            && let ExprKind::AddrOf(_, _, field) = value.kind
                            && let ExprKind::Field(receiver, _) = field.kind
                            && parameter(body, receiver) == Some(0)
                            && typeck.expr_ty_adjusted(receiver).ty_adt_def() == Some(*adt) =>
                    {
                        let field = accessed_field(typeck, field)?;
                        let declaration = cx.tcx.hir_span_with_body(cx.tcx.local_def_id_to_hir_id(local));
                        let source = cx.tcx.sess.source_map().span_to_snippet(declaration).ok()?;
                        let mut parser = new_parser_from_source_str(&cx.tcx.sess.psess, FileName::anon_source_code(&source), source, StripTokens::Nothing).map_or_else(
                            |errors| {
                                errors.into_iter().for_each(Diag::cancel);
                                None
                            },
                            Some,
                        )?;
                        let start = parser.token.span.lo();
                        let original = parser.parse_item(ForceCollect::No, AllowConstBlockItems::No).unwrap_or_else(|error| {
                            error.cancel();
                            None
                        })?;
                        let AstItemKind::Struct(_, _, data) = &original.kind else { return None };
                        let field_span = cx.tcx.def_span(field);
                        let offset = declaration.contains(field_span).then(|| field_span.lo().0.abs_diff(declaration.lo().0))?;
                        let written = &data.fields().iter().find(|written| written.span.lo().0.abs_diff(start.0) == offset)?.ty;
                        let optional = matches!(&written.kind, TyKind::Path(_, path) if path.segments.last().is_some_and(|segment| segment.ident.name == sym::Option && segment.args.is_some()));
                        (self.visit_ty(written).is_continue() && !optional).then(|| Some((field, cx.tcx.type_of(field).instantiate(cx.tcx, arguments).skip_normalization())))
                    }
                    _ => None,
                }
            })
            && {
                let tcx = cx.tcx;
                let owner_bounds = arguments
                    .types()
                    .next()
                    .is_some()
                    .then(|| {
                        let owner = ty::TraitRef::new(tcx, error_trait, [implementor]).upcast(tcx);
                        elaborate(tcx, [owner]).filter(move |&clause| clause != owner)
                    })
                    .into_iter()
                    .flatten();
                let source_bounds = sourced.filter(|&(_, source)| source.has_type_flags(ty::TypeFlags::HAS_TY_PARAM)).into_iter().flat_map(|(_, source)| {
                    [ty::TraitRef::new(tcx, error_trait, [source]).upcast(tcx), ty::Binder::dummy(ty::ClauseKind::TypeOutlives(ty::OutlivesClause(source, tcx.lifetimes.re_static))).upcast(tcx)]
                });
                let declared = tcx.clauses_of(adt.did()).instantiate_own(tcx, arguments).map(|(clause, _)| clause.skip_normalization());
                let required = elaborate(tcx, declared.chain(owner_bounds).chain(source_bounds)).collect::<FxHashSet<_>>();
                let inference = LazyCell::new(|| tcx.infer_ctxt().build_with_typing_env(ty::TypingEnv::new(ty::ParamEnv::new(tcx, required.iter().copied()), ty::TypingMode::non_body_analysis())));
                tcx.clauses_of(item.owner_id).instantiate_own_identity().all(|(clause, _)| {
                    let clause = clause.skip_normalization();
                    required.contains(&clause) || {
                        let (infcx, param_env) = &*inference;
                        infcx.evaluate_obligation(&Obligation::new(tcx, ObligationCause::dummy(), *param_env, clause)).is_ok_and(EvaluationResult::must_apply_considering_regions)
                    }
                })
            }
        {
            span_lint_hir_and_then(
                cx,
                MANUAL_ERROR_IMPL,
                item.hir_id(),
                cx.tcx.def_span(item.owner_id),
                with_forced_trimmed_paths!(format!("`{implementor}` implements `core::error::Error` by hand")),
                |diag| {
                    diag.help(match sourced {
                        Some((field, _)) => Cow::Owned(format!("derive `derive_more::Error` with `#[error(ignore)]` and mark field `{}` `#[error(source, not(backtrace))]`", cx.tcx.item_name(field))),
                        None if adt.all_fields().next().is_some() => Cow::Borrowed("derive `derive_more::Error` with `#[error(ignore)]`"),
                        None => Cow::Borrowed("derive `derive_more::Error`"),
                    });
                },
            );
        }
    }
}
