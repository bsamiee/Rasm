using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using Xunit;

namespace Rasm.Rhino.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlatformsTests {

    // --- [LOADING]

    [Theory]
    [InlineData("Windows", new[] { "Microsoft.macOS" })]
    [InlineData("macOS", new[] { "PresentationCore", "PresentationFramework", "WindowsBase" })]
    public void EveryTypeOfThePlugInAssemblyLoadsWhereTheOtherPlatformsInterfaceFrameworkIsAbsent(string platform, string[] absent) {
        Assembly plugIn = typeof(Session).Assembly;
        Assert.Contains(plugIn.GetReferencedAssemblies(), referenced => absent.Contains(referenced.Name, StringComparer.Ordinal));
        Without isolated = new(platform, absent);
        try {
            Assert.Null(Record.Exception(() => isolated.LoadFromAssemblyPath(plugIn.Location).GetTypes()));
        } finally {
            isolated.Unload();
        }
    }

    private sealed class Without(string platform, string[] absent) : AssemblyLoadContext(platform, isCollectible: true) {
        protected override Assembly? Load(AssemblyName assemblyName) =>
            absent.Contains(assemblyName.Name, StringComparer.Ordinal) ? AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect) : null;
    }
}
