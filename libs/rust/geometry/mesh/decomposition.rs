use core::iter;
use std::sync::OnceLock;

use itertools::Itertools as _;
use manifold_rust::{
    boolean3::compose_meshes,
    impl_mesh::ManifoldImpl,
    linalg::Vec3,
    manifold::Manifold,
    properties::Property,
    robust::{
        exact::rational::R3,
        ray_shoot::{self, WindingIndex},
        soup,
    },
};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};

use crate::Solid;

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn decompose(solid: &Solid) -> Vec<Solid> {
    let winding = |body: &ManifoldImpl, surface: &OnceLock<(WindingIndex, Vec<[Vec3; 3]>)>, probe: &R3| {
        let (index, triangles) = surface.get_or_init(|| {
            let triangles = soup::impl_to_tris(body);
            (WindingIndex::new(&triangles), triangles)
        });
        ray_shoot::winding_number_indexed(probe, triangles, index)
    };
    let (bodies, voids): (Vec<_>, Vec<_>) = solid
        .0
        .decompose()
        .into_iter()
        .map(Manifold::into_impl)
        .map(|shell| (shell.get_property(Property::Volume), shell, OnceLock::new()))
        .sorted_by(|(left, _, _), (right, _, _)| left.total_cmp(right))
        .partition(|&(volume, _, _)| volume > 0.0_f64);
    let mut enclosed = voids
        .into_par_iter()
        .filter_map(|(_, void, _)| {
            let probe = R3::from_vec3(void.vert_pos[0]);
            bodies.iter().position(|(_, body, surface)| body.bbox.contains_box(&void.bbox) && winding(body, surface, &probe) != 0_i32).map(|owner| (owner, void))
        })
        .collect::<Vec<_>>()
        .into_iter()
        .into_group_map();
    bodies
        .into_iter()
        .enumerate()
        .map(|(body, (_, shell, _))| (shell, enclosed.remove(&body)))
        .collect::<Vec<_>>()
        .into_par_iter()
        .map(|(shell, cavities)| match cavities {
            None => shell,
            Some(cavities) => compose_meshes(&iter::once(shell).chain(cavities).collect::<Vec<_>>()),
        })
        .map(|shell| Solid(Manifold::from_impl(shell)))
        .collect()
}
