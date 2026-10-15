"""HTTP exchanges with a server endpoint inside one client scope."""

from collections.abc import Callable
from http import HTTPMethod
import sys
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import httpx2
import msgspec

from blender.imaging.faults import paired
from blender.imaging.parameters import Presentation, Quantity
from blender.rendering.faults import Cause, RenderingResult, RenderingSendFault

# --- [MODELS] ---------------------------------------------------------------------------


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address and the seconds each socket wait allows."""

    url: Annotated[str, msgspec.Meta(title="URL", description="Server address with its scheme")]
    timeout: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Seconds each connect, read, and write waits"), Presentation(quantity=Some(Quantity.DURATION))] = 10.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def opened[T](endpoint: Endpoint, operation: Callable[[httpx2.Client], RenderingResult[T]]) -> RenderingResult[T]:
    """Runs an operation inside one client scope over an endpoint."""
    try:
        client = httpx2.Client(base_url=httpx2.URL(endpoint.url), timeout=httpx2.Timeout(endpoint.timeout))
    except (httpx2.InvalidURL, ValueError):
        return Error((RenderingSendFault(request=endpoint.url, error=Cause.INVALID_URL),))
    except OSError:
        return Error((RenderingSendFault(request=endpoint.url, error=Cause.BROKEN),))
    try:
        answer = operation(client)
    finally:
        try:
            client.close()
            released: RenderingResult[None] = Ok(None)
        except (httpx2.RequestError, OSError):
            released = Error((RenderingSendFault(request=endpoint.url, error=Cause.BROKEN),))
    return paired(answer, released).bind(lambda _: answer)


def requested(client: httpx2.Client, method: HTTPMethod, path: str, body: Option[msgspec.Struct | bytes] = Nothing) -> httpx2.Request:
    """Builds a request on the client's base URL."""
    match body.to_optional():
        case msgspec.Struct() as content:
            return client.build_request(method, path, content=msgspec.json.encode(content), headers={"Content-Type": "application/json"})
        case bytes(content):
            return client.build_request(method, path, content=content, headers={"Content-Type": "application/octet-stream"})
        case None:
            return client.build_request(method, path)


def sent(client: httpx2.Client, request: httpx2.Request) -> RenderingResult[bytes]:
    """Sends a request and reads its reply."""
    url = str(request.url)
    try:
        response = client.send(request)
    except OverflowError as error:
        return Error((RenderingSendFault(request=url, error=error),))
    except httpx2.RequestError as error:
        return Error((
            RenderingSendFault(
                request=url, error=Cause.TIMED_OUT if isinstance(error, httpx2.TimeoutException) else Cause.UNREACHABLE if isinstance(error, httpx2.ConnectError | httpx2.ProxyError) else Cause.BROKEN
            ),
        ))
    return Ok(response.content) if response.is_success else Error((RenderingSendFault(request=url, error=response.status_code),))


def exchange[T](client: httpx2.Client, request: httpx2.Request, reply: type[T]) -> RenderingResult[T]:
    """Sends a request and decodes its JSON reply."""
    return sent(client, request).bind(
        lambda content: catch(lambda: msgspec.json.decode(content, type=reply), exception=msgspec.DecodeError)().map_error(lambda _: (RenderingSendFault(request=request.url, error=Cause.UNREADABLE),))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "exchange", "opened", "requested", "sent"]
