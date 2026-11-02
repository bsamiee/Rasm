use core::iter;

use geo::{
    Buffer as _,
    buffer::{BufferStyle, LineJoin},
};
use kurbo::Join;
use linesweeper::{
    BinaryOp, FillRule,
    topology::{BinaryWindingNumber, OneOfTwo},
};

use crate::{LineworkError, MiterLimit, Region, Side, Tolerance, Width};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean<'tools>(target: &Region, tools: impl IntoIterator<Item = &'tools Region>, op: BinaryOp, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let tools = tools.into_iter().collect::<Vec<_>>();
    let inside = |winding: i32| FillRule::NonZero.is_inside(winding);
    crate::swept::<BinaryWindingNumber>(
        iter::chain(target.iter().map(|contour| (&contour.path, OneOfTwo::A)), tools.iter().flat_map(|tool| tool.iter().map(|contour| (&contour.path, OneOfTwo::B)))),
        tolerance,
        |&BinaryWindingNumber { shape_a, shape_b }| match op {
            BinaryOp::Union => inside(shape_a) || inside(shape_b),
            BinaryOp::Intersection => inside(shape_a) && inside(shape_b),
            BinaryOp::Difference => inside(shape_a) && !inside(shape_b),
            BinaryOp::Xor => inside(shape_a) != inside(shape_b),
        },
    )
}

pub fn offset(region: &Region, distance: Width, side: Side, join: Join, miter_limit: MiterLimit, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let distance = match side {
        Side::Outside => distance.into_inner(),
        Side::Inside => -distance.into_inner(),
    };
    let line_join = match join {
        Join::Miter => LineJoin::Miter(2.0_f64 * miter_limit.into_inner().recip().asin()),
        Join::Round => LineJoin::Round((8.0_f64 * tolerance.into_inner() / distance.abs()).sqrt()),
        Join::Bevel => LineJoin::Bevel,
    };
    Region::new(
        &crate::path(crate::polygons(region, tolerance).buffer_with_style(BufferStyle::new(distance).line_join(line_join)).into_iter().flat_map(|polygon| {
            let (exterior, interiors) = polygon.into_inner();
            iter::chain(iter::once(exterior), interiors)
        })),
        FillRule::NonZero,
        tolerance,
    )
}
