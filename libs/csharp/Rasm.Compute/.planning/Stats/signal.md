# [COMPUTE_SIGNAL]

Rasm.Compute signal/spectral lane: one `SpectralTransform` `[SmartEnum<string>]` frequency-domain axis whose rows carry transform and inverse delegates, folding to deferred `Transform.Apply` and `Transform.Invert` surfaces. `IO<Fin<T>>` preserves the outer lowering effect and inner domain fault without forcing either inside the numeric lane. Forward and inverse share one surface: `stft` preserves frame phase and overlap-add evidence, while evidence-destroying `spectrogram` and averaged periodograms return typed inverse faults. One `FilterDesign` `[SmartEnum<string>]` axis closes each IIR row's analog prototype into the shared `Bilinear` map, while FIR rows fold to windowed-sinc taps or a true Remez exchange over a barycentric levelled interpolant.

Per-bin transforms ride `MathNet.Numerics.IntegralTransforms.Fourier` over split `double[]` planes, windowing rides `MathNet.Numerics.Window`, dependence and density rows ride `MathNet.Numerics.Statistics`, magnitude and phase read `TensorPrimitives.Hypot`/`Atan2`, bin spacing reads `Fourier.FrequencyScale`, FFT overlap-add rides `Tensor/dispatch#KERNEL_DISPATCH` `ComplexZip(TensorOpFamily.Multiply)`, and FIR convolution with wavelet analysis and synthesis rides `Tensor/factor#KERNEL_LOWERING` `Conv1D`.

The Remez seeding pass crosses to `Tensor/blas#DENSE_ALGEBRA`; the per-bin complex Hermitian dominant pair stays page-local under a convergence witness because the dense owner is real-typed; `ComputeFault` and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies instants — the App-owned `ClockPolicy` stays at composition. Spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` and `Modal` own measured-mode identification and `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating end; conditioned signals feed those estimators and the twin.

## [01]-[INDEX]

- [02]-[SIGNAL_LANE]: effect-preserving FFT/STFT/PSD/wavelet `Transform` over MathNet Fourier + Window, paired row-owned inversion, Welch cross-spectral coherence, N-channel FDD measured-mode extraction, amplitude-domain dependence and nonparametric distribution description, and family-shaped FIR/IIR design over one band-aware `Bilinear` map with `Conv1D`, pooled FFT overlap-add, direct-form-II recurrence, and magnitude/phase/group-delay response.

## [02]-[SIGNAL_LANE]

- Owner: `SpectralTransform` `[SmartEnum<string>]` rows each carry a `Windowed` admission discriminant, a row-owned `SignalPolicy → Fin<SignalContext>` admission delegate, a `(ReadOnlyMemory<float>, SignalContext, Instant) → IO<Fin<SpectralOutput>>` transform delegate, and a `(SpectralOutput) → IO<Fin<ReadOnlyMemory<float>>>` inverse delegate — no transport on either column, because the `dwt` bank's convolutions lower against the `Tensor/factor#KERNEL_LOWERING` `ShardDispatch.Local` a local bank spells and the Fourier rows lower nothing at all. `SignalPolicy` `[Union]` closes `PerBin`, `Framed`, and `Wavelet` evidence; `SignalContext` exists only after row admission. `WaveletExtension` rows carry one boundary-extension delegate and the per-family admission that proves the extension reconstructs; `ExtensionClass` types the symmetry point a family's zero-stripped core admits. `FilterShape` `[Union]` closes `Windowed`, `Equiripple`, `Butterworth`, `Chebyshev1`, `Chebyshev2`, and `Elliptic` parameter evidence; its `Design` projection selects the corresponding `FilterDesign` row, so no caller supplies a reconstructible design knob. `FilterDesign` rows carry the sole `Recursive` result discriminator and admitted design delegate. `SpectralOutput` owns `Bins`/`Frames`/`Bands`; `Spectrogram` owns inverse-sufficient `Phasor` frames and magnitude-only `Power` frames. `FilterCoefficients` admits invariants before `Apply`, `Response`, or `ZeroPhase`; `FilterResponse` carries magnitude, unwrapped phase, and group delay. `DependenceKind` rows carry both arities of one co-variation measure and `DensityKernel` rows carry one taper each, while `ChannelQuery` closes the description request and `ChannelEvidence` its two result shapes.
- Cases: `SpectralTransform` fft · rfft · stft · spectrogram · welch-psd · dwt; `SignalPolicy` per-bin · framed · wavelet; `DependenceKind` pearson · spearman; `DensityKernel` gaussian · epanechnikov · uniform · triangular; `ChannelQuery` dependence · distribution; `FilterShape` windowed · equiripple · butterworth · chebyshev1 · chebyshev2 · elliptic; `FilterDesign` fir-window · fir-remez · iir-butterworth · iir-chebyshev1 · iir-chebyshev2 · iir-elliptic; `FilterBand` low-pass · high-pass · band-pass · band-stop; `WindowKind` hann · hamming · blackman · blackman-harris · blackman-nuttall · nuttall · flat-top · bartlett · bartlett-hann · cosine · lanczos · triangular · gauss · tukey · rectangular; `WaveletFamily` haar · db2 · db4 · sym4 · coif1; `WaveletExtension` zero · periodic · symmetric; `ExtensionClass` none · whole-point · half-point.
- Entry: `Transform.Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, IClock clock) → IO<Fin<SpectralOutput>>` composes sample admission with row policy admission, then dispatches over `SignalContext`. `Invert(SpectralOutput output) → IO<Fin<ReadOnlyMemory<float>>>` projects and dispatches the owning row. `Coherence(ReadOnlyMemory<float> x, ReadOnlyMemory<float> y, SignalPolicy policy, IClock clock)` composes synchronous-channel admission and a two-segment floor with the `welch-psd` row's `Framed` admission. `Modal(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock)` runs the N-channel frequency-domain decomposition over the same Welch admission — per-bin Hermitian cross-PSD matrices, dominant singular pair by power iteration, first-singular-value peak picking with half-power damping — returning the `ModalEstimate` measured-mode set. `Describe(Seq<ReadOnlyMemory<float>> channels, ChannelQuery query, IClock clock)` admits the same synchronous channel set under the query's own arity floor and folds either the all-pairs dependence matrix or the per-channel kernel density with its empirical CDF, entropy, and interquartile range on one shared support grid. `Design(FilterSpec spec, DenseSubstrate substrate)` projects `FilterShape.Design`, admits `FilterContext`, dispatches the row, and admits emitted coefficients, the substrate reaching the equiripple seed's least-squares leg alone. `FilterCoefficients.Apply(ReadOnlyMemory<float> signal) → IO<Fin<float[]>>`, `Response`, and `ZeroPhase` enter through coefficient admission.
- Auto: each `SpectralTransform` `Kernel` realizes one split-plane `Fourier` transform: `fft` full-length, `rfft` Hermitian half-spectrum, `stft` centered magnitude/phase frames over a recorded coverage extent, `spectrogram` squared STFT magnitudes, `welch-psd` averaged periodograms, and `dwt` a stride-2 `Conv1D` QMF cascade under the policy's own `WaveletExtension`. Each `Invert` realizes the paired inverse: `fft`/`rfft` reciprocal transforms, `stft` weighted overlap-add trimmed to the recorded coverage, and `dwt` zero-stuff synthesis at the mirrored shift, trimmed to recorded extents. `Design` produces windowed-sinc taps, a Remez-exchanged equiripple half-band, or shared-`Bilinear` IIR coefficients. `FilterCoefficients.Apply` routes short-FIR `Conv1D`, pooled long-FIR FFT overlap-add, or direct-form-II-transposed IIR recurrence.
- Receipt: the spectral fold mints no hot-path receipt. Evidence rides `Spectrum.Length`/`BinHz`/`Samples`/`Scaling`, each `Spectrogram` case's `Frames`/`Bins`, `WaveletDecomposition.Levels`/`Extents`, and `CrossSpectrum.Coherence`. Tensor-lowered legs compose the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.TensorRun(Family, Dtype, Elements, SimdWidth, Partitions)` their owning operation stamps. Bare `Fourier` transforms and IIR recurrence mint no fabricated tensor receipt.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new spectral transform is one `SpectralTransform` row binding admission, forward kernel, and inverse; a new window is one `WindowKind` row resolving its own published shape default; a new wavelet is one `WaveletFamily` row whose linear-phase and extension class DERIVE from its coefficients, so its admissible extensions follow with no roster edit; a new boundary mode is one `WaveletExtension` row; a new co-variation measure is one `DependenceKind` row carrying both arities and a new density taper one `DensityKernel` row, while a new description modality is one `ChannelQuery` case with its `ChannelEvidence` twin. Each new filter family adds one parameter-evidence `FilterShape` case and its projected `FilterDesign` row, closing any analog prototype into unchanged `Bilinear`. `FftTransform`/`StftTransform`/`PsdEstimator` collapse onto `Transform.Apply`; inverse siblings collapse onto `Transform.Invert`; FIR/IIR classes collapse onto `FilterCoefficients`; per-family designers collapse onto `FilterShape` and the row-owned design delegate.
- Boundary: `MathNet.Numerics.IntegralTransforms.Fourier` operates in place. Per-bin and framed kernels apply recorded `FourierOptions`; overlap-add and Welch pin `FourierOptions.NoScaling` and own their normalization — the kernel `Numerics/matrix#TRANSFORM_BAND` `SpectralScaling` roster names those same three conventions and its `Unscaled.RoundTrip(cells)` states the factor this lane divides back out, so the convention vocabulary agrees across the strata boundary while the option value stays the RECORDED EVIDENCE here: `FourierOptions` is what `Spectrum.Scaling` and `Spectrogram.Phasor.Scaling` hand the reciprocal leg and what every MathNet entrypoint consumes.
- Boundary: the kernel's `SpectralArena` cases own their buffers, where this lane transforms split `double[]` planes and pooled per-frame windows in place under its own framing, so composing the arena re-buffers every frame of an overlap-add for a transform this lane already performs. Split real/imaginary arrays let `TensorPrimitives.Hypot`/`Atan2` read contiguous spans; `ForwardReal`/`InverseReal` own packed half-spectra; `Fourier.FrequencyScale` owns bin resolution.
- Boundary: inversion consumes forward evidence and nothing else. `Spectrum` records `Samples` and `Scaling`; `Spectrogram.Phasor` records `Samples`, `FrameSize`, `Hop`, `Window`, `Scaling`, the half-open COVERAGE EXTENT the frame grid actually spans, magnitude, and phase; `Spectrogram.Power` and Welch output typed inverse faults; `WaveletDecomposition` records per-level `Extents` and the `WaveletExtension` its cascade ran under.
- Boundary: an overlap-add's first and last half-frame carry partial window mass by construction, so synthesis normalizes and TRIMS to the recorded extent rather than refusing the whole inversion over an edge the forward pass already measured — the same evidence-driven law the wavelet `Extents` trim rides, and a coverage floor that fails a signal whose interior reconstructs exactly is the deleted form.
- Boundary: window functions ride `MathNet.Numerics.Window`; periodic forms apply only where MathNet exposes them, and a shape-bearing row resolves the caller's `Option<double>` against its own published default rather than freezing one σ or α into the row.
- Boundary: IIR feedback routes to direct-form-II-transposed after finite, nonempty, nonzero-`a₀` admission. FIR application routes short taps through `KernelLowering`, while long taps accumulate through pooled `MemoryOwner<float>` storage under FFT overlap-add. `FilterResponse` derives group delay from unwrapped phase. One band-aware `Bilinear` consumes closed-over analog prototypes and owns the tangent pre-warp's own domain, so a normalized edge outside `(0, 1)` refuses there rather than reaching `Math.Tan` at a pole.
- Boundary: the equiripple row is a TRUE second Remez exchange, not a weighted-least-squares reweighting: Lawson's iteration converges at a linear rate whose ratio approaches one on dense grids, absorbs a zero-weight transition band into its own reweighting, and has published counterexamples where it converges to the wrong function — which is why scipy and MATLAB both ship Remez. The `FirRemez` key names the exchange the row runs.
- Boundary: `WaveletFamily` owns scaling tables because MathNet exposes no wavelet surface; linear phase and extension class are DERIVED from those tables under an exact symmetry compare, never asserted per row, and each `WaveletExtension` admits against the derived descriptor so the admissible set follows a new row automatically.
- Boundary: `Describe` is the amplitude-domain half of the same channel evidence: it re-runs no transform, derives its bandwidth from the channel's own spread when the query names none, and reads the empirical CDF and quartiles off one ascending copy because both members contract on sorted data.
- Boundary: spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` conditions channel pairs and `Modal` extracts the measured modes — the FDD first-singular-value spectrum is an operational estimate whose peaks are honest only where excitation is broadband, so `ModalEstimate` carries the full singular spectrum beside its picked modes and a consumer re-judges a peak against its own floor; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end, and conditioned signals feed learning and the twin.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
// The shape slot is the caller's `Option<double>` and each row resolves it against its OWN published default,
// so a shapeless row discards the slot and a shape-bearing one is tunable without a second entrypoint —
// where a frozen row constant left the σ and α that define those two windows unreachable from any policy.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindowKind {
    public static readonly WindowKind Hann           = new("hann",            static (w, p, _) => p ? Window.HannPeriodic(w) : Window.Hann(w));
    public static readonly WindowKind Hamming        = new("hamming",         static (w, p, _) => p ? Window.HammingPeriodic(w) : Window.Hamming(w));
    public static readonly WindowKind Cosine         = new("cosine",          static (w, p, _) => p ? Window.CosinePeriodic(w) : Window.Cosine(w));
    public static readonly WindowKind Lanczos        = new("lanczos",         static (w, p, _) => p ? Window.LanczosPeriodic(w) : Window.Lanczos(w));
    public static readonly WindowKind Blackman       = new("blackman",        static (w, _, _) => Window.Blackman(w));
    public static readonly WindowKind BlackmanHarris = new("blackman-harris", static (w, _, _) => Window.BlackmanHarris(w));
    public static readonly WindowKind BlackmanNuttall= new("blackman-nuttall",static (w, _, _) => Window.BlackmanNuttall(w));
    public static readonly WindowKind Nuttall        = new("nuttall",         static (w, _, _) => Window.Nuttall(w));
    public static readonly WindowKind FlatTop        = new("flat-top",        static (w, _, _) => Window.FlatTop(w));
    public static readonly WindowKind Bartlett       = new("bartlett",        static (w, _, _) => Window.Bartlett(w));
    public static readonly WindowKind BartlettHann   = new("bartlett-hann",   static (w, _, _) => Window.BartlettHann(w));
    public static readonly WindowKind Triangular     = new("triangular",      static (w, _, _) => Window.Triangular(w));
    public static readonly WindowKind Gauss          = new("gauss",           static (w, _, s) => Window.Gauss(w, s.IfNone(0.4)));   // sigma is RELATIVE to the half-width ((i−c)/(σ·c)); an absolute sigma flattens the taper to rectangular.
    public static readonly WindowKind Tukey          = new("tukey",           static (w, _, s) => Window.Tukey(w, s.IfNone(0.5)));   // alpha is the cosine-tapered FRACTION; 0 is rectangular and 1 is Hann.
    public static readonly WindowKind Rectangular    = new("rectangular",     static (w, _, _) => Window.Dirichlet(w));

    private readonly Func<int, bool, Option<double>, double[]> taper;

    public double[] Taper(int width, bool periodic, Option<double> shape = default) => taper(width, periodic, shape);
}

// Which symmetry point a family's ZERO-STRIPPED core reflects about: a whole-point core repeats its edge sample
// under reflection, a half-point core mirrors between samples and needs an even signal length, and a family
// with no exact symmetry admits neither.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExtensionClass {
    public static readonly ExtensionClass None = new("none");
    public static readonly ExtensionClass WholePoint = new("whole-point");
    public static readonly ExtensionClass HalfPoint = new("half-point");
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

    // DERIVED from the coefficients, never asserted on the row: a family cannot claim a symmetry its own table
    // does not have. The compare is EXACT — coif2's core is symmetric to 3.1e-2 and reconstructs through a
    // symmetric extension with an error of 1.7e0, so any tolerance here admits a bank that does not reconstruct.
    public bool LinearPhase => Mirrored(Core(scaling)) && Mirrored(Core(HighPass()));

    // Parity reads the ZERO-STRIPPED core, never the declared length: the CDF 9/7 bank ships `dec_len` 10 with
    // cores of 9 and 7, so a `Length & 1` test on the padded table classifies a whole-point bank as half-point
    // and reflects it about the wrong sample.
    public ExtensionClass Extension =>
        !LinearPhase ? ExtensionClass.None
        : (Core(scaling).Length & 1) == 1 ? ExtensionClass.WholePoint
        : ExtensionClass.HalfPoint;

    // Leading and trailing exact zeros are padding, not taps; the core is what carries the symmetry.
    private static ReadOnlySpan<double> Core(ReadOnlySpan<double> taps) {
        int lo = 0, hi = taps.Length - 1;
        while (lo <= hi && taps[lo] == 0.0) { lo++; }
        while (hi >= lo && taps[hi] == 0.0) { hi--; }
        return lo > hi ? [] : taps[lo..(hi + 1)];
    }

    // Symmetric OR antisymmetric — the high-pass of a linear-phase bank is the antisymmetric half of the pair.
    private static bool Mirrored(ReadOnlySpan<double> core) {
        bool symmetric = true, antisymmetric = true;
        for (int k = 0; k < core.Length; k++) {
            symmetric &= core[k] == core[^(k + 1)];
            antisymmetric &= core[k] == -core[^(k + 1)];
        }
        return symmetric || antisymmetric;
    }
}

// One boundary law per row: how the cascade extends its input, and which families that extension reconstructs.
// Admission is DERIVED from the family's own coefficients, so a new `WaveletFamily` row inherits its admissible
// extension set with no roster edit here and no row can claim a mode its filters do not satisfy.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveletExtension {
    // Zero-padding corrupts exactly L−2 samples at each boundary — the taps that reach past the edge — so only
    // the length-2 bank (Haar, which reaches nothing) reconstructs exactly. The interior is exact for every
    // family, and that bound is the whole reason a longer bank refuses rather than silently degrading its edges.
    public static readonly WaveletExtension Zero = new("zero",
        extend: static (x, left, right) => {
            double[] z = new double[left + x.Length + right];
            x.CopyTo(z.AsSpan(left));
            return z;
        },
        admit: static family => family.LowPass.Length == 2);
    // Circular extension reconstructs for EVERY bank and stays isometric for an orthogonal one, which is why it
    // is the roster-wide default: the analysis operator is a unitary circulant and its adjoint is its inverse.
    public static readonly WaveletExtension Periodic = new("periodic",
        extend: static (x, left, right) => {
            double[] z = new double[left + x.Length + right];
            for (int t = 0; t < z.Length; t++) { z[t] = x[((t - left) % x.Length + x.Length) % x.Length]; }
            return z;
        },
        admit: static _ => true);
    // Whole-point (WS) reflection for an odd core, half-point (HS) for an even one. Perfect reconstruction under
    // reflection needs a LINEAR-PHASE bank, and the only linear-phase orthogonal bank is Haar — a biorthogonal
    // family admits it, an orthogonal Daubechies/symlet/coiflet does not.
    public static readonly WaveletExtension Symmetric = new("symmetric",
        extend: static (x, left, right) => {
            double[] z = new double[left + x.Length + right];
            int period = 2 * x.Length - 2;
            for (int t = 0; t < z.Length; t++) {
                int folded = ((t - left) % period + period) % period;
                z[t] = x[folded < x.Length ? folded : period - folded];
            }
            return z;
        },
        admit: static family => family.Extension != ExtensionClass.None);

    private readonly Func<double[], int, int, double[]> extend;
    private readonly Func<WaveletFamily, bool> admit;

    internal double[] Extend(double[] x, int left, int right) => extend(x, left, right);

    internal Fin<Unit> Admit(WaveletFamily family, int samples) =>
        !admit(family)
            ? Fin.Fail<Unit>(ComputeFault.Create($"<dwt-extension:{Key}:{family.Key}>"))
        : this == Symmetric && family.Extension == ExtensionClass.HalfPoint && (samples & 1) == 1
            ? Fin.Fail<Unit>(ComputeFault.Create($"<dwt-half-point-parity:{samples}>"))
            : Fin.Succ(unit);
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

    // Neither directional column names transport: the `dwt` bank convolves through `KernelLowering` under the
    // `ShardDispatch.Local` a local bank spells at its own call, and the Fourier rows lower nothing — so a
    // carrier no row reads never enters the signature.
    internal IO<Fin<SpectralOutput>> Run(ReadOnlyMemory<float> signal, SignalContext context, Instant at) => kernel(signal, context, at);

    public IO<Fin<ReadOnlyMemory<float>>> Invert(SpectralOutput output) => invert(output);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterDesign {
    // Each IIR row closes its analog prototype into `Run`; every new row must bind its design without nullable columns or throwing accessors.
    public static readonly FilterDesign FirWindow      = new("fir-window",      recursive: false, static (_, spec, _) => Fin.Succ(Transform.WindowedSinc(spec)));
    public static readonly FilterDesign FirRemez       = new("fir-remez",       recursive: false, static (_, spec, substrate) => Transform.Equiripple(spec, substrate));
    public static readonly FilterDesign IirButterworth = new("iir-butterworth", recursive: true,  static (d, spec, _) => spec.Prewarp().Map(warped => Transform.Bilinear(d, Transform.Butterworth, warped)));
    public static readonly FilterDesign IirChebyshev1  = new("iir-chebyshev1",  recursive: true,  static (d, spec, _) => spec.Prewarp().Map(warped => Transform.Bilinear(d, Transform.Chebyshev1, warped)));
    public static readonly FilterDesign IirChebyshev2  = new("iir-chebyshev2",  recursive: true,  static (d, spec, _) => spec.Prewarp().Map(warped => Transform.Bilinear(d, Transform.Chebyshev2, warped)));
    public static readonly FilterDesign IirElliptic    = new("iir-elliptic",    recursive: true,  static (d, spec, _) => spec.Prewarp().Map(warped => Transform.Bilinear(d, Transform.Elliptic, warped)));

    // The dense substrate arrives as a VALUE from composition on every row because the equiripple seed solves a
    // real least-squares system through `Tensor/blas#DENSE_ALGEBRA`; the closed-form rows discard it, so the
    // serving leg is declared in the signature rather than read off ambient state.
    private readonly Func<FilterDesign, FilterContext, DenseSubstrate, Fin<FilterCoefficients>> design;

    public bool Recursive { get; }

    internal Fin<FilterCoefficients> Run(FilterSpec spec, DenseSubstrate substrate) => spec.Admit().Bind(context => design(this, context, substrate));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterBand {
    // One band owns all three modalities as row data — the FIR windowed-sinc combine, the analog→s-plane zpk transform the Bilinear consumes, and the equiripple desired/weight at a normalized frequency.
    // Each new band (all-pass, shelving) is one row every consumer's delegate dispatch breaks on at compile time; `Upper` reports whether the band reads the second edge (BP/BS).
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
    private static IEnumerable<Complex> Origin(int count) { for (int i = 0; i < count; i++) { yield return Complex.Zero; } }
    private static IEnumerable<Complex> Notch(int count, double w0) { for (int i = 0; i < count; i++) { yield return new Complex(0.0, w0); yield return new Complex(0.0, -w0); } }
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

// Dependence rows own BOTH arities of one measure: `Pairwise` scores two channels, `AllPairs` scores every pair in one
// library call. Rank correlation is the monotone measure the linear one cannot express, so a sensor relation that
// saturates or squares reads honestly on `spearman` where `pearson` reports a weak linear slope.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DependenceKind {
    public static readonly DependenceKind Pearson = new("pearson", Correlation.Pearson, static rows => Correlation.PearsonMatrix(rows));
    public static readonly DependenceKind Spearman = new("spearman", Correlation.Spearman, static rows => Correlation.SpearmanMatrix(rows));

    private readonly Func<IEnumerable<double>, IEnumerable<double>, double> pairwise;
    private readonly Func<IEnumerable<double[]>, Matrix<double>> allPairs;

    internal double Pairwise(double[] a, double[] b) => pairwise(a, b);

    internal Matrix<double> AllPairs(Seq<double[]> channels) => allPairs(channels);
}

// Kernel rows differ only in taper, so one estimate call carries the row; bandwidth is a shared axis riding the
// query rather than a per-row constant no caller can move.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DensityKernel {
    public static readonly DensityKernel Gaussian = new("gaussian", KernelDensity.EstimateGaussian);
    public static readonly DensityKernel Epanechnikov = new("epanechnikov", KernelDensity.EstimateEpanechnikov);
    public static readonly DensityKernel Uniform = new("uniform", KernelDensity.EstimateUniform);
    public static readonly DensityKernel Triangular = new("triangular", KernelDensity.EstimateTriangular);

    private readonly Func<double, double, IList<double>, double> estimate;

    internal double Estimate(double at, double bandwidth, IList<double> samples) => estimate(at, bandwidth, samples);
}

// --- [MODELS] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FilterShape {
    private FilterShape() { }

    public sealed record Windowed(WindowKind Window, Option<double> Shape) : FilterShape;
    // `Budget.Tolerance` IS the Remez acceptance ε_t — the de la Vallée Poussin ratio bound, not a coefficient
    // distance — so a caller tightening the tolerance tightens the levelling the design is accepted on.
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
        windowed: static (state, shape) => state.Context(shape.Window, shape.Shape, rippleDb: 1.0, stopbandDb: 1.0, FitBudget.Canonical),
        equiripple: static (state, shape) => state.Context(WindowKind.Rectangular, None, shape.RippleDb, shape.StopbandDb, shape.Budget),
        butterworth: static (state, _) => state.Context(WindowKind.Rectangular, None, rippleDb: 1.0, stopbandDb: 1.0, FitBudget.Canonical),
        chebyshev1: static (state, shape) => state.Context(WindowKind.Rectangular, None, shape.RippleDb, stopbandDb: 1.0, FitBudget.Canonical),
        chebyshev2: static (state, shape) => state.Context(WindowKind.Rectangular, None, rippleDb: 1.0, shape.StopbandDb, FitBudget.Canonical),
        elliptic: static (state, shape) => state.Context(WindowKind.Rectangular, None, shape.RippleDb, shape.StopbandDb, FitBudget.Canonical));
}

public sealed record FilterSpec(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double SampleRate, FilterShape Shape) {
    public static readonly FilterSpec CanonicalLowPass = new(FilterBand.LowPass, Order: 4, Cutoff: 1000.0, UpperCutoff: 0.0, SampleRate: 48000.0, new FilterShape.Butterworth());

    public double Nyquist => SampleRate * 0.5;
    public double NormalizedCutoff => Cutoff / Nyquist;          // [0, 1) cutoff for the FIR sinc.
    public double NormalizedUpper => UpperCutoff / Nyquist;

    internal Fin<FilterContext> Admit() => Shape.Project(this);

    internal Fin<FilterContext> Context(WindowKind window, Option<double> shape, double rippleDb, double stopbandDb, FitBudget budget) =>
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
            : budget.Admit().Map(admitted => new FilterContext(Band, Order, Cutoff, UpperCutoff, rippleDb, stopbandDb, window, shape, SampleRate, admitted));
}

internal sealed record FilterContext(FilterBand Band, int Order, double Cutoff, double UpperCutoff, double RippleDb, double StopbandDb, WindowKind Window, Option<double> Shape, double SampleRate, FitBudget Budget) {
    internal double Nyquist => SampleRate * 0.5;
    internal double NormalizedCutoff => Cutoff / Nyquist;
    internal double NormalizedUpper => UpperCutoff / Nyquist;

    // The bilinear pre-warp is `2fs·tan(πf/2)`, which is a pole at f = 1 and a sign flip past it, so every IIR
    // row proves its normalized edges STRICTLY inside (0, 1) — and the upper edge strictly above the lower —
    // before a prototype is placed. A DC or Nyquist edge otherwise maps to a zero or infinite analog frequency
    // and the expanded polynomial comes back all-zero or all-NaN with no diagnostic naming the cause.
    internal Fin<FilterContext> Prewarp() =>
        NormalizedCutoff <= 0.0 || NormalizedCutoff >= 1.0
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<bilinear-prewarp-domain:{NormalizedCutoff}>"))
        : Band.Upper && (NormalizedUpper <= NormalizedCutoff || NormalizedUpper >= 1.0)
            ? Fin.Fail<FilterContext>(ComputeFault.Create($"<bilinear-prewarp-band:{NormalizedCutoff}..{NormalizedUpper}>"))
            : Fin.Succ(this);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SignalPolicy {
    private SignalPolicy() { }

    public sealed record PerBin(double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Framed(WindowKind Window, Option<double> Shape, int FrameSize, int HopSize, double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Wavelet(int Levels, WaveletFamily Family, WaveletExtension Extension) : SignalPolicy;

    public static readonly SignalPolicy CanonicalFft   = new PerBin(SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalStft  = new Framed(WindowKind.Hann, Shape: None, FrameSize: 1024, HopSize: 512, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalWelch = new Framed(WindowKind.Hann, Shape: None, FrameSize: 256, HopSize: 128, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalDwt   = new Wavelet(Levels: 4, WaveletFamily.Db4, WaveletExtension.Periodic);

    internal bool IsFramed => this is Framed;

    internal static Fin<SignalContext> AdmitPerBin(SpectralTransform transform, SignalPolicy policy, int _) =>
        policy is not PerBin perBin
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-policy:{transform}>"))
        : perBin.SampleRate <= 0.0 || !double.IsFinite(perBin.SampleRate)
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-sample-rate:{perBin.SampleRate}>"))
            : Fin.Succ(new SignalContext(transform, WindowKind.Rectangular, None, 0, 0, 0, perBin.SampleRate, perBin.Scaling, WaveletFamily.Haar, WaveletExtension.Periodic));

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
            : Fin.Succ(new SignalContext(transform, framed.Window, framed.Shape, framed.FrameSize, framed.HopSize, 0, framed.SampleRate, framed.Scaling, WaveletFamily.Haar, WaveletExtension.Periodic));

    // Periodization halves the length at every level, so a sample count not divisible by 2^levels leaves a level
    // with an odd-length parent whose circular extension is no longer its own inverse. The gate REFUSES there —
    // duplicating a sample to reach the next power fabricates evidence the synthesis would then trim as real.
    internal static Fin<SignalContext> AdmitWavelet(SpectralTransform transform, SignalPolicy policy, int samples) =>
        policy is not Wavelet wavelet
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-policy:{transform}>"))
        : wavelet.Levels < 1
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<signal-levels:{wavelet.Levels}>"))
        : wavelet.Extension == WaveletExtension.Periodic && samples % (1 << wavelet.Levels) != 0
            ? Fin.Fail<SignalContext>(ComputeFault.Create($"<dwt-periodization-length:{samples}/{wavelet.Levels}>"))
            : wavelet.Extension.Admit(wavelet.Family, samples).Map(_ => new SignalContext(
                transform, WindowKind.Rectangular, None, 0, 0, wavelet.Levels, 0.0, FourierOptions.Default, wavelet.Family, wavelet.Extension));

    private static long FrameCells(int samples, int frame, int hop) =>
        (1L + (samples + frame / 2L - 1L) / hop) * (frame / 2L + 1L);
}

internal sealed record SignalContext(
    SpectralTransform Transform, WindowKind Window, Option<double> Shape, int FrameSize, int HopSize, int Levels,
    double SampleRate, FourierOptions Scaling, WaveletFamily Wavelet, WaveletExtension Extension);

// `Samples` and `Scaling` preserve packed-rfft length and reciprocal-transform normalization.
public sealed record Spectrum(SpectralTransform Transform, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, int Length, int Samples, double BinHz, double SampleRate, FourierOptions Scaling, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Spectrogram {
    private Spectrogram() { }

    // `CoverFrom`/`CoverTo` is the half-open sample range the frame grid's accumulated window mass actually
    // covers, measured by the FORWARD pass off the frame geometry alone — no transform cost. Synthesis
    // normalizes and trims to it, so an edge whose window mass never reached the floor is excluded by recorded
    // evidence rather than failing the whole inversion; the wavelet `Extents` trim rides the identical law.
    public sealed record Phasor(SpectralTransform Transform, int Frames, int Bins, int Samples, int FrameSize, int Hop, int CoverFrom, int CoverTo, double BinHz, WindowKind Window, Option<double> Shape, FourierOptions Scaling, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, Instant At) : Spectrogram;
    public sealed record Power(SpectralTransform Transform, int Frames, int Bins, int Hop, double BinHz, ReadOnlyMemory<double> Values, Instant At) : Spectrogram;

    public SpectralTransform Transform => Switch(
        phasor: static value => value.Transform,
        power: static value => value.Transform);
}

// Extents records the approximation length consumed at each cascade level (head = original signal), because the
// stride-2 floors are not recoverable from coefficient lengths alone and the synthesis bank trims against them.
// `Extension` rides beside them for the same reason: the synthesis shift and the buffer the reconstruction
// extends are the analysis extension's mirror, and a synthesis guessing the mode reconstructs a different signal.
public sealed record WaveletDecomposition(WaveletFamily Family, WaveletExtension Extension, int Levels, ReadOnlyMemory<double> Approximation, Seq<ReadOnlyMemory<double>> Details, ImmutableArray<int> Extents, Instant At);

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

// One request closes the amplitude-domain descriptions the frequency-domain surfaces cannot answer: how channels
// co-vary, and what distribution one channel actually has where moments alone hide multimodality.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelQuery {
    private ChannelQuery() { }

    public sealed record Dependence(DependenceKind Kind) : ChannelQuery;
    public sealed record Distribution(DensityKernel Kernel, int Support, Option<double> Bandwidth) : ChannelQuery;

    internal int Floor => Switch(dependence: static _ => 2, distribution: static _ => 1);
}

// Support, density, and CDF share one grid so a consumer plots or integrates them without re-deriving abscissae;
// entropy and the interquartile range are the two scalar shape reads a moment pair cannot give.
public sealed record ChannelDistribution(
    ReadOnlyMemory<double> Support,
    ReadOnlyMemory<double> Density,
    ReadOnlyMemory<double> Cdf,
    double Bandwidth,
    double Entropy,
    double InterquartileRange);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelEvidence {
    private ChannelEvidence() { }

    public sealed record Dependence(DependenceKind Kind, Matrix<double> Correlations, Instant At) : ChannelEvidence;
    public sealed record Distribution(DensityKernel Kernel, Seq<ChannelDistribution> Channels, Instant At) : ChannelEvidence;
}

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

    // The lowering takes ONE `ShardDispatch`, and a filter convolution is in-process, so it spells `Local` and
    // names no transport. It reads the SOLUTION off the outcome — a local convolution decomposes nothing, so its
    // receipt roster is empty and a filter folding it would publish a factorization no route ran.
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
                        new ConvWindow([B.Length], [1], [B.Length / 2], [1], 1, 1, [signal.Length]), new ShardDispatch.Local())
                    .Map(result => result.Map(static outcome => outcome.Solution.ToColumnMajorArray().Select(static value => (float)value).ToArray()))
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
    public static Fin<FilterCoefficients> Design(FilterSpec spec, DenseSubstrate substrate) =>
        spec.Shape.Design.Run(spec, substrate).Bind(static coefficients => coefficients.Admit());

    // One amplitude-domain description entry over the SAME synchronous-channel admission the frequency-domain
    // surfaces use; the query's own case selects the fold, so a dependence measure and a distribution estimate never
    // grow sibling entrypoints.
    public static Fin<ChannelEvidence> Describe(Seq<ReadOnlyMemory<float>> channels, ChannelQuery query, IClock clock) =>
        Synchronous(channels, query.Floor).Bind(rows => query.Switch(
            state: (Rows: rows, At: clock.GetCurrentInstant()),
            dependence: static (s, q) => Fin.Succ<ChannelEvidence>(new ChannelEvidence.Dependence(q.Kind, q.Kind.AllPairs(s.Rows), s.At)),
            distribution: static (s, q) => q.Support < 2
                ? Fin.Fail<ChannelEvidence>(ComputeFault.Create($"<describe-support:{q.Support}>"))
                : Fin.Succ<ChannelEvidence>(new ChannelEvidence.Distribution(
                    q.Kernel, s.Rows.Map(row => Distribution(row, q)), s.At))));

    // Channels admit as one synchronous set exactly as `Coherence` admits its pair — equal length, finite, and at
    // least the query's own arity floor, so a single-channel dependence matrix is a refusal rather than a 1x1 one.
    private static Fin<Seq<double[]>> Synchronous(Seq<ReadOnlyMemory<float>> channels, int floor) =>
        channels.Count < floor
            ? Fin.Fail<Seq<double[]>>(ComputeFault.Create($"<describe-channels:{channels.Count}<{floor}>"))
        : channels.Exists(row => row.Length != channels[0].Length)
            ? Fin.Fail<Seq<double[]>>(ComputeFault.Create($"<describe-length-miss:{channels[0].Length}>"))
        : channels[0].Length < 2 || channels.Exists(static row => !TensorPrimitives.IsFiniteAll<float>(row.Span))
            ? Fin.Fail<Seq<double[]>>(ComputeFault.Create("<describe-admission>"))
            : Fin.Succ(channels.Map(static row => {
                double[] widened = new double[row.Length];
                for (int i = 0; i < widened.Length; i++) { widened[i] = row.Span[i]; }
                return widened;
            }));

    // Bandwidth derives from the channel's own spread through Silverman's rule when the query names none, so the
    // estimate reads a measured scale rather than a caller-asserted constant; the sorted copy serves the CDF and the
    // quartile reads at once, since both members contract on ascending data.
    private static ChannelDistribution Distribution(double[] samples, ChannelQuery.Distribution query) {
        double[] sorted = [.. samples.Order()];
        double bandwidth = query.Bandwidth.IfNone(() =>
            Math.Max(1e-12, 1.06 * Statistics.StandardDeviation(samples) * Math.Pow(samples.Length, -0.2)));
        double[] support = Generate.LinearSpaced(query.Support, sorted[0], sorted[^1]);
        return new ChannelDistribution(
            support,
            [.. support.Select(at => query.Kernel.Estimate(at, bandwidth, samples))],
            [.. support.Select(at => SortedArrayStatistics.EmpiricalCDF(sorted, at))],
            bandwidth,
            Statistics.Entropy(samples),
            SortedArrayStatistics.InterquartileRange(sorted));
    }

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
        double[] taper = policy.Window.Taper(frame, periodic: false, policy.Shape);
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
        double[] taper = policy.Window.Taper(frame, periodic: false, policy.Shape);
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
        double[] taper = policy.Window.Taper(frame, periodic: true, policy.Shape);
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
        (int coverFrom, int coverTo) = Coverage(taper, signal.Length, frame, hop, frames);
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Frames(new Spectrogram.Phasor(
            policy.Transform, frames, bins, signal.Length, frame, hop, coverFrom, coverTo,
            BinHz(frame, policy.SampleRate), policy.Window, policy.Shape, policy.Scaling, magnitude, phase, at))));
    }

    // The overlap-add normalizer Σ w² over the frame grid is FRAME GEOMETRY, not signal content, so the forward
    // pass measures its covered span for free and records it. Every sample outside that span carries partial
    // window mass by construction — a centered grid can never cover its own first and last half-frame — so the
    // extent is the honest reconstruction range and the coverage floor is a per-sample test, not a whole-signal one.
    private static (int From, int To) Coverage(double[] taper, int samples, int frame, int hop, int frames) {
        double[] weight = new double[samples];
        int left = frame / 2;
        for (int f = 0; f < frames; f++) {
            int offset = f * hop - left;
            for (int i = 0; i < frame; i++) {
                int sample = offset + i;
                if (0 <= sample && sample < samples) { weight[sample] += taper[i] * taper[i]; }
            }
        }
        int from = 0, to = samples;
        while (from < to && weight[from] <= WindowMassFloor) { from++; }
        while (to > from && weight[to - 1] <= WindowMassFloor) { to--; }
        return (from, to);
    }

    // The squared-window mass below which a normalized sample amplifies its own quantization noise rather than
    // reconstructing — one named policy value the forward measurement and the synthesis trim both read.
    private const double WindowMassFloor = 1e-24;

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
        double[] taper = policy.Window.Taper(frame, periodic: false, policy.Shape);
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
        WaveletExtension extension = policy.Extension;
        int levels = policy.Levels;
        double[] seed = new double[signal.Length];
        TensorPrimitives.ConvertChecked<float, double>(signal.Span, seed);
        double[] h = family.LowPass.ToArray(), g = family.HighPass();
        return Cascade((Approx: seed, Details: Seq<ReadOnlyMemory<double>>(), Extents: Seq<int>()), 0)
            .Map(result => result.Map(state => (SpectralOutput)new SpectralOutput.Bands(
                new WaveletDecomposition(family, extension, state.Details.Count, state.Approx, state.Details, [.. state.Extents], at))));

        // Mallat cascade as an immutable Fin-threaded fold (coarsest detail at the head, per-level parent extents recorded for the synthesis trim); the cascade stops when the approximation can no longer fill the QMF support, so realized `Levels` is whatever depth the signal admits.
        IO<Fin<(double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents)>> Cascade((double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents) state, int level) =>
            level >= levels || state.Approx.Length < h.Length
                ? IO.pure(Fin.Succ(state))
                : Convolve2(state.Approx, h, g, extension).Bind(result => result.Match(
                    Succ: band => Cascade((band.Low, ((ReadOnlyMemory<double>)band.High).Cons(state.Details), state.Extents.Add(state.Approx.Length)), level + 1),
                    Fail: static error => IO.pure(Fin.Fail<(double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents)>(error))));
    }

    // One stride-2 Conv1D per QMF tap set: the approximation lowers through the factor lane against the reversed analysis filters (true convolution from cross-correlation), downsampled by 2.
    private static IO<Fin<(double[] Low, double[] High)>> Convolve2(double[] x, double[] h, double[] g, WaveletExtension extension) =>
        Downsample(x, h, extension).Bind(low => low.Match(
            Succ: admitted => Downsample(x, g, extension).Map(high => high.Map(detail => (admitted, detail))),
            Fail: static error => IO.pure(Fin.Fail<(double[] Low, double[] High)>(error))));

    // The boundary mode is PRE-EXTENSION, never a second convolution kernel: the signal extends by the row's own
    // law at the analysis shift `s = L/2 − 1`, the lowering convolves at pad 0, and the first ⌈N/2⌉ outputs are
    // the band. Pushing the mode into the `Conv1D` padding argument would need a padding vocabulary the lowering
    // owner does not have, and one boundary law would then live in two places.
    private const int AnalysisShift = 1;

    private static IO<Fin<double[]>> Downsample(double[] x, double[] filter, WaveletExtension extension) {
        int taps = filter.Length, shift = taps / 2 - AnalysisShift, band = (x.Length + 1) / 2;
        double[] z = extension.Extend(x, shift, taps - 1 - shift);
        return KernelLowering.Lower(TensorOpFamily.Conv1D,
                Matrix<double>.Build.Dense(1, z.Length, (_, c) => z[c]),
                Matrix<double>.Build.Dense(taps, 1, (r, _) => filter[taps - 1 - r]),
                new ConvWindow([taps], [2], [0], [1], 1, 1, [z.Length]), new ShardDispatch.Local())
            .Map(result => result.Map(outcome => outcome.Solution.ToColumnMajorArray()[..band]));
    }

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
        double[] taper = frames.Window.Taper(frames.FrameSize, periodic: true, frames.Shape);
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
        // Synthesis trims to the extent the FORWARD pass recorded rather than re-deriving one or refusing over
        // an edge it already measured — only an empty extent is a refusal, and that is a frame grid covering
        // nothing at all, never a signal whose interior reconstructs exactly.
        if (frames.CoverTo <= frames.CoverFrom) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create($"<stft-window-coverage:{frames.CoverFrom}/{frames.CoverTo}>"))); }
        float[] samples = new float[frames.CoverTo - frames.CoverFrom];
        for (int i = 0; i < samples.Length; i++) { samples[i] = (float)(sum[frames.CoverFrom + i] / weight[frames.CoverFrom + i]); }
        return IO.pure(Fin.Succ<ReadOnlyMemory<float>>(samples));
    }

    // Rows whose forward destroyed the inverse evidence answer a typed fault naming the destruction — magnitude-only
    // spectrograms and segment-averaged periodograms — never a fabricated reconstruction.
    internal static IO<Fin<ReadOnlyMemory<float>>> NonInvertible(SpectralOutput output, string evidence) =>
        IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create($"<invert-destroyed:{evidence}>")));

    // Inverse DWT feeds analysis filters unreversed to cross-correlating `Conv1D`, yielding the time-reversed synthesis convolution.
    // Each level zero-stuffs approximation/detail, extends under the RECORDED mode, convolves at stride one, sums, and trims to its recorded parent extent.
    internal static IO<Fin<ReadOnlyMemory<float>>> Synthesize(SpectralOutput output) {
        if (output is not SpectralOutput.Bands { Decomposition: WaveletDecomposition w }) { return IO.pure(Fin.Fail<ReadOnlyMemory<float>>(ComputeFault.Create("<invert-carrier-miss>"))); }
        double[] h = w.Family.LowPass.ToArray(), g = w.Family.HighPass();
        Seq<(ReadOnlyMemory<double> Detail, int Extent)> steps = toSeq(w.Details.Zip(toSeq(w.Extents.Reverse()), static (d, e) => (d, e)));
        return steps.Fold(
                IO.pure(Fin.Succ(w.Approximation.ToArray())),
                (effect, step) => effect.Bind(result => result.Match(
                    Succ: approximation => Reconstruct(approximation, step.Detail.ToArray(), h, g, w.Extension, step.Extent),
                    Fail: static error => IO.pure(Fin.Fail<double[]>(error)))))
            .Map(result => result.Map(static a => {
                float[] samples = new float[a.Length];
                for (int i = 0; i < a.Length; i++) { samples[i] = (float)a[i]; }
                return (ReadOnlyMemory<float>)samples;
            }));
    }

    private static IO<Fin<double[]>> Reconstruct(double[] approx, double[] detail, double[] h, double[] g, WaveletExtension extension, int extent) =>
        Resample(approx, h, extension, extent).Bind(low => low.Match(
            Succ: admitted => Resample(detail, g, extension, extent).Map(high => high.Map(detailBand => {
                double[] merged = new double[extent];
                int span = Math.Min(extent, Math.Min(admitted.Length, detailBand.Length));
                TensorPrimitives.Add(admitted.AsSpan(0, span), detailBand.AsSpan(0, span), merged.AsSpan(0, span));
                return merged;
            })),
            Fail: static error => IO.pure(Fin.Fail<double[]>(error))));

    // Zero-stuff by 2 then one stride-1 Conv1D — the transposed convolution spelled through the one lowering
    // owner; the filter feeds UNREVERSED because cross-correlation with h equals convolution with its reversal.
    // The synthesis shift is `L − 1 − s` against the analysis shift `s`, the perfect-reconstruction law
    // `s_analysis + s_synthesis = L − 1` stated explicitly: both shifts are spelled, so neither is a constant
    // one leg carries and the other silently assumes, which is exactly how an off-by-one PR drift survives a
    // round-trip on Haar and breaks on every longer bank.
    private static IO<Fin<double[]>> Resample(double[] x, double[] filter, WaveletExtension extension, int extent) {
        int taps = filter.Length, shift = taps - 1 - (taps / 2 - AnalysisShift);
        double[] up = new double[2 * x.Length];
        for (int i = 0; i < x.Length; i++) { up[2 * i] = x[i]; }
        double[] z = extension.Extend(up, shift, taps - 1 - shift);
        return KernelLowering.Lower(TensorOpFamily.Conv1D,
                Matrix<double>.Build.Dense(1, z.Length, (_, c) => z[c]),
                Matrix<double>.Build.Dense(taps, 1, (r, _) => filter[r]),
                new ConvWindow([taps], [1], [0], [1], 1, 1, [z.Length]), new ShardDispatch.Local())
            .Map(result => result.Map(outcome => outcome.Solution.ToColumnMajorArray()[..Math.Min(extent, 2 * x.Length)]));
    }

    // --- [FILTER_DESIGN] -- FIR windowed-sinc, equiripple-over-QR, and the shared IIR bilinear map.

    // Windowed-sinc FIR: the ideal band impulse response tapered by the WindowKind window, the FilterBand row owning the band combine — so the windowed-sinc carries no per-band branch.
    internal static FilterCoefficients WindowedSinc(FilterContext spec) {
        int taps = spec.Order | 1;                              // odd length for a Type-I linear-phase center tap.
        double[] window = spec.Window.Taper(taps, periodic: false, spec.Shape);
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

    // Equiripple FIR by the SECOND REMEZ EXCHANGE over an explicit transition (don't-care) band, the `FilterBand`
    // row supplying the per-frequency desired/weight (ripple-derived δp/δs). Transition rows stay zero-weight so
    // the design never chases the brick-wall discontinuity — a single hard 0/1 step with uniform weight is the
    // deleted naive form that Gibbs-rings. Odd `taps` is FORCED (`spec.Order | 1`), so the design is Type I by
    // construction and its cosine series P(ω) = Σ aₖ·cos(kω) has degree L = taps/2 — which is what makes L + 2
    // the alternation count the acceptance test demands, on every band and at every order.
    internal static Fin<FilterCoefficients> Equiripple(FilterContext spec, DenseSubstrate substrate) {
        int taps = spec.Order | 1, degree = taps / 2, reference = degree + 2, grid = 16 * taps;
        double c1 = spec.NormalizedCutoff;
        double c2 = spec.NormalizedUpper > c1 && spec.NormalizedUpper < 1.0 ? spec.NormalizedUpper : Math.Min(0.99, c1 + Math.Max(0.05, 0.25 * (1.0 - c1)));
        // The transition width derives for EVERY band kind — a quarter of the band, bounded by the distance to
        // each edge — because a single-edge band has a transition exactly as a two-edge one does; the zero a
        // `Band.Upper` gate assigned to LP/HP collapsed their don't-care region and put the design back on the
        // discontinuity the explicit band exists to avoid.
        double transition = Math.Min(0.25 * (c2 - c1), Math.Min(0.5 * c1, 0.5 * (1.0 - c2)));
        double dp = (Math.Pow(10.0, spec.RippleDb / 20.0) - 1.0) / (Math.Pow(10.0, spec.RippleDb / 20.0) + 1.0);   // passband ripple δp.
        double ds = Math.Pow(10.0, -spec.StopbandDb / 20.0);                                                      // stopband ripple δs.
        double passWeight = 1.0, stopWeight = ds <= 0.0 ? 1.0 : dp / ds;                                          // heavier where the ripple bound is tighter.
        double[] omega = [.. Enumerable.Range(0, grid).Select(r => Math.PI * r / (grid - 1))];
        double[] desired = new double[grid];
        double[] weight = new double[grid];
        for (int r = 0; r < grid; r++) {
            (double d, double w) = spec.Band.Ideal(omega[r] / Math.PI, c1, c2, transition, passWeight, stopWeight);
            (desired[r], weight[r]) = (d, w);
        }
        int[] live = [.. Enumerable.Range(0, grid).Where(r => weight[r] > 0.0)];
        if (live.Length < reference) { return Fin.Fail<FilterCoefficients>(ComputeFault.Create($"<remez-grid:{live.Length}<{reference}>")); }
        return Seed(spec, omega, desired, weight, live, degree, reference, substrate)
            .Bind(seed => Exchange(seed, omega, desired, weight, live, degree, reference, spec.Budget))
            .Map(levelled => Symmetric(Recover(levelled, omega, desired, weight, degree), taps));
    }

    // The initial reference set: one weighted-least-squares pass over the live grid places the alternation
    // pattern far better than a uniform spread on a narrow transition band, and the extrema of THAT error are
    // the seed. This is Lawson's one surviving role — a seed, never the design — and the same routine takes a
    // converged lower-degree reference by scaling its abscissae, so a design swept over order re-seeds from its
    // own predecessor instead of restarting the exchange from scratch at every degree.
    // The composition-selected `DenseSubstrate` threads in as a value, so the seed's least-squares leg declares
    // the substrate it runs on and the witnessed carrier reports which one actually SERVED it — a native leg
    // that declined and degraded to the managed terminal is visible here rather than assumed.
    private static Fin<int[]> Seed(
        FilterContext spec, double[] omega, double[] desired, double[] weight, int[] live, int degree, int reference, DenseSubstrate substrate) {
        Matrix<double> a = Matrix<double>.Build.Dense(live.Length, degree + 1, (r, c) => Math.Sqrt(weight[live[r]]) * Math.Cos(omega[live[r]] * c));
        Vector<double> b = Vector<double>.Build.Dense(live.Length, r => Math.Sqrt(weight[live[r]]) * desired[live[r]]);
        return DenseRoute.Solve(new FactorRoute.Orthonormal(QRMethod.Thin, Modified: false), a, b, TolerancePolicy.Derive(a, b), substrate)
            .Map(solved => {
                double[] error = [.. live.Select(r => weight[r] * (Cosine(solved.X, omega[r]) - desired[r]))];
                int[] extrema = Alternating(error, live);
                return extrema.Length >= reference ? Trimmed(extrema, error, live, reference) : Spread(live, reference);
            });
    }

    // One exchange step: level the error on the current reference, locate the alternating extrema of the
    // levelled error over the whole live grid, and exchange. Termination is the de la Vallée Poussin ratio
    // C = ‖E‖∞ / min|E(ωᵢ)| over the reference — bounded below by 1 and equal to 1 exactly at the Chebyshev
    // optimum — accepted at C − 1 ≤ ε_t, which `FitBudget.Tolerance` IS. A coefficient-distance test is the
    // DELETED form: it measures the iterate's motion, not its levelling, and it accepts a stalled exchange
    // whose error is nowhere near equioscillating.
    private static Fin<int[]> Exchange(
        int[] seed, double[] omega, double[] desired, double[] weight, int[] live, int degree, int reference, FitBudget budget) =>
        toSeq(Enumerable.Range(0, budget.MaxIterations))
            .Fold(Fin.Succ((Reference: seed, Levelled: false)), (acc, _) => acc.Bind(state => {
                if (state.Levelled) { return acc; }
                double delta = Levelled(state.Reference, omega, desired, weight);
                double[] error = [.. live.Select(r => weight[r] * (Interpolated(state.Reference, omega, desired, weight, delta, omega[r]) - desired[r]))];
                int[] extrema = Alternating(error, live);
                if (extrema.Length < reference) { return Fin.Fail<(int[], bool)>(ComputeFault.Create($"<remez-alternation:{extrema.Length}<{reference}>")); }
                int[] next = Trimmed(extrema, error, live, reference);
                Dictionary<int, double> magnitude = live.Select((r, i) => (r, i)).ToDictionary(row => row.r, row => Math.Abs(error[row.i]));
                double peak = next.Max(r => magnitude[r]);
                double floor = next.Min(r => magnitude[r]);
                return Fin.Succ((next, floor > 0.0 && peak / floor - 1.0 <= budget.Tolerance));
            }))
            .Bind(state => state.Levelled
                ? Fin.Succ(state.Reference)
                : Fin.Fail<int[]>(ComputeFault.Create($"<remez-nonconverged:{budget.MaxIterations}>")));

    // Levelling solves for the equioscillation amplitude δ in CLOSED form off the barycentric weights:
    // δ = Σ bₖ·D(ωₖ) / Σ (−1)ᵏ·bₖ / W(ωₖ). The barycentric form is mandatory — the cos(kω) Vandermonde it
    // replaces is exponentially ill-conditioned in the degree, so a moderately long filter's levelled system
    // returns coefficients whose own residual dwarfs the ripple the exchange is trying to equalize.
    private static double Levelled(int[] reference, double[] omega, double[] desired, double[] weight) {
        double[] b = Barycentric(reference, omega);
        double numerator = 0.0, denominator = 0.0;
        for (int k = 0; k < reference.Length; k++) {
            numerator += b[k] * desired[reference[k]];
            denominator += ((k & 1) == 0 ? 1.0 : -1.0) * b[k] / weight[reference[k]];
        }
        return numerator / denominator;
    }

    // The levelled interpolant through (xₖ, D(ωₖ) − (−1)ᵏ·δ/W(ωₖ)) in the barycentric second form; the x-axis
    // is cos ω, on which the cosine series IS a polynomial of the design's own degree.
    private static double Interpolated(int[] reference, double[] omega, double[] desired, double[] weight, double delta, double at) {
        double[] b = Barycentric(reference, omega);
        double x = Math.Cos(at), numerator = 0.0, denominator = 0.0;
        for (int k = 0; k < reference.Length; k++) {
            double dx = x - Math.Cos(omega[reference[k]]);
            if (dx == 0.0) { return desired[reference[k]] - ((k & 1) == 0 ? 1.0 : -1.0) * delta / weight[reference[k]]; }
            double share = b[k] / dx;
            numerator += share * (desired[reference[k]] - ((k & 1) == 0 ? 1.0 : -1.0) * delta / weight[reference[k]]);
            denominator += share;
        }
        return numerator / denominator;
    }

    private static double[] Barycentric(int[] reference, double[] omega) {
        double[] b = new double[reference.Length];
        for (int k = 0; k < reference.Length; k++) {
            double product = 1.0, xk = Math.Cos(omega[reference[k]]);
            for (int j = 0; j < reference.Length; j++) {
                if (j != k) { product *= xk - Math.Cos(omega[reference[j]]); }
            }
            b[k] = 1.0 / product;
        }
        return b;
    }

    // Local extrema of the weighted error whose signs ALTERNATE: a run of same-signed peaks contributes only its
    // largest, because the alternation count — not the extremum count — is what the equioscillation theorem
    // certifies, and counting a same-signed pair twice accepts a design that never equioscillated.
    private static int[] Alternating(double[] error, int[] live) {
        List<int> picked = [];
        for (int i = 0; i < error.Length; i++) {
            bool peak = (i == 0 || Math.Abs(error[i]) >= Math.Abs(error[i - 1])) &&
                        (i == error.Length - 1 || Math.Abs(error[i]) > Math.Abs(error[i + 1]));
            if (!peak || error[i] == 0.0) { continue; }
            if (picked.Count > 0 && Math.Sign(error[i]) == Math.Sign(error[picked[^1]])) {
                if (Math.Abs(error[i]) > Math.Abs(error[picked[^1]])) { picked[^1] = i; }
                continue;
            }
            picked.Add(i);
        }
        return [.. picked.Select(i => live[i])];
    }

    // More alternations than the reference admits: keep the largest-magnitude contiguous run of exactly
    // `reference` of them, because dropping from an end preserves the alternating sign pattern where dropping
    // the globally smallest does not.
    private static int[] Trimmed(int[] extrema, double[] error, int[] live, int reference) {
        Dictionary<int, double> magnitude = live.Select((r, i) => (r, i)).ToDictionary(row => row.r, row => Math.Abs(error[row.i]));
        int best = 0;
        double bestFloor = double.NegativeInfinity;
        for (int start = 0; start + reference <= extrema.Length; start++) {
            double floor = Enumerable.Range(start, reference).Min(k => magnitude[extrema[k]]);
            if (floor > bestFloor) { (best, bestFloor) = (start, floor); }
        }
        return extrema[best..(best + reference)];
    }

    private static int[] Spread(int[] live, int reference) =>
        [.. Enumerable.Range(0, reference).Select(k => live[(int)((long)k * (live.Length - 1) / (reference - 1))])];

    // The converged reference determines the interpolant, and the cosine coefficients recover from it by the
    // exact DCT-I over `degree + 1` abscissae: P is a degree-`degree` polynomial in cos ω, so sampling it there
    // and inverting is exact, never a least-squares refit that would move the design off its own levelling.
    private static Vector<double> Recover(int[] reference, double[] omega, double[] desired, double[] weight, int degree) {
        double delta = Levelled(reference, omega, desired, weight);
        double[] node = [.. Enumerable.Range(0, degree + 1).Select(m => Math.PI * m / degree)];
        double[] value = [.. node.Select(w => Interpolated(reference, omega, desired, weight, delta, w))];
        return Vector<double>.Build.Dense(degree + 1, k => {
            double sum = 0.0;
            for (int m = 0; m <= degree; m++) {
                double half = m == 0 || m == degree ? 0.5 : 1.0;
                sum += half * value[m] * Math.Cos(Math.PI * k * m / degree);
            }
            return 2.0 * sum / degree;
        });
    }

    private static double Cosine(Vector<double> coefficients, double at) =>
        Enumerable.Range(0, coefficients.Count).Sum(k => coefficients[k] * Math.Cos(at * k));

    private static FilterCoefficients Symmetric(Vector<double> halfBand, int taps) {
        int mid = taps / 2;
        double[] b = new double[taps];
        for (int i = 0; i < taps; i++) { int k = Math.Abs(i - mid); b[i] = halfBand[k] * (k == 0 ? 1.0 : 0.5); }   // cosine-series center tap a₀, off-center aₖ/2.
        return new FilterCoefficients(FilterDesign.FirRemez, b, new double[] { 1.0 });
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

        static Complex Product(IEnumerable<Complex> xs) { Complex p = Complex.One; foreach (Complex x in xs) { p *= x; } return p; }
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

(none)
