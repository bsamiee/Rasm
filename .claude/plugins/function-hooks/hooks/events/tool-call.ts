import type { ToolCallInput } from 'claude-code';
import { type Command, parse, type Scanner } from '../command.ts';
import { type Decision, deny, fold, none, type Option, type Policy, type Result, some, when } from '../composition.ts';
import { gitPaths, gitPolicy, type WorktreeEvent, worktreePolicy } from '../policies/git.ts';
import { type PathEvent, pathPolicy } from '../policies/paths.ts';
import { scriptPolicy } from '../policies/script.ts';
import { stdinPolicy } from '../policies/stdin.ts';
import { waitPolicy } from '../policies/wait.ts';
import { type Place, walkPolicy } from '../policies/walk.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };
type Exists = (path: string) => Promise<boolean>;
type Locate = Option<() => Promise<Place>>;

interface Facts {
    readonly parsed: Result<readonly Command[]>;
    readonly existing: readonly string[];
    readonly place: Option<Place>;
}

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isPath = (e: ToolCallInput): e is PathEvent => e.tool === 'Write';
const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';
const _isWorktree = (e: ToolCallInput): e is WorktreeEvent => e.tool === 'Agent' || e.tool === 'EnterWorktree';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _facts = (exists: Exists, locate: Locate, parsed: Promise<Result<readonly Command[]>>): Promise<Facts> =>
    Promise.all([parsed, locate.kind === 'some' ? locate.value().then(some) : none]).then(([known, place]) =>
        Promise.all((known.kind === 'ok' ? gitPaths(known.value) : []).map((path) => exists(path).then((found) => (found ? [path] : [])))).then((existing) => ({
            parsed: known,
            existing: existing.flat(),
            place,
        })),
    );

const _policies = (e: Commanded, { existing, place }: Facts, commands: readonly Command[]): readonly Policy<Commanded>[] => [
    gitPolicy(commands, existing),
    stdinPolicy(commands),
    waitPolicy(commands),
    ...(e.tool === 'Bash' ? [scriptPolicy(commands), ...(place.kind === 'some' ? [walkPolicy(commands, place.value)] : [])] : []),
];

const _decide = (e: Commanded, facts: Facts): Decision<Commanded> =>
    facts.parsed.kind === 'fault' ? deny(`command not parsed, ${facts.parsed.reason}`) : fold(_policies(e, facts, facts.parsed.value))(e);

// --- [DECISION] ------------------------------------------------------------------------

const decide = (e: ToolCallInput, scan: Scanner, exists: Exists, locate: Locate): Promise<Decision<ToolCallInput>> =>
    _hasCommand(e) ? _facts(exists, locate, parse(scan, e.command)).then((facts) => _decide(e, facts)) : Promise.resolve(fold([when(_isPath, pathPolicy), when(_isWorktree, worktreePolicy)])(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide };
