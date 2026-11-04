#![feature(const_result_trait_fn, const_trait_impl, default_field_values)]

use core::{error::Error, num::NonZero, range::RangeInclusive};

use approx::{assert_abs_diff_eq, assert_relative_eq};
use itertools::Either;
use nalgebra::{DVector, DVectorView, MatrixXx3, dmatrix, dvector};
use nonempty_collections::{NEVec, nev};
use num_dual::{Dual64, DualNum};
use rasm_analysis::{
    AnalysisError, Observations,
    quadrature::{Domain, Settings, Weight, integrate, weighted},
    regression::{Loss, Model, Options, Uncertainty, fit, log_density, observed_information, propagate},
};
use rasm_stats::{Bounds, BoundsError, Tolerance};

// --- [MODELS] --------------------------------------------------------------------------

struct Linear;

impl Model for Linear {
    #[expect(clippy::arithmetic_side_effects, reason = "Clippy cannot match arithmetic allowances through DualNum bounds")]
    fn evaluate<D>(&self, site: DVectorView<'_, D>, parameters: DVectorView<'_, D>) -> D
    where D: DualNum<Primitive = f64> {
        parameters[0].clone() * &site[0]
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn asymmetric_odd_jacobi_preserves_polynomial_moments() -> Result<(), Box<dyn Error>> {
    let integral = weighted(
        |coordinate| DVector::from_iterator(6, (0..=5).map(|degree| coordinate.powi(degree))),
        Weight::Jacobi { interval: Bounds::try_new(RangeInclusive { start: -1.0, last: 1.0 })?, alpha: 1.0.try_into()?, beta: 0.0.try_into()? },
        Settings { degree: const { NonZero::new(3).unwrap() }, .. },
        |value: &f64| value.abs(),
    )?;
    assert_abs_diff_eq!(integral.estimate.as_slice(), [2.0, -2.0 / 3.0, 2.0 / 3.0, -2.0 / 5.0, 2.0 / 5.0, -2.0 / 7.0].as_slice(), epsilon = 1.0e-12);
    Ok(())
}

#[test]
fn quadrature_compensates_unrepresentable_domain_masses() -> Result<(), Either<BoundsError, NEVec<AnalysisError>>> {
    [(200, -300), (-200, 300)].into_iter().try_for_each(|(extent, amplitude)| {
        let side = 10.0_f64.powi(extent);
        let bounds = Bounds::try_new(RangeInclusive { start: 0.0, last: side }).map_err(Either::Left)?;
        [(Domain::Box(nev![bounds, bounds]), 1.0), (Domain::Triangle(MatrixXx3::from_row_slice(&[0.0, side, 0.0, 0.0, 0.0, side])), 0.5)].into_iter().try_for_each(|(domain, fraction)| {
            let output = Dual64::new(1.0, 1.0) * 10.0_f64.powi(amplitude);
            let integral = integrate(|_| dvector![output], nev![domain], Settings { .. }, |value: &Dual64| value.re.abs().max(value.eps.abs())).map_err(Either::Right)?;
            let expected = fraction * 10.0_f64.powi(2 * extent + amplitude);
            for value in [integral.estimate[0].re, integral.estimate[0].eps] {
                assert_relative_eq!(value, expected, epsilon = 0.0, max_relative = 1.0e-12);
            }
            Ok(())
        })
    })
}

#[test]
fn robust_information_preserves_extreme_residual_curvature() -> Result<(), NEVec<AnalysisError>> {
    let seed = Observations::new(dmatrix![-1.0, 0.0, 1.0], dmatrix![-1.0, 0.0, 1.0], None)?;
    let (_, estimate) = fit(&seed, dmatrix![0_u8], Options { .. }, (None, None))?;
    let sites = dmatrix![1.0e200, 2.0e200, 3.0e200];
    let observations = Observations::new(sites.clone(), sites.clone(), None)?;
    let tolerance = const { Tolerance::try_new(1.0e100).ok().unwrap() };
    let scale = tolerance.into_inner();
    let soft_noise = scale * sites.sum();
    let soft_curvature = 2.0 * sites.iter().map(|site| (scale / site) * scale * scale).sum::<f64>();
    let cauchy_noise = scale * scale * sites.iter().map(|site| (site / scale).ln()).sum::<f64>();
    [(Loss::SoftL1(tolerance), soft_noise, 2.0 * soft_noise / soft_curvature), (Loss::Cauchy(tolerance), cauchy_noise, -cauchy_noise / (3.0 * scale * scale))].into_iter().try_for_each(
        |(loss, noise, covariance)| {
            let information = observed_information(&Linear, &observations, loss, &estimate)?;
            assert_relative_eq!(information.noise, noise, max_relative = 1.0e-12);
            assert_relative_eq!(information.covariance[(0, 0)], covariance, max_relative = 1.0e-12);
            Ok(())
        },
    )
}

#[test]
fn public_density_and_prediction_preserve_parameter_and_site_sensitivities() -> Result<(), NEVec<AnalysisError>> {
    let observations = Observations::new(dmatrix![1.0, 2.0, 4.0], dmatrix![3.0, 5.0, 9.0], Some(dvector![1.0, 4.0, 0.25]))?;
    let density = log_density(&Linear, &observations, const { Tolerance::try_new(2.0).ok().unwrap() }).map_err(NEVec::new)?;
    let mut slope = [0.0];
    assert_abs_diff_eq!(density(&[2.0], &mut slope), -0.65625, epsilon = 1.0e-12);
    assert_abs_diff_eq!(slope[0], 2.5, epsilon = 1.0e-12);
    let uncertainty = Uncertainty { parameters: dvector![2.0], covariance: dmatrix![0.25], noise: 0.5, freedom: 8.0 };
    let prediction = propagate(&Linear, &uncertainty, dmatrix![3.0, -4.0].as_view(), Some(&dmatrix![0.125]), None)?;
    assert_abs_diff_eq!(prediction.mean.as_slice(), [6.0, -8.0].as_slice(), epsilon = 1.0e-12);
    assert_abs_diff_eq!(prediction.variance.as_slice(), [2.75, 4.5].as_slice(), epsilon = 1.0e-12);
    Ok(())
}
