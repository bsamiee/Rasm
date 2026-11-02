use core::{iter, ops::ControlFlow, slice};

use kurbo::{Affine, BezPath, Line, ParamCurve as _, ParamCurveArclen as _, PathSeg, Point, QuadBez, Shape as _, Vec2};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_linework::{Anchor, Baseline, Element, Placement, transform};
use validated::Validated;

use crate::{
    AnnotationError, Graphics, Style, stroked, terminated,
    text::{Label, Typesetter, labelled},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Course {
    Straight,
    Curved,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Attachment {
    Middle,
    Underline,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Leader {
    pub tips: NEVec<Point>,
    pub elbow: Point,
    pub label: Label,
    pub course: Course = Course::Straight,
    pub attachment: Attachment = Attachment::Middle,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, leader: &Leader) -> Result<Graphics, NEVec<AnnotationError>> {
    let Leader { tips, elbow, label, course, attachment } = leader;
    let elbow = *placement * *elbow;
    let tips = tips.iter().map(|tip| *placement * *tip).collect::<Vec<_>>();
    let (side, anchor) = if tips.iter().map(|tip| elbow.x - tip.x).sum::<f64>() < 0.0 { (-1.0_f64, Anchor::End) } else { (1.0_f64, Anchor::Start) };
    let along = |distance: f64| Point::new(side.mul_add(distance, elbow.x), elbow.y);
    let end = along(style.landing.into_inner());
    let (at, baseline) = match attachment {
        Attachment::Middle => (end, Baseline::Middle),
        Attachment::Underline => (Point::new(end.x, end.y - style.gap.into_inner()), Baseline::Bottom),
    };
    let segments = tips
        .into_iter()
        .map(|tip| match course {
            Course::Straight => PathSeg::Line(Line::new(tip, elbow)),
            Course::Curved => PathSeg::Quad(QuadBez::new(tip, along(-0.5 * tip.distance(elbow)), elbow)),
        })
        .collect::<Vec<_>>();
    let (lettering, mask) = labelled(typesetter, style, label, at, Vec2::new(1.0, 0.0), anchor, baseline)?;
    let bounds = mask.bounding_box();
    let [near, far] = match (mask.is_empty(), anchor) {
        (true, Anchor::Start | Anchor::Middle | Anchor::End) => [end.x; 2],
        (false, Anchor::Start | Anchor::Middle) => [bounds.x0, bounds.x1],
        (false, Anchor::End) => [bounds.x1, bounds.x0],
    };
    let reach = end.x - near;
    let landing = match attachment {
        Attachment::Middle => Line::new(elbow, end),
        Attachment::Underline => Line::new(elbow, Point::new(far + reach, end.y)),
    };
    let shift = Placement::try_new(Affine::translate((reach, 0.0))).map_err(|error| nev![AnnotationError::from(error)])?;
    Validated::from(stroked(BezPath::from_path_segments(iter::once(PathSeg::Line(landing)).chain(segments.iter().copied())), &style.pen, None))
        .map4(
            segments
                .iter()
                .enumerate()
                .map(|(index, segment)| tipped(style, slice::from_ref(segment)).map_err(|error| AnnotationError::Item { index, errors: nev![error] }))
                .collect::<Validated<Vec<_>, _>>(),
            transform(lettering, shift).map_or_else(|errors| Validated::Fail(errors.into_nonempty_iter().map(AnnotationError::from).collect()), Validated::Good),
            Validated::from(mask.transform(shift).map_err(AnnotationError::from)),
            |stroke, heads, glyphs, mask| Graphics { elements: iter::once(stroke).chain(heads).chain(glyphs).collect(), mask },
        )
        .ok()
}

pub(crate) fn tipped(style: &Style, course: &[PathSeg]) -> Result<Element, AnnotationError> {
    let tolerance = style.tolerance.into_inner();
    let (first, last) = course.first().zip(course.last()).ok_or(AnnotationError::Degenerate)?;
    let base = course
        .iter()
        .try_fold(style.terminator.size.into_inner().max(tolerance), |left, segment| {
            let length = segment.arclen(tolerance);
            if left <= length { ControlFlow::Break(segment.eval(segment.inv_arclen(left, tolerance))) } else { ControlFlow::Continue(left - length) }
        })
        .break_value()
        .unwrap_or_else(|| last.end());
    let tip = first.start();
    terminated(style, tip, tip - base)
}
