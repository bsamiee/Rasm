using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Expressions;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DiscardedAwaitableTests : CSharpCodeFixVerifier<DiscardedAwaitableAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsDiscardOfTaskOrValueTask() => VerifyAnalyzerAsync(
        """
        using System.Threading.Tasks;

        internal static class DiscardedAwaitable {
            internal static void Started(int count) => {|#0:_ = Task.Run(() => count + 1)|};
            internal static void Delayed() => {|#1:_ = Task.Delay(1)|};
            internal static void Valued() => {|#2:_ = ValueTask.FromResult(1)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Task.Run(() => count + 1)"),
        Diagnostic().WithLocation(1).WithArguments("Task.Delay(1)"),
        Diagnostic().WithLocation(2).WithArguments("ValueTask.FromResult(1)"));

    [Fact]
    public Task IgnoresAwaitedReturnedOrNonAwaitableDiscard() => VerifyAnalyzerAsync(
        """
        using System.Threading.Tasks;

        internal static class ObservedAwaitable {
            internal static async Task Awaited() => await Task.Delay(1);
            internal static Task Returned() => Task.Delay(1);
            internal static async Task<int> Discarded() {
                _ = await Task.FromResult(1);
                return 1;
            }
            internal static void Value(int count) => _ = count + 1;
        }

        internal sealed record Awaited;
        """);

    [Fact]
    public Task ReportsDiscardedExtensionAwaitableButNotInvalidHiddenOrDynamic() => VerifyAnalyzerAsync(
        """
        using System.Runtime.CompilerServices;
        using System.Threading.Tasks;

        internal sealed record Awaited;

        internal static class AwaiterExtensions {
            internal static TaskAwaiter GetAwaiter(this Awaited value) => Task.CompletedTask.GetAwaiter();
        }

        internal sealed class Unrelated {
            internal int GetAwaiter() => 1;
        }

        internal sealed class Hidden {
            private TaskAwaiter GetAwaiter() => Task.CompletedTask.GetAwaiter();
        }

        internal static class CustomAwaitables {
            internal static void Extended(Awaited value) => {|#0:_ = value|};

            internal static void Named(Unrelated value) => _ = value;

            internal static void Inaccessible(Hidden value) => _ = value;

            internal static void Dynamic(dynamic value) => _ = value;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("value"));

    [Fact]
    public Task IgnoresDiscardWhoseDeclaringTypeRaisesTheResult() => VerifyAnalyzerAsync(
        """
        using System;
        using System.ComponentModel;
        using System.Threading.Tasks;

        internal sealed record Run;

        internal sealed class RunEventArgs(Run run) : EventArgs {
            internal Run Run { get; } = run;
        }

        internal sealed class Server {
            internal event EventHandler<RunEventArgs>? Completed;

            internal Task<Run> Start() {
                Run run = new();
                Completed?.Invoke(this, new(run));
                return Task.FromResult(run);
            }
        }

        internal sealed class Silent {
            internal Task<Run> Start() => Task.FromResult(new Run());
        }

        internal sealed class Prompt {
            internal event EventHandler<CancelEventArgs>? Closing;

            internal Task<bool> Show() {
                Closing?.Invoke(this, new());
                return Task.FromResult(true);
            }
        }

        internal static class Requests {
            internal static void Raised(Server server) => _ = server.Start();

            internal static void Unraised(Silent silent) => {|#0:_ = silent.Start()|};

            internal static void Primitive(Prompt prompt) => {|#1:_ = prompt.Show()|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("silent.Start()"),
        Diagnostic().WithLocation(1).WithArguments("prompt.Show()"));
}
