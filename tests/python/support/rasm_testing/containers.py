"""pytest plugin of the `containers` extra: Testcontainers servers started for one test or once for a test process, the RustFS module testcontainers lacks, a server row of a Compose file, and the catalog driver opened on each server."""

from collections.abc import Callable, Iterator, Mapping, Sequence
from contextlib import AbstractContextManager, closing, ExitStack, nullcontext
import functools
from pathlib import Path
import subprocess
from typing import overload, Protocol
import uuid

import boto3
import clickhouse_connect
from clickhouse_connect.driver import Client, default_port
from expression import Option
from expression.extra.result import catch
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
from rasm_testing._outcomes import FAILED, reaching

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
    model = catch(msgspec.json.decode, exception=msgspec.DecodeError)(listed.stdout, type=_Composed).map_error(functools.partial(Fault.of, Reason.INVALID, str(file)))
    row = model.bind(lambda composed: Option.of_optional(composed.services.get(service)).to_result_with(functools.partial(Fault.of, Reason.ABSENT, str(file), member=f"service {service}")))
    return row.bind(lambda raw: catch(msgspec.json.decode, exception=msgspec.DecodeError)(raw, type=Defined).map_error(functools.partial(Fault.of, Reason.INVALID, service)))


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
            endpoint, bucket = f"http://{host}:{server.get_exposed_port(port)}", uuid.uuid4().hex
            boto3.client("s3", endpoint_url=endpoint, aws_access_key_id=access_key, aws_secret_access_key=secret_key, region_name=region).create_bucket(Bucket=bucket)
            return nullcontext(S3Store(bucket, endpoint=endpoint, region=region, access_key_id=access_key, secret_access_key=secret_key, client_options={"allow_http": True}))
        case _:
            raise TypeError(type(server))


# --- [COMPOSITION] ----------------------------------------------------------------------

pytest_plugins = ("rasm_testing.plugin",)


@pytest.fixture(scope="session")
def shared() -> Iterator[Started]:
    """Yields the start of a server the test process shares, stopped after the process's last test."""
    with ExitStack() as stack:
        yield functools.partial(serving, stack, ())


@pytest.fixture
def started(request: pytest.FixtureRequest) -> Iterator[Started]:
    """Yields the start of a server the test controls, stopped after the test with its logs in the report of a failed one."""
    with ExitStack() as stack:
        yield functools.partial(serving, stack, (functools.partial(logged, request.node, FAILED),))


pytest_itemcollected = reaching(shared, started)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Defined", "RustfsContainer", "Started", "defined", "logged", "opened", "serving"]
