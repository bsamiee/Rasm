use core::{iter, num::NonZero, ops::RangeInclusive};

use average::Variance;
use nonempty_collections::NESlice;
use rand::{Rng, RngExt as _, SeedableRng};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use statrs::{
    distribution::{ContinuousCDF as _, StudentsT},
    statistics::{Data, OrderStatistics as _},
};

use crate::StatsError;

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(greater = 0.0, less = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Confidence(f64);

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tails(confidence: Confidence) -> [f64; 2] {
    let level = confidence.into_inner();
    [-level, level].map(|signed| f64::midpoint(1.0, signed))
}

pub fn interval(moments: &Variance, confidence: Confidence) -> Result<RangeInclusive<f64>, StatsError> {
    let distribution = StudentsT::new(0.0, 1.0, nalgebra::convert::<u64, f64>(moments.len()) - 1.0)?;
    let [low, high] = tails(confidence).map(|tail| distribution.inverse_cdf(tail).mul_add(moments.error(), moments.mean()));
    Ok(low..=high)
}

#[must_use]
pub fn bootstrap<T, R, F>(sample: &NESlice<'_, T>, statistic: F, resamples: NonZero<usize>, confidence: Confidence, rng: &mut R) -> RangeInclusive<f64>
where
    T: Clone + Sync,
    R: Rng + SeedableRng + Send,
    F: Fn(&mut [T]) -> f64 + Sync,
{
    let count = sample.len().get();
    let estimates = iter::repeat_with(|| rng.fork())
        .take(resamples.get())
        .collect::<Vec<R>>()
        .into_par_iter()
        .map_init(
            || Vec::with_capacity(count),
            |resample, mut fork| {
                resample.clear();
                resample.extend(iter::repeat_with(|| sample[fork.random_range(0..count)].clone()).take(count));
                statistic(resample)
            },
        )
        .collect::<Vec<f64>>();
    if estimates.iter().any(|estimate| estimate.is_nan()) {
        f64::NAN..=f64::NAN
    } else {
        let mut ordered = Data::new(estimates);
        let [low, high] = tails(confidence).map(|tail| ordered.quantile(tail));
        low..=high
    }
}
