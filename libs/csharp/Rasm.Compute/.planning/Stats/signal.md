# [COMPUTE_SIGNAL]

Rasm.Compute signal/spectral lane: one `SpectralTransform` `[SmartEnum<string>]` frequency-domain axis whose every row carries its own transform kernel as a row delegate, folding to ONE `Transform.Apply` that returns the polymorphic `SpectralOutput` `[Union]` (`Bins(Spectrum)` per-bin · `Frames(Spectrogram)` time-frequency · `Bands(WaveletDecomposition)` multi-resolution), and one `FilterDesign` `[SmartEnum<string>]` axis whose IIR rows each carry their analog `Prototype` pole-zero delegate folded through ONE shared `Bilinear` map and whose FIR rows fold to the windowed-sinc or the equiripple `DenseRoute` least-squares route. The per-bin transforms ride `MathNet.Numerics.IntegralTransforms.Fourier.Forward`/`Inverse`/`ForwardReal` over a split real/imaginary `double[]` pair, the windowing rides the `MathNet.Numerics.Window` factory through the `WindowKind` row, the magnitude/phase read through the vectorized `TensorPrimitives.Hypot`/`Atan2` (never a per-element `Math.Sqrt(re²+im²)` loop), the bin spacing reads the real `Fourier.FrequencyScale` resolution (never `1/N`), the FFT overlap-add spectral product rides the `Tensor/dispatch#KERNEL_DISPATCH` `ComplexZip(TensorOpFamily.Multiply)`, and the FIR convolution and the wavelet QMF cascade ride the `Tensor/factor#KERNEL_LOWERING` `Conv1D` — never re-implementing the radix-2/Bluestein kernel, the GEMM, or a second Complex carrier. The page owns the `SpectralTransform`/`FilterDesign`/`WindowKind`/`WaveletFamily` vocabulary, the `SpectralOutput`/`Spectrum`/`Spectrogram`/`WaveletDecomposition`/`FilterCoefficients` carriers, the `SignalPolicy`/`FilterSpec` policy records, and the `Transform` Apply/Design surface; the Fourier/window surfaces ride the admitted `MathNet.Numerics` assembly, the spectral product and the magnitude read the dispatch lane, the convolution rides the factor lane, the equiripple FIR fit and the modal eigen-spectra cross to the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute`/`Evd` route, and `ComputeFault`, `ClockPolicy`, and the `ComparerAccessors.StringOrdinal` accessor arrive settled. Spectral features feed the `Stats/estimator#ESTIMATOR_LANE` time-series and classification rows, and filtered/conditioned signals feed those estimators and the `Solver/clash#CLASH_AND_TWIN` Twin anomaly lane.

## [01]-[INDEX]

- [01]-[SIGNAL_LANE]: spectral `Transform` axis (FFT/STFT/PSD/wavelet) over MathNet Fourier + Window returning one `SpectralOutput` union; FIR/IIR filter design over one band-aware bilinear map (LP/HP/BP/BS) and three application routes (short-FIR Conv1D, long-FIR FFT overlap-add, IIR direct-form-II recurrence).

## [02]-[SIGNAL_LANE]

- Owner: `SpectralTransform` `[SmartEnum<string>]` the frequency-domain rows, each carrying a `Windowed` flag and a `Kernel` row delegate `(ReadOnlyMemory<float>, SignalPolicy, Instant) → Fin<SpectralOutput>` that IS the transform — so `Transform.Apply` is one delegate dispatch, never a switch cascade; `FilterDesign` `[SmartEnum<string>]` the filter rows carrying a `Recursive` column (FIR feed-forward versus IIR feedback) and, for the IIR rows, the analog `Prototype` delegate `(FilterSpec) → (Complex[] Zeros, Complex[] Poles, double Gain)` placing the normalized-lowpass s-plane pole/zero set; `WindowKind` `[SmartEnum<string>]` the window rows binding the `MathNet.Numerics.Window` factory through one `(width, periodic) → double[]` taper delegate; `WaveletFamily` `[SmartEnum<string>]` the wavelet rows carrying the frozen analysis scaling coefficients from which the QMF high-pass derives; `SpectralOutput` `[Union]` the one transform result (`Bins`/`Frames`/`Bands`); `Spectrum`/`Spectrogram`/`WaveletDecomposition` the case payloads; `FilterCoefficients` the typed FIR/IIR coefficient record whose `Apply` routes to `Conv1D` (short FIR), FFT overlap-add (long FIR), or the direct-form-II recurrence (IIR); `SignalPolicy`/`FilterSpec` the policy records; `Transform` the static spectral-and-filter surface.
- Cases: `SpectralTransform` rows fft · ifft · rfft · stft · spectrogram · welch-psd · dwt (7); `FilterDesign` rows fir-window · fir-remez · iir-butterworth · iir-chebyshev1 · iir-chebyshev2 · iir-elliptic (6); `FilterBand` rows low-pass · high-pass · band-pass · band-stop (4, each owning its FIR combine, analog→s-plane transform, and equiripple desired/weight); `WindowKind` rows hann · hamming · blackman · blackman-harris · blackman-nuttall · nuttall · flat-top · bartlett · bartlett-hann · cosine · lanczos · triangular · gauss · tukey · rectangular (15, the full `MathNet.Numerics.Window` catalog; the periodic-paired rows carry both forms, the single-form rows one); `WaveletFamily` rows haar · db2 · db4 · sym4 · coif1 (5).
- Entry: `public static Fin<SpectralOutput> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, ClockPolicy clocks)` is the one spectral fold dispatching to the row's `Kernel` — `Fin<T>` aborts on an empty signal or a frame larger than the signal, while the wavelet cascade truncates to the depth the signal admits (`WaveletDecomposition.Levels` reports the realized count); the fold is correlation-free and allocation-light because receipt evidence rides the sink edge, never the hot path; `public static Fin<FilterCoefficients> Design(FilterDesign design, FilterSpec spec)` runs the `FilterSpec.Admit` ingress gate (order ≥ 1, cutoff in `(0, Nyquist)`, ordered band edges for BP/BS) then dispatches to the row's `Designer` delegate, producing the typed coefficients (windowed-sinc, equiripple, or the band-transformed bilinear-prototype); `FilterCoefficients.Apply(ReadOnlyMemory<float> signal)` folds the FIR convolution or the IIR recurrence; `FilterCoefficients.ZeroPhase(ReadOnlyMemory<float> signal)` runs the forward-backward `filtfilt`.
- Auto: each `SpectralTransform` row's `Kernel` realizes its transform — fft widens the signal to a `double[]` once through `TensorPrimitives.ConvertChecked<float,double>`, runs `Fourier.Forward(real, imaginary, policy.Scaling)` over the split pair (imaginary zeroed), and reads `TensorPrimitives.Hypot(real, imaginary, magnitude)`/`Atan2(imaginary, real, phase)` into a full-length `Spectrum`; rfft runs `Fourier.ForwardReal` over the `N+2` packed buffer and yields the Hermitian half-spectrum (bins `0..N/2`); ifft runs `Fourier.Inverse(real, imaginary, policy.Scaling)` and returns the reconstructed real part; stft/spectrogram frame the signal by `policy.HopSize`, window each frame through the `WindowKind` taper, transform per frame, and stack the power columns into a `Spectrogram`; welch-psd averages the windowed periodograms of the overlapping segments into a `Spectrum`; dwt cascades the `policy.Wavelet` QMF analysis filters (the family selected across the haar/db2/db4/sym4/coif1 rows) through the stride-2 `Conv1D` lowering into a `WaveletDecomposition`; `Design` produces the FIR taps (windowed-sinc through the `WindowKind` factory plus the band transform, or the equiripple Lawson weighted-least-squares — band-aware over an explicit transition band — iterated over the `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR route) or the IIR `(b, a)` coefficients (one `Bilinear` map that band-transforms the row's analog `Prototype` to the LP/HP/BP/BS s-plane — origin zeros for HP, order-doubling root split for BP, ±jω₀ notch for BS — before discretizing); `FilterCoefficients.Apply` reverses the FIR taps and folds them through `KernelLowering.Lower(TensorOpFamily.Conv1D, …)` (short FIR, true convolution from the cross-correlation kernel), the FFT overlap-add spectral product through `Fourier` + `ComplexZip(TensorOpFamily.Multiply)` (long FIR), or the IIR `(b, a)` through the direct-form-II-transposed recurrence.
- Receipt: the spectral fold emits no receipt on the hot path — evidence rides the typed carrier (`Spectrum.Length`/`BinHz`, `Spectrogram.Frames`·`Bins`, `WaveletDecomposition.Levels`) and, for the tensor-lowered legs only, the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.TensorRun(Family, Dtype, Elements, SimdWidth, Partitions)` the composed op already stamps: a short-FIR application and the wavelet QMF cascade ride the `Tensor/factor#KERNEL_LOWERING` `Family = TensorOpFamily.Conv1D` (im2col→GEMM, inheriting the factor lane's GEMM provider/determinism evidence) and the long-FIR overlap-add rides the `Tensor/dispatch#KERNEL_DISPATCH` `Family = TensorOpFamily.Multiply` `ComplexZip` (elementwise SIMD, inheriting the dispatch lane's `SimdWidth`/`Partitions` determinism — no GEMM provider), both reproducible, while the IIR direct-form-II-transposed recurrence and the bare MathNet `Fourier` transforms (per-bin/framed/Welch) ride no tensor op and so mint no `TensorRun` — a per-bin FFT's evidence is its typed `Spectrum` shape, never a fabricated `TensorRun` over a `TensorOpFamily` that never executed.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new spectral transform is one `SpectralTransform` row binding its `Kernel` delegate over `Fourier`; a new window is one `WindowKind` row binding its `Window` factory; a new wavelet is one `WaveletFamily` row carrying its scaling coefficients (selected at the wire through `SignalPolicy.Wavelet`); a new filter family is one `FilterDesign` row binding its `Designer` delegate — a new IIR family also carrying its analog `Prototype` folded through the unchanged band-aware `Bilinear`; zero new surface — an `FftTransform`/`StftTransform`/`PsdEstimator`/`WaveletTransform` class family is the rejected form collapsed onto the one `Transform.Apply` delegate dispatch, a `FirFilter`/`IirFilter` sibling family is collapsed onto the one `FilterCoefficients` carrier, a `ButterworthDesigner`/`ChebyshevDesigner`/`EllipticDesigner` trio is collapsed onto the one `Bilinear` over per-row prototypes, and a re-implemented radix-2/Bluestein FFT, GEMM, or QR beside the admitted owners is the deleted form.
- Boundary: `MathNet.Numerics.IntegralTransforms.Fourier` operates in place with its `FourierOptions` scaling conventions, so the per-bin and framed transform kernels apply the policy's `FourierOptions.Default` (symmetric `1/√N` on BOTH directions, so a standalone forward∘inverse already composes to identity) while the two normalization-owning paths instead pin `FourierOptions.NoScaling` and apply their lone scale by hand — the FFT-convolution overlap-add (no per-transform scaling either way would otherwise round-trip to `N`×) divides the `1/N` the convolution theorem owns after the inverse, and the Welch periodogram divides the `1/(fs·U·segments)` power-density normalizer the PSD owns; either path never re-implements the radix-2/Bluestein kernel; the split real/imaginary `Forward(double[], double[], options)` form is preferred over the `Complex[]` form because it lets `TensorPrimitives.Hypot`/`Atan2` read magnitude/phase over contiguous vectorized spans with no `Complex` marshalling, while `ForwardReal`/`InverseReal` own the `N+2` packed half-spectrum the rfft/ifft-real rows deinterleave, and the bin resolution reads `Fourier.FrequencyScale(length, policy.SampleRate)` (the real `Δf = fs/N`) rather than the meaningless `1/N`; the window functions ride `MathNet.Numerics.Window` — the symmetric form (`Hann`/`Hamming`/`Cosine`/`Lanczos`/`Bartlett`/`BartlettHann`/`Blackman`/`BlackmanHarris`/`BlackmanNuttall`/`Nuttall`/`FlatTop`/`Triangular`, the `Gauss(width, sigma)` and `Tukey(width, r)` parameter-baked rows, and `Dirichlet` as the all-ones rectangular taper) for filter design, the `*Periodic` form (`HannPeriodic`/`HammingPeriodic`/`CosinePeriodic`/`LanczosPeriodic`, the only four MathNet ships) for the FFT-periodic STFT posture — so the periodic flag is honored only where MathNet exposes both forms, never a hand-rolled cosine taper or an `Enumerable.Repeat` rectangular reimplementation; the complex magnitude never runs a per-element `x.Magnitude` loop — `TensorPrimitives.Hypot` is the overflow-safe vectorized owner the split real/imaginary rows read, and the `Tensor/dispatch#KERNEL_DISPATCH` `ComplexAbs` `MagnitudeKernel` is the `Complex[]`-form magnitude sibling this page never needs — every magnitude-bearing row runs over the split `double[]` pair, while the overlap-add path holds its `Complex[]` only for the `ComplexZip(Multiply)` convolution product read back through `product[i].Real`, never a magnitude; IIR filters are recursive (feedback) so they do NOT lower to the feed-forward `Conv1D` GEMM — only FIR application rides the convolution route (short FIR through `KernelLowering`, long FIR through FFT overlap-add) while the `FilterCoefficients.Apply` fold routes IIR to the direct-form-II-transposed recurrence, three application mechanisms selected by the `Fir` discriminant and tap length (a forced `Conv1D` lowering for an IIR filter is the deleted form because the recurrence has feedback the GEMM cannot express); the FIR taps are REVERSED before the `Conv1D` feed because the lowering kernel computes cross-correlation while a filter is true convolution — for a linear-phase symmetric FIR the two coincide, but the reversal makes the route correct for any tap set; zero-phase `filtfilt` odd-reflects the edges, runs the recurrence forward then backward, and trims the pad — cancelling the IIR group delay (zero phase, `|H|²`) with the boundary transient a bare double pass would leave suppressed; the IIR coefficient SOURCE is one band-aware `Bilinear` map (pre-warp the band edge(s), frequency-transform the normalized-lowpass prototype to the LP/HP/BP/BS band — the lp2{lp,hp,bp,bs} zpk algebra, BP/BS doubling the order — bilinear-transform each s-plane root to z, expand the conjugate-paired roots to real `(b, a)` polynomials) consuming each row's closed-form analog `Prototype` (Butterworth poles on the unit circle, Chebyshev-I poles on an ellipse via `asinh(1/ε)/n`, Chebyshev-II reciprocal poles plus imaginary-axis zeros with the `∏(−p)/∏(−z)` gain, elliptic poles/zeros via the descending-Landen Jacobi `sn`/`cd` and the `sne`-product degree equation), never a per-family designer; the equiripple `fir-remez` row grounds the Lawson weighted-least-squares iteration over the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR (the `FilterBand`-supplied band-aware cosine design matrix solved across an EXPLICIT transition band under the ripple-derived `δp/δs` weight, the error envelope reweighting the active bands toward the Chebyshev minimax), never a hand-rolled normal-equations solve and never a brick-wall target that Gibbs-rings; the dwt cascade rides the `Tensor/factor#KERNEL_LOWERING` stride-2 `Conv1D` exactly as the FIR application rides convolution, and MathNet ships no wavelet surface so the `WaveletFamily` scaling tables are authored here (the analysis family selected at the wire by `policy.Wavelet`) and the high-pass derives by the QMF mirror `g[k] = (−1)ᵏ·h[N−1−k]`; the spectral features feed the `Stats/estimator#ESTIMATOR_LANE` time-series and classification rows, the modal spectra cross to the `Tensor/blas#DENSE_ALGEBRA` `Evd`/`SpectralResult` eigen lane (the FFT of a transient `Solver/contract#SOLVE_CONTRACT` `fea-transient` march on the modal seam), and the conditioned signal feeds the learning and Twin lanes — the transform shares the dispatch Complex kernels and never re-mints a second Complex carrier.

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
    public static readonly WindowKind Gauss          = new("gauss",           static (w, _) => Window.Gauss(w, w * 0.2));
    public static readonly WindowKind Tukey          = new("tukey",           static (w, _) => Window.Tukey(w, 0.5));
    public static readonly WindowKind Rectangular    = new("rectangular",     static (w, _) => Window.Dirichlet(w));

    private readonly Func<int, bool, double[]> taper;

    public double[] Taper(int width, bool periodic) => taper(width, periodic);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveletFamily {
    // Daubechies/symlet/coiflet analysis scaling (low-pass) coefficients; the analysis high-pass and the
    // synthesis pair derive by the quadrature-mirror relation, so one frozen table per family owns the bank.
    public static readonly WaveletFamily Haar  = new("haar",  [0.70710678118654752, 0.70710678118654752]);
    public static readonly WaveletFamily Db2   = new("db2",   [0.48296291314469025, 0.83651630373746899, 0.22414386804185735, -0.12940952255092145]);
    public static readonly WaveletFamily Db4   = new("db4",   [0.23037781330885523, 0.71484657055254153, 0.63088076792959036, -0.02798376941698385, -0.18703481171888114, 0.03084138183598697, 0.03288301166698295, -0.01059740178499728]);
    public static readonly WaveletFamily Sym4  = new("sym4",  [-0.07576571478927333, -0.02963552764599851, 0.49761866763201960, 0.80373875180591614, 0.29785779560527736, -0.09921954357684722, -0.01260396726203783, 0.03222310060404270]);
    public static readonly WaveletFamily Coif1 = new("coif1", [-0.01565572813546454, -0.07273261951285648, 0.38486484686420286, 0.85257202021225542, 0.33789766245780922, -0.07273261951285648]);

    private readonly double[] scaling;

    public ReadOnlyMemory<double> LowPass => scaling;

    // QMF mirror: the analysis high-pass alternates sign over the reversed scaling coefficients.
    public double[] HighPass() {
        var g = new double[scaling.Length];
        for (int k = 0; k < g.Length; k++) { g[k] = ((k & 1) == 0 ? 1.0 : -1.0) * scaling[scaling.Length - 1 - k]; }
        return g;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralTransform {
    public static readonly SpectralTransform Fft         = new("fft",         windowed: false, static (s, p, t) => Transform.PerBin(s, p, t, inverse: false));
    public static readonly SpectralTransform Ifft        = new("ifft",        windowed: false, static (s, p, t) => Transform.PerBin(s, p, t, inverse: true));
    public static readonly SpectralTransform Rfft        = new("rfft",        windowed: false, static (s, p, t) => Transform.RealForward(s, p, t));
    public static readonly SpectralTransform Stft        = new("stft",        windowed: true,  static (s, p, t) => Transform.ShortTime(s, p, t, power: false));
    public static readonly SpectralTransform Spectrogram = new("spectrogram", windowed: true,  static (s, p, t) => Transform.ShortTime(s, p, t, power: true));
    public static readonly SpectralTransform WelchPsd    = new("welch-psd",   windowed: true,  static (s, p, t) => Transform.Welch(s, p, t));
    public static readonly SpectralTransform Dwt         = new("dwt",         windowed: false, static (s, p, t) => Transform.Wavelet(s, p, t));

    private readonly Func<ReadOnlyMemory<float>, SignalPolicy, Instant, Fin<SpectralOutput>> kernel;

    public bool Windowed { get; }

    public Fin<SpectralOutput> Run(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at) => kernel(signal, policy, at);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterDesign {
    public static readonly FilterDesign FirWindow      = new("fir-window",      recursive: false, prototype: null,                  static (_, spec) => Fin.Succ(Transform.WindowedSinc(spec)));
    public static readonly FilterDesign FirRemez       = new("fir-remez",       recursive: false, prototype: null,                  static (_, spec) => Transform.Equiripple(spec));
    public static readonly FilterDesign IirButterworth = new("iir-butterworth", recursive: true,  prototype: Prototypes.Butterworth, static (d, spec) => Fin.Succ(Transform.Bilinear(d, spec)));
    public static readonly FilterDesign IirChebyshev1  = new("iir-chebyshev1",  recursive: true,  prototype: Prototypes.Chebyshev1,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, spec)));
    public static readonly FilterDesign IirChebyshev2  = new("iir-chebyshev2",  recursive: true,  prototype: Prototypes.Chebyshev2,  static (d, spec) => Fin.Succ(Transform.Bilinear(d, spec)));
    public static readonly FilterDesign IirElliptic    = new("iir-elliptic",    recursive: true,  prototype: Prototypes.Elliptic,    static (d, spec) => Fin.Succ(Transform.Bilinear(d, spec)));

    private readonly Func<FilterSpec, (Complex[] Zeros, Complex[] Poles, double Gain)>? prototype;
    private readonly Func<FilterDesign, FilterSpec, Fin<FilterCoefficients>> design;

    public bool Recursive { get; }

    // The analog normalized-lowpass pole/zero placement; FIR rows carry none (their Designer routes to WindowedSinc/Equiripple).
    public (Complex[] Zeros, Complex[] Poles, double Gain) Prototype(FilterSpec spec) =>
        (prototype ?? throw new InvalidOperationException(Key))(spec);

    // The row's design mechanism IS the dispatch: a new FilterDesign row cannot compile without binding its
    // Designer, so Transform.Design folds to one delegate call with no runtime-silent fall-through arm.
    public Fin<FilterCoefficients> Run(FilterSpec spec) => design(this, spec);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterBand {
    // One band owns all three modalities as row data — the FIR windowed-sinc combine, the analog→s-plane zpk
    // transform the Bilinear consumes, and the equiripple desired/weight at a normalized frequency — so a new
    // band (all-pass, shelving) is one row each consumer's delegate dispatch breaks on at compile time, never a
    // runtime-silent switch fall-through; `Upper` reports whether the band reads the second edge (BP/BS).
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

    // FIR windowed-sinc combine (lower/upper band kernels → taps); the s-plane zpk band-transform of the analog
    // prototype; the equiripple desired/weight at normalized frequency f with transition tr and band weights.
    public double[] Combine(double[] lower, double[] upper, int taps) => combine(lower, upper, taps);
    public (Complex[] Zeros, Complex[] Poles, double Gain) ToAnalog((Complex[] Zeros, Complex[] Poles, double Gain) prototype, double w1, double w2) => analog(prototype, w1, w2);
    public (double Desired, double Weight) Ideal(double f, double c1, double c2, double transition, double passWeight, double stopWeight) => ideal(f, c1, c2, transition, passWeight, stopWeight);

    static double[] Invert(double[] k, int taps) { var b = (double[])k.Clone(); for (int i = 0; i < b.Length; i++) { b[i] = -b[i]; } b[taps / 2] += 1.0; return b; }   // spectral inversion.
    static double[] Subtract(double[] upper, double[] lower) { var b = new double[upper.Length]; for (int i = 0; i < b.Length; i++) { b[i] = upper[i] - lower[i]; } return b; }
    static System.Collections.Generic.IEnumerable<Complex> Origin(int count) { for (int i = 0; i < count; i++) { yield return Complex.Zero; } }
    static System.Collections.Generic.IEnumerable<Complex> Notch(int count, double w0) { for (int i = 0; i < count; i++) { yield return new Complex(0.0, w0); yield return new Complex(0.0, -w0); } }
    // LP→BP root split: each prototype root scales by BW/2 then splits to r′ ± √(r′²−ω₀²) straddling ±jω₀.
    static Complex[] Pair(Complex[] roots, double halfBw, double w0Sq) {
        var split = new Complex[2 * roots.Length];
        for (int i = 0; i < roots.Length; i++) { Complex c = roots[i] * halfBw, d = Complex.Sqrt(c * c - w0Sq); split[2 * i] = c + d; split[2 * i + 1] = c - d; }
        return split;
    }
    // LP→BS root inversion: each prototype root maps to (BW/2)/r then splits to r′ ± √(r′²−ω₀²).
    static Complex[] PairInv(Complex[] roots, double halfBw, double w0Sq) {
        var split = new Complex[2 * roots.Length];
        for (int i = 0; i < roots.Length; i++) { Complex c = halfBw / roots[i], d = Complex.Sqrt(c * c - w0Sq); split[2 * i] = c + d; split[2 * i + 1] = c - d; }
        return split;
    }
    // HP/BS band gain = Re(∏(−zₖ)/∏(−pₖ)) over the prototype roots (the s=∞ contribution the bilinear maps to z=−1).
    static double BandGain(Complex[] zeros, Complex[] poles) {
        Complex zn = Complex.One; foreach (Complex z in zeros) { zn *= -z; }
        Complex pn = Complex.One; foreach (Complex p in poles) { pn *= -p; }
        return (zn / pn).Real;
    }
}

// --- [MODELS] ---------------------------------------------------------------------------

public sealed record FilterSpec(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double RippleDb, double StopbandDb, WindowKind Window, double SampleRate) {
    public static readonly FilterSpec CanonicalLowPass = new(FilterBand.LowPass, Order: 4, Cutoff: 1000.0, UpperCutoff: 0.0, RippleDb: 1.0, StopbandDb: 40.0, WindowKind.Hamming, SampleRate: 48000.0);

    public double Nyquist => SampleRate * 0.5;
    public double NormalizedCutoff => Cutoff / Nyquist;          // [0, 1) cutoff for the FIR sinc.
    public double NormalizedUpper => UpperCutoff / Nyquist;

    // Ingress gate: a degenerate order, an out-of-band cutoff (the bilinear pre-warp tan(π·f/2) needs f∈(0,1)),
    // or unordered band edges fault cleanly here instead of producing NaN coefficients downstream.
    public Fin<FilterSpec> Admit() =>
        Order < 1 ? Fin.Fail<FilterSpec>(ComputeFault.Create($"<filter-order:{Order}>"))
        : Cutoff <= 0.0 || NormalizedCutoff >= 1.0 ? Fin.Fail<FilterSpec>(ComputeFault.Create($"<filter-cutoff:{Cutoff}/{Nyquist}>"))
        : Band.Upper && (UpperCutoff <= Cutoff || NormalizedUpper >= 1.0)
            ? Fin.Fail<FilterSpec>(ComputeFault.Create($"<filter-band-edges:{Cutoff}..{UpperCutoff}>"))
            : Fin.Succ(this);
}

public sealed record SignalPolicy(SpectralTransform Transform, WindowKind Window, int FrameSize, int HopSize, int Levels, double SampleRate, FourierOptions Scaling, WaveletFamily Wavelet) {
    public static readonly SignalPolicy CanonicalFft   = new(SpectralTransform.Fft, WindowKind.Hann, FrameSize: 1024, HopSize: 512, Levels: 0, SampleRate: 48000.0, FourierOptions.Default, WaveletFamily.Db4);
    public static readonly SignalPolicy CanonicalStft  = CanonicalFft with { Transform = SpectralTransform.Stft };
    public static readonly SignalPolicy CanonicalWelch = CanonicalFft with { Transform = SpectralTransform.WelchPsd, FrameSize = 256, HopSize = 128 };
    public static readonly SignalPolicy CanonicalDwt   = CanonicalFft with { Transform = SpectralTransform.Dwt, Levels = 4, Wavelet = WaveletFamily.Db4 };
}

public sealed record Spectrum(SpectralTransform Transform, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, int Length, double BinHz, double SampleRate, Instant At);

public sealed record Spectrogram(int Frames, int Bins, int Hop, double BinHz, ReadOnlyMemory<double> Power, Instant At);

public sealed record WaveletDecomposition(WaveletFamily Family, int Levels, ReadOnlyMemory<double> Approximation, Seq<ReadOnlyMemory<double>> Details, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralOutput {
    private SpectralOutput() { }

    public sealed record Bins(Spectrum Spectrum) : SpectralOutput;
    public sealed record Frames(Spectrogram Spectrogram) : SpectralOutput;
    public sealed record Bands(WaveletDecomposition Decomposition) : SpectralOutput;
}

public sealed record FilterCoefficients(FilterDesign Design, ReadOnlyMemory<double> B, ReadOnlyMemory<double> A) {
    public bool Fir => A.Length <= 1;

    // Short FIR rides the feed-forward Conv1D GEMM, long FIR the FFT overlap-add spectral product, IIR the
    // direct-form-II-transposed recurrence with the feedback the convolution cannot express. The taps are
    // REVERSED for the Conv1D feed because the lowering kernel cross-correlates while a filter convolves.
    public Fin<float[]> Apply(ReadOnlyMemory<float> signal) =>
        signal.Length == 0
            ? Fin.Fail<float[]>(ComputeFault.Create("<signal-empty>"))
            : Fir
                ? B.Length > 64
                    ? OverlapAdd(signal.Span)
                    : KernelLowering.Lower(TensorOpFamily.Conv1D,
                            Matrix<double>.Build.Dense(1, signal.Length, (_, c) => signal.Span[c]),
                            Matrix<double>.Build.Dense(B.Length, 1, (r, _) => B.Span[B.Length - 1 - r]),
                            new ConvWindow([B.Length], [1], [B.Length / 2], [1], 1, 1, [signal.Length]), new ShardPlan.Single())
                        .Map(static y => y.ToColumnMajorArray().Select(static v => (float)v).ToArray())
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

    // FFT overlap-add: block the signal, transform each padded block and the taps once to the frequency
    // domain, take the dispatch-lane ComplexZip(Multiply) spectral product, inverse-transform, and accumulate
    // the (B.Length−1) tail across blocks. The long-FIR mechanism the same Apply fold selects by tap length.
    Fin<float[]> OverlapAdd(ReadOnlySpan<float> signal) {
        int taps = B.Length, block = 4 * taps, nfft = Transform.NextPow2(block + taps - 1);
        var filterSpec = new Complex[nfft];
        for (int k = 0; k < taps; k++) { filterSpec[k] = new Complex(B.Span[k], 0.0); }
        Fourier.Forward(filterSpec, FourierOptions.NoScaling);
        var y = new float[signal.Length];
        var frame = new Complex[nfft];
        var product = new Complex[nfft];
        for (int start = 0; start < signal.Length; start += block) {
            int len = Math.Min(block, signal.Length - start);
            Array.Clear(frame);
            for (int i = 0; i < len; i++) { frame[i] = new Complex(signal[start + i], 0.0); }
            Fourier.Forward(frame, FourierOptions.NoScaling);
            Fin<Unit> zip = TensorOps.ComplexZip(TensorOpFamily.Multiply, frame, filterSpec, product);
            if (zip.IsFail) { return Fin.Fail<float[]>(ComputeFault.Create("<overlap-add-product>")); }
            Fourier.Inverse(product, FourierOptions.NoScaling);
            for (int i = 0; i < len + taps - 1 && start + i < signal.Length; i++) { y[start + i] += (float)(product[i].Real / nfft); }
        }
        return Fin.Succ(y);
    }

    // Zero-phase filtfilt: odd-reflect the signal edges, filter forward, reverse, filter again, reverse back,
    // then trim the pad — the forward-backward cascade cancels the IIR group delay (zero phase, |H|²) and the
    // 3·(order) odd reflection suppresses the start/stop transients a bare double pass would leave ringing.
    public Fin<float[]> ZeroPhase(ReadOnlyMemory<float> signal) {
        if (signal.Length == 0) { return Fin.Fail<float[]>(ComputeFault.Create("<signal-empty>")); }
        ReadOnlySpan<float> x = signal.Span;
        int pad = Math.Min(3 * (Math.Max(B.Length, A.Length) - 1), x.Length - 1);
        var padded = new float[x.Length + 2 * pad];
        for (int i = 0; i < pad; i++) { padded[i] = 2f * x[0] - x[pad - i]; padded[^(i + 1)] = 2f * x[^1] - x[^(pad - i + 1)]; }   // odd reflection about each endpoint.
        x.CopyTo(padded.AsSpan(pad));
        float[] forward = DirectFormII(padded);
        Array.Reverse(forward);
        float[] backward = DirectFormII(forward);
        Array.Reverse(backward);
        return Fin.Succ(backward[pad..^pad]);
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class Transform {
    public static Fin<SpectralOutput> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, ClockPolicy clocks) =>
        signal.Length == 0
            ? Fin.Fail<SpectralOutput>(ComputeFault.Create("<signal-empty>"))
            : transform.Run(signal, policy with { Transform = transform }, clocks.Now);   // the argument transform is authoritative — the policy stamp can never disagree with the kernel that ran.

    // Admit the spec then dispatch to the row's Designer delegate — one delegate call, no design-identity
    // ternary, so a new FilterDesign row breaks at compile time (unbound Designer) rather than routing silently.
    public static Fin<FilterCoefficients> Design(FilterDesign design, FilterSpec spec) =>
        spec.Admit().Bind(design.Run);

    // --- [SPECTRAL] -- per-bin / framed / wavelet kernels each SpectralTransform row binds.

    internal static Fin<SpectralOutput> PerBin(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at, bool inverse) {
        int n = signal.Length, bins = n;                                                   // full complex transform yields n bins; the Hermitian half-spectrum is the rfft RealForward owner.
        var real = new double[n];
        var imaginary = new double[n];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, real);
        var magnitude = new double[bins];
        var phase = new double[bins];
        if (inverse) {
            Fourier.Inverse(real, imaginary, policy.Scaling);
            real.AsSpan(0, bins).CopyTo(magnitude);                                            // reconstructed real signal (sign-preserving), never the sign-stripped Hypot.
            imaginary.AsSpan(0, bins).CopyTo(phase);                                            // imaginary residual (≈0 for a Hermitian spectrum).
        } else {
            Fourier.Forward(real, imaginary, policy.Scaling);
            TensorPrimitives.Hypot(real.AsSpan(0, bins), imaginary.AsSpan(0, bins), magnitude); // vectorized overflow-safe magnitude, never a per-element x.Magnitude loop.
            TensorPrimitives.Atan2(imaginary.AsSpan(0, bins), real.AsSpan(0, bins), phase);
        }
        return Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, magnitude, phase, bins, BinHz(n, policy.SampleRate), policy.SampleRate, at)));
    }

    // rfft over the packed N+2 half-spectrum: ForwardReal packs bins 0..N/2 as interleaved (re, im) pairs.
    internal static Fin<SpectralOutput> RealForward(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at) {
        int n = signal.Length, bins = n / 2 + 1;               // ForwardReal packs N+2 (even N) / N+1 (odd N); no even-length truncation dropping the last sample.
        var data = new double[n + 2];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, data.AsSpan(0, n));
        Fourier.ForwardReal(data, n, policy.Scaling);
        var magnitude = new double[bins];
        var phase = new double[bins];
        for (int k = 0; k < bins; k++) { magnitude[k] = double.Hypot(data[2 * k], data[2 * k + 1]); phase[k] = Math.Atan2(data[2 * k + 1], data[2 * k]); }
        return Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, magnitude, phase, bins, BinHz(n, policy.SampleRate), policy.SampleRate, at)));
    }

    internal static Fin<SpectralOutput> ShortTime(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at, bool power) {
        int frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        if (frame > signal.Length || hop <= 0) { return Fin.Fail<SpectralOutput>(ComputeFault.Create($"<stft-frame-overruns:{frame}>")); }
        int frames = 1 + (signal.Length - frame) / hop;
        double[] taper = policy.Window.Taper(frame, periodic: true);
        var stack = new double[frames * bins];
        var real = new double[frame];
        var imaginary = new double[frame];
        for (int f = 0; f < frames; f++) {
            int offset = f * hop;
            for (int i = 0; i < frame; i++) { real[i] = signal.Span[offset + i] * taper[i]; }
            Array.Clear(imaginary);
            Fourier.Forward(real, imaginary, policy.Scaling);
            for (int k = 0; k < bins; k++) {
                double m = double.Hypot(real[k], imaginary[k]);
                stack[f * bins + k] = power ? m * m : m;
            }
        }
        return Fin.Succ<SpectralOutput>(new SpectralOutput.Frames(new Spectrogram(frames, bins, hop, BinHz(frame, policy.SampleRate), stack, at)));
    }

    // Welch PSD: average the windowed periodograms |Xf|²/(fs·U) of the overlapping segments, U the window
    // power normalizer; the averaged density is a Spectrum carrier whose Magnitude is the one-sided PSD.
    internal static Fin<SpectralOutput> Welch(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at) {
        int frame = policy.FrameSize, hop = policy.HopSize, bins = frame / 2 + 1;
        if (frame > signal.Length || hop <= 0) { return Fin.Fail<SpectralOutput>(ComputeFault.Create($"<welch-frame-overruns:{frame}>")); }
        int segments = 1 + (signal.Length - frame) / hop;
        double[] taper = policy.Window.Taper(frame, periodic: false);
        double u = 0.0;
        foreach (double t in taper) { u += t * t; }
        double norm = 1.0 / (policy.SampleRate * u * segments);
        var psd = new double[bins];
        var real = new double[frame];
        var imaginary = new double[frame];
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
        return Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(policy.Transform, psd, ReadOnlyMemory<double>.Empty, bins, BinHz(frame, policy.SampleRate), policy.SampleRate, at)));
    }

    // DWT cascade: each level low/high-pass filters the running approximation through the WaveletFamily QMF
    // pair over the stride-2 Conv1D lowering, halving the length; details accumulate, the approximation feeds
    // the next level — the same convolution lowering the FIR application rides, never a bespoke filter bank.
    internal static Fin<SpectralOutput> Wavelet(ReadOnlyMemory<float> signal, SignalPolicy policy, Instant at) {
        WaveletFamily family = policy.Wavelet;                  // policy-selected across haar/db2/db4/sym4/coif1.
        int levels = Math.Max(1, policy.Levels);
        double[] seed = new double[signal.Length];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, seed);
        double[] h = family.LowPass.ToArray(), g = family.HighPass();
        return Cascade(Fin.Succ((seed, Seq<ReadOnlyMemory<double>>())), 0)
            .Map(state => (SpectralOutput)new SpectralOutput.Bands(new WaveletDecomposition(family, state.Details.Count, state.Approx, state.Details, at)));

        // Mallat cascade as an immutable Fin-threaded fold: each level prepends its detail band (coarsest ends
        // at the head) and threads the halved approximation forward, with NO mutable list and no side effect
        // inside a combinator; the cascade stops when the running approximation can no longer fill the QMF
        // support, so the realized `Levels` is whatever depth the signal admits.
        Fin<(double[] Approx, Seq<ReadOnlyMemory<double>> Details)> Cascade(Fin<(double[] Approx, Seq<ReadOnlyMemory<double>> Details)> state, int level) =>
            level >= levels ? state : state.Bind(s => s.Approx.Length < h.Length
                ? Fin.Succ(s)
                : Convolve2(s.Approx, h, g).Bind(band => Cascade(Fin.Succ((band.Low, ((ReadOnlyMemory<double>)band.High).Cons(s.Details))), level + 1)));
    }

    // One stride-2 Conv1D per QMF tap set: the approximation lowers through the factor lane against the
    // reversed analysis filters (true convolution from the cross-correlation kernel), downsampled by 2.
    static Fin<(double[] Low, double[] High)> Convolve2(double[] x, double[] h, double[] g) =>
        Downsample(x, h).Bind(low => Downsample(x, g).Map(high => (low, high)));

    static Fin<double[]> Downsample(double[] x, double[] filter) =>
        KernelLowering.Lower(TensorOpFamily.Conv1D,
                Matrix<double>.Build.Dense(1, x.Length, (_, c) => x[c]),
                Matrix<double>.Build.Dense(filter.Length, 1, (r, _) => filter[filter.Length - 1 - r]),
                new ConvWindow([filter.Length], [2], [filter.Length / 2], [1], 1, 1, [x.Length]), new ShardPlan.Single())
            .Map(static y => y.ToColumnMajorArray());

    // --- [FILTER_DESIGN] -- FIR windowed-sinc, equiripple-over-QR, and the shared IIR bilinear map.

    // Windowed-sinc FIR: the ideal band impulse response tapered by the WindowKind window. The FilterBand row
    // owns the band combine — low-pass IS the sinc kernel, high-pass spectral-inverts it, band-pass/stop
    // difference (and BS inverts) the two low-pass kernels — so the windowed-sinc carries no per-band branch.
    internal static FilterCoefficients WindowedSinc(FilterSpec spec) {
        int taps = spec.Order | 1;                              // odd length for a Type-I linear-phase center tap.
        double[] window = spec.Window.Taper(taps, periodic: false);
        double[] lower = Kernel(spec.NormalizedCutoff);
        double[] upper = spec.Band.Upper ? Kernel(spec.NormalizedUpper) : [];   // second kernel only where the band reads a second edge.
        return new FilterCoefficients(FilterDesign.FirWindow, spec.Band.Combine(lower, upper, taps), new double[] { 1.0 });

        double[] Kernel(double fc) {
            var k = new double[taps];
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

    // Equiripple FIR by the Lawson weighted-least-squares iteration over the blas thin-QR route: the cosine
    // design matrix solves the band-error system over an EXPLICIT transition (don't-care) band, the `FilterBand`
    // row supplies the per-frequency desired/weight (honoring LP/HP/BP/BS and the ripple-derived passband-vs-
    // stopband weight δp/δs), and the error envelope reweights the active bands toward the Chebyshev minimax —
    // the transition rows stay zero-weight so the solve never rings the brick-wall discontinuity a raw target
    // would impose (a single hard 0/1 step with uniform weight is the deleted naive form that Gibbs-rings).
    internal static Fin<FilterCoefficients> Equiripple(FilterSpec spec) {
        int taps = spec.Order | 1, half = taps / 2 + 1, grid = 8 * taps;
        double c1 = spec.NormalizedCutoff;
        double c2 = spec.NormalizedUpper > c1 && spec.NormalizedUpper < 1.0 ? spec.NormalizedUpper : Math.Min(0.99, c1 + Math.Max(0.05, 0.25 * (1.0 - c1)));
        double transition = spec.Band.Upper ? Math.Min(0.25 * (c2 - c1), Math.Min(0.5 * c1, 0.5 * (1.0 - c2))) : 0.0;
        double dp = (Math.Pow(10.0, spec.RippleDb / 20.0) - 1.0) / (Math.Pow(10.0, spec.RippleDb / 20.0) + 1.0);   // passband ripple δp.
        double ds = Math.Pow(10.0, -spec.StopbandDb / 20.0);                                                      // stopband ripple δs.
        double passWeight = 1.0, stopWeight = ds <= 0.0 ? 1.0 : dp / ds;                                          // heavier where the ripple bound is tighter.
        var design = Matrix<double>.Build.Dense(grid, half, (r, c) => Math.Cos(Math.PI * ((double)r / (grid - 1)) * c));
        var target = Vector<double>.Build.Dense(grid);
        var weight = Vector<double>.Build.Dense(grid);
        for (int r = 0; r < grid; r++) {
            (double desired, double w) = spec.Band.Ideal((double)r / (grid - 1), c1, c2, transition, passWeight, stopWeight);
            target[r] = desired; weight[r] = w;
        }
        Fin<Vector<double>> last = Fin.Fail<Vector<double>>(ComputeFault.Create("<remez-no-iteration>"));
        for (int iter = 0; iter < 16; iter++) {
            var aw = Matrix<double>.Build.Dense(grid, half, (r, c) => Math.Sqrt(weight[r]) * design[r, c]);
            var bw = Vector<double>.Build.Dense(grid, r => Math.Sqrt(weight[r]) * target[r]);
            last = DenseRoute.Solve(new FactorRoute.Orthonormal(aw, QRMethod.Thin, Modified: false), bw, TolerancePolicy.Derive(aw, bw));
            if (last.IsFail) { return last.Map(static _ => default(FilterCoefficients)!); }
            Vector<double> response = design * last.IfFail(Vector<double>.Build.Dense(half));
            for (int r = 0; r < grid; r++) { weight[r] = weight[r] <= 0.0 ? 0.0 : Math.Max(1e-6, Math.Abs(response[r] - target[r])); }   // Lawson reweight over active bands; the transition stays don't-care.
        }
        return last.Map(c => Symmetric(c, taps));

        static FilterCoefficients Symmetric(Vector<double> halfBand, int taps) {
            int mid = taps / 2;
            var b = new double[taps];
            for (int i = 0; i < taps; i++) { int k = Math.Abs(i - mid); b[i] = halfBand[k] * (k == 0 ? 1.0 : 0.5); }   // cosine-series center tap a₀, off-center aₖ/2.
            return new FilterCoefficients(FilterDesign.FirRemez, b, new double[] { 1.0 });
        }
    }

    // One bilinear map for every IIR family AND band: pre-warp the band edge(s), let the `FilterBand` row
    // frequency-transform the normalized-lowpass prototype to the LP/HP/BP/BS s-plane (the lp2{lp,hp,bp,bs}
    // zpk algebra the band owns — BP/BS double the order, HP adds origin zeros, BS adds ±jω₀ notch zeros),
    // bilinear-transform each s-plane root to z, and expand the conjugate-paired roots to real (b, a)
    // polynomials. Per-family variation is the analog Prototype; per-band variation is `FilterBand.ToAnalog`.
    internal static FilterCoefficients Bilinear(FilterDesign design, FilterSpec spec) {
        var proto = design.Prototype(spec);
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
                var next = new Complex[poly.Length + 1];
                for (int i = 0; i < poly.Length; i++) { next[i] += poly[i]; next[i + 1] -= poly[i] * r; }
                poly = next;
            }
            return [.. poly.Select(c => c.Real * scale)];
        }
    }

    // --- [HELPERS]

    internal static int NextPow2(int n) { int p = 1; while (p < n) { p <<= 1; } return p; }
    static double BinHz(int length, double sampleRate) => Fourier.FrequencyScale(length, sampleRate) is [_, double second, ..] ? second : sampleRate / length;
}

// --- [COMPOSITION] ----------------------------------------------------------------------

// Closed-form analog normalized-lowpass prototypes (cutoff Ω=1 rad/s); Bilinear frequency-transforms and
// discretizes them. Butterworth places poles on the unit circle; Chebyshev on an ellipse; elliptic solves the
// degree equation by the Landen `Sne` product and places the equiripple pole/zero lattice via the Jacobi sn/cd.
public static class Prototypes {
    public static (Complex[] Zeros, Complex[] Poles, double Gain) Butterworth(FilterSpec spec) {
        int n = spec.Order;
        var poles = new Complex[n];
        for (int k = 0; k < n; k++) { double theta = Math.PI * (2.0 * k + 1.0 + n) / (2.0 * n); poles[k] = new Complex(Math.Cos(theta), Math.Sin(theta)); }
        return ([], poles, 1.0);
    }

    public static (Complex[] Zeros, Complex[] Poles, double Gain) Chebyshev1(FilterSpec spec) {
        int n = spec.Order;
        double eps = Math.Sqrt(Math.Pow(10.0, spec.RippleDb / 10.0) - 1.0), mu = Math.Asinh(1.0 / eps) / n;
        var poles = new Complex[n];
        for (int k = 0; k < n; k++) {
            double theta = Math.PI * (2.0 * k + 1.0) / (2.0 * n);
            poles[k] = new Complex(-Math.Sinh(mu) * Math.Sin(theta), Math.Cosh(mu) * Math.Cos(theta));
        }
        double gain = Product(poles);                          // leading gain ∏(−pₖ); even order drops by 1/√(1+ε²) to the ripple floor.
        return ([], poles, (n & 1) == 1 ? gain : gain / Math.Sqrt(1.0 + eps * eps));

        static double Product(Complex[] ps) { Complex p = Complex.One; foreach (Complex x in ps) { p *= -x; } return p.Real; }
    }

    public static (Complex[] Zeros, Complex[] Poles, double Gain) Chebyshev2(FilterSpec spec) {
        int n = spec.Order;
        double eps = 1.0 / Math.Sqrt(Math.Pow(10.0, spec.StopbandDb / 10.0) - 1.0), mu = Math.Asinh(1.0 / eps) / n;
        var poles = new Complex[n];
        var zeros = new Complex[n - n % 2];
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

    // Elliptic (Cauer) prototype: equiripple in both bands, the densest IIR prototype sharing the one Bilinear
    // discretizer. The εp/εs selectivity sets the passband modulus k through the Landen `Sne`-product degree
    // equation; the normalized Jacobi `Sne`/`Cde` (ascending Landen) place the imaginary-axis zeros, the
    // complex poles, and the odd-order real pole; the `v0` offset is the algebraic arc-form `Asne`. The pole
    // lattice is verified equal to the textbook analog Cauer prototype (poles in the LHP, equiripple stopband).
    public static (Complex[] Zeros, Complex[] Poles, double Gain) Elliptic(FilterSpec spec) {
        int n = spec.Order, half = n / 2, parity = n & 1;
        double gp = Math.Pow(10.0, -spec.RippleDb / 20.0);                                  // passband gain Gp.
        double eps = Math.Sqrt(Math.Pow(10.0, spec.RippleDb / 10.0) - 1.0);                 // passband ripple εp.
        double k1 = eps / Math.Sqrt(Math.Pow(10.0, spec.StopbandDb / 10.0) - 1.0);          // εp/εs selectivity.
        double k = DegreeModulus(k1, n);                                                    // passband modulus.
        double v0 = (-Complex.ImaginaryOne * Asne(new Complex(0.0, 1.0 / eps), k1) / n).Real;
        var poles = new Complex[n];
        var zeros = new Complex[2 * half];
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
        // Jacobi cd/sn in the normalized argument (uK over the quarter period), built by the ASCENDING Landen
        // recursion w ← (1+kₙ)·w/(1+kₙ·w²) from the descending modulus ladder seeded at sin(u·π/2) (the k=0 value).
        static Complex Cde(Complex u, double k) => Sne(Complex.One - u, k);
        static Complex Sne(Complex u, double k) {
            Span<double> v = stackalloc double[8];
            Landen(k, v);
            Complex w = Complex.Sin(u * (Math.PI / 2.0));
            for (int i = v.Length - 1; i >= 0; i--) { w = (1.0 + v[i]) * w / (1.0 + v[i] * w * w); }
            return w;
        }
        // Inverse sn: undo each ascending step (last-applied first) by the algebraic quadratic root, then refine
        // by Newton from that BOUNDED seed — the seed keeps u small so `Sne`'s `Complex.Sin` never overflows.
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

- [IIR_BILINEAR]: every IIR row grounds the analog-prototype-to-digital map on ONE `Bilinear` consuming the row's closed-form `Prototype` — Butterworth poles on the unit circle, Chebyshev-I all-pole on an ellipse via `asinh(1/ε)/n` carrying the `∏(−pₖ)` leading gain, Chebyshev-II reciprocal poles with imaginary-axis zeros carrying the `∏(−pₖ)/∏(−zₖ)` gain, and the elliptic equiripple lattice via the descending-Landen Jacobi `sn`/`cd` (the degree equation solved by the Landen `sne` product, the `v0` offset by the algebraic arc-form `asne`, the leading gain `Gpᵖᵃʳⁱᵗʸ⁻¹·∏(−pₖ)/∏(−zₖ)`); the recurrence consumes whatever `(b, a)` the bilinear expansion produces and the conjugate-paired-root polynomial expansion guarantees the real coefficient output. The Jacobi kernel is the ascending-Landen `sne` seeded at `sin(u·π/2)` (the modulus-zero value) and its descending-Landen algebraic inverse `asne` Newton-refined from that bounded seed — never the unseeded Newton that overflows for a small-ripple `j/ε` argument; the pole/zero set is verified equal to the textbook analog Cauer prototype, so a higher elliptic order only deepens the Landen ladder.
- [REMEZ_EXCHANGE]: the equiripple `fir-remez` row grounds the Lawson weighted-least-squares iteration over the `Tensor/blas#DENSE_ALGEBRA` `DenseRoute.Solve(FactorRoute.Orthonormal)` thin-QR Chebyshev route — the `FilterBand`-supplied band-aware cosine design matrix is solved across an EXPLICIT transition (don't-care) band under the ripple-derived `δp/δs` passband-vs-stopband weight, the error envelope reweights the active bands, and the iteration drives toward the minimax equioscillation; the transition band is load-bearing — a hard `0/1` brick-wall target with uniform weight Gibbs-rings (≈50% passband ripple) rather than converging, the deleted naive form. The windowed-sinc `fir-window` row rides the `WindowKind` `MathNet.Numerics.Window` factory directly with the `FilterBand` combine (spectral inversion for high-pass, kernel difference for band-pass/stop). The open leaf is the exact alternation-set convergence proof, grounded against the Chebyshev equioscillation theorem at the filter-design gate.
- [DWT_CASCADE]: the discrete wavelet transform `dwt` row grounds the filter-bank decomposition on the same stride-2 `Tensor/factor#KERNEL_LOWERING` `Conv1D` lowering the FIR application rides — the `WaveletFamily` carries the frozen Daubechies/symlet/coiflet analysis scaling coefficients (MathNet ships no wavelet surface) and the analysis high-pass derives by the QMF mirror `g[k] = (−1)ᵏ·h[N−1−k]`, so one frozen table per family owns the whole orthonormal bank. The open leaf is the boundary mode — the `Conv1D` lowering zero-pads, while a symmetric/periodic extension preserves perfect reconstruction at the signal edges; the synthesis (inverse-DWT) bank derives from the same analysis table by time-reversal and grounds at the reconstruction gate.
