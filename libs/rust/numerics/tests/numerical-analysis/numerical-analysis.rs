#![feature(default_field_values)]

use core::f64::consts::{FRAC_1_SQRT_2, PI, TAU};
use core::range::RangeInclusive;

use approx::assert_relative_eq;
use claims::assert_ok;
use nalgebra::{DVector, DVectorView, MatrixXx3, Vector3};
use nonempty_collections::nev;
use rasm_numerical_analysis::{Domain, Exponent, Settings, Weight, integrate, weighted};
use rasm_stats::Bounds;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn integral(integrand: impl Fn(DVectorView<'_, f64>) -> f64, domain: Domain) -> f64 {
    let integral = assert_ok!(integrate(|point| DVector::from_element(1, integrand(point)), [domain], Settings { .. }, |value: &f64| value.abs()));
    integral.estimate[0]
}

fn moment(weight: Weight, power: i32) -> f64 {
    let integral = assert_ok!(weighted(|node| DVector::from_element(1, node.powi(power)), weight, Settings { .. }, |value: &f64| value.abs()));
    integral.estimate[0]
}

#[test]
fn box_integrals_match_closed_forms() {
    let interval = |start, last| assert_ok!(Bounds::try_new(RangeInclusive { start, last }));
    assert_relative_eq!(integral(|point| point[0].sin(), Domain::Box(nev![interval(0.0, PI)])), 2.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|point| point[0] * point[1], Domain::Box(nev![interval(0.0, 1.0), interval(0.0, 2.0)])), 1.0, max_relative = 1.0e-12);
}

#[test]
fn triangle_integrals_scale_by_surface_area() {
    let plane = MatrixXx3::from_column_slice(&[0.0, 0.0, 1.0, 0.0, 0.0, 1.0]);
    let tilted = MatrixXx3::from_column_slice(&[0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0, 1.0]);
    assert_relative_eq!(integral(|_| 1.0, Domain::Triangle(plane.clone())), 0.5, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|point| point[0], Domain::Triangle(plane)), 1.0 / 6.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|_| 1.0, Domain::Triangle(tilted)), FRAC_1_SQRT_2, max_relative = 1.0e-12);
}

#[test]
fn spherical_integrals_match_closed_forms() {
    assert_relative_eq!(integral(|_| 1.0, Domain::Sphere), 2.0 * TAU, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|point| point[2].powi(2), Domain::Sphere), 2.0 * TAU / 3.0, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|_| 1.0, Domain::Hemisphere(Vector3::x_axis())), TAU, max_relative = 1.0e-12);
    assert_relative_eq!(integral(|point| point[0], Domain::Hemisphere(Vector3::x_axis())), PI, max_relative = 1.0e-12);
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
