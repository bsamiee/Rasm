use core::{iter, num::NonZero, range::RangeInclusive};

use faer::{MatRef, linalg::solvers::SolveLstsq as _};
use itertools::Itertools as _;
use nalgebra::{DMatrix, DVector, convert};
use nonempty_collections::NEVec;
use num_dual::DualNum;
use validated::Validated;

use crate::{AnalysisError, Estimate, Observations, Prediction, decompose, estimate};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, derive_more::AsRef)]
pub struct Polynomial {
    #[as_ref]
    exponents: DMatrix<usize>,
    #[as_ref([RangeInclusive<f64>])]
    domain: Box<[RangeInclusive<f64>]>,
    #[as_ref]
    estimate: Estimate,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn fit(observations: &Observations, degree: u8) -> Result<Polynomial, AnalysisError> {
    let (dimensions, points) = observations.sites.shape();
    let terms =
        usize::try_from((1..=u128::from(degree)).fold(1_u128, |count, total| count.saturating_mul(convert::<usize, u128>(dimensions).saturating_add(total)).div_euclid(total))).unwrap_or(usize::MAX);
    let freedom = points.checked_sub(terms).and_then(NonZero::new).ok_or_else(|| AnalysisError::Observations { found: points, needed: terms.saturating_add(1) })?;
    let exponents = DMatrix::from_iterator(
        dimensions,
        terms,
        (0..=usize::from(degree))
            .flat_map(|total| (0..dimensions).combinations_with_replacement(total))
            .flat_map(|monomial| (0..dimensions).map(move |dimension| monomial.iter().filter(|&&index| index == dimension).count())),
    );
    let domain = observations.sites.row_iter().map(|coordinates| RangeInclusive { start: coordinates.min(), last: coordinates.max() }).collect::<Box<[_]>>();
    let root = observations.weights.map(f64::sqrt);
    let mut weighted = design(&exponents, &domain, observations.sites.clone());
    iter::zip(weighted.row_iter_mut(), &root).for_each(|(mut row, &scale)| row *= scale);
    let target = observations.values.component_mul(&root);
    let svd = decompose(&weighted)?;
    let parameters = DVector::from_column_slice(svd.solve_lstsq(MatRef::from_column_major_slice(target.as_slice(), points, 1)).col_as_slice(0));
    let residual = (&target - &weighted * &parameters).norm_squared();
    Ok(Polynomial { exponents, domain, estimate: estimate(&svd, parameters, residual, freedom) })
}

pub fn predict<T>(polynomial: &Polynomial, sites: DMatrix<T>) -> Result<Prediction<T>, NEVec<AnalysisError>>
where T: DualNum<Primitive = f64> {
    let Polynomial { exponents, domain, estimate: Estimate { parameters, covariance, residual: _, freedom: _ } } = polynomial;
    iter::once((sites.nrows() == domain.len()).ok_or_else(|| AnalysisError::Shape { expected: domain.len(), found: sites.nrows() }))
        .chain(
            sites.iter().positions(|coordinate| !coordinate.re().is_finite()).map(|entry| sites.vector_to_matrix_index(entry)).map(|(dimension, point)| Err(AnalysisError::Site { dimension, point })),
        )
        .collect::<Validated<(), _>>()
        .ok()?;
    let terms = design(exponents, domain, sites);
    Ok(Prediction { mean: &terms * parameters.map(T::from), variance: (&terms * covariance.map(T::from)).component_mul(&terms).column_sum() })
}

fn design<T>(exponents: &DMatrix<usize>, domain: &[RangeInclusive<f64>], mut sites: DMatrix<T>) -> DMatrix<T>
where T: DualNum<Primitive = f64> {
    iter::zip(sites.row_iter_mut(), domain).for_each(|(mut coordinates, extent)| {
        let half = extent.last / 2.0 - extent.start / 2.0;
        coordinates.add_scalar_mut(T::from(-f64::midpoint(extent.start, extent.last)));
        coordinates /= T::from(if half > 0.0 { half } else { 1.0 });
    });
    DMatrix::from_row_iterator(
        sites.ncols(),
        exponents.ncols(),
        sites.column_iter().flat_map(|site| {
            exponents.column_iter().map(move |term| iter::zip(site.iter(), term.iter()).flat_map(|(coordinate, &exponent)| iter::repeat_n(coordinate.clone(), exponent)).product::<T>())
        }),
    )
}
