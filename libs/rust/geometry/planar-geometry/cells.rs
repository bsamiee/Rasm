use core::{borrow::Borrow, iter};

use cavalier_contours::{
    polyline::{BooleanOp, BooleanResult, PlineBooleanOptions, PlineSource as _, PlineVertex},
    shape_algorithms::{IndexedPolyline, Shape},
};
use itertools::{Either, Itertools as _, iproduct};
use nalgebra::{Point2, Vector2};
use spade::{DelaunayTriangulation, Triangulation as _};
use validated::Validated;

use crate::{Closure, PlanarError, Region, Tolerance, owners, position, resulting, surviving};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn voronoi<I>(sites: I, boundary: &Region, tolerance: Tolerance) -> Result<Vec<Region>, PlanarError>
where I: IntoIterator<Item: Borrow<Point2<f64>>> {
    let shape = boundary.as_ref();
    let sites = sites
        .into_iter()
        .enumerate()
        .map(|(site, point)| {
            let located = position(*point.borrow());
            spade::validate_vertex(&located).map(|()| located).map_err(|source| (site, source))
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(|sites| PlanarError::Insertion { sites })?;
    let bounds = sites
        .iter()
        .map(|site| Point2::new(site.x, site.y))
        .chain(shape.plines_index.bounds().into_iter().flat_map(|extent| [Point2::new(extent.min_x, extent.min_y), Point2::new(extent.max_x, extent.max_y)]));
    let Some((lower, upper)) = bounds.clone().reduce(|left, right| left.inf(&right)).zip(bounds.reduce(|left, right| left.sup(&right))) else {
        return Ok(Vec::new());
    };
    let center = nalgebra::center(&lower, &upper);
    let reach = 4.0 * nalgebra::distance(&center, &upper);
    let triangulation =
        DelaunayTriangulation::<spade::Point2<f64>>::bulk_load(sites.iter().copied().chain(iproduct!([-reach, reach], [-reach, reach]).map(|(x, y)| position(center + Vector2::new(x, y)))).collect())
            .map_err(|source| PlanarError::Extent { source })?;
    let located = sites.iter().map(|site| triangulation.locate_vertex(*site)).collect_vec();
    let claimed =
        located.iter().enumerate().filter_map(|(site, handle)| handle.map(|handle| (site, handle.fix().index()))).fold(vec![None; triangulation.num_vertices()], |mut claimed, (site, vertex)| {
            claimed[vertex].get_or_insert(site);
            claimed
        });
    let owners = owners(shape);
    let Shape { ccw_plines: bodies, cw_plines: holes, plines_index } = shape;
    let pos_equal_eps = tolerance.into_inner();
    let clip = |source: &IndexedPolyline<f64>, other: &IndexedPolyline<f64>, operation: BooleanOp| {
        let BooleanResult { pos_plines: kept, neg_plines: carved, result_info: _ } =
            source.polyline.boolean_opt(&other.polyline, operation, &PlineBooleanOptions { pline1_aabb_index: Some(&source.spatial_index), pos_equal_eps, collapsed_area_eps: None });
        (kept.into_iter().map(|piece| IndexedPolyline::new(piece.pline)).collect_vec(), carved.into_iter().map(|piece| piece.pline).collect_vec())
    };
    let clipped = |cell: IndexedPolyline<f64>| {
        let (near, nearby): (Vec<_>, Vec<_>) = cell
            .spatial_index
            .bounds()
            .into_iter()
            .flat_map(|extent| plines_index.query_iter(extent.min_x, extent.min_y, extent.max_x, extent.max_y))
            .partition_map(|index| index.checked_sub(bodies.len()).map_or(Either::Left(index), Either::Right));
        let carving = nearby.into_iter().filter_map(|hole| owners[hole].map(|owner| (owner, &holes[hole]))).into_group_map();
        let (kept, carved): (Vec<_>, Vec<_>) = near
            .into_iter()
            .map(|body| (body, &bodies[body]))
            .map(|(body, outline)| {
                let (inside, _) = clip(&cell, outline, BooleanOp::And);
                carving.get(&body).into_iter().flatten().fold((inside, Vec::new()), |(pieces, carved), hole| {
                    let (kept, cut): (Vec<_>, Vec<_>) = pieces.iter().map(|piece| clip(piece, hole, BooleanOp::Not)).unzip();
                    (kept.into_iter().flatten().collect(), iter::chain(carved, cut.into_iter().flatten()).collect())
                })
            })
            .unzip();
        resulting(kept.iter().flatten().map(|piece| piece.polyline.iter_vertexes()), carved.iter().flatten().map(|hole| hole.iter_vertexes()), tolerance)
    };
    located
        .into_iter()
        .enumerate()
        .map(|(site, handle)| {
            handle
                .filter(|handle| claimed[handle.fix().index()] == Some(site))
                .and_then(|handle| handle.as_voronoi_face().adjacent_edges().map(|edge| edge.from().position().map(|corner| PlineVertex::new(corner.x, corner.y, 0.0))).collect::<Option<Vec<_>>>())
                .map(|corners| surviving::<{ Closure::Closed }>(corners.into_iter(), tolerance))
                .transpose()?
                .flatten()
                .map_or_else(|| Ok(Region(Shape::empty())), clipped)
        })
        .collect()
}
