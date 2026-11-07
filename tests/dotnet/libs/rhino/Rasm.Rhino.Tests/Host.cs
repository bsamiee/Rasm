using Rhino;
using Xunit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class Host {
    [Fact]
    public void RunsInsideRhino() => Assert.Equal(("Rhinoceros", typeof(RhinoApp).Assembly.GetName().Version?.Major), (Path.GetFileName(Environment.ProcessPath), RhinoApp.Version.Major));
}
