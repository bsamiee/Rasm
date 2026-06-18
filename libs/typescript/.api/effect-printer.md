# [API_CATALOGUE] @effect/printer

`@effect/printer` supplies an algebraic pretty-printer: the `Doc<A>` document ADT with 13 variants, layout algorithms that convert `Doc<A>` to a flat `DocStream<A>` or structured `DocTree<A>`, `Flatten` and `Optimize` utilities, and `PageWidth` configuration. All combinators carry dual data-first / data-last overloads for pipe composition. Annotations carry arbitrary metadata through layout unchanged; rendering algorithms consume them to produce coloured or structured output.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/printer`
- package: `@effect/printer`
- modules: `@effect/printer/Doc`, `@effect/printer/DocStream`, `@effect/printer/DocTree`, `@effect/printer/Layout`, `@effect/printer/Optimize`, `@effect/printer/Flatten`, `@effect/printer/PageWidth`
- barrel: `@effect/printer` (re-exports all modules as namespaces)
- asset: runtime library
- rail: pretty-printing, document algebra

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Doc` — document ADT (`@effect/printer/Doc`)

`Doc<A>` is a tagged union of 13 variants; all extend `Doc.Variance<A>` which itself extends `Equal` and `Pipeable`.

| [INDEX] | [VARIANT]       | [FIELDS]                                  | [ROLE]                                       |
| :-----: | :-------------- | :---------------------------------------- | :------------------------------------------- |
|   [1]   | `Fail`          | —                                         | rejected document; layout algorithms skip    |
|   [2]   | `Empty`         | —                                         | empty string unit for `Cat`                  |
|   [3]   | `Char`          | `char: string`                            | single non-newline character                 |
|   [4]   | `Text`          | `text: string`                            | string ≥2 chars, no newline                  |
|   [5]   | `Line`          | —                                         | hard line; becomes space under `group`       |
|   [6]   | `FlatAlt`       | `left: Doc<A>`, `right: Doc<A>`           | layout: `left`; flattened: `right`           |
|   [7]   | `Cat`           | `left: Doc<A>`, `right: Doc<A>`           | concatenation                                |
|   [8]   | `Nest`          | `indent: number`, `doc: Doc<A>`           | indentation envelope                         |
|   [9]   | `Union`         | `left: Doc<A>`, `right: Doc<A>`           | layout alternative; first line of left wider |
|  [10]   | `Column`        | `react: (position: number) => Doc<A>`     | cursor-position reactive                     |
|  [11]   | `WithPageWidth` | `react: (pageWidth: PageWidth) => Doc<A>` | page-width reactive                          |
|  [12]   | `Nesting`       | `react: (level: number) => Doc<A>`        | nesting-level reactive                       |
|  [13]   | `Annotated`     | `annotation: A`, `doc: Doc<A>`            | annotation envelope                          |

`Doc.RenderConfig` is `Compact | Pretty | Smart`; `Pretty` and `Smart` accept `Partial<Omit<AvailablePerLine, "_tag">>` options.

[PUBLIC_TYPE_SCOPE]: `DocStream` — laid-out stream (`@effect/printer/DocStream`)

`DocStream<A>` is a tagged union of 7 variants produced by layout algorithms.

| [INDEX] | [VARIANT]              | [FIELDS]                                      | [ROLE]                |
| :-----: | :--------------------- | :-------------------------------------------- | :-------------------- |
|   [1]   | `FailedStream`         | —                                             | layout failure        |
|   [2]   | `EmptyStream`          | —                                             | end of stream         |
|   [3]   | `CharStream`           | `char: string`, `stream: DocStream<A>`        | single character node |
|   [4]   | `TextStream`           | `text: string`, `stream: DocStream<A>`        | text node             |
|   [5]   | `LineStream`           | `indentation: number`, `stream: DocStream<A>` | newline + indent      |
|   [6]   | `PushAnnotationStream` | `annotation: A`, `stream: DocStream<A>`       | annotation open       |
|   [7]   | `PopAnnotationStream`  | `stream: DocStream<A>`                        | annotation close      |

[PUBLIC_TYPE_SCOPE]: `DocTree`, `Flatten`, `PageWidth`, `Optimize`

| [INDEX] | [SYMBOL]                         | [KIND]       | [ROLE]                                                                                      |
| :-----: | :------------------------------- | :----------- | :------------------------------------------------------------------------------------------ |
|   [1]   | `DocTree<A>`                     | tagged union | 6 variants: `EmptyTree`, `CharTree`, `TextTree`, `LineTree`, `AnnotationTree`, `ConcatTree` |
|   [2]   | `Flatten<A>`                     | tagged union | 3 cases: `Flattened<A>` (`.value`), `AlreadyFlat`, `NeverFlat`                              |
|   [3]   | `PageWidth`                      | tagged union | `AvailablePerLine` (`lineWidth`, `ribbonFraction`) or `Unbounded`                           |
|   [4]   | `Layout.Options`                 | interface    | `{ pageWidth: PageWidth }`                                                                  |
|   [5]   | `Layout.FittingPredicate<A>`     | interface    | `(stream, indent, col, comparator) => boolean`                                              |
|   [6]   | `Optimize.Depth` / `FusionDepth` | tagged union | `Shallow` or `Deep`                                                                         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Doc` — primitives and constructors

| [INDEX] | [SURFACE]             | [SIGNATURE]                    | [ROLE]                                                                                                                                                                                      |
| :-----: | :-------------------- | :----------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | `Doc.char(char)`      | `(char: string) => Doc<never>` | single character                                                                                                                                                                            |
|   [2]   | `Doc.text(text)`      | `(text: string) => Doc<never>` | raw text, no newlines                                                                                                                                                                       |
|   [3]   | `Doc.string(str)`     | `(str: string) => Doc<never>`  | text ignoring embedded newlines                                                                                                                                                             |
|   [4]   | `Doc.empty`           | `Doc<never>`                   | empty document                                                                                                                                                                              |
|   [5]   | `Doc.fail`            | `Doc<never>`                   | always-rejected document                                                                                                                                                                    |
|   [6]   | `Doc.line`            | `Doc<never>`                   | newline or space under `group`                                                                                                                                                              |
|   [7]   | `Doc.lineBreak`       | `Doc<never>`                   | newline or empty under `group`                                                                                                                                                              |
|   [8]   | `Doc.softLine`        | `Doc<never>`                   | space if fits, else newline                                                                                                                                                                 |
|   [9]   | `Doc.softLineBreak`   | `Doc<never>`                   | empty if fits, else newline                                                                                                                                                                 |
|  [10]   | `Doc.hardLine`        | `Doc<never>`                   | unconditional newline                                                                                                                                                                       |
|  [11]   | punctuation constants | `Doc<never>` (each)            | `space`, `comma`, `dot`, `colon`, `semi`, `slash`, `backslash`, `equalSign`, `dquote`, `squote`, `vbar`, `langle`, `rangle`, `lparen`, `rparen`, `lbrace`, `rbrace`, `lbracket`, `rbracket` |

[ENTRYPOINT_SCOPE]: `Doc` — concatenation and separation combinators

| [INDEX] | [SURFACE]                                          | [SIGNATURE]                                     | [ROLE]                                        |
| :-----: | :------------------------------------------------- | :---------------------------------------------- | :-------------------------------------------- |
|   [1]   | `Doc.cat`                                          | `(that: Doc<B>) => (self: Doc<A>) => Doc<A\|B>` | two docs, no separator                        |
|   [2]   | `Doc.hcat`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | horizontal concatenation of collection        |
|   [3]   | `Doc.vcat`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | vertical concatenation (hard newlines)        |
|   [4]   | `Doc.fillCat`                                      | `(docs: Iterable<Doc<A>>) => Doc<A>`            | fill by concatenation, break when overflowing |
|   [5]   | `Doc.cats`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | grouped concat; newlines if overflow          |
|   [6]   | `Doc.hsep`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | horizontal, space-separated                   |
|   [7]   | `Doc.vsep`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | vertical, line-separated                      |
|   [8]   | `Doc.fillSep`                                      | `(docs: Iterable<Doc<A>>) => Doc<A>`            | fill by space, break when overflowing         |
|   [9]   | `Doc.seps`                                         | `(docs: Iterable<Doc<A>>) => Doc<A>`            | grouped sep; newlines if overflow             |
|  [10]   | `Doc.concatWith`                                   | `(f, docs) => Doc<A>`                           | fold with arbitrary binary combinator         |
|  [11]   | `Doc.catWithLine`                                  | dual overload                                   | two docs separated by `line`                  |
|  [12]   | `Doc.catWithSpace`                                 | dual overload                                   | two docs separated by `space`                 |
|  [13]   | `Doc.catWithSoftLine` / `Doc.catWithSoftLineBreak` | dual overloads                                  | soft-break separators                         |

[ENTRYPOINT_SCOPE]: `Doc` — layout, alignment, and annotation

| [INDEX] | [SURFACE]                 | [SIGNATURE]                                    | [ROLE]                                         |
| :-----: | :------------------------ | :--------------------------------------------- | :--------------------------------------------- |
|   [1]   | `Doc.nest(indent)`        | `(indent: number) => (self: Doc<A>) => Doc<A>` | indent subsequent lines by `indent` columns    |
|   [2]   | `Doc.align`               | `(self: Doc<A>) => Doc<A>`                     | set nesting to current column                  |
|   [3]   | `Doc.hang(indent)`        | `(indent: number) => (self: Doc<A>) => Doc<A>` | indent relative to first line of doc           |
|   [4]   | `Doc.indent(indent)`      | `(indent: number) => (self: Doc<A>) => Doc<A>` | indent whole document by `indent` spaces       |
|   [5]   | `Doc.group`               | `(self: Doc<A>) => Doc<A>`                     | try flat layout; fall back to broken layout    |
|   [6]   | `Doc.flatAlt`             | dual overload `(left, right)`                  | layout: `left`; under `group`/flatten: `right` |
|   [7]   | `Doc.union`               | dual overload `(left, right)`                  | direct layout alternative                      |
|   [8]   | `Doc.column`              | `(react: (pos: number) => Doc<A>) => Doc<A>`   | column-reactive document                       |
|   [9]   | `Doc.nesting`             | `(react: (level: number) => Doc<A>) => Doc<A>` | nesting-level-reactive document                |
|  [10]   | `Doc.pageWidth`           | `(react: (pw: PageWidth) => Doc<A>) => Doc<A>` | page-width-reactive document                   |
|  [11]   | `Doc.width`               | dual overload `(react: (w: number) => Doc<A>)` | measured-width-reactive document               |
|  [12]   | `Doc.fill`                | dual overload `(width: number)`                | fill to width with spaces                      |
|  [13]   | `Doc.fillBreak`           | dual overload `(width: number)`                | fill to width or break + indent                |
|  [14]   | `Doc.encloseSep`          | `(left, right, sep, docs)`                     | enclose collection with separators             |
|  [15]   | `Doc.list` / `Doc.tupled` | `(docs: Iterable<Doc<A>>) => Doc<A>`           | bracket-enclosed comma list                    |
|  [16]   | `Doc.annotate`            | dual overload `(annotation: A)`                | attach annotation to document                  |
|  [17]   | `Doc.alterAnnotations`    | dual overload `(f: (a: A) => Option<B>)`       | filter/remap annotations                       |
|  [18]   | `Doc.reAnnotate`          | dual overload `(f: (a: A) => B)`               | transform all annotations                      |
|  [19]   | `Doc.unAnnotate`          | `(self: Doc<A>) => Doc<never>`                 | strip all annotations                          |

[ENTRYPOINT_SCOPE]: `Doc` — rendering and utility

| [INDEX] | [SURFACE]                                      | [SIGNATURE]                                                                                         | [ROLE]                                    |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `Doc.render`                                   | `(self: Doc<A>, config: Doc.RenderConfig) => string`                                                | layout + render to string in one call     |
|   [2]   | `Doc.renderStream`                             | `(self: DocStream<A>) => string`                                                                    | render a pre-laid-out stream to string    |
|   [3]   | `Doc.flatten`                                  | `(self: Doc<A>) => Doc<A>`                                                                          | force flat layout                         |
|   [4]   | `Doc.changesUponFlattening`                    | `(self: Doc<A>) => Flatten<Doc<A>>`                                                                 | inspect flattenability without committing |
|   [5]   | `Doc.match`                                    | `(patterns) => (self) => R` dual overload                                                           | variant eliminator over all 13 cases      |
|   [6]   | `Doc.words(s, char?)`                          | `string => ReadonlyArray<Doc<never>>`                                                               | split string into word documents          |
|   [7]   | `Doc.reflow(s, char?)`                         | `string => Doc<never>`                                                                              | split + fill-sep join                     |
|   [8]   | `Doc.spaces(n)`                                | `(n: number) => Doc<never>`                                                                         | n spaces                                  |
|   [9]   | `Doc.punctuate`                                | dual overload `(punctuation, docs)`                                                                 | intersperse punctuation                   |
|  [10]   | `Doc.surround`                                 | dual overload `(left, right)`                                                                       | wrap doc in left/right                    |
|  [11]   | bracket helpers                                | `singleQuoted`, `doubleQuoted`, `parenthesized`, `angleBracketed`, `squareBracketed`, `curlyBraced` | convenience surrounds                     |
|  [12]   | `Doc.getSemigroup<A>()` / `Doc.getMonoid<A>()` | `() => Semigroup<Doc<A>>` / `() => Monoid<Doc<A>>`                                                  | typeclass instances                       |
|  [13]   | `Doc.Covariant` / `Doc.Invariant`              | typeclass instances                                                                                 | `Covariant` and `Invariant` for `Doc`     |

[ENTRYPOINT_SCOPE]: `Layout` — algorithms

| [INDEX] | [SURFACE]                   | [SIGNATURE]                                         | [ROLE]                                |
| :-----: | :-------------------------- | :-------------------------------------------------- | :------------------------------------ |
|   [1]   | `Layout.options(pageWidth)` | `(pageWidth: PageWidth) => Layout.Options`          | construct layout options              |
|   [2]   | `Layout.defaultOptions`     | `Layout.Options`                                    | 80-column `AvailablePerLine` defaults |
|   [3]   | `Layout.compact`            | `(self: Doc<A>) => DocStream<A>`                    | no indentation, no annotations        |
|   [4]   | `Layout.pretty`             | dual overload `(options) => (self) => DocStream<A>` | one-element lookahead layout          |
|   [5]   | `Layout.smart`              | dual overload `(options) => (self) => DocStream<A>` | multi-element lookahead layout        |
|   [6]   | `Layout.unbounded`          | `(self: Doc<A>) => DocStream<A>`                    | layout with `Unbounded` page width    |
|   [7]   | `Layout.wadlerLeijen`       | `(fits, options) => (self) => DocStream<A>`         | custom fitting-predicate layout       |

[ENTRYPOINT_SCOPE]: `DocStream` operations, `DocTree`, `Optimize`, `PageWidth`, `Flatten`

| [INDEX] | [SURFACE]                                                | [SIGNATURE / KIND]                                    | [ROLE]                                |
| :-----: | :------------------------------------------------------- | :---------------------------------------------------- | :------------------------------------ |
|   [1]   | `DocStream.match(patterns)`                              | dual overload, 7-branch eliminator                    | fold over all stream variants         |
|   [2]   | `DocStream.alterAnnotations(f)`                          | dual overload `(a: A) => Option<B>`                   | filter/remap stream annotations       |
|   [3]   | `DocStream.reAnnotate(f)` / `unAnnotate`                 | dual overload / `(self) => DocStream<never>`          | transform or strip stream annotations |
|   [4]   | `DocStream.foldMap(M, f)`                                | dual overload with `Monoid<M>`                        | monoidal fold over stream             |
|   [5]   | `DocStream.map(f)`                                       | dual overload                                         | functor map over annotations          |
|   [6]   | `DocTree.treeForm`                                       | `(self: DocStream<A>) => DocTree<A>`                  | convert stream to annotated tree      |
|   [7]   | `Optimize.optimize(depth)`                               | `(depth: FusionDepth) => (self: Doc<A>) => Doc<A>`    | fuse text nodes; `Shallow` or `Deep`  |
|   [8]   | `PageWidth.availablePerLine(lw, rf)`                     | `(lineWidth, ribbonFraction) => PageWidth`            | bounded page width                    |
|   [9]   | `PageWidth.unbounded`                                    | `PageWidth`                                           | no line limit                         |
|  [10]   | `PageWidth.remainingWidth(...)`                          | `(lineLength, ribbonFraction, indent, col) => number` | remaining usable columns              |
|  [11]   | `Flatten.flattened(value)` / `alreadyFlat` / `neverFlat` | constructors                                          | `Flatten<A>` case construction        |

## [4]-[IMPLEMENTATION_LAW]

[DOC_ALGEBRA]:
- `Doc<A>` is covariant in `A` and implements `Equal` and `Pipeable`; all combinators are pure.
- `Doc<never>` is the type of annotation-free documents; `Doc<A>` participates in annotation pipelines when `A` is a concrete type (e.g., ANSI color).
- `group` attempts to lay out the document on one line using `flatAlt` and `Union`; layout algorithms choose the flat branch when it fits within the page width.
- `nest`, `align`, `hang`, and `indent` are composition helpers over `Nest` and `Column` nodes; they do not interact with horizontal extent unless combined with `group`.
- `Doc.string` silently drops embedded `\n` characters; use `Doc.line` or `Doc.hardLine` for newlines in document construction.

[LAYOUT_SELECTION]:
- `Layout.compact` emits no indentation and strips all annotations; use only for machine-read output where size matters.
- `Layout.pretty` commits after one lookahead element; it can overrun on deeply nested alternatives. Use `Layout.smart` when output consistently runs past the right margin.
- `Layout.smart` checks all lines until it finds one at the same or lower indentation, making it correct for deeply grouped expressions at the cost of slightly higher overhead.
- Custom layout via `Layout.wadlerLeijen` requires a `FittingPredicate<A>`; the predicate receives the current stream, indentation, column, and a lazy alternative comparator.

[RENDER_PIPELINE]:
- Full pipeline: `Doc<A>` → `Layout.*` → `DocStream<A>` → render or `DocTree.treeForm`.
- `Doc.render` is the single-call shorthand; it internally selects `Layout.pretty`, `Layout.smart`, or `Layout.compact` based on `RenderConfig.style`.
- `Doc.renderStream` converts a `DocStream<A>` to a plain `string`, ignoring annotations.
- Annotated rendering for ANSI or HTML requires consuming `DocStream<A>` directly or converting via `DocTree.treeForm` and folding the tree.

[OPTIMIZE_AND_FLATTEN]:
- `Optimize.optimize(Shallow)` fuses only adjacent text nodes, avoiding deep recursion; safe to apply unconditionally.
- `Optimize.optimize(Deep)` recurses into `nesting`, `column`, and `Union` branches; measure before using on documents with many reactive nodes.
- `Doc.changesUponFlattening` returns `Flatten<Doc<A>>` without committing to flattening; use it to decide whether to wrap in `group` at call sites.
- `Flatten.Flattened` carries the flattened doc; `AlreadyFlat` means no-op; `NeverFlat` means `group` will reject this branch.
