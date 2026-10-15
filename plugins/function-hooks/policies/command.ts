import { all, bind, decoded, map, type Result } from '../composition.ts';
import { declared, type Invocation, inner, invocations, operands, unquoted } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<Result<string>>;

interface Context {
    readonly fed: boolean;
    readonly nested: boolean;
}
interface Span {
    readonly start: number;
    readonly end: number;
}
interface Command extends Context {
    readonly source: string;
    readonly words: readonly string[];
    readonly spans: readonly Span[];
    readonly invocations: readonly Invocation[];
}
interface Deletion {
    readonly program: string;
    readonly message: string;
}
interface Script {
    readonly commands: readonly Command[];
    readonly deletes: readonly Deletion[];
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: Span };
}

type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'fed' }
        | { readonly ruleId: 'delete'; readonly message: string; readonly metaVariables: { readonly single: { readonly PROGRAM: Capture } } }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WORD = /^(?!\d*[<>]|&>)./su;
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
utils:
    owner: {any: [{inside: {kind: command, pattern: $BODY}}, {inside: {kind: heredoc_redirect, inside: {kind: redirected_statement, matches: statement}}}, {inside: {kind: redirected_statement, matches: statement}}]}
    statement: {has: {field: body, matches: last}}
    last: {any: [{kind: command, pattern: $BODY}, {kind: 'list, pipeline, negated_command', has: {matches: last, nthChild: {position: 1, reverse: true}}}]}
    value: {kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number'}
rule: {matches: value, follows: {matches: value}, inside: {kind: file_redirect, matches: owner}}`,
        `id: fed
language: bash
utils:
    input: {any: [{kind: heredoc_redirect}, {kind: herestring_redirect}, {kind: file_redirect, regex: '^0?<'}]}
    stage: {any: [{inside: {kind: redirected_statement, field: body, has: {matches: input}}}, {inside: {kind: pipeline}, not: {nthChild: 1}}, {inside: {kind: pipeline, inside: {kind: heredoc_redirect}}}]}
rule: {kind: command, any: [{has: {matches: input}}, {matches: stage}, {inside: {stopBy: end, matches: stage}}]}`,
        String.raw`id: delete
language: bash
message: $PROGRAM deletes paths known at run time ($SOURCE)
utils:
    remover: {kind: 'command_name, word', regex: '^["'']?\\?(?:\S*/)?(?:rm|rimraf|trash|unlink|shred)["'']?$'}
    finder: {kind: 'command_name, word', regex: '^\\?(?:\S*/)?(?:fd|fdfind|g?find)$'}
    wrapper: {kind: 'command_name, word', regex: '^\\?(?:\S*/)?(?:sudo|doas|command|builtin|exec|env|nice|nohup|setsid|stdbuf|time|timeout|caffeinate|xargs|parallel|git|npx)$'}
    feeder: {kind: 'command_name, word', regex: '^\\?(?:\S*/)?(?:xargs|parallel)$'}
    execution: {kind: word, regex: '^(?:-[HIusigFalLp01q]*[xX]|--exec(?:-batch)?|-(?:exec|execdir|ok|okdir))$', follows: {stopBy: end, matches: searcher}}
    launcher: {any: [{matches: wrapper}, {matches: execution}]}
    allowed:
        any:
            - {kind: 'word, concatenation', regex: '^-|^[A-Za-z_][A-Za-z0-9_]*=|^\d+(?:\.\d+)?[smhd]?$'}
            - {kind: number}
            - {matches: wrapper}
            - {follows: {kind: word, regex: '^-(?:[aCcDdgIhJjkLnoPRSsTUu]|-(?:user|group|chdir|jobs|max-args|max-procs|max-lines|replace|delimiter|arg-file|signal|kill-after|adjustment|git-dir|work-tree))$'}}
    foreign: {kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number', not: {matches: allowed}}
    searcher: {all: [{matches: finder}, {any: [{kind: command_name}, {follows: {stopBy: {matches: foreign}, matches: wrapper}}]}]}
    program: {all: [{matches: remover}, {any: [{kind: command_name}, {follows: {stopBy: {matches: foreign}, matches: launcher}}]}]}
    unbounded:
        any:
            - kind: 'simple_expansion, expansion, command_substitution, process_substitution, arithmetic_expansion'
            - {kind: 'string, concatenation', has: {stopBy: end, kind: 'simple_expansion, expansion, command_substitution, arithmetic_expansion'}}
            - {kind: word, regex: '[*?\[{]'}
            - {kind: concatenation, has: {kind: word, regex: '[*?\[{]'}}
            - {kind: 'word, string, raw_string', regex: '^["'']?(?:~[^/"'']*|\.{1,2}|\*+|\.\*)?(?:/+(?:\.{1,2}|\*+|\.\*)?)*["'']?$'}
rule:
    kind: command
    not: {has: {kind: word, regex: '^--cached$'}}
    any:
        - all:
            - has: {all: [{matches: program}, {pattern: $PROGRAM}]}
            - has: {all: [{matches: unbounded}, {pattern: $SOURCE}, {follows: {stopBy: end, matches: program}}]}
        - has: {all: [{matches: program}, {pattern: $PROGRAM}, {follows: {stopBy: {matches: foreign}, matches: feeder, pattern: $SOURCE}}]}
        - all:
            - has: {all: [{matches: program}, {pattern: $PROGRAM}, {follows: {stopBy: {matches: foreign}, matches: execution}}]}
            - has: {all: [{matches: searcher}, {pattern: $SOURCE}]}
        - all:
            - has: {all: [{matches: searcher}, {pattern: $PROGRAM}]}
            - has: {kind: word, regex: '^-delete$', pattern: $SOURCE}`,
    ].join('\n---\n'),
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WORDS]

const _script = (hits: readonly Hit[], context: Context, source: string): Script => {
    const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
    return {
        commands: Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
            .values()
            .flatMap((own): readonly Command[] => {
                const hit = own.find((other) => other.ruleId === 'command');
                if (hit === undefined) {
                    return [];
                }
                const tokens = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')].filter(({ text }) => _WORD.test(text));
                const words = tokens.map(({ text }) => unquoted(text));
                return [{ source, words, spans: tokens.map(({ range }) => range.byteOffset), invocations: invocations(words), nested: context.nested, fed: context.fed || own.some((other) => other.ruleId === 'fed') }];
            })
            .toArray(),
        deletes: sorted.flatMap((hit) => (hit.ruleId === 'delete' ? [{ program: hit.metaVariables.single.PROGRAM.text, message: hit.message }] : [])),
    };
};

const _bodies = (command: Command): readonly string[] =>
    command.invocations
        .slice(-1)
        .flatMap((invocation) => [invocation, ...invocations(inner(invocation))])
        .flatMap((invocation) => {
            const [program, ...rest] = invocation;
            const { bodies, shell } = declared(program);
            const { inputs, options, values } = operands(invocation);
            return [...(program === 'eval' ? [rest.join(' ')] : []), ...(shell === true && options.includes('-c') ? inputs.slice(0, 1) : []), ...(bodies === undefined ? [] : [...inputs, ...values.flatMap(([name, value]) => (bodies.includes(name) ? [value] : []))])];
        });

// --- [PARSE]

const _parse = async (scanner: Scanner, text: string, context: Context): Promise<Result<Script>> =>
    bind(decoded<readonly Hit[]>('ast-grep', await scanner(text)), async (hits): Promise<Result<Script>> => {
        const script = _script(hits, context, text);
        const expanded = await Promise.all(
            script.commands.map(async (command) => map(all(await Promise.all(_bodies(command).map((body) => _parse(scanner, body, { fed: command.fed, nested: true })))), (nested): Script => ({ commands: [command, ...nested.flatMap(({ commands }) => commands)], deletes: nested.flatMap(({ deletes }) => deletes) }))),
        );
        return map(all(expanded), (scripts): Script => ({ commands: scripts.flatMap(({ commands }) => commands), deletes: [...script.deletes, ...scripts.flatMap(({ deletes }) => deletes)] }));
    });

const parse = (scanner: Scanner, command: string): Promise<Result<Script>> => _parse(scanner, command, { fed: false, nested: false });

// --- [SPANS]

const offset = (command: Command, index: number): number => command.words.length - command.invocations.slice(index).reduce((count, invocation) => count + invocation.length, 0);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner, Script, Span };
export { offset, parse, SCAN };
