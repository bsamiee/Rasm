use core::{cell::LazyCell, iter, ops::ControlFlow};
use std::rc::Rc;

use clippy_utils::{diagnostics::span_lint_hir_and_then, peel_blocks, res::MaybeQPath as _, source::snippet};
use itertools::Itertools as _;
use rustc_data_structures::fx::{FxHashMap, FxHashSet, FxIndexMap};
use rustc_hir::{
    HirId, Lifetime, LifetimeKind, QPath,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
    intravisit::{self, Visitor},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::{
    hir::nested_filter,
    mir,
    ty::{self, AssocKind, Binder, Clause, EarlyBinder, TraitRef, TyCtxt},
};
use rustc_span::{Span, Symbol};
use rustc_trait_selection::{
    regions::test_region_obligations,
    traits::{ObligationCause, ObligationCtxt},
};
use rustc_type_ir::{Upcast as _, elaborate::elaborate};

use super::{Bodies, canonical, normalized, resolved_item};
use crate::{expanded_externally, exported};

// --- [SERVICES] ------------------------------------------------------------------------

struct Declaration<'tcx> {
    tcx: TyCtxt<'tcx>,
    body: Span,
    module: DefId,
    generics: FxHashSet<Symbol>,
    parameters: FxHashMap<DefId, ty::GenericArg<'tcx>>,
    self_owner: Option<(DefId, HirId)>,
}

impl<'tcx> Visitor<'tcx> for Declaration<'tcx> {
    type NestedFilter = nested_filter::All;
    type Result = ControlFlow<()>;

    fn maybe_tcx(&mut self) -> TyCtxt<'tcx> { self.tcx }

    fn visit_qpath(&mut self, qpath: &'tcx QPath<'tcx>, id: HirId, _span: Span) -> Self::Result {
        let available = |resolved| match resolved {
            Res::Def(DefKind::TyParam | DefKind::ConstParam, item) => self.parameters.contains_key(&item) || self.body.contains(self.tcx.def_span(item)),
            Res::SelfTyAlias { alias_to: item, .. } | Res::SelfTyParam { trait_: item } | Res::SelfCtor(item) => self.self_owner == Some((item, id)) || self.body.contains(self.tcx.def_span(item)),
            Res::Def(_, item) => self.tcx.visibility(item).is_accessible_from(self.module, self.tcx),
            Res::PrimTy(_) | Res::Local(_) => true,
            Res::ToolMod | Res::OpenMod(_) | Res::NonMacroAttr(_) | Res::Err => false,
        };
        if let QPath::Resolved(_, resolved) = qpath {
            let local = matches!(resolved.res, Res::Local(_))
                || resolved.res.opt_def_id().is_some_and(|item| self.parameters.contains_key(&item) || self.body.contains(self.tcx.def_span(item)))
                || matches!(resolved.res, Res::SelfTyAlias { .. } | Res::SelfTyParam { .. } | Res::SelfCtor(_)) && available(resolved.res);
            let unchanged = local || resolved.segments.iter().take(1).all(|segment| !self.generics.contains(&segment.ident.name));
            let scoped = resolved.segments.iter().all(|segment| match segment.res {
                Res::Def(DefKind::TyParam | DefKind::ConstParam, _) | Res::SelfTyAlias { .. } | Res::SelfTyParam { .. } | Res::SelfCtor(_) => available(segment.res),
                _ => true,
            });
            if !unchanged || !scoped || !available(resolved.res) {
                return ControlFlow::Break(());
            }
        }
        intravisit::walk_qpath(self, qpath, id)
    }

    fn visit_lifetime(&mut self, lifetime: &'tcx Lifetime) -> Self::Result {
        if let LifetimeKind::Param(item) = lifetime.kind
            && !self.parameters.contains_key(&item.to_def_id())
            && !self.body.contains(self.tcx.def_span(item))
        {
            ControlFlow::Break(())
        } else {
            intravisit::walk_lifetime(self, lifetime)
        }
    }
}

pub(crate) struct Pass<'tcx> {
    bodies: Rc<Bodies<'tcx>>,
}

impl<'tcx> Pass<'tcx> {
    pub(crate) const fn new(_: TyCtxt<'tcx>, bodies: Rc<Bodies<'tcx>>) -> Self { Self { bodies } }
}

lint!(Pass<'_>; DUPLICATE_TRAIT_CONSTANT: "trait constants that every impl sets to the same value");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl<'tcx> LateLintPass<'tcx> for Pass<'tcx> {
    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let bodies = &self.bodies;
        let provided = tcx.hir_crate_items(()).definitions().filter(|&item| tcx.def_kind(item) == DefKind::AssocConst && !expanded_externally(cx, tcx.def_span(item))).fold(
            FxIndexMap::<LocalDefId, Vec<LocalDefId>>::default(),
            |mut provided, item| {
                if let Some(declared) = tcx.trait_item_of(item).and_then(DefId::as_local) {
                    provided.entry(declared).or_default().push(item);
                }
                provided
            },
        );
        let shared = provided.into_iter().filter_map(|(declared, constants)| {
            let &first = constants.first()?;
            let declared_type = normalized(tcx, declared.to_def_id(), tcx.type_of(declared).instantiate_identity())?;
            let primitive = matches!(declared_type.kind(), ty::Bool | ty::Char | ty::Int(_) | ty::Uint(_));
            let evaluated = LazyCell::new(|| tcx.const_eval_poly(first.to_def_id()).ok());
            let compatible = |constant: LocalDefId| {
                normalized(tcx, constant.to_def_id(), tcx.type_of(constant).instantiate_identity()).is_some_and(|actual| canonical(tcx, actual, None) == canonical(tcx, declared_type, None))
            };
            let output_matches = |output| {
                if output == declared_type {
                    return true;
                }
                let param_env = tcx.param_env(declared);
                let known = tcx.assumed_wf_types(declared).iter().map(|&(held, _)| held).collect();
                test_region_obligations(tcx, declared, param_env, &known, |infcx| {
                    let obligations = ObligationCtxt::new(infcx);
                    obligations.sub(&ObligationCause::dummy(), param_env, output, declared_type).map_err(|_| ())?;
                    obligations.evaluate_obligations_error_on_ambiguity().no_errors().ok_or(())
                })
            };
            if !(matches!(tcx.associated_item(declared).kind, AssocKind::Const { .. })
                && constants.len() > 1
                && tcx.local_trait_impls(tcx.parent(declared.to_def_id())).len() == constants.len()
                && constants.iter().copied().all(compatible)
                && (if primitive {
                    evaluated.as_ref().is_some_and(|value| constants.iter().all(|&constant| tcx.const_eval_poly(constant.to_def_id()).ok().as_ref() == Some(value)))
                } else {
                    constants.array_windows().all(|&[earlier, later]| bodies.equal(cx, earlier, later, |_, _, _, _| false, |left_type, right_type| left_type == right_type).is_some())
                })
                && (!exported(cx, declared) || tcx.hir_maybe_body_owned_by(declared).is_none() || bodies.equal(cx, declared, first, |_, _, _, _| false, |earlier, later| earlier == later).is_some()))
            {
                return None;
            }
            if primitive {
                return Some((declared, mir::Const::Val(*evaluated.as_ref()?, declared_type).to_string(), constants));
            }
            let body = tcx.hir_body_owned_by(first).value;
            let module = tcx.parent_module_from_def_id(declared);
            let generics =
                iter::successors(Some(declared.to_def_id()), |&item| tcx.generics_of(item).parent).flat_map(|item| &tcx.generics_of(item).own_params).map(|parameter| parameter.name).collect();
            let implementation = tcx.parent(first.to_def_id());
            let owner = tcx.parent(declared.to_def_id());
            let implemented = tcx.impl_trait_header(implementation).trait_ref.instantiate_identity().skip_normalization();
            let identity: Clause<'tcx> = Binder::dummy(TraitRef::identity(tcx, owner)).upcast(tcx);
            let available = elaborate(tcx, tcx.param_env(declared).caller_bounds().chain([identity])).collect::<FxHashSet<_>>();
            let parameters = iter::zip(&tcx.generics_of(owner).own_params, implemented.args)
                .cartesian_product(&tcx.generics_of(implementation).own_params)
                .filter(|&((declaration, argument), source)| source.name == declaration.name && tcx.mk_param_from_def(source) == argument)
                .map(|((declaration, _), source)| (source.def_id, tcx.mk_param_from_def(declaration)))
                .collect::<FxHashMap<_, _>>();
            let inherited: Option<Option<HirId>> = try {
                let (qpath, initializer) = peel_blocks(body).opt_qpath()?;
                let typeck = tcx.typeck(tcx.typeck_root_def_id_local(first));
                let called = typeck.qpath_res(qpath, initializer).opt_def_id()?;
                let (member, arguments) = resolved_item(tcx, called, typeck.node_args(initializer))?;
                let trait_owner = tcx.opt_associated_item(member)?.trait_container(tcx)?;
                let rebased = ty::GenericArgs::for_item(tcx, implementation, |parameter, _| parameters.get(&parameter.def_id).copied().unwrap_or_else(|| tcx.mk_param_from_def(parameter)));
                let arguments = EarlyBinder::bind(tcx, arguments).instantiate(tcx, rebased).skip_normalization();
                let self_path = match qpath {
                    QPath::Resolved(Some(held), _) | QPath::TypeRelative(held, _) => held
                        .opt_qpath()
                        .filter(|&(path, node)| matches!(typeck.qpath_res(path, node), Res::SelfTyAlias { alias_to, .. } | Res::SelfTyParam { trait_: alias_to } if alias_to == implementation))
                        .map(|(_, node)| node),
                    QPath::Resolved(None, path) => path
                        .segments
                        .first()
                        .filter(|segment| matches!(segment.res, Res::SelfTyAlias { alias_to, .. } | Res::SelfTyParam { trait_: alias_to } if alias_to == implementation))
                        .map(|_| initializer),
                };
                let required = TraitRef::from_assoc(tcx, trait_owner, arguments);
                let required = Binder::dummy(if self_path.is_some() { required.with_replaced_self_ty(tcx, tcx.types.self_param) } else { required });
                let symbolic =
                    available.iter().filter_map(|clause| clause.as_trait_clause().map(|bound| bound.map_bound(|predicate| predicate.trait_ref))).find(|&bound| bound == required)?.no_bound_vars()?;
                let symbolic_arguments = arguments.rebase_onto(tcx, trait_owner, symbolic.args);
                let output = normalized(tcx, declared.to_def_id(), tcx.type_of(member).instantiate(tcx, symbolic_arguments))?;
                (output_matches(output) && tcx.clauses_of(member).instantiate_own(tcx, symbolic_arguments).all(|(clause, _)| available.contains(&clause.skip_normalization()))).then_some(self_path)?
            };
            let parameters = if inherited.is_some() { parameters } else { FxHashMap::default() };
            let mut declaration = Declaration { tcx, body: body.span, module: module.to_def_id(), generics, parameters, self_owner: inherited.flatten().map(|node| (implementation, node)) };
            let value = (tcx.parent_module_from_def_id(first) == module && declaration.visit_expr(body).is_continue()).then(|| snippet(cx, body.span, "..").into_owned())?;
            Some((declared, value, constants))
        });
        for (declared, value, constants) in shared {
            let (name, owner) = (tcx.item_name(declared), tcx.item_name(tcx.parent(declared.to_def_id())));
            span_lint_hir_and_then(cx, DUPLICATE_TRAIT_CONSTANT, tcx.local_def_id_to_hir_id(declared), tcx.def_span(declared), format!("every `{owner}` impl sets `{name}` to `{value}`"), |diag| {
                diag.span_note(constants.iter().map(|&constant| tcx.def_span(constant)).collect::<Vec<_>>(), "set here");
                diag.help(format!("set the default of `{owner}::{name}` to `{value}` and delete `{name}` from each impl"));
            });
        }
    }
}
