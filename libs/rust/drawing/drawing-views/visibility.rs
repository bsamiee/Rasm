use core::f64::consts::FRAC_1_SQRT_2;
use core::{cmp, convert, iter};

use itertools::Itertools as _;
use kurbo::{Line, Nearest, ParamCurveNearest as _, Point, Rect, Shape as _};
use nalgebra::{Point3, Unit, Vector3};
use parry3d_f64::bounding_volume::{Aabb, BoundingVolume as _};
use parry3d_f64::math::Vector;
use parry3d_f64::partitioning::{Bvh, BvhBuildStrategy, BvhNode};
use parry3d_f64::query::{Ray, RayCast as _};
use parry3d_f64::shape::Triangle;
use rasm_drawing::{EdgeKind, Tolerance, Visibility};
use rasm_proximity::{Assembly, Distance, DistanceError, rays};
use rayon::iter::{Either, IndexedParallelIterator as _, IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{Feature, Frame, Prepared, Scene, ViewsError, clip, depth, picture, ray};

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
    let assembly: &Bvh = Assembly::as_ref(&scene.assembly);
    let cell = |point: Point| (point.to_vec2() / (tolerance * FRAC_1_SQRT_2)).round();
    let margins = |point: &Point3<f64>| {
        let homogeneous = clip(frame, point);
        [homogeneous.w - homogeneous.x, homogeneous.w + homogeneous.x, homogeneous.w - homogeneous.y, homogeneous.w + homogeneous.y, homogeneous.w - homogeneous.z, depth(frame, point) + tolerance]
    };
    let clipped = |Feature { ends: [start, end], kind }: Feature| {
        iter::zip(margins(&start), margins(&end))
            .try_fold([0.0_f64, 1.0_f64], |[enter, leave], (from, to)| {
                let crossing = from / (from - to);
                match (from < 0.0, to < 0.0) {
                    (true, true) => None,
                    (true, false) => Some([enter.max(crossing), leave]),
                    (false, true) => Some([enter, leave.min(crossing)]),
                    (false, false) => Some([enter, leave]),
                }
            })
            .filter(|[enter, leave]| enter < leave)
            .map(|stations| Feature { ends: stations.map(|station| start.lerp(&end, station)), kind })
    };
    let sight = |point: &Point3<f64>| {
        let normalized = frame.clip.transform_point(point);
        ray(frame, Point::new(normalized.x, normalized.y))
    };
    let meeting = |left: Triangle, right: Triangle| {
        let [left_normal, right_normal] = [left.robust_scaled_normal().try_normalize()?, right.robust_scaled_normal().try_normalize()?].map(Vector3::from);
        let [left_corners @ [left_anchor, ..], right_corners @ [right_anchor, ..]] = [left, right].map(|triangle| triangle.vertices().map(Point3::from));
        let axis = left_normal.cross(&right_normal);
        let flush = 64.0 * f64::EPSILON * iter::chain(left_corners, right_corners).map(|corner| corner.coords.amax()).fold(0.0, f64::max);
        let order = |first: &Point3<f64>, second: &Point3<f64>| axis.dot(&first.coords).total_cmp(&axis.dot(&second.coords));
        let span = |triangle: [Point3<f64>; 3], anchor: Point3<f64>, normal: Vector3<f64>| {
            let corners = triangle.map(|corner| (corner, normal.dot(&(corner - anchor))));
            corners
                .iter()
                .circular_tuple_windows()
                .filter_map(|(&(from, before), &(to, after))| {
                    (before.abs() <= flush).then_some(from).or_else(|| (before * after < 0.0 && after.abs() > flush).then(|| from.lerp(&to, before / (before - after))))
                })
                .collect_array::<2>()
                .filter(|_| corners.iter().any(|&(_, side)| side > flush) && corners.iter().any(|&(_, side)| side < -flush))
                .map(|[from, to]| cmp::minmax_by(from, to, order))
        };
        let [enter, leave] = span(left_corners, right_anchor, right_normal)?;
        let [entering, leaving] = span(right_corners, left_anchor, left_normal)?;
        let [low, high] = [cmp::max_by(enter, entering, order), cmp::min_by(leave, leaving, order)];
        (axis.norm_squared() > f64::EPSILON && order(&low, &high).is_lt()).then_some(Feature { ends: [low, high], kind: Some(EdgeKind::Crease) }).and_then(clipped)
    };
    let overlapping = |left: &BvhNode, right: &BvhNode| left.aabb().intersects(&right.aabb());
    let meetings = assembly
        .traverse_bvtt_single_tree_parallel::<false>()
        .into_iter()
        .map(|(left, right)| cmp::minmax(left, right))
        .chain(assembly.leaves(|node| overlapping(node, node)).map(|leaf| [leaf, leaf]))
        .map(|[left, right]| (left, &scene.bodies[left as usize].mesh, right, &scene.bodies[right as usize].mesh))
        .collect::<Vec<_>>()
        .into_par_iter()
        .flat_map_iter(|(left, near, right, far)| {
            near.bvh()
                .leaf_pairs(far.bvh(), overlapping)
                .filter(move |&(first, second)| left < right || first < second)
                .filter_map(move |(first, second)| meeting(near.triangle(first), far.triangle(second)))
        });
    let segments = &features
        .into_par_iter()
        .filter_map(clipped)
        .chain(meetings)
        .filter_map(|Feature { ends: [start, end], kind }| Unit::try_new_and_get(end - start, 0.0).map(|(direction, length)| (start, end, kind, direction, length)))
        .flat_map_iter(|(start, end, kind, direction, length)| {
            let probe = Ray::new(Vector::from(start), Vector::from(direction.into_inner()));
            let along = move |node: &BvhNode| node.aabb().intersects_local_ray(&probe, length);
            let piercings = assembly
                .leaves(along)
                .map(|leaf| &scene.bodies[leaf as usize])
                .flat_map(|Prepared { body: _, mesh, sources: _, surface: _ }| mesh.bvh().leaves(along).filter_map(|face| mesh.triangle(face).cast_local_ray(&probe, length, false)))
                .filter(|&time| tolerance < time && time < length - tolerance)
                .sorted_by(f64::total_cmp)
                .dedup_by(|&kept, &next| next - kept <= tolerance)
                .map(move |time| start + direction.scale(time));
            iter::once(start).chain(piercings).chain([end]).tuple_windows::<(_, _)>().map(move |ends| Feature { ends: ends.into(), kind })
        })
        .filter_map(|feature @ Feature { ends: [start, end], kind: _ }| {
            let [head, tail] = [start, end].map(|point| clip(frame, &point));
            let line = Line::new(picture(frame, head)?, picture(frame, tail)?);
            (cell(line.p0) != cell(line.p1)).then_some((feature, [head.w, tail.w], line))
        })
        .collect::<Vec<_>>();
    let contour = |feature: &Feature| {
        feature.kind.is_some_and(|kind| match kind {
            EdgeKind::Crease => false,
            EdgeKind::Silhouette | EdgeKind::Boundary | EdgeKind::Cut => true,
        })
    };
    let endpoints = |segment: usize| [segment.strict_mul(2), segment.strict_mul(2).strict_add(1)];
    let placed = |segment: usize, line: &Line, point: Point| {
        let Nearest { distance_sq, t: station } = line.nearest(point, 0.0);
        let [first, last] = endpoints(segment);
        let length = line.length();
        (distance_sq <= squared).then_some((
            station,
            match (station * length <= tolerance, (1.0 - station) * length <= tolerance) {
                (true, _) => Some(first),
                (false, true) => Some(last),
                (false, false) => None,
            },
        ))
    };
    let position = |&(Feature { ends: [start, end], kind: _ }, [entering, leaving], _): &(Feature, [f64; 2], Line), station: f64| {
        start.lerp(&end, station * entering / station.mul_add(entering - leaving, leaving))
    };
    let bounds = |line: &Line| {
        let Rect { x0, y0, x1, y1 } = line.bounding_box();
        Aabb::new(Vector::new(x0, y0, 0.0), Vector::new(x1, y1, 0.0))
    };
    let contours = Bvh::from_iter(BvhBuildStrategy::default(), segments.iter().enumerate().filter(|(_, (feature, ..))| contour(feature)).map(|(index, (_, _, line))| (index, bounds(line))));
    let (probes, crossings): (Vec<_>, Vec<_>) = segments
        .par_iter()
        .enumerate()
        .flat_map_iter(|(index, segment @ (feature, _, line))| {
            let reach = bounds(line).loosened(tolerance);
            contours
                .leaves(move |node| node.aabb().intersects(&reach))
                .map(|leaf| leaf as usize)
                .filter(move |&other| other != index && (!contour(feature) || index < other))
                .map(move |other| (other, &segments[other]))
                .flat_map(move |(other, crossed @ (_, _, over))| {
                    let node = move |point: Point| Some((point, placed(index, line, point)?, placed(other, over, point)?));
                    let mut contacts = [line.p0, line.p1, over.p0, over.p1].into_iter().filter_map(node).peekable();
                    let crossing = if contacts.peek().is_none() { line.crossing_point(*over).and_then(node) } else { None };
                    contacts.chain(crossing).filter_map(move |(point, (near, here), (far, there))| match (here, there) {
                        (None, None) => {
                            let [own, opposite] = [depth(frame, &position(segment, near)), depth(frame, &position(crossed, far))];
                            (opposite <= own + tolerance || (contour(feature) && own <= opposite + tolerance)).then_some(Either::Right(([(index, near), (other, far)], point)))
                        }
                        (Some(vertex), None) => Some(Either::Left((other, (far, vertex)))),
                        (None, Some(vertex)) => Some(Either::Left((index, (near, vertex)))),
                        (Some(_), Some(_)) => None,
                    })
                })
        })
        .partition_map(convert::identity);
    let shared = segments.len().strict_mul(2);
    let vertices = segments.iter().flat_map(|(_, _, line)| [line.p0, line.p1]).chain(crossings.iter().map(|&(_, point)| point)).collect::<Vec<_>>();
    let stations =
        iter::chain(probes, crossings.iter().enumerate().flat_map(|(crossing, &(sides, _))| sides.into_iter().map(move |(segment, station)| (segment, (station, shared.strict_add(crossing))))))
            .into_group_map();
    let (near, far): (Vec<_>, Vec<_>) = segments
        .par_iter()
        .enumerate()
        .flat_map_iter(|(index, segment @ &(Feature { ends: _, kind }, _, line))| {
            let length = line.length();
            let [first, last] = endpoints(index);
            let point = move |station: f64| position(segment, station);
            iter::once((0.0, first))
                .chain(
                    stations
                        .get(&index)
                        .into_iter()
                        .flatten()
                        .copied()
                        .sorted_by(|(left, _), (right, _)| left.total_cmp(right))
                        .dedup_by(move |&(kept, _), &(next, _)| (next - kept) * length <= tolerance),
                )
                .chain([(1.0, last)])
                .tuple_windows()
                .map(move |((from, tail), (to, head))| {
                    (Piece { ends: [tail, head], depths: [from, to].map(|station| depth(frame, &point(station))), kind, visibility: Visibility::Visible }, point(f64::midpoint(from, to)))
                })
        })
        .partition_map(|(piece, middle)| {
            let (start, _) = sight(&middle);
            match Unit::try_new_and_get(middle - start, 0.0).filter(|_| depth(frame, &middle) > tolerance).map(|(direction, length)| (direction, Distance::try_new(length))) {
                Some((direction, Ok(reach))) => Either::Right((piece, (start - direction.scale(tolerance), direction, ..reach))),
                None | Some((_, Err(DistanceError::FiniteViolated | DistanceError::GreaterOrEqualViolated))) => Either::Left(piece),
            }
        });
    let verdicts = rays::occluded(&scene.assembly, far.par_iter().map(|&(_, cast)| cast))?;
    Ok(Lines {
        vertices,
        pieces: iter::chain(near, iter::zip(far, verdicts).map(|((piece, _), occluded)| Piece { visibility: if occluded { Visibility::Hidden } else { Visibility::Visible }, ..piece })).collect(),
    })
}

pub(crate) fn window(frame: &Frame) -> [Point; 4] { [(-1.0, -1.0), (1.0, -1.0), (1.0, 1.0), (-1.0, 1.0)].map(|(x, y)| frame.picture * Point::new(x, y)) }
