using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Expressions;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class UnvisitedRecursionTests : CSharpCodeFixVerifier<UnvisitedRecursionAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsRecursionOverKeyedSuccessorsAndIgnoresVisitedSetOrTreeChildren() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal sealed record Node(Seq<Node> Children);

        internal static class Walks {
            internal static int Depth(IReadOnlyDictionary<string, Seq<string>> graph, string node) {
                int deepest = 0;
                foreach (string next in graph[node]) deepest = Math.Max(deepest, {|#0:Depth(graph, next)|});
                return deepest + 1;
            }

            internal static Seq<string> Reached(Func<string, Seq<string>> successors, string node) =>
                successors(node).Bind(next => {|#1:Reached(successors, next)|}).Add(node);

            internal static Unit Visited(HashMap<string, Seq<string>> graph, Action<string> act, string node) {
                act(node);
                return graph[node].Iter(next => {|#2:Visited(graph, act, next)|});
            }

            internal static Seq<string> Marked(IReadOnlyDictionary<string, Seq<string>> graph, System.Collections.Generic.HashSet<string> seen, string node) =>
                seen.Add(node) ? graph[node].Bind(next => Marked(graph, seen, next)).Add(node) : Seq<string>();

            internal static int Tree(Node node) => node.Children.Map(static child => Tree(child)).Sum() + 1;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Depth", "graph[node]", "node"),
        Diagnostic().WithLocation(1).WithArguments("Reached", "successors(node)", "node"),
        Diagnostic().WithLocation(2).WithArguments("Visited", "graph[node]", "node"));
}
