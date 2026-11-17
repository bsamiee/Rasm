using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Library;
using Rhino.Geometry;
using Rhino.PlugIns;
using Xunit;

namespace Tests;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class Grasshopper2Session : IAsyncLifetime {
    public async ValueTask InitializeAsync() {
        Assert.True(await RhinoTestHost.OnMainThread(static () => PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2"))));
        Assert.Equal(0, (await RhinoTestHost.OnMainThread(static () => PluginServer.LoadAllScopedPlugins())).Item2);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SlabVolumeComponentTests : IClassFixture<Grasshopper2Session> {

    // --- [SOLUTION]

    private static async Task<(Solution Solution, SlabVolumeComponent Component)> Solve(Curve outline, double height) {
        SlabVolumeComponent component = new();
        using CancellationTokenSource limit = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        limit.CancelAfter(TimeSpan.FromMinutes(1));
        Task<Solution> solving = await RhinoTestHost.OnMainThread(() => {
            Document document = Document.NewInertDocument();
            document.Objects.Add(component);
            component.Parameters.Input(0).PersistentDataWeak = Garden.TreeFromList([outline]);
            component.Parameters.Input(1).PersistentDataWeak = Garden.TreeFromList([height]);
            return document.Solution.Start(limit, SolutionMode.Headless);
        });
        return (await solving, component);
    }

    // --- [OUTPUTS]

    [Fact]
    public async Task VolumeOutputMatchesExtrusion() {
        var (solution, component) = await Solve(new Rectangle3d(Plane.WorldXY, 4, 2).ToNurbsCurve(), 0.5);
        Assert.Equal(SolutionPhase.Completed, solution.Phase);
        Assert.Equal(0, component.State.Data.Messages.ErrorCount);
        Assert.Equal(4.0, Assert.Single(component.Parameters.Output(0).State.Data.Tree().TryConvert<double>(TestContext.Current.CancellationToken).AllItems), 1e-6);
    }

    [Fact]
    public async Task OpenOutlineRaisesErrorAndNoVolume() {
        var (solution, component) = await Solve(new Line(Point3d.Origin, new Point3d(1, 0, 0)).ToNurbsCurve(), 0.5);
        Assert.Equal(SolutionPhase.Completed, solution.Phase);
        Assert.Equal("No slab", Assert.Single(component.State.Data.Messages.Errors).Text);
        Assert.Equal(0, component.Parameters.Output(0).State.Data.Tree().ItemCount);
    }
}
