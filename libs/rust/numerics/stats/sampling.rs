use core::{iter, num::NonZero, range::RangeInclusive};

use itertools::Itertools as _;
use nalgebra::DMatrix;
use rand::{Rng, RngExt as _, seq::SliceRandom as _};
use sobol_burley::NUM_DIMENSION_SETS_4D;

use crate::{Space, StatsError};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |window| window.start <= window.last), derive(Debug, Clone, Copy, PartialEq, Eq, Deref))]
pub struct Window(RangeInclusive<u16>);

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Design {
    Random { points: NonZero<usize> },
    LatinHypercube { points: NonZero<usize> },
    Sobol { window: Window, seed: u32 },
    Factorial { levels: NonZero<u16> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn sample<K, R>(space: &Space<K>, design: Design, rng: &mut R) -> Result<DMatrix<f64>, StatsError>
where R: Rng + ?Sized {
    let dimensions = space.len();
    Ok(place(
        space,
        match design {
            Design::Random { points } => DMatrix::from_fn(dimensions.get(), points.get(), |_, _| rng.random()),
            Design::LatinHypercube { points } => {
                let strata = nalgebra::convert::<usize, f64>(points.get());
                DMatrix::from_row_iterator(dimensions.get(), points.get(), gen {
                    for _ in 0..dimensions.get() {
                        let mut permutation = (0..points.get()).collect::<Vec<_>>();
                        permutation.shuffle(rng);
                        for stratum in permutation {
                            yield (nalgebra::convert::<usize, f64>(stratum) + rng.random::<f64>()) / strata;
                        }
                    }
                })
            }
            Design::Sobol { window, seed } => sobol(dimensions.get(), window, seed),
            Design::Factorial { levels } => {
                let points = iter::repeat_n(NonZero::<usize>::from(levels), dimensions.get())
                    .try_fold(NonZero::<usize>::MIN, NonZero::<usize>::checked_mul)
                    .filter(|points| points.checked_mul(dimensions).is_some())
                    .ok_or(StatsError::Factorial { levels, dimensions })?;
                let span = f64::from(levels.get().strict_sub(1));
                DMatrix::from_iterator(
                    dimensions.get(),
                    points.get(),
                    iter::repeat_n((0..levels.get()).map(|level| if span > 0.0 { f64::from(level) / span } else { 0.5 }), dimensions.get()).multi_cartesian_product().flatten(),
                )
            }
        },
    ))
}

#[must_use]
pub(crate) fn sobol(dimensions: usize, window: Window, seed: u32) -> DMatrix<f64> {
    DMatrix::from_iterator(
        dimensions,
        usize::from(window.last.strict_sub(window.start)).strict_add(1),
        window.iter().flat_map(|index| {
            (0_u32..).cartesian_product(0..NUM_DIMENSION_SETS_4D).flat_map(move |(block, set)| sobol_burley::sample_4d(u32::from(index), set, seed ^ block)).take(dimensions).map(f64::from)
        }),
    )
}

#[must_use]
pub(crate) fn place<K>(space: &Space<K>, mut unit: DMatrix<f64>) -> DMatrix<f64> {
    unit.row_iter_mut().zip(space.values()).for_each(|(mut row, bounds)| row.apply(|coordinate| *coordinate = coordinate.mul_add(bounds.last, (1.0 - *coordinate) * bounds.start)));
    unit
}
