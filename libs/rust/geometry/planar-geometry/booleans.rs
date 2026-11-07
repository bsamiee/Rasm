use cavalier_contours::{
    polyline::{BooleanOp, BooleanResult, PlineBooleanOptions, PlineSource as _},
    shape_algorithms::IndexedPolyline,
};

use crate::{Closure, PlanarError, Polyline, Region, Tolerance, resulting};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean(left: &Polyline<{ Closure::Closed }>, right: &Polyline<{ Closure::Closed }>, operation: BooleanOp, tolerance: Tolerance) -> Result<Region, PlanarError> {
    let IndexedPolyline { polyline: first, spatial_index } = left.as_ref();
    let BooleanResult { pos_plines: bodies, neg_plines: holes, result_info: _ } =
        first.boolean_opt(&right.outline.polyline, operation, &PlineBooleanOptions { pline1_aabb_index: Some(spatial_index), pos_equal_eps: tolerance.into_inner(), collapsed_area_eps: None });
    resulting(bodies.iter().map(|body| body.pline.iter_vertexes()), holes.iter().map(|hole| hole.pline.iter_vertexes()), tolerance)
}
