use itertools::Itertools as _;
use kurbo::{BezPath, PathEl};
use linesweeper::{BinaryOp, FillRule, binary_op};
use manifold_rust::cancel::CancelToken;
use nalgebra::Point3;
use nonempty_collections::NEVec;
use parry3d_f64::{
    math::Vector,
    shape::{TriMesh, TriMeshFlags},
};
use rasm_linework::{EdgeKind, LineworkError, Region, Tolerance};
use rasm_mesh::{
    Source,
    sections::{segments, trim},
};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};
use validated::Validated;

use crate::{Body, Feature, Frame, Operation, Prepared, View, ViewsError, clip, depth, picture, visibility::window};

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sections(view: &View) -> Result<Vec<(usize, Region)>, NEVec<ViewsError>> {
    let View { frame, scene, lines: _, crease: _, tolerance } = view;
    let pictured = |vertex: Vector| picture(frame, clip(frame, &Point3::from(vertex))).ok_or(linesweeper::Error::Infinity);
    let [first, second, third, fourth] = window(frame);
    let border = [PathEl::MoveTo(first), PathEl::LineTo(second), PathEl::LineTo(third), PathEl::LineTo(fourth), PathEl::ClosePath].into_iter().collect::<BezPath>();
    scene
        .bodies
        .par_iter()
        .filter(|prepared| prepared.sources.contains(&None))
        .filter_map(|Prepared { body, mesh, sources }| {
            mesh.triangles()
                .zip(sources)
                .filter(|&(_, source)| source.is_none())
                .map(|(triangle, _)| {
                    let [start, middle, end] = triangle.vertices().map(pictured);
                    Ok::<_, linesweeper::Error>([PathEl::MoveTo(start?), PathEl::LineTo(middle?), PathEl::LineTo(end?), PathEl::ClosePath])
                })
                .flatten_ok()
                .collect::<Result<BezPath, _>>()
                .and_then(|caps| binary_op(&caps, &border, FillRule::NonZero, BinaryOp::Intersection))
                .map_err(LineworkError::from)
                .and_then(|framed| Region::new(&framed.contours().flat_map(|contour| contour.path.iter()).collect::<BezPath>(), FillRule::EvenOdd, *tolerance))
                .map(|region| (!region.is_empty()).then_some((*body, region)))
                .map_err(|source| ViewsError::Linework { operation: Operation::Region(*body), source })
                .transpose()
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub(super) fn prepared(frame: &Frame, index: usize, body: Body, token: Option<&CancelToken>) -> Result<Option<Prepared>, ViewsError> {
    let refused = move |source| ViewsError::Mesh { body: index, source };
    let flagged = |mut mesh: TriMesh, sources: Vec<Option<Source>>| {
        mesh.set_flags(mesh.flags() | TriMeshFlags::HALF_EDGE_TOPOLOGY | TriMeshFlags::MERGE_DUPLICATE_VERTICES)
            .map(|()| Prepared { body: index, mesh, sources })
            .map_err(|source| ViewsError::Topology { body: index, source })
    };
    match body {
        Body::Solid(solid) => {
            let (exported, faces) = solid.to_mesh().map_err(refused)?;
            match exported.vertices().iter().map(|&vertex| depth(frame, &Point3::from(vertex))).minmax_by(f64::total_cmp).into_option() {
                Some((nearest, _)) if nearest >= 0.0 => Some((exported, faces.into_iter().map(Some).collect())),
                Some((_, farthest)) if farthest > 0.0 => trim(&solid, &frame.plane, token)
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
            .map(|(mesh, sources)| flagged(mesh, sources))
            .transpose()
        }
        Body::Surface(mesh) => {
            let sources = (0..).take(mesh.num_triangles()).map(|face| Some(Source { original: None, face })).collect();
            flagged(mesh, sources).map(Some)
        }
    }
}

pub(super) fn cuts(frame: &Frame, prepared: &Prepared, tolerance: Tolerance) -> Vec<Feature> {
    segments(&prepared.mesh, &frame.plane, rasm_mesh::Tolerance::try_new(tolerance.into_inner()).ok())
        .into_iter()
        .map(|ends| Feature { ends: ends.map(|end| frame.plane * Point3::new(end.x, end.y, 0.0)), kind: Some(EdgeKind::Cut) })
        .collect()
}
