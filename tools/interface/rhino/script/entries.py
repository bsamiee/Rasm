# ty: ignore[invalid-argument-type, unresolved-import]
# mypy: disable-error-code="import-not-found, import-untyped, no-any-unimported"
"""Entry points the host runs inside Rhino's CPython for the launch signal, the converging run, and the document report and release before a quit."""

from __future__ import annotations

from collections.abc import Iterable, Iterator
from itertools import chain
from pathlib import Path
import socket
import traceback
from typing import TYPE_CHECKING

import msgspec
import Rhino
from Rhino.PlugIns import PlugIn
from Rhino.UI import RhinoEtoApp

from interface.report import converged, Error, Header, Item, Measurement
from interface.rhino.script import appearance, containers, display, keyboard, options, plugins, template
from interface.rhino.script.accessors import action, port as listener
from interface.units import Units

if TYPE_CHECKING:
    from Grasshopper2.Doc import Document

# --- [OPERATIONS] -----------------------------------------------------------------------


def definitions() -> tuple[Document, ...]:
    """Grasshopper 2 documents while its plug-in is loaded, none before it loads."""
    if PlugIn.GetPlugInInfo(PlugIn.IdFromName("Grasshopper2")).IsLoaded:
        from Grasshopper2.Doc import Document

        return tuple(Document.AllDocuments)
    return ()


def emit(entries: Iterable[Item]) -> None:
    """Print the header line, then each row's change lines and each report line as it comes, a raise printed as one error line holding its message and its cause's with their .NET frames, the row label it notes, and the cause's raising Python frame."""

    def cause(error: BaseException) -> BaseException:
        return error if (inner := error.__cause__ or error.__context__) is None else cause(inner)

    folder = Path(Rhino.RhinoApp.GetDataDirectory(localUser=True, forceDirectoryCreation=False)) / "settings"
    print(Header(str(Rhino.RhinoApp.Version), str(folder)))
    try:
        for text in chain.from_iterable(map(converged, entries)):
            print(text)
    except Exception as error:
        root = cause(error)
        frame = traceback.extract_tb(root.__traceback__)[-1]
        print(Error(f"{''.join(chain.from_iterable(map(traceback.format_exception_only, dict.fromkeys((root, error)))))} at {frame.filename}:{frame.lineno}"))


# --- [COMPOSITION] ----------------------------------------------------------------------


def ready(address: str, port: int) -> None:
    """Send the listener port of Rhino's active document over one connection to the host at the address and port."""
    with socket.create_connection((address, port)) as connection:
        connection.sendall(str(listener(Rhino.RhinoDoc.ActiveDoc)).encode())


def main(doc: Rhino.RhinoDoc) -> None:
    """Converge and report the Settings window closed and every store's rows in store order over each unit system's template facts, Grasshopper 2 last once it loaded, then flush the settings on every path."""
    preferences = RhinoEtoApp.ApplicationPreferencesWindowForPage(None)

    def entries() -> Iterator[Item]:
        targets = {units: template.target(units) for units in Units}
        if preferences is not None:
            yield action(label="ApplicationPreferencesWindow.Visible", read=lambda: preferences.Visible, act=preferences.Close, target=False)
        yield from chain(options.rows(), appearance.rows(), keyboard.rows(), containers.rows(doc, targets), plugins.rows(), display.rows(), template.rows(targets))
        if not PlugIn.LoadPlugIn(PlugIn.IdFromName("Grasshopper2")):
            yield Error("Grasshopper 2 did not load, its rows stay unwritten")
            return
        from interface.rhino.script import grasshopper

        yield from grasshopper.rows(doc, display.point_width(), display.curve_width())

    try:
        emit(entries())
    finally:
        PlugIn.FlushSettingsSavedQueue()


def documents() -> None:
    """Report every titled Rhino and Grasshopper 2 document's path and an error for each one holding unsaved edits."""
    held, loaded = tuple(Rhino.RhinoDoc.OpenDocuments()), definitions()
    emit((
        Measurement(msgspec.json.encode([*(each.Path for each in held if each.Path), *(each.File.Path for each in loaded if each.File.Path)]).decode()),
        *(Error(f"Rhino document {each.Path} holds unsaved edits") for each in held if each.Modified and each.Path),
        *(Error(f"Grasshopper 2 document {each.File.Path} holds unsaved edits") for each in loaded if each.Modified and each.File.Path),
    ))


def release() -> None:
    """Mark every untitled Rhino and Grasshopper 2 document unmodified, so a quit prompts for none."""
    for document in (each for each in Rhino.RhinoDoc.OpenDocuments() if each.Modified and not each.Path):
        document.Modified = False
    for definition in (each for each in definitions() if each.Modified and not each.File.Path):
        definition.Unmodify()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["documents", "main", "ready", "release"]
