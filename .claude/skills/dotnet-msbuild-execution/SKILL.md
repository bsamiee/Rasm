---
name: dotnet-msbuild-execution
description: "enter a description here"
---

# [DOTNET_MSBUILD_EXECUTION]

## [01]-[WORKFLOW]-[ORGANIZING_BUILD_INFRASTRUCTURE]

1. Audit all `.csproj` files — Catalog every `<PropertyGroup>`, `<ItemGroup>`, and custom `<Target>` across the solution. Note which settings repeat and which are project-specific.
2. Create root `Directory.Build.props` — Move shared property defaults (LangVersion, Nullable, TreatWarningsAsErrors, metadata) here. These are imported before the project file so projects can override them.
3. Create root `Directory.Build.targets` — Move custom build targets, post-build validation, and any properties that depend on SDK-defined values (e.g., `OutputPath`, `TargetFramework` for single-targeting projects) here. These are imported after the SDK so all properties are available.
4. Create `Directory.Packages.props` — Enable Central Package Management (`ManagePackageVersionsCentrally`), list all `PackageVersion` entries, and remove `Version=` from `PackageReference` items in `.csproj` files.
5. Set up multi-level hierarchy — Create inner `Directory.Build.props` files for `libs/dotnet/` and `tests/dotnet/` folders with distinct settings. Use `GetPathOfFileAbove` to chain to the parent.
6. Simplify `.csproj` files — Remove all centralized properties, version attributes, and duplicated targets. Each project should only contain what is unique to it.
7. Validate — Run `dotnet restore && dotnet build` and verify no regressions. Use `dotnet msbuild -pp:output.xml` to inspect the final merged view if needed.
