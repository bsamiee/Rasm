// --- [IMPORTS] -------------------------------------------------------------------------

import type { ProcessRunResult } from 'claude-code';
import { fault, ok, type Result } from '../composition/result.ts';
import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<ProcessRunResult>;

interface Command {
    readonly words: readonly string[];
    readonly condition: boolean;
}

interface Body {
    readonly text: string;
    readonly condition: boolean;
}

interface Resolved {
    readonly commands: readonly Command[];
    readonly bodies: readonly Body[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MAX_DEPTH = 8;
const _SEPARATOR = '\u001f';
const _CONDITION =
    '{any: [{inside: {kind: while_statement, field: condition}}, {inside: {stopBy: end, inside: {kind: while_statement, field: condition}}}]}';
const _rule = (id: string, relation: string): string => `id: ${id}
language: bash
rule: {kind: command, all: [{any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}]}, ${relation}]}
rewriters: [{id: word, rule: {pattern: $W}, fix: $W}]
transform:
    ARGV: {rewrite: {source: $$$ARGS, rewriters: [word], joinBy: "\\u001f"}}
    NAME: {replace: {source: $CMD, replace: '\\n', by: ' '}}
    LINE: {replace: {source: $ARGV, replace: '\\n', by: ' '}}
message: "$NAME\\u001f$LINE"`;
const _RULES = [_rule('command', `{not: ${_CONDITION}}`), _rule('condition', _CONDITION)].join('\n---\n');
const SCAN: readonly string[] = [
    'mise',
    'exec',
    '--',
    'ast-grep',
    'scan',
    '--stdin',
    '--config',
    '/dev/null',
    '--inline-rules',
    _RULES,
    '--color',
    'never',
    '--report-style',
    'short',
];
const _REPORT = /^STDIN:\d+:\d+: \w+\[(?<rule>command|condition)\]: (?<line>.*)$/gmu;
const _QUOTED = /\$?'(?<single>[^']*)'|\$?"(?<double>(?:[^"\\]|\\.)*)"|\\(?<bare>[\s\S])/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _DIGITS = /^\d+$/u;
const _SHELLS: readonly string[] = ['sh', 'bash', 'zsh', 'dash', 'ksh'];
const _SUBCOMMANDS: readonly string[] = ['run', 'exec', 'tool', 'x'];
const _VALUE_OPTS: readonly string[] = ['-u', '-I', '-n', '-g', '--user', '--replace'];
const _WRAPPERS: readonly string[] = [
    'sudo',
    'doas',
    'env',
    'command',
    'exec',
    'nice',
    'nohup',
    'stdbuf',
    'timeout',
    'time',
    'xargs',
    'caffeinate',
    'arch',
    'setsid',
    'uv',
    'npm',
    'npx',
    'pnpm',
    'poetry',
    'hatch',
];

// --- [WORDS] ---------------------------------------------------------------------------

const _unquote = (word: string): string =>
    word.replace(
        _QUOTED,
        (_match: string, single?: string, double?: string, bare?: string): string =>
            single ?? (double === undefined ? (bare ?? '') : double.replace(_ESCAPED, '$<char>')),
    );

const _stripOptions = (words: readonly string[]): readonly string[] => {
    const [head] = words;
    if (head === undefined || !(head.includes('=') || _DIGITS.test(head) || _SUBCOMMANDS.includes(head) || head.startsWith('-'))) {
        return words;
    }
    return _stripOptions(words.slice(_VALUE_OPTS.includes(head) ? 2 : 1));
};

const pastAssignments = (words: readonly string[]): readonly string[] => {
    const index = words.findIndex((word) => !_ENV_ASSIGN.test(word));
    return index < 0 ? [] : words.slice(index);
};

const strip = (words: readonly string[]): readonly string[] => {
    const [head] = words;
    return head !== undefined && (_ENV_ASSIGN.test(head) || _WRAPPERS.includes(head)) ? strip(_stripOptions(words.slice(1))) : words;
};

const _inline = (words: readonly string[]): readonly string[] => {
    const hit = words.findIndex((word) => word.startsWith('-') && !word.startsWith('--') && word.endsWith('c'));
    return hit < 0
        ? []
        : words
              .slice(hit + 1)
              .filter((word) => word !== '--')
              .slice(0, 1);
};

// --- [RESOLUTION] ----------------------------------------------------------------------

const _texts = (name: string, words: readonly string[]): readonly string[] => {
    if (name === 'eval') {
        return [words.slice(1).join(' ')];
    }
    return _SHELLS.includes(name) ? _inline(words) : [];
};

const _resolve = (command: Command, depth: number): Resolved => {
    const words = strip(command.words);
    const texts = depth < _MAX_DEPTH ? _texts(basename(words[0] ?? ''), words) : [];
    return { commands: [command], bodies: texts.filter((text) => text !== '').map((text): Body => ({ text, condition: command.condition })) };
};

// --- [REPORT] --------------------------------------------------------------------------

const _commands = (report: string, condition: boolean): readonly Command[] =>
    [...report.matchAll(_REPORT)].map(
        (match): Command => ({
            words: (match.groups?.['line'] ?? '')
                .split(_SEPARATOR)
                .filter((word) => word !== '')
                .map(_unquote),
            condition: condition || match.groups?.['rule'] === 'condition',
        }),
    );

const _run = (scan: Scanner, text: string, condition: boolean): Promise<Result<readonly Command[]>> =>
    scan(text).then(
        ({ exitCode, stdout, stderr }): Result<readonly Command[]> =>
            exitCode === 0 && (stdout === '' || stdout.endsWith('\n'))
                ? ok(_commands(stdout, condition))
                : fault(`ast-grep exited ${exitCode} over the command, ${stderr.trim()}`),
        (cause: unknown): Result<readonly Command[]> => fault(`ast-grep did not run over the command, ${String(cause)}`),
    );

const _join = (left: Result<readonly Command[]>, right: Result<readonly Command[]>): Result<readonly Command[]> => {
    if (left.kind === 'fault') {
        return left;
    }
    return right.kind === 'fault' ? right : ok([...left.value, ...right.value]);
};

const _own = (parsed: Result<readonly Command[]>, depth: number): Result<readonly Command[]> =>
    parsed.kind === 'fault' ? parsed : ok(parsed.value.flatMap((command) => _resolve(command, depth).commands));

const _bodies = (parsed: Result<readonly Command[]>, depth: number): readonly Body[] =>
    parsed.kind === 'fault' ? [] : parsed.value.flatMap((command) => _resolve(command, depth).bodies);

const _nested =
    (scan: Scanner, body: Body, depth: number) =>
    (left: Result<readonly Command[]>): Promise<Result<readonly Command[]>> =>
        _parse(scan, body.text, depth + 1, body.condition).then((right) => _join(left, right));

const _chain = (
    scan: Scanner,
    own: Promise<Result<readonly Command[]>>,
    bodies: readonly Body[],
    depth: number,
): Promise<Result<readonly Command[]>> => bodies.reduce((chain, body) => chain.then(_nested(scan, body, depth)), own);

const _grow = (
    scan: Scanner,
    run: Promise<Result<readonly Command[]>>,
    own: Promise<Result<readonly Command[]>>,
    depth: number,
): Promise<Result<readonly Command[]>> => run.then((parsed) => _chain(scan, own, _bodies(parsed, depth), depth));

const _expand = (scan: Scanner, run: Promise<Result<readonly Command[]>>, depth: number): Promise<Result<readonly Command[]>> =>
    _grow(
        scan,
        run,
        run.then((parsed) => _own(parsed, depth)),
        depth,
    );

const _parse = (scan: Scanner, text: string, depth: number, condition: boolean): Promise<Result<readonly Command[]>> =>
    _expand(scan, _run(scan, text, condition), depth);

const parse = (scan: Scanner, command: string): Promise<Result<readonly Command[]>> => _parse(scan, command, 0, false);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner };
export { parse, pastAssignments, SCAN, strip };
