# [COMPUTE_SIGNAL]

Rasm.Compute signal/spectral lane: one `SpectralTransform` `[SmartEnum<string>]` frequency-domain axis whose rows carry transform and inverse delegates over a `SignalShape` admission row, folding to deferred `Transform.Apply` and `Transform.Invert` surfaces. `IO<Fin<T>>` preserves the outer lowering effect and inner domain fault without forcing either inside the numeric lane. Forward and inverse share one surface: `stft` preserves frame phase and overlap-add evidence, while evidence-destroying `spectrogram` and averaged periodograms return typed inverse faults. One framed fold raises every frame grid the lane needs and takes its per-frame reducer as a value, so the STFT rows, the Welch periodogram, and the N-channel cross-power matrix are three reads of one windowing-and-transform law.

Per-bin transforms ride `MathNet.Numerics.IntegralTransforms.Fourier` over split `double[]` planes, windowing composes the kernel `Numerics/transform#WINDOW` `WindowTaper` roster, dependence and density rows ride `MathNet.Numerics.Statistics`, magnitude and phase read `TensorPrimitives.Hypot`/`Atan2`, bin spacing reads `Fourier.FrequencyScale`, the modal peak floor reads the kernel `Rasm/Domain/stats#ORDER_STATISTICS` `Distribution.Of` exact median, and wavelet analysis and synthesis ride `Tensor/factor#KERNEL_LOWERING` `Conv` over `LoweringOperands.Windowed`.

The per-bin complex Hermitian dominant pair stays page-local under a convergence witness because the dense owner is real-typed; `ComputeFault` and `ComparerAccessors.StringOrdinal` arrive settled; NodaTime `IClock` supplies the semantic `Instant` a spectral receipt stamps and kernel `MonotonicTimeline` supplies any elapsed span it measures, so the app-stratum `ClockPolicy` never descends into this owner. Waveform corpora arrive already framed through `Runtime/field#FIELD_RESULT_CODEC` `InterchangeIo.ImportWaveforms` and fold into the same cross-power matrix a raised frame grid produces. Spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` and `Modal` own measured-mode identification and `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating end.

## [01]-[INDEX]

- [02]-[SIGNAL_LANE]: effect-preserving FFT/STFT/PSD/wavelet `Transform` over MathNet Fourier and the kernel taper roster, one framed fold under a shared `FrameGrid`, paired row-owned inversion, the Welch cross-power matrix serving two-channel coherence and N-channel FDD measured-mode extraction alike, one `CrossPower` matrix minted from raised planes or from an already-framed interchange corpus and read as two-channel coherence or N-channel FDD measured modes, and amplitude-domain dependence and nonparametric distribution description.

## [02]-[SIGNAL_LANE]

- Owner: `SpectralTransform` `[SmartEnum<string>]` rows each carry a `SignalShape` admission row, a `(ReadOnlyMemory<float>, SignalContext, Instant) → IO<Fin<SpectralOutput>>` transform column, and a `(SpectralOutput) → IO<Fin<ReadOnlyMemory<float>>>` inverse column — no transport on either, because the `dwt` bank's convolutions lower against the `Tensor/factor#KERNEL_LOWERING` `ShardDispatch.Local` a local bank spells and the Fourier rows lower nothing at all. `SignalShape` `[SmartEnum<int>]` is the admission axis: which `SignalPolicy` case a row admits AND the gate that case owes, so a row states its evidence shape as a value rather than a boolean the framed case alone could read. `SignalPolicy` `[Union]` closes `PerBin`, `Framed`, and `Wavelet` evidence; `SignalContext` exists only after row admission and `FrameGrid` is the frame geometry every framed leg derives ONCE. `WaveletExtension` rows carry one boundary-extension column and the per-family admission that proves the extension reconstructs; `ExtensionClass` types the symmetry point a family's zero-stripped core admits; `QmfShift` derives the analysis and synthesis shifts as one pair so the perfect-reconstruction law is structural. `SpectralOutput` owns `Bins`/`Frames`/`Bands`; `Spectrogram` owns inverse-sufficient `Phasor` frames and magnitude-only `Power` frames; `CrossPower` is the per-bin Hermitian cross-spectral matrix both mints produce and both measured-mode reads fold. `DependenceKind` rows carry both arities of one co-variation measure and `DensityKernel` rows carry one taper each, while `ChannelQuery` closes the description request and `ChannelEvidence` its two result shapes.
- Cases: `SpectralTransform` fft · rfft · stft · spectrogram · welch-psd · dwt; `SignalShape` per-bin · framed · overlapped · wavelet; `SignalPolicy` per-bin · framed · wavelet; `DependenceKind` pearson · spearman; `DensityKernel` gaussian · epanechnikov · uniform · triangular; `ChannelQuery` dependence · distribution; `WaveletFamily` haar · db2 · db4 · sym4 · coif1; `WaveletExtension` zero · periodic · symmetric; `ExtensionClass` none · whole-point · half-point.
- Entry: `Transform.Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, IClock clock) → IO<Fin<SpectralOutput>>` composes sample admission with the row's `SignalShape` gate, then dispatches over `SignalContext`. `Invert(SpectralOutput output) → IO<Fin<ReadOnlyMemory<float>>>` projects and dispatches the owning row. `Power(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock)` and `Power(WaveformCorpus corpus, WindowTaper window, Instant at)` are the TWO MINTS of one `CrossPower` matrix — the first raises its own frame grid under the `welch-psd` row's admission and a two-segment floor, the second walks the frames an interchange corpus already carries — and `Coherence(CrossPower power, int left, int right, Instant at)` and `Modal(CrossPower power, Instant at)` are the two READS of it: the first projects one pair's auto/cross spectra and magnitude-squared coherence, the second runs dominant-singular-pair power iteration per bin, first-singular-value peak picking, and half-power damping, returning the `ModalEstimate` measured-mode set. `Coherence(x, y, policy, clock)` and `Modal(channels, policy, clock)` are the mint-then-read convenience pair over two and N planes. `Describe(Seq<ReadOnlyMemory<float>> channels, ChannelQuery query, IClock clock)` admits the same synchronous channel set under the query's own arity floor and folds either the all-pairs dependence matrix or the per-channel kernel density with its empirical CDF, entropy, and interquartile range on one shared support grid. Every window materializes through the kernel `taper.Of(width, TaperFraming.FftFrame)` on the `Fin` rail.
- Auto: each `SpectralTransform` `Kernel` realizes one split-plane `Fourier` transform: `fft` full-length, `rfft` Hermitian half-spectrum, `stft` centered magnitude/phase frames over a recorded coverage extent, `spectrogram` squared STFT magnitudes, `welch-psd` averaged periodograms, and `dwt` a stride-2 `Conv` QMF cascade under the policy's own `WaveletExtension`. Each `Invert` realizes the paired inverse: `fft`/`rfft` reciprocal transforms, `stft` weighted overlap-add trimmed to the recorded coverage, and `dwt` zero-stuff synthesis at the mirrored shift, trimmed to recorded extents.
- Receipt: the spectral fold mints no hot-path receipt. Evidence rides `Spectrum.Length`/`BinHz`/`Samples`/`Scaling`, each `Spectrogram` case's `Frames`/`Bins`, `WaveletDecomposition.Levels`/`Extents`, and `CrossSpectrum.Coherence`. Tensor-lowered legs compose the `Runtime/receipts#RECEIPT_UNION` `ComputeReceipt.TensorRun(Family, Dtype, Elements, SimdWidth, Partitions)` their owning operation stamps. Bare `Fourier` transforms mint no fabricated tensor receipt.
- Packages: MathNet.Numerics, Rasm (kernel `Numerics/transform#WINDOW` `WindowTaper`/`TaperFraming`, `Numerics/atoms` `Dimension`, `Domain/stats#ORDER_STATISTICS` `Distribution`/`Scalar`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, Generator.Equals, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new spectral transform is one `SpectralTransform` row naming its `SignalShape` and binding forward and inverse; a new evidence shape is one `SignalShape` row carrying its gate and one `SignalPolicy` case; a new window is one `WindowTaper` row at the kernel taper owner, reaching every leg here with no edit on this page; a new wavelet is one `WaveletFamily` row whose linear-phase and extension class DERIVE from its coefficients, so its admissible extensions follow with no roster edit; a new boundary mode is one `WaveletExtension` row; a new co-variation measure is one `DependenceKind` row carrying both arities and a new density taper one `DensityKernel` row, while a new description modality is one `ChannelQuery` case with its `ChannelEvidence` twin. `FftTransform`/`StftTransform`/`PsdEstimator` collapse onto `Transform.Apply`; inverse siblings collapse onto `Transform.Invert`; a second per-frame windowing loop collapses onto `Framed`.
- Boundary: FILTER DESIGN AND APPLICATION ARE NOT THIS LANE'S. The windowed-sinc and Remez FIR designers, the Butterworth/Chebyshev/elliptic analog prototypes, the shared bilinear map, the direct-form-II recurrence, the FFT overlap-add application, and the frequency-response sweep were `LAW_WITHOUT_PRODUCER` at scale — census on landing: zero composers of `FilterShape`/`FilterCoefficients`/`FilterDesign`/`FilterSpec`/`FilterBand`/`FilterResponse` anywhere in `libs/dotnet` outside this page, and zero seam rows naming `Stats/signal` in `libs/contracts/manifest.json`, so neither the symbol census nor the registry census found a consumer. NAMED LOSS, stated so a later need re-derives rather than rediscovers: a true second Remez exchange over a barycentric levelled interpolant with de la Vallée Poussin acceptance, four closed-form analog prototypes including a Landen-ladder Jacobi elliptic lattice, the band-aware `lp2{lp,hp,bp,bs}` zpk algebra, and the tap-count-selected `Conv`-versus-overlap-add application route. WITNESS for the deletion rather than a wire-up: `Runtime/field#FIELD_RESULT_CODEC` already declares fitted filter banks as an INGESTED corpus — they arrive through `InterchangeIo.ImportWaveforms` as `WaveformCorpus` frames, python-fitted — so the estate's declared route for filter coefficients is import, not local design, and a designer with no caller competed with the seam that already has one.
- Boundary: `MathNet.Numerics.IntegralTransforms.Fourier` operates in place. Per-bin and framed kernels apply recorded `FourierOptions`; Welch and the cross-power fold pin `FourierOptions.NoScaling` and own their normalization — the kernel `Numerics/transform#SPECTRAL` `SpectralScaling` roster names those same three conventions and its `Unscaled.RoundTrip(cells)` states the factor this lane divides back out, so the convention vocabulary agrees across the strata boundary while the option value stays the RECORDED EVIDENCE here: `FourierOptions` is what `Spectrum.Scaling` and `Spectrogram.Phasor.Scaling` hand the reciprocal leg and what every MathNet entrypoint consumes.
- Boundary: the kernel's `SpectralArena` cases own their buffers, where this lane transforms split `double[]` planes and one pooled per-frame scratch pair in place under its own framing, so composing the arena re-buffers every frame of an overlap-add for a transform this lane already performs. Split real/imaginary arrays let `TensorPrimitives.Hypot`/`Atan2` read contiguous spans; `ForwardReal`/`InverseReal` own packed half-spectra; `Fourier.FrequencyScale` owns bin resolution. Frame scratch is `double[]` and not a pooled span owner because `Fourier.Forward(double[], double[], FourierOptions)` binds ARRAY arities — the reduction that matters is that `Framed` allocates its scratch pair ONCE per call where four hand loops allocated one pair each.
- Boundary: the taper roster serves this lane through FFT FRAMING ALONE. The kernel's shaped rows — Gauss, Tukey, Kaiser — bind their design to `TaperFraming.FilterDesign` and refuse `FftFrame` outright, and its fixed rows without a periodic twin refuse it too, so the four rows carrying that twin are exactly what a frame grid admits and the refusal is typed on the `Fin` rail where the deleted local roster ignored the flag and silently substituted the symmetric form. NAMED LOSS with the filter half: the σ and α policy values this lane declared for the kernel's shaped rows had their one consumer in the windowed-sinc leg, so `Option<TaperShape>` leaves `SignalPolicy.Framed`, `SignalContext`, and `Spectrogram.Phasor` — every surviving path spells `None` by construction and a column whose every reaching value is `None` is decorative.
- Boundary: inversion consumes forward evidence and nothing else. `Spectrum` records `Samples` and `Scaling` and DERIVES its sample rate from `BinHz · Samples` rather than storing a second authority; `Spectrogram.Phasor` records `Samples`, `FrameSize`, `Hop`, `Window`, `Scaling`, the half-open COVERAGE EXTENT the frame grid actually spans, magnitude, and phase; `Spectrogram.Power` and Welch output typed inverse faults; `WaveletDecomposition` records per-level `Extents` and the `WaveletExtension` its cascade ran under. `Spectrum.Phase` is an `Option`, because the averaged periodogram destroys phase and an empty span is an absence wearing a length.
- Boundary: an overlap-add's first and last half-frame carry partial window mass by construction, so synthesis normalizes and TRIMS to the recorded extent rather than refusing the whole inversion over an edge the forward pass already measured — the same evidence-driven law the wavelet `Extents` trim rides, and a coverage floor that fails a signal whose interior reconstructs exactly is the deleted form.
- Boundary: `WaveletFamily` owns scaling tables because MathNet exposes no wavelet surface; linear phase and extension class are DERIVED from those tables under an exact symmetry compare, never asserted per row, and each `WaveletExtension` admits against the derived descriptor so the admissible set follows a new row automatically. `QmfShift` derives both cascade shifts from the tap count as one pair, so the perfect-reconstruction law `analysis + synthesis = L − 1` holds by construction rather than as a constant one leg carries and the other assumes.
- Boundary: `Describe` is the amplitude-domain half of the same channel evidence: it re-runs no transform, derives its bandwidth from the channel's own spread when the query names none, and reads the empirical CDF and quartiles off one ascending copy because both members contract on sorted data.
- Boundary: memory-bearing evidence columns compare by HANDLE, not content. `ReadOnlyMemory<T>` is unreachable to Generator.Equals — the collection attributes require an `IEnumerable<T>` member — so `Spectrum`, `Spectrogram`, `CrossSpectrum`, `ChannelDistribution`, and `MeasuredMode` carry that posture deliberately: nothing keys, dedupes, or set-joins them, and `MeasuredMode` pairs by INDEX at its clash consumer precisely because a float frequency is no lookup key. `WaveletDecomposition` is the one exception and takes `[Equatable]` with `[property: OrderedEquality]` on its `ImmutableArray<int> Extents`, whose default equality is the underlying array REFERENCE.
- Boundary: waveform corpora — long SHM records, python-fitted filter banks, reference spectra — enter through `Runtime/field#FIELD_RESULT_CODEC` `InterchangeIo.ImportWaveforms` as `WaveformCorpus` under declared frame/hop selections, and `Transform.Power(corpus, window, at)` IS the consuming half of that seam: the carrier's `[FrameCount, Frame, Channels]` frame-major buffer feeds the same Hermitian accumulation the plane route feeds, so a screening-scale record never widens into a contiguous per-channel plane. The lane stores nothing and opens no `H5File` — the archive one-owner ruling stands, and the estimator (Arrow) and monitor (receipt-stream) storage declines stay closed.
- Boundary: spectral features feed `Stats/estimator#ESTIMATOR_LANE`; `Coherence` conditions channel pairs and `Modal` extracts the measured modes — the FDD first-singular-value spectrum is an operational estimate whose peaks are honest only where excitation is broadband, so `ModalEstimate` carries the full singular spectrum beside its picked modes and a consumer re-judges a peak against its own floor; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

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
    public static readonly WaveletFamily Haar  = new("haar",  [0.70710678118654752, 0.70710678118654752]);
    public static readonly WaveletFamily Db2   = new("db2",   [0.48296291314469025, 0.83651630373746899, 0.22414386804185735, -0.12940952255092145]);
    public static readonly WaveletFamily Db4   = new("db4",   [0.23037781330885523, 0.71484657055254153, 0.63088076792959036, -0.02798376941698385, -0.18703481171888114, 0.03084138183598697, 0.03288301166698295, -0.01059740178499728]);
    public static readonly WaveletFamily Sym4  = new("sym4",  [-0.07576571478927333, -0.02963552764599851, 0.49761866763201960, 0.80373875180591614, 0.29785779560527736, -0.09921954357684722, -0.01260396726203783, 0.03222310060404270]);
    public static readonly WaveletFamily Coif1 = new("coif1", [-0.01565572813546454, -0.07273261951285648, 0.38486484686420286, 0.85257202021225542, 0.33789766245780922, -0.07273261951285648]);

    private readonly double[] scaling;

    public ReadOnlyMemory<double> LowPass => scaling;

    public double[] HighPass() {
        double[] g = new double[scaling.Length];
        for (int k = 0; k < g.Length; k++) { g[k] = ((k & 1) == 0 ? 1.0 : -1.0) * scaling[scaling.Length - 1 - k]; }
        return g;
    }

    public bool LinearPhase => Mirrored(Core(scaling)) && Mirrored(Core(HighPass()));

    public ExtensionClass Extension =>
        !LinearPhase ? ExtensionClass.None
        : (Core(scaling).Length & 1) == 1 ? ExtensionClass.WholePoint
        : ExtensionClass.HalfPoint;

    private static ReadOnlySpan<double> Core(ReadOnlySpan<double> taps) {
        int lo = 0, hi = taps.Length - 1;
        while (lo <= hi && taps[lo] == 0.0) { lo++; }
        while (hi >= lo && taps[hi] == 0.0) { hi--; }
        return lo > hi ? [] : taps[lo..(hi + 1)];
    }

    private static bool Mirrored(ReadOnlySpan<double> core) {
        bool symmetric = true, antisymmetric = true;
        for (int k = 0; k < core.Length; k++) {
            symmetric &= core[k] == core[^(k + 1)];
            antisymmetric &= core[k] == -core[^(k + 1)];
        }
        return symmetric || antisymmetric;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WaveletExtension {
    public static readonly WaveletExtension Zero = new("zero",
        extend: static (x, left, right) => {
            double[] z = new double[left + x.Length + right];
            x.CopyTo(z.AsSpan(left));
            return z;
        },
        admits: static family => family.LowPass.Length == 2);
    public static readonly WaveletExtension Periodic = new("periodic",
        extend: static (x, left, right) => {
            double[] z = new double[left + x.Length + right];
            for (int t = 0; t < z.Length; t++) { z[t] = x[((t - left) % x.Length + x.Length) % x.Length]; }
            return z;
        },
        admits: static _ => true);
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
        admits: static family => family.Extension != ExtensionClass.None);

    [UseDelegateFromConstructor] internal partial double[] Extend(double[] x, int left, int right);
    [UseDelegateFromConstructor] private partial bool Admits(WaveletFamily family);

    internal Fin<Unit> Admit(WaveletFamily family, int samples) =>
        !Admits(family)
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Keys(Key, family.Key))))
        : this == Symmetric && family.Extension == ExtensionClass.HalfPoint && (samples & 1) == 1
            ? Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(ShapeRequirement.Dimensions, new ShapeEvidence.Alignment(samples, 2L))))
            : Fin.Succ(unit);
}

[SmartEnum<int>]
public sealed partial class SignalShape {
    public static readonly SignalShape PerBin = new(key: 0, gate: static (row, policy, _) =>
        policy is not SignalPolicy.PerBin perBin
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(row.Key))))
        : perBin.SampleRate <= 0.0 || !double.IsFinite(perBin.SampleRate)
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Scalar(perBin.SampleRate))))
            : Fin.Succ(SignalContext.OfPerBin(row, perBin)));

    public static readonly SignalShape Framed = new(key: 1, gate: static (row, policy, samples) => Frames(row, policy, samples));

    public static readonly SignalShape Overlapped = new(key: 2, gate: static (row, policy, samples) =>
        Frames(row, policy, samples).Bind(context => context.Grid.Hop < context.Grid.Frame
            ? Fin.Succ(context)
            : Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(context.Grid.Frame, context.Grid.Hop))))));

    public static readonly SignalShape Wavelet = new(key: 3, gate: static (row, policy, samples) =>
        policy is not SignalPolicy.Wavelet wavelet
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(row.Key))))
        : wavelet.Levels < 1
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Range(RangeRequirement.Positive, new ScalarEvidence.Value(wavelet.Levels))))
        : wavelet.Extension == WaveletExtension.Periodic && samples % (1 << wavelet.Levels) != 0
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(samples, wavelet.Levels))))
            : wavelet.Extension.Admit(wavelet.Family, samples).Map(_ => SignalContext.OfWavelet(row, wavelet)));

    [UseDelegateFromConstructor] internal partial Fin<SignalContext> Gate(SpectralTransform row, SignalPolicy policy, int samples);

    private static Fin<SignalContext> Frames(SpectralTransform row, SignalPolicy policy, int samples) =>
        policy is not SignalPolicy.Framed framed
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(row.Key))))
        : framed.SampleRate <= 0.0 || !double.IsFinite(framed.SampleRate)
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Scalar(framed.SampleRate))))
        : framed.HopSize <= 0 || framed.FrameSize < framed.HopSize || framed.FrameSize > samples
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Counts(framed.FrameSize, framed.HopSize, samples))))
        : FrameGrid.Cells(samples, framed.FrameSize, framed.HopSize) > Array.MaxLength
            ? Fin.Fail<SignalContext>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.WithinLimit, new CapacityEvidence.Extent(framed.FrameSize, framed.HopSize, samples))))
            : Fin.Succ(SignalContext.OfFramed(row, framed, samples));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralTransform {
    public static readonly SpectralTransform Fft         = new("fft",         SignalShape.PerBin,     Transform.PerBin,           Transform.InvertBins);
    public static readonly SpectralTransform Rfft        = new("rfft",        SignalShape.PerBin,     Transform.RealForward,      Transform.InvertPacked);
    public static readonly SpectralTransform Stft        = new("stft",        SignalShape.Overlapped, Transform.ShortTime,        Transform.InvertFrames);
    public static readonly SpectralTransform Spectrogram = new("spectrogram", SignalShape.Framed,     Transform.PowerSpectrogram, Transform.NonInvertible);
    public static readonly SpectralTransform WelchPsd    = new("welch-psd",   SignalShape.Framed,     Transform.Welch,            Transform.NonInvertible);
    public static readonly SpectralTransform Dwt         = new("dwt",         SignalShape.Wavelet,    Transform.Wavelet,          Transform.Synthesize);

    public SignalShape Shape { get; }

    [UseDelegateFromConstructor] internal partial IO<Fin<SpectralOutput>> Run(double[] plane, SignalContext context, Instant at);
    [UseDelegateFromConstructor] public partial IO<Fin<ReadOnlyMemory<float>>> Invert(SpectralOutput output);

    internal Fin<SignalContext> Admit(SignalPolicy policy, int samples) => Shape.Gate(this, policy, samples);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DependenceKind {
    public static readonly DependenceKind Pearson = new("pearson", Correlation.Pearson, static rows => Correlation.PearsonMatrix([.. rows]));
    public static readonly DependenceKind Spearman = new("spearman", Correlation.Spearman, static rows => Correlation.SpearmanMatrix([.. rows]));

    [UseDelegateFromConstructor] internal partial double Pairwise(IEnumerable<double> a, IEnumerable<double> b);
    [UseDelegateFromConstructor] internal partial Matrix<double> AllPairs(Seq<double[]> channels);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DensityKernel {
    public static readonly DensityKernel Gaussian = new("gaussian", KernelDensity.EstimateGaussian);
    public static readonly DensityKernel Epanechnikov = new("epanechnikov", KernelDensity.EstimateEpanechnikov);
    public static readonly DensityKernel Uniform = new("uniform", KernelDensity.EstimateUniform);
    public static readonly DensityKernel Triangular = new("triangular", KernelDensity.EstimateTriangular);

    [UseDelegateFromConstructor] internal partial double Estimate(double at, double bandwidth, IList<double> samples);
}

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SignalPolicy {
    private SignalPolicy() { }

    public sealed record PerBin(double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Framed(WindowTaper Window, int FrameSize, int HopSize, double SampleRate, FourierOptions Scaling) : SignalPolicy;
    public sealed record Wavelet(int Levels, WaveletFamily Family, WaveletExtension Extension) : SignalPolicy;

    public static readonly SignalPolicy CanonicalFft   = new PerBin(SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalStft  = new Framed(WindowTaper.Hann, FrameSize: 1024, HopSize: 512, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalWelch = new Framed(WindowTaper.Hann, FrameSize: 256, HopSize: 128, SampleRate: 48000.0, FourierOptions.Default);
    public static readonly SignalPolicy CanonicalDwt   = new Wavelet(Levels: 4, WaveletFamily.Db4, WaveletExtension.Periodic);
}

public readonly record struct FrameGrid(int Frame, int Hop, int Left, int Frames) {
    public int Bins => Frame / 2 + 1;

    public static FrameGrid Centered(int frame, int hop, int samples) =>
        new(frame, hop, Left: frame / 2, Frames: (int)(1L + (samples + (long)(frame / 2) - 1L) / hop));

    public static FrameGrid Interior(int frame, int hop, int samples) =>
        new(frame, hop, Left: 0, Frames: 1 + (samples - frame) / hop);

    internal static long Cells(int samples, int frame, int hop) =>
        (1L + (samples + frame / 2L - 1L) / hop) * (frame / 2L + 1L);
}

internal sealed record SignalContext(
    SpectralTransform Transform, WindowTaper Window, FrameGrid Grid, int Levels,
    double SampleRate, FourierOptions Scaling, WaveletFamily Wavelet, WaveletExtension Extension) {
    internal static SignalContext OfPerBin(SpectralTransform row, SignalPolicy.PerBin policy) =>
        new(row, WindowTaper.Dirichlet, default, 0, policy.SampleRate, policy.Scaling, WaveletFamily.Haar, WaveletExtension.Periodic);

    internal static SignalContext OfFramed(SpectralTransform row, SignalPolicy.Framed policy, int samples) =>
        new(row, policy.Window, FrameGrid.Centered(policy.FrameSize, policy.HopSize, samples), 0,
            policy.SampleRate, policy.Scaling, WaveletFamily.Haar, WaveletExtension.Periodic);

    internal static SignalContext OfWavelet(SpectralTransform row, SignalPolicy.Wavelet policy) =>
        new(row, WindowTaper.Dirichlet, default, policy.Levels, 0.0, FourierOptions.Default, policy.Family, policy.Extension);

    internal Fin<Arr<double>> Taper() => Window.Of(Dimension.Create(value: Grid.Frame), TaperFraming.FftFrame);

    internal FrameGrid Segments(int samples) => FrameGrid.Interior(Grid.Frame, Grid.Hop, samples);
}

public sealed record Spectrum(
    SpectralTransform Transform, ReadOnlyMemory<double> Magnitude, Option<ReadOnlyMemory<double>> Phase,
    int Length, int Samples, double BinHz, FourierOptions Scaling, Instant At) {
    public double SampleRate => BinHz * Samples;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Spectrogram {
    private Spectrogram() { }

    public sealed record Phasor(SpectralTransform Transform, FrameGrid Grid, int Samples, int CoverFrom, int CoverTo, double BinHz, WindowTaper Window, FourierOptions Scaling, ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase, Instant At) : Spectrogram;
    public sealed record Power(SpectralTransform Transform, FrameGrid Grid, double BinHz, ReadOnlyMemory<double> Values, Instant At) : Spectrogram;

    public SpectralTransform Transform => Switch(
        phasor: static value => value.Transform,
        power: static value => value.Transform);
}

[Equatable]
public sealed partial record WaveletDecomposition(
    WaveletFamily Family, WaveletExtension Extension, int Levels, ReadOnlyMemory<double> Approximation,
    Seq<ReadOnlyMemory<double>> Details, [property: OrderedEquality] ImmutableArray<int> Extents, Instant At);

public sealed record CrossPower(int Channels, int Bins, int Segments, double BinHz, double[] Real, double[] Imaginary) {
    internal int Cell(int bin, int row, int column) => (bin * Channels + row) * Channels + column;
    internal double Auto(int bin, int channel) => Real[Cell(bin, channel, channel)];
}

public sealed record CrossSpectrum(int Bins, double BinHz, ReadOnlyMemory<double> AutoX, ReadOnlyMemory<double> AutoY, ReadOnlyMemory<double> CrossMagnitude, ReadOnlyMemory<double> CrossPhase, ReadOnlyMemory<double> Coherence, Instant At) {
    public (ReadOnlyMemory<double> Magnitude, ReadOnlyMemory<double> Phase) Transfer() {
        double[] magnitude = new double[Bins];
        for (int k = 0; k < Bins; k++) { magnitude[k] = CrossMagnitude.Span[k] / Math.Max(1e-300, AutoX.Span[k]); }
        return (magnitude, CrossPhase);
    }
}

public sealed record MeasuredMode(double FrequencyHz, Option<double> DampingRatio, ReadOnlyMemory<double> ShapeMagnitude, ReadOnlyMemory<double> ShapePhase, double Singular);

public sealed record ModalEstimate(int Channels, int Bins, double BinHz, ReadOnlyMemory<double> SingularSpectrum, Seq<MeasuredMode> Modes, Instant At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelQuery {
    private ChannelQuery() { }

    public sealed record Dependence(DependenceKind Kind) : ChannelQuery;
    public sealed record Distribution(DensityKernel Kernel, int Support, Option<double> Bandwidth) : ChannelQuery;

    internal int Floor => Switch(dependence: static _ => 2, distribution: static _ => 1);
}

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralOutput {
    private SpectralOutput() { }

    public sealed record Bins(Spectrum Spectrum) : SpectralOutput;
    public sealed record Frames(Spectrogram Spectrogram) : SpectralOutput;
    public sealed record Bands(WaveletDecomposition Decomposition) : SpectralOutput;
}

internal readonly record struct QmfShift(int Analysis, int Synthesis) {
    internal static QmfShift Of(int taps) => new(taps / 2 - 1, taps - 1 - (taps / 2 - 1));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Transform {
    public static IO<Fin<SpectralOutput>> Apply(SpectralTransform transform, ReadOnlyMemory<float> signal, SignalPolicy policy, IClock clock) =>
        Admitted(Seq1(signal), floor: 1).Bind(planes => transform.Admit(policy, signal.Length).Map(context => (Plane: planes[0], Context: context))).Match(
            Succ: admitted => transform.Run(admitted.Plane, admitted.Context, clock.GetCurrentInstant()),
            Fail: static error => IO.pure(Fin.Fail<SpectralOutput>(error)));

    public static IO<Fin<ReadOnlyMemory<float>>> Invert(SpectralOutput output) =>
        output.Switch(
            bins: static b => b.Spectrum.Transform,
            frames: static f => f.Spectrogram.Transform,
            bands: static _ => SpectralTransform.Dwt).Invert(output);

    public static Fin<ChannelEvidence> Describe(Seq<ReadOnlyMemory<float>> channels, ChannelQuery query, IClock clock) =>
        Admitted(channels, query.Floor).Bind(rows => query.Switch(
            state: (Rows: rows, At: clock.GetCurrentInstant()),
            dependence: static (s, q) => Fin.Succ<ChannelEvidence>(new ChannelEvidence.Dependence(q.Kind, q.Kind.AllPairs(s.Rows), s.At)),
            distribution: static (s, q) => q.Support < 2
                ? Fin.Fail<ChannelEvidence>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))
                : Fin.Succ<ChannelEvidence>(new ChannelEvidence.Distribution(
                    q.Kernel, s.Rows.Map(row => Distribution(row, q)), s.At))));

    private static Fin<Seq<double[]>> Admitted(Seq<ReadOnlyMemory<float>> channels, int floor) =>
        channels.Count < floor
            ? Fin.Fail<Seq<double[]>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(channels.Count, floor))))
        : channels.Exists(row => row.Length != channels[0].Length)
            ? Fin.Fail<Seq<double[]>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Shape(
                ShapeRequirement.Dimensions,
                new ShapeEvidence.Count(channels.Find(row => row.Length != channels[0].Length).Map(static row => row.Length).IfNone(0), channels[0].Length))))
        : channels[0].Length == 0
            ? Fin.Fail<Seq<double[]>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input)))
        : channels.Exists(static row => !TensorPrimitives.IsFiniteAll<float>(row.Span))
            ? Fin.Fail<Seq<double[]>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(channels.Sum(static row => row.Length)))))
            : Fin.Succ(channels.Map(static row => {
                double[] widened = new double[row.Length];
                TensorPrimitives.ConvertChecked<float, double>(row.Span, widened);
                return widened;
            }));

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

    // --- [CROSS_POWER]

    public static Fin<CrossSpectrum> Coherence(CrossPower power, int left, int right, Instant at) {
        if (left == right || (uint)left >= (uint)power.Channels || (uint)right >= (uint)power.Channels) {
            return Fin.Fail<CrossSpectrum>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())));
        }
        (double[] crossMag, double[] crossPhase, double[] coherence) = (new double[power.Bins], new double[power.Bins], new double[power.Bins]);
        (double[] autoX, double[] autoY) = (new double[power.Bins], new double[power.Bins]);
        for (int k = 0; k < power.Bins; k++) {
            int cell = power.Cell(k, left, right);
            double re = power.Real[cell], im = power.Imaginary[cell];
            (autoX[k], autoY[k]) = (power.Auto(k, left) / power.Segments, power.Auto(k, right) / power.Segments);
            crossMag[k] = double.Hypot(re, im) / power.Segments;
            crossPhase[k] = Math.Atan2(im, re);
            coherence[k] = Math.Clamp((re * re + im * im) / Math.Max(1e-300, power.Auto(k, left) * power.Auto(k, right)), 0.0, 1.0);
        }
        return Fin.Succ(new CrossSpectrum(power.Bins, power.BinHz, autoX, autoY, crossMag, crossPhase, coherence, at));
    }

    public static Fin<CrossSpectrum> Coherence(ReadOnlyMemory<float> x, ReadOnlyMemory<float> y, SignalPolicy policy, IClock clock) =>
        Power(Seq(x, y), policy, clock).Bind(power => Coherence(power, left: 0, right: 1, clock.GetCurrentInstant()));

    public static Fin<ModalEstimate> Modal(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock) =>
        Power(channels, policy, clock).Bind(power => Modal(power, clock.GetCurrentInstant()));

    public static Fin<ModalEstimate> Modal(CrossPower power, Instant at) {
        int n = power.Channels;
        double[] s1 = new double[power.Bins];
        (double[] vre, double[] vim) = (new double[power.Bins * n], new double[power.Bins * n]);
        for (int k = 0; k < power.Bins; k++) { s1[k] = Dominant(power, k, vre.AsSpan(k * n, n), vim.AsSpan(k * n, n)) / power.Segments; }
        return Distribution<Scalar>.Of(toSeq(s1.Select(static v => (Scalar)v)), Seq<double>(), Op.Of(name: nameof(Modal)))
            .Map(spectrum => {
                double floor = ModalPeakFloor * spectrum.Median.To();
                Seq<MeasuredMode> modes = Seq<MeasuredMode>();
                for (int k = 1; k < power.Bins - 1; k++) {
                    if (s1[k] <= floor || s1[k] <= s1[k - 1] || s1[k] < s1[k + 1]) { continue; }
                    (double[] magnitude, double[] phase) = (new double[n], new double[n]);
                    TensorPrimitives.Hypot(vre.AsSpan(k * n, n), vim.AsSpan(k * n, n), magnitude);
                    TensorPrimitives.Atan2(vim.AsSpan(k * n, n), vre.AsSpan(k * n, n), phase);
                    modes = modes.Add(new MeasuredMode(k * power.BinHz, HalfPower(s1, k, power.BinHz), magnitude, phase, s1[k]));
                }
                return new ModalEstimate(n, power.Bins, power.BinHz, s1, modes, at);
            });
    }

    // --- [CORPUS]

    public static Fin<CrossPower> Power(WaveformCorpus corpus, WindowTaper window, Instant at) =>
        corpus.Channels < 2
            ? Fin.Fail<CrossPower>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(corpus.Channels, 2L))))
        : corpus.FrameCount < 2
            ? Fin.Fail<CrossPower>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Capacity(CapacityRequirement.Sufficient, new CapacityEvidence.Count(corpus.FrameCount, 2L))))
        : !TensorPrimitives.IsFiniteAll<float>(corpus.Frames.Span)
            ? Fin.Fail<CrossPower>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(corpus.Frames.Length))))
            : window.Of(Dimension.Create(value: corpus.Window.Frame), TaperFraming.FftFrame).Map(taper => {
                int n = corpus.Channels, frame = corpus.Window.Frame, bins = frame / 2 + 1;
                (double[] gre, double[] gim) = (new double[bins * n * n], new double[bins * n * n]);
                CrossPower power = new(n, bins, corpus.FrameCount, corpus.SampleRate / frame, gre, gim);
                (double[][] real, double[][] imaginary) = (
                    [.. Enumerable.Range(0, n).Select(_ => new double[frame])],
                    [.. Enumerable.Range(0, n).Select(_ => new double[frame])]);
                for (int f = 0; f < corpus.FrameCount; f++) {
                    ReadOnlySpan<float> block = corpus.Frames.Span.Slice(f * frame * n, frame * n);
                    for (int c = 0; c < n; c++) {
                        for (int i = 0; i < frame; i++) { real[c][i] = block[i * n + c] * taper[i]; }
                        Array.Clear(imaginary[c]);
                        Fourier.Forward(real[c], imaginary[c], FourierOptions.NoScaling);
                    }
                    Hermitian(power, real, imaginary);
                }
                return power;
            });

    public static Fin<CrossPower> Power(Seq<ReadOnlyMemory<float>> channels, SignalPolicy policy, IClock clock) =>
        Admitted(channels, floor: 2).Bind(planes => SpectralTransform.WelchPsd.Admit(policy, channels[0].Length).Bind(context => {
            FrameGrid grid = context.Segments(channels[0].Length);
            return grid.Frames < 2
                ? Fin.Fail<CrossPower>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Counts(channels[0].Length, grid.Frame, grid.Hop))))
                : context.Taper().Map(taper => {
                    int n = planes.Count;
                    (double[] gre, double[] gim) = (new double[grid.Bins * n * n], new double[grid.Bins * n * n]);
                    CrossPower seed = new(n, grid.Bins, grid.Frames, BinHz(grid.Frame, context.SampleRate), gre, gim);
                    return Framed(planes, grid, taper, FourierOptions.NoScaling, seed, static (power, _, real, imaginary) => {
                        Hermitian(power, real, imaginary);
                        return power;
                    });
                });
        }));

    private static void Hermitian(CrossPower power, double[][] real, double[][] imaginary) {
        for (int k = 0; k < power.Bins; k++) {
            for (int i = 0; i < power.Channels; i++) {
                for (int j = 0; j < power.Channels; j++) {
                    int cell = power.Cell(k, i, j);
                    power.Real[cell] += real[i][k] * real[j][k] + imaginary[i][k] * imaginary[j][k];
                    power.Imaginary[cell] += imaginary[i][k] * real[j][k] - real[i][k] * imaginary[j][k];
                }
            }
        }
    }

    private const int PowerIterationCap = 512;
    private const double ModalPeakFloor = 8.0;

    private static double Dominant(CrossPower power, int bin, Span<double> vre, Span<double> vim) {
        int n = power.Channels, seed = 0;
        for (int i = 1; i < n; i++) { if (power.Auto(bin, i) > power.Auto(bin, seed)) { seed = i; } }
        vre.Clear(); vim.Clear(); vre[seed] = 1.0;
        Span<double> wre = stackalloc double[n], wim = stackalloc double[n];
        double lambda = 0.0;
        for (int iteration = 0; iteration < PowerIterationCap; iteration++) {
            for (int i = 0; i < n; i++) {
                (wre[i], wim[i]) = (0.0, 0.0);
                for (int j = 0; j < n; j++) {
                    int cell = power.Cell(bin, i, j);
                    wre[i] += power.Real[cell] * vre[j] - power.Imaginary[cell] * vim[j];
                    wim[i] += power.Real[cell] * vim[j] + power.Imaginary[cell] * vre[j];
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

    private static Option<double> HalfPower(double[] s1, int peak, double binHz) {
        double half = s1[peak] / 2.0;
        int lo = peak, hi = peak;
        while (lo > 0 && s1[lo] > half) { lo--; }
        while (hi < s1.Length - 1 && s1[hi] > half) { hi++; }
        return s1[lo] <= half && s1[hi] <= half && peak > 0
            ? Some((hi - lo) * binHz / (2.0 * peak * binHz))
            : None;
    }

    // --- [FRAMED]

    private static TState Framed<TState>(
        Seq<double[]> planes, FrameGrid grid, Arr<double> taper, FourierOptions scaling,
        TState seed, Func<TState, int, double[][], double[][], TState> reduce) {
        int n = planes.Count;
        double[][] real = [.. Enumerable.Range(0, n).Select(_ => new double[grid.Frame])];
        double[][] imaginary = [.. Enumerable.Range(0, n).Select(_ => new double[grid.Frame])];
        TState state = seed;
        for (int f = 0; f < grid.Frames; f++) {
            int offset = f * grid.Hop - grid.Left;
            for (int c = 0; c < n; c++) {
                double[] plane = planes[c];
                for (int i = 0; i < grid.Frame; i++) {
                    int sample = offset + i;
                    real[c][i] = 0 <= sample && sample < plane.Length ? plane[sample] * taper[i] : 0.0;
                }
                Array.Clear(imaginary[c]);
                Fourier.Forward(real[c], imaginary[c], scaling);
            }
            state = reduce(state, f, real, imaginary);
        }
        return state;
    }

    // --- [SPECTRAL]

    internal static IO<Fin<SpectralOutput>> PerBin(double[] plane, SignalContext policy, Instant at) {
        int n = plane.Length;
        double[] real = [.. plane];
        double[] imaginary = new double[n];
        double[] magnitude = new double[n];
        double[] phase = new double[n];
        Fourier.Forward(real, imaginary, policy.Scaling);
        TensorPrimitives.Hypot(real, imaginary, magnitude);
        TensorPrimitives.Atan2(imaginary, real, phase);
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(
            policy.Transform, magnitude, Some<ReadOnlyMemory<double>>(phase), n, n, BinHz(n, policy.SampleRate), policy.Scaling, at))));
    }

    internal static IO<Fin<SpectralOutput>> RealForward(double[] plane, SignalContext policy, Instant at) {
        int n = plane.Length, bins = n / 2 + 1;
        double[] data = new double[n + 2];
        plane.CopyTo(data.AsSpan(0, n));
        Fourier.ForwardReal(data, n, policy.Scaling);
        double[] real = new double[bins];
        double[] imaginary = new double[bins];
        double[] magnitude = new double[bins];
        double[] phase = new double[bins];
        for (int k = 0; k < bins; k++) { real[k] = data[2 * k]; imaginary[k] = data[2 * k + 1]; }
        TensorPrimitives.Hypot(real, imaginary, magnitude);
        TensorPrimitives.Atan2(imaginary, real, phase);
        return IO.pure(Fin.Succ<SpectralOutput>(new SpectralOutput.Bins(new Spectrum(
            policy.Transform, magnitude, Some<ReadOnlyMemory<double>>(phase), bins, n, BinHz(n, policy.SampleRate), policy.Scaling, at))));
    }

    internal static IO<Fin<SpectralOutput>> ShortTime(double[] plane, SignalContext policy, Instant at) =>
        IO.pure(policy.Taper().Map(taper => {
            FrameGrid grid = policy.Grid;
            double[] magnitude = new double[grid.Frames * grid.Bins];
            double[] phase = new double[grid.Frames * grid.Bins];
            ignore(Framed(Seq1(plane), grid, taper, policy.Scaling, unit, (_, f, real, imaginary) => {
                TensorPrimitives.Hypot(real[0].AsSpan(0, grid.Bins), imaginary[0].AsSpan(0, grid.Bins), magnitude.AsSpan(f * grid.Bins, grid.Bins));
                TensorPrimitives.Atan2(imaginary[0].AsSpan(0, grid.Bins), real[0].AsSpan(0, grid.Bins), phase.AsSpan(f * grid.Bins, grid.Bins));
                return unit;
            }));
            (int coverFrom, int coverTo) = Coverage(taper, plane.Length, grid);
            return (SpectralOutput)new SpectralOutput.Frames(new Spectrogram.Phasor(
                policy.Transform, grid, plane.Length, coverFrom, coverTo,
                BinHz(grid.Frame, policy.SampleRate), policy.Window, policy.Scaling, magnitude, phase, at));
        }));

    private static (int From, int To) Coverage(Arr<double> taper, int samples, FrameGrid grid) {
        double[] weight = new double[samples];
        for (int f = 0; f < grid.Frames; f++) {
            int offset = f * grid.Hop - grid.Left;
            for (int i = 0; i < grid.Frame; i++) {
                int sample = offset + i;
                if (0 <= sample && sample < samples) { weight[sample] += taper[i] * taper[i]; }
            }
        }
        int from = 0, to = samples;
        while (from < to && weight[from] <= WindowMassFloor) { from++; }
        while (to > from && weight[to - 1] <= WindowMassFloor) { to--; }
        return (from, to);
    }

    private const double WindowMassFloor = 1e-24;

    internal static IO<Fin<SpectralOutput>> PowerSpectrogram(double[] plane, SignalContext policy, Instant at) =>
        ShortTime(plane, policy, at).Map(static result => result.Bind(output =>
            output is SpectralOutput.Frames { Spectrogram: Spectrogram.Phasor frames }
                ? Fin.Succ<SpectralOutput>(new SpectralOutput.Frames(new Spectrogram.Power(
                    SpectralTransform.Spectrogram, frames.Grid, frames.BinHz, Squared(frames.Magnitude), frames.At)))
                : Fin.Fail<SpectralOutput>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None())))));

    private static ReadOnlyMemory<double> Squared(ReadOnlyMemory<double> magnitude) {
        double[] power = magnitude.ToArray();
        TensorPrimitives.Multiply(power, power, power);
        return power;
    }

    internal static IO<Fin<SpectralOutput>> Welch(double[] plane, SignalContext policy, Instant at) =>
        IO.pure(policy.Taper().Map(taper => {
            FrameGrid grid = policy.Segments(plane.Length);
            double norm = 1.0 / (policy.SampleRate * TensorPrimitives.SumOfSquares<double>(taper.AsSpan()) * grid.Frames);
            double[] psd = Framed(Seq1(plane), grid, taper, FourierOptions.NoScaling, new double[grid.Bins], (acc, _, real, imaginary) => {
                for (int k = 0; k < acc.Length; k++) {
                    double mm = real[0][k] * real[0][k] + imaginary[0][k] * imaginary[0][k];
                    acc[k] += (k == 0 || ((grid.Frame & 1) == 0 && k == acc.Length - 1) ? 1.0 : 2.0) * mm * norm;
                }
                return acc;
            });
            return (SpectralOutput)new SpectralOutput.Bins(new Spectrum(
                policy.Transform, psd, None, grid.Bins, grid.Frame, BinHz(grid.Frame, policy.SampleRate), FourierOptions.NoScaling, at));
        }));

    private readonly record struct CascadeState(double[] Approx, Seq<ReadOnlyMemory<double>> Details, Seq<int> Extents);

    internal static IO<Fin<SpectralOutput>> Wavelet(double[] plane, SignalContext policy, Instant at) =>
        Cascade(new CascadeState(plane, Seq<ReadOnlyMemory<double>>(), Seq<int>()), 0, policy)
            .Map(state => (SpectralOutput)new SpectralOutput.Bands(new WaveletDecomposition(
                policy.Wavelet, policy.Extension, state.Details.Count, state.Approx, state.Details, [.. state.Extents], at)))
            .Run().As();

    private static FinT<IO, CascadeState> Cascade(CascadeState state, int level, SignalContext policy) =>
        level >= policy.Levels || state.Approx.Length < policy.Wavelet.LowPass.Length
            ? FinT<IO, CascadeState>.Succ(state)
            : from band in Convolve2(state.Approx, policy)
              from next in Cascade(
                  new CascadeState(band.Low, ((ReadOnlyMemory<double>)band.High).Cons(state.Details), state.Extents.Add(state.Approx.Length)),
                  level + 1, policy)
              select next;

    private static FinT<IO, (double[] Low, double[] High)> Convolve2(double[] x, SignalContext policy) =>
        from low in Downsample(x, policy.Wavelet.LowPass.ToArray(), policy.Extension)
        from high in Downsample(x, policy.Wavelet.HighPass(), policy.Extension)
        select (low, high);

    private static FinT<IO, double[]> Downsample(double[] x, double[] filter, WaveletExtension extension) {
        int taps = filter.Length, band = (x.Length + 1) / 2;
        QmfShift shift = QmfShift.Of(taps);
        double[] z = extension.Extend(x, shift.Analysis, taps - 1 - shift.Analysis);
        return FinT.liftIO(KernelLowering.Lower(
                TensorOpFamily.Conv,
                new LoweringOperands.Windowed(
                    Matrix<double>.Build.Dense(1, z.Length, (_, c) => z[c]),
                    Matrix<double>.Build.Dense(taps, 1, (r, _) => filter[taps - 1 - r]),
                    new ConvWindow([taps], [2], [0], [1], 1, 1, [z.Length])),
                new ShardDispatch.Local())
            .Map(result => result.Map(outcome => outcome.Solution.ToColumnMajorArray()[..band])));
    }

    // --- [INVERSE]

    internal static IO<Fin<ReadOnlyMemory<float>>> InvertBins(SpectralOutput output) =>
        IO.pure(Carrier<SpectralOutput.Bins>(output).Bind(bins => Phased(bins.Spectrum).Map(planes => {
            int n = bins.Spectrum.Samples;
            Fourier.Inverse(planes.Real, planes.Imaginary, bins.Spectrum.Scaling);
            double scale = bins.Spectrum.Scaling == FourierOptions.NoScaling ? 1.0 / n : 1.0;
            float[] samples = new float[n];
            for (int i = 0; i < n; i++) { samples[i] = (float)(planes.Real[i] * scale); }
            return (ReadOnlyMemory<float>)samples;
        })));

    internal static IO<Fin<ReadOnlyMemory<float>>> InvertPacked(SpectralOutput output) =>
        IO.pure(Carrier<SpectralOutput.Bins>(output).Bind(bins => Phased(bins.Spectrum).Map(planes => {
            int n = bins.Spectrum.Samples;
            double[] data = new double[n + 2];
            for (int k = 0; k < bins.Spectrum.Length; k++) { (data[2 * k], data[2 * k + 1]) = (planes.Real[k], planes.Imaginary[k]); }
            Fourier.InverseReal(data, n, bins.Spectrum.Scaling);
            float[] samples = new float[n];
            for (int i = 0; i < n; i++) { samples[i] = (float)data[i]; }
            return (ReadOnlyMemory<float>)samples;
        })));

    private static Fin<(double[] Real, double[] Imaginary)> Phased(Spectrum spectrum) =>
        spectrum.Phase.ToFin(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Required(ComputeSubject.Input))).Map(phase => {
            int width = Math.Max(spectrum.Samples, spectrum.Length);
            double[] real = new double[width];
            double[] imaginary = new double[width];
            for (int k = 0; k < spectrum.Length; k++) {
                real[k] = spectrum.Magnitude.Span[k] * Math.Cos(phase.Span[k]);
                imaginary[k] = spectrum.Magnitude.Span[k] * Math.Sin(phase.Span[k]);
            }
            return (real, imaginary);
        });

    internal static IO<Fin<ReadOnlyMemory<float>>> InvertFrames(SpectralOutput output) =>
        IO.pure(Carrier<SpectralOutput.Frames>(output)
            .Bind(frames => frames.Spectrogram is Spectrogram.Phasor phasor
                ? Fin.Succ(phasor)
                : Fin.Fail<Spectrogram.Phasor>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))))
            .Bind(frames => frames.Window.Of(Dimension.Create(value: frames.Grid.Frame), TaperFraming.FftFrame).Bind(taper => {
                FrameGrid grid = frames.Grid;
                double[] sum = new double[frames.Samples];
                double[] weight = new double[frames.Samples];
                double[] real = new double[grid.Frame];
                double[] imaginary = new double[grid.Frame];
                for (int f = 0; f < grid.Frames; f++) {
                    Array.Clear(real);
                    Array.Clear(imaginary);
                    int row = f * grid.Bins;
                    for (int k = 0; k < grid.Bins; k++) {
                        double magnitude = frames.Magnitude.Span[row + k];
                        double phase = frames.Phase.Span[row + k];
                        real[k] = magnitude * Math.Cos(phase);
                        imaginary[k] = magnitude * Math.Sin(phase);
                    }
                    int mirrored = (grid.Frame & 1) == 0 ? grid.Bins - 1 : grid.Bins;
                    for (int k = 1; k < mirrored; k++) {
                        real[grid.Frame - k] = real[k];
                        imaginary[grid.Frame - k] = -imaginary[k];
                    }
                    Fourier.Inverse(real, imaginary, frames.Scaling);
                    double scale = frames.Scaling == FourierOptions.NoScaling ? 1.0 / grid.Frame : 1.0;
                    int offset = f * grid.Hop - grid.Left;
                    for (int i = 0; i < grid.Frame; i++) {
                        int sample = offset + i;
                        if (0 <= sample && sample < frames.Samples) {
                            double window = taper[i];
                            sum[sample] += real[i] * scale * window;
                            weight[sample] += window * window;
                        }
                    }
                }
                if (frames.CoverTo <= frames.CoverFrom) { return Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Count(frames.CoverFrom, frames.CoverTo)))); }
                float[] samples = new float[frames.CoverTo - frames.CoverFrom];
                for (int i = 0; i < samples.Length; i++) { samples[i] = (float)(sum[frames.CoverFrom + i] / weight[frames.CoverFrom + i]); }
                return Fin.Succ<ReadOnlyMemory<float>>(samples);
            })));

    internal static IO<Fin<ReadOnlyMemory<float>>> NonInvertible(SpectralOutput output) =>
        IO.pure(Fin.Fail<ReadOnlyMemory<float>>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Supported, new ContractEvidence.None()))));

    internal static IO<Fin<ReadOnlyMemory<float>>> Synthesize(SpectralOutput output) =>
        Carrier<SpectralOutput.Bands>(output).Match(
            Succ: bands => toSeq(bands.Decomposition.Details.Zip(toSeq(bands.Decomposition.Extents.Reverse()), static (d, e) => (Detail: d, Extent: e)))
                .Fold(
                    FinT<IO, double[]>.Succ(bands.Decomposition.Approximation.ToArray()),
                    (rail, step) => rail.Bind(approximation => Reconstruct(approximation, step.Detail.ToArray(), bands.Decomposition, step.Extent)))
                .Map(static a => {
                    float[] samples = new float[a.Length];
                    for (int i = 0; i < a.Length; i++) { samples[i] = (float)a[i]; }
                    return (ReadOnlyMemory<float>)samples;
                })
                .Run().As(),
            Fail: static error => IO.pure(Fin.Fail<ReadOnlyMemory<float>>(error)));

    private static FinT<IO, double[]> Reconstruct(double[] approx, double[] detail, WaveletDecomposition w, int extent) =>
        from low in Resample(approx, w.Family.LowPass.ToArray(), w.Extension, extent)
        from high in Resample(detail, w.Family.HighPass(), w.Extension, extent)
        select Merged(low, high, extent);

    private static double[] Merged(double[] low, double[] high, int extent) {
        double[] merged = new double[extent];
        int span = Math.Min(extent, Math.Min(low.Length, high.Length));
        TensorPrimitives.Add(low.AsSpan(0, span), high.AsSpan(0, span), merged.AsSpan(0, span));
        return merged;
    }

    private static FinT<IO, double[]> Resample(double[] x, double[] filter, WaveletExtension extension, int extent) {
        int taps = filter.Length;
        QmfShift shift = QmfShift.Of(taps);
        double[] up = new double[2 * x.Length];
        for (int i = 0; i < x.Length; i++) { up[2 * i] = x[i]; }
        double[] z = extension.Extend(up, shift.Synthesis, taps - 1 - shift.Synthesis);
        return FinT.liftIO(KernelLowering.Lower(
                TensorOpFamily.Conv,
                new LoweringOperands.Windowed(
                    Matrix<double>.Build.Dense(1, z.Length, (_, c) => z[c]),
                    Matrix<double>.Build.Dense(taps, 1, (r, _) => filter[r]),
                    new ConvWindow([taps], [1], [0], [1], 1, 1, [z.Length])),
                new ShardDispatch.Local())
            .Map(result => result.Map(outcome => outcome.Solution.ToColumnMajorArray()[..Math.Min(extent, 2 * x.Length)])));
    }

    // --- [HELPERS]

    private static Fin<TCase> Carrier<TCase>(SpectralOutput output) where TCase : SpectralOutput =>
        output is TCase held
            ? Fin.Succ(held)
            : Fin.Fail<TCase>(new ComputeFault.Violation(ComputeArea.Stats, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Type(typeof(TCase)))));

    private static double BinHz(int length, double sampleRate) => Fourier.FrequencyScale(length, sampleRate) is [_, double second, ..] ? second : sampleRate / length;
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
