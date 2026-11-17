using Grasshopper2.Framework;
using Rasm.Rhino.PlugIns;
using Rhino.PlugIns;
using Xunit;

namespace Rasm.Rhino.Tests.Host;

// --- [SERVICES] ------------------------------------------------------------------------

[CollectionDefinition]
public sealed class Grasshopper2PlugIn : ICollectionFixture<Grasshopper2PlugIn>, IAsyncLifetime {
    public async ValueTask InitializeAsync() {
        Assert.Equal(0, (await RhinoHost.RunAsync((PlugInRegistry.LoadPlugIns([PlugIn.IdFromName("Grasshopper2")]) >> IO.lift(static () => PluginServer.LoadAllScopedPlugins())).OnUiThread())).failed);
        Assert.All(
            typeof(Grasshopper2PlugIn).Assembly.GetTypes().Where(static type => type is { IsAbstract: false, ContainsGenericParameters: false } && type.IsAssignableTo(typeof(GrasshopperIO.IStorable))),
            static type => Assert.True(GrasshopperIO.IO.ValidateIStorable(type, out string reason) && GrasshopperIO.IO.RegisterRestorableType(type), $"{type.FullName}: {reason}"));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
