use average::{Estimate as _, Merge, Variance};
use datasketches::tdigest::{TDigest, TDigestMut};
use rayon::iter::{FromParallelIterator, IntoParallelIterator, ParallelIterator as _};
use serde::{Deserialize, Deserializer, Serialize, Serializer, de::Error as _};
use serde_with::{Bytes, DeserializeAs};

use crate::StatsError;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Default, Serialize, Deserialize, derive_more::AsRef)]
pub struct Summary {
    #[as_ref]
    moments: Variance,
    #[serde(serialize_with = "encode", deserialize_with = "decode")]
    digest: TDigestMut,
    #[as_ref]
    non_finite: u64,
}

impl Summary {
    pub fn new(compression: u16) -> Result<Self, StatsError> { Ok(Self { moments: Variance::new(), digest: TDigestMut::new(compression)?, non_finite: 0 }) }
}

impl From<Summary> for TDigest {
    fn from(value: Summary) -> Self { value.digest.freeze() }
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
        let (mut moments, mut non_finite) = (Variance::new(), 0_u64);
        let digest = iter
            .into_iter()
            .inspect(|summary| {
                moments.merge(&summary.moments);
                non_finite = non_finite.strict_add(summary.non_finite);
            })
            .map(|summary| summary.digest)
            .collect();
        Self { moments, digest, non_finite }
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

// --- [OPERATIONS] ----------------------------------------------------------------------

fn encode<S>(digest: &TDigestMut, serializer: S) -> Result<S::Ok, S::Error>
where S: Serializer {
    serializer.serialize_bytes(&digest.clone().serialize())
}

fn decode<'de, D>(deserializer: D) -> Result<TDigestMut, D::Error>
where D: Deserializer<'de> {
    <Bytes as DeserializeAs<'de, Vec<u8>>>::deserialize_as(deserializer).and_then(|bytes| TDigestMut::deserialize(&bytes).map_err(D::Error::custom))
}
