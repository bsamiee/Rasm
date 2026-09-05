"""Run the Stryker mutation tests of one language and fail the run when its report holds zero mutants."""

# --- [IMPORTS] --------------------------------------------------------------------------

from pathlib import Path
from typing import Literal

import anyio
import cyclopts
from expression import Error, Ok, Result
import msgspec
import structlog

from eng.scripts.provision import exit_code, Failure, FileMissing, NoMutants, repository_root, run

# --- [TYPES] ----------------------------------------------------------------------------

type Language = Literal["dotnet", "typescript"]


class Mutated(msgspec.Struct, frozen=True, gc=False):
    """Mutants a Stryker run discovered for one language."""

    language: Language
    mutants: int


class _File(msgspec.Struct, frozen=True, gc=False):
    """Mutated file of a mutation testing report, its mutants stay undecoded."""

    mutants: list[msgspec.Raw]


class _Report(msgspec.Struct, frozen=True, gc=False):
    """Mutation testing report, the files keyed by path."""

    files: dict[str, _File]


class _JsonReporter(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    """StrykerJS json reporter options, the file name is relative to the working directory."""

    file_name: str


class _Config(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    """StrykerJS configuration, read for the report path it names."""

    json_reporter: _JsonReporter


# --- [CONSTANTS] ------------------------------------------------------------------------

_DOTNET_OUTPUT = ".artifacts/dotnet/stryker"  # CLI-only option, stryker-config.json rejects keys outside its schema

_log = structlog.get_logger(__name__)
_app = cyclopts.App(name="mutation")

# --- [OPERATIONS] -----------------------------------------------------------------------


async def _mutate(root: Path, language: Language) -> Result[Mutated, Failure]:
    """Run the Stryker command of a language after removing its previous report, then count the mutants the new report holds."""
    match language:
        case "dotnet":
            command: tuple[str, ...] = ("dotnet", "dnx", "dotnet-stryker", "--yes", "--allow-roll-forward", "--output", _DOTNET_OUTPUT)
            report = root / _DOTNET_OUTPUT / "reports" / "mutation-report.json"
        case "typescript":
            command = ("pnpm", "exec", "stryker", "run")
            report = root / msgspec.json.decode((root / "stryker.config.json").read_bytes(), type=_Config).json_reporter.file_name
    report.unlink(missing_ok=True)
    match await run(command, root):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result() if not report.is_file():
            return Error(FileMissing(report.parent, report.name))
        case Result():
            mutants = sum(len(file.mutants) for file in msgspec.json.decode(report.read_bytes(), type=_Report).files.values())
            return Ok(Mutated(language, mutants)) if mutants else Error(NoMutants(language, report))


def _report(mutated: Mutated) -> None:
    _log.info("mutated", language=mutated.language, mutants=mutated.mutants)


_app.result_action = (exit_code(_report), "sys_exit")


@_app.default
def main(*, language: Language) -> Result[Mutated, Failure]:
    """Run the mutation tests of one language, a report holding zero mutants exits 1."""
    return repository_root(Path(__file__).resolve()).bind(lambda found: anyio.run(_mutate, found, language))


if __name__ == "__main__":
    _app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Language", "Mutated", "main"]
