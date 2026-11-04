#![feature(default_field_values)]

use core::{assert_matches, iter, range::Range};

use approx::assert_abs_diff_eq;
use dashu::base::ConversionError;
use jiff::{SignedDuration, Timestamp, tz::TimeZone};
use rasm_site::{Elevation, ElevationError, Place, Position, SiteError};
use rasm_solar::Sun;
use rasm_weather::{
    Hour, Hours, Malformed, WeatherError,
    climate::{Statistic, summary},
    completed,
    epw::Day,
    resampled,
};
use uom::si::{
    angle::degree,
    f64::{Angle, HeatFluxDensity, Length, Ratio, ThermodynamicTemperature},
    heat_flux_density::watt_per_square_meter,
    length::meter,
    ratio::ratio,
    thermodynamic_temperature::{degree_celsius, kelvin},
};

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum SeriesError {
    #[error(transparent)]
    Site(#[from] SiteError),
    #[error("fixture elevation is not finite")]
    Elevation,
    #[error(transparent)]
    Weather(#[from] WeatherError),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn hours(records: Vec<Hour>) -> Result<Hours, SeriesError> {
    let place = Place {
        position: Position::try_new(Angle::new::<degree>(0.0), Angle::new::<degree>(0.0))?,
        elevation: Elevation::try_new(Length::new::<meter>(0.0)).map_err(|ElevationError::PredicateViolated| SeriesError::Elevation)?,
        zone: TimeZone::UTC,
    };
    Hours::try_new(Sun::new(place, None, None).map_err(WeatherError::Solar)?, records).map_err(SeriesError::Weather)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn resampling_preserves_boundaries() -> Result<(), SeriesError> {
    let step = SignedDuration::from_hours(1);
    let empty = hours(Vec::new())?;
    assert_eq!(AsRef::<[Hour]>::as_ref(&resampled(&empty, step, SignedDuration::ZERO)?), []);
    let middle = Timestamp::MAX.checked_sub(step).map_err(WeatherError::Time)?;
    let start = middle.checked_sub(step).map_err(WeatherError::Time)?;
    let dry_bulb = Some(ThermodynamicTemperature::new::<degree_celsius>(12.0));
    let source = hours(vec![Hour { interval: Range { start, end: Timestamp::MAX }, dry_bulb, .. }])?;
    let sampled = resampled(&source, step, SignedDuration::ZERO)?;
    let records: &[Hour] = sampled.as_ref();

    assert_eq!(records.iter().map(|hour| hour.interval).collect::<Vec<_>>(), [Range { start, end: middle }, Range { start: middle, end: Timestamp::MAX }]);
    assert!(records.iter().all(|hour| hour.dry_bulb == dry_bulb));
    Ok(())
}

#[test]
fn resampling_respects_missing_value_gap() -> Result<(), SeriesError> {
    let step = SignedDuration::from_hours(1);
    let source = hours(
        iter::zip([Some(0.0), None, None, Some(30.0)], 0_i64..)
            .map(|(dry_bulb, hour)| {
                Ok(Hour {
                    interval: Range {
                        start: Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_hours(hour))?,
                        end: Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_hours(hour.strict_add(1)))?,
                    },
                    dry_bulb: dry_bulb.map(ThermodynamicTemperature::new::<degree_celsius>),
                    ..
                })
            })
            .collect::<Result<Vec<_>, jiff::Error>>()
            .map_err(WeatherError::Time)?,
    )?;

    for (gap, expected) in [(2, [Some(0.0), None, None, Some(30.0)]), (3, [Some(0.0), Some(10.0), Some(20.0), Some(30.0)])] {
        let sampled = resampled(&source, step, SignedDuration::from_hours(gap))?;
        let records: &[Hour] = sampled.as_ref();
        assert_eq!(records.iter().map(|hour| hour.dry_bulb.is_some()).collect::<Vec<_>>(), expected.map(|value| value.is_some()));
        assert_abs_diff_eq!(
            records.iter().filter_map(|hour| hour.dry_bulb.map(|value| value.get::<degree_celsius>())).collect::<Vec<_>>(),
            expected.into_iter().flatten().collect::<Vec<_>>(),
            epsilon = 1.0e-10
        );
    }
    Ok(())
}

#[test]
fn summaries_preserve_duration_weights() -> Result<(), SeriesError> {
    let middle = Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_hours(1)).map_err(WeatherError::Time)?;
    let end = middle.checked_add(SignedDuration::from_hours(3)).map_err(WeatherError::Time)?;
    let source = hours(vec![
        Hour { interval: Range { start: Timestamp::UNIX_EPOCH, end: middle }, dry_bulb: Some(ThermodynamicTemperature::new::<degree_celsius>(0.0)), .. },
        Hour { interval: Range { start: middle, end }, dry_bulb: Some(ThermodynamicTemperature::new::<degree_celsius>(12.0)), .. },
    ])?;

    for (statistic, expected) in
        [(Statistic::Mean, 9.0), (Statistic::Quantile(Ratio::new::<ratio>(0.0)), 0.0), (Statistic::Quantile(Ratio::new::<ratio>(0.5)), 12.0), (Statistic::Quantile(Ratio::new::<ratio>(1.0)), 12.0)]
    {
        let result = summary(&source, |_, _| Some(()), |hour| hour.dry_bulb, statistic)?;
        assert_abs_diff_eq!(result.into_values().map(|value| value.get::<degree_celsius>()).collect::<Vec<_>>(), vec![expected], epsilon = 1.0e-10);
    }
    Ok(())
}

#[test]
fn means_preserve_finite_values() -> Result<(), SeriesError> {
    let smallest = 0.0_f64.next_up();
    let cases = [
        (vec![(1.0e308, 3600)], 1.0e308),
        (vec![(f64::MAX, 3600), (f64::MAX, 3600)], f64::MAX),
        (vec![(f64::MAX, 1), (-f64::MAX, 1)], 0.0),
        (vec![(f64::MAX, 1), (1.0, 1), (-f64::MAX, 1)], 1.0 / 3.0),
        (vec![(1.0e100, 1), (1.0, 1), (f64::MAX, 1), (-f64::MAX, 1), (-1.0e100, 1)], 0.2),
        (vec![(f64::MAX, 1), (-f64::MAX, 3)], -f64::MAX / 2.0),
        (iter::repeat_n((0.0, 1), 2).chain(iter::repeat_n((smallest, 1), 10)).collect(), smallest),
        (vec![(3.0 * smallest, 1), (-smallest, 1)], smallest),
    ];
    for (readings, expected) in cases {
        let step = SignedDuration::from_secs(readings.iter().map(|&(_, seconds)| seconds).sum());
        let mut start = Timestamp::UNIX_EPOCH;
        let source = hours(
            readings
                .into_iter()
                .map(|(value, seconds)| {
                    let end = start.checked_add(SignedDuration::from_secs(seconds))?;
                    let hour = Hour {
                        interval: Range { start, end },
                        dry_bulb: Some(ThermodynamicTemperature::new::<kelvin>(value)),
                        infrared: Some(HeatFluxDensity::new::<watt_per_square_meter>(value)),
                        ..
                    };
                    start = end;
                    Ok(hour)
                })
                .collect::<Result<Vec<_>, jiff::Error>>()
                .map_err(WeatherError::Time)?,
        )?;
        let means = summary(&source, |_, _| Some(()), |hour| hour.dry_bulb, Statistic::Mean)?;
        assert_eq!(means.into_values().map(|value| value.get::<kelvin>()).collect::<Vec<_>>(), [expected]);
        let sampled = resampled(&source, step, SignedDuration::ZERO)?;
        assert_eq!(AsRef::<[Hour]>::as_ref(&sampled).iter().filter_map(|hour| hour.infrared.map(|value| value.get::<watt_per_square_meter>())).collect::<Vec<_>>(), [expected]);
    }
    Ok(())
}

#[test]
fn precipitation_preserves_finite_depth() -> Result<(), SeriesError> {
    let step = SignedDuration::from_nanos(1);
    let interval = Range { start: Timestamp::UNIX_EPOCH, end: Timestamp::UNIX_EPOCH.checked_add(step).map_err(WeatherError::Time)? };
    let precipitation = Some(Length::new::<meter>(f64::MAX));
    let source = hours(vec![Hour { interval, precipitation, .. }])?;
    let sampled = resampled(&source, step, SignedDuration::ZERO)?;
    let records: &[Hour] = sampled.as_ref();
    assert_eq!(records.iter().map(|hour| (hour.interval, hour.precipitation)).collect::<Vec<_>>(), [(interval, precipitation)]);

    let smallest = 0.0_f64.next_up();
    let source = hours(vec![Hour {
        interval: Range { start: Timestamp::UNIX_EPOCH, end: Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_secs(6)).map_err(WeatherError::Time)? },
        precipitation: Some(Length::new::<meter>(9.0 * smallest)),
        ..
    }])?;
    let sampled = resampled(&source, SignedDuration::from_secs(1), SignedDuration::ZERO)?;
    assert_eq!(AsRef::<[Hour]>::as_ref(&sampled).iter().filter_map(|hour| hour.precipitation.map(|depth| depth.get::<meter>())).collect::<Vec<_>>(), vec![2.0 * smallest; 6]);
    Ok(())
}

#[test]
fn interpolation_preserves_opposite_extremes() -> Result<(), SeriesError> {
    let at = |seconds| Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_secs(seconds));
    let source = hours(
        iter::zip([f64::MAX, -f64::MAX], [0, 2])
            .map(|(value, seconds)| Ok(Hour { interval: Range { start: at(seconds)?, end: at(seconds.strict_add(2))? }, dry_bulb: Some(ThermodynamicTemperature::new::<kelvin>(value)), .. }))
            .collect::<Result<Vec<_>, jiff::Error>>()
            .map_err(WeatherError::Time)?,
    )?;
    let sampled = resampled(&source, SignedDuration::from_secs(1), SignedDuration::ZERO)?;
    assert_eq!(AsRef::<[Hour]>::as_ref(&sampled).iter().filter_map(|hour| hour.dry_bulb.map(|value| value.get::<kelvin>())).collect::<Vec<_>>(), [f64::MAX, f64::MAX, 0.0, -f64::MAX]);
    Ok(())
}

#[test]
fn interpolation_preserves_nanosecond_coordinates() -> Result<(), SeriesError> {
    let step = SignedDuration::from_nanos(1);
    let span = SignedDuration::from_nanos(2);
    let middle = Timestamp::MAX.checked_sub(span).map_err(WeatherError::Time)?;
    let start = middle.checked_sub(span).map_err(WeatherError::Time)?;
    let source = hours(vec![
        Hour { interval: Range { start, end: middle }, dry_bulb: Some(ThermodynamicTemperature::new::<kelvin>(0.0)), infrared: Some(HeatFluxDensity::new::<watt_per_square_meter>(0.0)), .. },
        Hour {
            interval: Range { start: middle, end: Timestamp::MAX },
            dry_bulb: Some(ThermodynamicTemperature::new::<kelvin>(4.0)),
            infrared: Some(HeatFluxDensity::new::<watt_per_square_meter>(4.0)),
            ..
        },
    ])?;
    let sampled = resampled(&source, step, SignedDuration::ZERO)?;
    let records: &[Hour] = sampled.as_ref();
    assert_eq!(records.iter().filter_map(|hour| hour.dry_bulb.map(|value| value.get::<kelvin>())).collect::<Vec<_>>(), [0.0, 0.0, 2.0, 4.0]);
    assert_eq!(records.iter().filter_map(|hour| hour.infrared.map(|value| value.get::<watt_per_square_meter>())).collect::<Vec<_>>(), [0.0, 1.0, 3.0, 4.0]);
    Ok(())
}

#[test]
fn completed_quantities_return_conversion_errors() -> Result<(), SeriesError> {
    let step = SignedDuration::from_hours(1);
    let source = hours(vec![Hour {
        interval: Range { start: Timestamp::UNIX_EPOCH, end: Timestamp::UNIX_EPOCH.checked_add(step).map_err(WeatherError::Time)? },
        dry_bulb: Some(ThermodynamicTemperature::new::<kelvin>(1.0e308)),
        dew_point: Some(ThermodynamicTemperature::new::<kelvin>(273.15)),
        relative_humidity: Some(Ratio::new::<ratio>(0.5)),
        opaque_sky_cover: Some(Ratio::new::<ratio>(0.0)),
        ..
    }])?;
    let source = completed(&source)?;
    assert_matches!(resampled(&source, step, SignedDuration::ZERO), Err(WeatherError::Conversion(ConversionError::OutOfBounds)));
    Ok(())
}

#[test]
fn resampled_totals_return_conversion_errors() -> Result<(), SeriesError> {
    let at = |seconds| Timestamp::UNIX_EPOCH.checked_add(SignedDuration::from_secs(seconds));
    let source = hours(
        [0, 1]
            .into_iter()
            .map(|seconds| Ok(Hour { interval: Range { start: at(seconds)?, end: at(seconds.strict_add(1))? }, precipitation: Some(Length::new::<meter>(f64::MAX)), .. }))
            .collect::<Result<Vec<_>, jiff::Error>>()
            .map_err(WeatherError::Time)?,
    )?;
    let step = SignedDuration::from_secs(2);
    let source = resampled(&source, step, SignedDuration::ZERO)?;
    assert_matches!(resampled(&source, step, SignedDuration::ZERO), Err(WeatherError::Conversion(ConversionError::OutOfBounds)));
    Ok(())
}

#[test]
fn calendar_days_preserve_leap_rules() -> Result<(), Malformed> {
    let leap: Day = "2/29".parse()?;
    let ordinal: Day = "60".parse()?;

    assert_eq!(leap, "29-Feb".parse()?);
    assert_matches!(leap.date(2024), Ok(date) if (date.month(), date.day()) == (2, 29));
    assert_matches!(leap.date(2023), Err(WeatherError::Time(_)));
    assert_matches!(ordinal.date(2024), Ok(date) if (date.month(), date.day()) == (3, 1));
    assert_matches!("2/30".parse::<Day>(), Err(Malformed::Time(_)));
    Ok(())
}
