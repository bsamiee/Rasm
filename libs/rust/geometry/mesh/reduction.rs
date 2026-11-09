use std::io::Cursor;

use itertools::Either;
use manifold_rust::cancel::CancelToken;
use meshopt::{SimplifyOptions, SimplifyVertex, VertexDataAdapter};
use nalgebra::Point3;
use parry3d_f64::{
    bounding_volume::Aabb,
    math::Vector,
    shape::{TriMesh, TriMeshBuilderError},
};

use crate::{MeshError, Operation, Solid, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Simplifier {
    Topology(SimplifyOptions),
    Sloppy,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Reduction {
    pub target_error: Option<Tolerance> = None,
    pub triangles: usize = 0,
    pub simplifier: Simplifier = Simplifier::Topology(SimplifyOptions::empty()),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Cloud<'points> {
    Plain(&'points [Point3<f64>]),
    Colored {
        points: &'points [(Point3<f64>, [f32; 3])],
        color_weight: f32,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reduce(reduction: Reduction, lock: impl FnMut(usize) -> SimplifyVertex, mesh: &TriMesh) -> Result<Option<(TriMesh, f64)>, MeshError> {
    let Reduction { target_error, triangles, simplifier } = reduction;
    let positions = positions(mesh.vertices().iter().copied().map(Point3::from), mesh.local_aabb());
    let vertices = adapter(&positions);
    let locks = Vec::from_fn(positions.len(), lock);
    let indices = mesh.indices().as_flattened();
    let target = triangles.saturating_mul(3).min(indices.len());
    let target_error = crate::tolerance(target_error).cast::<f32>();
    let mut result_error = 0.0;
    let (simplified, scale) = match simplifier {
        Simplifier::Topology(options) => (meshopt::simplify_with_locks(indices, &vertices, &locks, target, target_error, options | SimplifyOptions::ErrorAbsolute, Some(&mut result_error)), 1.0),
        Simplifier::Sloppy => {
            let scale = meshopt::simplify_scale(&vertices);
            (meshopt::simplify_sloppy_with_locks(indices, &vertices, &locks, target, (target_error / scale).min(1.0), Some(&mut result_error)), scale)
        }
    };
    let mut ordered = meshopt::optimize_vertex_cache(&simplified, positions.len());
    match crate::trimesh(meshopt::optimize_vertex_fetch(&mut ordered, mesh.vertices()), ordered, mesh.flags(), Operation::Reduce) {
        Ok(reduced) => Ok(Some((reduced, f64::from(result_error * scale)))),
        Err(MeshError::Mesh { operation: _, source: TriMeshBuilderError::EmptyIndices }) => Ok(None),
        Err(error @ (MeshError::Kernel { operation: _, status: _ } | MeshError::Invalid { operation: _, defect: _ } | MeshError::Mesh { operation: _, source: _ })) => Err(error),
    }
}

pub fn simplify(solid: &Solid, tolerance: Option<Tolerance>, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(Operation::Simplify, solid.0.simplify(crate::tolerance(tolerance)), token)
}

#[must_use]
pub fn reduce_points(cloud: Cloud<'_>, target: usize) -> Vec<u32> {
    let (points, colors, color_weight) = match cloud {
        Cloud::Plain(points) => (Either::Left(points.iter().copied()), None, 0.0),
        Cloud::Colored { points, color_weight } => (Either::Right(points.iter().map(|&(point, _)| point)), Some(points.iter().map(|&(_, color)| color).collect::<Vec<_>>()), color_weight),
    };
    match points.len() {
        0 => Vec::new(),
        count => meshopt::simplify_points(&adapter(&positions(points.clone(), Aabb::from_points(points.map(|point| Vector::from(point.coords))))), colors.as_deref(), color_weight, target.min(count)),
    }
}

fn positions(points: impl Iterator<Item = Point3<f64>>, bounds: Aabb) -> Vec<[f32; 3]> {
    let origin = Point3::from(bounds.center());
    points.map(|point| <[f32; 3]>::from((point - origin).cast::<f32>())).collect()
}

fn adapter(positions: &[[f32; 3]]) -> VertexDataAdapter<'_> {
    VertexDataAdapter { reader: Cursor::new(meshopt::typed_to_bytes(positions)), vertex_count: positions.len(), vertex_stride: size_of::<[f32; 3]>(), position_offset: 0 }
}
