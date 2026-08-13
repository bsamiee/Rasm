# [TS_UI_API_PROSEMIRROR_TRANSFORM]

`prosemirror-transform` owns document change as data: every edit is a `Step` — invertible, JSON-serializable, and rebasable — and `Transform` accumulates steps beside the documents and the `Mapping` they produce. `StepMap`/`Mapping` carry a position across those changes, which is the mechanism collaborative rebasing, decoration survival, and any stored anchor compose through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `prosemirror-transform`
- package: `prosemirror-transform` (MIT)
- module: `type: module`, `sideEffects: false`, one `.` entry with dual `import`/`require` conditions and bundled `.d.ts`/`.d.cts`
- runtime: pure value algebra — no DOM, no globals, so a step applies, inverts, and rebases identically on a server and in a browser
- depends: `prosemirror-model` — steps carry `Slice`, `Mark`, and `NodeType` values and rehydrate through a `Schema`
- rail: `view/content` — the change algebra every editing command emits and every wire carries

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: position mapping — the interface a step map, a mapping, and any foreign carrier satisfy.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :---------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Mappable`  | interface     | `map(pos, assoc?)`, `mapResult(pos, assoc?)` — the position carrier    |
|  [02]   | `MapResult` | class         | `pos` plus `deleted`, `deletedBefore`, `deletedAfter`, `deletedAcross` |

- `assoc` is `-1` or `1` and decides which side an insertion at exactly `pos` lands on; the default `1` pushes the position after inserted content.
- `deleted` reports that the change removed the content around the position, so a stored anchor drops rather than silently sliding.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: position mapping — the carrier a stored position rides across any set of changes.

| [INDEX] | [SURFACE]                                                        | [SHAPE]             | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------- | :------------------ | :------------------------------------------- |
|  [01]   | `new StepMap(ranges, inverted)`                                  | ctor                | one step's replaced-range table              |
|  [02]   | `StepMap.map(pos, assoc)` / `mapResult(pos, assoc)`              | instance            | the position through one step                |
|  [03]   | `StepMap.invert()` / `forEach(f)` / `StepMap.offset(n)`          | instance / static   | inversion, range enumeration, pure-shift map |
|  [04]   | `new Mapping(maps, mirror, from, to)`                            | ctor                | an ordered pipeline of step maps             |
|  [05]   | `Mapping.map(pos, assoc)` / `mapResult(pos, assoc)`              | instance            | the position through the whole pipeline      |
|  [06]   | `Mapping.appendMap(StepMap, mirrors)` / `appendMapping(Mapping)` | instance            | extends the pipeline, recording mirror pairs |
|  [07]   | `Mapping.appendMappingInverted(Mapping)` / `invert()`            | instance            | the undo direction                           |
|  [08]   | `Mapping.slice(from, to)` / `getMirror(n)` / `maps`              | instance / property | sub-pipeline, mirror index, and raw map list |

- `Mapping.from`/`to` bound the active window, so `mapping.slice(mapFrom)` maps a position through only the maps added after a known point — the primitive a rebase loop is written against.
- Every mirror pair marks a map and its inverse; recording it lets a position deleted by one step and restored by its mirror survive the round trip.

[ENTRYPOINT_SCOPE]: the step algebra — the atomic, invertible, serializable change unit.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]           | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :------------------------------------ |
|  [01]   | `Step.apply(doc) -> StepResult`                                              | instance          | applies, reporting failure as a value |
|  [02]   | `Step.invert(doc) -> Step`                                                   | instance          | the undoing step                      |
|  [03]   | `Step.map(Mappable) -> Step \| null`                                         | instance          | rebases the step; `null` where lost   |
|  [04]   | `Step.merge(other) -> Step \| null`                                          | instance          | folds two adjacent steps into one     |
|  [05]   | `Step.getMap() -> StepMap`                                                   | instance          | the position map this step produces   |
|  [06]   | `Step.toJSON()` / `Step.fromJSON(Schema, json)`                              | instance / static | wire round trip by `stepType`         |
|  [07]   | `Step.jsonID(id, stepClass)`                                                 | static            | registers a custom `stepType`         |
|  [08]   | `StepResult.ok(doc)` / `fail(message)` / `fromReplace(doc, from, to, slice)` | static            | the `{doc, failed}` outcome carrier   |

- `Step.apply` returns a `StepResult` whose `failed` holds a message and whose `doc` is `null` on failure; nothing throws, so a rebase loop tests `maybeStep(...).failed`.
- `invert` computes against the pre-step document, which is why `Transform.docs` retains each one.
- `Step.fromJSON` reads `json.stepType` against the registry and throws `RangeError` for an unregistered id — a custom step registers with `jsonID` on both peers before any wire carries it.

[ENTRYPOINT_SCOPE]: the concrete step rows — each registered under its own `stepType` in the JSON discriminant.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `new ReplaceStep(from, to, Slice, structure)`                               | ctor    | `"replace"` — the general content replacement    |
|  [02]   | `new ReplaceAroundStep(from, to, gapFrom, gapTo, Slice, insert, structure)` | ctor    | `"replaceAround"` — wrap/unwrap preserving a gap |
|  [03]   | `new AddMarkStep(from, to, Mark)` / `RemoveMarkStep(...)`                   | ctor    | `"addMark"` / `"removeMark"` on an inline range  |
|  [04]   | `new AddNodeMarkStep(pos, Mark)` / `RemoveNodeMarkStep(...)`                | ctor    | `"addNodeMark"` / `"removeNodeMark"` on one node |
|  [05]   | `new AttrStep(pos, attr, value)`                                            | ctor    | `"attr"` — one node attribute                    |
|  [06]   | `new DocAttrStep(attr, value)`                                              | ctor    | `"docAttr"` — one top-level document attribute   |
|  [07]   | `replaceStep(doc, from, to, Slice) -> Step \| null`                         | static  | derives the fitting step, or `null`              |

- `ReplaceStep.MAP_BIAS` is the static `-1 | 1` deciding which side positions map to across a replacement.
- `AttrStep` and `DocAttrStep` declare their own `fromJSON`; the rest rehydrate through the base `Step.fromJSON` registry.
- `structure: true` marks a step that changes structure rather than content, which makes `map` refuse to rebase it across a conflicting structural change.

[ENTRYPOINT_SCOPE]: the transform builder — chained document edits accumulating steps, documents, and one mapping.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]             | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------ | :------------------ | :------------------------------- |
|  [01]   | `new Transform(doc)` / `doc` / `before` / `steps` / `docs` / `mapping`          | ctor / property     | accumulator and retained state   |
|  [02]   | `Transform.step(Step)` / `maybeStep(Step) -> StepResult`                        | instance            | throws / reports as a value      |
|  [03]   | `Transform.docChanged` / `changedRange()`                                       | property / instance | change flag and touched range    |
|  [04]   | `Transform.replace(from, to, Slice)` / `replaceWith(from, to, content)`         | instance            | schema-fitted replacement        |
|  [05]   | `Transform.delete(from, to)` / `insert(pos, content)`                           | instance            | shorthands over `replace`        |
|  [06]   | `Transform.replaceRange(from, to, Slice)` / `replaceRangeWith(from, to, Node)`  | instance            | range variants that widen to fit |
|  [07]   | `Transform.deleteRange(from, to)`                                               | instance            | deletion widened to parent nodes |
|  [08]   | `Transform.lift(NodeRange, target)` / `wrap(NodeRange, wrappers)`               | instance            | structural edits over a range    |
|  [09]   | `Transform.join(pos, depth)` / `split(pos, depth, typesAfter)`                  | instance            | boundary removal and insertion   |
|  [10]   | `Transform.setBlockType(from, to, NodeType, attrs)`                             | instance            | retypes textblocks in range      |
|  [11]   | `Transform.setNodeMarkup(pos, type, attrs, marks)`                              | instance            | replaces type, attrs, and marks  |
|  [12]   | `Transform.setNodeAttribute(pos, attr, value)` / `setDocAttribute(attr, value)` | instance            | single-attribute edits           |
|  [13]   | `Transform.addNodeMark(pos, Mark)` / `removeNodeMark(pos, Mark \| MarkType)`    | instance            | node-level mark edits            |
|  [14]   | `Transform.addMark(from, to, Mark)` / `removeMark(from, to, mark)`              | instance            | inline mark edits over a range   |
|  [15]   | `Transform.clearIncompatible(pos, NodeType, ContentMatch)`                      | instance            | strips what the target rejects   |

- `Transform.removeMark` takes `Mark | MarkType | null`; `null` strips every mark in the range.
- Every `Transform` mutator returns `this`, so one transform chains an arbitrary edit and dispatches once.
- `Transform.step` throws on a failed step while `maybeStep` returns the `StepResult`; a rebase or speculative edit uses `maybeStep`.
- `Transform.docs` grows one entry per step and is what `Step.invert(doc)` reads for undo.

[ENTRYPOINT_SCOPE]: structural queries — admissibility answered before a step is built.

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `liftTarget(NodeRange) -> number \| null`                      | static  | the depth a range lifts to, or `null`        |
|  [02]   | `findWrapping(NodeRange, NodeType, attrs, innerRange)`         | static  | the `{type, attrs}` chain a wrap needs       |
|  [03]   | `canSplit(doc, pos, depth, typesAfter)` / `canJoin(doc, pos)`  | static  | split and join admissibility                 |
|  [04]   | `joinPoint(doc, pos, dir)` / `insertPoint(doc, pos, NodeType)` | static  | the nearest valid join or insertion position |
|  [05]   | `dropPoint(doc, pos, Slice)`                                   | static  | the position a dropped slice lands at        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each `Step` is the atomic unit of change and carries four guarantees: `apply` produces a new document or a typed failure, `invert(doc)` produces the exact undo, `map(mapping)` rebases it onto a changed document or returns `null`, and `toJSON`/`fromJSON(schema, json)` round-trips it as plain data. Every higher operation — a command, an input rule, a collaborative send — is a list of these.
- Position mapping is the whole composition mechanism: each step yields a `StepMap`, a `Transform` concatenates them into one `Mapping`, and `mapping.map(pos, assoc)` carries any stored position forward. Decoration sets, stored anchors, selections, and rebased remote steps all move by the same call, which is why an anchor needs no bespoke carry rule.
- Mirrors make undo and rebase exact: `appendMap(map, mirrors)` records that a map inverts an earlier one, so a position deleted and restored across the pair resolves to its original place instead of collapsing.
- Rebasing is invert-apply-remap: undo the local steps in reverse, apply the remote ones, then map each local step through the accumulated mapping slice and re-apply the ones that survive. `Mapping.slice(from)`, `Step.map`, and `maybeStep` are exactly the primitives that loop needs.
- Failure is a value, never an exception: `StepResult.failed` carries the message, `maybeStep` surfaces it, and `Step.map` returns `null` where the target vanished — so speculative and remote work never needs a try block.
- `Transform` mutators fit against the schema before emitting steps: `replace` derives a fitting slice, `wrap` consumes the chain `findWrapping` returns, and `clearIncompatible` strips content a target type rejects — none of them produce a document `Node.check()` rejects.

[STACKING]:
- `prosemirror-model`(`.api/prosemirror-model.md`): steps carry `Slice`, `Mark`, `NodeType`, and `Attrs` values; `Transform.wrap` takes the `NodeRange` `ResolvedPos.blockRange` mints, `clearIncompatible` takes a `ContentMatch` from `NodeType.contentMatch`, and `Step.fromJSON(schema, json)` is the sole path from wire data back to a step.
- `prosemirror-state`(`.api/prosemirror-state.md`): `Transaction extends Transform`, so `tr.steps`, `tr.docs`, and `tr.mapping` are these fields; a plugin field carries its own positions across an edit with `tr.mapping.map(pos)` inside `StateField.apply`, gated on `tr.docChanged`.
- `prosemirror-collab`(`.api/prosemirror-collab.md`): `sendableSteps(state)` hands out `readonly Step[]`, each serialized with `toJSON`, and `receiveTransaction(state, steps, clientIDs)` rebases the unconfirmed local steps over them using exactly the invert-apply-remap loop; the authority stores steps, never documents.
- `prosemirror-view`(`.api/prosemirror-view.md`): `DecorationSet.map(mapping, doc)` consumes a `Mapping` directly, so a decoration survives both local and remote changes with no bespoke logic.
- `prosemirror-commands`(`.api/prosemirror-commands.md`) + `prosemirror-schema-list`(`.api/prosemirror-schema-list.md`): both build on `liftTarget`, `findWrapping`, `canSplit`, and `canJoin` to answer admissibility, which is how a command's query arm returns `false` without touching the document.
- `core/interchange/codec`: `Wire` owns the branch's closed wire vocabulary; an editor change crosses as a step-JSON payload on one family row, so document edits ride the same fault classification, quarantine, and parity machinery as every other message.
- within-lib `view/content`: the editor page composes structural edits as `Transform` chains on `state.tr`, asks `canSplit`/`findWrapping`/`liftTarget` before offering an action, and carries every stored editor anchor with `tr.mapping.map` rather than recomputing it.

[LOCAL_ADMISSION]:
- Express every document change as steps on one `Transform` chain and dispatch once; a document rebuilt by hand loses invertibility, mapping, and the wire form together.
- Carry any stored position across a change with `tr.mapping.map(pos, assoc)` and honour `MapResult.deleted`; a recomputed anchor is a torn read.
- Test admissibility with `canSplit`, `canJoin`, `liftTarget`, `findWrapping`, or `insertPoint` before building the step, so a command's query arm answers without mutating.
- Register any custom step with `Step.jsonID` on every peer before it crosses a wire.

[RAIL_LAW]:
- Package: `prosemirror-transform`
- Owns: the change algebra — the abstract `Step` contract (`apply`, `invert`, `map`, `merge`, `getMap`, `toJSON`/`fromJSON`, `jsonID`) and its registered rows `ReplaceStep`, `ReplaceAroundStep`, `AddMarkStep`, `RemoveMarkStep`, `AddNodeMarkStep`, `RemoveNodeMarkStep`, `AttrStep`, `DocAttrStep`; the `StepMap`/`Mapping`/`MapResult` position-mapping mechanism with its mirror pairs; the `Transform` builder with every schema-fitting mutator; and the structural queries `liftTarget`, `findWrapping`, `canSplit`, `canJoin`, `joinPoint`, `insertPoint`, `dropPoint`, `replaceStep`
- Accept: one chained `Transform` per user-visible change, `maybeStep` for speculative and rebased work, `Mapping.slice` plus `Step.map` for a rebase loop, `mapping.map(pos, assoc)` with a `deleted` guard for every stored anchor, admissibility queries before construction, and `Step.toJSON`/`fromJSON(schema, json)` as the only wire form
- Reject: a document rebuilt by hand where steps express the change, a recomputed position where a `Mapping` carries it, `Transform.step` on a step whose success is unproven, a mutation attempted before its admissibility query, an unregistered custom step on a wire, and a try/catch wrapper around a surface that already returns typed failure
