import { all, bind, decoded, map, none, type Option, type Result, some } from '../composition.ts';
import { declared, type Invocation, inner, invocations, operands, unquoted, waits } from './invocation.ts';

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
    readonly stdin: Option<string>;
}
interface Step {
    readonly programs: readonly string[];
    readonly writes: readonly string[];
    readonly reads: readonly string[];
}
interface Deletion {
    readonly program: string;
    readonly message: string;
}
interface Script {
    readonly commands: readonly Command[];
    readonly sequences: readonly (readonly Step[])[];
    readonly deletes: readonly Deletion[];
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: Span };
}

type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture; readonly SCOPE?: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'heredoc' | 'herestring'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'step'; readonly metaVariables: { readonly single: { readonly SEQ?: Capture; readonly SCOPE?: Capture } } }
        | { readonly ruleId: 'write'; readonly metaVariables: { readonly single: { readonly NAME: Capture; readonly SCOPE?: Capture } } }
        | { readonly ruleId: 'fed' | 'read' }
        | { readonly ruleId: 'delete'; readonly message: string; readonly metaVariables: { readonly single: { readonly PROGRAM: Capture } } }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WORD = /^(?!\d*[<>]|&>)./su;
const _OWNER = `    owner: {any: [{inside: {kind: command, pattern: $BODY}}, {inside: {kind: heredoc_redirect, inside: {kind: redirected_statement, matches: statement}}}, {inside: {kind: redirected_statement, matches: statement}}]}
    statement: {has: {field: body, matches: last}}
    last: {any: [{kind: command, pattern: $BODY}, {kind: 'list, pipeline, negated_command', has: {matches: last, nthChild: {position: 1, reverse: true}}}]}
    value: {kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number'}`;
const _SCOPE = `    scope: {kind: 'program, subshell, command_substitution, process_substitution, pipeline'}
    scoped: {any: [{inside: {stopBy: {matches: scope}, kind: program}}, {inside: {stopBy: {matches: scope}, matches: scope, not: {kind: program}, pattern: $SCOPE}}]}`;
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
utils:
${_SCOPE}
rule: {kind: command, any: [{pattern: $CMD $$$ARGS}, {pattern: $CMD}], matches: scoped}`,
        `id: operand
language: bash
utils:
${_OWNER}
rule: {matches: value, follows: {matches: value}, inside: {kind: file_redirect, matches: owner}}`,
        `id: heredoc
language: bash
utils:
${_OWNER}
rule: {kind: heredoc_body, inside: {kind: heredoc_redirect, matches: owner}}`,
        `id: herestring
language: bash
utils:
${_OWNER}
rule: {matches: value, inside: {kind: herestring_redirect, matches: owner}}`,
        `id: fed
language: bash
utils:
    input: {any: [{kind: heredoc_redirect}, {kind: herestring_redirect}, {kind: file_redirect, regex: '^0?<'}]}
    tail: {any: [{inside: {kind: redirected_statement, field: body, has: {matches: input}}}, {nthChild: {position: 1, reverse: true}, inside: {kind: 'list, pipeline, negated_command', matches: tail}}]}
    stage: {any: [{matches: tail}, {inside: {kind: pipeline}, not: {nthChild: 1}}, {inside: {kind: pipeline, inside: {kind: heredoc_redirect}}}]}
rule: {kind: command, any: [{has: {matches: input}}, {matches: stage}, {inside: {stopBy: end, not: {kind: 'list, pipeline, negated_command'}, matches: stage}}]}`,
        `id: step
language: bash
utils:
${_SCOPE}
    chained: {any: [{kind: list}, {kind: redirected_statement, has: {field: body, kind: list}}]}
    tested: {inside: {kind: 'if_statement, elif_clause', field: condition}}
rule:
    kind: 'redirected_statement, variable_assignment, variable_assignments, command, declaration_command, unset_command, test_command, negated_command, for_statement, c_style_for_statement, while_statement, if_statement, case_statement, pipeline, subshell, compound_statement, function_definition'
    matches: scoped
    not: {any: [{matches: chained}, {matches: tested}, {inside: {stopBy: {not: {matches: chained}}, matches: chained, all: [{matches: tested}]}}]}
    any:
        - inside: {stopBy: {not: {matches: chained}}, kind: program}
        - inside: {stopBy: {not: {matches: chained}}, kind: 'compound_statement, do_group, subshell, if_statement, elif_clause, else_clause, case_item, command_substitution, process_substitution', pattern: $SEQ}`,
        `id: write
language: bash
utils:
${_SCOPE}
rule: {kind: variable_assignment, has: {field: name, pattern: $NAME}, not: {inside: {kind: command}}, matches: scoped}`,
        `id: read
language: bash
rule: {kind: variable_name, any: [{inside: {kind: 'simple_expansion, expansion'}}, {inside: {stopBy: end, kind: arithmetic_expansion}}]}`,
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
    const key = (capture: Capture | undefined): string => (capture === undefined ? '' : `${capture.range.byteOffset.start}:${capture.range.byteOffset.end}`);
    const owned = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
        .values()
        .flatMap((own) => {
            const hit = own.find((other) => other.ruleId === 'command');
            if (hit === undefined) {
                return [];
            }
            const tokens = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')].filter(({ text }) => _WORD.test(text));
            const words = tokens.map(({ text }) => unquoted(text));
            const given = own.findLast((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring');
            const command: Command = {
                source,
                words,
                spans: tokens.map(({ range }) => range.byteOffset),
                invocations: invocations(words),
                nested: context.nested,
                fed: context.fed || own.some((other) => other.ruleId === 'fed'),
                stdin: given === undefined ? none : some(given.ruleId === 'herestring' ? unquoted(given.text) : given.text),
            };
            return [{ command, hit }];
        })
        .toArray();
    const steps = sorted.flatMap((hit) => (hit.ruleId === 'step' ? [hit] : []));
    return {
        commands: owned.map(({ command }) => command),
        sequences: Map.groupBy(steps, ({ metaVariables }) => key(metaVariables.single.SEQ))
            .values()
            .map((held) =>
                held.map(({ range: { byteOffset }, metaVariables }): Step => {
                    const inside = ({ range }: Capture): boolean => range.byteOffset.start >= byteOffset.start && range.byteOffset.end <= byteOffset.end;
                    const scope = key(metaVariables.single.SCOPE);
                    const contained = owned.filter(({ hit }) => inside(hit));
                    return {
                        programs: contained.flatMap(({ command }) => command.invocations.slice(-1).map(([program]) => program)),
                        writes: [
                            ...sorted.flatMap((hit) => (hit.ruleId === 'write' && inside(hit) && key(hit.metaVariables.single.SCOPE) === scope ? [hit.metaVariables.single.NAME.text] : [])),
                            ...contained.flatMap(({ command, hit }) => (key(hit.metaVariables.single.SCOPE) === scope ? command.invocations.flatMap(([program]) => declared(program).writes ?? []) : [])),
                        ],
                        reads: ['PWD', ...sorted.flatMap((hit) => (hit.ruleId === 'read' && inside(hit) ? [hit.text] : []))],
                    };
                }),
            )
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
            const { bodies, shell, code = [] } = declared(program);
            const { inputs, values } = operands(invocation);
            const named = [...(bodies ?? []), ...(shell === true ? code : [])];
            return [...(program === 'eval' ? [rest.join(' ')] : []), ...(bodies === undefined ? [] : inputs), ...values.flatMap(([name, value]) => (named.includes(name) ? [value] : [])), ...(shell === true && waits(invocation) && command.stdin.kind === 'some' ? [command.stdin.value] : [])];
        });

// --- [PARSE]

const _parse = async (scanner: Scanner, text: string, context: Context): Promise<Result<Script>> =>
    bind(decoded<readonly Hit[]>('ast-grep', await scanner(text)), async (hits): Promise<Result<Script>> => {
        const script = _script(hits, context, text);
        const expanded = await Promise.all(script.commands.map(async (command) => map(all(await Promise.all(_bodies(command).map((body) => _parse(scanner, body, { fed: command.fed, nested: true })))), (nested) => ({ command, nested }))));
        return map(all(expanded), (held): Script => {
            const nested = held.flatMap(({ nested: scripts }) => scripts);
            return { commands: held.flatMap(({ command, nested: scripts }) => [command, ...scripts.flatMap(({ commands }) => commands)]), sequences: [...script.sequences, ...nested.flatMap(({ sequences }) => sequences)], deletes: [...script.deletes, ...nested.flatMap(({ deletes }) => deletes)] };
        });
    });

const parse = (scanner: Scanner, command: string): Promise<Result<Script>> => _parse(scanner, command, { fed: false, nested: false });

// --- [SPANS]

const offset = (command: Command, index: number): number => command.words.length - command.invocations.slice(index).reduce((count, invocation) => count + invocation.length, 0);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner, Script, Span };
export { offset, parse, SCAN };
