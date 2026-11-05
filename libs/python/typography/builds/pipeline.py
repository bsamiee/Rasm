"""Face builds across worker processes: sources reconciled from originals, ufo2ft binaries, inputs hashes that skip unchanged faces and checks, and publication."""

# mypy: disable-error-code="no-untyped-call"

from datetime import timedelta
from enum import auto, StrEnum
from pathlib import Path
from typing import Self
lazy from collections import Counter
lazy from collections.abc import Callable
lazy from datetime import UTC
lazy from functools import partial
lazy import hashlib
lazy from io import BytesIO
lazy import shutil
lazy import sys
lazy import time

from expression.extra.result import catch
import msgspec
lazy from anyio import amap, to_process, to_thread
lazy from expression import Option, Result
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

from typography.faces.declaration import Declared, Faces
from typography.shaping.corpus import Corpus
from typography.verification.requirements import Finding
lazy from typography.faces.declaration import declared, Face, PROFILE
lazy from typography.faces.faults import accumulated, Error, Fault, loaded, Ok, Outcome, paired, ran, read, Reason, stored
lazy from typography.faces.glyphs import Source
lazy from typography.faces.importing import imported
lazy from typography.rules.emission import emitted, FeatureWriter
lazy from typography.rules.measures import Rule
lazy from typography.rules.metrics import measured, Metrics as MetricRule
lazy from typography.rules.plans import executed, planned
lazy from typography.shaping.expectations import expected
lazy from typography.shaping.runs import Binary
lazy from typography.verification.requirements import approved, Check, covering, Expectations, inspected, requirements, verified

# --- [TYPES] ----------------------------------------------------------------------------


class Action(StrEnum):
    """Stage applied to each selected face: write its UFO source, compile it when changed, verify its build, approve its shaping, build and verify, or publish a verified build."""

    SOURCE = auto()
    BUILD = auto()
    CHECK = auto()
    APPROVE = auto()
    RUN = auto()
    PUBLISH = auto()

    @property
    def building(self) -> bool:
        """Whether the action compiles a face no current build answers for."""
        return self in {Action.BUILD, Action.RUN}


# --- [MODELS] ---------------------------------------------------------------------------


class Built(msgspec.Struct, frozen=True, kw_only=True):
    """Built face binary with the inputs hash it read and the hash of the bytes it wrote."""

    face: Faces
    path: Path
    inputs: str
    hash: str


class Report(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Result of one action on one face: binary hash, whether a current build was kept, and the face's own duration, a failed requirement answering in `UNMET` and no report."""

    face: str
    duration: timedelta
    hash: str = ""
    current: bool = False

    @classmethod
    def of(cls, face: str, started: int, done: Built | None = None, /, *, current: bool = False) -> Report:
        """Returns a face's report from a monotonic start in nanoseconds and its build, `None` for a source action."""
        return cls(face=face, duration=_elapsed(started), hash=done.hash if done else "", current=current)

    def extended(self, started: int, /) -> Report:
        """Returns the report with the time since a monotonic start added."""
        return msgspec.structs.replace(self, duration=self.duration + _elapsed(started))


class Verdict(msgspec.Struct, frozen=True, kw_only=True):
    """Check report of one face: the identity of everything its measures read, and every finding they gave."""

    identity: str
    findings: tuple[Finding, ...]


# --- [SERVICES] -------------------------------------------------------------------------


class Builds(msgspec.Struct, frozen=True, kw_only=True):
    """One invocation's declaration and corpus with the digests of its shared inputs, plain values each worker process receives whole."""

    declaration: Declared
    texts: Corpus
    shared: bytes
    modules: frozendict[str, bytes]
    common: frozenset[str]
    checking: bytes

    @classmethod
    def opened(cls, declaration: Declared, texts: Corpus, closure: bytes, /) -> Self:
        """Hashes the corpus, locked closure, interpreter, Unicode tables, and package sources once, sorting modules a build reads apart from check-only ones."""
        package = Path(__file__).parents[1]
        modules = {".".join((package.name, *path.relative_to(package).with_suffix("").parts)): hashlib.sha256(path.read_bytes()).digest() for path in sorted(package.rglob("*.py"))}
        rules, checks = Rule.__module__.rpartition(".")[0], verified.__module__.rpartition(".")[0]
        checked = {name for name in modules if name.rpartition(".")[0] == checks or name == expected.__module__}
        return cls(
            declaration=declaration,
            texts=texts,
            shared=b"".join((*map(_digest, (texts, closure, sys.version_info)), *(_digest((name, parsed_unicode_file(name))) for name in database))),
            modules=frozendict(modules),
            common=frozenset({name for name in modules if name.rpartition(".")[0] != rules and name not in checked} | {owner.__module__ for owner in (Rule, planned, emitted)}),
            checking=b"".join((PROFILE.read_bytes(), *(modules[name] for name in sorted(checked)))),
        )

    # --- [INPUTS]

    def inputs(self, original: Binary, face: Faces, /) -> str:
        """Hashes what one face's build reads: the shared digests, its original, record and plan, and the modules outside the rules with the rule modules of its plan, check-only modules outside every identity."""
        plan = planned(face)
        selected = tuple((name, self.modules[name]) for name in sorted(self.common | {type(rule).__module__ for rule in plan}))
        return _hashed(b"".join((self.shared, hashlib.sha256(original.data).digest(), *map(_digest, (face, plan, selected)))))

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

    # --- [BUILD]

    def built(self, face: Faces, original: Binary, /) -> Outcome[Built]:
        """Constructs native layout once, writes both fonts, and records the build's inputs and bytes hashes beside its binary."""
        inputs = self.inputs(original, face)
        return sourced(original, face, self.texts).bind(
            lambda product: (
                written(product[1], self.declaration.source(face))
                .bind(lambda _: compiled(product[0], product[2]))
                .bind(lambda data: stored(self.declaration.binary(face), partial(Path.write_bytes, data=data)).map(lambda path: Built(face=face, path=path, inputs=inputs, hash=_hashed(data))))
                .bind(lambda done: stored(self.declaration.stamp(face), partial(Path.write_bytes, data=msgspec.json.encode((done.inputs, done.hash)))).map(lambda _: done))
            )
        )

    def staged(self, action: Action, face: Faces, /) -> Outcome[tuple[Built, Report]]:
        """Reads a face's current build, compiling one where none is current and the action builds, and reports the time and whether a current build was kept.

        Args:
            action: Action the build serves.
            face: Declared face.

        Returns:
            Ok((build, report)), `ABSENT` for a missing current build the action does not compile, and every fault of a failed read or compile.
        """
        started = time.monotonic_ns()
        return Binary.read(self.declaration.original(face)).bind(
            lambda original: (
                self
                .current(face, original)
                .map(lambda done: Ok((done, Report.of(face.stem, started, done, current=True))))
                .default_with(
                    lambda: (
                        self.built(face, original).map(lambda done: (done, Report.of(face.stem, started, done)))
                        if action.building
                        else Error(Fault.of(Reason.ABSENT, face.stem, member="current build, run `foundry build` first"))
                    )
                )
            )
        )

    # --- [VERIFICATION]

    def recorded(self, done: Built, identity: str, /) -> Option[Verdict]:
        """Reads a face's check report where it holds an identity, the measures it records standing for this build."""
        return (
            read(self.declaration.check(done.face))
            .to_option()
            .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=Verdict), exception=msgspec.DecodeError)().to_option())
            .filter(lambda verdict: verdict.identity == identity)
        )

    def met(self, face: Faces, verdict: Verdict, report: Report, /) -> Outcome[Report]:
        """Returns a face's report where its verdict holds no finding, `UNMET` naming each missed requirement's count and the check report otherwise."""
        counts = Counter(finding.requirement for finding in verdict.findings)
        return Error(Fault.of(Reason.UNMET, face.stem, self.declaration.check(face), member=", ".join(f"{count} {requirement}" for requirement, count in counts.items()))) if counts else Ok(report)

    def measured(self, done: Built, report: Report, identity: str, checks: Outcome[tuple[Check, ...]], /) -> Outcome[Report]:
        """Verifies a stored build against requirements and its fontspector results and writes its check report, compiling nothing.

        Args:
            done: Current build.
            report: Report of the build's staging, extended with this face's verification time.
            identity: Identity the check report records.
            checks: Fontspector results on the built file, from `inspected`.

        Returns:
            Ok(report), `UNMET` for findings, `FAILED` for a measure the native boolean refuses, `UNREADABLE` for a stored file, and `UNWRITABLE` for the check report.
        """
        started, face, found = time.monotonic_ns(), done.face, self.declaration
        return (
            paired(paired(Binary.read(done.path), Binary.read(found.original(face))), covering(face, found.source(face)))
            .bind(
                lambda held: catch(exception=PathOpsError)(
                    lambda: verified(held[0][0], requirements(face), coverage=held[1], original=held[0][1], corpus=self.texts, path=done.path, approved=found.approved(face), checks=checks)
                )().map_error(partial(Fault.of, Reason.FAILED, face.stem))
            )
            .map(lambda findings: Verdict(identity=identity, findings=findings))
            .bind(lambda verdict: stored(found.check(face), partial(Path.write_bytes, data=msgspec.json.format(msgspec.json.encode(verdict)))).map(lambda _: verdict))
            .bind(lambda verdict: self.met(face, verdict, report.extended(started)))
        )

    async def verified(self, action: Action, faces: tuple[Faces, ...], /) -> Outcome[tuple[Report, ...]]:
        """Stages every face in parallel, measures in parallel each face whose check identity moved, and runs fontspector once before them over every face of each folder holding one.

        Args:
            action: Action staging each face, building where it builds.
            faces: Selected faces.

        Returns:
            One report per face, or every face's faults: `UNMET` for a face with findings, a recorded verdict answering where its identity holds.
        """
        held = await _across(partial(self.staged, action), faces)
        kept = tuple(*outcome.to_option().map(lambda pair: pair[0]).to_list() for outcome in held)
        files = {folder: tuple(sorted(folder.glob("*.ttf"))) for folder in {done.path.parent for done in kept}}
        digests = {folder: _digest(tuple((path.name, read(path).map(_hashed).default_value("")) for path in paths)) for folder, paths in files.items()}
        version = (await to_thread.run_sync(ran, "fontspector", "--version")).default_value(b"")
        identities = {
            done.path: _hashed(b"".join((done.inputs.encode(), digests[done.path.parent], read(self.declaration.approved(done.face)).default_value(b""), self.checking, version))) for done in kept
        }
        records = {done.path: self.recorded(done, identities[done.path]) for done in kept}
        stale = tuple(dict.fromkeys(path for done in kept if records[done.path].is_none() for path in files[done.path.parent]))
        checks = await to_thread.run_sync(inspected, stale, self.declaration.root) if stale else Ok(frozendict())

        async def verdict(outcome: Outcome[tuple[Built, Report]], /) -> Outcome[Report]:
            match outcome:
                case Result(tag="ok", ok=(done, report)):
                    match records[done.path]:
                        case Option(tag="some", some=found):
                            return self.met(done.face, found, report)
                        case _:
                            return await to_process.run_sync(self.measured, done, report, identities[done.path], checks.map(lambda results: results[done.path]))
                case Result(error=faults):
                    return Error(faults)

        return accumulated(await amap(verdict, held))

    # --- [ACTIONS]

    async def acted(self, action: Action, faces: tuple[Faces, ...], destination: Path | None, /) -> Outcome[tuple[Report, ...]]:
        """Runs an action over faces across worker processes, a publication answering whole on the faces' check before any copy."""
        match action:
            case Action.SOURCE:
                return accumulated(await _across(self.sourced, faces))
            case Action.BUILD:
                return accumulated(outcome.map(lambda pair: pair[1]) for outcome in await _across(partial(self.staged, action), faces))
            case Action.CHECK | Action.RUN:
                return await self.verified(action, faces)
            case Action.APPROVE:
                return accumulated(await _across(self.approved, faces))
            case Action.PUBLISH:
                if destination is None:
                    return Error(Fault.of(Reason.ABSENT, action, member="destination"))
                checked = await self.verified(Action.CHECK, faces)
                return accumulated(await _across(partial(self.published, destination), faces)) if checked.is_ok() else checked

    def sourced(self, face: Faces, /) -> Outcome[Report]:
        """Writes a face's UFO source from its original."""
        started = time.monotonic_ns()
        return (
            Binary
            .read(self.declaration.original(face))
            .bind(lambda original: sourced(original, face, self.texts))
            .bind(lambda product: written(product[1], self.declaration.source(face)))
            .map(lambda _: Report.of(face.stem, started))
        )

    def approved(self, face: Faces, /) -> Outcome[Report]:
        """Writes the shaping approval of a face's current build, `ABSENT` for a face no shaping requirement reads."""
        started = time.monotonic_ns()
        return (
            Ok(face)
            .filter_with(lambda _: any(isinstance(requirement, Expectations) for requirement in requirements(face)), lambda _: Fault.of(Reason.ABSENT, face.stem, member="shaping requirement"))
            .bind(partial(self.staged, Action.APPROVE))
            .bind(
                lambda held: (
                    Binary
                    .read(held[0].path)
                    .bind(lambda binary: approved(binary, self.texts, self.declaration.approved(face), held[0].path.name))
                    .map(lambda _: Report.of(face.stem, started, held[0]))
                )
            )
        )

    def published(self, destination: Path, face: Faces, /) -> Outcome[Report]:
        """Writes a face's current build into a destination folder."""
        started = time.monotonic_ns()
        return self.staged(Action.PUBLISH, face).bind(lambda held: published(held[0], destination).map(lambda _: Report.of(face.stem, started, held[0])))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [DIGESTS]


def _elapsed(started: int, /) -> timedelta:
    return timedelta(microseconds=(time.monotonic_ns() - started) // 1_000)


def _hashed(data: bytes, /) -> str:
    return hashlib.sha256(data).hexdigest()


def _digest(value: object, /) -> bytes:
    """SHA-256 of a value's deterministic JSON, equal values hashing equal in every process."""
    return hashlib.sha256(msgspec.json.encode(msgspec.to_builtins(value, str_keys=True, order="deterministic"))).digest()


def closure() -> Outcome[bytes]:
    """Reads the package's locked dependency closure, uv discovering the lock from the folder holding the package.

    Returns:
        Ok(tree) as `uv tree` prints it, `FAILED` for a failed launch or a refused lock.
    """
    package = Path(__file__).parents[1]
    return ran("uv", "tree", "--frozen", "--package", package.name, cwd=package.parent)


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
    return stored(path, lambda target: font.save(target, overwrite=True))


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
    return stored(ttf, partial(shutil.copyfile, found.path)).bind(lambda _: stored(woff2, lambda path: compress(str(found.path), str(path)))).map(lambda _: (ttf, woff2))


# --- [ACTIONS]


async def _across[R](step: Callable[[Faces], R], faces: tuple[Faces, ...], /) -> list[R]:
    """Runs one step per face across worker processes, its results in face order."""
    return await amap(lambda face: to_process.run_sync(step, face), faces)


async def performed(action: Action, declaration: Path, /, name: str = "", destination: Path | None = None, *, output: Path) -> Outcome[tuple[Report, ...]]:
    """Runs an action across worker processes over each face a name selects in a set declaration, keeping every face's faults.

    Args:
        action: Stage to run.
        declaration: Set declaration TOML.
        name: Set, family, ornament set, typographic family, or face stem, empty for the whole set.
        destination: Folder a publication copies into.
        output: Folder the set's sources, binaries, and check reports write to.

    Returns:
        One report per face in declaration order, or every face's faults: `UNREADABLE` and `INVALID` for the declaration or corpus, `UNMET` for a build missing a requirement,
        which stops a publication whole, `ABSENT` for an unknown name, a missing current build, a publication without destination, a face a rule cannot measure, or an approval
        no requirement of the face reads, `FAILED` for failed hinting, a failed `uv` or fontspector run, or an outline operation the native boolean refuses, `UNWRITABLE` for an output.
    """
    selected = declared(declaration, output).bind(
        lambda found: paired(
            paired(loaded(Corpus, found.corpus), closure()).map(lambda inputs: Builds.opened(found, *inputs)),
            Ok(tuple(face for face in found.set.faces if name in {"", found.set.name, face.family, face.typographic, face.stem})).filter_with(
                bool, lambda _: Fault.of(Reason.ABSENT, found.set.name, member=f"family or face {name}")
            ),
        )
    )
    match selected:
        case Result(tag="ok", ok=(builds, faces)):
            return await builds.acted(action, faces, destination)
        case Result(error=faults):
            return Error(faults)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Action", "Builds", "Built", "Report", "Verdict", "closure", "compiled", "constructed", "performed", "published", "sourced", "written"]
