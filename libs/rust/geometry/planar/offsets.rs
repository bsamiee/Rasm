use core::iter;

use cavalier_contours::{
    polyline::{self, CoincidentSegmentBehavior, PlineOffsetOptions, PlineOrientation, PlineSource as _, TouchingLoopBehavior},
    shape_algorithms::{IndexedPolyline, OffsetLoop, Shape, ShapeOffsetOptions},
};
use itertools::Itertools as _;

use crate::{Closure, Distance, PlanarError, Polyline, Region, Tolerance, measured, resulting, surviving};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn parallel(
    polyline: &Polyline<{ Closure::Open }>,
    distance: Distance,
    tolerance: Tolerance,
    touching_loop_behavior: TouchingLoopBehavior,
    coincident_segment_behavior: CoincidentSegmentBehavior,
) -> Result<Vec<Polyline<{ Closure::Open }>>, PlanarError> {
    offset(&polyline.outline, distance, tolerance, touching_loop_behavior, coincident_segment_behavior)
        .iter()
        .filter_map(|outline| surviving::<{ Closure::Open }>(outline.iter_vertexes(), tolerance).map(|kept| kept.map(measured)).transpose())
        .collect()
}

pub fn region(region: &Region, distance: Distance, tolerance: Tolerance) -> Result<Region, PlanarError> {
    let shape = region.as_ref();
    let Shape { ccw_plines: bodies, cw_plines: holes, plines_index: _ } = shape;
    let (reach, signed) = (tolerance.into_inner(), distance.into_inner());
    let (positive, negative): (Vec<_>, Vec<_>) = iter::chain(bodies, holes)
        .enumerate()
        .flat_map(|(parent, ring)| {
            let role = if parent < bodies.len() { PlineOrientation::CounterClockwise } else { PlineOrientation::Clockwise };
            let shrinking = (signed > 0.0) == (role == PlineOrientation::CounterClockwise);
            offset(ring, distance, tolerance, TouchingLoopBehavior::Separate, CoincidentSegmentBehavior::Preserve)
                .into_iter()
                .filter(move |outline| outline.vertex_count() > 1 && (outline.orientation() == role || !shrinking))
                .map(move |outline| (parent, outline))
        })
        .partition(|(_, outline)| outline.orientation() == PlineOrientation::CounterClockwise);
    let ((grown_parents, grown), (carved_parents, carved)): ((Vec<_>, Vec<_>), (Vec<_>, Vec<_>)) = (positive.into_iter().unzip(), negative.into_iter().unzip());
    let Shape { ccw_plines: grown, cw_plines: carved, plines_index } = Shape::from_plines(iter::chain(grown, carved));
    let looped = |origins: Vec<usize>, loops: Vec<IndexedPolyline<f64>>| iter::zip(origins, loops).map(|(parent_loop_idx, indexed_pline)| OffsetLoop { parent_loop_idx, indexed_pline }).collect_vec();
    let (grown, carved) = (looped(grown_parents, grown), looped(carved_parents, carved));
    let crossings = shape.find_intersects_between_offset_loops(&grown, &carved, &plines_index, reach);
    let slices = shape.create_valid_slices_from_intersects(&grown, &carved, &crossings, signed, &ShapeOffsetOptions { pos_equal_eps: reach, offset_dist_eps: reach, slice_join_eps: reach });
    let Shape { ccw_plines: stitched, cw_plines: cut, plines_index: _ } = shape.stitch_slices_together(&slices, &grown, &carved, reach, reach);
    resulting(stitched.iter().map(|outline| outline.polyline.iter_vertexes()), cut.iter().map(|outline| outline.polyline.iter_vertexes()), tolerance)
}

fn offset(
    outline: &IndexedPolyline<f64>,
    distance: Distance,
    tolerance: Tolerance,
    touching_loop_behavior: TouchingLoopBehavior,
    coincident_segment_behavior: CoincidentSegmentBehavior,
) -> Vec<polyline::Polyline<f64>> {
    outline.polyline.parallel_offset_opt(
        distance.into_inner(),
        &PlineOffsetOptions {
            aabb_index: Some(&outline.spatial_index),
            handle_self_intersects: false,
            pos_equal_eps: tolerance.into_inner(),
            offset_dist_eps: tolerance.into_inner(),
            touching_loop_behavior,
            coincident_segment_behavior,
        },
    )
}
