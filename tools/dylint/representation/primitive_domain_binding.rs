use clippy_utils::{
    diagnostics::span_lint_hir_and_then,
    get_enclosing_block, get_parent_expr, peel_hir_expr_while,
    res::{MaybeDef as _, MaybeResPath as _},
    sym,
};
use rustc_hir::{Expr, ExprKind, LetStmt, PatKind, def_id::DefId};
use rustc_lint::{LateContext, LateLintPass};
use rustc_middle::ty::Ty;

use crate::{any_expr, bounds_of, expanded_externally, is_text, parameter_of};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(PRIMITIVE_DOMAIN_BINDING: "`let` bindings of a number or text with a name stating a duration, instant, byte size, calendar value, physical quantity, or a path a filesystem API reads");

impl<'tcx> LateLintPass<'tcx> for Pass {
    fn check_local(&mut self, cx: &LateContext<'tcx>, local: &'tcx LetStmt<'tcx>) {
        const EPOCH: &str = "epoch";
        let typeck = cx.typeck_results();
        let pathlike = |held: Ty<'_>| matches!(held.peel_refs().opt_diag_name(cx), Some(sym::Path | sym::PathBuf | sym::OsStr));
        let bounded = |(callee, parameter): (DefId, Ty<'tcx>)| {
            bounds_of(cx.tcx, callee, parameter.peel_refs())
                .any(|bound| matches!(bound.def_id().opt_diag_name(cx), Some(sym::AsRef | sym::Into)) && pathlike(bound.skip_binder().trait_ref.args.type_at(1)))
        };
        let consumed =
            |argument: &'tcx Expr<'tcx>| get_parent_expr(cx, argument).is_some_and(|consumer| pathlike(typeck.expr_ty(consumer)) || parameter_of(cx, consumer, argument).is_some_and(bounded));
        let reaches = |used: &'tcx Expr<'tcx>| consumed(peel_hir_expr_while(used, |inner| get_parent_expr(cx, inner).filter(|outer| matches!(outer.kind, ExprKind::AddrOf(..)))));
        let filesystem = |binding| get_enclosing_block(cx, local.hir_id).is_some_and(|block| any_expr(cx.tcx, block, |used| used.res_local_id() == Some(binding) && reaches(used)));
        if let PatKind::Binding(_, binding, ident, None) = local.pat.kind
            && !expanded_externally(cx, local.span)
            && let held = typeck.node_type(binding)
            && let (number, text) = (held.is_numeric(), is_text(cx, held))
            && (number || text)
            && let (instant, duration) =
                ((number, "an instant", "bind a `jiff::Timestamp`"), (number, "a duration", "bind a `core::time::Duration` and convert where a protocol or API takes the number"))
            && let name = ident.name.as_str().to_lowercase()
            && let mut segments = name.rsplit('_')
            && let Some(last) = segments.next()
            && let Some((fits, stated, help)) = match (segments.next(), last) {
                (_, "timestamp") | (Some("since"), EPOCH) | (Some("created" | "updated" | "modified" | "expires" | "issued" | "started" | "finished" | "deleted"), "at") => Some(instant),
                (previous, "secs" | "seconds" | "ms" | "millis" | "milliseconds" | "us" | "micros" | "ns" | "nanos" | "mins" | "minutes" | "hours") =>
                    Some(if previous == Some(EPOCH) { instant } else { duration }),
                (_, "timeout" | "interval" | "delay" | "ttl" | "duration" | "elapsed") => Some(duration),
                (_, "bytes" | "kb" | "kib" | "mb" | "mib" | "gb" | "gib") => Some((held.is_integral(), "a byte size", "bind a `bytesize::ByteSize`")),
                (_, "path" | "dir" | "directory" | "file" | "folder") => Some((text && filesystem(binding), "a path", "bind a `PathBuf` or `&Path`")),
                (_, "date" | "time" | "datetime") => Some((text, "a calendar value", "bind a `jiff::civil::Date`, `Time`, or `DateTime`")),
                (Some(_), "mm" | "km" | "ft" | "kg" | "lbs" | "deg" | "rad" | "kpa" | "mpa" | "psi" | "kw" | "kwh" | "hz" | "khz" | "mhz" | "pct")
                | (
                    _,
                    "percent" | "millimeters" | "millimetres" | "centimeters" | "centimetres" | "meters" | "metres" | "kilometers" | "kilometres" | "feet" | "inches" | "kilograms" | "degrees"
                    | "radians" | "pascals" | "watts" | "kilowatts" | "joules" | "celsius" | "kelvin" | "fahrenheit" | "hertz",
                ) => Some((
                    held == cx.tcx.types.f64,
                    "a physical quantity",
                    "bind a `uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)",
                )),
                _ => None,
            }
            && fits
        {
            span_lint_hir_and_then(cx, PRIMITIVE_DOMAIN_BINDING, local.hir_id, local.pat.span, format!("`{ident}` holds {stated} as `{held}`"), |diag| {
                diag.help(help);
            });
        }
    }
}
