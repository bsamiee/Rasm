use manifold_rust::cancel::CancelToken;
use meshopt::{SimplifyOptions, SimplifyVertex};
use nalgebra::Point3;
use parry3d_f64::{
    bounding_volume::Aabb,
    math::Vector,
    shape::{TriMesh, TriMeshBuilderError},
};

use crate::{Defect, MeshError, Operation, Solid, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Deviation(f32);

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Simplifier {
    Topology(SimplifyOptions),
    Sloppy,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Reduction {
    pub target_error: Deviation,
    pub triangles: usize = 0,
    pub simplifier: Simplifier = Simplifier::Topology(SimplifyOptions::empty()),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Cloud<'points> {
    Plain(&'points [Point3<f64>]),
    Colored { points: &'points [(Point3<f64>, [f32; 3])], color_weight: f32 },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reduce(reduction: Reduction, lock: impl FnMut(usize) -> SimplifyVertex, mesh: &TriMesh) -> Result<Option<(TriMesh, f64)>, MeshError> {
    let Reduction { target_error, triangles, simplifier } = reduction;
    let positions = positions(mesh.vertices().iter().copied().map(Point3::from), Point3::from(mesh.local_aabb().center()));
    let locks = Vec::from_fn(positions.len(), lock);
    let indices = mesh.indices().as_flattened();
    let target = triangles.saturating_mul(3).min(indices.len());
    let (simplified, result_error) = match simplifier {
        Simplifier::Topology(options) => {
            let mut result_error = 0.0;
            (meshopt::simplify_with_locks_decoder(indices, &positions, &locks, target, target_error.into_inner(), options | SimplifyOptions::ErrorAbsolute, Some(&mut result_error)), result_error)
        }
        Simplifier::Sloppy => {
            let (scale, mut result_error) = (meshopt::simplify_scale_decoder(&positions), 0.0);
            (meshopt::simplify_sloppy_with_locks_decoder(indices, &positions, &locks, target, (target_error.into_inner() / scale).min(1.0), Some(&mut result_error)), result_error * scale)
        }
    };
    let mut ordered = meshopt::optimize_vertex_cache(&simplified, positions.len());
    let vertices = meshopt::optimize_vertex_fetch(&mut ordered, mesh.vertices());
    (!ordered.is_empty())
        .then(|| {
            let mut reduced = crate::trimesh(vertices, ordered.into_chunks::<3>(), Operation::Reduce)?;
            reduced.set_flags(mesh.flags()).map_err(|error| MeshError::Mesh { operation: Operation::Reduce, source: TriMeshBuilderError::TopologyError(error) })?;
            Ok((reduced, f64::from(result_error)))
        })
        .transpose()
}

pub fn simplify(solid: &Solid, tolerance: Option<Tolerance>, token: Option<&CancelToken>) -> Result<Option<Solid>, MeshError> {
    Solid::checked(Operation::Simplify, solid.0.simplify(tolerance.map_or_default(Tolerance::into_inner)), token, Some(Defect::SelfIntersecting))
}

#[must_use]
pub fn reduce_points(cloud: Cloud<'_>, target: usize) -> Vec<u32> {
    let centred = |points: &[Point3<f64>]| positions(points.iter().copied(), Point3::from(Aabb::from_points(points.iter().map(|point| Vector::from(point.coords))).center()));
    match cloud {
        Cloud::Plain([]) | Cloud::Colored { points: [], color_weight: _ } => Vec::new(),
        Cloud::Plain(points) => meshopt::simplify_points_decoder(&centred(points), None, 0.0, target.min(points.len())),
        Cloud::Colored { points, color_weight } => {
            let (points, colors) = points.iter().copied().unzip::<_, _, Vec<_>, Vec<_>>();
            meshopt::simplify_points_decoder(&centred(&points), Some(&colors), color_weight, target.min(points.len()))
        }
    }
}

fn positions(points: impl Iterator<Item = Point3<f64>>, origin: Point3<f64>) -> Vec<[f32; 3]> { points.map(|point| <[f32; 3]>::from((point - origin).cast::<f32>())).collect() }
