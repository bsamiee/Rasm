use core::f64::consts::{PI, TAU};
use core::iter;
use core::num::NonZero;
use core::range::Range;
use std::collections::HashMap;

use approx::assert_abs_diff_eq;
use claims::{assert_ok, assert_some};
use jiff::SignedDuration;
use jiff::civil::date;
use jiff::tz::{Offset, TimeZone};
use nalgebra::{Isometry3, Point3, UnitQuaternion, Vector3};
use parry3d_f64::partitioning::BvhBuildStrategy;
use parry3d_f64::shape::SharedShape;
use proptest::prelude::ProptestConfig;
use proptest::test_runner::FileFailurePersistence;
use rasm_proximity::Assembly;
use rasm_site::{Elevation, Place, Position};
use rasm_solar_geometry::{Refraction, Sensor, SkyMeasure, Sun, Transmittance, bin, direction, lapsed, patches, position, sky_view, windows};
use rstest::rstest;
use solar_positioning::RefractionCorrection;
use typed_floats::tf64::PositiveFinite;
use uom::si::angle::degree;
use uom::si::f64::{Angle, Length, ThermodynamicTemperature};
use uom::si::length::meter;
use uom::si::ratio::ratio;
use uom::si::solid_angle::steradian;
use uom::si::thermodynamic_temperature::{degree_celsius, kelvin};

// --- [OPERATIONS] ----------------------------------------------------------------------

fn golden() -> (Sun, TimeZone) {
    let zone = TimeZone::fixed(assert_ok!(Offset::from_hours(-7)));
    let place = Place {
        position: assert_ok!(Position::try_new(Angle::new::<degree>(39.742_476), Angle::new::<degree>(-105.1786))),
        elevation: assert_ok!(Elevation::try_new(Length::new::<meter>(1830.14))),
        zone: zone.clone(),
    };
    (assert_ok!(Sun::new(place, Some(Refraction::Stated(assert_ok!(RefractionCorrection::new(820.0, 11.0)))), Some(SignedDuration::from_secs(67)))), zone)
}

fn facing(normal: Vector3<f64>) -> [Sensor; 1] { [Sensor { point: Point3::origin(), normal: nalgebra::Unit::new_normalize(normal) }] }

#[test]
fn position_matches_nrel_spa_reference() {
    let (sun, zone) = golden();
    let at = assert_ok!(date(2003, 10, 17).at(12, 30, 30, 0).to_zoned_datetime(zone)).timestamp();
    let (_, sun) = assert_ok!(position(&sun, at));
    assert_abs_diff_eq!(sun.azimuth(), 194.340_241, epsilon = 1.0e-6);
    assert_abs_diff_eq!(sun.zenith_angle(), 50.111_622, epsilon = 1.0e-6);
}

#[test]
fn lapsed_matches_standard_atmosphere_at_tropopause() {
    let (temperature, pressure) = lapsed(ThermodynamicTemperature::new::<degree_celsius>(15.0), Length::new::<meter>(11_000.0));
    assert_abs_diff_eq!(temperature.get::<kelvin>(), 216.65, epsilon = 1.0e-9);
    assert_abs_diff_eq!(pressure.get::<ratio>(), 0.223_361, epsilon = 1.0e-6);
}

#[rstest]
#[case::north(0.0, 0.0, Vector3::y())]
#[case::east(90.0, 0.0, Vector3::x())]
#[case::south(180.0, 0.0, -Vector3::y())]
#[case::west(270.0, 0.0, -Vector3::x())]
#[case::zenith(0.0, 90.0, Vector3::z())]
fn direction_turns_clockwise_from_north(#[case] azimuth: f64, #[case] altitude: f64, #[case] expected: Vector3<f64>) {
    let found = direction(Angle::new::<degree>(azimuth), Angle::new::<degree>(altitude), UnitQuaternion::identity());
    assert_abs_diff_eq!((found.into_inner() - expected).norm(), 0.0, epsilon = 1.0e-12);
}

#[rstest]
#[case::sunrise_and_sunset(0.0)]
#[case::high_sun(40.0)]
fn sun_windows_open_and_close_where_the_sun_crosses_their_elevation(#[case] elevation: f64) {
    let (sun, zone) = golden();
    let day = Range {
        start: assert_ok!(date(2003, 10, 17).at(0, 0, 0, 0).to_zoned_datetime(zone.clone())).timestamp(),
        end: assert_ok!(date(2003, 10, 18).at(0, 0, 0, 0).to_zoned_datetime(zone)).timestamp(),
    };
    let found = assert_ok!(windows(&sun, day, Angle::new::<degree>(elevation)));
    let [window] = assert_some!(found.as_array::<1>(), "one daylight window above {elevation} degrees: {found:?}");
    let elevated = |at| assert_ok!(position(&sun, at)).1.elevation_angle();
    assert_abs_diff_eq!(elevated(window.start), elevation, epsilon = 1.0e-3);
    assert_abs_diff_eq!(elevated(window.end), elevation, epsilon = 1.0e-3);
    let middle = window.start.checked_add(window.end.duration_since(window.start) / 2);
    assert!(elevated(assert_ok!(middle)) > elevation, "the sun stands above {elevation} degrees inside its window");
}

// --- [SKY]

proptest::proptest! {
    #![proptest_config(ProptestConfig { cases: 32, failure_persistence: Some(Box::new(FileFailurePersistence::WithSource("regressions"))), ..ProptestConfig::default() })]

    #[test]
    fn sky_patches_follow_reinhart_counts_cover_the_sphere_and_bin_to_themselves(subdivision in 1_u8..=4, turn in proptest::array::uniform3(-PI..PI)) {
        let (split, north) = (assert_some!(NonZero::new(subdivision)), UnitQuaternion::from_scaled_axis(Vector3::from(turn)));
        let sky = Vec::from_iter(patches(split, north));
        proptest::prop_assert_eq!(sky.len(), 144 * usize::from(subdivision).pow(2) + 2, "Reinhart sky of {} subdivisions with its ground patch", subdivision);
        let (ground, down) = (sky[0], -(north * Vector3::z()));
        let grounded = (ground.solid_angle.get::<steradian>() - TAU).abs() < 1.0e-12 && (ground.direction.into_inner() - down).norm() < 1.0e-12;
        proptest::prop_assert!(grounded, "the first patch is the ground hemisphere below the turned frame: {:?}", ground);
        let covered = sky.iter().map(|patch| patch.solid_angle.get::<steradian>()).sum::<f64>();
        proptest::prop_assert!(4.0_f64.mul_add(-PI, covered).abs() < 1.0e-9, "the patches cover the sphere once: {}", covered);
        let misbinned = Vec::from_iter(sky.iter().enumerate().filter(|&(index, patch)| bin(split, north, patch.direction) != index).map(|(index, _)| index));
        proptest::prop_assert!(misbinned.is_empty(), "each patch center bins to its own index: {:?}", misbinned);
    }
}

#[rstest]
#[case::view_factor_of_a_tilted_sensor(SkyMeasure::ViewFactor, Vector3::new(0.6, 0.0, 0.8), 0.9)]
#[case::exposure_of_a_tilted_sensor(SkyMeasure::Exposure, Vector3::new(0.6, 0.0, 0.8), 1.0 - 0.8_f64.acos() / PI)]
#[case::vertical_sky_component_of_a_window(SkyMeasure::VerticalComponent, Vector3::x(), 3.0_f64.mul_add(PI, 8.0) / (14.0 * PI))]
fn unobstructed_sensor_sees_its_closed_form_share_of_the_sky(#[case] measure: SkyMeasure, #[case] normal: Vector3<f64>, #[case] share: f64) {
    let open = assert_ok!(Assembly::new(BvhBuildStrategy::default(), []));
    let offset = assert_ok!(PositiveFinite::new(1.0e-6));
    let seen = assert_ok!(sky_view(&open, &HashMap::<usize, Transmittance>::new(), offset, assert_some!(NonZero::new(4)), &facing(normal), measure));
    assert_abs_diff_eq!(seen[0].get::<ratio>(), share, epsilon = 2.0e-3);
}

#[rstest]
#[case::both_translucent(Some(0.25), 0.15)]
#[case::upper_sheet_opaque(None, 0.0)]
fn sky_through_stacked_sheets_is_the_product_of_their_transmittances(#[case] upper: Option<f64>, #[case] share: f64) {
    let sheet = |height: f64| (Isometry3::translation(0.0, 0.0, height), SharedShape::cuboid(1.0e4, 1.0e4, 0.01));
    let covered = assert_ok!(Assembly::new(BvhBuildStrategy::default(), [sheet(1.0), sheet(2.0)]));
    let translucent: HashMap<usize, Transmittance> =
        HashMap::from_iter(iter::once((0, 0.6)).chain(upper.map(|upper| (1, upper))).map(|(element, passed)| (element, assert_ok!(Transmittance::try_new(passed)))));
    let offset = assert_ok!(PositiveFinite::new(1.0e-6));
    let seen = assert_ok!(sky_view(&covered, &translucent, offset, NonZero::<u8>::MIN, &facing(Vector3::z()), SkyMeasure::ViewFactor));
    assert_abs_diff_eq!(seen[0].get::<ratio>(), share, epsilon = 1.0e-12);
}
