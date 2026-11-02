use core::{iter, num::NonZero};
use std::collections::BTreeSet;

use average::Variance;
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrix, DMatrixView, DVector};
use nonempty_collections::{NEBTreeMap, NonEmptyIterator as _};
use rand::{Rng, RngExt as _, seq::SliceRandom as _};
use rayon::iter::{IntoParallelRefIterator as _, ParallelIterator as _};

use crate::{
    Space, StatsError,
    sampling::{place, sobol},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Indices {
    pub first: Variance,
    pub total: Variance,
}

#[derive(Debug, derive_more::AsRef)]
pub struct Saltelli<'space, K> {
    space: &'space Space<K>,
    #[as_ref]
    points: DMatrix<f64>,
}

impl<'space, K> Saltelli<'space, K> {
    #[must_use]
    pub fn new<R>(space: &'space Space<K>, base: NonZero<u16>, rng: &mut R) -> Self
    where R: Rng + ?Sized {
        let dimensions = space.len().get();
        let pairs = sobol(dimensions.strict_mul(2), base, rng.next_u32());
        let (sample, resample) = pairs.rows_range_pair(..dimensions, dimensions..);
        Self {
            space,
            points: place(
                space,
                DMatrix::from_columns(
                    &iter::zip(sample.column_iter(), resample.column_iter())
                        .flat_map(|(point, partner)| {
                            iter::once(point.clone_owned())
                                .chain((0..dimensions).map(move |dimension| {
                                    DVector::from_iterator(
                                        dimensions,
                                        iter::zip(point, partner).enumerate().map(|(row, (&coordinate, &replacement))| if row == dimension { replacement } else { coordinate }),
                                    )
                                }))
                                .chain(iter::once(partner.clone_owned()))
                        })
                        .collect::<Vec<_>>(),
                ),
            ),
        }
    }

    pub fn indices(&self, outputs: &[f64]) -> Result<NEBTreeMap<&'space K, Indices>, StatsError>
    where K: Ord {
        evaluated(&self.points, outputs)?;
        let dimensions = self.space.len().get();
        let width = dimensions.strict_add(2);
        let runs = DMatrixView::from_slice(outputs, width, outputs.len().strict_div(width));
        let moments = iter::chain(runs.row(0), runs.row(dimensions.strict_add(1))).collect::<Variance>();
        let standard = runs.add_scalar(-moments.mean()).unscale(moments.population_variance().sqrt());
        let (sample, resample) = (standard.row(0), standard.row(dimensions.strict_add(1)));
        Ok(self
            .space
            .keys()
            .enumerate()
            .map(|(dimension, key)| {
                let (first, total) =
                    izip!(sample, standard.row(dimension.strict_add(1)), resample).map(|(kept, crossed, swapped)| (swapped * (crossed - kept), (kept - crossed).powi(2) / 2.0_f64)).unzip();
                (key, Indices { first, total })
            })
            .collect())
    }
}

#[derive(Debug, Clone)]
pub struct Effects {
    pub signed: Variance,
    pub absolute: Variance,
}

#[nutype::nutype(validate(greater_or_equal = 2, predicate = |levels| levels.is_multiple_of(2)), derive(Debug, Clone, Copy, PartialEq, Eq))]
pub struct Levels(u16);

#[derive(Debug)]
struct Step {
    dimension: usize,
    delta: f64,
}

#[derive(Debug, derive_more::AsRef)]
pub struct Morris<'space, K> {
    space: &'space Space<K>,
    #[as_ref]
    points: DMatrix<f64>,
    steps: Box<[Step]>,
}

impl<'space, K> Morris<'space, K> {
    #[must_use]
    pub fn new<R>(space: &'space Space<K>, trajectories: NonZero<u16>, surplus: u16, levels: Levels, rng: &mut R) -> Self
    where R: Rng + ?Sized {
        let dimensions = space.len().get();
        let grid = levels.into_inner();
        let half = grid.strict_div(2);
        let span = f64::from(grid.strict_sub(1));
        let jump = f64::from(half) / span;
        let candidates = iter::repeat_with(|| {
            let mut ranks = (0..dimensions).collect::<Vec<_>>();
            ranks.shuffle(rng);
            let moves = ranks
                .into_iter()
                .map(|rank| {
                    let level = f64::from(rng.random_range(0..half)) / span;
                    if rng.random_bool(0.5) { (rank, level, jump) } else { (rank, level + jump, -jump) }
                })
                .collect::<Vec<_>>();
            (
                DMatrix::from_columns(
                    &(0..=dimensions).map(|point| DVector::from_iterator(dimensions, moves.iter().map(|&(rank, start, delta)| if rank < point { start + delta } else { start }))).collect::<Vec<_>>(),
                ),
                moves.iter().enumerate().sorted_unstable_by_key(|&(_, &(rank, _, _))| rank).map(|(dimension, &(_, _, delta))| Step { dimension, delta }).collect::<Vec<_>>(),
            )
        })
        .take(NonZero::<usize>::from(trajectories).get().strict_add(usize::from(surplus)))
        .collect::<Vec<_>>();
        let squared = DMatrix::from_vec(
            candidates.len(),
            candidates.len(),
            candidates
                .par_iter()
                .flat_map_iter(|(left, _)| {
                    candidates.iter().map(move |(right, _)| left.column_iter().cartesian_product(right.column_iter()).map(|(point, other)| point.metric_distance(&other)).sum::<f64>().powi(2))
                })
                .collect(),
        );
        let spread = |mut score: DVector<f64>, pick: usize| {
            score.axpy(1.0_f64, &squared.column(pick), 1.0_f64);
            score.row_mut(pick).fill(f64::NEG_INFINITY);
            score
        };
        let seed = squared.column_sum().argmax().0;
        let picks = iter::once(seed)
            .chain((1..trajectories.get()).scan(spread(DVector::zeros(candidates.len()), seed), |score, _| {
                let pick = score.argmax().0;
                *score = spread(score.clone(), pick);
                Some(pick)
            }))
            .collect::<BTreeSet<_>>();
        let chosen = candidates.into_iter().enumerate().filter_map(|(index, candidate)| picks.contains(&index).then_some(candidate)).collect::<Vec<_>>();
        Self {
            space,
            points: place(space, DMatrix::from_columns(&chosen.iter().flat_map(|(trajectory, _)| trajectory.column_iter()).collect::<Vec<_>>())),
            steps: chosen.into_iter().flat_map(|(_, steps)| steps).collect(),
        }
    }

    pub fn effects(&self, outputs: &[f64]) -> Result<NEBTreeMap<&'space K, Effects>, StatsError>
    where K: Ord {
        evaluated(&self.points, outputs)?;
        let dimensions = self.space.len().get();
        let elementary = DMatrix::from_columns(
            &outputs
                .chunks_exact(dimensions.strict_add(1))
                .zip(self.steps.chunks_exact(dimensions))
                .map(|(runs, steps)| {
                    DVector::from_iterator(
                        dimensions,
                        runs.array_windows::<2>()
                            .zip(steps)
                            .map(|(&[before, after], step)| (step.dimension, (after - before) / step.delta))
                            .sorted_unstable_by_key(|&(dimension, _)| dimension)
                            .map(|(_, effect)| effect),
                    )
                })
                .collect::<Vec<_>>(),
        );
        Ok(self
            .space
            .keys()
            .enumerate()
            .map(|(dimension, key)| (key, Effects { signed: elementary.row(dimension).iter().copied().collect(), absolute: elementary.row(dimension).iter().map(|effect| effect.abs()).collect() }))
            .collect())
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn evaluated(points: &DMatrix<f64>, outputs: &[f64]) -> Result<(), StatsError> { (outputs.len() == points.ncols()).ok_or(StatsError::Outputs { expected: points.ncols(), found: outputs.len() }) }
