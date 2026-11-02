use meshopt::{SimplifyOptions, SimplifyVertex};
use nalgebra::{Point3, Translation3};
use parry3d_f64::shape::TriMesh;

use crate::{MeshError, Operation, Solid};

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
    pub deviation: Deviation,
    pub triangles: usize = 0,
    pub simplifier: Simplifier = Simplifier::Topology(SimplifyOptions::empty()),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn reduce(reduction: Reduction, lock: impl FnMut(usize) -> SimplifyVertex, mesh: &TriMesh) -> Result<Option<(TriMesh, f64)>, MeshError> {
    let Reduction { deviation, triangles, simplifier } = reduction;
    let positions = positions(mesh.vertices().iter().copied().map(Point3::from), Point3::from(mesh.local_aabb().center()));
    let locks = Vec::from_fn(positions.len(), lock);
    let indices = mesh.indices().as_flattened();
    let (target, scale) = (triangles.saturating_mul(3).min(indices.len()), meshopt::simplify_scale_decoder(&positions));
    let mut achieved = 0.0;
    let (simplified, extent) = match simplifier {
        Simplifier::Topology(options) =>
            (meshopt::simplify_with_locks_decoder(indices, &positions, &locks, target, deviation.into_inner(), options | SimplifyOptions::ErrorAbsolute, Some(&mut achieved)), 1.0),
        Simplifier::Sloppy => (meshopt::simplify_sloppy_with_locks_decoder(indices, &positions, &locks, target, (deviation.into_inner() / scale).min(1.0), Some(&mut achieved)), scale),
    };
    let mut ordered = meshopt::optimize_vertex_cache(&simplified, positions.len());
    let vertices = meshopt::optimize_vertex_fetch(&mut ordered, mesh.vertices());
    (!ordered.is_empty()).then(|| crate::trimesh(vertices, ordered.as_chunks::<3>().0.to_vec(), mesh.flags()).map(|reduced| (reduced, f64::from(achieved * extent)))).transpose()
}

pub fn simplify(solid: &Solid, tolerance: f64) -> Result<Option<Solid>, MeshError> { Solid::checked(Operation::Simplify, solid.0.simplify(tolerance)) }

#[must_use]
pub fn reduce_points(points: &[Point3<f64>], target: usize) -> Vec<u32> {
    match points {
        [] => Vec::new(),
        [origin, ..] => meshopt::simplify_points_decoder(&positions(points.iter().copied(), *origin), None, 0.0, target.min(points.len())),
    }
}

fn positions(points: impl Iterator<Item = Point3<f64>>, origin: Point3<f64>) -> Vec<[f32; 3]> {
    let frame = Translation3::from(origin.coords);
    points.map(|point| <[f32; 3]>::from(frame.inverse_transform_point(&point).cast::<f32>())).collect()
}
