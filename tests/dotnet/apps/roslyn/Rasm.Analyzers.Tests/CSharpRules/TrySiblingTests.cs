using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TrySiblingTests : CSharpCodeFixVerifier<TrySiblingAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "Throwing instance, static, generic, indexer, and extension members of listed types with a Try sibling are reported; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/LoggerParameterTypeAnalyzerTests.cs BeginScope_InvalidParameterType_XmlCommentId")]
    public Task ThrowingForms() => new CSharpCodeFixTest<TrySiblingAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using Catalog;
            using Rhino;
            using Rhino.Collections;

            internal static class ThrowingForms {
                internal static string Setting(PersistentSettings settings) => {|#0:settings.GetString("unit")|};

                internal static int Count(PersistentSettings settings) => {|#1:settings.GetInteger("count")|};

                internal static object Entry(ArchivableDictionary entries) => {|#2:entries["unit"]|};

                internal static double Scale(ArchivableDictionary entries) => {|#3:entries.GetDouble("scale")|};

                internal static int Found(Catalog.Registry registry) => {|#4:registry.Find("unit")|};

                internal static int Parsed(string text) => {|#5:Catalog.Registry.Parse(text)|};

                internal static T Chosen<T>(Catalog.Registry registry) where T : struct => {|#6:registry.Pick<T>("unit")|};

                internal static int Located(Catalog.Registry registry) => {|#7:registry.Locate("unit")|};
            }

            namespace Catalog {
                internal static class Lookups {
                    internal static int Locate(this Registry registry, string name) => name.Length;

                    internal static bool TryLocate(this Registry registry, string name, out int located) {
                        located = name.Length;
                        return true;
                    }
                }

                internal sealed class Registry {
                    internal int Find(string name) => name.Length;

                    internal bool TryFind(string name, out int found) {
                        found = name.Length;
                        return true;
                    }

                    internal static int Parse(string text) => text.Length;

                    internal static bool TryParse(string text, out int value) {
                        value = text.Length;
                        return true;
                    }

                    internal T Pick<T>(string name) where T : struct => name.Length > 0 ? default(T) : throw new System.ArgumentException(name, nameof(name));

                    internal bool TryPick<T>(string name, out T picked) where T : struct {
                        picked = default;
                        return name.Length > 0;
                    }
                }
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3110.types = T:Rhino.PersistentSettings|T:Rhino.Collections.ArchivableDictionary|T:Catalog.Registry|T:Catalog.Lookups
            """), }, },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("""settings.GetString("unit")""", "TryGetString"),
            Diagnostic().WithLocation(1).WithArguments("""settings.GetInteger("count")""", "TryGetInteger"),
            Diagnostic().WithLocation(2).WithArguments("""entries["unit"]""", "TryGetValue"),
            Diagnostic().WithLocation(3).WithArguments("""entries.GetDouble("scale")""", "TryGetDouble"),
            Diagnostic().WithLocation(4).WithArguments("""registry.Find("unit")""", "TryFind"),
            Diagnostic().WithLocation(5).WithArguments("Catalog.Registry.Parse(text)", "TryParse"),
            Diagnostic().WithLocation(6).WithArguments("""registry.Pick<T>("unit")""", "TryPick"),
            Diagnostic().WithLocation(7).WithArguments("""registry.Locate("unit")""", "TryLocate"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact(DisplayName = "Try forms, members with no matching sibling, writes, and unlisted types are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldNotReportError")]
    public Task HandledForms() => new CSharpCodeFixTest<TrySiblingAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System.Collections.Generic;
            using System.Globalization;
            using Rhino;
            using Rhino.Collections;

            internal static class HandledForms {
                internal static string Tried(PersistentSettings settings) => settings.TryGetString("unit", out string value) ? value : "";

                internal static string Defaulted(PersistentSettings settings) => settings.GetString("unit", "mm");

                internal static void Stored(ArchivableDictionary entries, object value) => entries["unit"] = value;

                internal static int Unlisted(Dictionary<string, int> entries) => entries["unit"];

                internal static int Number(string text) => int.Parse(text, CultureInfo.InvariantCulture);

                internal static long Wide(Catalog.Ledger ledger) => ledger.Count("unit");

                internal static void Dropped(Catalog.Ledger ledger) => ledger.Remove("unit");
            }

            namespace Catalog {
                internal sealed class Ledger {
                    internal long Count(string name) => name.Length;

                    internal bool TryCount(string name, out int counted) {
                        counted = name.Length;
                        return true;
                    }

                    internal void Remove(string name) => _ = name;

                    internal bool TryRemove(string name, out bool removed) {
                        removed = name.Length > 0;
                        return removed;
                    }
                }
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3110.types = T:Rhino.PersistentSettings|T:Rhino.Collections.ArchivableDictionary|T:Catalog.Ledger
            """), }, },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact(DisplayName = "No type is reported without a RASM3110.types row; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/LoggerParameterTypeAnalyzerTests.cs LogInformation_NoConfigurationFile")]
    public Task UnlistedTypes() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal static class UnlistedTypes {
            internal static string Setting(PersistentSettings settings) => settings.GetString("unit");
        }
        """);
}
