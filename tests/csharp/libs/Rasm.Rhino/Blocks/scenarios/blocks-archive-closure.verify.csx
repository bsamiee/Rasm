using System;
using System.IO;
using Rasm.Rhino.Blocks;
using Rhino.FileIO;

Scenario.Run("blocks-archive-closure", CAPTURE_PATH, (key, facts) => {
    string root = Path.Combine(path1: Path.GetTempPath(), path2: $"RasmClosure{Guid.NewGuid():N}");
    Directory.CreateDirectory(path: root);
    string subDir = Path.Combine(path1: root, path2: "sub");
    Directory.CreateDirectory(path: subDir);
    string nestedPath = Path.Combine(path1: root, path2: "nested.3dm");
    string childPath = Path.Combine(path1: subDir, path2: "child.3dm");
    string parentPath = Path.Combine(path1: root, path2: "parent.3dm");
    using (File3dm nested = new()) {
        Probe.Require(nested.Write(path: nestedPath, version: 8), "nested write");
    }
    using (File3dm child = new()) {
        Probe.Require(
            child.AllInstanceDefinitions.AddLinked(
                filename: Path.Combine(path1: "..", path2: "nested.3dm"),
                name: "NestedLink",
                description: string.Empty) >= 0,
            "child nested link");
        Probe.Require(child.Write(path: childPath, version: 8), "child write");
    }
    using (File3dm parent = new()) {
        Probe.Require(
            parent.AllInstanceDefinitions.AddLinked(
                filename: Path.Combine(path1: "sub", path2: "child.3dm"),
                name: "ChildLink",
                description: string.Empty) >= 0,
            "parent child link");
        Probe.Require(parent.Write(path: parentPath, version: 8), "parent write");
    }
    using File3dm parentModel = File3dm.Read(path: parentPath)
        ?? throw new InvalidOperationException(message: "parent read");
    Seq<Archive.LinkedArchiveEdge> edges = Archive.LinkedArchiveClosure(root: parentModel, rootPath: parentPath, key: key)
        .IfFail(error => throw new InvalidOperationException(message: error.Message));
    ArchiveClosureReport report = Archive.ValidateArchiveClosure(root: parentModel, rootPath: parentPath, key: key)
        .IfFail(error => throw new InvalidOperationException(message: error.Message));
    facts.Add("closure.count", edges.Count);
    facts.Add("validate.valid", report.Valid);
    facts.Add("validate.broken", report.Broken.Count);
    facts.Add("validate.cycles", report.Cycles.Count);
    facts.Add("closure.child", edges[0].Link.Full.Value);
    facts.Add("closure.nested", edges.Count > 1 ? edges[1].Link.Full.Value : string.Empty);
    Probe.Require(edges.Count == 2, $"closure.count={edges.Count}");
    Probe.Require(report.Valid, "validate.valid");
    Probe.Require(report.Broken.IsEmpty, $"validate.broken={report.Broken.Count}");
    Probe.Require(report.Cycles.IsEmpty, $"validate.cycles={report.Cycles.Count}");
    Probe.Require(report.Edges.Count == edges.Count, $"validate.edges={report.Edges.Count}");
    Probe.Require(edges[0].Depth == 0, $"depth0={edges[0].Depth}");
    Probe.Require(
        string.Equals(
            a: Path.GetFullPath(path: childPath),
            b: edges[0].Link.Full.Value,
            comparisonType: StringComparison.OrdinalIgnoreCase),
        "child path");
    Probe.Require(edges[1].Depth == 1, $"depth1={edges[1].Depth}");
    Probe.Require(
        string.Equals(
            a: Path.GetFullPath(path: nestedPath),
            b: edges[1].Link.Full.Value,
            comparisonType: StringComparison.OrdinalIgnoreCase),
        "nested path");
});
