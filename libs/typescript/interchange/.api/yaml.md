# [API_CATALOGUE] yaml

`yaml` (eemeli/yaml) is the complete YAML 1.2 parser, document model, and stringifier. `parse` converts a YAML string to a JS value; `stringify` converts any JS value to a YAML string; `parseDocument` and `parseAllDocuments` produce mutable `Document` AST objects with `errors` and `warnings` arrays. `Document`, `Scalar`, `YAMLMap`, `YAMLSeq`, `Alias`, and `Pair` form the full node model. `visit` / `visitAsync` traverse any node tree. CST-level access is available via `Lexer`, `Parser`, `LineCounter`, and the `CST` namespace.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `yaml`
- package: `yaml`
- entry: `yaml` (single barrel — `dist/index.d.ts`)
- secondary entry: `yaml/util` (utility functions not in the main barrel)
- asset: parse/stringify API, `Document` + node model, visitor, schema, CST pipeline
- rail: interchange / yaml

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and stream
- rail: interchange

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                                                     |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------------------------- |
|   [1]   | `Document<C, S>`  | class         | mutable YAML document; `contents`, `errors`, `warnings`, `schema`, CRUD ops                |
|   [2]   | `Document.Parsed` | interface     | narrowed `Document` from `parseDocument`; `range` and `directives` present                 |
|   [3]   | `EmptyStream`     | interface     | `Array<Document.Parsed> & { empty: true }` returned by `parseAllDocuments` for empty input |
|   [4]   | `Schema`          | class         | resolves tags, scalars, and collections; controlled via `SchemaOptions`                    |

[PUBLIC_TYPE_SCOPE]: node model
- rail: interchange

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [RAIL]                                                                        |
| :-----: | :------------- | :------------ | :---------------------------------------------------------------------------- |
|   [1]   | `Node<T>`      | type alias    | `Alias \| Scalar<T> \| YAMLMap<unknown, T> \| YAMLSeq<T>`                     |
|   [2]   | `ParsedNode`   | type alias    | `Alias.Parsed \| Scalar.Parsed \| YAMLMap.Parsed \| YAMLSeq.Parsed`           |
|   [3]   | `NodeType<T>`  | utility type  | maps a JS type `T` to its canonical YAML node type                            |
|   [4]   | `Range`        | type alias    | `[start, value-end, node-end]` character offset triple                        |
|   [5]   | `Scalar<T>`    | class         | wraps a scalar value; `value`, `type` (`PLAIN`/`QUOTE_DOUBLE`/etc.), `anchor` |
|   [6]   | `YAMLMap<K,V>` | class         | mapping node; `items: Pair<K,V>[]`; CRUD: `get`, `set`, `delete`, `has`       |
|   [7]   | `YAMLSeq<T>`   | class         | sequence node; `items: T[]`; CRUD: `get`, `set`, `delete`                     |
|   [8]   | `Pair<K,V>`    | class         | key-value pair inside a `YAMLMap`                                             |
|   [9]   | `Alias`        | class         | YAML alias (`*anchor`); resolves to its target node                           |

[PUBLIC_TYPE_SCOPE]: option bags
- rail: interchange

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [KEY_OPTIONS]                                                                                          |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------------------------------------- |
|   [1]   | `ParseOptions`      | type          | `intAsBigInt`, `keepSourceTokens`, `lineCounter`, `prettyErrors`, `strict`, `stringKeys`, `uniqueKeys` |
|   [2]   | `DocumentOptions`   | type          | `logLevel`, `version` (`'1.1' \| '1.2' \| 'next'`)                                                     |
|   [3]   | `SchemaOptions`     | type          | `schema`, `customTags`, `merge`, `compat`, `resolveKnownTags`, `sortMapEntries`, `toStringDefaults`    |
|   [4]   | `CreateNodeOptions` | type          | `aliasDuplicateObjects`, `anchorPrefix`, `flow`, `keepUndefined`, `tag`                                |
|   [5]   | `ToJSOptions`       | type          | `mapAsMap`, `maxAliasCount`, `onAnchor`, `reviver`                                                     |
|   [6]   | `ToStringOptions`   | type          | `blockQuote`, `collectionStyle`, `indent`, `lineWidth`, `singleQuote`, `trailingComma`, and more       |

[PUBLIC_TYPE_SCOPE]: errors and visitor
- rail: interchange

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :--------------- | :------------ | :-------------------------------------------------------- |
|   [1]   | `YAMLError`      | class         | base; `name`, `code`, `message`, `pos`, `linePos`         |
|   [2]   | `YAMLParseError` | class         | thrown by `parse()`; wraps source offset and line info    |
|   [3]   | `YAMLWarning`    | class         | non-fatal; collected in `Document.warnings`               |
|   [4]   | `visitor`        | type alias    | sync visitor map `{ Alias?, Map?, Pair?, Scalar?, Seq? }` |
|   [5]   | `asyncVisitor`   | type alias    | async variant of `visitor`; used by `visitAsync`          |

[PUBLIC_TYPE_SCOPE]: CST pipeline
- rail: interchange

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :------------ | :------------ | :------------------------------------------------------- |
|   [1]   | `Lexer`       | class         | tokenises raw YAML source into a CST token stream        |
|   [2]   | `Parser`      | class         | converts token stream into CST `Document` events         |
|   [3]   | `LineCounter` | class         | tracks line/col positions; passed via `ParseOptions`     |
|   [4]   | `Composer`    | class         | converts CST into `Document<ParsedNode>` AST             |
|   [5]   | `CST`         | namespace     | re-exports all CST token types and the CST document type |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and stringify
- rail: interchange

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [RAIL]                                                          |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------- |
|   [1]   | `parse(src, options?): any`                                                              | throwing parse  | single-document YAML → JS value; may `console.warn` on warnings |
|   [2]   | `parse(src, reviver, options?): any`                                                     | throwing parse  | same with JSON-style reviver function                           |
|   [3]   | `stringify(value, options?): string`                                                     | stringify       | JS value → YAML string; always ends with `\n`                   |
|   [4]   | `stringify(value, replacer?, options?): string`                                          | stringify       | same with JSON-style replacer array or function                 |
|   [5]   | `parseDocument<C, S>(source, options?): Document.Parsed<C, S> \| Document<C, S>`         | document parse  | returns mutable AST; errors/warnings in arrays, never throws    |
|   [6]   | `parseAllDocuments<C, S>(source, options?): Array<Document.Parsed<C, S>> \| EmptyStream` | multi-doc parse | lazy stream of documents separated by `---` or `...`            |

[ENTRYPOINT_SCOPE]: document operations
- rail: interchange

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------- |
|   [1]   | `doc.get(key, keepScalar?)`                            | accessor       | retrieve value at key; unwraps `Scalar` by default |
|   [2]   | `doc.set(key, value)`                                  | mutator        | add or replace a key-value pair                    |
|   [3]   | `doc.delete(key): boolean`                             | mutator        | remove by key                                      |
|   [4]   | `doc.has(key): boolean`                                | predicate      | key existence check                                |
|   [5]   | `doc.getIn(path, keepScalar?)`                         | deep accessor  | path-based nested get                              |
|   [6]   | `doc.setIn(path, value)`                               | deep mutator   | path-based nested set                              |
|   [7]   | `doc.deleteIn(path): boolean`                          | deep mutator   | path-based nested delete                           |
|   [8]   | `doc.createNode<T>(value, options?): NodeType<T>`      | node builder   | JS value → typed YAML node using document schema   |
|   [9]   | `doc.createPair<K,V>(key, value, options?): Pair<K,V>` | node builder   | key + value → `Pair` node                          |
|  [10]   | `doc.createAlias(node, name?): Alias`                  | node builder   | create anchored alias; assigns anchor if absent    |
|  [11]   | `doc.toJS(options?): any`                              | projection     | AST → plain JS value                               |
|  [12]   | `doc.toString(options?): string`                       | projection     | AST → YAML string                                  |
|  [13]   | `doc.clone(): Document`                                | copy           | deep clone of document and its contents            |

[ENTRYPOINT_SCOPE]: visitor and type guards
- rail: interchange

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY] | [RAIL]                                                             |
| :-----: | :------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|   [1]   | `visit(node, visitor): void`                       | sync visitor   | depth-first traversal; return `visit.BREAK/SKIP/REMOVE` to control |
|   [2]   | `visitAsync(node, visitor): Promise<void>`         | async visitor  | same but visitor callbacks may be async                            |
|   [3]   | `isAlias(value): value is Alias`                   | type guard     | —                                                                  |
|   [4]   | `isCollection(value): value is YAMLMap \| YAMLSeq` | type guard     | —                                                                  |
|   [5]   | `isDocument(value): value is Document`             | type guard     | —                                                                  |
|   [6]   | `isMap(value): value is YAMLMap`                   | type guard     | —                                                                  |
|   [7]   | `isPair(value): value is Pair`                     | type guard     | —                                                                  |
|   [8]   | `isScalar(value): value is Scalar`                 | type guard     | —                                                                  |
|   [9]   | `isSeq(value): value is YAMLSeq`                   | type guard     | —                                                                  |
|  [10]   | `isNode(value): value is Node`                     | type guard     | —                                                                  |

## [4]-[IMPLEMENTATION_LAW]

[PARSE_TOPOLOGY]:
- `parse()` throws `YAMLParseError` on invalid input and logs warnings via `console.warn`; use `parseDocument` when you need non-throwing error collection.
- `parseDocument` and `parseAllDocuments` never throw; inspect `doc.errors` before treating the document as valid.
- `'empty' in docs` is the correct type guard for `EmptyStream` returned from `parseAllDocuments`.
- `version` defaults to `'1.2'` (core schema); set `'1.1'` to activate the yaml-1.1 schema with boolean aliases and merge keys by default.
- `stringKeys: true` forces all map keys to strings and rejects non-scalar keys.

[STRINGIFY_TOPOLOGY]:
- `stringify()` always appends `\n`; do not strip it when writing to files.
- `collectionStyle: 'block'` or `'flow'` overrides per-node `flow` flags globally.
- `indent` default is `2`; `lineWidth` default is `80` (soft limit); set `lineWidth: 0` to disable folding.
- `singleQuote: true` prefers single-quoted scalars where valid; `singleQuote: false` disables them entirely.

[NODE_MODEL_TOPOLOGY]:
- `Scalar.Type` closed set: `'BLOCK_FOLDED' | 'BLOCK_LITERAL' | 'PLAIN' | 'QUOTE_DOUBLE' | 'QUOTE_SINGLE'`. Access via `Scalar.PLAIN` etc. static constants.
- `YAMLMap.items` is `Pair<K,V>[]`; never manipulate it directly — use `doc.set`, `doc.delete`, or the `visit` API.
- `ToJSOptions.mapAsMap: true` returns `Map` instead of `Object` for YAML mappings; useful for numeric or non-string keys.
- `ToJSOptions.maxAliasCount` defaults to `100`; set `-1` to disable entity-expansion protection only when the source is trusted.

[SCHEMA_TOPOLOGY]:
- Built-in schemas: `'failsafe'` (scalars as strings only), `'core'` (YAML 1.2 default), `'json'` (JSON-compatible subset), `'yaml-1.1'`.
- `customTags` accepts a `Tags` array or a `(tags: Tags) => Tags` transformer for schema extension.
- `resolveKnownTags: true` (default for `'core'`) enables `!!binary`, `!!omap`, `!!pairs`, `!!set`, `!!timestamp` in the core schema.

[RAIL_LAW]:
- Package: `yaml`
- Owns: YAML 1.2 parse, stringify, and document model
- Accept: YAML strings via `parse` / `parseDocument`; JS values via `stringify`
- Reject: hand-rolled YAML regex parsing, manual CST traversal when the public visitor API covers the case
