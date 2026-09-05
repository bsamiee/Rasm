"""Merge the coverage data the tests of one language left into the reports of that language."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
from typing import Literal

import anyio
import cyclopts
from expression import Result
import msgspec
import structlog

from eng.scripts.provision import exit_code, Failure, repository_root, run_each

# --- [TYPES] ----------------------------------------------------------------------------

type Language = Literal["dotnet", "python", "typescript"]


class Merged(msgspec.Struct, frozen=True, gc=False):
    """Data files a merge read, zero when the tests of the language left none."""

    language: Language
    files: int


class _Merge(msgspec.Struct, frozen=True, gc=False):
    """Glob of the data files the tests of a language write and the commands that merge them."""

    data: str
    commands: tuple[tuple[str, ...], ...]


# --- [CONSTANTS] ------------------------------------------------------------------------

_DOTNET_REPORTS = ".artifacts/dotnet/coverage/*/*.cobertura*.xml"
_VITEST_BLOBS = ".artifacts/typescript/test-results/.vitest-reports/@rasm"
_REPORT_GENERATOR = ("dotnet", "dnx", "dotnet-reportgenerator-globaltool", "--yes")
_LANGUAGES: dict[Language, _Merge] = {
    "dotnet": _Merge(
        _DOTNET_REPORTS,
        (
            (
                *_REPORT_GENERATOR,
                f"-reports:{_DOTNET_REPORTS}",
                "-targetdir:.artifacts/dotnet/coverage",
                "-reporttypes:Cobertura;lcov;MarkdownSummaryGithub",
            ),
        ),
    ),
    # The reporting commands combine the parallel data files, an explicit combine fails on empty input
    "python": _Merge(".artifacts/python/coverage/data/.coverage*", (("uv", "run", "coverage", "lcov"), ("uv", "run", "coverage", "xml"))),
    "typescript": _Merge(f"{_VITEST_BLOBS}/*.json", (("pnpm", "exec", "vitest", "run", "--merge-reports", _VITEST_BLOBS),)),
}

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="coverage")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _merge(root: Path, language: Language) -> Result[Merged, Failure]:
    """Run the merge commands of a language when its tests left data files."""
    merge = _LANGUAGES[language]
    files = len([path async for path in anyio.Path(root).glob(merge.data)])
    commands = merge.commands if files else ()
    return (await run_each(commands, root)).map(lambda _: Merged(language, files))


def _report(merged: Merged) -> None:
    """Log the merged data file count, or the absence of data."""
    _log.info("merged" if merged.files else "no data", language=merged.language, files=merged.files)


_app.result_action = (exit_code(_report), "sys_exit")


@_app.default
def main(*, language: Language) -> Result[Merged, Failure]:
    """Merge the coverage of one language, a language without data files is reported and exits 0."""
    return repository_root(Path(__file__).resolve()).bind(lambda found: anyio.run(_merge, found, language))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Language", "Merged", "main"]
