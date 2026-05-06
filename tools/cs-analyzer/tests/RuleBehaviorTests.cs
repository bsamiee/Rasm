using System.Collections.Immutable;
using System.Globalization;
using Foundation.CSharp.Analyzers.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Foundation.CSharp.Analyzers.Tests;

public sealed class RuleBehaviorTests {
    private static readonly ImmutableArray<RuleCase> Cases = [
        new("CSP0001", File(scope: "Domain/Services", type: "ImperativeConditional"), Domain(type: "ImperativeConditional", members: """
            public int Clamp(int value) {
                if (value > 0) {
                    return value;
                }
                return 0;
            }
            """)),
        new("CSP0002", File(scope: "Domain/Services", type: "MatchCollapse"), WithLanguageExt(ns: "Domain.Services", type: "MatchCollapse", members: """
            public int Collapse(LanguageExt.Fin<int> value) {
                int result = value.Match(Succ: static input => input, Fail: static _ => 0);
                return result;
            }
            """)),
        new("CSP0003", File(scope: "Domain/Models", type: "DoubleSignature"), Model(type: "DoubleSignature", members: """
            public double Magnitude { get; }

            public DoubleSignature(double magnitude) {
                Magnitude = magnitude;
            }
            """)),
        new("CSP0003", File(scope: "Domain/Services", type: "NestedPrimitiveSignature"), Domain(type: "NestedPrimitiveSignature", members: """
            public System.Threading.Tasks.Task<string> Read() => System.Threading.Tasks.Task.FromResult("value");
            """)),
        new("CSP0003", File(scope: "SharedKernel/Primitives", type: "SharedPrimitiveSignature"), Source(ns: "SharedKernel.Primitives", type: "SharedPrimitiveSignature", members: """
            public string Name { get; }

            public SharedPrimitiveSignature(string name) {
                Name = name;
            }
            """)),
        new("CSP0004", File(scope: "Domain/Services", type: "CollectionSignature"), Domain(type: "CollectionSignature", members: """
            public System.Collections.Generic.List<int> Read() => new();
            """)),
        new("CSP0005", File(scope: "Domain/Services", type: "OverloadSpam"), Domain(type: "OverloadSpam", members: """
            public int Compute() => 0;
            public int Compute(int first) => first;
            public int Compute(int first, int second) => first + second;
            """)),
        new("CSP0006", File(scope: "Domain/Services", type: "ValueTaskResult"), Domain(type: "ValueTaskResult", members: """
            public int Run(System.Threading.Tasks.ValueTask<int> value) => value.Result;
            """)),
        new("CSP0007", File(scope: "Domain/Services", type: "WallClock"), Domain(type: "WallClock", members: """
            public System.DateTime Read() => System.DateTime.UtcNow;
            """)),
        new("CSP0008", File(scope: "Domain/Services", type: "HttpClientConstruction"), Domain(type: "HttpClientConstruction", members: """
            public System.Net.Http.HttpClient Create() => new();
            """)),
        new("CSP0009", File(scope: "Domain/Services", type: "TryFinallyUsage"), Domain(type: "TryFinallyUsage", members: """
            public int Compute() {
                try {
                    return 1;
                } finally {
                    _ = "cleanup".Length;
                }
            }
            """)),
        new("CSP0010", File(scope: "Domain/Services", type: "AsyncVoid"), Domain(type: "AsyncVoid", members: """
            public async void Run() {
                await System.Threading.Tasks.Task.Yield();
            }
            """)),
        new("CSP0011", File(scope: "Domain/Services", type: "MutableCollection"), Domain(type: "MutableCollection", members: """
            public object Create() => new System.Collections.Generic.List<int>();
            """)),
        new("CSP0012", File(scope: "Domain/Models", type: "MutableAutoProperty"), Model(type: "MutableAutoProperty", members: """
            public int Value { get; set; }
            """)),
        new("CSP0013", File(scope: "Domain/Services", type: "ClosureCapture"), Domain(type: "ClosureCapture", members: """
            public int Run(int input) {
                int offset = 3;
                System.Func<int, int> projector = value => value + offset;
                return projector(input);
            }
            """)),
        new("CSP0014", File(scope: "Domain/Services", type: "TaskRunFanOut"), Domain(type: "TaskRunFanOut", members: """
            public System.Threading.Tasks.Task<int> Run() =>
                System.Threading.Tasks.Task.Run(static () => 1);
            """)),
        new("CSP0015", File(scope: "Domain/Services", type: "VarInference"), Domain(type: "VarInference", members: """
            public int Run() {
                var value = 1;
                return value;
            }
            """)),
        new("CSP0017", File(scope: "Domain/Performance", type: "HotPathClosure"), Source(ns: "Domain.Performance", type: "HotPathClosure", members: """
            public int Run(int input) {
                int offset = 3;
                System.Func<int, int> projector = value => value + offset;
                return projector(input);
            }
            """)),
        new("CSP0101", File(scope: "Integration", type: "BoundaryAdapterUsage"), Boundary(type: "BoundaryAdapterUsage", members: """
            public int Clamp(int value) {
                if (value > 0) {
                    return value;
                }
                return 0;
            }
            """)),
        new("CSP0102", File(scope: "Integration", type: "InvalidReasonBoundary"), Boundary(type: "InvalidReasonBoundary", members: """
            [BoundaryImperativeExemption(
                ruleId: "CSP0001",
                reason: BoundaryImperativeReason.CleanupFinally,
                ticket: "RASM-BOUNDARY-0102",
                expiresOnUtc: "2999-01-01T00:00:00Z")]
            public int Clamp(int value) {
                if (value > 0) {
                    return value;
                }
                return 0;
            }
            """)),
        new("CSP0103", File(scope: "Integration", type: "ExpiredBoundary"), Boundary(type: "ExpiredBoundary", members: """
            [BoundaryImperativeExemption(
                ruleId: "CSP0001",
                reason: BoundaryImperativeReason.ProtocolRequired,
                ticket: "RASM-BOUNDARY-0103",
                expiresOnUtc: "2000-01-01T00:00:00Z")]
            public int Clamp(int value) {
                if (value > 0) {
                    return value;
                }
                return 0;
            }
            """)),
        new("CSP0104", File(scope: "Domain/Services", type: "NullSentinel"), Domain(type: "NullSentinel", members: """
            public bool Missing(string value) => value == null;
            """)),
        new("CSP0201", File(scope: "Domain/Services", type: "ArraySignature"), Domain(type: "ArraySignature", members: """
            public int[] Read() => [];
            """)),
        new("CSP0202", File(scope: "Domain/Models", type: "MutableField"), Model(type: "MutableField", members: """
            public int Count;
            """)),
        new("CSP0203", File(scope: "Domain/Models", type: "PublicCtorPrimitive"), WithFin(ns: "Domain.Models", type: "PublicCtorPrimitive", declaration: """
            public readonly record struct PublicCtorPrimitive(int Value) {
                public static LanguageExt.Fin<PublicCtorPrimitive> Create(int value) => new();
            }
            """)),
        new("CSP0204", File(scope: "Domain/Services", type: "ConcurrentCollection"), Domain(type: "ConcurrentCollection", members: """
            public object Create() => new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
            """)),
        new("CSP0301", File(scope: "Domain/Services", type: "DiscardedTask"), Domain(type: "DiscardedTask", members: """
            public void Run() {
                _ = SendAsync();
            }

            private System.Threading.Tasks.Task SendAsync() => System.Threading.Tasks.Task.CompletedTask;
            """)),
        new("CSP0302", File(scope: "Domain/Services", type: "UnboundedWhenAll"), Domain(type: "UnboundedWhenAll", members: """
            public System.Threading.Tasks.Task Run(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task> tasks) =>
                System.Threading.Tasks.Task.WhenAll(tasks);
            """)),
        new("CSP0303", File(scope: "Domain/Services", type: "RunInTransform"), WithEff(ns: "Domain.Services", type: "RunInTransform", members: """
            public int Run(LanguageExt.Eff<int> effect) => effect.Run();
            """)),
        new("CSP0401", File(scope: "Domain/Services", type: "TimerConstruction"), Domain(type: "TimerConstruction", members: """
            public System.Threading.Timer Create() =>
                new(static _ => { }, state: null, dueTime: 0, period: 1);
            """)),
        new("CSP0402", File(scope: "Domain/Services", type: "FluentValidationUsage"), """
            namespace FluentValidation {
                public sealed class Validator {
                    public void RuleFor() { }
                }
            }

            namespace Domain.Services {
                public sealed class FluentValidationUsage {
                    public void Run() {
                        new FluentValidation.Validator().RuleFor();
                    }
                }
            }
            """),
        new("CSP0403", File(scope: "Integration", type: "SyncValidate"), BoundaryPrelude(source: """
            namespace FluentValidation {
                public interface IValidator<T> { }
                public sealed class Validator : IValidator<int> {
                    public bool Validate(int value) => true;
                }
            }

            namespace Integration {
                [Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]
                public sealed class SyncValidate {
                    public bool Run(int value) => new FluentValidation.Validator().Validate(value);
                }
            }
            """)),
        new("CSP0404", File(scope: "Domain/Services", type: "UnboundedChannel"), Domain(type: "UnboundedChannel", members: """
            public System.Threading.Channels.Channel<int> Create() =>
                System.Threading.Channels.Channel.CreateUnbounded<int>();
            """)),
        new("CSP0405", File(scope: "Domain/Services", type: "BoundedChannelMissingFullMode"), Domain(type: "BoundedChannelMissingFullMode", members: """
            public System.Threading.Channels.Channel<int> Create() =>
                System.Threading.Channels.Channel.CreateBounded<int>(1);
            """)),
        new("CSP0406", File(scope: "Application/Bootstrap", type: "CompositionRoot"), ScrutorRoot(ns: "Application.Bootstrap")),
        new("CSP0406", File(scope: "Infrastructure/DependencyInjection", type: "CompositionRoot"), ScrutorRoot(ns: "Infrastructure.DependencyInjection")),
        new("CSP0501", File(scope: "Domain/Services", type: "InterfacePollution"), """
            namespace Domain.Services;

            public interface IPort {
                int Read();
            }

            public sealed class Port : IPort {
                public int Read() => 1;
            }
            """),
        new("CSP0502", File(scope: "Domain/Services", type: "PositionalArguments"), """
            namespace Domain.Services;

            public sealed class PositionalArguments {
                public int Run() => Target.Apply(1);
            }

            public static class Target {
                public static int Apply(int value) => value;
            }
            """),
        new("CSP0503", File(scope: "Domain/Services", type: "SingleUsePrivateHelper"), Domain(type: "SingleUsePrivateHelper", members: """
            public int Run() => Once();
            private int Once() => 1;
            """)),
        new("CSP0504", File(scope: "Integration", type: "DomainScopeAttributeUsage"), Contract(type: "DomainScopeAttributeUsage", attributes: "[DomainScope]", members: """
            public void Execute() { }
            """)),
        new("CSP0504", File(scope: "Domain/Services", type: "ReturnPolicy"), Domain(type: "ReturnPolicy", members: """
            public void Compute() { }
            """)),
        new("CSP0505", File(scope: "Domain/Traits", type: "TypeClassPolicy"), """
            namespace Domain.Traits;

            public interface ITypeClassPolicy {
                int Compute(int value);
            }
            """),
        new("CSP0506", File(scope: "Domain/Projection", type: "ProjectionPolicy"), """
            namespace Domain.Projection;

            public sealed class Customer {
                public string Name { get; }

                public Customer(string name) {
                    Name = name;
                }
            }

            public static class CustomerProjection {
                public static string AsDisplayName(Customer customer) => customer.Name;
            }
            """),
        new("CSP0601", File(scope: "Domain/Performance", type: "HotPathLinq"), Source(ns: "Domain.Performance", type: "HotPathLinq", members: """
            public int[] Run(int[] values) =>
                System.Linq.Enumerable.Select(values, static value => value).ToArray();
            """)),
        new("CSP0602", File(scope: "Domain/Performance", type: "HotPathLambda"), Source(ns: "Domain.Performance", type: "HotPathLambda", members: """
            public int Run(int input) {
                System.Func<int, int> projector = value => value + 1;
                return projector(input);
            }
            """)),
        new("CSP0603", File(scope: "Domain/Interop", type: "LibraryImportRequired"), """
            namespace Domain.Interop;

            public static class LibraryImportRequired {
                [System.Runtime.InteropServices.DllImport("native")]
                private static extern int Native();
                public static int Run() => Native();
            }
            """),
        new("CSP0604", File(scope: "Domain/Services", type: "TelemetryIdentity"), Domain(type: "TelemetryIdentity", members: """
            public System.Diagnostics.ActivitySource Create() => new("domain");
            """)),
        new("CSP0605", File(scope: "Domain/Services", type: "HardcodedOtlp"), Domain(type: "HardcodedOtlp", members: """
            public string Endpoint() => "http://otel-collector:4317/v1/traces";
            """)),
        new("CSP0606", File(scope: "Domain/Services", type: "RegexStaticCall"), Domain(type: "RegexStaticCall", members: """
            public bool Run(string value) =>
                System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Z]+$");
            """)),
        new("CSP0607", File(scope: "Domain/Services", type: "GeneratedRegexCharset"), """
            namespace Domain.Services;

            public sealed partial class GeneratedRegexCharset {
                [System.Text.RegularExpressions.GeneratedRegex("^[ABC]{3}$")]
                private static partial System.Text.RegularExpressions.Regex CodeRegex();
            }
            """),
        new("CSP0608", File(scope: "Domain/Services", type: "EnumeratorCancellation"), Domain(type: "EnumeratorCancellation", members: """
            public async System.Collections.Generic.IAsyncEnumerable<int> Stream(System.Threading.CancellationToken cancellationToken) {
                await System.Threading.Tasks.Task.Yield();
                yield return 1;
            }
            """)),
        new("CSP0701", File(scope: "Domain/Models", type: "PrimitiveShape"), """
            namespace Domain.Models;

            public readonly struct PrimitiveShape {
                public int Value { get; }
            }
            """),
        new("CSP0702", File(scope: "Domain/Models", type: "SiblingUnion"), """
            namespace Domain.Models;

            public abstract record Result;
            public sealed record Success : Result;
            """),
        new("CSP0703", File(scope: "Domain/Models", type: "NestedInvalidValidation"), """
            namespace LanguageExt {
                public sealed class Fin<T> { }
                public sealed class Validation<TError, TValue> { }
            }

            namespace Domain.Models {
                public sealed class NestedInvalidValidation {
                    public LanguageExt.Fin<LanguageExt.Validation<string, NestedInvalidValidation>> Create() => new();
                }
            }
            """),
        new("CSP0704", File(scope: "Domain/Services", type: "RegexRuntimeConstruction"), Domain(type: "RegexRuntimeConstruction", members: """
            public System.Text.RegularExpressions.Regex Create() => new(pattern: "^[A-Z]+$");
            """)),
        new("CSP0705", File(scope: "Integration", type: "BoundaryMatchCollapse"), WithLanguageExt(ns: "Integration", type: "BoundaryMatchCollapse", attributes: "[Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]", members: """
            public int Run(LanguageExt.Fin<int> value) {
                int result = value.Match(Succ: static input => input, Fail: static _ => 0);
                return result;
            }
            """)),
        new("CSP0706", File(scope: "Domain/Services", type: "EarlyReturnGuard"), Domain(type: "EarlyReturnGuard", members: """
            public int Run(bool valid) {
                if (!valid) {
                    return 0;
                }
                return 1;
            }
            """)),
        new("CSP0707", File(scope: "Domain/Services", type: "VariableReassignment"), Domain(type: "VariableReassignment", members: """
            public int Run(int value) {
                int current = value;
                current = current + 1;
                return current;
            }
            """)),
        new("CSP0708", File(scope: "Domain/Services", type: "ApiSurfaceInflation"), Domain(type: "ApiSurfaceInflation", members: """
            public int Get() => 1;
            public int GetMany() => 1;
            public int TryGet() => 1;
            public int GetOrDefault() => 1;
            """)),
        new("CSP0709", File(scope: "Domain/Services", type: "NullPattern"), Domain(type: "NullPattern", members: """
            public bool Missing(string value) => value is null;
            """)),
        new("CSP0710", File(scope: "Domain/Services", type: "FilterMapChain"), """
            namespace LanguageExt {
                public sealed class Seq<T> {
                    public Seq<T> Filter(System.Func<T, bool> predicate) => this;
                    public Seq<TOut> Map<TOut>(System.Func<T, TOut> selector) => new();
                }
            }

            namespace Domain.Services {
                public sealed class FilterMapChain {
                    public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                        values.Filter(static value => value > 0).Map(static value => value);
                }
            }
            """),
        new("CSP0711", File(scope: "Domain/Services", type: "AsyncAwaitInEff"), WithEff(ns: "Domain.Services", type: "AsyncAwaitInEff", members: """
            public async LanguageExt.Eff<int> Run() {
                await System.Threading.Tasks.Task.Yield();
                return new LanguageExt.Eff<int>();
            }
            """)),
        new("CSP0712", File(scope: "Domain/Models", type: "AtomProperty"), """
            namespace LanguageExt {
                public sealed class Atom<T> { }
            }

            namespace Domain.Models {
                public sealed class AtomProperty {
                    public LanguageExt.Atom<int> State { get; } = new();
                }
            }
            """),
        new("CSP0713", File(scope: "Domain/Models", type: "InvalidValidationFactory"), """
            namespace LanguageExt {
                public sealed class Validation<TError, TValue> { }
            }

            namespace Domain.Models {
                public readonly record struct CustomerId(int Value) {
                    public static LanguageExt.Validation<string, CustomerId> Create(int value) => new();
                }
            }
            """),
        new("CSP0714", File(scope: "Domain/Models", type: "DateTimeField"), Model(type: "DateTimeField", members: """
            public System.DateTime CreatedAt { get; }
            """)),
        new("CSP0715", File(scope: "Domain/Entities", type: "AnemicEntity"), Source(ns: "Domain.Entities", type: "AnemicEntity", members: """
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            """)),
        new("CSP0717", File(scope: "Domain/Services", type: "WithBypass"), WithFin(ns: "Domain.Services", type: "WithBypass", declaration: """
            public sealed record Customer {
                private Customer(string name) {
                    Name = name;
                }

                public string Name { get; init; }

                public static LanguageExt.Fin<Customer> Create(string name) => new();
                public static Customer Unsafe() => new("value");
            }

            public sealed class WithBypass {
                public Customer Rewrite(Customer customer) => customer with { Name = "next" };
            }
            """)),
        new("CSP0718", File(scope: "Domain/Services", type: "MutableAccumulator"), Domain(type: "MutableAccumulator", members: """
            public System.Collections.Generic.List<int> Run(int[] values) {
                System.Collections.Generic.List<int> output = new();
                foreach (int value in values) {
                    output.Add(value);
                }
                return output;
            }
            """)),
        new("CSP0719", File(scope: "Domain/Services", type: "UnsafeNumericConversion"), Domain(type: "UnsafeNumericConversion", members: """
            public int Run(long value) => System.Convert.ToInt32(value);
            """)),
        new("CSP0720", File(scope: "Domain/Models", type: "InitBypass"), WithFin(ns: "Domain.Models", type: "InitBypass", declaration: """
            public readonly record struct InitBypass {
                public int Value { get; init; }
                public static LanguageExt.Fin<InitBypass> Create(int value) => new();
            }
            """)),
        new("CSP0901", File(scope: "Integration", type: "InvalidBoundaryExemption"), Boundary(type: "InvalidBoundaryExemption", attributes: """
            [BoundaryImperativeExemption(
                ruleId: "",
                reason: (BoundaryImperativeReason)999,
                ticket: "",
                expiresOnUtc: "not-a-utc-date")]
            """, members: """
            public void Handle() { }
            """)),
        new("CSP0902", File(scope: "Integration", type: "ExpiredBoundaryExemption"), Boundary(type: "ExpiredBoundaryExemption", attributes: """
            [BoundaryImperativeExemption(
                ruleId: "CSP0009",
                reason: BoundaryImperativeReason.ProtocolRequired,
                ticket: "RASM-EXPIRED-001",
                expiresOnUtc: "2000-01-01T00:00:00Z")]
            """, members: """
            public void Handle() { }
            """)),
    ];

    [Theory]
    [MemberData(nameof(RuleCases))]
    public async Task NewlyAddedOrChangedRulesEmitExpectedDiagnosticAsync(string expectedRuleId, string filePath, string source) {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHarness
            .AnalyzeAsync(source: source, filePath: filePath)
            .ConfigureAwait(true);
        ImmutableArray<Diagnostic> matches = [
            .. diagnostics.Where((Diagnostic diagnostic) => StringComparer.Ordinal.Equals(x: diagnostic.Id, y: expectedRuleId)),
        ];
        DiagnosticDescriptor descriptor = AnalyzerTestHarness.SupportedDiagnostics()
            .Single((DiagnosticDescriptor candidate) => StringComparer.Ordinal.Equals(x: candidate.Id, y: expectedRuleId));

        Assert.True(
            condition: !matches.IsEmpty,
            userMessage: $"Expected diagnostic '{expectedRuleId}'. Actual diagnostics:{Environment.NewLine}{DiagnosticText(diagnostics: diagnostics)}");
        Diagnostic match = matches[0];
        Assert.Equal(expected: descriptor.DefaultSeverity, actual: match.Severity);
        Assert.Equal(expected: descriptor.Category, actual: match.Descriptor.Category);
        Assert.NotEqual(expected: Location.None, actual: match.Location);
        Assert.False(condition: string.IsNullOrWhiteSpace(value: match.GetMessage(formatProvider: CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void RuleCasesCoverEveryActiveDiagnostic() {
        ImmutableHashSet<string> activeIds = AnalyzerTestHarness.SupportedDiagnostics()
            .Select(static (DiagnosticDescriptor descriptor) => descriptor.Id)
            .ToImmutableHashSet(StringComparer.Ordinal);
        ImmutableHashSet<string> coveredIds = Cases
            .Select(static (RuleCase row) => row.Id)
            .ToImmutableHashSet(StringComparer.Ordinal);
        ImmutableArray<string> missingIds = [
            .. activeIds
                .Except(coveredIds, StringComparer.Ordinal)
                .OrderBy(static (string id) => id, StringComparer.Ordinal),
        ];
        ImmutableArray<string> staleIds = [
            .. coveredIds
                .Except(activeIds, StringComparer.Ordinal)
                .OrderBy(static (string id) => id, StringComparer.Ordinal),
        ];
        Assert.True(
            condition: missingIds.IsEmpty && staleIds.IsEmpty,
            userMessage: $"Missing behavior cases: {string.Join(", ", missingIds)}. Stale behavior cases: {string.Join(", ", staleIds)}");
    }

    [Fact]
    public async Task BoundaryPropertyExemptionSuppressesAccessorImperativeDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/BoundaryPropertyExemption.cs",
            source: Boundary(type: "BoundaryPropertyExemption", members: """
                private int _value;

                [BoundaryImperativeExemption(
                    ruleId: "CSP0001",
                    reason: BoundaryImperativeReason.ProtocolRequired,
                    ticket: "RASM-BOUNDARY-001",
                    expiresOnUtc: "2999-01-01T00:00:00Z")]
                public int Value {
                    get {
                        if (_value > 0) {
                            return _value;
                        }
                        return 0;
                    }
                }
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0101", collection: ids);
    }

    [Fact]
    public async Task RhinoNamespaceClassifiesAsBoundaryScopeAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Core/Rhino/RhinoBoundary.cs",
            source: Source(ns: "Core.Rhino", type: "RhinoBoundary", members: """
                public int Clamp(int value) {
                    if (value > 0) {
                        return value;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0101", collection: ids);
        Assert.DoesNotContain(expected: "CSP0001", collection: ids);
    }

    [Fact]
    public async Task ValidValidationErrorReturnTypeDoesNotEmitValidationTypeDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Models/ValidValidationFactory.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Validation<TError, TValue> { }
                }

                namespace LanguageExt.Common {
                    public sealed class Error { }
                }

                namespace Domain.Models {
                    public readonly record struct CustomerId {
                        private CustomerId(int value) {
                            Value = value;
                        }

                        public int Value { get; }
                        public static LanguageExt.Validation<LanguageExt.Common.Error, CustomerId> Create(int value) => new();
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0703", collection: ids);
    }

    [Fact]
    public async Task ValidatedPrimitiveIgnoresImplicitStructConstructorAndValueProjectionAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Models/ValidPrimitive.cs",
            source: WithFin(ns: "Domain.Models", type: "Distance", declaration: """
                public readonly record struct Distance {
                    private Distance(double value) {
                        Value = value;
                    }

                    public double Value { get; }
                    public static LanguageExt.Fin<Distance> Create(double value) => new();
                }
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0203", collection: ids);
        Assert.DoesNotContain(expected: "CSP0701", collection: ids);
        Assert.DoesNotContain(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task DomainScopeVarInferenceEmitsOutsideDomainNamespaceAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/DomainScopeVarInference.cs",
            source: Contract(type: "DomainScopeVarInference", attributes: "[DomainScope]", members: """
                public int Execute() {
                    var value = 1;
                    return value;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0015", collection: ids);
    }

    [Fact]
    public async Task ApplicationScopeVarInferenceEmitsOutsideApplicationNamespaceAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/ApplicationScopeVarInference.cs",
            source: Contract(type: "ApplicationScopeVarInference", attributes: "[ApplicationScope]", members: """
                public int Execute() {
                    var value = 1;
                    return value;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0015", collection: ids);
    }

    [Fact]
    public async Task AnalysisLibraryPathDoesNotApplyDomainSignatureRulesAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/libs/csharp/analysis/Analyze.cs",
            source: Source(ns: "Analysis", type: "Analyze", members: """
                public double Execute(double value) => value;
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task NonScrutorScanDoesNotEmitRegistrationStrategyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Application/Bootstrap/UnrelatedScan.cs",
            source: """
                namespace Application.Bootstrap;

                public sealed class CompositionRoot {
                    public ServiceCollection Configure(ServiceCollection services) {
                        _ = services.Scan(selector => selector.FromAssemblies());
                        return services;
                    }
                }

                public sealed class ServiceCollection {
                    public ServiceCollection Scan(System.Action<ITypeSourceSelector> configure) {
                        ITypeSourceSelector builder = new TypeSourceSelector();
                        configure(builder);
                        return this;
                    }
                }

                public interface ITypeSourceSelector {
                    ITypeSourceSelector FromAssemblies();
                }

                public sealed class TypeSourceSelector : ITypeSourceSelector {
                    public ITypeSourceSelector FromAssemblies() => this;
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0406", collection: ids);
    }

    [Fact]
    public async Task InlineBoundedChannelOptionsWithFullModeDoesNotEmitChannelFullModeDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/BoundedChannel.cs",
            source: Domain(type: "BoundedChannel", members: """
                public System.Threading.Channels.Channel<int> Create() =>
                    System.Threading.Channels.Channel.CreateBounded<int>(
                        new System.Threading.Channels.BoundedChannelOptions(1) {
                            FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait,
                        });
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0405", collection: ids);
    }

    public static IEnumerable<object[]> RuleCases() =>
        Cases.Select(static (RuleCase row) => new object[] { row.Id, row.Path, row.Source });

    private static async Task<ImmutableArray<string>> AnalyzeIdsAsync(string filePath, string source) =>
        [
            .. (await AnalyzerTestHarness.AnalyzeAsync(source: source, filePath: filePath).ConfigureAwait(true))
                .Select(static (Diagnostic diagnostic) => diagnostic.Id),
        ];

    private static string DiagnosticText(ImmutableArray<Diagnostic> diagnostics) =>
        string.Join(
            separator: Environment.NewLine,
            values: diagnostics.Select(static (Diagnostic diagnostic) =>
                $"{diagnostic.Id}|{diagnostic.Severity}|{diagnostic.Descriptor.Category}|{diagnostic.Location.GetLineSpan().StartLinePosition}|{diagnostic.GetMessage(formatProvider: CultureInfo.InvariantCulture)}"));

    private static string File(string scope, string type) =>
        $"/workspace/src/{scope}/{type}.cs";

    private static string Domain(string type, string members) =>
        Source(ns: "Domain.Services", type: type, members: members);

    private static string Model(string type, string members) =>
        Source(ns: "Domain.Models", type: type, members: members);

    private static string Source(string ns, string type, string members) =>
        $$"""
        namespace {{ns}};

        public sealed class {{type}} {
        {{members}}
        }
        """;

    private static string Boundary(string type, string members) =>
        Boundary(type: type, attributes: string.Empty, members: members);

    private static string Boundary(string type, string attributes, string members) =>
        BoundaryPrelude(source: $$"""
            namespace Integration;

            [BoundaryAdapter]
            {{attributes}}
            public sealed class {{type}} {
            {{members}}
            }
            """);

    private static string Contract(string type, string attributes, string members) =>
        BoundaryPrelude(source: $$"""
            namespace Integration;

            {{attributes}}
            public sealed class {{type}} {
            {{members}}
            }
            """);

    private static string BoundaryPrelude(string source) =>
        $$"""
        using Foundation.CSharp.Analyzers.Contracts;

        {{source}}
        """;

    private static string WithFin(string ns, string type, string declaration) =>
        $$"""
        namespace LanguageExt {
            public sealed class Fin<T> { }
        }

        namespace {{ns}} {
        {{declaration}}
        }
        """;

    private static string WithEff(string ns, string type, string members) =>
        $$"""
        namespace LanguageExt {
            public sealed class Eff<T> {
                public T Run() => default!;
            }
        }

        namespace {{ns}} {
            public sealed class {{type}} {
        {{members}}
            }
        }
        """;

    private static string WithLanguageExt(string ns, string type, string members) =>
        WithLanguageExt(ns: ns, type: type, attributes: string.Empty, members: members);

    private static string WithLanguageExt(string ns, string type, string attributes, string members) =>
        $$"""
        namespace LanguageExt {
            public sealed class Option<T> { }
            public sealed class Fin<T> {
                public TResult Match<TResult>(System.Func<T, TResult> Succ, System.Func<object, TResult> Fail) => default!;
            }
        }

        namespace {{ns}} {
            {{attributes}}
            public sealed class {{type}} {
        {{members}}
            }
        }
        """;

    private static string ScrutorRoot(string ns) =>
        $$"""
        namespace Scrutor {
            public interface ITypeSourceSelector {
                ITypeSourceSelector FromAssemblies();
                ITypeSourceSelector UsingRegistrationStrategy();
            }
        }

        namespace {{ns}} {
            public sealed class CompositionRoot {
                public ServiceCollection Configure(ServiceCollection services) {
                    _ = services.Scan(selector => selector.FromAssemblies());
                    return services;
                }
            }

            public sealed class ServiceCollection {
                public ServiceCollection Scan(System.Action<Scrutor.ITypeSourceSelector> configure) {
                    Scrutor.ITypeSourceSelector builder = new TypeSourceSelector();
                    configure(builder);
                    return this;
                }
            }

            public sealed class TypeSourceSelector : Scrutor.ITypeSourceSelector {
                public Scrutor.ITypeSourceSelector FromAssemblies() => this;
                public Scrutor.ITypeSourceSelector UsingRegistrationStrategy() => this;
            }
        }
        """;

    private readonly record struct RuleCase(
        string Id,
        string Path,
        string Source);
}
