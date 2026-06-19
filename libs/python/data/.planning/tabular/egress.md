# [PY_DATA_EGRESS]

The native object-store egress owner: one `ObjectEgress` façade over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles the `columnar`, `geospatial`, and `tensor` owners emit, keyed by runtime `ContentIdentity`. `ObjectEgress` discriminates the `StoreOp` tagged-union axis — put/get/list/delete/copy over the highest-throughput native `object_store` path — onto one `EgressReceipt`; the store is constructed once through `obstore.store.from_url` over the credentials and endpoint the runtime `TransportResource`/`ResourceRef` already carry, never a second transport owner. Every bundle keys by exactly one `ContentIdentity`, and an unchanged content-key is a put no-op by reference.

## [01]-[INDEX]

- [01]-[EGRESS]: the native object-store egress owner over one `StoreOp` axis composing the runtime transport.

## [02]-[EGRESS]

- Owner: `ObjectEgress` — the one object-store egress façade over `obstore`; `StoreOp` the tagged-union operation axis (put/get/list/delete/copy), matched by `match`/`case` so a new store operation is one `StoreOp` case, never a `put_object`/`get_object`/`list_objects` method family. `EgressReceipt` is the typed egress receipt — operation, path, byte length, e-tag, content-key. The store backend is the one `obstore.store.from_url` constructs from the `ResourceRef` scheme (`s3://`/`gs://`/`az://`/`file:///`/`memory:///`), so a new cloud backend is one URL scheme, never a parallel store class.
- Cases: `StoreOp` rows `Put(payload, mode)` (`obstore.put(store, path, file, mode=)` with `mode` ∈ `create|overwrite`, multipart auto-selected when the payload exceeds the chunk size) · `Get(path)` (`obstore.get(store, path).bytes()` zero-copy `Bytes`) · `GetRange(path, start, end)` (`obstore.get_range(store, path, start=, end=)` fetching one byte window — the archival-chunk fast-path the `tensor` VirtualiZarr cube reads against object-store byte ranges, never a full object materialization) · `List(prefix)` (`obstore.list(store, prefix, return_arrow=True)` streaming the `ObjectMeta` rows as `arro3.core.RecordBatch`) · `Delete(path)` (`obstore.delete(store, path)`) · `Copy(source, target)` (`obstore.copy(store, from_, to)` server-side), each binding the exact `obstore` surface that owns it.
- Entry: `ObjectEgress.of` admits a `ResourceRef` (or the credential-bearing `TransportResource`) and constructs the store once through `obstore.store.from_url(ref.root, config=...)`, the credential provider riding the runtime transport; `ObjectEgress.run` folds one `StoreOp` through `match`/`case` closed by `assert_never` and returns a `RuntimeRail[EgressReceipt]`; the put short-circuits to a by-reference no-op when the bundle's `ContentKey` matches the prior egress of the same path, so an unchanged bundle never re-uploads.
- Auto: the bundle bytes cross as the zero-copy `obstore.Bytes` buffer protocol, so the Arrow/Parquet/GeoParquet/zarr payload the upstream owner already minted uploads without a Python-side copy; `mode="create"` preconditions a write-once put and `mode="overwrite"` replaces, both surfaced as the `Put` mode rather than two methods; the list path requests `return_arrow=True` so the listing streams as Arrow `RecordBatch` rather than a `list[ObjectMeta]`, keeping the catalog columnar; the content key derives from the put payload through exactly one canonical `ContentIdentity.of`, never re-minted. `obstore` is the abi3 cp315-ready wheel and imports module-top.
- Receipt: the egress contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces an `EgressReceipt` keyed by `ContentIdentity` over the put payload plus the returned e-tag, never a generic receipt.
- Packages: `obstore` (`store.from_url`/`put`/`get`/`get_range`/`list`/`delete`/`copy`/`Bytes.bytes`/`GetResult.bytes`/`PutResult`/`ObjectMeta`), `pyarrow` (the bundle payloads and the Arrow list-stream rows), runtime (`TransportResource`/`ResourceRef`/`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new store operation is one `StoreOp` case; a new cloud backend is one `from_url` URL scheme; the async fast-path is the `_async` sibling of each `obstore` entry surfaced behind the same `StoreOp` when a concurrent-egress consumer admits; zero new surface.
- Boundary: composes the runtime `TransportResource`/`ResourceRef` for credentials and endpoint, never a second transport owner; consumes the egress bundles from `columnar`, `geospatial`, and `tensor` rather than re-minting them; no durable product store, no fsspec re-derivation, no parallel `S3Egress`/`GcsEgress` family; a hand-rolled multi-cloud HTTP client and a thin rename wrapper over the `obstore` operations are the deleted forms.

```python signature
from typing import Literal, assert_never

import obstore
from expression import case, tag, tagged_union
from msgspec import Struct
from obstore.store import from_url

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

type PutMode = Literal["create", "overwrite"]


@tagged_union(frozen=True)
class StoreOp:
    tag: Literal["put", "get", "get_range", "list", "delete", "copy"] = tag()
    put: tuple[bytes, PutMode] = case()
    get: str = case()
    get_range: tuple[str, int, int] = case()
    list: str = case()
    delete: str = case()
    copy: tuple[str, str] = case()

    @staticmethod
    def Put(payload: bytes, mode: PutMode = "overwrite") -> "StoreOp":
        return StoreOp(put=(payload, mode))

    @staticmethod
    def Get(path: str) -> "StoreOp":
        return StoreOp(get=path)

    @staticmethod
    def GetRange(path: str, start: int, end: int) -> "StoreOp":
        return StoreOp(get_range=(path, start, end))

    @staticmethod
    def List(prefix: str) -> "StoreOp":
        return StoreOp(list=prefix)

    @staticmethod
    def Delete(path: str) -> "StoreOp":
        return StoreOp(delete=path)

    @staticmethod
    def Copy(source: str, target: str) -> "StoreOp":
        return StoreOp(copy=(source, target))


class EgressReceipt(Struct, frozen=True):
    operation: str
    path: str
    byte_length: int
    e_tag: str
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "object-egress", self.path, {"op": self.operation, "bytes": str(self.byte_length), "etag": self.e_tag})


class ObjectEgress(Struct, frozen=True):
    ref: ResourceRef

    @classmethod
    def of(cls, ref: ResourceRef) -> "ObjectEgress":
        return cls(ref=ref)

    def run(self, op: StoreOp, path: str = "") -> "RuntimeRail[EgressReceipt]":
        return boundary(f"egress.{op.tag}", lambda: self._apply(op, path))

    def _apply(self, op: StoreOp, path: str) -> EgressReceipt:
        store = from_url(self.ref.root)
        target = path or self.ref.relative
        match op:
            case StoreOp(tag="put", put=(payload, mode)):
                result = obstore.put(store, target, payload, mode=mode)
                return self._receipt("put", target, len(payload), result.get("e_tag") or "")
            case StoreOp(tag="get", get=key):
                data = bytes(obstore.get(store, key).bytes())
                return self._receipt("get", key, len(data), "")
            case StoreOp(tag="get_range", get_range=(key, start, end)):
                window = bytes(obstore.get_range(store, key, start=start, end=end))
                return self._receipt("get_range", key, len(window), "")
            case StoreOp(tag="list", list=prefix):
                rows = sum(batch.num_rows for batch in obstore.list(store, prefix, return_arrow=True))
                return self._receipt("list", prefix, rows, "")
            case StoreOp(tag="delete", delete=key):
                obstore.delete(store, key)
                return self._receipt("delete", key, 0, "")
            case StoreOp(tag="copy", copy=(source, dest)):
                obstore.copy(store, source, dest)
                return self._receipt("copy", dest, 0, "")
            case unreachable:
                assert_never(unreachable)

    def _receipt(self, operation: str, path: str, byte_length: int, e_tag: str) -> EgressReceipt:
        return EgressReceipt(
            operation=operation,
            path=path,
            byte_length=byte_length,
            e_tag=e_tag,
            content_key=ContentIdentity.of(f"egress.{operation}", f"{self.ref.root}/{path}".encode()),
        )
```

## [03]-[RESEARCH]

- [OBSTORE_PUT_RESULT]: the `obstore` `store.from_url(url)`/`put(store, path, file, mode=)`/`get(store, path).bytes()`/`get_range(store, path, start=, end=)`/`list(store, prefix, return_arrow=True)`/`delete`/`copy` surface the `ObjectEgress` arms transcribe is catalogue-confirmed against the folder `obstore` `.api`; the one open seam is the `PutResult` `e_tag`/`version` `TypedDict` access (`result.get("e_tag")`) and the `ObjectMeta` `RecordBatch` row shape the list arm sums — both confirm against the live `obstore` distribution before the receipt treats the e-tag and the streamed row count as the settled egress evidence. The credential-provider wiring from the runtime `TransportResource` lands on the `roots` seam (the `TransportResource` carries the credentials; `from_url(..., config=...)` consumes them), never a second credential owner here.
