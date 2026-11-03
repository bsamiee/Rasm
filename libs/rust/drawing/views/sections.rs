use core::cmp::Ordering;

use itertools::Itertools as _;
use kurbo::{BezPath, Rect, Shape as _, Triangle};
use linesweeper::{BinaryOp, FillRule};
use manifold_rust::cancel::{self, CancelToken};
use nalgebra::Point3;
use nonempty_collections::NEVec;
use parry3d_f64::shape::TriMeshFlags;
use rasm_linework::{EdgeKind, LineworkError, Region, Tolerance, regions::boolean};
use rasm_mesh::{Source, sections::trim};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Body, Feature, Frame, Operation, Prepared, View, ViewsError, clip, depth, picture, visibility::window};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sections(view: &View) -> Result<Vec<(usize, Region)>, NEVec<ViewsError>> {
    let View { frame, scene, lines: _, crease: _, tolerance } = view;
    let [corner, _, opposite, _] = window(frame);
    let border = Rect::from_points(corner, opposite);
    let pictured = |vertex| picture(frame, clip(frame, &Point3::from(vertex))).ok_or(linesweeper::Error::Infinity);
    scene
        .bodies
        .par_iter()
        .filter(|prepared| prepared.sources.contains(&None))
        .filter_map(|Prepared { body, mesh, sources, surface: _ }| {
            mesh.triangles()
                .zip(sources)
                .filter(|&(_, source)| source.is_none())
                .map(|(triangle, _)| {
                    let [start, middle, end] = triangle.vertices().map(pictured);
                    Ok::<_, linesweeper::Error>(Triangle::new(start?, middle?, end?).path_elements(tolerance.into_inner()))
                })
                .flatten_ok()
                .collect::<Result<BezPath, _>>()
                .map_err(LineworkError::from)
                .and_then(|caps| Region::new(&caps, FillRule::NonZero, *tolerance))
                .and_then(|capped| boolean(&capped, [&Region::new(&border, FillRule::NonZero, *tolerance)?], BinaryOp::Intersection, *tolerance))
                .map(|region| (!region.is_empty()).then_some((*body, region)))
                .map_err(|source| ViewsError::Linework { operation: Operation::Region(*body), source })
                .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub(super) fn prepared(frame: &Frame, index: usize, body: Body, token: Option<&CancelToken>) -> Result<Option<Prepared>, ViewsError> {
    (!cancel::is_cancelled(token)).ok_or(ViewsError::Cancelled)?;
    let refused = move |source| ViewsError::Mesh { body: index, source };
    let topology = move |source| ViewsError::Topology { body: index, source };
    match body {
        Body::Solid(solid) => {
            let (exported, faces) = solid.to_mesh().map_err(refused)?;
            let aabb = exported.local_aabb();
            let rounding = f64::EPSILON * 64.0_f64 * aabb.mins.abs().max(aabb.maxs.abs()).max_element().max(frame.plane.translation.vector.amax());
            match exported.vertices().iter().map(|&vertex| depth(frame, &Point3::from(vertex))).minmax_by(f64::total_cmp).into_option() {
                Some((nearest, _)) if nearest >= -rounding => Some((exported, faces.into_iter().map(Some).collect())),
                Some((_, farthest)) if farthest > rounding => trim(&solid, &frame.plane, token)
                    .map_err(refused)?
                    .map(|trimmed| {
                        let originals = solid.original_ids();
                        trimmed
                            .to_mesh()
                            .map_err(refused)
                            .map(|(mesh, sources)| (mesh, sources.into_iter().map(|source| source.original.is_none_or(|original| originals.contains(&original)).then_some(source)).collect()))
                    })
                    .transpose()?,
                Some(_) | None => None,
            }
            .map(|(mut mesh, sources)| mesh.set_flags(TriMeshFlags::HALF_EDGE_TOPOLOGY).map(|()| Prepared { body: index, mesh, sources, surface: false }).map_err(topology))
            .transpose()
        }
        Body::Surface(mut mesh) => {
            mesh.set_flags(mesh.flags() | TriMeshFlags::HALF_EDGE_TOPOLOGY | TriMeshFlags::MERGE_DUPLICATE_VERTICES).map_err(topology)?;
            let sources = (0..).take(mesh.num_triangles()).map(|face| Some(Source { original: None, face })).collect();
            Ok(Some(Prepared { body: index, mesh, sources, surface: true }))
        }
    }
}

pub(super) fn cuts(frame: &Frame, prepared: &Prepared, tolerance: Tolerance) -> Vec<Feature> {
    let reach = tolerance.into_inner();
    let side = |offset: f64| if offset.abs() <= reach { Ordering::Equal } else { offset.total_cmp(&0.0) };
    prepared
        .mesh
        .bvh()
        .leaves(|node| {
            let corners = node.aabb().vertices().map(|corner| depth(frame, &Point3::from(corner)));
            corners.iter().any(|&corner| corner <= reach) && corners.iter().any(|&corner| corner >= -reach)
        })
        .filter_map(|face| {
            let corners = prepared.mesh.triangle(face).vertices().map(|vertex| {
                let point = Point3::from(vertex);
                (point, depth(frame, &point))
            });
            corners
                .iter()
                .circular_tuple_windows()
                .filter_map(|(&(from, before), &(to, after))| match (side(before), side(after)) {
                    (Ordering::Equal, _) => Some(from),
                    (Ordering::Less, Ordering::Greater) => Some(from.lerp(&to, before / (before - after))),
                    (Ordering::Greater, Ordering::Less) => Some(to.lerp(&from, after / (after - before))),
                    (Ordering::Less | Ordering::Greater, Ordering::Equal) | (Ordering::Less, Ordering::Less) | (Ordering::Greater, Ordering::Greater) => None,
                })
                .collect_array::<2>()
                .filter(|_| corners.iter().any(|&(_, beyond)| side(beyond) == Ordering::Greater))
                .map(|ends| Feature { ends, kind: Some(EdgeKind::Cut) })
        })
        .collect()
}
