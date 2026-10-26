"""HTTP exchanges inside one client scope."""

from collections.abc import Callable
from http import HTTPMethod
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import httpx2
import msgspec

from blender.records.fields import Positive, Presentation, Quantity
from blender.rendering.faults import RenderingResult, RenderingSendFault

# --- [MODELS] ---------------------------------------------------------------------------


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address with socket timeout."""

    url: Annotated[str, msgspec.Meta(title="URL", description="Server address with scheme")]
    timeout: Annotated[Positive, msgspec.Meta(description="Seconds per connect, read, and write"), Presentation(quantity=Some(Quantity.DURATION))] = 10.0

    def opened[T](self, operation: Callable[[httpx2.Client], RenderingResult[T]]) -> RenderingResult[T]:
        """Runs operation inside one client scope, the client closing on every path."""
        try:
            client = httpx2.Client(base_url=httpx2.URL(self.url), timeout=httpx2.Timeout(self.timeout))
        except (httpx2.InvalidURL, ValueError, OSError) as error:
            return Error((RenderingSendFault(request=self.url, error=error),))
        with client:
            return operation(client)


# --- [OPERATIONS] -----------------------------------------------------------------------


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
    except (OverflowError, httpx2.RequestError) as error:
        return Error((RenderingSendFault(request=url, error=error),))
    return Ok(response) if response.is_success else Error((RenderingSendFault(request=url, error=response.status_code),))


def exchange[T](client: httpx2.Client, method: HTTPMethod, path: str, reply: type[T], body: Option[msgspec.Struct | bytes] = Nothing) -> RenderingResult[T]:
    """Sends request and decodes JSON reply."""
    return sent(client, method, path, body).bind(
        lambda response: catch(lambda: msgspec.json.decode(response.content, type=reply), exception=msgspec.DecodeError)().map_error(
            lambda error: (RenderingSendFault(request=str(response.request.url), error=error),)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "exchange", "sent"]
