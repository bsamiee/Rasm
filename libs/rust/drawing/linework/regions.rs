use std::iter;

use geo::{
    Buffer as _,
    buffer::{BufferStyle, LineJoin},
};
use geo_types::{LineString, MultiPolygon, Polygon};
use kurbo::{BezPath, Join};
use linesweeper::{
    BinaryOp, FillRule,
    topology::{BinaryWindingNumber, Contour, ContourIdx, OneOfTwo},
};

use crate::{LineworkError, Region, Side, Tolerance, Width};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn boolean<'tools>(target: &Region, tools: impl IntoIterator<Item = &'tools Region>, op: BinaryOp, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let inside = |winding: i32| FillRule::NonZero.is_inside(winding);
    crate::swept::<BinaryWindingNumber>(
        iter::chain(target.iter().map(|contour| (&contour.path, OneOfTwo::A)), tools.into_iter().flat_map(|tool| tool.iter().map(|contour| (&contour.path, OneOfTwo::B)))).collect(),
        tolerance,
        |&BinaryWindingNumber { shape_a, shape_b }| match op {
            BinaryOp::Union => inside(shape_a) || inside(shape_b),
            BinaryOp::Intersection => inside(shape_a) && inside(shape_b),
            BinaryOp::Difference => inside(shape_a) && !inside(shape_b),
            BinaryOp::Xor => inside(shape_a) != inside(shape_b),
        },
    )
}

pub fn offset(region: &Region, distance: Width, side: Side, join: Join, miter_limit: f64, tolerance: Tolerance) -> Result<Region, LineworkError> {
    let distance = match side {
        Side::Outside => distance.into_inner(),
        Side::Inside => -distance.into_inner(),
    };
    let line_join = match join {
        Join::Miter if miter_limit >= 1.0_f64 => LineJoin::Miter(2.0_f64 * miter_limit.recip().asin()),
        Join::Round => LineJoin::Round((8.0_f64 * tolerance.into_inner() / distance.abs()).sqrt()),
        Join::Miter | Join::Bevel => LineJoin::Bevel,
    };
    region.iter().try_for_each(|contour| crate::finite(&contour.path))?;
    Region::new(
        &crate::path(polygons(region, tolerance).buffer_with_style(BufferStyle::new(distance).line_join(line_join)).into_iter().flat_map(|polygon| {
            let (exterior, interiors) = polygon.into_inner();
            iter::chain(iter::once(exterior), interiors)
        })),
        FillRule::NonZero,
        tolerance,
    )
}

pub(crate) fn polygons(region: &Region, tolerance: Tolerance) -> MultiPolygon<f64> {
    let ring = |path: &BezPath| crate::flattened(path, tolerance).into_iter().flat_map(|line| line.0).collect::<LineString<f64>>();
    let holes = region.iter().fold(vec![Vec::new(); region.len()], |mut holes, contour| match contour {
        Contour { path, parent: Some(ContourIdx(parent)), outer: false } => {
            holes.get_mut(*parent).into_iter().for_each(|group| group.push(ring(path)));
            holes
        }
        Contour { path: _, parent: _, outer: _ } => holes,
    });
    region.iter().zip(holes).filter_map(|(contour, holes)| contour.outer.then(|| Polygon::new(ring(&contour.path), holes))).collect()
}
