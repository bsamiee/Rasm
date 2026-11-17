using CsCheck;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using QuikGraph;
using Rasm.Grasshopper.Doc;
using Xunit;

namespace Rasm.Rhino.Tests.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------

[Collection(typeof(Grasshopper2PlugIn))]
public sealed class GraphTests {

    // --- [WIRES]

    [Fact]
    public Task WireGraphMatchesReachabilityForRanksCyclesShortcutsAndLayers() =>
        Wiring.SampleAsync(static async wiring => {
            Seq<(int From, int To)> wires = toSeq(
                from source in Enumerable.Range(0, wiring.Count)
                from target in Enumerable.Range(0, wiring.Count)
                where source != target && wiring.Edges[source, target]
                select (From: source, To: target)).Strict();
            (WireGraph graph, Seq<Guid> ids) = await RhinoHost.RunAsync((
                from document in use(static () => Grasshopper2.Doc.Document.NewInertDocument(), static held => IO.lift(held.Close))
                from relays in toSeq(Enumerable.Range(0, wiring.Count)).TraverseM(_ => DocumentEdits.Add(document, new Relay())).As()
                from wired in wires.TraverseM(wire => DocumentEdits.Connect(relays[wire.From], relays[wire.To], ActionList.Empty)).As()
                select (WireGraph.Of(document.Objects.Connectivity), relays.Map(static relay => relay.InstanceId).Strict())).Bracket());
            Seq<Set<int>> reach = Range(0, wiring.Count).Fold(
                toSeq(Enumerable.Range(0, wiring.Count)).Map(source => toSet(wires.Filter(wire => wire.From == source).Map(static wire => wire.To))).Strict(),
                static (held, _) => held.Map(near => near + toSet(near.SelectMany(via => held[via]))).Strict());
            Seq<Set<int>> component = toSeq(Enumerable.Range(0, wiring.Count)).Map(at => toSet(Enumerable.Range(0, wiring.Count).Where(other => other == at || (reach[at].Contains(other) && reach[other].Contains(at))))).Strict();
            Seq<int> ranks = Range(0, wiring.Count).Fold(
                toSeq(Enumerable.Range(0, wiring.Count)).Map(static _ => 0).Strict(),
                (held, _) => held.Map((rank, at) => wires.Filter(wire => component[at].Contains(wire.To) && !component[at].Contains(wire.From)).Fold(rank, (deepest, wire) => Math.Max(deepest, held[wire.From] + 1))).Strict());
            Assert.Equal(ranks, ids.Map(id => graph.Ranks[id]));
            Assert.Equal(
                toHashSet(component.Filter(static members => members.Count > 1).Map(members => members.Map(member => ids[member]))),
                toHashSet(graph.Cycles().Map(static members => toSet(members))));
            Assert.Equal(
                toHashSet(wires.Filter(wire => !component[wire.From].Contains(wire.To)
                        && toSeq(Enumerable.Range(0, wiring.Count)).Exists(via => !component[via].Contains(wire.From) && !component[via].Contains(wire.To) && reach[wire.From].Contains(via) && reach[via].Contains(wire.To)))
                    .Map(wire => new SEquatableEdge<Guid>(ids[wire.From], ids[wire.To]))),
                toHashSet(graph.Redundant()));
            Assert.Equal(
                toSeq(Enumerable.Range(0, wiring.Count).GroupBy(at => ranks[at]).OrderBy(static layer => layer.Key).Select(layer => toSet(layer.Select(at => ids[at])))),
                graph.Layers().Map(static layer => toSet(layer)));
        });

    // --- [GENERATORS]

    private static readonly Gen<(int Count, bool[,] Edges)> Wiring =
        from count in Gen.Int[2, 6]
        from edges in Gen.Bool.Array2D[count, count]
        select (count, edges);
}
