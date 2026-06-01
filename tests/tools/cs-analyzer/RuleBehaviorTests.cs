using System.Collections.Immutable;
using System.Globalization;
using Foundation.CSharp.Analyzers.Tests.Infrastructure;
using Microsoft.CodeAnalysis;

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
        new("CSP0104", File(scope: "Domain/Services", type: "NullSentinel"), Domain(type: "NullSentinel", members: """
            public bool Missing(string value) => value == null;
            """)),
        new("CSP0201", File(scope: "Domain/Services", type: "ArraySignature"), Domain(type: "ArraySignature", members: """
            public int[] Read() => [];
            """)),
        new("CSP0202", File(scope: "Domain/Models", type: "MutableField"), Model(type: "MutableField", members: """
            public int Count;
            """)),
        new("CSP0203", File(scope: "Domain/Models", type: "PublicCtorPrimitive"), WithFin(ns: "Domain.Models", declaration: """
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
        new("CSP0717", File(scope: "Domain/Services", type: "WithBypass"), WithFin(ns: "Domain.Services", declaration: """
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
        new("CSP0720", File(scope: "Domain/Models", type: "InitBypass"), WithFin(ns: "Domain.Models", declaration: """
            public readonly record struct InitBypass {
                public int Value { get; init; }
                public static LanguageExt.Fin<InitBypass> Create(int value) => new();
            }
            """)),
        new("CSP0723", File(scope: "Domain/Services", type: "RhinoActiveDocLeak"), """
            namespace Rhino {
                public sealed class RhinoDoc {
                    public static RhinoDoc? ActiveDoc { get; }
                }
                public static class RhinoApp {
                    public static void WriteLine(string text) { }
                }
            }

            namespace Domain.Services {
                public sealed class RhinoActiveDocLeak {
                    public Rhino.RhinoDoc? Read() => Rhino.RhinoDoc.ActiveDoc;
                }
            }
            """),
        new("CSP0724", File(scope: "Domain/Models", type: "FlagsEnumNoComposition"), """
            namespace Domain.Models;

            [System.Flags]
            public enum FlagsEnumNoComposition {
                None = 0,
                Alpha = 1,
                Beta = 2,
            }
            """),
        new("CSP0725", File(scope: "Domain/Services", type: "ImperativeAccumulator"), Domain(type: "ImperativeAccumulator", members: """
            public int Sum(int[] values) {
                int total = 0;
                foreach (int value in values) {
                    total = total + value;
                }
                return total;
            }
            """)),
        new("CSP0726", File(scope: "Domain/Services", type: "PositionalRecordConstructor"), """
            namespace Domain.Services;

            public sealed record Triple(int A, int B, int C);

            public sealed class PositionalRecordConstructor {
                public Triple Build() => new Triple(1, 2, 3);
            }
            """),
        new("CSP0727", File(scope: "Domain/Services", type: "SwitchPrecedenceTrap"), Domain(type: "SwitchPrecedenceTrap", members: """
            public int Quantile(int count, int fraction) =>
                count * fraction switch {
                    int idx => idx,
                };
            """)),
        new("CSP0728", File(scope: "Domain/Services", type: "MapFailDiscardsException"), """
            namespace LanguageExt {
                public sealed class Try<T> {
                    public TryResult<T> Run() => new();
                }
                public sealed class TryResult<T> {
                    public TryResult<T> MapFail(System.Func<object, object> project) => this;
                }
                public static class Try {
                    public static Try<T> lift<T>(System.Func<T> run) => new();
                }
            }

            namespace Domain.Services {
                public sealed class MapFailDiscardsException {
                    public LanguageExt.TryResult<int> Run() =>
                        LanguageExt.Try.lift(run: static () => 1).Run().MapFail(project: static _ => new object());
                }
            }
            """),
        new("CSP0729", File(scope: "Integration", type: "BoundaryOverloadAdjacency"), Boundary(type: "BoundaryOverloadAdjacency", members: """
            public int CommitActions(int value) => value;
            public int Other() => 0;
            public int CommitActions(string value) => value.Length;
            """)),
        new("CSP0730", File(scope: "Domain/Services", type: "OperationalReceipt"), """
            namespace LanguageExt {
                public readonly record struct Seq<T>;
            }

            namespace Domain.Services {
                public readonly record struct OperationalReceipt(int Changed, LanguageExt.Seq<System.Guid> Created, LanguageExt.Seq<System.Guid> Deleted) {
                    public static OperationalReceipt None => new(Changed: 0, Created: new(), Deleted: new());
                    public static OperationalReceipt Count(int changed) => new(Changed: changed, Created: new(), Deleted: new());
                    public static OperationalReceipt CreatedObject(System.Guid id) => new(Changed: 1, Created: new(), Deleted: new());
                }
            }
            """),
        new("CSP0731", File(scope: "Domain/Services", type: "ReceiptChainCollapse"), """
            namespace Domain.Services;

            public readonly record struct OperationReceipt(int Created, int Deleted, int Changed) {
                public static OperationReceipt CreatedOne() => new(Created: 1, Deleted: 0, Changed: 0);
                public static OperationReceipt DeletedOne() => new(Created: 0, Deleted: 1, Changed: 0);
                public static OperationReceipt ChangedOne() => new(Created: 0, Deleted: 0, Changed: 1);
                public static OperationReceipt operator +(OperationReceipt left, OperationReceipt right) =>
                    new(Created: left.Created + right.Created, Deleted: left.Deleted + right.Deleted, Changed: left.Changed + right.Changed);
            }

            public sealed class ReceiptChainCollapse {
                public OperationReceipt Run() =>
                    OperationReceipt.CreatedOne() + OperationReceipt.DeletedOne() + OperationReceipt.ChangedOne();
            }
            """),
        new("CSP0732", File(scope: "Domain/Services", type: "ReceiptConstructionOwner"), """
            namespace Domain.Services;

            public readonly record struct CameraReceipt(int Resources, int Redraw) {
                public static CameraReceipt Empty => new(Resources: 0, Redraw: 0);
            }

            public sealed class ReceiptConstructionOwner {
                public CameraReceipt Rewrite(CameraReceipt receipt) => receipt with { Redraw = 1 };
            }
            """),
        new("CSP0733", File(scope: "Domain/Services", type: "GeneratedCaseAlias"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
            }

            namespace Domain.Services {
                [Thinktecture.Union]
                public abstract record GeneratedCaseAlias {
                    private GeneratedCaseAlias() { }
                    public sealed record AlphaCase(int Value) : GeneratedCaseAlias;
                    public static GeneratedCaseAlias AlphaCase(int value) => new AlphaCase(Value: value);
                }
            }
            """),
        new("CSP0734", File(scope: "Domain/Services", type: "StateDispatchCapture"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
            }

            namespace Domain.Services {
                [Thinktecture.Union]
                public abstract record Choice {
                    public sealed record A(int Value) : Choice;
                    public sealed record B(int Value) : Choice;
                    public sealed record C(int Value) : Choice;
                    public int Switch(System.Func<A, int> a, System.Func<B, int> b, System.Func<C, int> c) => 0;
                }

                public sealed class StateDispatchCapture {
                    public int Run(Choice choice, int offset) =>
                        choice.Switch(
                            a: value => value.Value + offset,
                            b: value => value.Value + offset,
                            c: value => value.Value + offset);
                }
            }
            """),
        new("CSP0735", File(scope: "Domain/Services", type: "TraverseFusion"), """
            namespace LanguageExt {
                public sealed class Seq<T> {
                    public Seq<TOut> Map<TOut>(System.Func<T, TOut> project) => new();
                    public Seq<TOut> Traverse<TOut>(System.Func<T, TOut> project) => new();
                }
            }

            namespace Domain.Services {
                public sealed class TraverseFusion {
                    private static readonly System.Func<int, int> identity = static value => value;
                    public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                        values.Map(static value => value + 1).Traverse(identity);
                }
            }
            """),
        new("CSP0736", File(scope: "Rasm.Rhino/Camera", type: "FoldAppendAccumulator"), """
            namespace LanguageExt {
                public sealed class Seq<T> {
                    public static Seq<T> Empty => new();
                    public Seq<T> Add(T value) => this;
                    public TOut Fold<TOut>(TOut initialState, System.Func<TOut, T, TOut> f) => initialState;
                }
            }

            namespace Rasm.Rhino.Camera {
                public sealed class FoldAppendAccumulator {
                    public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                        values.Fold(LanguageExt.Seq<int>.Empty, static (acc, value) => acc.Add(value));
                }
            }
            """),
        new("CSP0737", File(scope: "Domain/Services", type: "SamePayloadUnion"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
                public sealed class SkipUnionOpsAttribute : System.Attribute { }
            }

            namespace Domain.Services {
                [Thinktecture.Union]
                [Thinktecture.SkipUnionOps]
                public abstract record SamePayloadUnion {
                    private SamePayloadUnion() { }
                    public sealed record Shown(System.Guid Id, int Serial) : SamePayloadUnion;
                    public sealed record Hidden(System.Guid Id, int Serial) : SamePayloadUnion;
                    public sealed record Closed(System.Guid Id, int Serial) : SamePayloadUnion;
                }
            }
            """),
        new("CSP0737", File(scope: "Domain/Services", type: "MixedPayloadUnion"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
                public sealed class SkipUnionOpsAttribute : System.Attribute { }
            }

            namespace Domain.Services {
                [Thinktecture.Union]
                [Thinktecture.SkipUnionOps]
                public abstract record MixedPayloadUnion {
                    private MixedPayloadUnion() { }
                    public sealed record Shown(System.Guid Id, int Serial) : MixedPayloadUnion;
                    public sealed record Hidden(System.Guid Id, int Serial) : MixedPayloadUnion;
                    public sealed record Closed(System.Guid Id, int Serial) : MixedPayloadUnion;
                    public sealed record Renamed(System.Guid Id, string Name) : MixedPayloadUnion;
                }
            }
            """),
        new("CSP0737", File(scope: "Domain/Services", type: "ExplicitPayloadUnion"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
                public sealed class SkipUnionOpsAttribute : System.Attribute { }
            }

            namespace Domain.Services {
                public sealed class PositiveMagnitude { }

                [Thinktecture.Union]
                [Thinktecture.SkipUnionOps]
                public abstract record ExplicitPayloadUnion {
                    private ExplicitPayloadUnion() { }
                    public sealed record Hard : ExplicitPayloadUnion;
                    public sealed record Polynomial : ExplicitPayloadUnion { public Polynomial(PositiveMagnitude k) => K = k; public PositiveMagnitude K { get; } }
                    public sealed record Exponential : ExplicitPayloadUnion { public Exponential(PositiveMagnitude k) => K = k; public PositiveMagnitude K { get; } }
                    public sealed record Cubic : ExplicitPayloadUnion { public Cubic(PositiveMagnitude k) => K = k; public PositiveMagnitude K { get; } }
                    public sealed record Round : ExplicitPayloadUnion { public Round(PositiveMagnitude r) => R = r; public PositiveMagnitude R { get; } }
                }
            }
            """),
        new("CSP0738", File(scope: "Domain/Services", type: "ExclusivePayloadBag"), """
            namespace LanguageExt {
                public readonly record struct Option<T> {
                    public Option<TOut> Map<TOut>(System.Func<T, TOut> project) => new();
                    public static Option<T> operator |(Option<T> left, Option<T> right) => left;
                }
            }

            namespace Domain.Services {
                public enum PayloadPhase { Mouse, Draw, Post, Gumball }
                public sealed class MouseArgs { public object View => new(); }
                public sealed class DrawArgs { public object View => new(); }
                public sealed class PostArgs { public object View => new(); }
                public sealed class Gumball { }

                public readonly record struct ExclusivePayloadBag(
                    PayloadPhase Phase,
                    LanguageExt.Option<MouseArgs> Mouse,
                    LanguageExt.Option<DrawArgs> Draw,
                    LanguageExt.Option<PostArgs> Post,
                    LanguageExt.Option<Gumball> Gumball) {
                    public LanguageExt.Option<object> View =>
                        Mouse.Map(static args => args.View) | Draw.Map(static args => args.View) | Post.Map(static args => args.View);
                }
            }
            """),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinUnitConditional"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinUnitConditional", members: """
            public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, Error error) =>
                accepted ? LanguageExt.Fin.Succ(unit) : LanguageExt.Fin.Fail<LanguageExt.Unit>(error);
            """)),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinUnitConditionalReversed"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinUnitConditionalReversed", members: """
            public LanguageExt.Fin<LanguageExt.Unit> Run(bool rejected, Error error) =>
                rejected ? LanguageExt.Fin.Fail<LanguageExt.Unit>(error) : LanguageExt.Fin.Succ(unit);
            """)),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinValueConditional"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinValueConditional", members: """
            public LanguageExt.Fin<int> Run(bool accepted, int value, Error error) =>
                accepted ? LanguageExt.Fin.Succ(value) : LanguageExt.Fin.Fail<int>(error);
            """)),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinConstructedValueConditional"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinConstructedValueConditional", members: """
            public readonly record struct Measure(double Value);

            public LanguageExt.Fin<Measure> Run(bool accepted, double value) =>
                accepted ? LanguageExt.Fin.Succ(new Measure(value)) : LanguageExt.Fin.Fail<Measure>(new Error($"bad {value}"));
            """)),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinValueConditionalReversed"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinValueConditionalReversed", members: """
            public LanguageExt.Fin<int> Run(bool rejected, int value, Error error) =>
                rejected ? LanguageExt.Fin.Fail<int>(error) : LanguageExt.Fin.Succ(value);
            """)),
        new("CSP0739", File(scope: "Domain/Services", type: "GuardableFinSwitchExpression"), WithFinUnit(ns: "Domain.Services", type: "GuardableFinSwitchExpression", members: """
            public LanguageExt.Fin<int> Run(bool accepted, int value, Error error) =>
                accepted switch {
                    true => LanguageExt.Fin.Succ(value),
                    false => LanguageExt.Fin.Fail<int>(error),
                };
            """)),
        new("CSP0740", File(scope: "Domain/Services", type: "ManualVisitorUnion"), """
            namespace Domain.Services {
                public abstract record ManualVisitorUnion {
                    private ManualVisitorUnion() { }
                    public abstract TResult Fold<TResult>(
                        System.Func<Created, TResult> created,
                        System.Func<Changed, TResult> changed,
                        System.Func<Deleted, TResult> deleted);

                    public sealed record Created(int Id) : ManualVisitorUnion {
                        public override TResult Fold<TResult>(
                            System.Func<Created, TResult> created,
                            System.Func<Changed, TResult> changed,
                            System.Func<Deleted, TResult> deleted) => created(this);
                    }

                    public sealed record Changed(int Id) : ManualVisitorUnion {
                        public override TResult Fold<TResult>(
                            System.Func<Created, TResult> created,
                            System.Func<Changed, TResult> changed,
                            System.Func<Deleted, TResult> deleted) => changed(this);
                    }

                    public sealed record Deleted(int Id) : ManualVisitorUnion {
                        public override TResult Fold<TResult>(
                            System.Func<Created, TResult> created,
                            System.Func<Changed, TResult> changed,
                            System.Func<Deleted, TResult> deleted) => deleted(this);
                    }
                }
            }
            """),
        new("CSP0740", File(scope: "Domain/Services", type: "InternalClosedOverrideRail"), """
            namespace Domain.Services {
                internal abstract record InternalClosedOverrideRail {
                    internal abstract int Run(int value);
                    internal sealed record First : InternalClosedOverrideRail { internal override int Run(int value) => value; }
                    internal sealed record Second : InternalClosedOverrideRail { internal override int Run(int value) => value; }
                    internal sealed record Third : InternalClosedOverrideRail { internal override int Run(int value) => value; }
                }
            }
            """),
        new("CSP0741", File(scope: "Domain/Services", type: "ForwardingRequestFamily"), """
            namespace LanguageExt {
                public sealed class Fin<T> { }
            }

            namespace Domain.Services {
                public sealed class Scope { }
                public sealed class Intent<T> {
                    public LanguageExt.Fin<T> Run(Scope scope) => new();
                }
                public static class Intent {
                    public static Intent<T> Select<T>(string name) => new();
                    public static Intent<T> Pick<T>(string name) => new();
                    public static Intent<T> Choose<T>(string name) => new();
                }

                public abstract record ForwardingRequestFamily<T> {
                    private ForwardingRequestFamily() { }
                    public abstract LanguageExt.Fin<T> Apply(Scope scope);
                    public sealed record Select(string Name) : ForwardingRequestFamily<T> {
                        public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Select<T>(Name).Run(scope: scope);
                    }
                    public sealed record Pick(string Name) : ForwardingRequestFamily<T> {
                        public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Pick<T>(Name).Run(scope: scope);
                    }
                    public sealed record Choose(string Name) : ForwardingRequestFamily<T> {
                        public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Choose<T>(Name).Run(scope: scope);
                    }
                }
            }
            """),
        new("CSP0742", File(scope: "Domain/Services", type: "ManualOpAdmissionGate"), """
            namespace LanguageExt {
                public sealed class Fin<T> { }
                public static class Fin {
                    public static Fin<T> Succ<T>(T value) => new();
                    public static Fin<T> Fail<T>(object error) => new();
                }
            }
            namespace Rhino.Geometry {
                public readonly record struct Plane(bool IsValid);
            }

            namespace Domain.Services {
                public sealed class Op {
                    public object InvalidResult() => new();
                }
                public sealed class ManualOpAdmissionGate {
                    public LanguageExt.Fin<Rhino.Geometry.Plane> Run(Rhino.Geometry.Plane plane, Op op) =>
                        plane.IsValid
                            ? LanguageExt.Fin.Succ(value: plane)
                            : LanguageExt.Fin.Fail<Rhino.Geometry.Plane>(error: op.InvalidResult());
                }
            }
            """),
        new("CSP0743", File(scope: "Domain/Services", type: "ManualGenericProjectionGate"), """
            namespace LanguageExt {
                public sealed class Fin<T> { }
                public static class Fin {
                    public static Fin<T> Succ<T>(T value) => new();
                    public static Fin<T> Fail<T>(LanguageExt.Common.Error error) => new();
                }
            }

            namespace LanguageExt.Common {
                public class Error { }
            }

            namespace Domain.Services {
                public sealed class Op {
                    public LanguageExt.Common.Error Unsupported(System.Type geometryType, System.Type outputType) => new();
                }
                public sealed class ManualGenericProjectionGate {
                    public LanguageExt.Fin<TOut> Project<TOut>(Item value, Op op) =>
                        typeof(TOut) == typeof(Item)
                            ? LanguageExt.Fin.Succ((TOut)(object)value)
                            : LanguageExt.Fin.Fail<TOut>(op.Unsupported(typeof(Item), typeof(TOut)));
                }
                public readonly record struct Item(int Value);
            }
            """),
        new("CSP0802", File(scope: "Domain/Models", type: "UnqualifiedUnionOps"), """
            namespace Thinktecture {
                public sealed class UnionAttribute : System.Attribute { }
            }

            namespace Domain.Models {
                [Thinktecture.Union]
                public abstract record UnqualifiedUnionOps {
                    private UnqualifiedUnionOps() { }
                    public sealed record Value : UnqualifiedUnionOps;
                }
            }
            """),
    ];

    [Theory]
    [MemberData(nameof(RuleCases))]
    public async Task NewlyAddedOrChangedRulesEmitExpectedDiagnosticAsync(string expectedRuleId, string filePath, string source) {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHarness
            .AnalyzeAsync(source: source, filePath: filePath)
            .ConfigureAwait(true);
        ImmutableArray<Diagnostic> matches = [
            .. diagnostics.Where(diagnostic => StringComparer.Ordinal.Equals(x: diagnostic.Id, y: expectedRuleId)),
        ];
        DiagnosticDescriptor descriptor = AnalyzerTestHarness.SupportedDiagnostics()
            .Single(candidate => StringComparer.Ordinal.Equals(x: candidate.Id, y: expectedRuleId));

        Assert.False(
            condition: matches.IsEmpty,
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
            .Select(static descriptor => descriptor.Id)
            .ToImmutableHashSet(StringComparer.Ordinal);
        ImmutableHashSet<string> coveredIds = Cases
            .Select(static row => row.Id)
            .ToImmutableHashSet(StringComparer.Ordinal);
        ImmutableArray<string> missingIds = [
            .. activeIds
                .Except(coveredIds, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal),
        ];
        ImmutableArray<string> staleIds = [
            .. coveredIds
                .Except(activeIds, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal),
        ];
        Assert.True(
            condition: missingIds.IsEmpty && staleIds.IsEmpty,
            userMessage: $"Missing behavior cases: {string.Join(", ", missingIds)}. Stale behavior cases: {string.Join(", ", staleIds)}");
    }

    [Fact]
    public async Task BoundaryAdapterAccessingRhinoActiveDocDoesNotEmitRhinoActiveDocLeakDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/RhinoActiveDocBoundary.cs",
            source: """
                using Foundation.CSharp.Analyzers.Contracts;

                namespace Rhino {
                    public sealed class RhinoDoc {
                        public static RhinoDoc? ActiveDoc { get; }
                    }
                }

                namespace Integration {
                    [BoundaryAdapter]
                    public sealed class RhinoActiveDocBoundary {
                        public Rhino.RhinoDoc? Read() => Rhino.RhinoDoc.ActiveDoc;
                    }
                }
                """).ConfigureAwait(true);

        Assert.Empty(collection: ids);
    }
    [Fact]
    public async Task LoopWithoutOuterScopeAccumulatorDoesNotEmitImperativeAccumulatorDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/LoopWithLocalAccumulator.cs",
            source: Boundary(type: "LoopWithLocalAccumulator", attributes: string.Empty, members: """
                public int LoopOnly(int[] values) {
                    foreach (int value in values) {
                        int local = value + 1;
                        _ = local;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Empty(collection: ids);
    }
    [Fact]
    public async Task FlagsEnumWithBitwiseCompositionDoesNotEmitFlagsEnumOveruseDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Models/FlagsEnumWithComposition.cs",
            source: """
                namespace Domain.Models;

                [System.Flags]
                public enum FlagsEnumWithComposition {
                    None = 0,
                    Alpha = 1,
                    Beta = 2,
                    Both = Alpha | Beta,
                }

                public sealed class FlagsEnumWithCompositionConsumer {
                    public FlagsEnumWithComposition Combine(FlagsEnumWithComposition lhs, FlagsEnumWithComposition rhs) =>
                        lhs | rhs;
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0724", collection: ids);
    }
    [Fact]
    public async Task InputShapePolymorphicOverloadsDoNotEmitOverloadSpamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/InputShapePolymorphism.cs",
            source: Domain(type: "InputShapePolymorphism", members: """
                public int Of(double absolute, double relative, double angle, int units) => 0;
                public int Of(int units) => 0;
                public int Of(string document) => 0;
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0005", collection: ids);
    }
    [Fact]
    public async Task ArityLadderStillEmitsOverloadSpamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ArityLadderSpam.cs",
            source: Domain(type: "ArityLadderSpam", members: """
                public int Compute(int first) => first;
                public int Compute(int first, int second) => first + second;
                public int Compute(int first, int second, int third) => first + second + third;
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0005", collection: ids);
    }
    [Fact]
    public async Task UnionDispatchingMethodPairDoesNotEmitOverloadSpamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/UnionDispatchingPair.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                }

                namespace Rasm.Domain {
                    [Thinktecture.Union]
                    public abstract record GeometryShape<TA, TB> {
                        public sealed record One(TA Value) : GeometryShape<TA, TB>;
                        public sealed record Pair(TA First, TB Second) : GeometryShape<TA, TB>;
                    }
                }

                namespace Domain.Services {
                    public sealed class UnionDispatchingPair {
                        public int Validate<T>(T? value) where T : class => 0;
                        public int Validate<TA, TB>(Rasm.Domain.GeometryShape<TA, TB> shape) => 0;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0005", collection: ids);
    }
    [Fact]
    public async Task AdjacentOverloadsDoNotEmitOverloadAdjacencyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/AdjacentOverloads.cs",
            source: Boundary(type: "AdjacentOverloads", members: """
                public int CommitActions(int value) => value;
                public int CommitActions(string value) => value.Length;
                public int Other() => 0;
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0729", collection: ids);
    }
    [Fact]
    public async Task AccessSeparatedOverloadsDoNotEmitOverloadAdjacencyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/AccessSeparatedOverloads.cs",
            source: Boundary(type: "AccessSeparatedOverloads", members: """
                private int CommitActions(int value) => value;
                public int Other() => 0;
                public int CommitActions(string value) => value.Length;
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0729", collection: ids);
    }
    [Fact]
    public async Task InterfaceGroupedOverloadsDoNotEmitOverloadAdjacencyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/InterfaceGroupedOverloads.cs",
            source: """
                namespace Foundation.CSharp.Analyzers.Contracts {
                    [System.AttributeUsage(System.AttributeTargets.All)]
                    public sealed class BoundaryAdapterAttribute : System.Attribute { }
                }

                namespace Integration {
                    public interface IEquality<T> {
                        public bool Equals(T x, T y);
                        public int GetHashCode(T value);
                    }

                    [Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]
                    public sealed class InterfaceGroupedOverloads : IEquality<int>, IEquality<string> {
                        public bool Equals(int x, int y) => x == y;
                        public int GetHashCode(int value) => value.GetHashCode();
                        public bool Equals(string x, string y) => string.Equals(x, y, System.StringComparison.Ordinal);
                        public int GetHashCode(string value) => value.GetHashCode();
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0729", collection: ids);
    }
    [Fact]
    public async Task BoundaryPropertyAccessorImperativeControlFlowIsStructurallyAllowedAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/BoundaryPropertyAccess.cs",
            source: Boundary(type: "BoundaryPropertyAccess", members: """
                private int _value;

                public int Value {
                    get {
                        if (_value > 0) {
                            return _value;
                        }
                        return 0;
                    }
                }
                """)).ConfigureAwait(true);

        Assert.Empty(collection: ids);
    }

    [Fact]
    public async Task BoundaryTerminalMatchCallsAreStructurallyAllowedAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/BoundaryTerminalMatch.cs",
            source: WithLanguageExt(ns: "Integration", type: "BoundaryTerminalMatch", attributes: "[Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]", members: """
                public void RunExpression(LanguageExt.Fin<int> value) {
                    value.Match(Succ: static _ => LanguageExt.Unit.Default, Fail: static _ => LanguageExt.Unit.Default);
                }

                public void RunDiscard(LanguageExt.Fin<int> value) {
                    _ = value.Match(Succ: static _ => LanguageExt.Unit.Default, Fail: static _ => LanguageExt.Unit.Default);
                }
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0705", collection: ids);
    }

    [Fact]
    public async Task BoundaryTerminalPureMatchStillReportsBoundaryDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/BoundaryTerminalPureMatch.cs",
            source: WithLanguageExt(ns: "Integration", type: "BoundaryTerminalPureMatch", attributes: "[Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]", members: """
                public void Run(LanguageExt.Fin<int> value) {
                    _ = value.Match(Succ: static input => input, Fail: static _ => 0);
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0705", collection: ids);
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

        Assert.Empty(collection: ids);
    }

    [Fact]
    public async Task GrasshopperLibraryPathClassifiesAsBoundaryScopeAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/libs/csharp/Rasm.Grasshopper/Components/Bridge.cs",
            source: WithLanguageExt(ns: "Rasm.Grasshopper.Components", type: "Bridge", members: """
                public int Run(LanguageExt.Fin<int> value) {
                    if (value is null) {
                        return 0;
                    }
                    int result = value.Match(Succ: static input => input, Fail: static _ => 0);
                    return result;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0705", collection: ids);
        Assert.DoesNotContain(expected: "CSP0001", collection: ids);
        Assert.DoesNotContain(expected: "CSP0002", collection: ids);
    }

    [Fact]
    public async Task GrasshopperAppPathClassifiesAsBoundaryScopeAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/apps/grasshopper/Example/Components/ExtractPoints.cs",
            source: Source(ns: "Example.Components", type: "ExtractPoints", members: """
                public int Run(int value) {
                    if (value > 0) {
                        return value;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Empty(collection: ids);
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
            source: WithFin(ns: "Domain.Models", declaration: """
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
    public async Task AnalysisLibraryPathAppliesFunctionalRulesAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/libs/csharp/Rasm/Analysis/Analyze.cs",
            source: Source(ns: "Rasm.Analysis", type: "Analyze", members: """
                public int Execute(int value) {
                    if (value > 0) {
                        return value;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0001", collection: ids);
        Assert.DoesNotContain(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task RasmAnalysisNamespaceOutsideCanonicalPathAppliesFunctionalRulesAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Features/Evaluate.cs",
            source: Source(ns: "Rasm.Analysis.Spatial", type: "Evaluate", members: """
                public int Execute(int value) {
                    if (value > 0) {
                        return value;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0001", collection: ids);
        Assert.DoesNotContain(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task AnalysisOperationSurfaceAppliesFunctionalRulesAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/libs/csharp/Rasm/Analysis/Analyze.cs",
            source: Source(ns: "Rasm.Analysis", type: "Operation", members: """
                public double Execute(double value) {
                    if (value > 0) {
                        return value;
                    }
                    return 0;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0001", collection: ids);
        Assert.DoesNotContain(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task AnalysisOperationSurfaceExemptionIsRasmOnlyAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/Operation.cs",
            source: BoundaryPrelude(source: """
                namespace Other.Analysis;

                [DomainScope]
                public sealed class Operation {
                    public double Execute(double value) => value;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0003", collection: ids);
    }

    [Fact]
    public async Task BoundaryAdapterInsideAnalysisScopeKeepsBoundaryStrictnessOnlyAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/libs/csharp/Rasm/Analysis/RhinoBoundary.cs",
            source: WithLanguageExt(ns: "Rasm.Analysis", type: "RhinoBoundary", attributes: "[Foundation.CSharp.Analyzers.Contracts.BoundaryAdapter]", members: """
                public int Run(LanguageExt.Fin<int> value) {
                    if (value is null) {
                        return 0;
                    }
                    int result = value.Match(Succ: static input => input, Fail: static _ => 0);
                    return result;
                }
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0705", collection: ids);
        Assert.DoesNotContain(expected: "CSP0001", collection: ids);
        Assert.DoesNotContain(expected: "CSP0002", collection: ids);
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
    public async Task UnionCaseFactoryReturningContainingTypeDoesNotEmitExtensionProjectionDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Projection/UnionFactory.cs",
            source: """
                namespace Domain.Projection;

                public sealed class ScalarMetric { }

                public abstract record CurvatureMode {
                    public sealed record ScalarCase(ScalarMetric Metric) : CurvatureMode;
                    public static CurvatureMode Scalar(ScalarMetric metric) => new ScalarCase(Metric: metric);
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0506", collection: ids);
    }
    [Fact]
    public async Task ParenthesizedSwitchInputDoesNotEmitSwitchPrecedenceDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ParenthesizedSwitchInput.cs",
            source: Domain(type: "ParenthesizedSwitchInput", members: """
                public int Quantile(int count, int fraction) =>
                    (count * fraction) switch {
                        int idx => idx,
                    };
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0727", collection: ids);
    }
    [Fact]
    public async Task ParenthesizedSwitchResultDoesNotEmitSwitchPrecedenceDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ParenthesizedSwitchResult.cs",
            source: Domain(type: "ParenthesizedSwitchResult", members: """
                public int Scale(int count, int fraction) =>
                    count * (fraction switch {
                        int x => x,
                    });
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0727", collection: ids);
    }
    [Fact]
    public async Task SwitchAsLeftOperandDoesNotEmitSwitchPrecedenceDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SwitchAsLeftOperand.cs",
            source: Domain(type: "SwitchAsLeftOperand", members: """
                public int Shift(int value) =>
                    (value switch {
                        int x => x,
                    }) + 1;
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0727", collection: ids);
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

    [Fact]
    public async Task ProofReceiptDoesNotEmitOperationalReceiptFactStreamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SpectralReceipt.cs",
            source: """
                namespace Domain.Services;

                public enum SpectralStatus {
                    Complete,
                }

                public readonly record struct SpectralReceipt(SpectralStatus Status, double Tolerance, int Iterations);
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0730", collection: ids);
    }

    [Fact]
    public async Task FactStreamReceiptDoesNotEmitOperationalReceiptFactStreamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/DocumentReceipt.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Seq<T>;
                }

                namespace Domain.Services {
                    public readonly record struct DocumentReceipt {
                        private readonly LanguageExt.Seq<Change> changes;
                        private DocumentReceipt(LanguageExt.Seq<Change> changes) {
                            this.changes = changes;
                        }

                        public LanguageExt.Seq<System.Guid> Created => new();
                        public LanguageExt.Seq<System.Guid> Deleted => new();
                        public static DocumentReceipt Empty => new(changes: new());
                        public static DocumentReceipt Objects(LanguageExt.Seq<System.Guid> created) => new(changes: new());

                        private abstract record Change;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0730", collection: ids);
    }

    [Fact]
    public async Task CompactTwoBucketReceiptDoesNotEmitOperationalReceiptFactStreamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SmallReceipt.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Seq<T>;
                }

                namespace Domain.Services {
                    public readonly record struct SmallReceipt(int Changed, LanguageExt.Seq<System.Guid> Created) {
                        public static SmallReceipt None => new(Changed: 0, Created: new());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0730", collection: ids);
    }

    [Fact]
    public async Task NamespaceDoesNotExemptOperationalReceiptFactStreamDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Rasm/Vectors/OperationalReceipt.cs",
            source: """
                namespace Foundation.CSharp.Analyzers.Contracts {
                    public sealed class DomainScopeAttribute : System.Attribute { }
                }

                namespace LanguageExt {
                    public readonly record struct Seq<T>;
                }

                namespace Rasm.Vectors {
                    [Foundation.CSharp.Analyzers.Contracts.DomainScope]
                    public readonly record struct OperationalReceipt(int Changed, LanguageExt.Seq<System.Guid> Created, LanguageExt.Seq<System.Guid> Deleted);
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0730", collection: ids);
    }

    [Fact]
    public async Task TwoTermReceiptChainDoesNotEmitReceiptChainCollapseDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/TwoTermReceipt.cs",
            source: """
                namespace Domain.Services;

                public readonly record struct OperationReceipt(int Created, int Deleted) {
                    public static OperationReceipt CreatedOne() => new(Created: 1, Deleted: 0);
                    public static OperationReceipt DeletedOne() => new(Created: 0, Deleted: 1);
                    public static OperationReceipt operator +(OperationReceipt left, OperationReceipt right) =>
                        new(Created: left.Created + right.Created, Deleted: left.Deleted + right.Deleted);
                }

                public sealed class TwoTermReceipt {
                    public OperationReceipt Run() => OperationReceipt.CreatedOne() + OperationReceipt.DeletedOne();
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0731", collection: ids);
    }

    [Fact]
    public async Task ReceiptOwnerConstructionDoesNotEmitReceiptConstructionOwnerDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ReceiptOwnerConstruction.cs",
            source: """
                namespace Domain.Services;

                public readonly record struct CameraReceipt(int Resources, int Redraw) {
                    public static CameraReceipt Empty => new(Resources: 0, Redraw: 0);
                    public static CameraReceipt RedrawOnly(int redraw) => Empty with { Redraw = redraw };
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0732", collection: ids);
    }

    [Fact]
    public async Task StaticStateDispatchDoesNotEmitStateThreadedDispatchDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/StaticStateDispatch.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    public abstract record Choice {
                        public sealed record A(int Value) : Choice;
                        public sealed record B(int Value) : Choice;
                        public sealed record C(int Value) : Choice;
                        public int Switch(System.Func<A, int> a, System.Func<B, int> b, System.Func<C, int> c) => 0;
                    }

                    public sealed class StaticStateDispatch {
                        public int Run(Choice choice) =>
                            choice.Switch(
                                a: static value => value.Value,
                                b: static value => value.Value,
                                c: static value => value.Value);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0734", collection: ids);
    }

    [Fact]
    public async Task DistinctPayloadUnionDoesNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/DistinctPayloadUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record DistinctPayloadUnion {
                        private DistinctPayloadUnion() { }
                        public sealed record Text(string Value) : DistinctPayloadUnion;
                        public sealed record Icon(string Name, string Assembly) : DistinctPayloadUnion;
                        public sealed record Native(int Handle) : DistinctPayloadUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task EmptyUnionCasesDoNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/EmptyCaseUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record EmptyCaseUnion {
                        private EmptyCaseUnion() { }
                        public sealed record Created : EmptyCaseUnion;
                        public sealed record Updated : EmptyCaseUnion;
                        public sealed record Deleted : EmptyCaseUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task BehaviorOnlyUnionCasesDoNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/BehaviorOnlyFault.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record BehaviorOnlyFault {
                        private BehaviorOnlyFault() { }
                        public sealed record Missing : BehaviorOnlyFault { public string Message => "missing"; public string Category => "Input"; }
                        public sealed record Cancelled : BehaviorOnlyFault { public string Message => "cancelled"; public string Category => "Flow"; }
                        public sealed record Invalid : BehaviorOnlyFault { public string Message => "invalid"; public string Category => "Input"; }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task ErrorUnionCasesDoNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/UiFault.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    public abstract record Error;
                    public abstract record Expected : Error;

                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record UiFault : Expected {
                        private UiFault() { }
                        public sealed record InvalidInput(string Op, string Detail) : UiFault;
                        public sealed record MutationRejected(string Op, string Detail) : UiFault;
                        public sealed record EditorFailure(string Op, string Detail) : UiFault;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task GenericControlFlowUnionDoesNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PromptTransition.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record PromptTransition<TState> {
                        private PromptTransition() { }
                        public sealed record Stay(TState State) : PromptTransition<TState>;
                        public sealed record Forward(TState State) : PromptTransition<TState>;
                        public sealed record Commit(TState State) : PromptTransition<TState>;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task TwoCaseSamePayloadUnionDoesNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/TwoCasePayloadUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record TwoCasePayloadUnion {
                        private TwoCasePayloadUnion() { }
                        public sealed record First(System.Guid Id, int Serial) : TwoCasePayloadUnion;
                        public sealed record Second(System.Guid Id, int Serial) : TwoCasePayloadUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task SameTypesWithDifferentSemanticNamesDoNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SemanticPayloadUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record SemanticPayloadUnion {
                        private SemanticPayloadUnion() { }
                        public sealed record Radius(double Radius) : SemanticPayloadUnion;
                        public sealed record Thickness(double Thickness) : SemanticPayloadUnion;
                        public sealed record Tolerance(double Tolerance) : SemanticPayloadUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task SamePayloadNameWithDifferentTypesDoesNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/NativeEventPayloadUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    public sealed class MouseArgs { }
                    public sealed class DrawArgs { }
                    public sealed class PostArgs { }

                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record NativeEventPayloadUnion {
                        private NativeEventPayloadUnion() { }
                        public sealed record Mouse(MouseArgs Value) : NativeEventPayloadUnion;
                        public sealed record Draw(DrawArgs Value) : NativeEventPayloadUnion;
                        public sealed record Post(PostArgs Value) : NativeEventPayloadUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task SamePayloadCasesWithOwnedBehaviorDoNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/NativeResourceUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record NativeResourceUnion {
                        private NativeResourceUnion() { }
                        public sealed record Register(int Handle) : NativeResourceUnion { public string Route => "register"; }
                        public sealed record Change(int Handle) : NativeResourceUnion { public string Route => "change"; }
                        public sealed record Close(int Handle) : NativeResourceUnion { public string Route => "close"; }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task TwoCaseExplicitPayloadClusterDoesNotEmitSamePayloadUnionCasesDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ExplicitTwoCasePayloadUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                    public sealed class SkipUnionOpsAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    public sealed class PositiveMagnitude { }

                    [Thinktecture.Union]
                    [Thinktecture.SkipUnionOps]
                    public abstract record ExplicitTwoCasePayloadUnion {
                        private ExplicitTwoCasePayloadUnion() { }
                        public sealed record Polynomial : ExplicitTwoCasePayloadUnion { public Polynomial(PositiveMagnitude k) => K = k; public PositiveMagnitude K { get; } }
                        public sealed record Exponential : ExplicitTwoCasePayloadUnion { public Exponential(PositiveMagnitude k) => K = k; public PositiveMagnitude K { get; } }
                        public sealed record Round : ExplicitTwoCasePayloadUnion { public Round(PositiveMagnitude r) => R = r; public PositiveMagnitude R { get; } }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0737", collection: ids);
    }

    [Fact]
    public async Task PrimitiveOptionsDoNotEmitExclusiveOptionalPayloadBagDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/MetadataOptions.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Option<T> { }
                }

                namespace Domain.Services {
                    public readonly record struct MetadataOptions(
                        LanguageExt.Option<string> Name,
                        LanguageExt.Option<int> Count,
                        LanguageExt.Option<System.Guid> Id);
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0738", collection: ids);
    }

    [Fact]
    public async Task IndependentComplexOptionsDoNotEmitExclusiveOptionalPayloadBagDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/FilterOptions.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Option<T> { }
                }

                namespace Domain.Services {
                    public sealed class Curve { }
                    public sealed class Surface { }
                    public sealed class Mesh { }

                    public readonly record struct FilterOptions(
                        LanguageExt.Option<Curve> Curve,
                        LanguageExt.Option<Surface> Surface,
                        LanguageExt.Option<Mesh> Mesh);
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0738", collection: ids);
    }

    [Fact]
    public async Task AdditivePolicyStateDoesNotEmitExclusiveOptionalPayloadBagDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PolicyState.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Option<T> {
                        public Option<TOut> Map<TOut>(System.Func<T, TOut> project) => new();
                        public static Option<T> operator |(Option<T> left, Option<T> right) => left;
                    }
                }

                namespace Domain.Services {
                    public sealed class PointSpec { }
                    public sealed class LimitSpec { }
                    public sealed class BoxSpec { }
                    public sealed class SelectionSpec { }

                    public sealed record PolicyState(
                        LanguageExt.Option<PointSpec> Point,
                        LanguageExt.Option<LimitSpec> Limits,
                        LanguageExt.Option<BoxSpec> Box,
                        LanguageExt.Option<SelectionSpec> Selection) {
                        public static PolicyState Add(PolicyState left, PolicyState right) =>
                            new(
                                Point: left.Point | right.Point,
                                Limits: left.Limits | right.Limits,
                                Box: left.Box | right.Box,
                        Selection: left.Selection | right.Selection);

                        public LanguageExt.Option<object> Any =>
                            Point.Map(static value => (object)value)
                            | Limits.Map(static value => (object)value)
                            | Box.Map(static value => (object)value);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0738", collection: ids);
    }

    [Fact]
    public async Task ConjunctiveQueryBagDoesNotEmitExclusiveOptionalPayloadBagDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SheetQuery.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Option<T> {
                        public Option<TOut> Map<TOut>(System.Func<T, TOut> project) => new();
                        public static Option<T> operator |(Option<T> left, Option<T> right) => left;
                    }
                }

                namespace Domain.Services {
                    public sealed class LayerCriterion { }
                    public sealed class ViewCriterion { }
                    public sealed class ObjectCriterion { }

                    public readonly record struct SheetQuery(
                        LanguageExt.Option<LayerCriterion> Layer,
                        LanguageExt.Option<ViewCriterion> View,
                        LanguageExt.Option<ObjectCriterion> Object) {
                        public LanguageExt.Option<object> First =>
                            Layer.Map(static value => (object)value)
                            | View.Map(static value => (object)value)
                            | Object.Map(static value => (object)value);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0738", collection: ids);
    }

    [Fact]
    public async Task FallbackContextViewDoesNotEmitExclusiveOptionalPayloadBagDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PageEvent.cs",
            source: """
                namespace LanguageExt {
                    public readonly record struct Option<T> {
                        public Option<TOut> Map<TOut>(System.Func<T, TOut> project) => new();
                        public static Option<T> operator |(Option<T> left, Option<T> right) => left;
                    }
                }

                namespace Domain.Services {
                    public sealed class MouseContext { public object View => new(); }
                    public sealed class ScriptContext { public object View => new(); }
                    public sealed class DocumentContext { public object View => new(); }

                    public readonly record struct PageEvent(
                        LanguageExt.Option<MouseContext> Mouse,
                        LanguageExt.Option<ScriptContext> Script,
                        LanguageExt.Option<DocumentContext> Document) {
                        public LanguageExt.Option<object> View =>
                            Mouse.Map(static value => value.View)
                            | Script.Map(static value => value.View)
                            | Document.Map(static value => value.View);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0738", collection: ids);
    }

    [Fact]
    public async Task DirectTraverseDoesNotEmitTraverseFusionDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/DirectTraverse.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Seq<T> {
                        public Seq<TOut> Traverse<TOut>(System.Func<T, TOut> project) => new();
                    }
                }

                namespace Domain.Services {
                    public sealed class DirectTraverse {
                        public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                            values.Traverse(static value => value + 1);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0735", collection: ids);
    }

    [Fact]
    public async Task IndexedMapTraversalDoesNotEmitTraverseFusionDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/IndexedTraverse.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Seq<T> {
                        public Seq<TOut> Map<TOut>(System.Func<T, int, TOut> project) => new();
                        public Seq<TOut> Traverse<TOut>(System.Func<T, TOut> project) => new();
                    }
                }

                namespace Domain.Services {
                    public sealed class IndexedTraverse {
                        private static readonly System.Func<int, int> identity = static value => value;
                        public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                            values.Map(static (value, index) => value + index).Traverse(identity);
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0735", collection: ids);
    }

    [Fact]
    public async Task FoldConsAccumulatorDoesNotEmitFoldAppendAccumulatorDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/FoldConsAccumulator.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Seq<T> {
                        public static Seq<T> Empty => new();
                        public Seq<T> Cons(T value) => this;
                        public TOut Fold<TOut>(TOut initialState, System.Func<TOut, T, TOut> f) => initialState;
                    }
                }

                namespace Domain.Services {
                    public sealed class FoldConsAccumulator {
                        public LanguageExt.Seq<int> Run(LanguageExt.Seq<int> values) =>
                            values.Fold(LanguageExt.Seq<int>.Empty, static (acc, value) => acc.Cons(value));
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0736", collection: ids);
    }

    [Fact]
    public async Task FoldSetAccumulatorDoesNotEmitFoldAppendAccumulatorDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Rasm.Rhino/Blocks/FoldSetAccumulator.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Seq<T> {
                        public TOut Fold<TOut>(TOut initialState, System.Func<TOut, T, TOut> f) => initialState;
                    }
                    public sealed class HashSet<T> {
                        public static HashSet<T> Empty => new();
                        public HashSet<T> Add(T key) => this;
                    }
                }

                namespace Rasm.Rhino.Blocks {
                    public sealed class FoldSetAccumulator {
                        public LanguageExt.HashSet<int> Run(LanguageExt.Seq<int> values) =>
                            values.Fold(LanguageExt.HashSet<int>.Empty, static (ids, id) => ids.Add(key: id));
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0736", collection: ids);
    }

    [Fact]
    public async Task NonUnitConditionalEmitsGuardableFinConditionalDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/NonUnitConditional.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "NonUnitConditional", members: """
                public LanguageExt.Fin<int> Run(bool accepted, int value, Error error) =>
                    accepted ? LanguageExt.Fin.Succ(value) : LanguageExt.Fin.Fail<int>(error);
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task EffectfulSuccessConditionalDoesNotEmitGuardableFinUnitDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/EffectfulSuccessConditional.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "EffectfulSuccessConditional", members: """
                public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, Error error) =>
                    accepted ? Validate() : LanguageExt.Fin.Fail<LanguageExt.Unit>(error);

                private static LanguageExt.Fin<LanguageExt.Unit> Validate() =>
                    LanguageExt.Fin.Succ(unit);
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task SideEffectingSuccessConditionalDoesNotEmitGuardableFinUnitDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SideEffectingSuccessConditional.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "SideEffectingSuccessConditional", members: """
                public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, Error error) =>
                    accepted ? LanguageExt.Fin.Succ(Side(() => Mutate())) : LanguageExt.Fin.Fail<LanguageExt.Unit>(error);

                private static LanguageExt.Unit Side(System.Action run) {
                    run();
                    return unit;
                }

                private static void Mutate() { }
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task RichFailureDetailEmitsGuardableFinConditionalDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/RichFailureDetail.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "RichFailureDetail", members: """
                public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, int value) =>
                    accepted ? LanguageExt.Fin.Succ(unit) : LanguageExt.Fin.Fail<LanguageExt.Unit>(new Error($"bad {value}"));
                """)).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task NonCommonErrorFailureDoesNotEmitGuardableFinUnitDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/NonCommonErrorFailure.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "NonCommonErrorFailure", members: """
                public sealed record UiFault(string Detail);

                public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, UiFault fault) =>
                    accepted ? LanguageExt.Fin.Succ(unit) : LanguageExt.Fin.Fail<LanguageExt.Unit>(fault);
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task OwnerRailsDoNotEmitGuardableFinConditionalDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/OwnerRails.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "OwnerRails", members: """
                public sealed class Op {
                    public LanguageExt.Fin<int> AcceptValue(int value) => new();
                    public Error InvalidResult(string detail) => new(detail);
                }

                public readonly struct Optional<T> {
                    public LanguageExt.Fin<T> ToFin(Error error) => new();
                }

                public readonly record struct Policy(int Value) {
                    public LanguageExt.Fin<Policy> Admit(Error error) =>
                        Value > 0 ? LanguageExt.Fin.Succ(this) : LanguageExt.Fin.Fail<Policy>(error);
                }

                public LanguageExt.Fin<LanguageExt.Unit> Confirm(bool accepted) =>
                    accepted ? LanguageExt.Fin.Succ(unit) : LanguageExt.Fin.Fail<LanguageExt.Unit>(new Error());

                public LanguageExt.Fin<int> Accept(Op op, int value) =>
                    op.AcceptValue(value);

                public LanguageExt.Fin<int> Existing(Optional<int> value, Error error) =>
                    value.ToFin(error);

                public LanguageExt.Fin<LanguageExt.Unit> NativeBoundary(bool accepted, Op op, string detail) =>
                    accepted ? LanguageExt.Fin.Succ(unit) : LanguageExt.Fin.Fail<LanguageExt.Unit>(op.InvalidResult(detail));
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task GuardCallDoesNotEmitGuardableFinUnitDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/GuardCall.cs",
            source: WithFinUnit(ns: "Domain.Services", type: "GuardCall", members: """
                public LanguageExt.Fin<LanguageExt.Unit> Run(bool accepted, Error error) =>
                    guard(accepted, error);
                """)).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0739", collection: ids);
    }

    [Fact]
    public async Task ThinktectureUnionDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/GeneratedDispatchUnion.cs",
            source: """
                namespace Thinktecture {
                    public sealed class UnionAttribute : System.Attribute { }
                }

                namespace Domain.Services {
                    [Thinktecture.Union]
                    public abstract record GeneratedDispatchUnion {
                        private GeneratedDispatchUnion() { }
                        public sealed record Created(int Id) : GeneratedDispatchUnion;
                        public sealed record Changed(int Id) : GeneratedDispatchUnion;
                        public sealed record Deleted(int Id) : GeneratedDispatchUnion;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task FrameworkOverrideHierarchyDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/FrameworkWriter.cs",
            source: """
                namespace Domain.Services {
                    public sealed class FrameworkWriter : System.IO.TextWriter {
                        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task TwoCaseManualUnionDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/TwoCaseManualUnion.cs",
            source: """
                namespace Domain.Services {
                    public abstract record TwoCaseManualUnion {
                        private TwoCaseManualUnion() { }
                        public abstract int Project();
                        public sealed record First(int Value) : TwoCaseManualUnion { public override int Project() => Value; }
                        public sealed record Second(int Value) : TwoCaseManualUnion { public override int Project() => Value; }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task GenericNestedCaseDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/GenericCaseManualUnion.cs",
            source: """
                namespace Domain.Services {
                    public abstract record GenericCaseManualUnion<TState> {
                        private GenericCaseManualUnion() { }
                        public abstract TState State { get; }
                        public sealed record Button(TState Value) : GenericCaseManualUnion<TState> { public override TState State => Value; }
                        public sealed record Label(TState Value) : GenericCaseManualUnion<TState> { public override TState State => Value; }
                        public sealed record Field<T>(TState Value, T Payload) : GenericCaseManualUnion<TState> { public override TState State => Value; }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task ExtraCaseBehaviorDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/BehaviorfulManualUnion.cs",
            source: """
                namespace Domain.Services {
                    public abstract record BehaviorfulManualUnion {
                        private BehaviorfulManualUnion() { }
                        public abstract int Project();
                        public sealed record First(int Value) : BehaviorfulManualUnion { public override int Project() => Value; }
                        public sealed record Second(int Value) : BehaviorfulManualUnion { public override int Project() => Value; }
                        public sealed record Third(int Value) : BehaviorfulManualUnion {
                            public override int Project() {
                                int adjusted = Value + 1;
                                return adjusted;
                            }
                        }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task PubliclyExtensibleManualHierarchyDoesNotEmitManualClosedUnionOverrideDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PublicHierarchy.cs",
            source: """
                namespace Domain.Services {
                    public abstract record PublicHierarchy {
                        protected PublicHierarchy() { }
                        public abstract int Project();
                        public sealed record First(int Value) : PublicHierarchy { public override int Project() => Value; }
                        public sealed record Second(int Value) : PublicHierarchy { public override int Project() => Value; }
                        public sealed record Third(int Value) : PublicHierarchy { public override int Project() => Value; }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0740", collection: ids);
    }

    [Fact]
    public async Task ForwardingRequestCapsuleDoesNotEmitForwardingRequestCaseFamilyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/RequestCapsule.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                }

                namespace Domain.Services {
                    public sealed class Scope { }
                    public abstract record Request<T> {
                        private readonly System.Func<Scope, LanguageExt.Fin<T>>? run;
                        protected Request() { }
                        private protected Request(System.Func<Scope, LanguageExt.Fin<T>> run) => this.run = run;
                        public virtual LanguageExt.Fin<T> Apply(Scope scope) => run!(scope);
                    }
                    public sealed record RequestCapsule<T>(System.Func<Scope, LanguageExt.Fin<T>> Run) : Request<T>(Run);
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0741", collection: ids);
    }

    [Fact]
    public async Task TwoForwardingRequestCasesDoNotEmitForwardingRequestCaseFamilyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/TwoForwardingCases.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                }

                namespace Domain.Services {
                    public sealed class Scope { }
                    public sealed class Intent<T> {
                        public LanguageExt.Fin<T> Run(Scope scope) => new();
                    }
                    public static class Intent {
                        public static Intent<T> First<T>() => new();
                        public static Intent<T> Second<T>() => new();
                    }
                    public abstract record TwoForwardingCases<T> {
                        public abstract LanguageExt.Fin<T> Apply(Scope scope);
                        public sealed record First : TwoForwardingCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.First<T>().Run(scope);
                        }
                        public sealed record Second : TwoForwardingCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Second<T>().Run(scope);
                        }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0741", collection: ids);
    }

    [Fact]
    public async Task ValidatingForwardingRequestCaseDoesNotEmitForwardingRequestCaseFamilyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ValidatingRequestCases.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                }

                namespace Domain.Services {
                    public sealed class Scope { }
                    public sealed class Intent<T> {
                        public LanguageExt.Fin<T> Run(Scope scope) => new();
                    }
                    public static class Intent {
                        public static Intent<T> First<T>() => new();
                        public static Intent<T> Second<T>() => new();
                        public static Intent<T> Third<T>() => new();
                    }
                    public abstract record ValidatingRequestCases<T> {
                        public abstract LanguageExt.Fin<T> Apply(Scope scope);
                        public sealed record First : ValidatingRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) {
                                _ = scope.GetHashCode();
                                return Intent.First<T>().Run(scope);
                            }
                        }
                        public sealed record Second : ValidatingRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Second<T>().Run(scope);
                        }
                        public sealed record Third : ValidatingRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Third<T>().Run(scope);
                        }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0741", collection: ids);
    }

    [Fact]
    public async Task RichScriptedRequestCasesDoNotEmitForwardingRequestCaseFamilyDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ScriptedRequestCases.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                }

                namespace Domain.Services {
                    public sealed class Scope { public Scope Rebind(string name) => this; }
                    public sealed class Intent<T> {
                        public LanguageExt.Fin<T> Run(Scope scope) => new();
                    }
                    public static class Intent {
                        public static Intent<T> First<T>() => new();
                        public static Intent<T> Second<T>() => new();
                        public static Intent<T> Third<T>() => new();
                    }
                    public abstract record ScriptedRequestCases<T> {
                        public abstract LanguageExt.Fin<T> Apply(Scope scope);
                        public sealed record First(string Name) : ScriptedRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.First<T>().Run(scope.Rebind(Name));
                        }
                        public sealed record Second(string Name) : ScriptedRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Second<T>().Run(scope.Rebind(Name));
                        }
                        public sealed record Third(string Name) : ScriptedRequestCases<T> {
                            public override LanguageExt.Fin<T> Apply(Scope scope) => Intent.Third<T>().Run(scope.Rebind(Name));
                        }
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0741", collection: ids);
    }

    [Fact]
    public async Task ManualOpAdmissionSwitchEmitsManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/ManualSwitchGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Plane(bool IsValid);
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class ManualSwitchGate {
                        public LanguageExt.Fin<Rhino.Geometry.Plane> Run(Rhino.Geometry.Plane plane, Op op) =>
                            plane switch {
                                Rhino.Geometry.Plane value when value.IsValid => LanguageExt.Fin.Succ(value: value),
                                _ => LanguageExt.Fin.Fail<Rhino.Geometry.Plane>(error: op.InvalidResult()),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task ManualOpAdmissionPropertyPatternEmitsManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PropertyPatternGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Box(bool IsValid);
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class PropertyPatternGate {
                        public LanguageExt.Fin<Rhino.Geometry.Box> Run(Rhino.Geometry.Box box, Op op) =>
                            box switch {
                                { IsValid: true } value => LanguageExt.Fin.Succ(value: value),
                                _ => LanguageExt.Fin.Fail<Rhino.Geometry.Box>(error: op.InvalidResult()),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task ManualOpAdmissionAcceptSuccessArmEmitsManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/AcceptSuccessGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                    public sealed class Seq<T> { }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Box(bool IsValid);
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidResult() => new();
                        public LanguageExt.Fin<LanguageExt.Seq<T>> Accept<T>(T value) => new();
                    }
                    public sealed class AcceptSuccessGate {
                        public LanguageExt.Fin<LanguageExt.Seq<Rhino.Geometry.Box>> Run(Rhino.Geometry.Box box, Op op) =>
                            box switch {
                                { IsValid: true } value => op.Accept(value: value),
                                _ => LanguageExt.Fin.Fail<LanguageExt.Seq<Rhino.Geometry.Box>>(error: op.InvalidResult()),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task RangeGateDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/RangeGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class RangeGate {
                        public LanguageExt.Fin<double> Run(double distance, Op op) =>
                            distance > 0.0
                                ? LanguageExt.Fin.Succ(value: distance)
                                : LanguageExt.Fin.Fail<double>(error: op.InvalidResult());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task InvalidInputGateDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/InvalidInputGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Plane(bool IsValid);
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidInput() => new();
                    }
                    public sealed class InvalidInputGate {
                        public LanguageExt.Fin<Rhino.Geometry.Plane> Run(Rhino.Geometry.Plane plane, Op op) =>
                            plane.IsValid
                                ? LanguageExt.Fin.Succ(value: plane)
                                : LanguageExt.Fin.Fail<Rhino.Geometry.Plane>(error: op.InvalidInput());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task TransformedSuccessDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/TransformedSuccess.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Plane(bool IsValid);
                }

                namespace Domain.Services {
                    public readonly record struct PlaneBox(Rhino.Geometry.Plane Plane);
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class TransformedSuccess {
                        public LanguageExt.Fin<PlaneBox> Run(Rhino.Geometry.Plane plane, Op op) =>
                            plane.IsValid
                                ? LanguageExt.Fin.Succ(value: new PlaneBox(plane))
                                : LanguageExt.Fin.Fail<PlaneBox>(error: op.InvalidResult());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task FinUnitGateDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/UnitGate.cs",
            source: """
                namespace LanguageExt {
                    public readonly struct Unit { }
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }

                namespace Domain.Services {
                    public readonly record struct Plane(bool IsValid);
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class UnitGate {
                        public LanguageExt.Fin<LanguageExt.Unit> Run(Plane plane, Op op) =>
                            plane.IsValid
                                ? LanguageExt.Fin.Succ(value: new LanguageExt.Unit())
                                : LanguageExt.Fin.Fail<LanguageExt.Unit>(error: op.InvalidResult());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task PrivateIsValidStructDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/PrivateFrameGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                }

                namespace Domain.Services {
                    public readonly record struct DetailFrame(bool IsValid);
                    public sealed class Op {
                        public object InvalidResult() => new();
                    }
                    public sealed class PrivateFrameGate {
                        public LanguageExt.Fin<DetailFrame> Run(DetailFrame frame, Op op) =>
                            frame.IsValid
                                ? LanguageExt.Fin.Succ(value: frame)
                                : LanguageExt.Fin.Fail<DetailFrame>(error: op.InvalidResult());
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task ProjectionAdmissionMethodDoesNotEmitManualOpAdmissionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/AcceptResultsGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Fail<T>(object error) => new();
                    }
                    public sealed class Seq<T> { }
                }
                namespace Rhino.Geometry {
                    public readonly record struct Box(bool IsValid);
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public object InvalidResult() => new();
                        public LanguageExt.Fin<LanguageExt.Seq<T>> AcceptResults<T>(T value) => new();
                    }
                    public sealed class AcceptResultsGate {
                        public LanguageExt.Fin<LanguageExt.Seq<Rhino.Geometry.Box>> Run(Rhino.Geometry.Box box, Op op) =>
                            box switch {
                                { IsValid: true } value => op.AcceptResults(value),
                                _ => LanguageExt.Fin.Fail<LanguageExt.Seq<Rhino.Geometry.Box>>(error: op.InvalidResult()),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0742", collection: ids);
    }

    [Fact]
    public async Task SwitchExpressionEmitsManualGenericProjectionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/SwitchProjectionGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public sealed class Seq<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(LanguageExt.Common.Error error) => new();
                    }
                }

                namespace LanguageExt.Common {
                    public class Error { }
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public LanguageExt.Common.Error Unsupported(System.Type geometryType, System.Type outputType) => new();
                    }
                    public sealed class SwitchProjectionGate {
                        public LanguageExt.Fin<TOut> Project<TOut>(LanguageExt.Seq<int> values, Op op) =>
                            typeof(TOut) switch {
                                System.Type t when t == typeof(LanguageExt.Seq<int>) => LanguageExt.Fin.Succ((TOut)(object)values),
                                _ => LanguageExt.Fin.Fail<TOut>(op.Unsupported(typeof(SwitchProjectionGate), typeof(TOut))),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0743", collection: ids);
    }

    [Fact]
    public async Task AcceptValueMapEmitsManualGenericProjectionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/AcceptValueProjectionGate.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> {
                        public Fin<TOut> Map<TOut>(System.Func<T, TOut> map) => new();
                    }
                    public static class Fin {
                        public static Fin<T> Fail<T>(LanguageExt.Common.Error error) => new();
                    }
                }

                namespace LanguageExt.Common {
                    public class Error { }
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public LanguageExt.Fin<int> AcceptValue(int value) => new();
                        public LanguageExt.Common.Error Unsupported(System.Type geometryType, System.Type outputType) => new();
                    }
                    public sealed class AcceptValueProjectionGate {
                        public LanguageExt.Fin<TOut> Project<TOut>(int value, Op op) =>
                            typeof(TOut) == typeof(int)
                                ? op.AcceptValue(value).Map(static accepted => (TOut)(object)accepted)
                                : LanguageExt.Fin.Fail<TOut>(op.Unsupported(typeof(int), typeof(TOut)));
                    }
                }
                """).ConfigureAwait(true);

        Assert.Contains(expected: "CSP0743", collection: ids);
    }

    [Fact]
    public async Task ProjectionOwnerDoesNotEmitManualGenericProjectionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/AtomProjection.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(LanguageExt.Common.Error error) => new();
                    }
                }

                namespace LanguageExt.Common {
                    public class Error { }
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public LanguageExt.Common.Error Unsupported(System.Type geometryType, System.Type outputType) => new();
                    }
                    public static class AtomProjection {
                        public static LanguageExt.Fin<TOut> Self<TSelf, TOut>(TSelf value, Op op) =>
                            typeof(TOut) == typeof(TSelf)
                                ? LanguageExt.Fin.Succ((TOut)(object)value!)
                                : LanguageExt.Fin.Fail<TOut>(op.Unsupported(typeof(TSelf), typeof(TOut)));
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0743", collection: ids);
    }

    [Fact]
    public async Task OptionBoundaryCastDoesNotEmitManualGenericProjectionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Integration/OptionBoundaryProjection.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Option<T> { }
                    public static class Option {
                        public static Option<T> Some<T>(T value) => new();
                        public static Option<T> None<T>() => new();
                    }
                }

                namespace Integration {
                    public sealed class OptionBoundaryProjection {
                        public LanguageExt.Option<TOut> Project<TOut>(object value) =>
                            value is TOut typed ? LanguageExt.Option.Some(typed) : LanguageExt.Option.None<TOut>();
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0743", collection: ids);
    }

    [Fact]
    public async Task MultiOutputReceiptSwitchDoesNotEmitManualGenericProjectionGateDiagnosticAsync() {
        ImmutableArray<string> ids = await AnalyzeIdsAsync(
            filePath: "/workspace/src/Domain/Services/MultiOutputReceiptSwitch.cs",
            source: """
                namespace LanguageExt {
                    public sealed class Fin<T> { }
                    public static class Fin {
                        public static Fin<T> Succ<T>(T value) => new();
                        public static Fin<T> Fail<T>(LanguageExt.Common.Error error) => new();
                    }
                }

                namespace LanguageExt.Common {
                    public class Error { }
                }

                namespace Domain.Services {
                    public sealed class Op {
                        public LanguageExt.Common.Error Unsupported(System.Type geometryType, System.Type outputType) => new();
                    }
                    public readonly record struct Result(int Value);
                    public readonly record struct Receipt(int Count);
                    public sealed class MultiOutputReceiptSwitch {
                        public LanguageExt.Fin<TOut> Project<TOut>(Result result, Receipt receipt, Op op) =>
                            typeof(TOut) switch {
                                System.Type t when t == typeof(Result) => LanguageExt.Fin.Succ((TOut)(object)result),
                                System.Type t when t == typeof(Receipt) => LanguageExt.Fin.Succ((TOut)(object)receipt),
                                _ => LanguageExt.Fin.Fail<TOut>(op.Unsupported(typeof(MultiOutputReceiptSwitch), typeof(TOut))),
                            };
                    }
                }
                """).ConfigureAwait(true);

        Assert.DoesNotContain(expected: "CSP0743", collection: ids);
    }

    public static TheoryData<string, string, string> RuleCases() =>
        Cases.Aggregate(
            seed: [],
            func: static (TheoryData<string, string, string> data, RuleCase row) => {
                data.Add(row.Id, row.Path, row.Source);
                return data;
            });

    private static async Task<ImmutableArray<string>> AnalyzeIdsAsync(string filePath, string source) =>
        [
            .. (await AnalyzerTestHarness.AnalyzeAsync(source: source, filePath: filePath).ConfigureAwait(true))
                .Select(static diagnostic => diagnostic.Id)
                .Distinct(StringComparer.Ordinal),
        ];

    private static string DiagnosticText(ImmutableArray<Diagnostic> diagnostics) =>
        string.Join(
            separator: Environment.NewLine,
            values: diagnostics.Select(static diagnostic => $"{diagnostic.Id}|{diagnostic.Severity}|{diagnostic.Descriptor.Category}|{diagnostic.Location.GetLineSpan().StartLinePosition}|{diagnostic.GetMessage(formatProvider: CultureInfo.InvariantCulture)}"));

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

    private static string WithFin(string ns, string declaration) =>
        $$"""
        namespace LanguageExt {
            public sealed class Fin<T> { }
        }

        namespace {{ns}} {
        {{declaration}}
        }
        """;

    private static string WithFinUnit(string ns, string type, string members) =>
        $$"""
        namespace LanguageExt {
            public readonly struct Unit { }
            public sealed class Fin<T> { }
            public static class Fin {
                public static Fin<T> Succ<T>(T value) => new();
                public static Fin<T> Fail<T>(object error) => new();
            }
            public static class Prelude {
                public static readonly Unit unit = new();
                public static Fin<Unit> guard(bool condition, object error) => new();
            }
        }

        namespace LanguageExt.Common {
            public sealed record Error(string? Detail = null);
        }

        namespace {{ns}} {
            using LanguageExt.Common;
            using static LanguageExt.Prelude;

            public sealed class {{type}} {
        {{members}}
            }
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
            public readonly struct Unit {
                public static Unit Default => new();
            }
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
