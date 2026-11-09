"""Interface themes: every member of the host's theme and style structs bound through RNA to a value or a color derivation, resolved over a base theme and written by the host's theme writer."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial, reduce
from graphlib import CycleError, TopologicalSorter
import math
import os
from pathlib import Path
import re
import tempfile
from types import MappingProxyType
from typing import Annotated, Any, Final

import _rna_xml  # ruff: ignore[import-private-name]
from bl_ui.space_userpref import USERPREF_MT_interface_theme_presets
import bpy
from coloraide.everything import ColorAll
from expression import Result
from expression.extra.result import catch
import msgspec
import numpy as np

from rasm_bpy.commands.operators import called
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.data._rna.paths import _rna, Struct
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.ui._resources.theme import _color
from rasm_records import accumulated, argued, Case, Error, Fault, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Source = str | int | float | bool
type _Options = Mapping[str, Any]  # type: ignore[explicit-any]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LIGHTNESS: Final = tuple(np.linspace(0.0, 1.0, np.iinfo(np.uint8).max + 1).tolist())

# --- [MODELS] ---------------------------------------------------------------------------

# --- [STEPS]


class Adjust(Case, frozen=True, kw_only=True):
    """Sets a value to `value * factor + offset`: a color's channel named as coloraide reads it (`oklch.l`, `hct.t`, `alpha`), or a number when `channel` is empty."""

    channel: str = ""
    factor: float = 1.0
    offset: float = 0.0


class Harmony(Case, frozen=True, kw_only=True):
    """Takes the color at `index` of a coloraide harmony of the value (`complement`, `triad`), `options` passed to `Color.harmony`."""

    name: str
    index: int
    options: _Options = msgspec.field(default_factory=dict)


class Filter(Case, frozen=True, kw_only=True):
    """Runs a coloraide filter over the value (`grayscale`, `saturate`, `deutan`), `options` passed to `Color.filter`."""

    name: str
    amount: float | None = None
    options: _Options = msgspec.field(default_factory=dict)


class _Composed(Case, frozen=True, kw_only=True):
    """Step reading other colors, each a member's host path or CSS text composited over every `over` color."""

    colors: Annotated[tuple[str, ...], msgspec.Meta(min_length=1)]
    over: tuple[str, ...] = ()
    options: _Options = msgspec.field(default_factory=dict)

    def shown(self, read: Callable[[str], ColorAll], /) -> tuple[ColorAll, ...]:
        """Returns the colors the step reads, one with no backdrop keeping its space and `none` channels."""
        return tuple(ColorAll.layer((read(color), *map(read, self.over))) if self.over else read(color) for color in self.colors)


class Mix(_Composed, frozen=True, kw_only=True):
    """Takes the point at `amount` along an interpolation from the value through `colors`, `options` passed to `Color.interpolate`, a `none` channel keeping the value's."""

    amount: float = 0.5


class Ramp(_Composed, frozen=True, kw_only=True):
    """Maps the value through an interpolation over `colors` at the position its channel `by` reads, `options` passed to `Color.interpolate`, keeping its alpha."""

    by: str = "oklch.l"


class Composite(_Composed, frozen=True, kw_only=True, tag="Layer"):
    """Composites the value over `colors`, `options` passed to `Color.layer` (`blend`, `operator`, `space`)."""


class Closest(_Composed, frozen=True, kw_only=True):
    """Takes the one of `colors` closest to the value, `options` passed to `Color.closest` (`method`)."""


class Legible(_Composed, frozen=True, kw_only=True):
    """Moves the value's OKLCH lightness the least that holds `ratio` shown over every one of `colors`, or to the lightness of most contrast, `options` passed to `Color.contrast` (`method`)."""

    ratio: Annotated[float, msgspec.Meta(gt=0.0)]


type _Step = Adjust | Harmony | Filter | Mix | Ramp | Composite | Closest | Legible

# --- [THEME]


class _Band(msgspec.Struct, frozen=True, kw_only=True):
    """Range of one channel a selected member's base color holds, a `low` above `high` wrapping past the channel's end as a hue does."""

    channel: str
    low: float = -math.inf
    high: float = math.inf

    def __post_init__(self) -> None:
        """Refuses a channel coloraide cannot read."""
        ColorAll("white").get(self.channel)

    def holds(self, value: ColorAll, /) -> bool:
        """Whether the color's channel sits in the range, an undefined channel reading as coloraide resolves it."""
        level = value.get(self.channel, nans=False)
        return self.low <= level <= self.high if self.low <= self.high else level >= self.low or level <= self.high


class _Binding(msgspec.Struct, frozen=True, kw_only=True):
    """Members a value binds to: each member whose host path `members` fully matches, of the kind the source and steps read, whose base color holds every `where` band.

    `source` is the value the steps start from: another member's host path, literal CSS color, number, flag, or enum identifier, the member's own base value when absent.
    """

    members: str
    where: tuple[_Band, ...] = ()
    source: _Source | None = None
    steps: tuple[_Step, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a selection regular expression that fails to compile and a source, bands, and steps reading different kinds."""
        try:
            re.compile(self.members)
        except re.PatternError as error:
            raise ValueError(f"{self.members} is no regular expression: {error}") from error
        if len(self.kinds) > 1:
            raise ValueError(f"{self.members} binds a source, bands, and steps over {', '.join(kind.__name__ for kind in self.kinds)}")

    @property
    def reads(self) -> frozenset[str]:
        """Text of the source and of each color the steps read, member paths among them."""
        return frozenset({str(self.source), *(color for step in self.steps if isinstance(step, _Composed) for color in (*step.colors, *step.over))})

    @property
    def kinds(self) -> frozenset[type]:
        """Kinds of member a literal source, the bands, and the steps read, any kind when empty."""
        match self.source:
            case bool():
                literal: tuple[type, ...] = (bool,)
            case int() | float():
                literal = (float,)
            case str(text) if ColorAll.match(text, fullmatch=True):
                literal = (ColorAll,)
            case _:
                literal = ()
        return frozenset({*literal, *((ColorAll,) if self.where else ()), *(float if isinstance(step, Adjust) and not step.channel else ColorAll for step in self.steps)})


class _InterfaceTheme(msgspec.Struct, frozen=True, kw_only=True):
    """Theme stated as bindings over a base theme, a later binding of a member replacing an earlier one.

    `base` is an installed theme's preset file stem or an XML file, the session's theme when empty, the factory theme in a factory-startup run.
    `fit` is the keywords of the `Color.fit` every stored color takes (`method`, `pspace`).
    """

    base: str = ""
    fit: _Options = msgspec.field(default_factory=dict)
    bindings: tuple[_Binding, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a gamut mapping coloraide lacks, which it reads only for a color outside the gamut."""
        ColorAll("srgb", [2.0, 0.0, 0.0]).fit(**self.fit)


class _Declared(msgspec.Struct, frozen=True):
    """Themes an extension declares by name, the `theme` member of the record `rasm-eng assets` passes."""

    theme: Mapping[str, _InterfaceTheme] = MappingProxyType({})


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VALUES]


def _kind(prop: bpy.types.Property, /) -> type:
    """Returns the kind of value a member holds."""
    match prop:
        case bpy.types.FloatProperty(subtype="COLOR_GAMMA"):
            return ColorAll
        case bpy.types.IntProperty() | bpy.types.FloatProperty():
            return float
        case bpy.types.BoolProperty():
            return bool
        case _:
            return str


def _tone(value: Value, /) -> ColorAll:
    """Returns a color member's stored value as a color."""
    *channels, alpha = _color(value)  # ty: ignore[invalid-argument-type]
    return ColorAll("srgb", channels, alpha)


def _stored(value: ColorAll, fit: _Options, /) -> ColorAll:
    """Returns a color at the eight bits per channel a theme stores, fit to the gamut, the form its contrast and its readers take."""
    return ColorAll(value.convert("srgb").fit(**fit).to_string(hex=True, alpha=True))


def _legible(value: ColorAll, grounds: tuple[ColorAll, ...], ratio: float, fit: _Options, options: _Options, /) -> ColorAll:
    """Returns the stored color at the lightness nearest its own holding the ratio shown over every ground, the lightness of most contrast when none does."""

    def lit(lightness: float) -> ColorAll:
        return _stored(value.clone().set("oklch.l", lightness), fit)

    def contrast(tone: ColorAll) -> float:
        return min(ColorAll.layer((tone, ground)).contrast(ground, **options) for ground in grounds)

    own = value.get("oklch.l", nans=False)
    ordered = sorted((own, *_LIGHTNESS), key=lambda lightness: abs(lightness - own))
    return next((tone for tone in map(lit, ordered) if contrast(tone) >= ratio), None) or max(map(lit, ordered), key=contrast)


def _stepped(read: Callable[[str], ColorAll], fit: _Options, value: ColorAll, step: _Step, /) -> ColorAll:
    """Returns a color after one step, coloraide members refusing names and channels they lack."""
    match step:
        case Adjust(channel=channel, factor=factor, offset=offset):
            return value.set(channel, lambda level: level * factor + offset, nans=False)
        case Harmony(name=name, index=index, options=options):
            return value.harmony(name, **options)[index]
        case Filter(name=name, amount=amount, options=options):
            return value.filter(name, amount, **options)
        case Mix(amount=amount, options=options):
            return ColorAll.interpolate((value, *step.shown(read)), **options)(amount)
        case Ramp(by=by, options=options):
            return ColorAll.interpolate(step.shown(read), **options)(value.get(by, nans=False)).set("alpha", value.alpha(nans=False))
        case Composite(options=options):
            return ColorAll.layer((value, *step.shown(read)), **options)
        case Closest(options=options):
            return value.closest(step.shown(read), **options)
        case Legible(ratio=ratio, options=options):
            return _legible(value, step.shown(read), ratio, fit, options)


def _evaluated(fit: _Options, binding: _Binding, prop: bpy.types.Property, values: Mapping[str, Value], key: str, /) -> Outcome[Value]:
    """Returns the value one binding stores in its member, a refused name, channel, or literal as fault."""
    source = key if binding.source is None else binding.source

    def read(text: str) -> ColorAll:
        return _tone(values[text]) if text in values else ColorAll(text)

    try:
        match prop:
            case bpy.types.FloatProperty(subtype="COLOR_GAMMA", array_length=length):
                final = _stored(reduce(partial(_stepped, read, fit), binding.steps, read(str(source))), fit)
                return Ok((*final.coords(nans=False), final.alpha(nans=False))[:length])
            case _:
                held: Value = reduce(lambda number, step: number * step.factor + step.offset, (step for step in binding.steps if isinstance(step, Adjust)), values.get(str(source), source))
                return Ok(round(held) if isinstance(prop, bpy.types.IntProperty) and isinstance(held, int | float) else held)
    except (ValueError, TypeError, KeyError, IndexError) as error:
        return Error(Fault.of(Reason.INVALID, key, error))


def _reached(binding: _Binding, descriptors: Mapping[str, Descriptor], base: Mapping[str, Value], /) -> tuple[str, ...]:
    """Returns each member a binding selects, its kind the one a source member holds when nothing else states one."""
    named = descriptors.get(str(binding.source))
    kinds = binding.kinds or frozenset(() if named is None else (_kind(named.prop),))
    return tuple(
        key
        for key, descriptor in descriptors.items()
        if re.fullmatch(binding.members, key) and (not kinds or _kind(descriptor.prop) in kinds) and all(band.holds(_tone(base[key])) for band in binding.where)
    )


def _resolved(theme: _InterfaceTheme, descriptors: Mapping[str, Descriptor], base: Mapping[str, Value], /) -> Outcome[Mapping[str, Value]]:
    """Returns the value of each bound member over the base values, each member resolved after the members it reads, or the fault of every binding reaching no member, a dependency cycle, and every value refused."""
    reached = tuple((binding, _reached(binding, descriptors, base)) for binding in theme.bindings)
    bound = {key: binding for binding, keys in reached for key in keys}
    reads = {key: (binding.reads & bound.keys()) - {key} for key, binding in bound.items()}
    missed = tuple(Error(Fault.of(Reason.ABSENT, "Interface theme", member=binding.members)) for binding, keys in reached if not keys)
    try:
        order = tuple(TopologicalSorter(reads).static_order())
    except CycleError as error:
        return Error(Fault.of(Reason.INVALID, "Interface theme", error))
    values, known = dict(base), dict[str, Outcome[Value]]()

    def settled(key: str) -> Outcome[Value]:
        return accumulated(known[read] for read in reads[key]).bind(lambda _: _evaluated(theme.fit, bound[key], descriptors[key].prop, values, key))

    for key in order:
        known[key] = settled(key)
        values.update((key, value) for value in known[key].to_option().to_list())
    return accumulated((*missed, *(known[key] for key in order if all(known[read].is_ok() for read in reads[key])))).map(lambda _: MappingProxyType({key: values[key] for key in bound}))


# --- [SESSIONS]


def _roots(context: bpy.types.Context, /) -> tuple[Struct, ...]:
    """Returns the structs the host's theme presets hold."""
    held: tuple[Struct, ...] = tuple(context.path_resolve(path) for path, _ in USERPREF_MT_interface_theme_presets.preset_xml_map)  # type: ignore[misc]  # ty: ignore[invalid-assignment]
    return held


def _laid(context: bpy.types.Context, theme: _InterfaceTheme, /) -> Outcome[Mapping[str, Value]]:
    """Runs a stated base theme through the host's preset run, then writes each bound member, answering what each holds."""

    def bound(snapshots: tuple[Snapshot, ...]) -> Outcome[Mapping[str, Value]]:
        roots = _roots(context)
        descriptors = {_rna(descriptor.path): descriptor for snapshot, root in zip(snapshots, roots, strict=True) for descriptor in snapshot.selection(root) if not descriptor.compound}
        base = {_rna((*owner, *name)): value for snapshot in snapshots for owner, held in snapshot.tree.rows for name, value in held.rows}
        return _resolved(theme, descriptors, base).bind(
            lambda values: accumulated(assigned(root, {descriptors[key].path: value for key, value in values.items() if descriptors[key].root is type(root)}) for root in roots).map(lambda _: values)
        )

    preset = bpy.utils.preset_find(theme.base, USERPREF_MT_interface_theme_presets.preset_subdir, ext=".xml") or theme.base
    based = called(bpy.ops.script.execute_preset, {"filepath": preset, "menu_idname": USERPREF_MT_interface_theme_presets.__name__}) if theme.base else Ok(frozenset())
    return based.bind(lambda _: accumulated(map(Snapshot.capture, _roots(context)))).bind(bound)


def _restored(context: bpy.types.Context, snapshots: tuple[Snapshot, ...], filepath: str, /, *, dirty: bool) -> None:
    """Writes the captured session values, file, and unsaved mark back, a refusal of what the host held a raised defect."""
    preferences: bpy.types.Preferences = context.preferences  # ty: ignore[invalid-assignment]
    match accumulated(snapshot.apply(root).outcome for snapshot, root in zip(snapshots, _roots(context), strict=True)):
        case Result(tag="ok"):
            preferences.themes[0].filepath, preferences.is_dirty = filepath, dirty
        case Result(error=faults):
            raise RuntimeError("\n".join(fault.message for fault in faults))


@contextmanager
def _held(context: bpy.types.Context, theme: _InterfaceTheme, /) -> Generator[Outcome[Mapping[str, Value]]]:
    """Lays a theme over the session for the scope, the session's theme, file, and unsaved mark restored after.

    Args:
        context: Context whose preferences hold the session's theme.
        theme: Theme laid.

    Yields:
        Stored value of each bound member by host path, or fault of the capture, the base theme, and every binding.
    """
    preferences: bpy.types.Preferences = context.preferences  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        filepath, dirty = preferences.themes[0].filepath, preferences.is_dirty
        yield accumulated(map(Snapshot.capture, _roots(context))).map(lambda snapshots: stack.callback(_restored, context, snapshots, filepath, dirty=dirty)).bind(lambda _: _laid(context, theme))


def _written(context: bpy.types.Context, theme: _InterfaceTheme, target: Path, /) -> Outcome[Path]:
    """Writes a theme as an interface theme file through the host's writer, the session untouched after.

    Args:
        context: Context whose preferences hold the session's theme.
        theme: Theme written.
        target: XML file written, the name the host lists the theme under.

    Returns:
        Ok(target), or fault of the theme, or `UNWRITABLE` fault of a refused write.
    """

    @catch(exception=OSError)
    def authored() -> Path:
        target.parent.mkdir(parents=True, exist_ok=True)
        _rna_xml.xml_file_write(context, str(target), USERPREF_MT_interface_theme_presets.preset_xml_map)
        return target

    with _held(context, theme) as laid:
        return laid.bind(lambda _: authored().map_error(partial(Fault.of, Reason.UNWRITABLE, str(target))))


def installed(context: bpy.types.Context, theme: _InterfaceTheme, name: str, /) -> Outcome[Path]:
    """Writes a theme named `name` and installs it through `theme_install`, into the user's theme presets and live, the host's reset marking preferences to keep it.

    Args:
        context: Context whose preferences hold the session's theme.
        theme: Theme installed.
        name: Name the host lists the theme under.

    Returns:
        Ok(file installed), or fault of the write or the install.
    """
    preferences: bpy.types.Preferences = context.preferences  # ty: ignore[invalid-assignment]
    with tempfile.TemporaryDirectory() as folder:
        answer = _written(context, theme, Path(folder) / f"{name}.xml").bind(lambda path: called(bpy.ops.preferences.theme_install, {"filepath": str(path)}))
    return answer.map(lambda _: Path(preferences.themes[0].filepath))


def themes_written(declared: str, source: str, folder: str, /) -> str | None:
    """Writes each theme an extension declares to `<name>.xml` in a folder, a relative base file read from the extension folder.

    Args:
        declared: `argument` text of the extension's `[tool.rasm-eng]` table.
        source: Extension folder relative base files resolve under.
        folder: Folder each theme file is written into.

    Returns:
        None, or the message of every fault, the value `sys.exit` exits with.
    """
    os.chdir(source)
    match argued(msgspec.json.Decoder(_Declared), source, declared).bind(lambda found: accumulated(_written(bpy.context, theme, Path(folder) / f"{name}.xml") for name, theme in found.theme.items())):
        case Result(tag="ok"):
            return None
        case Result(error=faults):
            return "\n".join(fault.message for fault in faults)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjust", "Closest", "Composite", "Filter", "Harmony", "Legible", "Mix", "Ramp", "installed", "themes_written"]
