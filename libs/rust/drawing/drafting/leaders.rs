
use core::iter;

use kurbo::{Affine, BezPath, Line, PathSeg, Point, QuadSpline, Shape as _};
use nonempty_collections::NEVec;
use rasm_drawing::{Anchor, Baseline, Placement, Width};

use crate::{
    Content, DraftingError, Graphics, Head, Terminator, bounds, drawn, indexed, joined, polyline, stroked,
    text::{Frame, Typesetter},
    tipped, transformed,
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

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, leader: &Leader) -> Result<Graphics, NEVec<DraftingError>> {
    let Leader { legs, elbow, content, curve } = leader;
    let elbow = *placement * *elbow;
    let tips = legs.iter().map(|leg| *placement * leg.tip).collect::<Vec<_>>();
    let (side, anchor) = if tips.iter().map(|tip| elbow.x - tip.x).sum::<f64>() < 0.0 { (-1.0_f64, Anchor::End) } else { (1.0_f64, Anchor::Start) };
    let along = |distance: f64| Point::new(side.mul_add(distance, elbow.x), elbow.y);
    let end = along(style.leader.landing.into_inner());
    let underlined = match content {
        Content::Label(label) => label.frame == Some(Frame::Underline),
        Content::Symbol(_) | Content::Control(_) => false,
    };
    let (at, baseline) = if underlined { (Point::new(end.x, end.y - style.gap.into_inner()), Baseline::Bottom) } else { (end, Baseline::Middle) };
    let drawn = drawn(typesetter, style, placement, content, at, anchor, baseline)?;
    let extent = bounds(&drawn, Affine::IDENTITY);
    let [near, far] = match (drawn.masks.iter().all(|mask| mask.contours().is_empty()), anchor) {
        (true, Anchor::Start | Anchor::Middle | Anchor::End) => [end.x; 2],
        (false, Anchor::Start | Anchor::Middle) => [extent.x0, extent.x1],
        (false, Anchor::End) => [extent.x1, extent.x0],
    };
    let reach = end.x - near;
    let landing = if underlined { Line::new(elbow, Point::new(far + reach, end.y)) } else { Line::new(elbow, end) };
    let paths = iter::zip(legs.iter(), tips)
        .map(|(Leg { tip: _, vertices, head: _ }, tip)| {
            let points = vertices.iter().map(|vertex| *placement * *vertex);
            match curve {
                Curve::Straight => polyline(iter::once(tip).chain(points).chain(iter::once(elbow))),
                Curve::Spline => {
                    let approach = along(-0.5 * points.clone().next_back().unwrap_or(tip).distance(elbow));
                    BezPath::from_path_segments(QuadSpline::new(iter::once(tip).chain(points).chain([approach, elbow]).collect()).to_quads().map(PathSeg::Quad))
                }
            }
        })
        .collect::<Vec<_>>();
    let terminator = &style.leader.terminator;
    let heads = indexed(iter::zip(legs.iter(), &paths).map(|(leg, path)| tipped(terminator, leg.head.unwrap_or(terminator.head), path.segments(), style.tolerance).map_err(NEVec::from)))
        .map(|elements| Graphics { elements, .. });
    let outline = landing.path_elements(style.tolerance.into_inner()).chain(paths.iter().flat_map(BezPath::iter)).collect::<BezPath>();
    joined([stroked(outline, &style.pen, None)], [heads, transformed(drawn, Affine::translate((reach, 0.0)))])
}
