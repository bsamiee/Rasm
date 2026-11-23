using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TrySiblingTests : CSharpCodeFixVerifier<TrySiblingAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsMembersWhoseTrySiblingTakesTheSameArgumentsAndAnOutOfTheResultAndIgnoresMismatchedSiblings() => new CSharpCodeFixTest<TrySiblingAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using Catalog;
            using Rhino;

            internal static class Called {
                internal static int Found(Catalog.Registry registry) => {|#0:registry.Find("unit")|};
                internal static int Parsed(string text) => {|#1:Catalog.Registry.Parse(text)|};
                internal static T Chosen<T>(Catalog.Registry registry) where T : struct => {|#2:registry.Pick<T>("unit")|};
                internal static int Located(Catalog.Registry registry) => {|#3:registry.Locate("unit")|};
                internal static long Counted(Catalog.Registry registry) => registry.Count("unit");
                internal static void Dropped(Catalog.Registry registry) => registry.Remove("unit");
                internal static int Sized(Catalog.Registry registry) => registry.Size("unit");
                internal static int Ranked(Catalog.Registry registry) => registry.Rank("unit");
                internal static int Slotted(Catalog.Registry registry) => registry.Slot("unit");
                internal static int Weighed(Catalog.Registry registry) => registry.Weigh("unit");
                internal static string Setting(PersistentSettings settings) => {|#4:settings.GetString("unit")|};
                internal static string Defaulted(PersistentSettings settings) => settings.GetString("unit", "mm");
            }

            namespace Catalog {
                internal static class Lookups {
                    internal static int Locate(this Registry registry, string name) => name.Length;
                    internal static bool TryLocate(this Registry registry, string name, out int located) => (located = name.Length) > 0;
                }

                internal sealed class Registry {
                    internal int Find(string name) => name.Length;
                    internal bool TryFind(string name, out int found) => (found = name.Length) > 0;
                    internal static int Parse(string text) => text.Length;
                    internal static bool TryParse(string text, out int value) => (value = text.Length) > 0;
                    internal T Pick<T>(string name) where T : struct => default;
                    internal bool TryPick<T>(string name, out T picked) where T : struct {
                        picked = default;
                        return name.Length > 0;
                    }
                    internal long Count(string name) => name.Length;
                    internal bool TryCount(string name, out int counted) => (counted = name.Length) > 0;
                    internal void Remove(string name) => _ = name;
                    internal bool TryRemove(string name, out bool removed) => removed = name.Length > 0;
                    internal int Size(string name) => name.Length;
                    internal static bool TrySize(string name, out int size) => (size = name.Length) > 0;
                    internal int Rank(string name) => name.Length;
                    internal int TryRank(string name, out int rank) => rank = name.Length;
                    internal int Slot(string name) => name.Length;
                    internal bool TrySlot(string name, ref int slot) => (slot = name.Length) > 0;
                    internal int Weigh(string name) => name.Length;
                    internal bool TryWeigh(object name, out int weight) => (weight = name.GetHashCode()) > 0;
                }
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3110.types = T:Rhino.PersistentSettings|T:Catalog.Registry|T:Catalog.Lookups
            """), }, },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("""registry.Find("unit")""", "TryFind"),
            Diagnostic().WithLocation(1).WithArguments("Catalog.Registry.Parse(text)", "TryParse"),
            Diagnostic().WithLocation(2).WithArguments("""registry.Pick<T>("unit")""", "TryPick"),
            Diagnostic().WithLocation(3).WithArguments("""registry.Locate("unit")""", "TryLocate"),
            Diagnostic().WithLocation(4).WithArguments("""settings.GetString("unit")""", "TryGetString"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsAnIndexerReadOfAListedTypeAndIgnoresItsWriteAndUnlistedTypes() => new CSharpCodeFixTest<TrySiblingAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System.Collections.Generic;
            using Rhino.Collections;

            internal static class Indexed {
                internal static object Read(ArchivableDictionary entries) => {|#0:entries["unit"]|};
                internal static void Stored(ArchivableDictionary entries, object value) => entries["unit"] = value;
                internal static int Unlisted(Dictionary<string, int> entries) => entries["unit"];
            }
            """,
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3110.types = T:Rhino.Collections.ArchivableDictionary
            """), }, },
        ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("""entries["unit"]""", "TryGetValue") },
    }.RunAsync(TestContext.Current.CancellationToken);
}
