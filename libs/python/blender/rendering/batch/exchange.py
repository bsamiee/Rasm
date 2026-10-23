"""HTTP exchanges inside one client scope."""

from collections.abc import Callable
from http import HTTPMethod
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import httpx2
import msgspec

from blender.imaging.faults import scoped
from blender.imaging.parameters import Positive, Presentation, Quantity
from blender.rendering.faults import Cause, RenderingResult, RenderingSendFault

# --- [MODELS] ---------------------------------------------------------------------------


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address with socket timeout."""

    url: Annotated[str, msgspec.Meta(title="URL", description="Server address with scheme")]
    timeout: Annotated[Positive, msgspec.Meta(description="Seconds per connect, read, and write"), Presentation(quantity=Some(Quantity.DURATION))] = 10.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def opened[T](endpoint: Endpoint, operation: Callable[[httpx2.Client], RenderingResult[T]]) -> RenderingResult[T]:
    """Runs operation inside one client scope."""
    try:
        client = httpx2.Client(base_url=httpx2.URL(endpoint.url), timeout=httpx2.Timeout(endpoint.timeout))
    except (httpx2.InvalidURL, ValueError):
        return Error((RenderingSendFault(request=endpoint.url, error=Cause.INVALID_URL),))
    except OSError:
        return Error((RenderingSendFault(request=endpoint.url, error=Cause.BROKEN),))

    def released(held: httpx2.Client) -> RenderingResult[None]:
        try:
            held.close()
        except (httpx2.RequestError, OSError):
            return Error((RenderingSendFault(request=endpoint.url, error=Cause.BROKEN),))
        return Ok(None)

    return scoped(Ok(client), operation, released)


def sent(client: httpx2.Client, method: HTTPMethod, path: str, body: Option[msgspec.Struct | bytes] = Nothing) -> RenderingResult[httpx2.Response]:
    """Sends request, non-success status as fault."""
    match body.to_optional():
        case msgspec.Struct() as content:
            request = client.build_request(method, path, content=msgspec.json.encode(content), headers={"Content-Type": "application/json"})
        case bytes(content):
            request = client.build_request(method, path, content=content, headers={"Content-Type": "application/octet-stream"})
        case None:
            request = client.build_request(method, path)
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
    return Ok(response) if response.is_success else Error((RenderingSendFault(request=url, error=response.status_code),))


def exchange[T](client: httpx2.Client, method: HTTPMethod, path: str, reply: type[T], body: Option[msgspec.Struct | bytes] = Nothing) -> RenderingResult[T]:
    """Sends request and decodes JSON reply."""
    return sent(client, method, path, body).bind(
        lambda response: catch(lambda: msgspec.json.decode(response.content, type=reply), exception=msgspec.DecodeError)().map_error(
            lambda _: (RenderingSendFault(request=str(response.request.url), error=Cause.UNREADABLE),)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "exchange", "opened", "sent"]
