use core::num::NonZero;
use core::{iter, ptr};
use std::collections::BTreeSet;

use average::{Covariance, Variance};
use itertools::{Itertools as _, izip};
use nalgebra::{DMatrix, DMatrixView, DVector};
use nonempty_collections::{NEBTreeMap, NonEmptyIterator as _};
use rand::seq::SliceRandom as _;
use rand::{Rng, RngExt as _};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};

use crate::descriptive::symmetric;
use crate::sampling::{Window, place, sobol};
use crate::{Moment, Sample, Space, StatsError};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Outputs<'space, K, E> = Vec<Result<Sensitivity<'space, K, E>, StatsError>>;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Sensitivity<'space, K, E> {
    pub dimensions: NEBTreeMap<&'space K, E>,
    pub non_finite: u64,
}

#[derive(Debug, Clone)]
pub struct Indices {
    pub first: Covariance,
    pub total: Variance,
    pub variance: Variance,
}

#[derive(Debug, derive_more::AsRef)]
pub struct Saltelli<'space, K> {
    space: &'space Space<K>,
    #[as_ref]
    points: DMatrix<f64>,
}

impl<'space, K> Saltelli<'space, K> {
    #[must_use]
    pub fn new(space: &'space Space<K>, window: Window, seed: u32) -> Self {
        let dimensions = space.len().get();
        let width = dimensions.strict_add(2);
        let pairs = sobol(dimensions.strict_mul(2), window, seed);
        let (sample, resample) = pairs.rows_range_pair(..dimensions, dimensions..);
        Self {
            space,
            points: place(
                space,
                DMatrix::from_iterator(
                    dimensions,
                    pairs.ncols().strict_mul(width),
                    iter::zip(sample.column_iter(), resample.column_iter()).flat_map(|(point, partner)| {
                        (0..width).flat_map(move |slot| {
                            iter::zip(point, partner)
                                .enumerate()
                                .map(move |(row, (&coordinate, &replacement))| if slot == row.strict_add(1) || slot == width.strict_sub(1) { replacement } else { coordinate })
                        })
                    }),
                ),
            ),
        }
    }

    pub fn indices(&self, outputs: DMatrixView<'_, f64>) -> Result<Outputs<'space, K, Indices>, StatsError>
    where K: Ord + Sync {
        let width = self.space.len().get().strict_add(2);
        grouped(&self.points, outputs, width, &iter::repeat(()), |runs, _| {
            let (sample, resample) = (runs.row(0), runs.row(width.strict_sub(1)));
            let variance = iter::chain(sample.iter(), resample.iter()).copied().collect::<Variance>();
            (variance.population_variance() > 0.0).ok_or(StatsError::Zero(Moment::Variance))?;
            Ok(self
                .space
                .keys()
                .enumerate()
                .map(|(dimension, key)| {
                    let crossed = runs.row(dimension.strict_add(1));
                    (
                        key,
                        Indices {
                            first: izip!(resample.iter(), crossed.iter(), sample.iter()).map(|(&swapped, &crossed, &kept)| (swapped, crossed - kept)).collect(),
                            total: iter::zip(sample.iter(), crossed.iter()).map(|(kept, crossed)| (kept - crossed).powi(2) / 2.0_f64).collect(),
                            variance: variance.clone(),
                        },
                    )
                })
                .collect())
        })
    }
}

#[derive(Debug, Clone)]
pub struct Effects {
    pub signed: Variance,
    pub absolute: Variance,
}

#[nutype::nutype(validate(greater_or_equal = 2, predicate = |levels| levels.is_multiple_of(2)), derive(Debug, Clone, Copy, PartialEq, Eq))]
pub struct Levels(u16);

#[derive(Debug, derive_more::AsRef)]
pub struct Morris<'space, K> {
    space: &'space Space<K>,
    #[as_ref]
    points: DMatrix<f64>,
    steps: Box<[(usize, f64)]>,
}

impl<'space, K> Morris<'space, K> {
    #[must_use]
    pub fn new<R>(space: &'space Space<K>, trajectories: NonZero<u16>, surplus: Option<NonZero<u16>>, levels: Levels, rng: &mut R) -> Self
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
                DMatrix::from_row_iterator(
                    dimensions,
                    dimensions.strict_add(1),
                    moves.iter().flat_map(|&(rank, start, delta)| (0..=dimensions).map(move |point| if rank < point { start + delta } else { start })),
                ),
                moves.iter().map(|&(position, _, delta)| (position, delta)).collect::<Vec<_>>(),
            )
        })
        .take(NonZero::<usize>::from(trajectories).get().strict_add(surplus.map_or_default(|surplus| NonZero::<usize>::from(surplus).get())))
        .collect::<Vec<_>>();
        let chosen = if surplus.is_some() {
            let distance =
                |left: &DMatrix<f64>, right: &DMatrix<f64>| left.column_iter().cartesian_product(right.column_iter()).map(|(point, other)| point.metric_distance(&other)).sum::<f64>().powi(2);
            let squared = symmetric(&candidates, |(left, _), (right, _)| if ptr::eq(left, right) { 0.0_f64 } else { distance(left, right) });
            let spread = |score: &mut DVector<f64>, pick: usize| {
                score.axpy(1.0_f64, &squared.column(pick), 1.0_f64);
                score.row_mut(pick).fill(f64::NEG_INFINITY);
            };
            let seed = squared.column_sum().argmax().0;
            let mut initial = DVector::zeros(candidates.len());
            spread(&mut initial, seed);
            let picks = iter::once(seed)
                .chain((1..trajectories.get()).scan(initial, |score, _| {
                    let pick = score.argmax().0;
                    spread(score, pick);
                    Some(pick)
                }))
                .collect::<BTreeSet<_>>();
            candidates.into_iter().enumerate().filter_map(|(index, candidate)| picks.contains(&index).then_some(candidate)).collect()
        } else {
            candidates
        };
        Self {
            space,
            points: place(space, DMatrix::from_iterator(dimensions, chosen.len().strict_mul(dimensions.strict_add(1)), chosen.iter().flat_map(|(trajectory, _)| trajectory.iter().copied()))),
            steps: chosen.into_iter().flat_map(|(_, steps)| steps).collect(),
        }
    }

    pub fn effects(&self, outputs: DMatrixView<'_, f64>) -> Result<Outputs<'space, K, Effects>, StatsError>
    where K: Ord + Sync {
        let dimensions = self.space.len().get();
        grouped(&self.points, outputs, dimensions.strict_add(1), &self.steps.chunks_exact(dimensions), |runs, steps| {
            let elementary = DMatrix::from_iterator(
                dimensions,
                runs.ncols(),
                runs.column_iter().zip(steps).flat_map(|(group, steps)| {
                    steps.iter().map(move |&(position, delta)| {
                        let change = group.fixed_rows::<2>(position);
                        (change.y - change.x) / delta
                    })
                }),
            );
            Ok(self
                .space
                .keys()
                .enumerate()
                .map(|(dimension, key)| (key, Effects { signed: elementary.row(dimension).iter().copied().collect(), absolute: elementary.row(dimension).iter().map(|effect| effect.abs()).collect() }))
                .collect())
        })
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn grouped<'space, K, X, E, P, M>(points: &DMatrix<f64>, outputs: DMatrixView<'_, f64>, width: usize, partners: &P, estimate: M) -> Result<Outputs<'space, K, E>, StatsError>
where
    K: Sync,
    X: Send,
    E: Send,
    P: Iterator<Item = X> + Clone + Sync,
    M: Fn(&DMatrix<f64>, &[X]) -> Result<NEBTreeMap<&'space K, E>, StatsError> + Sync,
{
    (outputs.ncols() == points.ncols()).ok_or(StatsError::Shape { expected: points.ncols(), found: outputs.ncols() })?;
    let groups = outputs.ncols().strict_div(width);
    Ok((0..outputs.nrows())
        .into_par_iter()
        .map(|output| {
            let row = outputs.row(output);
            let runs = |group: usize| row.columns(group.strict_mul(width), width);
            let (columns, paired): (Vec<_>, Vec<_>) = (0..groups).zip(partners.clone()).filter(|&(group, _)| runs(group).iter().all(|run| run.is_finite())).unzip();
            let failed = groups.strict_sub(columns.len());
            (!columns.is_empty()).ok_or(StatsError::Degenerate { found: failed, sample: Sample::Groups })?;
            Ok(Sensitivity {
                dimensions: estimate(&DMatrix::from_iterator(width, columns.len(), columns.iter().flat_map(|&group| runs(group).into_iter().copied())), &paired)?,
                non_finite: nalgebra::convert::<usize, u64>(failed),
            })
        })
        .collect())
}
