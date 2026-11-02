use nalgebra::{Matrix3, Point3};
use parry3d_f64::{mass_properties::MassProperties, math::Vector, shape::TriMesh};

use crate::{MeshError, Solid};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Measures {
    pub volume: f64,
    pub surface_area: f64,
    pub genus: i32,
    pub centroid: Point3<f64>,
    pub inertia: Matrix3<f64>,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Curvature {
    pub gaussian: f64,
    pub mean: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn measure(solid: &Solid) -> Measures {
    let (vertices, indices) = crate::boundary(&solid.0);
    let mass = MassProperties::from_trimesh(1.0, &vertices, &indices);
    Measures {
        volume: solid.0.volume(),
        surface_area: solid.0.surface_area(),
        genus: solid.0.genus(),
        centroid: Point3::from(mass.local_com),
        inertia: Matrix3::from(mass.reconstruct_inertia_matrix()),
    }
}

pub fn curvature(solid: &Solid) -> Result<(TriMesh, Vec<Curvature>), MeshError> {
    let buffers = solid.0.calculate_curvature(0, 1).get_mesh_glp::<f64, u32>(-1);
    let (vertices, values) = buffers.vert_properties.as_chunks::<5>().0.iter().map(|&[x, y, z, gaussian, mean]| (Vector::new(x, y, z), Curvature { gaussian, mean })).unzip();
    Ok((TriMesh::new(vertices, buffers.tri_verts.as_chunks::<3>().0.to_vec())?, values))
}
