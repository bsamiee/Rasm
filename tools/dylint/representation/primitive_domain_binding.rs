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
use rustc_middle::ty::{self, FloatTy, Ty, TyCtxt, print::with_forced_trimmed_paths};
use rustc_span::{Ident, Symbol};

use crate::{accessed_field, bounds_of, expanded_externally, exported, generated, is_text, parameter_of, paths, signature};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, derive_more::Display, strum::IntoStaticStr)]
enum Clock {
    #[display("Timestamp")]
    #[strum(serialize = "jiff::Timestamp")]
    Instant,
    #[strum(serialize = "core::time::Duration")]
    Duration,
}

impl Clock {
    fn whole(self, tcx: TyCtxt<'_>) -> Ty<'_> {
        match self {
            Self::Instant => tcx.types.i64,
            Self::Duration => tcx.types.u64,
        }
    }
}

#[derive(Clone, Copy, PartialEq, Eq, strum::EnumString)]
enum Unit {
    #[strum(serialize = "secs", serialize = "seconds")]
    Second,
    #[strum(serialize = "ms", serialize = "millis", serialize = "milliseconds")]
    Millisecond,
    #[strum(serialize = "us", serialize = "micros")]
    Microsecond,
    #[strum(serialize = "ns", serialize = "nanos")]
    Nanosecond,
    #[strum(serialize = "mins", serialize = "minutes")]
    Minute,
    #[strum(serialize = "hours")]
    Hour,
}

impl Unit {
    const fn encoded(self) -> Option<&'static str> {
        match self {
            Self::Second => Some("Seconds"),
            Self::Millisecond => Some("MilliSeconds"),
            Self::Microsecond => Some("MicroSeconds"),
            Self::Nanosecond => Some("NanoSeconds"),
            Self::Minute | Self::Hour => None,
        }
    }

    const fn constructor(self, clock: Clock) -> Option<&'static str> {
        match (clock, self) {
            (Clock::Instant, Self::Second) => Some("Timestamp::from_second"),
            (Clock::Instant, Self::Millisecond) => Some("Timestamp::from_millisecond"),
            (Clock::Instant, Self::Microsecond) => Some("Timestamp::from_microsecond"),
            (Clock::Instant, Self::Nanosecond) => Some("Timestamp::from_nanosecond"),
            (Clock::Instant, Self::Minute | Self::Hour) => None,
            (Clock::Duration, Self::Second) => Some("Duration::from_secs"),
            (Clock::Duration, Self::Millisecond) => Some("Duration::from_millis"),
            (Clock::Duration, Self::Microsecond) => Some("Duration::from_micros"),
            (Clock::Duration, Self::Nanosecond) => Some("Duration::from_nanos"),
            (Clock::Duration, Self::Minute) => Some("Duration::from_mins"),
            (Clock::Duration, Self::Hour) => Some("Duration::from_hours"),
        }
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BYTE_SIZES: &[(&str, &str)] = &[("bytes", "b"), ("kb", "kb"), ("kib", "kib"), ("mb", "mb"), ("mib", "mib"), ("gb", "gb"), ("gib", "gib")];

const QUANTITIES: &[(&str, bool, &str, &str, &str)] = &[
    ("mm", true, "Length", "length", "millimeter"),
    ("km", true, "Length", "length", "kilometer"),
    ("ft", true, "Length", "length", "foot"),
    ("kg", true, "Mass", "mass", "kilogram"),
    ("lbs", true, "Mass", "mass", "pound"),
    ("deg", true, "Angle", "angle", "degree"),
    ("rad", true, "Angle", "angle", "radian"),
    ("kpa", true, "Pressure", "pressure", "kilopascal"),
    ("mpa", true, "Pressure", "pressure", "megapascal"),
    ("psi", true, "Pressure", "pressure", "pound_force_per_square_inch"),
    ("kw", true, "Power", "power", "kilowatt"),
    ("kwh", true, "Energy", "energy", "kilowatt_hour"),
    ("hz", true, "Frequency", "frequency", "hertz"),
    ("khz", true, "Frequency", "frequency", "kilohertz"),
    ("mhz", true, "Frequency", "frequency", "megahertz"),
    ("pct", true, "Ratio", "ratio", "percent"),
    ("percent", false, "Ratio", "ratio", "percent"),
    ("millimeters", false, "Length", "length", "millimeter"),
    ("millimetres", false, "Length", "length", "millimeter"),
    ("centimeters", false, "Length", "length", "centimeter"),
    ("centimetres", false, "Length", "length", "centimeter"),
    ("meters", false, "Length", "length", "meter"),
    ("metres", false, "Length", "length", "meter"),
    ("kilometers", false, "Length", "length", "kilometer"),
    ("kilometres", false, "Length", "length", "kilometer"),
    ("feet", false, "Length", "length", "foot"),
    ("inches", false, "Length", "length", "inch"),
    ("kilograms", false, "Mass", "mass", "kilogram"),
    ("degrees", false, "Angle", "angle", "degree"),
    ("radians", false, "Angle", "angle", "radian"),
    ("pascals", false, "Pressure", "pressure", "pascal"),
    ("watts", false, "Power", "power", "watt"),
    ("kilowatts", false, "Power", "power", "kilowatt"),
    ("joules", false, "Energy", "energy", "joule"),
    ("celsius", false, "ThermodynamicTemperature", "thermodynamic_temperature", "degree_celsius"),
    ("kelvin", false, "ThermodynamicTemperature", "thermodynamic_temperature", "kelvin"),
    ("fahrenheit", false, "ThermodynamicTemperature", "thermodynamic_temperature", "degree_fahrenheit"),
    ("hertz", false, "Frequency", "frequency", "hertz"),
];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Domain {
    Time { clock: Clock, unit: Option<Unit>, float: Option<FloatTy> },
    ByteSize { constructor: &'static str },
    Path { borrowed: bool },
    Calendar { civil: &'static str },
    Quantity { quantity: &'static str, module: &'static str, unit: &'static str },
}

impl Domain {
    fn of<'tcx>(cx: &LateContext<'tcx>, name: Symbol, held: Ty<'tcx>) -> Option<Self> {
        const EPOCH: &str = "epoch";
        let name = name.as_str().to_lowercase();
        let mut segments = name.rsplit('_');
        let last = segments.next()?;
        let float = if let ty::Float(float) = held.kind() { Some(*float) } else { None };
        let time = |clock, unit| held.is_numeric().then_some(Self::Time { clock, unit, float });
        let unit = last.parse::<Unit>().ok();
        match (segments.next(), last) {
            (_, "timestamp") | (Some("since"), EPOCH) | (Some("created" | "updated" | "modified" | "expires" | "issued" | "started" | "finished" | "deleted"), "at") => time(Clock::Instant, None),
            (previous, _) if unit.is_some() => time(if previous == Some(EPOCH) { Clock::Instant } else { Clock::Duration }, unit),
            (_, "timeout" | "interval" | "delay" | "ttl" | "duration" | "elapsed") => time(Clock::Duration, None),
            _ if let Some(&(_, constructor)) = BYTE_SIZES.iter().find(|&&(suffix, _)| suffix == last) => held.is_integral().then_some(Self::ByteSize { constructor }),
            (_, "path" | "dir" | "directory" | "file" | "folder") => is_text(cx, held).then_some(Self::Path { borrowed: held.is_ref() }),
            (_, "date") => is_text(cx, held).then_some(Self::Calendar { civil: "Date" }),
            (_, "time") => is_text(cx, held).then_some(Self::Calendar { civil: "Time" }),
            (_, "datetime") => is_text(cx, held).then_some(Self::Calendar { civil: "DateTime" }),
            (previous, _) => QUANTITIES
                .iter()
                .find(|&&(suffix, prefixed, ..)| suffix == last && (previous.is_some() || !prefixed))
                .filter(|_| held == cx.tcx.types.f64)
                .map(|&(_, _, quantity, module, measure)| Self::Quantity { quantity, module, unit: measure }),
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
            && matches!(Domain::of(cx, cx.tcx.hir_name(read), typeck.expr_ty(expr)), Some(Domain::Path { .. }))
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
                        .filter(|&(.., domain, _)| !matches!(domain, Domain::Quantity { .. }) || !serialized(item))
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
        for (node, ident, domain, held) in
            iter::chain(mem::take(&mut self.declared), items).filter(|&(node, _, domain, _)| !matches!(domain, Domain::Path { .. }) || self.reached.contains(&node) || open(node))
        {
            let wire = matches!(tcx.hir_node(node), Node::Field(field) if serialized(match tcx.local_parent(field.def_id) {
                variant if tcx.def_kind(variant) == DefKind::Variant => tcx.local_parent(variant),
                adt => adt,
            }));
            let constant = matches!(tcx.hir_node(node), Node::Item(_) | Node::ImplItem(_) | Node::TraitItem(_))
                && matches!(tcx.def_kind(node.owner.def_id), DefKind::Const | DefKind::AssocConst | DefKind::Static { .. });
            let built = |declared: &str, constructor: Option<String>| {
                constructor.map_or_else(|| format!("declare `{ident}` as `{declared}`"), |constructor| format!("declare `{ident}` as `{declared}` and build it with `{constructor}`"))
            };
            let (help, note) = match domain {
                Domain::Time { clock, unit, float } if wire && let Some(encoded) = unit.unwrap_or(Unit::Second).encoded() => {
                    let encoding = if float.is_some() { String::from("WithFrac<f64>") } else { format!("<{}>", clock.whole(tcx)) };
                    (
                        format!("declare `{ident}` as `{}` with `#[serde_as(as = \"{clock}{encoded}{encoding}\")]`", <&str>::from(clock)),
                        unit.is_none().then(|| format!("`{clock}MilliSeconds`, `{clock}MicroSeconds`, and `{clock}NanoSeconds` encode other wire units")),
                    )
                }
                Domain::Time { clock, unit, float } => {
                    let constructor = match (clock, unit, float) {
                        (Clock::Duration, Some(Unit::Second), Some(float)) => Some(format!("Duration::from_secs_{}", float.name_str())),
                        (_, Some(unit), None) => unit.constructor(clock).map(String::from),
                        (_, None, _) | (Clock::Instant | Clock::Duration, Some(_), Some(_)) => None,
                    };
                    (built(clock.into(), constructor), None)
                }
                Domain::ByteSize { constructor } =>
                    (built("bytesize::ByteSize", Some(format!("ByteSize::{constructor}"))), wire.then(|| String::from("`ByteSize` serializes as text in human-readable formats"))),
                Domain::Path { .. } if constant => (format!("declare `{ident}` as `&std::path::Path` through `Path::new` under `#![feature(const_convert)]`"), None),
                Domain::Path { borrowed } => (built(if borrowed { "&std::path::Path" } else { "std::path::PathBuf" }, None), None),
                Domain::Calendar { civil } => (built(&format!("jiff::civil::{civil}"), None), None),
                Domain::Quantity { quantity, module, unit } => (built(&format!("uom::si::f64::{quantity}"), Some(format!("{quantity}::new::<uom::si::{module}::{unit}>"))), None),
            };
            let stated = match domain {
                Domain::Time { clock: Clock::Instant, .. } => "an instant",
                Domain::Time { clock: Clock::Duration, .. } => "a duration",
                Domain::ByteSize { .. } => "a byte size",
                Domain::Path { .. } => "a path",
                Domain::Calendar { .. } => "a calendar value",
                Domain::Quantity { .. } => "a physical quantity",
            };
            span_lint_hir_and_then(cx, PRIMITIVE_DOMAIN_BINDING, node, ident.span, format!("`{ident}` holds {stated} as `{held}`"), |diag| {
                diag.help(help);
                if let Some(note) = note {
                    diag.note(note);
                }
            });
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn declaration<'tcx>(cx: &LateContext<'tcx>, node: HirId, ident: Ident, held: Ty<'tcx>) -> Option<(HirId, Ident, Domain, String)> {
    Some((node, ident, Domain::of(cx, ident.name, held)?, with_forced_trimmed_paths!(held.to_string())))
}
