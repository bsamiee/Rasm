"""Applies the interface to Illustrator, Photoshop, InDesign, and Acrobat through the `adobe` MCP server, one write launch per scripted product."""

from collections.abc import Callable, Coroutine, Mapping, Sequence
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
from interface.host import application, Applied, DEADLINE, Failed, Host, LAUNCH_ENVIRONMENT, Outcome, parse, plist, quit_application, Stage, terminated
from interface.report import ABSENT, Kind, line

# --- [MODELS] ---------------------------------------------------------------------------


class Server(msgspec.Struct, frozen=True):
    """Command and arguments of a `.mcp.json` server row."""

    command: str
    args: tuple[str, ...]


class Servers(msgspec.Struct, frozen=True):
    """Server rows the Adobe run reads."""

    adobe: Server


class Installed(msgspec.Struct, frozen=True):
    """Installed Adobe application as the `applications` tool reports it."""

    processes: tuple[int, ...]
    scriptable: bool
    url: str


class Creator(msgspec.Struct, frozen=True, rename={"signature": "CFBundleSignature"}):
    """Creator code a bundle's `Info.plist` declares, none for a bundle that declares none."""

    signature: str | None = None


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
async def chosen(product: Product, discovered: Sequence[Installed]) -> Installed | Failed:
    """One scriptable installed application whose bundle declares the product's creator code."""
    match [each for each in discovered if each.scriptable and (await plist(Path.from_uri(each.url), Creator)).signature == product.signature]:
        case [installed]:
            return installed
        case matches:
            return Failed(product.name.lower(), (f"{len(matches)} installed applications declare the creator code {product.signature} and the run needs one",))


async def execute(session: ClientSession, product: Product, installed: Installed, source: str, request: Request) -> str | Failed:
    """Lines `script.jsx` returns from a launch of the application for the request, none and no launch for a product no script reaches, or the server's failure."""
    match product.script:
        case None:
            return ""
        case script:
            await launch(installed)
            arguments = {"direct": source, "with arguments": [msgspec.json.encode(request).decode()], **script.arguments}
            if (result := await session.call_tool("execute", {"application": installed.url, "command": script.command, "arguments": arguments, "artifacts": []})).isError:
                return Failed(product.name.lower(), tuple(block.text for block in result.content if isinstance(block, TextContent)))
            reply = msgspec.convert(result.structuredContent, Executed).reply
            match dict(zip(reply.keywords, reply.items, strict=True)).get(int.from_bytes(b"----")):
                case Item(text=text):
                    return text
                case None:
                    return Failed(product.name.lower(), (f"{script.command} replied without a result",))


# --- [PROCESS]
async def launch(installed: Installed) -> None:
    """Launch the application's bundle without activating it in the login session's environment, returning once the instance has checked in with Launch Services."""
    await anyio.run_process(["/usr/bin/osascript", "-e", f'tell application "{Path.from_uri(installed.url)}" to launch'], env=LAUNCH_ENVIRONMENT)


async def running(installed: Installed) -> list[psutil.Process]:
    """Every process whose executable is the main executable of the application's bundle."""
    bundle = Path.from_uri(installed.url)
    return [process for process in psutil.process_iter(["exe"]) if (exe := process.info["exe"]) and (owner := await application(Path(exe))) is not None and owner.path == bundle]


async def quit_running(installed: Installed) -> tuple[str, ...]:
    """Errors of quitting every running instance of the application, then of terminating the helpers its bundle holds, which outlive the quit."""
    bundle = Path.from_uri(installed.url)
    stuck = await quit_application(bundle, await running(installed))
    return (*stuck, *await terminated([process for process in psutil.process_iter(["exe"]) if (exe := process.info["exe"]) and Path(exe).is_relative_to(bundle)]))


async def cycle(session: ClientSession, product: Product, installed: Installed, source: str, request: Request) -> str | Failed:
    """Lines of the request run in one launch of the application, or its failure with the quit errors appended."""
    ran = await execute(session, product, installed, source, request)
    stuck = await quit_running(installed)
    match ran:
        case Failed(errors=errors):
            return Failed(product.name.lower(), (*errors, *stuck))
        case str() if stuck:
            return Failed(product.name.lower(), stuck, parse(product.name.lower(), ran).changes, Stage.QUIT)
        case str():
            return ran


# --- [STAGES]
async def concurrently[Value](call: Callable[[Product], Coroutine[object, object, Value]]) -> dict[Product, Value]:
    """Each product's result of the call, run for every product concurrently."""
    handles = dict[Product, anyio.TaskHandle[Value]]()
    async with anyio.create_task_group() as group:
        handles.update({product: group.start_soon(call, product) for product in Product})
    return {product: handle.return_value for product, handle in handles.items()}


async def stopped(product: Product, pick: Installed | Failed) -> Installed | Failed:
    """Application with every running instance quit, or the failure of its pick or its quit."""
    match pick:
        case Failed() as failed:
            return failed
        case Installed() as installed:
            stuck = await quit_running(installed)
            return Failed(product.name.lower(), stuck, stage=Stage.QUIT) if stuck else installed


async def prepare(product: Product, pick: Installed | Failed, declared: tuple[Row, ...], busy: tuple[Product, ...]) -> Prepared | Failed:
    """Product's stores located from its bundle and its files written, leaving a row of a domain every Adobe product shares unwritten while a product runs."""
    match pick:
        case Failed() as failed:
            return failed
        case Installed() as installed:
            folders = await stores.located(product, Path.from_uri(installed.url))
            header = () if product.script else (line(Kind.HEADER, folders.bundle.version, folders.bundle.identifier),)
            held = tuple(row for row in declared if busy and isinstance(row.path, Shared))
            unwritten = tuple(line(Kind.ERROR, f"{row.label} stays unwritten because {', '.join(still.name.lower() for still in busy)} did not quit") for row in held)
            return Prepared(installed, folders, (*header, *await stores.written(folders, tuple(row for row in declared if row not in held)), *unwritten))


async def staged(session: ClientSession, product: Product, prepared: Prepared, source: str, declared: tuple[Row, ...]) -> Outcome:
    """Outcome of the write launch and of the settings-folder files written after its quit."""
    name, scripted, installed = product.name.lower(), tuple(row for row in declared if not isinstance(row.path, Filed)), prepared.installed
    settings = str(prepared.folders.preferences) if isinstance(prepared.folders, stores.Support) else None
    match await cycle(session, product, installed, source, Request(name, scripted, settings)):
        case Failed() as failed:
            return msgspec.structs.replace(failed, changes=(*parse(name, "\n".join(prepared.filed)).changes, *failed.changes))
        case written:
            folder = anyio.Path(applied.settings) if isinstance(applied := parse(name, written), Applied) else None
            arranged = await stores.folder_written(folder, declared) if folder else ()
            return parse(name, "\n".join((*prepared.filed, written, *arranged)))


async def converge(session: ClientSession, product: Product, ready: Prepared | Failed, declared: tuple[Row, ...], source: str) -> Outcome:
    """Product's rows written in one launch, its settings-folder files after that launch's quit, and its instance reopened when one ran at discovery."""
    match ready:
        case Failed() as failed:
            return failed
        case Prepared(installed=installed) as prepared:
            outcome = await staged(session, product, prepared, source, declared)
            if installed.processes:
                await launch(installed)
            return outcome


async def served(host: Host, parameters: StdioServerParameters) -> tuple[Outcome, ...]:
    """Each product's outcome through one session of the server the parameters start, running the quit, file, and launch phases in order and each phase for every product concurrently."""
    declared, source = rows(host.units), await anyio.Path(__file__).with_name("script.jsx").read_text(encoding="utf-8")
    async with stdio_client(parameters) as (read, write), ClientSession(read, write, read_timeout_seconds=timedelta(seconds=DEADLINE)) as session:
        await session.initialize()
        discovered = msgspec.convert((await session.call_tool("applications", {})).structuredContent, Discovery).applications
        picks = {product: await chosen(product, discovered) for product in Product}
        halted = await concurrently(lambda product: stopped(product, picks[product]))
        busy = tuple(product for product, pick in picks.items() if isinstance(pick, Installed) and isinstance(halted[product], Failed))
        ready = await concurrently(lambda product: prepare(product, halted[product], declared[product], busy))
        outcomes = await concurrently(lambda product: converge(session, product, ready[product], declared[product], source))
    return tuple(outcomes.values())


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Outcome, ...]:
    """Each product's outcome through the `adobe` server row."""
    server = msgspec.json.decode(host.servers, type=Servers).adobe
    return await served(host, StdioServerParameters(command=server.command, args=list(server.args), env=dict(host.environ), cwd=host.root))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
