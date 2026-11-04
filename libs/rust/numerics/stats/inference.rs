use core::{cmp::Ordering, iter, num::NonZero, range::RangeInclusive};

use average::{Mean, Skewness, Variance};
use nonempty_collections::NESlice;
use rand::{Rng, RngExt as _, SeedableRng};
use rayon::iter::{IntoParallelIterator as _, ParallelIterator as _};
use statrs::{
    distribution::{ContinuousCDF as _, Normal, StudentsT},
    statistics::Data,
};

use crate::{Sample, StatsError, descriptive::quantile_data};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Interval {
    Percentile,
    Bca,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(greater = 0.0, less = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Confidence(f64);

// --- [OPERATIONS] ----------------------------------------------------------------------

fn tails(confidence: Confidence) -> [f64; 2] {
    let level = confidence.into_inner();
    [-level, level].map(|signed| f64::midpoint(1.0, signed))
}

pub fn interval(moments: &Variance, confidence: Confidence) -> Result<RangeInclusive<f64>, StatsError> {
    let distribution = StudentsT::new(0.0, 1.0, nalgebra::convert::<u64, f64>(moments.len()) - 1.0).map_err(|source| StatsError::Observations { found: moments.len(), source })?;
    let [start, last] = tails(confidence).map(|tail| distribution.inverse_cdf(tail).mul_add(moments.error(), moments.mean()));
    Ok(RangeInclusive { start, last })
}

pub fn bootstrap<T, R, F>(
    sample: &NESlice<'_, T>,
    statistic: F,
    block: NonZero<usize>,
    resamples: NonZero<usize>,
    method: Interval,
    confidence: Confidence,
    rng: &mut R,
) -> Result<RangeInclusive<f64>, StatsError>
where
    T: Clone + Sync,
    R: Rng + SeedableRng + Send,
    F: Fn(&mut [T]) -> f64 + Sync,
{
    fn finite<'estimate, I>(estimates: I) -> Result<(), StatsError>
    where I: IntoIterator<Item = &'estimate f64> {
        let found = estimates.into_iter().filter(|estimate| !estimate.is_finite()).count();
        (found == 0).ok_or(StatsError::Degenerate { found, sample: Sample::Resamples })
    }
    let count = sample.len().get();
    let estimates = iter::repeat_with(|| rng.fork())
        .take(resamples.get())
        .collect::<Vec<R>>()
        .into_par_iter()
        .map_init(
            || Vec::with_capacity(count),
            |resample, mut fork| {
                resample.clear();
                resample.extend(
                    iter::repeat_with(|| fork.random_range(0..count))
                        .flat_map(|start| {
                            let (before, after) = sample.as_ref().split_at(start);
                            iter::chain(after, before).take(block.get())
                        })
                        .take(count)
                        .cloned(),
                );
                statistic(resample)
            },
        )
        .collect::<Vec<f64>>();
    finite(&estimates)?;
    let levels = match method {
        Interval::Percentile => tails(confidence),
        Interval::Bca => {
            let blocks = count.div_ceil(block.get());
            let groups = blocks.min(resamples.get());
            let edge = |group: usize| group.strict_mul(blocks).strict_div(groups).strict_mul(block.get()).min(count);
            let jackknife = (0..groups)
                .into_par_iter()
                .map_init(
                    || Vec::with_capacity(count),
                    |leftover, group| {
                        let [start, last] = [group, group.strict_add(1)].map(edge);
                        leftover.clear();
                        leftover.extend(sample.iter().take(start).chain(sample.iter().skip(last)).cloned());
                        (!leftover.is_empty()).then(|| statistic(leftover))
                    },
                )
                .flatten()
                .collect::<Vec<f64>>();
            let full = statistic(&mut sample.iter().cloned().collect::<Vec<_>>());
            finite(iter::once(&full).chain(&jackknife))?;
            let below = estimates
                .iter()
                .map(|estimate| match estimate.total_cmp(&full) {
                    Ordering::Less => 1.0,
                    Ordering::Equal => 0.5,
                    Ordering::Greater => 0.0,
                })
                .collect::<Mean>()
                .mean();
            (0.0 < below && below < 1.0).ok_or(StatsError::Degenerate { found: resamples.get(), sample: Sample::Bias })?;
            let normal = Normal::standard();
            let bias = normal.inverse_cdf(below);
            let moments = jackknife.into_iter().collect::<Skewness>();
            let acceleration = if moments.is_empty() { 0.0 } else { -moments.skewness() / (6.0 * nalgebra::convert::<u64, f64>(moments.len()).sqrt()) };
            tails(confidence).map(|tail| {
                let shifted = bias + normal.inverse_cdf(tail);
                normal.cdf(bias + shifted / acceleration.mul_add(-shifted, 1.0))
            })
        }
    };
    let mut ordered = Data::new(estimates);
    let [start, last] = levels.map(|level| quantile_data(&mut ordered, level));
    Ok(RangeInclusive { start, last })
}
