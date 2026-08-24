"""Filesystem image, emission, freshness, and transaction inputs for contracts generation."""

from dataclasses import dataclass
import difflib
import os
from pathlib import Path, PurePosixPath
from shutil import copy2, copytree, rmtree
import stat

from expression import Error, Ok, Result

from assay.core.model import Fault
from assay.core.transaction import audit_image, CLOSE_ON_EXEC, DIRECTORY_OPEN_FLAGS, FILE_READ_FLAGS, NO_FOLLOW


# --- [TYPES] ----------------------------------------------------------------------------

type Emission = tuple[str, bytes]


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class GenerationImage:
    """One repo-shaped stage holding exactly the generated roots and derived files a generation may commit."""

    root: Path
    roots: tuple[str, ...]

    def sources(self, repo: Path, files: tuple[str, ...]) -> Result[tuple[tuple[Path, Path], ...], Fault]:
        """Project staged sources and committed targets for the transaction.

        Returns:
            Ordered source-target pairs, or a fault naming absent staged inputs.
        """
        paths = (*self.roots, *files)
        if any(not _relative(path) for path in paths) or len(set(paths)) != len(paths) or _pairwise_overlap(paths):
            return Error(Fault(("contracts", "generate"), message="staged transaction source scope is unsafe"))
        rows = tuple((self.root / rel, repo / rel) for rel in paths)
        missing = tuple(
            str(source.relative_to(self.root))
            for index, (source, _) in enumerate(rows)
            if not _source_kind(source, directory=index < len(self.roots))
        )
        return Error(Fault(("contracts", "generate"), message=f"staged transaction inputs are absent: {', '.join(missing)}")) if missing else Ok(rows)

    def read(self, rel: str) -> Result[bytes, Fault]:
        """Read one exact regular file from the staged image without following filesystem aliases.

        Returns:
            File bytes, or a fault retaining the unsafe staged image for diagnosis.
        """
        if not _relative(rel):
            return Error(Fault(("contracts", "generate"), message=f"unsafe staged read path: {rel}"))
        try:
            return Ok(_read(self.root, rel))
        except OSError as exc:
            return Error(Fault(("contracts", "generate"), message=str(exc)[:1024]))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PATHS]


def _relative(path: str) -> bool:
    pure = PurePosixPath(path)
    return bool(pure.parts) and path == pure.as_posix() and not pure.is_absolute() and not any(part in {"", ".", ".."} for part in pure.parts)


def _overlap(left: Path | PurePosixPath, right: Path | PurePosixPath) -> bool:
    return left == right or left.is_relative_to(right) or right.is_relative_to(left)


def _pairwise_overlap(paths: tuple[str, ...]) -> bool:
    pure = tuple(PurePosixPath(path) for path in paths)
    return any(_overlap(left, right) for index, left in enumerate(pure) for right in pure[index + 1 :])


def _source_kind(path: Path, *, directory: bool) -> bool:
    try:
        mode = os.lstat(path).st_mode
    except OSError:
        return False
    return stat.S_ISDIR(mode) if directory else stat.S_ISREG(mode)


def _status(path: Path) -> os.stat_result | None:
    try:
        return os.lstat(path)
    except FileNotFoundError:
        return None


# --- [DESCRIPTORS]


def _open_root(base: Path) -> int:
    descriptor = os.open(base, DIRECTORY_OPEN_FLAGS)
    if not stat.S_ISDIR(os.fstat(descriptor).st_mode):
        os.close(descriptor)
        raise OSError(f"generation image root is not a directory: {base}")
    return descriptor


def _open_child(parent: int, name: str, *, create: bool) -> int:
    try:
        return os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=parent)
    except FileNotFoundError:
        if not create:
            raise
        os.mkdir(name, 0o755, dir_fd=parent)
        return os.open(name, DIRECTORY_OPEN_FLAGS, dir_fd=parent)
    except OSError as exc:
        raise OSError(f"generation path parent is not an alias-free directory: {name}") from exc


def _open_parent(base: Path, rel: str, *, create: bool) -> tuple[int, str]:
    descriptor = _open_root(base)
    try:
        for part in PurePosixPath(rel).parts[:-1]:
            child = _open_child(descriptor, part, create=create)
            os.close(descriptor)
            descriptor = child
    except BaseException:
        os.close(descriptor)
        raise
    return descriptor, PurePosixPath(rel).name


def _read(base: Path, rel: str) -> bytes:
    parent, name = _open_parent(base, rel, create=False)
    try:
        status = os.stat(name, dir_fd=parent, follow_symlinks=False)
        if not stat.S_ISREG(status.st_mode) or status.st_nlink != 1:
            raise OSError(f"generation file is not an unaliased regular file: {base / rel}")
        descriptor = os.open(name, FILE_READ_FLAGS, dir_fd=parent)
        try:
            opened = os.fstat(descriptor)
            if not stat.S_ISREG(opened.st_mode) or opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
                raise OSError(f"generation file changed while opened: {base / rel}")
            chunks: list[bytes] = []
            while chunk := os.read(descriptor, 1 << 20):
                chunks.append(chunk)
            return b"".join(chunks)
        finally:
            os.close(descriptor)
    finally:
        os.close(parent)


# --- [STAGE]


def _compose(repo: Path, generated: Path, stage: Path, output_roots: tuple[str, ...], owned_files: tuple[str, ...]) -> None:
    # The stage mirrors the owned set alone: this run's clean Buf trees, plus each derived file seeded from its committed bytes
    # so an emission that rewrites nothing still hands the transaction a complete source.
    stage_status = _status(stage)
    if stage_status is not None:
        if not stat.S_ISDIR(stage_status.st_mode):
            raise OSError(f"generation stage is not a directory: {stage}")
        rmtree(stage)
    stage.mkdir(parents=True)
    for rel in (*output_roots, *owned_files):
        (stage / rel).parent.mkdir(parents=True, exist_ok=True)
    for out in output_roots:
        copytree(generated / out, stage / out, symlinks=True)
    for rel in owned_files:
        if _status(repo / rel) is not None:
            copy2(repo / rel, stage / rel, follow_symlinks=False)


# --- [EMISSION]


def _opened_bytes(descriptor: int, status: os.stat_result, path: Path) -> bytes:
    opened = os.fstat(descriptor)
    if not stat.S_ISREG(opened.st_mode) or opened.st_nlink != 1 or (opened.st_dev, opened.st_ino) != (status.st_dev, status.st_ino):
        raise OSError(f"generation emission target changed while opened: {path}")
    chunks: list[bytes] = []
    while chunk := os.read(descriptor, 1 << 20):
        chunks.append(chunk)
    return b"".join(chunks)


def _replace_payload(descriptor: int, payload: bytes, path: Path) -> None:
    os.ftruncate(descriptor, 0)
    os.lseek(descriptor, 0, os.SEEK_SET)
    offset = 0
    while offset < len(payload):
        written = os.write(descriptor, payload[offset:])
        if written <= 0:
            raise OSError(f"generation emission made no progress: {path}")
        offset += written


def _open_emission(parent: int, name: str, path: Path) -> tuple[int, bytes | None]:
    try:
        status = os.stat(name, dir_fd=parent, follow_symlinks=False)
    except FileNotFoundError:
        descriptor = os.open(name, os.O_RDWR | os.O_CREAT | os.O_EXCL | CLOSE_ON_EXEC | NO_FOLLOW, 0o644, dir_fd=parent)
        return descriptor, None
    if not stat.S_ISREG(status.st_mode) or status.st_nlink != 1:
        raise OSError(f"generation emission target is not an unaliased regular file: {path}")
    descriptor = os.open(name, os.O_RDWR | CLOSE_ON_EXEC | NO_FOLLOW, dir_fd=parent)
    try:
        payload = _opened_bytes(descriptor, status, path)
    except BaseException:
        os.close(descriptor)
        raise
    return descriptor, payload


def _render_one(base: Path, rel: str, payload: bytes) -> bool:
    parent, name = _open_parent(base, rel, create=True)
    try:
        descriptor, before = _open_emission(parent, name, base / rel)
        try:
            differs = before != payload
            if differs:
                _replace_payload(descriptor, payload, base / rel)
            return differs
        finally:
            os.close(descriptor)
    finally:
        os.close(parent)


def _render(base: Path, emissions: tuple[Emission, ...]) -> list[tuple[str, bool]]:
    return [(rel, _render_one(base, rel, payload)) for rel, payload in emissions]


# --- [INVENTORY]


def _scan(directory: int, prefix: PurePosixPath, files: set[str], unsafe: set[str]) -> list[tuple[int, PurePosixPath]]:
    children: list[tuple[int, PurePosixPath]] = []
    with os.scandir(directory) as entries:
        for entry in entries:
            rel = (prefix / entry.name).as_posix()
            if entry.is_dir(follow_symlinks=False):
                try:
                    children.append((os.open(entry.name, DIRECTORY_OPEN_FLAGS, dir_fd=directory), prefix / entry.name))
                except OSError:
                    unsafe.add(rel)
            elif entry.is_file(follow_symlinks=False):
                status = entry.stat(follow_symlinks=False)
                (files if status.st_nlink == 1 else unsafe).add(rel)
            else:
                unsafe.add(rel)
    return children


def _inventory(base: Path) -> tuple[frozenset[str], frozenset[str]]:
    try:
        descriptor = _open_root(base)
    except FileNotFoundError:
        return frozenset(), frozenset()
    except OSError:
        return frozenset(), frozenset({"."})
    files: set[str] = set()
    unsafe: set[str] = set()
    pending = [(descriptor, PurePosixPath())]
    while pending:
        directory, prefix = pending.pop()
        try:
            pending.extend(_scan(directory, prefix, files, unsafe))
        finally:
            os.close(directory)
    return frozenset(files), frozenset(unsafe)


def _diff(committed: Path, fresh: Path, rel: str) -> tuple[str, ...]:
    try:
        before = _read(committed, rel).decode().splitlines(keepends=True)
        after = _read(fresh, rel).decode().splitlines(keepends=True)
    except OSError, UnicodeDecodeError:
        return (f"--- {rel}\n+++ {rel}\n@@ binary or unreadable @@\n",)
    return tuple(difflib.unified_diff(before, after, fromfile=f"committed/{rel}", tofile=f"scratch/{rel}", n=1))


# --- [COMPOSITION] ----------------------------------------------------------------------


def compose_image(
    repo: Path, generated: Path, stage: Path, output_roots: tuple[str, ...], owned_files: tuple[str, ...]
) -> Result[GenerationImage, Fault]:
    """Overlay this run's clean Buf output and seed every derived file it rewrites into one repo-shaped stage.

    The owned set is the whole transaction scope: nothing a package merely contains — a package-manager link farm, a build
    output, a compiler cache — is imaged, copied, or swapped, so no artifact outside the generation can refuse the commit.

    Returns:
        Complete staged image, or a path/copy fault with no committed mutation.
    """
    owned = (*output_roots, *owned_files)
    invalid = tuple(path for path in owned if not _relative(path))
    stage_scopes = tuple(generated / path for path in output_roots) + tuple(repo / path for path in owned_files)
    detail = (
        f"unsafe paths: {', '.join(invalid)}"
        if invalid
        else "generation owned paths repeat or overlap"
        if len(set(owned)) != len(owned) or _pairwise_overlap(owned)
        else "generation roots must be canonical absolute paths"
        if any(not path.is_absolute() or "." in path.parts or ".." in path.parts for path in (repo, generated, stage))
        else "generation stage overlaps an input or committed target"
        if any(_overlap(stage, source) for source in stage_scopes)
        else ""
    )
    if detail:
        return Error(Fault(("contracts", "generate"), message=detail))
    roots = tuple(generated / out for out in output_roots)
    absent = tuple(str(source) for source in roots if not _source_kind(source, directory=True))
    if absent:
        return Error(Fault(("contracts", "generate"), message=f"generation input root is absent or unsafe: {', '.join(absent)}"))
    # A derived file absent from the estate stays absent here: its own emission rule names the repair as a finding, and the
    # transaction never reaches a commit while one stands.
    for source in (*roots, *(repo / rel for rel in owned_files if _status(repo / rel) is not None)):
        audited = audit_image(source, ("contracts", "generate"))
        if audited.is_error():
            return Error(audited.error)
    try:
        _compose(repo, generated, stage, output_roots, owned_files)
    except OSError as exc:
        rmtree(stage, ignore_errors=True)
        return Error(Fault(("contracts", "generate"), message=str(exc)[:1024]))
    audited = audit_image(stage, ("contracts", "generate"))
    if audited.is_error():
        rmtree(stage, ignore_errors=True)
        return Error(audited.error)
    return Ok(GenerationImage(root=stage, roots=output_roots))


def render(base: Path, emissions: tuple[Emission, ...]) -> Result[tuple[tuple[str, bool], ...], Fault]:
    """Write complete derived bytes into the staged image and report which staged shells changed.

    Returns:
        Relative paths with change bits, or a path/write fault.
    """
    invalid = tuple(path for path, _ in emissions if not _relative(path))
    paths = tuple(path for path, _ in emissions)
    detail = (
        f"unsafe emission paths: {', '.join(invalid)}"
        if invalid
        else "generation emission paths repeat or overlap"
        if len(set(paths)) != len(paths) or _pairwise_overlap(paths)
        else ""
    )
    if detail:
        return Error(Fault(("contracts", "generate"), message=detail))
    try:
        changed = _render(base, emissions)
    except OSError as exc:
        return Error(Fault(("contracts", "generate"), message=str(exc)[:1024]))
    return Ok(tuple(changed))


def changes(base: Path, emissions: tuple[Emission, ...]) -> Result[tuple[tuple[str, bool], ...], Fault]:
    """Compare derived bytes with committed unaliased files without mutating either image.

    Returns:
        Relative paths with change bits, or a fault for an unsafe committed path.
    """
    invalid = tuple(path for path, _ in emissions if not _relative(path))
    paths = tuple(path for path, _ in emissions)
    if invalid or len(set(paths)) != len(paths) or _pairwise_overlap(paths):
        return Error(Fault(("contracts", "generate"), message="committed comparison paths are unsafe"))
    rows: list[tuple[str, bool]] = []
    try:
        for rel, payload in emissions:
            try:
                different = _read(base, rel) != payload
            except FileNotFoundError:
                different = True
            rows.append((rel, different))
    except OSError as exc:
        return Error(Fault(("contracts", "generate"), message=str(exc)[:1024]))
    return Ok(tuple(rows))


def tree(base: Path) -> frozenset[str]:
    """Return every file below a generated root as a relative path."""
    return _inventory(base)[0]


def freshness_rows(repo: Path, scratch: Path, roots: tuple[str, ...], diff_lines: int) -> tuple[tuple[tuple[str, str], ...], str]:
    """Classify changed, missing, and orphan files across generated roots and return a bounded unified diff.

    Returns:
        Ordered stale rows and a clipped unified diff.
    """
    stale: list[tuple[str, str]] = []
    diff: list[str] = []
    for root in roots:
        committed, fresh = repo / root, scratch / root
        (have, have_unsafe), (want, want_unsafe) = _inventory(committed), _inventory(fresh)
        changed: list[str] = []
        unsafe = set(have_unsafe | want_unsafe)
        for rel in sorted(have & want):
            try:
                if _read(committed, rel) != _read(fresh, rel):
                    changed.append(rel)
            except OSError:
                unsafe.add(rel)
        stale.extend(("special", f"{root}/{rel}") for rel in sorted(unsafe))
        stale.extend(
            (kind, f"{root}/{rel}")
            for kind, rows in (("changed", changed), ("missing", sorted(want - have)), ("orphan", sorted(have - want)))
            for rel in rows
        )
        diff.extend(line for rel in changed for line in _diff(committed, fresh, rel))
    clipped = "".join(diff[:diff_lines]) + ("... diff clipped\n" if len(diff) > diff_lines else "")
    return tuple(stale), clipped


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("changes", "compose_image", "Emission", "freshness_rows", "GenerationImage", "render", "tree")
