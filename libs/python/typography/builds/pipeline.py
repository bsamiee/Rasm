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
lazy from importlib.metadata import distributions
lazy from inspect import getfile
lazy from io import BytesIO
lazy import shutil
lazy import sys
lazy import time

from expression.extra.result import catch
import msgspec
lazy from expression import Option
lazy from fontTools import unicodedata as scripts
lazy from fontTools.otlLib.builder import buildStatTable
lazy from fontTools.otlLib.optimize.gpos import COMPRESSION_LEVEL
lazy from fontTools.ttLib import TTFont
lazy from fontTools.ttLib.woff2 import compress
lazy from fontTools.varLib.instancer.names import ELIDABLE_AXIS_VALUE_NAME
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
lazy from typography.faces.faults import accumulated, Fault, Ok, Outcome, paired, Reason
lazy from typography.faces.glyphs import Source
lazy from typography.faces.importing import imported
lazy from typography.rules.emission import emitted, FeatureWriter
lazy from typography.rules.measures import Rule
lazy from typography.rules.metrics import measured, Metrics as MetricRule
lazy from typography.rules.plans import executed, planned
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
        return cls(face=face, duration=timedelta(microseconds=(time.monotonic_ns() - started) // 1_000), hash=done.hash if done else "", current=current, findings=len(findings), first=findings[:3])


# --- [SERVICES] -------------------------------------------------------------------------


class Builds(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """One invocation's declaration, corpus, source identities, native table identities, and acquired original snapshots."""

    declaration: Declared
    texts: Corpus

    # --- [INPUTS]

    @cached_property
    def original(self) -> Callable[[Path], Outcome[Binary]]:
        """Acquires each original once for this invocation."""
        return cache(Binary.read)

    @cached_property
    def inputs(self) -> Callable[[Binary, Faces], str]:
        """Hashes shared sources and actual loaded Unicode tables once, then each selected plan and original once."""
        package, rules = Path(__file__).parents[1], Path(getfile(Rule)).parent
        sources = {path: hashlib.sha256(path.read_bytes()).digest() for path in sorted(package.rglob("*.py"))}
        common = {path for path in sources if path.parent != rules} | {Path(getfile(owner)) for owner in (Rule, planned, emitted)}
        dependencies = tuple(sorted((entry.name, entry.version) for entry in distributions()))

        def encoded(value: object, /) -> bytes:
            return msgspec.json.encode(msgspec.to_builtins(value, str_keys=True, order="deterministic"))

        shared = tuple(hashlib.sha256(encoded(value)).digest() for value in (self.texts, dependencies, sys.version_info))
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
        """Constructs native layout once, writes both fonts, and records the build with the source it used."""
        inputs = self.inputs(original, face)
        return sourced(original, face, self.texts).bind(
            lambda product: (
                written(product[1], self.declaration.source(face))
                .bind(lambda _: compiled(product[0], product[2]))
                .bind(lambda data: _stored(self.declaration.binary(face), partial(Path.write_bytes, data=data)).map(lambda path: Built(face=face, path=path, hash=_hashed(data), inputs=inputs)))
                .bind(_saved)
                .map(lambda done: (done, product[0]))
            )
        )

    def current(self, face: Faces, original: Binary, /) -> Option[Built]:
        """Reads a recorded build whose input identity and external stored binary still match."""
        return (
            _read(self.declaration.binary(face).with_suffix(".json"))
            .to_option()
            .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=Built, dec_hook=lambda kind, value: kind(value)), exception=msgspec.DecodeError)().to_option())
            .filter(lambda found: self.inputs(original, face) == found.inputs and _read(found.path).map(_hashed) == Ok(found.hash))
        )

    def checked(self, found: Built, source: Source, original: Binary, /) -> Outcome[Built]:
        """Verifies a stored binary against source requirements and writes its findings and build record."""
        face = found.face
        measure = partial(verified, source=source, corpus=self.texts, path=found.path, approved=self.declaration.approved(face))
        return (
            Binary
            .read(found.path)
            .map(lambda binary: msgspec.structs.replace(found, findings=measure(binary, requirements(face), original=original)))
            .bind(lambda verdict: _stored(self.declaration.check(face), partial(Path.write_bytes, data=msgspec.json.format(msgspec.json.encode(verdict.findings)))).bind(lambda _: _saved(verdict)))
        )


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


def sourced(original: Binary, face: Faces, texts: Corpus, /) -> Outcome[tuple[Source, ufoLib2.Font, TTFont]]:
    """Executes drawing rules, compiles layout once, and derives final metrics from that native construction before serialization."""
    plan = planned(face)
    return (
        imported(original.data, face)
        .bind(lambda source: executed(source, plan, original=original, corpus=texts))
        .bind(lambda source: constructed(source, next(rule for rule in plan if isinstance(rule, MetricRule)), texts))
    )


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


def _saved(built: Built, /) -> Outcome[Built]:
    return _stored(built.path.with_suffix(".json"), partial(Path.write_bytes, data=msgspec.json.encode(built, enc_hook=str))).map(lambda _: built)


def published(found: Built, destination: Path, /) -> Outcome[tuple[Path, Path]]:
    """Writes a build's TrueType copy and WOFF2 into a destination folder, `UNWRITABLE` for a failed file operation."""
    ttf, woff2 = destination / found.path.name, (destination / found.path.name).with_suffix(".woff2")
    return _stored(ttf, partial(shutil.copyfile, found.path)).bind(lambda _: _stored(woff2, lambda path: compress(str(found.path), str(path)))).map(lambda _: (ttf, woff2))


# --- [ACTIONS]


def _required(builds: Builds, face: Faces, original: Binary, /) -> Outcome[Built]:
    return builds.current(face, original).to_result_with(lambda: Fault.of(Reason.ABSENT, face.stem, member="current build, run `foundry build` first"))


def _acted(action: Action, builds: Builds, face: Faces, destination: Path | None, original: Binary, /) -> Outcome[Report]:
    """Runs one action over one face, a check, approval, or publication reading the face's current build."""
    answer = partial(Report.of, face.stem, time.monotonic_ns())
    found, texts = builds.declaration, builds.texts
    match action:
        case Action.SOURCE:
            return sourced(original, face, texts).bind(lambda product: written(product[1], found.source(face))).map(lambda _: answer())
        case Action.BUILD:
            return builds.current(face, original).map(lambda done: Ok(answer(done, current=True))).default_with(lambda: builds.built(face, original).map(lambda pair: answer(pair[0])))
        case Action.RUN:
            held = builds.current(face, original)
            return (
                held
                .map(lambda done: sourced(original, face, texts).map(lambda product: (done, product[0])))
                .default_with(lambda: builds.built(face, original))
                .bind(lambda pair: builds.checked(*pair, original))
                .map(lambda done: answer(done, current=held.is_some()))
            )
        case Action.CHECK:
            return _required(builds, face, original).bind(lambda done: sourced(original, face, texts).bind(lambda product: builds.checked(done, product[0], original))).map(answer)
        case Action.APPROVE:
            return _required(builds, face, original).bind(lambda done: Binary.read(done.path).bind(lambda binary: approved(binary, texts, found.approved(face))).map(lambda _: done)).map(answer)
        case Action.PUBLISH:
            return (
                Option
                .of_optional(destination)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, action, member="destination"))
                .bind(lambda folder: _required(builds, face, original).bind(lambda done: published(done, folder).map(lambda _: done)))
                .map(answer)
            )


def performed(action: Action, declaration: Path, /, name: str = "", destination: Path | None = None) -> Outcome[tuple[Report, ...]]:
    """Runs an action over each face a name selects in a set declaration, keeping every face's faults.

    Args:
        action: Stage to run.
        declaration: Set declaration TOML.
        name: Set, family, ornament set, typographic family, or face stem, empty for the whole set.
        destination: Folder a publication copies into.

    Returns:
        One report per face in declaration order, or every face's faults: `UNREADABLE` and `INVALID` for the declaration or corpus, `ABSENT` for an unknown name, a
        missing current build, a publication without destination, or a face a rule cannot measure, `FAILED` for failed hinting, `UNWRITABLE` for an output.
    """
    return declared(declaration).bind(
        lambda found: paired(
            corpus(found.corpus),
            Ok(tuple(face for face in found.set.faces if name in {"", found.set.name, face.family, face.typographic, face.stem})).filter_with(
                bool, lambda _: Fault.of(Reason.ABSENT, found.set.name, member=f"family or face {name}")
            ),
        ).bind(
            lambda pair: accumulated(
                builds.original(found.original(face)).bind(lambda original: _acted(action, builds, face, destination, original))
                for builds in (Builds(declaration=found, texts=pair[0]),)
                for face in pair[1]
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Action", "Builds", "Built", "Report", "compiled", "constructed", "performed", "published", "sourced", "written"]
