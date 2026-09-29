import { all, bind, decoded, fromUndefined, map, none, type Option, ok, type Result, some } from './composition.ts';
import { invocations, operands, PROGRAMS } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<Result<string>>;
type Marker = 'looped' | 'polled' | 'fed';

interface Command {
    readonly words: readonly string[];
    readonly looped: boolean;
    readonly polled: boolean;
    readonly fed: boolean;
    readonly writes: readonly string[];
    readonly reads: readonly string[];
}
interface Script {
    readonly commands: readonly Command[];
    readonly clocks: readonly string[];
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: { readonly start: number } };
}
interface Owned {
    readonly single: { readonly BODY: Capture; readonly DEST: Capture };
}

type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'write' | 'read'; readonly metaVariables: Owned }
        | { readonly ruleId: Marker | 'clock' }
    );
type Enclosing = Pick<Command, Marker>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WORD = /^(?!\d*[<>]|&>)./su;
const _QUOTED = /(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _OWNER = `utils:
    owner: {any: [{inside: {kind: command, pattern: $BODY}}, {inside: {kind: heredoc_redirect, inside: {kind: redirected_statement, matches: statement}}}, {inside: {kind: redirected_statement, matches: statement}}]}
    statement: {has: {field: body, matches: last}}
    last: {any: [{kind: command, pattern: $BODY}, {kind: 'list, pipeline, negated_command', has: {matches: last, nthChild: {position: 1, reverse: true}}}]}`;

// --- [SCAN] ----------------------------------------------------------------------------

const _marker = (id: Marker, relation: string): string => `id: ${id}
language: bash
utils:
    read: {kind: command, has: {field: name, regex: '^read$'}}
    input: {any: [{kind: heredoc_redirect}, {kind: herestring_redirect}, {kind: file_redirect, regex: '^0?<'}]}
    stage: {any: [{inside: {kind: redirected_statement, field: body, has: {matches: input}}}, {inside: {kind: pipeline}, not: {nthChild: 1}}, {inside: {kind: pipeline, inside: {kind: heredoc_redirect}}}]}
rule: {kind: command, ${relation}}`;
const _redirect = (id: 'write' | 'read', relation: string): string => `id: ${id}
language: bash
${_OWNER}
rule: {kind: file_redirect, all: [{has: {field: destination, pattern: $DEST, not: {kind: number}}}, {matches: owner}, ${relation}]}`;
const SCAN: readonly string[] = [
    'scan',
    '--stdin',
    '--config',
    '/dev/null',
    '--json=compact',
    '--inline-rules',
    [
        `id: command
language: bash
rule: {kind: command, any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}]}`,
        `id: operand
language: bash
${_OWNER}
    value: {kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number'}
rule: {matches: value, follows: {matches: value}, inside: {kind: file_redirect, matches: owner}}`,
        _marker('looped', 'inside: {kind: do_group, stopBy: end}'),
        _marker(
            'polled',
            "inside: {stopBy: end, any: [{kind: while_statement, not: {has: {field: condition, any: [{matches: read}, {has: {stopBy: end, matches: read}}]}}}, {kind: c_style_for_statement, not: {has: {field: condition, regex: '.'}}}]}",
        ),
        _marker('fed', 'any: [{has: {matches: input}}, {matches: stage}, {inside: {stopBy: end, matches: stage}}]'),
        _redirect('write', "{not: {regex: '^\\d*<'}}"),
        _redirect('read', "{regex: '^\\d*<'}"),
        `id: clock
language: bash
rule: {any: [{kind: variable_name, regex: '^(SECONDS|EPOCHREALTIME|EPOCHSECONDS)$'}, {kind: command, has: {field: name, regex: '^date$'}, inside: {kind: arithmetic_expansion, stopBy: end}}]}`,
    ].join('\n---\n'),
];

// --- [WORDS] ---------------------------------------------------------------------------

const _unquoted = (_match: string, quote: string | undefined, body: string): string => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body);
const _owner = (hit: Hit): number => (hit.ruleId === 'operand' || hit.ruleId === 'write' || hit.ruleId === 'read' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start;

const _script = (hits: readonly Hit[], enclosing: Enclosing): Script => {
    const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
    return {
        commands: [...Map.groupBy(sorted, _owner).values()].flatMap((own): readonly Command[] => {
            const hit = own.find((other) => other.ruleId === 'command');
            if (hit?.ruleId !== 'command') {
                return [];
            }
            const marked = (marker: Marker): boolean => enclosing[marker] || own.some((other) => other.ruleId === marker);
            const destinations = (id: 'write' | 'read'): readonly string[] => own.flatMap((other) => (other.ruleId === id ? [other.metaVariables.single.DEST.text.replace(_QUOTED, _unquoted)] : []));
            return [
                {
                    words: [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')].flatMap(({ text }) =>
                        _WORD.test(text) ? [text.replace(_QUOTED, _unquoted)] : [],
                    ),
                    looped: marked('looped'),
                    polled: marked('polled'),
                    fed: marked('fed'),
                    writes: destinations('write'),
                    reads: destinations('read'),
                },
            ];
        }),
        clocks: sorted.flatMap((hit) => (hit.ruleId === 'clock' ? [hit.text] : [])),
    };
};

const _body = (command: Command, depth: number): Option<string> => {
    const maxDepth = 8;
    const invocation = invocations(command.words).at(-1);
    if (invocation === undefined || depth >= maxDepth) {
        return none;
    }
    const [program, ...rest] = invocation;
    const { inputs, options } = operands(invocation);
    const inline = PROGRAMS[program]?.shell === true && options.includes('-c') ? fromUndefined(inputs[0]) : none;
    return program === 'eval' ? some(rest.join(' ')) : inline;
};

// --- [PARSE] ---------------------------------------------------------------------------

const _parse = async (scan: Scanner, text: string, depth: number, enclosing: Enclosing): Promise<Result<Script>> =>
    bind(decoded<readonly Hit[]>('ast-grep', await scan(text)), async (hits): Promise<Result<Script>> => {
        const script = _script(hits, enclosing);
        const placed = await Promise.all(
            script.commands.map(async (command): Promise<Result<Script>> => {
                const body = _body(command, depth);
                return body.kind === 'some'
                    ? map(await _parse(scan, body.value, depth + 1, command), (nested) => ({ ...nested, commands: [command, ...nested.commands] }))
                    : ok({ commands: [command], clocks: [] });
            }),
        );
        return map(all(placed), (scripts) => ({ commands: scripts.flatMap(({ commands }) => commands), clocks: [...script.clocks, ...scripts.flatMap(({ clocks }) => clocks)] }));
    });

const parse = (scan: Scanner, command: string): Promise<Result<Script>> => _parse(scan, command, 0, { looped: false, polled: false, fed: false });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Script };
export { parse, SCAN };
