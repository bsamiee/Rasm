use cavalier_contours::{
    polyline::{PlineOffsetOptions, PlineOrientation, PlineSource as _, TouchingLoopBehavior},
    shape_algorithms::{IndexedPolyline, ShapeOffsetOptions},
};

use crate::{Closure, Polyline, Region, spans};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Offset(f64);

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn parallel<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, offset: Offset, options: &PlineOffsetOptions<'_, f64>) -> Vec<Polyline<CLOSURE>> {
    let IndexedPolyline { polyline: source, spatial_index } = polyline.as_ref();
    let orientation = source.orientation();
    let inward = (offset.into_inner() > 0.0) == (orientation == PlineOrientation::CounterClockwise);
    let touching_loop_behavior = match CLOSURE {
        Closure::Open => options.touching_loop_behavior,
        Closure::Closed => TouchingLoopBehavior::Separate,
    };
    source
        .parallel_offset_opt(offset.into_inner(), &PlineOffsetOptions { aabb_index: Some(spatial_index), handle_self_intersects: false, touching_loop_behavior, ..*options })
        .into_iter()
        .filter(|outline| match CLOSURE {
            Closure::Open => true,
            Closure::Closed => (!inward || outline.orientation() == orientation) && spans(outline, options.pos_equal_eps),
        })
        .map(|outline| Polyline(IndexedPolyline::new(outline)))
        .collect()
}

#[must_use]
pub fn region(region: &Region, offset: Offset, options: &ShapeOffsetOptions<f64>) -> Region { Region(region.as_ref().parallel_offset(offset.into_inner(), options)) }
