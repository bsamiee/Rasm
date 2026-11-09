use kurbo::{BezPath, Join, Shape as _, Stroke, StrokeOpts, stroke};
use linesweeper::{BinaryOp, FillRule, binary_op_with_eps};

use crate::{LineworkError, MiterLimit, Region, Side, Tolerance, Width};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean<'tools>(target: &Region, tools: impl IntoIterator<Item = &'tools Region>, op: BinaryOp, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let tools = tools.into_iter().flat_map(|tool| tool.path_elements(0.0)).collect::<BezPath>();
    binary_op_with_eps(&target.path_elements(0.0).collect::<BezPath>(), &tools, FillRule::NonZero, op, tolerance.into_inner())
        .map(|contours| Region(contours.into_contours().collect()))
        .map_err(LineworkError::from)
}

pub fn offset(region: &Region, distance: Width, side: Side, join: Join, miter_limit: MiterLimit, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let band = Region::new(
        stroke(region.path_elements(0.0), &Stroke::new(2.0_f64 * distance.into_inner()).with_join(join).with_miter_limit(miter_limit.into_inner()), &StrokeOpts::default(), tolerance.into_inner()),
        FillRule::NonZero,
        tolerance,
    )?;
    boolean(
        region,
        [&band],
        match side {
            Side::Outside => BinaryOp::Union,
            Side::Inside => BinaryOp::Difference,
        },
        tolerance,
    )
}
