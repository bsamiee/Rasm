// --- [IMPORTS] -------------------------------------------------------------------------

import type { ProcessRunResult } from 'claude-code';
import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Argv = readonly string[];

type Scanner = (text: string) => Promise<ProcessRunResult>;

interface Command {
    readonly words: Argv;
    readonly condition: boolean;
}

type Parse = { readonly kind: 'parsed'; readonly commands: readonly Command[] } | { readonly kind: 'unparsed'; readonly reason: string };

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

const _stripOptions = (argv: Argv): Argv => {
    const [head] = argv;
    if (head === undefined || !(head.includes('=') || _DIGITS.test(head) || _SUBCOMMANDS.includes(head) || head.startsWith('-'))) {
        return argv;
    }
    return _stripOptions(argv.slice(_VALUE_OPTS.includes(head) ? 2 : 1));
};

const pastAssignments = (argv: Argv): Argv => {
    const index = argv.findIndex((word) => !_ENV_ASSIGN.test(word));
    return index < 0 ? [] : argv.slice(index);
};

const strip = (argv: Argv): Argv => {
    const [head] = argv;
    return head !== undefined && (_ENV_ASSIGN.test(head) || _WRAPPERS.includes(head)) ? strip(_stripOptions(argv.slice(1))) : argv;
};

const _inline = (argv: Argv): readonly string[] => {
    const hit = argv.findIndex((word) => word.startsWith('-') && !word.startsWith('--') && word.endsWith('c'));
    return hit < 0
        ? []
        : argv
              .slice(hit + 1)
              .filter((word) => word !== '--')
              .slice(0, 1);
};

// --- [RESOLUTION] ----------------------------------------------------------------------

const _texts = (name: string, argv: Argv): readonly string[] => {
    if (name === 'eval') {
        return [argv.slice(1).join(' ')];
    }
    return _SHELLS.includes(name) ? _inline(argv) : [];
};

const _resolve = (command: Command, depth: number): Resolved => {
    const argv = strip(command.words);
    const texts = depth < _MAX_DEPTH ? _texts(basename(argv[0] ?? ''), argv) : [];
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

const _run = (scan: Scanner, text: string, condition: boolean): Promise<Parse> =>
    scan(text).then(
        ({ exitCode, stdout, stderr }): Parse =>
            exitCode === 0 && (stdout === '' || stdout.endsWith('\n'))
                ? { kind: 'parsed', commands: _commands(stdout, condition) }
                : { kind: 'unparsed', reason: `ast-grep exited ${exitCode} over the command, ${stderr.trim()}` },
        (cause: unknown): Parse => ({ kind: 'unparsed', reason: `ast-grep did not run over the command, ${String(cause)}` }),
    );

const _join = (left: Parse, right: Parse): Parse => {
    if (left.kind === 'unparsed') {
        return left;
    }
    return right.kind === 'unparsed' ? right : { kind: 'parsed', commands: [...left.commands, ...right.commands] };
};

const _own = (parsed: Parse, depth: number): Parse =>
    parsed.kind === 'unparsed' ? parsed : { kind: 'parsed', commands: parsed.commands.flatMap((command) => _resolve(command, depth).commands) };

const _bodies = (parsed: Parse, depth: number): readonly Body[] =>
    parsed.kind === 'unparsed' ? [] : parsed.commands.flatMap((command) => _resolve(command, depth).bodies);

const _nested =
    (scan: Scanner, body: Body, depth: number) =>
    (left: Parse): Promise<Parse> =>
        _parse(scan, body.text, depth + 1, body.condition).then((right) => _join(left, right));

const _chain = (scan: Scanner, own: Promise<Parse>, bodies: readonly Body[], depth: number): Promise<Parse> =>
    bodies.reduce((chain, body) => chain.then(_nested(scan, body, depth)), own);

const _grow = (scan: Scanner, run: Promise<Parse>, own: Promise<Parse>, depth: number): Promise<Parse> =>
    run.then((parsed) => _chain(scan, own, _bodies(parsed, depth), depth));

const _expand = (scan: Scanner, run: Promise<Parse>, depth: number): Promise<Parse> =>
    _grow(
        scan,
        run,
        run.then((parsed) => _own(parsed, depth)),
        depth,
    );

const _parse = (scan: Scanner, text: string, depth: number, condition: boolean): Promise<Parse> => _expand(scan, _run(scan, text, condition), depth);

const parse = (scan: Scanner, command: string): Promise<Parse> => _parse(scan, command, 0, false);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Argv, Command, Parse, Scanner };
export { parse, pastAssignments, SCAN, strip };
