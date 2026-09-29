"""Applies the interface to Illustrator, Photoshop, InDesign, and Acrobat through the `adobe` MCP server, one write launch per scripted product."""

from collections.abc import Mapping, Sequence
from datetime import timedelta
from pathlib import Path

import anyio
from mcp import ClientSession
from mcp.client.stdio import stdio_client, StdioServerParameters
from mcp.types import TextContent
import msgspec
import psutil

from interface.adobe import stores
from interface.adobe.rows import Filed, Product, Row, rows, Shared
from interface.host import Applied, Bundle, bundle, DEADLINE, Failed, gather, Host, launch, outcome, parse, quitted, running, terminated
from interface.report import ABSENT, Kind, line

# --- [MODELS] ---------------------------------------------------------------------------


class Installed(msgspec.Struct, frozen=True, rename={"processes": "processIdentifiers"}):
    """Installed Adobe application as the `applications` tool reports it."""

    identifier: str
    processes: tuple[int, ...]
    scriptable: bool
    url: str


class Discovery(msgspec.Struct, frozen=True):
    """Result of the `applications` tool."""

    applications: tuple[Installed, ...]


class Item(msgspec.Struct, frozen=True):
    """Text item of an Apple event reply."""

    text: str


class Reply(msgspec.Struct, frozen=True):
    """Apple event reply items with the keyword of each."""

    items: tuple[Item, ...]
    keywords: tuple[int, ...]


class Executed(msgspec.Struct, frozen=True):
    """Result of the `execute` tool for a command the application answered."""

    reply: Reply


class Prepared(msgspec.Struct, frozen=True):
    """Product quit with its files written and the file stage's report lines."""

    installed: Installed
    folders: stores.Folders
    filed: tuple[str, ...]


class Request(msgspec.Struct, frozen=True):
    """Input `script.jsx` reads, its settings folder the one the header reports where the product's API names none, with the report's line kinds and its spelling of a value the store lacks."""

    application: str
    rows: tuple[Row, ...]
    settings: str | None
    kinds: Mapping[str, str] = msgspec.field(default_factory=lambda: {kind.name.lower(): kind.value for kind in Kind})
    absent: str = ABSENT


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SERVER]
def chosen(product: Product, discovered: Sequence[Installed]) -> Installed | Failed:
    """One scriptable installed application with the product's bundle id."""
    match [each for each in discovered if each.scriptable and each.identifier == product.identifier]:
        case [installed]:
            return installed
        case matches:
            return Failed(product.name.lower(), (f"{len(matches)} installed applications declare the bundle id {product.identifier} and the run needs one",))


async def execute(session: ClientSession, product: Product, prepared: Prepared, source: str, request: Request) -> str | Failed:
    """Lines `script.jsx` returns from a launch of the application for the request, none and no launch for a product no script reaches, or the server's failure."""
    match product.script:
        case None:
            return ""
        case script:
            await launch(prepared.folders.bundle)
            arguments = {"direct": source, "with arguments": [msgspec.json.encode(request).decode()], **script.arguments}
            if (result := await session.call_tool("execute", {"application": prepared.installed.url, "command": script.command, "arguments": arguments, "artifacts": []})).isError:
                return Failed(product.name.lower(), tuple(block.text for block in result.content if isinstance(block, TextContent)))
            reply = msgspec.convert(result.structuredContent, Executed).reply
            match dict(zip(reply.keywords, reply.items, strict=True)).get(int.from_bytes(b"----")):
                case Item(text=text):
                    return text
                case None:
                    return Failed(product.name.lower(), (f"{script.command} replied without a result",))


# --- [PROCESS]
async def quit_running(application: Bundle) -> tuple[str, ...]:
    """Errors of quitting every running instance of the application, then of terminating the helpers its bundle holds, which outlive the quit."""
    stuck = await quitted(application, running(application))
    return (*stuck, *await terminated([process for process in psutil.process_iter(["exe"]) if (exe := process.info["exe"]) and Path(exe).is_relative_to(application.path)]))


async def cycle(session: ClientSession, product: Product, prepared: Prepared, source: str, request: Request) -> str | Failed:
    """Lines of the request run in one launch of the application, or its failure with the quit errors appended."""
    ran = await execute(session, product, prepared, source, request)
    stuck = await quit_running(prepared.folders.bundle)
    match ran:
        case Failed(errors=errors):
            return Failed(product.name.lower(), (*errors, *stuck))
        case str() if stuck:
            return Failed(product.name.lower(), stuck, outcome(product.name.lower(), parse(ran)).changes)
        case str():
            return ran


# --- [STAGES]
async def stopped(product: Product, pick: Installed | Failed) -> Installed | Failed:
    """Application with every running instance quit, or the failure of its pick or its quit."""
    match pick:
        case Failed() as failed:
            return failed
        case Installed() as installed:
            stuck = await quit_running(await bundle(Path.from_uri(installed.url)))
            return Failed(product.name.lower(), stuck) if stuck else installed


async def prepare(product: Product, pick: Installed | Failed, declared: tuple[Row, ...], busy: tuple[Product, ...]) -> Prepared | Failed:
    """Product's stores located from its bundle and its files written, leaving a row of a domain every Adobe product shares unwritten while a product runs."""
    match pick:
        case Failed() as failed:
            return failed
        case Installed() as installed:
            folders = await stores.located(product, Path.from_uri(installed.url))
            header = () if product.script else (line(Kind.HEADER, folders.bundle.version, str(Path.home() / "Library" / "Preferences")),)
            held = tuple(row for row in declared if busy and isinstance(row.path, Shared))
            unwritten = tuple(line(Kind.ERROR, f"{row.label} stays unwritten because {', '.join(still.name.lower() for still in busy)} did not quit") for row in held)
            return Prepared(installed, folders, (*header, *await stores.written(folders, tuple(row for row in declared if row not in held)), *unwritten))


async def staged(session: ClientSession, product: Product, prepared: Prepared, source: str, declared: tuple[Row, ...]) -> Applied | Failed:
    """Outcome of the write launch and of the settings-folder files written after its quit."""
    name, scripted = product.name.lower(), tuple(row for row in declared if not isinstance(row.path, Filed))
    settings = str(prepared.folders.preferences) if isinstance(prepared.folders, stores.Support) else None
    match await cycle(session, product, prepared, source, Request(name, scripted, settings)):
        case Failed() as failed:
            return msgspec.structs.replace(failed, changes=(*outcome(name, parse("\n".join(prepared.filed))).changes, *failed.changes))
        case written:
            folder = anyio.Path(applied.folder) if isinstance(applied := outcome(name, parse(written)), Applied) else None
            arranged = await stores.folder_written(folder, declared) if folder else ()
            return outcome(name, parse("\n".join((*prepared.filed, written, *arranged))))


async def converge(session: ClientSession, product: Product, ready: Prepared | Failed, declared: tuple[Row, ...], source: str) -> Applied | Failed:
    """Product's rows written in one launch, its settings-folder files after that launch's quit, and its instance reopened when one ran at discovery."""
    match ready:
        case Failed() as failed:
            return failed
        case Prepared(installed=installed, folders=folders) as prepared:
            result = await staged(session, product, prepared, source, declared)
            if installed.processes:
                await launch(folders.bundle)
            return result


async def served(host: Host, parameters: StdioServerParameters) -> tuple[Applied | Failed, ...]:
    """Each product's outcome through one session of the server the parameters start, running the quit, file, and launch phases in order and each phase for every product concurrently."""
    declared, source = rows(host.units), await anyio.Path(__file__).with_name("script.jsx").read_text(encoding="utf-8")
    async with stdio_client(parameters) as (read, write), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        discovered = msgspec.convert((await session.call_tool("applications", {})).structuredContent, Discovery).applications
        picks = {product: chosen(product, discovered) for product in Product}
        halted = dict(zip(Product, await gather(stopped(product, picks[product]) for product in Product), strict=True))
        busy = tuple(product for product, pick in picks.items() if isinstance(pick, Installed) and isinstance(halted[product], Failed))
        ready = dict(zip(Product, await gather(prepare(product, halted[product], declared[product], busy) for product in Product), strict=True))
        return await gather(converge(session, product, ready[product], declared[product], source) for product in Product)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed, ...]:
    """Each product's outcome through the `adobe` server row."""
    server = host.server("adobe")
    return await served(host, StdioServerParameters(command=server.command, args=list(server.args), env=dict(host.environ), cwd=host.root))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
