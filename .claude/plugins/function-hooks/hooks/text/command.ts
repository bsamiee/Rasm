// --- [IMPORTS] -------------------------------------------------------------------------

import type { ProcessRunResult } from 'claude-code';
import { fault, fromNullable, none, type Option, ok, type Result, some } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<ProcessRunResult>;

interface Command {
    readonly words: readonly string[];
    readonly condition: boolean;
    readonly looped: boolean;
    readonly writes: readonly string[];
    readonly reads: readonly string[];
    readonly clocks: readonly string[];
}

interface Hit {
    readonly rule: string;
    readonly line: number;
    readonly column: number;
    readonly text: string;
}

interface Nested {
    readonly before: readonly Command[];
    readonly command: Command;
    readonly text: string;
    readonly rest: readonly Command[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MAX_DEPTH = 8;
const _SEPARATOR = '\u001f';
const _CONDITION = '{any: [{inside: {kind: while_statement, field: condition}}, {inside: {stopBy: end, inside: {kind: while_statement, field: condition}}}]}';
const _LOOPED = '{inside: {kind: do_group, stopBy: end}}';
const _INPUT = "{regex: '^\\d*<'}";
const _rule = (id: string, relation: string): string => `id: ${id}
language: bash
rule: {kind: command, all: [{any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}]}, ${relation}]}
rewriters: [{id: word, rule: {pattern: $W}, fix: $W}]
transform:
    ARGV: {rewrite: {source: $$$ARGS, rewriters: [word], joinBy: "\\u001f"}}
    NAME: {replace: {source: $CMD, replace: '\\n', by: ' '}}
    LINE: {replace: {source: $ARGV, replace: '\\n', by: ' '}}
message: "$NAME\\u001f$LINE"`;
const _redirect = (id: string, relation: string): string => `id: ${id}
language: bash
rule: {kind: file_redirect, all: [{has: {field: destination, pattern: $DEST, not: {kind: number}}}, {any: [{inside: {kind: command}}, {inside: {kind: heredoc_redirect}}, {inside: {kind: redirected_statement, not: {has: {field: body, kind: 'compound_statement, subshell, for_statement, c_style_for_statement, while_statement, if_statement, case_statement, function_definition'}}}}]}, ${relation}]}
message: "$DEST"`;
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
    [
        _rule('command', `{not: ${_CONDITION}}, {not: ${_LOOPED}}`),
        _rule('condition', _CONDITION),
        _rule('looped', `{not: ${_CONDITION}}, ${_LOOPED}`),
        _redirect('write', `{not: ${_INPUT}}`),
        _redirect('read', _INPUT),
        `id: clock
language: bash
rule: {any: [{kind: variable_name, pattern: $C, regex: '^(SECONDS|EPOCHREALTIME|EPOCHSECONDS)$'}, {kind: command, pattern: $C, has: {field: name, regex: '^date$'}, inside: {kind: arithmetic_expansion, stopBy: end}}]}
message: "$C"`,
    ].join('\n---\n'),
    '--color',
    'never',
    '--report-style',
    'short',
];
const _REPORT = /^STDIN:(?<line>\d+):(?<column>\d+): \w+\[(?<rule>\w+)\]: (?<text>.*)$/gmu;
const _WORD = /^(?!\d*[<>]|&>)./su;
const _QUOTED = /(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _ENV_ASSIGN = /^[A-Za-z_][A-Za-z0-9_]*=/u;
const _DIGITS = /^\d+$/u;
const _INLINE = /^-[A-Za-z]*c[A-Za-z]*$/u;
const LAUNCHERS: readonly string[] = ['mise', 'doppler', 'op'];

// --- [WORDS] ---------------------------------------------------------------------------

const basename = (text: string): string => text.split('/').at(-1) ?? '';

const _unquoted = (_match: string, quote: string | undefined, body: string): string => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body);

const _stripOptions = (words: readonly string[]): readonly string[] => {
    const [head] = words;
    if (head === undefined || !(head.includes('=') || _DIGITS.test(head) || ['run', 'exec', 'tool', 'x'].includes(head) || head.startsWith('-'))) {
        return words;
    }
    return _stripOptions(words.slice(['-u', '-I', '-n', '-g', '--user', '--replace'].includes(head) ? 2 : 1));
};

const pastAssignments = (words: readonly string[]): readonly string[] => {
    const index = words.findIndex((word) => !_ENV_ASSIGN.test(word));
    return index < 0 ? [] : words.slice(index);
};

const strip = (words: readonly string[]): readonly string[] => {
    const [head] = words;
    return head !== undefined &&
        (_ENV_ASSIGN.test(head) ||
            ['sudo', 'doas', 'env', 'command', 'exec', 'nice', 'nohup', 'stdbuf', 'timeout', 'time', 'xargs', 'caffeinate', 'arch', 'setsid', 'uv', 'npm', 'npx', 'pnpm', 'poetry', 'hatch'].includes(
                head,
            ))
        ? strip(_stripOptions(words.slice(1)))
        : words;
};

const _body = (command: Command, depth: number): Option<string> => {
    const [head, ...rest] = strip(command.words);
    if (head === undefined || depth >= _MAX_DEPTH) {
        return none;
    }
    const name = basename(head);
    const hit = rest.findIndex((word) => _INLINE.test(word));
    const inline = ['sh', 'bash', 'zsh', 'dash', 'ksh'].includes(name) && hit >= 0 ? fromNullable(rest.slice(hit + 1).find((word) => !word.startsWith('-'))) : none;
    const text = name === 'eval' ? some(rest.join(' ')) : inline;
    return text.kind === 'some' && text.value === '' ? none : text;
};

const _nested = (todo: readonly Command[], depth: number, at: number): Option<Nested> => {
    const command = todo[at];
    if (command === undefined) {
        return none;
    }
    const text = _body(command, depth);
    return text.kind === 'some' ? some({ before: todo.slice(0, at), command, text: text.value, rest: todo.slice(at + 1) }) : _nested(todo, depth, at + 1);
};

// --- [REPORT] --------------------------------------------------------------------------

const _before = (left: Hit, right: Hit): number => (left.line === right.line ? left.column - right.column : left.line - right.line);

const _hits = (report: string): readonly Hit[] =>
    [...report.matchAll(_REPORT)]
        .flatMap(({ groups }): readonly Hit[] => {
            const rule = groups?.['rule'];
            const line = groups?.['line'];
            const column = groups?.['column'];
            const text = groups?.['text'];
            return rule === undefined || line === undefined || column === undefined || text === undefined ? [] : [{ rule, line: Number(line), column: Number(column), text }];
        })
        .toSorted(_before);

const _texts = (hits: readonly Hit[], rule: string): readonly string[] => hits.flatMap((hit) => (hit.rule === rule ? [hit.text.replace(_QUOTED, _unquoted)] : []));

const _commands = (hits: readonly Hit[], condition: boolean, looped: boolean): readonly Command[] => {
    const starts = hits.filter((hit) => ['command', 'condition', 'looped'].includes(hit.rule));
    return starts.map((start, nth) => {
        const next = fromNullable(starts[nth + 1]);
        const span = hits.filter((hit) => (nth === 0 || _before(start, hit) <= 0) && (next.kind === 'none' || _before(hit, next.value) < 0));
        return {
            words: start.text.split(_SEPARATOR).flatMap((field) => (_WORD.test(field) ? [field.replace(_QUOTED, _unquoted)] : [])),
            condition: condition || start.rule === 'condition',
            looped: looped || start.rule === 'looped',
            writes: _texts(span, 'write'),
            reads: _texts(span, 'read'),
            clocks: _texts(span, 'clock'),
        };
    });
};

// --- [PARSE] ---------------------------------------------------------------------------

const _placed = (scan: Scanner, depth: number, done: readonly Command[], nested: Nested): Promise<Result<readonly Command[]>> =>
    _parse(scan, nested.text, depth + 1, nested.command.condition, nested.command.looped).then((inner) => {
        if (inner.kind === 'fault') {
            return inner;
        }
        const placed = [...done, ...nested.before, nested.command, ...inner.value];
        const next = _nested(nested.rest, depth, 0);
        return next.kind === 'none' ? ok([...placed, ...nested.rest]) : _placed(scan, depth, placed, next.value);
    });

const _parse = (scan: Scanner, text: string, depth: number, condition: boolean, looped: boolean): Promise<Result<readonly Command[]>> =>
    scan(text)
        .then(
            ({ exitCode, stdout, stderr }): Result<readonly Command[]> =>
                exitCode === 0 && (stdout === '' || stdout.endsWith('\n')) ? ok(_commands(_hits(stdout), condition, looped)) : fault(`ast-grep exited ${exitCode} over the command, ${stderr.trim()}`),
            (cause: unknown): Result<readonly Command[]> => fault(`ast-grep did not run over the command, ${String(cause)}`),
        )
        .then((parsed) => {
            if (parsed.kind === 'fault') {
                return parsed;
            }
            const nested = _nested(parsed.value, depth, 0);
            return nested.kind === 'none' ? parsed : _placed(scan, depth, [], nested.value);
        });

const parse = (scan: Scanner, command: string): Promise<Result<readonly Command[]>> => _parse(scan, command, 0, false, false);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner };
export { basename, LAUNCHERS, parse, pastAssignments, SCAN, strip };
