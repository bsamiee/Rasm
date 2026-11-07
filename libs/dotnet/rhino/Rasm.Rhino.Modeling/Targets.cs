namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveFit {
    public (SweepRebuild RebuildType, int RebuildPointCount, double RefitTolerance) Parameters => Switch(
        asIs: static _ => (SweepRebuild.None, 0, 0d), rebuild: static fit => (SweepRebuild.Rebuild, fit.Count, 0d), refit: static fit => (SweepRebuild.Refit, 0, fit.Tolerance));

    public sealed record AsIs : CurveFit;

    public sealed record Rebuild(int Count) : CurveFit;

    public sealed record Refit(double Tolerance) : CurveFit;
}
