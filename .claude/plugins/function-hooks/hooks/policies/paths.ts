// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, pass } from '../composition/decision.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type PathEvent = Extract<ToolCallInput, { readonly tool: 'Write' }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SECOND_FILES: readonly RegExp[] = [
    /^(?:project\.json|\.nxignore)$/u,
    /^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u,
    /^tsconfig\.(?!base\.json$).+\.json$/u,
    /^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u,
    /^biome\.jsonc$/u,
    /^\.yamllint(?:\.yml)?$/u,
];

// --- [RULES] ---------------------------------------------------------------------------

const pathGuard = <E extends PathEvent>(e: E): Decision<E> => {
    const name = basename(e.file_path);
    return _SECOND_FILES.some((pattern) => pattern.test(name))
        ? deny(`${name} is a second file beside its owner, put the fact in the owner`)
        : pass(e);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { PathEvent };
export { pathGuard };
