use core::{fmt, iter, mem};
use std::{borrow::Cow, io};

use itertools::{Itertools as _, Position};
use kurbo::{Affine, CubicBez, DEFAULT_ACCURACY, ParamCurve as _, ParamCurveArclen as _, ParamCurveExtrema as _, PathSeg, Point, Rect, Vec2};
use nonempty_collections::NEVec;
use petgraph::{
    EdgeType,
    dot::{Config, Dot},
};
use petgraph_graphml::GraphMl;

use crate::{Drawing, Format, LayoutError, Placed, Routed};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Attributes<'weight> = Vec<(Cow<'static, str>, Cow<'weight, str>)>;

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn write<N, E, Ty, W>(format: Format, node: impl Fn(&N) -> Attributes<'_>, edge: impl Fn(&E) -> Attributes<'_>, drawing: &Drawing<N, E, Ty>, mut writer: W) -> Result<(), LayoutError>
where
    Ty: EdgeType,
    W: io::Write,
{
    const POINTS_PER_INCH: f64 = 72.0;
    const ARROW_LENGTH: f64 = 10.0;
    const POSITION: &str = "pos";
    const WIDTH: &str = "width";
    const HEIGHT: &str = "height";
    fn merged(geometry: impl IntoIterator<Item = (&'static str, String)>, attributes: Attributes<'_>) -> Attributes<'_> {
        let mut kept = geometry.into_iter().filter(|&(key, _)| attributes.iter().all(|(name, _)| *name != key)).map(|(key, value)| (Cow::Borrowed(key), Cow::Owned(value))).collect::<Vec<_>>();
        kept.extend(attributes);
        kept
    }
    fn borrowed<'weight>(attributes: &'weight Attributes<'_>) -> Attributes<'weight> { attributes.iter().map(|(key, value)| (key.clone(), Cow::Borrowed(&**value))).collect() }
    let exported = match format {
        Format::GraphMl => {
            let corner = |keys: [&'static str; 4], rect: Rect| {
                let rect = rect.abs();
                iter::zip(keys, [rect.x0, rect.y0, rect.width(), rect.height()]).map(|(key, value)| (key, value.to_string()))
            };
            GraphMl::new(&drawing.map(
                |_, Placed { weight, frame }| merged(corner(["x", "y", WIDTH, HEIGHT], *frame), node(weight)),
                |_, Routed { weight, path, label }| {
                    merged(iter::once(("path", path.to_svg())).chain(label.iter().flat_map(|&rect| corner(["label_x", "label_y", "label_width", "label_height"], rect))), edge(weight))
                },
            ))
            .export_node_weights(Box::new(borrowed))
            .export_edge_weights(Box::new(borrowed))
            .to_writer(writer)
        }
        Format::Dot => {
            let bounds = drawing
                .node_weights()
                .map(|placed| placed.frame.abs())
                .chain(drawing.edge_weights().flat_map(|routed| routed.path.segments().map(|segment| segment.bounding_box()).chain(routed.label.map(|rect| rect.abs()))))
                .reduce(|whole, rect| whole.union(rect));
            let flip = bounds.map_or(Affine::IDENTITY, |Rect { x0: _, y0, x1: _, y1 }| Affine::FLIP_Y.then_translate(Vec2::new(0.0, y0 + y1)));
            let coordinates = |point: Point| {
                let Point { x, y } = flip * point;
                format!("{x},{y}")
            };
            let arrows = |attributes: &Attributes<'_>| {
                let value = |key: &str| attributes.iter().rev().find(|(name, _)| *name == key).map(|(_, value)| &**value);
                let unarrowed = |token: Option<&str>| token == Some("none");
                let (tail, head) = match value("dir") {
                    Some("forward") => (false, true),
                    Some("back") => (true, false),
                    Some("both") => (true, true),
                    dir if unarrowed(dir) => (false, false),
                    Some(_) | None => (false, drawing.is_directed()),
                };
                [(tail, "arrowtail"), (head, "arrowhead")].map(|(drawn, shape)| (drawn && !unarrowed(value(shape))).then_some(ARROW_LENGTH))
            };
            let spline = |segments: &NEVec<PathSeg>, [tail, head]: [Option<f64>; 2]| {
                let lengths = segments.iter().map(|segment| segment.arclen(DEFAULT_ACCURACY)).collect::<Vec<_>>();
                let total = lengths.iter().sum::<f64>();
                let fit = (total / (tail.unwrap_or_default() + head.unwrap_or_default())).min(1.0);
                let from = (tail.unwrap_or_default() * fit).min(total);
                let to = head.unwrap_or_default().mul_add(-fit, total).max(from);
                let parameter = |segment: &PathSeg, along: f64, length: f64| match along {
                    ..=0.0 => 0.0,
                    along if along >= length => 1.0,
                    along => segment.inv_arclen(along, DEFAULT_ACCURACY),
                };
                let cubics = iter::zip(segments.iter(), lengths.iter().scan(0.0, |start: &mut f64, &length| Some((mem::replace(start, *start + length), length))))
                    .filter_map(|(segment, (start, length))| {
                        let (low, high) = (from.max(start), to.min(start + length));
                        (low <= high).then(|| segment.subsegment(parameter(segment, low - start, length)..parameter(segment, high - start, length)).to_cubic())
                    })
                    .collect::<Vec<_>>();
                iter::chain(tail.map(|_| format!("s,{}", coordinates(segments.first().start()))), head.map(|_| format!("e,{}", coordinates(segments.last().end()))))
                    .chain(iter::chain(cubics.first().map(|cubic| cubic.p0), cubics.iter().flat_map(|&CubicBez { p0: _, p1, p2, p3 }| [p1, p2, p3])).map(coordinates))
                    .join(" ")
            };
            let quoted = |text: &str| {
                format!(
                    "\"{}\"",
                    text.chars().zip(text.chars().skip(1).map(Some).chain([None])).format_with("", |pair, sink| match pair {
                        ('"', _) => sink(&"\\\""),
                        ('\\', Some('"' | '\n') | None) => sink(&"\\\\"),
                        (character, _) => sink(&character),
                    })
                )
            };
            let listed = |attributes: &Attributes<'_>| attributes.iter().format_with(", ", |(key, value), sink| sink(&format_args!("{}={}", quoted(key), quoted(value)))).to_string();
            let attributed = drawing.map(
                |_, Placed { weight, frame }| {
                    merged(
                        [
                            (POSITION, coordinates(frame.center())),
                            (WIDTH, (frame.width().abs() / POINTS_PER_INCH).to_string()),
                            (HEIGHT, (frame.height().abs() / POINTS_PER_INCH).to_string()),
                            ("shape", "box".to_owned()),
                            ("fixedsize", "true".to_owned()),
                        ],
                        node(weight),
                    )
                },
                |_, Routed { weight, path, label }| {
                    let attributes = edge(weight);
                    let [tail, head] = arrows(&attributes);
                    let pos = path
                        .subpaths()
                        .filter_map(|elements| NEVec::try_from_vec(kurbo::segments(elements.iter().copied()).collect()))
                        .with_position()
                        .map(|(Position { is_first, is_last }, segments)| spline(&segments, [tail.filter(|_| is_first), head.filter(|_| is_last)]))
                        .join(";");
                    merged(iter::chain(Some((POSITION, pos)).filter(|(_, pos)| !pos.is_empty()), label.map(|rect| ("lp", coordinates(rect.center())))), attributes)
                },
            );
            write!(
                writer,
                "{kind} {{\n{bounds}{content}}}\n",
                kind = if drawing.is_directed() { "digraph" } else { "graph" },
                bounds = fmt::from_fn(|formatter| bounds.map_or(Ok(()), |Rect { x0, y0, x1, y1 }| writeln!(formatter, "    graph [bb=\"{x0},{y0},{x1},{y1}\"]"))),
                content = fmt::from_fn(|formatter| {
                    Dot::with_attr_getters(&attributed, &[Config::NodeNoLabel, Config::EdgeNoLabel, Config::GraphContentOnly], &|_, reference| listed(reference.weight()), &|_, (_, attributes)| {
                        listed(attributes)
                    })
                    .graph_fmt(formatter, |_, _| Ok(()), |_, _| Ok(()))
                }),
            )
        }
    };
    exported.map_err(|source| LayoutError::Export { format, source })
}
