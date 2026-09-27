import type { ProcessRunResult } from 'claude-code';
import { bind, fault, fromNullable, none, type Option, ok, type Result, some } from './composition.ts';
import { invocations } from './invocation.ts';
import { basename } from './path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<ProcessRunResult>;

interface Command {
    readonly words: readonly string[];
    readonly looped: boolean;
    readonly polled: boolean;
    readonly fed: boolean;
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

type Enclosing = Pick<Command, 'looped' | 'polled' | 'fed'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _INPUT = "{regex: '^\\d*<'}";
const _marker = (id: keyof Enclosing, relation: string): string => `id: ${id}
language: bash
utils:
    read: {kind: command, has: {field: name, regex: '^read$'}}
    input: {any: [{kind: heredoc_redirect}, {kind: herestring_redirect}, {kind: file_redirect, regex: '^0?<'}]}
    stage: {any: [{inside: {kind: redirected_statement, field: body, has: {matches: input}}}, {inside: {kind: pipeline}, not: {nthChild: 1}}, {inside: {kind: pipeline, inside: {kind: heredoc_redirect}}}]}
rule: {kind: command, ${relation}}
message: ${id}`;
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
        `id: command
language: bash
rule: {kind: command, any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}]}
rewriters: [{id: word, rule: {pattern: $W}, fix: $W}]
transform:
    ARGV: {rewrite: {source: $$$ARGS, rewriters: [word], joinBy: "\\u001f"}}
    NAME: {replace: {source: $CMD, replace: '\\n', by: ' '}}
    LINE: {replace: {source: $ARGV, replace: '\\n', by: ' '}}
message: "$NAME\\u001f$LINE"`,
        _marker('looped', 'inside: {kind: do_group, stopBy: end}'),
        _marker(
            'polled',
            "inside: {stopBy: end, any: [{kind: while_statement, not: {has: {field: condition, any: [{matches: read}, {has: {stopBy: end, matches: read}}]}}}, {kind: c_style_for_statement, not: {has: {field: condition, regex: '.'}}}]}",
        ),
        _marker('fed', 'any: [{has: {matches: input}}, {matches: stage}, {inside: {stopBy: end, matches: stage}}]'),
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
const _INLINE = /^-[A-Za-z]*c[A-Za-z]*$/u;

// --- [WORDS] ---------------------------------------------------------------------------

const _unquoted = (_match: string, quote: string | undefined, body: string): string => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body);

const _body = (command: Command, depth: number): Option<string> => {
    const maxDepth = 8;
    const invocation = invocations(command.words).at(-1);
    if (invocation === undefined || depth >= maxDepth) {
        return none;
    }
    const [head, ...rest] = invocation;
    const name = basename(head);
    const hit = rest.findIndex((word) => _INLINE.test(word));
    const inline = ['sh', 'bash', 'zsh', 'dash', 'ksh'].includes(name) && hit >= 0 ? fromNullable(rest.slice(hit + 1).find((word) => !word.startsWith('-'))) : none;
    const text = name === 'eval' ? some(rest.join(' ')) : inline;
    return text.kind === 'some' && text.value === '' ? none : text;
};

const _nested = (commands: readonly Command[], depth: number, at: number): Option<Nested> => {
    const command = commands[at];
    if (command === undefined) {
        return none;
    }
    const text = _body(command, depth);
    return text.kind === 'some' ? some({ before: commands.slice(0, at), command, text: text.value, rest: commands.slice(at + 1) }) : _nested(commands, depth, at + 1);
};

// --- [REPORT] --------------------------------------------------------------------------

const _compare = (left: Hit, right: Hit): number => (left.line === right.line ? left.column - right.column : left.line - right.line);

const _hits = (report: string): readonly Hit[] =>
    [...report.matchAll(_REPORT)]
        .flatMap(({ groups }): readonly Hit[] => {
            const rule = groups?.['rule'];
            const line = groups?.['line'];
            const column = groups?.['column'];
            const text = groups?.['text'];
            return rule === undefined || line === undefined || column === undefined || text === undefined ? [] : [{ rule, line: Number(line), column: Number(column), text }];
        })
        .toSorted(_compare);

const _texts = (hits: readonly Hit[], rule: string): readonly string[] => hits.flatMap((hit) => (hit.rule === rule ? [hit.text.replace(_QUOTED, _unquoted)] : []));

const _commands = (hits: readonly Hit[], enclosing: Enclosing): readonly Command[] => {
    const starts = hits.filter((hit) => hit.rule === 'command');
    return starts.map((start, nth) => {
        const next = fromNullable(starts[nth + 1]);
        const span = hits.filter((hit) => (nth === 0 || _compare(start, hit) <= 0) && (next.kind === 'none' || _compare(hit, next.value) < 0));
        const marked = (rule: keyof Enclosing): boolean => enclosing[rule] || span.some((hit) => hit.rule === rule);
        return {
            words: start.text.split('\u001f').flatMap((field) => (_WORD.test(field) ? [field.replace(_QUOTED, _unquoted)] : [])),
            looped: marked('looped'),
            polled: marked('polled'),
            fed: marked('fed'),
            writes: _texts(span, 'write'),
            reads: _texts(span, 'read'),
            clocks: _texts(span, 'clock'),
        };
    });
};

// --- [PARSE] ---------------------------------------------------------------------------

const _placed = (scan: Scanner, depth: number, done: readonly Command[], nested: Nested): Promise<Result<readonly Command[]>> =>
    _parse(scan, nested.text, depth + 1, nested.command).then((inner) =>
        bind(inner, (commands) => {
            const placed = [...done, ...nested.before, nested.command, ...commands];
            const next = _nested(nested.rest, depth, 0);
            return next.kind === 'none' ? ok([...placed, ...nested.rest]) : _placed(scan, depth, placed, next.value);
        }),
    );

const _parse = (scan: Scanner, text: string, depth: number, enclosing: Enclosing): Promise<Result<readonly Command[]>> =>
    scan(text)
        .then(
            ({ exitCode, stdout, stderr }): Result<readonly Command[]> =>
                exitCode === 0 && (stdout === '' || stdout.endsWith('\n')) ? ok(_commands(_hits(stdout), enclosing)) : fault(`ast-grep exited ${exitCode} over the command, ${stderr.trim()}`),
            (cause: unknown): Result<readonly Command[]> => fault(`ast-grep did not run over the command, ${String(cause)}`),
        )
        .then((parsed) =>
            bind(parsed, (commands) => {
                const nested = _nested(commands, depth, 0);
                return nested.kind === 'none' ? ok(commands) : _placed(scan, depth, [], nested.value);
            }),
        );

const parse = (scan: Scanner, command: string): Promise<Result<readonly Command[]>> => _parse(scan, command, 0, { looped: false, polled: false, fed: false });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner };
export { parse, SCAN };
