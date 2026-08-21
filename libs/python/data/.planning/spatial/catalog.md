# [PY_DATA_CATALOG]

Cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` resolving which cloud assets cover a query region — the discovery layer above the raster/vector claims (`spatial/geospatial#GEO`) and the archival byte-window read (`tabular/egress#EGRESS`) that the geospatial and object-store lanes lack. `StacCatalog.discover` folds the one `StacQuery` search axis onto the single keyword-only `Client.search`, the `Surface` discriminant alone routing the shared rows to `Client.collection_search` on a `FreeText` case, pages the lazy `ItemSearch` into a `pystac.ItemCollection`, and emits one `StacDiscovery` keyed by a runtime `ContentIdentity` over the matched item-id set. One `Signing` value encodes the request boundary as a frozen `SchemeRow` read by name — into `Client.open(modifier=)` for discovery, over the `ItemSearch` for asset hrefs, and into `odc.stac.load(patch_url=)` for the COG load — never a signed-vs-unsigned client pair.

Discovered collections encode as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `tabular/columnar#SCAN` scan and `tabular/query#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `tabular/egress#EGRESS` receipt owner over the `runtime/transport/roots#STORE` `StoreOp.GetRange` byte-window read, the `gridded/virtual#VIRTUAL` `VirtualReference.apply` virtual-chunk registration over the `gridded/virtual#MANIFEST` `FieldVirtual` manifest, and the `odc-stac` `odc.stac.load` COG datacube driven by the catalogue-derived `stac_cfg`/`patch_url`. Every bundle keys by exactly one runtime `ContentIdentity`; the network legs ride the runtime `guarded(RetryClass.HTTP, ...)` envelope, never a second object-store transport, virtual-cube builder, COG loader, or hand-fed band metadata.

## [01]-[INDEX]

- [02]-[CATALOG]: the `StacCatalog` discovery owner over `pystac-client`, the `StacQuery` search axis with the `Surface` discriminant routing item-vs-collection search, emitting one `StacDiscovery`.
- [03]-[TABLE]: the one `StacGeoClaim` STAC-table owner over `stac-geoparquet`, folding a closed `StacTableOp` axis both directions behind one `apply` over a `(source, sink)` capability roster.
- [04]-[ASSETS]: the one awaitable `AssetFold` routing signed hrefs into egress byte-windows, virtual cube chunks, or the `odc-stac` COG datacube.

## [02]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root carrying one `Signing`. `Signing` holds TWO orthogonal axes on one value: `scheme` names the href-REWRITE dispatch (a `SchemeRow` read by name) and `credentials` the obstore-native provider the asset-BYTE reads bind — Planetary Computer does both, NASA Earthdata mints storage credentials and rewrites nothing, a public catalog does neither, so neither axis is a member of the other's vocabulary and no fake scheme row stands in for a credential-only catalog. `StacQuery` is the tagged-union search axis folded by `match`/`case` onto the single keyword-only `Client.search`; a new search modality is one case, never a `search_bbox`/`search_intersects`/`search_cql2` method family. `Surface` is the discovery-method discriminant carrying its own frozen `SurfaceRow` `(method, cap, accepts, materialize)`: the `accepts` keyword-admission set is the boundary that keeps the union total across surfaces — an `ids`/`intersects`/`collections` row a `FreeText` union folds in never reaches `collection_search`, which rejects it — and the `materialize` policy owns the structural divergence between the two iterators, since the `ITEM` row signs the `ItemSearch` and reads `matched()` while a `CollectionSearch` carries no Azure hrefs and no `sign`/`matched` member, yielding zero hrefs and typed absence for every fact it cannot answer. The credential axis rides the branch `Option` and lowers to the runtime's own nullable `Provider` spelling at one projection, `Signing.provider()`, because `ResourceRef` is the owner of that admission and no asset arm re-derives it. `Signing` encodes the request boundary as one frozen `SchemeRow` (`NONE`/`PLANETARY_COMPUTER`) whose `open_kwargs`/`sign`/`patch_url` callables are read by name, never a positional triple, parallel `match` statements, or a forwarded bare callable.
- Cases: `StacQuery` rows — `Bbox`, `Intersects` (a GeoJSON-geometry dict the server intersects server-side, no shapely at the boundary), `Datetime` (an RFC-3339 interval), `Ids`, `Collection`, `Cql2Filter` (a CQL2-JSON predicate the STAC API evaluates server-side), `Cql2Query` (the legacy `query` extension), `Order` (server-side sort with field projection), and `FreeText` (whose presence flips `Surface` from `ITEM` to `COLLECTION`, routing to `Client.collection_search`). Each carries a `params()` projection contributing exactly its own keyword arguments, so an n-axis query unions the per-case keyword dicts rather than forking a method per axis; because `collection_search` shares the `bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields` axis with `search`, the shared rows union onto either surface unchanged, the surface alone differing.
- Entry: `StacCatalog.discover` computes the pure plan — reduces the query tuple's `params()` into one keyword set and recovers `Surface.of_queries` and its `.row` — then drives the whole blocking `pystac_client` sequence (`Client.open`, `row.call`, `row.materialize`) through one `guarded(RetryClass.HTTP, on_thread, ...)` envelope, the `THREAD_BAND`-bounded hop, so the synchronous I/O never stalls the event loop and the transient `429`/`5xx`/timeout set retries under a `Retry-After`-honouring backoff as one logical discovery. `row.call` reads the method, cap, and `accepts` keyword filter off the `SurfaceRow` so a cross-surface param never reaches a method that rejects it, and `row.materialize` returns the full shaped `(collection, item_ids, matched, href_count, expiry, url)` outcome — the `ITEM` row's one `planetary_computer.sign(ItemSearch)` dispatch both materializes the page and rewrites every Azure blob href in a single pass, reports `matched()` and the resolved-GET `url_with_parameters()`, and reads each item's `msft:expiry`; the `COLLECTION` row materializes without signing, yielding a zero href count and three ABSENCES — no server total, no expiry horizon, no resolved url — never its own page length wearing the archive total's name. Results flatten through `.bind(self._shape(surface))`, which folds the railed `ContentIdentity.of` over the item-id set through `.map` into one `StacDiscovery` rather than stuffing a `RuntimeRail[ContentKey]` into the `content_key` field.
- Auto: the `params` fold is the union law — a bbox+datetime+cloud-cover+order query is one `search`, never four; `Surface.of_queries` flips to `collection_search` exactly when a `FreeText` case is present, the one boolean-free routing read, never a `search_by_<axis>` family. `ItemSearch` is lazy so `matched()` reads the API total without materializing every page, and `sign` over the lazy handle is the one canonical materialize-plus-sign — never the deprecated `get_all_items`, never a `next`-link follow loop, never a materialize-then-re-sign two-pass. `min(msft:expiry)` over the signed items reports the token-validity horizon as EVIDENCE alone: the bound credential provider owns refresh inside the store handle, so a fan-out outliving that window re-signs transparently rather than failing mid-read against a horizon this owner could only report and never renew. `pystac-client`/`pystac`/`planetary-computer` and both `obstore.auth` providers declare once at module scope under `lazy import`/`lazy from` and reify on first use, so the signing band costs nothing until a discovery runs; every `_SCHEME` cell reads the provider inside a call-time thunk, because a module-scope cell holding a live attribute reifies the proxy at import and re-opens the eager band the manifest bans. The runtime rails ride eager module-level.
- Receipt: one `StacDiscovery` carries the signed `ItemCollection`, the local item-id census, the SERVER's own `matched()` total as its own optional fact, the href count, the `msft:expiry` horizon, the resolved `url`, and the `ContentKey`; `contribute()` yields one emitted-phase `Receipt.of("catalog", ...)` spelling the `domain`/`kind`/`key` lifted columns beside the `rasm.catalog.items` measure it records, the counts native scalars, and the three surface-optional facts OMITTED where the surface reported none, never a parallel result-versus-receipt pair.
- Packages: `pystac-client` (the keyword-only `Client.search`/`collection_search`, `ItemSearch.{item_collection,matched,url_with_parameters}` the `ITEM` row reads, `CollectionSearch.collection_list` the `COLLECTION` row reads), `pystac` (`ItemCollection`/`Item`/`Asset`, the `msft:expiry` token horizon), `planetary-computer` (`sign` the `singledispatch` over the lazy `ItemSearch`, `sign_inplace` the `modifier=` callable, `set_subscription_key`), `obstore` (`auth.planetary_computer.PlanetaryComputerAsyncCredentialProvider` and `auth.earthdata.NasaEarthdataAsyncCredentialProvider`, the two providers that own token refresh inside the store rather than beside it), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`Receipt`/`Metrics`/`Provider`/`RetryClass`/`guarded`/`on_thread`).
- Law: the subscription key is composition-bound on BOTH halves — the byte-read half rides each `Signing`'s own obstore provider, and the href-rewrite half rides the one process slot `planetary_computer.sign` reads behind the `_bind_subscription` compare-and-refuse latch, so two same-process apps with divergent keys collide at the factory as a typed `subscription-key-collision` refusal instead of the second app silently re-signing the first's hrefs; the factory therefore answers the rail, exactly like every admission that can refuse.
- Growth: a new search modality is one `StacQuery` case with its key on the owning surface's `accepts` set; a new href-rewrite scheme is one `SignScheme` member with its `SchemeRow`; a new credential estate is one `Signing` factory binding its own obstore provider with no `SignScheme` member at all; a new discovery surface is one `Surface` member with its `SurfaceRow` whose `accepts` names the method's admissible keywords; zero new surface; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.CATALOG` in this module's one `RAISES` table, which every section anchors on.
- Boundary: composes the runtime credential and resilience owners, never a second STAC paging loop, CQL2 compiler, SAS token fetch, conformance negotiator, or retry/backoff loop; no live UI, no durable catalog store. A `search_by_<axis>` method family, a `cap`-keyword ternary fork where the `SurfaceRow` carries the name, a blind `**params` splat onto `collection_search` where `accepts` filters the rejected keyword, a `signing.sign(...) if surface is ITEM else ...` branch where `materialize` routes, and a hand-opened `boundary` re-spelling the retry/span/lift the `guarded` envelope fuses are rejected.

```python signature
import threading
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field
from opentelemetry import trace
from opentelemetry.trace import SpanKind

# the signing providers declare at the module boundary and reify on first use: every `_SCHEME` cell below holds a
# call-time thunk over `planetary_computer`, never a live attribute, because a row dereferencing one at module
# scope would reify the proxy at import and pay the whole signing band for an unsigned catalogue read.
lazy import planetary_computer
lazy from obstore.auth.earthdata import NasaEarthdataAsyncCredentialProvider
lazy from obstore.auth.planetary_computer import PlanetaryComputerAsyncCredentialProvider
lazy from pystac_client import Client

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, FaultRow, RuntimeRail, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import Provider, origin

if TYPE_CHECKING:
    from collections.abc import Iterable

    from pystac import Collection, ItemCollection
    from pystac_client import ItemSearch

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.catalog")

# the raise anchors the retried legs on this page key on: `reliability/resilience#RESILIENCE` `guarded` takes the
# caller's own rostered `at: FaultRow[L]`, so the breaker arc, the rate bucket, the span, and the lifted fault all
# derive ONE coordinate the roster proves against a real module — the free `subject=<str>` it retired could spell a
# leg this package never declares. Every row here is an outbound catalog leg, so each declares TRANSIENT. The table
# seats in this fence and every section of the module anchors on it.
STAC_DISCOVER: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="discover", arm="boundary", defect="discover-refused", retriability=TRANSIENT
)
STAC_CLAIM: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="claim", arm="boundary", defect="claim-refused", retriability=TRANSIENT
)
STAC_COVERAGE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.coverage", arm="boundary", defect="coverage-refused", retriability=TRANSIENT
)
# the construction and admission corners beside them: each is CALLER-repairable — a colliding subscription key, an
# unrouted source/sink pair, a column the item never declared, a fold re-entered on a collection surface — so each
# rides `config` and refuses identically on a re-offer. The signing row carries NO slot: the colliding value is a
# subscription key, and a secret is the one coordinate a fault must never name.
STAC_SIGNING: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="signing", arm="config", defect="key-collision", retriability=TERMINAL
)
STAC_ROUTE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="table.route", arm="config", defect="unrouted-pair", retriability=TERMINAL, slots=("source", "sink")
)
STAC_COLUMN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.claim", arm="config", defect="column-undeclared", retriability=TERMINAL, slots=("column",)
)
# the fold-surface refusal moves from `boundary` to `config`: a collection discovery re-entered on the asset fold is
# a caller's own composition error the same inputs refuse identically, never a provider raise a re-issue may clear,
# and the retriability read the two cases drive apart is exactly what the misfiled arm was inverting.
STAC_SURFACE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.surface", arm="config", defect="collection-terminal", retriability=TERMINAL
)
# two outcome-shape refusals a provider answered but a consumer cannot use: a byte window the egress keyed nothing
# for, and an aggregate that yielded no `VirtualReceipt`. Both stay `boundary` — the provider ran and its answer is
# the defect — and both stay TERMINAL, since re-issuing the same window or the same aggregate answers alike.
STAC_KEYLESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.egress", arm="boundary", defect="no-content-key", retriability=TERMINAL, slots=("href", "window")
)
STAC_CUBE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.cube", arm="boundary", defect="no-virtual-receipt", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    STAC_DISCOVER,
    STAC_CLAIM,
    STAC_COVERAGE,
    STAC_SIGNING,
    STAC_ROUTE,
    STAC_COLUMN,
    STAC_SURFACE,
    STAC_KEYLESS,
    STAC_CUBE,
]))

type Bound = tuple[float, float, float, float]
type Geometry = dict[str, object]
type Predicate = dict[str, object]
type Fields = dict[str, tuple[str, ...]]
type SearchParams = dict[str, object]
type Headers = dict[str, str]
type Modifier = Callable[[object], object]
type OpenKwargs = dict[str, object]
# `Credential` names the PRESENT half of the runtime's `Provider` alias (`Callable[[], Any] | None`). This page carries the
# credential estate in the branch absence carrier, so `Some(None)` is unrepresentable and the nullable spelling
# re-appears exactly once — at the `ResourceRef` seam, whose own field IS `Provider` and which owns that admission.
type Credential = Callable[[], object]
# first slot: the `ITEM` row's signed `ItemCollection`, or the `COLLECTION` row's `list[Collection]`. Slots three,
# five, and six are OPTIONAL because only the `ITEM` surface reports them and even it reports the total only where the
# API declares one — the `COLLECTION` iterator carries no `matched`, no Azure expiry, and no resolved GET url.
type Materialized = tuple[
    "ItemCollection | list[Collection]", tuple[str, ...], Option[int], int, Option[str], Option[str]
]


_PC_LOCK: Final = threading.Lock()
_PC_BOUND: list[str] = []


def _bind_subscription(key: str) -> "RuntimeRail[None]":
    # the href-rewrite half has ONE process slot — `planetary_computer.sign` reads the module global and carries no
    # per-call slot — so the latch is compare-and-refuse under a composition-time lock: the first composition binds
    # the key, an identical re-bind is a no-op, and a DIVERGENT key refuses typed, because a silent overwrite
    # re-signs the first app's hrefs under the second app's subscription.
    with _PC_LOCK:
        match _PC_BOUND:
            case []:
                planetary_computer.set_subscription_key(key)
                _PC_BOUND.append(key)
                return Ok(None)
            case [bound] if bound == key:
                return Ok(None)
            case _:
                return Error(STAC_SIGNING.raised())


class SchemeRow(Struct, frozen=True):
    open_kwargs: Callable[[Headers, float | None], OpenKwargs]
    sign: Callable[["ItemSearch"], "ItemCollection"]
    patch_url: Callable[[], Modifier | None]


class SignScheme(StrEnum):
    NONE = "none"
    PLANETARY_COMPUTER = "planetary_computer"

    @property
    def row(self) -> SchemeRow:
        return _SCHEME[self]


_SCHEME: Final[Map[SignScheme, SchemeRow]] = Map.of_seq([
    (
        SignScheme.NONE,
        SchemeRow(
            open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout},
            sign=lambda search: search.item_collection(),
            patch_url=lambda: None,
        ),
    ),
    (
        SignScheme.PLANETARY_COMPUTER,
        SchemeRow(
            open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout, "modifier": planetary_computer.sign_inplace},
            sign=lambda search: planetary_computer.sign(search),
            patch_url=lambda: planetary_computer.sign,
        ),
    ),
])


def _pc_credentials(subscription_key: str | None) -> Credential:
    return PlanetaryComputerAsyncCredentialProvider(subscription_key=subscription_key)


def _earthdata_credentials(credentials_url: str, auth: "tuple[str, str] | str | None") -> Credential:
    return NasaEarthdataAsyncCredentialProvider(credentials_url, auth=auth)


class Signing(Struct, frozen=True):
    headers: Headers = field(default_factory=dict)
    timeout: float | None = None
    scheme: SignScheme = SignScheme.NONE
    # the store-credential axis, orthogonal to `scheme`: the provider owns token refresh INSIDE the obstore handle
    # every asset-byte read crosses, so a long fan-out re-signs transparently where a reported expiry horizon could
    # only be observed. A catalog needing credentials without href rewriting binds this and keeps `SignScheme.NONE`,
    # and a PUBLIC catalog carries the branch absence carrier rather than a `None` three asset arms each re-tested.
    credentials: Option[Credential] = Nothing

    @staticmethod
    def none() -> "Signing":
        return Signing()

    @staticmethod
    def of_headers(headers: Headers, timeout: float | None = None) -> "Signing":
        return Signing(headers=headers, timeout=timeout)

    @staticmethod
    def planetary_computer(
        subscription_key: str | None = None, headers: Headers | None = None, timeout: float | None = None
    ) -> "RuntimeRail[Signing]":
        # `planetary_computer.sign` resolves its subscription key off the module global and carries no per-call slot,
        # so the href-rewrite half rides the ONE process slot behind `_bind_subscription`'s compare-and-refuse latch
        # while the byte-read half takes the key on its own composition-bound provider — two compositions with
        # distinct keys collide at the latch as a typed refusal, never as app B silently re-signing app A's hrefs.
        bound = _bind_subscription(subscription_key) if subscription_key is not None else Ok(None)
        return bound.map(
            lambda _none: Signing(
                headers=headers or {},
                timeout=timeout,
                scheme=SignScheme.PLANETARY_COMPUTER,
                credentials=Some(_pc_credentials(subscription_key)),
            )
        )

    @staticmethod
    def earthdata(
        credentials_url: str, auth: "tuple[str, str] | str | None" = None, headers: Headers | None = None, timeout: float | None = None
    ) -> "Signing":
        # NASA Earthdata mints short-lived S3 credentials and rewrites no href, so it earns no `SignScheme` member:
        # the rewrite axis stays NONE and the credential axis carries the whole estate at zero new surface.
        return Signing(headers=headers or {}, timeout=timeout, credentials=Some(_earthdata_credentials(credentials_url, auth)))

    def provider(self) -> Provider:
        # this projection is the ONE lowering back to the runtime's nullable `Provider` spelling, at the seam that owns
        # it: three asset arms stamp the same coordinate, so the projection lives here rather than as three
        # `to_optional()` calls a later arm forgets.
        return self.credentials.to_optional()

    def open_kwargs(self) -> OpenKwargs:
        return self.scheme.row.open_kwargs(self.headers, self.timeout)

    def sign(self, search: "ItemSearch") -> "ItemCollection":
        return self.scheme.row.sign(search)

    def patch_url(self) -> Modifier | None:
        return self.scheme.row.patch_url()


class SurfaceRow(Struct, frozen=True):
    method: str
    cap: str
    accepts: frozenset[str]
    materialize: Callable[[object, Signing], Materialized]

    def call(self, client: object, cap_value: int | None, limit: int | None, params: SearchParams) -> object:
        plan = {self.cap: cap_value, "limit": limit, **{k: v for k, v in params.items() if k in self.accepts}}
        return getattr(client, self.method)(**plan)


def _materialize_items(search: object, signing: Signing) -> Materialized:
    collection = signing.sign(search)
    item_ids = tuple(item.id for item in collection)
    href_count = sum(len(item.assets) for item in collection)
    # this horizon is the minimum over the items that CARRY a token expiry; a page whose items carry none reports no
    # horizon, and the two `None` sentinels this fold sees — an unreported total and an unexpiring page — admit at
    # this single read site, the last line in the module that names either.
    expiry = min((e for item in collection if (e := item.properties.get("msft:expiry"))), default=None)
    # `matched()` and this page's own item census are TWO measurements, and the first answers
    # `int | None` because a STAC API reports the total only where it declares the capability, so an unreported
    # total stays ABSENT and `item_ids` keeps the local census under its own name. Coalescing them published a page
    # length as if the server had counted the archive.
    return collection, item_ids, Option.of_optional(search.matched()), href_count, Option.of_optional(expiry), Some(search.url_with_parameters())


def _materialize_collections(search: object, _: Signing) -> Materialized:
    # `CollectionSearch` carries no `matched`, no Azure hrefs, and no resolved-GET url, so this row reports three
    # absences rather than substituting its own page length for a total the surface never answers.
    collections = search.collection_list()
    return collections, tuple(c.id for c in collections), Nothing, 0, Nothing, Nothing


class Surface(StrEnum):
    ITEM = "item"
    COLLECTION = "collection"

    @property
    def row(self) -> SurfaceRow:
        return _SURFACE[self]

    @staticmethod
    def of_queries(queries: "tuple[StacQuery, ...]") -> "Surface":
        return Surface.COLLECTION if any(q.tag == "free_text" for q in queries) else Surface.ITEM


_SHARED: Final[frozenset[str]] = frozenset({"bbox", "datetime", "query", "filter", "filter_lang", "sortby", "fields"})

_SURFACE: Final[Map[Surface, SurfaceRow]] = Map.of_seq([
    (
        Surface.ITEM,
        SurfaceRow(method="search", cap="max_items", accepts=_SHARED | {"ids", "collections", "intersects"}, materialize=_materialize_items),
    ),
    (
        Surface.COLLECTION,
        SurfaceRow(method="collection_search", cap="max_collections", accepts=_SHARED | {"q"}, materialize=_materialize_collections),
    ),
])


@tagged_union(frozen=True)
class StacQuery:
    tag: Literal["bbox", "intersects", "datetime", "ids", "collection", "cql2_filter", "cql2_query", "order", "free_text"] = tag()
    bbox: Bound = case()
    intersects: Geometry = case()
    datetime: tuple[str, str] = case()
    ids: tuple[str, ...] = case()
    collection: tuple[str, ...] = case()
    cql2_filter: Predicate = case()
    cql2_query: Predicate = case()
    order: tuple[tuple[str, ...], Fields] = case()
    free_text: str = case()

    @staticmethod
    def Bbox(west: float, south: float, east: float, north: float) -> "StacQuery":
        return StacQuery(bbox=(west, south, east, north))

    @staticmethod
    def Intersects(geometry: Geometry) -> "StacQuery":
        return StacQuery(intersects=geometry)

    @staticmethod
    def Datetime(start: str, end: str) -> "StacQuery":
        return StacQuery(datetime=(start, end))

    @staticmethod
    def Ids(*item_ids: str) -> "StacQuery":
        return StacQuery(ids=item_ids)

    @staticmethod
    def Collection(*ids: str) -> "StacQuery":
        return StacQuery(collection=ids)

    @staticmethod
    def Cql2Filter(predicate: Predicate) -> "StacQuery":
        return StacQuery(cql2_filter=predicate)

    @staticmethod
    def Cql2Query(query: Predicate) -> "StacQuery":
        return StacQuery(cql2_query=query)

    @staticmethod
    def Order(sortby: tuple[str, ...], include: tuple[str, ...] = (), exclude: tuple[str, ...] = ()) -> "StacQuery":
        return StacQuery(order=(sortby, {"include": include, "exclude": exclude}))

    @staticmethod
    def FreeText(q: str) -> "StacQuery":
        return StacQuery(free_text=q)

    def params(self) -> SearchParams:
        match self:
            case StacQuery(tag="bbox", bbox=extent):
                return {"bbox": list(extent)}
            case StacQuery(tag="intersects", intersects=geometry):
                return {"intersects": geometry}
            case StacQuery(tag="datetime", datetime=(start, end)):
                return {"datetime": f"{start}/{end}"}
            case StacQuery(tag="ids", ids=item_ids):
                return {"ids": list(item_ids)}
            case StacQuery(tag="collection", collection=ids):
                return {"collections": list(ids)}
            case StacQuery(tag="cql2_filter", cql2_filter=predicate):
                return {"filter": predicate, "filter_lang": "cql2-json"}
            case StacQuery(tag="cql2_query", cql2_query=query):
                return {"query": query}
            case StacQuery(tag="order", order=(sortby, fields)):
                return {"sortby": list(sortby), "fields": {axis: list(names) for axis, names in fields.items()}}
            case StacQuery(tag="free_text", free_text=q):
                return {"q": q}
            case unreachable:
                assert_never(unreachable)


class StacDiscovery(Struct, frozen=True):
    endpoint: str
    surface: Surface
    collection: "ItemCollection | list[Collection]"
    # `item_ids` is the LOCAL census — the ids this discovery holds — beside `matched`, the SERVER's declared archive
    # total. Two measurements, two names, and the second absent on every surface and every API that reports none.
    item_ids: tuple[str, ...]
    matched: Option[int]
    href_count: int
    expiry: Option[str]
    url: Option[str]
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        # `domain`/`kind`/`key` are the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads —
        # this pair goes to `Metrics.record` too, beside the identity this discovery minted, so the durable row lands
        # in the `catalog` partition a predicate prunes and rejoins the live series its twin emitted. A fact the
        # surface never reported OMITS its key: `"none"` is a token horizon a dashboard can sort, and the endpoint
        # standing in for an unresolved url made every collection search look like a GET that happened.
        Metrics.record({"rasm.catalog.items": float(len(self.item_ids))}, domain="catalog", kind=self.endpoint)
        reported = Block.of_seq([
            ("matched", self.matched.map(lambda total: int(total))),
            ("expiry", self.expiry.map(lambda horizon: str(horizon))),
            ("url", self.url.map(lambda resolved: str(resolved))),
        ])
        yield Receipt.of(
            "catalog",
            (
                "emitted",
                self.endpoint,
                {
                    "domain": "catalog",
                    "kind": self.endpoint,
                    "key": self.content_key.hex,
                    "surface": self.surface.value,
                    "items": len(self.item_ids),
                    "hrefs": self.href_count,
                }
                | dict(reported.choose(lambda pair: pair[1].map(lambda held: (pair[0], held)))),
            ),
        )


class StacCatalog(Struct, frozen=True):
    endpoint: str
    signing: Signing = field(default_factory=Signing.none)

    @classmethod
    def open(cls, endpoint: str, signing: Signing | None = None) -> "StacCatalog":
        return cls(endpoint=endpoint, signing=signing or Signing.none())

    async def discover(self, *queries: StacQuery, max_items: int | None = None, limit: int | None = None) -> "RuntimeRail[StacDiscovery]":
        params = reduce(lambda acc, q: acc | q.params(), queries, {})
        surface = Surface.of_queries(queries)
        row = surface.row
        # discovery is an outbound network leg — kind=CLIENT per the store span-kind law, the guarded child span riding beneath.
        with _TRACER.start_as_current_span(
            f"stac.discover.{surface.value}", kind=SpanKind.CLIENT, attributes={"rasm.geo.remote": True, "rasm.geo.op": f"discover.{surface.value}"}
        ):
            return (
                await guarded(
                    RetryClass.HTTP, on_thread, lambda: self._discover(row, params, max_items, limit), abandon=True,
                    at=STAC_DISCOVER, on=Some(origin(self.endpoint)),
                )
            ).bind(self._shape(surface))

    def _discover(self, row: SurfaceRow, params: SearchParams, max_items: int | None, limit: int | None) -> Materialized:
        client = Client.open(self.endpoint, **self.signing.open_kwargs())
        return row.materialize(row.call(client, max_items, limit, params), self.signing)

    def _shape(self, surface: Surface) -> "Callable[[Materialized], RuntimeRail[StacDiscovery]]":
        def shape(materialized: Materialized) -> "RuntimeRail[StacDiscovery]":
            collection, item_ids, matched, href_count, expiry, url = materialized
            return ContentIdentity.of("stac.discover", "\n".join(item_ids).encode()).map(
                lambda key: StacDiscovery(
                    endpoint=self.endpoint,
                    surface=surface,
                    collection=collection,
                    item_ids=item_ids,
                    matched=matched,
                    href_count=href_count,
                    expiry=expiry,
                    url=url,
                    content_key=key,
                )
            )

        return shape
```

## [03]-[TABLE]

- Owner: `StacGeoClaim` — the ONE STAC-table owner over `stac-geoparquet`, folding a closed `StacTableOp` axis in BOTH directions onto one awaitable `apply`: the discovered `pystac.ItemCollection` or a STAC-NDJSON file crossing INTO the `pyarrow` carrier the `tabular/columnar#SCAN` scan and `tabular/query#QUERY` engine consume, and back OUT to GeoParquet, NDJSON, or Delta. Two rosters carry what four sibling entrypoints used to hold between them: `_TABLE_ROUTE` is the one-call capability roster keyed by the `(TableSource, TableSink)` pair, and `_OpRow` is the dispatch row keyed by the op tag, carrying the retry class the op's remote arm elects beside the provider raise set its one boundary narrows to.
- Cases: `TableSource` — `Items` (an in-memory `pystac.Item` iterable), `Ndjson` (a STAC-NDJSON file admitted through the closed `NdjsonRef` local-versus-remote carrier), and `Table` (an Arrow table or reader already in hand). `Table` is a source because an Arrow table already held IS the input of a write exactly as an item iterable is, and that is what lets ONE `(source, sink)` roster hold the whole legality where a table-in entrypoint and a source-to-disk entrypoint each held half of it. `TableSink` — `Parquet` (versioned GeoParquet), `NdjsonOut`, `DeltaLake`. The schema axis is the `ACCEPTED_SCHEMA_OPTIONS` literal: `"FullFile"` scans every batch for the widest schema (the parse default, correctness over a heterogeneous multi-collection result), `"FirstBatch"` infers from the first batch (the write default, the lower-latency one-call path) — a parameter row, not a parallel parse. `StacPayload` names the three products — a materialized Arrow table, a written `(destination, ContentKey)` pair, and rehydrated items.
- Entry: `StacGeoClaim.apply(op)` is the one entry. It reads `_STAC_ROW` off the op tag and matches the row's retry class against the op's OWN remote coordinate: a class paired with a remote NDJSON source rides `guarded(RetryClass.HTTP, on_thread, ...)` under `SpanKind.CLIENT`, and every other pairing rides the plain banded `on_thread` hop under `INTERNAL`, so the network-versus-local discriminant is recovered from the value and never picked by which method a caller reached for. Legality is settled before the fold runs: `StacTableOp.Write` reads `_TABLE_ROUTE.try_find((source.tag, sink.tag))` and answers `RuntimeRail[StacTableOp]`, so a pair with no one-call provider surface refuses at CONSTRUCTION; `Parse` and `Rehydrate` are total over their inputs and answer the bare value, the narrowest carrier that states each outcome. Inside the hop one `boundary(..., catch=row.catch())` fences the whole leg on the op's real raise set and self-flattens the already-railed `ContentIdentity.of` through `.bind(lambda rail: rail)`.
- Auto: `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly, no NDJSON round-trip; `to_parquet` stamps the GeoParquet schema version so a downstream reader resolves the column layout without a side channel; the source-to-disk rows collapse parse-and-write into one provider call that never materializes an intermediate reader; a source already holding an Arrow table streams back at zero copy through `to_reader` rather than re-parsing a format it has already left. The parse leg materializes exactly once, at the claim boundary, because the `QueryReceipt` census reads the row count and the identity reads the whole Arrow bytes — a reader drained for the census and then handed onward is a spent handle, so the table crosses and `Table.to_reader()` hands the streaming carrier back at zero copy. `stac-geoparquet`/`pystac`/`deltalake` declare as module-scope `lazy` lines and reify at first use, so each `_TABLE_ROUTE` write and each `_OpRow` raise set is a call-time thunk — a module-scope tuple dereferencing one of those proxies reifies it at import and re-opens the eager band the manifest bans. The `geopandas`-backed trio is a fallback never called — the `arrow.*` namespace is the canonical carrier.
- Receipt: the shared `tabular/columnar` `QueryReceipt` keyed by `ContentIdentity`, never a parallel table-receipt rail. A write leg moves bytes to a destination this process does not hold, so its census row is the destination beside the key the write minted, never a re-read of what was just written.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow`/`parse_stac_ndjson_to_arrow`/`parse_stac_items_to_parquet`/`parse_stac_ndjson_to_parquet`/`to_parquet`/`stac_table_to_items`/`stac_table_to_ndjson`/`parse_stac_ndjson_to_delta_lake`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS`/`DEFAULT_*` schema axis), `pystac` (`Item.from_dict`, the `STACError` failure tree the rehydrate row narrows to), `deltalake` (`exceptions.DeltaError`, the Delta row's own native raise), `pyarrow` (the `RecordBatchReader`/`Table` carrier), runtime (`ContentIdentity`/`RuntimeRail`/`boundary`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new schema mode is one `ACCEPTED_SCHEMA_OPTIONS` row; a new source is one `TableSource` case with its `reader`/`emit`/`remote` arms; a new sink is one `TableSink` case with its `emit` arm; a new one-call path is one `_TABLE_ROUTE` row; a new work class or raise surface is one `_OpRow` column value; zero new entrypoint.
- Boundary: composes the `tabular/columnar`/`tabular/query`/`tabular/egress` owners, never a second table engine or writer; no durable catalog store. A hand-built STAC-to-Arrow schema, a hand-rolled parquet writer, a materialize-then-write two-hop where a one-call row writes straight to disk, an entrypoint family whose method names carry the source-versus-sink and local-versus-remote discriminants the values already hold, a `case _, _` corner reject evaluated inside the fold where the roster settles legality at construction, a catch-less `boundary` funnelling a whole leg through `Exception`, and the geopandas trio where the zero-copy Arrow path applies are rejected.

```python signature
from collections.abc import Callable
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import pyarrow as pa
from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry.trace import SpanKind

lazy import pystac
lazy from deltalake.exceptions import DeltaError
lazy from stac_geoparquet.arrow import (
    DEFAULT_JSON_CHUNK_SIZE,
    DEFAULT_PARQUET_SCHEMA_VERSION,
    parse_stac_items_to_arrow,
    parse_stac_items_to_parquet,
    parse_stac_ndjson_to_arrow,
    parse_stac_ndjson_to_delta_lake,
    parse_stac_ndjson_to_parquet,
    stac_table_to_items,
    stac_table_to_ndjson,
    to_parquet,
)

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import origin

if TYPE_CHECKING:
    from collections.abc import Iterable

type SchemaInference = Literal["FullFile", "FirstBatch"]


@tagged_union(frozen=True)
class NdjsonRef:
    # this carrier admits the network discriminant ONCE, at the source factory, off the uri scheme. Retry election,
    # span kind, and the traced source attribute all read this one value's tag, so no second `startswith` probe
    # stands beside the first to drift from it, and a source that is a handle rather than a path is unspellable here.
    tag: Literal["local", "remote"] = tag()
    local: str = case()
    remote: str = case()

    @staticmethod
    def of(path: str) -> "NdjsonRef":
        return NdjsonRef(remote=path) if path.startswith(("http://", "https://")) else NdjsonRef(local=path)

    def uri(self) -> str:
        match self:
            case NdjsonRef(tag="remote", remote=url):
                return url
            case NdjsonRef(tag="local", local=path):
                return path
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class TableSource:
    tag: Literal["items", "ndjson", "table"] = tag()
    items: "Iterable[object]" = case()
    ndjson: tuple[NdjsonRef, int | None] = case()
    # an Arrow table or reader already in hand: the INPUT of a write exactly as an item iterable is, which is what
    # lets one `(source, sink)` roster hold the whole corner law where a table-in surface and a source-to-disk
    # surface each held half of it and refused the other half at call.
    table: "pa.RecordBatchReader | pa.Table" = case()

    @staticmethod
    def Items(items: "Iterable[object]") -> "TableSource":
        return TableSource(items=items)

    @staticmethod
    def Ndjson(path: str, limit: int | None = None) -> "TableSource":
        return TableSource(ndjson=(NdjsonRef.of(path), limit))

    @staticmethod
    def Table(table: "pa.RecordBatchReader | pa.Table") -> "TableSource":
        return TableSource(table=table)

    def reader(self, schema: SchemaInference) -> "pa.RecordBatchReader":
        # TOTAL over the source axis, so the parse leg reads no capability roster: a table already in hand streams
        # back at zero copy rather than re-parsing a format it has already left.
        match self:
            case TableSource(tag="items", items=items):
                return parse_stac_items_to_arrow(items, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema)
            case TableSource(tag="ndjson", ndjson=(ref, limit)):
                return parse_stac_ndjson_to_arrow(ref.uri(), chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema, limit=limit)
            case TableSource(tag="table", table=held):
                return held if isinstance(held, pa.RecordBatchReader) else held.to_reader()
            case unreachable:
                assert_never(unreachable)

    def emit(self) -> tuple[object, int | None]:
        # this projection yields the provider-ready first argument and the NDJSON page cap, so each row binds
        # its one-call surface positionally instead of re-destructuring the pair the roster already proved legal.
        match self:
            case TableSource(tag="items", items=items):
                return items, None
            case TableSource(tag="ndjson", ndjson=(ref, limit)):
                return ref.uri(), limit
            case TableSource(tag="table", table=held):
                return held, None
            case unreachable:
                assert_never(unreachable)

    def remote(self) -> Option[str]:
        # this projection answers the source's foreign coordinate, or absence. Only a `remote` NDJSON source
        # names one, so a local parse and a table-in write never elect a network retry budget by which
        # entrypoint a caller reached for.
        match self:
            case TableSource(tag="ndjson", ndjson=(NdjsonRef(tag="remote", remote=url), _limit)):
                return Some(url)
            case _:
                return Nothing


@tagged_union(frozen=True)
class TableSink:
    tag: Literal["parquet", "ndjson_out", "delta_lake"] = tag()
    parquet: tuple[str, str | None] = case()
    ndjson_out: str = case()
    delta_lake: str = case()

    @staticmethod
    def Parquet(output_path: str, schema_version: str | None = None) -> "TableSink":
        return TableSink(parquet=(output_path, schema_version))

    @staticmethod
    def NdjsonOut(dest: str) -> "TableSink":
        return TableSink(ndjson_out=dest)

    @staticmethod
    def DeltaLake(table_or_uri: str) -> "TableSink":
        return TableSink(delta_lake=table_or_uri)

    def emit(self) -> tuple[str, str | None]:
        # every sink carries exactly ONE destination; only the GeoParquet row carries a schema version beside it.
        match self:
            case TableSink(tag="parquet", parquet=(output_path, version)):
                return output_path, version
            case TableSink(tag="ndjson_out", ndjson_out=dest):
                return dest, None
            case TableSink(tag="delta_lake", delta_lake=table_or_uri):
                return table_or_uri, None
            case unreachable:
                assert_never(unreachable)


class _Emit(Struct, frozen=True):
    payload: object
    dest: str
    schema: SchemaInference
    schema_version: str
    limit: int | None


def _emitted(source: TableSource, sink: TableSink, schema: SchemaInference) -> _Emit:
    payload, limit = source.emit()
    dest, version = sink.emit()
    # an omitting caller inherits the PACKAGE's own declared GeoParquet schema version, resolved at
    # call time because a default argument would dereference the lazy `stac_geoparquet` proxy at import.
    return _Emit(payload=payload, dest=dest, schema=schema, schema_version=version or DEFAULT_PARQUET_SCHEMA_VERSION, limit=limit)


def _written(emitted: _Emit) -> bytes:
    return Path(emitted.dest).read_bytes()


def _transit(emitted: _Emit) -> bytes:
    # a Delta table is a directory the writer owns rather than a file this process can read back, so the identity
    # preimage is the transit the write performed.
    return f"{emitted.payload}->{emitted.dest}".encode()


class _RouteRow(Struct, frozen=True):
    # ONE legal `(source, sink)` corner: the identity kind, the one-call provider write, and the preimage that write
    # leaves behind. Both callables are call-time thunks over the lazy `stac_geoparquet` names for the reason every
    # `_SCHEME` cell is — a row dereferencing one at module scope reifies the proxy at import.
    kind: str
    write: Callable[[_Emit], None]
    preimage: Callable[[_Emit], bytes]


# this capability roster IS the corner law: a pair absent here has no one-call `stac-geoparquet` surface at all, and
# absence alone refuses it — `(table, delta_lake)` because the Delta writer reads an NDJSON file and not an Arrow
# table, and `(items, ndjson_out)`, `(items, delta_lake)`, `(ndjson, ndjson_out)` because the package publishes no
# such call. Splitting the same law across two entrypoints taught a caller the delta corner only by calling the
# wrong one, and a `case _, _` tail taught it only by running the fold.
_TABLE_ROUTE: Final[Map[tuple[str, str], _RouteRow]] = Map.of_seq([
    (
        ("table", "parquet"),
        _RouteRow(kind="stac.geoparquet", write=lambda e: to_parquet(e.payload, e.dest, schema_version=e.schema_version), preimage=_written),
    ),
    (("table", "ndjson_out"), _RouteRow(kind="stac.ndjson", write=lambda e: stac_table_to_ndjson(e.payload, e.dest), preimage=_written)),
    (
        ("items", "parquet"),
        _RouteRow(
            kind="stac.geoparquet",
            write=lambda e: parse_stac_items_to_parquet(
                e.payload, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=e.schema, output_path=e.dest, schema_version=e.schema_version
            ),
            preimage=_written,
        ),
    ),
    (
        ("ndjson", "parquet"),
        _RouteRow(
            kind="stac.geoparquet",
            write=lambda e: parse_stac_ndjson_to_parquet(
                e.payload, e.dest, chunk_size=DEFAULT_JSON_CHUNK_SIZE, limit=e.limit, schema_version=e.schema_version
            ),
            preimage=_written,
        ),
    ),
    (
        ("ndjson", "delta_lake"),
        _RouteRow(kind="stac.delta", write=lambda e: parse_stac_ndjson_to_delta_lake(e.payload, e.dest, limit=e.limit), preimage=_transit),
    ),
])


@tagged_union(frozen=True)
class StacTableOp:
    tag: Literal["parse", "write", "rehydrate"] = tag()
    parse: tuple[TableSource, SchemaInference] = case()
    write: tuple[TableSource, TableSink, SchemaInference] = case()
    rehydrate: "pa.Table" = case()

    @staticmethod
    def Parse(source: TableSource, schema: SchemaInference = "FullFile") -> "StacTableOp":
        # `FullFile` is the parse default because a heterogeneous multi-collection discovery needs the widest schema
        # and a first batch cannot supply it; `Write` flips to `FirstBatch`, the one-call latency path.
        return StacTableOp(parse=(source, schema))

    @staticmethod
    def Write(source: TableSource, sink: TableSink, schema: SchemaInference = "FirstBatch") -> "RuntimeRail[StacTableOp]":
        # this factory is the ONE corner gate, read BEFORE the op exists and before any file opens. `Parse` and
        # `Rehydrate` answer a bare value because they are total over their inputs — each carrier states its own
        # real outcome, and railing a factory that cannot refuse makes every caller unwrap a decision nobody makes.
        return (
            _TABLE_ROUTE.try_find((source.tag, sink.tag))
            .to_result_with(lambda: STAC_ROUTE.raised(source.tag, sink.tag))
            .map(lambda _row: StacTableOp(write=(source, sink, schema)))
        )

    @staticmethod
    def Rehydrate(table: "pa.Table") -> "StacTableOp":
        return StacTableOp(rehydrate=table)

    def remote(self) -> Option[str]:
        # this projection answers the op's own foreign coordinate: both source-reading cases forward their
        # source's, and rehydrate reads an in-memory table that can name none.
        match self:
            case StacTableOp(tag="parse", parse=(source, _schema)):
                return source.remote()
            case StacTableOp(tag="write", write=(source, _sink, _schema)):
                return source.remote()
            case _:
                return Nothing


@tagged_union(frozen=True)
class StacPayload:
    tag: Literal["arrow", "written", "items"] = tag()
    arrow: "pa.Table" = case()
    # `written` pairs the destination a write reached with the key it minted there — one shape for the GeoParquet,
    # NDJSON, and Delta sinks alike, so the receipt census reads one shape instead of a per-sink column.
    written: tuple[str, ContentKey] = case()
    items: tuple[object, ...] = case()


class StacResult(Struct, frozen=True):
    payload: StacPayload
    receipt: QueryReceipt


class _OpRow(Struct, frozen=True):
    # `retry` names the class this op's REMOTE arm elects, absent where no payload of the case names a foreign
    # coordinate — that envelope is then skipped by ABSENCE, exactly as the runtime policy table skips a stage
    # whose class carries no row.
    retry: Option[RetryClass]
    # `catch` names the op's provider raise set, read at CALL time so no module-scope tuple dereferences the lazy
    # `pystac` or `deltalake` proxies at import. An unlisted raise propagates as the defect it is rather than
    # re-keying beside a real codec failure, which is why the catch-all default is never taken here.
    catch: Callable[[], tuple[type[BaseException], ...]]


_STAC_ROW: Final[Map[str, _OpRow]] = Map.of_seq([
    # an NDJSON read raises `OSError`; a malformed record or a schema conflict raises `ValueError`.
    ("parse", _OpRow(retry=Some(RetryClass.HTTP), catch=lambda: (OSError, ValueError))),
    # write rows add the Delta corner's own native rail, whose commit conflicts and protocol refusals are seam
    # failures rather than defects.
    ("write", _OpRow(retry=Some(RetryClass.HTTP), catch=lambda: (OSError, ValueError, DeltaError))),
    # rehydrate rebuilds the STAC model, so the pystac failure tree root is its real raise surface.
    ("rehydrate", _OpRow(retry=Nothing, catch=lambda: (pystac.STACError, ValueError))),
])


class StacGeoClaim(Struct, frozen=True):
    async def apply(self, op: StacTableOp) -> "RuntimeRail[StacResult]":
        # ONE entry over the closed op union in BOTH directions, and the dispatch ROW elects the work class: a row
        # carrying a retry class over an op whose source names a remote coordinate rides the runtime envelope, and
        # every other pairing rides the plain banded hop. A caller can no longer force an HTTP budget onto a local
        # file read by picking a second entrypoint, and a remote NDJSON read can no longer skip one by picking the first.
        row = _STAC_ROW[op.tag]
        match row.retry, op.remote():
            case Option(tag="some", some=cls), Option(tag="some", some=url):
                # abandon frees the band slot when an enclosing deadline trips — a wedged remote read runs out
                # unobserved; kind=CLIENT marks the outbound network leg per the store span-kind law.
                with _TRACER.start_as_current_span(
                    f"stac.claim.{op.tag}",
                    kind=SpanKind.CLIENT,
                    attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag, "rasm.geo.source": url},
                ):
                    # the op tag rides `rasm.geo.op` on the span above, so the row owns the coordinate alone. `on`
                    # answers the other question: WHICH peer this dial reached. It crosses the runtime `origin` fold
                    # because the arc is written per ORIGIN — keying it on the whole NDJSON href would mint one window
                    # per object and no arc would ever reach its trip.
                    acquired = await guarded(cls, on_thread, lambda: self._run(op, row), abandon=True, at=STAC_CLAIM, on=Some(origin(url)))
                    # `_run` is itself railed, so `guarded` yields a doubled `RuntimeRail`; the identity `bind` is the monadic join.
                    return acquired.bind(lambda inner: inner).bind(lambda payload: self._result(payload, op.tag))
            case _:
                # arrow parse and parquet writes block on disk — the banded thread hop, never the loop. The hop
                # re-wraps nothing: `_run` already converted its provider raise on the row's narrowed set, and a
                # second boundary here would re-widen that set to the `Exception` default it exists to refuse.
                with _TRACER.start_as_current_span(f"stac.claim.{op.tag}", attributes={"rasm.geo.op": op.tag}):
                    return (await on_thread(self._run, op, row)).bind(lambda payload: self._result(payload, op.tag))

    def _run(self, op: StacTableOp, row: _OpRow) -> "RuntimeRail[StacPayload]":
        # this fence is the ONE fault conversion for the whole leg, narrowed to the row's own raise set; a write
        # arm returns an already-railed `ContentIdentity.of`, so the self-flatten threads the identity fault
        # through the single carrier rather than swallowing it in an `Ok`.
        return boundary(STAC_CLAIM, lambda: self._payload(op), catch=row.catch()).bind(lambda rail: rail)

    def _payload(self, op: StacTableOp) -> "RuntimeRail[StacPayload]":
        match op:
            case StacTableOp(tag="parse", parse=(source, schema)):
                return Ok(StacPayload(arrow=source.reader(schema).read_all()))
            case StacTableOp(tag="write", write=(source, sink, schema)):
                # this roster read is a plain index: `StacTableOp.Write` already proved the corner, so no second
                # refusal arm stands here to re-decide what construction settled.
                route = _TABLE_ROUTE[(source.tag, sink.tag)]
                emitted = _emitted(source, sink, schema)
                route.write(emitted)
                return ContentIdentity.of(route.kind, route.preimage(emitted)).map(lambda key: StacPayload(written=(emitted.dest, key)))
            case StacTableOp(tag="rehydrate", rehydrate=table):
                return Ok(StacPayload(items=tuple(pystac.Item.from_dict(record) for record in stac_table_to_items(table))))
            case unreachable:
                assert_never(unreachable)

    def _result(self, payload: StacPayload, op_tag: str) -> "RuntimeRail[StacResult]":
        match payload:
            case StacPayload(tag="arrow", arrow=table):
                census = table
            case StacPayload(tag="written", written=(dest, key)):
                # a write moves bytes to a destination this process does not hold, so the census row IS the
                # destination beside the key the write minted — never a re-read of what was just written.
                census = pa.table({"target": [dest], "key": [key.hex]})
            case StacPayload(tag="items", items=items):
                # rehydrate yields `pystac.Item` objects; lower each to a dict for the Arrow receipt table `QueryReceipt` keys.
                census = pa.Table.from_pylist([item.to_dict() for item in items])
            case unreachable:
                assert_never(unreachable)
        return QueryReceipt.railed("stac_geoparquet", op_tag, census).map(lambda receipt: StacResult(payload=payload, receipt=receipt))
```

## [04]-[ASSETS]

- Owner: `AssetFold` — one awaitable fold over the signed `StacDiscovery.collection` discriminating a `FoldTarget` axis (`Egress`/`Cube`/`Coverage`) into the settled downstream seams, not a new transport. `Egress` reads the intersecting COG/GeoTIFF byte windows through `tabular/egress#EGRESS` `ObjectEgress.run_async(StoreOp.GetRange(...))` over a store this fold opens under the discovery's OWN credential provider, so the read half and the signing half share one token custody; `Cube` registers the cube-bearing hrefs as virtual chunk byte-ranges through `gridded/virtual#VIRTUAL` `VirtualReference.apply(VersionOp(aggregate=(...)))` composing the `gridded/virtual#MANIFEST` `FieldVirtual` manifest; `Coverage` reads the `proj`/`raster`/`eo` extensions into one `RasterGeoClaim` with `stac_cfg` and drives the `odc-stac` `odc.stac.load` COG datacube with `Signing.patch_url()` threaded so the reads are SAS-signed.
- Cases: the `FoldTarget` value IS the route. One shared `_raster_hrefs` generator yields the `(asset, href)` pairs whose `media_type` is in `{MediaType.COG, MediaType.GEOTIFF}` — the one media-type gate for both raster arms, never a per-arm raw-MIME set. Extension reads ride the typed `obj.ext.<short>` accessor the object type statically scopes (`sample.ext.proj`, `sample.ext.eo`, `asset.ext.raster`), so a missing extension is a typed absence, not a `KeyError`, never a raw `properties` probe; each accessor's own nullable slot then admits at the single read site in `_claim` and no interior reader re-derives it. `BandSource` is the closed two-member vocabulary naming which roster answered the census.
- Entry: `AssetFold.over` guards on `surface is Surface.COLLECTION`, returning a typed reject so a collection terminal never reaches the asset arms, then materializes `sources` once. `Egress` arms fan the egress owner's `run_async` across the byte windows inside one task group under the run-scoped `_WINDOW_BAND` limiter — one instance per running event loop, shared by every concurrent fold on it — independent reads never serialize on the store's latency — and thread the order-preserved rails through `traversed(..., by=Disposition.ABORT)` so the first byte-window fault aborts the fold; the egress owner short-circuits an unchanged content-key to a by-reference no-op. `Cube` arms cross the blocking `VirtualReference.apply` on the banded `on_thread` hop and narrow its `VirtualOutcome` to `VirtualReceipt` through one `isinstance` arm, reading the real `chunk_refs` manifest count rather than `len(sources)`. `Coverage` arms ride the HTTP envelope and self-flattens; `_claim` is the ACCUMULATING admission over the item's `proj`/`raster`/`eo` extensions, reporting every undeclared column at once through `traversed(..., by=Disposition.ACCUMULATE)` and answering one `ClaimBundle` whose optional columns carry their own absence, and only a bundle that proved its columns reaches `odc.stac.load`. Every arm closes through `_rekey`, folding the railed `ContentIdentity.of` into a `StacDiscovery` preserving every source field with the real folded count — the `Egress` preimage the per-window `href:window:content_key` rows read off the receipts, each row REFUSING a receipt that minted no key so two keyless windows never share one identity row, and its count the summed `EgressReceipt.byte_length`; the `Cube` count the manifest `chunk_refs`; the `Coverage` count the admitted band census over rows that name only what the scene measured — so changed remote bytes flip the egress key, a coverage is byte-distinct from its egress, and a single new asset flips the key.
- Auto: the byte window is the COG/GeoTIFF IFD header/overview/tile range passed straight to `GetRange`, one HTTP range request, never a full-object read; the virtual cube reuses the `FieldVirtual` owner's `ObjectStoreRegistry` backend map so STAC asset URLs register through the same runtime store fold the egress arm speaks — one transport AND one credential custody across discovery, egress, and cube, so no arm walks a token-less handle over assets the discovery signed. `ClaimBundle` is the admitted-fact owner: `proj:epsg` and the band census are required and refuse together, while the `raster:bands` fill, the `proj:transform` affine, the `eo:cloud_cover` fraction, and each asset's title ride their own absence. The census names WHICH roster answered it — `eo:bands` declares it and the per-asset `raster:bands` descriptors default it under their own source name — because one integer standing for a spectral count, a descriptor count, and a forged zero told a reader nothing. An undeclared fill omits the `nodata` override from `stac_cfg` entirely, so `odc.stac.load` reads each band's own descriptor rather than a fabricated `0.0` that would mask every genuine zero pixel; the per-band config keys by the asset NAME, the coordinate that loader addresses, never a human title or an href. `Signing.patch_url()` rides `patch_url=` so the COG reads are signed by the same dispatch that signed discovery. Loaded cube `sizes`, the CRS, and a measured cloud fraction fold into the `Coverage` key so a cloudy and a clear scene of one bbox key byte-distinct.
- Receipt: the fold re-mints the one `StacDiscovery` keyed by the fold-target `ContentIdentity` over its arm payload with the folded count; no new receipt rail.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`media_type`/`MediaType.COG`/`GEOTIFF`, the `obj.ext.proj`/`ext.eo`/`ext.raster` accessors), `odc-stac` (`odc.stac.load`), `tabular/egress` (`ObjectEgress.of`/`run_async`, the `EgressReceipt.byte_length` the fold sums), `runtime/transport/roots#STORE` (`StoreOp.GetRange` the operation axis, `Provider` the credential carry), `gridded/virtual` (`VirtualReference.apply`/`ManifestWrite`/`VirtualReceipt.chunk_refs`), `gridded/virtual#MANIFEST` (`FieldVirtual` the composed manifest cube), `spatial/geospatial` (`RasterGeoClaim`/`Resampling`), `spatial/catalog` (`Surface` the terminal guard reads), `expression` (`Block.of_seq`/`Block.choose`/`Error`, the `Option` absence carrier and its `of_optional`/`map2`/`to_result_with` admission matrix), runtime (`ContentIdentity`/`RuntimeRail`/`FaultRow`/`traversed`/`Disposition`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new archival format is one `MediaType` member in the `_raster_hrefs` gate; a new cube source is the existing `gridded/virtual#MANIFEST` `VirtualParser` case upstream, zero change here; a new coverage knob is one field on the `Coverage` row; a new extension read is one typed accessor in `_claim` landing on its own carrier, and a required one is one more rail in the accumulating sweep; a new band roster is one `BandSource` member with its `_census` arm; zero new surface.
- Boundary: reads the settled `tabular/egress`/`gridded/virtual`/`spatial/geospatial`/`odc-stac` fences and re-mints none — no second object-store transport, virtual-cube builder, COG loader, or raster claim, no full-object read where a byte window applies. A raw `properties` probe where the typed accessor applies, a `VirtualOutcome` consumed without the `VirtualReceipt` narrowing before `.chunk_refs`, a `len(sources)` count where the real receipt evidence is the count, a `_rekey` dropping `expiry`/`matched`/`url`, a caller-supplied egress owner whose store may carry no credential lifetime where this fold binds the discovery's own provider, a hand-fed `stac_cfg` where the extension read derives it, a coalesce fusing an undeclared column with a real measurement, a sentinel folded into a content preimage where the absent fact must refuse, and a fail-fast admission that names one missing column per re-run are rejected.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import anyio
from anyio import CapacityLimiter
from anyio.lowlevel import RunVar
from expression import Error, Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, structs

lazy import odc.stac
lazy from pystac import MediaType

from rasm.data.gridded.virtual import FieldVirtual, ManifestWrite, VirtualReceipt, VirtualReference, VersionOp
from rasm.data.spatial.geospatial import RasterGeoClaim, Resampling
from rasm.data.tabular.egress import EgressReceipt, ObjectEgress
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef, StoreOp, origin

if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

type StacCfg = dict[str, dict[str, object]]
type Window = tuple[int, int]

_SOURCE_OWNER: Final[str] = "rasm.data.spatial.catalog"


_WINDOW_BAND: Final[int] = 8  # concurrent byte-window reads across every egress fold; the store's own client pools connections under it
_WINDOW_LIMITER: Final[RunVar[CapacityLimiter]] = RunVar("stac-window-band")  # per-event-loop instance under the one shared band


def _window_band() -> CapacityLimiter:
    # run-scoped band: an anyio limiter binds to the loop that first uses it, so a module-global instance breaks on a
    # second loop or backend; RunVar scopes one instance per running loop — concurrent folds within one runtime share
    # the band and never multiply it, while a later loop mints its own instead of tripping on a dead binding.
    held = _WINDOW_LIMITER.get(None)
    if held is None:
        held = CapacityLimiter(_WINDOW_BAND)
        _WINDOW_LIMITER.set(held)
    return held


def _raster_hrefs(collection: object) -> "Iterator[tuple[object, str]]":
    # the media-type gate builds at CALL time — a module-scope `raster` set would dereference the lazy
    # `MediaType` proxy at import and pay the whole pystac band for a fold no caller has run yet.
    raster = {MediaType.COG, MediaType.GEOTIFF}
    return ((asset, asset.href) for item in collection for asset in item.assets.values() if asset.media_type in raster)


class BandSource(StrEnum):
    # WHICH roster the band census came off. `len(eo.bands or bands)` published a spectral census and a raster
    # descriptor census under one integer and a forged `0` where the item declared neither, so no reader could tell
    # a five-band scene from a five-descriptor asset from an item that answered nothing at all.
    SPECTRAL = "eo:bands"
    DESCRIPTOR = "raster:bands"


class ClaimBundle(Struct, frozen=True):
    # `ClaimBundle` holds the ADMITTED coverage facts, each column carrying its own absence. `crs` and the band
    # census are REQUIRED — a fabricated `EPSG:None` and a zero-band scene are not coverages — and the admission
    # reports every missing one at once. Every other column is genuinely optional at a STAC boundary and used to
    # fuse with a measurement: `0.0` is a pixel value a scene may carry, `()` is an affine, a title is not an href.
    crs: str
    band_count: int
    band_source: BandSource
    nodata: Option[float]
    transform: Option[tuple[float, ...]]
    cloud_cover: Option[float]
    stac_cfg: StacCfg
    # `claim` is the sibling-owned raster-READ carrier. `spatial/geospatial#GEO` declares `nodata` a required
    # `float`, and a read claim carrying no fill is not a claim, so an undeclared-nodata scene carries `Nothing`
    # here and the COG load reads each band's own descriptor rather than a fill this catalogue never measured.
    # `crs` and `band_count` ride this bundle's own columns, so no arm depends on the claim being present.
    claim: Option[RasterGeoClaim]


class _Census(Struct, frozen=True):
    count: int
    source: BandSource


def _census(spectral: "Option[tuple[object, ...]]", descriptors: "Option[tuple[object, ...]]") -> "Option[_Census]":
    # this census is DECLARED off `eo:bands`, DEFAULTED off the per-asset `raster:bands` descriptors under their
    # source name, and ABSENT where the item carries neither — three states the `or` coalesce collapsed into one
    # integer. The absent state never survives admission, so the bundle carries the count beside its source alone.
    match spectral.filter(lambda bands: bool(bands)), descriptors.filter(lambda bands: bool(bands)):
        case Option(tag="some", some=bands), _:
            return Some(_Census(count=len(bands), source=BandSource.SPECTRAL))
        case _, Option(tag="some", some=bands):
            return Some(_Census(count=len(bands), source=BandSource.DESCRIPTOR))
        case _:
            return Nothing


def _stac_cfg(named: "Block[tuple[str, object]]", nodata: Option[float]) -> StacCfg:
    # an asset NAME is the key `odc.stac.load` addresses per-band config by — never a human title, never an href.
    # Two untitled assets keyed by `title or href` addressed each other's bands, and an href is an address rather
    # than a name. The `nodata` override is OMITTED where the scene declared none, so the loader reads each band's
    # own descriptor instead of the fill this fold used to fabricate — and a fabricated `0.0` masked every genuine
    # zero pixel in the scene across the five reads the claim slot reaches.
    return nodata.map(lambda fill: {"*": {"assets": {name: {"nodata": fill} for name, _asset in named}}}).default_value({})


def _window_row(paired: "tuple[tuple[str, Window], EgressReceipt]") -> "RuntimeRail[str]":
    # one preimage row per read: the href, the byte window, and the payload's OWN content key. A control-plane
    # receipt mints no key — `tabular/egress#EGRESS` types the slot nullable for exactly that case — and folding
    # `''` in its place gave two keyless windows one identical row, so the content identity stopped discriminating
    # content. An absent key REFUSES instead, and the sweep accumulates so one re-run names every keyless window.
    (href, (start, end)), receipt = paired
    return (
        Option.of_optional(receipt.content_key)
        .map(lambda key: f"{href}:{start}-{end}:{key.hex}")
        .to_result_with(lambda: STAC_KEYLESS.raised(href, f"{start}-{end}"))
    )


@tagged_union(frozen=True)
class FoldTarget:
    tag: Literal["egress", "cube", "coverage"] = tag()
    # the egress arm names the store ROOT, never a pre-built owner: the fold binds the signing's own credential
    # provider onto the handle it opens, so an asset read cannot cross a store carrying no token lifetime while the
    # discovery that produced its hrefs was signed. A caller-supplied owner could silently be that store.
    egress: tuple[ResourceRef, "Mapping[str, Window]"] = case()
    cube: tuple[ResourceRef, str] = case()
    coverage: tuple[str, str, "dict[str, int] | None"] = case()

    @staticmethod
    def Egress(ref: ResourceRef, windows: "Mapping[str, Window]") -> "FoldTarget":
        return FoldTarget(egress=(ref, windows))

    @staticmethod
    def Cube(ref: ResourceRef, concat_dim: str = "time") -> "FoldTarget":
        return FoldTarget(cube=(ref, concat_dim))

    @staticmethod
    def Coverage(groupby: str = "time", resampling: Resampling = "nearest", chunks: "dict[str, int] | None" = None) -> "FoldTarget":
        return FoldTarget(coverage=(groupby, resampling, chunks))


class AssetFold(Struct, frozen=True):
    discovery: StacDiscovery
    signing: Signing

    async def over(self, target: FoldTarget) -> "RuntimeRail[StacDiscovery]":
        if self.discovery.surface is Surface.COLLECTION:
            return Error(STAC_SURFACE.raised())
        # each asset href becomes a credential-bearing REF stamped with the signing estate's own provider: the byte
        # reads authenticate and refresh through the same custody that signed discovery, and every downstream
        # consumer (egress lane, manifest registry) reads that one column instead of taking a provider beside a ref.
        # hrefs and refs are TWO values one name conflated: the byte-window arm addresses the store by href STRING
        # (`GetRange` takes a path, `windows` keys by href, and both content preimages spell it), while the manifest
        # arm needs the credential-bearing coordinate. Folding them onto one tuple left every window lookup missing,
        # so the egress fan offered zero reads and keyed an empty preimage, and the cube preimage joined structs.
        hrefs = tuple(href for _, href in _raster_hrefs(self.discovery.collection))
        sources = tuple(ResourceRef.admit(href, _SOURCE_OWNER, self.signing.provider()) for href in hrefs)
        match target:
            case FoldTarget(tag="egress", egress=(ref, windows)):
                # one owner per fold over the signing's own provider: the byte reads authenticate and refresh through
                # the same credential estate that signed discovery, never a second token custody beside it. The
                # provider stamps onto the residence COORDINATE — the lane takes no `credential_provider=` beside its
                # ref, because a lane credentialed apart from its residence is two resolutions one memo key cannot serve.
                egress = ObjectEgress.of(structs.replace(ref, credentials=self.signing.provider()))
                windowed = tuple((href, w) for href in hrefs if (w := windows.get(href)) is not None)
                # byte windows ride the egress owner's awaitable leg as independent reads — a sequential await pays the
                # store's latency once per window — fanned inside one task group under the run-scoped window band;
                # each child rails its own outcome so the group exits clean, the handle order preserves the windowed
                # order, and `traversed(by=ABORT)` still aborts the fold on the first failed read.
                band = _window_band()

                async def ranged(href: str, start: int, end: int) -> "RuntimeRail[EgressReceipt]":
                    async with band:
                        return await egress.run_async(StoreOp.GetRange(href, start, end), path=href)

                async with anyio.create_task_group() as group:
                    handles = tuple(group.start_soon(ranged, href, start, end) for href, (start, end) in windowed)
                rails = Block.of_seq(handle.return_value for handle in handles)
                return traversed(rails, by=Disposition.ABORT).bind(
                    # per-window content identity: each preimage row binds the href, the byte window, and the returned
                    # payload's operation-bytes ContentKey off the receipt — two folds over one href set with different
                    # remote content (or different windows) key distinctly; a bare href-plus-total-length preimage is the
                    # deleted content-blind form, and byte-length accounting stays the folded count. The rows sweep on
                    # ACCUMULATE, so a fold over several keyless receipts names all of them in one refusal.
                    lambda receipts: traversed(
                        Block.of_seq(tuple(zip(windowed, receipts, strict=True))).map(_window_row), by=Disposition.ACCUMULATE
                    ).bind(lambda rows: self._rekey("egress", "\n".join(rows).encode(), sum(r.byte_length for r in receipts)))
                )
            case FoldTarget(tag="cube", cube=(ref, concat_dim)):
                # the manifest walk reads archival headers over the SAME credential estate that signed the hrefs, so a
                # cube registration over a signed catalog authenticates instead of walking a token-less handle.
                manifest = ManifestWrite(
                    cube=FieldVirtual(sources=sources, target=structs.replace(ref, credentials=self.signing.provider()), concat_dim=concat_dim)
                )
                # icechunk registration and commit block on store I/O — the banded thread hop, never the loop; `apply` is railed, so the hop carries the rail whole.
                outcome_rail = await on_thread(VirtualReference(sources=sources, ref=ref).apply, VersionOp(aggregate=(manifest, {}, None)))
                return outcome_rail.bind(
                    lambda outcome: (
                        self._rekey("cube", f"{concat_dim}|{'|'.join(hrefs)}".encode(), outcome.chunk_refs)
                        if isinstance(outcome, VirtualReceipt)
                        else Error(STAC_CUBE.raised())
                    )
                )
            case FoldTarget(tag="coverage", coverage=(groupby, resampling, chunks)):
                return (
                    await guarded(
                        RetryClass.HTTP, on_thread, lambda: self._coverage(groupby, resampling, chunks), abandon=True,
                        at=STAC_COVERAGE, on=Some(origin(self.discovery.endpoint)),
                    )
                ).bind(lambda rail: rail)
            case unreachable:
                assert_never(unreachable)

    def _coverage(self, groupby: str, resampling: Resampling, chunks: "dict[str, int] | None") -> "RuntimeRail[StacDiscovery]":
        # this extension read is a fallible ADMISSION, so the load runs only behind a bundle that proved its columns.
        return self._claim(next(iter(self.discovery.collection)), resampling).bind(
            lambda bundle: self._loaded(bundle, groupby, resampling, chunks)
        )

    def _loaded(self, bundle: ClaimBundle, groupby: str, resampling: Resampling, chunks: "dict[str, int] | None") -> "RuntimeRail[StacDiscovery]":
        cube = odc.stac.load(
            list(self.discovery.collection),
            stac_cfg=bundle.stac_cfg,
            patch_url=self.signing.patch_url(),
            groupby=groupby,
            resampling=resampling,
            chunks=chunks or {},
        )
        # preimage rows name the facts this coverage MEASURED. An unmeasured cloud fraction contributes no row
        # rather than the literal `None` the f-string folded into a content key, so a scene that reported no cloud
        # measure and a scene whose measure decoded to nothing can never share an identity.
        measured = Block.of_seq([
            Some(f"crs={bundle.crs}"),
            Some(f"sizes={tuple(cube.sizes.values())}"),
            bundle.cloud_cover.map(lambda fraction: f"cloud_cover={fraction}"),
        ])
        return self._rekey("coverage", "\n".join(measured.choose(lambda held: held)).encode(), bundle.band_count)

    def _rekey(self, tag: str, payload: bytes, folded: int) -> "RuntimeRail[StacDiscovery]":
        return ContentIdentity.of(f"stac.assets.{tag}", payload).map(
            lambda key: StacDiscovery(
                endpoint=self.discovery.endpoint,
                surface=self.discovery.surface,
                collection=self.discovery.collection,
                item_ids=self.discovery.item_ids,
                matched=self.discovery.matched,
                href_count=folded,
                expiry=self.discovery.expiry,
                url=self.discovery.url,
                content_key=key,
            )
        )

    @staticmethod
    def _claim(sample: object, resampling: Resampling) -> "RuntimeRail[ClaimBundle]":
        # ONE accumulating admission over the STAC extension surface. Each foreign read lands on its own carrier
        # first, then the columns a coverage cannot exist without sweep together, so a malformed item reports EVERY
        # missing one in a single refusal instead of one re-run per repair.
        projection, eo = sample.ext.proj, sample.ext.eo
        named = Block.of_seq(tuple(sample.assets.items()))
        first = named.try_head()
        spectral = Option.of_optional(eo.bands).map(tuple)
        # raster descriptors ride the FIRST asset, so an item carrying no assets carries no descriptors, and the
        # indexed read that used to sit here died on an `IndexError` inside the offloaded fold.
        descriptors = first.bind(lambda pair: Option.of_optional(pair[1].ext.raster.bands)).map(tuple)
        census = _census(spectral, descriptors)
        # this read answers the first DECLARED band fill, or nothing: `next(..., 0.0)` fabricated one for every
        # scene whose bands declare none, and `0.0` is a measurement — a real zero pixel — so that fabrication
        # masked live data.
        nodata = descriptors.bind(lambda bands: Block.of_seq(bands).choose(lambda band: Option.of_optional(band.nodata)).try_head()).map(float)
        # `proj:transform` is optional and a provider may carry a short or empty list; the affine is six
        # coefficients or it is not an affine, and the empty-tuple slot spelled "unknown" and "identity" alike.
        transform = (
            Option.of_optional(projection.transform)
            .map(lambda affine: tuple(float(value) for value in affine))
            .filter(lambda affine: len(affine) == 6)
        )
        code = Option.of_optional(projection.epsg).to_result_with(lambda: STAC_COLUMN.raised("proj:epsg"))
        holds = first.to_result_with(lambda: STAC_COLUMN.raised("item.assets"))
        counted = census.to_result_with(lambda: STAC_COLUMN.raised("eo:bands|raster:bands"))
        demanded: "Block[RuntimeRail[object]]" = Block.of_seq([code, holds, counted])
        return traversed(demanded, by=Disposition.ACCUMULATE).bind(
            lambda _verdict: code.map2(
                counted,
                lambda epsg, bands: ClaimBundle(
                    crs=f"EPSG:{epsg}",
                    band_count=bands.count,
                    band_source=bands.source,
                    nodata=nodata,
                    transform=transform,
                    cloud_cover=Option.of_optional(eo.cloud_cover).map(float),
                    stac_cfg=_stac_cfg(named, nodata),
                    # this sibling declares `transform` empty-when-unknown and `nodata` required, so the affine
                    # lowers to the owner's own absent form at this one seam and the claim mints only where a fill
                    # was measured — a fabricated fill is what a required slot with no absent form would demand.
                    claim=nodata.map(
                        lambda fill: RasterGeoClaim(
                            crs=f"EPSG:{epsg}",
                            band_count=bands.count,
                            resampling=resampling,
                            nodata=fill,
                            transform=transform.default_value(()),
                        )
                    ),
                ),
            )
        )
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
