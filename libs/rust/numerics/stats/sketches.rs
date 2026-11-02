use average::{Estimate as _, Merge, Variance};
use datasketches::tdigest::{TDigest, TDigestMut};
use rayon::iter::{FromParallelIterator, IntoParallelIterator, ParallelIterator as _};

use crate::StatsError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default)]
pub struct Summary {
    moments: Variance,
    digest: TDigestMut,
    non_finite: u64,
}

impl Summary {
    pub fn new(compression: u16) -> Result<Self, StatsError> { Ok(Self { moments: Variance::new(), digest: TDigestMut::new(compression)?, non_finite: 0 }) }

    #[must_use]
    pub const fn moments(&self) -> &Variance { &self.moments }

    #[must_use]
    pub fn digest(&self) -> TDigest { self.digest.clone().freeze() }

    #[must_use]
    pub const fn non_finite(&self) -> u64 { self.non_finite }
}

impl Extend<f64> for Summary {
    fn extend<T>(&mut self, iter: T)
    where T: IntoIterator<Item = f64> {
        iter.into_iter().for_each(|value| {
            if value.is_finite() {
                self.moments.add(value);
                self.digest.update(value);
            } else {
                self.non_finite = self.non_finite.strict_add(1);
            }
        });
    }
}

impl FromIterator<f64> for Summary {
    fn from_iter<T>(iter: T) -> Self
    where T: IntoIterator<Item = f64> {
        let mut summary = Self::default();
        summary.extend(iter);
        summary
    }
}

impl Merge for Summary {
    fn merge(&mut self, other: &Self) {
        self.moments.merge(&other.moments);
        self.digest.merge(&other.digest);
        self.non_finite = self.non_finite.strict_add(other.non_finite);
    }
}

impl FromIterator<Self> for Summary {
    fn from_iter<T>(iter: T) -> Self
    where T: IntoIterator<Item = Self> {
        let (moments, digests, non_finite) = iter.into_iter().fold((Variance::new(), Vec::new(), 0_u64), |(mut total, mut digests, count), Self { moments, digest, non_finite }| {
            total.merge(&moments);
            digests.push(digest);
            (total, digests, count.strict_add(non_finite))
        });
        Self { moments, digest: digests.into_iter().collect(), non_finite }
    }
}

impl FromParallelIterator<Self> for Summary {
    fn from_par_iter<I>(par_iter: I) -> Self
    where I: IntoParallelIterator<Item = Self> {
        par_iter.into_par_iter().collect::<Vec<_>>().into_iter().collect()
    }
}

impl FromParallelIterator<f64> for Summary {
    fn from_par_iter<I>(par_iter: I) -> Self
    where I: IntoParallelIterator<Item = f64> {
        par_iter
            .into_par_iter()
            .fold(Self::default, |mut summary, value| {
                summary.extend([value]);
                summary
            })
            .collect()
    }
}
