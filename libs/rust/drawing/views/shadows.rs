use core::{
    f64::consts::{PI, TAU},
    iter,
};
use std::collections::HashMap;

use enum_map::EnumMap;
use itertools::Itertools as _;
use kurbo::{BezPath, PathEl, Point, Shape};
use linesweeper::{BinaryOp, FillRule};
use nalgebra::{Isometry3, Orthographic3, Point3, Translation3, Unit, UnitQuaternion, UnitVector3, Vector3, Vector4};
use nonempty_collections::NEVec;
use parry3d_f64::{bounding_volume::BoundingVolume as _, math::Vector, query::details::line_toi_with_halfspace, shape::Triangle};
use rasm_linework::{LineworkError, Region, Tolerance, regions::boolean};
use rayon::{
    iter::{IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _},
    join,
};
use validated::Validated;

use crate::{
    Frame, Operation, Prepared, Projection, View, ViewsError, clip,
    edges::features,
    faces::{Face, visible},
    picture, ray, visibility,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
pub enum Shadow {
    Attached,
    Cast,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn shadows(view: &View, sun: UnitVector3<f64>) -> Result<EnumMap<Shadow, Vec<Face>>, NEVec<ViewsError>> {
    fn mapped(from: &Frame, to: &Frame, (anchor, normal): (Point3<f64>, UnitVector3<f64>), shape: &impl Shape, tolerance: Tolerance) -> Result<BezPath, LineworkError> {
        let normalized = from.picture.inverse();
        let moved = |point: Point| {
            let (near, segment) = ray(from, normalized * point);
            line_toi_with_halfspace(Vector::from(anchor.coords), Vector::from(normal.into_inner()), Vector::from(near.coords), Vector::from(segment))
                .and_then(|time| picture(to, clip(to, &(near + segment * time))))
                .ok_or(LineworkError::Sweep(linesweeper::Error::Infinity))
        };
        shape
            .path_elements(tolerance.into_inner())
            .map(|element| {
                Ok(match element {
                    PathEl::MoveTo(point) => PathEl::MoveTo(moved(point)?),
                    PathEl::LineTo(point) => PathEl::LineTo(moved(point)?),
                    PathEl::QuadTo(control, point) => {
                        let [control, point] = [control, point].map(moved);
                        PathEl::QuadTo(control?, point?)
                    }
                    PathEl::CurveTo(first, second, point) => {
                        let [first, second, point] = [first, second, point].map(moved);
                        PathEl::CurveTo(first?, second?, point?)
                    }
                    PathEl::ClosePath => PathEl::ClosePath,
                })
            })
            .collect()
    }
    let View { frame, scene, lines, crease, tolerance } = view;
    let reach = tolerance.into_inner();
    let Some(bounds) = scene.bodies.iter().map(|prepared| prepared.mesh.local_aabb()).reduce(|left, right| left.merged(&right)) else {
        return Ok(EnumMap::default());
    };
    let sphere = bounds.bounding_sphere();
    let radius = sphere.radius() + reach;
    let light = Frame::new(
        Isometry3::from_parts(
            Translation3::from(Vector3::from(sphere.center()) + sun.scale(radius)),
            UnitQuaternion::rotation_between_axis(&Vector3::z_axis(), &sun).unwrap_or_else(|| UnitQuaternion::from_axis_angle(&Vector3::x_axis(), PI)),
        )
        .inverse(),
        Projection::Parallel { volume: Orthographic3::new(-radius, radius, -radius, radius, 0.0, 2.0 * radius), .. },
    )
    .map_err(NEVec::new)?;
    let (lit, receivers) = join(
        || {
            visibility::lines(&light, scene, scene.bodies.par_iter().flat_map_iter(|prepared| features(&light, prepared, *crease)).collect(), *tolerance)
                .map_err(NEVec::new)
                .and_then(|outline| visible(&light, scene, &outline, *tolerance))
        },
        || visible(frame, scene, lines, *tolerance),
    );
    let (lit, receivers) = lit
        .map_or_else(Validated::Fail, Validated::Good)
        .map2(receivers.map_or_else(Validated::Fail, Validated::Good), |lit, receivers| {
            (lit.into_iter().map(|(Face { body, source, normal: _, region }, _)| ((body, source), region)).collect::<HashMap<_, _>>(), receivers)
        })
        .ok()?;
    let triangles = scene
        .bodies
        .iter()
        .flat_map(|Prepared { body, mesh, sources }| iter::zip(mesh.triangles(), sources).filter_map(move |(triangle, source)| source.map(|source| ((*body, source), triangle))))
        .into_group_map();
    let viewpoint = frame.inverse * Vector4::new(0.0, 0.0, -1.0, 0.0);
    let seen = |anchor: &Point3<f64>, normal: &UnitVector3<f64>| normal.dot(&(viewpoint.xyz() - anchor.coords.scale(viewpoint.w))) > 0.0;
    let planes = |members: &[Triangle]| {
        let oriented = members
            .iter()
            .filter_map(|triangle| Unit::try_new(Vector3::from(triangle.scaled_normal()), 0.0).map(|normal| ([triangle.a, triangle.b, triangle.c].map(Point3::from), normal)))
            .collect::<Vec<_>>();
        oriented
            .iter()
            .filter_map(|&(corners, normal)| {
                oriented
                    .iter()
                    .position(|([origin, ..], along)| along.dot(&normal) > 0.0 && corners.iter().all(|corner| along.dot(&(corner - origin)).abs() <= reach))
                    .map(|plane| (plane, corners))
            })
            .into_group_map()
            .into_iter()
            .sorted_unstable_by_key(|&(plane, _)| plane)
            .filter_map(|(plane, coplanar)| oriented.get(plane).map(|&([anchor, ..], normal)| (anchor, normal, coplanar)))
            .collect::<Vec<_>>()
    };
    let cover = |members: &[[Point3<f64>; 3]]| {
        members
            .iter()
            .map(|corners| {
                let [first, second, third] = corners.map(|corner| picture(frame, clip(frame, &corner)).ok_or(linesweeper::Error::Infinity));
                Ok::<_, linesweeper::Error>([PathEl::MoveTo(first?), PathEl::LineTo(second?), PathEl::LineTo(third?), PathEl::ClosePath])
            })
            .flatten_ok()
            .collect::<Result<BezPath, _>>()
            .map_err(LineworkError::from)
            .and_then(|path| Region::new(&path, FillRule::NonZero, *tolerance))
    };
    let wide = |region: &Region| {
        region
            .iter()
            .filter(|contour| {
                let perimeter = contour.path.perimeter(reach);
                perimeter > TAU * reach && 2.0 * contour.path.area().abs() > reach * perimeter
            })
            .flat_map(|contour| contour.path.iter())
            .collect::<BezPath>()
    };
    receivers
        .into_par_iter()
        .map(|(Face { body, source, normal, region }, _)| {
            let planes = triangles.get(&(body, source)).map_or_default(|members| planes(members));
            let parts = match <[_; 1]>::try_from(planes) {
                Ok([(anchor, along, _)]) => vec![Ok(((anchor, along), region))],
                Err(several) => several
                    .into_iter()
                    .filter(|(anchor, along, _)| seen(anchor, along))
                    .map(|(anchor, along, coplanar)| cover(&coplanar).and_then(|covered| boolean(&region, [&covered], BinaryOp::Intersection, *tolerance)).map(|part| ((anchor, along), part)))
                    .collect(),
            };
            parts
                .into_iter()
                .map(|part| {
                    part.and_then(|(plane @ (anchor, along), part)| match (seen(&anchor, &along) == (along.dot(&sun) > 0.0), lit.get(&(body, source))) {
                        (false, _) => Ok((Shadow::Attached, part)),
                        (true, None) => Ok((Shadow::Cast, part)),
                        (true, Some(sunlit)) => mapped(frame, &light, plane, &part, *tolerance)
                            .and_then(|pictured| Region::new(&pictured, FillRule::NonZero, *tolerance))
                            .and_then(|pictured| boolean(&pictured, [sunlit], BinaryOp::Difference, *tolerance))
                            .and_then(|shade| mapped(&light, frame, plane, &wide(&shade), *tolerance))
                            .and_then(|path| Region::new(&path, FillRule::NonZero, *tolerance))
                            .and_then(|shade| Region::new(&wide(&shade), FillRule::NonZero, *tolerance))
                            .map(|shade| (Shadow::Cast, shade)),
                    })
                })
                .collect::<Result<Vec<_>, _>>()
                .and_then(|parts| {
                    parts
                        .into_iter()
                        .into_group_map()
                        .into_iter()
                        .filter_map(|(kind, regions)| match <[_; 1]>::try_from(regions) {
                            Ok([single]) => Some(Ok((kind, single))),
                            Err(several) => several.split_first().map(|(first, rest)| boolean(first, rest, BinaryOp::Union, *tolerance).map(|united| (kind, united))),
                        })
                        .filter_ok(|(_, united)| !united.is_empty())
                        .map_ok(|(kind, united)| (kind, Face { body, source, normal, region: united }))
                        .collect::<Result<Vec<_>, _>>()
                })
                .map_err(|refusal| ViewsError::Linework { operation: Operation::Region(body), source: refusal })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map(|faces| {
            faces.into_iter().flatten().fold(EnumMap::<Shadow, Vec<Face>>::default(), |mut kinds, (kind, face)| {
                kinds[kind].push(face);
                kinds
            })
        })
}
