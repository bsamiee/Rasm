# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence ride one tagged-union fact stream the `assay code` rail consumes. `Evidence` is the single slot/kind union whose three cases carry frozen value objects, never positional tuples: `member` a `MemberFact` (one row of the official distribution surface — entry point, importable member, or import root — a source may later name), `span` a `SpanFact` (one `tree-sitter` capture), and `drift` a `DriftFact` (a cross-language re-mint of a canonical wire-projection name the topology law forbids). The byte/`Point` extent every span-shaped fact carries collapses into one `Locus` value object, so `SpanFact` and `DriftFact` share a single `locus` field rather than re-declaring a `(lang, point, start_byte)` triple each, and every fold reads `fact.text`/`fact.locus.lang` by name.

`GrammarRegistry.scan` is the one polymorphic structural-extraction entry, parameterized over the probe, the multi-language `Corpus` input, the `Disposition` traversal output, and the `Into[R]` projector. Any probe — the `binding` declaration alternation, the grammar-bundled `tags`/`highlights`, or the TypeScript-dialect-only `locals` — runs through the same `scan`, so every bundled source is a first-class `Evidence` producer, not a compiled-but-unreachable column, and the Python-absent `locals` column is the live proof of the partial-coverage compile. `drift` is a `binding` scan post-filtered to the canonical names; `run` is the single-corpus leg `scan` folds across the corpus through `traversed` under one `code.scan` parent span.

`GrammarRegistry` is the reused-`Parser`/compile-once owner the `tree-sitter` parsing law demands. Grammars wrap once into a `Map[Lang, Grammar]` row, each probe's per-grammar S-expression source compiles once against its own grammar through `CompiledProbe.of`, and a probe missing a language column compiles total over its covered keys via `Map.filter` before the compile `map`. The build resolves every capture name to its integer id once (`Query.capture_count`/`capture_name(i)` into the per-grammar `captures` table) and prunes non-load-bearing captures via `Query.disable_capture` off the `PROBE_KEEP` allowlist, so the hot fold and the drift filter key on integers and the `binding` probe's whole-declaration `@decl` span never materializes at run time. A node kind absent from a grammar's symbol table never reaches that grammar's `Query`, so no cross-grammar pattern raises `QueryError` at build and no `sources[lang]` index raises `KeyError` over an uncovered grammar.

`ApiCatalogue.reflect` reflects one distribution's full official surface — the `importlib.metadata` entry points AND the importable member inventory of every top-module root the reversed `packages_distributions` rows map to the distribution — into `member` evidence through the canonical `reliability/faults#FAULT` `@trapped("reflect", catch=ImportError)` aspect — the faults owner's signature-preserving `iscoroutinefunction`-dispatched `_guard`/`async_boundary`-weaving decorator, the definition-time aspect form the `surfaces-and-dispatch#ASPECTS` law prefers over an inline `boundary` lambda at every call site — so a missing distribution or a failing top-module import lands as one `BoundaryFault` `import_` row the faults-owner `CLASSIFY` `ImportError` dispatch mints, the `PackageNotFoundError` and `ModuleNotFoundError` subclasses routing through that same row, never a raised exception and never a re-tag. Each `MemberFact` carries the distribution `version` and its real group — `ep.group` for an entry point, the `_GROUP_MEMBER` row for a walked public symbol, `_GROUP_IMPORT` for the import root — never a caller-supplied family constant repeated per member, so the page's own Boundary claim holds: a source cannot name a member absent from the catalogue.

The page weaves five admitted surfaces as one structural rail: `tree-sitter` (the reused `Parser`, the per-grammar compiled `Query` with its build-time capture-id table and `disable_capture` prune, the `Node.descendant_count` budget signal, the `Node.has_error` parse-health fact, the `Point`/byte extents), `msgspec` (the `Locus`/`SpanFact`/`MemberFact`/`DriftFact` frozen value objects, `gc=False` on the container-free `MemberFact`), `expression` (the `Evidence` `@tagged_union`, the `Block`/`Map` drift fold, `identity` the flatten spelling, the `Option`-folded `__all__` walk), `opentelemetry-api` (the per-file `code.query` span nested under the one `code.scan` corpus span, attributes batched through `set_attributes` under an `is_recording()` gate plus a total `Status(StatusCode.OK | ERROR)` on both), and runtime `reliability/faults#FAULT`/`observability/receipts#RECEIPT` (the `@trapped` import lift, the `SCOPES[Scope.EVIDENCE]`-minted tracer, the `Disposition`-keyed multi-grammar `traversed` fold, the `EvidenceScan` `ReceiptContributor` streaming the scan onto the one receipt rail). A structural scan is therefore a traced, fault-railed, receipted leg the way the sibling `evidence/identity#IDENTITY` and `transport/wire#WIRE_RAIL` pages are.

The surface produces evidence the rail reads, never a competing search owner, a guessed environment status, an exception into domain flow, an untraced parse, or a span whose trace status disagrees with its rail outcome.

## [01]-[INDEX]

- [01]-[EVIDENCE]: the value-object-carrying evidence union over one shared `Locus`, the grammar registry with reused parsers, compile-once probes, the build-time capture-id table and `PROBE_KEEP` prune, the probe table compiled total over partial coverage with `locals` the live partial column, the one `scan` entry polymorphic over probe/corpus/disposition/output under one `code.scan` parent span, the member-level API catalogue, the integer-keyed cross-language drift fold, the `EvidenceScan` receipt seam.

## [02]-[EVIDENCE]

- Owner: `Evidence` — the one slot/kind evidence union the `assay code` rail consumes, its `member`/`span`/`drift` cases carrying the `MemberFact`/`SpanFact`/`DriftFact` frozen value objects (the API-surface row, the structural-capture extent, the cross-language re-mint defect); `Locus` the shared extent value object (`lang`, the `tree_sitter.Point`, the `Byte` byte offsets) both `SpanFact` and `DriftFact` carry by one `locus` field; `Lang` the closed grammar discriminant, `Probe` the closed probe-name discriminant, `Corpus` the multi-language `(Lang, bytes)` input row, `Into[R]` the output-projector alias, `Byte` the `Meta(ge=0)`-bounded offset; `Grammar` the value object binding one `Language` to its single reused `Parser`; `CompiledProbe` the value object binding one named probe to its per-grammar `Query` compiled once through `PROBE_SOURCES` plus its per-grammar `captures` `Map[str, int]` id table resolved at that same build, the `PROBE_KEEP` allowlist row pruning non-load-bearing captures via `disable_capture` before any run; `GrammarRegistry` the static owner folding the `GRAMMARS` rows into the reused-parser/compile-once registry and owning the one `scan` extraction entry over the single-corpus `run`; `ApiCatalogue` the static surface reflecting one distribution — entry points, the importable members of every `packages_distributions`-mapped root, and the import roots — into `member` evidence through the canonical `@trapped("reflect", catch=ImportError)` faults aspect; `EvidenceScan` the `ReceiptContributor`-shaped value object folding a scan's evidence into one `emitted` `Receipt`; the page's one tracer mints from the `reliability/faults#FAULT` `SCOPES[Scope.EVIDENCE]` row, never a page literal.
- Cases: the three evidence families collapse into one union whose cases carry frozen value objects, not positional tuples — `span` carries a `SpanFact(capture, capture_id, text, locus)` (the capture name for the receipt reader, the build-resolved integer id for the hot fold), `drift` a `DriftFact(name, bindings, locus)`, and `member` a `MemberFact(distribution, version, import_name, group, symbol)` — `MemberFact` the one container-free `gc=False` leaf the `msgspec.md` topology law admits, the span-shaped records staying GC-tracked because `Point`, the nested `Locus`, and `frozenset[Lang]` are container fields the `gc=False` admission excludes. `Locus(lang, point, start_byte, end_byte)` is the one extent value object both span-shaped facts share: the `tree_sitter.Point` `(row, column)` read straight off `Node.start_point` rather than a re-minted `tuple[int, int]` alias the package already owns, the byte offsets `Meta(ge=0)`-bounded `Byte`. A structural capture and a cross-language re-mint are one self-describing extent fact discriminated by whether the carried name binds in more than one grammar namespace; the name rides on the `SpanFact.text` field read once off `Node.text`, so the `_cross` fold and the canonical filter read `fact.text`/`fact.locus.lang` by name and `fact.capture_id` by integer, never re-touch the source buffer. Correlation flows through the `EvidenceScan` receipt and the `assay code` receipt the rail mints, never a per-case id field.
- Entry: `Evidence.of` is the one capture-keyed factory minting a `span` `Evidence` carrying a `SpanFact` from a `(Lang, capture, capture_id, Node)` quad, reading the bound text off `node.text` and the extent through `Locus.of` off `node.start_point`/`start_byte`/`end_byte` rather than recomputing positions from the source buffer. `GrammarRegistry.scan` is the one polymorphic structural-extraction entry, parameterized over both input and output: the `Probe`, the `Corpus` multi-language `(Lang, bytes)` `Block`, the `Disposition` that grades the multi-grammar outcome AND selects the return shape, and the `Into[R]` projector — `Evidence.of` (the default) for the receipt rail's `Evidence` stream, `SpanFact.of` for the drift fold's typed-field `SpanFact`, both sharing the one `SpanFact.of` extent-extraction primitive `Evidence.of` delegates to. `scan` opens the one `code.scan` parent span over the whole corpus fold — probe and corpus-size attributes batched behind `is_recording()`, its status set from the traversal outcome through the `Result.default_with` fault fold so the N nested per-file `code.query` spans hang under one traced leg whose status and rail agree — then folds `run` across the probe's COVERED corpus columns through `traversed(rails, by=by)` and flattens the nested `Block` through `Block.collect(identity)`. Coverage is registry data on both entries: `scan` pre-filters the corpus on `row[0] in PROBES[probe].queries` so a `locals` scan over a Python row yields no evidence and no fault, while a direct `run` call on an uncovered language returns the typed `config` `uncovered-grammar` fault rather than escaping a `queries[lang]` `KeyError` into domain flow. The default `Disposition.ACCUMULATE` combines a parse fault on one grammar rather than aborting the others and returns `RuntimeRail[Block[R]]`, while `Disposition.PARTITION` returns the `RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]` split a corpus-health scan reads to keep the parsed evidence and the per-file faults on one pass; the output shape is carried statically by `@overload` arms keyed on the `Disposition` `Literal` exactly as the faults owner's `traversed` carries it — `scan` is the disposition-keyed mirror of `traversed`, never a second widened return forcing a caller re-narrow. `GrammarRegistry.run` is the single-corpus leg, running one `QueryCursor` over the reused `Parser`'s `Tree` under one `code.query` span whose `probe`/`lang`/`nodes`/`captures`/`truncated`/`flawed` attributes batch through `set_attributes` only behind an `is_recording()` gate, with `set_byte_range`/`set_max_start_depth` scoping the run and the probe's build-resolved `captures` id table keying each emitted fact. The run grades on two observable axes plus one hang guard. The `deadline` arm is a deterministic PRE-gate: `tree.root_node.descendant_count` is a static tree-size signal known the instant the parse returns, so a tree at or over `budget` short-circuits to `Status(StatusCode.ERROR, "deadline")` plus a `BoundaryFault` `deadline` row carrying the budget BEFORE the query runs. The `QueryCursor` is built `match_limit=budget` so `did_exceed_match_limit` is a LIVE post-query grade: `True` sets `Status(StatusCode.ERROR, "resource")` plus a `resource` row, otherwise the run sets `Status(StatusCode.OK)` and returns the captured `Ok` `Block`. `progress_callback` is the third axis — the cancel hook whose predicate receives the cursor's byte offset and cancels by returning `True`, so the guard cancels on `next(ticks) >= budget` over a run-local `itertools.count` tick stream — whose cancel tree-sitter surfaces as a bounded partial result rather than a grade, so the size pre-gate is sized to keep it unreachable and the ceiling stays a hang guard, not a rail outcome. Parse health rides the same discipline: `parse` never raises, a syntactically broken source yields a `Tree` whose `root_node.has_error` flags the recovery-law fact, and `run` surfaces it as the `evidence.flawed` span attribute — a traced corpus-health signal, never a rail fault, since a flawed tree still produces admissible captures. The trace status and the rail outcome are the same fact, and the grade arms return `Error` without raising, so `run` sets the span status itself rather than the faults owner's raised-exception weave. `GrammarRegistry.drift` is the `binding` scan post-filtered to the `canonical` names, its capture filter an integer compare against the build-derived `_BINDING_NAME_ID` row (the build prune already scopes the binding scan to `@name` captures, so the id compare is the belt on a build-guaranteed brace), folding the captured `SpanFact` bindings into a persistent name-to-languages `Map` through `Block.fold`/`Map.change` and emitting one `drift` `Evidence` per binding only where the carried name binds across more than one grammar namespace. `ApiCatalogue.reflect` carries the `@trapped("reflect", catch=ImportError)` faults aspect directly so the faults-owner `CLASSIFY` `ImportError` row lands the lift in the typed `import_` case (no tuple of exception types and no second `map_error` re-tag, since `PackageNotFoundError` and `ModuleNotFoundError` are `ImportError` subclasses `CLASSIFY` already routes), returning `RuntimeRail[Block[Evidence]]` that mines the FULL surface: one `member` per entry point (the `dist.version`, the `ep.group` family, the `ep.name` symbol, the `ep.module` top segment as its `import_name`), one `member` per importable public symbol of every top-module root the reversed `packages_distributions` rows resolve — `import_module(root)` under the same trapped fence, the symbol roster the `Option`-folded `__all__` read with the underscore-filtered `vars` walk as the `default_with` fallback — under the `_GROUP_MEMBER` group, plus one `_GROUP_IMPORT` row per root. `EvidenceScan.contribute` folds a scan's `Block[Evidence]` into one `emitted` `Receipt` carrying the per-kind tally and the multi-namespace drift names through `Receipt.of`, so a structural scan streams onto the one `observability/receipts#RECEIPT` rail rather than minting a private evidence log.
- Auto: `GRAMMARS` is the `Map[Lang, Grammar]` built once at module import — each `Grammar` wraps its capsule exactly once (`Language(tree_sitter_python.language())`, `Language(tree_sitter_typescript.language_typescript())`, `Language(tree_sitter_typescript.language_tsx())`, the deprecated `int`-pointer/`Language.query` forms refused) and constructs the one reusable `Parser` bound to that grammar, the reused-parser-per-grammar topology the `runtime/.api/tree-sitter.md` `[PARSING_TOPOLOGY]`/`[LOCAL_ADMISSION]` law fixes against per-parse construction. `PROBE_SOURCES` is the `Map[Probe, Map[Lang, str]]` pairing each probe name with its per-grammar S-expression source, and `PROBES` is the `Map[Probe, CompiledProbe]` `PROBE_SOURCES.map`s into compiled queries — `CompiledProbe.of` compiles each grammar against ITS OWN-language source by `Map.filter`ing the grammars to the covered keys (`lang in sources`) then `map`ping `Query(grammar.language, sources[lang])`, prunes each compiled query to its `PROBE_KEEP` allowlist through `disable_capture` (the `binding` row keeps only `_CAPTURE_NAME`, so the `@decl` whole-declaration capture never allocates a node list at run time), and enumerates `Query.capture_count`/`capture_name(i)` into the per-grammar `captures` id table, so compile cost, capture pruning, and name-to-id resolution are all paid once per grammar at import, never per run. The compile is total over covered keys: the cross-grammar `[...]` alternation mixing Python `class_definition`/`type_alias_statement` with TypeScript `class_declaration`/`interface_declaration`/`type_alias_declaration` against one `pattern` was a `QueryError` at compile for every grammar (the deleted form), and a probe omitting a language column compiles over its covered languages rather than raising `KeyError` on a `sources[lang]` index — `locals` is the LIVE partial column, the TypeScript grammar's bundled scope-resolution source (`ts_ts.LOCALS_QUERY`) the Python grammar does not ship, reaching `scan` with zero new entry. The `binding` probe is the per-grammar declaration alternation capturing `(identifier) @name` (`_BINDING_PY` the Python arm — its type-alias pattern anchored `left:` so the aliased VALUE identifier in `type Alias = int` never emits as a declared binding — `_BINDING_TS` the shared `class_declaration`/`interface_declaration`/`type_alias_declaration` arm both TypeScript dialects bind) scoped by the grammar to exactly its own declaration nodes; the `tags`/`highlights` probes pull each grammar's OWN bundled `TAGS_QUERY`/`HIGHLIGHTS_QUERY` `Final[str]` source (`ts_py` for Python, `ts_ts` for both TypeScript dialects) the grammar packages ship lazily through `importlib.resources`, never a Python tags source compiled against the TypeScript grammar; the bundled sources carry only the internally-evaluated built-in `#match?` predicate, so no `QueryPredicate` handler threads through `captures` — a custom-directive probe earns that handler as its own row, never a speculative parameter. `run` reads the bound name off `Node.text` and the extent through `Locus.of` off `Node.start_point`/`start_byte`/`end_byte`, opens the `code.query` span around the parse-and-capture leg behind an `is_recording()` gate batched through `set_attributes`, gates the `deadline` arm on the static `tree.root_node.descendant_count >= budget` tree-size signal BEFORE running the query, builds the `QueryCursor` with `match_limit=budget` so truncation is a detectable `did_exceed_match_limit` grade rather than the dead default-`0xFFFFFFFF` cap, caps the run with the `progress_callback` cancel hook (the modern hang guard the law mandates over the `@deprecated` `timeout_micros`): the callback fires with the cursor's byte offset and `True` CANCELS, so the guard cancels on `next(ticks) >= budget` over a run-local `itertools.count` — a tick ceiling, never an offset compare, since a healthy large file legitimately outruns any offset bound — and stamps `evidence.flawed` off `tree.root_node.has_error` so per-file parse health is a traced fact of every clean run; the post-query grade is the single `did_exceed_match_limit` check, and the grading folds into `run` rather than a single-caller helper threading wide positional state. `scan` wraps the corpus fold in the one `code.scan` parent span (probe + corpus/covered-size attributes, status folded from the traversal rail through `default_with`), filters the `Corpus` to the probe's covered columns, maps `run` across the survivors, threads the rails through `traversed(rails, by=by)` so the `Disposition` row selects the multi-grammar output shape, and flattens the nested `Block` through `Block.collect(identity)` — the `PARTITION` arm flattening only the ok-block of the `(oks, faults)` split and threading the fault block through, gated on `by is Disposition.PARTITION` so the block and tuple return shapes never cross. `drift` filters the `binding` scan's spans on `fact.capture_id == _BINDING_NAME_ID[fact.locus.lang] and fact.text in canonical` — `_BINDING_NAME_ID` the `Map[Lang, int]` derived off the binding probe's `captures` table at import, so the per-fact compare is integer-keyed — then folds them into the persistent `Map[str, frozenset[Lang]]` through `Block.fold`/`Map.change` reading `fact.text`/`fact.locus.lang` by name so a legitimately distinct same-named concept in one namespace yields no defect and a re-minted identity seed, receipt rail, or capability descriptor across Python and TypeScript yields one `drift` row per binding location of a multi-namespace name — a false positive filtered by namespace multiplicity, never a blanket name match, and the fold persistent rather than a mutated accumulator. `ApiCatalogue.reflect` binds the canonical `reliability/faults#FAULT` `@trapped("reflect", catch=ImportError)` aspect (the faults owner's signature-preserving `_guard`/`async_boundary`-weaving definition-time decorator per the `surfaces-and-dispatch#ASPECTS` law) so the lift returns `RuntimeRail` with every raise of the metadata read AND the top-module import converted to a `BoundaryFault` `import_` row exactly once through the single `catch=ImportError`; the per-root `_mined` walk is expression-shaped — `Option.of_optional` lifts the root's `__all__`, `map(Block.of_seq)` admits the declared export roster, and `default_with` falls back to the underscore-filtered `vars(surface)` public names — the roots themselves the sorted reversed `packages_distributions` rows (the dash-to-underscore normalization only the no-row fallback), and `import_module` here IS the domain capability (reflection over a foreign distribution), the one dynamic-import site the page owns, inside the trapped boundary kernel and nowhere else. `EvidenceScan.contribute` folds the scan's `Block[Evidence]` into a per-tag count `Map` through `Map.change` keyed `f"count.{ev.tag}"` and drains the `drift` rows through one `Block.choose` yielding `f"drift.{name}"`-keyed pairs, the disjoint `count.*` and `drift.*` prefixes keeping any drifting symbol name (even one literally named `member`/`span`/`drift`/`count`) from colliding with a tally key in the merged flat fact map, then mints one `emitted` `Receipt` through `Receipt.of` the `observability/receipts#RECEIPT` `Signals.emit` consumes, so the scan satisfies the `ReceiptContributor` Protocol structurally exactly as the sibling `evidence/reproduction#SEED_REPRODUCTION` `SeedReproduction` does; the `canonical` set arrives as the shared one-name-one-owner registry the topology law owns, never re-minted here.
- Packages: `importlib` (`metadata.distribution`, `metadata.packages_distributions` the top-module reverse rows, `Distribution.entry_points`, `Distribution.version`, `EntryPoint.group`/`EntryPoint.name`/`EntryPoint.module`, `PackageNotFoundError`, `import_module` the trapped-fenced member-walk import), `itertools` (`count` the progress-tick cancel guard), `tree-sitter` (`Language`/`Parser.parse`/`Query`/`Query.capture_count`/`Query.capture_name`/`Query.disable_capture`/`QueryCursor(query, match_limit=budget)`/`QueryCursor.captures`/`set_byte_range`/`set_max_start_depth`/`did_exceed_match_limit`/`Point`/`Node.text`/`Node.start_point`/`Node.start_byte`/`Node.end_byte`/`Node.descendant_count`/`Node.has_error`), `tree-sitter-python` (`language`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`), `tree-sitter-typescript` (`language_typescript`/`language_tsx`/`TAGS_QUERY`/`HIGHLIGHTS_QUERY`/`LOCALS_QUERY`), `expression` (`Ok`/`Error`/`Some`/`Nothing`/`Option.of_optional`/`Option.default_value`/`Option.default_with`/`identity`/`Block.of_seq`/`Block.cons`/`Block.append`/`Block.choose`/`Block.filter`/`Block.collect`/`Block.map`/`Block.fold`/`Map.of_seq`/`Map.empty`/`Map.filter`/`Map.map`/`Map.change`/`Map.try_find`/`tagged_union`/`case`/`tag`), `msgspec` (`Struct`, `Meta` the `Byte` offset bound, `gc=False` on the container-free `MemberFact` leaf — `Point`, the nested `Locus`, and `frozenset[Lang]` keep the span-shaped records GC-tracked), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.set_attributes`/`Span.is_recording`/`Span.set_status`/`Status`/`StatusCode` — the `code.scan` corpus span over the per-file `code.query` spans, both with a total OK/ERROR status), `rasm.runtime.faults` (`RuntimeRail`/`BoundaryFault`/`Disposition`/`Scope`/`SCOPES`/`trapped`/`traversed` — the `@trapped` import-lift aspect, the `SCOPES[Scope.EVIDENCE]` tracer row, the `Disposition`-keyed traversal), `rasm.runtime.receipts` (`Receipt` the `EvidenceScan` contribution mints).
- Growth: a new evidence family is one `Evidence` case carrying its own value object plus one match arm; a new language is one `Lang` member, one `GRAMMARS` `Grammar` row binding its capsule and reused parser, and one own-language source column per `PROBE_SOURCES` probe, each compiling, pruning, and id-resolving once against the new grammar through the `Map.filter`-total `CompiledProbe.of`; a new structural probe is one `Probe` literal member plus one `PROBE_SOURCES` row carrying its per-grammar source (partial coverage included, as `locals` proves), reached by `scan` with zero new entry; a new capture allowlist is one `PROBE_KEEP` row pruned at the same build; a new traversal output shape is one `Disposition` member the faults owner adds, threaded by `scan`'s `by` parameter; a new canonical name is one entry the caller adds to the `canonical` set; a new mined member field is one field on the `MemberFact` value object and a new member group is one `_GROUP_*` constant beside the existing rows; a distribution shipping a second top module lands through the same reversed `packages_distributions` read with zero page edits; a custom-predicate probe source earns the `QueryPredicate` handler as one row-borne addition when a non-built-in directive actually ships; the per-tag scan tally absorbs a new evidence case through the existing `Map.change` fold with zero new arm; zero new surface, zero new parser class, zero per-language parser branch.
- Boundary: no package version tables in planning pages, no guessed environment status, no parallel canonical-name registry minted here, one fault rail and one tracer minted from the `SCOPES[Scope.EVIDENCE]` row (`code.scan` the corpus parent, `code.query` the per-file derivation span — never a second tracer per probe); a source cannot name a member absent from the catalogue evidence, and the catalogue now carries the member inventory that claim requires; the scan is ONE-SHOT by charter — an incremental `Tree.edit`/`changed_ranges` re-scan cache is the ruled-out form for this owner; the deleted forms are per-parse `Parser`/`Query` construction, positional untyped tuple payloads read by `ev.span[2]` index where a value object names the field, a `(lang, point, start_byte)` triple re-declared on both `SpanFact` and `DriftFact` where one shared `Locus` carries the extent, a re-minted `tuple[int, int]` `Point2` alias where the package owns the `tree_sitter.Point` value object, a `sources[lang]` `KeyError` on a probe missing a language column where the `Map.filter`-then-`map` compile is total over the covered keys, a `queries[lang]`/`captures[lang]` `KeyError` escaping `run` on a partial-probe corpus row where the coverage-filtered `scan` and the `config` `uncovered-grammar` fault own the uncovered arm, `tags`/`highlights`/`locals` compiled but unreachable where `scan` makes every probe an `Evidence` producer, a per-page `trace.get_tracer("<literal>")` beside the `SCOPES` row the `Scope.EVIDENCE` member already carries, a per-fact capture-name STRING compare where the build-resolved `captures` id table and `_BINDING_NAME_ID` row key the drift fold on integers, a dead `@decl` whole-declaration span materialized per match only to be filtered where the `PROBE_KEEP` `disable_capture` prune scopes the query at build, a speculative `QueryPredicate` parameter threaded for bundled sources whose only predicate is the internally-evaluated `#match?`, a parallel per-corpus `query` plus a `drift`-only fold where one `scan` parameterized over probe/corpus/disposition/output owns the multi-grammar traversal, a bare corpus fold with no `code.scan` parent leaving N `code.query` spans rootless where the one scan span carries the leg, a bare widened `RuntimeRail[Block[R]] | RuntimeRail[tuple[...]]` `scan` return forcing every caller to re-narrow where the `@overload` arms keyed on the `Disposition` `Literal` carry the precise per-disposition output type the faults owner's `traversed` carries, an `into: Into[R] = ...` default on a `scan` overload binding `R` to `Evidence` where the `into`-omitted arms pin `Block[Evidence]` and only the `into`-supplied arms range over `R`, a `traversed(accumulate=True)` keyword the faults owner does not expose where `by=Disposition.ACCUMULATE` is the one disposition-keyed traversal, a post-query `match (did_exceed_match_limit, descendant_count >= budget)` that runs the unbounded `captures` BEFORE the deadline check where the static `descendant_count` pre-gate short-circuits the oversized tree, a `QueryCursor` left on the dead default `match_limit=0xFFFFFFFF` where `match_limit=budget` makes the truncation grade live, a continue-shaped `progress_callback` predicate (`step < budget`) — the supplied int is the cursor's byte offset and `True` CANCELS, so that spelling aborts every healthy run at its first tick and returns clipped captures as a clean `Ok` — where the run-local `itertools.count` tick ceiling cancels only past the budget, a parse-error expectation spelled as an exception arm where `parse` never raises and `Node.has_error` is the recovery-law fact the `evidence.flawed` attribute carries, a `catch=(PackageNotFoundError, ImportError)` tuple and a second `map_error` re-tag indexing a non-`boundary` case slot where the faults-owner `CLASSIFY` `ImportError` row already lands the `import_` case from `catch=ImportError`, a raised `PackageNotFoundError` crossing into domain flow, an entry-points-ONLY `reflect` whose evidence cannot back the member-absence Boundary claim where the trapped member walk mines the importable surface, a dash-to-underscore `import_name` guess where the reversed `packages_distributions` rows carry the distribution's real top modules, a caller-passed `family` constant repeated per `MemberFact` where `ep.group`/`_GROUP_MEMBER`/`_GROUP_IMPORT` carry the real group, a hand-rolled local `reflected` wrapper duplicating the canonical `@trapped` faults aspect, a scattered second `import_module` site where reflection's one boundary kernel owns dynamic import, a single-caller `_outcome`/`_drift_facts`/`_grade` helper threading wide positional state where the grading folds into `run`, a mutated `Map` accumulator in the drift fold, a hand-rolled `lambda out: out` where the admitted `identity` is the flatten spelling, a `drift` projector re-minting a throwaway `Evidence` only to read its `.span` slot where `SpanFact.of` is the shared extent primitive `Evidence.of` itself delegates to, node-type string matching in a recursion where the compiled query fits, a blanket same-name match without namespace multiplicity, a flat receipt fact map where a drifting symbol named for a tag collides with a count key absent the disjoint `count.*`/`drift.*` prefixes, a `code.scan` or `code.query` span left `UNSET` on a fault arm where the trace status must match the rail outcome, an attribute computed without an `is_recording()` gate, a private evidence log where the `EvidenceScan` `ReceiptContributor` streams onto the one receipt rail, and a second structural-search owner; the structural-parsing, drift, and API surfaces emit `Evidence` the `assay code` rail consumes, never a competing search owner, and the `canonical` registry and the per-run `assay code` receipt are upstream/downstream owners this page reads, never re-mints.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from importlib import import_module, metadata
from itertools import count
from typing import Annotated, Final, Literal, overload

from expression import Error, Nothing, Ok, Option, Some, case, identity, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Meta, Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode
from tree_sitter import Language, Node, Parser, Point, Query, QueryCursor
import tree_sitter_python as ts_py
import tree_sitter_typescript as ts_ts

from rasm.runtime.faults import SCOPES, BoundaryFault, Disposition, RuntimeRail, Scope, trapped, traversed
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type Lang = Literal["python", "typescript", "tsx"]
type Probe = Literal["binding", "tags", "highlights", "locals"]
type Byte = Annotated[int, Meta(ge=0)]
type Corpus = Block[tuple[Lang, bytes]]
type Into[R] = Callable[[Lang, str, int, Node], R]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BUDGET: Final[int] = 1 << 20
_CAPTURE_NAME: Final[str] = "name"
_GROUP_IMPORT: Final[str] = "import"
_GROUP_MEMBER: Final[str] = "member"
# the scan span name IS the receipt subject: one symbol keeps the trace leg and the receipt
# row correlated by construction, never two literals that can drift apart.
_SPAN_QUERY: Final[str] = "code.query"
_SPAN_SCAN: Final[str] = "code.scan"

_BINDING_PY: Final[str] = "[(class_definition name: (identifier) @name) (type_alias_statement left: (type (identifier) @name))] @decl"
_BINDING_TS: Final[str] = (
    "[(class_declaration name: (type_identifier) @name)"
    " (interface_declaration name: (type_identifier) @name)"
    " (type_alias_declaration name: (type_identifier) @name)] @decl"
)


# --- [MODELS] ---------------------------------------------------------------------------


class Locus(Struct, frozen=True):
    lang: Lang
    point: Point
    start_byte: Byte
    end_byte: Byte

    @staticmethod
    def of(lang: Lang, node: Node) -> "Locus":
        return Locus(lang, node.start_point, node.start_byte, node.end_byte)


class SpanFact(Struct, frozen=True):
    capture: str
    capture_id: int
    text: str
    locus: Locus

    @staticmethod
    def of(lang: Lang, capture: str, capture_id: int, node: Node) -> "SpanFact":
        return SpanFact(capture, capture_id, node.text.decode() if node.text is not None else "", Locus.of(lang, node))


class MemberFact(Struct, frozen=True, gc=False):
    distribution: str
    version: str
    import_name: str
    group: str
    symbol: str


class DriftFact(Struct, frozen=True):
    name: str
    bindings: frozenset[Lang]
    locus: Locus


@tagged_union(frozen=True)
class Evidence:
    tag: Literal["member", "span", "drift"] = tag()
    member: MemberFact = case()
    span: SpanFact = case()
    drift: DriftFact = case()

    @staticmethod
    def of(lang: Lang, capture: str, capture_id: int, node: Node) -> "Evidence":
        return Evidence(span=SpanFact.of(lang, capture, capture_id, node))


class Grammar(Struct, frozen=True):
    lang: Lang
    language: Language
    parser: Parser

    @staticmethod
    def of(lang: Lang, capsule: object) -> "Grammar":
        language = Language(capsule)
        return Grammar(lang=lang, language=language, parser=Parser(language))


class CompiledProbe(Struct, frozen=True):
    name: Probe
    queries: Map[Lang, Query]
    captures: Map[Lang, Map[str, int]]

    @staticmethod
    def of(name: Probe, sources: "Map[Lang, str]", grammars: "Map[Lang, Grammar]", kept: Option[frozenset[str]]) -> "CompiledProbe":
        covered = grammars.filter(lambda lang, _grammar: lang in sources)
        queries = covered.map(lambda lang, grammar: CompiledProbe._pruned(Query(grammar.language, sources[lang]), kept))
        return CompiledProbe(
            name=name,
            queries=queries,
            captures=queries.map(lambda _lang, query: Map.of_seq((query.capture_name(i), i) for i in range(query.capture_count))),
        )

    @staticmethod
    def _pruned(query: Query, kept: Option[frozenset[str]]) -> Query:
        # `disable_capture` mutates the compiled query in place ONCE at build, so a pruned
        # capture never allocates a node list at run time; the id table still enumerates it.
        kept.map(lambda keep: Block.of_seq(query.capture_name(i) for i in range(query.capture_count)).filter(lambda cap: cap not in keep).map(query.disable_capture))
        return query


# --- [TABLES] ---------------------------------------------------------------------------

GRAMMARS: Final[Map[Lang, Grammar]] = Map.of_seq(
    (lang, Grammar.of(lang, capsule))
    for lang, capsule in (("python", ts_py.language()), ("typescript", ts_ts.language_typescript()), ("tsx", ts_ts.language_tsx()))
)
# `locals` is the live partial-coverage column: the TypeScript grammar ships the source, the
# Python grammar does not, so the Map.filter-total compile is exercised, never just claimed.
PROBE_SOURCES: Final[Map[Probe, Map[Lang, str]]] = Map.of_seq(
    (name, Map.of_seq(rows))
    for name, rows in (
        ("binding", (("python", _BINDING_PY), ("typescript", _BINDING_TS), ("tsx", _BINDING_TS))),
        ("tags", (("python", ts_py.TAGS_QUERY), ("typescript", ts_ts.TAGS_QUERY), ("tsx", ts_ts.TAGS_QUERY))),
        ("highlights", (("python", ts_py.HIGHLIGHTS_QUERY), ("typescript", ts_ts.HIGHLIGHTS_QUERY), ("tsx", ts_ts.HIGHLIGHTS_QUERY))),
        ("locals", (("typescript", ts_ts.LOCALS_QUERY), ("tsx", ts_ts.LOCALS_QUERY))),
    )
)
# binding emits only the bound name: the `@decl` whole-declaration span prunes at build, so
# the drift scan never materializes full-source captures it would only filter away.
PROBE_KEEP: Final[Map[Probe, frozenset[str]]] = Map.of_seq((("binding", frozenset((_CAPTURE_NAME,))),))
PROBES: Final[Map[Probe, CompiledProbe]] = PROBE_SOURCES.map(
    lambda name, sources: CompiledProbe.of(name, sources, GRAMMARS, PROBE_KEEP.try_find(name))
)
# capture name -> id resolved once at registry build; the drift fold compares integers per fact.
_BINDING_NAME_ID: Final[Map[Lang, int]] = PROBES["binding"].captures.map(lambda _lang, table: table[_CAPTURE_NAME])


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final[trace.Tracer] = trace.get_tracer(SCOPES[Scope.EVIDENCE])


# --- [OPERATIONS] -----------------------------------------------------------------------


class GrammarRegistry:
    @staticmethod
    def run[R](
        lang: Lang,
        source: bytes,
        probe: Probe,
        into: Into[R],
        *,
        max_depth: int | None = None,
        byte_range: tuple[int, int] | None = None,
        budget: int = _BUDGET,
    ) -> RuntimeRail[Block[R]]:
        # Coverage is registry data: a partial probe run directly against an uncovered grammar
        # is the typed `config` fault, never a `queries[lang]` KeyError into domain flow.
        probed = PROBES[probe]
        if lang not in probed.queries:
            return Error(BoundaryFault(config=(f"{probe}:{lang}", "uncovered-grammar")))
        # `match_limit=budget` makes `did_exceed_match_limit` a LIVE truncation grade — the
        # default `0xFFFFFFFF` cap never trips, so the `resource` arm would be dead and a
        # match-explosion would return silently clipped captures as a clean `Ok`.
        cursor = QueryCursor(probed.queries[lang], match_limit=budget)
        if max_depth is not None:
            cursor.set_max_start_depth(max_depth)
        if byte_range is not None:
            cursor.set_byte_range(*byte_range)

        ids = probed.captures[lang]
        with _TRACER.start_as_current_span(_SPAN_QUERY) as scope:
            if scope.is_recording():
                scope.set_attributes({"evidence.probe": probe, "evidence.lang": lang})
            tree = GRAMMARS[lang].parser.parse(source)
            # Two observable axes bound the run and grade it: `descendant_count` (static tree
            # size, gated FIRST so the query never starts on an oversized tree) and the
            # `match_limit=budget` post-query `did_exceed_match_limit`. `progress_callback` is the
            # third axis — the cancel hook replacing the deprecated `timeout_micros`: it fires with
            # the cursor's BYTE OFFSET and returning True CANCELS, so the hang guard keys on the
            # tick count, never the offset (a continue-shaped `step < budget` predicate cancels
            # every healthy run at its first tick); a cancel surfaces as bounded partial captures,
            # not a grade, and the size pre-gate keeps it unreachable in practice.
            visited = tree.root_node.descendant_count
            if visited >= budget:
                fault = BoundaryFault(deadline=(f"{probe}:{lang}", float(budget), "descendant-count"))
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            ticks = count(1)
            captures = cursor.captures(tree.root_node, progress_callback=lambda _offset: next(ticks) >= budget)
            truncated = cursor.did_exceed_match_limit
            if scope.is_recording():
                scope.set_attributes({
                    "evidence.nodes": visited,
                    "evidence.captures": sum(map(len, captures.values())),
                    "evidence.truncated": truncated,
                    # `parse` never raises: a broken source still yields a Tree with error nodes,
                    # so parse health is a traced fact per the recovery law, never a rail fault.
                    "evidence.flawed": tree.root_node.has_error,
                })
            if truncated:
                fault = BoundaryFault(resource=(f"{probe}:{lang}", "match-limit"))
                scope.set_status(Status(StatusCode.ERROR, fault.tag))
                return Error(fault)
            scope.set_status(Status(StatusCode.OK))
            return Ok(Block.of_seq(into(lang, name, ids[name], node) for name, nodes in captures.items() for node in nodes))

    @overload
    @staticmethod
    def scan(
        probe: Probe, corpus: Corpus, *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...
    ) -> RuntimeRail[Block[Evidence]]: ...
    @overload
    @staticmethod
    def scan(
        probe: Probe, corpus: Corpus, *, by: Literal[Disposition.PARTITION], budget: int = ...
    ) -> RuntimeRail[tuple[Block[Evidence], Block[BoundaryFault]]]: ...
    @overload
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ..., budget: int = ...
    ) -> RuntimeRail[Block[R]]: ...
    @overload
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R], *, by: Literal[Disposition.PARTITION], budget: int = ...
    ) -> RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]: ...
    @staticmethod
    def scan[R](
        probe: Probe, corpus: Corpus, into: Into[R] = Evidence.of, *, by: Disposition = Disposition.ACCUMULATE, budget: int = _BUDGET
    ) -> RuntimeRail[Block[R]] | RuntimeRail[tuple[Block[R], Block[BoundaryFault]]]:
        # The `by` row selects the multi-grammar output shape exactly as the faults owner's
        # `traversed` overloads carry it; the one `code.scan` parent span brackets the corpus
        # fold so every per-file `code.query` span nests under one traced leg whose status and
        # rail outcome are the same fact.
        flatten: Callable[[Block[Block[R]]], Block[R]] = lambda nested: nested.collect(identity)
        # a partial probe scans only its covered columns — a `locals` scan over a Python row
        # yields no evidence and no fault, since the missing column is a build-time registry fact.
        covered = corpus.filter(lambda row: row[0] in PROBES[probe].queries)
        with _TRACER.start_as_current_span(_SPAN_SCAN) as scope:
            if scope.is_recording():
                scope.set_attributes({"evidence.probe": probe, "evidence.corpus": len(corpus), "evidence.covered": len(covered)})
            rails = covered.map(lambda row: GrammarRegistry.run(row[0], row[1], probe, into, budget=budget))
            graded = (
                traversed(rails, by=by).map(lambda split: (flatten(split[0]), split[1]))
                if by is Disposition.PARTITION
                else traversed(rails, by=by).map(flatten)
            )
            scope.set_status(graded.map(lambda _ok: Status(StatusCode.OK)).default_with(lambda fault: Status(StatusCode.ERROR, fault.tag)))
            return graded

    @staticmethod
    def drift(corpus: Corpus, canonical: frozenset[str]) -> RuntimeRail[Block[Evidence]]:
        # the capture filter keys on the build-resolved integer id; the build prune already
        # scopes the binding scan to `@name`, so the compare is the belt on a build-time brace.
        return GrammarRegistry.scan("binding", corpus, SpanFact.of).map(
            lambda spans: GrammarRegistry._cross(
                spans.filter(lambda fact: fact.capture_id == _BINDING_NAME_ID[fact.locus.lang] and fact.text in canonical)
            )
        )

    @staticmethod
    def _cross(spans: Block[SpanFact]) -> Block[Evidence]:
        seed: Map[str, frozenset[Lang]] = Map.empty()
        bindings = spans.fold(lambda acc, fact: acc.change(fact.text, lambda b: Some(b.default_value(frozenset()) | {fact.locus.lang})), seed)
        return spans.choose(
            lambda fact: Some(Evidence(drift=DriftFact(fact.text, bindings[fact.text], fact.locus))) if len(bindings[fact.text]) > 1 else Nothing
        )


class ApiCatalogue:
    @staticmethod
    @trapped("reflect", catch=ImportError)
    def reflect(distribution: str) -> Block[Evidence]:
        # import roots are metadata facts, never a dash-to-underscore guess: the reversed
        # `packages_distributions` rows carry every top module the distribution ships, the
        # normalized name only the fallback where the environment carries no row.
        dist = metadata.distribution(distribution)
        version = dist.version
        mapped = Block.of_seq(sorted(mod for mod, dists in metadata.packages_distributions().items() if distribution in dists))
        roots = mapped if len(mapped) else Block.of_seq((distribution.replace("-", "_"),))
        return Block.of_seq(
            Evidence(member=MemberFact(distribution, version, ep.module.partition(".")[0], ep.group, ep.name))
            for ep in dist.entry_points
        ).append(roots.collect(lambda root: ApiCatalogue._mined(distribution, version, root)))

    @staticmethod
    def _mined(distribution: str, version: str, root: str) -> Block[Evidence]:
        # reflection's one dynamic-import site: `import_module` raises inside `reflect`'s trapped
        # fence, so a broken root lands as the same `import_` row a missing distribution does.
        surface = import_module(root)
        return (
            Option.of_optional(getattr(surface, "__all__", None))
            .map(Block.of_seq)
            .default_with(lambda: Block.of_seq(name for name in vars(surface) if not name.startswith("_")))
            .map(lambda symbol: Evidence(member=MemberFact(distribution, version, root, _GROUP_MEMBER, symbol)))
            .cons(Evidence(member=MemberFact(distribution, version, root, _GROUP_IMPORT, root)))
        )


# --- [COMPOSITION] ----------------------------------------------------------------------


class EvidenceScan(Struct, frozen=True):
    owner: str
    evidence: Block[Evidence]

    def contribute(self) -> Iterable[Receipt]:
        # The per-tag `Map.change` fold tallies every case under `count.{tag}`; `Block.choose`
        # drains the `drift` arm into `(drift.{name}, langs)` pairs, the disjoint `count.*`/`drift.*`
        # prefixes keeping a symbol named for a tag from colliding with a tally key in the flat map.
        counts = self.evidence.fold(lambda acc, ev: acc.change(f"count.{ev.tag}", lambda n: Some(n.default_value(0) + 1)), Map.empty())
        drifts = self.evidence.choose(
            lambda ev: Some((f"drift.{ev.drift.name}", ",".join(sorted(ev.drift.bindings)))) if ev.tag == "drift" else Nothing
        )
        facts: dict[str, object] = dict(counts.items()) | dict(drifts)
        return (Receipt.of(self.owner, ("emitted", _SPAN_SCAN, facts)),)
```
