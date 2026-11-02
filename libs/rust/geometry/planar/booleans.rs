use cavalier_contours::polyline::{self, BooleanOp, BooleanResult, BooleanResultPline, PlineBooleanOptions, PlineSource as _};

use crate::{Closure, Polyline, Region, oriented, spans};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn boolean(left: &Polyline<{ Closure::Closed }>, right: &Polyline<{ Closure::Closed }>, operation: BooleanOp, options: &PlineBooleanOptions<'_, f64>) -> Region {
    let BooleanResult { pos_plines: bodies, neg_plines: holes, result_info: _ } =
        left.0.polyline.boolean_opt(&right.0.polyline, operation, &PlineBooleanOptions { pline1_aabb_index: Some(&left.0.spatial_index), ..*options });
    let spanning = |outlines: Vec<BooleanResultPline<polyline::Polyline<f64>>>| outlines.into_iter().map(|outline| outline.pline).filter(|outline| spans(outline, options.pos_equal_eps));
    oriented(spanning(bodies), spanning(holes))
}
