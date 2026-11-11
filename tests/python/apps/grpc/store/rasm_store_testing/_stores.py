"""Stores of the store service: the session a test process holds, the kinds it makes stores of, and each store's release by its test's outcome."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, closing, contextmanager, ExitStack, nullcontext
from datetime import datetime, timedelta, UTC
from enum import StrEnum
from functools import partial
from itertools import chain, count
import time
from types import GeneratorType
from typing import Protocol, Self
import uuid

from connectrpc.code import Code
from connectrpc.errors import ConnectError
from connectrpc.protocol import ProtocolType
from expression import Option
from expression.extra.result import catch
import msgspec
from pyqwest import HTTPVersion, SyncClient, SyncHTTPTransport
import pytest
from rasm.store.store_connect import StoreServiceClientSync
from rasm.store.store_pb import BucketStore, CacheStore, ClickhouseStore, PostgresStore, SessionRequest, StoreRequest, TemplateRequest
lazy import clickhouse_connect
lazy from clickhouse_connect.driver import Client
lazy from obstore.store import S3Credential, S3Store
lazy from psycopg_pool import NullConnectionPool
lazy from redis import Redis

from rasm_records import Error, Fault, Faults, Nothing, Outcome, typed
lazy from rasm_testing import settled

# --- [TYPES] ----------------------------------------------------------------------------

type Postgres = Store[PostgresStore, NullConnectionPool]
type Bucket = Store[BucketStore, S3Store]
type Clickhouse = Store[ClickhouseStore, Client]
type Cache = Store[CacheStore, Redis]


class Record(Protocol):
    """Store message of any kind, named by its service id."""

    @property
    def id(self) -> str:
        """Service id of the store."""
        ...


class Rpc[Q, R](Protocol):
    """Generated client call taking one request message and a deadline."""

    @property
    def __name__(self) -> str:
        """Method name of the call."""
        ...

    def __call__(self, client: StoreServiceClientSync, request: Q, /, *, timeout_ms: int | None = None) -> R:
        """Sends the request and returns the service's reply."""
        ...


class Stores(Protocol):
    """Maker of the stores one test holds."""

    def __call__[R: Record, C](self, source: Kind[R, C] | Template[R, C], /) -> Store[R, C]:
        """Returns a new store of a kind, or a clone of a template, released when the test ends."""
        ...


class Refusal(StrEnum):
    """Store refusals a test report names, each value the report template of its fault."""

    SILENT = "store service at {subject} granted no session"
    REFUSED = "store service refused {subject}"
    LEAKED = "store {subject} holds a connection its test left open"


# --- [MODELS] ---------------------------------------------------------------------------


class Kind[R: Record, C](msgspec.Struct, frozen=True, kw_only=True):
    """Store kind of the service: the call creating a store and the driver opened on its record within a session."""

    created: Rpc[SessionRequest, R]
    opened: Callable[[Lease, R], AbstractContextManager[C]]


class Templated[R: Record, C](Kind[R, C], frozen=True, kw_only=True):
    """Store kind holding templates: the call opening the build of a digest and the call cloning its sealed template."""

    opening: Rpc[TemplateRequest, R]
    cloning: Rpc[TemplateRequest, R]


class Template[R: Record, C](msgspec.Struct, frozen=True, kw_only=True):
    """Schema a store starts from: its kind, the digest naming it on the service, and the build filling the first store of that digest."""

    kind: Templated[R, C]
    digest: str
    build: Callable[[C], object]


class Store[R, C](msgspec.Struct, frozen=True, kw_only=True):
    """Isolated store of one test: the service's record of it and the catalog driver opened on it."""

    record: R
    client: C


# --- [SERVICES] -------------------------------------------------------------------------


class Lease(msgspec.Struct, frozen=True, kw_only=True):
    """Session of one test process on the store service, held while its stream stays open, each store acquisition and release bounded by the time its test holds under `limit`."""

    client: StoreServiceClientSync
    session: str
    limit: pytest.StashKey[timedelta]
    failed: pytest.StashKey[bool]

    # --- [SESSION]

    @classmethod
    @contextmanager
    def granted(cls, address: str, limit: pytest.StashKey[timedelta], failed: pytest.StashKey[bool], /) -> Generator[Outcome[Self]]:
        """Holds a new session on the service at an address until the scope ends.

        Args:
            address: Service URL, scheme included.
            limit: Key under which a test holds the time its runner allows it.
            failed: Key under which a test holds whether its setup or call failed.

        Yields:
            The session once the service sent its first lease, or a `SILENT` fault naming the address, the service's refusal its cause where the hold ended with one.
        """
        with (
            SyncHTTPTransport(http_version=HTTPVersion.HTTP2) as transport,
            StoreServiceClientSync(address, protocol=ProtocolType.GRPC, send_compression=None, http_client=SyncClient(transport)) as client,
            ExitStack() as stack,
        ):
            lease = cls(client=client, session=str(uuid.uuid7()), limit=limit, failed=failed)
            held = typed(GeneratorType, StoreServiceClientSync.hold_session.__name__, client.hold_session(SessionRequest(session=lease.session))).map(lambda hold: stack.enter_context(closing(hold)))
            silent = partial(Fault.of, Refusal.SILENT, address)
            yield held.bind(lambda hold: catch(next, exception=ConnectError)(hold, None).map_error(silent)).bind(lambda first: Option.of_optional(first).to_result_with(silent)).map(lambda _: lease)

    def ended(self) -> Outcome[object]:
        """Ends the session, which drops each store no test kept."""
        return self.called(StoreServiceClientSync.end_session, SessionRequest(session=self.session))

    def ending(self, node: pytest.Item, /) -> Option[float]:
        """Monotonic instant a call a test starts now ends by, absent where its runner states no limit."""
        return Option.of_optional(node.stash.get(self.limit, None)).map(lambda limit: time.monotonic() + limit / timedelta(seconds=1))

    def called[Q, R](self, rpc: Rpc[Q, R], request: Q, /, *, end: Option[float] = Nothing) -> Outcome[R]:
        """Returns the reply of one service call by `end`, or a `REFUSED` fault naming the call with its `ConnectError` as cause."""
        timeout = end.map(lambda at: timedelta(seconds=at - time.monotonic()) // timedelta(milliseconds=1)).to_optional()
        return catch(rpc, exception=ConnectError)(self.client, request, timeout_ms=timeout).map_error(partial(Fault.of, Refusal.REFUSED, rpc.__name__))

    # --- [STORES]

    def held[R: Record, C](self, stack: ExitStack, node: pytest.Item, source: Kind[R, C] | Template[R, C], /) -> Store[R, C]:
        """Returns a store with its driver open inside a test's scope, which closes the driver and then drops the store of a passed test or keeps the store of a failed one.

        Args:
            stack: Scope of the test.
            node: Test whose outcome decides the release.
            source: Kind of a new store, or template the store starts as a clone of.

        Returns:
            The store the service created or cloned.
        """
        end = self.ending(node)
        kind, acquired = (source.kind, self.cloned(source, end)) if isinstance(source, Template) else (source, self.called(source.created, SessionRequest(session=self.session), end=end))
        record = settled(acquired)
        stack.callback(self._released, node, record)
        return Store(record=record, client=stack.enter_context(kind.opened(self, record)))

    def cloned[R: Record, C](self, template: Template[R, C], end: Option[float], /) -> Outcome[R]:
        """Returns a clone of a template, building and sealing it first where no session holds its digest, all by `end`.

        A session finding the digest held waits on the service for that build, and opens the next build where its builder abandoned it.
        """
        request = TemplateRequest(session=self.session, digest=template.digest)
        copy = partial(self.called, template.kind.cloning, request, end=end)
        attempts = (copy().or_else_with(partial(_answered, Code.NOT_FOUND, partial(self._opened, template, request, end, copy))) for _ in count())
        return next(outcome for outcome in attempts if outcome.is_ok() or not _refused(Code.NOT_FOUND, outcome.error))

    def _opened[R: Record, C](self, template: Template[R, C], request: TemplateRequest, end: Option[float], copy: Callable[[], Outcome[R]], /) -> Outcome[R]:
        built = self.called(template.kind.opening, request, end=end).bind(partial(self._built, template, end))
        return built.bind(lambda _: copy()).or_else_with(partial(_answered, Code.ALREADY_EXISTS, copy))

    def _built[R: Record, C](self, template: Template[R, C], end: Option[float], build: R, /) -> Outcome[object]:
        with ExitStack() as abandoned:
            abandoned.callback(self.called, StoreServiceClientSync.drop_store, StoreRequest(id=build.id), end=end)
            with template.kind.opened(self, build) as client:
                template.build(client)
            abandoned.pop_all()
        return self.called(StoreServiceClientSync.seal_template, StoreRequest(id=build.id), end=end)

    def _released(self, node: pytest.Item, record: Record, /) -> None:
        request, end = StoreRequest(id=record.id), self.ending(node)
        if node.stash.get(self.failed, False):
            settled(self.called(StoreServiceClientSync.keep_store, request, end=end))
            node.add_report_section("teardown", "kept store", str(record))
        else:
            settled(self.called(StoreServiceClientSync.drop_store, request, end=end).or_else_with(partial(_answered, Code.FAILED_PRECONDITION, lambda: Error(Fault.of(Refusal.LEAKED, record.id)))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _refused(code: Code, faults: Faults, /) -> bool:
    match faults:
        case (Fault(cause=ConnectError(code=refused)),):
            return refused is code
        case _:
            return False


def _answered[T](code: Code, answer: Callable[[], Outcome[T]], faults: Faults, /) -> Outcome[T]:
    return answer() if _refused(code, faults) else Error(faults)


def _credential(record: BucketStore, /) -> S3Credential:
    return {
        "access_key_id": record.access_key_id,
        "secret_access_key": record.secret_access_key,
        "token": record.session_token,
        "expires_at": datetime.now(UTC) + timedelta(seconds=record.credentials_ttl_seconds),
    }


def _bucket(lease: Lease, record: BucketStore, /) -> AbstractContextManager[S3Store]:
    renewed = iter(partial(lease.client.renew_bucket, StoreRequest(id=record.id)), None)
    provider = partial(next, map(_credential, chain((record,), renewed)))
    return nullcontext(S3Store(record.bucket, endpoint=record.endpoint, region=record.region, client_options={"allow_http": True}, credential_provider=provider))


# --- [KINDS] ----------------------------------------------------------------------------

POSTGRES: Templated[PostgresStore, NullConnectionPool] = Templated(
    created=StoreServiceClientSync.create_postgres,
    opened=lambda _, record: NullConnectionPool(
        kwargs={"host": record.host, "port": record.port, "dbname": record.database, "user": record.username, "password": record.password}, max_size=record.connection_limit, open=False
    ),
    opening=StoreServiceClientSync.open_postgres_template,
    cloning=StoreServiceClientSync.clone_postgres_template,
)
BUCKET: Kind[BucketStore, S3Store] = Kind(created=StoreServiceClientSync.create_bucket, opened=_bucket)
CLICKHOUSE: Templated[ClickhouseStore, Client] = Templated(
    created=StoreServiceClientSync.create_clickhouse,
    opened=lambda _, record: closing(clickhouse_connect.get_client(host=record.host, port=record.http_port, database=record.database, username=record.username, password=record.password)),
    opening=StoreServiceClientSync.open_clickhouse_template,
    cloning=StoreServiceClientSync.clone_clickhouse_template,
)
CACHE: Kind[CacheStore, Redis] = Kind(
    created=StoreServiceClientSync.create_cache, opened=lambda _, record: closing(Redis(host=record.host, port=record.port, db=record.database, username=record.username, password=record.password))
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BUCKET", "CACHE", "CLICKHOUSE", "POSTGRES", "Bucket", "Cache", "Clickhouse", "Kind", "Lease", "Postgres", "Refusal", "Store", "Stores", "Template", "Templated"]
