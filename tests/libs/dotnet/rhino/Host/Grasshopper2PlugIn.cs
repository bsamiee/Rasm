using Grasshopper2.Framework;
using Rasm.Rhino.PlugIns;
using Rhino.PlugIns;
using Xunit;

namespace Rasm.Rhino.Tests.Host;

// --- [SERVICES] ------------------------------------------------------------------------

[CollectionDefinition]
public sealed class Grasshopper2PlugIn : ICollectionFixture<Grasshopper2PlugIn>, IAsyncLifetime {
    public async ValueTask InitializeAsync() =>
        Assert.Equal(0, (await RhinoHost.RunAsync((PlugInRegistry.LoadPlugIns([PlugIn.IdFromName("Grasshopper2")]) >> IO.lift(static () => PluginServer.LoadAllScopedPlugins())).OnUiThread())).failed);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
