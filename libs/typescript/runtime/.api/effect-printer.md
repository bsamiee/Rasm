# [@effect/printer] — the Doc algebra the cli/render rows compose

`@effect/printer` is the annotation-parametric Wadler/Leijen pretty-printing algebra: `Doc<A>` is an immutable document tree (`A` = the abstract annotation type carried through every node, never resolved by this package), a full combinator surface for concatenation/alternatives/nesting/alignment/enclosure, three layout algorithms (`compact` | `pretty` | `smart`) that compile a `Doc` to the intermediate `DocStream`, a `DocTree` decorated form for structure-aware backends, `Optimize` fusion over shallow/deep depth, and annotation transforms (`annotate`/`reAnnotate`/`alterAnnotations`/`unAnnotate`) that let a downstream renderer bind `A` to concrete markup. It is the single document-composition owner behind the terminal render rows; no ad-hoc string concatenation, manual column math, or hand-rolled line-wrapping survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/printer`
- package: `@effect/printer`
- version: `0.49.0`
- license: MIT
- asset: ESM `.d.ts` declaration surface (`dist/dts/*.d.ts`); peer `effect`
- owner: `edge`
- rail: render
- peer: `effect` (typeclass instances, `Order`, `Equal`, `Match` on the `Doc` union), `@effect/typeclass` (Semigroup/Monoid derivation)
- namespaces: `Doc` (the algebra + constructors + combinators + `render`), `DocStream` (post-layout token stream), `DocTree` (decorated tree form), `Layout` (algorithms + `Options`), `PageWidth` (width policy), `Optimize` (fusion), `Flatten` (flatten-analysis)
- capability: annotation-parametric document tree, concatenation/alternatives/nesting/alignment/enclosure combinators, three layout algorithms over a `PageWidth`, intermediate `DocStream`/`DocTree` forms for custom backends, associativity fusion, and annotation retargeting that defers concrete markup to the consumer
- import: `import { Doc, Layout, PageWidth, Optimize } from "@effect/printer"`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Doc core algebra (`Doc`)
- rail: render
- `Doc<A>` is a closed union of thirteen node interfaces; `A` is phantom until a renderer resolves it. Every combinator preserves `A`, so annotation is a distinct concern from layout. Refine a node with the `isX` guards or `Doc.match` rather than inspecting `_tag`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]      | [RAIL]                                                                       |
| :-----: | :-------------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Doc.Doc<A>`                      | tagged union       | `Fail\|Empty\|Char\|Text\|Line\|FlatAlt\|Cat\|Nest\|Union\|Column\|WithPageWidth\|Nesting\|Annotated` |
|  [02]   | `Doc.DocTypeId`                   | unique symbol type | nominal brand carried by every node                                          |
|  [03]   | `Doc.DocTypeLambda`               | `TypeLambda`       | HKT witness for the `Covariant`/`Invariant` instances                        |
|  [04]   | `Doc.FlatAlt` / `Doc.Union`       | node interface     | layout-alternative nodes the algorithms select between                       |
|  [05]   | `Doc.Column` / `Doc.Nesting`      | node interface     | position-reactive nodes (`f: (int) => Doc<A>`)                               |
|  [06]   | `Doc.WithPageWidth`               | node interface     | width-reactive node (`f: (PageWidth) => Doc<A>`)                             |
|  [07]   | `Doc.Annotated`                   | node interface     | `{ annotation: A; doc: Doc<A> }` markup carrier                              |
|  [08]   | `Doc.RenderConfig`                | config union       | `render` discriminant: `{ style: "compact" }` \| `{ style: "pretty"\|"smart", options? }` |
|  [09]   | `isDoc`/`isCat`/`isUnion`/`isAnnotated`/… | refinement    | fourteen `(u) => u is <Node>` guards over the union                          |

[PUBLIC_TYPE_SCOPE]: intermediate + analysis forms
- rail: render
- Layout lowers `Doc<A>` to `DocStream<A>`; `renderStream` folds a stream to `string`. `DocTree` is the reparsed, structure-aware form for backends that need nesting scopes (HTML/markup), reached via `DocTree.treeForm`. `Flatten<A>` is the group-analysis result deciding whether a subtree can collapse to one line.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                                                        |
| :-----: | :---------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `DocStream.DocStream<A>` | tagged union  | `Failed\|Empty\|Char\|Text\|Line\|PushAnnotation\|PopAnnotation` token stream |
|  [02]   | `DocTree.DocTree<A>`    | tagged union  | `Empty\|Char\|Text\|Line\|Annotation\|Concat` decorated tree                  |
|  [03]   | `Flatten.Flatten<A>`    | tagged union  | `Flattened\|AlreadyFlat\|NeverFlat` group-collapse analysis                   |
|  [04]   | `Optimize.FusionDepth`  | tagged union  | `Shallow\|Deep` associativity-fusion depth                                    |

[PUBLIC_TYPE_SCOPE]: layout + width model
- rail: render
- A layout algorithm is a `Doc<A> => DocStream<A>` parameterized by `Layout.Options` carrying one `PageWidth`. `AvailablePerLine(lineWidth, ribbonFraction)` bounds the printable ribbon; `Unbounded` disables line-breaking entirely (single-line output).

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                                              |
| :-----: | :------------------------ | :------------- | :----------------------------------------------------------------- |
|  [01]   | `Layout.Layout<A>`        | function alias | `(options: Layout.Options) => DocStream<A>`                        |
|  [02]   | `Layout.Options`          | record         | `{ pageWidth: PageWidth }` layout configuration                    |
|  [03]   | `PageWidth.PageWidth`     | tagged union   | `AvailablePerLine\|Unbounded` width policy                         |
|  [04]   | `PageWidth.AvailablePerLine` | node        | `{ lineWidth: number; ribbonFraction: number }`                    |

[PUBLIC_TYPE_SCOPE]: typeclass instances (annotation-parametric)
- rail: render
- The instances make `Doc<A>` a first-class monoidal, mappable value; compose documents with `effect/Monoid` folds and retarget annotations with the `Covariant` map instead of restructuring the tree.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]  | [RAIL]                                                        |
| :-----: | :----------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Doc.getSemigroup<A>()` / `Doc.getMonoid<A>()` | instance   | concat/empty monoid over `Doc<A>` (identity = `Doc.empty`)   |
|  [02]   | `Doc.Covariant` / `Doc.Invariant`          | instance       | map/imap the annotation type `A`                             |
|  [03]   | `DocStream.Functor` / `DocTree.Covariant`  | instance       | annotation map over the intermediate forms                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: primitives + character vocabulary (`Doc`)
- rail: render
- Leaf constructors and the punctuation vocabulary; `string` splits embedded newlines to `hardLine`, `char`/`text` assume none. `softLine` renders a space when the group fits and a line break otherwise; `line` renders a space when flattened, `lineBreak` renders nothing when flattened.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :--------------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Doc.char(c)` / `Doc.text(s)` / `Doc.string(s)`                  | primitive      | single char / newline-free text / newline-splitting text    |
|  [02]   | `Doc.empty` / `Doc.fail`                                         | primitive      | identity element / layout-abort sentinel                    |
|  [03]   | `Doc.line` / `Doc.lineBreak` / `Doc.hardLine`                   | primitive      | flatten-to-space / flatten-to-empty / unconditional break   |
|  [04]   | `Doc.softLine` / `Doc.softLineBreak`                            | primitive      | fit-aware break (space / nothing when it fits)              |
|  [05]   | `Doc.lparen`/`rparen`/`comma`/`colon`/`space`/`vbar`/`dquote`/… | vocabulary     | 19 single-char document constants                           |

[ENTRYPOINT_SCOPE]: concatenation family (`Doc`)
- rail: render
- One associative concat (`cat`) plus separator-specialized folds over an `Iterable<Doc<A>>`; `concatWith` is the parameterized owner (`hcat`/`vcat`/`hsep`/`vsep`/`fillSep`/`fillCat`/`seps` are its fixed separator rows). Reach for the fold over manual `reduce`.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Doc.cat(self, that)` / `Doc.cats(docs)`               | concat         | pairwise / iterable line-or-space concatenation             |
|  [02]   | `Doc.concatWith(docs, f)`                              | fold           | reduce docs with a binary combiner (the parameterized owner)|
|  [03]   | `Doc.hcat`/`vcat`/`fillCat(docs)`                      | fold           | horizontal / vertical / fit-packed no-separator concat      |
|  [04]   | `Doc.hsep`/`vsep`/`fillSep`/`seps(docs)`               | fold           | space-separated horizontal / vertical / fit-packed / group  |
|  [05]   | `Doc.catWithSpace`/`catWithLine`/`catWithSoftLine(a,b)`| concat         | binary concat with a fixed separator                        |

[ENTRYPOINT_SCOPE]: layout combinators (`Doc`)
- rail: render
- The layout-decision surface: `group` is the fit-or-break primitive (tries the flattened form, falls back on overflow); `flatAlt`/`union` supply the two alternatives it chooses between. `nest`/`align`/`hang`/`indent` control indentation; `column`/`nesting`/`width`/`pageWidth` react to the current render position. All are dual (data-first and data-last) where they take a numeric or doc argument.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                                            |
| :-----: | :------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Doc.group(self)`                            | alternative    | render flattened if it fits, else keep line breaks               |
|  [02]   | `Doc.flatAlt(self, that)` / `Doc.union(a,b)` | alternative    | pick `that` when flattened / non-deterministic layout choice     |
|  [03]   | `Doc.nest(self, indent)`                     | indentation    | add `indent` to the nesting level of `self`                      |
|  [04]   | `Doc.align(self)` / `Doc.hang(self, i)`      | indentation    | nest to current column / hang subsequent lines at offset `i`     |
|  [05]   | `Doc.indent(self, i)`                        | indentation    | indent the whole document by `i` columns                         |
|  [06]   | `Doc.column(f)` / `Doc.nesting(f)`           | position       | build a doc from the current column / nesting level              |
|  [07]   | `Doc.width(self, f)` / `Doc.pageWidth(f)`    | position       | react to rendered width / to the active `PageWidth`              |

[ENTRYPOINT_SCOPE]: enclosure, fill, list (`Doc`)
- rail: render
- Delimited-collection and padding combinators; `encloseSep(docs, left, right, sep)` is the parameterized owner (`list` = brackets+comma, `tupled` = parens+comma are its fixed rows). `fill`/`fillBreak` pad to a target width; `punctuate` interleaves a trailing separator; `reflow`/`words` wrap prose to the page width.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Doc.encloseSep(docs, left, right, sep)`                        | enclosure      | delimited collection (the parameterized owner)              |
|  [02]   | `Doc.list(docs)` / `Doc.tupled(docs)`                           | enclosure      | `[a, b, c]` / `(a, b, c)` fixed-delimiter rows              |
|  [03]   | `Doc.surround(self, left, right)` / `Doc.parenthesized`/`doubleQuoted`/… | enclosure | wrap between two docs / fixed-delimiter wrappers            |
|  [04]   | `Doc.fill(self, w)` / `Doc.fillBreak(self, w)`                  | padding        | pad with spaces to width `w` (break past `w` for `fillBreak`)|
|  [05]   | `Doc.punctuate(docs, punctuator)`                              | separator      | append `punctuator` after all but the last doc              |
|  [06]   | `Doc.reflow(s, char?)` / `Doc.words(s, char?)`                 | prose          | split on `char` and lay out with `fillSep` / to doc list    |

[ENTRYPOINT_SCOPE]: annotation transforms (`Doc`)
- rail: render
- Bind or retarget the phantom `A`. `annotate` attaches an annotation; `reAnnotate` maps `A => B` uniformly; `alterAnnotations` maps one annotation to zero-or-many (remove/replace/split); `unAnnotate` erases to `Doc<never>`. This is the seam where a concrete renderer resolves `A`.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :--------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Doc.annotate(self, annotation)`         | annotation     | attach an `A` annotation to a subtree                       |
|  [02]   | `Doc.reAnnotate(self, f)`                 | annotation     | map every annotation `A => B` (`Doc<A> => Doc<B>`)          |
|  [03]   | `Doc.alterAnnotations(self, f)`           | annotation     | `A => Iterable<B>` remove/replace/multiply annotations      |
|  [04]   | `Doc.unAnnotate(self)`                     | annotation     | strip all annotations to `Doc<never>`                       |

[ENTRYPOINT_SCOPE]: layout algorithms + rendering
- rail: render
- `Layout.pretty`/`smart`/`compact` are the three lowering algorithms; `smart` looks further ahead than `pretty` before committing a break. `Doc.render` is the one-call façade discriminating on `Doc.RenderConfig` (`compact` | `pretty` | `smart` + options); `renderStream` folds an already-lowered `DocStream` to text. `Optimize.optimize` fuses associative `Cat`/`Text` nodes before layout.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Doc.render(self, config)` / `Doc.render(config)`              | render façade  | dual: lower + fold to `string` by `RenderConfig` style      |
|  [02]   | `Layout.pretty(self, options)` / `Layout.smart(self, options)` | layout         | lower `Doc<A>` to `DocStream<A>` with look-ahead            |
|  [03]   | `Layout.compact(self)` / `Layout.unbounded(self)`              | layout         | ignore width (compact single-column / never break)         |
|  [04]   | `Layout.options(pageWidth)` / `Layout.defaultOptions`          | config         | build `Layout.Options` / 80-col 1.0-ribbon default          |
|  [05]   | `PageWidth.availablePerLine(w, ribbon)` / `PageWidth.unbounded`| width          | bounded ribbon / unbounded width policy                     |
|  [06]   | `DocStream.renderStream(self)` / `Doc.renderStream(self)`      | render         | fold a `DocStream<A>` to `string` (annotations dropped)     |
|  [07]   | `Optimize.optimize(self, depth)`                              | fusion         | associativity-fuse (`FusionDepth.Shallow`\|`Deep`)          |
|  [08]   | `DocTree.treeForm(stream)` / `DocTree.renderSimplyDecorated(t, …)` | backend    | reparse a stream to `DocTree` / fold with scope callbacks   |

## [04]-[IMPLEMENTATION_LAW]

[PRINTER_TOPOLOGY]:
- `Doc<A>` is an immutable algebraic tree; `A` is a phantom annotation carried unchanged through every combinator and resolved only by a renderer or `reAnnotate`. Layout and annotation are orthogonal axes — never encode color/markup as text nodes.
- rendering is a two-stage pipeline: a layout algorithm (`pretty`/`smart`/`compact`) lowers `Doc<A>` to the `DocStream<A>` token stream, then `renderStream` folds the stream to `string`. `Doc.render` collapses both stages behind one `RenderConfig` discriminant.
- `group` is the sole fit-or-break decision point; layout width is governed by the active `PageWidth` (`AvailablePerLine` ribbon or `Unbounded`). Never compute column positions manually — use `column`/`nesting`/`width`/`pageWidth`.
- the separator folds (`hsep`/`vsep`/`fillSep`/`hcat`/`vcat`/`fillCat`/`seps`) are fixed rows over `concatWith`, and `list`/`tupled` are fixed rows over `encloseSep`; extend by choosing the parameterized owner with a new separator/delimiter, never by hand-rolling a `reduce`.

[STACKS_WITH]:
- `@effect/printer-ansi` (`.api/effect-printer-ansi.md`): the ANSI renderer instantiates `A = Ansi`. `AnsiDoc = Doc<Ansi>`; `Ansi.color`/`bold`/`underlined` are the concrete annotations that `Doc.annotate` attaches, and `AnsiDoc.render` (not `Doc.render`) is the terminal renderer that resolves `Ansi` to SGR escape codes via the `Push/PopAnnotation` stream events. Author documents once against abstract `Doc<A>`; bind `A` at the terminal edge.
- `@effect/cli` (`.api/effect-cli.md`): the `cli/render` rows compose `Doc` for help text, usage, and diagnostics; `@effect/cli`'s `HelpDoc` is `Doc`-shaped and its wizard/usage output lowers through `AnsiDoc.render`. A CLI verb's structured output is a `Doc<Ansi>` value the render row folds — never a pre-joined string.
- `effect` (`.api/effect.md`): the `Doc` union is refined with `Match.type<Doc<A>>().pipe(Match.tag(...))` for structure-aware backends; `Doc.getMonoid`/`getSemigroup` plug into `effect/Array.combineAll` folds, and `Doc.Covariant` into `effect/Effect.map`-style annotation retargeting. `Doc.render` is a pure `string` producer wrapped in `Effect.sync` at the effectful edge.

[LOCAL_ADMISSION]:
- The `cli/render` folder authors output as `Doc<Ansi>` (the `AnsiDoc` alias), composing structure with the concatenation/layout/enclosure combinators and semantic markup with `Doc.annotate`, then folds to a terminal string at the boundary with `AnsiDoc.render`. Prose columns use `reflow`/`fill`; tables use `align`/`encloseSep`.
- Keep `A` abstract in reusable render helpers (`Doc<A>` in, `Doc<A>` out); resolve `A` to `Ansi` only at the leaf where a color/weight is chosen, so the same document renders plain or styled by swapping the renderer.
- Choose the algorithm by intent: `pretty` for normal terminal output, `smart` for deeply-nested structures needing earlier breaks, `compact`/`Unbounded` for machine-readable single-line emission (log lines, `--json`-adjacent plain forms).

[RAIL_LAW]:
- Package: `@effect/printer`
- Owns: annotation-parametric document composition, layout algorithms over a `PageWidth`, intermediate `DocStream`/`DocTree` forms, associativity fusion, and annotation retargeting
- Accept: `Doc<A>` combinator composition, `concatWith`/`encloseSep` parameterized folds, `group`/`nest`/`align` layout control, `annotate`/`reAnnotate` markup binding, `Doc.render` with a `RenderConfig`
- Reject: manual string concatenation for structured output, hand-rolled line-wrapping or column math, encoding markup as literal text/escape codes, per-call `reduce` where a separator fold exists, resolving the annotation type before the terminal render edge
