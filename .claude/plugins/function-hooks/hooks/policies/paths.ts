// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type PathEvent = Extract<ToolCallInput, { readonly tool: 'Write' }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SECOND_FILE = /^(?:project\.json|\.nxignore|\.miserc\.toml|tsconfig\.(?!base\.json$).+\.json|mise\..+\.toml)$/u;

// --- [RULES] ---------------------------------------------------------------------------

const pathGuard = <E extends PathEvent>(e: E): Decision<E> => {
    const name = basename(e.file_path);
    return _SECOND_FILE.test(name) ? deny(`${name} is a second file beside its owner, put the fact in the owner`) : rewrite(e);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { PathEvent };
export { pathGuard };
