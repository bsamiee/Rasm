use manifold_rust::linalg::Vec3;
use nalgebra::{Isometry3, Point2, Point3, Vector3};
use parry3d_f64::{
    math::Vector,
    query::{IntersectResult, SplitResult},
    shape::TriMesh,
};

use crate::{MeshError, Operation, Solid};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Contour {
    Section,
    Silhouette,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn trim(solid: &Solid, plane: &Isometry3<f64>) -> Result<Option<Solid>, MeshError> {
    let (normal, bias) = halfspace(plane);
    Solid::checked(Operation::Trim, solid.0.trim_by_plane(Vec3::from(<[f64; 3]>::from(normal)), bias))
}

#[must_use]
pub fn contours(solid: &Solid, plane: &Isometry3<f64>, contour: Contour) -> Vec<Vec<Point2<f64>>> {
    let local = solid.0.transform(&crate::columns(&plane.inverse().to_homogeneous()));
    match contour {
        Contour::Section => local.slice(0.0),
        Contour::Silhouette => local.project(),
    }
    .to_polygons()
    .into_iter()
    .map(|ring| ring.into_iter().map(|vertex| Point2::from(<[f64; 2]>::from(vertex))).collect())
    .collect()
}

#[must_use]
pub fn split(mesh: &TriMesh, plane: &Isometry3<f64>, epsilon: f64) -> SplitResult<TriMesh> {
    let (normal, bias) = halfspace(plane);
    mesh.local_split(Vector::from(normal), bias, epsilon)
}

#[must_use]
pub fn segments(mesh: &TriMesh, plane: &Isometry3<f64>, epsilon: f64) -> Vec<[Point2<f64>; 2]> {
    let (normal, bias) = halfspace(plane);
    match mesh.intersection_with_local_plane(Vector::from(normal), bias, epsilon) {
        IntersectResult::Intersect(polyline) => polyline.segments().map(|segment| [segment.a, segment.b].map(|vertex| plane.inverse_transform_point(&Point3::from(vertex)).xy())).collect(),
        IntersectResult::Negative | IntersectResult::Positive => Vec::new(),
    }
}

fn halfspace(plane: &Isometry3<f64>) -> (Vector3<f64>, f64) {
    let normal = plane.rotation.transform_vector(&Vector3::z());
    (normal, normal.dot(&plane.translation.vector))
}
