use core::{cmp, iter};

use cavalier_contours::{
    core::math::{self, Vector2},
    polyline::{
        self, FindIntersectsOptions, PlineBasicIntersect, PlineCreation as _, PlineIntersectsCollection, PlineOverlappingIntersect, PlineSource as _, PlineSourceMut as _, PlineViewData, SplitResult,
    },
    shape_algorithms::{IndexedPolyline, Shape},
    static_aabb2d_index::Control,
};
use itertools::{Either, Itertools as _};
use nalgebra::Point2;

use crate::{Closure, Polyline, Region, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Station<const N: usize> {
    pub point: Point2<f64>,
    pub lengths: [f64; N],
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn winding_number(region: &Region, point: Point2<f64>, tolerance: Tolerance) -> Option<i32> {
    let Shape { ccw_plines: bodies, cw_plines: holes, plines_index } = region.as_ref();
    let (at, reach) = (Vector2::new(point.x, point.y), tolerance.into_inner());
    plines_index
        .query_iter(point.x - reach, point.y - reach, point.x + reach, point.y + reach)
        .filter_map(|index| index.checked_sub(bodies.len()).map_or_else(|| bodies.get(index), |hole| holes.get(hole)))
        .map(|IndexedPolyline { polyline: outline, spatial_index }| {
            spatial_index
                .query_iter(point.x - reach, point.y - reach, point.x + reach, point.y + reach)
                .all(|segment| polyline::seg_distance_is_greater_than(outline.at(segment), outline.at(outline.next_wrapping_index(segment)), at, reach, reach))
                .then(|| outline.winding_number(at))
        })
        .sum()
}

#[must_use]
pub fn closest<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, point: Point2<f64>, tolerance: Tolerance) -> Station<1> {
    let IndexedPolyline { polyline: path, spatial_index } = polyline.as_ref();
    let at = Vector2::new(point.x, point.y);
    let projected = |segment: usize| {
        let near = polyline::seg_closest_point(path.at(segment), path.at(path.next_wrapping_index(segment)), at, tolerance.into_inner());
        (segment, near, math::dist_squared(near, at))
    };
    let mut nearest = projected(0);
    spatial_index.visit_neighbors(point.x, point.y, &mut |segment: usize, squared: f64| {
        if squared > nearest.2 {
            Control::Break(())
        } else {
            nearest = cmp::min_by(nearest, projected(segment), |left, right| left.2.total_cmp(&right.2));
            Control::Continue
        }
    });
    let (segment, near, _) = nearest;
    Station { point: Point2::new(near.x, near.y), lengths: [length_at(path, segment, near, tolerance)] }
}

#[must_use]
pub fn intersections<const LEFT: Closure, const RIGHT: Closure>(left: &Polyline<LEFT>, right: &Polyline<RIGHT>, tolerance: Tolerance) -> Vec<Either<Station<2>, [Station<2>; 2]>> {
    let (IndexedPolyline { polyline: first, spatial_index }, second) = (left.as_ref(), &right.as_ref().polyline);
    let reach = tolerance.into_inner();
    let station = |point: Vector2<f64>, on_left: usize, on_right: usize| Station {
        point: Point2::new(point.x, point.y),
        lengths: [length_at(first, on_left, point, tolerance), length_at(second, on_right, point, tolerance)],
    };
    let continues = |end: &Station<2>, next: &Station<2>| iter::zip(end.lengths, next.lengths).all(|(before, after)| (before - after).abs() <= reach);
    let along = |contact: &Either<Station<2>, [Station<2>; 2]>| match contact {
        Either::Left(Station { point: _, lengths: [on_left, _] }) => *on_left,
        Either::Right([Station { point: _, lengths: [start, _] }, Station { point: _, lengths: [end, _] }]) => start.min(*end),
    };
    let PlineIntersectsCollection { basic_intersects: crossings, overlapping_intersects: overlaps } =
        first.find_intersects_opt(second, &FindIntersectsOptions { pline1_aabb_index: Some(spatial_index), pos_equal_eps: reach });
    let fragments = overlaps
        .into_iter()
        .map(|PlineOverlappingIntersect { start_index1, start_index2, point1, point2 }| [station(point1, start_index1, start_index2), station(point2, start_index1, start_index2)])
        .sorted_by(|[before, _], [after, _]| before.lengths[1].total_cmp(&after.lengths[1]))
        .collect_vec();
    crossings
        .into_iter()
        .map(|PlineBasicIntersect { start_index1, start_index2, point }| Either::Left(station(point, start_index1, start_index2)))
        .chain(
            fragments
                .iter()
                .map(|[start, end]| (start, end))
                .coalesce(|(start, end), (next, last)| if continues(end, next) { Ok((start, last)) } else { Err(((start, end), (next, last))) })
                .map(|(start, end)| Either::Right([*start, *end])),
        )
        .sorted_by(|before, after| along(before).total_cmp(&along(after)))
        .collect()
}

#[must_use]
pub fn slice<const CLOSURE: Closure>(polyline: &Polyline<CLOSURE>, lengths: [f64; 2], tolerance: Tolerance) -> Option<Polyline<{ Closure::Open }>> {
    let path = &polyline.as_ref().polyline;
    let total = path.path_length();
    lengths.into_iter().all(f64::is_finite).then_some(())?;
    let [from, to] = lengths.map(|length| match CLOSURE {
        Closure::Open => length.clamp(0.0, total),
        Closure::Closed => length.rem_euclid(total),
    });
    let reversed = CLOSURE == Closure::Open && to < from;
    let [start, end] = if reversed { [to, from] } else { [from, to] };
    let ((start_index, start_point), (end_index, end_point)) = (path.find_point_at_path_length(start).ok()?, path.find_point_at_path_length(end).ok()?);
    PlineViewData::from_slice_points(path, start_point, start_index, end_point, end_index, tolerance.into_inner()).map(|view| {
        let mut outline = polyline::Polyline::create_from_remove_repeat(&view.view(path), tolerance.into_inner());
        if reversed {
            outline.invert_direction_mut();
        }
        Polyline(IndexedPolyline::new(outline))
    })
}

fn length_at(path: &polyline::Polyline<f64>, segment: usize, point: Vector2<f64>, tolerance: Tolerance) -> f64 {
    let SplitResult { updated_start, split_vertex } = polyline::seg_split_at_point(path.at(segment), path.at(path.next_wrapping_index(segment)), point, tolerance.into_inner());
    path.iter_segments().take(segment).map(|(start, end)| polyline::seg_length(start, end)).sum::<f64>() + polyline::seg_length(updated_start, split_vertex)
}
