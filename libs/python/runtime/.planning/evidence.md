# [PY_RUNTIME_EVIDENCE]

External-API and structural-parsing evidence, plus the private entrypoint grammar. `ApiPackage`/`ApiMember` record distribution/import/owner/capability/entrypoint facts the `assay code` rail consumes; `tree-sitter` supplies structural-parsing evidence over Python and TypeScript sources. `Entrypoint` is the type-hint-driven command axis over `cyclopts` backing the companion daemon's PRIVATE entry only — never a new public command surface.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | API       | API + structural-parsing evidence records                      |
|   [2]   | ENTRY     | the private companion entrypoint grammar                       |

## [2]-[API]

- Owner: `ApiPackage` — the distribution/import/owner/capability/entrypoint record over `importlib.metadata`; `ApiMember` the official-surface row source may later name; `Structural` the static surface over `tree-sitter` parsing Python/TypeScript sources into evidence the `assay code` rail consumes.
- Entry: `ApiPackage.reflect` reads one distribution's metadata and entry points; `Structural.query` runs a tree-sitter S-expression query over a source tree returning matched spans.
- Packages: `importlib.metadata` (stdlib), `tree-sitter` (`Language`/`Parser`/`Query`), `tree-sitter-python`, `tree-sitter-typescript`.
- Growth: a new evidence field is one column on `ApiPackage`; a new language is one `tree-sitter` grammar binding; zero new surface.
- Boundary: no package version tables in planning pages, no guessed environment status; source cannot name a member absent from `.api` evidence; the structural-parsing surface emits evidence the `assay code` rail consumes, never a competing search owner.

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
    def query(language: Language, source: bytes, pattern: str) -> list[tuple[int, int]]:
        tree = Parser(language).parse(source)
        return [(node.start_byte, node.end_byte) for _, node in Query(language, pattern).captures(tree.root_node).items()]
```

## [3]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven command axis over `cyclopts` backing the companion daemon's PRIVATE entry and package-internal entrypoints only.
- Entry: `Entrypoint.app` returns the `cyclopts.App` whose commands bind to the companion serve and drain; arguments bind from type hints, never from a hand-parsed `argv`.
- Packages: `cyclopts` (`App`/`Parameter`).
- Growth: a new private command is one `@app.command` method on the existing app; zero new surface.
- Boundary: never a new public command surface — the `seam-splits` Assay-command-surface law reserves public commands to `tools/assay`; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from cyclopts import App

from rasm.runtime.server_host import Credential, ServerHost


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry")

    @app.command
    async def serve(bind: str, *, grace: float = 5.0) -> None:
        host = ServerHost(bind, Credential.InsecureLoopback(), grace)
        await host.serve()

    return app
```

## [4]-[RESEARCH]

- [TREE_SITTER_QUERY]: the `tree_sitter.Query.captures` return shape (a mapping of capture-name to node list under `tree-sitter>=0.25.2`) is verified against `.api/api-tree-sitter.md`; the `Parser(language)` constructor arity confirms at fence transcription.
