use core::{cmp, iter};

use cavalier_contours::{
    core::math::{self, Vector2},
    polyline::{
        self, FindIntersectsOptions, PlineBasicIntersect, PlineIntersectsCollection, PlineSource as _, PlineVertex, PlineViewData, SplitResult,
        internal::pline_intersects::{OverlappingSlice, sort_and_join_overlapping_intersects},
    },
    shape_algorithms::IndexedPolyline,
    static_aabb2d_index::Control,
};
use itertools::{Either, Itertools as _};
use nalgebra::{Point2, UnitVector2};

use crate::{Closure, Distance, PlanarError, Polyline, Region, Tolerance, boundary, measured, segment, surviving, winding_number};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Station<const N: usize> {
    pub point: Point2<f64>,
    pub lengths: [f64; N],
    pub tangents: [UnitVector2<f64>; N],
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Clearance {
    pub point: Point2<f64>,
    pub tangent: UnitVector2<f64>,
    pub distance: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn closest<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, point: Point2<f64>, tolerance: Tolerance) -> Station<1> {
    let (index, near, _) = nearest(&polyline.outline, Vector2::new(point.x, point.y), tolerance);
    let (length, tangent) = along(polyline, index, near, tolerance);
    Station { point: Point2::new(near.x, near.y), lengths: [length], tangents: [tangent] }
}

#[must_use]
pub fn station<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, length: Distance, tolerance: Tolerance) -> Station<1> {
    let (index, point, length) = located(polyline, length.into_inner(), tolerance);
    Station { point: Point2::new(point.x, point.y), lengths: [length], tangents: [tangent(segment(&polyline.outline.polyline, index), point)] }
}

#[must_use]
pub fn intersections<const LEFT: Closure, const RIGHT: Closure>(left: &Polyline<LEFT>, right: &Polyline<RIGHT>, tolerance: Tolerance) -> Vec<Either<Station<2>, [Station<2>; 2]>> {
    let (IndexedPolyline { polyline: first, spatial_index }, second) = (&left.outline, &right.outline.polyline);
    let reach = tolerance.into_inner();
    let station = |point: Vector2<f64>, on_left: usize, on_right: usize| {
        let [(before, ahead), (after, onward)] = [along(left, on_left, point, tolerance), along(right, on_right, point, tolerance)];
        Station { point: Point2::new(point.x, point.y), lengths: [before, after], tangents: [ahead, onward] }
    };
    let along_left = |contact: &Either<Station<2>, [Station<2>; 2]>| match contact {
        Either::Left(Station { point: _, lengths: [on_left, _], tangents: _ }) => *on_left,
        Either::Right([Station { point: _, lengths: [start, _], tangents: _ }, Station { point: _, lengths: [end, _], tangents: _ }]) => start.min(*end),
    };
    let PlineIntersectsCollection { basic_intersects: crossings, overlapping_intersects: mut overlaps } =
        first.find_intersects_opt(second, &FindIntersectsOptions { pline1_aabb_index: Some(spatial_index), pos_equal_eps: reach });
    let spans = sort_and_join_overlapping_intersects(&mut overlaps, first, second, reach);
    crossings
        .into_iter()
        .map(|PlineBasicIntersect { start_index1, start_index2, point }| Either::Left(station(point, start_index1, start_index2)))
        .chain(spans.into_iter().map(|OverlappingSlice { start_indexes: (start_left, start_right), end_indexes: (end_left, end_right), view_data, is_loop: _, opposing_directions: _ }| {
            Either::Right([station(view_data.updated_start.pos(), start_left, start_right), station(view_data.end_point, end_left, end_right)])
        }))
        .sorted_by(|before, after| along_left(before).total_cmp(&along_left(after)))
        .collect()
}

pub fn slice<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, lengths: [Distance; 2], tolerance: Tolerance) -> Result<Option<Polyline<{ Closure::Open }>>, PlanarError> {
    sliced(polyline, lengths.map(Distance::into_inner), tolerance)
}

pub fn pieces<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, region: &Region, tolerance: Tolerance) -> Result<Vec<(Polyline<{ Closure::Open }>, Option<i32>)>, PlanarError> {
    let Polyline { outline: IndexedPolyline { polyline: path, spatial_index }, lengths } = polyline;
    let shape = region.as_ref();
    let (reach, total) = (tolerance.into_inner(), *lengths.last());
    let cuts = spatial_index
        .bounds()
        .into_iter()
        .flat_map(|bounds| shape.plines_index.query_iter(bounds.min_x, bounds.min_y, bounds.max_x, bounds.max_y))
        .filter_map(|index| boundary(shape, index))
        .flat_map(|boundary| {
            let PlineIntersectsCollection { basic_intersects: crossings, overlapping_intersects: overlaps } =
                path.find_intersects_opt(&boundary.polyline, &FindIntersectsOptions { pline1_aabb_index: Some(spatial_index), pos_equal_eps: reach });
            crossings
                .into_iter()
                .map(|PlineBasicIntersect { start_index1, start_index2: _, point }| (start_index1, point))
                .chain(overlaps.into_iter().flat_map(|overlap| [(overlap.start_index1, overlap.point1), (overlap.start_index1, overlap.point2)]))
                .map(|(index, point)| along(polyline, index, point, tolerance).0)
                .collect_vec()
        })
        .sorted_by(f64::total_cmp)
        .dedup_by(|before, after| (after - before).abs() <= reach)
        .collect_vec();
    let spans = match (CLOSURE, cuts.as_slice()) {
        (Closure::Open, _) => iter::once(0.0).chain(cuts.iter().copied()).chain([total]).tuple_windows().collect_vec(),
        (Closure::Closed, []) => vec![(0.0, total)],
        (Closure::Closed, [first, ..]) => cuts.iter().copied().chain([first + total]).tuple_windows().collect_vec(),
    };
    spans
        .into_iter()
        .filter_map(|(start, end)| {
            sliced(polyline, [start, end], tolerance)
                .map(|piece| {
                    piece.map(|piece| {
                        let (_, middle, _) = located(polyline, f64::midpoint(start, end), tolerance);
                        (piece, winding_number(region, Point2::new(middle.x, middle.y), tolerance))
                    })
                })
                .transpose()
        })
        .collect()
}

#[must_use]
pub fn clearance(region: &Region, point: Point2<f64>, tolerance: Tolerance) -> Option<Clearance> {
    let shape = region.as_ref();
    let at = Vector2::new(point.x, point.y);
    let mut best: Option<(&IndexedPolyline<f64>, usize, Vector2<f64>, f64)> = None;
    shape.plines_index.visit_neighbors(point.x, point.y, &mut |index: usize, squared: f64| {
        if best.is_some_and(|(_, _, _, nearest)| squared > nearest) {
            Control::Break(())
        } else {
            if let Some(outline) = boundary(shape, index)
                && let (closest, near, distance) = nearest(outline, at, tolerance)
                && best.is_none_or(|(_, _, _, nearest)| distance < nearest)
            {
                best = Some((outline, closest, near, distance));
            }
            Control::Continue
        }
    });
    best.map(|(outline, index, near, squared)| {
        let distance = squared.sqrt();
        Clearance {
            point: Point2::new(near.x, near.y),
            tangent: tangent(segment(&outline.polyline, index), near),
            distance: if winding_number(region, point, tolerance).is_some_and(|winding| winding != 0) { -distance } else { distance },
        }
    })
}

fn nearest(outline: &IndexedPolyline<f64>, at: Vector2<f64>, tolerance: Tolerance) -> (usize, Vector2<f64>, f64) {
    let IndexedPolyline { polyline: path, spatial_index } = outline;
    let projected = |index: usize| {
        let (start, end) = segment(path, index);
        let near = polyline::seg_closest_point(start, end, at, tolerance.into_inner());
        (index, near, math::dist_squared(near, at))
    };
    let mut closest = projected(0);
    spatial_index.visit_neighbors(at.x, at.y, &mut |index: usize, squared: f64| {
        if squared > closest.2 {
            Control::Break(())
        } else {
            closest = cmp::min_by(closest, projected(index), |left, right| left.2.total_cmp(&right.2));
            Control::Continue
        }
    });
    closest
}

fn along<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, index: usize, point: Vector2<f64>, tolerance: Tolerance) -> (f64, UnitVector2<f64>) {
    let (start, end) = segment(&polyline.outline.polyline, index);
    let SplitResult { updated_start: _, split_vertex } = polyline::seg_split_at_point(start, end, point, tolerance.into_inner());
    (polyline.lengths.get(index).copied().unwrap_or_default() - polyline::seg_length(split_vertex, end), tangent((start, end), point))
}

fn tangent((start, end): (PlineVertex<f64>, PlineVertex<f64>), point: Vector2<f64>) -> UnitVector2<f64> {
    let direction = polyline::seg_tangent_vector(start, end, point);
    UnitVector2::new_normalize(nalgebra::Vector2::new(direction.x, direction.y))
}

fn located<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, length: f64, tolerance: Tolerance) -> (usize, Vector2<f64>, f64) {
    let Polyline { outline: IndexedPolyline { polyline: path, spatial_index: _ }, lengths } = polyline;
    let total = *lengths.last();
    let target = match CLOSURE {
        Closure::Open => length.clamp(0.0, total),
        Closure::Closed => length.rem_euclid(total),
    };
    let index = lengths.partition_point(|end| *end < target);
    let before = index.checked_sub(1).and_then(|previous| lengths.get(previous)).copied().unwrap_or_default();
    let (start, end) = segment(path, index);
    let point = PlineViewData::create_on_single_segment(path, index, start, end.pos(), tolerance.into_inner())
        .and_then(|view| view.view(path).find_point_at_path_length(target - before).ok())
        .map_or_else(|| end.pos(), |(_, point)| point);
    (index, point, target)
}

fn sliced<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, [from, to]: [f64; 2], tolerance: Tolerance) -> Result<Option<Polyline<{ Closure::Open }>>, PlanarError> {
    let path = &polyline.outline.polyline;
    let (reach, total) = (tolerance.into_inner(), *polyline.lengths.last());
    let ([start, end], inverted, whole) = match CLOSURE {
        Closure::Open => (cmp::minmax_by(from.clamp(0.0, total), to.clamp(0.0, total), f64::total_cmp), to < from, false),
        Closure::Closed => {
            let span = (to - from).rem_euclid(total);
            ([from, from + span], false, (to - from).abs() > reach && (span <= reach || span >= total - reach))
        }
    };
    let (start_index, start_point, _) = located(polyline, start, tolerance);
    let view = if whole {
        PlineViewData::from_new_start(path, start_point, start_index, reach)
    } else {
        let (end_index, end_point, _) = located(polyline, end, tolerance);
        PlineViewData::from_slice_points(path, start_point, start_index, end_point, end_index, reach)
    };
    view.map_or(Ok(None), |view| surviving::<{ Closure::Open }>(PlineViewData { inverted_direction: inverted, ..view }.view(path).iter_vertexes(), tolerance).map(|kept| kept.map(measured)))
}
