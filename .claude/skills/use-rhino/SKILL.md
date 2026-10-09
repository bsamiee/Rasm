---
name: use-rhino
description: "Use when a task drives a Rhino document, a .3dm file, or a Grasshopper 2 canvas, covering slots, orientation, layers, selection, commands, files, views, materials, settings, and definitions."
---

# [USE_RHINO]

This skill provides guidance for properly and effectively working in Rhino and Grasshopper using the `rhino-mcp-platform` and `rhinocommon` cli

## [01]-[SHARED_PROCESS]

All calls leave the application in the background, never brought to the foreground. Only start the application with approved options below, depending on task:
1a. New work: use `spawn_slot` to start a new Rhino process, skips splash and uses default template
1b. Existing work: `open -g <file>` opens the file in Rhino
2. !`dotnet run "$(git rev-parse --show-toplevel)/.claude/skills/use-rhino/scripts/Instances.cs"`

- One Rhino application holds every open document, each `spawn_slot` adds a document window to it
- Grasshopper 2 keeps definitions as tabs of one editor: `g2_start`, then `Editor.Instance.Documents.Queue(Document.NewActiveDocument(), null)`

## [02]-[SCRIPTS]

- Code Rhino runs is C# 10 through `run_csharp`, one call per application (one main-thread queue), `#r "<assembly>"` below line 1

