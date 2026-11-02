use core::{convert, iter};

use itertools::Itertools as _;
use kurbo::{Line, Nearest, ParamCurveNearest as _, Point, Rect, Shape as _};
use nalgebra::{Point3, Unit};
use parry3d_f64::{
    bounding_volume::{Aabb, BoundingVolume as _},
    math::Vector,
    partitioning::{Bvh, BvhBuildStrategy},
};
use rasm_linework::{EdgeKind, Tolerance, Visibility};
use rasm_proximity::{Distance, DistanceError, rays};
use rayon::iter::{Either, IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{Feature, Frame, Scene, ViewsError, clip, depth, picture, ray};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub(crate) struct Lines {
    pub(crate) vertices: Vec<Point>,
    pub(crate) pieces: Vec<Piece>,
}

#[derive(Debug, Clone, Copy)]
pub(crate) struct Piece {
    pub(crate) ends: [usize; 2],
    pub(crate) depths: [f64; 2],
    pub(crate) kind: Option<EdgeKind>,
    pub(crate) visibility: Visibility,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn lines(frame: &Frame, scene: &Scene, features: Vec<Feature>, tolerance: Tolerance) -> Result<Lines, ViewsError> {
    let tolerance = tolerance.into_inner();
    let squared = tolerance.powi(2);
    let margins = |point: &Point3<f64>| {
        let homogeneous = clip(frame, point);
        (
            homogeneous,
            [
                homogeneous.w - homogeneous.x,
                homogeneous.w + homogeneous.x,
                homogeneous.w - homogeneous.y,
                homogeneous.w + homogeneous.y,
                homogeneous.w - homogeneous.z,
                depth(frame, point) + tolerance,
            ],
        )
    };
    let segments = &features
        .into_par_iter()
        .filter_map(|Feature { ends: [start, end], kind }| {
            let [(head, before), (tail, after)] = [start, end].map(|point| margins(&point));
            let [enter, leave] = iter::zip(before, after)
                .try_fold([0.0_f64, 1.0_f64], |[enter, leave], (from, to)| {
                    let crossing = from / (from - to);
                    match (from < 0.0, to < 0.0) {
                        (true, true) => None,
                        (true, false) => Some([enter.max(crossing), leave]),
                        (false, true) => Some([enter, leave.min(crossing)]),
                        (false, false) => Some([enter, leave]),
                    }
                })
                .filter(|[enter, leave]| enter < leave)?;
            let [entry, exit] = [enter, leave].map(|station| head.lerp(&tail, station));
            let line = Line::new(picture(frame, entry)?, picture(frame, exit)?);
            let Rect { x0, y0, x1, y1 } = line.bounding_box();
            (line.length() > tolerance).then_some((
                Feature { ends: [enter, leave].map(|station| start.lerp(&end, station)), kind },
                [entry.w, exit.w],
                line,
                Aabb::new(Vector::new(x0, y0, 0.0), Vector::new(x1, y1, 0.0)).loosened(tolerance),
            ))
        })
        .collect::<Vec<_>>();
    let contour = |feature: &Feature| {
        feature.kind.is_some_and(|kind| match kind {
            EdgeKind::Crease => false,
            EdgeKind::Silhouette | EdgeKind::Boundary | EdgeKind::Cut => true,
        })
    };
    let placed = |segment: usize, line: &Line, point: Point| {
        let Nearest { distance_sq, t: station } = line.nearest(point, 0.0);
        let length = line.length();
        let first = segment.strict_mul(2);
        (distance_sq <= squared).then_some(match (station * length <= tolerance, (1.0 - station) * length <= tolerance) {
            (true, _) => Either::Left(first),
            (false, true) => Either::Left(first.strict_add(1)),
            (false, false) => Either::Right(station),
        })
    };
    let contours = Bvh::from_iter(BvhBuildStrategy::default(), segments.iter().enumerate().filter(|(_, (feature, ..))| contour(feature)).map(|(index, &(.., reach))| (index, reach)));
    let (probes, crossings): (Vec<_>, Vec<_>) = segments
        .par_iter()
        .enumerate()
        .flat_map_iter(|(index, (feature, _, line, reach))| {
            contours
                .intersect_aabb(reach)
                .map(|leaf| leaf as usize)
                .filter(move |&other| other != index && (!contour(feature) || index < other))
                .filter_map(move |other| segments.get(other).map(|&(_, _, crossed, _)| (other, crossed)))
                .flat_map(move |(other, crossed)| {
                    let contacts = [((index, line), (other, &crossed)), ((other, &crossed), (index, line))]
                        .into_iter()
                        .flat_map(|((own, from), (onto, target))| {
                            [(own.strict_mul(2), from.p0), (own.strict_mul(2).strict_add(1), from.p1)]
                                .map(|(vertex, end)| placed(onto, target, end).map(|place| place.right().map(|station| (onto, (station, vertex)))))
                        })
                        .flatten()
                        .collect::<Vec<_>>();
                    if contacts.is_empty() {
                        line.crossing_point(crossed)
                            .and_then(|point| Some((point, placed(index, line, point)?, placed(other, &crossed, point)?)))
                            .and_then(|(point, here, there)| match (here, there) {
                                (Either::Right(near), Either::Right(far)) => Some(Either::Right(([(index, near), (other, far)], point))),
                                (Either::Left(vertex), Either::Right(station)) => Some(Either::Left((other, (station, vertex)))),
                                (Either::Right(station), Either::Left(vertex)) => Some(Either::Left((index, (station, vertex)))),
                                (Either::Left(_), Either::Left(_)) => None,
                            })
                            .into_iter()
                            .collect::<Vec<_>>()
                    } else {
                        contacts.into_iter().flatten().map(Either::Left).collect::<Vec<_>>()
                    }
                })
        })
        .partition_map(convert::identity);
    let shared = segments.len().strict_mul(2);
    let vertices = segments.iter().flat_map(|(_, _, line, _)| [line.p0, line.p1]).chain(crossings.iter().map(|&(_, point)| point)).collect::<Vec<_>>();
    let stations =
        iter::chain(probes, crossings.iter().enumerate().flat_map(|(crossing, &(sides, _))| sides.map(|(segment, station)| (segment, (station, shared.strict_add(crossing)))))).into_group_map();
    let piece = |(ends, depths, kind): ([usize; 2], [f64; 2], Option<EdgeKind>), visibility: Visibility| Piece { ends, depths, kind, visibility };
    let (near, far): (Vec<_>, Vec<_>) = segments
        .par_iter()
        .enumerate()
        .flat_map_iter(|(index, &(Feature { ends: [start, end], kind }, [entering, leaving], line, _))| {
            let length = line.length();
            let first = index.strict_mul(2);
            let point = move |station: f64| start.lerp(&end, station * entering / station.mul_add(entering - leaving, leaving));
            iter::chain([(0.0, first), (1.0, first.strict_add(1))], stations.get(&index).into_iter().flatten().copied())
                .sorted_by(|(left, _), (right, _)| left.total_cmp(right))
                .dedup_by(move |&(kept, _), &(next, _)| (next - kept) * length <= tolerance)
                .tuple_windows()
                .map(move |((from, tail), (to, head))| (([tail, head], [from, to].map(|station| depth(frame, &point(station))), kind), point(f64::midpoint(from, to))))
        })
        .partition_map(|(parts, middle)| {
            let homogeneous = clip(frame, &middle);
            let normalized = homogeneous.xy().unscale(homogeneous.w);
            let (start, _) = ray(frame, Point::new(normalized.x, normalized.y));
            match (depth(frame, &middle) > tolerance).then_some(middle - start).and_then(|offset| Unit::try_new_and_get(offset, 0.0)).map(|(direction, length)| (direction, Distance::try_new(length)))
            {
                Some((direction, Ok(reach))) => Either::Right((parts, (start - direction.scale(tolerance), direction, ..reach))),
                None | Some((_, Err(DistanceError::FiniteViolated | DistanceError::GreaterOrEqualViolated))) => Either::Left(piece(parts, Visibility::Visible)),
            }
        });
    rays::occluded(&scene.assembly, far.par_iter().map(|&(_, cast)| cast))
        .map(|verdicts| Lines {
            vertices,
            pieces: iter::chain(near, iter::zip(far, verdicts).map(|((parts, _), occluded)| piece(parts, if occluded { Visibility::Hidden } else { Visibility::Visible }))).collect(),
        })
        .map_err(ViewsError::from)
}

pub(crate) fn window(frame: &Frame) -> [Point; 4] { [(-1.0, -1.0), (1.0, -1.0), (1.0, 1.0), (-1.0, 1.0)].map(|(x, y)| frame.picture * Point::new(x, y)) }
