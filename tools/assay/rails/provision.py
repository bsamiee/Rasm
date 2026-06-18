"""Run local provisioning infrastructure through the Forge-owned CLI."""

from dataclasses import dataclass
from typing import override

from expression import Result  # noqa: TC002  # beartype resolves handler return annotations at registry runtime
from expression.collections import block
from expression.extra.result import sequence

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # registry runtime resolves handler annotations
from tools.assay.core.engine import fan_out
from tools.assay.core.model import BaseParams, Check, Claim, Fault, fold, Input, Language, Mode, Report, Runner, Tool  # noqa: TC001
from tools.assay.core.routing import Routed, Scope


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ProvisionParams(BaseParams):
    """Parameters for Forge-owned Rasm provisioning commands."""

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


# --- [CONSTANTS] ------------------------------------------------------------------------

_ROUTED: Routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_PYTHON_ABI_PROBE: str = "import sys, sysconfig; print(sys.implementation.cache_tag, sysconfig.get_config_var('Py_GIL_DISABLED') or 0)"
_ONNXRUNTIME_LIB_PROBE: str = 'test -n "${ONNXRUNTIME_LIB:-}" && test -e "$ONNXRUNTIME_LIB" && printf "%s\\n" "$ONNXRUNTIME_LIB"'


# --- [OPERATIONS] -----------------------------------------------------------------------


def _tool(name: str, argv: tuple[str, ...], *, mode: Mode = Mode.RUN, timeout: float = 120.0) -> Tool:
    return Tool(
        name=name, runner=Runner.DIRECT, command=argv, input=Input.NONE, language=Language.PYTHON, claim=Claim.PROVISION, mode=mode, timeout=timeout
    )


def _run(settings: AssaySettings, scope: ArtifactScope, verb: str, tools: tuple[Tool, ...]) -> Result[Report, Fault]:
    outcomes = sequence(block.of_seq(fan_out(tuple(Check(tool=tool) for tool in tools), settings=settings, scope=scope, routed=_ROUTED)))
    return outcomes.map(lambda done: fold(Claim.PROVISION, verb, tuple(done), promote_empty=True))


def _stack(command: str, *, timeout: float = 120.0) -> Tool:
    mode = Mode.VERIFY if command.startswith("verify") else Mode.RUN
    return _tool(f"rasm-provision-{command}", ("rasm-provision", command), mode=mode, timeout=timeout)


def up(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Start the Forge-owned provisioning services and verify their extensions.

    Returns:
        Provision report, or a provisioning fault.
    """
    return _run(settings, scope, "up", (_stack("up", timeout=300.0),))


def down(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Stop labelled provisioning services and remove script-owned data.

    Returns:
        Provision report, or a provisioning fault.
    """
    return _run(settings, scope, "down", (_stack("down"),))


def status(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report Docker Compose status for the provisioning services.

    Returns:
        Provision report, or a provisioning fault.
    """
    return _run(settings, scope, "status", (_stack("status"),))


def env(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Emit generated paths and DSNs for the provisioning services.

    Returns:
        Provision report, or a provisioning fault.
    """
    return _run(settings, scope, "env", (_stack("env"),))


def verify(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Verify provisioning extensions and local scientific runtime probes.

    Returns:
        Provision report, or a provisioning fault.
    """
    probes = (
        _stack("verify", timeout=180.0),
        _tool("duckdb-version", ("duckdb", "--version")),
        _tool("forge-python-abi", ("forge-scientific-env", "python3", "-c", _PYTHON_ABI_PROBE)),
        _tool("forge-openblas", ("forge-scientific-env", "pkg-config", "--modversion", "openblas")),
        _tool("forge-onnxruntime-lib", ("forge-scientific-env", "sh", "-lc", _ONNXRUNTIME_LIB_PROBE)),
    )
    return _run(settings, scope, "verify", probes)


__all__ = ["ProvisionParams", "down", "env", "status", "up", "verify"]
