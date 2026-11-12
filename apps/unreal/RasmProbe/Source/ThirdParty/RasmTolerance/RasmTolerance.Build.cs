// Copyright 2026 Bardia Samiee. All Rights Reserved.

using System.IO;
using UnrealBuildTool;

public class RasmTolerance : ModuleRules
{
	public RasmTolerance(ReadOnlyTargetRules Target) : base(Target)
	{
		Type = ModuleType.External;
		PublicSystemIncludePaths.Add(Path.Combine(ModuleDirectory, "include"));
		PublicAdditionalLibraries.Add(Path.Combine(ModuleDirectory, "lib", "librasm-tolerance.a"));
	}
}
