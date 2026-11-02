use core::{cell::LazyCell, iter, num::NonZero};

use manifold_rust::{
    manifold::Manifold,
    properties::Property,
    robust::{
        exact::rational::R3,
        ray_shoot::{self, WindingIndex},
        soup,
    },
};
use parry3d_f64::{
    shape::SharedShape,
    transformation::{
        vhacd::VHACDParameters,
        voxelization::{FillMode, VoxelSet},
    },
};

use crate::Solid;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn decompose(solid: &Solid) -> Vec<Solid> {
    let (bodies, voids): (Vec<Manifold>, Vec<Manifold>) = solid.0.decompose().into_iter().partition(|shell| shell.as_impl().get_property(Property::Volume) > 0.0_f64);
    let surfaces = LazyCell::new(|| {
        bodies
            .iter()
            .map(|body| {
                let triangles = soup::impl_to_tris(body.as_impl());
                (WindingIndex::new(&triangles), triangles, body.volume())
            })
            .collect::<Vec<_>>()
    });
    let owners = voids
        .iter()
        .map(|void| {
            void.as_impl().vert_pos.first().and_then(|&vertex| {
                let probe = R3::from_vec3(vertex);
                surfaces
                    .iter()
                    .enumerate()
                    .filter(|(_, (index, triangles, _))| ray_shoot::winding_number_indexed(&probe, triangles, index) != 0_i32)
                    .min_by(|(_, (_, _, left)), (_, (_, _, right))| left.total_cmp(right))
                    .map(|(owner, _)| owner)
            })
        })
        .collect::<Vec<_>>();
    bodies
        .iter()
        .enumerate()
        .map(|(body, shell)| {
            Solid(Manifold::compose(&iter::once(shell).chain(iter::zip(&voids, &owners).filter(|&(_, owner)| *owner == Some(body)).map(|(void, _)| void)).cloned().collect::<Vec<_>>()))
        })
        .collect()
}

#[must_use]
pub fn convex_parts(solid: &Solid, parameters: &VHACDParameters) -> SharedShape {
    let (vertices, indices) = crate::boundary(&solid.0);
    SharedShape::convex_decomposition_with_params(&vertices, &indices, parameters)
}

#[must_use]
pub fn voxels(solid: &Solid, resolution: NonZero<u32>, fill_mode: FillMode) -> VoxelSet {
    let (vertices, indices) = crate::boundary(&solid.0);
    VoxelSet::voxelize(&vertices, &indices, resolution.get(), fill_mode, false)
}
