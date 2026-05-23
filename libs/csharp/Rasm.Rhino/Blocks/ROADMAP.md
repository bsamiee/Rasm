# [H1][RASM_RHINO_BLOCKS_ROADMAP]
>**Dictum:** *Blocks are authored geometry, document state, and archive identity in one rail.*

<br>

[IMPORTANT] `Blocks` owns Rhino block definitions and instances as a first-class `Rasm.Rhino` concern. It lets downstream plugin/app code author block members from `Construction` output or duplicated document objects, attach metadata, create or update definitions, place instances, inspect dependencies, manage links, and receive receipts without sequencing RhinoCommon table calls.

`Blocks` is not an `InstanceDefinitionTable` facade. It is the category owner for block lifecycle, placement, replacement, linked/archive behavior, dependency graph, selected block parts, events, cleanup, preview/export, and block receipts. Native RhinoCommon remains compile/runtime truth; Rasm adds typed intent, validation, batching, policy, audit, and central ownership.

---
## [1][SOURCE_TRUTH]
>**Dictum:** *Block claims must trace to local RhinoWIP evidence.*

<br>

| [INDEX] | [SOURCE] | [STATUS] | [USE] |
| :-----: | -------- | -------- | ----- |
| **[1]** | `scripts/rhino.sh api doctor` | Verified RhinoWIP `9.0.26132.12306`. | Host version and XML/decompile availability. |
| **[2]** | `RhinoCommon.xml` | Primary. | `RhinoDoc`, `InstanceDefinition`, `InstanceObject`, `InstanceDefinitionTable`, `File3dm` APIs. |
| **[3]** | `scripts/rhino.sh api decompile rhino-common <type>` | Required for ambiguity. | Sentinels, obsolete overloads, hidden/internal members. |
| **[4]** | `Rasm.Rhino` source | Required. | Existing receipts, selection, exchange graph, and mutation rails. |

[VERIFY] Re-run source checks before implementation. Every named Rhino member requires local XML or decompile proof before code claims capability.

---
## [2][API_MAP]
>**Dictum:** *Native capability enters through verified block semantics.*

<br>

| [INDEX] | [CONCERN] | [RHINO WIP SURFACE] | [BLOCK RAIL IMPLICATION] |
| :-----: | --------- | ------------------- | ------------------------ |
| **[1]** | Live table | `RhinoDoc.InstanceDefinitions`, `InstanceDefinitionTable`, `Count`, `ActiveCount`, indexer. | `Count` includes deleted/purged slots; `ActiveCount` excludes deleted; indexer throws out of range, returns deleted definitions with `IsDeleted`, and can return `null` for purged slots. |
| **[2]** | Definition identity | `InstanceDefinition : InstanceDefinitionGeometry`, inherited `ModelComponent`, `DeletedName`. | Snapshot `Id`, `Index`, `Name`, deleted/status/name state, description, URL fields, object ids, unit system, and source archive. |
| **[3]** | Metadata | User strings, `UserDictionary`, `Modify(int, UserData, bool)`, `IsValidComponentName`. | Separate simple user text from custom user data; validate metadata once before native mutation. |
| **[4]** | Lookup/name allocation | `Find(string)`, `Find(Guid, ignoreDeleted)`, `InstanceDefinitionIndex(Guid, ignoreDeleted)`, `GetList`, `GetUnusedInstanceDefinitionName`. | Use `Option` for absence; mark `Find(string, bool)` and suffix name allocation overloads obsolete/redundant. |
| **[5]** | Create/update/delete | `Add`, `Modify`, `ModifyGeometry`, `UndoModify`, `Delete(deleteReferences, quiet)`, `Purge`, `PurgeUnused`, `Undelete`, `Compact(bool ignoreUndoReferences)`. | Convert `-1`, `false`, null, and exceptions into `Fin`; `PurgeUnused()` returns count and `Compact(bool ignoreUndoReferences)` returns no count. |
| **[6]** | Base point | `InstanceDefinitionTable.Add(..., Point3d basePoint, ...)`. | Treat base point as creation input; no durable public definition base-point snapshot is assumed. |
| **[7]** | Placement | `ObjectTable.AddInstanceObject(index, Transform[, ObjectAttributes[, HistoryRecord, bool]])`. | `Guid.Empty` means failure; place one or many instances from definition snapshots. |
| **[8]** | Replacement | `ObjectTable.ReplaceInstanceObject(Guid|ObjRef, int)`. | Native returns `bool`; map success to replacement receipt for target instance ids without inventing created/deleted ids. |
| **[9]** | Transform/history | `ObjectTable.Transform(..., deleteOriginal)`, `TransformWithHistory`. | Distinguish copy-vs-delete transform from history copy; history mode creates a transformed copy, leaves the original, and records history only when history recording is enabled. |
| **[10]** | Instance inspection | `InstanceObject.InstanceXform`, `InsertionPoint`, `InstanceDefinition`, `UsesDefinition`, `SubObjectFromComponentIndex`. | Capture placement transform, insertion, selected member context, and reconstructed definition parts. |
| **[11]** | Explode/flatten | `InstanceObject.Explode(...)`, `ObjectTable.AddExplodedInstancePieces(...)`, linked `LayerStyle.Active`. | `Explode` inspects pieces; `AddExplodedInstancePieces` mutates docs; linked flattening is not geometric explode. |
| **[12]** | Dependencies | `UseCount`, `GetReferences(0|1|2)`, `GetContainers`, `UsesDefinition`, `InUse`, `UsesLayer`, `UsesLinetype`, `ObjectAttributes.IsInstanceDefinitionObject`, `RhinoObject.IsInstanceDefinitionGeometry`. | Graph must distinguish definition members, top-level inserts, nested inserts, containers, and resource blockers. |
| **[13]** | Linked lifecycle | `RhinoDoc.LinkedInstanceDefinitionUpdate`, `FileReference.CreateFromFullAndRelativePaths`, `ModifySourceArchive(int, FileReference, InstanceDefinitionUpdateType, bool)`, `RefreshLinkedBlock`, `UpdateLinkedInstanceDefinition`, `DestroySourceArchive`. | Keep source set, refresh, reload, detach, document update policy, and archive status as distinct operations; `FileReference` is disposable, full path null can throw, and detach converts source state to `Static`. |
| **[14]** | Linked modes | `InstanceDefinitionUpdateType.Static`, `Embedded`, `LinkedAndEmbedded`, `Linked`; `InstanceDefinitionLayerStyle.None`, `Active`, `Reference`; `ArchiveFileStatus`. | Validate update type, layer style, and archive status through native state; `None` applies to static/linked-and-embedded, and `Reference` is linked default. |
| **[15]** | Archive definitions | `File3dmInstanceDefinitionTable`, `AddLinked`, `InstanceDefinitionGeometry`, `SourceArchive`, `GetObjectIds`. | File3dm exposes source archive and definition geometry; live link state belongs to open `RhinoDoc`. |
| **[16]** | Archive instances | `InstanceReferenceGeometry(Guid, Transform)`, `File3dmObjectTable.AddInstanceObject(int, Transform[, ObjectAttributes])`, `AddInstanceObject(InstanceReferenceGeometry[, ObjectAttributes])`. | Offline placement uses definition id or table index and never live document receipts. |
| **[17]** | Selection parts | `ObjRef.InstanceDefinitionPart`, `ComponentIndexType.InstanceDefinitionPart`. | `Commands/Selection` owns `ObjRef`; Blocks consumes snapshots and projects block-member context. |
| **[18]** | Events | `InstanceDefinitionTableEventArgs.EventType`, `InstanceDefinitionIndex`, `NewState`, `OldState`; event types `Added`, `Deleted`, `Undeleted`, `Modified`, `Sorted`. | Future watcher snapshots values during callback and owns scoped unsubscribe. |
| **[19]** | Preview/export/text fields | `CreatePreviewBitmap`, `InstanceDefinitionTable.Export`, `TextFields.BlockName`, `BlockInstanceCount`, `BlockDescription`, `BlockInsertionCoordinate`, `BlockAttributeText`, `GetInstanceAttributeFields`. | Preview returns nullable disposable `System.Drawing.Bitmap`; export and text-field audit belong to block management. |

[CRITICAL] `Rhino.DocObjects.ObjectInstance` was not found in local RhinoWIP. Use `InstanceObject`.

---
## [3][TARGET_SHAPE]
>**Dictum:** *One category owner subsumes table calls and block ceremony.*

<br>

### [3.1][PUBLIC_RAIL]

`RhinoBlocks` is the public owner for live and archive block operations. It receives `RhinoDoc` and `Context` where required, uses the same document/UI boundary discipline as `DocumentEdit`, and exposes one `Run<T>` rail over a Thinktecture `BlockOp` union.

Operation families:
- Author: create from `Construction` output, clone existing objects, validate members/attributes, create-or-update, modify metadata, modify geometry.
- Manage: lookup, allocate names, rename, delete, undelete, purge, compact, snapshot, audit, cleanup.
- Place: place one/many, replace instance definition, transform, copy, history-copy, explode, linked flatten.
- Link: create archive links, mutate source archive, refresh, reload geometry, detach source archive, inspect linked status.
- Graph: inspect members, inserts, selected parts, references, containers, resource blockers, archive edges.
- Observe: scoped table events, preview bitmap, export, text-field/attribute summaries.

---
### [3.2][CREATION_FLOW]

| [INDEX] | [STEP] | [OWNER] | [RESULT] |
| :-----: | ------ | ------- | -------- |
| **[1]** | Project member geometry. | `Construction.Project<TOut>`. | Block-ready owned geometry plus optional attributes. |
| **[2]** | Admit existing objects. | `Blocks` author policy. | Duplicated geometry and duplicated attributes; definition members are excluded unless explicitly cloned. |
| **[3]** | Validate definition intent. | Thinktecture value objects and smart enums. | Name, conflict policy, base point, URL, user strings/data, source/link policy. |
| **[4]** | Create or update definition. | `InstanceDefinitionTable.Add` or `ModifyGeometry`. | Definition index and `BlockSnapshot`; modify geometry is treated as definition-wide cascade. |
| **[5]** | Place or replace instances. | `ObjectTable.AddInstanceObject` or `ReplaceInstanceObject`. | Created or replaced instance ids with grouped `DocumentReceipt`. |

The rail lets downstream code generate geometry once, capture it as a block definition, place it repeatedly, and manage later block lifecycle without direct table plumbing.

---
### [3.3][VALUE_ADD]

| [INDEX] | [CAPABILITY] | [VALUE] |
| :-----: | ------------ | ------- |
| **[1]** | Author/update | Construct members, duplicate attrs, validate metadata, and create-or-update by name or id. |
| **[2]** | Identity/idempotency | Allocate names, resolve conflicts, preserve ids/indexes, and report deltas. |
| **[3]** | Placement/mutation | Batch transforms, replace definitions, model copy-vs-delete transform, and preserve history-copy semantics. |
| **[4]** | Explode/flatten | Separate inspection explode, document mutation explode, and linked layer-style flattening. |
| **[5]** | Link/archive lifecycle | Refresh, reload, detach, detect stale/missing archives, and bridge offline File3dm graph work. |
| **[6]** | Graph/audit/events | Track members, inserts, nesting, containers, resource blockers, scoped events, and table snapshots. |
| **[7]** | Preview/export/text | Own bitmap preview/export summaries; expose block text fields as annotation metadata. |
| **[8]** | Cleanup/state | Purge unused, compact, delete/undelete, and return typed diagnostics for partial native success. |

---
### [3.4][FILE_ARCHITECTURE]

| [INDEX] | [FILE] | [OWNERSHIP] |
| :-----: | ------ | ----------- |
| **[1]** | `Blocks.cs` | `RhinoBlocks` public owner, runtime boundary, and one `Run<T>` rail. |
| **[2]** | `Operations.cs` | `BlockOp` union and dispatch for author, manage, place, link, graph, observe, audit, and export operations. |
| **[3]** | `State.cs` | Specs, policies, snapshots, graph records, audit records, receipts, and diagnostics. |
| **[4]** | `Archive.cs` | Offline `File3dm` linked definition/instance graph and Exchange migration support. |

`Events.cs` is deferred until scoped watcher lifetime becomes a durable multi-caller concern. Do not add separate metadata, link, graph, preview, purge, selection, or replacement files before durable ownership pressure exists.

---
### [3.5][EXTERNAL_STACK]

| [INDEX] | [OWNER] | [USE] |
| :-----: | ------- | ----- |
| **[1]** | RhinoCommon | Tables, mutation, source archives, events, preview/export, and document truth. |
| **[2]** | `Construction` | Block-ready geometry and member attributes; Blocks never recreates shape factories. |
| **[3]** | LanguageExt | `Option`, `Fin`, `Validation`, `Seq`, traversal, and terminal boundary collapse. |
| **[4]** | Thinktecture | `BlockOp`, names, ids, paths, conflict/update/link policies, and generated dispatch. |
| **[5]** | System/BCL | `StringComparison`, `FrozenDictionary`, lexical policy, and bitmap disposal discipline only where proven. |

---
## [4][CENTRALIZATION]
>**Dictum:** *Block logic belongs in one owner, with existing receipts preserved.*

<br>

| [INDEX] | [CURRENT LOCATION] | [MOVE OR PRESERVE] | [REASON] |
| :-----: | ------------------ | ------------------ | -------- |
| **[1]** | `Exchange/State.cs` `ArchiveUpdate.LinkBlocks`. | Move into `BlockOp.CreateArchiveLinks` after parity. | Linked block mutation is block lifecycle, not generic archive patching. |
| **[2]** | `Exchange/Archive.cs` linked graph rows. | Move into `Blocks/Archive.cs`; `Exchange` consumes `BlockGraph`. | Block graph owns member, insert, linked archive, and instance edges. |
| **[3]** | `Exchange/State.cs` `FileSheetEdit.RefreshLinks`. | Move to `BlockOp.RefreshLinks` after receipt parity. | Refresh is linked block lifecycle, not sheet editing. |
| **[4]** | `Exchange/State.cs` `FileSheetEdit.FlattenLinkedBlocks`. | Move to `BlockOp.FlattenLinked` after behavior parity. | Flatten mutates block layer style and linked state. |
| **[5]** | `Exchange/State.cs` `FileSheetEdit.ExportBlock`. | Move to `BlockOp.Export` after export receipt parity. | `InstanceDefinitionTable.Export` is block export. |
| **[6]** | `Commands/Selection.cs` `InstanceDefinitionPart<T>`. | Preserve; Blocks consumes snapshots. | Selection owns `ObjRef` lifetime and pick context. |
| **[7]** | `Commands/Document.cs` `DocumentResourceKind.Block`, `DocumentReceipt`. | Preserve. | Shared mutation receipts stay generic. |
| **[8]** | `UI/Overlay.cs` display pipeline drawing. | Preserve. | UI owns live drawing; Blocks owns preview bitmap/export snapshots. |

[IMPORTANT] Replacement is removal-clean: migrate ownership, update callers, preserve receipts, then delete stale `FileSheetEdit` block cases.

---
## [5][VALIDATION]
>**Dictum:** *Block implementation proves native truth before claiming capability.*

<br>

Docs-only refinement gate:
- `bash scripts/rhino.sh api doctor`.
- Targeted `bash scripts/rhino.sh api xml rhino-common "<symbol>"` for every newly named Rhino member.
- `bash scripts/rhino.sh api decompile rhino-common "<type>"` where XML is missing or ambiguous.
- `git diff --check -- libs/csharp/Rasm.Rhino/Blocks/ROADMAP.md`.

Future implementation gate:
- `dotnet build libs/csharp/Rasm.Rhino/Rasm.Rhino.csproj --no-restore`.
- `bash scripts/check-cs.sh check`.

Runtime Rhino verification is required only when behavior claims depend on mutation inside a running RhinoWIP session.
