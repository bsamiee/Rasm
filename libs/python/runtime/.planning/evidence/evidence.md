# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence. `ApiPackage`/`ApiMember` record distribution/import/owner/capability/entrypoint facts over `importlib.metadata`; `Structural` runs `tree-sitter` S-expression queries over Python and TypeScript sources into matched spans the `assay code` rail consumes, and folds those spans into a cross-language drift detector that locates a re-minted canonical concept — a second wire-projection name the topology law forbids — across both grammars. The surface produces evidence the rail reads — never a competing search owner, never a guessed environment status.

## [1]-[INDEX]

One cluster: `[2]-[API]` — API records, structural-parsing spans, and the cross-language drift detector.

## [2]-[API]

- Owner: `ApiPackage` — the distribution/import/owner/capability/entrypoint record over `importlib.metadata`; `ApiMember` the official-surface row a source may later name; `Structural` the static surface over `tree-sitter` parsing Python/TypeScript sources into evidence the `assay code` rail consumes; `DriftSpan` the cross-language drift row carrying the offending name, language, and byte span.
- Entry: `ApiPackage.reflect` reads one distribution's metadata and entry points; `Structural.query` runs a tree-sitter S-expression query over a source through `QueryCursor(Query(language, pattern)).captures(...)`, returning `(capture-name, start-byte, end-byte)` rows by flattening the capture-name-to-node-list `dict` the cursor returns; `Structural.drift` runs one identifier-binding query per grammar over `(language-tag, source)` corpora, captures every declaration of a canonical wire-projection name from the shared `canonical` name set, and returns a `DriftSpan` per location only when the name is bound in more than one language namespace — the named cross-language drift defect the `assay code` rail consumes.
- Auto: the drift query is one S-expression capture over a binding identifier (`(identifier) @name` scoped to a type/class/interface declaration) compiled once per `GRAMMARS` row through the `Query(language, pattern)` constructor; the detector folds `Structural.query` spans into a name-to-locations `Map` and emits a `DriftSpan` only where the same canonical name binds across two distinct language namespaces, so a legitimately distinct same-named concept in one namespace yields no defect and a re-minted identity seed, receipt rail, or capability descriptor across Python and TypeScript yields one `DriftSpan` per re-mint — a false positive is filtered by namespace multiplicity, never a blanket name match; the `canonical` set arrives as the shared one-name-one-owner registry, never re-minted here.
- Packages: `importlib.metadata` (stdlib), `tree-sitter` (`Language`/`Parser`/`Query`/`QueryCursor.captures`), `tree-sitter-python` (`language`), `tree-sitter-typescript` (`language_typescript`/`language_tsx`), `expression` (`Map`), `msgspec`.
- Growth: a new evidence field is one column on `ApiPackage`; a new language is one `GRAMMARS` grammar row plus one drift-query pattern; a new canonical name is one entry the caller adds to the `canonical` set; zero new surface.
- Boundary: no package version tables in planning pages, no guessed environment status, no parallel canonical-name registry minted here; a source cannot name a member absent from the catalogue evidence; a blanket same-name match without namespace multiplicity and a second structural-search owner are the deleted forms; the structural-parsing and drift surfaces emit evidence the `assay code` rail consumes, never a competing search owner.

```python signature
from importlib import metadata

from expression.collections import Map
from msgspec import Struct
from tree_sitter import Language, Parser, Query, QueryCursor


class ApiMember(Struct, frozen=True):
    symbol: str
    family: str


class ApiPackage(Struct, frozen=True):
    distribution: str
    import_name: str
    owner: str
    capability: str
    entrypoints: tuple[str, ...]
    members: tuple[ApiMember, ...]

    @classmethod
    def reflect(cls, distribution: str, owner: str, capability: str) -> "ApiPackage":
        dist = metadata.distribution(distribution)
        eps = tuple(ep.name for ep in dist.entry_points)
        return cls(distribution, distribution.replace("-", "_"), owner, capability, eps, members=())


class DriftSpan(Struct, frozen=True):
    name: str
    language: str
    start_byte: int
    end_byte: int


class Structural:
    @staticmethod
    def query(language: Language, source: bytes, pattern: str) -> tuple[tuple[str, int, int], ...]:
        tree = Parser(language).parse(source)
        captures = QueryCursor(Query(language, pattern)).captures(tree.root_node)
        return tuple(
            (name, node.start_byte, node.end_byte)
            for name, nodes in captures.items()
            for node in nodes
        )

    @staticmethod
    def drift(corpora: tuple[tuple[str, Language, bytes, str], ...], canonical: frozenset[str]) -> tuple[DriftSpan, ...]:
        spans = tuple(
            DriftSpan(name=source[start:end].decode(), language=lang, start_byte=start, end_byte=end)
            for lang, language, source, pattern in corpora
            for _, start, end in Structural.query(language, source, pattern)
            if source[start:end].decode() in canonical
        )
        languages: Map[str, frozenset[str]] = Map.empty()
        for span in spans:
            languages = languages.add(span.name, languages.try_find(span.name).default_value(frozenset()) | {span.language})
        return tuple(span for span in spans if len(languages[span.name]) > 1)
```

## [3]-[RESEARCH]

[TREE_SITTER_QUERY] and [DRIFT_GRAMMARS] are reflection-confirmed: the `tree-sitter` binding compiles patterns through the `Query(language, pattern)` constructor (the deprecated `Language.query(pattern)` shim is refused) and runs captures through `QueryCursor(query).captures(node)` returning a `dict[str, list[Node]]` the fence flattens, `Parser(language).parse(source)` parses bytes to a `Tree`, and the grammar pointers are `tree_sitter_python.language()`, `tree_sitter_typescript.language_typescript()`, and `tree_sitter_typescript.language_tsx()`. No open RESEARCH seam remains on this page.
