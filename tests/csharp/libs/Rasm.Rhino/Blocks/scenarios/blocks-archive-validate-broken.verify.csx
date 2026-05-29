using System;
using System.IO;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Exchange;
using Rhino;
using Rhino.Commands;
using Rhino.FileIO;

Scenario.Run("blocks-archive-validate-broken", CAPTURE_PATH, (key, facts) => {
    string root = Path.Combine(path1: Path.GetTempPath(), path2: $"RasmBroken{Guid.NewGuid():N}");
    Directory.CreateDirectory(path: root);
    string parentPath = Path.Combine(path1: root, path2: "parent.3dm");
    string missingChild = Path.Combine(path1: root, path2: "missing-child.3dm");
    using (File3dm parent = new()) {
        Probe.Require(
            parent.AllInstanceDefinitions.AddLinked(
                filename: Path.GetFileName(path: missingChild),
                name: "MissingLink",
                description: string.Empty) >= 0,
            "parent missing link");
        Probe.Require(parent.Write(path: parentPath, version: 8), "parent write");
    }
    using File3dm parentModel = File3dm.Read(path: parentPath)
        ?? throw new InvalidOperationException(message: "parent read");
    ArchiveClosureReport direct = Archive.ValidateArchiveClosure(root: parentModel, rootPath: parentPath, key: key)
        .IfFail(error => throw new InvalidOperationException(message: error.Message));
    facts.Add("direct.valid", direct.Valid);
    facts.Add("direct.broken", direct.Broken.Count);
    Probe.Require(!direct.Valid, "direct invalid");
    Probe.Require(direct.Broken.Count >= 1, $"direct.broken={direct.Broken.Count}");
    using DocumentScope scope = DocumentScope.Open();
    RhinoBlocks blocks = RhinoBlocks.Live(document: scope.Active, mode: RunMode.Scripted);
    FileEndpoint endpoint = Probe.Expect(FileEndpoint.From(path: parentPath), "endpoint");
    BlockOutcome outcome = Probe.Expect(
        blocks.Run(op: new BlockOp.ValidateArchiveClosure(Source: endpoint), key: key),
        "validate op");
    BlockOutcome.ClosureReport routed = outcome is BlockOutcome.ClosureReport value
        ? value
        : throw new InvalidOperationException(message: $"unexpected outcome: {outcome.GetType().Name}");
    facts.Add("op.valid", routed.Value.Valid);
    facts.Add("op.broken", routed.Value.Broken.Count);
    Probe.Require(!routed.Value.Valid, "op invalid");
    Probe.Require(routed.Value.Broken.Count >= 1, $"op.broken={routed.Value.Broken.Count}");
});
