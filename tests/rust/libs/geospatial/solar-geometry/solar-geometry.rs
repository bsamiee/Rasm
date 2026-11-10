use approx::assert_abs_diff_eq;
use claims::assert_ok;
use jiff::SignedDuration;
use jiff::civil::date;
use jiff::tz::{Offset, TimeZone};
use nalgebra::{UnitQuaternion, Vector3};
use rasm_site::{Elevation, Place, Position};
use rasm_solar_geometry::{Refraction, Sun, direction, lapsed, position};
use rstest::rstest;
use solar_positioning::RefractionCorrection;
use uom::si::angle::degree;
use uom::si::f64::{Angle, Length, ThermodynamicTemperature};
use uom::si::length::meter;
use uom::si::ratio::ratio;
use uom::si::thermodynamic_temperature::{degree_celsius, kelvin};

// --- [OPERATIONS] ----------------------------------------------------------------------

#[test]
fn position_matches_nrel_spa_reference() {
    let zone = TimeZone::fixed(assert_ok!(Offset::from_hours(-7)));
    let place = Place {
        position: assert_ok!(Position::try_new(Angle::new::<degree>(39.742_476), Angle::new::<degree>(-105.1786))),
        elevation: assert_ok!(Elevation::try_new(Length::new::<meter>(1830.14))),
        zone: zone.clone(),
    };
    let sun = assert_ok!(Sun::new(place, Some(Refraction::Stated(assert_ok!(RefractionCorrection::new(820.0, 11.0)))), Some(SignedDuration::from_secs(67))));
    let at = assert_ok!(date(2003, 10, 17).at(12, 30, 30, 0).to_zoned(zone)).timestamp();
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
