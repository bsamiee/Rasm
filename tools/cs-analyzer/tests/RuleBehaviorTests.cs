using System.Collections.Immutable;
using Foundation.CSharp.Analyzers.Tests.Infrastructure;
using Xunit;

namespace Foundation.CSharp.Analyzers.Tests;

public sealed class RuleBehaviorTests {
    [Theory]
    [MemberData(nameof(RuleCases))]
    public async Task NewlyAddedOrChangedRulesEmitExpectedDiagnosticAsync(string expectedRuleId, string filePath, string source) {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(source: source, filePath: filePath).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(static id => id, StringComparer.Ordinal),
        ];
        Assert.True(
            condition: diagnosticIds.Contains(expectedRuleId, StringComparer.Ordinal),
            userMessage: $"Expected diagnostic '{expectedRuleId}'. Actual diagnostics: {string.Join(", ", diagnosticIds)}");
    }
    [Fact]
    public async Task BoundaryPropertyExemptionSuppressesAccessorImperativeDiagnosticAsync() {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(
                    filePath: "/workspace/src/Integration/BoundaryPropertyExemption.cs",
                    source: """
                        using Foundation.CSharp.Analyzers.Contracts;

                        namespace Integration;

                        [BoundaryAdapter]
                        public sealed class BoundaryPropertyExemption {
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
                        }
                        """).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id),
        ];
        Assert.DoesNotContain("CSP0101", diagnosticIds);
    }
    [Fact]
    public async Task ValidValidationErrorReturnTypeDoesNotEmitValidationTypeDiagnosticAsync() {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(
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
                        """).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id),
        ];
        Assert.DoesNotContain("CSP0703", diagnosticIds);
    }
    [Fact]
    public async Task DomainScopeVarInferenceEmitsOutsideDomainNamespaceAsync() {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(
                    filePath: "/workspace/src/Integration/DomainScopeVarInference.cs",
                    source: """
                        using Foundation.CSharp.Analyzers.Contracts;

                        namespace Integration;

                        [DomainScope]
                        public sealed class DomainScopeVarInference {
                            public int Execute() {
                                var value = 1;
                                return value;
                            }
                        }
                        """).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id),
        ];
        Assert.Contains("CSP0015", diagnosticIds);
    }
    [Fact]
    public async Task NonScrutorScanDoesNotEmitRegistrationStrategyDiagnosticAsync() {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(
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
                        """).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id),
        ];
        Assert.DoesNotContain("CSP0406", diagnosticIds);
    }
    [Fact]
    public async Task InlineBoundedChannelOptionsWithFullModeDoesNotEmitChannelFullModeDiagnosticAsync() {
        ImmutableArray<string> diagnosticIds = [
            .. (await AnalyzerTestHarness.AnalyzeAsync(
                    filePath: "/workspace/src/Domain/Services/BoundedChannel.cs",
                    source: """
                        namespace Domain.Services;

                        public sealed class BoundedChannel {
                            public System.Threading.Channels.Channel<int> Create() =>
                                System.Threading.Channels.Channel.CreateBounded<int>(
                                    new System.Threading.Channels.BoundedChannelOptions(1) {
                                        FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait,
                                    });
                        }
                        """).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id),
        ];
        Assert.DoesNotContain("CSP0405", diagnosticIds);
    }
    public static IEnumerable<object[]> RuleCases() =>
        [
            Case(
                ruleId: "CSP0017",
                filePath: "/workspace/src/Domain/Performance/HotPathClosure.cs",
                source: """
                    namespace Domain.Performance;

                    public sealed class HotPathClosure {
                        public int Run(int input) {
                            int offset = 3;
                            System.Func<int, int> projector = value => value + offset;
                            return projector(input);
                        }
                    }
                    """),
            Case(
                ruleId: "CSP0101",
                filePath: "/workspace/src/Integration/BoundaryAdapterUsage.cs",
                source: """
                    using Foundation.CSharp.Analyzers.Contracts;

                    namespace Integration;

                    [BoundaryAdapter]
                    public sealed class BoundaryAdapterUsage {
                        public int Clamp(int value) {
                            if (value > 0) {
                                return value;
                            }
                            return 0;
                        }
                    }
                    """),
            Case(
                ruleId: "CSP0009",
                filePath: "/workspace/src/Domain/Services/TryFinallyUsage.cs",
                source: """
                    namespace Domain.Services;

                    public sealed class TryFinallyUsage {
                        public int Compute() {
                            try {
                                return 1;
                            } finally {
                                _ = "cleanup".Length;
                            }
                        }
                    }
                    """),
            Case(
                ruleId: "CSP0901",
                filePath: "/workspace/src/Integration/InvalidBoundaryExemption.cs",
                source: """
                    using Foundation.CSharp.Analyzers.Contracts;

                    namespace Integration;

                    [BoundaryAdapter]
                    [BoundaryImperativeExemption(
                        ruleId: "",
                        reason: (BoundaryImperativeReason)999,
                        ticket: "",
                        expiresOnUtc: "not-a-utc-date")]
                    public sealed class InvalidBoundaryExemption {
                        public void Handle() { }
                    }
                    """),
            Case(
                ruleId: "CSP0902",
                filePath: "/workspace/src/Integration/ExpiredBoundaryExemption.cs",
                source: """
                    using Foundation.CSharp.Analyzers.Contracts;

                    namespace Integration;

                    [BoundaryAdapter]
                    [BoundaryImperativeExemption(
                        ruleId: "CSP0009",
                        reason: BoundaryImperativeReason.ProtocolRequired,
                        ticket: "RASM-EXPIRED-001",
                        expiresOnUtc: "2000-01-01T00:00:00Z")]
                    public sealed class ExpiredBoundaryExemption {
                        public void Handle() { }
                    }
                    """),
            Case(
                ruleId: "CSP0504",
                filePath: "/workspace/src/Integration/DomainScopeAttributeUsage.cs",
                source: """
                    using Foundation.CSharp.Analyzers.Contracts;

                    namespace Integration;

                    [DomainScope]
                    public sealed class DomainScopeAttributeUsage {
                        public void Execute() { }
                    }
                    """),
            Case(
                ruleId: "CSP0406",
                filePath: "/workspace/src/Application/Bootstrap/CompositionRoot.cs",
                source: """
                    namespace Scrutor {
                        public interface ITypeSourceSelector {
                            ITypeSourceSelector FromAssemblies();
                            ITypeSourceSelector UsingRegistrationStrategy();
                        }
                    }

                    namespace Application.Bootstrap {
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
                    """),
            Case(
                ruleId: "CSP0406",
                filePath: "/workspace/src/Infrastructure/DependencyInjection/CompositionRoot.cs",
                source: """
                    namespace Scrutor {
                        public interface ITypeSourceSelector {
                            ITypeSourceSelector FromAssemblies();
                            ITypeSourceSelector UsingRegistrationStrategy();
                        }
                    }

                    namespace Infrastructure.DependencyInjection {
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
                    """),
            Case(
                ruleId: "CSP0003",
                filePath: "/workspace/src/Domain/Services/NestedPrimitiveSignature.cs",
                source: """
                    namespace Domain.Services;

                    public sealed class NestedPrimitiveSignature {
                        public System.Threading.Tasks.Task<string> Read() => System.Threading.Tasks.Task.FromResult("value");
                    }
                    """),
            Case(
                ruleId: "CSP0003",
                filePath: "/workspace/src/SharedKernel/Primitives/SharedPrimitiveSignature.cs",
                source: """
                    namespace SharedKernel.Primitives;

                    public sealed class SharedPrimitiveSignature {
                        public string Name { get; }

                        public SharedPrimitiveSignature(string name) {
                            Name = name;
                        }
                    }
                    """),
            Case(
                ruleId: "CSP0717",
                filePath: "/workspace/src/Domain/Services/WithBypass.cs",
                source: """
                    namespace LanguageExt {
                        public sealed class Fin<T> { }
                    }

                    namespace Domain.Services {
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
                    }
                    """),
            Case(
                ruleId: "CSP0702",
                filePath: "/workspace/src/Domain/Models/SiblingUnion.cs",
                source: """
                    namespace Domain.Models;

                    public abstract record Result;
                    public sealed record Success : Result;
                    """),
            Case(
                ruleId: "CSP0301",
                filePath: "/workspace/src/Domain/Services/DiscardedTask.cs",
                source: """
                    namespace Domain.Services;

                    public sealed class DiscardedTask {
                        public void Run() {
                            _ = SendAsync();
                        }

                        private System.Threading.Tasks.Task SendAsync() => System.Threading.Tasks.Task.CompletedTask;
                    }
                    """),
            Case(
                ruleId: "CSP0006",
                filePath: "/workspace/src/Domain/Services/ValueTaskResult.cs",
                source: """
                    namespace Domain.Services;

                    public sealed class ValueTaskResult {
                        public int Run(System.Threading.Tasks.ValueTask<int> value) => value.Result;
                    }
                    """),
            Case(
                ruleId: "CSP0713",
                filePath: "/workspace/src/Domain/Models/InvalidValidationFactory.cs",
                source: """
                    namespace LanguageExt {
                        public sealed class Validation<TError, TValue> { }
                    }

                    namespace Domain.Models {
                        public readonly record struct CustomerId(int Value) {
                            public static LanguageExt.Validation<string, CustomerId> Create(int value) => new();
                        }
                    }
                    """),
            Case(
                ruleId: "CSP0703",
                filePath: "/workspace/src/Domain/Models/NestedInvalidValidation.cs",
                source: """
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
            Case(
                ruleId: "CSP0504",
                filePath: "/workspace/src/Domain/Services/ReturnPolicy.cs",
                source: """
                    namespace Domain.Services;

                    public sealed class ReturnPolicy {
                        public void Compute() { }
                    }
                    """),
            Case(
                ruleId: "CSP0505",
                filePath: "/workspace/src/Domain/Traits/TypeClassPolicy.cs",
                source: """
                    namespace Domain.Traits;

                    public interface ITypeClassPolicy {
                        int Compute(int value);
                    }
                    """),
            Case(
                ruleId: "CSP0506",
                filePath: "/workspace/src/Domain/Projection/ProjectionPolicy.cs",
                source: """
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
            Case(
                ruleId: "CSP0715",
                filePath: "/workspace/src/Domain/Entities/AnemicEntity.cs",
                source: """
                    namespace Domain.Entities;

                    public sealed class AnemicEntity {
                        public int Id { get; set; }
                        public string Name { get; set; } = string.Empty;
                    }
                    """),
        ];
    private static object[] Case(string ruleId, string filePath, string source) => [ruleId, filePath, source];
}
