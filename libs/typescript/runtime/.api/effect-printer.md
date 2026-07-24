# [TS_RUNTIME_API_EFFECT_PRINTER]

`@effect/printer` owns annotation-parametric Wadler/Leijen pretty-printing: `Doc<A>` is an immutable document tree whose phantom `A` a downstream renderer binds to concrete markup, composed through one combinator algebra and lowered to `string` by three width-driven layout algorithms over a `PageWidth`. It is the single document-composition owner behind every terminal render row; structured output composes as a `Doc`, never hand-joined strings or manual column math.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/printer`
- package: `@effect/printer` (MIT)
- asset: ESM `.d.ts` declaration surface (`dist/dts/*.d.ts`); peer `effect`
- owner: `edge`
- rail: render
- peer: `effect` (typeclass instances, `Order`, `Equal`, `Match` on the `Doc` union), `@effect/typeclass` (Semigroup/Monoid derivation)
- namespaces: `Doc` (the algebra + constructors + combinators + `render`), `DocStream` (post-layout token stream), `DocTree` (decorated tree form), `Layout` (algorithms + `Options`), `PageWidth` (width policy), `Optimize` (fusion), `Flatten` (flatten-analysis)
- import: `import { Doc, Layout, PageWidth, Optimize } from "@effect/printer"`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Doc core algebra (`Doc`)
- `Doc<A>` is a closed node union with `A` phantom until a renderer resolves it; every combinator preserves `A`, so annotation stays a distinct concern from layout. Refine a node with the `isX` guards or `Doc.match`, never by inspecting `_tag`.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]      | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------- | :----------------- | :------------------------------------------------------------- |
|  [01]   | `Doc.Doc<A>`                              | tagged union       | the closed node union rostered on the `[DOC]` line             |
|  [02]   | `Doc.DocTypeId`                           | unique symbol type | nominal brand carried by every node                            |
|  [03]   | `Doc.DocTypeLambda`                       | `TypeLambda`       | HKT witness for the `Covariant`/`Invariant` instances          |
|  [04]   | `Doc.FlatAlt` / `Doc.Union`               | node interface     | layout-alternative nodes the algorithms select between         |
|  [05]   | `Doc.Column` / `Doc.Nesting`              | node interface     | position-reactive nodes (`f: (int) => Doc<A>`)                 |
|  [06]   | `Doc.WithPageWidth`                       | node interface     | width-reactive node (`f: (PageWidth) => Doc<A>`)               |
|  [07]   | `Doc.Annotated`                           | node interface     | `{ annotation: A; doc: Doc<A> }` markup carrier                |
|  [08]   | `Doc.RenderConfig`                        | config union       | `render` discriminant: `compact` \| `pretty`/`smart` + options |
|  [09]   | `isDoc`/`isCat`/`isUnion`/`isAnnotated`/… | refinement         | `(u) => u is <Node>` guards over the union                     |

[DOC]: `Doc = |Fail|Empty|Char|Text|Line|FlatAlt|Cat|Nest|Union|Column|WithPageWidth|Nesting|Annotated`

[PUBLIC_TYPE_SCOPE]: intermediate + analysis forms
- Layout lowers `Doc<A>` to `DocStream<A>`, which `renderStream` folds to `string`. `DocTree` is the reparsed structure-aware form for markup backends needing nesting scopes, reached via `DocTree.treeForm`; `Flatten<A>` is the group-analysis deciding whether a subtree collapses to one line.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `DocStream.DocStream<A>` | tagged union  | `Failed\|Empty\|Char\|Text\|Line\|PushAnnotation\|PopAnnotation` token stream |
|  [02]   | `DocTree.DocTree<A>`     | tagged union  | `Empty\|Char\|Text\|Line\|Annotation\|Concat` decorated tree                  |
|  [03]   | `Flatten.Flatten<A>`     | tagged union  | `Flattened\|AlreadyFlat\|NeverFlat` group-collapse analysis                   |
|  [04]   | `Optimize.FusionDepth`   | tagged union  | `Shallow\|Deep` associativity-fusion depth                                    |

[PUBLIC_TYPE_SCOPE]: layout + width model
- A layout algorithm is a `Doc<A> => DocStream<A>` parameterized by `Layout.Options` carrying one `PageWidth`: `AvailablePerLine(lineWidth, ribbonFraction)` bounds the printable ribbon, `Unbounded` disables line-breaking for single-line output.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `Layout.Layout<A>`           | function alias | `(options: Layout.Options) => DocStream<A>`     |
|  [02]   | `Layout.Options`             | record         | `{ pageWidth: PageWidth }` layout configuration |
|  [03]   | `PageWidth.PageWidth`        | tagged union   | `AvailablePerLine\|Unbounded` width policy      |
|  [04]   | `PageWidth.AvailablePerLine` | node           | `{ lineWidth: number; ribbonFraction: number }` |

[PUBLIC_TYPE_SCOPE]: typeclass instances (annotation-parametric)
- Instances make `Doc<A>` a first-class monoidal, mappable value: fold documents with `effect/Monoid` and retarget annotations through the `Covariant` map rather than restructuring the tree.

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `Doc.getSemigroup<A>()` / `Doc.getMonoid<A>()` | instance      | concat/empty monoid over `Doc<A>` (identity = `Doc.empty`) |
|  [02]   | `Doc.Covariant` / `Doc.Invariant`              | instance      | map/imap the annotation type `A`                           |
|  [03]   | `DocStream.Functor` / `DocTree.Covariant`      | instance      | annotation map over the intermediate forms                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: primitives + character vocabulary (`Doc`)
- Leaf constructors and punctuation constants: `string` splits embedded newlines to `hardLine` while `char`/`text` assume none; `softLine` renders a space when the group fits and a break otherwise, `line` a space when flattened, `lineBreak` nothing when flattened.

| [INDEX] | [SURFACE]                                                   | [SHAPE]    | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------------------- | :--------- | :------------------------------------------------------- |
|  [01]   | `Doc.char(c)` / `Doc.text(s)` / `Doc.string(s)`             | primitive  | single char / newline-free text / newline-splitting text |
|  [02]   | `Doc.empty` / `Doc.fail`                                    | primitive  | identity element / layout-abort sentinel                 |
|  [03]   | `Doc.line` / `Doc.lineBreak` / `Doc.hardLine`               | primitive  | flatten→space / flatten→empty / unconditional break      |
|  [04]   | `Doc.softLine` / `Doc.softLineBreak`                        | primitive  | fit-aware break (space / nothing when it fits)           |
|  [05]   | `lparen`/`rparen`/`comma`/`colon`/`space`/`vbar`/`dquote`/… | vocabulary | single-char `Doc` punctuation constants                  |

[ENTRYPOINT_SCOPE]: concatenation family (`Doc`)
- `concatWith` is the parameterized fold over an `Iterable<Doc<A>>`; `hcat`/`vcat`/`hsep`/`vsep`/`fillSep`/`fillCat`/`seps` are its fixed separator rows and `cat` the pairwise concat. Reach for the fold over a manual `reduce`.

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------------ | :------ | :----------------------------------------------------------- |
|  [01]   | `Doc.cat(self, that)` / `Doc.cats(docs)`                | concat  | pairwise / iterable line-or-space concatenation              |
|  [02]   | `Doc.concatWith(docs, f)`                               | fold    | reduce docs with a binary combiner (the parameterized owner) |
|  [03]   | `Doc.hcat`/`vcat`/`fillCat(docs)`                       | fold    | horizontal / vertical / fit-packed no-separator concat       |
|  [04]   | `Doc.hsep`/`vsep`/`fillSep`/`seps(docs)`                | fold    | space-separated horizontal / vertical / fit-packed / group   |
|  [05]   | `Doc.catWithSpace`/`catWithLine`/`catWithSoftLine(a,b)` | concat  | binary concat with a fixed separator                         |

[ENTRYPOINT_SCOPE]: layout combinators (`Doc`)
- `group` is the fit-or-break primitive, choosing between the `flatAlt`/`union` alternatives; `nest`/`align`/`hang`/`indent` drive indentation and `column`/`nesting`/`width`/`pageWidth` react to the current render position. Each is dual (data-first and data-last) where it takes a numeric or doc argument.

| [INDEX] | [SURFACE]                                    | [SHAPE]     | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------- | :---------- | :----------------------------------------------------------- |
|  [01]   | `Doc.group(self)`                            | alternative | render flattened if it fits, else keep line breaks           |
|  [02]   | `Doc.flatAlt(self, that)` / `Doc.union(a,b)` | alternative | pick `that` when flattened / non-deterministic layout choice |
|  [03]   | `Doc.nest(self, indent)`                     | indentation | add `indent` to the nesting level of `self`                  |
|  [04]   | `Doc.align(self)` / `Doc.hang(self, i)`      | indentation | nest to current column / hang subsequent lines at offset `i` |
|  [05]   | `Doc.indent(self, i)`                        | indentation | indent the whole document by `i` columns                     |
|  [06]   | `Doc.column(f)` / `Doc.nesting(f)`           | position    | build a doc from the current column / nesting level          |
|  [07]   | `Doc.width(self, f)` / `Doc.pageWidth(f)`    | position    | react to rendered width / to the active `PageWidth`          |

[ENTRYPOINT_SCOPE]: enclosure, fill, list (`Doc`)
- `encloseSep(docs, left, right, sep)` is the parameterized delimiter fold; `list` (brackets+comma) and `tupled` (parens+comma) are its fixed rows. `fill`/`fillBreak` pad to a width, `punctuate` interleaves a trailing separator, `reflow`/`words` wrap prose to the page width.

| [INDEX] | [SURFACE]                                      | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------- | :-------- | :----------------------------------------------- |
|  [01]   | `Doc.encloseSep(docs, left, right, sep)`       | enclosure | delimited collection (the parameterized owner)   |
|  [02]   | `Doc.list(docs)` / `Doc.tupled(docs)`          | enclosure | `[a, b, c]` / `(a, b, c)` fixed-delimiter rows   |
|  [03]   | `Doc.surround(self, left, right)`              | enclosure | wrap a doc between two delimiter docs            |
|  [04]   | `Doc.parenthesized`/`doubleQuoted`/…           | enclosure | fixed-delimiter wrappers                         |
|  [05]   | `Doc.fill(self, w)` / `Doc.fillBreak(self, w)` | padding   | pad to width `w`; `fillBreak` breaks past `w`    |
|  [06]   | `Doc.punctuate(docs, punctuator)`              | separator | append `punctuator` after all but the last doc   |
|  [07]   | `Doc.reflow(s, char?)` / `Doc.words(s, char?)` | prose     | split on `char` → `fillSep` layout / to doc list |

[ENTRYPOINT_SCOPE]: annotation transforms (`Doc`)
- Bind or retarget the phantom `A`: `annotate` attaches one, `reAnnotate` maps `A => B` uniformly, `alterAnnotations` maps one annotation to zero-or-many, `unAnnotate` erases to `Doc<never>`. This is the seam where a concrete renderer resolves `A`.

| [INDEX] | [SURFACE]                        | [SHAPE]    | [CAPABILITY]                                           |
| :-----: | :------------------------------- | :--------- | :----------------------------------------------------- |
|  [01]   | `Doc.annotate(self, annotation)` | annotation | attach an `A` annotation to a subtree                  |
|  [02]   | `Doc.reAnnotate(self, f)`        | annotation | map every annotation `A => B` (`Doc<A> => Doc<B>`)     |
|  [03]   | `Doc.alterAnnotations(self, f)`  | annotation | `A => Iterable<B>` remove/replace/multiply annotations |
|  [04]   | `Doc.unAnnotate(self)`           | annotation | strip all annotations to `Doc<never>`                  |

[ENTRYPOINT_SCOPE]: layout algorithms + rendering
- `Layout.pretty`/`smart`/`compact` are the three lowering algorithms, `smart` looking further ahead than `pretty` before committing a break. `Doc.render` is the one-call façade discriminating on `Doc.RenderConfig`; `renderStream` folds an already-lowered `DocStream` to text, and `Optimize.optimize` fuses associative `Cat`/`Text` nodes before layout.

| [INDEX] | [SURFACE]                                                 | [SHAPE]       | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `Doc.render(self, config)` / `Doc.render(config)`         | render façade | dual: lower + fold to `string` by `RenderConfig` style  |
|  [02]   | `Layout.pretty(self, options)`                            | layout        | lower `Doc<A>` to `DocStream<A>` (standard)             |
|  [03]   | `Layout.smart(self, options)`                             | layout        | lower with deeper break look-ahead                      |
|  [04]   | `Layout.compact(self)` / `Layout.unbounded(self)`         | layout        | ignore width (compact single-column / never break)      |
|  [05]   | `Layout.options(pageWidth)` / `Layout.defaultOptions`     | config        | build `Layout.Options` / 80-col 1.0-ribbon default      |
|  [06]   | `PageWidth.availablePerLine(w, ribbon)`                   | width         | bounded ribbon width policy                             |
|  [07]   | `PageWidth.unbounded`                                     | width         | unbounded width policy (never break)                    |
|  [08]   | `DocStream.renderStream(self)` / `Doc.renderStream(self)` | render        | fold a `DocStream<A>` to `string` (annotations dropped) |
|  [09]   | `Optimize.optimize(self, depth)`                          | fusion        | associativity-fuse (`FusionDepth.Shallow`\|`Deep`)      |
|  [10]   | `DocTree.treeForm(stream)`                                | backend       | reparse a `DocStream` to `DocTree`                      |
|  [11]   | `DocTree.renderSimplyDecorated(t, …)`                     | backend       | fold a `DocTree` with scope callbacks                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Doc<A>` is an immutable algebraic tree; `A` is a phantom annotation carried unchanged through every combinator and resolved only by a renderer or `reAnnotate`, so layout and annotation are orthogonal axes and markup never encodes as text nodes.
- A layout algorithm (`pretty`/`smart`/`compact`) lowers `Doc<A>` to the `DocStream<A>` token stream, then `renderStream` folds it to `string`; `Doc.render` collapses both stages behind one `RenderConfig` discriminant.
- `group` is the sole fit-or-break decision point, and the active `PageWidth` (`AvailablePerLine` ribbon or `Unbounded`) governs layout width; `column`/`nesting`/`width`/`pageWidth` read render position rather than hand-computed columns.
- Separator folds (`hsep`/`vsep`/`fillSep`/`hcat`/`vcat`/`fillCat`/`seps`) are fixed rows over `concatWith` and `list`/`tupled` fixed rows over `encloseSep`; a new separator or delimiter extends the parameterized owner rather than a hand-rolled `reduce`.

[STACKING]:
- `@effect/printer-ansi` (`.api/effect-printer-ansi.md`): instantiates `A = Ansi` and aliases `AnsiDoc = Doc<Ansi>`; `Ansi.color`/`bold`/`underlined` are the concrete annotations `Doc.annotate` attaches, and `AnsiDoc.render` (not `Doc.render`) resolves `Ansi` to SGR escapes through the `Push`/`PopAnnotation` stream events. Author against abstract `Doc<A>`, bind `A` at the terminal edge.
- `@effect/cli` (`.api/effect-cli.md`): folds its `Doc`-shaped `HelpDoc`, usage, and diagnostics through `AnsiDoc.render`, so a verb's structured output is a `Doc<Ansi>` value the `cli/render` rows lower, never a pre-joined string.
- `effect` (`.api/effect.md`): refines the `Doc` union through `Match.type<Doc<A>>().pipe(Match.tag(...))`, folds `Doc.getMonoid`/`getSemigroup` into `effect/Array.combineAll`, retargets annotations through `Doc.Covariant`, and wraps the pure-`string` `Doc.render` in `Effect.sync` at the effectful edge.

[LOCAL_ADMISSION]:
- `cli/render` folder authors output as `Doc<Ansi>` (`AnsiDoc`): structure through the concatenation/layout/enclosure combinators, semantic markup through `Doc.annotate`, folded to a terminal string with `AnsiDoc.render`; prose columns take `reflow`/`fill`, tables `align`/`encloseSep`.
- Reusable render helpers keep `A` abstract (`Doc<A>` in, `Doc<A>` out), resolving `A` to `Ansi` only at the leaf choosing a color or weight, so one document renders plain or styled by swapping the renderer.
- Algorithm follows intent: `pretty` for normal terminal output, `smart` for deeply-nested structures needing earlier breaks, `compact`/`Unbounded` for machine-readable single-line emission.

[RAIL_LAW]:
- Package: `@effect/printer`
- Owns: annotation-parametric document composition, layout algorithms over a `PageWidth`, intermediate `DocStream`/`DocTree` forms, associativity fusion, and annotation retargeting
- Accept: `Doc<A>` combinator composition, `concatWith`/`encloseSep` parameterized folds, `group`/`nest`/`align` layout control, `annotate`/`reAnnotate` markup binding, `Doc.render` with a `RenderConfig`
- Reject: manual string concatenation for structured output, hand-rolled line-wrapping or column math, encoding markup as literal text/escape codes, per-call `reduce` where a separator fold exists, resolving the annotation type before the terminal render edge
