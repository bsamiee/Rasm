namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveFit {
    private CurveFit(SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance) => Parameters = (rebuildType, rebuildPointCount, refitTolerance);

    public (SweepRebuild RebuildType, int RebuildPointCount, double RefitTolerance) Parameters { get; }

    public sealed record AsIs() : CurveFit(SweepRebuild.None, 0, 0d);

    public sealed record Rebuild(int Count) : CurveFit(SweepRebuild.Rebuild, Count, 0d);

    public sealed record Refit(double Tolerance) : CurveFit(SweepRebuild.Refit, 0, Tolerance);
}
