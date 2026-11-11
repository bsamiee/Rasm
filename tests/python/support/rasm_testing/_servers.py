"""Servers a test controls: Testcontainers containers started for one test or once for a test process, the RustFS module testcontainers lacks, the catalog driver opened on each server, and an HTTP server on a loopback port answering from a function of the test."""

from collections.abc import Callable, Mapping, Sequence
from contextlib import AbstractContextManager, closing, ExitStack, nullcontext
from datetime import timedelta
from functools import partial
from pathlib import Path
import socket
import subprocess
from typing import overload, Protocol
import uuid

import anyio
from anyio.from_thread import BlockingPortal
import boto3
import clickhouse_connect
from clickhouse_connect.driver import Client, default_port
from expression import Option
from expression.extra.result import catch
from hypercorn.asyncio import serve
from hypercorn.config import Config
from hypercorn.typing import ASGIReceiveCallable, ASGISendCallable, Scope
import msgspec
from obstore.store import S3Store
from psycopg_pool import ConnectionPool
import pytest
from redis import Redis
from testcontainers.community.clickhouse import ClickHouseContainer
from testcontainers.community.postgres import PostgresContainer
from testcontainers.community.valkey import ValkeyContainer
from testcontainers.core.container import DockerContainer
from testcontainers.core.wait_strategies import ExecWaitStrategy

from rasm_records import Error, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Started(Protocol):
    """Start of a server inside the scope of a test or of its process."""

    def __call__[S: DockerContainer](self, server: S, /) -> S:
        """Starts a server and returns it, stopped when the scope ends."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Defined(msgspec.Struct, frozen=True, kw_only=True):
    """Image and command of one Compose service as Compose resolves them, the command absent where the image's own runs."""

    image: str
    command: tuple[str, ...] | None = None


class _Composed(msgspec.Struct, frozen=True, kw_only=True):
    """Compose model `docker compose config` prints, each service undecoded until one is read."""

    services: Mapping[str, msgspec.Raw]


class Received(msgspec.Struct, frozen=True, kw_only=True):
    """Request a hosted server received: method, path, headers under lower-case names, whole body, and the event loop clock when the body ended."""

    method: str
    path: str
    headers: Mapping[str, str]
    body: bytes
    at: timedelta


class Answer(msgspec.Struct, frozen=True, kw_only=True):
    """Response a hosted server sends once `delay` has passed."""

    status: int = 200
    headers: Mapping[str, str] = frozendict()
    body: bytes = b""
    delay: timedelta = timedelta()


# --- [SERVICES] -------------------------------------------------------------------------


class RustfsContainer(DockerContainer):
    """RustFS S3 server, ready once its `/health/ready` answers the image's own `curl` inside the container."""

    def __init__(self, image: str, /, *, access_key: str, secret_key: str, port: int = 9000, region: str = "us-east-1") -> None:
        """Declares the server.

        Args:
            image: RustFS image.
            access_key: Access key of the root user.
            secret_key: Secret key of the root user.
            port: Port the server listens on inside its container.
            region: Region requests are signed for.
        """
        super().__init__(image, env={"RUSTFS_ACCESS_KEY": access_key, "RUSTFS_SECRET_KEY": secret_key, "RUSTFS_REGION": region}, ports=[port])
        self.waiting_for(ExecWaitStrategy(["curl", "--fail", "--silent", f"http://127.0.0.1:{port}/health/ready"]))
        self.port, self.access_key, self.secret_key, self.region = port, access_key, secret_key, region


# --- [OPERATIONS] -----------------------------------------------------------------------


def defined(file: Path, service: str, /) -> Outcome[Defined]:
    """Returns the image and command of one service of a Compose file as `docker compose config` resolves them with every profile on.

    Returns:
        The service's row, or a fault naming the file Compose refuses or the service its model holds without an image.
    """
    try:
        listed = subprocess.run(("docker", "compose", "--file", str(file), "--profile", "*", "config", "--format", "json"), check=True, capture_output=True)
    except (OSError, subprocess.CalledProcessError) as error:
        return Error(Fault.of(Reason.FAILED, str(file), error))
    model = catch(msgspec.json.decode, exception=msgspec.DecodeError)(listed.stdout, type=_Composed).map_error(partial(Fault.of, Reason.INVALID, str(file)))
    row = model.bind(lambda composed: Option.of_optional(composed.services.get(service)).to_result_with(partial(Fault.of, Reason.ABSENT, str(file), member=f"service {service}")))
    return row.bind(lambda raw: catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=Defined).map_error(partial(Fault.of, Reason.INVALID, service)))


def serving[S: DockerContainer](stack: ExitStack, reports: Sequence[Callable[[DockerContainer], object]], server: S, /) -> S:
    """Starts a server inside an exit stack, which hands the server to each report and then stops it when its scope ends."""
    stack.enter_context(server)
    for report in reports:
        stack.callback(report, server)
    return server


def logged(node: pytest.Item, failed: pytest.StashKey[bool], server: DockerContainer, /) -> None:
    """Adds a server's logs to the report of a failed test."""
    if node.stash.get(failed, False):
        node.add_report_section("teardown", server.image, b"".join(server.get_logs()).decode(errors="replace"))


def hosting(stack: ExitStack, portal: BlockingPortal, answering: Callable[[Received], Answer], /) -> tuple[str, Sequence[Received]]:
    """Serves HTTP/1.1 and h2c on a loopback port the system picks, answering each request from `answering` on the portal's event loop, until the exit stack's scope ends.

    Returns:
        The server's base URL and the requests it received so far, in arrival order.
    """
    received: list[Received] = []

    async def app(scope: Scope, receive: ASGIReceiveCallable, send: ASGISendCallable) -> None:
        if scope["type"] != "http":
            return
        body, more = b"", True
        while more:
            match await receive():
                case {"type": "http.request", "body": bytes() as chunk, "more_body": bool() as more}:
                    body += chunk
                case _:
                    more = False
        request = Received(
            method=scope["method"], path=scope["path"], headers={name.decode(): value.decode() for name, value in scope["headers"]}, body=body, at=timedelta(seconds=anyio.current_time())
        )
        received.append(request)
        answer = answering(request)
        await anyio.sleep(answer.delay / timedelta(seconds=1))
        await send({"type": "http.response.start", "status": answer.status, "headers": [(name.encode(), value.encode()) for name, value in answer.headers.items()]})
        await send({"type": "http.response.body", "body": answer.body, "more_body": False})

    listener = socket.create_server(("127.0.0.1", 0))
    host, port = listener.getsockname()
    config = Config()
    config.bind, config.graceful_timeout = [f"fd://{listener.detach()}"], 0
    stopped = portal.call(anyio.Event)
    served = portal.start_task_soon(partial(serve, app, config, shutdown_trigger=stopped.wait))
    stack.callback(served.result)
    stack.callback(portal.call, stopped.set)
    return f"http://{host}:{port}", received


@overload
def opened(server: PostgresContainer, /) -> AbstractContextManager[ConnectionPool]: ...
@overload
def opened(server: ClickHouseContainer, /) -> AbstractContextManager[Client]: ...
@overload
def opened(server: ValkeyContainer, /) -> AbstractContextManager[Redis]: ...
@overload
def opened(server: RustfsContainer, /) -> AbstractContextManager[S3Store]: ...
def opened(server: DockerContainer, /) -> AbstractContextManager[object]:
    """Opens the catalog driver of a started server at its current address.

    Returns:
        A scope yielding a connection pool, a ClickHouse client, a Valkey client, or an object store on a new bucket of a RustFS server.

    Raises:
        TypeError: If no catalog driver serves the server's class.
    """
    host = server.get_container_host_ip()
    match server:
        case PostgresContainer():
            return ConnectionPool(server.get_connection_url(driver=None), open=False)
        case ClickHouseContainer(username=username, password=password, dbname=database):
            return closing(clickhouse_connect.get_client(host=host, port=server.get_exposed_port(default_port("http", secure=False)), username=username, password=password, database=database))
        case ValkeyContainer(password=password):
            return closing(Redis(host=host, port=server.get_exposed_port(), password=password))
        case RustfsContainer(port=port, access_key=access_key, secret_key=secret_key, region=region):
            endpoint, bucket = f"http://{host}:{server.get_exposed_port(port)}", uuid.uuid7().hex
            boto3.client("s3", endpoint_url=endpoint, aws_access_key_id=access_key, aws_secret_access_key=secret_key, region_name=region).create_bucket(Bucket=bucket)
            return nullcontext(S3Store(bucket, endpoint=endpoint, region=region, access_key_id=access_key, secret_access_key=secret_key, client_options={"allow_http": True}))
        case _:
            raise TypeError(type(server))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Answer", "Defined", "Received", "RustfsContainer", "Started", "defined", "hosting", "logged", "opened", "serving"]
