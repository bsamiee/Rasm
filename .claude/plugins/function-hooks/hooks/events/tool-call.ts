// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, fold, type Rule, when } from '../composition/decision.ts';
import { gitGuard, gitPaths } from '../policies/git.ts';
import { type PathEvent, pathGuard } from '../policies/paths.ts';
import { waitGuard } from '../policies/shell.ts';
import { type Command, type Parse, parse, type Scanner } from '../text/argv.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

type Exists = (path: string) => Promise<boolean>;

interface Facts {
    readonly parsed: Parse;
    readonly existing: readonly string[];
}

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isPath = (e: ToolCallInput): e is PathEvent => e.tool === 'Write';

const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _paths = (parsed: Parse): readonly string[] => (parsed.kind === 'parsed' ? gitPaths(parsed.commands) : []);

const _seed = (parsed: Parse): Facts => ({ parsed, existing: [] });

const _found =
    (exists: Exists, path: string) =>
    (facts: Facts): Promise<Facts> =>
        exists(path).then((found) => (found ? { ...facts, existing: [...facts.existing, path] } : facts));

const _walk = (exists: Exists, seed: Promise<Facts>, paths: readonly string[]): Promise<Facts> =>
    paths.reduce((chain, path) => chain.then(_found(exists, path)), seed);

const _grow = (exists: Exists, parsed: Promise<Parse>, seed: Promise<Facts>): Promise<Facts> =>
    parsed.then((known) => _walk(exists, seed, _paths(known)));

const _facts = (exists: Exists, parsed: Promise<Parse>): Promise<Facts> => _grow(exists, parsed, parsed.then(_seed));

const _rules = (e: Commanded, commands: readonly Command[], existing: readonly string[]): readonly Rule<Commanded>[] =>
    e.tool === 'Bash' ? [gitGuard(commands, existing), waitGuard(commands)] : [gitGuard(commands, existing)];

const _decide = (e: Commanded, facts: Facts): Decision<Commanded> =>
    facts.parsed.kind === 'unparsed' ? deny(`command not parsed, ${facts.parsed.reason}`) : fold(_rules(e, facts.parsed.commands, facts.existing))(e);

// --- [DECISION] ------------------------------------------------------------------------

const decide = async (e: ToolCallInput, scan: Scanner, exists: Exists): Promise<Decision<ToolCallInput>> =>
    _hasCommand(e) ? _facts(exists, parse(scan, e.command)).then((facts) => _decide(e, facts)) : when(_isPath, pathGuard)(e);

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide };
