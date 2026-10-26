"""HTTP exchanges inside one client scope."""

from collections.abc import Callable
from http import HTTPMethod
from typing import Annotated

from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import httpx2
import msgspec

from blender.records.faults import Fault, Faults, Outcome
from blender.records.fields import Positive
from blender.rendering.faults import RenderingReason

# --- [MODELS] ---------------------------------------------------------------------------


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address with socket timeout."""

    url: Annotated[str, msgspec.Meta(description="Server address with scheme")]
    timeout: Annotated[Positive, msgspec.Meta(description="Seconds per connect, read, and write")] = 10.0

    def opened[T](self, operation: Callable[[httpx2.Client], Outcome[T]]) -> Outcome[T]:
        """Runs operation inside one client scope, the client closing on every path."""

        def refused(error: httpx2.InvalidURL) -> Faults:
            return (Fault(reason=RenderingReason.EXCHANGE, subject=self.url, cause=error),)

        def held(url: httpx2.URL) -> Outcome[T]:
            with httpx2.Client(base_url=url, timeout=httpx2.Timeout(self.timeout)) as client:
                return operation(client)

        return catch(httpx2.URL, exception=httpx2.InvalidURL)(self.url).map_error(refused).bind(held)


# --- [OPERATIONS] -----------------------------------------------------------------------


def sent(client: httpx2.Client, method: HTTPMethod, path: str, body: Option[msgspec.Struct | bytes] = Nothing) -> Outcome[httpx2.Response]:
    """Sends request, non-success status as fault."""
    match body.to_optional():
        case msgspec.Struct() as content:
            request = client.build_request(method, path, content=msgspec.json.encode(content), headers={"Content-Type": "application/json"})
        case bytes(content):
            request = client.build_request(method, path, content=content, headers={"Content-Type": "application/octet-stream"})
        case None:
            request = client.build_request(method, path)
    url = str(request.url)

    def refused(error: httpx2.RequestError) -> Faults:
        return (Fault(reason=RenderingReason.EXCHANGE, subject=url, cause=error),)

    return (
        catch(client.send, exception=httpx2.RequestError)(request)
        .map_error(refused)
        .bind(lambda response: Ok(response) if response.is_success else Error((Fault(reason=RenderingReason.STATUS, subject=url, member=str(response.status_code)),)))
    )


def exchange[T](client: httpx2.Client, method: HTTPMethod, path: str, reply: type[T], body: Option[msgspec.Struct | bytes] = Nothing) -> Outcome[T]:
    """Sends request and decodes JSON reply."""
    return sent(client, method, path, body).bind(
        lambda response: catch(lambda: msgspec.json.decode(response.content, type=reply), exception=msgspec.DecodeError)().map_error(
            lambda error: (Fault(reason=RenderingReason.EXCHANGE, subject=str(response.request.url), cause=error),)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "exchange", "sent"]
