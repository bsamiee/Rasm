#![feature(default_field_values)]

use core::{assert_matches, error::Error, num::NonZero};

use approx::assert_abs_diff_eq;
use claims::assert_err;
use ndarray::{Array1, array};
use rasm_spectra::{
    Rate, Samples, Series, SpectraError,
    power::{Average, Scaling, Taper, Welch, estimate, spectrogram, transfer},
};
use rayon::ThreadPoolBuilder;
use uom::si::{f64::Frequency, frequency::hertz, time::second};

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn impulse_preserves_one_sided_power_and_frequency_bins() -> Result<(), SpectraError> {
    [
        (const { NonZero::new(4).unwrap() }, vec![0.0, 2.5, 5.0], vec![0.025, 0.05, 0.025], vec![0.0625, 0.125, 0.0625]),
        (const { NonZero::new(5).unwrap() }, vec![0.0, 2.0, 4.0], vec![0.02, 0.04, 0.04], vec![0.04, 0.08, 0.08]),
    ]
    .into_iter()
    .try_for_each(|(segment, frequencies, density, power)| {
        let series = Series { rate: Rate::try_new(Frequency::new::<hertz>(10.0))?, samples: Samples::try_new(Array1::from_shape_fn(segment.get(), |index| if index == 0 { 1.0 } else { 0.0 }))? };
        [(Scaling::Density, density), (Scaling::Spectrum, power)].into_iter().try_for_each(|(scaling, expected)| {
            let welch = Welch { segment, taper: Taper::Rectangular, detrend: None, scaling, .. };
            let spectrum = estimate(&series, welch)?;
            let frames = spectrogram(&series, welch)?;
            assert_abs_diff_eq!(spectrum.frequencies.map(Frequency::get::<hertz>).to_vec(), frequencies, epsilon = 1.0e-12);
            assert_abs_diff_eq!(spectrum.powers.to_vec(), expected, epsilon = 1.0e-12);
            assert_eq!(frames.powers.dim(), (1, expected.len()));
            assert_abs_diff_eq!(frames.powers.row(0).to_vec(), expected, epsilon = 1.0e-12);
            Ok(())
        })
    })
}

#[test]
fn sampled_sine_retains_integrated_power() -> Result<(), SpectraError> {
    let series = Series { rate: Rate::try_new(Frequency::new::<hertz>(8.0))?, samples: Samples::try_new(array![0.0, 1.0, 0.0, -1.0])? };
    [(Scaling::Density, vec![0.0, 0.25, 0.0]), (Scaling::Spectrum, vec![0.0, 0.5, 0.0])].into_iter().try_for_each(|(scaling, expected)| {
        let spectrum = estimate(&series, Welch { segment: const { NonZero::new(4).unwrap() }, taper: Taper::Rectangular, detrend: None, scaling, .. })?;
        assert_abs_diff_eq!(spectrum.frequencies.map(Frequency::get::<hertz>).to_vec(), vec![0.0, 2.0, 4.0], epsilon = 1.0e-12);
        assert_abs_diff_eq!(spectrum.powers.to_vec(), expected, epsilon = 1.0e-12);
        Ok(())
    })
}

#[test]
fn padded_frames_reset_destructive_input_and_omit_incomplete_tail() -> Result<(), Box<dyn Error>> {
    let amplitudes = [1.0_f64, 0.0, -2.0, 0.0, 3.0, 0.0, -4.0, 0.0];
    let series = Series {
        rate: Rate::try_new(Frequency::new::<hertz>(10.0))?,
        samples: Samples::try_new(Array1::from_iter(amplitudes.into_iter().flat_map(|amplitude| [amplitude, 0.0, 0.0]).chain([100.0, 100.0])))?,
    };
    let welch =
        Welch { segment: const { NonZero::new(3).unwrap() }, hop: Some(const { NonZero::new(3).unwrap() }), padding: 509, taper: Taper::Rectangular, detrend: None, scaling: Scaling::Spectrum, .. };
    let frames = ThreadPoolBuilder::new().num_threads(1).build()?.install(|| spectrogram(&series, welch))?;
    assert_eq!(frames.powers.dim(), (amplitudes.len(), 257));
    assert_abs_diff_eq!(frames.times.mapv(|time| time.get::<second>()).to_vec(), vec![0.15, 0.45, 0.75, 1.05, 1.35, 1.65, 1.95, 2.25], epsilon = 1.0e-12);
    assert_abs_diff_eq!(frames.frequencies[1].get::<hertz>(), 10.0 / 512.0, epsilon = 1.0e-12);
    assert_abs_diff_eq!(frames.frequencies[256].get::<hertz>(), 5.0, epsilon = 1.0e-12);
    frames.powers.rows().into_iter().zip(amplitudes).for_each(|(row, amplitude)| {
        let expected = Array1::from_shape_fn(257, |bin| amplitude.powi(2) * if bin == 0 || bin == 256 { 1.0 / 9.0 } else { 2.0 / 9.0 });
        assert_abs_diff_eq!(row.to_vec(), expected.to_vec(), epsilon = 1.0e-12);
    });
    Ok(())
}

#[test]
fn transfer_uses_shared_prefix_under_mean_and_median() -> Result<(), SpectraError> {
    [12_usize, 16].into_iter().try_for_each(|samples| {
        let input = Series { rate: Rate::try_new(Frequency::new::<hertz>(10.0))?, samples: Samples::try_new(Array1::from_shape_fn(samples, |index| if index.is_multiple_of(4) { 1.0 } else { 0.0 }))? };
        let output = Series { rate: input.rate, samples: Samples::try_new(input.samples.mapv(|sample| sample * 2.0))? };
        let extended_input = Series { rate: input.rate, samples: Samples::try_new(Array1::from_iter(input.samples.iter().copied().chain([99.0; 8])))? };
        let extended_output = Series { rate: output.rate, samples: Samples::try_new(Array1::from_iter(output.samples.iter().copied().chain([-77.0; 8])))? };
        [Average::Mean, Average::Median].into_iter().flat_map(|average| [(&input, &extended_output), (&extended_input, &output)].map(|(from, to)| (average, from, to))).try_for_each(
            |(average, from, to)| {
                let measured = transfer(
                    from,
                    to,
                    Welch { segment: const { NonZero::new(4).unwrap() }, hop: Some(const { NonZero::new(4).unwrap() }), padding: 4, taper: Taper::Rectangular, detrend: None, average, .. },
                )?;
                assert_abs_diff_eq!(measured.frequencies.map(Frequency::get::<hertz>).to_vec(), vec![0.0, 1.25, 2.5, 3.75, 5.0], epsilon = 1.0e-12);
                assert_abs_diff_eq!(measured.response.mapv(|bin| bin.re).to_vec(), vec![2.0; 5], epsilon = 1.0e-12);
                assert_abs_diff_eq!(measured.response.mapv(|bin| bin.im).to_vec(), vec![0.0; 5], epsilon = 1.0e-12);
                assert_abs_diff_eq!(measured.coherence.to_vec(), vec![1.0; 5], epsilon = 1.0e-12);
                Ok(())
            },
        )
    })
}

#[test]
fn transfer_refuses_unusable_shared_windows() -> Result<(), SpectraError> {
    let input = Series { rate: Rate::try_new(Frequency::new::<hertz>(10.0))?, samples: Samples::try_new(array![1.0, 0.0, 0.0, 0.0])? };
    let short = Series { rate: input.rate, samples: Samples::try_new(array![1.0, 0.0, 0.0])? };
    let silent = Series { rate: input.rate, samples: Samples::try_new(Array1::zeros(4))? };
    let welch = Welch { segment: const { NonZero::new(4).unwrap() }, taper: Taper::Rectangular, detrend: None, .. };
    assert_matches!(assert_err!(transfer(&input, &short, welch)), SpectraError::Segment { segment: _, padding: 0, samples: 3 });
    assert_matches!(assert_err!(transfer(&input, &silent, welch)), SpectraError::Power { frequency: _ });
    Ok(())
}
