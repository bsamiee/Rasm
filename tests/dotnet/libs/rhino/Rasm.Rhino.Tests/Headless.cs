using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.UI;
using Xunit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class Headless {
    // --- [THREADS]

    [Fact]
    public async Task OnUiThreadRunsOnRhinoUiThread() =>
        Assert.Equal(false, await RunAsync(IO.lift(static () => RhinoApp.InvokeRequired).OnUiThread()));

    // --- [SCOPES]

    [Fact]
    public async Task HeadlessDocumentClosesWithItsScope() =>
        Assert.Equal(false, await RunAsync(
            from serial in Documents.With(new DocumentSource.Headless(None), static doc => IO.pure(doc.RuntimeSerialNumber))
            from open in IO.lift(() => RhinoDoc.FromRuntimeSerialNumber(serial) is not null).OnUiThread()
            select open));

    // --- [UNITS]

    [Fact]
    public async Task CommittedUnitsAndTolerancesReadBack() {
        UnitsAndTolerances written = new(LengthUnit.Millimeters, new Tolerances(0.01, 0.001, Math.PI / 360), DistanceDisplayMode.Decimal, 4);
        Assert.Equal(written, await RunAsync(Documents.With(new DocumentSource.Headless(None), doc =>
            (Commits.Commit(doc, nameof(UnitsAndTolerances), new RedrawPolicy.Silent(), written.Write(doc, Space.Model)) >> IO.lift(() => Space.Model.Read(doc))).OnUiThread())));
    }

    [Theory]
    [InlineData(1d, 25.4d)]
    [InlineData(12d, 304.8d)]
    public void InchesConvertToMillimetres(double inches, double millimetres) =>
        Assert.Equal(millimetres, Quantities.As(Quantities.From(inches, LengthUnit.Inches), LengthUnit.Millimeters), 12);

    [Fact]
    public void DecimalResolutionIsOneUnitInTheLastPlace() =>
        Assert.Equal(0.001d, Quantities.As(Quantities.Resolution(DistanceDisplayMode.Decimal, 3, LengthUnit.Millimeters), LengthUnit.Millimeters), 15);

    [Fact]
    public void FractionalResolutionIsAPowerOfTwoInchFraction() =>
        Assert.Equal(1d / 16d, Quantities.As(Quantities.Resolution(DistanceDisplayMode.Fractional, 4, LengthUnit.Meters), LengthUnit.Inches), 15);

    // --- [LAYERS]

    [Fact]
    public async Task UnknownLayerPathFailsAsMissing() =>
        Assert.True(await RunAsync(Documents.With(new DocumentSource.Headless(None), static doc => Layers.Resolve(doc, new LayerRef.Path("Absent::Layer")).Map(static layer => layer.FullPath).OnUiThread()))
            is Fin<string>.Fail { Error: RhinoFault { Reason: var reason } } && reason == RhinoReason.Missing);

    // --- [EXECUTION]

    private static Task<Fin<T>> RunAsync<T>(IO<T> effect) => Session.RunSafeAsync(effect, TestContext.Current.CancellationToken);
}
