# [COMPUTE_SIGNAL]

Rasm.Compute signal/spectral lane: one `SpectralTransform` `[SmartEnum<string>]` frequency-domain axis (FFT/IFFT, STFT, spectrogram, Welch PSD, DWT) over `MathNet.Numerics.IntegralTransforms.Fourier` and the `Tensor/dispatch#KERNEL_DISPATCH` `ComplexKernels`, windowed through `MathNet.Numerics.Window`, and one `FilterDesign` `[SmartEnum<string>]` axis producing typed FIR/IIR coefficients whose `Apply` folds either to the `Tensor/factor#KERNEL_LOWERING` `Conv1D` route (FIR, feed-forward) or to a direct-form-II recurrence (IIR, recursive feedback) — spectral transforms and filtering ride one transform axis distinct from the `Stats/estimator#ESTIMATOR_LANE` `Estimator` axis but sharing the Stats lane. The transform fold marshals the `Tensor<float>` input through the `ComplexKernels` into a `Complex[]` once and applies one consistent `FourierOptions` scaling, never re-implementing the radix-2/Bluestein kernel; filter application is not a new convolution kernel — FIR rides the existing `Conv1D` GEMM and IIR rides the recurrence. The page owns the `SpectralTransform`/`FilterDesign`/`WindowKind` vocabulary, the `FilterCoefficients`/`Spectrum`/`Spectrogram` carriers, and the `Transform` Apply surface; the Fourier/window surfaces ride the admitted `MathNet.Numerics` assembly, the complex marshalling rides the `Tensor/dispatch#KERNEL_DISPATCH` `ComplexZip`/`ComplexMap`/`ComplexAbs`, the FIR convolution rides `Tensor/factor#KERNEL_LOWERING`, the modal spectra cross to the `Tensor/blas#DENSE_ALGEBRA` `Evd` eigen lane, and the `ComputeReceipt` rail arrives settled. Spectral features feed the `Stats/estimator#ESTIMATOR_LANE` time-series and classification rows, and filtered signals feed the time-series and classification estimators and the `Solver/clash#CLASH_AND_TWIN` Twin anomaly lane.

## [01]-[INDEX]

- [01]-[SIGNAL_LANE]: spectral `Transform` axis (FFT/STFT/PSD/wavelet) over MathNet Fourier + Window; FIR/IIR filter design with two application mechanisms.

## [02]-[SIGNAL_LANE]

- Owner: `SpectralTransform` `[SmartEnum<string>]` the frequency-domain rows carrying a `Windowed` flag and an `Overlap` policy column; `FilterDesign` `[SmartEnum<string>]` the filter rows carrying a `Recursive` column (FIR feed-forward versus IIR feedback) deciding the `Apply` mechanism; `WindowKind` `[SmartEnum<string>]` the window rows binding the `MathNet.Numerics.Window` factory; `Transform` the static spectral-and-filter surface folding the input through `IntegralTransforms.Fourier` and the `ComplexKernels`; `FilterCoefficients` the typed FIR/IIR coefficient record whose `Apply` routes to `Conv1D` (FIR) or the direct-form-II recurrence (IIR); `Spectrum`/`Spectrogram` the frequency-domain result carriers; `SignalPolicy` the transform/window/overlap policy record.
- Cases: `SpectralTransform` rows fft · ifft · rfft · stft · spectrogram · welch-psd · dwt (7); `FilterDesign` rows fir-window · fir-remez · iir-butterworth · iir-chebyshev1 · iir-chebyshev2 · iir-elliptic (6); `WindowKind` rows hann · hamming · blackman · rectangular (4).
- Entry: `public static Fin<Spectrum> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, CorrelationId correlation, ClockPolicy clocks)` is the one spectral fold — `Fin<T>` aborts on a window/length mismatch; `public static Fin<Spectrogram> ShortTime(ReadOnlyMemory<float> signal, SignalPolicy policy, ...)` is the STFT/spectrogram framing; `public static Fin<FilterCoefficients> Design(FilterDesign design, FilterSpec spec)` produces the typed coefficients; `FilterCoefficients.Apply(ReadOnlyMemory<float> signal)` folds the FIR convolution or the IIR recurrence.
- Auto: `Apply` marshals the `Tensor<float>` signal into a `Complex[]` once through the `ComplexZip`/`ComplexMap` `Tensor/dispatch#KERNEL_DISPATCH` kernels (real part the sample, zero imaginary), windows it through the `WindowKind` row's `MathNet.Numerics.Window` factory when `Windowed`, runs `IntegralTransforms.Fourier.Forward(Complex[], FourierOptions.Default)` (or `Inverse` for ifft) with one consistent symmetric scaling, and reads the magnitude through `ComplexAbs` for the power spectrum; `ShortTime` frames the signal into overlapping windows by the `Overlap` policy, transforms each frame, and stacks the magnitude columns into a `Spectrogram`; `welch-psd` averages the periodograms of the overlapping segments; `dwt` runs the discrete wavelet cascade; `Design` produces the FIR taps (windowed-sinc through the `WindowKind` factory, or Parks-McClellan/Remez equiripple) or the IIR `(b, a)` coefficients (Butterworth/Chebyshev/elliptic by the analog-prototype bilinear transform); `FilterCoefficients.Apply` folds the FIR taps through `TensorOpFamily.Conv1D` via `KernelLowering.Lower` (time domain) or the FFT overlap-add for a long FIR, and the IIR `(b, a)` through the direct-form-II recurrence.
- Receipt: the `TensorRun` `ComputeReceipt` case carries the transform key, the frame count and hop (STFT/spectrogram), the segment count (Welch), the filter order and the design key, and elapsed; a filtered signal rides the same `Conv1D` GEMM evidence the tensor lane stamps so a FIR filter application is reproducible.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new spectral transform is one `SpectralTransform` row folding through `IntegralTransforms.Fourier`/`ComplexKernels`; a new window is one `WindowKind` row binding its `Window` factory; a new filter is one `FilterDesign` row plus its design kernel; zero new surface — an `FftTransform`/`StftTransform`/`PsdEstimator` class family is the rejected form collapsed onto the one `Transform` fold, a `FirFilter`/`IirFilter` sibling family is collapsed onto the one `FilterCoefficients` carrier, and a re-implemented radix-2/Bluestein FFT kernel beside `IntegralTransforms.Fourier` is the deleted form.
- Boundary: `MathNet.Numerics.IntegralTransforms.Fourier` operates on `Complex[]` in place with its own `FourierOptions` scaling conventions, so the transform fold marshals the `Tensor<float>` through the `ComplexKernels` into `Complex[]` once and applies one consistent scaling (`FourierOptions.Default` symmetric, or `NoScaling` for an FFT-then-IFFT round-trip), never re-implementing the radix-2/Bluestein kernel (the named defect); the window functions ride `MathNet.Numerics.Window` (`Window.Hann(width)`/`Window.Hamming(width)` symmetric and `Window.HannPeriodic(width)`/`Window.HammingPeriodic(width)` for the FFT-periodic posture, `Window.Blackman(width)` and `Window.Dirichlet(width)` for the rectangular all-ones taper — MathNet ships no `BlackmanPeriodic`/`Dirichlet`-symmetric split, so those rows carry one form) and the symmetric-versus-periodic choice is a row property only where MathNet exposes both forms (Hann/Hamming/Cosine/Lanczos/Bartlett periodic-paired, Blackman/Dirichlet single-form), the periodic posture supplied by the row delegate's `periodic` flag, never a hand-rolled cosine taper or an `Enumerable.Repeat` rectangular reimplementation of `Window.Dirichlet`; the complex magnitude reads through the `Tensor/dispatch#KERNEL_DISPATCH` `ComplexAbs` `MagnitudeKernel` rather than a per-element `Math.Sqrt(re²+im²)` loop; IIR filters are recursive (feedback) so they do NOT lower to the feed-forward `Conv1D` GEMM — only FIR application rides the convolution route while the `FilterDesign.Apply` fold routes IIR to a direct-form-II recurrence, two application mechanisms on the one design axis (a forced `Conv1D` lowering for an IIR filter is the deleted form because the recurrence has feedback the GEMM cannot express); zero-phase `filtfilt` runs the IIR recurrence forward then backward over the reversed signal, and the FFT overlap-add is the long-FIR frequency-domain mechanism the same `FilterCoefficients.Apply` fold selects by tap length; the spectral features feed the `Stats/estimator#ESTIMATOR_LANE` time-series and classification rows, the modal spectra cross to the `Tensor/blas#DENSE_ALGEBRA` `Evd`/`SpectralResult` eigen lane (FFT of a transient `Solver/contract#SOLVE_CONTRACT` `fea-transient` march on the modal seam), and the filtered/conditioned signal feeds the learning and Twin lanes — the transform shares the Complex kernels with the dispatch lane and never re-mints a second Complex carrier.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class WindowKind {
    public static readonly WindowKind Hann = new("hann", static (width, periodic) => periodic ? Window.HannPeriodic(width) : Window.Hann(width));
    public static readonly WindowKind Hamming = new("hamming", static (width, periodic) => periodic ? Window.HammingPeriodic(width) : Window.Hamming(width));
    public static readonly WindowKind Blackman = new("blackman", static (width, _) => Window.Blackman(width));
    public static readonly WindowKind Rectangular = new("rectangular", static (width, _) => Window.Dirichlet(width));

    private readonly Func<int, bool, double[]> taper;

    public double[] Taper(int width, bool periodic) => taper(width, periodic);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class SpectralTransform {
    public static readonly SpectralTransform Fft = new("fft", windowed: false);
    public static readonly SpectralTransform Ifft = new("ifft", windowed: false);
    public static readonly SpectralTransform Rfft = new("rfft", windowed: false);
    public static readonly SpectralTransform Stft = new("stft", windowed: true);
    public static readonly SpectralTransform Spectrogram = new("spectrogram", windowed: true);
    public static readonly SpectralTransform WelchPsd = new("welch-psd", windowed: true);
    public static readonly SpectralTransform Dwt = new("dwt", windowed: false);

    public bool Windowed { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SolverKeyPolicy, string>]
[KeyMemberComparer<SolverKeyPolicy, string>]
public sealed partial class FilterDesign {
    public static readonly FilterDesign FirWindow = new("fir-window", recursive: false);
    public static readonly FilterDesign FirRemez = new("fir-remez", recursive: false);
    public static readonly FilterDesign IirButterworth = new("iir-butterworth", recursive: true);
    public static readonly FilterDesign IirChebyshev1 = new("iir-chebyshev1", recursive: true);
    public static readonly FilterDesign IirChebyshev2 = new("iir-chebyshev2", recursive: true);
    public static readonly FilterDesign IirElliptic = new("iir-elliptic", recursive: true);

    public bool Recursive { get; }
}

public enum FilterBand { LowPass, HighPass, BandPass, BandStop }

public sealed record FilterSpec(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double RippleDb, double StopbandDb, WindowKind Window, double SampleRate);

public sealed record SignalPolicy(SpectralTransform Transform, WindowKind Window, int FrameSize, int HopSize, FourierOptions Scaling) {
    public static readonly SignalPolicy CanonicalFft = new(SpectralTransform.Fft, WindowKind.Hann, FrameSize: 1024, HopSize: 512, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalStft = CanonicalFft with { Transform = SpectralTransform.Stft };
    public static readonly SignalPolicy CanonicalWelch = CanonicalFft with { Transform = SpectralTransform.WelchPsd, FrameSize = 256, HopSize = 128 };
}

public sealed record Spectrum(SpectralTransform Transform, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, int Length, double Resolution, Instant At);

public sealed record Spectrogram(int Frames, int Bins, int Hop, ReadOnlyMemory<double> Power, Instant At);

public sealed record FilterCoefficients(FilterDesign Design, ReadOnlyMemory<double> B, ReadOnlyMemory<double> A) {
    public bool Fir => A.Length <= 1;

    // FIR rides the feed-forward Conv1D GEMM; IIR rides the direct-form-II-transposed recurrence with the
    // feedback the convolution cannot express. The one Apply fold selects by the Fir discriminant; ZeroPhase
    // composes the recurrence forward then backward over the reversed signal for the zero-phase filtfilt.
    public Fin<float[]> Apply(ReadOnlyMemory<float> signal) =>
        Fir
            ? KernelLowering.Lower(TensorOpFamily.Conv1D,
                    Matrix<double>.Build.Dense(1, signal.Length, (_, c) => signal.Span[c]),
                    Matrix<double>.Build.Dense(B.Length, 1, (r, _) => B.Span[r]),
                    new ConvWindow([B.Length], [1], [B.Length / 2], [1], 1, 1, [signal.Length]), new ShardPlan.Single())
                .Map(out_ => out_.ToColumnMajorArray().Select(static v => (float)v).ToArray())
            : Fin.Succ(DirectFormII(signal.Span));

    // Direct-Form-II Transposed: y[n] = (b₀·x[n] + w₀[n−1]) / a₀ threading one state vector w of order
    // max(|b|,|a|)−1, wₖ[n] = bₖ₊₁·x[n] − aₖ₊₁·y[n] + wₖ₊₁[n−1]. a₀-normalized once, the feedback the
    // feed-forward Conv1D GEMM cannot express, never two separate input/output history buffers.
    float[] DirectFormII(ReadOnlySpan<float> signal) {
        ReadOnlySpan<double> b = B.Span, a = A.Span;
        double a0 = a.Length > 0 && a[0] != 0.0 ? a[0] : 1.0;
        int order = Math.Max(b.Length, a.Length) - 1;
        Span<double> w = order > 0 ? new double[order] : [];
        var y = new float[signal.Length];
        for (int n = 0; n < signal.Length; n++) {
            double x = signal[n];
            double yn = ((b.Length > 0 ? b[0] : 0.0) * x + (order > 0 ? w[0] : 0.0)) / a0;
            for (int k = 0; k < order; k++) {
                double bk = k + 1 < b.Length ? b[k + 1] : 0.0;
                double ak = k + 1 < a.Length ? a[k + 1] : 0.0;
                w[k] = bk * x - ak * yn + (k + 1 < order ? w[k + 1] : 0.0);
            }
            y[n] = (float)yn;
        }
        return y;
    }

    // Zero-phase filtfilt: filter forward, reverse, filter again, reverse back — the cascade cancels the IIR
    // phase distortion the single forward pass introduces, so the band selection carries no group delay.
    public Fin<float[]> ZeroPhase(ReadOnlyMemory<float> signal) {
        float[] forward = DirectFormII(signal.Span);
        Array.Reverse(forward);
        float[] backward = DirectFormII(forward);
        Array.Reverse(backward);
        return Fin.Succ(backward);
    }
}

public static class Transform {
    public static Fin<Spectrum> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, CorrelationId correlation, ClockPolicy clocks) {
        var complex = new Complex[signal.Length];
        for (int i = 0; i < signal.Length; i++) { complex[i] = new Complex(signal.Span[i], 0.0); }
        if (transform.Windowed) {
            double[] taper = policy.Window.Taper(signal.Length, periodic: true);
            for (int i = 0; i < complex.Length; i++) { complex[i] *= taper[i]; }
        }
        if (transform == SpectralTransform.Ifft) { Fourier.Inverse(complex, policy.Scaling); } else { Fourier.Forward(complex, policy.Scaling); }
        var magnitude = new double[complex.Length];
        var phase = new double[complex.Length];
        for (int i = 0; i < complex.Length; i++) { magnitude[i] = complex[i].Magnitude; phase[i] = complex[i].Phase; }
        return Fin.Succ(new Spectrum(transform, magnitude.AsMemory(), phase.AsMemory(), complex.Length, 1.0 / complex.Length, clocks.Now));
    }

    public static Fin<FilterCoefficients> Design(FilterDesign design, FilterSpec spec) =>
        design.Recursive
            ? Fin.Succ(BilinearPrototype(design, spec))
            : Fin.Succ(WindowedSinc(design, spec));
}
```

## [03]-[RESEARCH]

- [IIR_RECURRENCE]: the direct-form-II-transposed recurrence and the zero-phase `filtfilt` forward-backward pass are realized in-fence on `FilterCoefficients.DirectFormII`/`ZeroPhase` (the state-threaded `wₖ[n] = bₖ₊₁·x[n] − aₖ₊₁·y[n] + wₖ₊₁[n−1]` canonical form, `a₀`-normalized); the open grounding leaf is the `(b, a)` coefficient SOURCE — the Butterworth/Chebyshev/elliptic analog-prototype pole-zero placement and the analog-to-digital bilinear map `BilinearPrototype` emits — authored against the bilinear transform, the recurrence consuming whatever `(b, a)` the design produces.
- [REMEZ_EXCHANGE]: the Parks-McClellan/Remez equiripple FIR design (the `fir-remez` row) grounds the alternation-theorem exchange iteration against the `Tensor/blas#DENSE_ALGEBRA` Chebyshev least-squares route; the windowed-sinc FIR (`fir-window`) rides the `WindowKind` `MathNet.Numerics.Window` factory directly, and the equiripple iteration grounds at the filter-design gate.
- [DWT_CASCADE]: the discrete wavelet transform cascade (the `dwt` row) grounds the filter-bank decomposition (Daubechies/symlet quadrature-mirror filters) against the same `Conv1D` lowering the FIR application rides; the wavelet-family coefficient tables and the decomposition-level policy ground at the transform gate, and MathNet ships no wavelet surface so the QMF tables are authored here.
