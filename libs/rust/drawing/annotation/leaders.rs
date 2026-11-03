use core::iter;

use kurbo::{Affine, BezPath, Line, PathEl, Point, Shape as _, Vec2};
use nonempty_collections::NEVec;
use rasm_linework::{Anchor, Baseline, Placement, Width};
use validated::Validated;

use crate::{
    AnnotationError, Content, Graphics, Head, Terminator, bounds, indexed, polyline, stroked, symbols,
    text::{Frame, Typesetter, labelled},
    tipped, transformed, validated,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Curve {
    Straight,
    Spline,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Style {
    pub terminator: Terminator,
    pub landing: Width,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Leg {
    pub tip: Point,
    pub vertices: Vec<Point> = Vec::new(),
    pub head: Option<Head> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Leader {
    pub legs: NEVec<Leg>,
    pub elbow: Point,
    pub content: Content,
    pub curve: Curve = Curve::Straight,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, leader: &Leader) -> Result<Graphics, NEVec<AnnotationError>> {
    let Leader { legs, elbow, content, curve } = leader;
    let elbow = *placement * *elbow;
    let tips = legs.iter().map(|leg| *placement * leg.tip).collect::<Vec<_>>();
    let (side, anchor) = if tips.iter().map(|tip| elbow.x - tip.x).sum::<f64>() < 0.0 { (-1.0_f64, Anchor::End) } else { (1.0_f64, Anchor::Start) };
    let along = |distance: f64| Point::new(side.mul_add(distance, elbow.x), elbow.y);
    let end = along(style.leader.landing.into_inner());
    let underlined = match content {
        Content::Label(label) => label.frame == Some(Frame::Underline),
        Content::Symbol(_) => false,
    };
    let (at, baseline) = if underlined { (Point::new(end.x, end.y - style.gap.into_inner()), Baseline::Bottom) } else { (end, Baseline::Middle) };
    let drawn = match content {
        Content::Label(label) => labelled(typesetter, style, label, at, Vec2::new(1.0, 0.0), anchor, baseline)?,
        Content::Symbol(instance) => symbols::draw(typesetter, style, placement, &symbols::Instance { at: placement.inverse() * at, ..instance.clone() })?,
    };
    let extent = bounds(&drawn);
    let [near, far] = match (drawn.masks.iter().all(|mask| mask.is_empty()), anchor) {
        (true, Anchor::Start | Anchor::Middle | Anchor::End) => [end.x; 2],
        (false, Anchor::Start | Anchor::Middle) => [extent.x0, extent.x1],
        (false, Anchor::End) => [extent.x1, extent.x0],
    };
    let reach = end.x - near;
    let landing = if underlined { Line::new(elbow, Point::new(far + reach, end.y)) } else { Line::new(elbow, end) };
    let paths = iter::zip(legs.iter(), tips)
        .map(|(Leg { tip: _, vertices, head: _ }, tip)| {
            let points = vertices.iter().map(|vertex| *placement * *vertex).collect::<Vec<_>>();
            match curve {
                Curve::Straight => polyline(iter::once(tip).chain(points).chain(iter::once(elbow))),
                Curve::Spline => {
                    let approach = along(-0.5 * points.last().copied().unwrap_or(tip).distance(elbow));
                    let controls = points.into_iter().chain(iter::once(approach)).collect::<Vec<_>>();
                    iter::once(PathEl::MoveTo(tip))
                        .chain(controls.array_windows().map(|&[control, next]| PathEl::QuadTo(control, control.midpoint(next))))
                        .chain(controls.last().map(|&control| PathEl::QuadTo(control, elbow)))
                        .collect::<BezPath>()
                }
            }
        })
        .collect::<Vec<_>>();
    let heads = indexed(iter::zip(legs.iter(), &paths).map(|(leg, path)| {
        let terminator = leg.head.map_or_else(|| style.leader.terminator.clone(), |head| Terminator { head, ..style.leader.terminator.clone() });
        tipped(&terminator, &path.segments().collect::<Vec<_>>(), style.tolerance).map_err(NEVec::from)
    }));
    let outline = landing.path_elements(style.tolerance.into_inner()).chain(paths.iter().flat_map(BezPath::iter)).collect::<BezPath>();
    Validated::from(stroked(outline, &style.pen, None))
        .map3(validated(heads), validated(transformed(drawn, Affine::translate((reach, 0.0)))), |stroke, terminators, moved| {
            [Graphics { elements: iter::once(stroke).chain(terminators).collect(), masks: Vec::new() }, moved].into_iter().collect()
        })
        .ok()
}
