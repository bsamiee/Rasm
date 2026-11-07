using Darkroom.Chain;
using Darkroom.Output;
using Darkroom.Queue;
using Darkroom.Review;
using MathNet.Numerics.Integration;
using Rasm.Rhino;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.PlugIns;
using Rhino;
using Rhino.DocObjects.SnapShots;
using Rhino.PlugIns;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class DarkroomPlugIn : DefinedPlugIn<DarkroomPlugIn>, IDefined<PlugInDefinition> {
    private const uint ChainSnapshot = 0x5244_5353u;

    internal static TimeProvider Clock { get; } = TimeProvider.System;

    internal static IO<GaussLegendreRule> Integration { get; } =
        IO.lift(static () => new SettingsNode(new SettingsRoot.PlugInNode(Session.PlugIn.Id)))
            .Bind(static node => Preferences.Setting(node, "Samples", 32, Some<Func<int, bool>>(static count => count > 0)).Read)
            .Map(static count => DarkroomEffect.Derived.Derived(count, static samples => new GaussLegendreRule(0d, 1d, samples)));

    public static PlugInDefinition Definition { get; } = new() {
        LoadTime = PlugInLoadTime.AtStartup,
        Logging = [Dispatch.Logging],
        Commands = Dispatch.Commands + Exports.Commands,
        Load = [
            IO.lift(static () => new SnapshotDefinition<ChainState, Unit>(new Guid(0x6affd2cd, 0x7fc8, 0x41fb, 0x8c, 0x42, 0x79, 0x67, 0x63, 0x65, 0x60, 0xb3), Session.PlugIn.Name, SnapShotsClient.RenderingCategory) {
                Document = Some(new SnapshotRecord<RhinoDoc, ChainState>(
                    ChainSnapshot, ArchivableDictionaries.Json<ChainState>(nameof(ChainState), 4), ChainState.Read,
                    static (doc, state) => new ChainEdit.Restore(state).Committed(doc).Map(static _ => unit))),
            }).Bind(static snapshots => snapshots.Register),
            RenderHistory.Load,
            Dispatch.Load,
        ],
    };
}
