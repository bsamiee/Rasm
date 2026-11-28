"""File change asks over a git tree: the tree read, per-file and linked-group questions, and the commands each answer runs."""

from collections import Counter
from enum import auto, Enum
from functools import partial, reduce
from itertools import accumulate, chain
from pathlib import Path
import shlex

from expression import Ok
import msgspec

from form._card import Asked, Box, Offer, overflow, row_form, Rows, Source, spanned
from form._show import Binary, decoded, diff, Document, endpoints, git, patched, ran, read
from form._spec import Change, Changes, Files, gathered, HEADER_UNITS, History, Loc, Outcome, PICK, quoted, Row, Span

# --- [TYPES] ----------------------------------------------------------------------------


class Placement(Enum):
    COPY = auto()
    TREE = auto()
    HISTORY = auto()


type Verdict = tuple[Row, Row]

# --- [CONSTANTS] ------------------------------------------------------------------------

_VERDICTS: frozendict[tuple[Placement, Change], Verdict] = frozendict({
    (Placement.COPY, Change.ADDED): (("Save", "Writes the file"), ("Skip", "Writes nothing")),
    (Placement.COPY, Change.MODIFIED): (("Apply edit", "Writes the change"), ("Leave as is", "Changes nothing")),
    (Placement.COPY, Change.DELETED): (("Delete", "Deletes the file"), ("Keep", "Keeps the file")),
    (Placement.TREE, Change.ADDED): (("Keep", "Keeps the new file"), ("Undo", "Deletes the file")),
    (Placement.TREE, Change.MODIFIED): (("Keep", "Keeps the edit"), ("Undo", "Restores the file as of the base")),
    (Placement.TREE, Change.DELETED): (("Delete", "Leaves the file deleted"), ("Restore", "Restores the file")),
    (Placement.HISTORY, Change.ADDED): (("Keep", "Leaves the commits as they are"), ("Revert", "Stages the file's removal for a commit")),
    (Placement.HISTORY, Change.MODIFIED): (("Keep", "Leaves the commits as they are"), ("Revert", "Stages the file as of the base for a commit")),
    (Placement.HISTORY, Change.DELETED): (("Keep", "Leaves the commits as they are"), ("Revert", "Stages the file's return for a commit")),
})
_QUESTIONS: frozendict[tuple[Placement, Change], str] = frozendict({
    (Placement.COPY, Change.ADDED): "Save new {noun} {path}?",
    (Placement.COPY, Change.MODIFIED): "Apply this edit to {noun} {path}?",
    (Placement.COPY, Change.DELETED): "Delete {noun} {path}?",
    (Placement.TREE, Change.ADDED): "Keep new {noun} {path}?",
    (Placement.TREE, Change.MODIFIED): "Keep this edit to {noun} {path}?",
    (Placement.TREE, Change.DELETED): "Keep {noun} {path} deleted?",
    (Placement.HISTORY, Change.ADDED): "Keep new {noun} {path} from {range}?",
    (Placement.HISTORY, Change.MODIFIED): "Keep the edit to {noun} {path} from {range}?",
    (Placement.HISTORY, Change.DELETED): "Keep the deletion of {noun} {path} from {range}?",
})
_GROUP: frozendict[Placement, tuple[str, str]] = frozendict({
    Placement.COPY: ("saved", "Writes nothing"),
    Placement.TREE: ("undone", "Keeps every file"),
    Placement.HISTORY: ("reverted", "Keeps every file"),
})


# --- [MODELS] ---------------------------------------------------------------------------


class Filed(Document, frozen=True, kw_only=True):
    change: Change


type Step = tuple[Asked, tuple[int, str] | None]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def _listed(change: Change, raw: bytes, /) -> tuple[tuple[Change, str], ...]:
    return tuple((change, path) for path in raw.decode().split("\0") if path)


def _tracked(ask: Files, at: Loc, span: Span, /) -> Outcome[tuple[tuple[Change, str], ...]]:
    base, tip = span
    listings = (
        ran(
            at, git(ask.changes, "diff", "--name-only", "-z", "--no-renames", "--relative", f"--diff-filter={change}", "--end-of-options", base, *((tip,) if tip is not None else ()), "--", *ask.paths)
        ).map(partial(_listed, change))
        for change in Change
    )
    untracked = () if tip is not None else (ran(at, git(ask.changes, "ls-files", "-z", "--others", "--exclude-standard", "--", *ask.paths)).map(partial(_listed, Change.ADDED)),)
    return gathered((*listings, *untracked)).map(lambda parts: tuple(chain.from_iterable(parts)))


def _filed(change: Change, found: Document | Binary, /) -> Filed | Binary:
    match found:
        case Binary():
            return found
        case Document():
            return Filed(**msgspec.structs.asdict(found), change=change)


def _fetched(ask: Files, span: Span, at: Loc, listed: tuple[Change, str], /) -> Outcome[Filed | Binary]:
    (base, tip), (change, path) = span, listed
    match change, tip:
        case Change.DELETED, _:
            command = f"git show {base[:9]}:{path}"
            raw = ran(at, git(ask.changes, "show", f"{base}:./{path}"))
        case _, str():
            command = f"git show {tip[:9]}:{path}"
            raw = ran(at, git(ask.changes, "show", f"{tip}:./{path}"))
        case _:
            command = str((ask.changes / path).resolve())
            raw = read(ask.changes / path, at)
    return raw.bind(partial(decoded, Path(path), Source(shown=path, full=command), at)).map(partial(_filed, change))


# --- [COMMANDS]


def _placed(ask: Files, /) -> Placement:
    match ask:
        case History():
            return Placement.HISTORY
        case Changes(into=None):
            return Placement.TREE
        case Changes():
            return Placement.COPY


def _commands(ask: Files, span: Span, filed: Filed, /) -> tuple[tuple[str, ...], tuple[str, ...]]:
    path, tree = filed.path.as_posix(), ask.changes / filed.path
    restored = f"{shlex.join(git(ask.changes, 'show', f'{span[0]}:./{path}'))} > {shlex.quote(str(tree))} && {shlex.join(git(ask.changes, 'add', '--', path))}"
    made = () if tree.parent.is_dir() else (shlex.join(("mkdir", "-p", "--", str(tree.parent))),)
    match ask, filed.change:
        case Changes(into=Path() as into), Change.ADDED:
            parent = (into / filed.path).parent
            return (" && ".join((*(() if parent.is_dir() else (shlex.join(("mkdir", "-p", "--", str(parent))),)), shlex.join(("cp", "--", str(tree), str(into / filed.path))))),), ()
        case Changes(into=Path() as into), Change.MODIFIED:
            return (f"{shlex.join((*diff(ask.changes, span), '--', path))} | {shlex.join(git(into, 'apply'))}",), ()
        case Changes(into=Path() as into), Change.DELETED:
            return (shlex.join(("rm", "--", str(into / filed.path))),), ()
        case _, Change.ADDED:
            return (), (f"{shlex.join(git(ask.changes, 'rm', '-q', '--cached', '--ignore-unmatch', '--', path))} && {shlex.join(('rm', '-f', '--', str(tree)))}",)
        case _, Change.DELETED:
            return (), (" && ".join((*made, restored)),)
        case _:
            return (), (restored,)


# --- [QUESTIONS]


def _question(ask: Files, filed: Filed, /) -> str:
    scope = ask.range if isinstance(ask, History) else ""
    return _QUESTIONS[_placed(ask), filed.change].format(noun=ask.noun or "file", path=filed.path.as_posix(), range=scope)


def _offers(verdict: Verdict, commands: tuple[tuple[str, ...], tuple[str, ...]], /) -> tuple[Offer, Offer]:
    ((approve, does), (decline, kept)), (approving, declining) = verdict, commands
    return Offer(name=approve, does=does, recommended=True, runs=approving), Offer(name=decline, does=kept, runs=declining)


def _alone(ask: Files, span: Span, at: Loc, box: Box, filed: Filed, /) -> Outcome[Step]:
    noun, path = ask.noun or "file", filed.path.as_posix()
    rows = (Rows.of(found),) if (found := filed.rows(ask.keys)) else ()
    match filed.change:
        case Change.MODIFIED:
            command = shlex.join(("git", "diff", span[0][:9], *((span[1][:9],) if span[1] is not None else ()), "--", path))
            raw = ran(at, (*diff(ask.changes, span, "--unified=1"), "--", path))
            shown = raw.bind(lambda text: patched(text.decode(errors="replace"), at, Source(shown=command, full=command), box))
            body = shown.map(lambda blocks: (*rows, *blocks))
        case Change.ADDED | Change.DELETED:
            body = Ok((f"{spanned(path)} · {'new' if filed.change is Change.ADDED else 'deleted'} {noun}", *rows, filed.body))
    offers = _offers(_VERDICTS[_placed(ask), filed.change], _commands(ask, span, filed))
    return body.map(lambda blocks: (Asked(question=_question(ask, filed), header="", offers=offers, shared=blocks, at=at), None))


def _edges(files: tuple[Filed, ...], /) -> tuple[tuple[tuple[int, int], ...], tuple[str, ...]]:
    paths = frozendict({filed.path.as_posix(): index for index, filed in enumerate(files)})
    named = frozendict((name, tuple(index for index, filed in enumerate(files) if name in {filed.path.stem, filed.path.with_suffix("").as_posix()})) for filed in files for name in filed.names)
    near = tuple(
        (index, name, tuple(found for found in named[name] if files[found].path.parent == filed.path.parent) or named[name]) for index, filed in enumerate(files) for name in sorted(filed.names)
    )
    edges = (
        *((index, paths[link]) for index, filed in enumerate(files) for link in sorted(filed.paths) if link in paths),
        *((index, only) for index, _, found in near for only in found if len(found) == 1),
    )
    ambiguous = tuple(f"{quoted(files[index].path.as_posix())}: link {quoted(name)} matches {len(found)} files" for index, name, found in near if len(found) > 1)
    return tuple((start, end) for start, end in edges if start != end), ambiguous


def _merged(groups: tuple[frozenset[int], ...], edge: tuple[int, int], /) -> tuple[frozenset[int], ...]:
    joined = frozenset().union(*(group for group in groups if not group.isdisjoint(edge)))
    return (*(group for group in groups if group.isdisjoint(edge)), joined)


def _groups(files: tuple[Filed, ...], /) -> tuple[tuple[tuple[Filed, ...], ...], tuple[str, ...]]:
    edges, ambiguous = _edges(files)
    linking, linked = Counter(start for start, _ in edges), Counter(end for _, end in edges)
    components = reduce(_merged, edges, tuple(frozenset({index}) for index in range(len(files))))
    ordered = tuple(sorted(component, key=lambda index: (-linking[index], -linked[index], files[index].path)) for component in components if len(component) > 1)
    return tuple(tuple(files[index] for index in members) for members in sorted(ordered)), ambiguous


def _gathered(ask: Files, span: Span, at: Loc, members: tuple[Filed, ...], /) -> tuple[Step, ...]:
    hub, count = members[0].path.as_posix(), len(members)
    files = f"{count} new {f'{ask.noun} ' if ask.noun is not None else ''}files"
    placement = _placed(ask)
    (approve, _), (decline, _) = _VERDICTS[placement, Change.ADDED]
    commands = tuple(_commands(ask, span, filed) for filed in members)
    acting, dropped = _GROUP[placement]
    runs = tuple(chain.from_iterable(approving or declining for approving, declining in commands))
    joined = (" && ".join(runs),) if runs else ()
    copy = placement is Placement.COPY
    offers = (
        Offer(name=f"{approve} all", does=f"{approve}s all {count} files" if copy else f"Keeps all {count} files", recommended=True, runs=joined if copy else ()),
        Offer(name=PICK, does="Opens one checkbox per file"),
        Offer(name=f"{decline} all", does=dropped if copy else f"{decline}s all {count} files", runs=() if copy else joined),
    )
    blocks = (f"{files} · linked by {spanned(hub)}", *chain.from_iterable((Rows(sections=((filed.path.as_posix(), filed.rows(ask.keys)),)), filed.body) for filed in members))
    question = f"{approve} {files} linked by {hub}?"
    picks = tuple(
        Offer(name=filed.path.as_posix(), does=" · ".join(value for _, value in filed.rows(ask.keys)) or filed.path.name, runs=approving or declining)
        for filed, (approving, declining) in zip(members, commands, strict=True)
    )
    asked = Asked(question=question, header="", offers=offers, shared=blocks, at=at)
    pick = Asked(question=f"Which of the {files} linked by {hub} should be {acting}?", header="Pick", offers=picks, multiple=True, at=at)
    return (asked, None), (pick, (0, PICK))


def _planned(ask: Files, at: Loc, box: Box, span: Span, entries: tuple[Filed | Binary, ...], /) -> Outcome[tuple[tuple[Step, ...], tuple[str, ...]]]:
    files = tuple(entry for entry in entries if isinstance(entry, Filed))
    skipped = tuple(f"{quoted(entry.path.as_posix())}: skipped, binary {entry.size} bytes" for entry in entries if isinstance(entry, Binary))
    missing = tuple(f"keys: {quoted(key)} names no front matter leaf" for key in sorted(frozenset.intersection(*(filed.missing(ask.keys) for filed in files))) if files)
    groups, ambiguous = _groups(tuple(filed for filed in files if filed.change is Change.ADDED)) if ask.group else ((), ())
    multi = len(files) > 1
    trials = tuple((members, _gathered(ask, span, at, members)) for members in groups)
    checked = tuple(
        (members, steps, overflow(steps[0][0], row_form(row for block in steps[0][0].shared if isinstance(block, Rows) for _, section in block.sections for row in section)[0], box, multi=multi))
        for members, steps in trials
    )
    kept = tuple((members, steps) for members, steps, reason in checked if reason.is_none())
    split = tuple(f"{quoted(members[0].path.as_posix())}: group split, {reason}" for members, _, reason in checked for reason in reason.to_list())
    grouped = frozenset(filed.path for members, _ in kept for filed in members)
    loose = tuple(sorted((filed for filed in files if filed.path not in grouped), key=lambda filed: (tuple(Change).index(filed.change), filed.path)))
    singles = gathered(_alone(ask, span, at, box, filed) for filed in loose)
    word = (ask.noun or "file").capitalize()
    nothing = () if entries else (f"{quoted(str(ask.changes))}: no changes",)
    return singles.map(lambda alone: (_headed(word, (*(steps for _, steps in kept), *((step,) for step in alone))), (*skipped, *missing, *ambiguous, *split, *nothing)))


def _headed(word: str, items: tuple[tuple[Step, ...], ...], /) -> tuple[Step, ...]:
    count = len(items)
    starts = tuple(accumulate(map(len, items), initial=0))
    return tuple(
        chain.from_iterable(
            tuple((msgspec.structs.replace(asked, header=asked.header or numbered(word, index, count)), None if after is None else (start + after[0], after[1])) for asked, after in steps)
            for index, (steps, start) in enumerate(zip(items, starts, strict=False), 1)
        )
    )


def numbered(word: str, index: int, count: int, /) -> str:
    suffix = f" {index}/{count}" if count > 1 else ""
    return f"{word[: HEADER_UNITS - len(suffix)].rstrip()}{suffix}"


def changed(ask: Files, at: Loc, box: Box, /) -> Outcome[tuple[tuple[Step, ...], tuple[str, ...]]]:
    revision = ask.range if isinstance(ask, History) else None
    return endpoints(ask.changes, revision, at).bind(
        lambda span: _tracked(ask, at, span).bind(lambda listed: gathered(map(partial(_fetched, ask, span, at), listed))).bind(partial(_planned, ask, at, box, span))
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Step", "changed", "numbered"]
