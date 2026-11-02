use core::{iter, num::NonZero};

use itertools::Itertools as _;
use nalgebra::DMatrix;
use rand::{Rng, RngExt as _, seq::SliceRandom as _};
use sobol_burley::NUM_DIMENSION_SETS_4D;

use crate::Space;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Design {
    Random { points: NonZero<usize> },
    LatinHypercube { points: NonZero<usize> },
    Sobol { points: NonZero<u16> },
    Factorial { levels: NonZero<u16> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn sample<K, R>(space: &Space<K>, design: Design, rng: &mut R) -> DMatrix<f64>
where R: Rng + ?Sized {
    let dimensions = space.len().get();
    place(
        space,
        match design {
            Design::Random { points } => DMatrix::from_fn(dimensions, points.get(), |_, _| rng.random()),
            Design::LatinHypercube { points } => {
                let strata = nalgebra::convert::<usize, f64>(points.get());
                DMatrix::from_row_iterator(
                    dimensions,
                    points.get(),
                    iter::repeat_with(|| {
                        let mut permutation = (0..points.get()).collect::<Vec<_>>();
                        permutation.shuffle(rng);
                        permutation
                    })
                    .take(dimensions)
                    .flatten(),
                )
                .map(|stratum| (nalgebra::convert::<usize, f64>(stratum) + rng.random::<f64>()) / strata)
            }
            Design::Sobol { points } => sobol(dimensions, points, rng.next_u32()),
            Design::Factorial { levels } if levels.get() == 1 => DMatrix::from_element(dimensions, 1, 0.5),
            Design::Factorial { levels } => {
                let span = f64::from(levels.get().strict_sub(1));
                DMatrix::from_iterator(
                    dimensions,
                    iter::repeat_n(NonZero::<usize>::from(levels).get(), dimensions).fold(1, usize::strict_mul),
                    iter::repeat_n((0..levels.get()).map(|level| f64::from(level) / span), dimensions).multi_cartesian_product().flatten(),
                )
            }
        },
    )
}

#[must_use]
pub(crate) fn sobol(dimensions: usize, points: NonZero<u16>, seed: u32) -> DMatrix<f64> {
    DMatrix::from_iterator(
        dimensions,
        NonZero::<usize>::from(points).get(),
        (0..points.get()).flat_map(|index| {
            (0_u32..).cartesian_product(0..NUM_DIMENSION_SETS_4D).flat_map(move |(block, set)| sobol_burley::sample_4d(u32::from(index), set, seed ^ block)).take(dimensions).map(f64::from)
        }),
    )
}

#[must_use]
pub(crate) fn place<K>(space: &Space<K>, mut unit: DMatrix<f64>) -> DMatrix<f64> {
    unit.row_iter_mut().zip(space.values()).for_each(|(mut row, bounds)| row.apply(|coordinate| *coordinate = coordinate.mul_add(bounds.last, (1.0 - *coordinate) * bounds.start)));
    unit
}
