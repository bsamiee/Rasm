use core::{fmt, iter, mem};
use std::{borrow::Cow, io};

use itertools::{Itertools as _, Position};
use kurbo::{Affine, CubicBez, DEFAULT_ACCURACY, ParamCurve as _, ParamCurveArclen as _, PathSeg, Point, Rect, Vec2};
use nonempty_collections::{IntoIteratorExt as _, NEVec, NonEmptyIterator};
use petgraph::{
    EdgeType,
    stable_graph::EdgeIndex,
    visit::{EdgeRef as _, IntoEdgeReferences as _, IntoNodeReferences as _},
};
use petgraph_graphml::GraphMl;
use uom::si::{
    f64::Length,
    length::{inch, point_computer},
};

use crate::{Drawing, LayoutError, Placed, Routed};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Attributes<'weight> = Vec<(Cow<'static, str>, Cow<'weight, str>)>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, derive_more::Display)]
pub enum Format {
    #[display("GraphML")]
    GraphMl,
    #[display("DOT")]
    Dot(Length),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn write<N, E, Ty, W>(format: Format, node: impl Fn(&N) -> Attributes<'_>, edge: impl Fn(&E) -> Attributes<'_>, drawing: &Drawing<N, E, Ty>, mut writer: W) -> Result<(), LayoutError>
where
    Ty: EdgeType,
    W: io::Write,
{
    fn borrowed<'weight>(attributes: &'weight Attributes<'_>) -> Attributes<'weight> { attributes.iter().map(|(key, value)| (key.clone(), Cow::Borrowed(&**value))).collect() }
    match format {
        Format::GraphMl => {
            let corner = |keys: [&'static str; 4], rect: Rect| iter::zip(keys, [rect.x0, rect.y0, rect.width(), rect.height()]).map(|(key, value)| (key, value.to_string()));
            let labeled = move |label: Option<Rect>| label.into_iter().flat_map(move |rect| corner(["label_x", "label_y", "label_width", "label_height"], rect));
            GraphMl::new(&drawing.graph.map(
                |_, Placed { weight, frame, shape, parent, label }| {
                    merged(
                        corner(["x", "y", "width", "height"], *frame)
                            .chain(iter::once(("shape", shape.to_string())))
                            .chain(parent.map(|cluster| ("parent", format!("n{}", cluster.index()))))
                            .chain(labeled(*label)),
                        node(weight),
                    )
                },
                |_, Routed { weight, path, label }| merged(iter::once(("path", path.to_svg())).chain(labeled(*label)), edge(weight)),
            ))
            .export_node_weights(Box::new(borrowed))
            .export_edge_weights(Box::new(borrowed))
            .to_writer(writer)
        }
        Format::Dot(unit) => {
            let document = dot(unit, node, edge, drawing)?;
            write!(writer, "{document}")
        }
    }
    .map_err(|source| LayoutError::Export { format, source })
}

fn merged(geometry: impl IntoIterator<Item = (&'static str, String)>, attributes: Attributes<'_>) -> Attributes<'_> {
    let mut kept = geometry.into_iter().filter(|&(key, _)| attributes.iter().all(|(name, _)| *name != key)).map(|(key, value)| (Cow::Borrowed(key), Cow::Owned(value))).collect::<Vec<_>>();
    kept.extend(attributes);
    kept
}

fn dot<N, E, Ty>(unit: Length, node: impl Fn(&N) -> Attributes<'_>, edge: impl Fn(&E) -> Attributes<'_>, drawing: &Drawing<N, E, Ty>) -> Result<impl fmt::Display, LayoutError>
where Ty: EdgeType {
    const ARROW_LENGTH: f64 = 10.0;
    const ARROW_NORMAL: (&str, f64) = ("normal", 1.0);
    const DIRECTIONS: [(&str, [bool; 2]); 4] = [("forward", [false, true]), ("back", [true, false]), ("both", [true, true]), ("none", [false, false])];
    const POS: &str = "pos";
    fn quoted(text: &str) -> impl fmt::Display {
        fmt::from_fn(move |formatter| {
            write!(
                formatter,
                "\"{}\"",
                text.chars()
                    .zip(text.chars().skip(1).map(Some).chain([None]))
                    .scan(false, |odd, (character, next)| {
                        *odd = character == '\\' && !*odd;
                        Some((character, next, *odd))
                    })
                    .format_with("", |escaped, sink| match escaped {
                        ('"', _, _) => sink(&"\\\""),
                        ('\\', Some('"' | '\n') | None, true) => sink(&"\\\\"),
                        (character, _, _) => sink(&character),
                    })
            )
        })
    }
    fn listed(attributes: &Attributes<'_>) -> impl fmt::Display { attributes.iter().format_with(", ", |(key, value), sink| sink(&format_args!("{}={}", quoted(key), quoted(value)))) }
    fn arrow(name: &str) -> f64 {
        const ARROW_GAP: (&str, f64) = ("none", 0.5);
        const ARROW_MODIFIERS: [&str; 5] = ["o", "l", "r", "e", "half"];
        const ARROW_SHAPES: [(&str, f64); 14] = [
            ("invempty", 1.0),
            ARROW_NORMAL,
            ("crow", 1.0),
            ("tee", 0.5),
            ("box", 1.0),
            ("diamond", 1.2),
            ("dot", 0.8),
            ARROW_GAP,
            ("inv", 1.0),
            ("vee", 1.0),
            ("pen", 1.0),
            ("mpty", 1.0),
            ("curve", 1.0),
            ("icurve", 1.0),
        ];
        iter::successors(Some((name, ("", 0.0))), |&(rest, _)| {
            let unmodified = iter::successors(Some(rest), |rest| ARROW_MODIFIERS.into_iter().find_map(|modifier| rest.strip_prefix(modifier))).last()?;
            ARROW_SHAPES
                .into_iter()
                .find_map(|(shape, factor)| Some((unmodified.strip_prefix(shape)?, (shape, factor))))
                .or_else(|| (unmodified.len() < rest.len()).then_some((unmodified, ARROW_NORMAL)))
        })
        .skip(1)
        .take(4)
        .with_position()
        .enumerate()
        .map(|(position, (Position { is_first: _, is_last }, (_, (shape, factor))))| match (shape == ARROW_GAP.0, position, is_last) {
            (true, 3, _) | (true, 0, true) => 0.0,
            _ => factor,
        })
        .sum()
    }
    let directed = drawing.graph.is_directed();
    let (points, inches) = (unit.get::<point_computer>(), unit.get::<inch>());
    let Rect { x0: _, y0, x1: _, y1 } = drawing.canvas;
    let placement = Affine::FLIP_Y.then_translate(Vec2::new(0.0, y0 + y1)).then_scale(points);
    let coordinates = move |point: Point| {
        let Point { x, y } = placement * point;
        format!("{x},{y}")
    };
    let arrows = |index: EdgeIndex, attributes: &Attributes<'_>| -> Result<[Option<f64>; 2], LayoutError> {
        let value = |key: &str| attributes.iter().rev().find(|(name, _)| *name == key).map(|(_, value)| &**value);
        let length = value("arrowsize").map(|size| size.parse::<f64>().map_err(|source| LayoutError::Arrowsize { edge: index, value: size.to_owned(), source })).transpose()?.unwrap_or(1.0)
            * ARROW_LENGTH
            / points;
        let [tail, head] = value("dir").and_then(|dir| DIRECTIONS.into_iter().find_map(|(name, drawn)| (name == dir).then_some(drawn))).unwrap_or([false, directed]);
        Ok([(tail, "arrowtail"), (head, "arrowhead")]
            .map(|(drawn, key)| Some(length * arrow(value(key).filter(|name| !name.is_empty()).unwrap_or(ARROW_NORMAL.0))).filter(|&clip| drawn && clip > 0.0)))
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
        let cubics = iter::zip(segments.iter(), lengths.iter().scan(0.0, |start: &mut f64, &length| Some((mem::replace(start, *start + length), length)))).filter_map(|(segment, (start, length))| {
            let (low, high) = (from.max(start), to.min(start + length));
            (low <= high).then(|| segment.subsegment(parameter(segment, low - start, length)..parameter(segment, high - start, length)).to_cubic())
        });
        iter::chain(tail.map(|_| format!("s,{}", coordinates(segments.first().start()))), head.map(|_| format!("e,{}", coordinates(segments.last().end()))))
            .chain(cubics.with_position().flat_map(|(Position { is_first, is_last: _ }, CubicBez { p0, p1, p2, p3 })| iter::chain(is_first.then_some(p0), [p1, p2, p3])).map(coordinates))
            .join(" ")
    };
    let depth = move |placed: &Placed<N>| iter::successors(placed.parent, |&parent| drawing.graph.node_weight(parent).and_then(|cluster| cluster.parent)).count();
    let bounds = placement.transform_rect_bbox(drawing.canvas);
    let edges = drawing
        .graph
        .edge_references()
        .map(|reference| {
            let Routed { weight, path, label } = reference.weight();
            let given = edge(weight);
            let [tail, head] = arrows(reference.id(), &given)?;
            let pos = path
                .subpaths()
                .filter_map(|elements| kurbo::segments(elements.iter().copied()).try_into_nonempty_iter().map(NonEmptyIterator::collect::<NEVec<_>>))
                .with_position()
                .map(|(Position { is_first, is_last }, segments)| spline(&segments, [tail.filter(|_| is_first), head.filter(|_| is_last)]))
                .join(";");
            Ok((reference, merged(iter::chain(Some((POS, pos)).filter(|(_, pos)| !pos.is_empty()), label.map(|rect| ("lp", coordinates(rect.center())))), given)))
        })
        .collect::<Result<Vec<_>, LayoutError>>()?;
    Ok(fmt::from_fn(move |formatter| {
        writeln!(formatter, "{} {{", if directed { "digraph" } else { "graph" })?;
        writeln!(formatter, "    graph [bb=\"{},{},{},{}\"]", bounds.x0, bounds.y0, bounds.x1, bounds.y1)?;
        drawing.graph.node_references().sorted_by_cached_key(|&(_, placed)| depth(placed)).try_for_each(|(index, Placed { weight, frame, shape, parent: _, label: _ })| {
            let attributes = merged(
                [
                    (POS, coordinates(frame.center())),
                    ("width", (frame.width() * inches).to_string()),
                    ("height", (frame.height() * inches).to_string()),
                    ("shape", shape.to_string()),
                    ("fixedsize", "true".to_owned()),
                ],
                node(weight),
            );
            writeln!(formatter, "    {} [{}]", index.index(), listed(&attributes))
        })?;
        edges.iter().try_for_each(|(reference, attributes)| {
            writeln!(formatter, "    {} {} {} [{}]", reference.source().index(), if directed { "->" } else { "--" }, reference.target().index(), listed(attributes))
        })?;
        formatter.write_str("}\n")
    }))
}
