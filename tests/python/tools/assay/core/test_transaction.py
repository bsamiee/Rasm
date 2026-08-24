"""Laws for recovery-journalled replacement of a complete staged image."""


# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import os
from pathlib import Path

import msgspec
import pytest

from assay.core.model import RailStatus
import assay.core.transaction as transaction_mod
from assay.core.transaction import audit_image, SwapTransaction
from tests.python._testkit.spec import assert_error_status, assert_ok


COVERS: tuple[object, ...] = (SwapTransaction, audit_image)


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEAD_PID = 99_999_999


# --- [OPERATIONS] -----------------------------------------------------------------------


def _file(path: Path, value: bytes) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(value)
    return path


def _fifo(path: Path) -> None:
    create = getattr(os, "mkfifo", None)
    assert callable(create)
    create(path)


def _transaction(root: Path, *targets: Path) -> SwapTransaction:
    return SwapTransaction(marker=root / ".transaction.json", argv=("test", "transaction"), targets=targets)


def _identity(path: Path) -> transaction_mod._Identity:
    return transaction_mod._identity(os.lstat(path))


def _artifact(path: Path) -> transaction_mod._Artifact:
    return transaction_mod._Artifact(identity=_identity(path), content=transaction_mod._content(transaction_mod._snapshot(path)))


# --- [INSTALL_ATOMICITY]


def test_install_copy_failure_leaves_every_target_unchanged(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    source_dir = tmp_path / "image/package"
    target_dir = tmp_path / "live/package"
    _file(source_dir / "generated/new.txt", b"new")
    _file(target_dir / "generated/old.txt", b"old")
    source_file = _file(tmp_path / "image/schema.json", b"schema-new")
    target_file = _file(tmp_path / "live/schema.json", b"schema-old")

    original = transaction_mod.copy2

    def copy(source: Path, target: Path, *, follow_symlinks: bool = True) -> Path:
        if source == source_file:
            raise OSError("second copy refused")
        return Path(original(source, target, follow_symlinks=follow_symlinks))

    monkeypatch.setattr(transaction_mod, "copy2", copy)
    fault = assert_error_status(
        _transaction(tmp_path, target_dir, target_file).install(((source_dir, target_dir), (source_file, target_file))), RailStatus.FAULTED
    )

    assert "second copy refused" in fault.message
    assert (target_dir / "generated/old.txt").read_bytes() == b"old"
    assert target_file.read_bytes() == b"schema-old"


def test_multi_target_commit_replaces_every_target_and_clears_journal(tmp_path: Path) -> None:
    targets = (_file(tmp_path / "a", b"old-a"), _file(tmp_path / "b", b"old-b"))
    sources = (_file(tmp_path / "image/a", b"new-a"), _file(tmp_path / "image/b", b"new-b"))
    transaction = _transaction(tmp_path, *targets)

    assert_ok(transaction.install(tuple(zip(sources, targets, strict=True))))

    assert tuple(target.read_bytes() for target in targets) == (b"new-a", b"new-b")
    assert not transaction.marker.exists()


def test_install_durability_barriers_bracket_prepared_renames_and_committed_cleanup(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    target = _file(tmp_path / "target", b"old")
    source = _file(tmp_path / "image/source", b"new")
    transaction = _transaction(tmp_path, target)
    events: list[str] = []
    original_write = SwapTransaction._write

    def sync_path(_path: Path, _directories: transaction_mod._Directories) -> None:
        events.append("stage-synced")

    original_sync = transaction_mod._Directories.sync

    def sync(directories: transaction_mod._Directories, *parents: Path) -> None:
        events.append("parents-synced")
        original_sync(directories, *parents)

    def write(owner: SwapTransaction, journal: transaction_mod._Journal, directories: transaction_mod._Directories, *, create: bool = False) -> None:
        events.append(f"journal-{journal.phase}")
        original_write(owner, journal, directories, create=create)

    monkeypatch.setattr(transaction_mod, "_sync_path", sync_path)
    monkeypatch.setattr(transaction_mod._Directories, "sync", sync)
    monkeypatch.setattr(SwapTransaction, "_write", write)

    assert_ok(transaction.install(((source, target),)))

    assert events.index("journal-staging") < events.index("stage-synced") < events.index("journal-prepared")
    assert events[events.index("journal-prepared") - 1] == "parents-synced"
    assert events[events.index("journal-committed") - 1] == "parents-synced"
    assert events[-1] == "parents-synced"


def test_install_preserves_symlinks_without_following_them(tmp_path: Path) -> None:
    source = tmp_path / "image/package"
    source.mkdir(parents=True)
    _file(source / "value", b"payload")
    (source / "alias").symlink_to("value")
    target = tmp_path / "live/package"
    target.parent.mkdir(parents=True)
    transaction = _transaction(tmp_path, target)

    assert_ok(transaction.install(((source, target),)))

    assert (target / "alias").is_symlink() and (target / "alias").readlink() == Path("value")


# --- [SWAP_ROLLBACK]


def test_late_swap_failure_rolls_every_prior_target_back(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    targets = (_file(tmp_path / "a", b"old-a"), _file(tmp_path / "b", b"old-b"))
    sources = (_file(tmp_path / "image/a", b"new-a"), _file(tmp_path / "image/b", b"new-b"))
    transaction = _transaction(tmp_path, *targets)
    swaps = tuple(
        (_file(target.with_name(f"{target.name}.staged.{os.getpid()}.{index}"), source.read_bytes()), target)
        for index, (source, target) in enumerate(zip(sources, targets, strict=True))
    )
    original = os.replace
    refused = swaps[1]

    def replace(source: str, target: str, *, src_dir_fd: int | None = None, dst_dir_fd: int | None = None) -> None:
        if source == refused[0].name and target == refused[1].name:
            raise OSError("second swap refused")
        original(source, target, src_dir_fd=src_dir_fd, dst_dir_fd=dst_dir_fd)

    monkeypatch.setattr(transaction_mod.os, "replace", replace)
    fault = assert_error_status(transaction.commit(swaps), RailStatus.FAULTED)

    assert "second swap refused" in fault.message
    assert tuple(target.read_bytes() for target in targets) == (b"old-a", b"old-b")
    assert not transaction.marker.exists()
    assert all(not staged.exists() for staged, _ in swaps)


# --- [DEAD_RECOVERY]


def test_dead_prepared_transaction_rolls_the_whole_image_back(tmp_path: Path) -> None:
    targets = (_file(tmp_path / "a", b"old-a"), _file(tmp_path / "b", b"old-b"))
    sources = (_file(tmp_path / "image/a", b"new-a"), _file(tmp_path / "image/b", b"new-b"))
    transaction = _transaction(tmp_path, *targets)
    entries = tuple(
        transaction_mod._Entry(
            staged=str(_file(target.with_name(f"{target.name}.staged.{_DEAD_PID}.{index}"), source.read_bytes())),
            target=str(target),
            previous=str(target.with_name(f"{target.name}.previous.{_DEAD_PID}.{index}")),
            original=_artifact(target),
        )
        for index, (source, target) in enumerate(zip(sources, targets, strict=True))
    )
    entries = tuple(msgspec.structs.replace(entry, image=_artifact(Path(entry.staged))) for entry in entries)
    first = entries[0]
    Path(first.target).replace(first.previous)
    Path(first.staged).replace(first.target)
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=entries))
    )

    assert assert_ok(transaction.recover()) == "back"
    assert tuple(target.read_bytes() for target in targets) == (b"old-a", b"old-b")
    assert not transaction.marker.exists()
    assert all(not Path(entry.previous).exists() for entry in entries)


def test_dead_committed_transaction_keeps_the_whole_image_and_cleans_backups(tmp_path: Path) -> None:
    targets = (_file(tmp_path / "a", b"new-a"), _file(tmp_path / "b", b"new-b"))
    transaction = _transaction(tmp_path, *targets)
    entries: list[transaction_mod._Entry] = []
    for index, target in enumerate(targets):
        previous = _file(target.with_name(f"{target.name}.previous.{_DEAD_PID}.{index}"), f"old-{index}".encode())
        entries.append(
            transaction_mod._Entry(
                staged=str(target.with_name(f"{target.name}.staged.{_DEAD_PID}.{index}")),
                target=str(target),
                previous=str(previous),
                original=_artifact(previous),
                image=_artifact(target),
            )
        )
    journal_entries = tuple(entries)
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.COMMITTED, entries=journal_entries))
    )

    assert assert_ok(transaction.recover()) == "forward"
    assert tuple(target.read_bytes() for target in targets) == (b"new-a", b"new-b")
    assert not transaction.marker.exists()
    assert all(not Path(entry.previous).exists() for entry in journal_entries)


def test_live_transaction_is_busy_and_preserves_every_shape(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    target = _file(tmp_path / "target", b"old")
    source = _file(tmp_path / "image/source", b"new")
    transaction = _transaction(tmp_path, target)
    staged = _file(tmp_path / "target.staged.42.0", source.read_bytes())
    entry = transaction_mod._Entry(
        staged=str(staged), target=str(target), previous=str(tmp_path / "target.previous.42.0"), original=_artifact(target), image=_artifact(staged)
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=42, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=(entry,)))
    )
    monkeypatch.setattr(transaction_mod, "proc_identity_dead", lambda _pid, _created: False)

    fault = assert_error_status(transaction.recover(), RailStatus.BUSY)

    assert "42" in fault.message
    assert target.read_bytes() == b"old" and staged.read_bytes() == b"new" and transaction.marker.is_file()


# --- [JOURNAL_INTEGRITY]


def test_corrupt_journal_fails_closed_without_touching_targets(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    transaction = _transaction(tmp_path, target)
    transaction.marker.write_bytes(b"{not json")

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)
    assert "undecodable transaction journal retained" in fault.message
    assert target.read_bytes() == b"old" and transaction.marker.exists()


def test_journal_with_unowned_fields_fails_closed_without_aliasing_semantics(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    transaction = _transaction(tmp_path, target)
    journal = transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.STAGING, entries=())
    payload = msgspec.json.decode(msgspec.json.encode(journal))
    assert isinstance(payload, dict)
    payload["existed"] = True
    transaction.marker.write_bytes(msgspec.json.encode(payload))

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "undecodable transaction journal retained" in fault.message
    assert target.read_bytes() == b"old" and transaction.marker.exists()


def test_decodable_journal_cannot_escape_the_admitted_target_scope(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    escaped = _file(tmp_path / "escaped", b"keep")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(tmp_path / f"escaped.staged.{_DEAD_PID}.0"),
        target=str(escaped),
        previous=str(tmp_path / f"escaped.previous.{_DEAD_PID}.0"),
        original=_artifact(escaped),
        image=_artifact(escaped),
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=(entry,)))
    )

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "unsafe transaction journal retained" in fault.message
    assert target.read_bytes() == b"old" and escaped.read_bytes() == b"keep" and transaction.marker.exists()


def test_journal_symlink_is_retained_without_following_its_payload(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    outside = _file(tmp_path / "outside", b"not-a-journal")
    transaction = _transaction(tmp_path, target)
    transaction.marker.symlink_to(outside.name)

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "unsafe transaction journal retained" in fault.message
    assert transaction.marker.is_symlink() and outside.read_bytes() == b"not-a-journal" and target.read_bytes() == b"old"


def test_existing_journal_is_an_atomic_claim_and_is_never_overwritten(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    source = _file(tmp_path / "image/source", b"new")
    transaction = _transaction(tmp_path, target)
    transaction.marker.write_bytes(b"owner-evidence")

    fault = assert_error_status(transaction.install(((source, target),)), RailStatus.FAULTED)

    assert "journal already exists" in fault.message
    assert transaction.marker.read_bytes() == b"owner-evidence" and target.read_bytes() == b"old"


# --- [RECOVERY_GUARDS]


def test_prepared_recovery_never_deletes_an_unrelated_new_target(tmp_path: Path) -> None:
    target = tmp_path / "target"
    staged = _file(tmp_path / f"target.staged.{_DEAD_PID}.0", b"staged")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(staged), target=str(target), previous=str(tmp_path / f"target.previous.{_DEAD_PID}.0"), original=None, image=_artifact(staged)
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=(entry,)))
    )
    _file(target, b"external")

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "replaced externally" in fault.message
    assert target.read_bytes() == b"external" and staged.read_bytes() == b"staged" and transaction.marker.exists()


def test_prepared_recovery_refuses_a_replaced_backup(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"new")
    staged_artifact = _artifact(target)
    previous = _file(tmp_path / f"target.previous.{_DEAD_PID}.0", b"forged-old")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(tmp_path / f"target.staged.{_DEAD_PID}.0"),
        target=str(target),
        previous=str(previous),
        original=transaction_mod._Artifact(
            identity=transaction_mod._Identity(
                device=staged_artifact.identity.device, inode=staged_artifact.identity.inode + 1, kind=staged_artifact.identity.kind
            ),
            content=b"\0" * 32,
        ),
        image=staged_artifact,
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=(entry,)))
    )

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "backup identity or content changed" in fault.message
    assert target.read_bytes() == b"new" and previous.read_bytes() == b"forged-old" and transaction.marker.exists()


def test_committed_recovery_never_invents_a_missing_target_from_a_stage(tmp_path: Path) -> None:
    target = tmp_path / "target"
    staged = _file(tmp_path / f"target.staged.{_DEAD_PID}.0", b"new")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(staged), target=str(target), previous=str(tmp_path / f"target.previous.{_DEAD_PID}.0"), original=None, image=_artifact(staged)
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.COMMITTED, entries=(entry,)))
    )

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "committed transaction target is absent" in fault.message
    assert not target.exists() and staged.read_bytes() == b"new" and transaction.marker.exists()


def test_prepared_recovery_detects_in_place_content_tampering(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"new")
    previous = _file(tmp_path / f"target.previous.{_DEAD_PID}.0", b"old")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(tmp_path / f"target.staged.{_DEAD_PID}.0"),
        target=str(target),
        previous=str(previous),
        original=_artifact(previous),
        image=_artifact(target),
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.PREPARED, entries=(entry,)))
    )
    target.write_bytes(b"tampered")

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "replaced externally" in fault.message
    assert target.read_bytes() == b"tampered" and previous.read_bytes() == b"old" and transaction.marker.exists()


def test_staging_recovery_retains_a_partial_stage_without_recorded_ownership(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    staged = _file(tmp_path / f"target.staged.{_DEAD_PID}.0", b"partial")
    transaction = _transaction(tmp_path, target)
    entry = transaction_mod._Entry(
        staged=str(staged), target=str(target), previous=str(tmp_path / f"target.previous.{_DEAD_PID}.0"), original=_artifact(target)
    )
    transaction.marker.write_bytes(
        msgspec.json.encode(transaction_mod._Journal(pid=_DEAD_PID, create_time=1.0, phase=transaction_mod._Phase.STAGING, entries=(entry,)))
    )

    fault = assert_error_status(transaction.recover(), RailStatus.FAULTED)

    assert "unowned partial" in fault.message
    assert staged.read_bytes() == b"partial" and target.read_bytes() == b"old" and transaction.marker.exists()


# --- [IMAGE_ADMISSION]


def test_install_refuses_special_hardlinked_and_escaping_images(tmp_path: Path) -> None:
    target = _file(tmp_path / "live/target", b"old")
    transaction = _transaction(tmp_path, target)
    hardlinked = _file(tmp_path / "image/hardlinked", b"new")
    os.link(hardlinked, tmp_path / "image/alias")

    hardlink_fault = assert_error_status(transaction.install(((hardlinked, target),)), RailStatus.FAULTED)
    assert "hard-linked" in hardlink_fault.message

    special = tmp_path / "special"
    special.mkdir()
    _fifo(special / "pipe")
    special_fault = assert_error_status(transaction.install(((special, target),)), RailStatus.FAULTED)
    assert "special file" in special_fault.message

    image = tmp_path / "escape"
    image.mkdir()
    _file(tmp_path / "outside", b"outside")
    (image / "outside").symlink_to("../outside")
    escape_fault = assert_error_status(transaction.install(((image, target),)), RailStatus.FAULTED)
    assert "symlink escapes" in escape_fault.message
    assert target.read_bytes() == b"old" and not transaction.marker.exists()


# --- [PATH_SCOPE]


def test_symlinked_target_parent_is_never_followed(tmp_path: Path) -> None:
    outside = tmp_path / "outside"
    outside.mkdir()
    target = _file(outside / "target", b"old")
    linked = tmp_path / "linked"
    linked.symlink_to(outside, target_is_directory=True)
    source = _file(tmp_path / "image/source", b"new")
    transaction = SwapTransaction(marker=tmp_path / ".transaction.json", argv=("test",), targets=(linked / "target",))

    fault = assert_error_status(transaction.install(((source, linked / "target"),)), RailStatus.FAULTED)

    assert "not alias-free" in fault.message
    assert target.read_bytes() == b"old" and not transaction.marker.exists()


def test_overlapping_target_scope_refuses_before_any_journal_or_mutation(tmp_path: Path) -> None:
    directory = tmp_path / "package"
    child = _file(directory / "schema", b"old")
    source_dir = tmp_path / "image/package"
    _file(source_dir / "schema", b"new")
    source_child = _file(tmp_path / "image/schema", b"new-child")
    transaction = _transaction(tmp_path, directory, child)

    fault = assert_error_status(transaction.install(((source_dir, directory), (source_child, child))), RailStatus.FAULTED)

    assert "scope is unsafe" in fault.message
    assert child.read_bytes() == b"old" and not transaction.marker.exists()


def test_auxiliary_names_cannot_alias_the_journal_or_another_target(tmp_path: Path) -> None:
    target = _file(tmp_path / "target", b"old")
    source = _file(tmp_path / "image/source", b"new")
    marker = target.with_name(f"{target.name}.staged.{os.getpid()}.0")
    transaction = SwapTransaction(marker=marker, argv=("test",), targets=(target,))

    fault = assert_error_status(transaction.install(((source, target),)), RailStatus.FAULTED)

    assert "auxiliary path scope is unsafe" in fault.message
    assert target.read_bytes() == b"old" and not marker.exists()
