// Copyright 2026 Bardia Samiee. All Rights Reserved.

using System.IO;
using UnrealBuildTool;

public class RasmTolerance : ModuleRules {
    public RasmTolerance(ReadOnlyTargetRules Target) : base(Target) {
        Type = ModuleType.External;
        string Installed = Path.Combine(ModuleDirectory, Target.Platform.ToString());
        PublicSystemIncludePaths.Add(Path.Combine(Installed, "include"));
        PublicAdditionalLibraries.Add(Path.Combine(Installed, "lib", "librasm-tolerance.a"));
    }
}
