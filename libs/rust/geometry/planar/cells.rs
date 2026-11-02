use core::borrow::Borrow;
use std::collections::HashSet;

use cavalier_contours::{
    polyline::{self, BooleanOp, PlineBooleanOptions, PlineCreation as _, PlineSource as _, PlineVertex},
    shape_algorithms::{IndexedPolyline, Shape},
};
use itertools::{Itertools as _, iproduct};
use nalgebra::{Point2, Vector2};
use nonempty_collections::{NEVec, NonEmptyIterator as _};
use spade::{DelaunayTriangulation, Triangulation as _};

use crate::{Closure, PlanarError, Polyline, Region, booleans};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn voronoi<I>(sites: I, boundary: &Polyline<{ Closure::Closed }>, options: &PlineBooleanOptions<'_, f64>) -> Result<Vec<Region>, PlanarError>
where I: IntoIterator<Item: Borrow<Point2<f64>>> {
    let Some(sites) = NEVec::try_from_vec(sites.into_iter().map(|site| *site.borrow()).collect()) else {
        return Ok(Vec::new());
    };
    let vertex = |site: Point2<f64>| spade::mitigate_underflow(<[f64; 2]>::from(site).into());
    let extent = boundary.0.polyline.extents().map(|extents| [Point2::new(extents.min_x, extents.min_y), Point2::new(extents.max_x, extents.max_y)]);
    let bounds = || sites.nonempty_iter().copied().chain(extent.into_iter().flatten());
    let lower = bounds().reduce(|left, right| left.inf(&right));
    let upper = bounds().reduce(|left, right| left.sup(&right));
    let center = nalgebra::center(&lower, &upper);
    let reach = 4.0 * nalgebra::distance(&center, &upper);
    let triangulation = DelaunayTriangulation::<spade::Point2<f64>>::bulk_load(
        sites.iter().copied().chain(iproduct!([-reach, reach], [-reach, reach]).map(|(x, y)| center + Vector2::new(x, y))).map(vertex).collect(),
    )?;
    let located = sites.iter().map(|site| triangulation.locate_vertex(vertex(*site))).collect::<Vec<_>>();
    let owners = located.iter().enumerate().unique_by(|(_, handle)| handle.map(|handle| handle.fix())).map(|(index, _)| index).collect::<HashSet<_>>();
    Ok(located
        .into_iter()
        .enumerate()
        .map(|(index, handle)| {
            handle
                .filter(|_| owners.contains(&index))
                .and_then(|handle| handle.as_voronoi_face().adjacent_edges().map(|edge| edge.from().position().map(|corner| PlineVertex::new(corner.x, corner.y, 0.0))).collect::<Option<Vec<_>>>())
                .map_or_else(
                    || Region(Shape::empty()),
                    |corners| {
                        let cell = polyline::Polyline::from_iter(corners.into_iter(), true);
                        booleans::boolean(boundary, &Polyline(IndexedPolyline::new(cell.remove_repeat_pos(options.pos_equal_eps).unwrap_or(cell))), BooleanOp::And, options)
                    },
                )
        })
        .collect())
}
