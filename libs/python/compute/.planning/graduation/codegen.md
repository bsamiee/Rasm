# [PY_COMPUTE_CODEGEN]

The typed-stub generator that decodes the C# graduation-evidence bundle shape. `StubCodegen` consumes the offline-seam bundle the C# side seals, decodes it once with msgspec, and projects each owner descriptor into a `msgspec.Struct` stub through the stdlib `ast` builder, so downstream Python composes against the C# owner row by import rather than by re-typing it. The bundle shape is consumed at the boundary and never re-minted: this owner reads the evidence bundle, owner, and field descriptors the C# graduation evidence already carries and adds zero shape. It emits type stubs only, never runtime behavior, and imports nothing from a C# interior.

## [01]-[INDEX]

- [01]-[STUB_CODEGEN]: the evidence-bundle decode and the per-field-kind annotation fold on one `StubCodegen` owner.

## [02]-[STUB_CODEGEN]

- Owner: `StubCodegen` — the one generator consuming the C# graduation-evidence bundle shape and emitting typed Python stubs; it reads `EvidenceBundle`, `OwnerDescriptor`, and `FieldDescriptor` (the shape the C# graduation evidence carries) and projects each descriptor into a `msgspec.Struct` stub through the stdlib `ast` builder. The bundle is decoded once with msgspec and never re-minted.
- Cases: one `FieldKind` literal per primitive the C# seam emits — `i32`, `i64`, `f64`, `bool`, `string`, `key`, `array`, `nested` — folded by `match` to the Python annotation (`int`, `float`, `bool`, `str`, `ContentKey`, `tuple[..., ...]`, the nested stub name). A new managed field kind is one literal and one match arm, never a parallel emitter.
- Entry: `StubCodegen.emit(raw)` returns `RuntimeRail[GeneratedModule]` through one `boundary`; it decodes the bundle with the module `_BUNDLE_DECODER`, folds the owners into `ast.ClassDef` nodes, and renders the module source with `ast.unparse`. `GeneratedModule` carries the rendered source, the owner count, and the bundle `ContentKey`.
- Seam: `EvidenceBundle` is the decoded projection of the C# graduation-evidence bundle — the owners, the C# schema version, and the `ContentKey` the C# side sealed it under. This is the only place compute reads the C# evidence shape; it imports nothing from a C# interior and re-mints nothing, so the seam crosses once, offline, as bytes.
- Packages: stdlib `ast` (stub synthesis), `msgspec` (bundle decode), runtime (`RuntimeRail`, `boundary`, `ContentKey`).
- Growth: a new field kind is one `FieldKind` literal and one match arm; zero new surface.
- Boundary: codegen emits type stubs only — `msgspec.Struct` declarations — never runtime behavior, never a re-derived evidence shape, and never a C# source shape. The C# graduation-evidence bundle is the upstream authority; this owner is a one-directional offline consumer. `ast` and `msgspec` are both reflectable on cp315, so there is no deploy gate.

```python signature
import ast
from typing import Literal

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


type FieldKind = Literal["i32", "i64", "f64", "bool", "string", "key", "array", "nested"]

_ANNOTATION: dict[FieldKind, str] = {"i32": "int", "i64": "int", "f64": "float", "bool": "bool", "string": "str", "key": "ContentKey"}


class FieldDescriptor(Struct, frozen=True):
    name: str
    kind: FieldKind
    element: "FieldDescriptor | None" = None
    nested: str | None = None


class OwnerDescriptor(Struct, frozen=True):
    name: str
    fields: tuple[FieldDescriptor, ...]


class EvidenceBundle(Struct, frozen=True):
    schema_version: str
    owners: tuple[OwnerDescriptor, ...]
    bundle_key: ContentKey


class GeneratedModule(Struct, frozen=True):
    source: str
    owner_count: int
    bundle_key: ContentKey


_BUNDLE_DECODER = msgspec.json.Decoder(type=EvidenceBundle)


class StubCodegen:
    @staticmethod
    def emit(raw: bytes) -> "RuntimeRail[GeneratedModule]":
        return boundary("codegen.stub", lambda: StubCodegen._render(_BUNDLE_DECODER.decode(raw)))

    @staticmethod
    def _render(bundle: EvidenceBundle) -> GeneratedModule:
        body: list[ast.stmt] = [StubCodegen._class(owner) for owner in bundle.owners]
        module = ast.Module(body=body, type_ignores=[])
        ast.fix_missing_locations(module)
        return GeneratedModule(source=ast.unparse(module), owner_count=len(bundle.owners), bundle_key=bundle.bundle_key)

    @staticmethod
    def _class(owner: OwnerDescriptor) -> ast.ClassDef:
        fields = [StubCodegen._field(f) for f in owner.fields]
        return ast.ClassDef(
            name=owner.name,
            bases=[ast.Name(id="Struct", ctx=ast.Load())],
            keywords=[ast.keyword(arg="frozen", value=ast.Constant(value=True))],
            body=fields or [ast.Pass()],
            decorator_list=[],
            type_params=[],
        )

    @staticmethod
    def _field(field: FieldDescriptor) -> ast.AnnAssign:
        return ast.AnnAssign(
            target=ast.Name(id=field.name, ctx=ast.Store()),
            annotation=ast.parse(StubCodegen._annotation(field), mode="eval").body,
            value=None,
            simple=1,
        )

    @staticmethod
    def _annotation(field: FieldDescriptor) -> str:
        match field:
            case FieldDescriptor(kind="array", element=FieldDescriptor() as inner):
                return f"tuple[{StubCodegen._annotation(inner)}, ...]"
            case FieldDescriptor(kind="nested", nested=str() as ref):
                return ref
            case FieldDescriptor(kind=kind):
                return _ANNOTATION[kind]
```
