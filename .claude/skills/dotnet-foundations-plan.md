# [DOTNET_FOUNDATIONS_PLAN]

The 4 foundation skills (`dotnet-coding`, `dotnet-languageext`, `dotnet-thinktecture`, `dotnet-mapperly`) cover composition, the result and effect library, the generated types, and boundary mapping. This plan surveys `Directory.Packages.props` and the language against them, names the gaps, and sets the next skills as categories of concern in tiers, each built by the workflow in `dotnet-skills-plan.md` [03] to [08] from a research folder that must exist before the skill, because no section is written from memory.

## [01]-[START]

Read this section as the brief for any session that extends the .NET skills. The goal is one unified standard for a monorepo that will hold many domains (mathematics, algorithms, science, data in every form, backends, messaging, native work), where every skill drives agent behavior under the workspace standards in CLAUDE.md and the 4 foundation skills, and nothing an agent writes from a skill is naive, forced, hand-rolled, legacy, or coupled to one project.

Orientation, in order: this plan, `dotnet-skills-plan.md` for the workflow and the ownership split, the memories on skill authoring, content, snippets, and the research folder, the intros of every existing skill, then the tier table in [04] to find the next skill and `docs/research/dotnet/<skill>/` to see whether its research exists.

When the research does not exist, research comes first and produces no skill text:
1. Define the concern and its full set of concepts as the field understands them today, exhaustive and current only, independent of this project, with the manifest's packages for the concern placed inside that set rather than driving it
2. Check for an official skill from each package's repository and for well-made skills on GitHub, read them as coverage starting points, and copy nothing of their structure or wording, because most are below this standard and some state wrong behavior
3. Gather the material through the search skills and the package documentation, verify every member and behavior against the package (documentation lookup, the package XML, a scratch compile), and write it into research files under the conventions in the research-folder memory, in the source's framing without pretending to be a skill
4. Extend this plan with the disposition of every research file, the fork sequence, and any concept that has no owning skill yet, added to the tier table before anything is built

When the research exists, run the workflow: the disposition in the plan, sequential forks with a commit between each, the consistency pass, the fresh-agent reviews, and the findings recorded in [07].

Every concept enters whole. Caching means the layers, keys, expiry and eviction, invalidation, and stampede protection, not a memoization example, and the same completeness applies to a state machine, a channel, publish and subscribe, a retry policy, or a parser, each placed in the skill that owns its concern and built with the sibling packages as vocabulary only where the functionality belongs to them. A concept with no owning skill is a gap in the tier table, never a paragraph squeezed into the nearest skill.

Rules that hold in every session:
- Nothing legacy, obsolete, or version-bound enters, prose names no version numbers, and every term is the current established term of its field
- One approach per concept unless the concern needs two, and then the rule states which applies when, so no skill carries dual paradigms
- Every rule sits at the workspace's abstraction level, so an agent never builds result flow in the wrong layer, spams types, or forces a package where the construct does not belong
- Every snippet follows the snippet discipline and compiles against the package before a fork reports
- Every agent runs alone, reports in at most 15 lines, and adjusts the plan when the plan is wrong

## [02]-[SURVEY]

The manifest groups packages by domain, and the skills own concerns, so the survey maps each group to the concern that owns it. The Core group is covered apart from the packages listed, the analyzers apart from the Thinktecture one, and every other group is uncovered:

| [INDEX] | [GROUP]                  | [GAP]                                                              | [HOME]                             |
| :-----: | :----------------------- | :----------------------------------------------------------------- | :--------------------------------- |
|   [01]  | Core                     | NodaTime, Parsec, hashing, HighPerformance, RecyclableMemoryStream | concurrency, text, io, performance |
|   [02]  | Analyzers                | Meziantou, ErrorProne, Roslynator, Threading rules                 | Each skill names its diagnostics   |
|   [03]  | Quantities               | UnitsNet, NodaMoney as typed numbers                               | numerics, vocabulary in coding     |
|   [04]  | Mathematics, Solvers     | Generic math, tensors, linear algebra, optimization                | numerics                           |
|   [05]  | Algorithms               | Graphs, spatial indexes, tries, probabilistic sets, caching, ZLinq | algorithms                         |
|   [06]  | Parsing                  | Parsec, Pidgin, Sep, NCalc                                         | text                               |
|   [07]  | Serialization and RPC    | System.Text.Json contexts, converters, Protobuf, Avro, YAML, CBOR  | serialization                      |
|   [08]  | Data Storage and Formats | EF Core, linq2db, Marten, ADO, Arrow, Parquet, HDF5                | data                               |
|   [09]  | Application Hosting      | DI, configuration, options, resilience, caching, command line      | hosting                            |
|   [10]  | Observability            | OpenTelemetry, logging at the boundary                             | hosting                            |
|   [11]  | Testing                  | xunit v3, CsCheck, time providers, test runtimes                   | testing                            |
|   [12]  | Messaging, Protocols     | NATS, Kafka, RabbitMQ, MQTT, CloudEvents, OPC UA                   | messaging, later                   |
|   [13]  | Domain packs             | Geometry, geospatial, BIM, fabrication, energy, structural, ML, UI | One skill per pack, later          |

Time is the largest overlooked vocabulary gap: NodaTime sits in the Core group and every skill snippet uses `DateTime` or `DateTimeOffset`, and the library holds more than the replacement types: `IClock` with `SystemClock` and `FakeClock` for injection, the `Instant`, `LocalDateTime`, `ZonedDateTime`, and `OffsetDateTime` distinctions, `DateTimeZoneProviders.Tzdb`, `Duration` against `Period`, `Interval`, calendar systems, `NodaTime.Text` patterns for invariant parsing and formatting, and the serialization and store integrations already in the manifest (System.Text.Json, Protobuf, Npgsql, Marten). Quantities are the second: money and physical values appear as `decimal` and `double`. `LanguageExt.Parsec` is the one library companion with no home, and it needs a research file before a section exists.

## [03]-[LANGUAGE_GAPS]

`dotnet-coding` covers composition (functions, expressions, immutability, results, effects) and leaves declaration decisions uncovered. These belong to one new skill, `dotnet-types`, because they are rules rather than worked flows and the coding skill is at its size:
- Interfaces: when a real interface (several implementations behind one contract, a static abstract member set, a host framework requirement) and when a function dependency, abstract classes, default interface members out
- Generics: constraints, variance, `allows ref struct`, static abstract members with the library traits as the implicit example, generic math as the numerics entry
- Extension members: C# 14 `extension` blocks and extension methods, used to adapt a type an assembly does not own at a boundary and to give pipelines over domain types their operators, never to hide state or to hold what belongs on the type
- Structs, classes, records: value semantics, `readonly record struct`, `ref struct` and `Span<T>` confined to a boundary or a hot path, primary constructors, collection expressions, `params ReadOnlySpan<T>`, the `field` keyword
- Equality and comparison: `IEquatable<T>`, `IComparable<T>`, comparers as values, ordinal string comparison, `HashCode.Combine`, structural equality of records, with the generated types' comparers as the implicit example
- Attributes and enums: declaring and reading attributes, `[Flags]` with bit operations, plain enums only as closed sets with no behavior (a smart enum otherwise), `AttributeUsage`, source-generated partials
- Object-oriented patterns in functional form as one reference: strategy as a function parameter, template method as a higher-order function, visitor as a union `Switch` or fold, builder as a record with `with` or `With`, observer as a `Source`, command as a union of commands with an interpreter, decorator as composition, factory as `From`, singleton as a static readonly value or `Memo`, state machine as a transition function, repository as function dependencies returning `IO` or `OptionT`, null object as `Option`

Hardening risks in the 4 skills that the same work settles: every snippet is an `internal static class`, so the rule for an instance type with immutable state (a record with methods, a class holding injected functions, a host adapter) must be stated once, and interfaces appear only as a repository in one reference, so an agent never writes one.

## [04]-[CATEGORIES]

A category skill owns a concern of code (how time, text, bytes, numbers, or concurrency are handled) and treats the packages that serve it as vocabulary, with one reference per package family. A package skill exists only where the package is a language of its own with a generated API or a large surface an agent writes against directly (the 3 existing ones, the future domain packs). A package that serves a concern never gets its own skill, and minutiae never get a skill.

| [INDEX] | [TIER] | [SKILL]                | [IMPLICIT]                                                            |
| :-----: | :----- | :--------------------- | :-------------------------------------------------------------------- |
|   [01]  | 1      | `dotnet-architecture`  | README layout, the 4 foundation skills                                |
|   [02]  | 1      | `dotnet-types`         | Thinktecture, LanguageExt traits                                      |
|   [03]  | 1      | `dotnet-time`          | NodaTime, `TimeProvider`, Cronos                                      |
|   [04]  | 1      | `dotnet-concurrency`   | LanguageExt `IO`, `Fork`, STM, Threading analyzers                    |
|   [05]  | 1      | `dotnet-text`          | Parsec, Sep, NCalc, Markdig, MessageFormat                            |
|   [06]  | 1      | `dotnet-serialization` | Thinktecture, NodaTime, UnitsNet converters                           |
|   [07]  | 2      | `dotnet-io`            | RecyclableMemoryStream, `System.IO.Hashing`, Zstd, LZ4, object stores |
|   [08]  | 2      | `dotnet-performance`   | HighPerformance, DotNext, `System.Numerics.Tensors`                   |
|   [09]  | 2      | `dotnet-numerics`      | MathNet, UnitsNet, NodaMoney, PeterO, DoubleDouble                    |
|   [10]  | 2      | `dotnet-algorithms`    | QuikGraph, RBush, tries, SuperLinq, ZLinq                             |
|   [11]  | 2      | `dotnet-caching`       | BitFaster, HybridCache, FusionCache, Redis                            |
|   [12]  | 2      | `dotnet-data`          | Marten, KurrentDB, MongoDB, graph and embedded stores                 |
|   [13]  | 2      | `dotnet-sql`           | Npgsql, EF Core, linq2db, Weasel, SQLite, SQL Server, MySQL           |
|   [14]  | 2      | `dotnet-hosting`       | Generic host, DI, Options, Configuration, Composition                 |
|   [15]  | 2      | `dotnet-observability` | OpenTelemetry, Serilog, Pyroscope, runtime diagnostics                |
|   [16]  | 2      | `dotnet-resilience`    | Polly, Extensions.Resilience, RateLimiting                            |
|   [17]  | 2      | `dotnet-http`          | HttpClient factory, service discovery, gRPC, NSign                    |
|   [18]  | 2      | `dotnet-security`      | Cryptography, IdentityModel, OpenIddict, KeyVault, KMS, Vault         |
|   [19]  | 2      | `dotnet-interop`       | `LibraryImport`, `Rasm.Native.*`, vcpkg packaging, Silk.NET, Wasmtime |
|   [20]  | 3      | `dotnet-analytics`     | DuckDB, ClickHouse, QuestDB, InfluxDB, Trino, Arrow, Parquet, Delta   |
|   [21]  | 3      | `dotnet-search`        | Lucene, Elastic, OpenSearch, Meilisearch, Qdrant, pgvector, HNSW      |
|   [22]  | 3      | `dotnet-messaging`     | NATS, Kafka, RabbitMQ, MQTT, CloudEvents                              |
|   [23]  | 3      | `dotnet-roslyn`        | Analyzers and source generators of our own                            |
|   [24]  | 3      | Domain packs           | Each pack's packages, AI and MCP among them                           |

Each skill's scope:
- `dotnet-architecture` holds the shape of a library or application project (files at the root, namespaces, access modifiers, partials, `InternalsVisibleTo`), when a project splits, the public API of a package (capabilities exposed, one result type, boundary types), dependency direction and layering, the abstraction level of a function (workflow, domain, boundary), refactoring discipline (extract a function before a type, a type that earns its existence, a tuple before a record, a parameter before a class), size limits, and the versionless change rule
- `dotnet-types` holds the declaration decisions in [03], shape discipline (no stringly typed values, a value object or smart enum where a string or number carries meaning, an enum only as a closed set without behavior), reflection confined to a boundary with source generation preferred, and the patterns reference
- `dotnet-time` holds clocks as injected values, the instant, local, zoned, and offset distinctions, zones, durations and periods, intervals, text patterns, and scheduling
- `dotnet-concurrency` holds `Task` at the boundary, cancellation, timeouts, channels and async streams with their conversion to a `Source`, parallelism, and synchronization
- `dotnet-text` holds strings and spans of chars, regex generation, `SearchValues`, format and parse, encoding, globalization, CSV, parser combinators, and expression evaluation
- `dotnet-serialization` holds contracts at the host, System.Text.Json contexts and converters for the vocabulary types, binary formats, and schema
- `dotnet-io` holds files, streams, binary framing, compression, non-cryptographic hashing, paths, and object storage (S3, Blob, GCS, Minio) as bytes
- `dotnet-performance` holds spans and memory, buffers and pooling, SIMD and tensors, allocation, and measurement
- `dotnet-numerics` holds generic math, numeric type choice, tolerance, random with explicit seeds, quantities, and the linear algebra entry
- `dotnet-algorithms` holds structure selection by problem shape (graphs, spatial, strings, probabilistic) and the LINQ extensions
- `dotnet-caching` holds what to cache where (memoization of a pure function, a state-threaded cache in a workflow, a bounded in-process cache, an application cache with a backplane, a distributed store), keys, expiry and eviction, invalidation, and stampede protection
- `dotnet-data` holds the access discipline every store shares (access at the boundary as `IO` and `OptionT`, projections, value objects in stores, transactions and units of work, connection lifetime, schema from types, store choice by shape) and the document, event, graph, and embedded key-value stores as references
- `dotnet-sql` holds relational access with PostgreSQL first (its current type system, JSON, arrays, ranges, window functions, `MERGE`, notifications, the NodaTime and spatial mappings), SQLite, SQL Server, and MySQL as references, EF Core and linq2db as the access layers, and Weasel for schema
- `dotnet-hosting` holds the composition root only: the generic host, DI wiring of runtimes and functions, configuration and options, feature flags, health, lifetime, the command line, service lifecycles, and plugin composition
- `dotnet-observability` holds traces, metrics, and logs at the boundary with OpenTelemetry, Serilog as the log pipeline, redaction and compliance, profiling, and the runtime diagnostics packages, with the official OpenTelemetry skills as its coverage check
- `dotnet-resilience` holds pipelines (retry, hedging, circuit breaker, bulkhead, timeout, rate limiting) at the host, the split with `Schedule` and `Retry` in the domain, and the rule that one call carries one policy
- `dotnet-http` holds HTTP and gRPC clients and servers: the client factory, handlers, service discovery, content negotiation, signatures, and the resilience handler integration
- `dotnet-security` holds cryptographic hashing and signing, secrets, tokens and identity, and key services
- `dotnet-interop` holds native bindings, marshalling, memory ownership across the boundary, and the packaging of native assets under `eng/native/`
- `dotnet-analytics` holds columnar and time-series stores and formats (DuckDB, ClickHouse, QuestDB, InfluxDB, Trino, Snowflake, Arrow, Parquet, Delta, Lance, ADBC and Flight, Substrait) and the array formats (HDF5, Zarr, NetCDF, SafeTensors) as a reference
- `dotnet-search` holds full-text and vector search (Lucene, Elastic, OpenSearch, Meilisearch, Qdrant, Weaviate, pgvector, sqlite-vec, LanceDB, HNSW) and embedding storage
- `dotnet-messaging` holds producers and consumers as sources and sinks, contracts, and delivery
- `dotnet-roslyn` holds authoring analyzers and source generators when the workspace writes its own
- Domain packs are one skill per pack in the survey's last row, with AI and model-context-protocol clients as one pack and Rhino as another

Placement decisions inside the tiers:
- Resilience is split by layer: the domain composes `Schedule` and `Retry` around an `IO`, the host applies a resilience pipeline to an `HttpClient` or a connection, and never both on one call, stated once in `dotnet-resilience`
- Solvers and optimization are a reference under `dotnet-numerics` until a modeling discipline of their own justifies a skill
- Streaming stays split as it is: `Source`, `Conduit`, and pipes in `dotnet-languageext`, `Channel<T>` and `IAsyncEnumerable<T>` as boundary forms in `dotnet-concurrency` with the conversion to a `Source`
- Hashing splits by purpose: a content hash is `dotnet-io`, a cryptographic hash or signature is `dotnet-security`, and a hex or base64 string is `dotnet-text`
- Caching is one skill because expiry, eviction, invalidation, and stampede protection are one discipline across layers, and the memoization and state-threaded forms in `dotnet-languageext` and the `dotnet-coding` references stay where they are as the pure-side forms it points to
- Analyzer diagnostics belong to the skill that owns the rule they enforce, named beside the rule as `dotnet-document` names its Roslynator rules

## [05]-[ORDER]

Tier 1 first, in the listed order, because `dotnet-architecture` and `dotnet-types` close the language foundation, `dotnet-time` fixes the vocabulary that every later snippet uses, and concurrency, text, and serialization are the boundary concerns every application meets. Testing is planned separately and is not part of these tiers. Before each skill: gather its research folder under `docs/research/dotnet/<skill>/` (extracted documentation, articles, or fresh research through the search skills), read it in full, then extend this plan with the disposition of its files and the fork sequence. Before the tier, once the review sequence of the 4 skills is committed, one fresh agent runs the NodaTime vocabulary pass over the 4 skills with each type verified against the package by a scratch compile. After the tier: a fresh-agent review of the new skills against the existing ones, and the quantities vocabulary pass once `dotnet-numerics` exists.

## [06]-[DECISIONS]

- NodaTime as vocabulary: every skill snippet that touches time uses `Instant`, `LocalDate`, `ZonedDateTime`, and an injected `IClock` or `Func<Instant>`, with `DateTimeOffset` only at a host boundary, applied to the 4 existing skills in the vocabulary pass
- Quantities as vocabulary: money is `NodaMoney.Money` and a physical value is a `UnitsNet` quantity, never `decimal` or `double` in the domain, stated in `dotnet-numerics` and the coding intro
- Parser combinators: `LanguageExt.Parsec` is the one parser combinator library because it composes with the result types, and Pidgin leaves the manifest unless a need Parsec cannot meet is recorded
- Splitting declarations into `dotnet-types` rather than growing `dotnet-coding` past its size
- Dependency injection: the container wires the runtime record and the specialized functions at the composition root and the domain never sees it, to be stated in `dotnet-hosting`, with the functions reference's "no container required" sentence aligned

## [07]-[FINDINGS]

The build and review sequences of the 4 skills exposed patterns that every later skill avoids, and specific items that the next passes fix:

| [INDEX] | [PATTERN]                                          | [RULE]                                                           |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------- |
|   [01]  | Members guessed that the package lacks             | Every snippet compiles against the package before the report     |
|   [02]  | Placeholders that collide with library names       | Placeholders are checked against the packages' public names      |
|   [03]  | Facts dropped while a section was trimmed          | Research is commented out per fact, the review restores losses   |
|   [04]  | A shortened sentence with an inverted condition    | Shortening keeps every condition, or the sentence stays long     |
|   [05]  | A skill section that summarizes a reference        | A section defers with one line and never summarizes              |
|   [06]  | Nested HTML comments in research                   | One marker per region, inner markers fold into a note            |
|   [07]  | An official package skill stating a wrong behavior | Official skills are coverage checks, every claim is verified     |
|   [08]  | A commit during an agent's run                     | Commits happen only between agents                               |
|   [09]  | A member restated in another owner's skill         | Each member has one owning skill, checked before it enters       |
|   [10]  | Research examples that violate the standard        | An example is corrected to the standard as it enters             |
|   [11]  | A reference intro repeating the sibling pointer    | Pointers live in the skill intro only                            |
|   [12]  | A skill naming a package the manifest lacks        | Every named package is in the manifest, or its absence is stated |
|   [13]  | A catalog restating skill facts with extra detail  | A catalog holds members and signatures, facts live in the skill  |

| [INDEX] | [SKILL]      | [ITEM]                                                        | [ACTION]                                                 |
| :-----: | :----------- | :------------------------------------------------------------ | :------------------------------------------------------- |
|   [01]  | coding       | `DateTime` in every time snippet                              | Applied in the NodaTime pass with a scratch compile      |
|   [02]  | coding       | Static classes only, interfaces absent                        | Instance-type and interface rules in `dotnet-types`      |
|   [03]  | coding       | Functions reference: composition needs no container           | Align with `dotnet-hosting` when it exists               |
|   [04]  | coding       | Quantities as `decimal` and `double`                          | Vocabulary pass when `dotnet-numerics` exists            |
|   [05]  | languageext  | `api.md` restates `IO.lift`, `tail`, `Fork`, `Bracket`        | Trim the rows to members and signatures                  |
|   [06]  | languageext  | `LanguageExt.Parsec` with no research                         | Research folder under `dotnet-text`                      |
|   [07]  | languageext  | Scratch compile project in a session scratchpad               | Durable snippet project under `tests/dotnet/`            |
|   [08]  | thinktecture | AspNetCore, Swashbuckle, Newtonsoft integrations unlisted     | Add to the manifest or remove from the skill             |
|   [09]  | thinktecture | Swashbuckle schema filters undocumented                       | Research, then state or record the omission              |
|   [10]  | thinktecture | Value-objects [15] dates and currency uncommented             | Homes in `dotnet-time` and `dotnet-numerics`             |
|   [11]  | thinktecture | `Thinktecture.EntityFrameworkCore.*` not in the manifest      | Decide when `dotnet-data` is planned                     |
|   [12]  | mapperly     | `Seq<Line>` to `List<LineDto>` at the DTO edge                | Kept with the host-contract sentence, recheck            |
|   [13]  | all          | Analyzer diagnostics unnamed beside the rules                 | Pass after the analyzer configuration settles            |
|   [14]  | all          | Pidgin beside Parsec in the manifest                          | Remove unless a recorded need stays                      |
|   [15]  | mapperly     | Research [12] priority numbers wrong inside the comment       | Correct the comment when the research is next opened     |
|   [16]  | mapperly     | `Seq` to `List<T>` target reports `RMG020`                    | Upstream issue, `IReadOnlyList<T>` targets meanwhile     |
|   [17]  | mapperly     | `Directory.Build.props` has no Mapperly property group        | Add the group the skill's [02] describes                 |
|   [18]  | coding       | `Event` placeholder beside the library's static `Event` class | Kept as the domain term, rename if it confuses a compile |
|   [19]  | thinktecture | [03.1] maps `Validate` to `Fin<T>` generically                | Kept as the `ISmartEnum` constraint demonstration        |
|   [20]  | time         | `Duration` ambiguous between LanguageExt and NodaTime         | `dotnet-time` owns the alias rule                        |
|   [21]  | mapperly     | `Interval` placeholder collides with `NodaTime.Interval`      | Rename in the next pass                                  |
|   [22]  | time         | `NotPast` derives today as `clock().InUtc().Date`             | `dotnet-time` decides the zone or an injected date       |
