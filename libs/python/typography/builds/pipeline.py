"""Face builds: sources reconciled from originals, ufo2ft binaries, inputs hashes that skip unchanged faces, checks, and publication."""

from datetime import timedelta
from enum import auto, StrEnum
from pathlib import Path
lazy from collections.abc import Callable
lazy from datetime import UTC
lazy from functools import partial
lazy import hashlib
lazy import inspect
lazy from io import BytesIO
lazy import shutil
lazy import sys
lazy import time

from expression.extra.result import catch
import msgspec
lazy from expression import Ok, Option, Some
lazy from fontTools import unicodedata as scripts
lazy from fontTools.otlLib.builder import buildStatTable
lazy from fontTools.ttLib.woff2 import compress
lazy from fontTools.varLib.instancer.names import ELIDABLE_AXIS_VALUE_NAME
lazy from ttfautohint import TAError, ttfautohint
lazy from ufo2ft import compileTTF
lazy from ufo2ft.constants import OPENTYPE_CATEGORIES_KEY, OPENTYPE_META_KEY, TRUETYPE_INSTRUCTIONS_KEY
lazy from ufo2ft.featureWriters import CursFeatureWriter, GdefFeatureWriter, KernFeatureWriter, MarkFeatureWriter
lazy import ufoLib2
lazy from ufoLib2.objects import Anchor as UfoAnchor, Component as UfoComponent, Features, Glyph as UfoGlyph, Info as UfoInfo
lazy from ufoLib2.objects.info import GaspBehavior, GaspRangeRecord, WidthClass

from typography.faces.declaration import Faces
from typography.verification.requirements import Finding
lazy from typography.faces.declaration import Declared, declared, Face
lazy from typography.faces.faults import accumulated, Fault, Outcome, paired, Reason
lazy from typography.faces.glyphs import Glyph, Letter, Metrics, Source
lazy from typography.faces.importing import imported
lazy from typography.outlines.paths import trace
lazy from typography.rules.emission import emitted
lazy from typography.rules.plans import executed, Hinting, planned
lazy from typography.shaping.corpus import Corpus, corpus
lazy from typography.shaping.runs import Binary
lazy from typography.verification.requirements import approved, requirements, verified

# --- [TYPES] ----------------------------------------------------------------------------


class Action(StrEnum):
    """Stage applied to each selected face: write its UFO source, compile it when changed, verify its build, approve its shaping, build and verify, or publish."""

    SOURCE = auto()
    BUILD = auto()
    CHECK = auto()
    APPROVE = auto()
    RUN = auto()
    PUBLISH = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Built(msgspec.Struct, frozen=True, kw_only=True):
    """Built face binary with its bytes hash, its inputs hash, and the findings of its last check."""

    face: Faces
    path: Path
    hash: str
    inputs: str
    findings: tuple[Finding, ...] = ()


class Report(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Result of one action on one face: binary hash, whether a current build was kept, duration, and findings count with the first three."""

    face: str
    duration: timedelta
    hash: str = ""
    current: bool = False
    findings: int = 0
    first: tuple[Finding, ...] = ()

    @classmethod
    def of(cls, face: str, started: int, done: Built | None = None, /, *, current: bool = False) -> Report:
        """Returns a face's report from a monotonic start in nanoseconds and its build, `None` for a source action."""
        findings = done.findings if done else ()
        return cls(
            face=face, duration=timedelta(milliseconds=(time.monotonic_ns() - started) // 1_000_000), hash=done.hash if done else "", current=current, findings=len(findings), first=findings[:3]
        )


# --- [CODECS] ---------------------------------------------------------------------------


_RECORDS = msgspec.json.Encoder(enc_hook=str)
_RECORD = msgspec.json.Decoder(Built, dec_hook=lambda kind, raw: kind(raw))

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FILES]


def _read(path: Path, /) -> Outcome[bytes]:
    return catch(Path.read_bytes, exception=OSError)(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path)))


def _stored(path: Path, writing: Callable[[Path], object], /) -> Outcome[Path]:
    """Writes a file or folder through a writer after creating its parent folders."""

    @catch(exception=OSError)
    def made() -> Path:
        path.parent.mkdir(parents=True, exist_ok=True)
        writing(path)
        return path

    return made().map_error(partial(Fault.of, Reason.UNWRITABLE, str(path)))


def _hashed(data: bytes, /) -> str:
    return hashlib.sha256(data).hexdigest()


# --- [SOURCE]


def sourced(declared: Declared, face: Faces, texts: Corpus, /) -> Outcome[Source]:
    """Imports a face's original and executes its plan, rules measuring the original as a `Binary`."""
    path = declared.original(face)
    return paired(imported(path, face), Binary.read(path)).bind(lambda pair: executed(pair[0], planned(face), original=pair[1], corpus=texts))


def _inputs(declared: Declared, face: Faces, texts: Corpus, /) -> Outcome[str]:
    """Hashes the face's original, the face, its plan, the corpus, and the source text of the pipeline, plan, emission, and rule modules."""
    plan = planned(face)
    modules = sorted({__name__, planned.__module__, emitted.__module__, *(type(rule).__module__ for rule in plan.rules)})
    encoded = (
        *(msgspec.json.encode(msgspec.to_builtins(value, str_keys=True, order="deterministic")) for value in (face, plan, texts)),
        *(inspect.getsource(sys.modules[module]).encode() for module in modules),
    )
    return _read(declared.original(face)).map(lambda original: _hashed(b"".join(hashlib.sha256(part).digest() for part in (original, *encoded))))


def _glyph(glyph: Glyph, /) -> UfoGlyph:
    drawn = UfoGlyph(glyph.name.text, width=glyph.advance, unicodes=list(glyph.codes), components=[UfoComponent(component.name, component.transform) for component in glyph.components])
    drawn.anchors = [UfoAnchor(anchor.x, anchor.y, anchor.name) for anchor in glyph.anchors]
    trace(drawn.getPen(), glyph.contours)
    return drawn


def _scripts(source: Source, /) -> list[str]:
    return sorted({scripts.script(chr(code)) for glyph in source.by_role(Letter) for code in glyph.name.codes})


def _font(source: Source, metrics: Metrics, /) -> ufoLib2.Font:
    """Returns a source's UFO: glyphs in order, records, vertical metrics, family flags, gasp, the dropout program, and emitted features, fields an ornament face lacks left to ufo2ft fallbacks."""
    face, info, emission, languages = source.face, source.info, emitted(source), _scripts(source)
    categories = {glyph.name.text: glyph.category for glyph in source.glyphs}
    font = ufoLib2.Font(
        features=Features(emission.features),
        groups={name: list(members) for name, members in emission.groups.items()},
        info=UfoInfo(
            familyName=face.typographic,
            styleName=face.style,
            versionMajor=info.version[0],
            versionMinor=info.version[1],
            copyright=info.copyright or None,
            openTypeNameDesigner=info.designer or None,
            openTypeOS2VendorID=info.vendor or None,
            unitsPerEm=source.upm,
            ascender=metrics.ascender,
            descender=metrics.descender,
            xHeight=metrics.x_height or None,
            capHeight=metrics.cap_height or None,
            italicAngle=metrics.italic_angle,
            openTypeHheaCaretOffset=metrics.caret_offset,
            openTypeOS2TypoLineGap=0,
            openTypeOS2WinAscent=metrics.win_ascent,
            openTypeOS2WinDescent=metrics.win_descent,
            openTypeOS2WeightClass=face.weight if isinstance(face, Face) else None,
            openTypeOS2Selection=[7, 8],
            openTypeOS2Type=[2],
            openTypeHeadCreated=f"{info.created.astimezone(UTC):%Y/%m/%d %H:%M:%S}",
            openTypeHeadFlags=[0, 1],
            openTypeHeadLowestRecPPEM=metrics.lowest_ppem,
        ),
    )
    font.info.openTypeOS2WidthClass = WidthClass(face.width) if isinstance(face, Face) else None
    font.info.openTypeGaspRangeRecords = [GaspRangeRecord(rangeMaxPPEM=0xFFFF, rangeGaspBehavior=list(GaspBehavior))]
    font.kerning = dict(emission.kerning)
    font.lib.update({
        **({OPENTYPE_CATEGORIES_KEY: categories} if {"mark", "ligature"} & {*categories.values()} else {}),
        TRUETYPE_INSTRUCTIONS_KEY: {"formatVersion": "1", "controlValueProgram": "PUSHW[ ] 511 SCANCTRL[ ] PUSHB[ ] 4 SCANTYPE[ ]"},
        **({OPENTYPE_META_KEY: {"dlng": languages, "slng": languages}} if languages else {}),
    })
    for glyph in source.glyphs:
        font.addGlyph(_glyph(glyph))
    font.glyphOrder = [glyph.name.text for glyph in source.glyphs]
    return font


def _measured(source: Source, /) -> Outcome[Metrics]:
    return Option.of_optional(source.metrics).to_result_with(lambda: Fault.of(Reason.ABSENT, source.face.stem, member="vertical metrics"))


def written(source: Source, path: Path, /) -> Outcome[Path]:
    """Writes a source as a UFO through ufoLib2, replacing the one at the path."""
    return _measured(source).bind(lambda metrics: _stored(path, lambda target: _font(source, metrics).save(target, overwrite=True)))


# --- [COMPILE]


def _binary(source: Source, metrics: Metrics, /) -> bytes:
    """Compiles a source through four ufo2ft feature writers with the mark writer in append mode, adds the STAT table ufo2ft omits for a static font from its weight class and italic angle, and sets `modified` to `created`."""
    writers = [CursFeatureWriter, KernFeatureWriter, MarkFeatureWriter(mode="append"), GdefFeatureWriter]
    font = compileTTF(_font(source, metrics), featureWriters=writers, removeOverlaps=True, overlapsBackend="pathops", reverseDirection=True, useProductionNames=False)
    named = " ".join(word for word in source.face.style.split() if word != "Italic") or "Regular"
    upright = {"tag": "wght", "name": "Weight", "values": [{"value": font["OS/2"].usWeightClass, "name": named, "flags": ELIDABLE_AXIS_VALUE_NAME * (named == "Regular")}]}
    buildStatTable(font, [upright, *(({"tag": "ital", "name": "Italic", "values": [{"value": 1, "name": "Italic"}]},) if font["post"].italicAngle else ())], macNames=False)
    font["head"].modified = font["head"].created
    font.recalcTimestamp = False
    buffer = BytesIO()
    font.save(buffer)
    return buffer.getvalue()


def compiled(source: Source, /) -> Outcome[bytes]:
    """Compiles a source to TrueType bytes, hinted through ttfautohint where the face's plan holds `Hinting`, equal inputs giving equal bytes."""
    failed, hinted, languages = partial(Fault.of, Reason.FAILED, source.face.stem), any(isinstance(rule, Hinting) for rule in planned(source.face).rules), _scripts(source)
    hinting = partial(ttfautohint, no_info=True, epoch=round(source.info.created.timestamp()), **({"default_script": languages[0].lower()} if languages else {}))
    return (
        _measured(source)
        .bind(lambda metrics: catch(exception=Exception)(_binary)(source, metrics).map_error(failed))
        .bind(lambda data: catch(exception=TAError)(hinting)(in_buffer=data).map_error(failed) if hinted else Ok(data))
    )


# --- [BUILD]


def _saved(built: Built, /) -> Outcome[Built]:
    return _stored(built.path.with_suffix(".json"), partial(Path.write_bytes, data=_RECORDS.encode(built))).map(lambda _: built)


def built(declared: Declared, face: Faces, texts: Corpus, /) -> Outcome[tuple[Built, Source]]:
    """Sources, writes, and compiles a face, recording the build beside its binary as JSON, and returns it with the source it compiled from."""
    return paired(_inputs(declared, face, texts), sourced(declared, face, texts)).bind(
        lambda pair: (
            written(pair[1], declared.source(face))
            .bind(lambda _: compiled(pair[1]))
            .bind(lambda data: _stored(declared.binary(face), partial(Path.write_bytes, data=data)).map(lambda path: Built(face=face, path=path, hash=_hashed(data), inputs=pair[0])))
            .bind(_saved)
            .map(lambda done: (done, pair[1]))
        )
    )


def current(declared: Declared, face: Faces, texts: Corpus, /) -> Option[Built]:
    """Returns a face's recorded build while its inputs hash matches the current inputs and its binary matches the recorded hash, the relation `Reproducible` checks."""
    return (
        _read(declared.binary(face).with_suffix(".json"))
        .to_option()
        .bind(lambda raw: catch(exception=msgspec.DecodeError)(_RECORD.decode)(raw).to_option())
        .filter(lambda found: _inputs(declared, face, texts) == Ok(found.inputs) and _read(found.path).map(_hashed) == Ok(found.hash))
    )


def checked(declared: Declared, found: Built, source: Source, texts: Corpus, /) -> Outcome[Built]:
    """Verifies a build against its face's requirements and writes the findings to its check report and build record, `UNREADABLE` for an unreadable binary and `UNWRITABLE` for an unwritable output."""
    face = found.face
    measured = partial(verified, source=source, corpus=texts, path=found.path, approved=declared.approved(face))
    return (
        paired(Binary.read(declared.original(face)), Binary.read(found.path))
        .map(lambda pair: msgspec.structs.replace(found, findings=measured(pair[1], requirements(face, recorded=Some(found.hash)), original=pair[0])))
        .bind(lambda verdict: _stored(declared.check(face), partial(Path.write_bytes, data=msgspec.json.format(msgspec.json.encode(verdict.findings)))).bind(lambda _: _saved(verdict)))
    )


def published(found: Built, destination: Path, /) -> Outcome[tuple[Path, Path]]:
    """Copies a build's TrueType and a WOFF2 of it into a destination folder without deleting anything there, `FAILED` where the copy's hash differs from the build's."""
    ttf, woff2 = destination / found.path.name, (destination / found.path.name).with_suffix(".woff2")
    return (
        catch(exception=OSError)(shutil.copyfile)(found.path, ttf)
        .bind(lambda _: catch(exception=OSError)(compress)(str(found.path), str(woff2)))
        .map_error(partial(Fault.of, Reason.UNWRITABLE, str(destination)))
        .bind(lambda _: _read(ttf))
        .filter_with(lambda data: _hashed(data) == found.hash, lambda _: Fault.of(Reason.FAILED, str(ttf), f"hash differs from {found.hash}"))
        .map(lambda _: (ttf, woff2))
    )


# --- [ACTIONS]


def _required(found: Declared, face: Faces, texts: Corpus, /) -> Outcome[Built]:
    return current(found, face, texts).to_result_with(lambda: Fault.of(Reason.ABSENT, face.stem, member="current build, run `foundry build` first"))


def _acted(action: Action, found: Declared, face: Faces, texts: Corpus, destination: Path | None, /) -> Outcome[Report]:
    """Runs one action over one face, a check, approval, or publication reading the face's current build."""
    answer = partial(Report.of, face.stem, time.monotonic_ns())
    match action:
        case Action.SOURCE:
            return sourced(found, face, texts).bind(lambda source: written(source, found.source(face))).map(lambda _: answer())
        case Action.BUILD | Action.RUN:
            return (
                current(found, face, texts)
                .map(lambda done: Ok(answer(done, current=True)))
                .default_with(lambda: built(found, face, texts).bind(lambda pair: checked(found, *pair, texts) if action is Action.RUN else Ok(pair[0])).map(answer))
            )
        case Action.CHECK:
            return _required(found, face, texts).bind(lambda done: sourced(found, face, texts).bind(lambda source: checked(found, done, source, texts))).map(answer)
        case Action.APPROVE:
            return _required(found, face, texts).bind(lambda done: Binary.read(done.path).bind(lambda binary: approved(binary, texts, found.approved(face))).map(lambda _: done)).map(answer)
        case Action.PUBLISH:
            return (
                Option
                .of_optional(destination)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, action, member="destination"))
                .bind(lambda folder: _required(found, face, texts).bind(lambda done: published(done, folder).map(lambda _: done)))
                .map(answer)
            )


def performed(action: Action, declaration: Path, /, *, name: str = "", destination: Path | None = None) -> Outcome[tuple[Report, ...]]:
    """Runs an action over each face a name selects in a set declaration, keeping every face's faults.

    Args:
        action: Stage to run.
        declaration: Set declaration TOML.
        name: Set, family, ornament set, typographic family, or face stem, empty for the whole set.
        destination: Folder a publication copies into.

    Returns:
        One report per face in declaration order, or every face's faults: `UNREADABLE` and `INVALID` for the declaration or corpus, `ABSENT` for an unknown name, a
        missing current build, a publication without destination, or a face a rule cannot measure, `FAILED` for a failed compile or a copy with another hash, `UNWRITABLE` for an output.
    """
    return declared(declaration).bind(
        lambda found: paired(
            corpus(found.corpus),
            Ok(tuple(face for face in found.set.faces if name in {"", found.set.name, face.family, face.typographic, face.stem})).filter_with(
                bool, lambda _: Fault.of(Reason.ABSENT, found.set.name, member=f"family or face {name}")
            ),
        ).bind(lambda pair: accumulated(_acted(action, found, face, pair[0], destination) for face in pair[1]))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Action", "Built", "Report", "built", "checked", "compiled", "current", "performed", "published", "sourced", "written"]
