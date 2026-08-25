"""Own Assay artifact storage: the fsspec store boundary, per-run scopes, zstd history frames, and retention.

`ArtifactStore` validates every path against the store root; `ArtifactScope` projects .NET artifact
directories and command flags; history payloads compress under one store-owned zstd frame boundary.
"""

import contextlib
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Final, Protocol, runtime_checkable, TYPE_CHECKING

import msgspec
import zstandard

from assay.core.model import ArtifactKind, Envelope, Report, wire_encode


if TYPE_CHECKING:
    from collections.abc import Callable

    from upath import UPath

    from assay.composition.settings import AssaySettings
    from assay.core.model import Claim


# --- [TYPES] ----------------------------------------------------------------------------


class ByteSink(Protocol):
    """Writable half of a backend file handle."""

    def write(self, data: bytes, /) -> object:
        """Write bytes to the open handle."""


@runtime_checkable
class ArtifactFileSystem(Protocol):
    """Structural subset of fsspec used by Assay artifact storage."""

    def makedirs(self, path: str, *, exist_ok: bool = False) -> object:
        """Create a directory path if needed."""

    def glob(self, path: str) -> list[str]:
        """Expand a backend glob."""

    def ls(self, path: str, *, detail: bool = False) -> list[str] | list[dict[str, object]]:
        """List direct backend children; detail mode returns metadata rows."""

    def find(self, path: str, *, detail: bool = False) -> list[str] | dict[str, dict[str, object]]:
        """List recursive backend children; detail mode returns metadata keyed by path."""

    def info(self, path: str) -> dict[str, object]:
        """Return backend metadata for a path."""

    def exists(self, path: str) -> bool:
        """Return whether the backend path exists."""

    def cat_file(self, path: str) -> bytes:
        """Read bytes from a backend file."""

    def rm(self, path: str, *, recursive: bool = False) -> object:
        """Remove a backend path."""

    def open(self, path: str, mode: str = "rb", *, autocommit: bool = True) -> contextlib.AbstractContextManager[ByteSink]:
        """Open a backend file.

        `autocommit=False` defers writes to the active backend transaction.
        """

    @property
    def transaction(self) -> contextlib.AbstractContextManager[object]:
        """A backend write transaction, or nullcontext when unsupported."""
        return contextlib.nullcontext()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACTS: Final[str] = ".artifacts"
_ARTIFACTS_PATH_FLAG: Final[str] = "--artifacts-path"
_BUILD: Final[str] = "build"
_DOTNET_ROOT: Final[str] = f"{_ARTIFACTS}/dotnet"
_HISTORY_COMPRESSOR: Final[zstandard.ZstdCompressor] = zstandard.ZstdCompressor(level=10)
_HISTORY_DECOMPRESSOR: Final[zstandard.ZstdDecompressor] = zstandard.ZstdDecompressor()
_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)
_REPORT_DECODER: Final[msgspec.json.Decoder[Report]] = msgspec.json.Decoder(Report)
_DOTNET_FIRST_RUN_SENTINELS: Final[tuple[str, ...]] = ("dotnetFirstUseSentinel", "aspNetCertificateSentinel", "toolpath.sentinel")
PY_ARTIFACT_ROOTS: Final[dict[str, str]] = {
    "coverage": f"{_ARTIFACTS}/python/coverage",
    "benchmarks": f"{_ARTIFACTS}/python/benchmarks",
    "mutmut": f"{_ARTIFACTS}/python/mutmut/work",
}
PY_COVERAGE_FILES: Final[dict[str, str]] = {fmt: f"{PY_ARTIFACT_ROOTS['coverage']}/coverage.{fmt}" for fmt in ("json", "xml", "lcov")}
DOTNET_ARTIFACT_ROOTS: Final[dict[str, str]] = {
    "stryker": f"{_ARTIFACTS}/dotnet/stryker/work",
    "stryker-output": f"{_ARTIFACTS}/dotnet/stryker",
    "trx": f"{_ARTIFACTS}/dotnet/trx",
}
DOTNET_BUILD_CLOSURE: Final[str] = "dotnet"

# --- [BOUNDARIES] -----------------------------------------------------------------------


def mtime_from_info(info: dict[str, object]) -> float:
    """Coerce fsspec mtime/created metadata to a POSIX float for history ranking.

    Returns:
        Modification time as a POSIX float, or 0.0 when the backend omits both keys.
    """
    match info.get("mtime", info.get("created", 0.0)):
        case int() | float() as value:
            return float(value)
        case datetime() as value:
            return value.timestamp()
        case _:
            return 0.0


def size_from_info(info: dict[str, object], *, fallback: int = 0) -> int:
    """Coerce fsspec size metadata to an int for artifact byte counts.

    Returns:
        File size as an int, or fallback when the backend omits or non-ints the key.
    """
    match info.get("size", fallback):
        case int() as value:
            return value
        case _:
            return fallback


def safe_segment(part: str | UPath) -> str:
    """Admit one artifact path segment, rejecting traversal, separators, and NUL bytes.

    Returns:
        The validated segment text.

    Raises:
        ValueError: When the segment is absolute, empty, dotted, multi-piece, or NUL-bearing.
    """
    text = str(part).replace("\\", "/")
    pieces = tuple(p for p in text.split("/") if p)
    match (any(p in {".", ".."} for p in pieces), len(pieces) == 1 and text == pieces[0], "\x00" in text):
        case (False, True, False):
            return text
        case _:
            raise ValueError(f"unsafe artifact path segment: {text!r}")


def unframe(payload: bytes) -> bytes:
    """Inflate a zstd-framed payload, passing an unframed payload through unchanged.

    The frame-magic prefix discriminates history frames from plain artifacts, so a single store-owned read boundary
    serves both compressed history and uncompressed SARIF/coverage payloads with no per-consumer codec knowledge.

    Returns:
        The inflated bytes for a zstd frame, or the original bytes when the magic prefix is absent.
    """
    return _HISTORY_DECOMPRESSOR.decompress(payload) if payload[:4] == zstandard.FRAME_HEADER else payload


def _root_parts(root: str) -> tuple[str, ...]:
    return tuple(part for part in root.split("/") if part)


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)  # ruff:ignore[too-many-public-methods]
class ArtifactStore:
    """Validated artifact filesystem boundary."""

    fs: ArtifactFileSystem
    root: str

    def path(self, *parts: str | UPath) -> str:
        """Build a validated store-relative backend path.

        Returns:
            Backend path under the store root.
        """
        return "/".join((self.root, *(safe_segment(p) for p in parts)))

    def ensure(self, *parts: str) -> str:
        """Create and return a directory under the store root.

        Returns:
            Store-relative directory path.
        """
        path = self.path(*parts)
        self.fs.makedirs(path, exist_ok=True)
        return path

    def ensure_path(self, path: str | UPath) -> str:
        """Create a directory at a backend path previously emitted by this store.

        Returns:
            The validated, created backend path.
        """
        validated = self.backend_path(path)
        self.fs.makedirs(validated, exist_ok=True)
        return validated

    def glob(self, pattern: str) -> tuple[str, ...]:
        """Expand a glob under the store root.

        Returns:
            Matching store paths.
        """
        return tuple(self._normalize_backend_path(path) for path in sorted(self.fs.glob(f"{self.root}/{pattern}")))

    def walk(self, *parts: str | UPath, recursive: bool = False, detail: bool = False) -> tuple[str, ...] | tuple[tuple[str, dict[str, object]], ...]:
        """List children under a store-relative path, optionally recursive and with metadata.

        Returns:
            Matching backend paths, or (path, metadata) rows when detail is set; empty tuple when the path is absent.
        """
        base = self.path(*parts)
        try:
            rows = self.fs.find(base, detail=detail) if recursive else self.fs.ls(base, detail=detail)
        except FileNotFoundError:
            return ()
        match (detail, rows):
            case (False, list() as paths):
                return tuple(self._normalize_backend_path(str(path)) for path in sorted(str(p) for p in paths))
            case (True, dict() as keyed):
                return tuple(sorted((self._normalize_backend_path(str(path)), dict(info)) for path, info in keyed.items()))
            case (True, list() as details):
                return tuple(
                    sorted((self._normalize_backend_path(str(row.get("name", row.get("path", "")))), row) for row in details if isinstance(row, dict))
                )
            case _:
                return ()

    def info(self, *parts: str | UPath) -> dict[str, object]:
        """Return backend metadata for a store-relative path."""
        return self.fs.info(self.path(*parts))

    def info_path(self, path: str | UPath) -> dict[str, object]:
        """Return backend metadata for a previously emitted backend path."""
        return self.fs.info(self.backend_path(path))

    def exists_path(self, path: str | UPath) -> bool:
        """Return whether a previously emitted backend path exists."""
        return bool(self.fs.exists(self.backend_path(path)))

    def size_path(self, path: str | UPath, *, fallback: int = 0) -> int:
        """Return a backend path size as an int.

        Returns:
            File size, or fallback when the backend omits size metadata.
        """
        return size_from_info(self.info_path(path), fallback=fallback)

    def mtime_path(self, path: str | UPath, *, fallback: float = 0.0) -> float:
        """Return a backend path modification time as a float.

        Returns:
            Modification time, or fallback when the backend omits mtime metadata.
        """
        value = self.info_path(path).get("mtime", fallback)
        return value if isinstance(value, int | float) else fallback

    def backend_path(self, path: str | UPath) -> str:
        """Validate and normalize a backend path previously emitted by this store.

        Returns:
            Backend path under this store root.

        Raises:
            ValueError: When the path is outside this store root.
        """
        text = str(path).replace("\\", "/")
        normalized = self._normalize_backend_path(text)
        match normalized == self.root or normalized.startswith(f"{self.root}/"):
            case True:
                return normalized
            case False:
                raise ValueError(f"artifact path escaped store root: {text!r}")

    def _normalize_backend_path(self, path: str) -> str:
        return path.removeprefix("/") if not self.root.startswith("/") else path

    def exists(self, *parts: str) -> bool:
        """Return whether a store-relative path exists."""
        return bool(self.fs.exists(self.path(*parts)))

    def read_bytes(self, *parts: str | UPath) -> bytes:
        """Read bytes from a validated store-relative path, inflating a zstd-framed history payload.

        Returns:
            File payload bytes, inflated when the file carries the zstd frame magic.
        """
        return unframe(self.fs.cat_file(self.path(*parts)))

    def read_text(self, *parts: str | UPath, encoding: str = "utf-8", errors: str = "replace") -> str:
        """Read text from a validated store-relative path.

        Returns:
            Decoded text payload.
        """
        return self.read_bytes(*parts).decode(encoding, errors=errors)

    def read_path(self, path: str | UPath) -> bytes:
        """Read bytes from a validated backend path returned by the store, inflating a zstd-framed history payload.

        Returns:
            File payload bytes, inflated when the file carries the zstd frame magic.
        """
        return unframe(self.fs.cat_file(self.backend_path(path)))

    def read_text_path(self, path: str | UPath, encoding: str = "utf-8", errors: str = "replace") -> str:
        """Read text from a validated backend path returned by the store.

        Returns:
            Decoded text payload.
        """
        return self.read_path(path).decode(encoding, errors=errors)

    def _write_at(self, path: str, payload: bytes, *, create: bool, transaction: bool) -> str:
        parent = path.rsplit("/", 1)[0] if "/" in path else self.root
        with self.fs.transaction if transaction else contextlib.nullcontext():
            self.fs.makedirs(parent, exist_ok=True)
            if create and self.fs.exists(path):
                raise FileExistsError(path)
            with self.fs.open(path, "wb", autocommit=not transaction) as fh:
                fh.write(payload)
        return path

    def write_bytes(self, payload: bytes, *parts: str | UPath, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated store-relative path.

        ``create=True`` propagates FileExistsError from the backend when the target already exists.

        Returns:
            Backend path that received the payload.
        """
        return self._write_at(self.path(*parts), payload, create=create, transaction=transaction)

    def write_text(self, payload: str, *parts: str | UPath, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated store-relative path and return the backend path.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes(payload.encode(encoding), *parts, create=create, transaction=transaction)

    def write_bytes_path(self, payload: bytes, path: str | UPath, *, create: bool = False, transaction: bool = False) -> str:
        """Write bytes to a validated backend path emitted by this store.

        ``create=True`` propagates FileExistsError from the backend when the target already exists.

        Returns:
            Backend path that received the payload.
        """
        return self._write_at(self.backend_path(path), payload, create=create, transaction=transaction)

    def open_write(self, *parts: str | UPath) -> tuple[str, object]:
        """Open a validated store-relative backend file for incremental byte writes.

        Returns:
            Backend path and writable file object.
        """
        path = self.path(*parts)
        parent = path.rsplit("/", 1)[0] if "/" in path else self.root
        self.fs.makedirs(parent, exist_ok=True)
        return path, self.fs.open(path, "wb")

    def write_text_path(self, payload: str, path: str | UPath, *, create: bool = False, transaction: bool = False, encoding: str = "utf-8") -> str:
        """Write text to a validated backend path emitted by this store.

        Returns:
            Backend path that received the payload.
        """
        return self.write_bytes_path(payload.encode(encoding), path, create=create, transaction=transaction)

    def write_many(self, rows: tuple[tuple[bytes, tuple[str | UPath, ...]], ...], *, transaction: bool = True) -> tuple[str, ...]:
        """Write multiple payloads, each committed atomically when the backend supports transactions.

        Returns:
            Backend paths that received the payloads.
        """
        resolved = tuple((self.path(*parts), payload) for payload, parts in rows)
        return tuple(self._write_at(path, payload, create=False, transaction=transaction) for path, payload in resolved)

    def adopt_file(self, source: str | UPath | Path, *parts: str | UPath) -> str:
        """Copy a local tool-produced file into the configured artifact backend.

        Returns:
            Backend path that received the copied payload.
        """
        return self.write_bytes(Path(str(source)).read_bytes(), *parts)

    def remove(self, *parts: str | UPath, recursive: bool = False) -> object:
        """Remove a validated store-relative path.

        Returns:
            Backend-specific removal result.
        """
        return self.fs.rm(self.path(*parts), recursive=recursive)

    def remove_path(self, path: str | UPath, *, recursive: bool = False) -> object:
        """Remove a validated backend path returned by the store.

        Returns:
            Backend-specific removal result.
        """
        return self.fs.rm(self.backend_path(path), recursive=recursive)

    def write_history(self, run_id: str, payload: bytes) -> str:
        """Persist one encoded Envelope in run history under a zstd frame.

        Returns:
            Backend path written.
        """
        return self.write_bytes(_HISTORY_COMPRESSOR.compress(payload), ArtifactKind.HISTORY.value, run_id, "envelope.json")

    def write_full_report(self, run_id: str, name: str, report: Report) -> tuple[str, int]:
        """Persist an unclipped Report under a zstd frame before previewing or clipping.

        Returns:
            Backend path and uncompressed payload byte count.
        """
        payload = wire_encode(report)
        return self.write_bytes(_HISTORY_COMPRESSOR.compress(payload), ArtifactKind.HISTORY.value, run_id, name), len(payload)

    def _sorted_run_ids(self, root: str) -> tuple[str, ...]:
        detailed = self.walk(root, detail=True)
        detail_rows = tuple(
            (row[0].rstrip("/").rsplit("/", 1)[-1], row[1]) for row in detailed if isinstance(row, tuple) and isinstance(row[1], dict)
        )
        rows = detail_rows or tuple((path.rstrip("/").rsplit("/", 1)[-1], {}) for path in self.glob(f"{root}/*"))
        return tuple(run_id for run_id, _ in sorted(rows, key=lambda row: (mtime_from_info(row[1]), row[0])))

    def _prune(self, root: str, keep: int) -> None:
        runs = self._sorted_run_ids(root)
        for run_id in runs[: max(0, len(runs) - keep)]:
            try:
                self.remove(root, run_id, recursive=True)
            except FileNotFoundError:
                _ = self.exists(root, run_id)

    def sorted_history_ids(self) -> tuple[str, ...]:
        """Return history run ids ranked oldest-first by (mtime, run_id) within the history root.

        Backends that omit mtime fold to 0.0, leaving the lexicographic run id as the chronological tiebreaker.

        Returns:
            Run ids ordered oldest-first.
        """
        return self._sorted_run_ids(ArtifactKind.HISTORY.value)

    def retain_history(self, keep: int) -> None:
        """Prune old run history by backend metadata age, keeping the newest keep runs."""
        self._prune(ArtifactKind.HISTORY.value, keep)

    def retain_scopes(self, claim: Claim, keep: int) -> None:
        """Prune old per-claim scope run-dirs by backend metadata age, keeping the newest keep runs.

        Mirrors retain_history over a claim root to bound per-rail scope accumulation.
        """
        self._prune(claim.value, keep)

    def retain_builds(self, keep: int) -> None:
        """Prune old build-closure scope dirs by backend metadata age, keeping the newest keep closures.

        Covers stable `build/<closure>/<config>` dirs outside history and per-claim scope roots.
        """
        self._prune(_BUILD, keep)

    def load_history(self, run_id: str) -> Envelope | None:
        """Load one run Envelope and restore its full report artifact when available.

        Returns:
            Restored Envelope, or None when the run cannot be read.
        """
        match run_id:
            case "":
                return None
            case _:
                try:
                    env = _ENVELOPE_DECODER.decode(self.read_bytes(ArtifactKind.HISTORY.value, run_id, "envelope.json"))
                    return self.restore_full_report(env)
                except OSError, msgspec.MsgspecError:
                    return None

    def restore_full_report(self, env: Envelope) -> Envelope:
        """Replace a clipped report with its full-report artifact when present.

        Returns:
            Envelope with the full report restored when possible.
        """
        match env.report:
            case None:
                return env
            case report:
                artifact = next((a for a in report.artifacts if a.id == "full-report"), None)
                if artifact is None:
                    return env
                try:
                    restored = _REPORT_DECODER.decode(self.read_path(artifact.path))
                except OSError, msgspec.MsgspecError:
                    return env
                return msgspec.structs.replace(env, report=restored)

    def resolve_artifacts(self, token: str, *roots: str, latest: bool = False) -> tuple[str, ...]:
        """Resolve a token through direct paths, basenames, substring matches, or explicit latest lookup.

        Returns:
            Matching backend paths ordered newest-first.
        """
        try:
            direct = self.backend_path(token)
        except ValueError:
            direct = ""
        if direct and self.exists_path(direct):
            return (direct,)
        if latest:
            return next((ranked for root in roots if (ranked := self._ranked_files(root))), ())
        if not token.strip():
            return ()
        stem = token.rsplit("/", 1)[-1]
        return self._ranked_files(*roots, accept=lambda path: token in {path, stem} or token in path)

    def _ranked_files(self, *roots: str, accept: Callable[[str], bool] = lambda _path: True) -> tuple[str, ...]:
        def rows(root: str) -> tuple[tuple[str, dict[str, object]], ...]:
            try:
                found = self.fs.find(self.path(*_root_parts(root)), detail=True)
            except FileNotFoundError:
                return ()
            return tuple((self._normalize_backend_path(str(path)), dict(info)) for path, info in (found.items() if isinstance(found, dict) else ()))

        matches = tuple((path, info) for root in roots for path, info in rows(root) if accept(path))
        return tuple(path for path, _ in sorted(matches, key=lambda row: (-mtime_from_info(row[1]), row[0])))


@dataclass(frozen=True, slots=True)
class ArtifactScope:
    """Scoped .NET artifact directory and command flags."""

    store: ArtifactStore
    path: str
    dotnet_flags: tuple[str, ...]

    @classmethod
    def open(cls, settings: AssaySettings, claim: Claim) -> ArtifactScope:
        """Open a per-run artifact scope for a claim.

        Returns:
            Artifact scope rooted under the claim and run id.
        """
        store = settings.store()
        path = store.path(claim.value, settings.run_id)
        return cls(store, path, (_ARTIFACTS_PATH_FLAG, path))

    @classmethod
    def build(cls, settings: AssaySettings, closure: str, configuration: str | None = None) -> ArtifactScope:
        """Open a stable build-closure artifact scope, pre-seeding the cold ``DOTNET_CLI_HOME`` first-run sentinels.

        A ``Configuration`` member passes as its string value (StrEnum); ``None`` reads the settings configuration.

        Returns:
            Artifact scope rooted under the build closure id.
        """
        store = settings.store(protocol="file", root=str(settings.local_root / _DOTNET_ROOT))
        path = store.ensure(_BUILD, closure, str(configuration) if configuration else settings.configuration.value)
        scope = cls(store, path, (_ARTIFACTS_PATH_FLAG, path))
        scope._preseed_dotnet_first_run(settings.dotnet_sdk_version)
        return scope

    def _preseed_dotnet_first_run(self, sdk_version: str) -> None:
        marker = f"{self.path}/dotnet-cli/.dotnet/{sdk_version}.{_DOTNET_FIRST_RUN_SENTINELS[0]}"
        if not sdk_version or self.store.exists_path(marker):
            return
        for suffix in _DOTNET_FIRST_RUN_SENTINELS:
            self.store.write_bytes_path(b"", f"{self.path}/dotnet-cli/.dotnet/{sdk_version}.{suffix}")

    def ensure(self) -> str:
        """Materialize this scope's directory through the store boundary.

        Returns:
            The created scope path.
        """
        return self.store.ensure_path(self.path)

    @property
    def dotnet_env(self) -> dict[str, str]:
        """Build .NET command environment isolation variables.

        VBCSCompiler shared compilation stays on; the per-closure ``--artifacts-path`` is the isolation boundary because
        the build-server pipe identity is per-user and per-SDK, so concurrent closures share servers without cross-talk.
        ``DOTNET_NOLOGO``/``DOTNET_CLI_TELEMETRY_OPTOUT`` pair with the pre-seeded sentinels to silence the first-run primer.
        """
        return {"DOTNET_CLI_HOME": f"{self.path}/dotnet-cli", "DOTNET_NOLOGO": "1", "DOTNET_CLI_TELEMETRY_OPTOUT": "1"}

    @property
    def sarif_dir(self) -> str:
        """Scope-local SARIF drop directory consumed by the CspSarifDir-conditioned analyzer ErrorLog.

        Materialized eagerly because Roslyn `/errorlog` does not create parent directories.
        """
        return self.store.ensure_path(f"{self.path}/sarif")


# --- [OPERATIONS] -----------------------------------------------------------------------


def prune_python_artifacts(root: Path, keep: int) -> None:
    """Bound Python heavy-lane artifact accumulation under the repo root, keeping the newest ``keep`` benchmark autosaves.

    Only the benchmark autosave tree accumulates across runs; the coverage files and the mutmut work tree self-overwrite,
    so the bound applies solely to ``PY_ARTIFACT_ROOTS['benchmarks']``.

    Args:
        root: Local repository root that anchors the relative heavy-lane roots.
        keep: Maximum number of benchmark autosave files to retain, oldest pruned first.
    """
    benchmarks = root / PY_ARTIFACT_ROOTS["benchmarks"]
    if not benchmarks.is_dir():
        return
    files = sorted((p for p in benchmarks.rglob("*") if p.is_file()), key=lambda p: p.stat().st_mtime)
    for stale in files[: max(0, len(files) - keep)]:
        try:
            stale.unlink()
        except OSError:
            _ = stale.exists()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ArtifactFileSystem",
    "ArtifactScope",
    "ArtifactStore",
    "DOTNET_ARTIFACT_ROOTS",
    "DOTNET_BUILD_CLOSURE",
    "PY_ARTIFACT_ROOTS",
    "PY_COVERAGE_FILES",
    "mtime_from_info",
    "prune_python_artifacts",
    "safe_segment",
    "size_from_info",
    "unframe",
]
