// Copyright 2026 Bardia Samiee. All Rights Reserved.

using UnrealBuildTool;

public class Tolerance : ModuleRules {
    public Tolerance(ReadOnlyTargetRules Target) : base(Target) {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
        PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine" });
        PrivateDependencyModuleNames.Add("RasmTolerance");
    }
}
