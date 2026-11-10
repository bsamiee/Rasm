"""Interface themes: every member of the host's theme and style structs bound through RNA to a value or a color derivation, resolved over a base theme and written by the host's theme writer."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
from graphlib import CycleError, TopologicalSorter
from itertools import tee
import math
from pathlib import Path
import re
import tempfile
from types import MappingProxyType
from typing import Annotated, Any, Final

import _rna_xml  # ruff: ignore[import-private-name]
from bl_ui.space_userpref import USERPREF_MT_interface_theme_presets
import bpy
from coloraide.everything import ColorAll
from expression import Option, Result
from expression.extra.result import catch
import msgspec
import numpy as np

from rasm_bpy.commands.operators import called
from rasm_bpy.data._rna.descriptors import Value
from rasm_bpy.data._rna.paths import rna, rooted, Struct
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.ui._resources.theme import display_color
from rasm_records import accumulated, Case, Error, Fault, Ok, Outcome, paired, Reason, typed

# --- [TYPES] ----------------------------------------------------------------------------

type _Source = str | int | float | bool
type _Options = Mapping[str, Any]  # type: ignore[explicit-any]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LIGHTNESS: Final = tuple(np.linspace(0.0, 1.0, np.iinfo(np.uint8).max + 1).tolist())

# --- [MODELS] ---------------------------------------------------------------------------

# --- [STEPS]


class Adjust(Case, frozen=True, kw_only=True):
    """Sets a value to `value * factor + offset`: a color's channel named as coloraide reads it (`oklch.l`, `hct.t`, `alpha`), or a number when `channel` is absent."""

    channel: str | None = None
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
        return frozenset({*literal, *((ColorAll,) if self.where else ()), *(float if isinstance(step, Adjust) and step.channel is None else ColorAll for step in self.steps)})


class _InterfaceTheme(msgspec.Struct, frozen=True, kw_only=True):
    """Theme stated as bindings over a base theme, a later binding of a member replacing an earlier one.

    `base` is an installed theme's preset file stem or an XML file, the session's theme when absent, the factory theme in a factory-startup run.
    `fit` is the keywords of the `Color.fit` every stored color takes (`method`, `pspace`).
    """

    base: str | None = None
    fit: _Options = msgspec.field(default_factory=dict)
    bindings: tuple[_Binding, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a gamut mapping coloraide lacks, which it reads only for a color outside the gamut."""
        ColorAll("srgb", [2.0, 0.0, 0.0]).fit(**self.fit)


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
    *channels, alpha = display_color(np.asarray(value, np.float64))
    return ColorAll("srgb", channels, alpha)


def _stored(value: ColorAll, fit: _Options, /) -> ColorAll:
    """Returns a color at the eight bits per channel a theme stores, fit to the gamut, the form its contrast and its readers take."""
    return ColorAll(value.convert("srgb").fit(**fit).to_string(hex=True, alpha=True))


def _stepped(read: Callable[[str], ColorAll], fit: _Options, value: ColorAll, step: _Step, /) -> ColorAll:
    """Returns a color after one step, coloraide members refusing names and channels they lack."""
    match step:
        case Adjust(channel=str(channel), factor=factor, offset=offset):
            return value.set(channel, lambda level: level * factor + offset, nans=False)
        case Adjust():
            raise ValueError("Adjust without a channel sets a number, not a color")
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
            grounds, own = step.shown(read), value.get("oklch.l", nans=False)
            tones = (_stored(value.clone().set("oklch.l", lightness), fit) for lightness in sorted((own, *_LIGHTNESS), key=lambda lightness: abs(lightness - own)))
            passing, every = tee((min(ColorAll.layer((tone, ground)).contrast(ground, **options) for ground in grounds), tone) for tone in tones)
            return next((tone for contrast, tone in passing if contrast >= ratio), None) or max(every, key=lambda pair: pair[0])[1]


# --- [SESSIONS]


def _roots(context: bpy.types.Context, /) -> Outcome[tuple[Struct, ...]]:
    """Returns the structs the host's theme presets hold."""
    return accumulated(typed(bpy.types.bpy_struct, path, context.path_resolve(path)) for path, _ in USERPREF_MT_interface_theme_presets.preset_xml_map)


def _restored(context: bpy.types.Context, preferences: bpy.types.Preferences, snapshots: tuple[Snapshot, ...], filepath: str, /, *, dirty: bool) -> None:
    """Writes the captured session values, file, and unsaved mark back, a refusal of what the host held a raised defect."""
    match _roots(context).bind(lambda roots: accumulated(snapshot.apply(root).outcome for snapshot, root in zip(snapshots, roots, strict=True))):
        case Result(tag="ok"):
            preferences.themes[0].filepath, preferences.is_dirty = filepath, dirty
        case Result(error=faults):
            raise RuntimeError("\n".join(fault.message for fault in faults))


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


def installed(context: bpy.types.Context, theme: _InterfaceTheme, name: str, /) -> Outcome[Path]:
    """Lays a theme over the session, writes it through the host's theme writer, restores the session's theme, file, and unsaved mark, and installs the file through `theme_install`.

    Args:
        context: Context whose preferences hold the session's theme.
        theme: Theme installed, each bound member resolved after the members it reads.
        name: Name the host lists the theme under.

    Returns:
        Ok(file installed), or fault of the capture, the base theme, every binding reaching no member, a dependency cycle, every value refused, the write, or the install.
    """

    def bound(roots: tuple[Struct, ...], snapshots: tuple[Snapshot, ...]) -> Outcome[Mapping[str, Value]]:
        descriptors = {rna(descriptor.path): descriptor for snapshot, root in zip(snapshots, roots, strict=True) for descriptor in snapshot.selection(root) if not descriptor.compound}
        base = {rna((*owner, *name)): value for snapshot in snapshots for owner, held in snapshot.tree.rows for name, value in held.rows}
        reached = tuple(
            (
                binding,
                tuple(
                    key
                    for key, descriptor in descriptors.items()
                    if re.fullmatch(binding.members, key) and (not kinds or _kind(descriptor.prop) in kinds) and all(band.holds(_tone(base[key])) for band in binding.where)
                ),
            )
            for binding in theme.bindings
            for named in (descriptors.get(str(binding.source)),)
            for kinds in (binding.kinds or frozenset(() if named is None else (_kind(named.prop),)),)
        )
        chosen = {key: binding for binding, keys in reached for key in keys}
        reads = {key: (binding.reads & chosen.keys()) - {key} for key, binding in chosen.items()}
        missed = tuple(Error(Fault.of(Reason.ABSENT, "Interface theme", member=binding.members)) for binding, keys in reached if not keys)
        try:
            order = tuple(TopologicalSorter(reads).static_order())
        except CycleError as error:
            return Error(Fault.of(Reason.INVALID, "Interface theme", error))
        values, known = base.copy(), dict[str, Outcome[Value]]()

        def settled(key: str) -> Outcome[Value]:
            return accumulated(known[other] for other in reads[key]).bind(lambda _: _evaluated(theme.fit, chosen[key], descriptors[key].prop, values, key))

        for key in order:
            known[key] = settled(key)
            values.update((key, value) for value in known[key].to_option().to_list())
        return (
            accumulated((*missed, *(known[key] for key in order if all(known[other].is_ok() for other in reads[key]))))
            .bind(lambda _: accumulated(assigned(root, {descriptors[key].path: values[key] for key in chosen if descriptors[key].root is type(root)}) for root in roots))
            .map(lambda _: MappingProxyType({key: values[key] for key in chosen}))
        )

    with tempfile.TemporaryDirectory() as folder:
        target = Path(folder) / f"{name}.xml"
        with ExitStack() as stack:
            written = (
                paired(rooted(bpy.types.Preferences, context), _roots(context).bind(lambda roots: accumulated(map(Snapshot.capture, roots))))
                .map(lambda held: stack.callback(_restored, context, *held, held[0].themes[0].filepath, dirty=held[0].is_dirty))
                .bind(
                    lambda _: (
                        Option
                        .of_optional(theme.base)
                        .map(
                            lambda base: called(
                                bpy.ops.script.execute_preset,
                                {
                                    "filepath": bpy.utils.preset_find(base, USERPREF_MT_interface_theme_presets.preset_subdir, ext=".xml") or base,
                                    "menu_idname": USERPREF_MT_interface_theme_presets.__name__,
                                },
                            )
                        )
                        .default_value(Ok(frozenset()))
                    )
                )
                .bind(lambda _: _roots(context))
                .bind(lambda roots: accumulated(map(Snapshot.capture, roots)).bind(partial(bound, roots)))
                .bind(
                    lambda _: (
                        catch(target.parent.mkdir, exception=OSError)(parents=True, exist_ok=True)
                        .bind(lambda _: catch(_rna_xml.xml_file_write, exception=OSError)(context, str(target), USERPREF_MT_interface_theme_presets.preset_xml_map))
                        .map_error(partial(Fault.of, Reason.UNWRITABLE, str(target)))
                    )
                )
            )
        answer = written.bind(lambda _: called(bpy.ops.preferences.theme_install, {"filepath": str(target)}))
    return answer.bind(lambda _: rooted(bpy.types.Preferences, context)).map(lambda preferences: Path(preferences.themes[0].filepath))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjust", "Closest", "Composite", "Filter", "Harmony", "Legible", "Mix", "Ramp", "installed"]
