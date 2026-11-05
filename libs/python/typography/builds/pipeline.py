"""Face builds: sources reconciled from originals, ufo2ft binaries, inputs hashes that skip unchanged faces, checks, and publication."""

# mypy: disable-error-code="no-untyped-call"

from datetime import timedelta
from enum import auto, StrEnum
from functools import cached_property
from pathlib import Path
lazy from collections.abc import Callable
lazy from datetime import UTC
lazy from functools import cache, partial
lazy import hashlib
lazy from inspect import getfile
lazy from io import BytesIO
lazy import shutil
lazy import subprocess
lazy import sys
lazy import time
lazy from typing import Literal

from expression.extra.result import catch
import msgspec
lazy from expression import Option
lazy from fontTools import unicodedata as scripts
lazy from fontTools.otlLib.builder import buildStatTable
lazy from fontTools.otlLib.optimize.gpos import COMPRESSION_LEVEL
lazy from fontTools.ttLib import TTFont
lazy from fontTools.ttLib.woff2 import compress
lazy from fontTools.varLib.instancer.names import ELIDABLE_AXIS_VALUE_NAME
lazy from pathops import PathOpsError
lazy from ttfautohint import TAError, ttfautohint
lazy from ufo2ft import compileTTF
lazy from ufo2ft.constants import OPENTYPE_CATEGORIES_KEY, OPENTYPE_META_KEY, TRUETYPE_INSTRUCTIONS_KEY
lazy import ufoLib2
lazy from ufoLib2.objects import Anchor as UfoAnchor, Features, Glyph as UfoGlyph, Info as UfoInfo
lazy from ufoLib2.objects.info import GaspBehavior, GaspRangeRecord
lazy from youseedee import database, parsed_unicode_file

from typography.faces.declaration import Faces
from typography.verification.requirements import Finding
lazy from typography.faces.declaration import Declared, declared, Face
lazy from typography.faces.faults import accumulated, Error, Fault, Faults, loaded, Ok, Outcome, paired, read, Reason
lazy from typography.faces.glyphs import Source
lazy from typography.faces.importing import imported
lazy from typography.rules.emission import emitted, FeatureWriter
lazy from typography.rules.measures import Rule
lazy from typography.rules.metrics import measured, Metrics as MetricRule
lazy from typography.rules.plans import executed, planned
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.expectations import expected
lazy from typography.shaping.runs import Binary
lazy from typography.verification.requirements import approved, Check, Expectations, inspected, requirements, verified

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
    """Built face binary with the inputs hash it read, the hash of the bytes it wrote, and the findings of its last check."""

    face: Faces
    path: Path
    inputs: str
    hash: str
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
        return cls(face=face, duration=timedelta(microseconds=(time.monotonic_ns() - started) // 1_000), hash=done.hash if done else "", current=current, findings=len(findings), first=findings[:3])


# --- [SERVICES] -------------------------------------------------------------------------


class Builds(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """One invocation's declaration, corpus, locked dependency closure, source identities, native table identities, and acquired original snapshots."""

    declaration: Declared
    texts: Corpus
    closure: bytes

    # --- [INPUTS]

    @cached_property
    def original(self) -> Callable[[Path], Outcome[Binary]]:
        """Acquires each original once for this invocation."""
        return cache(Binary.read)

    @cached_property
    def inputs(self) -> Callable[[Binary, Faces], str]:
        """Hashes the sources a build reads, the locked closure, and actual loaded Unicode tables once, then each selected plan and original once, check-only modules outside every identity."""
        package, rules, checking = Path(__file__).parents[1], Path(getfile(Rule)).parent, Path(getfile(verified)).parent
        sources = {path: hashlib.sha256(path.read_bytes()).digest() for path in sorted(package.rglob("*.py"))}
        common = {path for path in sources if path.parent not in {rules, checking} and path != Path(getfile(expected))} | {Path(getfile(owner)) for owner in (Rule, planned, emitted)}

        def encoded(value: object, /) -> bytes:
            return msgspec.json.encode(msgspec.to_builtins(value, str_keys=True, order="deterministic"))

        shared = tuple(hashlib.sha256(encoded(value)).digest() for value in (self.texts, self.closure, sys.version_info))
        unicode = tuple(hashlib.sha256(encoded((name, parsed_unicode_file(name)))).digest() for name in database)

        @cache
        def hashed(original: Binary, face: Faces, /) -> str:
            plan = planned(face)
            paths = common | {Path(getfile(type(rule))) for rule in plan}
            selected = tuple((path.relative_to(package).as_posix(), sources[path]) for path in sorted(paths))
            return _hashed(b"".join((*shared, *unicode, hashlib.sha256(original.data).digest(), *(hashlib.sha256(encoded(value)).digest() for value in (face, plan, selected)))))

        return hashed

    # --- [BUILD]

    def built(self, face: Faces, original: Binary, /) -> Outcome[tuple[Built, Source]]:
        """Constructs native layout once, writes both fonts, and records the build's inputs and bytes hashes beside its binary."""
        inputs = self.inputs(original, face)
        return sourced(original, face, self.texts).bind(
            lambda product: (
                written(product[1], self.declaration.source(face))
                .bind(lambda _: compiled(product[0], product[2]))
                .bind(lambda data: _stored(self.declaration.binary(face), partial(Path.write_bytes, data=data)).map(lambda path: Built(face=face, path=path, inputs=inputs, hash=_hashed(data))))
                .bind(lambda done: _stored(self.declaration.stamp(face), partial(Path.write_bytes, data=msgspec.json.encode((done.inputs, done.hash)))).map(lambda _: (done, product[0])))
            )
        )

    def current(self, face: Faces, original: Binary, /) -> Option[Built]:
        """Reads a build whose recorded hashes still match its inputs and stored binary."""
        path = self.declaration.binary(face)
        return (
            read(self.declaration.stamp(face))
            .to_option()
            .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=tuple[str, str]), exception=msgspec.DecodeError)().to_option())
            .filter(lambda found: self.inputs(original, face) == found[0] and read(path).map(_hashed) == Ok(found[1]))
            .map(lambda found: Built(face=face, path=path, inputs=found[0], hash=found[1]))
        )

    def checked(self, found: Built, source: Source, original: Binary, checks: Outcome[tuple[Check, ...]], /) -> Outcome[Built]:
        """Verifies a stored binary against source requirements and its fontspector results and writes its findings, `FAILED` for a measure the native boolean refuses."""
        face = found.face
        measure = partial(verified, source=source, corpus=self.texts, path=found.path, approved=self.declaration.approved(face), checks=checks)
        return (
            Binary
            .read(found.path)
            .bind(
                lambda binary: catch(exception=PathOpsError)(lambda: msgspec.structs.replace(found, findings=measure(binary, requirements(face), original=original)))().map_error(
                    partial(Fault.of, Reason.FAILED, face.stem)
                )
            )
            .bind(lambda verdict: _stored(self.declaration.check(face), partial(Path.write_bytes, data=msgspec.json.format(msgspec.json.encode(verdict.findings)))).map(lambda _: verdict))
        )

    def verified(self, action: Literal[Action.CHECK, Action.RUN], faces: tuple[Faces, ...], /) -> Outcome[tuple[Report, ...]]:
        """Stages each face's current build, a run building one where none is current, verifies every staged build against one fontspector run, and reports each face's own staging and verification time."""
        found = self.declaration

        def staged(face: Faces, started: int, /) -> Outcome[tuple[Built, Source, bool, Binary, int]]:
            return self.original(found.original(face)).bind(  # ty: ignore[unsound-return-statement]
                lambda original: (
                    self
                    .current(face, original)
                    .map(lambda done: sourced(original, face, self.texts).map(lambda product: (done, product[0], True)))
                    .default_with(lambda: self.built(face, original).map(lambda pair: (*pair, False)) if action is Action.RUN else Error(_uncurrent(face)))
                    .map(lambda held: (*held, original, time.monotonic_ns() - started))
                )
            )

        held = tuple((face, staged(face, time.monotonic_ns())) for face in faces)
        checks = inspected(tuple(*outcome.to_option().map(lambda kept: kept[0].path).to_list() for _, outcome in held), found.configuration)

        def reported(face: Faces, kept: tuple[Built, Source, bool, Binary, int], /) -> Outcome[Report]:
            done, source, current, original, spent = kept
            started = time.monotonic_ns() - spent
            return self.checked(done, source, original, checks.map(lambda results: results[done.path])).map(lambda verdict: Report.of(face.stem, started, verdict, current=current))

        return accumulated(outcome.bind(partial(reported, face)) for face, outcome in held)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FILES]


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


def closure() -> Outcome[bytes]:
    """Reads the package's locked dependency closure, uv discovering the lock from the folder holding the package.

    Returns:
        Ok(tree) as `uv tree` prints it, `FAILED` for a failed launch or a refused lock.
    """
    package = Path(__file__).parents[1]
    command = ("uv", "tree", "--frozen", "--package", package.name)
    try:
        return Ok(subprocess.run(command, capture_output=True, check=True, cwd=package.parent).stdout)
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error))
    except subprocess.CalledProcessError as error:
        return Error(Fault.of(Reason.FAILED, " ".join(command), error.stderr.decode().strip()))


# --- [SOURCE]


def sourced(original: Binary, face: Faces, texts: Corpus, /) -> Outcome[tuple[Source, ufoLib2.Font, TTFont]]:
    """Executes drawing rules, compiles layout once, and derives final metrics from that native construction before serialization, `FAILED` for an outline operation the native boolean refuses."""
    plan = planned(face)
    try:
        return (
            imported(original.data, face)
            .bind(lambda source: executed(source, plan, original=original, corpus=texts))
            .bind(lambda source: constructed(source, next(rule for rule in plan if isinstance(rule, MetricRule)), texts))
        )
    except PathOpsError as error:
        return Error(Fault.of(Reason.FAILED, face.stem, error))


def _font(writer: FeatureWriter, /) -> ufoLib2.Font:
    """Returns a source's UFO: glyphs in order, records, vertical metrics, family flags, gasp, the dropout program, and emitted features, fields an ornament face lacks left to ufo2ft fallbacks."""
    source = writer.source
    face, info, metrics, languages = source.face, source.info, source.metrics, list(source.scripts)
    categories = {glyph.name.text: glyph.category for glyph in source.glyphs}
    font = ufoLib2.Font(
        features=Features(writer.substitutions),
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
    font.info.openTypeOS2WidthClass = face.width if isinstance(face, Face) else None
    font.info.openTypeGaspRangeRecords = [GaspRangeRecord(rangeMaxPPEM=0xFFFF, rangeGaspBehavior=list(GaspBehavior))]
    font.lib.update({
        **({OPENTYPE_CATEGORIES_KEY: categories} if {"mark", "ligature"} & {*categories.values()} else {}),
        TRUETYPE_INSTRUCTIONS_KEY: {"formatVersion": "1", "controlValueProgram": "PUSHW[ ] 511 SCANCTRL[ ] PUSHB[ ] 4 SCANTYPE[ ]"},
        **({OPENTYPE_META_KEY: {"slng": languages, **({"dlng": sorted(face.genre.script.scripts)} if isinstance(face, Face) else {})}} if languages else {}),
    })
    for glyph in source.glyphs:
        drawn = UfoGlyph(glyph.name.text, width=glyph.advance, unicodes=sorted(glyph.codes))
        drawn.anchors = [UfoAnchor(anchor.x, anchor.y, anchor.name) for anchor in glyph.anchors]
        glyph.draw(drawn.getPen())
        font.addGlyph(drawn)
    font.glyphOrder = [glyph.name.text for glyph in source.glyphs]
    return font


def written(font: ufoLib2.Font, path: Path, /) -> Outcome[Path]:
    """Writes a constructed UFO through ufoLib2, replacing the one at the path."""
    return _stored(path, lambda target: font.save(target, overwrite=True))


# --- [COMPILE]


def constructed(source: Source, rule: MetricRule, texts: Corpus, /) -> Outcome[tuple[Source, ufoLib2.Font, TTFont]]:
    """Constructs both native font representations with metrics measured on the selected native layout."""
    writer = FeatureWriter(source=source)
    ufo = _font(writer)
    font = compileTTF(ufo, featureWriters=[writer], removeOverlaps=True, overlapsBackend="pathops", reverseDirection=True, useProductionNames=False, ftConfig={COMPRESSION_LEVEL: 9})
    vertical = measured(source, rule, corpus=texts, binary=Binary(data=font))
    ufo.info.ascender, ufo.info.descender = vertical.ascender, vertical.descender
    ufo.info.openTypeOS2WinAscent, ufo.info.openTypeOS2WinDescent = vertical.win_ascent, vertical.win_descent
    font["hhea"].ascent, font["hhea"].descent = vertical.ascender, vertical.descender
    font["OS/2"].sTypoAscender, font["OS/2"].sTypoDescender = vertical.ascender, vertical.descender
    font["OS/2"].usWinAscent, font["OS/2"].usWinDescent = vertical.win_ascent, vertical.win_descent
    named = " ".join(word for word in source.face.style.split() if word != "Italic") or "Regular"
    upright = {"tag": "wght", "name": "Weight", "values": [{"value": font["OS/2"].usWeightClass, "name": named, "flags": ELIDABLE_AXIS_VALUE_NAME * (named == "Regular")}]}
    buildStatTable(font, [upright, *(({"tag": "ital", "name": "Italic", "values": [{"value": 1, "name": "Italic"}]},) if font["post"].italicAngle else ())], macNames=False)
    font["head"].modified = font["head"].created
    font.recalcTimestamp = False
    return Ok((msgspec.structs.replace(source, metrics=vertical), ufo, font))  # ty: ignore[unsound-return-statement]


def _serialized(font: TTFont, /) -> bytes:
    buffer = BytesIO()
    font.save(buffer)
    return buffer.getvalue()


def compiled(source: Source, font: TTFont, /) -> Outcome[bytes]:
    """Serializes a constructed native font, hinting where its genre selects it, equal inputs giving equal bytes."""
    failed, hinted = partial(Fault.of, Reason.FAILED, source.face.stem), isinstance(source.face, Face) and source.face.genre.hinting
    hinting = partial(
        ttfautohint,
        no_info=True,
        epoch=round(source.info.created.timestamp()),
        **({"default_script": scripts.script(chr(source.face.genre.script.stem)).lower()} if isinstance(source.face, Face) else {}),
    )
    return Ok(_serialized(font)).bind(lambda data: catch(exception=TAError)(hinting)(in_buffer=data).map_error(failed) if hinted else Ok(data))  # ty: ignore[unsound-return-statement]


# --- [BUILD]


def published(found: Built, destination: Path, /) -> Outcome[tuple[Path, Path]]:
    """Writes a build's TrueType copy and WOFF2 into a destination folder, `UNWRITABLE` for a failed file operation."""
    ttf, woff2 = destination / found.path.name, (destination / found.path.name).with_suffix(".woff2")
    return _stored(ttf, partial(shutil.copyfile, found.path)).bind(lambda _: _stored(woff2, lambda path: compress(str(found.path), str(path)))).map(lambda _: (ttf, woff2))


# --- [ACTIONS]


def _uncurrent(face: Faces, /) -> Faults:
    return Fault.of(Reason.ABSENT, face.stem, member="current build, run `foundry build` first")


def _acted(action: Literal[Action.SOURCE, Action.BUILD, Action.APPROVE, Action.PUBLISH], builds: Builds, face: Faces, destination: Path | None, original: Binary, /) -> Outcome[Report]:
    """Runs one action over one face, an approval or publication reading the face's current build."""
    answer = partial(Report.of, face.stem, time.monotonic_ns())
    found, texts = builds.declaration, builds.texts
    match action:
        case Action.SOURCE:
            return sourced(original, face, texts).bind(lambda product: written(product[1], found.source(face))).map(lambda _: answer())
        case Action.BUILD:
            return builds.current(face, original).map(lambda done: Ok(answer(done, current=True))).default_with(lambda: builds.built(face, original).map(lambda pair: answer(pair[0])))
        case Action.APPROVE:
            return (
                Ok(face)
                .filter_with(lambda _: any(isinstance(requirement, Expectations) for requirement in requirements(face)), lambda _: Fault.of(Reason.ABSENT, face.stem, member="shaping requirement"))
                .bind(lambda _: builds.current(face, original).to_result_with(partial(_uncurrent, face)))
                .bind(lambda done: Binary.read(done.path).bind(lambda binary: approved(binary, texts, found.approved(face), done.path.name)).map(lambda _: done))
                .map(answer)
            )
        case Action.PUBLISH:
            return (
                Option
                .of_optional(destination)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, action, member="destination"))
                .bind(lambda folder: builds.current(face, original).to_result_with(partial(_uncurrent, face)).bind(lambda done: published(done, folder).map(lambda _: done)))
                .map(answer)
            )


def performed(action: Action, declaration: Path, /, name: str = "", destination: Path | None = None, *, output: Path) -> Outcome[tuple[Report, ...]]:
    """Runs an action over each face a name selects in a set declaration, keeping every face's faults.

    Args:
        action: Stage to run.
        declaration: Set declaration TOML.
        name: Set, family, ornament set, typographic family, or face stem, empty for the whole set.
        destination: Folder a publication copies into.
        output: Folder the set's sources, binaries, and check reports write to.

    Returns:
        One report per face in declaration order, or every face's faults: `UNREADABLE` and `INVALID` for the declaration, its fontspector configuration, or corpus,
        `ABSENT` for an unknown name, a missing current build, a publication without destination, a face a rule cannot measure, or an approval no requirement of the
        face reads, `FAILED` for failed hinting, a failed `uv` or fontspector run, or an outline operation the native boolean refuses, `UNWRITABLE` for an output.
    """

    def acted(builds: Builds, faces: tuple[Faces, ...], /) -> Outcome[tuple[Report, ...]]:
        match action:
            case Action.CHECK | Action.RUN as verifying:
                return builds.verified(verifying, faces)
            case Action.SOURCE | Action.BUILD | Action.APPROVE | Action.PUBLISH as single:
                return accumulated(builds.original(builds.declaration.original(face)).bind(lambda original: _acted(single, builds, face, destination, original)) for face in faces)

    return declared(declaration, output).bind(
        lambda found: paired(
            paired(loaded(Corpus, found.corpus), closure()).map(lambda inputs: Builds(declaration=found, texts=inputs[0], closure=inputs[1])),
            Ok(tuple(face for face in found.set.faces if name in {"", found.set.name, face.family, face.typographic, face.stem})).filter_with(
                bool, lambda _: Fault.of(Reason.ABSENT, found.set.name, member=f"family or face {name}")
            ),
        ).bind(lambda pair: acted(*pair))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Action", "Builds", "Built", "Report", "closure", "compiled", "constructed", "performed", "published", "sourced", "written"]
