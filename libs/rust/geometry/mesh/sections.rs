use core::num::NonZero;
use std::borrow::Cow;
use std::collections::HashSet;

use itertools::Itertools as _;
use manifold_rust::cancel::CancelToken;
use manifold_rust::linalg;
use nalgebra::{Isometry3, Point2, Point3, Vector3};
use parry3d_f64::query::{IntersectResult, SplitResult};
use parry3d_f64::shape::TriMesh;

use crate::{MeshError, Operation, Solid, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Contour {
    Section { height: f64 },
    Silhouette,
}

#[derive(Debug, Clone)]
pub struct Trim<'solid> {
    pub solid: Cow<'solid, Solid>,
    pub cap: Option<NonZero<u32>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn trim<'solid>(solid: &'solid Solid, plane: &Isometry3<f64>, token: Option<&CancelToken>) -> Result<Option<Trim<'solid>>, MeshError> {
    fn originals(solid: &Solid) -> impl Iterator<Item = i32> { solid.0.as_impl().mesh_relation.mesh_id_transform.values().map(|relation| relation.original_id) }
    let (normal, bias) = halfspace(plane);
    let epsilon = solid.0.get_epsilon();
    match solid.0.as_impl().vert_pos.iter().map(|&vertex| linalg::dot(normal, vertex) - bias).minmax_by(f64::total_cmp).into_option() {
        Some((nearest, _)) if nearest >= -epsilon => Ok(Some(Trim { solid: Cow::Borrowed(solid), cap: None })),
        Some((_, farthest)) if farthest > epsilon => Ok(Solid::checked(Operation::Trim, solid.0.trim_by_plane(normal, bias), token)?.map(|trimmed| {
            let retained = originals(solid).collect::<HashSet<_>>();
            let cap = originals(&trimmed).find(|cap| !retained.contains(cap)).and_then(crate::original_id);
            Trim { solid: Cow::Owned(trimmed), cap }
        })),
        Some(_) | None => Ok(None),
    }
}

#[must_use]
pub fn contours(solid: &Solid, plane: &Isometry3<f64>, contours: impl IntoIterator<Item = Contour>) -> Vec<Vec<Vec<Vec<Point2<f64>>>>> {
    let local = solid.0.transform(&crate::columns(&plane.inverse().to_homogeneous()));
    contours
        .into_iter()
        .map(|contour| match contour {
            Contour::Section { height } => local.slice(height),
            Contour::Silhouette => local.project(),
        })
        .map(|section| section.decompose().into_iter().map(|polygon| polygon.to_polygons().into_iter().map(|ring| ring.into_iter().map(crate::coordinates).collect()).collect()).collect())
        .collect()
}

#[must_use]
pub fn split(mesh: &TriMesh, plane: &Isometry3<f64>, epsilon: Option<Tolerance>) -> SplitResult<TriMesh> {
    let (normal, bias) = halfspace(plane);
    mesh.local_split(normal, bias, crate::tolerance(epsilon))
}

#[must_use]
pub fn segments(mesh: &TriMesh, plane: &Isometry3<f64>, epsilon: Option<Tolerance>) -> Vec<[Point2<f64>; 2]> {
    let (normal, bias) = halfspace(plane);
    match mesh.intersection_with_local_plane(normal, bias, crate::tolerance(epsilon)) {
        IntersectResult::Intersect(polyline) => polyline.segments().map(|segment| [segment.a, segment.b].map(|vertex| plane.inverse_transform_point(&Point3::from(vertex)).xy())).collect(),
        IntersectResult::Negative | IntersectResult::Positive => Vec::new(),
    }
}

fn halfspace<V>(plane: &Isometry3<f64>) -> (V, f64)
where V: From<[f64; 3]> {
    let normal = plane.rotation * Vector3::z();
    (crate::coordinates(normal), normal.dot(&plane.translation.vector))
}
