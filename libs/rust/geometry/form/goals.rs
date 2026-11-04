use core::iter;
use std::collections::BTreeMap;

use faer::{
    Accum, MatMut, MatRef, get_global_parallelism,
    linalg::solvers::Solve as _,
    sparse::{SparseColMat, Triplet, linalg::matmul::sparse_dense_matmul},
};
use itertools::Itertools as _;
use nalgebra::{DMatrixView, DVector, Isometry3, Matrix3xX, Point3, Unit, Vector3};
use nlopt::Target;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_optimize::{
    Solution,
    nonlinear::{Settings, optimize},
};
use rasm_stats::Space;
use rayon::iter::{Either, ParallelIterator as _};
use validated::Validated;

use crate::{
    Form, FormError, Input, Network, balance,
    densities::{Solved, solved},
    form, view, view_mut,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Measure {
    Position { node: usize, target: Point3<f64> },
    Plane { node: usize, plane: Isometry3<f64> },
    Length { edge: usize, target: f64 },
    Force { edge: usize, target: f64 },
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Goal {
    pub weight: f64,
    pub measure: Measure,
}

#[derive(Debug, Clone)]
pub struct Optimum {
    pub solution: Solution,
    pub form: Form,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn optima<K>(
    network: &Network,
    goals: &[Goal],
    space: &Space<K>,
    groups: &[K],
    starts: DMatrixView<'_, f64>,
    settings: &Settings<'_>,
) -> Result<Vec<Result<Optimum, NEVec<FormError>>>, NEVec<FormError>>
where
    K: Ord + Sync,
{
    let (first, _) = space.values().next();
    let side = (!first.contains(&0.0)).then_some(first.start.is_sign_positive());
    let dimensions = space.keys().into_iter().enumerate().map(|(dimension, key)| (key, dimension)).collect::<BTreeMap<_, _>>();
    let refusals = iter::chain(
        (groups.len() != network.edges.len()).then_some(FormError::Shape { input: Input::Groups, expected: network.edges.len(), found: groups.len() }),
        space
            .values()
            .into_iter()
            .positions(|bounds| bounds.contains(&0.0) || side.is_some_and(|side| bounds.start.is_sign_positive() != side))
            .map(|index| FormError::Entry { input: Input::Bounds, index }),
    )
    .map(Err)
    .collect::<Validated<(), _>>();
    let memberships = groups
        .iter()
        .enumerate()
        .map(|(edge, key)| dimensions.get(key).map(|&dimension| Triplet::new(edge, dimension, 1.0)).ok_or(FormError::Entry { input: Input::Groups, index: edge }))
        .collect::<Validated<Vec<_>, _>>();
    let rank = |node: usize| network.index.get(node).copied().flatten();
    let resolved = goals
        .iter()
        .enumerate()
        .map(|(index, &goal)| {
            match goal.measure {
                Measure::Position { node, target } => rank(node).filter(|_| target.iter().all(|coordinate| coordinate.is_finite())),
                Measure::Plane { node, plane } => rank(node).filter(|_| plane.to_homogeneous().iter().all(|entry| entry.is_finite())),
                Measure::Length { edge, target } => (edge < network.edges.len() && target.is_finite() && target >= 0.0).then_some(edge),
                Measure::Force { edge, target } => (edge < network.edges.len() && target.is_finite()).then_some(edge),
            }
            .filter(|_| goal.weight.is_finite() && goal.weight > 0.0)
            .map(|column| (column, goal))
            .ok_or(FormError::Entry { input: Input::Goals, index })
        })
        .collect::<Validated<Vec<_>, _>>();
    let (memberships, resolved) = refusals.map3(memberships, resolved, |(), triplets, columns| (triplets, columns)).ok()?;
    let grouping = SparseColMat::<usize, f64>::try_new_from_triplets(network.edges.len(), space.len().get(), &memberships).map_err(|source| NEVec::new(FormError::from(source)))?;
    Ok(starts
        .par_column_iter()
        .map(|start| -> Result<Optimum, NEVec<FormError>> {
            let mut densities = DVector::zeros(grouping.nrows());
            let solution =
                optimize(space, start.into_owned(), Target::Minimize, |point: &[f64], slot: Option<&mut [f64]>| objective(network, &resolved, &grouping, &mut densities, point, slot), settings)
                    .map_err(|failure| failure.either(NEVec::new, |failures| failures.into_nonempty_iter().map(FormError::from).collect()))?;
            grouped(&grouping, solution.point.as_slice(), &mut densities);
            let Solved { factor: _, sign: _, spans, free } = solved(network, densities.as_view()).map_err(NEVec::new)?;
            Ok(Optimum { solution, form: form(network, &free, &spans, densities.as_view(), balance(network, &spans, densities.as_view())) })
        })
        .collect())
}

fn grouped(grouping: &SparseColMat<usize, f64>, point: &[f64], densities: &mut DVector<f64>) {
    sparse_dense_matmul(
        MatMut::from_column_major_slice_mut(densities.as_mut_slice(), grouping.nrows(), 1),
        Accum::Replace,
        grouping.as_ref(),
        MatRef::from_column_major_slice(point, grouping.ncols(), 1),
        1.0,
        get_global_parallelism(),
    );
}

fn objective(network: &Network, goals: &[(usize, Goal)], grouping: &SparseColMat<usize, f64>, densities: &mut DVector<f64>, point: &[f64], slot: Option<&mut [f64]>) -> Result<f64, FormError> {
    grouped(grouping, point, densities);
    let Solved { factor, sign, spans, free } = solved(network, densities.as_view())?;
    let edge = |column: usize| Unit::try_new_and_get(spans.column(column).into_owned(), 0.0).map_or_else(|| (Vector3::zeros(), 0.0), |(direction, length)| (direction.into_inner(), length));
    let terms = goals.iter().map(|&(column, Goal { weight, measure })| match measure {
        Measure::Position { node: _, target } => {
            let residual = free.column(column) - target.coords;
            (weight * residual.norm_squared(), Either::Left((column, residual * (2.0 * weight))))
        }
        Measure::Plane { node: _, plane } => {
            let residual = plane.inverse_transform_point(&Point3::from(free.column(column).into_owned())).z;
            (weight * residual * residual, Either::Left((column, plane.rotation * Vector3::z() * (2.0 * weight * residual))))
        }
        Measure::Length { edge: _, target } => {
            let (direction, length) = edge(column);
            let residual = length - target;
            (weight * residual * residual, Either::Right((column, direction * (2.0 * weight * residual), 0.0)))
        }
        Measure::Force { edge: _, target } => {
            let ((direction, length), density) = (edge(column), densities.row(column).x);
            let residual = density.mul_add(length, -target);
            (weight * residual * residual, Either::Right((column, direction * (2.0 * weight * residual * density), 2.0 * weight * residual * length)))
        }
    });
    match slot {
        None => Ok(terms.map(|(value, _)| value).sum()),
        Some(slot) => {
            let (value, mut nodal, spanned, explicit) =
                terms.fold((0.0, Matrix3xX::zeros(free.ncols()), Matrix3xX::zeros(spans.ncols()), DVector::zeros(spans.ncols())), |(total, mut nodal, mut spanned, mut explicit), (value, partial)| {
                    match partial {
                        Either::Left((column, gradient)) => nodal.column_mut(column).axpy(1.0, &gradient, 1.0),
                        Either::Right((column, gradient, density)) => {
                            spanned.column_mut(column).axpy(1.0, &gradient, 1.0);
                            explicit.row_mut(column).add_scalar_mut(density);
                        }
                    }
                    (total + value, nodal, spanned, explicit)
                });
            sparse_dense_matmul(view_mut(&mut nodal), Accum::Add, network.free.transpose(), view(&spanned), 1.0, get_global_parallelism());
            factor.solve_in_place(view_mut(&mut nodal));
            let mut adjoint = Matrix3xX::zeros(spans.ncols());
            sparse_dense_matmul(view_mut(&mut adjoint), Accum::Replace, network.free.as_ref(), view(&nodal), 1.0, get_global_parallelism());
            let gradient = explicit - adjoint.component_mul(&spans).row_sum_tr() * sign;
            sparse_dense_matmul(
                MatMut::from_column_major_slice_mut(slot, grouping.ncols(), 1),
                Accum::Replace,
                grouping.transpose(),
                MatRef::from_column_major_slice(gradient.as_slice(), gradient.len(), 1),
                1.0,
                get_global_parallelism(),
            );
            Ok(value)
        }
    }
}
