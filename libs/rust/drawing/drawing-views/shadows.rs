use core::f64::consts::PI;
use core::iter;
use std::borrow::Cow;
use std::collections::HashMap;
use std::sync::Arc;

use itertools::{Itertools as _, izip};
use kurbo::{BezPath, PathEl, Point, Shape as _, flatten};
use linesweeper::{BinaryOp, FillRule};
use manifold_rust::cancel::{self, CancelToken};
use nalgebra::{Isometry3, Orthographic3, Point3, Translation3, UnitQuaternion, UnitVector3, Vector3};
use nonempty_collections::NEVec;
use parry3d_f64::bounding_volume::BoundingVolume as _;
use parry3d_f64::math::Vector;
use parry3d_f64::query::details::line_toi_with_halfspace;
use parry3d_f64::shape::Triangle;
use rasm_drawing::regions::boolean;
use rasm_drawing::{LineworkError, Region};
use rasm_mesh::Source;
use rayon::iter::{IntoParallelIterator as _, IntoParallelRefIterator as _, ParallelIterator as _};
use rayon::join;
use validated::Validated;

use crate::faces::{Face, visible};
use crate::visibility::Lines;
use crate::{Frame, Operation, Prepared, Projection, Scene, View, ViewsError, clip, outline, picture, ray};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn shadows(view: &View, sun: UnitVector3<f64>, token: Option<&CancelToken>) -> Result<Vec<Face>, NEVec<ViewsError>> {
    let View { frame, scene, lines, crease, tolerance } = view;
    let reach = tolerance.into_inner();
    let Some(bounds) = scene.bodies.iter().map(|prepared| prepared.mesh.local_aabb()).reduce(|left, right| left.merged(&right)) else {
        return Ok(Vec::new());
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
    let seed = |members: &[(u32, Triangle)]| {
        let flat = members
            .iter()
            .max_by(|(_, left), (_, right)| left.area().total_cmp(&right.area()))
            .and_then(|(_, reference)| reference.robust_scaled_normal().try_normalize().map(|normal| (normal, normal.dot(reference.a))))
            .is_none_or(|(normal, offset)| members.iter().all(|(_, triangle)| triangle.vertices().iter().all(|&corner| (normal.dot(corner) - offset).abs() <= reach)));
        members.first().filter(|_| flat).map(|&(first, _)| first)
    };
    let keyed = |Prepared { body: _, mesh, sources, surface: _ }: &Prepared| {
        let seeds = izip!(0_u32.., mesh.triangles(), sources)
            .filter_map(|(index, triangle, source)| source.map(|source| (source, (index, triangle))))
            .into_group_map()
            .into_iter()
            .map(|(source, members)| (source, seed(&members)))
            .collect::<HashMap<_, _>>();
        seeds.values().any(Option::is_none).then(|| izip!(0_u32.., sources).map(|(index, source)| source.map(|source| Source { face: seeds[&source].unwrap_or(index), ..source })).collect::<Vec<_>>())
    };
    let keys = scene.bodies.par_iter().map(keyed).collect::<Vec<_>>();
    let seamed = keys.iter().any(Option::is_some);
    let origins = iter::zip(&scene.bodies, &keys)
        .filter_map(|(prepared, keys)| keys.as_ref().map(|keys| (prepared, keys)))
        .flat_map(|(prepared, keys)| iter::zip(keys, &prepared.sources).filter_map(|(&key, &source)| Some(((prepared.body, key?), source?))))
        .collect::<HashMap<_, _>>();
    let planar = if seamed {
        Cow::Owned(Scene {
            bodies: iter::zip(&scene.bodies, keys)
                .map(|(prepared, keys)| keys.map_or_else(|| prepared.clone(), |sources| Prepared { body: prepared.body, mesh: Arc::clone(&prepared.mesh), sources, surface: prepared.surface }))
                .collect(),
            assembly: Arc::clone(&scene.assembly),
        })
    } else {
        Cow::Borrowed(scene)
    };
    let faced = |seen: &Frame, outline: &Lines| {
        (!cancel::is_cancelled(token)).ok_or(NEVec::new(ViewsError::Cancelled))?;
        visible(seen, &planar, outline, *tolerance)
    };
    let outlined = |seen: &Frame| {
        (!cancel::is_cancelled(token)).ok_or(NEVec::new(ViewsError::Cancelled))?;
        outline(seen, &planar, *crease, *tolerance).map_err(NEVec::new).and_then(|outlined| faced(seen, &outlined))
    };
    let (sunward, viewed) = join(|| outlined(&light), || if seamed { outlined(frame) } else { faced(frame, lines) });
    let (lit, receivers) = sunward
        .map_or_else(Validated::Fail, Validated::Good)
        .map2(viewed.map_or_else(Validated::Fail, Validated::Good), |outcomes, receivers| {
            (outcomes.into_iter().map(|(Face { body, source, normal: _, region }, _)| ((body, source), region)).collect::<HashMap<_, _>>(), receivers)
        })
        .ok()?;
    let mapped = |from: &Frame, to: &Frame, (anchor, normal): (Point3<f64>, UnitVector3<f64>), path: &BezPath| {
        let normalized = from.picture.inverse();
        let moved = |point: Point| {
            let (near, segment) = ray(from, normalized * point);
            line_toi_with_halfspace(Vector::from(anchor.coords), Vector::from(normal.into_inner()), Vector::from(near.coords), Vector::from(segment))
                .and_then(|time| picture(to, clip(to, &(near + segment * time))))
        };
        let mut elements = Some(BezPath::new());
        flatten(path, reach, |element| {
            let Some(outline) = elements.as_mut() else { return };
            match match element {
                PathEl::MoveTo(point) => moved(point).map(PathEl::MoveTo),
                PathEl::LineTo(point) | PathEl::QuadTo(_, point) | PathEl::CurveTo(_, _, point) => moved(point).map(PathEl::LineTo),
                PathEl::ClosePath => Some(PathEl::ClosePath),
            } {
                Some(element) => outline.push(element),
                None => elements = None,
            }
        });
        elements.ok_or(LineworkError::Sweep(linesweeper::Error::Infinity))
    };
    let wide = |path: &BezPath| 2.0 * path.area().abs() > reach * path.perimeter(reach);
    receivers
        .into_par_iter()
        .filter_map(|(Face { body, source: key, normal, region }, anchor)| {
            let plane = (anchor, normal);
            match lit.get(&(body, key)).filter(|_| normal.dot(&sun) > 0.0) {
                None => Ok(region),
                Some(sunlit) => region
                    .contours()
                    .iter()
                    .map(|contour| mapped(frame, &light, plane, &contour.path))
                    .flatten_ok()
                    .collect::<Result<BezPath, _>>()
                    .and_then(|pictured| Region::new(pictured, FillRule::NonZero, *tolerance))
                    .and_then(|pictured| boolean(&pictured, [sunlit], BinaryOp::Difference, *tolerance))
                    .and_then(|unlit| {
                        unlit
                            .contours()
                            .iter()
                            .filter(|contour| wide(&contour.path))
                            .map(|contour| mapped(&light, frame, plane, &contour.path))
                            .filter_ok(wide)
                            .flatten_ok()
                            .collect::<Result<BezPath, _>>()
                    })
                    .and_then(|shade| Region::new(shade, FillRule::NonZero, *tolerance)),
            }
            .map(|shade| (!shade.contours().is_empty()).then(|| Face { body, source: origins.get(&(body, key)).copied().unwrap_or(key), normal, region: shade }))
            .map_err(|refusal| ViewsError::Linework { operation: Operation::Region(body), source: refusal })
            .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
