# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working on Arches within Rasm. The root repository standards also apply.

## Project Overview

This is a Rhino 3D plugin for creating architectural arch geometry. The plugin provides parametric generation of various historical arch types (semicircular, segmental, pointed, horseshoe, basket handle, Tudor, Persian, etc.) within Rhino's modeling environment.

## Build Commands

Run these commands from the Rasm repository root.

```bash
# Build entire solution
dotnet build Workspace.slnx

# Build specific project
dotnet build apps/arches/Rhino.Arches.Plugin/Rhino.Arches.Plugin.csproj

# Build release
dotnet build Workspace.slnx -c Release
```

The plugin output is `Rhino.Arches.Plugin.rhp` for Rhino 9 on macOS. Shared .NET settings and the RhinoCommon version come from Rasm's root build files and package catalog.

## Architecture

### Project Structure

```text
apps/arches/
  ├── Arches/                    # Host-independent interfaces
  │   ├── ArchBase.cs             # Abstract base class for all arches
  │   ├── IArchRepository.cs      # Main repository interface
  │   └── I*ArchRepository.cs     # Category-specific interfaces
  │
  ├── Rhino.Arches.Core/          # Geometry computation (depends on Arches, RhinoCommon)
  │   ├── RhinoArchBase.cs        # Rhino-specific arch base with Arc list and drawing
  │   ├── Extensions/             # Static geometry helpers (ArcGeometry, CircleGeometry, etc.)
  │   ├── Circular/               # SemiCircularArch, SegmentalArch
  │   ├── TwoCentered/            # EquilateralArch, LancetArch, DepressedArch
  │   ├── ThreeCentered/          # BasketHandleArch, DepreesedArch
  │   ├── FourCentered/           # TudorArch, PersianArch
  │   └── HorseShoe/              # RoundedArch, PointedArch
  │
  ├── Rhino.Arches.Interaction/   # User interaction layer (depends on Arches, Core)
  │   ├── InteractionBase.cs      # Base class for click-to-draw workflows
  │   ├── Repository/             # Implements I*ArchRepository interfaces
  │   └── Faactory/ClickFactory   # Point picking and option handling
  │
  └── Rhino.Arches.Plugin/        # Rhino plugin entry point (depends on Interaction)
      ├── Arches_Plugin.cs        # Plugin registration
      ├── ArchCommandBase.cs      # Base command class
      └── Commands/               # Rhino command implementations
```

### Key Patterns

**Repository Pattern**: `ArchesRepository` (singleton) provides access to category-specific repositories (`CircularArchRepository`, `TwoCenteredArchRepository`, etc.) that implement interfaces from the `Arches` project.

**Arch Construction**: Each arch type has a static `Build*` method that takes span points and normal vector, returning a new arch instance with computed `Arc` geometry.

**Mirror Building**: Most arches are symmetric. `RhinoArchBase.MirrorBuild()` constructs one side's arcs then mirrors them across the centerline.

**Interaction Flow**: Commands use `InteractionBase.StartCommand()` → option selection → `DrawByStartAndEnd()` or `Get_StartEndApexArch()` for point picking → arch builder function.

### Command Naming

All Rhino commands use the prefix `mz_` (defined in `CommandConstants.cs`). Commands are registered via classes inheriting from `ArchCommandBase`.

### Arch Categories

- **Circular**: Single-center arches (semicircular, segmental)
- **TwoCentered**: Gothic/pointed arches (equilateral, lancet, depressed)
- **ThreeCentered**: Basket handle/elliptical approximations
- **FourCentered**: Tudor, Persian/Keel arches
- **HorseShoe**: Moorish-style arches extending below spring line
