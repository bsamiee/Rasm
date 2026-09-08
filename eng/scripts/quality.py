"""Run the checkers or the writers of every file kind in a scope, each token a kind word or a path and an empty scope the tree."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Sequence
from pathlib import Path, PurePosixPath
from typing import Literal

import anyio
import cyclopts
from expression import Error, Ok, Result
import msgspec
import structlog

from eng.scripts.provision import ChecksFailed, exit_code, Failure, message, repository_root, run, ScopeUnknown

# --- [TYPES] ----------------------------------------------------------------------------

type Mode = Literal["lint", "format"]


class Step(msgspec.Struct, frozen=True, gc=False):
    """Command words that take their files last, over the files of the kind that match one of the globs."""

    words: tuple[str, ...]
    globs: tuple[str, ...] = ("**",)


class Kind(msgspec.Struct, frozen=True, gc=False):
    """Globs of the files of one kind, its steps per mode, and the globs of the files it excludes."""

    globs: tuple[str, ...]
    steps: frozendict[Mode, tuple[Step, ...]] = frozendict()
    excluded: tuple[str, ...] = ()


class Command(msgspec.Struct, frozen=True, gc=False):
    """Step words with the files in scope they read, the words alone name the command in a failure."""

    words: tuple[str, ...]
    files: tuple[str, ...]


class Ran(msgspec.Struct, frozen=True, gc=False):
    """Files a run selected and the commands it executed over them."""

    mode: Mode
    files: int
    commands: int


# --- [CONSTANTS] ------------------------------------------------------------------------

# Steps over every file in scope, before the kind steps
_TREE = Kind(
    ("**",),
    frozendict({
        "lint": (Step(("typos", "--force-exclude")), Step(("ast-grep", "scan", "--no-ignore", "hidden"))),
        "format": (Step(("typos", "--write-changes", "--force-exclude")),),
    }),
)
_KINDS: frozendict[str, Kind] = frozendict({
    "dotnet": Kind(
        ("**/*.cs", "**/*.csproj", "**/*.props", "**/*.targets", "**/*.slnx"),
        frozendict({"format": (Step(("dotnet", "format", "Workspace.slnx", "--no-restore", "--include"), ("**/*.cs",)),)}),
    ),
    "python": Kind(
        ("**/*.py", "**/*.pyi"),
        frozendict({"lint": (Step(("ruff", "check")),), "format": (Step(("ruff", "check", "--fix")), Step(("ruff", "format")))}),
    ),
    "typescript": Kind(
        ("**/*.ts", "**/*.tsx", "**/*.mts", "**/*.cts", "**/*.json", "**/*.jsonc"),
        frozendict({
            "lint": (Step(("biome", "check", "--error-on-warnings")),),
            "format": (Step(("biome", "check", "--write", "--error-on-warnings")),),
        }),
    ),
    "shell": Kind(("**/*.sh",), frozendict({"lint": (Step(("shellcheck", "--shell=bash", "--enable=all")),), "format": (Step(("shfmt", "-w")),)})),
    "yaml": Kind(
        ("**/*.yaml", "**/*.yml"),
        frozendict({
            "lint": (
                Step(("yamllint",)),
                Step(("actionlint",), (".github/workflows/**",)),
                Step(("zizmor", "--offline"), (".github/workflows/**", ".github/actions/**")),
            ),
            "format": (Step(("yamlfmt", "-no_global_conf", "-formatter", "indent=4,retain_line_breaks=true")),),
        }),
        ("**/__snapshots__/**", "**/pnpm-lock.yaml"),  # Generated YAML keeps its generator's layout
    ),
    "sql": Kind(("**/*.sql",), frozendict({"lint": (Step(("sqlfluff", "lint")),), "format": (Step(("pg-formatter", "--inplace")),)})),
    "markdown": Kind(("**/*.md",)),
})

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="quality")

# --- [SCOPE] ----------------------------------------------------------------------------


def _matches(file: str, globs: Sequence[str]) -> bool:
    """Return whether a file matches one of the globs over its whole path."""
    return any(PurePosixPath(file).full_match(glob) for glob in globs)


def _selects(selector: Kind | PurePosixPath, file: str) -> bool:
    """Return whether a selector takes a file, a kind by its globs and a path as the file or one of its ancestors."""
    match selector:
        case Kind():
            return _matches(file, selector.globs) and not _matches(file, selector.excluded)
        case PurePosixPath():
            return PurePosixPath(file).is_relative_to(selector)


def scope(root: Path, files: Sequence[str], tokens: Sequence[str]) -> Result[list[str], ScopeUnknown]:
    """Select the files in the union of the tokens, every file for no token, and collect every unknown token."""
    if unknown := tuple(token for token in tokens if token not in _KINDS and not (root / token).exists()):
        return Error(ScopeUnknown(unknown))
    selectors = [_KINDS.get(token, PurePosixPath(token)) for token in tokens] or [_TREE]
    return Ok([file for file in files if any(_selects(selector, file) for selector in selectors)])


# --- [COMMANDS] -------------------------------------------------------------------------


def commands(files: Sequence[str], mode: Mode) -> list[Command]:
    """Return the commands of a mode in table order, each over the files its step reads, and none for a step without files."""
    return [
        Command(step.words, selected)
        for kind in (_TREE, *_KINDS.values())
        for step in kind.steps.get(mode, ())
        if (selected := tuple(file for file in files if _selects(kind, file) and _matches(file, step.globs)))
    ]


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _listing(root: Path) -> Result[list[str], Failure]:
    """List the tracked and untracked files that exist on disk, the index still names a deleted file."""
    listed = await run(("git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"), root, capture=True)
    return listed.map(lambda output: [file for file in output.split("\0") if file and (root / file).exists()])


async def _each(root: Path, planned: Sequence[Command]) -> Result[int, ChecksFailed]:
    """Run every command in order on the inherited console and collect the words of the ones that failed."""
    failed = [" ".join(command.words) for command in planned if (await run((*command.words, *command.files), root)).is_error()]
    return Error(ChecksFailed(tuple(failed))) if failed else Ok(len(planned))


async def _quality(root: Path, mode: Mode, tokens: Sequence[str]) -> Result[Ran, Failure]:
    """Select the files of the scope and run the commands of the mode over them."""
    match (await _listing(root)).bind(lambda files: scope(root, files, tokens)):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=selected):
            return (await _each(root, commands(selected, mode))).map(lambda count: Ran(mode, len(selected), count))


def _report(ran: Ran) -> None:
    """Log the file and command counts of the run."""
    _log.info(ran.mode, files=ran.files, commands=ran.commands)


_app.result_action = (exit_code(_report, message), "sys_exit")


@_app.default
def main(mode: Mode, *tokens: str) -> Result[Ran, Failure]:
    """Run the checkers (lint) or the writers (format) of every file kind in the scope, a kind word or a path per token and the tree for none."""
    return repository_root(Path(__file__).resolve()).bind(lambda root: anyio.run(_quality, root, mode, tokens))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Mode", "Step", "Kind", "Command", "Ran", "scope", "commands", "main"]
