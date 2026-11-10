use core::cmp::max_by;
use core::f64::consts::TAU;
use core::{array, iter};
use std::borrow::Cow;

use acadrust::entities::{BoundaryEdge, BoundaryPath, BoundaryPathFlags, EntityType, Face3D, Hatch, HatchStyleType, LwPolyline, LwVertex, PolylineEdge, Spline, SplineEdge, SplineFlags, VertexFlags};
use acadrust::types::{Matrix3, Vector2, Vector3};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Cap, Circle, Join, Line, ParamCurve, ParamCurveNearest as _, PathEl, PathSeg, Point, Shape as _, Stroke, StrokeOpts, SvgArc, Vec2, flatten, segments, stroke};
use linesweeper::FillRule;
use rasm_drawing::{Dashes, LineworkError, Region, Tolerance};

use crate::{CadError, Refusal, Subject};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub(super) fn ocs(normal: Vector3, elevation: f64) -> Affine {
    let Matrix3 { m: [[x_x, y_x, normal_x], [x_y, y_y, normal_y], _] } = Matrix3::arbitrary_axis(normal);
    Affine::new([x_x, x_y, y_x, y_y, normal_x * elevation, normal_y * elevation])
}

pub(crate) fn path(entity: &EntityType, frame: Affine, tolerance: Tolerance) -> Result<Option<BezPath>, Refusal> {
    let splined = |spline: &Spline| -> Result<Option<BezPath>, Refusal> {
        let weights = Some(spline.weights.as_slice()).filter(|weights| spline.flags.rational && !weights.is_empty());
        match (spline.control_points.as_slice(), spline.fit_points.as_slice()) {
            ([], []) => Ok(None),
            ([], [_, ..]) => Err(Refusal::Fitted),
            (control, _) => nurbs(usize::try_from(spline.degree)?, &control.iter().map(|vertex| frame * point(*vertex)).collect::<Vec<_>>(), weights, &spline.knots, tolerance.into_inner())
                .map(|mut path| {
                    if spline.flags.closed || spline.flags.periodic {
                        path.push(PathEl::ClosePath);
                    }
                    path
                })
                .map(Some)
                .ok_or(Refusal::Unsupported),
        }
    };
    let edges = |face: &Face3D| -> BezPath {
        let corners = [face.first_corner, face.second_corner, face.third_corner, face.fourth_corner].map(|corner| frame * point(corner));
        let invisible = [face.invisible_edges.is_first_invisible(), face.invisible_edges.is_second_invisible(), face.invisible_edges.is_third_invisible(), face.invisible_edges.is_fourth_invisible()];
        let sides = corners.iter().circular_tuple_windows::<(_, _)>().zip(invisible).filter(|((from, to), _)| from != to).map(|((from, to), hidden)| (*from, *to, hidden)).collect::<Vec<_>>();
        match sides.iter().position(|(_, _, hidden)| *hidden) {
            None => iter::chain(sides.first().map(|(from, _, _)| PathEl::MoveTo(*from)), sides.iter().map(|(_, to, _)| PathEl::LineTo(*to))).chain(sides.first().map(|_| PathEl::ClosePath)).collect(),
            Some(seam) => sides
                .iter()
                .cycle()
                .skip(seam.strict_add(1))
                .zip(sides.iter().cycle().skip(seam).map(|(_, _, hidden)| *hidden))
                .take(sides.len())
                .filter(|((_, _, hidden), _)| !hidden)
                .flat_map(|((from, to, _), resumed)| iter::chain(resumed.then_some(PathEl::MoveTo(*from)), iter::once(PathEl::LineTo(*to))))
                .collect(),
        }
    };
    match entity {
        EntityType::Line(line) => Ok(Some(BezPath::from_vec(vec![PathEl::MoveTo(frame * point(line.start)), PathEl::LineTo(frame * point(line.end))]))),
        EntityType::Arc(arc) => Ok(Some(framed(frame * ocs(arc.normal, arc.center.z), tolerance.into_inner(), |local| {
            kurbo::Arc::new(point(arc.center), (arc.radius, arc.radius), arc.start_angle, sweep(arc.start_angle, arc.end_angle), 0.0).path_elements(local).collect()
        }))),
        EntityType::Circle(circle) =>
            Ok(Some(framed(frame * ocs(circle.normal, circle.center.z), tolerance.into_inner(), |local| Circle::new(point(circle.center), circle.radius).path_elements(local).collect()))),
        EntityType::Ellipse(ellipse) => {
            let (major, minor) = (ellipse.major_axis, ellipse.normal.normalize().cross(&ellipse.major_axis) * ellipse.minor_axis_ratio);
            Ok(Some(framed(frame * Affine::new([major.x, major.y, minor.x, minor.y, ellipse.center.x, ellipse.center.y]), tolerance.into_inner(), |local| {
                kurbo::Arc::new(Point::ORIGIN, (1.0, 1.0), ellipse.start_parameter, sweep(ellipse.start_parameter, ellipse.end_parameter), 0.0).path_elements(local).collect()
            })))
        }
        EntityType::LwPolyline(_) | EntityType::Polyline2D(_) | EntityType::Polyline(_) | EntityType::Polyline3D(_) =>
            Ok(lightweight(entity).map(|polyline| framed(frame * ocs(polyline.normal, polyline.elevation), tolerance.into_inner(), |local| chained(&polyline, local)))),
        EntityType::Spline(spline) => splined(spline),
        EntityType::Helix(helix) => splined(&helix.spline),
        EntityType::Face3D(face) => Ok(Some(edges(face))),
        EntityType::Point(_)
        | EntityType::Text(_)
        | EntityType::MText(_)
        | EntityType::Dimension(_)
        | EntityType::Hatch(_)
        | EntityType::Solid(_)
        | EntityType::Insert(_)
        | EntityType::Block(_)
        | EntityType::BlockEnd(_)
        | EntityType::Ray(_)
        | EntityType::XLine(_)
        | EntityType::Viewport(_)
        | EntityType::AttributeDefinition(_)
        | EntityType::AttributeEntity(_)
        | EntityType::Leader(_)
        | EntityType::MultiLeader(_)
        | EntityType::MLine(_)
        | EntityType::Mesh(_)
        | EntityType::RasterImage(_)
        | EntityType::Solid3D(_)
        | EntityType::Region(_)
        | EntityType::Body(_)
        | EntityType::Surface(_)
        | EntityType::Table(_)
        | EntityType::Tolerance(_)
        | EntityType::PolyfaceMesh(_)
        | EntityType::Wipeout(_)
        | EntityType::Shape(_)
        | EntityType::Underlay(_)
        | EntityType::Seqend(_)
        | EntityType::Ole2Frame(_)
        | EntityType::PolygonMesh(_)
        | EntityType::Light(_)
        | EntityType::SectionSymbol(_)
        | EntityType::ViewBorder(_)
        | EntityType::Extended(_)
        | EntityType::Unknown(_) => Ok(None),
    }
}

pub(super) fn band(entity: &EntityType, frame: Affine, tolerance: Tolerance) -> Result<Option<Region>, LineworkError> {
    let Some(polyline) = lightweight(entity) else { return Ok(None) };
    let plane = frame * ocs(polyline.normal, polyline.elevation);
    let local = tolerance.into_inner() / plane.spectral_norm();
    let widths = spans(&polyline)
        .map(|(from, to)| match (from.start_width, from.end_width) {
            (0.0, 0.0) => (from, to, polyline.constant_width, polyline.constant_width),
            (start, end) => (from, to, start, end),
        })
        .collect::<Vec<_>>();
    let constant = |width: f64| widths.iter().all(|&(_, _, start, end)| start.total_cmp(&width).is_eq() && end.total_cmp(&width).is_eq());
    let tapered = || -> BezPath {
        fn polygon(points: impl IntoIterator<Item = Point>) -> BezPath {
            let mut points = points.into_iter();
            let path = iter::chain(points.next().map(PathEl::MoveTo), points.map(PathEl::LineTo)).chain(iter::once(PathEl::ClosePath)).collect::<BezPath>();
            if path.area() < 0.0 { path.reverse_subpaths() } else { path }
        }
        let (outlines, seams) = widths
            .iter()
            .filter(|(from, to, _, _)| from.location != to.location)
            .map(|&(from, to, start, end)| {
                let (origin, target) = (planar(from.location), planar(to.location));
                let centerline = arc(origin, target, from.bulge).map_or_else(
                    || {
                        let normal = (target - origin).normalize().turn_90();
                        vec![(origin, normal), (target, normal)]
                    },
                    |curve| {
                        let mut stations = Vec::new();
                        flatten(curve.path_elements(local), local, |element| stations.extend(element.end_point()));
                        stations.into_iter().map(|station| (station, (station - curve.center).normalize() * -curve.sweep_angle.signum())).collect()
                    },
                );
                let lengths = iter::once(0.0)
                    .chain(centerline.array_windows().scan(0.0, |run, [(low, _), (high, _)]| {
                        *run += low.distance(*high);
                        Some(*run)
                    }))
                    .collect::<Vec<_>>();
                let total = lengths.last().copied().unwrap_or_default();
                let sides = iter::zip(&centerline, &lengths)
                    .map(|(&(station, normal), length)| {
                        let half = (length / total).mul_add(end - start, start) / 2.0;
                        (station + normal * half, station - normal * half)
                    })
                    .collect::<Vec<_>>();
                (polygon(sides.iter().map(|(left, _)| *left).chain(sides.iter().rev().map(|(_, right)| *right))), (target, sides.first().copied(), sides.last().copied()))
            })
            .unzip::<_, _, Vec<_>, Vec<_>>();
        let joints = seams
            .iter()
            .circular_tuple_windows::<(_, _)>()
            .take(seams.len().saturating_sub(usize::from(!polyline.is_closed)))
            .filter_map(|((corner, _, ends), (_, starts, _))| {
                ends.zip(*starts).map(|((end_left, end_right), (start_left, start_right))| [polygon([*corner, end_left, start_left]), polygon([*corner, end_right, start_right])])
            })
            .flatten();
        iter::chain(outlines, joints).flat_map(BezPath::into_elements).collect()
    };
    let outline = match &widths {
        _ if constant(0.0) => None,
        [(_, _, width, _), ..] if constant(*width) => Some(stroke(chained(&polyline, local).iter(), &Stroke::new(*width).with_caps(Cap::Butt).with_join(Join::Miter), &StrokeOpts::default(), local)),
        _ => Some(tapered()),
    };
    outline
        .map(|mut outline| {
            outline.apply_affine(plane);
            Region::new(outline, FillRule::NonZero, tolerance)
        })
        .transpose()
}

pub(super) fn boundary(hatch: &Hatch, frame: Affine, tolerance: Tolerance, subject: &Subject) -> Result<Region, CadError> {
    let plane = frame * ocs(hatch.normal, hatch.elevation);
    let local = tolerance.into_inner() / plane.spectral_norm();
    let contour = |path: &BoundaryPath| -> Result<BezPath, Refusal> {
        let edges = path
            .edges
            .iter()
            .map(|edge| match edge {
                BoundaryEdge::Line(line) => Ok(BezPath::from_vec(vec![PathEl::MoveTo(planar(line.start)), PathEl::LineTo(planar(line.end))])),
                BoundaryEdge::CircularArc(arc) => {
                    let turn = if arc.counter_clockwise { sweep(arc.start_angle, arc.end_angle) } else { -sweep(arc.end_angle, arc.start_angle) };
                    Ok(kurbo::Arc::new(planar(arc.center), (arc.radius, arc.radius), arc.start_angle, turn, 0.0).path_elements(local).collect())
                }
                BoundaryEdge::EllipticArc(arc) => {
                    let (major, center) = (planar(arc.major_axis_endpoint).to_vec2(), planar(arc.center));
                    let minor = major.turn_90() * arc.minor_axis_ratio;
                    let (start, turn) = if arc.counter_clockwise { (arc.start_angle, sweep(arc.start_angle, arc.end_angle)) } else { (-arc.start_angle, -sweep(-arc.end_angle, -arc.start_angle)) };
                    Ok(framed(Affine::new([major.x, major.y, minor.x, minor.y, center.x, center.y]), local, |within| {
                        kurbo::Arc::new(Point::ORIGIN, (1.0, 1.0), start, turn, 0.0).path_elements(within).collect()
                    }))
                }
                BoundaryEdge::Spline(spline) => {
                    let stated = spline.control_points.iter().map(|control| control.z).collect::<Vec<_>>();
                    let weights = Some(stated.as_slice()).filter(|weights| spline.rational && !weights.is_empty());
                    match (spline.control_points.as_slice(), spline.fit_points.as_slice()) {
                        ([], [_, ..]) => Err(Refusal::Fitted),
                        (control, _) =>
                            nurbs(usize::try_from(spline.degree)?, &control.iter().map(|vertex| point(*vertex)).collect::<Vec<_>>(), weights, &spline.knots, local).ok_or(Refusal::Unsupported),
                    }
                }
                BoundaryEdge::Polyline(polyline) => Ok(chained(
                    &LwPolyline {
                        vertices: polyline.vertices.iter().map(|vertex| LwVertex::with_bulge(Vector2::new(vertex.x, vertex.y), vertex.z)).collect(),
                        is_closed: polyline.is_closed,
                        ..LwPolyline::new()
                    },
                    local,
                )),
            })
            .collect::<Result<Vec<_>, _>>()?;
        let mut elements = edges.iter().flat_map(BezPath::iter).filter(|element| !matches!(element, PathEl::ClosePath));
        let first = elements.next();
        Ok(iter::chain(
            first,
            elements.map(|element| match element {
                PathEl::MoveTo(start) => PathEl::LineTo(start),
                PathEl::LineTo(_) | PathEl::QuadTo(_, _) | PathEl::CurveTo(_, _, _) | PathEl::ClosePath => element,
            }),
        )
        .chain(first.map(|_| PathEl::ClosePath))
        .collect())
    };
    let loops = hatch.paths.iter().map(contour).collect::<Result<Vec<_>, _>>().map_err(|reason| CadError::Refused { subject: subject.clone(), reason })?;
    let (rule, deepest) = match hatch.style {
        HatchStyleType::Normal => (FillRule::EvenOdd, None),
        HatchStyleType::Outer => (FillRule::EvenOdd, Some(1)),
        HatchStyleType::Ignore => (FillRule::NonZero, Some(0)),
    };
    let depth = |index: usize, start: Point| loops.iter().take(index).chain(loops.iter().skip(index.strict_add(1))).filter(|other| other.winding(start) != 0).count();
    let mut kept = loops
        .iter()
        .enumerate()
        .filter(|(index, path)| deepest.is_none_or(|limit| path.elements().first().and_then(PathEl::end_point).is_none_or(|start| depth(*index, start) <= limit)))
        .flat_map(|(_, path)| path.iter())
        .collect::<BezPath>();
    kept.apply_affine(plane);
    Region::new(kept, rule, tolerance).map_err(|source| CadError::Geometry { subject: subject.clone(), source })
}

#[must_use]
pub(crate) fn curves(path: &BezPath) -> Vec<EntityType> {
    path.subpaths()
        .filter_map(|elements| {
            let closed = matches!(elements.last(), Some(PathEl::ClosePath));
            let pieces = segments(elements.iter().copied()).collect::<Vec<_>>();
            match &pieces {
                [] => None,
                lines if lines.iter().all(|piece| matches!(piece, PathSeg::Line(_))) => Some(EntityType::LwPolyline(LwPolyline {
                    vertices: corners(lines, closed).map(|corner| LwVertex::new(Vector2::new(corner.x, corner.y))).collect(),
                    is_closed: closed,
                    ..LwPolyline::new()
                })),
                curved => Some(EntityType::Spline(cubic(curved, closed))),
            }
        })
        .collect()
}

#[must_use]
pub(super) fn loops(region: &Region) -> Vec<BoundaryPath> {
    region
        .contours()
        .iter()
        .map(|contour| {
            let pieces = contour.path.segments().collect::<Vec<_>>();
            let external = if contour.outer { BoundaryPathFlags::EXTERNAL.bits() } else { BoundaryPathFlags::DEFAULT.bits() };
            let (flags, edge) = if pieces.iter().all(|piece| matches!(piece, PathSeg::Line(_))) {
                (
                    external | BoundaryPathFlags::POLYLINE.bits(),
                    BoundaryEdge::Polyline(PolylineEdge { vertices: corners(&pieces, true).map(|corner| Vector3::new(corner.x, corner.y, 0.0)).collect(), is_closed: true }),
                )
            } else {
                let spline = cubic(&pieces, true);
                (
                    external,
                    BoundaryEdge::Spline(SplineEdge {
                        degree: spline.degree,
                        rational: false,
                        periodic: false,
                        knots: spline.knots,
                        control_points: spline.control_points.into_iter().map(|control| Vector3 { z: 1.0, ..control }).collect(),
                        fit_points: Vec::new(),
                        start_tangent: Vector2::ZERO,
                        end_tangent: Vector2::ZERO,
                    }),
                )
            };
            BoundaryPath { flags: BoundaryPathFlags::from_bits(flags), edges: vec![edge], boundary_handles: Vec::new() }
        })
        .collect()
}

#[must_use]
pub(super) fn signed(dashes: &Dashes, offset: f64) -> Vec<f64> {
    let runs = dashes.iter().zip([1.0, -1.0].into_iter().cycle()).map(|(length, sign)| length * sign).collect::<Vec<_>>();
    let starts = dashes
        .iter()
        .scan(0.0, |position, length| {
            let start = *position;
            *position += length;
            Some(start)
        })
        .collect::<Vec<_>>();
    let phase = offset.rem_euclid(dashes.iter().sum());
    let index = starts.partition_point(|start| *start <= phase).strict_sub(1);
    let within = phase - starts[index];
    let (before, after) = runs.split_at(index);
    match after.split_first() {
        Some((run, rest)) if within > 0.0 =>
            iter::once(run.signum() * (run.abs() - within)).chain(rest.iter().copied()).chain(before.iter().copied()).chain(iter::once(run.signum() * within)).collect(),
        _ => iter::chain(after, before).copied().collect(),
    }
}

fn nurbs(degree: usize, control: &[Point], weights: Option<&[f64]>, knots: &[f64], tolerance: f64) -> Option<BezPath> {
    fn polynomial<T>(control: &[T], point: impl Fn(&T) -> Point) -> Option<PathEl> {
        match control {
            [_, end] => Some(PathEl::LineTo(point(end))),
            [_, handle, end] => Some(PathEl::QuadTo(point(handle), point(end))),
            [_, leading, trailing, end] => Some(PathEl::CurveTo(point(leading), point(trailing), point(end))),
            _ => None,
        }
    }
    let cartesian = |&[x, y, weight]: &[f128; 3]| Point::new((x / weight).cast::<f64>(), (y / weight).cast::<f64>());
    let inserted = |weighted: &[[f128; 3]], spread: &[f64], value: f64| -> Option<(Vec<[f128; 3]>, Vec<f64>)> {
        let span = spread.partition_point(|knot| *knot <= value).checked_sub(1)?;
        let present = spread.iter().filter(|knot| knot.total_cmp(&value).is_eq()).count();
        let (kept, shifted) = (span.checked_sub(degree)?, span.checked_sub(present)?);
        let blended = (0..=weighted.len())
            .map(|index| match index {
                at if at <= kept => weighted.get(at).copied(),
                at if at > shifted => weighted.get(at.checked_sub(1)?).copied(),
                at => {
                    let (low, high) = (spread.get(at)?, spread.get(at.checked_add(degree)?)?);
                    let [low, value, high] = [*low, value, *high].map(f128::from);
                    let width = high - low;
                    let (before, after) = ((high - value) / width, (value - low) / width);
                    let (previous, current) = (weighted.get(at.checked_sub(1)?)?, weighted.get(at)?);
                    #[expect(clippy::suboptimal_flops, reason = "native f128 mul_add produces incorrect results on Darwin")]
                    let blended = array::from_fn(|axis| previous[axis] * before + current[axis] * after);
                    Some(blended)
                }
            })
            .collect::<Option<Vec<_>>>()?;
        let split = span.checked_add(1)?;
        Some((blended, spread.iter().take(split).chain(iter::once(&value)).chain(spread.iter().skip(split)).copied().collect()))
    };
    let count = control.len();
    let rational = weights.is_some_and(|weights| weights.array_windows().any(|[left, right]| left.total_cmp(right).is_ne()));
    let valid = degree > 0
        && count > degree
        && knots.len() == count.checked_add(degree)?.checked_add(1)?
        && weights.is_none_or(|weights| weights.len() == count && weights.iter().all(|weight| weight.is_finite() && *weight > 0.0))
        && control.iter().all(|vertex| vertex.is_finite())
        && knots.iter().all(|knot| knot.is_finite())
        && knots.is_sorted();
    let domain = knots.get(degree..=count).filter(|_| valid)?;
    if !rational
        && count == degree.strict_add(1)
        && domain[0] < domain[1]
        && knots.chunk_by(|left, right| left.total_cmp(right).is_eq()).map(<[f64]>::len).eq([count, count])
        && let Some(element) = polynomial(control, |point| *point)
    {
        return Some([PathEl::MoveTo(control[0]), element].into_iter().collect());
    }
    let breaks = domain.chunk_by(|left, right| left.total_cmp(right).is_eq()).filter_map(<[f64]>::first).copied().collect::<Vec<_>>();
    let (_, lows) = breaks.split_last()?;
    let (points, knots) = breaks.iter().try_fold(
        (
            iter::zip(control, weights.into_iter().flatten().copied().chain(iter::repeat(1.0)))
                .map(|(vertex, weight)| {
                    let weight = f128::from(weight);
                    [f128::from(vertex.x) * weight, f128::from(vertex.y) * weight, weight]
                })
                .collect::<Vec<_>>(),
            knots.to_vec(),
        ),
        |state, &value| {
            let present = state.1.iter().filter(|knot| knot.total_cmp(&value).is_eq()).count();
            (present..degree).try_fold(state, |(weighted, spread), _| inserted(&weighted, &spread, value))
        },
    )?;
    let pieces = lows
        .iter()
        .map(|low| {
            let span = knots.partition_point(|knot| knot <= low).checked_sub(1)?;
            points.get(span.checked_sub(degree)?..=span)
        })
        .collect::<Option<Vec<_>>>()?;
    let mut path = BezPath::new();
    path.move_to(cartesian(pieces.first()?.first()?));
    let mut pieces = pieces.into_iter();
    let mut pending = Vec::new();
    let mut controls = Vec::new();
    while let Some(piece) = pending.pop().map(Cow::Owned).or_else(|| pieces.next().map(Cow::Borrowed)) {
        if !rational && let Some(element) = polynomial(&piece, cartesian) {
            path.push(element);
            continue;
        }
        controls.clear();
        controls.extend(piece.iter().map(cartesian));
        let last = piece.len().strict_sub(1);
        let (start, end) = (controls[0], controls[last]);
        let magnitude = controls.iter().map(|point| point.x.abs().max(point.y.abs())).fold(0.0, |largest, next| max_by(largest, next, f64::total_cmp));
        let flat = magnitude == 0.0 || {
            let scale = magnitude.max(tolerance);
            let normalized = |point: Point| Point::new(point.x / scale, point.y / scale);
            let chord = Line::new(normalized(start), normalized(end));
            let reach = (tolerance / scale).max((magnitude / scale) * f64::EPSILON * 64.0);
            controls.iter().all(|&point| chord.nearest(normalized(point), 0.0).distance_sq <= reach * reach)
        };
        if flat {
            path.line_to(end);
            continue;
        }
        let mut left = piece.into_owned();
        let mut right = left.clone();
        for level in 1..=last {
            for index in (level..=last).rev() {
                let (low, high) = (left[index.strict_sub(1)], left[index]);
                left[index] = array::from_fn(|axis| low[axis].midpoint(high[axis]));
            }
            right[last.strict_sub(level)] = left[last];
        }
        pending.extend([right, left]);
    }
    Some(path)
}

fn lightweight(entity: &EntityType) -> Option<Cow<'_, LwPolyline>> {
    let drawn = |flags: i32| flags & i32::from(VertexFlags::SPLINE_CONTROL.bits()) == 0;
    let lowered = |vertices: Vec<LwVertex>, is_closed: bool, elevation: f64, normal: Vector3| Some(Cow::Owned(LwPolyline { vertices, is_closed, elevation, normal, ..LwPolyline::new() }));
    match entity {
        EntityType::LwPolyline(polyline) => Some(Cow::Borrowed(polyline)),
        EntityType::Polyline2D(polyline) => lowered(
            polyline
                .vertices
                .iter()
                .filter(|vertex| drawn(i32::from(vertex.flags.bits())))
                .map(|vertex| {
                    let (start_width, end_width) = match (vertex.start_width, vertex.end_width) {
                        (0.0, 0.0) => (polyline.start_width, polyline.end_width),
                        stated => stated,
                    };
                    LwVertex { location: Vector2::new(vertex.location.x, vertex.location.y), bulge: vertex.bulge, start_width, end_width, vertex_id: vertex.id }
                })
                .collect(),
            polyline.flags.is_closed(),
            polyline.elevation,
            polyline.normal,
        ),
        EntityType::Polyline(polyline) => lowered(
            polyline.vertices.iter().filter(|vertex| drawn(i32::from(vertex.flags.bits()))).map(|vertex| LwVertex::new(Vector2::new(vertex.location.x, vertex.location.y))).collect(),
            polyline.flags.is_closed(),
            0.0,
            Vector3::UNIT_Z,
        ),
        EntityType::Polyline3D(polyline) => lowered(
            polyline.vertices.iter().filter(|vertex| drawn(vertex.flags)).map(|vertex| LwVertex::new(Vector2::new(vertex.position.x, vertex.position.y))).collect(),
            polyline.flags.closed,
            0.0,
            Vector3::UNIT_Z,
        ),
        _ => None,
    }
}

fn spans(polyline: &LwPolyline) -> impl Iterator<Item = (&LwVertex, &LwVertex)> {
    polyline.vertices.iter().circular_tuple_windows().take(polyline.vertices.len().saturating_sub(usize::from(!polyline.is_closed)))
}

fn chained(polyline: &LwPolyline, tolerance: f64) -> BezPath {
    let start = polyline.vertices.first().map(|vertex| PathEl::MoveTo(planar(vertex.location)));
    let bulged = spans(polyline).flat_map(|(from, to)| {
        let (origin, target) = (planar(from.location), planar(to.location));
        let curve = arc(origin, target, from.bulge);
        curve.map(|bend| bend.append_iter(tolerance)).into_iter().flatten().chain(curve.is_none().then_some(PathEl::LineTo(target)))
    });
    iter::chain(start, bulged).chain(start.and_then(|_| polyline.is_closed.then_some(PathEl::ClosePath))).collect()
}

fn cubic(pieces: &[PathSeg], closed: bool) -> Spline {
    let count = pieces.len();
    Spline {
        degree: 3,
        flags: SplineFlags { closed, planar: true, ..SplineFlags::new() },
        knots: iter::successors(Some(0.0_f64), |knot| Some(knot + 1.0))
            .take(count.strict_add(1))
            .enumerate()
            .flat_map(|(index, knot)| iter::repeat_n(knot, if index == 0 || index == count { 4 } else { 3 }))
            .collect(),
        control_points: iter::chain(
            pieces.first().map(ParamCurve::start),
            pieces.iter().flat_map(|piece| {
                let cubic = piece.to_cubic();
                [cubic.p1, cubic.p2, cubic.p3]
            }),
        )
        .map(|control| Vector3::new(control.x, control.y, 0.0))
        .collect(),
        ..Spline::new()
    }
}

fn corners(pieces: &[PathSeg], closed: bool) -> impl Iterator<Item = Point> { pieces.iter().map(ParamCurve::start).chain(pieces.last().filter(|_| !closed).map(ParamCurve::end)) }

fn arc(from: Point, to: Point, bulge: f64) -> Option<kurbo::Arc> {
    let radius = from.distance(to) * bulge.mul_add(bulge, 1.0) / (4.0 * bulge.abs());
    (bulge != 0.0).then_some(SvgArc { from, to, radii: Vec2::new(radius, radius), x_rotation: 0.0, large_arc: bulge.abs() > 1.0, sweep: bulge > 0.0 }).as_ref().and_then(kurbo::Arc::from_svg_arc)
}

fn sweep(start: f64, end: f64) -> f64 {
    let swept = (end - start).rem_euclid(TAU);
    if swept == 0.0 && end - start != 0.0 { TAU } else { swept }
}

fn framed(affine: Affine, tolerance: f64, local: impl FnOnce(f64) -> BezPath) -> BezPath {
    let mut path = local(tolerance / affine.spectral_norm());
    path.apply_affine(affine);
    path
}

const fn point(vector: Vector3) -> Point { Point::new(vector.x, vector.y) }

const fn planar(vector: Vector2) -> Point { Point::new(vector.x, vector.y) }
