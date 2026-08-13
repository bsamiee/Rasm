# [TS_UI_API_PROSEMIRROR_MODEL]

`prosemirror-model` owns the document algebra: one `Schema` compiles `NodeSpec`/`MarkSpec` data rows and their content expressions into `NodeType`/`MarkType` owners, and every `Node`, `Fragment`, `Slice`, and `Mark` under it is immutable, position-indexed, and schema-checked. `DOMParser`/`DOMSerializer` derive both DOM directions from those same spec rows, and `toJSON`/`fromJSON(schema, json)` fix one canonical wire shape for the whole family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `prosemirror-model`
- package: `prosemirror-model` (MIT)
- module: `type: module`, `sideEffects: false`, one `.` entry with dual `import`/`require` conditions and bundled `.d.ts`/`.d.cts`
- runtime: pure data — DOM-free except `DOMParser`/`DOMSerializer`, which take an explicit `document`, so document values build and serialize to JSON on a server with no browser global
- depends: `orderedmap` — the insertion-ordered map `SchemaSpec.nodes`/`marks` accept and `Schema.spec` normalizes to
- rail: `view/content` — the schema and document-value floor every other ProseMirror package types against

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema declaration — plain data rows, never subclasses; `new Schema(spec)` compiles them into the runtime owners.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `SchemaSpec<Nodes, Marks>` | interface     | `{nodes, marks?, topNode?}`; string unions key `Schema.nodes`/`marks` |
|  [02]   | `NodeSpec`                 | interface     | one node row; open index signature carries foreign keys               |
|  [03]   | `MarkSpec`                 | interface     | one mark row; `inclusive`, `excludes`, `spanning`, `code`             |
|  [04]   | `AttributeSpec`            | interface     | `{default?, validate?}` per attribute                                 |
|  [05]   | `Attrs`                    | type alias    | `{readonly [attr: string]: any}` — frozen attribute bag               |
|  [06]   | `DOMOutputSpec`            | union         | `Element` \| `{dom, contentDOM?}` \| `[tag, ...args]` array form      |

- `NodeSpec` structure keys: `content`, `marks`, `group`, `inline`, `atom`, `attrs`, `selectable`, `draggable`, `code`, `whitespace`, `defining`, `definingAsContext`, `definingForContent`, `isolating`, `linebreakReplacement`.
- `NodeSpec` DOM keys: `toDOM(node)`, `parseDOM: readonly TagParseRule[]`, `leafText(node)`, `toDebugString(node)`.
- `AttributeSpec.validate` is a type-expression string (`"string|null"`) or a thrower; an attribute without `default` is required at every `create` call and makes `NodeType.hasRequiredAttrs()` true.

[PUBLIC_TYPE_SCOPE]: parse rules — the DOM-side half of a spec row, split by matched subject.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `GenericParseRule` | interface     | shared keys: `priority`, `consuming`, `context`, `mark`, `ignore`        |
|  [02]   | `TagParseRule`     | interface     | `tag` + `node`/`getAttrs(HTMLElement)`/`contentElement`/`getContent`     |
|  [03]   | `StyleParseRule`   | interface     | `style` + `getAttrs(string)`/`clearMark(mark)`; `tag` forbidden          |
|  [04]   | `ParseRule`        | union         | `TagParseRule \| StyleParseRule` — the discriminant is `tag`             |
|  [05]   | `ParseOptions`     | interface     | `preserveWhitespace`, `findPositions`, `from`/`to`, `topNode`, `context` |

- `NodeSpec.parseDOM` narrows to `TagParseRule[]`; only `MarkSpec.parseDOM` admits the full `ParseRule` union, so a style-matched rule is a mark rule.
- `getAttrs` returning `false` rejects the match and lets the next rule by `priority` try; returning `null` accepts with defaults.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the schema owner and the type owners it compiles — the only construction path for every document value.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new Schema(SchemaSpec)`                                                     | ctor     | compiles specs, resolves content, links marks |
|  [02]   | `Schema.node(string \| NodeType, Attrs, Fragment \| Node \| Node[], Mark[])` | instance | builds one node through its type              |
|  [03]   | `Schema.text(string, Mark[])`                                                | instance | builds a text node; empty text throws         |
|  [04]   | `Schema.mark(string \| MarkType, Attrs)`                                     | instance | builds one mark                               |
|  [05]   | `Schema.nodeFromJSON(any)` / `markFromJSON`                                  | property | bound field functions, safe as callbacks      |
|  [06]   | `Schema.nodes` / `Schema.marks` / `topNodeType`                              | property | name-keyed type maps and the doc type         |
|  [07]   | `NodeType.create(Attrs, content, Mark[])`                                    | instance | builds without validating content             |
|  [08]   | `NodeType.createChecked(...)` / `createAndFill(...)`                         | instance | validates content / fills required children   |
|  [09]   | `NodeType.validContent(Fragment)` / `allowsMarkType(MarkType)`               | instance | content and mark admissibility queries        |
|  [10]   | `MarkType.create(Attrs)` / `isInSet(Mark[])` / `excludes(MarkType)`          | instance | mark construction and set queries             |

- `Schema.nodes`/`marks` intersect the mapped `Nodes`/`Marks` string union with an open `[key: string]: NodeType` index, so an unlisted name compiles and fails at runtime — type the schema's unions to recover the compile error.
- `NodeType.create` skips content validation; `createChecked` throws `RangeError` and `createAndFill` returns `null` where no valid filling exists.
- `Schema.cached` is a `{[key: string]: any}` scratch map the ecosystem keys by package name; it is the one sanctioned per-schema memo slot.

[ENTRYPOINT_SCOPE]: document values — immutable trees, their fragments, slices, and marks.

| [INDEX] | [SURFACE]                                                              | [SHAPE]           | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :------------------------------------------ |
|  [01]   | `Node.nodeSize` / `childCount` / `textContent`                         | property          | position arithmetic and flattened text      |
|  [02]   | `Node.child(number)` / `maybeChild(number)` / `forEach(f)`             | instance          | direct child access                         |
|  [03]   | `Node.nodesBetween(from, to, f)` / `descendants(f)`                    | instance          | ranged and full traversal; `false` prunes   |
|  [04]   | `Node.resolve(number) -> ResolvedPos`                                  | instance          | the only path-resolution entry, cached      |
|  [05]   | `Node.slice(from, to, includeParents)` / `cut(from, to)`               | instance          | open-ended slice / structurally closed cut  |
|  [06]   | `Node.replace(from, to, Slice)` / `copy(Fragment)`                     | instance          | immutable replace; throws `ReplaceError`    |
|  [07]   | `Node.canReplace(...)` / `canReplaceWith(...)` / `canAppend(Node)`     | instance          | content-match queries before an edit        |
|  [08]   | `Node.check()`                                                         | instance          | throws `RangeError` on any schema violation |
|  [09]   | `Node.toJSON()` / `Node.fromJSON(Schema, any)`                         | instance / static | the canonical wire round trip               |
|  [10]   | `Fragment.from(...)` / `fromArray(...)` / `fromJSON(Schema, any)`      | static            | child-list construction                     |
|  [11]   | `Fragment.append` / `cut` / `addToStart` / `addToEnd` / `replaceChild` | instance          | persistent child-list edits                 |
|  [12]   | `Fragment.findDiffStart(other)` / `findDiffEnd(other)`                 | instance          | diff positions for incremental redraw       |
|  [13]   | `new Slice(Fragment, openStart, openEnd)` / `Slice.maxOpen(Fragment)`  | ctor / static     | open-depth clipboard/replacement payload    |
|  [14]   | `Mark.addToSet(Mark[])` / `removeFromSet(Mark[])` / `isInSet(Mark[])`  | instance          | the mark-set algebra                        |
|  [15]   | `Mark.sameSet(a, b)` / `Mark.setFrom(marks)` / `Mark.none`             | static            | set comparison, coercion, and the empty set |

- `Fragment.empty` is the shared empty-fragment value every construction path returns for no children.
- `Node.check` returns `void` and reports by throwing — a boolean-shaped call site inverts its meaning.
- `Node.toJSON` emits `{type}`, adding `attrs`, `content`, and `marks` only when non-empty, and a text node adds `text`; `Fragment.toJSON` returns `null` for an empty fragment and `Slice.toJSON` omits zero `openStart`/`openEnd`.
- `Node.slice` returns open ends for clipboard and transform use while `cut` returns a closed `Node`; passing a `cut` result where a `Slice` belongs silently drops the open depth.

[ENTRYPOINT_SCOPE]: positions and content matching — the coordinate system every command and step reads.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]             | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------------------------------------- | :------------------ | :---------------------------- |
|  [01]   | `ResolvedPos.parent` / `depth` / `parentOffset` / `pos`                           | property            | resolved path and offset      |
|  [02]   | `ResolvedPos.node(depth)` / `index(depth)` / `start(depth)` / `end(depth)`        | instance            | ancestor access by `depth`    |
|  [03]   | `ResolvedPos.before(depth)` / `after(depth)` / `posAtIndex(index, depth)`         | instance            | ancestor boundary positions   |
|  [04]   | `ResolvedPos.nodeAfter` / `nodeBefore` / `textOffset`                             | property            | immediate neighbours          |
|  [05]   | `ResolvedPos.marks()` / `marksAcross(ResolvedPos)`                                | instance            | inherited mark set            |
|  [06]   | `ResolvedPos.blockRange(other, pred)` / `sharedDepth(pos)` / `sameParent(other)`  | instance            | mints a `NodeRange`           |
|  [07]   | `new NodeRange($from, $to, depth)`                                                | ctor                | range bounds and indices      |
|  [08]   | `ContentMatch.matchType(NodeType)` / `matchFragment(Fragment)`                    | instance            | walks the content automaton   |
|  [09]   | `ContentMatch.fillBefore(Fragment, toEnd, startIndex)` / `findWrapping(NodeType)` | instance            | filler nodes / wrapping chain |
|  [10]   | `ContentMatch.validEnd` / `defaultType` / `edgeCount` / `edge(n)`                 | property / instance | acceptance state and edges    |

- `ResolvedPos.node`, `index`, `start`, and `end` count a negative `depth` inward from the position.
- `NodeRange` carries `start`, `end`, `parent`, `startIndex`, and `endIndex`; `ResolvedPos.blockRange` mints the value lift, wrap, and the list operations take.
- `ContentMatch.edge(n)` returns the outgoing `{type, next}` pairs of the state `edgeCount` sizes.

[ENTRYPOINT_SCOPE]: the DOM boundary — both directions derive from the same spec rows.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `DOMParser.fromSchema(Schema)`                                      | static   | derives rules from every `parseDOM`, cached        |
|  [02]   | `new DOMParser(Schema, ParseRule[])`                                | ctor     | hand-ordered rules for a foreign HTML dialect      |
|  [03]   | `DOMParser.parse(DOMNode, ParseOptions)`                            | instance | parses to a complete `Node` under `topNode`        |
|  [04]   | `DOMParser.parseSlice(DOMNode, ParseOptions)`                       | instance | parses to an open `Slice` for paste                |
|  [05]   | `DOMSerializer.fromSchema(Schema)`                                  | static   | derives serializers from every `toDOM`             |
|  [06]   | `DOMSerializer.serializeFragment(Fragment, {document}, target)`     | instance | renders children into a `DocumentFragment`         |
|  [07]   | `DOMSerializer.serializeNode(Node, {document})`                     | instance | renders one node subtree                           |
|  [08]   | `DOMSerializer.renderSpec(Document, DOMOutputSpec)`                 | static   | expands one array spec to `{dom, contentDOM?}`     |
|  [09]   | `DOMSerializer.nodesFromSchema(Schema)` / `marksFromSchema(Schema)` | static   | the raw spec-function maps, for a partial override |

- `serializeFragment`/`serializeNode` take `{document}` and default to the ambient `document`; pass an explicit one to render outside a browser.
- `renderSpec` reads `0` inside an array spec as the content hole — exactly one hole per spec, and a leaf or atom node carries none.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Schema` compiles data once: `new Schema({nodes, marks})` accepts plain objects or `OrderedMap`s, resolves every `content` expression into a `ContentMatch` automaton on each `NodeType`, links `marks`/`excludes` groups, and freezes the result — a document node carries its `NodeType` by reference, so a value built under one schema is unusable under another and every `fromJSON` names the schema that admits it.
- Content expressions are the published grammar, not free text: names, group names, `|` choice, `*`/`+`/`?` repetition, `{n,m}` counts, and sequence, evaluated left to right into the automaton `matchType`, `matchFragment`, `fillBefore`, and `findWrapping` walk. Editing the grammar re-derives every structural command's behaviour, which is why block structure lands as one spec row rather than as command branches.
- Every document value is persistent: `Node`, `Fragment`, `Slice`, and `Mark` share structure across edits and expose no mutator, so identity comparison is a valid change test and `eq` is the deep one. Attribute bags are `readonly` and shared between nodes with the same markup.
- Positions are integer offsets into a flat token stream: entering or leaving a non-text node costs one, a text character costs one, and `Node.resolve(pos)` is the only path from an integer to the tree. `ResolvedPos` values cache per document, so resolving the same position twice is free and holding one across an edit is a stale read.
- One canonical JSON: `Node.toJSON` emits `{type, attrs?, content?, marks?}` with `text` on text nodes, and `Node.fromJSON(schema, json)` rebuilds through the schema with attribute validation. `Node.check()` is the assertion form, throwing on any content, mark, or attribute violation.
- `DOMParser` and `DOMSerializer` are two projections of one spec set: `parseDOM` rules rank by `priority` and the first accepting rule wins, `toDOM` renders through `renderSpec` with `0` marking the content hole, and a round trip through both is lossless exactly when the two halves of each spec row agree.

[STACKING]:
- `prosemirror-transform`(`.api/prosemirror-transform.md`): every `Step` is typed against these values — `ReplaceStep` carries a `Slice`, `AddMarkStep` a `Mark`, `Transform.wrap` takes the `NodeRange` `ResolvedPos.blockRange` mints, and `Step.fromJSON(schema, json)` rehydrates a wire step through this `Schema` alone. `Transform.clearIncompatible(pos, parentType, match)` consumes a `ContentMatch` straight from `NodeType.contentMatch`.
- `prosemirror-state`(`.api/prosemirror-state.md`): `EditorState.create({schema})` derives `doc` from `schema.topNodeType.createAndFill()`, `Selection` subclasses hold `ResolvedPos` pairs, and `Transaction.storedMarks` carries the `readonly Mark[]` `ResolvedPos.marksAcross` computes.
- `prosemirror-view`(`.api/prosemirror-view.md`): `EditorProps.domParser`/`clipboardParser` take a `DOMParser` and `clipboardSerializer` a `DOMSerializer`, so a paste dialect is a rule set rather than a handler; `NodeSpec.toDOM` renders any node the `nodeViews` record leaves unclaimed.
- `prosemirror-schema-list`(`.api/prosemirror-schema-list.md`): `addListNodes(nodes, itemContent, listGroup)` folds three `NodeSpec` rows into the `OrderedMap` this package's `SchemaSpec.nodes` accepts, keeping list structure a data row.
- `core/value/schema`: `Shape` owns the branch's value shapes and derives the editor's document contract; the `Node.toJSON` object is the payload a `Shape` member admits at the boundary, and `Node.fromJSON(schema, json)` is the single decode back, so no parallel document model exists beside the schema.
- `core/value/contentKey`: `Digest` keys a serialized document by content, making a stored `Node.toJSON` payload addressable like every other byte plane in the branch.
- within-lib `view/content`: the editor page declares one `Schema` per document class from `NodeSpec`/`MarkSpec` rows, derives its `DOMParser`/`DOMSerializer` with `fromSchema`, and exposes node families by widening the spec roster — a new block is one row with its content-expression edit.

[LOCAL_ADMISSION]:
- Declare document structure as `NodeSpec`/`MarkSpec` rows with `content` expressions and let `new Schema` compile them; a structural rule enforced by command branches instead of the grammar is the rejected shape.
- Cross every boundary as `Node.toJSON` / `Node.fromJSON(schema, json)` and gate an untrusted payload with `Node.check()`; a hand-walked JSON tree bypasses attribute validation.
- Derive both DOM directions with `DOMParser.fromSchema` / `DOMSerializer.fromSchema` and extend by adding `parseDOM`/`toDOM` to the owning spec row.
- Resolve positions through `Node.resolve` and read structure off `ResolvedPos`; recomputing depth by walking `content` arrays re-implements the resolver.

[RAIL_LAW]:
- Package: `prosemirror-model`
- Owns: the schema algebra — `Schema` compiling `NodeSpec`/`MarkSpec` rows and content expressions into `NodeType`/`MarkType` with their `ContentMatch` automata; the immutable `Node`/`Fragment`/`Slice`/`Mark` document values with their position coordinate system, `ResolvedPos` and `NodeRange`; the canonical `toJSON`/`fromJSON` wire shape and `Node.check` validation; and the `DOMParser`/`DOMSerializer` pair derived from the same spec rows
- Accept: one `Schema` per document class built from data rows, `createChecked`/`createAndFill` for construction, `ResolvedPos`/`NodeRange` for every structural query, `ContentMatch.findWrapping`/`fillBefore` for admissibility, `DOMParser.fromSchema`/`DOMSerializer.fromSchema` with per-row `parseDOM`/`toDOM` overrides, and `Node.toJSON`/`fromJSON(schema, json)` as the only boundary form
- Reject: a parallel document model or a hand-walked JSON tree beside the schema, structural rules enforced in command branches rather than in a content expression, `NodeType.create` where content admissibility is unproven, a `Node.check()` call read as a boolean, a `ResolvedPos` held across an edit, and a hand-written DOM parser or serializer where a `parseDOM`/`toDOM` row carries the mapping
