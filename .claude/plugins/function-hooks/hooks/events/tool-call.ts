// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Command, parse, type Scanner } from '../command.ts';
import { type Decision, deny, fold, type Policy, type Result, when } from '../composition.ts';
import { gitPaths, gitPolicy, type WorktreeEvent, worktreePolicy } from '../policies/git.ts';
import { type PathEvent, pathPolicy } from '../policies/paths.ts';
import { scriptPolicy } from '../policies/script.ts';
import { waitPolicy } from '../policies/shell.ts';
import { type Place, walkPolicy } from '../policies/walk.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

interface Facts {
    readonly parsed: Result<readonly Command[]>;
    readonly existing: readonly string[];
    readonly place: Place;
}

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isPath = (e: ToolCallInput): e is PathEvent => e.tool === 'Write';

const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

const _isWorktree = (e: ToolCallInput): e is WorktreeEvent => e.tool === 'Agent' || e.tool === 'EnterWorktree';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _step = (exists: (path: string) => Promise<boolean>, facts: Facts, path: string): Promise<Facts> =>
    exists(path).then((found) => (found ? { ...facts, existing: [...facts.existing, path] } : facts));

const _walk = (exists: (path: string) => Promise<boolean>, facts: Facts, path: string, rest: readonly string[]): Promise<Facts> =>
    rest.reduce((chain, next) => chain.then((known) => _step(exists, known, next)), _step(exists, facts, path));

const _gathered = (exists: (path: string) => Promise<boolean>, place: Place, parsed: Promise<Result<readonly Command[]>>): Promise<Facts> =>
    parsed.then((known) => {
        const facts: Facts = { parsed: known, existing: [], place };
        const [head, ...tail] = known.kind === 'ok' ? gitPaths(known.value) : [];
        return head === undefined ? facts : _walk(exists, facts, head, tail);
    });

const _facts = (exists: (path: string) => Promise<boolean>, locate: () => Promise<Place>, parsed: Promise<Result<readonly Command[]>>): Promise<Facts> =>
    locate().then((place) => _gathered(exists, place, parsed));

const _decide = (e: Commanded, facts: Facts, walking: boolean): Decision<Commanded> => {
    if (facts.parsed.kind === 'fault') {
        return deny(`command not parsed, ${facts.parsed.reason}`);
    }
    const commands = facts.parsed.value;
    const policies: readonly Policy<Commanded>[] = [
        gitPolicy(commands, facts.existing),
        ...(e.tool === 'Bash' ? [waitPolicy(commands), scriptPolicy(commands), ...(walking ? [walkPolicy(commands, facts.place)] : [])] : []),
    ];
    return fold(policies)(e);
};

// --- [DECISION] ------------------------------------------------------------------------

const decide = async (e: ToolCallInput, scan: Scanner, exists: (path: string) => Promise<boolean>, locate: () => Promise<Place>, walking: boolean): Promise<Decision<ToolCallInput>> =>
    _hasCommand(e) ? _facts(exists, locate, parse(scan, e.command)).then((facts) => _decide(e, facts, walking)) : fold([when(_isPath, pathPolicy), when(_isWorktree, worktreePolicy)])(e);

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide };
