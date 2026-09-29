# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type, no-matching-overload]
# mypy: disable-error-code="import-untyped, call-overload"
"""Rhino's command aliases as the rows of `aliases.txt` and the shortcut keys the interface binds."""

from functools import partial
from pathlib import Path

from Rhino.ApplicationSettings import CommandAlias, CommandAliasList, ShortcutKey, ShortcutKeySettings
from System.Collections.Generic import List

from interface.report import Row

# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> tuple[Row, ...]:
    """Rows of the whole alias set by name with each macro and instant flag, then of each bound shortcut key."""
    entries = [line.partition(" ") for line in Path(__file__).with_name("aliases.txt").read_text(encoding="utf-8").splitlines() if line]
    instant = {name for name, _, macro in entries if not macro}
    shortcuts = (
        (ShortcutKey.F3, "! _Properties"),
        (ShortcutKey.CtrlF1, "'_SetMaximizedViewport Top"),
        (ShortcutKey.CtrlF2, "'_SetMaximizedViewport Front"),
        (ShortcutKey.CtrlF3, "'_SetMaximizedViewport Right"),
        (ShortcutKey.CtrlF4, "'_SetMaximizedViewport Perspective"),
    )
    return (
        Row(
            label="CommandAliasList",
            read=lambda: {held.Alias: (held.Macro, held.Instant) for held in map(CommandAliasList.GetAlias, range(CommandAliasList.Count))},
            write=lambda aliases: CommandAliasList.Update(List[CommandAlias]([CommandAlias(name, macro, flag) for name, (macro, flag) in aliases.items()]), replaceAll=True),
            target={name: (macro, name in instant) for name, _, macro in entries if macro},
        ),
        *(Row(label=f'ShortcutKeySettings["{each}"]', read=partial(ShortcutKeySettings.GetMacro, each), write=partial(ShortcutKeySettings.SetMacro, each), target=macro) for each, macro in shortcuts),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
