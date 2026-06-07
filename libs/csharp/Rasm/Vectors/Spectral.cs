using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SpectralAssemblyKind {
    public static readonly SpectralAssemblyKind Dec = new(key: 0);
    public static readonly SpectralAssemblyKind EdgeConnection = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralScaleNormalization {
    public static readonly SpectralScaleNormalization Raw = new(key: 0);
    public static readonly SpectralScaleNormalization FirstNonZeroEigenvalue = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralEnergyNormalization {
    public static readonly SpectralEnergyNormalization Raw = new(key: 0);
    public static readonly SpectralEnergyNormalization UnitL1 = new(key: 1);
    public static readonly SpectralEnergyNormalization UnitL2 = new(key: 2);
    public static readonly SpectralEnergyNormalization ZScore = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SpectralZeroModePolicy {
    public static readonly SpectralZeroModePolicy Keep = new(key: 0);
    public static readonly SpectralZeroModePolicy Drop = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class SpectralDistanceKind {
    public static readonly SpectralDistanceKind Euclidean = new(key: 0);
    public static readonly SpectralDistanceKind Manhattan = new(key: 1);
    public static readonly SpectralDistanceKind Cosine = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SpectralTieBreak { public static readonly SpectralTieBreak InputOrder = new(key: 0); }

[Union]
public abstract partial record SpectralFilter {
    public sealed record HeatCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record WaveCase(PositiveMagnitude Energy, PositiveMagnitude Bandwidth) : SpectralFilter;
    public sealed record BiharmonicCase : SpectralFilter;
    public sealed record DiffusionCase(PositiveMagnitude Time) : SpectralFilter;
    public sealed record CommuteTimeCase : SpectralFilter;
    public sealed record IdentityCase : SpectralFilter;
    private SpectralFilter() { }
    public static SpectralFilter Heat(PositiveMagnitude time) => new HeatCase(Time: time);
    public static SpectralFilter Wave(PositiveMagnitude energy, PositiveMagnitude bandwidth) => new WaveCase(Energy: energy, Bandwidth: bandwidth);
    public static SpectralFilter Biharmonic => new BiharmonicCase();
    public static SpectralFilter Diffusion(PositiveMagnitude time) => new DiffusionCase(Time: time);
    public static SpectralFilter CommuteTime => new CommuteTimeCase();
    public static SpectralFilter Identity => new IdentityCase();
    public Option<SpectralFilter> Compose(SpectralFilter other) =>
        (this, other) switch {
            (HeatCase a, HeatCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Heat(time: time)),
            (DiffusionCase a, DiffusionCase b) => Positive(value: a.Time.Value + b.Time.Value).Map(static time => Diffusion(time: time)),
            (IdentityCase, _) when other is not null => Some(other),
            (_, IdentityCase) => Some(this),
            _ => Option<SpectralFilter>.None,
        };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double Weight(double eigenvalue) => Switch(
        state: eigenvalue,
        heatCase: static (lambda, c) => Math.Exp(d: -c.Time.Value * lambda),
        waveCase: static (lambda, c) => RawWaveWeight(eigenvalue: lambda, energy: c.Energy.Value, bandwidth: c.Bandwidth.Value),
        biharmonicCase: static (lambda, _) => lambda > RhinoMath.SqrtEpsilon ? 1.0 / (lambda * lambda) : 0.0,
        diffusionCase: static (lambda, c) => Math.Exp(d: -2.0 * c.Time.Value * lambda),
        commuteTimeCase: static (lambda, _) => lambda > RhinoMath.SqrtEpsilon ? 1.0 / lambda : 0.0,
        identityCase: static (_, _) => 1.0);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, Op key) =>
        ApplyDetailed(basis: basis, sources: sources, policy: SpectralDescriptorPolicy.Raw, key: key);
    internal Fin<SpectralDescriptor> ApplyDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralDescriptorPolicy policy, Op key) =>
        Optional(this).ToFin(key.InvalidInput())
                .Bind(filter => guard(basis.IsValid, key.InvalidInput())
                .Bind(_ => SpectralDescriptorPolicy.Admit(policy: policy, key: key))
                .Bind(activePolicy => SpectralCore.EvaluateFilteredDetailed(basis: basis, sources: sources, filter: filter, policy: activePolicy, key: key)));
    private static double RawWaveWeight(double eigenvalue, double energy, double bandwidth) =>
        eigenvalue < RhinoMath.SqrtEpsilon ? 0.0 : ((Math.Log(d: energy) - Math.Log(d: eigenvalue)) / Math.Max(val1: bandwidth, val2: SpectralCore.WaveBandwidthFloor)) switch { double ratio => Math.Exp(d: -0.5 * ratio * ratio) };
    private static Option<PositiveMagnitude> Positive(double value) =>
        PositiveMagnitude.TryCreate(value: value, obj: out PositiveMagnitude magnitude) ? Some(magnitude) : Option<PositiveMagnitude>.None;
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralAssemblyReceipt(int VertexCount, int EdgeCount, int FaceCount, int AdmittedFaceCount, int SkippedDegenerateFaces, int SkippedMissingEdges, int SkippedInvalidNormals, int SkippedInvalidTangents, bool FlippedIntrinsicRejected, int MatrixRows, int MatrixCols, int NonZeros, int PositiveStar0Count, int PositiveStar1Count, int PositiveStar2Count, double BoundaryCompositionResidual, Option<int> Genus, int HarmonicDimension, SpectralAssemblyKind Kind, int BoundaryEdgeCount = 0, int BoundaryComponentCount = 0, int NonManifoldEdgeCount = 0, int EulerCharacteristic = 0, bool TopologyEulerValidated = false, int ComponentCount = 1, int PositiveMassCount = 0, double SymmetryResidual = 0.0, Option<int> FactorNonZeros = default) {
    internal bool IsValid =>
        Kind is SpectralAssemblyKind kind
        && new[] { VertexCount, EdgeCount, FaceCount, AdmittedFaceCount, SkippedDegenerateFaces, SkippedMissingEdges, SkippedInvalidNormals, SkippedInvalidTangents, MatrixRows, MatrixCols, NonZeros, PositiveStar0Count, PositiveStar1Count, PositiveStar2Count, HarmonicDimension, BoundaryEdgeCount, BoundaryComponentCount, NonManifoldEdgeCount, ComponentCount, PositiveMassCount }.All(static count => count >= 0)
        && AdmittedFaceCount + SkippedDegenerateFaces + SkippedMissingEdges <= FaceCount
        && RhinoMath.IsValidDouble(x: BoundaryCompositionResidual)
        && RhinoMath.IsValidDouble(x: SymmetryResidual)
        && FactorNonZeros.Map(static value => value > 0).IfNone(noneValue: true)
        && (kind.Equals(SpectralAssemblyKind.EdgeConnection)
            ? ComponentCount == 2 && MatrixRows == EdgeCount * ComponentCount && MatrixCols == MatrixRows && PositiveMassCount <= EdgeCount
            : ComponentCount == 1 && PositiveStar0Count <= VertexCount && PositiveStar1Count <= EdgeCount && PositiveStar2Count <= FaceCount && (Genus is { IsSome: true, Case: int genus } ? genus >= 0 && HarmonicDimension == 2 * genus : HarmonicDimension == 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormReceipt(Option<int> Genus, int ExpectedDimension, int ConstraintRows, int EdgeCount, int Rank, int Nullity, int BasisCount, double SvdTolerance, double MinNullEigenvalue, double MaxNullEigenvalue, double MaxClosedResidual, double MaxCoClosedResidual, double Star1OrthonormalResidual, int PositiveStar1Count, EigenSolveReceipt<double, Arr<double>> Eigen) {
    public bool IsValid {
        get {
            int expected = ExpectedDimension;
            double tolerance = SvdTolerance;
            return new[] { expected, ConstraintRows, EdgeCount, Rank, Nullity, BasisCount, PositiveStar1Count }.All(static value => value >= 0)
                && Rank + Nullity == EdgeCount
                && BasisCount == expected
                && Nullity >= expected
                && PositiveStar1Count <= EdgeCount
                && Genus.Map(genus => expected == 2 * genus).IfNone(expected == 0)
                && new[] { tolerance, MinNullEigenvalue, MaxNullEigenvalue, MaxClosedResidual, MaxCoClosedResidual, Star1OrthonormalResidual }.All(RhinoMath.IsValidDouble)
                && tolerance > 0.0
                && MinNullEigenvalue >= -RhinoMath.SqrtEpsilon
                && MaxNullEigenvalue >= MinNullEigenvalue - RhinoMath.SqrtEpsilon
                && MaxClosedResidual <= Math.Max(val1: 1.0e-7, val2: tolerance * 1.0e3)
                && MaxCoClosedResidual <= Math.Max(val1: 1.0e-7, val2: tolerance * 1.0e3)
                && Star1OrthonormalResidual <= Math.Max(val1: 1.0e-7, val2: tolerance * 1.0e3)
                && Eigen.IsUsable;
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct HarmonicOneFormBasis(Arr<Arr<double>> Forms, HarmonicOneFormReceipt Receipt) {
    public bool IsValid {
        get {
            HarmonicOneFormReceipt receipt = Receipt;
            return receipt.IsValid && Forms.Count == receipt.BasisCount && Forms.ForAll(form => form.Count == receipt.EdgeCount && form.ForAll(RhinoMath.IsValidDouble));
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct DiscreteCalculus(SparseMatrix D0, SparseMatrix D1, Arr<double> Star0, Arr<double> Star1, Arr<double> Star2, SpectralAssemblyReceipt Receipt, Option<SignpostTransportReceipt> Transport = default, Option<HarmonicOneFormBasis> Harmonic = default) {
    public bool IsValid => D0.IsValid && D1.IsValid && Receipt.IsValid && Star0.Count == D0.Cols.Value && Star1.Count == D0.Rows.Value && Star2.Count == D1.Rows.Value && Star0.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0) && Star1.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0) && Star2.ForAll(static v => RhinoMath.IsValidDouble(x: v) && v > 0.0) && Transport.Map(static receipt => receipt.IsValid).IfNone(noneValue: true) && Harmonic.Map(static basis => basis.IsValid).IfNone(noneValue: true);
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(DiscreteCalculus) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(SpectralAssemblyReceipt) => Fin.Succ((TOut)(object)Receipt),
            Type t when t == typeof(SignpostTransportReceipt) => Transport.Map(static receipt => (TOut)(object)receipt).ToFin(key.InvalidResult()),
            Type t when t == typeof(HarmonicOneFormBasis) => Harmonic.Map(static basis => (TOut)(object)basis).ToFin(key.InvalidResult()),
            Type t when t == typeof(HarmonicOneFormReceipt) => Harmonic.Map(static basis => (TOut)(object)basis.Receipt).ToFin(key.InvalidResult()),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(DiscreteCalculus), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralBasis(Arr<double> Eigenvalues, Arr<Arr<double>> Eigenvectors) {
    internal int VertexCount => Eigenvectors.IsEmpty ? 0 : Eigenvectors[index: 0].Count;
    public bool IsValid { get { int vertexCount = VertexCount; return Eigenvalues.Count > 0 && Eigenvalues.Count == Eigenvectors.Count && Eigenvalues.ForAll(static lambda => RhinoMath.IsValidDouble(x: lambda) && lambda >= -RhinoMath.SqrtEpsilon) && Eigenvectors.ForAll(phi => vertexCount > 0 && phi.Count == vertexCount && phi.ForAll(RhinoMath.IsValidDouble)); } }
    public SpectralBasis Truncate(int k) =>
        k <= 0 || k >= Eigenvalues.Count ? this : new SpectralBasis(Eigenvalues: new Arr<double>([.. Eigenvalues.AsIterable().Take(k)]), Eigenvectors: new Arr<Arr<double>>([.. Eigenvectors.AsIterable().Take(k)]));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorPolicy(SpectralScaleNormalization ScaleNormalization, SpectralEnergyNormalization EnergyNormalization, SpectralZeroModePolicy ZeroModePolicy, Option<Dimension> CropCount) {
    public static SpectralDescriptorPolicy Raw => new(ScaleNormalization: SpectralScaleNormalization.Raw, EnergyNormalization: SpectralEnergyNormalization.Raw, ZeroModePolicy: SpectralZeroModePolicy.Keep, CropCount: None);
    internal bool IsValid => ScaleNormalization is not null && EnergyNormalization is not null && ZeroModePolicy is not null && CropCount.Map(static count => count.Value > 0).IfNone(noneValue: true);
    internal bool IsRaw => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && EnergyNormalization.Equals(SpectralEnergyNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal bool IsValueOnly => ScaleNormalization.Equals(SpectralScaleNormalization.Raw) && ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) && CropCount.IsNone;
    internal static Fin<SpectralDescriptorPolicy> Admit(SpectralDescriptorPolicy policy, Op key) =>
        guard(policy.IsValid, key.InvalidInput()).ToFin().Map(_ => policy);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralWaveReceipt(double Energy, double Bandwidth, Option<double> FirstNonZeroScale, int ZeroModeCount, int CroppedEigenpairCount, int NonZeroEigenpairCount, double RawWeightSum, double NormalizedWeightSum, Option<double> MinLogEigenvalue, Option<double> MaxLogEigenvalue, bool WksNormalized) {
    internal bool IsValid {
        get {
            Option<double> first = FirstNonZeroScale, minLog = MinLogEigenvalue, maxLog = MaxLogEigenvalue;
            bool firstValid = first.IsNone || (RhinoMath.IsValidDouble(x: first.IfNone(0.0)) && first.IfNone(0.0) > 0.0);
            bool rangeValid = minLog.IsNone || maxLog.IsNone || (RhinoMath.IsValidDouble(x: minLog.IfNone(0.0)) && RhinoMath.IsValidDouble(x: maxLog.IfNone(0.0)) && minLog.IfNone(0.0) <= maxLog.IfNone(0.0));
            return new[] { Energy, Bandwidth, RawWeightSum, NormalizedWeightSum }.All(RhinoMath.IsValidDouble)
                && Energy > 0.0
                && Bandwidth > 0.0
                && ZeroModeCount >= 0
                && CroppedEigenpairCount >= NonZeroEigenpairCount
                && NonZeroEigenpairCount > 0
                && RawWeightSum > 0.0
                && (!WksNormalized || Math.Abs(value: NormalizedWeightSum - 1.0) <= 1.0e-9)
                && firstValid
                && rangeValid;
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptorReceipt(SpectralFilter Filter, int VertexCount, int EigenpairCount, int SourceCount, bool ComparisonReady, bool Pairwise, bool EnergyNormalized, bool ScaleNormalized, SpectralDescriptorPolicy Policy = default, int ZeroModeCount = 0, int CroppedEigenpairCount = 0, Option<SpectralWaveReceipt> Wave = default) {
    public bool IsValid =>
        Filter is not null
        && VertexCount > 0
        && EigenpairCount > 0
        && SourceCount >= 0
        && ZeroModeCount >= 0
        && CroppedEigenpairCount > 0
        && CroppedEigenpairCount <= EigenpairCount
        && ZeroModeCount <= EigenpairCount
        && SourceCount <= VertexCount
        && (!Pairwise || SourceCount > 0)
        && (!ComparisonReady || !Policy.IsRaw || Wave.IsSome);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralDescriptor(Arr<double> Values, SpectralDescriptorReceipt Receipt) {
    public bool IsValid => Receipt.IsValid && Values.Count == Receipt.VertexCount && Values.ForAll(RhinoMath.IsValidDouble);
    public Fin<SpectralDescriptor> Normalize(SpectralDescriptorPolicy policy, Op? key = null) =>
        SpectralCore.NormalizeDescriptor(descriptor: this, policy: policy, key: key.OrDefault());
    public Fin<SpectralRanking> Rank(Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op? key = null) =>
        SpectralCore.RankDescriptors(query: this, candidates: candidates, policy: policy, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRankingPolicy(SpectralDescriptorPolicy Descriptor, SpectralDistanceKind Distance, SpectralTieBreak TieBreak) {
    public static SpectralRankingPolicy Default => new(Descriptor: SpectralDescriptorPolicy.Raw, Distance: SpectralDistanceKind.Euclidean, TieBreak: SpectralTieBreak.InputOrder);
    internal bool IsValid => Descriptor.IsValid && Distance is not null && TieBreak is not null;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRank(int Index, double Distance, SpectralDescriptor Descriptor);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpectralRanking(SpectralDescriptor Query, Seq<SpectralRank> Items, SpectralRankingPolicy Policy);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class SpectralCore {
    internal const double WaveBandwidthFloor = 1e-9;
    private const double DegenerateTriangleArea = 1e-14;
    private readonly record struct IntrinsicTriangle(int A, int B, int C, int[] Edges, double Area, double LAb, double LBc, double LCa) {
        internal double QuadArea => 4.0 * Area;
        internal (double A, double B, double C) Cotangents => (A: ((Length(side: 0) * Length(side: 0)) + (Length(side: 2) * Length(side: 2)) - (Length(side: 1) * Length(side: 1))) / QuadArea, B: ((Length(side: 0) * Length(side: 0)) + (Length(side: 1) * Length(side: 1)) - (Length(side: 2) * Length(side: 2))) / QuadArea, C: ((Length(side: 1) * Length(side: 1)) + (Length(side: 2) * Length(side: 2)) - (Length(side: 0) * Length(side: 0))) / QuadArea);
        internal (Point3d A, Point3d B, Point3d C) Points(Mesh mesh) => (mesh.Vertices[index: A], mesh.Vertices[index: B], mesh.Vertices[index: C]);
        internal int Vertex(int side) => side switch { 0 => A, 1 => B, _ => C };
        internal int Edge(int side) => Edges[side];
        internal double Length(int side) => side switch { 0 => LAb, 1 => LBc, _ => LCa };
        internal double Orientation(MeshKernel.IntrinsicMesh imesh, int side) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: Edge(side));
            return edge.Lo == Vertex(side: side) && edge.Hi == Vertex(side: (side + 1) % 3) ? 1.0 : -1.0;
        }
        internal (int I, int J, double Sign, double LA, double LB, double LOpp) CrouzeixPair(MeshKernel.IntrinsicMesh imesh, int side) => (Edge(side), Edge(side: (side + 1) % 3), Orientation(imesh: imesh, side: side) * Orientation(imesh: imesh, side: (side + 1) % 3), Length(side: (side + 1) % 3), Length(side: side), Length(side: (side + 2) % 3));
        internal double Angle(int side) => AngleFromLengths(opposite: Length(side: (side + 1) % 3), adjacent1: Length(side: side == 0 ? 0 : side - 1), adjacent2: Length(side: side == 0 ? 2 : side));
    }

    private static double AngleFromLengths(double opposite, double adjacent1, double adjacent2) =>
        Math.Acos(d: Math.Min(val1: 1.0, val2: Math.Max(val1: -1.0, val2: ((adjacent1 * adjacent1) + (adjacent2 * adjacent2) - (opposite * opposite)) / (2.0 * adjacent1 * adjacent2))));

    // --- [DESCRIPTORS] ----------------------------------------------------------------------
    // BOUNDARY ADAPTER -- dense mesh buffer avoids Seq materialisation churn.
    internal static Fin<SpectralDescriptor> EvaluateFilteredDetailed(SpectralBasis basis, Option<Seq<int>> sources, SpectralFilter filter, SpectralDescriptorPolicy policy, Op key) {
        int n = basis.VertexCount;
        int[] sourceSet = sources is { IsSome: true, Case: Seq<int> values } ? [.. values.AsIterable()] : [];
        if (n == 0 || (sources.IsSome && sourceSet.Length == 0) || sourceSet.Any(s => s < 0 || s >= n) || sourceSet.Distinct().Count() != sourceSet.Length)
            return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        if (!basis.Eigenvectors.ForAll(phi => phi.Count == n)) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        int zeroModeCount = basis.Eigenvalues.AsIterable().Count(static lambda => lambda <= RhinoMath.SqrtEpsilon);
        double firstNonZero = basis.Eigenvalues.AsIterable().FirstOrDefault(static lambda => lambda > RhinoMath.SqrtEpsilon);
        if (policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue) && firstNonZero <= RhinoMath.SqrtEpsilon) return Fin.Fail<SpectralDescriptor>(error: key.InvalidResult());
        int[] eigenIndices = [.. Enumerable.Range(start: 0, count: basis.Eigenvalues.Count)
            .Where(i => policy.ZeroModePolicy.Equals(SpectralZeroModePolicy.Keep) || basis.Eigenvalues[index: i] > RhinoMath.SqrtEpsilon)
            .Take(policy.CropCount.Map(static count => count.Value).IfNone(basis.Eigenvalues.Count))];
        if (eigenIndices.Length == 0) return Fin.Fail<SpectralDescriptor>(error: key.InvalidInput());
        double[] scaledEigenvalues = [.. eigenIndices.Select(k => policy.ScaleNormalization.Equals(SpectralScaleNormalization.FirstNonZeroEigenvalue) ? basis.Eigenvalues[index: k] / firstNonZero : basis.Eigenvalues[index: k])];
        (double[] weights, Option<SpectralWaveReceipt> wave) = WeightsOf(filter: filter, eigenvalues: scaledEigenvalues, firstNonZero: firstNonZero, zeroModeCount: zeroModeCount, croppedCount: eigenIndices.Length);
        if (weights.Any(static weight => !RhinoMath.IsValidDouble(x: weight)) || (filter is SpectralFilter.WaveCase && wave.IsNone)) return Fin.Fail<SpectralDescriptor>(key.InvalidResult());
        bool isPairwise = sourceSet.Length > 0;
        double normFactor = isPairwise ? 1.0 / sourceSet.Length : 1.0;
        double[] result = new double[n];
        for (int ki = 0; ki < eigenIndices.Length; ki++) {
            int k = eigenIndices[ki];
            double w = weights[ki];
            Arr<double> phi = basis.Eigenvectors[index: k];
            if (isPairwise)
                for (int v = 0; v < n; v++) {
                    double phiV = phi[index: v];
                    for (int s = 0; s < sourceSet.Length; s++) {
                        double delta = phiV - phi[index: sourceSet[s]];
                        result[v] += w * delta * delta;
                    }
                }
            if (!isPairwise)
                for (int v = 0; v < n; v++) result[v] += w * phi[index: v] * phi[index: v];
        }
        if (isPairwise) for (int v = 0; v < n; v++) result[v] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: result[v] * normFactor));
        return NormalizeValues(values: result, policy: policy, key: key).Map(values => new SpectralDescriptor(Values: new Arr<double>(values), Receipt: new SpectralDescriptorReceipt(Filter: filter, VertexCount: n, EigenpairCount: basis.Eigenvalues.Count, SourceCount: sourceSet.Length, ComparisonReady: !policy.IsRaw || wave.IsSome, Pairwise: isPairwise, EnergyNormalized: !policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), ScaleNormalized: !policy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw), Policy: policy, ZeroModeCount: zeroModeCount, CroppedEigenpairCount: eigenIndices.Length, Wave: wave)));
    }
    internal static Fin<SpectralDescriptor> NormalizeDescriptor(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        from activePolicy in SpectralDescriptorPolicy.Admit(policy: policy, key: key)
        from _ in activePolicy.IsValueOnly
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(SpectralDescriptor), outputType: typeof(SpectralDescriptorPolicy)))
        from values in NormalizeValues(values: [.. descriptor.Values.AsIterable()], policy: activePolicy, key: key)
        let receipt = descriptor.Receipt with { ComparisonReady = !activePolicy.IsRaw || descriptor.Receipt.Wave.IsSome, EnergyNormalized = !activePolicy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw), ScaleNormalized = !activePolicy.ScaleNormalization.Equals(SpectralScaleNormalization.Raw), Policy = activePolicy }
        select new SpectralDescriptor(Values: new Arr<double>(values), Receipt: receipt);
    internal static Fin<SpectralRanking> RankDescriptors(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) =>
        !policy.IsValid || !query.IsValid || candidates.IsEmpty || !candidates.ForAll(static candidate => candidate.IsValid)
            ? Fin.Fail<SpectralRanking>(key.InvalidInput())
            : from normalizedQuery in NormalizeDescriptorForRanking(descriptor: query, policy: policy.Descriptor, key: key)
              from normalizedCandidates in candidates.TraverseM(candidate => NormalizeDescriptorForRanking(descriptor: candidate, policy: policy.Descriptor, key: key)).As()
              from ranks in RankNormalized(query: normalizedQuery, candidates: normalizedCandidates, policy: policy, key: key)
              select new SpectralRanking(Query: normalizedQuery, Items: ranks, Policy: policy);
    private static Fin<SpectralDescriptor> NormalizeDescriptorForRanking(SpectralDescriptor descriptor, SpectralDescriptorPolicy policy, Op key) =>
        descriptor.Receipt.ComparisonReady && SamePolicy(left: descriptor.Receipt.Policy, right: policy)
            ? Fin.Succ(descriptor)
            : NormalizeDescriptor(descriptor: descriptor, policy: policy, key: key);
    private static bool SamePolicy(SpectralDescriptorPolicy left, SpectralDescriptorPolicy right) =>
        left.ScaleNormalization.Equals(right.ScaleNormalization)
        && left.EnergyNormalization.Equals(right.EnergyNormalization)
        && left.ZeroModePolicy.Equals(right.ZeroModePolicy)
        && left.CropCount.Match(
            Some: l => right.CropCount.Match(Some: r => l.Value == r.Value, None: static () => false),
            None: () => right.CropCount.IsNone);
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WeightsOf(SpectralFilter filter, double[] eigenvalues, double firstNonZero, int zeroModeCount, int croppedCount) =>
        filter is SpectralFilter.WaveCase wave ? WaveWeightsOf(wave: wave, eigenvalues: eigenvalues, firstNonZero: firstNonZero, zeroModeCount: zeroModeCount, croppedCount: croppedCount) : ([.. eigenvalues.Select(filter.Weight)], Option<SpectralWaveReceipt>.None);
    private static (double[] Weights, Option<SpectralWaveReceipt> Wave) WaveWeightsOf(SpectralFilter.WaveCase wave, double[] eigenvalues, double firstNonZero, int zeroModeCount, int croppedCount) {
        double[] raw = [.. eigenvalues.Select(wave.Weight)];
        double sum = raw.Sum();
        if (!RhinoMath.IsValidDouble(x: sum) || sum <= RhinoMath.SqrtEpsilon) return (raw, Option<SpectralWaveReceipt>.None);
        double[] normalized = [.. raw.Select(weight => weight / sum)];
        double[] positiveLogs = [.. eigenvalues.Where(static lambda => lambda > RhinoMath.SqrtEpsilon).Select(static lambda => Math.Log(d: lambda))];
        SpectralWaveReceipt receipt = new(
            Energy: wave.Energy.Value,
            Bandwidth: Math.Max(val1: wave.Bandwidth.Value, val2: WaveBandwidthFloor),
            FirstNonZeroScale: firstNonZero > RhinoMath.SqrtEpsilon ? Some(firstNonZero) : Option<double>.None,
            ZeroModeCount: zeroModeCount,
            CroppedEigenpairCount: croppedCount,
            NonZeroEigenpairCount: positiveLogs.Length,
            RawWeightSum: sum,
            NormalizedWeightSum: normalized.Sum(),
            MinLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Min()),
            MaxLogEigenvalue: positiveLogs.Length == 0 ? Option<double>.None : Some(positiveLogs.Max()),
            WksNormalized: true);
        return (normalized, receipt.IsValid ? Some(receipt) : Option<SpectralWaveReceipt>.None);
    }
    private static Fin<Seq<SpectralRank>> RankNormalized(SpectralDescriptor query, Seq<SpectralDescriptor> candidates, SpectralRankingPolicy policy, Op key) {
        int valueCount = query.Values.Count;
        if (valueCount <= 0 || candidates.Exists(candidate => candidate.Values.Count != valueCount)) return Fin.Fail<Seq<SpectralRank>>(key.InvalidInput());
        double[] queryValues = [.. query.Values.AsIterable()];
        SpectralRank[] ranks = [.. candidates.AsIterable().Select((candidate, index) => new SpectralRank(Index: index, Distance: DistanceOf(a: queryValues, b: [.. candidate.Values.AsIterable()], kind: policy.Distance), Descriptor: candidate)).OrderBy(static rank => rank.Distance).ThenBy(static rank => rank.Index)];
        return ranks.All(static rank => RhinoMath.IsValidDouble(x: rank.Distance)) ? Fin.Succ(toSeq(ranks)) : Fin.Fail<Seq<SpectralRank>>(key.InvalidResult());
    }
    private static double DistanceOf(double[] a, double[] b, SpectralDistanceKind kind) =>
        kind switch {
            SpectralDistanceKind k when k.Equals(SpectralDistanceKind.Manhattan) => MathNet.Numerics.Distance.Manhattan(a, b),
            SpectralDistanceKind k when k.Equals(SpectralDistanceKind.Cosine) => MathNet.Numerics.Distance.Cosine(a, b),
            _ => MathNet.Numerics.Distance.Euclidean(a, b),
        };
    private static Fin<double[]> NormalizeValues(double[] values, SpectralDescriptorPolicy policy, Op key) {
        if (!values.All(RhinoMath.IsValidDouble)) return Fin.Fail<double[]>(key.InvalidResult());
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.Raw)) return Fin.Succ(values);
        double[] result = [.. values];
        if (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) || policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL2))
            return (policy.EnergyNormalization.Equals(SpectralEnergyNormalization.UnitL1) ? result.Sum(static value => Math.Abs(value: value)) : Math.Sqrt(d: result.Sum(static value => value * value))) switch {
                double scale when scale > RhinoMath.SqrtEpsilon => Fin.Succ<double[]>([.. result.Select(value => value / scale)]),
                _ => Fin.Fail<double[]>(key.InvalidResult()),
            };
        double mean = result.Average();
        double variance = result.Sum(value => (value - mean) * (value - mean)) / result.Length;
        double sigma = Math.Sqrt(d: variance);
        return sigma > RhinoMath.SqrtEpsilon ? Fin.Succ<double[]>([.. result.Select(value => (value - mean) / sigma)]) : Fin.Fail<double[]>(key.InvalidResult());
    }
    // --- [DEC_ASSEMBLY] ---------------------------------------------------------------------
    private static IntrinsicTriangle? IntrinsicTriangleOf(MeshKernel.IntrinsicMesh imesh, int faceIdx) =>
        IntrinsicTriangleOf(imesh: imesh, faceIdx: faceIdx, missingEdges: out _, degenerate: out _);
    private static IntrinsicTriangle? IntrinsicTriangleOf(MeshKernel.IntrinsicMesh imesh, int faceIdx, out bool missingEdges, out bool degenerate) {
        (int A, int B, int C)? slot = imesh.Triangles[index: faceIdx];
        int[] edges = imesh.EdgesOfFace(faceIdx: faceIdx);
        double area = imesh.AreaOfFace(faceIdx: faceIdx);
        missingEdges = edges.Length != 3 || edges.Any(static edge => edge < 0);
        degenerate = slot is null || area < DegenerateTriangleArea;
        return slot is null || missingEdges || degenerate
            ? null
            : new IntrinsicTriangle(A: slot.Value.A, B: slot.Value.B, C: slot.Value.C, Edges: edges, Area: area, LAb: imesh.EdgeAt(index: edges[0]).Length, LBc: imesh.EdgeAt(index: edges[1]).Length, LCa: imesh.EdgeAt(index: edges[2]).Length);
    }
    private static double BoundaryCompositionResidual(List<(int Row, int Col, double Value)> d0, List<(int Row, int Col, double Value)> d1) {
        ILookup<int, (int Row, int Col, double Value)> d0ByEdge = d0.ToLookup(static row => row.Row);
        return d1
            .SelectMany(row => d0ByEdge[key: row.Col].Select(e => (Face: row.Row, Vertex: e.Col, Value: row.Value * e.Value)))
            .GroupBy(static e => (e.Face, e.Vertex))
            .DefaultIfEmpty()
            .Max(static group => group is null ? 0.0 : Math.Abs(value: group.Sum(static e => e.Value)));
    }
    internal static Arr<double> ComputeIntrinsicStar1(MeshKernel.IntrinsicMesh imesh) {
        int eCount = imesh.EdgeCount;
        double[] star1 = new double[eCount];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            star1[e] = 0.5 * (CotAtOppositeCorner(imesh: imesh, edge: edge, faceIdx: edge.Face0)
                            + CotAtOppositeCorner(imesh: imesh, edge: edge, faceIdx: edge.Face1));
        }
        return new Arr<double>(star1);
    }
    private static double CotAtOppositeCorner(MeshKernel.IntrinsicMesh imesh, MeshKernel.IntrinsicEdge edge, int faceIdx) {
        if (faceIdx < 0 || IntrinsicTriangleOf(imesh: imesh, faceIdx: faceIdx) is not { } face) return 0.0;
        int oppV = (face.A != edge.Lo && face.A != edge.Hi) ? face.A
                 : (face.B != edge.Lo && face.B != edge.Hi) ? face.B
                 : face.C;
        int eOppLo = imesh.IndexOfEdge(lo: oppV, hi: edge.Lo);
        int eOppHi = imesh.IndexOfEdge(lo: oppV, hi: edge.Hi);
        return eOppLo < 0 || eOppHi < 0
            ? 0.0
            : ((imesh.EdgeAt(index: eOppLo).Length * imesh.EdgeAt(index: eOppLo).Length) + (imesh.EdgeAt(index: eOppHi).Length * imesh.EdgeAt(index: eOppHi).Length) - (edge.Length * edge.Length)) / (4.0 * face.Area);
    }
    internal static Fin<DiscreteCalculus> Build(MeshSpace space, Op key) =>
        Build(space: space, kind: MeshLaplacian.IntrinsicDelaunay, key: key);
    internal static Fin<DiscreteCalculus> Build(MeshSpace space, MeshLaplacian kind, Op key) =>
        from activeKind in Optional(kind).ToFin(key.InvalidInput())
        from imesh in activeKind.Equals(MeshLaplacian.TuftedIntrinsic)
            ? space.Cache.TuftedIntrinsicMeshSnapshot(key: key)
            : space.Cache.IntrinsicMeshSnapshot(key: key)
        from laplacian in space.Laplacian(kind: activeKind, key: key)
        from topology in MeshKernel.TopologyDetailed(space: space, key: key)
        from dec in AssembleDecOperators(imesh: imesh, mass: laplacian.MassLumped, topology: topology, key: key)
        from transport in MeshKernel.SignpostTransportReceiptOf(space: space, imesh: imesh, key: key)
        from harmonic in topology.Genus.Map(static genus => genus > 0).IfNone(noneValue: false)
            ? BuildHarmonicOneForms(calculus: dec, topology: topology, key: key).Map(static basis => Some(basis))
            : Fin.Succ(Option<HarmonicOneFormBasis>.None)
        select dec with { Transport = Some(transport), Harmonic = harmonic };

    private static Fin<DiscreteCalculus> AssembleDecOperators(MeshKernel.IntrinsicMesh imesh, Arr<double> mass, TopologyReceipt topology, Op key) {
        int vertCount = imesh.VertexCount;
        int edgeCount = imesh.EdgeCount;
        int[] liveFaces = [.. imesh.LiveFaceIndices()];
        int faceCount = liveFaces.Length;
        List<(int Row, int Col, double Value)> d0 = new(capacity: 2 * edgeCount);
        List<(int Row, int Col, double Value)> d1 = new(capacity: 3 * faceCount);
        Arr<double> star1 = ComputeIntrinsicStar1(imesh: imesh);
        double[] star2 = new double[faceCount];
        int admitted = 0;
        int skippedDegenerate = 0;
        int skippedMissing = 0;
        for (int e = 0; e < edgeCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            d0.AddRange([(e, edge.Lo, -1.0), (e, edge.Hi, +1.0)]);
        }
        for (int row = 0; row < faceCount; row++) {
            if (IntrinsicTriangleOf(imesh: imesh, faceIdx: liveFaces[row], missingEdges: out bool missingEdges, degenerate: out _) is not { } face) {
                skippedMissing += missingEdges ? 1 : 0;
                skippedDegenerate += missingEdges ? 0 : 1;
                continue;
            }
            admitted++;
            star2[row] = 1.0 / face.Area;
            for (int side = 0; side < 3; side++) {
                d1.Add(item: (row, face.Edge(side: side), face.Orientation(imesh: imesh, side: side)));
            }
        }
        double boundaryResidual = BoundaryCompositionResidual(d0: d0, d1: d1);
        int harmonicDimension = topology.Genus.Map(static g => 2 * g).IfNone(0);
        return mass.Count != vertCount
            || !mass.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0)
            || boundaryResidual > RhinoMath.SqrtEpsilon
            ? Fin.Fail<DiscreteCalculus>(key.InvalidResult())
            : from D0 in SparseMatrix.FromTriplets(rows: Dimension.Create(value: edgeCount), cols: Dimension.Create(value: vertCount), triplets: d0, key: key)
              from D1 in SparseMatrix.FromTriplets(rows: Dimension.Create(value: faceCount), cols: Dimension.Create(value: edgeCount), triplets: d1, key: key)
              let boundaryEdgeCount = Enumerable.Range(start: 0, count: imesh.EdgeCount).Count(edgeIndex => imesh.EdgeAt(index: edgeIndex).Face1 < 0)
              let receipt = new SpectralAssemblyReceipt(VertexCount: vertCount, EdgeCount: edgeCount, FaceCount: faceCount, AdmittedFaceCount: admitted, SkippedDegenerateFaces: skippedDegenerate, SkippedMissingEdges: skippedMissing, SkippedInvalidNormals: 0, SkippedInvalidTangents: 0, FlippedIntrinsicRejected: imesh.HasFlips, MatrixRows: D0.Rows.Value + D1.Rows.Value, MatrixCols: D0.Cols.Value + D1.Cols.Value, NonZeros: D0.Values.Count + D1.Values.Count, PositiveStar0Count: mass.Count(static value => value > RhinoMath.ZeroTolerance), PositiveStar1Count: star1.Count(static value => value > RhinoMath.ZeroTolerance), PositiveStar2Count: star2.Count(static value => value > RhinoMath.ZeroTolerance), BoundaryCompositionResidual: boundaryResidual, Genus: topology.Genus, HarmonicDimension: harmonicDimension, BoundaryEdgeCount: boundaryEdgeCount, BoundaryComponentCount: topology.BoundaryComponents, NonManifoldEdgeCount: topology.NonManifoldEdges, EulerCharacteristic: topology.EulerCharacteristic, TopologyEulerValidated: topology.EulerValidated, Kind: SpectralAssemblyKind.Dec)
              select new DiscreteCalculus(D0: D0, D1: D1, Star0: mass, Star1: star1, Star2: new Arr<double>(star2), Receipt: receipt);
    }

    private static Fin<HarmonicOneFormBasis> BuildHarmonicOneForms(DiscreteCalculus calculus, TopologyReceipt topology, Op key) {
        int edgeCount = calculus.D0.Rows.Value;
        int vertexCount = calculus.D0.Cols.Value;
        int faceCount = calculus.D1.Rows.Value;
        int expected = topology.Genus.Map(static genus => 2 * genus).IfNone(0);
        double tolerance = 1.0e-8;
        if (expected <= 0 || edgeCount <= 0 || expected > edgeCount || calculus.Star1.Count != edgeCount)
            return Fin.Fail<HarmonicOneFormBasis>(key.InvalidInput());
        double[] normal = new double[edgeCount * edgeCount];
        void AddOuter(IReadOnlyList<(int Col, double Value)> row) {
            for (int i = 0; i < row.Count; i++)
                for (int j = 0; j < row.Count; j++)
                    normal[(row[i].Col * edgeCount) + row[j].Col] += row[i].Value * row[j].Value;
        }
        for (int row = 0; row < calculus.D1.Rows.Value; row++) {
            List<(int Col, double Value)> entries = [];
            for (int k = calculus.D1.RowPtr[row]; k < calculus.D1.RowPtr[row + 1]; k++) entries.Add(item: (calculus.D1.ColInd[k], calculus.D1.Values[k]));
            AddOuter(row: entries);
        }
        List<(int Col, double Value)>[] coClosed = [.. Enumerable.Range(start: 0, count: vertexCount).Select(static _ => new List<(int Col, double Value)>())];
        for (int edge = 0; edge < calculus.D0.Rows.Value; edge++)
            for (int k = calculus.D0.RowPtr[edge]; k < calculus.D0.RowPtr[edge + 1]; k++)
                coClosed[calculus.D0.ColInd[k]].Add(item: (edge, calculus.D0.Values[k] * calculus.Star1[edge]));
        for (int row = 0; row < coClosed.Length; row++) AddOuter(row: coClosed[row]);
        Arr<double> upper = new([.. Enumerable.Range(start: 0, count: edgeCount).SelectMany(row => Enumerable.Range(start: row, count: edgeCount - row).Select(col => 0.5 * (normal[(row * edgeCount) + col] + normal[(col * edgeCount) + row])))]);
        return from matrix in SymmetricMatrix.Of(dim: Dimension.Create(value: edgeCount), upper: upper, key: key)
               from eigen in matrix.DecomposeEigenDetailed(key: key)
               let sorted = eigen.Pairs.AsIterable().OrderBy(static pair => pair.Eigenvalue).ToArray()
               from enough in sorted.Length >= expected ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
               from forms in Star1OrthonormalForms(vectors: sorted.Take(count: expected).Select(static pair => pair.Eigenvector), star1: calculus.Star1, key: key)
               let maxClosed = MaxResidual(matrix: calculus.D1, forms: forms)
               let maxCoClosed = MaxCoClosedResidual(d0: calculus.D0, star1: calculus.Star1, forms: forms)
               let orthonormal = Star1OrthonormalResidual(forms: forms, star1: calculus.Star1)
               let nullity = sorted.Count(pair => pair.Eigenvalue <= tolerance)
               let receipt = new HarmonicOneFormReceipt(
                   Genus: topology.Genus,
                   ExpectedDimension: expected,
                   ConstraintRows: faceCount + vertexCount,
                   EdgeCount: edgeCount,
                   Rank: edgeCount - nullity,
                   Nullity: nullity,
                   BasisCount: forms.Count,
                   SvdTolerance: tolerance,
                   MinNullEigenvalue: sorted[0].Eigenvalue,
                   MaxNullEigenvalue: sorted[expected - 1].Eigenvalue,
                   MaxClosedResidual: maxClosed,
                   MaxCoClosedResidual: maxCoClosed,
                   Star1OrthonormalResidual: orthonormal,
                   PositiveStar1Count: calculus.Star1.Count(static value => value > 0.0),
                   Eigen: eigen)
               let basis = new HarmonicOneFormBasis(Forms: forms, Receipt: receipt)
               from valid in basis.IsValid ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
               select basis;
    }
    private static Fin<Arr<Arr<double>>> Star1OrthonormalForms(IEnumerable<Arr<double>> vectors, Arr<double> star1, Op key) {
        List<Arr<double>> forms = [];
        foreach (Arr<double> candidate in vectors) {
            double[] values = [.. candidate.AsIterable()];
            if (values.Length != star1.Count) return Fin.Fail<Arr<Arr<double>>>(key.InvalidResult());
            for (int basis = 0; basis < forms.Count; basis++) {
                double dot = Star1Inner(left: values, right: forms[basis], star1: star1);
                for (int i = 0; i < values.Length; i++) values[i] -= dot * forms[basis][i];
            }
            double norm = Math.Sqrt(d: Math.Max(val1: 0.0, val2: Star1Inner(left: values, right: values, star1: star1)));
            if (!RhinoMath.IsValidDouble(x: norm) || norm <= RhinoMath.SqrtEpsilon) return Fin.Fail<Arr<Arr<double>>>(key.InvalidResult());
            forms.Add(item: new Arr<double>([.. values.Select(value => value / norm)]));
        }
        return Fin.Succ(new Arr<Arr<double>>([.. forms]));
    }
    private static double Star1Inner(double[] left, Arr<double> right, Arr<double> star1) =>
        Enumerable.Range(start: 0, count: left.Length).Sum(i => left[i] * right[i] * star1[i]);
    private static double Star1Inner(double[] left, double[] right, Arr<double> star1) =>
        Enumerable.Range(start: 0, count: left.Length).Sum(i => left[i] * right[i] * star1[i]);
    private static double MaxResidual(SparseMatrix matrix, Arr<Arr<double>> forms) {
        double max = 0.0;
        foreach (Arr<double> form in forms.AsIterable())
            for (int row = 0; row < matrix.Rows.Value; row++) {
                double value = 0.0;
                for (int k = matrix.RowPtr[row]; k < matrix.RowPtr[row + 1]; k++) value += matrix.Values[k] * form[matrix.ColInd[k]];
                max = Math.Max(val1: max, val2: Math.Abs(value: value));
            }
        return max;
    }
    private static double MaxCoClosedResidual(SparseMatrix d0, Arr<double> star1, Arr<Arr<double>> forms) {
        double max = 0.0;
        foreach (Arr<double> form in forms.AsIterable()) {
            double[] coClosed = new double[d0.Cols.Value];
            for (int edge = 0; edge < d0.Rows.Value; edge++) {
                double value = star1[edge] * form[edge];
                for (int k = d0.RowPtr[edge]; k < d0.RowPtr[edge + 1]; k++) coClosed[d0.ColInd[k]] += d0.Values[k] * value;
            }
            max = Math.Max(val1: max, val2: coClosed.Max(static value => Math.Abs(value: value)));
        }
        return max;
    }
    private static double Star1OrthonormalResidual(Arr<Arr<double>> forms, Arr<double> star1) {
        double max = 0.0;
        for (int i = 0; i < forms.Count; i++) {
            double[] left = [.. forms[i].AsIterable()];
            for (int j = 0; j < forms.Count; j++)
                max = Math.Max(val1: max, val2: Math.Abs(value: Star1Inner(left: left, right: forms[j], star1: star1) - (i == j ? 1.0 : 0.0)));
        }
        return max;
    }

    // --- [CROUZEIX_RAVIART] -----------------------------------------------------------------
    internal static Fin<(SparseMatrix Matrix, SpectralAssemblyReceipt Receipt)> BuildCrouzeixRaviartHeatSystemDetailed(MeshKernel.IntrinsicMesh mesh, double time, Op key) {
        if (!RhinoMath.IsValidDouble(x: time) || time <= 0.0) return Fin.Fail<(SparseMatrix Matrix, SpectralAssemblyReceipt Receipt)>(error: key.InvalidInput());
        if (!mesh.IsFrozen) return Fin.Fail<(SparseMatrix Matrix, SpectralAssemblyReceipt Receipt)>(error: key.InvalidInput());
        if (mesh.HasFlips) return Fin.Fail<(SparseMatrix Matrix, SpectralAssemblyReceipt Receipt)>(error: key.Unsupported(geometryType: typeof(MeshKernel.IntrinsicMesh), outputType: typeof(SparseMatrix)));
        int eCount = mesh.EdgeCount;
        if (eCount == 0) return Fin.Fail<(SparseMatrix Matrix, SpectralAssemblyReceipt Receipt)>(error: key.InvalidInput());
        List<(int Row, int Col, double Value)> triplets = new(capacity: (2 * eCount) + (mesh.Triangles.Count * 36));
        double[] mass = new double[eCount];
        int admitted = 0;
        int skippedDegenerate = 0;
        int skippedMissing = 0;
        foreach (int f in mesh.LiveFaceIndices()) {
            if (IntrinsicTriangleOf(imesh: mesh, faceIdx: f, missingEdges: out bool missingEdges, degenerate: out _) is not { } face) {
                skippedMissing += missingEdges ? 1 : 0;
                skippedDegenerate += missingEdges ? 0 : 1;
                continue;
            }
            admitted++;
            double contribution = face.Area / 3.0;
            for (int k = 0; k < 3; k++) mass[face.Edge(side: k)] += contribution;
            for (int side = 0; side < 3; side++)
                EmitCrouzeixRaviartPair(triplets: triplets, eCount: eCount, pair: face.CrouzeixPair(imesh: mesh, side: side), area: face.Area, time: time);
        }
        for (int e = 0; e < eCount; e++) triplets.AddRange([(e, e, mass[e]), (e + eCount, e + eCount, mass[e])]);
        return SparseMatrix.FromTriplets(rows: Dimension.Create(value: 2 * eCount), cols: Dimension.Create(value: 2 * eCount), triplets: triplets, key: key)
            .Map(matrix => (Matrix: matrix, Receipt: new SpectralAssemblyReceipt(VertexCount: 0, EdgeCount: eCount, FaceCount: mesh.Triangles.Count, AdmittedFaceCount: admitted, SkippedDegenerateFaces: skippedDegenerate, SkippedMissingEdges: skippedMissing, SkippedInvalidNormals: 0, SkippedInvalidTangents: 0, FlippedIntrinsicRejected: mesh.HasFlips, MatrixRows: matrix.Rows.Value, MatrixCols: matrix.Cols.Value, NonZeros: matrix.Values.Count, PositiveStar0Count: 0, PositiveStar1Count: 0, PositiveStar2Count: admitted, BoundaryCompositionResidual: 0.0, Genus: Option<int>.None, HarmonicDimension: 0, ComponentCount: 2, PositiveMassCount: mass.Count(static value => value > RhinoMath.ZeroTolerance), SymmetryResidual: 0.0, FactorNonZeros: Option<int>.None, Kind: SpectralAssemblyKind.EdgeConnection)));
    }
    private static void EmitCrouzeixRaviartPair(List<(int Row, int Col, double Value)> triplets, int eCount, (int I, int J, double Sign, double LA, double LB, double LOpp) pair, double area, double time) {
        double dot = (pair.LA * pair.LA) + (pair.LB * pair.LB) - (pair.LOpp * pair.LOpp);
        double weight = dot / (2.0 * area);
        double cosTheta = dot / (2.0 * pair.LA * pair.LB);
        double sinTheta = 2.0 * area / (pair.LA * pair.LB);
        MatrixKernel.AddHermitianRealBlockTriplets(triplets: triplets, order: eCount, i: pair.I, j: pair.J, real: weight * pair.Sign * cosTheta * time, imaginary: -weight * pair.Sign * sinTheta * time, diagonal: weight * time);
    }
    internal static Vector3d[] SampleCrouzeixRaviartFaceField(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Arr<double> stacked) {
        int eCount = imesh.EdgeCount;
        Vector3d[] field = new Vector3d[imesh.Triangles.Count];
        foreach (int f in imesh.LiveFaceIndices()) {
            if (IntrinsicTriangleOf(imesh: imesh, faceIdx: f) is not { } face) continue;
            (Point3d pa, Point3d pb, Point3d pc) = face.Points(mesh: mesh);
            Vector3d normal = Vector3d.CrossProduct(a: pb - pa, b: pc - pa);
            if (!normal.Unitize()) continue;
            Vector3d acc = Vector3d.Zero;
            for (int k = 0; k < 3; k++) {
                int e = face.Edges[k];
                MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
                Vector3d tangent = (Vector3d)(mesh.Vertices[index: edge.Hi] - mesh.Vertices[index: edge.Lo]);
                if (!tangent.Unitize()) continue;
                Vector3d perp = Vector3d.CrossProduct(a: normal, b: tangent);
                acc += (stacked[index: e] * tangent) + (stacked[index: e + eCount] * perp);
            }
            double mag = acc.Length;
            field[f] = mag > RhinoMath.ZeroTolerance ? acc / mag : Vector3d.Zero;
        }
        return field;
    }
    internal static Arr<double> ComputeIntrinsicVertexDivergence(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Vector3d[] faceFields) {
        int n = mesh.Vertices.Count;
        double[] div = new double[n];
        foreach (int f in imesh.LiveFaceIndices()) {
            if (IntrinsicTriangleOf(imesh: imesh, faceIdx: f) is not { } face) continue;
            (double cotA, double cotB, double cotC) = face.Cotangents;
            (Point3d pa, Point3d pb, Point3d pc) = face.Points(mesh: mesh);
            ScatterCotangentDivergence(div: div, a: face.A, b: face.B, c: face.C, ab: pb - pa, bc: pc - pb, ca: pa - pc, cot: (A: cotA, B: cotB, C: cotC), g: faceFields[f]);
        }
        return new Arr<double>(div);
    }
    private static void ScatterCotangentDivergence(double[] div, int a, int b, int c, Vector3d ab, Vector3d bc, Vector3d ca, (double A, double B, double C) cot, Vector3d g) {
        div[a] += 0.5 * ((cot.B * (ab * g)) + (cot.C * (-ca * g)));
        div[b] += 0.5 * ((cot.C * (bc * g)) + (cot.A * (-ab * g)));
        div[c] += 0.5 * ((cot.A * (ca * g)) + (cot.B * (-bc * g)));
    }

    // --- [CDS_HOLONOMY] ---------------------------------------------------------------------
    internal static Arr<double> ComputeIntrinsicAngleDefects(MeshKernel.IntrinsicMesh imesh) {
        int n = imesh.VertexCount;
        double[] defect = new double[n];
        for (int v = 0; v < n; v++) defect[v] = 2.0 * Math.PI;
        foreach (int f in imesh.LiveFaceIndices()) {
            if (IntrinsicTriangleOf(imesh: imesh, faceIdx: f) is not { } face) continue;
            for (int side = 0; side < 3; side++) defect[face.Vertex(side: side)] -= face.Angle(side: side);
        }
        return new Arr<double>(defect);
    }
    internal static Fin<Arr<double>> DistributeHolonomy(MeshSpace space, MeshKernel.IntrinsicMesh imesh, Seq<(int Vertex, double ConeIndex)> cones, Op key) {
        Arr<double> defects = ComputeIntrinsicAngleDefects(imesh: imesh);
        return from topology in MeshKernel.TopologyDetailed(space: space, key: key)
               from _ in topology.Genus.Match(
                   Some: genus => genus > 0
                       ? Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(Arr<double>)))
                       : Fin.Succ(unit),
                   None: () => Fin.Succ(unit))
               from __ in ValidateGaussBonnet(mesh: space.Native, imesh: imesh, defects: defects, cones: cones, key: key) let u = BuildConePrimalOneForm(imesh: imesh, defects: defects, cones: cones) let star1 = ComputeIntrinsicStar1(imesh: imesh) let rhs = IntrinsicCoexactRhs(imesh: imesh, star1: star1, u: u) from beta in space.Cache.Cholesky(key: key).Bind(factor => factor.Solve(rhs: rhs, key: key)) let dBeta = IntrinsicEdgeGradient(imesh: imesh, beta: beta) select new Arr<double>([.. dBeta.AsIterable().Select(static value => -value)]);
    }
    private static Fin<Unit> ValidateGaussBonnet(Mesh mesh, MeshKernel.IntrinsicMesh imesh, Arr<double> defects, Seq<(int Vertex, double ConeIndex)> cones, Op key) {
        bool valid = mesh.GetNakedEdges() is null && defects.Count == imesh.VertexCount && defects.ForAll(RhinoMath.IsValidDouble) && !cones.Exists(c => c.Vertex < 0 || c.Vertex >= imesh.VertexCount || !RhinoMath.IsValidDouble(x: c.ConeIndex));
        double sumK = Enumerable.Sum(defects.AsIterable());
        double euler = sumK / (2.0 * Math.PI);
        double sumPrescribed = Enumerable.Sum(cones.AsIterable(), static c => c.ConeIndex);
        return !valid || Math.Abs(value: sumPrescribed - euler) > 1.0e-6
            ? Fin.Fail<Unit>(error: key.InvalidInput())
            : Fin.Succ(unit);
    }
    private static Arr<double> BuildConePrimalOneForm(MeshKernel.IntrinsicMesh imesh, Arr<double> defects, Seq<(int Vertex, double ConeIndex)> cones) {
        int n = imesh.VertexCount;
        int eCount = imesh.EdgeCount;
        double[] target = new double[n];
        for (int v = 0; v < n; v++) target[v] = -defects[index: v];
        for (int c = 0; c < cones.Count; c++) {
            (int v, double k) = cones[index: c];
            target[v] += 2.0 * Math.PI * k;
        }
        double[] u = new double[eCount];
        for (int v = 0; v < n; v++) {
            if (Math.Abs(value: target[v]) < RhinoMath.ZeroTolerance) continue;
            int e = imesh.FirstIncidentEdge(vertexIdx: v);
            if (e < 0) continue;
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            double sign = v == edge.Hi ? 1.0 : -1.0;
            u[e] += target[v] * sign;
        }
        return new Arr<double>(u);
    }
    private static Arr<double> IntrinsicCoexactRhs(MeshKernel.IntrinsicMesh imesh, Arr<double> star1, Arr<double> u) {
        int n = imesh.VertexCount;
        int eCount = imesh.EdgeCount;
        double[] rhs = new double[n];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            double weighted = star1[index: e] * u[index: e];
            rhs[edge.Lo] += -weighted;
            rhs[edge.Hi] += +weighted;
        }
        return new Arr<double>(rhs);
    }
    private static Arr<double> IntrinsicEdgeGradient(MeshKernel.IntrinsicMesh imesh, Arr<double> beta) {
        int eCount = imesh.EdgeCount;
        double[] grad = new double[eCount];
        for (int e = 0; e < eCount; e++) {
            MeshKernel.IntrinsicEdge edge = imesh.EdgeAt(index: e);
            grad[e] = beta[index: edge.Hi] - beta[index: edge.Lo];
        }
        return new Arr<double>(grad);
    }

    // --- [HEAT_SCAFFOLD] --------------------------------------------------------------------
    internal static Arr<double> BuildSourceDelta(int n, Seq<int> sources, Arr<double> mass) {
        double[] delta = new double[n];
        for (int s = 0; s < sources.Count; s++) delta[sources[index: s]] = mass[index: sources[index: s]];
        return new Arr<double>(delta);
    }
    internal static List<(int Row, int Col, double Value)> PoissonTriplets(SparseLaplacian L, Seq<int> sources) {
        List<(int Row, int Col, double Value)> result = [];
        for (int i = 0; i < L.Stiffness.Rows.Value; i++)
            for (int k = L.Stiffness.RowPtr[index: i]; k < L.Stiffness.RowPtr[index: i + 1]; k++)
                result.Add(item: (i, L.Stiffness.ColInd[index: k], L.Stiffness.Values[index: k]));
        for (int i = 0; i < sources.Count; i++) result.Add(item: (sources[index: i], sources[index: i], 1.0e10));
        return result;
    }
    internal static Vector3d[] ComputeTriangleGradients(Mesh mesh, Arr<double> u) {
        Vector3d[] gradients = new Vector3d[mesh.Faces.Count];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d n = Vector3d.CrossProduct(a: pb - pa, b: pc - pa); double twoArea = n.Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            Vector3d nUnit = n / twoArea;
            Vector3d ua = Vector3d.CrossProduct(a: nUnit, b: pc - pb) * u[index: face.A];
            Vector3d ub = Vector3d.CrossProduct(a: nUnit, b: pa - pc) * u[index: face.B];
            Vector3d uc = Vector3d.CrossProduct(a: nUnit, b: pb - pa) * u[index: face.C];
            Vector3d g = (ua + ub + uc) / twoArea;
            double len = g.Length;
            gradients[f] = len > RhinoMath.ZeroTolerance ? -g / len : Vector3d.Zero;
        }
        return gradients;
    }
    internal static Arr<double> ComputeVertexDivergence(Mesh mesh, Vector3d[] gradients) {
        int n = mesh.Vertices.Count;
        double[] div = new double[n];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d ab = pb - pa; Vector3d bc = pc - pb; Vector3d ca = pa - pc;
            double twoArea = Vector3d.CrossProduct(a: ab, b: pc - pa).Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            ScatterCotangentDivergence(div: div, a: face.A, b: face.B, c: face.C, ab: ab, bc: bc, ca: ca, cot: (A: -ab * ca / twoArea, B: ab * -bc / twoArea, C: bc * ca / twoArea), g: gradients[f]);
        }
        return new Arr<double>(div);
    }


}
