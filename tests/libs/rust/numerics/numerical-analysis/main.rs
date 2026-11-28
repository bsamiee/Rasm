#![feature(default_field_values)]

use core::f64::consts::{FRAC_1_SQRT_2, PI, TAU};
use core::iter;
use core::num::NonZero;
use core::range::RangeInclusive;

use approx::{assert_abs_diff_eq, assert_relative_eq};
use claims::{assert_ok, assert_some};
use nalgebra::{DMatrix, DVector, MatrixXx3, Vector3};
use nonempty_collections::nev;
use proptest::prelude::ProptestConfig;
use proptest::test_runner::FileFailurePersistence;
use rasm_numerical_analysis::{Domain, Exponent, Extrapolation, Method, Observations, Reading, Settings, Spline, Weight, crossings, curve, evaluate, integrate, roots, weighted};
use rasm_stats::Bounds;
use rstest::rstest;
use strict_num::NormalizedF64;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn integral<const N: usize>(integrand: impl Fn([f64; N]) -> f64, domain: Domain) -> f64 {
    let integral = assert_ok!(integrate(|point| DVector::from_element(1, integrand(*assert_some!(point.as_slice().as_array::<N>()))), [domain], Settings { .. }, |value: &f64| value.abs()));
    let &[estimate] = assert_some!(integral.estimate.as_slice().as_array::<1>());
    estimate
}

fn moment(weight: Weight, power: i32) -> f64 {
    let integral = assert_ok!(weighted(|node| DVector::from_element(1, node.powi(power)), weight, Settings { .. }, |value: &f64| value.abs()));
    let &[estimate] = assert_some!(integral.estimate.as_slice().as_array::<1>());
    estimate
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

// --- [SPLINES]

fn spline_through(sites: &[f64], values: &[f64], method: Method) -> Spline {
    let row = |entries: &[f64]| DMatrix::from_row_slice(1, entries.len(), entries);
    assert_ok!(curve(&assert_ok!(Observations::new(row(sites), row(values), None)), method, None, (None, None)))
}

fn read(spline: &Spline, reading: Reading, at: &[f64]) -> Vec<f64> {
    Vec::from_iter(assert_ok!(evaluate(spline, DMatrix::from_row_slice(1, at.len(), at).as_view(), &[reading], Extrapolation::Refuse)).iter().copied())
}

#[rstest]
#[case::weighted_hermite_interpolant(Method::Monotone, [0.0, 1.0, 5.0], [71.0 / 168.0, 223.0 / 84.0], 9.0 / 7.0)]
#[case::natural_interpolating_spline(Method::Smoothing(Some(NormalizedF64::ONE)), [0.0, 1.0, 0.0], [19.0 / 32.0, 7.0 / 8.0], 0.5)]
fn spline_through_three_points_matches_its_hand_worked_midpoints_and_knot_slope(#[case] method: Method, #[case] values: [f64; 3], #[case] middles: [f64; 2], #[case] slope: f64) {
    let spline = spline_through(&[0.0, 1.0, 3.0], &values, method);
    let found = read(&spline, Reading::Value, &[0.5, 2.0]);
    assert_eq!(found.len(), 2, "one reading per site: {found:?}");
    assert_abs_diff_eq!(found[..], middles[..], epsilon = 1.0e-12);
    assert_abs_diff_eq!(read(&spline, Reading::Derivative(NonZero::<u8>::MIN), &[1.0])[0], slope, epsilon = 1.0e-12);
}

proptest::proptest! {
    #![proptest_config(ProptestConfig { cases: 64, failure_persistence: Some(Box::new(FileFailurePersistence::WithSource("regressions"))), ..ProptestConfig::default() })]

    #[test]
    fn companion_roots_recover_the_factors_of_an_expanded_product(start in -8.0_f64..-4.0, gaps in proptest::collection::vec(0.5_f64..3.0, 0..5)) {
        let factors = Vec::from_iter(iter::once(start).chain(gaps).scan(0.0, |held, step| {
            *held += step;
            Some(*held)
        }));
        let expanded = factors.iter().fold(vec![1.0], |product, factor| {
            let (raised, scaled) = (iter::once(0.0).chain(product.iter().copied()), product.iter().map(|coefficient| -factor * coefficient).chain([0.0]));
            Vec::from_iter(iter::zip(raised, scaled).map(|(raised, scaled)| raised + scaled))
        });
        let mut found = assert_ok!(roots::polynomial(&expanded));
        found.sort_by(|left, right| left.value.re.total_cmp(&right.value.re));
        proptest::prop_assert_eq!(found.len(), factors.len(), "one root per factor of {:?}", factors);
        for (factor, root) in iter::zip(&factors, &found) {
            proptest::prop_assert!((root.value.re - factor).abs() <= 1.0e-8 * factor.abs().max(1.0) && root.value.im.abs() <= 1.0e-8, "root {:?} for factor {}", root, factor);
        }
    }

    #[test]
    fn bracketed_roots_recover_each_sign_change_of_a_factored_product(start in -8.0_f64..-4.0, gaps in proptest::collection::vec(0.5_f64..3.0, 0..5)) {
        let factors = Vec::from_iter(iter::once(start).chain(gaps).scan(0.0, |held, step| {
            *held += step;
            Some(*held)
        }));
        let product = |at: f64| factors.iter().map(|factor| at - factor).product::<f64>();
        let interval = assert_ok!(Bounds::try_new(RangeInclusive { start: -10.0, last: 20.0 }));
        let found = assert_ok!(roots::bracketed(product, interval, assert_some!(NonZero::new(600)), None, (None, None)));
        proptest::prop_assert_eq!(found.len(), factors.len(), "one root per factor of {:?}: {:?}", factors, found);
        for (factor, root) in iter::zip(&factors, &found) {
            proptest::prop_assert!((root - factor).abs() <= 1.0e-9 * factor.abs().max(1.0), "root {} for factor {}", root, factor);
        }
    }

    #[test]
    fn monotone_spline_through_rising_data_stays_rising_and_reads_exact_cubic_slopes_areas_and_crossings(steps in proptest::collection::vec((0.2_f64..2.0, 0.1_f64..3.0), 2..8)) {
        let sites = Vec::from_iter(iter::once(0.0).chain(steps.iter().scan(0.0, |held, (width, _)| {
            *held += width;
            Some(*held)
        })));
        let values = Vec::from_iter(iter::once(1.0).chain(steps.iter().scan(1.0, |held, (_, rise)| {
            *held += rise;
            Some(*held)
        })));
        let spline = spline_through(&sites, &values, Method::Monotone);
        let at_breaks = read(&spline, Reading::Value, &sites);
        proptest::prop_assert_eq!(at_breaks.len(), values.len());
        proptest::prop_assert!(iter::zip(&at_breaks, &values).all(|(found, value)| (found - value).abs() <= 1.0e-12), "{:?} passes through {:?}", at_breaks, values);
        let dense = Vec::from_iter(sites.array_windows().flat_map(|&[start, last]| (0..16).map(move |step| start + (last - start) * f64::from(step) / 16.0)).chain(sites.last().copied()));
        let traced = read(&spline, Reading::Value, &dense);
        proptest::prop_assert_eq!(traced.len(), dense.len());
        proptest::prop_assert!(traced.array_windows().all(|[before, after]| after >= before), "rising data reads as a rising curve: {:?}", traced);
        for &[start, last] in sites.array_windows() {
            let (middle, step) = (f64::midpoint(start, last), (last - start) / 8.0);
            let probes = [2.0_f64.mul_add(-step, middle), middle - step, middle, middle + step, 2.0_f64.mul_add(step, middle), start, last];
            let &[far_left, left, centre, right, far_right, low, high] = assert_some!(read(&spline, Reading::Value, &probes).as_array::<7>());
            let stencil = 8.0_f64.mul_add(right - left, far_left - far_right) / (12.0 * step);
            let slope = read(&spline, Reading::Derivative(NonZero::<u8>::MIN), &[middle])[0];
            proptest::prop_assert!((slope - stencil).abs() <= 1.0e-8 * stencil.abs().max(1.0), "slope {} at {} against the five point stencil {}", slope, middle, stencil);
            let simpson = (last - start) / 6.0 * 4.0_f64.mul_add(centre, low + high);
            let area = read(&spline, Reading::Integral(RangeInclusive { start, last }), &[middle])[0];
            proptest::prop_assert!((area - simpson).abs() <= 1.0e-10 * simpson.abs().max(1.0), "area {} over [{}, {}] against Simpson's rule {}", area, start, last, simpson);
        }
        let level = f64::midpoint(values[0], values[1]);
        let crossed = assert_ok!(crossings(&spline, level, 0));
        let &[once] = assert_some!(crossed.as_array::<1>(), "a rising curve crosses {level} once: {crossed:?}");
        proptest::prop_assert!((sites[0]..sites[1]).contains(&once) && (read(&spline, Reading::Value, &[once])[0] - level).abs() <= 1.0e-9, "the crossing {} reads {}", once, level);
    }
}
