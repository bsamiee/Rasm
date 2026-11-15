#![feature(default_field_values)]

use core::f64::consts::{FRAC_1_SQRT_2, PI, TAU};
use core::range::RangeInclusive;

use approx::assert_relative_eq;
use claims::assert_ok;
use nalgebra::{DVector, MatrixXx3, Vector3};
use nonempty_collections::nev;
use rasm_numerical_analysis::{Domain, Exponent, Settings, Weight, integrate, weighted};
use rasm_stats::Bounds;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn integral<const N: usize>(integrand: impl Fn([f64; N]) -> f64, domain: Domain) -> f64 {
    let integral = assert_ok!(integrate(|point| DVector::from_element(1, integrand(assert_ok!(<[f64; N]>::try_from(point.as_slice())))), [domain], Settings { .. }, |value: &f64| value.abs()));
    assert_ok!(<[f64; 1]>::try_from(integral.estimate.as_slice()).map(|[estimate]| estimate))
}

fn moment(weight: Weight, power: i32) -> f64 {
    let integral = assert_ok!(weighted(|node| DVector::from_element(1, node.powi(power)), weight, Settings { .. }, |value: &f64| value.abs()));
    assert_ok!(<[f64; 1]>::try_from(integral.estimate.as_slice()).map(|[estimate]| estimate))
}

#[test]
fn box_integrals_match_closed_forms() {
    let interval = |start, last| assert_ok!(Bounds::try_new(RangeInclusive { start, last }));
    assert_relative_eq!(integral(|[x]| x.sin(), Domain::Box(nev![interval(0.0, PI)])), 2.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[x, y]| x * y, Domain::Box(nev![interval(0.0, 1.0), interval(0.0, 2.0)])), 1.0, max_relative = 1.0e-12);
}

#[test]
fn triangle_integrals_scale_by_surface_area() {
    let plane = MatrixXx3::from_column_slice(&[0.0, 0.0, 1.0, 0.0, 0.0, 1.0]);
    let tilted = MatrixXx3::from_column_slice(&[0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0, 1.0]);
    assert_relative_eq!(integral(|[_, _]| 1.0, Domain::Triangle(plane.clone())), 0.5, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[x, _]| x, Domain::Triangle(plane)), 1.0 / 6.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[_, _, _]| 1.0, Domain::Triangle(tilted)), FRAC_1_SQRT_2, max_relative = 1.0e-12);
}

#[test]
fn spherical_integrals_match_closed_forms() {
    assert_relative_eq!(integral(|[_, _, _]| 1.0, Domain::Sphere), 2.0 * TAU, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[_, _, z]| z.powi(2), Domain::Sphere), 2.0 * TAU / 3.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[_, _, _]| 1.0, Domain::Hemisphere(Vector3::x_axis())), TAU, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|[x, _, _]| x, Domain::Hemisphere(Vector3::x_axis())), PI, max_relative = 1.0e-12);
}

#[test]
fn jacobi_weight_matches_beta_moments() {
    let weight =
        Weight::Jacobi { interval: assert_ok!(Bounds::try_new(RangeInclusive { start: 1.0, last: 5.0 })), alpha: assert_ok!(Exponent::try_new(0.5)), beta: assert_ok!(Exponent::try_new(-0.5)) };
    assert_relative_eq!(moment(weight, 0), TAU, max_relative = 1.0e-12);
    assert_relative_eq!(moment(weight, 1), 2.0 * TAU, max_relative = 1.0e-12);
}

#[test]
fn laguerre_weight_matches_gamma_moments() {
    let weight = Weight::Laguerre { alpha: assert_ok!(Exponent::try_new(0.5)) };
    assert_relative_eq!(moment(weight, 0), PI.sqrt() / 2.0, max_relative = 1.0e-12);
    assert_relative_eq!(moment(weight, 1), 3.0 * PI.sqrt() / 4.0, max_relative = 1.0e-12);
}

#[test]
fn hermite_weight_matches_gaussian_moments() {
    assert_relative_eq!(moment(Weight::Hermite, 0), PI.sqrt(), max_relative = 1.0e-12);
    assert_relative_eq!(moment(Weight::Hermite, 2), PI.sqrt() / 2.0, max_relative = 1.0e-12);
}
