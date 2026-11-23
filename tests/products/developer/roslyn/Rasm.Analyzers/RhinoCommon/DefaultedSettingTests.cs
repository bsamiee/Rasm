using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefaultedSettingTests : CSharpCodeFixVerifier<DefaultedSettingAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task GetTakingADefaultIsReportedWithItsTryGetCallWhileGetWithoutDefaultAndTryGetAreSilent() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using Rhino;

        internal static class Reads {
            internal static bool Defaulted(PersistentSettings settings) => {|#0:settings.GetBool("held", false)|};

            internal static DayOfWeek Day(PersistentSettings settings) => {|#1:settings.GetEnumValue("day", DayOfWeek.Monday)|};

            internal static Color Colored(PersistentSettings settings, Color color) => {|#2:settings.GetColor(defaultValue: color, key: "color")|};

            internal static bool Required(PersistentSettings settings) => settings.GetBool("held");

            internal static bool Tried(PersistentSettings settings) => settings.TryGetBool("held", out bool value) && value;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("GetBool", """settings.TryGetBool("held", out bool value)"""),
        Diagnostic().WithLocation(1).WithArguments("GetEnumValue", """settings.TryGetEnumValue<DayOfWeek>("day", out DayOfWeek value)"""),
        Diagnostic().WithLocation(2).WithArguments("GetColor", """settings.TryGetColor("color", out Color value)"""));
}
