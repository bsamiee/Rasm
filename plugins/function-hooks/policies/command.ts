import { all, bind, decoded, map, type Result } from '../composition.ts';
import { declared, type Invocation, invocations, operands } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<Result<string>>;
type Marker = 'looped' | 'polled' | 'fed';
type Context = Readonly<Record<Marker | 'nested', boolean>>;

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Command extends Context {
    readonly words: readonly string[];
    readonly spans: readonly Span[];
    readonly invocations: readonly Invocation[];
    readonly writes: readonly string[];
    readonly reads: readonly string[];
}
interface Script {
    readonly commands: readonly Command[];
    readonly clocks: readonly string[];
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: Span };
}
interface Owned {
    readonly single: { readonly BODY: Capture; readonly DEST: Capture };
}

type Hit = Capture & ({ readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } } | { readonly ruleId: 'operand' | 'write' | 'read'; readonly metaVariables: Owned } | { readonly ruleId: Marker | 'clock' });

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WORD = /^(?!\d*[<>]|&>)./su;
const _QUOTED = /(?:\$?(?<quote>["'])|\\)(?<body>(?<=')[^']*|(?<=")(?:[^"\\]|\\.)*|(?<=\\)[\s\S])\k<quote>/gu;
const _ESCAPED = /\\(?<char>["\\$`\n])/gu;
const _OWNER = `utils:
    owner: {any: [{inside: {kind: command, pattern: $BODY}}, {inside: {kind: heredoc_redirect, inside: {kind: redirected_statement, matches: statement}}}, {inside: {kind: redirected_statement, matches: statement}}]}
    statement: {has: {field: body, matches: last}}
    last: {any: [{kind: command, pattern: $BODY}, {kind: 'list, pipeline, negated_command', has: {matches: last, nthChild: {position: 1, reverse: true}}}]}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RULES]

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
const SCAN: Invocation = [
    'ast-grep',
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
        _marker('polled', "inside: {stopBy: end, any: [{kind: while_statement, not: {has: {field: condition, any: [{matches: read}, {has: {stopBy: end, matches: read}}]}}}, {kind: c_style_for_statement, not: {has: {field: condition, regex: '.'}}}]}"),
        _marker('fed', 'any: [{has: {matches: input}}, {matches: stage}, {inside: {stopBy: end, matches: stage}}]'),
        _redirect('write', "{not: {regex: '^\\d*<'}}"),
        _redirect('read', "{regex: '^\\d*<'}"),
        `id: clock
language: bash
rule: {any: [{kind: variable_name, regex: '^(SECONDS|EPOCHREALTIME|EPOCHSECONDS)$'}, {kind: command, has: {field: name, regex: '^date$'}, inside: {kind: arithmetic_expansion, stopBy: end}}]}`,
    ].join('\n---\n'),
];

// --- [WORDS]

const _unquoted = (_match: string, quote: string | undefined, body: string): string => (quote === '"' ? body.replace(_ESCAPED, '$<char>') : body);
const _owner = (hit: Hit): number => (hit.ruleId === 'operand' || hit.ruleId === 'write' || hit.ruleId === 'read' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start;
const _destinations = (own: readonly Hit[], id: 'write' | 'read'): readonly string[] => own.flatMap((other) => (other.ruleId === id ? [other.metaVariables.single.DEST.text.replace(_QUOTED, _unquoted)] : []));

const _script = (hits: readonly Hit[], context: Context): Script => {
    const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
    return {
        commands: [...Map.groupBy(sorted, _owner).values()].flatMap((own): readonly Command[] => {
            const hit = own.find((other) => other.ruleId === 'command');
            if (hit === undefined) {
                return [];
            }
            const rules = new Set(own.map((other) => other.ruleId));
            const marked = (marker: Marker): boolean => context[marker] || rules.has(marker);
            const tokens = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')].filter(({ text }) => _WORD.test(text));
            const words = tokens.map(({ text }) => text.replace(_QUOTED, _unquoted));
            return [{ words, spans: tokens.map(({ range }) => range.byteOffset), invocations: invocations(words), nested: context.nested, looped: marked('looped'), polled: marked('polled'), fed: marked('fed'), writes: _destinations(own, 'write'), reads: _destinations(own, 'read') }];
        }),
        clocks: sorted.flatMap((hit) => (hit.ruleId === 'clock' ? [hit.text] : [])),
    };
};

const _bodies = (command: Command): readonly string[] => {
    const invocation = command.invocations.at(-1);
    if (invocation === undefined) {
        return [];
    }
    const [program, ...rest] = invocation;
    const { bodies, shell } = declared(program);
    const { inputs, options, values } = operands(invocation);
    return [...(program === 'eval' ? [rest.join(' ')] : []), ...(shell === true && options.includes('-c') ? inputs.slice(0, 1) : []), ...(bodies === undefined ? [] : [...inputs, ...values.flatMap(([name, value]) => (bodies.includes(name) ? [value] : []))])];
};

// --- [PARSE]

const _joined = (scripts: readonly Script[]): Script => ({ commands: scripts.flatMap(({ commands }) => commands), clocks: scripts.flatMap(({ clocks }) => clocks) });

const _parse = async (scan: Scanner, text: string, context: Context): Promise<Result<Script>> =>
    bind(decoded<readonly Hit[]>('ast-grep', await scan(text)), async (hits): Promise<Result<Script>> => {
        const script = _script(hits, context);
        const expanded = await Promise.all(script.commands.map(async (command): Promise<Result<Script>> => map(all(await Promise.all(_bodies(command).map((body) => _parse(scan, body, { ...command, nested: true })))), (inner) => _joined([{ commands: [command], clocks: [] }, ...inner]))));
        return map(all(expanded), (scripts) => _joined([{ commands: [], clocks: script.clocks }, ...scripts]));
    });

const parse = (scan: Scanner, command: string): Promise<Result<Script>> => _parse(scan, command, { looped: false, polled: false, fed: false, nested: false });

// --- [SPANS]

const offset = (command: Command, index: number): number => command.words.length - command.invocations.slice(index).reduce((count, invocation) => count + invocation.length, 0);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner, Script };
export { offset, parse, SCAN };
