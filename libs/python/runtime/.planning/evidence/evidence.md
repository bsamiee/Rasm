# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence. `ApiPackage`/`ApiMember` record distribution/import/owner/capability/entrypoint facts over `importlib.metadata`; `Structural` runs `tree-sitter` S-expression queries over Python and TypeScript sources into matched spans the `assay code` rail consumes. The surface produces evidence the rail reads — never a competing search owner, never a guessed environment status.

## [1]-[INDEX]

One cluster: `[2]-[API]` — API and structural-parsing evidence records.

## [2]-[API]

- Owner: `ApiPackage` — the distribution/import/owner/capability/entrypoint record over `importlib.metadata`; `ApiMember` the official-surface row a source may later name; `Structural` the static surface over `tree-sitter` parsing Python/TypeScript sources into evidence the `assay code` rail consumes.
- Entry: `ApiPackage.reflect` reads one distribution's metadata and entry points; `Structural.query` runs a tree-sitter S-expression query over a source returning `(capture-name, start-byte, end-byte)` rows, flattening the capture-name-to-node-list mapping the binding returns.
- Packages: `importlib.metadata` (stdlib), `tree-sitter` (`Language`/`Parser`/`Query`), `tree-sitter-python`, `tree-sitter-typescript`, `msgspec`.
- Growth: a new evidence field is one column on `ApiPackage`; a new language is one `tree-sitter` grammar binding; zero new surface.
- Boundary: no package version tables in planning pages, no guessed environment status; a source cannot name a member absent from the catalogue evidence; the structural-parsing surface emits evidence the `assay code` rail consumes, never a competing search owner.

```python signature
from importlib import metadata

from msgspec import Struct
from tree_sitter import Language, Parser, Query


class ApiMember(Struct, frozen=True):
    symbol: str
    family: str
    rail: str


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


class Structural:
    @staticmethod
    def query(language: Language, source: bytes, pattern: str) -> tuple[tuple[str, int, int], ...]:
        tree = Parser(language).parse(source)
        captures = Query(language, pattern).captures(tree.root_node)
        return tuple(
            (name, node.start_byte, node.end_byte)
            for name, nodes in captures.items()
            for node in nodes
        )
```

## [3]-[RESEARCH]

- [TREE_SITTER_QUERY]: the `Query(language, pattern)` constructor versus the `Language.query(pattern)` factory, and the `Parser(language)` versus `Parser(); parser.language = language` construction, confirm against the current `tree-sitter` Python binding at fence transcription; the capture return is the `dict[str, list[Node]]` shape the fence flattens.
