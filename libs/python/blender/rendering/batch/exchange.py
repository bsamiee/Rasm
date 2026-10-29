"""HTTP exchanges inside one client scope."""

from collections.abc import Callable
import datetime
from functools import partial
from http import HTTPMethod

from expression import Nothing, Option
from expression.extra.result import catch
import httpx2
import msgspec

from blender.records.faults import Fault, Outcome
from blender.rendering.faults import RenderingReason

# --- [MODELS] ---------------------------------------------------------------------------


class Endpoint(msgspec.Struct, frozen=True, kw_only=True):
    """Server address with socket timeout."""

    url: str
    timeout: datetime.timedelta = datetime.timedelta(seconds=10)

    def __post_init__(self) -> None:
        """Validates a timeout above zero."""
        if self.timeout <= datetime.timedelta():
            raise ValueError(self.timeout)

    def opened[T](self, operation: Callable[[httpx2.Client], Outcome[T]]) -> Outcome[T]:
        """Runs operation inside one client scope, the client closing on every path."""

        def held(url: httpx2.URL) -> Outcome[T]:
            with httpx2.Client(base_url=url, timeout=self.timeout.total_seconds()) as client:
                return operation(client)

        return catch(httpx2.URL, exception=httpx2.InvalidURL)(self.url).map_error(partial(Fault.of, RenderingReason.EXCHANGE, self.url)).bind(held)


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
    return (
        catch(client.send, exception=httpx2.RequestError)(request)
        .map_error(partial(Fault.of, RenderingReason.EXCHANGE, url))
        .filter_with(lambda response: response.is_success, lambda response: Fault.of(RenderingReason.STATUS, url, member=str(response.status_code)))
    )


def exchange[T](client: httpx2.Client, method: HTTPMethod, path: str, reply: type[T], body: Option[msgspec.Struct | bytes] = Nothing) -> Outcome[T]:
    """Sends request and decodes JSON reply."""
    return sent(client, method, path, body).bind(
        lambda response: catch(msgspec.json.decode, exception=msgspec.DecodeError)(response.content, type=reply).map_error(partial(Fault.of, RenderingReason.EXCHANGE, str(response.request.url)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Endpoint", "exchange", "sent"]
