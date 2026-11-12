using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DefaultedSettingTests : CSharpCodeFixVerifier<DefaultedSettingAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task SettingsReads() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using System.Drawing;
        using Rhino;

        internal static class SettingsReads {
            internal static bool Defaulted(PersistentSettings settings) => {|#0:settings.GetBool("held", false)|};

            internal static DayOfWeek Day(PersistentSettings settings) => {|#1:settings.GetEnumValue("day", DayOfWeek.Monday)|};

            internal static bool Tried(PersistentSettings settings) => settings.TryGetBool("held", out bool value) && value;

            internal static Color Colored(PersistentSettings settings) => {|#2:settings.GetColor("color", Color.Red)|};

            internal static Color? Nullable(PersistentSettings settings, Color? color) => {|#3:settings.GetColor(defaultValue: color, key: "color")|};

            internal static bool Hidden(PersistentSettings settings, IEnumerable<string> legacy) => settings.GetSettingIsHiddenFromUserInterface("hidden", legacy);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("GetBool", """settings.TryGetBool("held", out bool value)"""),
        Diagnostic().WithLocation(1).WithArguments("GetEnumValue", """settings.TryGetEnumValue<DayOfWeek>("day", out DayOfWeek value)"""),
        Diagnostic().WithLocation(2).WithArguments("GetColor", """settings.TryGetColor("color", out Color value)"""),
        Diagnostic().WithLocation(3).WithArguments("GetColor", """settings.TryGetColor("color", out Color? value)"""));
}
