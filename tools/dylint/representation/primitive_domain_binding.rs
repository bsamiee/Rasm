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

#[derive(Clone, Copy, PartialEq, Eq, derive_more::Display, strum::IntoStaticStr)]
#[strum(serialize_all = "snake_case")]
enum Quantity {
    Length,
    Mass,
    Angle,
    Pressure,
    Power,
    Energy,
    Frequency,
    LinearNumberDensity,
    Ratio,
    ThermodynamicTemperature,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq)]
enum Domain {
    Time { clock: Clock, unit: Option<Unit>, float: Option<FloatTy> },
    ByteSize { constructor: &'static str },
    Path { borrowed: bool },
    Calendar { civil: &'static str },
    Quantity { quantity: Quantity, unit: &'static str, float: FloatTy },
}

impl Domain {
    fn of<'tcx>(cx: &LateContext<'tcx>, name: Symbol, held: Ty<'tcx>) -> Option<Self> {
        const EPOCH: &str = "epoch";
        const RATE: &str = "per";
        const COUNTS: &[&str] = &["samples", "dots", "pixels", "px", "lines", "cycles", "ticks", "cells"];
        const BYTE_SIZES: &[(&str, &str)] = &[("bytes", "b"), ("kb", "kb"), ("kib", "kib"), ("mb", "mb"), ("mib", "mib"), ("gb", "gb"), ("gib", "gib")];
        const QUANTITIES: &[(&str, bool, Quantity, &str)] = &[
            ("mm", true, Quantity::Length, "millimeter"),
            ("cm", true, Quantity::Length, "centimeter"),
            ("km", true, Quantity::Length, "kilometer"),
            ("ft", true, Quantity::Length, "foot"),
            ("kg", true, Quantity::Mass, "kilogram"),
            ("lbs", true, Quantity::Mass, "pound"),
            ("deg", true, Quantity::Angle, "degree"),
            ("rad", true, Quantity::Angle, "radian"),
            ("kpa", true, Quantity::Pressure, "kilopascal"),
            ("mpa", true, Quantity::Pressure, "megapascal"),
            ("psi", true, Quantity::Pressure, "pound_force_per_square_inch"),
            ("kw", true, Quantity::Power, "kilowatt"),
            ("kwh", true, Quantity::Energy, "kilowatt_hour"),
            ("hz", true, Quantity::Frequency, "hertz"),
            ("khz", true, Quantity::Frequency, "kilohertz"),
            ("mhz", true, Quantity::Frequency, "megahertz"),
            ("dpi", false, Quantity::LinearNumberDensity, "inch"),
            ("ppi", false, Quantity::LinearNumberDensity, "inch"),
            ("pct", true, Quantity::Ratio, "percent"),
            ("percent", false, Quantity::Ratio, "percent"),
            ("millimeters", false, Quantity::Length, "millimeter"),
            ("millimetres", false, Quantity::Length, "millimeter"),
            ("centimeters", false, Quantity::Length, "centimeter"),
            ("centimetres", false, Quantity::Length, "centimeter"),
            ("meters", false, Quantity::Length, "meter"),
            ("metres", false, Quantity::Length, "meter"),
            ("kilometers", false, Quantity::Length, "kilometer"),
            ("kilometres", false, Quantity::Length, "kilometer"),
            ("feet", false, Quantity::Length, "foot"),
            ("inch", false, Quantity::Length, "inch"),
            ("inches", false, Quantity::Length, "inch"),
            ("kilograms", false, Quantity::Mass, "kilogram"),
            ("degrees", false, Quantity::Angle, "degree"),
            ("radians", false, Quantity::Angle, "radian"),
            ("pascals", false, Quantity::Pressure, "pascal"),
            ("watts", false, Quantity::Power, "watt"),
            ("kilowatts", false, Quantity::Power, "kilowatt"),
            ("joules", false, Quantity::Energy, "joule"),
            ("celsius", false, Quantity::ThermodynamicTemperature, "degree_celsius"),
            ("kelvin", false, Quantity::ThermodynamicTemperature, "kelvin"),
            ("fahrenheit", false, Quantity::ThermodynamicTemperature, "degree_fahrenheit"),
            ("hertz", false, Quantity::Frequency, "hertz"),
        ];
        let name = name.as_str().to_lowercase();
        let mut segments = name.rsplit('_');
        let last = segments.next()?;
        let previous = segments.next();
        let float = if let ty::Float(float) = held.kind() { Some(*float) } else { None };
        let time = |clock, unit| held.is_numeric().then_some(Self::Time { clock, unit, float });
        let measured = |word: &str| QUANTITIES.iter().find(|&&(suffix, ..)| suffix == word);
        let stored = |quantity, unit| float.filter(|float| matches!(float, FloatTy::F32 | FloatTy::F64)).map(|float| Self::Quantity { quantity, unit, float });
        let time_unit = last.parse::<Unit>().ok();
        match (previous, last) {
            (Some(RATE), _) => measured(last)
                .filter(|&&(_, _, quantity, _)| quantity == Quantity::Length && segments.next().is_some_and(|word| COUNTS.contains(&word)))
                .and_then(|&(_, _, _, unit)| stored(Quantity::LinearNumberDensity, unit)),
            (_, "timestamp") | (Some("since"), EPOCH) | (Some("created" | "updated" | "modified" | "expires" | "issued" | "started" | "finished" | "deleted"), "at") => time(Clock::Instant, None),
            _ if time_unit.is_some() => time(if previous == Some(EPOCH) { Clock::Instant } else { Clock::Duration }, time_unit),
            (_, "timeout" | "interval" | "delay" | "ttl" | "duration" | "elapsed") => time(Clock::Duration, None),
            _ if let Some(&(_, constructor)) = BYTE_SIZES.iter().find(|&&(suffix, _)| suffix == last) => held.is_integral().then_some(Self::ByteSize { constructor }),
            (_, "path" | "dir" | "directory" | "file" | "folder") => is_text(cx, held).then_some(Self::Path { borrowed: held.is_ref() }),
            (_, "date") => is_text(cx, held).then_some(Self::Calendar { civil: "Date" }),
            (_, "time") => is_text(cx, held).then_some(Self::Calendar { civil: "Time" }),
            (_, "datetime") => is_text(cx, held).then_some(Self::Calendar { civil: "DateTime" }),
            _ => measured(last).filter(|&&(_, prefixed, ..)| previous.is_some() || !prefixed).and_then(|&(_, _, quantity, unit)| stored(quantity, unit)),
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
            && matches!(Domain::of(cx, cx.tcx.hir_name(read), typeck.expr_ty(expr)), Some(Domain::Path { borrowed: _ }))
            && let argument = argument()
            && get_parent_expr(cx, argument).is_some_and(|consumer| pathlike(typeck.expr_ty(consumer)) || parameter_of(cx, consumer, argument).is_some_and(bounded))
        {
            self.reached.insert(read);
        }
    }

    fn check_crate_post(&mut self, cx: &LateContext<'tcx>) {
        fn whole(clock: Clock, tcx: TyCtxt<'_>) -> Ty<'_> {
            match clock {
                Clock::Instant => tcx.types.i64,
                Clock::Duration => tcx.types.u64,
            }
        }

        const fn encoded(unit: Unit) -> Option<&'static str> {
            match unit {
                Unit::Second => Some("Seconds"),
                Unit::Millisecond => Some("MilliSeconds"),
                Unit::Microsecond => Some("MicroSeconds"),
                Unit::Nanosecond => Some("NanoSeconds"),
                Unit::Minute | Unit::Hour => None,
            }
        }

        const fn unit_constructor(clock: Clock, unit: Unit) -> Option<&'static str> {
            match (clock, unit) {
                (Clock::Instant, Unit::Second) => Some("Timestamp::from_second"),
                (Clock::Instant, Unit::Millisecond) => Some("Timestamp::from_millisecond"),
                (Clock::Instant, Unit::Microsecond) => Some("Timestamp::from_microsecond"),
                (Clock::Instant, Unit::Nanosecond) => Some("Timestamp::from_nanosecond"),
                (Clock::Instant, Unit::Minute | Unit::Hour) => None,
                (Clock::Duration, Unit::Second) => Some("Duration::from_secs"),
                (Clock::Duration, Unit::Millisecond) => Some("Duration::from_millis"),
                (Clock::Duration, Unit::Microsecond) => Some("Duration::from_micros"),
                (Clock::Duration, Unit::Nanosecond) => Some("Duration::from_nanos"),
                (Clock::Duration, Unit::Minute) => Some("Duration::from_mins"),
                (Clock::Duration, Unit::Hour) => Some("Duration::from_hours"),
            }
        }

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
                        .filter(|&(.., domain, _)| !matches!(domain, Domain::Quantity { quantity: _, unit: _, float: _ }) || !serialized(item))
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
            iter::chain(mem::take(&mut self.declared), items).filter(|&(node, _, domain, _)| !matches!(domain, Domain::Path { borrowed: _ }) || self.reached.contains(&node) || open(node))
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
                Domain::Time { clock, unit, float } if wire && let Some(encoded) = encoded(unit.unwrap_or(Unit::Second)) => {
                    let encoding = if float.is_some() { String::from("WithFrac<f64>") } else { format!("<{}>", whole(clock, tcx)) };
                    (
                        format!("declare `{ident}` as `{}` with `#[serde_as(as = \"{clock}{encoded}{encoding}\")]`", <&str>::from(clock)),
                        unit.is_none().then(|| format!("`{clock}MilliSeconds`, `{clock}MicroSeconds`, and `{clock}NanoSeconds` encode other wire units")),
                    )
                }
                Domain::Time { clock, unit, float } => {
                    let constructor = match float {
                        Some(float) => matches!((clock, unit), (Clock::Duration, Some(Unit::Second))).then(|| format!("Duration::from_secs_{}", float.name_str())),
                        None => unit.and_then(|unit| unit_constructor(clock, unit)).map(String::from),
                    };
                    (built(clock.into(), constructor), None)
                }
                Domain::ByteSize { constructor } =>
                    (built("bytesize::ByteSize", Some(format!("ByteSize::{constructor}"))), wire.then(|| String::from("`ByteSize` serializes as text in human-readable formats"))),
                Domain::Path { borrowed: _ } if constant => (format!("declare `{ident}` as `&std::path::Path` through `Path::new` under `#![feature(const_convert)]`"), None),
                Domain::Path { borrowed } => (built(if borrowed { "&std::path::Path" } else { "std::path::PathBuf" }, None), None),
                Domain::Calendar { civil } => (built(&format!("jiff::civil::{civil}"), None), None),
                Domain::Quantity { quantity, unit, float } => {
                    let declared = format!("uom::si::{}::{quantity}", float.name_str());
                    let measure = format!("uom::si::{}::{}{unit}", <&str>::from(quantity), if quantity == Quantity::LinearNumberDensity { "per_" } else { "" });
                    if constant {
                        (
                            format!("declare `{ident}` as `{declared}` through a `{quantity} {{ dimension: PhantomData, units: PhantomData, value }}` literal holding its value in base units"),
                            Some(format!("`{quantity}::new::<{measure}>` builds it outside a const context")),
                        )
                    } else {
                        (built(&declared, Some(format!("{quantity}::new::<{measure}>"))), None)
                    }
                }
            };
            let stated = match domain {
                Domain::Time { clock: Clock::Instant, unit: _, float: _ } => "an instant",
                Domain::Time { clock: Clock::Duration, unit: _, float: _ } => "a duration",
                Domain::ByteSize { constructor: _ } => "a byte size",
                Domain::Path { borrowed: _ } => "a path",
                Domain::Calendar { civil: _ } => "a calendar value",
                Domain::Quantity { quantity: _, unit: _, float: _ } => "a physical quantity",
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
