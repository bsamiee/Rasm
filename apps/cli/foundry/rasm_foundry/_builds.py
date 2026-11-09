"""Face builds of a declared set across worker processes: sources reconciled from originals, binaries, inputs hashes that skip unchanged faces and checks, approvals, and publication."""

from datetime import timedelta
from enum import auto, StrEnum
from pathlib import Path
import sys
from typing import get_args, Self
lazy from collections import Counter
lazy from collections.abc import Callable
lazy from functools import partial
lazy import hashlib
lazy from importlib import metadata
lazy import time

from expression.extra.result import catch
import msgspec
lazy from anyio import amap, to_process, to_thread
lazy from expression import Option, Result
lazy from fontTools.ufoLib import UFOReader
lazy from fontTools.ufoLib.errors import UFOLibError
lazy from packaging.requirements import Requirement
lazy from packaging.utils import canonicalize_name

from rasm_foundry._checks import _Check, _external, _inspected, _ran
from rasm_foundry._declaration import _Declared, _declared, _Faces
import rasm_typography
lazy from rasm_records import accumulated, committed, Error, Fault, Ok, Outcome, paired, read, Reason, RecordFile
lazy from rasm_typography import Binary, compressed, constructed, Construction, Corpus, Coverage, Expectations, Expected, Face, Finding, Metrics, OrnamentFace, planned, requirements, Rules, verified

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

    face: _Faces
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


class _Verdict(msgspec.Struct, frozen=True, kw_only=True):
    """Check report of one face: the identity of everything its measures read, and every finding they gave."""

    identity: str
    findings: tuple[Finding, ...]


# --- [SERVICES] -------------------------------------------------------------------------


class _Builds(msgspec.Struct, frozen=True, kw_only=True):
    """One invocation's declaration and corpus with the digests of its shared inputs, plain values each worker process receives whole."""

    # --- [STATE]

    declaration: _Declared
    texts: Corpus
    shared: bytes
    modules: frozendict[str, bytes]
    common: frozenset[str]
    checking: bytes

    # --- [INPUTS]

    @classmethod
    def opened(cls, declaration: _Declared, texts: Corpus, closure: frozendict[str, str], /) -> Self:
        """Hashes the corpus, installed closure, interpreter, platform, and the library and app sources once, sorting modules a build reads apart from check-only ones and rule modules apart from the rest."""
        modules = {
            ".".join((root.name, *path.relative_to(root).with_suffix("").parts)): hashlib.sha256(path.read_bytes()).digest()
            for root in (Path(rasm_typography.__file__).parent, Path(__file__).parent)
            for path in sorted(root.rglob("*.py"))
        }
        checks = verified.__module__.rpartition(".")[0]
        checked = {name for name in modules if name.rpartition(".")[0] == checks or name in {Expected.__module__, _inspected.__module__}}
        return cls(
            declaration=declaration,
            texts=texts,
            shared=b"".join(map(_digest, (texts, closure, sys.version_info, sys.platform))),
            modules=frozendict(modules),
            common=frozenset(modules.keys() - checked - {rule.__module__ for rule in (*get_args(Rules.__value__), Metrics)}),
            checking=b"".join(modules[name] for name in sorted(checked)),
        )

    def inputs(self, original: Binary, face: _Faces, /) -> str:
        """Hashes what one face's build reads: the shared digests, its original, record and plan, and the modules outside the rules with the rule modules of its plan, check-only modules outside every identity."""
        plan = planned(face)
        selected = tuple((name, self.modules[name]) for name in sorted(self.common | {type(rule).__module__ for rule in plan}))
        return _hashed(b"".join((self.shared, hashlib.sha256(original.data).digest(), *map(_digest, (face, plan, selected)))))

    def current(self, face: _Faces, original: Binary, /) -> Option[Built]:
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

    def made(self, face: _Faces, original: Binary, /) -> Outcome[Construction]:
        """Constructs a face from its original and writes its UFO source."""
        return constructed(original, face, self.texts).bind(lambda made: _written(made, self.declaration.source(face)))

    def built(self, face: _Faces, original: Binary, /) -> Outcome[Built]:
        """Constructs a face, writes its binary, and records the build's inputs and bytes hashes beside it."""
        inputs = self.inputs(original, face)
        return (
            self
            .made(face, original)
            .bind(Construction.compiled)
            .bind(lambda data: committed(self.declaration.binary(face), data).map(lambda path: Built(face=face, path=path, inputs=inputs, hash=_hashed(data))))
            .bind(lambda done: committed(self.declaration.stamp(face), msgspec.json.encode((done.inputs, done.hash))).map(lambda _: done))
        )

    def staged(self, action: Action, face: _Faces, /) -> Outcome[tuple[Built, Report]]:
        """Reads a face's current build, compiling one where none is current and the action builds, and reports the time and whether a current build was kept.

        Args:
            action: Action the build serves.
            face: Declared face.

        Returns:
            Ok((build, report)), `ABSENT` for a missing current build the action does not compile, and every fault of a failed read or compile.
        """
        started = time.monotonic_ns()
        return (
            read(self.declaration.original(face))
            .bind(Binary.of)
            .bind(
                lambda original: (
                    self
                    .current(face, original)
                    .map(lambda done: Ok((done, Report.of(face.postscript, started, done, current=True))))
                    .default_with(
                        lambda: (
                            self.built(face, original).map(lambda done: (done, Report.of(face.postscript, started, done)))
                            if action.building
                            else Error(Fault.of(Reason.ABSENT, face.postscript, member="current build, run `foundry build` first"))
                        )
                    )
                )
            )
        )

    # --- [VERIFICATION]

    def recorded(self, done: Built, identity: str, /) -> Option[_Verdict]:
        """Reads a face's check report where it holds an identity, the measures it records standing for this build."""
        return (
            read(self.declaration.check(done.face))
            .to_option()
            .bind(lambda raw: catch(lambda: msgspec.json.decode(raw, type=_Verdict), exception=msgspec.DecodeError)().to_option())
            .filter(lambda verdict: verdict.identity == identity)
        )

    def met(self, face: _Faces, verdict: _Verdict, report: Report, /) -> Outcome[Report]:
        """Returns a face's report where its verdict holds no finding, `UNMET` naming each missed requirement's count and the check report otherwise."""
        counts = Counter(finding.requirement for finding in verdict.findings)
        return (
            Error(Fault.of(Reason.UNMET, face.postscript, self.declaration.check(face), member=", ".join(f"{count} {requirement}" for requirement, count in counts.items()))) if counts else Ok(report)
        )

    def covering(self, face: _Faces, /) -> Outcome[Coverage]:
        """Returns the code points a built face answers for, a genre's for a text face and the mappings its written UFO states for an ornament face."""
        match face:
            case Face(genre=genre):
                return Ok(genre.coverage)
            case OrnamentFace():
                source = self.declaration.source(face)
                return (
                    catch(lambda: UFOReader(source).getGlyphSet().getUnicodes(), exception=UFOLibError)()
                    .map_error(partial(Fault.of, Reason.UNREADABLE, str(source)))
                    .map(lambda mapped: Coverage(required=frozenset(*codes for codes in mapped.values()), excluded=(), blocks=frozenset()))
                )

    def measured(self, done: Built, report: Report, identity: str, checks: Outcome[tuple[_Check, ...]], /) -> Outcome[Report]:
        """Verifies a stored build against its requirements and other programs' checks and writes its check report, compiling nothing.

        Args:
            done: Current build.
            report: Report of the build's staging, extended with this face's verification time.
            identity: Identity the check report records.
            checks: Fontspector results on the built file, from `_inspected`.

        Returns:
            Ok(report), `UNMET` for findings, `FAILED` for a measure the native boolean refuses, `UNREADABLE` and `INVALID` for a stored file, and `UNWRITABLE` for the check report.
        """
        started, face, found = time.monotonic_ns(), done.face, self.declaration
        return (
            paired(paired(read(done.path).bind(Binary.of), read(found.original(face)).bind(Binary.of)), paired(self.covering(face), RecordFile(path=found.approved(face), form=Expected).found()))
            .bind(lambda held: verified(held[0][0], face, coverage=held[1][0], original=held[0][1], corpus=self.texts, approved=held[1][1]))
            .map(lambda findings: _Verdict(identity=identity, findings=(*_external(face, done.path, checks), *findings)))
            .bind(lambda verdict: committed(found.check(face), msgspec.json.format(msgspec.json.encode(verdict))).map(lambda _: verdict))
            .bind(lambda verdict: self.met(face, verdict, report.extended(started)))
        )

    async def verified(self, action: Action, faces: tuple[_Faces, ...], /) -> Outcome[tuple[Report, ...]]:
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
        version = (await to_thread.run_sync(_ran, "fontspector", "--version")).default_value(b"")
        identities = {
            done.path: _hashed(b"".join((done.inputs.encode(), digests[done.path.parent], read(self.declaration.approved(done.face)).default_value(b""), self.checking, version))) for done in kept
        }
        records = {done.path: self.recorded(done, identities[done.path]) for done in kept}
        stale = tuple(dict.fromkeys(path for done in kept if records[done.path].is_none() for path in files[done.path.parent]))
        checks = await to_thread.run_sync(_inspected, stale) if stale else Ok(frozendict())

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

    async def acted(self, action: Action, faces: tuple[_Faces, ...], destination: Path | None, /) -> Outcome[tuple[Report, ...]]:
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

    def sourced(self, face: _Faces, /) -> Outcome[Report]:
        """Writes a face's UFO source from its original."""
        started = time.monotonic_ns()
        return read(self.declaration.original(face)).bind(Binary.of).bind(partial(self.made, face)).map(lambda _: Report.of(face.postscript, started))

    def approved(self, face: _Faces, /) -> Outcome[Report]:
        """Writes the shaping approval of a face's current build, `ABSENT` for a face no shaping requirement reads."""
        started = time.monotonic_ns()
        return (
            Ok(face)
            .filter_with(lambda _: any(isinstance(requirement, Expectations) for requirement in requirements(face)), lambda _: Fault.of(Reason.ABSENT, face.postscript, member="shaping requirement"))
            .bind(partial(self.staged, Action.APPROVE))
            .bind(
                lambda held: (
                    read(held[0].path)
                    .bind(Binary.of)
                    .bind(lambda binary: committed(self.declaration.approved(face), Expected.of(binary, self.texts).encoded))
                    .map(lambda _: Report.of(face.postscript, started, held[0]))
                )
            )
        )

    def published(self, destination: Path, face: _Faces, /) -> Outcome[Report]:
        """Writes a face's current build into a destination folder as TrueType and WOFF2, `UNWRITABLE` for a failed file operation."""
        started = time.monotonic_ns()
        return self.staged(Action.PUBLISH, face).bind(
            lambda held: (
                read(held[0].path)
                .bind(lambda data: paired(committed(destination / held[0].path.name, data), committed((destination / held[0].path.name).with_suffix(".woff2"), compressed(data))))
                .map(lambda _: Report.of(face.postscript, started, held[0]))
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DIGESTS]


def _elapsed(started: int, /) -> timedelta:
    return timedelta(microseconds=(time.monotonic_ns() - started) // 1_000)


def _hashed(data: bytes, /) -> str:
    return hashlib.sha256(data).hexdigest()


def _digest(value: object, /) -> bytes:
    """SHA-256 of a value's deterministic JSON, equal values hashing equal in every process."""
    return hashlib.sha256(msgspec.json.encode(msgspec.to_builtins(value, str_keys=True, order="deterministic"))).digest()


def _closure() -> Outcome[frozendict[str, str]]:
    """Reads the installed version of every distribution the app requires, transitively, each requirement taken where its marker holds for the extras requesting it.

    Returns:
        Ok(versions) by canonical name, `UNREADABLE` for a required distribution the environment lacks.
    """
    type Requested = frozenset[tuple[str, frozenset[str]]]
    package = canonicalize_name(__name__.partition(".")[0])

    def reached(pending: Requested, held: Requested, /) -> Requested:
        following = (
            frozenset(
                (canonicalize_name(requirement.name), frozenset(requirement.extras))
                for name, extras in pending
                for requirement in map(Requirement, metadata.requires(name) or ())
                if requirement.marker is None or any(requirement.marker.evaluate({"extra": extra}) for extra in extras or {""})
            )
            - held
        )
        return reached(following, held | following) if following else held

    root = frozenset({(package, frozenset[str]())})
    return catch(lambda: frozendict({name: metadata.version(name) for name, _ in reached(root, root)}), exception=metadata.PackageNotFoundError)().map_error(
        partial(Fault.of, Reason.UNREADABLE, f"{package} dependency closure")
    )


# --- [FILES]


def _written(made: Construction, path: Path, /) -> Outcome[Construction]:
    """Writes a construction's UFO through ufoLib2, replacing the one at the path in one atomic step."""
    return committed(path, lambda stage: catch(made.ufo.save, exception=OSError)(stage).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path)))).map(lambda _: made)


# --- [ACTIONS]


async def _across[R](step: Callable[[_Faces], R], faces: tuple[_Faces, ...], /) -> list[R]:
    """Runs one step per face across worker processes, its results in face order."""
    return await amap(lambda face: to_process.run_sync(step, face), faces)


async def performed(action: Action, declaration: Path, /, name: str = "", destination: Path | None = None, *, output: Path) -> Outcome[tuple[Report, ...]]:
    """Runs an action across worker processes over each face a name selects in a set declaration, keeping every face's faults.

    Args:
        action: Stage to run.
        declaration: Set declaration TOML.
        name: Set, family, ornament set, typographic family, or PostScript name, empty for the whole set.
        destination: Folder a publication copies into.
        output: Folder the set's sources, binaries, and check reports write to.

    Returns:
        One report per face in declaration order, or every face's faults: `UNREADABLE` and `INVALID` for the declaration, corpus, or an approval, `UNMET` for a build missing a
        requirement, which stops a publication whole, `ABSENT` for an unknown name, a missing current build, a publication without destination, a face a rule cannot measure, or
        an approval no requirement of the face reads, `FAILED` for failed hinting, a failed fontspector run or Unicode download, or an outline operation the native boolean refuses,
        `UNWRITABLE` for an output.
    """
    selected = _declared(declaration, output).bind(
        lambda found: paired(
            paired(RecordFile(path=found.corpus, form=Corpus).read(), _closure()).map(lambda inputs: _Builds.opened(found, *inputs)),
            Ok(tuple(face for face in found.set.faces if name in {"", found.set.name, face.family, face.typographic, face.postscript})).filter_with(
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

__all__ = ["Action", "Built", "Report", "performed"]
