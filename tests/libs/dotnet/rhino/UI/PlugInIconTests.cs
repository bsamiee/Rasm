using System.Drawing;
using System.Reflection;
using System.Runtime.Loader;
using Rhino.PlugIns;
using Rhino.UI;
using Xunit;

namespace Rasm.Rhino.Tests.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlugInIconTests {

    // --- [ICONS]

    private static readonly HashMap<string, string> Shells = toHashMap(
        from path in (AppContext.GetData("RhinoPlugIns") as string ?? throw new InvalidOperationException("runtime option RhinoPlugIns is absent, build through the project")).Split(';')
        select (Path.GetFileName(path), path));

    public static TheoryData<string, int> PlugInSizes { get; } = [.. from shell in Shells.Keys from size in (int[])[16, 24, 32, 48, 256] select (shell, size)];

    [Theory]
    [MemberData(nameof(PlugInSizes))]
    public async Task RhinoReadsTheDescribedPlugInIconAsAVisibleBitmapOfEachStandardSize(string shell, int size) =>
        Assert.Equal((new Size(size, size), true), await RhinoHost.RunAsync(IO.lift(() => {
            AssemblyLoadContext isolated = new(shell, isCollectible: true);
            try {
                Assembly plugIn = isolated.LoadFromAssemblyPath(Shells[shell]);
                string icon = plugIn.GetCustomAttributes<PlugInDescriptionAttribute>().Single(static description => description.DescriptionType is DescriptionType.Icon).Value;
                using Bitmap read = DrawingUtilities.LoadBitmapWithScaleDown(icon, size, plugIn);
                return (read.Size, Enumerable.Range(0, size * size).Any(pixel => read.GetPixel(pixel % size, pixel / size).A > 0));
            } finally {
                isolated.Unload();
            }
        }).OnUiThread()));
}
