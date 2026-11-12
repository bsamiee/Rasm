// Copyright 2026 Bardia Samiee. All Rights Reserved.

using UnrealBuildTool;

public class RasmProbe : ModuleRules
{
	public RasmProbe(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		PublicDependencyModuleNames.AddRange(new string[] { "Core" });
		PrivateDependencyModuleNames.AddRange(new string[] { "CoreUObject", "Engine", "RasmTolerance" });
	}
}
