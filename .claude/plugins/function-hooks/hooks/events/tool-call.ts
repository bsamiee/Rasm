// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, fold, type Policy, when } from '../composition/decision.ts';
import type { Result } from '../composition/result.ts';
import { gitPaths, gitPolicy } from '../policies/git.ts';
import { type PathEvent, pathPolicy } from '../policies/paths.ts';
import { waitPolicy } from '../policies/shell.ts';
import { type Command, parse, type Scanner } from '../text/command.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

type Exists = (path: string) => Promise<boolean>;

interface Facts {
    readonly parsed: Result<readonly Command[]>;
    readonly existing: readonly string[];
}

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isPath = (e: ToolCallInput): e is PathEvent => e.tool === 'Write';

const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _paths = (parsed: Result<readonly Command[]>): readonly string[] => (parsed.kind === 'ok' ? gitPaths(parsed.value) : []);

const _seed = (parsed: Result<readonly Command[]>): Facts => ({ parsed, existing: [] });

const _found =
    (exists: Exists, path: string) =>
    (facts: Facts): Promise<Facts> =>
        exists(path).then((found) => (found ? { ...facts, existing: [...facts.existing, path] } : facts));

const _walk = (exists: Exists, seed: Promise<Facts>, paths: readonly string[]): Promise<Facts> =>
    paths.reduce((chain, path) => chain.then(_found(exists, path)), seed);

const _grow = (exists: Exists, parsed: Promise<Result<readonly Command[]>>, seed: Promise<Facts>): Promise<Facts> =>
    parsed.then((known) => _walk(exists, seed, _paths(known)));

const _facts = (exists: Exists, parsed: Promise<Result<readonly Command[]>>): Promise<Facts> => _grow(exists, parsed, parsed.then(_seed));

const _policies = (e: Commanded, commands: readonly Command[], existing: readonly string[]): readonly Policy<Commanded>[] =>
    e.tool === 'Bash' ? [gitPolicy(commands, existing), waitPolicy(commands)] : [gitPolicy(commands, existing)];

const _decide = (e: Commanded, facts: Facts): Decision<Commanded> =>
    facts.parsed.kind === 'fault' ? deny(`command not parsed, ${facts.parsed.reason}`) : fold(_policies(e, facts.parsed.value, facts.existing))(e);

// --- [DECISION] ------------------------------------------------------------------------

const decide = async (e: ToolCallInput, scan: Scanner, exists: Exists): Promise<Decision<ToolCallInput>> =>
    _hasCommand(e) ? _facts(exists, parse(scan, e.command)).then((facts) => _decide(e, facts)) : when(_isPath, pathPolicy)(e);

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide };
