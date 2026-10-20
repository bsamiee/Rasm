use core::{iter, mem};

use clippy_utils::{diagnostics::span_lint_hir_and_then, get_parent_expr, has_repr_attr, paths::PathNS, peel_hir_expr_while, res::MaybeDef as _, sym};
use rustc_abi::ExternAbi;
use rustc_data_structures::fx::FxHashSet;
use rustc_hir::{
    Expr, ExprKind, HirId, LetStmt, Node, PatKind, TyKind,
    def::{DefKind, Res},
    def_id::{DefId, LocalDefId},
};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::{Ty, TyCtxt};
use rustc_span::{Ident, Symbol};

use crate::{accessed_field, bounds_of, expanded_externally, exported, generated, is_text, parameter_of, paths, signature};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Domain {
    Instant,
    Duration,
    ByteSize,
    Path,
    Calendar,
    Quantity,
}

impl Domain {
    fn of<'tcx>(cx: &LateContext<'tcx>, name: Symbol, held: Ty<'tcx>) -> Option<Self> {
        const EPOCH: &str = "epoch";
        let name = name.as_str().to_lowercase();
        let mut segments = name.rsplit('_');
        let last = segments.next()?;
        let number = held.is_numeric();
        match (segments.next(), last) {
            (_, "timestamp") | (Some("since"), EPOCH) | (Some("created" | "updated" | "modified" | "expires" | "issued" | "started" | "finished" | "deleted"), "at") => number.then_some(Self::Instant),
            (previous, "secs" | "seconds" | "ms" | "millis" | "milliseconds" | "us" | "micros" | "ns" | "nanos" | "mins" | "minutes" | "hours") =>
                number.then_some(if previous == Some(EPOCH) { Self::Instant } else { Self::Duration }),
            (_, "timeout" | "interval" | "delay" | "ttl" | "duration" | "elapsed") => number.then_some(Self::Duration),
            (_, "bytes" | "kb" | "kib" | "mb" | "mib" | "gb" | "gib") => held.is_integral().then_some(Self::ByteSize),
            (_, "path" | "dir" | "directory" | "file" | "folder") => is_text(cx, held).then_some(Self::Path),
            (_, "date" | "time" | "datetime") => is_text(cx, held).then_some(Self::Calendar),
            (Some(_), "mm" | "km" | "ft" | "kg" | "lbs" | "deg" | "rad" | "kpa" | "mpa" | "psi" | "kw" | "kwh" | "hz" | "khz" | "mhz" | "pct")
            | (
                _,
                "percent" | "millimeters" | "millimetres" | "centimeters" | "centimetres" | "meters" | "metres" | "kilometers" | "kilometres" | "feet" | "inches" | "kilograms" | "degrees" | "radians"
                | "pascals" | "watts" | "kilowatts" | "joules" | "celsius" | "kelvin" | "fahrenheit" | "hertz",
            ) => (held == cx.tcx.types.f64).then_some(Self::Quantity),
            _ => None,
        }
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

pub(crate) struct Pass {
    serialized: FxHashSet<DefId>,
    declared: Vec<(HirId, Ident, Domain, String)>,
    reached: FxHashSet<HirId>,
}

lint! {
    Pass;
    PRIMITIVE_DOMAIN_BINDING:
        "bindings, parameters, fields, constants, and statics of a number or text named for a duration, instant, byte size, calendar value, physical quantity, or a path a filesystem API reads",
}

impl Pass {
    pub(crate) fn new(tcx: TyCtxt<'_>) -> Self { Self { serialized: paths(tcx, PathNS::Type, &["serde::Serialize", "serde::Deserialize"]), declared: Vec::new(), reached: FxHashSet::default() } }
}

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        if let PatKind::Binding(_, binding, ident, None) = local.pat.kind
            && !expanded_externally(cx, local.span)
        {
            self.declared.extend(declaration(cx, binding, ident, cx.typeck_results().node_type(binding)));
        }
    }

    fn check_expr(&mut self, cx: &LateContext<'tcx>, expr: &'tcx Expr<'tcx>) {
        let typeck = cx.typeck_results();
        let pathlike = |held: Ty<'_>| matches!(held.peel_refs().opt_diag_name(cx), Some(sym::Path | sym::PathBuf | sym::OsStr));
        let bounded = |(callee, parameter): (DefId, Ty<'tcx>)| {
            bounds_of(cx.tcx, callee, parameter.peel_refs())
                .any(|bound| matches!(bound.def_id().opt_diag_name(cx), Some(sym::AsRef | sym::Into)) && pathlike(bound.skip_binder().trait_ref.args.type_at(1)))
        };
        let argument = || peel_hir_expr_while(expr, |inner| get_parent_expr(cx, inner).filter(|outer| matches!(outer.kind, ExprKind::AddrOf(..))));
        if let ExprKind::Closure(closure) = expr.kind
            && !expanded_externally(cx, expr.span)
        {
            self.declared.extend(iter::zip(cx.tcx.hir_body(closure.body).params, closure.fn_decl.inputs).filter_map(|(parameter, written)| match (parameter.pat.kind, written.kind) {
                (_, TyKind::Infer(..)) => None,
                (PatKind::Binding(_, binding, name, None), _) => declaration(cx, binding, name, typeck.node_type(binding)),
                _ => None,
            }));
        }
        let read = match expr.kind {
            ExprKind::Field(..) => accessed_field(typeck, expr).and_then(DefId::as_local).map(|field| cx.tcx.local_def_id_to_hir_id(field)),
            ExprKind::Path(qpath) => match cx.qpath_res(&qpath, expr.hir_id) {
                Res::Local(binding) => Some(binding),
                Res::Def(DefKind::Const | DefKind::AssocConst | DefKind::Static { .. }, item) => item.as_local().map(|item| cx.tcx.local_def_id_to_hir_id(item)),
                _ => None,
            },
            _ => None,
        };
        if let Some(read) = read
            && Domain::of(cx, cx.tcx.hir_name(read), typeck.expr_ty(expr)) == Some(Domain::Path)
            && let argument = argument()
            && get_parent_expr(cx, argument).is_some_and(|consumer| pathlike(typeck.expr_ty(consumer)) || parameter_of(cx, consumer, argument).is_some_and(bounded))
        {
            self.reached.insert(read);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        let tcx = cx.tcx;
        let type_of = |item: DefId| tcx.type_of(item).instantiate_identity().skip_normalization();
        let serialized = |adt: LocalDefId| self.serialized.iter().any(|&serialized| tcx.non_blanket_impls_for_ty(serialized, type_of(adt.to_def_id())).next().is_some());
        let items = tcx
            .hir_crate_items(())
            .definitions()
            .filter(|&item| !expanded_externally(cx, tcx.def_span(item)) && tcx.trait_impl_of_assoc(item.to_def_id()).is_none() && !generated(cx, tcx.local_def_id_to_hir_id(item)))
            .flat_map(|item| -> Vec<_> {
                let node = tcx.local_def_id_to_hir_id(item);
                match tcx.def_kind(item) {
                    DefKind::Const | DefKind::AssocConst | DefKind::Static { nested: false, .. } =>
                        tcx.opt_item_ident(item.to_def_id()).and_then(|name| declaration(cx, node, name, type_of(item.to_def_id()))).into_iter().collect(),
                    DefKind::Struct | DefKind::Enum | DefKind::Union if !has_repr_attr(cx, node) => tcx
                        .adt_def(item)
                        .all_fields()
                        .filter_map(|field| declaration(cx, tcx.local_def_id_to_hir_id(field.did.as_local()?), field.ident(tcx), type_of(field.did)))
                        .filter(|&(.., domain, _)| domain != Domain::Quantity || !serialized(item))
                        .collect(),
                    DefKind::Fn | DefKind::AssocFn
                        if let declared = signature(tcx, item.to_def_id())
                            && declared.abi() == ExternAbi::Rust
                            && !tcx.codegen_fn_attrs(item).contains_extern_indicator()
                            && let inputs = declared.inputs() =>
                        match tcx.hir_maybe_body_owned_by(item) {
                            Some(body) => iter::zip(body.params, inputs)
                                .filter_map(|(parameter, &input)| if let PatKind::Binding(_, binding, name, None) = parameter.pat.kind { declaration(cx, binding, name, input) } else { None })
                                .collect(),
                            None => iter::zip(tcx.fn_arg_idents(item), inputs).filter_map(|(name, &input)| declaration(cx, node, (*name)?, input)).collect(),
                        },
                    _ => Vec::new(),
                }
            });
        let open = |node: HirId| match tcx.hir_node(node) {
            Node::Field(field) => exported(cx, field.def_id),
            Node::Pat(_) => matches!(tcx.parent_hir_node(node), Node::Param(_)) && exported(cx, tcx.hir_enclosing_body_owner(node)),
            _ => exported(cx, node.owner.def_id),
        };
        for (node, ident, domain, held) in iter::chain(mem::take(&mut self.declared), items).filter(|&(node, _, domain, _)| domain != Domain::Path || self.reached.contains(&node) || open(node)) {
            let wire = matches!(tcx.hir_node(node), Node::Field(field) if serialized(match tcx.local_parent(field.def_id) {
                variant if tcx.def_kind(variant) == DefKind::Variant => tcx.local_parent(variant),
                adt => adt,
            }));
            let constant = matches!(tcx.hir_node(node), Node::Item(_) | Node::ImplItem(_) | Node::TraitItem(_))
                && matches!(tcx.def_kind(node.owner.def_id), DefKind::Const | DefKind::AssocConst | DefKind::Static { .. });
            let (stated, declared, encoded) = match domain {
                Domain::Instant => ("an instant", "`jiff::Timestamp`", Some("TimestampSeconds<i64>")),
                Domain::Duration => ("a duration", "`core::time::Duration`", Some("DurationSeconds<u64>")),
                Domain::ByteSize => ("a byte size", "`bytesize::ByteSize`", None),
                Domain::Path => ("a path", "`PathBuf` or `&Path`", None),
                Domain::Calendar => ("a calendar value", "`jiff::civil::Date`, `Time`, or `DateTime`", None),
                Domain::Quantity =>
                    ("a physical quantity", "`uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)", None),
            };
            let help = match (encoded.filter(|_| wire), domain, constant) {
                (Some(encoded), ..) => format!("declare a {declared} with `#[serde_as(as = \"{encoded}\")]`"),
                (None, Domain::Path, true) => String::from("declare a `&Path` through `Path::new` under `#![feature(const_convert)]`"),
                (None, Domain::Duration, _) => format!("declare a {declared} and convert where a protocol or API takes the number"),
                _ => format!("declare a {declared}"),
            };
            span_lint_hir_and_then(cx, PRIMITIVE_DOMAIN_BINDING, node, ident.span, format!("`{ident}` holds {stated} as `{held}`"), |diag| {
                diag.help(help);
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn declaration<'tcx>(cx: &LateContext<'tcx>, node: HirId, ident: Ident, held: Ty<'tcx>) -> Option<(HirId, Ident, Domain, String)> {
    Some((node, ident, Domain::of(cx, ident.name, held)?, held.to_string()))
}
