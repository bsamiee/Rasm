use parry3d_f64::mass_properties::MassProperties;
use parry3d_f64::math::Vector;
use parry3d_f64::shape::{TriMesh, TriMeshFlags};

use crate::{MeshError, Operation, Solid};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Measures {
    pub mass_properties: MassProperties,
    pub surface_area: f64,
    pub genus: i32,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Curvature {
    pub gaussian: f64,
    pub mean: f64,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn measure(solid: &Solid) -> Measures {
    let surface = solid.0.as_impl();
    let vertices = surface.vert_pos.iter().map(|&vertex| crate::coordinates(vertex)).collect::<Vec<Vector>>();
    let indices = surface.halfedge.as_chunks::<3>().0.iter().map(|edges| edges.map(|edge| edge.start_vert.cast_unsigned())).collect::<Vec<_>>();
    Measures { mass_properties: MassProperties::from_trimesh(1.0, &vertices, &indices), surface_area: solid.0.surface_area(), genus: solid.0.genus() }
}

pub fn curvature(solid: &Solid) -> Result<(TriMesh, Vec<Curvature>), MeshError> {
    let buffers = solid.0.calculate_curvature(0, 1).get_mesh_glp::<f64, u32>(-1);
    let (vertices, values) = buffers.vert_properties.as_chunks::<5>().0.iter().map(|&[x, y, z, gaussian, mean]| (Vector::new(x, y, z), Curvature { gaussian, mean })).unzip();
    crate::trimesh(vertices, buffers.tri_verts, TriMeshFlags::empty(), Operation::Export).map(|mesh| (mesh, values))
}
