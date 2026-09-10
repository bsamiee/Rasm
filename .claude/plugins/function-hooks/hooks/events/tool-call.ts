// --- [IMPORTS] -------------------------------------------------------------------------

import type { EngineInterface, On, ToolCallInput } from 'claude-code';
import { type Decision, deny, fold, type Rule, when } from '../composition/decision.ts';
import { gitGuard, gitPaths } from '../policies/git.ts';
import { type PathEvent, pathGuard } from '../policies/paths.ts';
import { waitGuard } from '../policies/shell.ts';
import { type Command, type Parse, parse, SCAN, type Scanner } from '../text/argv.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

interface Facts {
    readonly parsed: Parse;
    readonly existing: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CTRL = /\p{Cc}+/gu;

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isPath = (e: ToolCallInput): e is PathEvent => e.tool === 'Write';

const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _scan =
    ($: EngineInterface): Scanner =>
    (text: string): ReturnType<Scanner> =>
        $.process.run(SCAN, { stdin: text });

const _paths = (parsed: Parse): readonly string[] => (parsed.kind === 'parsed' ? gitPaths(parsed.commands) : []);

const _seed = (parsed: Parse): Facts => ({ parsed, existing: [] });

const _found =
    ($: EngineInterface, path: string) =>
    (facts: Facts): Promise<Facts> =>
        $.fs.exists(path).then((exists) => (exists ? { ...facts, existing: [...facts.existing, path] } : facts));

const _walk = ($: EngineInterface, seed: Promise<Facts>, paths: readonly string[]): Promise<Facts> =>
    paths.reduce((chain, path) => chain.then(_found($, path)), seed);

const _grow = ($: EngineInterface, parsed: Promise<Parse>, seed: Promise<Facts>): Promise<Facts> =>
    parsed.then((known) => _walk($, seed, _paths(known)));

const _facts = ($: EngineInterface, parsed: Promise<Parse>): Promise<Facts> => _grow($, parsed, parsed.then(_seed));

const _rules = (e: Commanded, commands: readonly Command[], existing: readonly string[]): readonly Rule<Commanded>[] =>
    e.tool === 'Bash' ? [gitGuard(commands, existing), waitGuard(commands)] : [gitGuard(commands, existing)];

const _decide = (e: Commanded, facts: Facts): Decision<Commanded> =>
    facts.parsed.kind === 'unparsed' ? deny(`command not parsed, ${facts.parsed.reason}`) : fold(_rules(e, facts.parsed.commands, facts.existing))(e);

const _answer = <E extends ToolCallInput, R>(decision: Decision<E>, next: (e: E) => R): R | { readonly deny: string } =>
    decision.kind === 'deny' ? { deny: decision.reason.replace(_CTRL, ' ') } : next(decision.e);

// --- [REGISTRATION] --------------------------------------------------------------------

const toolCall = (on: On): void => {
    on('tool.call', ($, e, next) =>
        _hasCommand(e)
            ? _facts($, parse(_scan($), e.command)).then((facts) => _answer(_decide(e, facts), next))
            : _answer(when(_isPath, pathGuard)(e), next),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolCall };
