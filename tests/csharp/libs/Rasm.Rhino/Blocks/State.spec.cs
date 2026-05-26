using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.TestKit;
using Rhino.Display;

namespace Rasm.Rhino.Tests.Blocks;

// --- [CONSTANTS] ------------------------------------------------------------------------
internal static class BlockStateCases {
    public static readonly Op Key = Op.Of(name: "blocks-state-test");
}

// --- [ALGEBRAIC] ------------------------------------------------------------------------
public sealed class BlockPolicyLaws {
    [Fact]
    public void UpdateLayerAndLifecyclePoliciesAdmitOnlyNativeStates() {
        Spec.SmartEnumKeysUnique(items: [UpdatePolicy.Static, UpdatePolicy.LinkedAndEmbedded, UpdatePolicy.Linked], key: static policy => policy.Key);
        Spec.SmartEnumKeysUnique(items: [LayerStyle.None, LayerStyle.Active, LayerStyle.Reference], key: static style => style.Key);
        Assert.True(condition: LayerStyle.None.AppliesTo(policy: UpdatePolicy.Static));
        Assert.True(condition: LayerStyle.None.AppliesTo(policy: UpdatePolicy.LinkedAndEmbedded));
        Assert.True(condition: LayerStyle.Active.AppliesTo(policy: UpdatePolicy.Linked));
        Assert.True(condition: LayerStyle.Reference.AppliesTo(policy: UpdatePolicy.Linked));
        Assert.False(condition: LayerStyle.Reference.AppliesTo(policy: UpdatePolicy.LinkedAndEmbedded));
        Assert.True(condition: ArchiveStatus.LinkedFileIsDifferent.RequiresRefresh);
        Assert.True(condition: ArchiveStatus.LinkedFileIsDifferent.CanRefresh);
        Assert.True(condition: ArchiveStatus.LinkedFileNotFound.IsBroken);
        Assert.False(condition: ArchiveStatus.NotALinked.RequiresRefresh);
    }

    [Fact]
    public void PublicPoliciesRejectInvalidDirectConstruction() {
        Spec.Fail((PreviewSpec.Default with { Width = 8 }).Admit(key: BlockStateCases.Key));
        Spec.Fail(new WatchPolicy(Debounce: TimeSpan.Zero).Admit(key: BlockStateCases.Key));
        Spec.Fail(new FlattenPolicy(MaxDepth: 0).Admit(key: BlockStateCases.Key));
        Spec.Fail(new LinkCreatePolicy(Update: UpdatePolicy.Static, Layer: LayerStyle.Reference).Admit(key: BlockStateCases.Key));
        Spec.Succ(new LinkCreatePolicy(Update: UpdatePolicy.Linked, Layer: LayerStyle.Reference).Admit(key: BlockStateCases.Key), then: policy =>
            Assert.Equal(expected: LinkReloadPolicy.NestedQuiet, actual: policy.EffectiveReload));
    }
}

public sealed class PreviewSpecLaws {
    [Fact]
    public void FingerprintChangesForEveryNativePreviewAxis() {
        PreviewSpec baseline = PreviewSpec.Default;
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Width = 320 }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Height = 320 }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Projection = DefinedViewportProjection.Top }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { Camera = IsometricCamera.Southwest }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { DisplayMode = DisplayModeRef.Of(id: Guid.Parse(input: "3f1e9f89-f4e6-4e5c-95f7-a848a0833cfa")) }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { DrawDecorations = true }).Fingerprint);
        Assert.NotEqual(expected: baseline.Fingerprint, actual: (baseline with { ApplyDpiScaling = true }).Fingerprint);
    }
}

