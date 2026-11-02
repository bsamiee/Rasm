use core::{iter, num::NonZero};
use std::sync::OnceLock;

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
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};

use crate::Solid;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn decompose(solid: &Solid) -> Vec<Solid> {
    let (mut bodies, voids): (Vec<_>, Vec<_>) =
        solid.0.decompose().into_iter().map(|shell| (shell.as_impl().get_property(Property::Volume), shell, OnceLock::new())).partition(|&(volume, _, _)| volume > 0.0_f64);
    bodies.sort_by(|(left, _, _), (right, _, _)| left.total_cmp(right));
    let mut owned = voids
        .into_par_iter()
        .filter_map(|(_, void, _)| {
            let probe = R3::from_vec3(*void.as_impl().vert_pos.first()?);
            bodies
                .iter()
                .position(|(_, body, surface)| {
                    body.as_impl().bbox.contains_box(&void.as_impl().bbox) && {
                        let (index, triangles) = surface.get_or_init(|| {
                            let triangles = soup::impl_to_tris(body.as_impl());
                            (WindingIndex::new(&triangles), triangles)
                        });
                        ray_shoot::winding_number_indexed(&probe, triangles, index) != 0_i32
                    }
                })
                .map(|owner| (owner, void))
        })
        .collect::<Vec<_>>();
    owned.sort_by_key(|&(owner, _)| owner);
    let mut owned = owned.into_iter().peekable();
    bodies
        .into_iter()
        .enumerate()
        .map(|(body, (_, shell, _))| iter::once(shell).chain(iter::from_fn(|| owned.next_if(|&(owner, _)| owner == body).map(|(_, void)| void))).collect::<Vec<_>>())
        .collect::<Vec<_>>()
        .into_par_iter()
        .map(|shells| match <[Manifold; 1]>::try_from(shells) {
            Ok([body]) => Solid(body),
            Err(shells) => Solid(Manifold::compose(&shells)),
        })
        .collect()
}

#[must_use]
pub fn convex_parts(solid: &Solid, parameters: &VHACDParameters) -> SharedShape {
    let (vertices, indices) = crate::boundary(solid.0.get_mesh_glp(-1));
    SharedShape::convex_decomposition_with_params(&vertices, &indices, parameters)
}

#[must_use]
pub fn voxels(solid: &Solid, resolution: NonZero<u32>, fill_mode: FillMode, keep_voxel_to_primitives_map: bool) -> VoxelSet {
    let (vertices, indices) = crate::boundary(solid.0.get_mesh_glp(-1));
    VoxelSet::voxelize(&vertices, &indices, resolution.get(), fill_mode, keep_voxel_to_primitives_map)
}
