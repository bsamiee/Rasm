import { all, bind, fault, map, ok, type Result } from '../composition.ts';
import { basename, declared, type Invocation, invocations, operands, unquoted, waits } from './invocation.ts';
import type { System } from './policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Scanner = (text: string) => Promise<Result<string>>;

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Command {
    readonly source: string;
    readonly words: readonly { readonly text: string; readonly span: Span }[];
    readonly invocations: readonly Invocation[];
    readonly bodies: readonly string[];
    readonly fed: boolean;
    readonly nested: boolean;
}
interface Script {
    readonly commands: readonly Command[];
    readonly sequences: readonly (readonly { readonly programs: readonly string[]; readonly writes: readonly string[]; readonly reads: readonly string[] }[])[];
    readonly deletes: readonly { readonly program: string; readonly message: string }[];
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
const _SCAN = [
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
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SCAN]

const scanner = async ({ run, root, cwd }: System): Promise<Scanner> => {
    const folder = root.kind === 'some' ? root.value : cwd;
    const exited = (argv: Invocation, stdin: string): Promise<Result<string>> =>
        run(argv, { cwd: folder, stdin }).then(
            ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault({ kind: 'exited', subject: basename(argv[0]), code: exitCode, output: stderr.trim() })),
            (cause: unknown) => fault({ kind: 'unstarted', subject: basename(argv[0]), cause }),
        );
    const [program, ...args] = _SCAN;
    const resolved = map(await exited(['mise', 'which', program], ''), (path) => [path.trim(), ...args] as const);
    return async (text) => bind(resolved, (argv) => exited(argv, text));
};

// --- [PARSE]

const parse = async (scan: Scanner, source: string, { fed, nested }: Pick<Command, 'fed' | 'nested'>): Promise<Result<Script>> =>
    bind(
        bind(await scan(source), (printed): Result<readonly Hit[]> => {
            try {
                return ok(JSON.parse(printed));
            } catch (cause) {
                return fault({ kind: 'undecoded', subject: _SCAN[0], cause });
            }
        }),
        async (hits) => {
            const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
            const key = (capture: Capture | undefined): string => (capture === undefined ? '' : `${capture.range.byteOffset.start}:${capture.range.byteOffset.end}`);
            const owned = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
                .values()
                .flatMap((own) => {
                    const hit = own.find((other) => other.ruleId === 'command');
                    if (hit === undefined) {
                        return [];
                    }
                    const words = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')].filter(({ text }) => _WORD.test(text)).map(({ text, range }) => ({ text: unquoted(text), span: range.byteOffset }));
                    const given = own.findLast((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring');
                    const stdin = given === undefined ? [] : [given.ruleId === 'herestring' ? unquoted(given.text) : given.text];
                    const chain = invocations(words.map(({ text }) => text));
                    const last = chain.slice(-1);
                    const bodies = [...last, ...last.flatMap((invocation) => invocations(operands(invocation).inner))].flatMap((invocation) => [...operands(invocation).bodies, ...(declared(invocation[0]).shell !== undefined && waits(invocation) ? stdin : [])]);
                    return [{ command: { source, words, invocations: chain, bodies, nested, fed: fed || own.some((other) => other.ruleId === 'fed') }, hit }];
                })
                .toArray();
            const steps = sorted
                .filter((hit) => hit.ruleId === 'step')
                .map(({ range: { byteOffset }, metaVariables }) => {
                    const inside = ({ range }: Capture): boolean => range.byteOffset.start >= byteOffset.start && range.byteOffset.end <= byteOffset.end;
                    const scope = key(metaVariables.single.SCOPE);
                    const contained = owned.filter(({ hit }) => inside(hit));
                    return {
                        sequence: key(metaVariables.single.SEQ),
                        programs: contained.flatMap(({ command }) => command.invocations.slice(-1)).map(([program]) => program),
                        writes: [
                            ...sorted.flatMap((hit) => (hit.ruleId === 'write' && inside(hit) && key(hit.metaVariables.single.SCOPE) === scope ? [hit.metaVariables.single.NAME.text] : [])),
                            ...contained
                                .filter(({ hit }) => key(hit.metaVariables.single.SCOPE) === scope)
                                .flatMap(({ command }) => command.invocations)
                                .flatMap(([program]) => declared(program).writes ?? []),
                        ],
                        reads: ['PWD', ...sorted.flatMap((hit) => (hit.ruleId === 'read' && inside(hit) ? [hit.text] : []))],
                    };
                });
            const script = { commands: [], sequences: [...Map.groupBy(steps, ({ sequence }) => sequence).values()], deletes: sorted.flatMap((hit) => (hit.ruleId === 'delete' ? [{ program: hit.metaVariables.single.PROGRAM.text, message: hit.message }] : [])) };
            const expanded = await Promise.all(owned.map(async ({ command }) => map(all(await Promise.all(command.bodies.map((body) => parse(scan, body, { fed: command.fed, nested: true })))), (inner) => [{ commands: [command], sequences: [], deletes: [] }, ...inner])));
            return map(all(expanded), (lists): Script => {
                const scripts = [script, ...lists.flat()];
                return { commands: scripts.flatMap(({ commands }) => commands), sequences: scripts.flatMap(({ sequences }) => sequences), deletes: scripts.flatMap(({ deletes }) => deletes) };
            });
        },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Command, Scanner, Script, Span };
export { parse, scanner };
