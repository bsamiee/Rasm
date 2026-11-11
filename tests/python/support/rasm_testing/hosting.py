"""pytest plugin of the `hosting` extra: an event loop thread and the HTTP servers a test hosts on it, each on a loopback port answering from a function of the test."""

from collections.abc import Callable, Iterator, Mapping, Sequence
from contextlib import ExitStack
from datetime import timedelta
import functools
import socket

import anyio
from anyio.from_thread import BlockingPortal, start_blocking_portal
from hypercorn.asyncio import serve
from hypercorn.config import Config
from hypercorn.typing import ASGIReceiveCallable, ASGISendCallable, Scope
import msgspec
import pytest

from rasm_testing._outcomes import reaching

# --- [TYPES] ----------------------------------------------------------------------------

type Hosted = Callable[[Callable[[Received], Answer]], tuple[str, Sequence[Received]]]

# --- [MODELS] ---------------------------------------------------------------------------


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
    headers: Mapping[str, str] = {}
    body: bytes = b""
    delay: timedelta = timedelta()


# --- [OPERATIONS] -----------------------------------------------------------------------


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
                case {"type": "http.request", "body": bytes(chunk), "more_body": bool(more)}:
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
    served = portal.start_task_soon(functools.partial(serve, app, config, shutdown_trigger=stopped.wait))
    stack.callback(served.result)
    stack.callback(portal.call, stopped.set)
    return f"http://{host}:{port}", received


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def portal() -> Iterator[BlockingPortal]:
    """Yields an event loop running on its own thread until the test ends, where the test hosts the servers it reaches over loopback."""
    with start_blocking_portal() as running:
        yield running


@pytest.fixture
def hosted(portal: BlockingPortal) -> Iterator[Hosted]:
    """Yields the start of an HTTP server answering from a function, which returns the server's base URL and the requests it received, each server stopped after the test."""
    with ExitStack() as stack:
        yield functools.partial(hosting, stack, portal)


pytest_itemcollected = reaching(portal)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Answer", "Hosted", "Received", "hosting"]
