# [COMPUTE_SIGNAL]

Rasm.Compute signal/spectral lane: one `SpectralTransform` `[SmartEnum<string>]` frequency-domain axis whose rows carry transform and inverse delegates, folding to deferred `Transform.Apply` and `Transform.Invert` surfaces. `IO<Fin<T>>` preserves the outer lowering effect and inner domain fault without forcing either inside the numeric lane. Forward and inverse share one surface: `stft` preserves frame phase and overlap-add evidence, while evidence-destroying `spectrogram` and averaged periodograms return typed inverse faults. One `FilterDesign` `[SmartEnum<string>]` axis closes each IIR row's analog prototype into the shared `Bilinear` map, while FIR rows fold to windowed-sinc or equiripple `DenseRoute` least squares.

Vocabulary owned here: `SpectralTransform`/`FilterDesign`/`WindowKind`/`WaveletFamily`, the `SpectralOutput`/`SignalPolicy`/`Spectrogram`/`FilterShape` unions, the admitted `SignalContext`/`FilterContext` interiors, the `Spectrum`/`WaveletDecomposition`/`CrossSpectrum`/`MeasuredMode`/`ModalEstimate`/`FilterCoefficients`/`FilterResponse` carriers, the `FilterSpec` record, and the `Transform` Apply/Invert/Design/Coherence/Modal surface. Per-bin transforms ride `MathNet.Numerics.IntegralTransforms.Fourier` over split `double[]` planes, windowing rides `MathNet.Numerics.Window`, magnitude/phase read `TensorPrimitives.Hypot`/`Atan2`, bin spacing reads `Fourier.FrequencyScale`, FFT overlap-add rides `Tensor/dispatch#KERNEL_DISPATCH` `ComplexZip(TensorOpFamily.Multiply)`, and FIR convolution plus wavelet analysis/synthesis ride `Tensor/factor#KERNEL_LOWERING` `Conv1D`. Equiripple FIR crosses to `Tensor/blas#DENSE_ALGEBRA`; the per-bin complex Hermitian dominant pair stays page-local under a convergence witness because the dense owner is real-typed; `ComputeFault` and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies instants — the App-owned `ClockPolicy` stays at composition. Spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` plus the `Modal` frequency-domain decomposition own measured-mode identification, and `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end; conditioned signals feed those estimators and the twin.

## [01]-[INDEX]

- [01]-[SIGNAL_LANE]: effect-preserving FFT/STFT/PSD/wavelet `Transform` over MathNet Fourier + Window, paired row-owned inversion, Welch cross-spectral coherence, N-channel FDD measured-mode extraction, and family-shaped FIR/IIR design over one band-aware `Bilinear` map with `Conv1D`, pooled FFT overlap-add, direct-form-II recurrence, and magnitude/phase/group-delay response.

## [02]-[SIGNAL_LANE]

- Owner: `SpectralTransform` `[SmartEnum<string>]` rows each carry a `Windowed` admission discriminant, a row-owned `SignalPolicy → Fin<SignalContext>` admission delegate, a `(ReadOnlyMemory<float>, SignalContext, Instant) → IO<Fin<SpectralOutput>>` transform delegate, and a `SpectralOutput → IO<Fin<ReadOnlyMemory<float>>>` inverse delegate. `SignalPolicy` `[Union]` closes `PerBin`, `Framed`, and `Wavelet` evidence; `SignalContext` exists only after row admission. `FilterShape` `[Union]` closes `Windowed`, `Equiripple`, `Butterworth`, `Chebyshev1`, `Chebyshev2`, and `Elliptic` parameter evidence; its `Design` projection selects the corresponding `FilterDesign` row, so no caller supplies a reconstructible design knob. `FilterDesign` rows carry the sole `Recursive` result discriminator and admitted design delegate. `SpectralOutput` owns `Bins`/`Frames`/`Bands`; `Spectrogram` owns inverse-sufficient `Phasor` frames and magnitude-only `Power` frames. `FilterCoefficients` admits invariants before `Apply`, `Response`, or `ZeroPhase`; `FilterResponse` carries magnitude, unwrapped phase, and group delay.
- Cases: `SpectralTransform` fft · rfft · stft · spectrogram · welch-psd · dwt; `SignalPolicy` per-bin · framed · wavelet; `FilterShape` windowed · equiripple · butterworth · chebyshev1 · chebyshev2 · elliptic; `FilterDesign` fir-window · fir-remez · iir-butterworth · iir-chebyshev1 · iir-chebyshev2 · iir-elliptic; `FilterBand` low-pass · high-pass · band-pass · band-stop; `WindowKind` hann · hamming · blackman · blackman-harris · blackman-nuttall · nuttall · flat-top · bartlett · bartlett-hann · cosine · lanczos · triangular · gauss · tukey · rectangular; `WaveletFamily` haar · db2 · db4 · sym4 · coif1.
- Entry: `Transform.Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, IClock clock) → IO<Fin<SpectralOutput>>` composes sample admission with row policy admission, then dispatches over `SignalContext`. `Invert(SpectralOutput output) → IO<Fin<ReadOnlyMemory<float>>>` projects and dispatches the owning row. `Coherence(ReadOnlyMemory<float> x, ReadOnlyMemory<float> y, SignalPolicy policy, IClock clock)` composes synchronous-channel admission plus a two-segment floor with the `welch-psd` row's `Framed` admission. `Modal(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock)` runs the N-channel frequency-domain decomposition over the same Welch admission — per-bin Hermitian cross-PSD matrices, dominant singular pair by power iteration, first-singular-value peak picking with half-power damping — returning the `ModalEstimate` measured-mode set. `Design(FilterSpec spec)` projects `FilterShape.Design`, admits `FilterContext`, dispatches the row, and admits emitted coefficients. `FilterCoefficients.Apply → IO<Fin<float[]>>`, `Response`, and `ZeroPhase` enter through coefficient admission.
- Auto: each `SpectralTransform` `Kernel` realizes one split-plane `Fourier` transform: `fft` full-length, `rfft` Hermitian half-spectrum, `stft` centered magnitude/phase frames, `spectrogram` squared STFT magnitudes, `welch-psd` averaged periodograms, and `dwt` a stride-2 `Conv1D` QMF cascade. Each `Invert` realizes the paired inverse: `fft`/`rfft` reciprocal transforms, `stft` weighted overlap-add, and `dwt` zero-stuff synthesis trimmed to recorded extents. `Design` produces windowed-sinc or equiripple FIR taps and shared-`Bilinear` IIR coefficients. `FilterCoefficients.Apply` routes short-FIR `Conv1D`, pooled long-FIR FFT overlap-add, or direct-form-II-transposed IIR recurrence.
- Receipt: the spectral fold mints no hot-path receipt. Evidence rides `Spectrum.Length`/`BinHz`/`Samples`/`Scaling`, each `Spectrogram` case's `Frames`/`Bins`, `WaveletDecomposition.Levels`/`Extents`, and `CrossSpectrum.Coherence`. Tensor-lowered legs compose the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.TensorRun(Family, Dtype, Elements, SimdWidth, Partitions)` their owning operation stamps. Bare `Fourier` transforms and IIR recurrence mint no fabricated tensor receipt.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new spectral transform is one `SpectralTransform` row binding admission, forward kernel, and inverse; a new window is one `WindowKind` row; a new wavelet is one `WaveletFamily` row. A new filter family adds one parameter-evidence `FilterShape` case and its projected `FilterDesign` row, closing any analog prototype into unchanged `Bilinear`. `FftTransform`/`StftTransform`/`PsdEstimator` collapse onto `Transform.Apply`; inverse siblings collapse onto `Transform.Invert`; FIR/IIR classes collapse onto `FilterCoefficients`; per-family designers collapse onto `FilterShape` plus the row-owned design delegate.
- Boundary: `MathNet.Numerics.IntegralTransforms.Fourier` operates in place. Per-bin and framed kernels apply recorded `FourierOptions`; overlap-add and Welch pin `FourierOptions.NoScaling` and own their normalization. Split real/imaginary arrays let `TensorPrimitives.Hypot`/`Atan2` read contiguous spans; `ForwardReal`/`InverseReal` own packed half-spectra; `Fourier.FrequencyScale` owns bin resolution. Inversion consumes forward evidence: `Spectrum` records `Samples` and `Scaling`; `Spectrogram.Phasor` records `Samples`, `FrameSize`, `Hop`, `Window`, `Scaling`, magnitude, and phase for weighted overlap-add; `Spectrogram.Power` and Welch output typed inverse faults; `WaveletDecomposition` records per-level `Extents`. Window functions ride `MathNet.Numerics.Window`; periodic forms apply only where MathNet exposes them. IIR feedback routes to direct-form-II-transposed after finite, nonempty, nonzero-`a₀` admission. FIR application routes short taps through `KernelLowering`, while long taps accumulate through pooled `MemoryOwner<float>` storage plus FFT overlap-add. `FilterResponse` derives group delay from unwrapped phase. One band-aware `Bilinear` consumes closed-over analog prototypes. Equiripple FIR rides `DenseRoute.Solve` across an explicit transition band. `WaveletFamily` owns scaling tables because MathNet exposes no wavelet surface. Spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` conditions channel pairs and `Modal` extracts the measured modes — the FDD first-singular-value spectrum is an operational estimate whose peaks are honest only where excitation is broadband, so `ModalEstimate` carries the full singular spectrum beside its picked modes and a consumer re-judges a peak against its own floor; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end, and conditioned signals feed learning and the twin.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindowKind {
    public static readonly WindowKind Hann           = new("hann",            static (w, p) => p ? Window.HannPeriodic(w) : Window.Hann(w));
    public static readonly WindowKind Hamming        = new("hamming",         static (w, p) => p ? Window.HammingPeriodic(w) : Window.Hamming(w));
    public static readonly WindowKind Cosine         = new("cosine",          static (w, p) => p ? Window.CosinePeriodic(w) : Window.Cosine(w));
    public static readonly WindowKind Lanczos        = new("lanczos",         static (w, p) => p ? Window.LanczosPeriodic(w) : Window.Lanczos(w));
    public static readonly WindowKind Blackman       = new("blackman",        static (w, _) => Window.Blackman(w));
    public static readonly WindowKind BlackmanHarris = new("blackman-harris", static (w, _) => Window.BlackmanHarris(w));
    public static readonly WindowKind BlackmanNuttall= new("blackman-nuttall",static (w, _) => Window.BlackmanNuttall(w));
    public static readonly WindowKind Nuttall        = new("nuttall",         static (w, _) => Window.Nuttall(w));
    public static readonly WindowKind FlatTop        = new("flat-top",        static (w, _) => Window.FlatTop(w));
    public static readonly WindowKind Bartlett       = new("bartlett",        static (w, _) => Window.Bartlett(w));
    public static readonly WindowKind BartlettHann   = new("bartlett-hann",   static (w, _) => Window.BartlettHann(w));
    public static readonly WindowKind Triangular     = new("triangular",      static (w, _) => Window.Triangular(w));
    public static readonly WindowKind Gauss          = new("gauss",           static (w, _) => Window.Gauss(w, 0.4));   // sigma is RELATIVE to the half-width ((i−c)/(σ·c)); an absolute sigma flattens the taper to rectangular.
    public static readonly WindowKind Tukey          = new("tukey",           static (w, _) => Window.Tukey(w, 0.5));
    public static readonly WindowKind Rectangular    = new("rectangular",     static (w, _) => Window.Dirichlet(w));

    private readonly Func<int, bool, double[]> taper;

    public double[] Taper(int width, bool periodic) => taper(width, periodic);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveletFamily {
    // Daubechies/symlet/coiflet analysis scaling (low-pass) coefficients; the high-pass and synthesis pair derive by the QMF relation, one frozen table per family owning the bank.
    public static readonly WaveletFamily Haar  = new("haar",  [0.70710678118654752, 0.70710678118654752]);
    public static readonly WaveletFamily Db2   = new("db2",   [0.48296291314469025, 0.83651630373746899, 0.22414386804185735, -0.12940952255092145]);
    public static readonly WaveletFamily Db4   = new("db4",   [0.23037781330885523, 0.71484657055254153, 0.63088076792959036, -0.02798376941698385, -0.18703481171888114, 0.03084138183598697, 0.03288301166698295, -0.01059740178499728]);
    public static readonly WaveletFamily Sym4  = new("sym4",  [-0.07576571478927333, -0.02963552764599851, 0.49761866763201960, 0.80373875180591614, 0.29785779560527736, -0.09921954357684722, -0.01260396726203783, 0.03222310060404270]);
    public static readonly WaveletFamily Coif1 = new("coif1", [-0.01565572813546454, -0.07273261951285648, 0.38486484686420286, 0.85257202021225542, 0.33789766245780922, -0.07273261951285648]);

    private readonly double[] scaling;

    public ReadOnlyMemory<double> LowPass => scaling;

    // QMF mirror: the analysis high-pass alternates sign over the reversed scaling coefficients.
    public double[] HighPass() {
        double[] g = new double[scaling.Length];
        for (int k = 0; k < g.Length; k++) { g[k] = ((k & 1) == 0 ? 1.0 : -1.0) * scaling[scaling.Length - 1 - k]; }
        return g;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralTransform {
    // Kernel IS the forward transform and Invert IS its inverse — rows whose forward destroys evidence bind a
    // typed-fault inverse naming the destruction, so `Transform.Invert` stays total over every row.
    public static readonly SpectralTransform Fft         = new("fft",         windowed: false, SignalPolicy.AdmitPerBin, Transform.PerBin, Transform.InvertBins);
    public static readonly SpectralTransform Rfft        = new("rfft",        windowed: false, SignalPolicy.AdmitPerBin, Transform.RealForward, Transform.InvertPacked);
    public static readonly SpectralTransform Stft        = new("stft",        windowed: true,  SignalPolicy.AdmitFramed, Transform.ShortTime, Transform.InvertFrames);
    public static readonly SpectralTransform Spectrogram = new("spectrogram", windowed: true,  SignalPolicy.AdmitFramed, Transform.PowerSpectrogram, static o => Transform.NonInvertible(o, "<phase-discarded>"));
    public static readonly SpectralTransform WelchPsd    = new("welch-psd",   windowed: true,  SignalPolicy.AdmitFramed, Transform.Welch, static o => Transform.NonInvertible(o, "<segment-averaged>"));
    public static readonly SpectralTransform Dwt         = new("dwt",         windowed: false, SignalPolicy.AdmitWavelet, Transform.Wavelet, Transform.Synthesize);

    private readonly Func<SpectralTransform, SignalPolicy, int, Fin<SignalContext>> admit;
    private readonly Func<ReadOnlyMemory<float>, SignalContext, Instant, IO<Fin<SpectralOutput>>> kernel;
    private readonly Func<SpectralOutput, IO<Fin<ReadOnlyMemory<float>>>> invert;

    public bool Windowed { get; }

    internal Fin<SignalContext> Admit(SignalPolicy policy, int samples) =>
        Windowed != policy.IsFramed
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-window-policy:{this}>"))
            : admit(this, policy, samples);

    internal IO<Fin<SpectralOutput>> Run(ReadOnlyMemory<float> signal, SignalContext context, Instant at) => kernel(signal, context, at);

    public IO<Fin<ReadOnlyMemory<float>>> Invert(SpectralOutput output) => invert(output);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterDesign {
    // Each IIR row closes its analog prototype into `Run`; every new row must bind its design without nullable columns or throwing accessors.
    public static readonly FilterDesign FirWindow      = new("fir-window",      recursive: false, static (_, spec) => Fin.Succ(Transform.WindowedSinc(spec)));
    public static readonly FilterDesign FirRemez       = new("fir-remez",       recursive: false, static (_, spec) => Transform.Equiripple(spec));
    public static readonly FilterDesign IirButterworth = new("iir-butterworth", recursive: true,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, Transform.Butterworth, spec)));
    public static readonly FilterDesign IirChebyshev1  = new("iir-chebyshev1",  recursive: true,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, Transform.Chebyshev1, spec)));
    public static readonly FilterDesign IirChebyshev2  = new("iir-chebyshev2",  recursive: true,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, Transform.Chebyshev2, spec)));
    public static readonly FilterDesign IirElliptic    = new("iir-elliptic",    recursive: true,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, Transform.Elliptic, spec)));

    private readonly Func<FilterDesign, FilterContext, Fin<FilterCoefficients>> design;

    public bool Recursive { get; }

    internal Fin<FilterCoefficients> Run(FilterSpec spec) => spec.Admit().Bind(context => design(this, context));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterBand {
    // One band owns all three modalities as row data — the FIR windowed-sinc combine, the analog→s-plane zpk transform the Bilinear consumes, and the equiripple desired/weight at a normalized frequency.
    // A new band (all-pass, shelving) is one row each consumer's delegate dispatch breaks on at compile time; `Upper` reports whether the band reads the second edge (BP/BS).
    public static readonly FilterBand LowPass  = new("low-pass",  upper: false,
        combine: static (lower, _, _)           => lower,
        analog:  static (p, w1, _)              => ([.. p.Zeros.Select(z => z * w1)], [.. p.Poles.Select(s => s * w1)], Math.Pow(w1, p.Poles.Length - p.Zeros.Length)),
        ideal:   static (f, c1, c2, _, wp, ws)  => f <= c1 ? (1.0, wp) : f >= c2 ? (0.0, ws) : (0.0, 0.0));
    public static readonly FilterBand HighPass = new("high-pass", upper: false,
        combine: static (lower, _, taps)        => Invert(lower, taps),
        analog:  static (p, w1, _)              => ([.. p.Zeros.Select(z => w1 / z), .. Origin(p.Poles.Length - p.Zeros.Length)], [.. p.Poles.Select(s => w1 / s)], BandGain(p.Zeros, p.Poles)),
        ideal:   static (f, c1, c2, _, wp, ws)  => f <= c1 ? (0.0, ws) : f >= c2 ? (1.0, wp) : (0.0, 0.0));
    public static readonly FilterBand BandPass = new("band-pass", upper: true,
        combine: static (lower, upper, _)       => Subtract(upper, lower),
        analog:  static (p, w1, w2)             => ([.. Pair(p.Zeros, (w2 - w1) * 0.5, w1 * w2), .. Origin(p.Poles.Length - p.Zeros.Length)], Pair(p.Poles, (w2 - w1) * 0.5, w1 * w2), Math.Pow(w2 - w1, p.Poles.Length - p.Zeros.Length)),
        ideal:   static (f, c1, c2, tr, wp, ws) => c1 <= f && f <= c2 ? (1.0, wp) : f <= c1 - tr || f >= c2 + tr ? (0.0, ws) : (0.0, 0.0));
    public static readonly FilterBand BandStop = new("band-stop", upper: true,
        combine: static (lower, upper, taps)    => Invert(Subtract(upper, lower), taps),
        analog:  static (p, w1, w2)             => ([.. PairInv(p.Zeros, (w2 - w1) * 0.5, w1 * w2), .. Notch(p.Poles.Length - p.Zeros.Length, Math.Sqrt(w1 * w2))], PairInv(p.Poles, (w2 - w1) * 0.5, w1 * w2), BandGain(p.Zeros, p.Poles)),
        ideal:   static (f, c1, c2, tr, wp, ws) => f <= c1 || f >= c2 ? (1.0, wp) : c1 + tr <= f && f <= c2 - tr ? (0.0, ws) : (0.0, 0.0));

    private readonly Func<double[], double[], int, double[]> combine;
    private readonly Func<(Complex[] Zeros, Complex[] Poles, double Gain), double, double, (Complex[] Zeros, Complex[] Poles, double Gain)> analog;
    private readonly Func<double, double, double, double, double, double, (double Desired, double Weight)> ideal;

    public bool Upper { get; }

    // FIR windowed-sinc combine (lower/upper kernels → taps); the s-plane zpk band-transform; the equiripple desired/weight at normalized frequency f with transition tr and band weights.
    public double[] Combine(double[] lower, double[] upper, int taps) => combine(lower, upper, taps);
    public (Complex[] Zeros, Complex[] Poles, double Gain) ToAnalog((Complex[] Zeros, Complex[] Poles, double Gain) prototype, double w1, double w2) => analog(prototype, w1, w2);
    public (double Desired, double Weight) Ideal(double f, double c1, double c2, double transition, double passWeight, double stopWeight) => ideal(f, c1, c2, transition, passWeight, stopWeight);

    private static double[] Invert(double[] k, int taps) { double[] b = (double[])k.Clone(); for (int i = 0; i < b.Length; i++) { b[i] = -b[i]; } b[taps / 2] += 1.0; return b; }   // spectral inversion.
    private static double[] Subtract(double[] upper, double[] lower) { double[] b = new double[upper.Length]; for (int i = 0; i < b.Length; i++) { b[i] = upper[i] - lower[i]; } return b; }
    private static System.Collections.Generic.IEnumerable<Complex> Origin(int count) { for (int i = 0; i < count; i++) { yield return Complex.Zero; } }
    private static System.Collections.Generic.IEnumerable<Complex> Notch(int count, double w0) { for (int i = 0; i < count; i++) { yield return new Complex(0.0, w0); yield return new Complex(0.0, -w0); } }
    // LP→BP root split: each prototype root scales by BW/2 then splits to r′ ± √(r′²−ω₀²) straddling ±jω₀.
    private static Complex[] Pair(Complex[] roots, double halfBw, double w0Sq) {
        Complex[] split = new Complex[2 * roots.Length];
        for (int i = 0; i < roots.Length; i++) { Complex c = roots[i] * halfBw, d = Complex.Sqrt(c * c - w0Sq); split[2 * i] = c + d; split[2 * i + 1] = c - d; }
        return split;
    }
    // LP→BS root inversion: each prototype root maps to (BW/2)/r then splits to r′ ± √(r′²−ω₀²).
    private static Complex[] PairInv(Complex[] roots, double halfBw, double w0Sq) {
        Complex[] split = new Complex[2 * roots.Length];
        for (int i = 0; i < roots.Length; i++) { Complex c = halfBw / roots[i], d = Complex.Sqrt(c * c - w0Sq); split[2 * i] = c + d; split[2 * i + 1] = c - d; }
        return split;
    }
    // HP/BS band gain = Re(∏(−zₖ)/∏(−pₖ)) over the prototype roots (the s=∞ contribution the bilinear maps to z=−1).
    private static double BandGain(Complex[] zeros, Complex[] poles) {
        Complex zn = Complex.One; foreach (Complex z in zeros) { zn *= -z; }
        Complex pn = Complex.One; foreach (Complex p in poles) { pn *= -p; }
        return (zn / pn).Real;
    }
}

// --- [MODELS] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilterShape {
    private FilterShape() { }

    public sealed record Windowed(WindowKind Window) : FilterShape;
    public sealed record Equiripple(double RippleDb, double StopbandDb, FitBudget Budget) : FilterShape;
    public sealed record Butterworth : FilterShape;
    public sealed record Chebyshev1(double RippleDb) : FilterShape;
    public sealed record Chebyshev2(double StopbandDb) : FilterShape;
    public sealed record Elliptic(double RippleDb, double StopbandDb) : FilterShape;

    internal FilterDesign Design => Switch(
        windowed: static _ => FilterDesign.FirWindow,
        equiripple: static _ => FilterDesign.FirRemez,
        butterworth: static _ => FilterDesign.IirButterworth,
        chebyshev1: static _ => FilterDesign.IirChebyshev1,
        chebyshev2: static _ => FilterDesign.IirChebyshev2,
        elliptic: static _ => FilterDesign.IirElliptic);

    internal Fin<FilterContext> Project(FilterSpec spec) => Switch(
        state: spec,
        windowed: static (state, shape) => state.Context(shape.Window, rippleDb: 1.0, stopbandDb: 1.0, FitBudget.Canonical),
        equiripple: static (state, shape) => state.Context(WindowKind.Rectangular, shape.RippleDb, shape.StopbandDb, shape.Budget),
        butterworth: static (state, _) => state.Context(WindowKind.Rectangular, rippleDb: 1.0, stopbandDb: 1.0, FitBudget.Canonical),
        chebyshev1: static (state, shape) => state.Context(WindowKind.Rectangular, shape.RippleDb, stopbandDb: 1.0, FitBudget.Canonical),
        chebyshev2: static (state, shape) => state.Context(WindowKind.Rectangular, rippleDb: 1.0, shape.StopbandDb, FitBudget.Canonical),
        elliptic: static (state, shape) => state.Context(WindowKind.Rectangular, shape.RippleDb, shape.StopbandDb, FitBudget.Canonical));
}

public sealed record FilterSpec(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double SampleRate, FilterShape Shape) {
    public static readonly FilterSpec CanonicalLowPass = new(FilterBand.LowPass, Order: 4, Cutoff: 1000.0, UpperCutoff: 0.0, SampleRate: 48000.0, new FilterShape.Butterworth());

    public double Nyquist => SampleRate * 0.5;
    public double NormalizedCutoff => Cutoff / Nyquist;          // [0, 1) cutoff for the FIR sinc.
    public double NormalizedUpper => UpperCutoff / Nyquist;

    internal Fin<FilterContext> Admit() => Shape.Project(this);

    internal Fin<FilterContext> Context(WindowKind window, double rippleDb, double stopbandDb, FitBudget budget) =>
        Order < 1
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<filter-order:{Order}>"))
        : SampleRate <= 0.0 || !double.IsFinite(SampleRate)
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<filter-sample-rate:{SampleRate}>"))
        : rippleDb <= 0.0 || stopbandDb <= 0.0 || !double.IsFinite(rippleDb) || !double.IsFinite(stopbandDb)
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<filter-ripple:{rippleDb}/{stopbandDb}>"))
        : Cutoff <= 0.0 || !double.IsFinite(Cutoff) || NormalizedCutoff >= 1.0
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<filter-cutoff:{Cutoff}/{Nyquist}>"))
        : Band.Upper && (UpperCutoff <= Cutoff || !double.IsFinite(UpperCutoff) || NormalizedUpper >= 1.0)
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<filter-band-edges:{Cutoff}..{UpperCutoff}>"))
            : budget.Admit().Map(admitted => new FilterContext(Band, Order, Cutoff, UpperCutoff, rippleDb, stopbandDb, window, SampleRate, admitted));
}

internal sealed record FilterContext(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double RippleDb, double StopbandDb, WindowKind Window, double SampleRate, FitBudget Budget) {
    internal double Nyquist => SampleRate * 0.5;
    internal double NormalizedCutoff => Cutoff / Nyquist;
    internal double NormalizedUpper => UpperCutoff / Nyquist;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SignalPolicy {
    private SignalPolicy() { }

    public sealed record PerBin(double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Framed(WindowKind Window, int FrameSize, int HopSize, double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Wavelet(int Levels, WaveletFamily Family) : SignalPolicy;

    public static readonly SignalPolicy CanonicalFft   = new PerBin(SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalStft  = new Framed(WindowKind.Hann, FrameSize: 1024, HopSize: 512, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalWelch = new Framed(WindowKind.Hann, FrameSize: 256, HopSize: 128, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalDwt   = new Wavelet(Levels: 4, WaveletFamily.Db4);

    internal bool IsFramed => this is Framed;

    internal static Fin<SignalContext> AdmitPerBin(SpectralTransform transform, SignalPolicy policy, int _) =>
        policy is not PerBin perBin
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-policy:{transform}>"))
        : perBin.SampleRate <= 0.0 || !double.IsFinite(perBin.SampleRate)
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-sample-rate:{perBin.SampleRate}>"))
            : Fin.Succ(new SignalContext(transform, WindowKind.Rectangular, 0, 0, 0, perBin.SampleRate, perBin.Scaling, WaveletFamily.Haar));

    internal static Fin<SignalContext> AdmitFramed(SpectralTransform transform, SignalPolicy policy, int samples) =>
        policy is not Framed framed
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-policy:{transform}>"))
        : framed.SampleRate <= 0.0 || !double.IsFinite(framed.SampleRate)
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-sample-rate:{framed.SampleRate}>"))
        : framed.HopSize <= 0 || framed.FrameSize < framed.HopSize || framed.FrameSize > samples
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-frame:{framed.FrameSize}/{framed.HopSize}/{samples}>"))
        : FrameCells(samples, framed.FrameSize, framed.HopSize) > Array.MaxLength
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-frame-capacity:{framed.FrameSize}/{framed.HopSize}/{samples}>"))
        : transform == SpectralTransform.Stft && framed.HopSize >= framed.FrameSize
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<stft-overlap:{framed.FrameSize}/{framed.HopSize}>"))
            : Fin.Succ(new SignalContext(transform, framed.Window, framed.FrameSize, framed.HopSize, 0, framed.SampleRate, framed.Scaling, WaveletFamily.Haar));

    internal static Fin<SignalContext> AdmitWavelet(SpectralTransform transform, SignalPolicy policy, int _) =>
        policy is not Wavelet wavelet
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-policy:{transform}>"))
        : wavelet.Levels < 1
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-levels:{wavelet.Levels}>"))
            : Fin.Succ(new SignalContext(transform, WindowKind.Rectangular, 0, 0, wavelet.Levels, 0.0, FourierOptions.Default, wavelet.Family));

    private static long FrameCells(int samples, int frame, int hop) =>
        (1L + (samples + frame / 2L - 1L) / hop) * (frame / 2L + 1L);
}

internal sealed record SignalContext(SpectralTransform Transform, WindowKind Window, int FrameSize, int HopSize, int Levels, double SampleRate, FourierOptions Scaling, WaveletFamily Wavelet);

// `Samples` and `Scaling` preserve packed-rfft length and reciprocal-transform normalization.
public sealed record Spectrum(SpectralTransform Transform, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, int Length, int Samples, double BinHz, double SampleRate, FourierOptions Scaling, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Spectrogram {
    private Spectrogram() { }

    public sealed record Phasor(SpectralTransform Transform, int Frames, int Bins, int Samples, int FrameSize, int Hop, double BinHz, WindowKind Window, FourierOptions Scaling, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, Instant At) : Spectrogram;
    public sealed record Power(SpectralTransform Transform, int Frames, int Bins, int Hop, double BinHz, ReadOnlyMemory<double> Values, Instant At) : Spectrogram;

    public SpectralTransform Transform => Switch(
        phasor: static value => value.Transform,
        power: static value => value.Transform);
}

// Extents records the approximation length consumed at each cascade level (head = original signal), because the
// stride-2 floors are not recoverable from coefficient lengths alone and the synthesis bank trims against them.
public sealed record WaveletDecomposition(WaveletFamily Family, int Levels, ReadOnlyMemory<double> Approximation, Seq<ReadOnlyMemory<double>> Details, ImmutableArray<int> Extents, Instant At);

// Two-channel Welch cross-spectral estimate: auto-spectra, the complex cross-spectrum as magnitude/phase, and the
// magnitude-squared coherence γ² = |Sxy|²/(Sxx·Syy) — the measured-mode identification ingress.
public sealed record CrossSpectrum(int Bins, double BinHz, ReadOnlyMemory<double> AutoX, ReadOnlyMemory<double> AutoY, ReadOnlyMemory<double> CrossMagnitude, ReadOnlyMemory<double> CrossPhase, ReadOnlyMemory<double> Coherence, Instant At) {
    // H1 FRF estimate |Sxy|/Sxx with the cross phase — the transfer function modal extraction reads; derived per read, never stored beside its sources.
    public (ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase) Transfer() {
        double[] magnitude = new double[Bins];
        for (int k = 0; k < Bins; k++) { magnitude[k] = CrossMagnitude.Span[k] / Math.Max(1e-300, AutoX.Span[k]); }
        return (magnitude, CrossPhase);
    }
}

// One operational-deflection mode measured from ambient records: peak frequency, half-power damping where the peak
// bandwidth resolves, and the dominant singular vector as per-channel magnitude/phase — the FE-updating ingress.
public sealed record MeasuredMode(double FrequencyHz, Option<double> DampingRatio, ReadOnlyMemory<double> ShapeMagnitude, ReadOnlyMemory<double> ShapePhase, double Singular);

public sealed record ModalEstimate(int Channels, int Bins, double BinHz, ReadOnlyMemory<double> SingularSpectrum, Seq<MeasuredMode> Modes, Instant At);

public sealed record FilterResponse(int Bins, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, ReadOnlyMemory<double> GroupDelay);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralOutput {
    private SpectralOutput() { }

    public sealed record Bins(Spectrum Spectrum) : SpectralOutput;
    public sealed record Frames(Spectrogram Spectrogram) : SpectralOutput;
    public sealed record Bands(WaveletDecomposition Decomposition) : SpectralOutput;
}

public sealed record FilterCoefficients(FilterDesign Design, ReadOnlyMemory<double> B, ReadOnlyMemory<double> A) {
    // Tap count where the O(n·taps) Conv1D GEMM crosses the O(n·log taps) FFT overlap-add — one named policy value, never an inline literal.
    private const int OverlapAddCrossover = 64;

    // `Design.Recursive` is the sole FIR/IIR discriminator; coefficient-array lengths never recover design identity.
    public bool Fir => !Design.Recursive;

    public Fin<FilterCoefficients> Admit() =>
        B.Length == 0 || A.Length == 0
            ? Fin.Fail<FilterCoefficients>(ComputeFault.Create($"<filter-coefficients-empty:{B.Length}/{A.Length}>"))
        : !TensorPrimitives.IsFiniteAll<double>(B.Span) || !TensorPrimitives.IsFiniteAll<double>(A.Span)
            ? Fin.Fail<FilterCoefficients>(ComputeFault.Create("<filter-coefficients-nonfinite>"))
        : B.Length > int.MaxValue / 4
            ? Fin.Fail<FilterCoefficients>(ComputeFault.Create($"<filter-coefficients-capacity:{B.Length}>"))
        : A.Span[0] == 0.0
            ? Fin.Fail<FilterCoefficients>(ComputeFault.Create("<filter-a0-zero>"))
            : Fin.Succ(this);

    // Short FIR rides the feed-forward Conv1D GEMM, long FIR the FFT overlap-add product, IIR the direct-form-II-transposed recurrence with the feedback convolution cannot express.
    // Taps are REVERSED for the Conv1D feed because the lowering kernel cross-correlates while a filter convolves.
    public IO<Fin<float[]>> Apply(ReadOnlyMemory<float> signal) =>
        Admit().Match(
            Succ: _ => ApplyAdmitted(signal),
            Fail: static error => IO.pure(Fin.Fail<float[]>(error)));

    private IO<Fin<float[]>> ApplyAdmitted(ReadOnlyMemory<float> signal) =>
        signal.Length == 0
            ? IO.pure(Fin.Fail<float[]>(ComputeFault.Create("<signal-empty>")))
        : !TensorPrimitives.IsFiniteAll<float>(signal.Span)
            ? IO.pure(Fin.Fail<float[]>(ComputeFault.Create("<signal-nonfinite>")))
        : Fir
            ? B.Length > OverlapAddCrossover
                ? IO.pure(OverlapAdd(signal.Span))
                : KernelLowering.Lower(TensorOpFamily.Conv1D,
                        Matrix<double>.Build.Dense(1, signal.Length, (_, c) => signal.Span[c]),
                        Matrix<double>.Build.Dense(B.Length, 1, (r, _) => B.Span[B.Length - 1 - r]),
                        new ConvWindow([B.Length], [1], [B.Length / 2], [1], 1, 1, [signal.Length]), new ShardPlan.Single())
                    .Map(result => result.Map(static y => y.ToColumnMajorArray().Select(static value => (float)value).ToArray()))
            : IO.pure(Finite(DirectFormII(signal.Span)));

    // Frequency response over a normalized [0, π] grid: H(e^{jω}) = B(e^{−jω})/A(e^{−jω}) by one complex Horner
    // sweep per bin — the verification surface every designed filter reads before application.
    public Fin<FilterResponse> Response(int bins) =>
        bins < 2 || bins > Array.MaxLength / 3
            ? Fin.Fail<FilterResponse>(ComputeFault.Create($"<filter-response-bins:{bins}>"))
            : Admit().Bind(_ => Sweep(bins));

    private Fin<FilterResponse> Sweep(int bins) {
        double[] magnitude = new double[bins];
        double[] phase = new double[bins];
        double[] groupDelay = new double[bins];
        double step = Math.PI / (bins - 1);
        for (int r = 0; r < bins; r++) {
            Complex z = Complex.Exp(new Complex(0.0, -step * r));
            Complex h = Horner(B.Span, z) / Horner(A.Span, z);
            magnitude[r] = h.Magnitude;
            phase[r] = h.Phase;
            if (r > 0) {
                double delta = phase[r] - phase[r - 1];
                phase[r] -= Math.Round(delta / (2.0 * Math.PI)) * 2.0 * Math.PI;
            }
        }
        groupDelay[0] = -(phase[1] - phase[0]) / step;
        for (int r = 1; r < bins - 1; r++) { groupDelay[r] = -(phase[r + 1] - phase[r - 1]) / (2.0 * step); }
        groupDelay[^1] = -(phase[^1] - phase[^2]) / step;
        return TensorPrimitives.IsFiniteAll<double>(magnitude) && TensorPrimitives.IsFiniteAll<double>(phase) && TensorPrimitives.IsFiniteAll<double>(groupDelay)
            ? Fin.Succ(new FilterResponse(bins, magnitude, phase, groupDelay))
            : Fin.Fail<FilterResponse>(ComputeFault.Create("<filter-response-singular>"));

        static Complex Horner(ReadOnlySpan<double> coefficients, Complex z) {
            Complex acc = Complex.Zero;
            for (int i = coefficients.Length - 1; i >= 0; i--) { acc = acc * z + coefficients[i]; }
            return acc;
        }
    }

    // Direct-Form-II Transposed, a₀-normalized once: one state vector w of order max(|b|,|a|)−1 threads the feedback the feed-forward Conv1D GEMM cannot express, never two separate input/output history buffers.
    private float[] DirectFormII(ReadOnlySpan<float> signal) {
        ReadOnlySpan<double> b = B.Span, a = A.Span;
        double a0 = a[0];
        int order = Math.Max(b.Length, a.Length) - 1;
        Span<double> w = order > 0 ? new double[order] : [];
        float[] y = new float[signal.Length];
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

    // FFT overlap-add (long FIR): the dispatch-lane ComplexZip(Multiply) spectral product per block, accumulating the (B.Length−1) tail across blocks — the mechanism the Apply fold selects by tap length.
    private Fin<float[]> OverlapAdd(ReadOnlySpan<float> signal) {
        int taps = B.Length, block = 4 * taps, nfft = (int)BitOperations.RoundUpToPowerOf2((uint)(block + taps - 1));
        Complex[] filterSpec = new Complex[nfft];
        for (int k = 0; k < taps; k++) { filterSpec[k] = new Complex(B.Span[k], 0.0); }
        Fourier.Forward(filterSpec, FourierOptions.NoScaling);
        using MemoryOwner<float> output = MemoryOwner<float>.Allocate(signal.Length, AllocationMode.Clear);
        Span<float> y = output.Span;
        Complex[] frame = new Complex[nfft];
        Complex[] product = new Complex[nfft];
        for (int start = 0; start < signal.Length; start += block) {
            int len = Math.Min(block, signal.Length - start);
            Array.Clear(frame);
            for (int i = 0; i < len; i++) { frame[i] = new Complex(signal[start + i], 0.0); }
            Fourier.Forward(frame, FourierOptions.NoScaling);
            Fin<Unit> zip = TensorOps.ComplexZip(TensorOpFamily.Multiply, frame, filterSpec, product);
            if (zip.IsFail) { return zip.Map(static _ => Array.Empty<float>()); }   // the zip's own fault rides out; no generic re-wrap erases its diagnostic.
            Fourier.Inverse(product, FourierOptions.NoScaling);
            // Accumulate at start + i − taps/2 so the long-tap path lands the SAME centered alignment the short-tap
            // Conv1D takes with padding taps/2; a causal write would shift the output by the group delay per route.
            for (int i = 0; i < len + taps - 1; i++) {
                int at = start + i - taps / 2;
                if (0 <= at && at < signal.Length) { y[at] += (float)(product[i].Real / nfft); }
            }
        }
        return Finite(y.ToArray());
    }

    // Zero-phase `filtfilt` uses a `3·order` odd reflection around the forward-backward cascade, cancelling group delay while suppressing endpoint transients.
    public Fin<float[]> ZeroPhase(ReadOnlyMemory<float> signal) =>
        Admit().Bind(_ => ZeroPhaseAdmitted(signal));

    private Fin<float[]> ZeroPhaseAdmitted(ReadOnlyMemory<float> signal) {
        if (signal.Length == 0) { return Fin.Fail<float[]>(ComputeFault.Create("<signal-empty>")); }
        if (!TensorPrimitives.IsFiniteAll<float>(signal.Span)) { return Fin.Fail<float[]>(ComputeFault.Create("<signal-nonfinite>")); }
        ReadOnlySpan<float> x = signal.Span;
        int pad = Math.Min(3 * (Math.Max(B.Length, A.Length) - 1), x.Length - 1);
        float[] padded = new float[x.Length + 2 * pad];
        for (int i = 0; i < pad; i++) { padded[i] = 2f * x[0] - x[pad - i]; padded[^(i + 1)] = 2f * x[^1] - x[^(pad - i + 1)]; }   // odd reflection about each endpoint.
        x.CopyTo(padded.AsSpan(pad));
        float[] forward = DirectFormII(padded);
        Array.Reverse(forward);
        float[] backward = DirectFormII(forward);
        Array.Reverse(backward);
        return Finite(backward[pad..^pad]);
    }

    private static Fin<float[]> Finite(float[] samples) =>
        TensorPrimitives.IsFiniteAll<float>(samples)
            ? Fin.Succ(samples)
            : Fin.Fail<float[]>(ComputeFault.Create("<filter-output-nonfinite>"));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Transform {
    // One prologue admits finite samples and composes the row's policy-case gate; row-owned `Windowed` controls frame/hop admission.
    public static IO<Fin<SpectralOutput>> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, IClock clock) =>
        signal.Length == 0
            ? IO.pure(Fin.Fail<SpectralOutput>(ComputeFault.Create("<signal-empty>")))
        : !TensorPrimitives.IsFiniteAll<float>(signal.Span)
            ? IO.pure(Fin.Fail<SpectralOutput>(ComputeFault.Create("<signal-nonfinite>")))
            : transform.Admit(policy, signal.Length).Match(
                Succ: context => transform.Run(signal, context, clock.GetCurrentInstant()),
                Fail: static error => IO.pure(Fin.Fail<SpectralOutput>(error)));

    // Inversion projects the owning row from forward evidence and dispatches its paired inverse or typed destruction fault.
    public static IO<Fin<ReadOnlyMemory<float>>> Invert(SpectralOutput output) =>
        output.Switch(
            bins: static b => b.Spectrum.Transform,
            frames: static f => f.Spectrogram.Transform,
            bands: static _ => SpectralTransform.Dwt).Invert(output);

    // Admit the spec then dispatch to the row's Run — one delegate call, no design-identity ternary, so a new FilterDesign row breaks at compile time (unbound Run) rather than routing silently.
    public static Fin<FilterCoefficients> Design(FilterSpec spec) =>
        spec.Shape.Design.Run(spec).Bind(static coefficients => coefficients.Admit());

    // Synchronous Welch segments accumulate `Sxx`, `Syy`, and complex `Sxy`; per-bin `γ² = |Sxy|²/(Sxx·Syy)` feeds measured-mode extraction.
    public static Fin<CrossSpectrum> Coherence(ReadOnlyMemory<float> x, ReadOnlyMemory<float> y, SignalPolicy policy, IClock clock) =>
        x.Length != y.Length
            ? Fin.Fail<CrossSpectrum>(ComputeFault.Create($"<coherence-length-miss:{x.Length}!={y.Length}>"))
        : x.Length == 0 || !TensorPrimitives.IsFiniteAll<float>(x.Span) || !TensorPrimitives.IsFiniteAll<float>(y.Span)
            ? Fin.Fail<CrossSpectrum>(ComputeFault.Create("<coherence-admission>"))
            : SpectralTransform.WelchPsd.Admit(policy, x.Length).Bind(context =>
                1 + (x.Length - context.FrameSize) / context.HopSize < 2
                    ? Fin.Fail<CrossSpectrum>(ComputeFault.Create($"<coherence-segments:{x.Length}/{context.FrameSize}/{context.HopSize}>"))   // single-segment coherence is identically 1 — evidence-free.
                    : CoherenceAdmitted(x, y, context, clock.GetCurrentInstant()));

    private static Fin<CrossSpectrum> CoherenceAdmitted(ReadOnlyMemory<float> x, ReadOnlyMemory<float> y, SignalContext policy, Instant at) {
        int frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        int segments = 1 + (x.Length - frame) / hop;
        double[] taper = policy.Window.Taper(frame, periodic: false);
        (double[] sxx, double[] syy, double[] sxyRe, double[] sxyIm) = (new double[bins], new double[bins], new double[bins], new double[bins]);
        (double[] xr, double[] xi, double[] yr, double[] yi) = (new double[frame], new double[frame], new double[frame], new double[frame]);
        for (int s = 0; s < segments; s++) {
            int offset = s * hop;
            for (int i = 0; i < frame; i++) { xr[i] = x.Span[offset + i] * taper[i]; yr[i] = y.Span[offset + i] * taper[i]; }
            Array.Clear(xi);
            Array.Clear(yi);
            Fourier.Forward(xr, xi, FourierOptions.NoScaling);
            Fourier.Forward(yr, yi, FourierOptions.NoScaling);
            for (int k = 0; k < bins; k++) {
                sxx[k] += xr[k] * xr[k] + xi[k] * xi[k];
                syy[k] += yr[k] * yr[k] + yi[k] * yi[k];
                sxyRe[k] += xr[k] * yr[k] + xi[k] * yi[k];   // X·conj(Y) real part.
                sxyIm[k] += xi[k] * yr[k] - xr[k] * yi[k];   // X·conj(Y) imaginary part.
            }
        }
        (double[] crossMag, double[] crossPhase, double[] coherence) = (new double[bins], new double[bins], new double[bins]);
        for (int k = 0; k < bins; k++) {
            crossMag[k] = double.Hypot(sxyRe[k], sxyIm[k]) / segments;
            crossPhase[k] = Math.Atan2(sxyIm[k], sxyRe[k]);
            coherence[k] = Math.Clamp((sxyRe[k] * sxyRe[k] + sxyIm[k] * sxyIm[k]) / Math.Max(1e-300, sxx[k] * syy[k]), 0.0, 1.0);
        }
        return Fin.Succ(new CrossSpectrum(bins, BinHz(frame, policy.SampleRate), [.. sxx.Select(value => value / segments)], [.. syy.Select(value => value / segments)], crossMag, crossPhase, coherence, at));
    }

    // Frequency-domain decomposition over N synchronous ambient channels: the same Welch fold accumulates the per-bin
    // Hermitian cross-PSD matrix G(f); a PSD matrix's SVD is its EVD, so the dominant singular pair per bin resolves by
    // Hermitian power iteration; peaks of the first singular-value spectrum are the measured natural frequencies and the
    // paired eigenvector the operational mode shape — the OMA route from records to as-built modes.
    public static Fin<ModalEstimate> Modal(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock) =>
        channels.Count < 2
            ? Fin.Fail<ModalEstimate>(ComputeFault.Create($"<modal-channels:{channels.Count}<2>"))
        : channels[0].Length == 0 || channels.Exists(c => c.Length != channels[0].Length) || channels.Exists(c => !TensorPrimitives.IsFiniteAll<float>(c.Span))
            ? Fin.Fail<ModalEstimate>(ComputeFault.Create("<modal-admission>"))
            : SpectralTransform.WelchPsd.Admit(policy, channels[0].Length).Bind(context =>
                1 + (channels[0].Length - context.FrameSize) / context.HopSize < 2
                    ? Fin.Fail<ModalEstimate>(ComputeFault.Create($"<modal-segments:{channels[0].Length}/{context.FrameSize}/{context.HopSize}>"))
                    : Fin.Succ(ModalAdmitted(channels, context, clock.GetCurrentInstant())));

    private static ModalEstimate ModalAdmitted(Seq<ReadOnlyMemory<float>> channels, SignalContext policy, Instant at) {
        int n = channels.Count, frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        int segments = 1 + (channels[0].Length - frame) / hop;
        double[] taper = policy.Window.Taper(frame, periodic: false);
        (double[] gre, double[] gim) = (new double[bins * n * n], new double[bins * n * n]);
        (double[][] xr, double[][] xi) = ([.. Enumerable.Range(0, n).Select(_ => new double[frame])], [.. Enumerable.Range(0, n).Select(_ => new double[frame])]);
        for (int s = 0; s < segments; s++) {
            int offset = s * hop;
            for (int c = 0; c < n; c++) {
                for (int i = 0; i < frame; i++) { xr[c][i] = channels[c].Span[offset + i] * taper[i]; }
                Array.Clear(xi[c]);
                Fourier.Forward(xr[c], xi[c], FourierOptions.NoScaling);
            }
            for (int k = 0; k < bins; k++) {
                for (int i = 0; i < n; i++) {
                    for (int j = 0; j < n; j++) {                                        // Gij(k) += Xi(k)·conj(Xj(k)) — Hermitian by construction.
                        int cell = (k * n + i) * n + j;
                        gre[cell] += xr[i][k] * xr[j][k] + xi[i][k] * xi[j][k];
                        gim[cell] += xi[i][k] * xr[j][k] - xr[i][k] * xi[j][k];
                    }
                }
            }
        }
        double[] s1 = new double[bins];
        (double[] vre, double[] vim) = (new double[bins * n], new double[bins * n]);
        for (int k = 0; k < bins; k++) { s1[k] = Dominant(gre, gim, k, n, vre.AsSpan(k * n, n), vim.AsSpan(k * n, n)) / segments; }
        double floor = ModalPeakFloor * Median(s1);
        Seq<MeasuredMode> modes = Seq<MeasuredMode>();
        double binHz = BinHz(frame, policy.SampleRate);
        for (int k = 1; k < bins - 1; k++) {
            if (s1[k] <= floor || s1[k] <= s1[k - 1] || s1[k] < s1[k + 1]) { continue; }
            (double[] magnitude, double[] phase) = (new double[n], new double[n]);
            TensorPrimitives.Hypot(vre.AsSpan(k * n, n), vim.AsSpan(k * n, n), magnitude);
            TensorPrimitives.Atan2(vim.AsSpan(k * n, n), vre.AsSpan(k * n, n), phase);
            modes = modes.Add(new MeasuredMode(k * binHz, HalfPower(s1, k, binHz), magnitude, phase, s1[k]));
        }
        return new ModalEstimate(n, bins, binHz, s1, modes, at);
    }

    // Hermitian power iteration WITH a convergence witness: iterate until the Rayleigh estimate settles
    // (|λ − λ_prev| ≤ 1e-10·λ) under a hard cap, and a bin that never converges returns 0 so an unwitnessed pair
    // can never become a picked peak — a fixed-count sweep treating clustered modes as settled evidence is the
    // deleted form. Stays page-local because the Tensor/blas dense owner is real-typed and this pair is complex
    // Hermitian; equiripple FIR still crosses to the dense owner.
    private static double Dominant(double[] gre, double[] gim, int bin, int n, Span<double> vre, Span<double> vim) {
        int seed = 0;
        for (int i = 1; i < n; i++) { if (gre[(bin * n + i) * n + i] > gre[(bin * n + seed) * n + seed]) { seed = i; } }
        vre.Clear(); vim.Clear(); vre[seed] = 1.0;
        Span<double> wre = stackalloc double[n], wim = stackalloc double[n];
        double lambda = 0.0;
        for (int iteration = 0; iteration < 512; iteration++) {
            for (int i = 0; i < n; i++) {
                (wre[i], wim[i]) = (0.0, 0.0);
                for (int j = 0; j < n; j++) {
                    int cell = (bin * n + i) * n + j;
                    wre[i] += gre[cell] * vre[j] - gim[cell] * vim[j];
                    wim[i] += gre[cell] * vim[j] + gim[cell] * vre[j];
                }
            }
            double norm = Math.Sqrt(TensorPrimitives.SumOfSquares<double>(wre) + TensorPrimitives.SumOfSquares<double>(wim));
            if (norm < 1e-300) { return 0.0; }
            bool settled = Math.Abs(norm - lambda) <= 1e-10 * norm;
            lambda = norm;
            for (int i = 0; i < n; i++) { vre[i] = wre[i] / norm; vim[i] = wim[i] / norm; }
            if (settled) { return lambda; }
        }
        return 0.0;
    }

    // Half-power (−3 dB) bandwidth damping ζ ≈ Δf/(2·f_peak); None when the band never resolves inside the spectrum.
    private static Option<double> HalfPower(double[] s1, int peak, double binHz) {
        double half = s1[peak] / 2.0;
        int lo = peak, hi = peak;
        while (lo > 0 && s1[lo] > half) { lo--; }
        while (hi < s1.Length - 1 && s1[hi] > half) { hi++; }
        return s1[lo] <= half && s1[hi] <= half && peak > 0
            ? Some((hi - lo) * binHz / (2.0 * peak * binHz))
            : None;
    }

    private static double Median(double[] values) {
        double[] sorted = [.. values.Order()];
        return sorted.Length == 0 ? 0.0 : sorted[sorted.Length / 2];
    }

    private const double ModalPeakFloor = 8.0;

    // --- [SPECTRAL] -- forward kernels each SpectralTransform row binds.

    internal static IO<Fin<SpectralOutput>> PerBin(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) {
        int n = signal.Length, bins = n;                                                   // full complex transform yields n bins; the Hermitian half-spectrum is the rfft RealForward owner.
        double[] real = new double[n];
        double[] imaginary = new double[n];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, real);
        double[] magnitude = new double[bins];
        double[] phase = new double[bins];
        Fourier.Forward(real, imaginary, policy.Scaling);
        TensorPrimitives.Hypot(real.AsSpan(0, bins), imaginary.AsSpan(0, bins), magnitude); // vectorized overflow-safe magnitude, never a per-element x.Magnitude loop.
        TensorPrimitives.Atan2(imaginary.AsSpan(0, bins), real.AsSpan(0, bins), phase);
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, magnitude, phase, bins, n, BinHz(n, policy.SampleRate), policy.SampleRate, policy.Scaling, at))));
    }

    // rfft over the packed N+2 half-spectrum: ForwardReal packs bins 0..N/2 as interleaved (re, im) pairs.
    internal static IO<Fin<SpectralOutput>> RealForward(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) {
        int n = signal.Length, bins = n / 2 + 1;               // ForwardReal packs N+2 (even N) / N+1 (odd N); no even-length truncation dropping the last sample.
        double[] data = new double[n + 2];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, data.AsSpan(0, n));
        Fourier.ForwardReal(data, n, policy.Scaling);
        double[] real = new double[bins];
        double[] imaginary = new double[bins];
        double[] magnitude = new double[bins];
        double[] phase = new double[bins];
        for (int k = 0; k < bins; k++) { real[k] = data[2 * k]; imaginary[k] = data[2 * k + 1]; }
        TensorPrimitives.Hypot(real, imaginary, magnitude);
        TensorPrimitives.Atan2(imaginary, real, phase);
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, magnitude, phase, bins, n, BinHz(n, policy.SampleRate), policy.SampleRate, policy.Scaling, at))));
    }

    internal static IO<Fin<SpectralOutput>> ShortTime(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) {
        int frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        int left = frame / 2, frames = (int)(1L + (signal.Length + (long)left - 1L) / hop);
        double[] taper = policy.Window.Taper(frame, periodic: true);
        double[] magnitude = new double[frames * bins];
        double[] phase = new double[frames * bins];
        double[] real = new double[frame];
        double[] imaginary = new double[frame];
        for (int f = 0; f < frames; f++) {
            int offset = f * hop - left;
            for (int i = 0; i < frame; i++) {
                int sample = offset + i;
                real[i] = 0 <= sample && sample < signal.Length ? signal.Span[sample] * taper[i] : 0.0;
            }
            Array.Clear(imaginary);
            Fourier.Forward(real, imaginary, policy.Scaling);
            Span<double> magnitudeRow = magnitude.AsSpan(f * bins, bins);
            Span<double> phaseRow = phase.AsSpan(f * bins, bins);
            TensorPrimitives.Hypot(real.AsSpan(0, bins), imaginary.AsSpan(0, bins), magnitudeRow);
            TensorPrimitives.Atan2(imaginary.AsSpan(0, bins), real.AsSpan(0, bins), phaseRow);
        }
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Frames(new Spectrogram.Phasor(policy.Transform, frames, bins, signal.Length, frame, hop, BinHz(frame, policy.SampleRate), policy.Window, policy.Scaling, magnitude, phase, at))));
    }

    internal static IO<Fin<SpectralOutput>> PowerSpectrogram(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) =>
        ShortTime(signal, policy, at).Map(static result => result.Bind(output =>
            output is SpectralOutput.Frames { Spectrogram: Spectrogram.Phasor frames }
                ? Fin.Succ<SpectralOutput>(new SpectralOutput.Frames(new Spectrogram.Power(
                    SpectralTransform.Spectrogram,
                    frames.Frames,
                    frames.Bins,
                    frames.Hop,
                    frames.BinHz,
                    Squared(frames.Magnitude),
                    frames.At)))
                : Fin.Fail<SpectralOutput>(ComputeFault.Create("<spectrogram-carrier-miss>"))));

    private static ReadOnlyMemory<double> Squared(ReadOnlyMemory<double> magnitude) {
        double[] power = magnitude.ToArray();
        TensorPrimitives.Multiply(power, power, power);
        return power;
    }

    // Welch PSD: average the windowed periodograms |Xf|²/(fs·U) of the overlapping segments (U the window power normalizer); the averaged density is a Spectrum whose Magnitude is the one-sided PSD.
    internal static IO<Fin<SpectralOutput>> Welch(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) {
        int frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        int segments = 1 + (signal.Length - frame) / hop;
        double[] taper = policy.Window.Taper(frame, periodic: false);
        double u = 0.0;
        foreach (double t in taper) { u += t * t; }
        double norm = 1.0 / (policy.SampleRate * u * segments);
        double[] psd = new double[bins];
        double[] real = new double[frame];
        double[] imaginary = new double[frame];
        for (int s = 0; s < segments; s++) {
            int offset = s * hop;
            for (int i = 0; i < frame; i++) { real[i] = signal.Span[offset + i] * taper[i]; }
            Array.Clear(imaginary);
            Fourier.Forward(real, imaginary, FourierOptions.NoScaling);
            for (int k = 0; k < bins; k++) {
                double mm = real[k] * real[k] + imaginary[k] * imaginary[k];
                psd[k] += (k == 0 || ((frame & 1) == 0 && k == bins - 1) ? 1.0 : 2.0) * mm * norm;   // DC (and even-frame Nyquist) are unique; every other bin folds its conjugate twin.
            }
        }
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, psd, ReadOnlyMemory<double>.Empty, bins, frame, BinHz(frame, policy.SampleRate), policy.SampleRate, FourierOptions.NoScaling, at))));
    }

    // DWT cascade over the stride-2 Conv1D lowering, halving the length per level — the same convolution lowering the FIR application rides, never a bespoke filter bank.
    internal static IO<Fin<SpectralOutput>> Wavelet(ReadOnlyMemory<float> signal, SignalContext policy, Instant at) {
        WaveletFamily family = policy.Wavelet;                  // policy-selected across haar/db2/db4/sym4/coif1.
        int levels = policy.Levels;
        double[] seed = new double[signal.Length];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, seed);
        double[] h = family.LowPass.ToArray(), g = family.HighPass();
        return Cascade((Approx: seed, Details: Seq<ReadOnlyMemory<double>>(), Extents: Seq<int>()), 0)
            .Map(result => result.Map(state => (SpectralOutput)new SpectralOutput.Bands(new WaveletDecomposition(family, state.Details.Count, state.Approx, state.Details, [.. state.Extents], at))));

        // Mallat cascade as an immutable Fin-threaded fold (coarsest detail at the head, per-level parent extents recorded for the synthesis trim); the cascade stops when the approximation can no longer fill the QMF support, so realized `Levels` is whatever depth the signal admits.
        IO<Fin<(double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents)>> Cascade((double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents) state, int level) =>
            level >= levels || state.Approx.Length < h.Length
                ? IO.pure(Fin.Succ(state))
                : Convolve2(state.Approx, h, g).Bind(result => result.Match(
                    Succ: band => Cascade((band.Low, ((ReadOnlyMemory<double>)band.High).Cons(state.Details), state.Extents.Add(state.Approx.Length)), level + 1),
                    Fail: static error => IO.pure(Fin.Fail<(double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents)>(error))));
    }

    // One stride-2 Conv1D per QMF tap set: the approximation lowers through the factor lane against the reversed analysis filters (true convolution from cross-correlation), downsampled by 2.
    private static IO<Fin<(double[] Low, double[] High)>> Convolve2(double[] x, double[] h, double[] g) =>
        Downsample(x, h).Bind(low => low.Match(
            Succ: admitted => Downsample(x, g).Map(high => high.Map(detail => (admitted, detail))),
            Fail: static error => IO.pure(Fin.Fail<(double[] Low, double[] High)>(error))));

    private static IO<Fin<double[]>> Downsample(double[] x, double[] filter) =>
        KernelLowering.Lower(TensorOpFamily.Conv1D,
                Matrix<double>.Build.Dense(1, x.Length, (_, c) => x[c]),
                Matrix<double>.Build.Dense(filter.Length, 1, (r, _) => filter[filter.Length - 1 - r]),
                new ConvWindow([filter.Length], [2], [filter.Length / 2], [1], 1, 1, [x.Length]), new ShardPlan.Single())
            .Map(result => result.Map(static y => y.ToColumnMajorArray()));

    // --- [INVERSE] -- row-owned inverse kernels; each consumes the forward carrier, never raw samples.

    // fft inverse: complex bins reconstruct from magnitude·e^{iφ} and ride Fourier.Inverse under the RECORDED
    // scaling — Default/Asymmetric compose to identity, NoScaling divides the N the round trip carries.
    internal static IO<Fin<ReadOnlyMemory<float>>> InvertBins(SpectralOutput output) {
        if (output is not SpectralOutput.Bins { Spectrum: Spectrum spectrum }) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<invert-carrier-miss>"))); }
        int n = spectrum.Samples;
        double[] real = new double[n];
        double[] imaginary = new double[n];
        for (int k = 0; k < n; k++) {
            real[k] = spectrum.Magnitude.Span[k] * Math.Cos(spectrum.Phase.Span[k]);
            imaginary[k] = spectrum.Magnitude.Span[k] * Math.Sin(spectrum.Phase.Span[k]);
        }
        Fourier.Inverse(real, imaginary, spectrum.Scaling);
        double scale = spectrum.Scaling == FourierOptions.NoScaling ? 1.0 / n : 1.0;
        float[] samples = new float[n];
        for (int i = 0; i < n; i++) { samples[i] = (float)(real[i] * scale); }
        return IO.pure(Fin.Succ<ReadOnlyMemory<float>>(samples));
    }

    // rfft inverse: the packed N+2 half-spectrum rebuilds from magnitude/phase and rides Fourier.InverseReal —
    // Spectrum.Samples recovers the original length the bin count alone cannot (even/odd packing).
    internal static IO<Fin<ReadOnlyMemory<float>>> InvertPacked(SpectralOutput output) {
        if (output is not SpectralOutput.Bins { Spectrum: Spectrum spectrum }) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<invert-carrier-miss>"))); }
        int n = spectrum.Samples, bins = spectrum.Length;
        double[] data = new double[n + 2];
        for (int k = 0; k < bins; k++) {
            data[2 * k] = spectrum.Magnitude.Span[k] * Math.Cos(spectrum.Phase.Span[k]);
            data[2 * k + 1] = spectrum.Magnitude.Span[k] * Math.Sin(spectrum.Phase.Span[k]);
        }
        Fourier.InverseReal(data, n, spectrum.Scaling);
        float[] samples = new float[n];
        for (int i = 0; i < n; i++) { samples[i] = (float)data[i]; }
        return IO.pure(Fin.Succ<ReadOnlyMemory<float>>(samples));
    }

    internal static IO<Fin<ReadOnlyMemory<float>>> InvertFrames(SpectralOutput output) {
        if (output is not SpectralOutput.Frames { Spectrogram: Spectrogram.Phasor frames }) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<invert-carrier-miss>"))); }
        double[] taper = frames.Window.Taper(frames.FrameSize, periodic: true);
        double[] sum = new double[frames.Samples];
        double[] weight = new double[frames.Samples];
        double[] real = new double[frames.FrameSize];
        double[] imaginary = new double[frames.FrameSize];
        int left = frames.FrameSize / 2;
        for (int f = 0; f < frames.Frames; f++) {
            Array.Clear(real);
            Array.Clear(imaginary);
            int row = f * frames.Bins;
            for (int k = 0; k < frames.Bins; k++) {
                double magnitude = frames.Magnitude.Span[row + k];
                double phase = frames.Phase.Span[row + k];
                real[k] = magnitude * Math.Cos(phase);
                imaginary[k] = magnitude * Math.Sin(phase);
            }
            int mirrored = (frames.FrameSize & 1) == 0 ? frames.Bins - 1 : frames.Bins;
            for (int k = 1; k < mirrored; k++) {
                real[frames.FrameSize - k] = real[k];
                imaginary[frames.FrameSize - k] = -imaginary[k];
            }
            Fourier.Inverse(real, imaginary, frames.Scaling);
            double scale = frames.Scaling == FourierOptions.NoScaling ? 1.0 / frames.FrameSize : 1.0;
            int offset = f * frames.Hop - left;
            for (int i = 0; i < frames.FrameSize; i++) {
                int sample = offset + i;
                if (0 <= sample && sample < frames.Samples) {
                    double window = taper[i];
                    sum[sample] += real[i] * scale * window;
                    weight[sample] += window * window;
                }
            }
        }
        if (weight.Any(static value => value <= 1e-24)) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<stft-window-coverage>"))); }
        float[] samples = new float[frames.Samples];
        for (int i = 0; i < samples.Length; i++) { samples[i] = (float)(sum[i] / weight[i]); }
        return IO.pure(Fin.Succ<ReadOnlyMemory<float>>(samples));
    }

    // Rows whose forward destroyed the inverse evidence answer a typed fault naming the destruction — magnitude-only
    // spectrograms and segment-averaged periodograms — never a fabricated reconstruction.
    internal static IO<Fin<ReadOnlyMemory<float>>> NonInvertible(SpectralOutput output, string evidence) =>
        IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create($"<invert-destroyed:{evidence}>")));

    // Inverse DWT feeds analysis filters unreversed to cross-correlating `Conv1D`, yielding the time-reversed synthesis convolution.
    // Each level zero-stuffs approximation/detail, convolves at stride one, sums, and trims to its recorded parent extent.
    internal static IO<Fin<ReadOnlyMemory<float>>> Synthesize(SpectralOutput output) {
        if (output is not SpectralOutput.Bands { Decomposition: WaveletDecomposition w }) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<invert-carrier-miss>"))); }
        double[] h = w.Family.LowPass.ToArray(), g = w.Family.HighPass();
        Seq<(ReadOnlyMemory<double> Detail, int Extent)> steps = toSeq(w.Details.Zip(toSeq(w.Extents.Reverse()), static (d, e) => (d, e)));
        return steps.Fold(
                IO.pure(Fin.Succ(w.Approximation.ToArray())),
                (effect, step) => effect.Bind(result => result.Match(
                    Succ: approximation => Reconstruct(approximation, step.Detail.ToArray(), h, g, step.Extent),
                    Fail: static error => IO.pure(Fin.Fail<double[]>(error)))))
            .Map(result => result.Map(static a => {
                float[] samples = new float[a.Length];
                for (int i = 0; i < a.Length; i++) { samples[i] = (float)a[i]; }
                return (ReadOnlyMemory<float>)samples;
            }));
    }

    private static IO<Fin<double[]>> Reconstruct(double[] approx, double[] detail, double[] h, double[] g, int extent) =>
        Resample(approx, h).Bind(low => low.Match(
            Succ: admitted => Resample(detail, g).Map(high => high.Map(detailBand => {
                double[] merged = new double[extent];
                int span = Math.Min(extent, Math.Min(admitted.Length, detailBand.Length));
                TensorPrimitives.Add(admitted.AsSpan(0, span), detailBand.AsSpan(0, span), merged.AsSpan(0, span));
                return merged;
            })),
            Fail: static error => IO.pure(Fin.Fail<double[]>(error))));

    // Zero-stuff by 2 then one stride-1 Conv1D — the transposed convolution spelled through the one lowering
    // owner; the filter feeds UNREVERSED because cross-correlation with h equals convolution with its reversal.
    private static IO<Fin<double[]>> Resample(double[] x, double[] filter) {
        double[] up = new double[2 * x.Length];
        for (int i = 0; i < x.Length; i++) { up[2 * i] = x[i]; }
        return KernelLowering.Lower(TensorOpFamily.Conv1D,
                Matrix<double>.Build.Dense(1, up.Length, (_, c) => up[c]),
                Matrix<double>.Build.Dense(filter.Length, 1, (r, _) => filter[r]),
                new ConvWindow([filter.Length], [1], [filter.Length / 2], [1], 1, 1, [up.Length]), new ShardPlan.Single())
            .Map(result => result.Map(static y => y.ToColumnMajorArray()));
    }

    // --- [FILTER_DESIGN] -- FIR windowed-sinc, equiripple-over-QR, and the shared IIR bilinear map.

    // Windowed-sinc FIR: the ideal band impulse response tapered by the WindowKind window, the FilterBand row owning the band combine — so the windowed-sinc carries no per-band branch.
    internal static FilterCoefficients WindowedSinc(FilterContext spec) {
        int taps = spec.Order | 1;                              // odd length for a Type-I linear-phase center tap.
        double[] window = spec.Window.Taper(taps, periodic: false);
        double[] lower = Kernel(spec.NormalizedCutoff);
        double[] upper = spec.Band.Upper ? Kernel(spec.NormalizedUpper) : [];   // second kernel only where the band reads a second edge.
        return new FilterCoefficients(FilterDesign.FirWindow, spec.Band.Combine(lower, upper, taps), new double[] { 1.0 });

        double[] Kernel(double fc) {
            double[] k = new double[taps];
            int mid = taps / 2;
            double sum = 0.0;
            for (int i = 0; i < taps; i++) {
                int m = i - mid;
                k[i] = (m == 0 ? 2.0 * fc : Math.Sin(2.0 * Math.PI * fc * m) / (Math.PI * m)) * window[i];
                sum += k[i];
            }
            for (int i = 0; i < taps; i++) { k[i] /= sum; }   // unity DC gain.
            return k;
        }
    }

    // Equiripple FIR by the Lawson weighted-least-squares over the blas thin-QR across an EXPLICIT transition (don't-care) band, the `FilterBand` row supplying the per-frequency desired/weight (ripple-derived δp/δs) and the error envelope reweighting toward the Chebyshev minimax.
    // Transition rows stay zero-weight so the solve never rings the brick-wall discontinuity — a single hard 0/1 step with uniform weight is the deleted naive form that Gibbs-rings.
    internal static Fin<FilterCoefficients> Equiripple(FilterContext spec) {
        int taps = spec.Order | 1, half = taps / 2 + 1, grid = 8 * taps;
        double c1 = spec.NormalizedCutoff;
        double c2 = spec.NormalizedUpper > c1 && spec.NormalizedUpper < 1.0 ? spec.NormalizedUpper : Math.Min(0.99, c1 + Math.Max(0.05, 0.25 * (1.0 - c1)));
        double transition = spec.Band.Upper ? Math.Min(0.25 * (c2 - c1), Math.Min(0.5 * c1, 0.5 * (1.0 - c2))) : 0.0;
        double dp = (Math.Pow(10.0, spec.RippleDb / 20.0) - 1.0) / (Math.Pow(10.0, spec.RippleDb / 20.0) + 1.0);   // passband ripple δp.
        double ds = Math.Pow(10.0, -spec.StopbandDb / 20.0);                                                      // stopband ripple δs.
        double passWeight = 1.0, stopWeight = ds <= 0.0 ? 1.0 : dp / ds;                                          // heavier where the ripple bound is tighter.
        Matrix<double> design = Matrix<double>.Build.Dense(grid, half, (r, c) => Math.Cos(Math.PI * ((double)r / (grid - 1)) * c));
        Vector<double> target = Vector<double>.Build.Dense(grid);
        Vector<double> weight = Vector<double>.Build.Dense(grid);
        for (int r = 0; r < grid; r++) {
            (double desired, double w) = spec.Band.Ideal((double)r / (grid - 1), c1, c2, transition, passWeight, stopWeight);
            target[r] = desired; weight[r] = w;
        }
        Fin<(Vector<double> Coefficients, Vector<double> Weight, bool Converged)> fitted =
            toSeq(Enumerable.Range(0, spec.Budget.MaxIterations)).Fold(
                Fin.Succ((Vector<double>.Build.Dense(half), weight, false)),
                (state, iteration) => state.Bind(current => {
                    if (current.Converged) { return Fin.Succ(current); }
                    Matrix<double> aw = Matrix<double>.Build.Dense(grid, half, (r, c) => Math.Sqrt(current.Weight[r]) * design[r, c]);
                    Vector<double> bw = Vector<double>.Build.Dense(grid, r => Math.Sqrt(current.Weight[r]) * target[r]);
                    return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), aw, bw, TolerancePolicy.Derive(aw, bw))
                        .Map(coefficients => {
                            Vector<double> response = design * coefficients;
                            // Lawson reweight MULTIPLIES the running weight by the error envelope, renormalized by the peak — a replacing
                            // update discards the δp/δs band ratio after one pass, an unnormalized product underflows over the iteration.
                            double[] raw = [.. Enumerable.Range(0, grid).Select(r => current.Weight[r] <= 0.0 ? 0.0 : current.Weight[r] * Math.Abs(response[r] - target[r]))];
                            double peak = TensorPrimitives.Max<double>(raw);
                            Vector<double> nextWeight = peak > 0.0 ? Vector<double>.Build.DenseOfArray(raw) / peak : current.Weight;
                            bool converged = iteration > 0 &&
                                TensorPrimitives.Distance<double>(coefficients.ToArray(), current.Coefficients.ToArray()) <=
                                spec.Budget.Tolerance * (1.0 + TensorPrimitives.Norm<double>(coefficients.ToArray()));
                            return (coefficients, nextWeight, converged);
                        });
                }));
        return fitted.Bind(state => state.Converged
            ? Fin.Succ(Symmetric(state.Coefficients, taps))
            : Fin.Fail<FilterCoefficients>(ComputeFault.Create($"<remez-nonconverged:{spec.Budget.MaxIterations}>")));

        static FilterCoefficients Symmetric(Vector<double> halfBand, int taps) {
            int mid = taps / 2;
            double[] b = new double[taps];
            for (int i = 0; i < taps; i++) { int k = Math.Abs(i - mid); b[i] = halfBand[k] * (k == 0 ? 1.0 : 0.5); }   // cosine-series center tap a₀, off-center aₖ/2.
            return new FilterCoefficients(FilterDesign.FirRemez, b, new double[] { 1.0 });
        }
    }

    // One bilinear map for every IIR family AND band: pre-warp the band edge(s), the `FilterBand` row frequency-transforming the normalized-lowpass prototype to the LP/HP/BP/BS s-plane
    // (the lp2{lp,hp,bp,bs} zpk algebra — BP/BS double the order, HP adds origin zeros, BS adds ±jω₀ notch zeros); per-family variation is the analog prototype the row closes over, per-band variation `FilterBand.ToAnalog`.
    internal static FilterCoefficients Bilinear(FilterDesign design, Func<FilterContext, (Complex[] Zeros, Complex[] Poles, double Gain)> prototype, FilterContext spec) {
        (Complex[] Zeros, Complex[] Poles, double Gain) proto = prototype(spec);
        double fs2 = 2.0 * spec.SampleRate;
        double w1 = fs2 * Math.Tan(Math.PI * spec.NormalizedCutoff / 2.0);   // pre-warped lower/only band edge.
        double w2 = fs2 * Math.Tan(Math.PI * spec.NormalizedUpper / 2.0);    // pre-warped upper band edge (BP/BS).
        (Complex[] sZeros, Complex[] sPoles, double bandGain) = spec.Band.ToAnalog(proto, w1, w2);
        Complex[] zZeros = [.. sZeros.Select(s => (fs2 + s) / (fs2 - s))];
        Complex[] zPoles = [.. sPoles.Select(s => (fs2 + s) / (fs2 - s))];
        while (zZeros.Length < zPoles.Length) { zZeros = [.. zZeros, new Complex(-1.0, 0.0)]; }   // bilinear maps s=∞ zeros to z=−1.
        double k = proto.Gain * bandGain * Real(Product(sZeros.Select(s => fs2 - s)) / Product(sPoles.Select(s => fs2 - s)));
        return new FilterCoefficients(design, Expand(zZeros, k), Expand(zPoles, 1.0));

        static Complex Product(System.Collections.Generic.IEnumerable<Complex> xs) { Complex p = Complex.One; foreach (Complex x in xs) { p *= x; } return p; }
        static double Real(Complex c) => c.Real;
        // Expand ∏(z − root) to a real coefficient polynomial; conjugate-paired roots cancel the imaginary part.
        static double[] Expand(Complex[] roots, double scale) {
            Complex[] poly = [Complex.One];
            foreach (Complex r in roots) {
                Complex[] next = new Complex[poly.Length + 1];
                for (int i = 0; i < poly.Length; i++) { next[i] += poly[i]; next[i + 1] -= poly[i] * r; }
                poly = next;
            }
            return [.. poly.Select(c => c.Real * scale)];
        }
    }

    // --- [HELPERS]

    private static double BinHz(int length, double sampleRate) => Fourier.FrequencyScale(length, sampleRate) is [_, double second, ..] ? second : sampleRate / length;

    // --- [PROTOTYPES] -- closed-form normalized-lowpass poles and zeros consumed by the one `Bilinear` map.

    internal static (Complex[] Zeros, Complex[] Poles, double Gain) Butterworth(FilterContext spec) {
        int n = spec.Order;
        Complex[] poles = new Complex[n];
        for (int k = 0; k < n; k++) { double theta = Math.PI * (2.0 * k + 1.0 + n) / (2.0 * n); poles[k] = new Complex(Math.Cos(theta), Math.Sin(theta)); }
        return ([], poles, 1.0);
    }

    internal static (Complex[] Zeros, Complex[] Poles, double Gain) Chebyshev1(FilterContext spec) {
        int n = spec.Order;
        double eps = Math.Sqrt(Math.Pow(10.0, spec.RippleDb / 10.0) - 1.0), mu = Math.Asinh(1.0 / eps) / n;
        Complex[] poles = new Complex[n];
        for (int k = 0; k < n; k++) {
            double theta = Math.PI * (2.0 * k + 1.0) / (2.0 * n);
            poles[k] = new Complex(-Math.Sinh(mu) * Math.Sin(theta), Math.Cosh(mu) * Math.Cos(theta));
        }
        double gain = Product(poles);                          // leading gain ∏(−pₖ); even order drops by 1/√(1+ε²) to the ripple floor.
        return ([], poles, (n & 1) == 1 ? gain : gain / Math.Sqrt(1.0 + eps * eps));

        static double Product(Complex[] ps) { Complex p = Complex.One; foreach (Complex x in ps) { p *= -x; } return p.Real; }
    }

    internal static (Complex[] Zeros, Complex[] Poles, double Gain) Chebyshev2(FilterContext spec) {
        int n = spec.Order;
        double eps = 1.0 / Math.Sqrt(Math.Pow(10.0, spec.StopbandDb / 10.0) - 1.0), mu = Math.Asinh(1.0 / eps) / n;
        Complex[] poles = new Complex[n];
        Complex[] zeros = new Complex[n - n % 2];
        for (int k = 0, z = 0; k < n; k++) {
            double theta = Math.PI * (2.0 * k + 1.0) / (2.0 * n);
            Complex sp = new(-Math.Sinh(mu) * Math.Sin(theta), Math.Cosh(mu) * Math.Cos(theta));
            poles[k] = 1.0 / sp;                                // inverse Chebyshev reciprocates the Cheby-I poles.
            if (Math.Abs(Math.Cos(theta)) > 1e-12) { zeros[z++] = new Complex(0.0, 1.0 / Math.Cos(theta)); }
        }
        Complex pn = Complex.One; foreach (Complex p in poles) { pn *= -p; }   // unity-passband leading gain
        Complex zn = Complex.One; foreach (Complex z in zeros) { zn *= -z; }   // ∏(−pₖ)/∏(−zₖ): |H(0)| = 1.
        return (zeros, poles, (pn / zn).Real);
    }

    // Elliptic selectivity derives modulus `k` through the Landen `Sne` degree equation; normalized `Sne`/`Cde` place the Cauer zero-pole lattice.
    // `Asne` derives the `v0` offset, keeping every pole in the left half-plane and both bands equiripple.
    internal static (Complex[] Zeros, Complex[] Poles, double Gain) Elliptic(FilterContext spec) {
        int n = spec.Order, half = n / 2, parity = n & 1;
        double gp = Math.Pow(10.0, -spec.RippleDb / 20.0);                                  // passband gain Gp.
        double eps = Math.Sqrt(Math.Pow(10.0, spec.RippleDb / 10.0) - 1.0);                 // passband ripple εp.
        double k1 = eps / Math.Sqrt(Math.Pow(10.0, spec.StopbandDb / 10.0) - 1.0);          // εp/εs selectivity.
        double k = DegreeModulus(k1, n);                                                    // passband modulus.
        double v0 = (-Complex.ImaginaryOne * Asne(new Complex(0.0, 1.0 / eps), k1) / n).Real;
        Complex[] poles = new Complex[n];
        Complex[] zeros = new Complex[2 * half];
        for (int i = 0; i < half; i++) {
            double ui = (2.0 * i + 1.0) / n;
            Complex pole = Complex.ImaginaryOne * Cde(new Complex(ui, -v0), k);             // j·cd(uᵢ − j·v₀): LHP conjugate pair.
            poles[2 * i] = pole; poles[2 * i + 1] = Complex.Conjugate(pole);
            Complex zero = Complex.ImaginaryOne / (k * Cde(new Complex(ui, 0.0), k));       // j/(k·cd(uᵢ)): imaginary-axis pair.
            zeros[2 * i] = zero; zeros[2 * i + 1] = Complex.Conjugate(zero);
        }
        if (parity == 1) { poles[n - 1] = Complex.ImaginaryOne * Sne(new Complex(0.0, v0), k); }   // odd real LHP pole j·sn(j·v₀).
        Complex pn = Complex.One; foreach (Complex p in poles) { pn *= -p; }
        Complex zn = Complex.One; foreach (Complex z in zeros) { zn *= -z; }
        double gain = (Math.Pow(gp, 1 - parity) * (pn / zn)).Real;                          // leading gain so |H(0)| = Gpᵖᵃʳⁱᵗʸ⁻¹·... (1 odd, Gp even).
        return (zeros, poles, gain);

        // Degree equation by the Landen `Sne`-product: k = √(1 − (k'ⁿ·∏ sn⁴((2i−1)/n, k'))²), k' the complement.
        static double DegreeModulus(double k1, int n) {
            double kc = Math.Sqrt(1.0 - k1 * k1), prod = 1.0;
            for (int i = 1; i <= n / 2; i++) { double s = Sne(new Complex((2.0 * i - 1.0) / n, 0.0), kc).Real; prod *= s * s * s * s; }
            double kp = Math.Pow(kc, n) * prod;
            return Math.Sqrt(1.0 - kp * kp);
        }
        // Jacobi cd/sn in the normalized argument, built by the ASCENDING Landen recursion w ← (1+kₙ)·w/(1+kₙ·w²) from the descending modulus ladder seeded at sin(u·π/2) (the k=0 value).
        static Complex Cde(Complex u, double k) => Sne(Complex.One - u, k);
        static Complex Sne(Complex u, double k) {
            Span<double> v = stackalloc double[8];
            Landen(k, v);
            Complex w = Complex.Sin(u * (Math.PI / 2.0));
            for (int i = v.Length - 1; i >= 0; i--) { w = (1.0 + v[i]) * w / (1.0 + v[i] * w * w); }
            return w;
        }
        // Inverse sn: undo each ascending step (last-applied first) by the algebraic quadratic root, then Newton-refine from that BOUNDED seed — the seed keeps u small so `Sne`'s `Complex.Sin` never overflows.
        static Complex Asne(Complex w, double k) {
            Span<double> v = stackalloc double[8];
            Landen(k, v);
            Complex ww = w;
            foreach (double kn in v) {
                if (kn < 1e-15 || ww == Complex.Zero) { continue; }                         // a vanished modulus is the identity step.
                Complex disc = Complex.Sqrt((1.0 + kn) * (1.0 + kn) - 4.0 * kn * ww * ww);
                ww = ((1.0 + kn) - disc) / (2.0 * kn * ww);
            }
            Complex u = 2.0 / Math.PI * Complex.Asin(ww);
            for (int i = 0; i < 6; i++) {
                Complex df = (Sne(u + 1e-7, k) - Sne(u - 1e-7, k)) / 2e-7;
                if (df == Complex.Zero) { break; }
                u -= (Sne(u, k) - w) / df;
            }
            return u;
        }
        // Descending modulus ladder kₙ₊₁ = (1−k'ₙ)/(1+k'ₙ) shared by the forward `Sne` and the inverse `Asne`.
        static void Landen(double k, Span<double> moduli) {
            double kk = k;
            for (int i = 0; i < moduli.Length; i++) { double kp = Math.Sqrt(1.0 - kk * kk); kk = (1.0 - kp) / (1.0 + kp); moduli[i] = kk; }
        }
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [REMEZ_EXCHANGE]-[OPEN]: exact alternation-set convergence proof for the equiripple Lawson weighted-least-squares iteration; verify against the Chebyshev equioscillation theorem at the filter-design gate.
- [DWT_BOUNDARY]-[OPEN]: boundary mode for the DWT cascade — the `Conv1D` lowering zero-pads while a symmetric/periodic extension preserves perfect reconstruction at signal edges, so the landed synthesis bank is edge-exact only in the interior; verify the extension mode at the reconstruction gate.
