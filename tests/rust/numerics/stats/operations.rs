use core::{assert_matches, range::RangeInclusive};

use approx::assert_ulps_eq;
use itertools::Either;
use nalgebra::dmatrix;
use nonempty_collections::{NonEmptyIterator as _, nebtm};
use rasm_stats::{
    Bounds, BoundsError, StatsError,
    calibration::{Trace, convergence},
    descriptive::quantile,
};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn weighted_quantiles_preserve_finite_extrema_and_r8_positions() {
    for (frequencies, probability, expected) in [
        ([(-f64::MAX, 1), (f64::MAX, 1)], 0.5, 0.0),
        ([(-f64::MAX, 3), (f64::MAX, 2)], 0.5, -f64::MAX),
        ([(-f64::MAX, 2), (f64::MAX, 3)], 0.5, f64::MAX),
        ([(-f64::MAX, 1), (f64::MAX, 1)], 0.0, -f64::MAX),
        ([(-f64::MAX, 1), (f64::MAX, 1)], 1.0, f64::MAX),
        ([(0.0, 1), (8.0, 9)], 0.125, 5.0),
    ] {
        assert_ulps_eq!(quantile(frequencies, probability), Some(expected));
    }
}

#[test]
fn retained_nonfinite_draws_preserve_source_locations() -> Result<(), BoundsError> {
    let bounds = Bounds::try_new(RangeInclusive { start: 0.0, last: 6.0 })?;
    let space = nebtm![0_u8 => bounds, 1 => bounds];
    let traces = vec![Trace { draws: dmatrix![0.0, 1.0, 2.0, 3.0, 4.0, 5.0; 5.0, 4.0, 3.0, 2.0, 1.0, 0.0], progress: Vec::new() }; 2];
    for ((trace, dimension, draw), value) in [((0, 1, 0), f64::NAN), ((1, 0, 3), f64::INFINITY), ((1, 1, 5), f64::NEG_INFINITY)] {
        let mut traces = traces.clone();
        traces[trace].draws[(dimension, draw)] = value;
        assert_matches!(
            convergence(&space, &traces),
            Err(StatsError::Draw { trace: found_trace, dimension: found_dimension, draw: found_draw })
                if (found_trace, found_dimension, found_draw) == (trace, dimension, draw),
            "Draw failure must retain source location {trace}/{dimension}/{draw}"
        );
    }
    Ok(())
}

#[test]
fn split_diagnostics_ignore_discarded_nonfinite_draws() -> Result<(), Either<BoundsError, StatsError>> {
    let space = nebtm![() => Bounds::try_new(RangeInclusive { start: -2.0, last: 2.0 }).map_err(Either::Left)?];
    let traces = [
        Trace { draws: dmatrix![-1.0, 1.0, f64::NAN, -1.0, 1.0], progress: Vec::new() },
        Trace { draws: dmatrix![-1.0, 1.0, f64::INFINITY, -1.0, 1.0, f64::NEG_INFINITY, f64::NAN], progress: Vec::new() },
    ];
    let diagnostics = convergence(&space, &traces).map_err(Either::Right)?;
    assert_ulps_eq!(diagnostics.values().next().0.rhat, 0.5_f64.sqrt());
    Ok(())
}
