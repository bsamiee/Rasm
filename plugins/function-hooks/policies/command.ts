import { all, bind, fault, map, none, type Option, ok, type Result } from '../composition.ts';
import { declared, type Invocation, invocations, operands, placed, unquoted, waits } from './invocation.ts';
import type { System } from './policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Command {
    readonly words: readonly { readonly text: string; readonly span: Span; readonly unbounded: boolean }[];
    readonly invocations: readonly Invocation[];
    readonly cwd: Option<string>;
    readonly fed: boolean;
    readonly nested: boolean;
}
interface Script {
    readonly commands: readonly Command[];
    readonly sequences: readonly (readonly { readonly programs: readonly string[]; readonly writes: readonly string[]; readonly reads: readonly string[] }[])[];
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
        | { readonly ruleId: 'fed' | 'read' | 'unbounded' }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORD = /^(?!\d*[<>]|&>)./su;
const STACK = /^(?:-|[+-]\d+)$/u;
const VALUE = 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number';
const OWNER = {
    owner: { any: [{ inside: { kind: 'command', pattern: '$BODY' } }, { inside: { kind: 'heredoc_redirect', inside: { kind: 'redirected_statement', matches: 'statement' } } }, { inside: { kind: 'redirected_statement', matches: 'statement' } }] },
    statement: { has: { field: 'body', matches: 'last' } },
    last: {
        any: [
            { kind: 'command', pattern: '$BODY' },
            { kind: 'list, pipeline, negated_command', has: { matches: 'last', nthChild: { position: 1, reverse: true } } },
        ],
    },
};
const SCOPE = {
    scope: { kind: 'program, subshell, command_substitution, process_substitution, pipeline' },
    scoped: { any: [{ inside: { stopBy: { matches: 'scope' }, kind: 'program' } }, { inside: { stopBy: { matches: 'scope' }, matches: 'scope', not: { kind: 'program' }, pattern: '$SCOPE' } }] },
};
const SCAN = [
    'scan',
    '--stdin',
    '--config',
    '/dev/null',
    '--json=compact',
    '--inline-rules',
    [
        { id: 'command', utils: SCOPE, rule: { kind: 'command', any: [{ pattern: '$CMD $$$ARGS' }, { pattern: '$CMD' }], matches: 'scoped' } },
        { id: 'operand', utils: OWNER, rule: { kind: VALUE, follows: { kind: VALUE }, inside: { kind: 'file_redirect', matches: 'owner' } } },
        { id: 'heredoc', utils: OWNER, rule: { kind: 'heredoc_body', inside: { kind: 'heredoc_redirect', matches: 'owner' } } },
        { id: 'herestring', utils: OWNER, rule: { kind: VALUE, inside: { kind: 'herestring_redirect', matches: 'owner' } } },
        {
            id: 'fed',
            utils: {
                input: { any: [{ kind: 'heredoc_redirect' }, { kind: 'herestring_redirect' }, { kind: 'file_redirect', regex: '^0?<' }] },
                tail: { any: [{ inside: { kind: 'redirected_statement', field: 'body', has: { matches: 'input' } } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list, pipeline, negated_command', matches: 'tail' } }] },
                stage: { any: [{ matches: 'tail' }, { inside: { kind: 'pipeline' }, not: { nthChild: 1 } }, { inside: { kind: 'pipeline', inside: { kind: 'heredoc_redirect' } } }] },
            },
            rule: { kind: 'command', any: [{ has: { matches: 'input' } }, { matches: 'stage' }, { inside: { stopBy: 'end', not: { kind: 'list, pipeline, negated_command' }, matches: 'stage' } }] },
        },
        {
            id: 'step',
            utils: { ...SCOPE, chained: { any: [{ kind: 'list' }, { kind: 'redirected_statement', has: { field: 'body', kind: 'list' } }] }, tested: { inside: { kind: 'if_statement, elif_clause', field: 'condition' } } },
            rule: {
                kind: 'redirected_statement, variable_assignment, variable_assignments, command, declaration_command, unset_command, test_command, negated_command, for_statement, c_style_for_statement, while_statement, if_statement, case_statement, pipeline, subshell, compound_statement, function_definition',
                matches: 'scoped',
                not: { any: [{ matches: 'chained' }, { matches: 'tested' }, { inside: { stopBy: { not: { matches: 'chained' } }, matches: 'chained', all: [{ matches: 'tested' }] } }] },
                any: [{ inside: { stopBy: { not: { matches: 'chained' } }, kind: 'program' } }, { inside: { stopBy: { not: { matches: 'chained' } }, kind: 'compound_statement, do_group, subshell, if_statement, elif_clause, else_clause, case_item, command_substitution, process_substitution', pattern: '$SEQ' } }],
            },
        },
        { id: 'write', utils: SCOPE, rule: { kind: 'variable_assignment', has: { field: 'name', pattern: '$NAME' }, not: { inside: { kind: 'command' } }, matches: 'scoped' } },
        { id: 'read', rule: { kind: 'variable_name', any: [{ inside: { kind: 'simple_expansion, expansion' } }, { inside: { stopBy: 'end', kind: 'arithmetic_expansion' } }] } },
        {
            id: 'unbounded',
            rule: {
                inside: { kind: 'command' },
                any: [
                    { kind: 'simple_expansion, expansion, command_substitution, process_substitution, arithmetic_expansion' },
                    { kind: 'string, concatenation', has: { stopBy: 'end', kind: 'simple_expansion, expansion, command_substitution, arithmetic_expansion' } },
                    { kind: 'word', regex: String.raw`[*?\[{]` },
                    { kind: 'concatenation', has: { kind: 'word', regex: String.raw`[*?\[{]` } },
                    { kind: 'word, string, raw_string', regex: String.raw`^["']?(?:~[^/"']*|\.{1,2}|\*+|\.\*)?(?:/+(?:\.{1,2}|\*+|\.\*)?)*["']?$` },
                ],
            },
        },
    ]
        .map((rule) => JSON.stringify({ language: 'bash', ...rule }))
        .join('\n---\n'),
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const parse = async (system: System, command: string): Promise<Result<Script>> => {
    const exited = (argv: Invocation, stdin: string): Promise<Result<string>> =>
        system.run(argv, { cwd: system.root.kind === 'some' ? system.root.value : system.cwd, stdin }).then(
            ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault({ kind: 'exited', program: argv[0], code: exitCode, output: stderr.trim() })),
            (cause: unknown) => fault({ kind: 'thrown', program: argv[0], cause }),
        );
    const scripted = async (scan: Invocation, source: string, context: Pick<Command, 'cwd' | 'fed' | 'nested'>): Promise<Result<Script>> =>
        bind(
            bind(await exited(scan, source), (printed): Result<readonly Hit[]> => {
                try {
                    return ok(JSON.parse(printed));
                } catch (cause) {
                    return fault({ kind: 'thrown', program: scan[0], cause });
                }
            }),
            async (hits) => {
                const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
                const key = (capture: Capture | undefined): string => (capture === undefined ? '' : `${capture.range.byteOffset.start}:${capture.range.byteOffset.end}`);
                const inside = (outer: Capture | undefined, { range: { byteOffset } }: Capture): boolean => outer === undefined || (byteOffset.start >= outer.range.byteOffset.start && byteOffset.end <= outer.range.byteOffset.end);
                const unbounded = new Set(sorted.flatMap((hit) => (hit.ruleId === 'unbounded' ? [hit.range.byteOffset.start] : [])));
                const owned = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
                    .values()
                    .flatMap((own) => {
                        const hit = own.find((other) => other.ruleId === 'command');
                        if (hit === undefined) {
                            return [];
                        }
                        const words = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')]
                            .filter(({ text }) => WORD.test(text))
                            .map(({ text, range: { byteOffset } }) => ({ text: unquoted(text), span: byteOffset, unbounded: unbounded.has(byteOffset.start) }));
                        const chain = invocations(words.map(({ text }) => text));
                        return [
                            {
                                hit,
                                words,
                                invocations: chain,
                                stdin: own
                                    .filter((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring')
                                    .slice(-1)
                                    .map((given) => (given.ruleId === 'herestring' ? unquoted(given.text) : given.text)),
                                targets: chain
                                    .filter(([program]) => declared(program).chdir !== undefined)
                                    .slice(0, 1)
                                    .map((mover) => [...operands(mover).inputs, ...(declared(mover[0]).chdir ?? [])][0]),
                                fed: context.fed || own.some((other) => other.ruleId === 'fed'),
                                nested: context.nested,
                            },
                        ];
                    })
                    .toArray();
                const located = owned.map((part) => ({
                    ...part,
                    cwd: owned
                        .filter(({ hit }) => hit.range.byteOffset.end <= part.hit.range.byteOffset.start && inside(hit.metaVariables.single.SCOPE, part.hit))
                        .flatMap(({ targets }) => targets)
                        .reduce((folder, target) => (target === undefined || STACK.test(target) ? none : placed(target, folder, system.home)), context.cwd),
                }));
                const sequences = Map.groupBy(
                    sorted
                        .filter((hit) => hit.ruleId === 'step')
                        .map((step) => {
                            const scope = key(step.metaVariables.single.SCOPE);
                            const contained = located.filter(({ hit }) => inside(step, hit));
                            return {
                                sequence: key(step.metaVariables.single.SEQ),
                                programs: contained.flatMap(({ invocations: chain }) => chain.slice(-1)).map(([program]) => program),
                                writes: [
                                    ...sorted.flatMap((hit) => (hit.ruleId === 'write' && inside(step, hit) && key(hit.metaVariables.single.SCOPE) === scope ? [hit.metaVariables.single.NAME.text] : [])),
                                    ...contained
                                        .filter(({ hit }) => key(hit.metaVariables.single.SCOPE) === scope)
                                        .flatMap(({ targets }) => targets)
                                        .map(() => 'PWD'),
                                ],
                                reads: ['PWD', ...sorted.flatMap((hit) => (hit.ruleId === 'read' && inside(step, hit) ? [hit.text] : []))],
                            };
                        }),
                    ({ sequence }) => sequence,
                ).values();
                const inner = all(
                    await Promise.all(
                        located.flatMap(({ invocations: chain, stdin, cwd, fed }) => {
                            const last = chain.slice(-1);
                            return [...last, ...last.flatMap((invocation) => invocations(operands(invocation).inner))]
                                .flatMap((invocation) => [...operands(invocation).bodies, ...(declared(invocation[0]).shell !== undefined && waits(invocation) ? stdin : [])])
                                .map((body) => scripted(scan, body, { cwd, fed, nested: true }));
                        }),
                    ),
                );
                return map(inner, (scripts) => ({
                    commands: [...located.map(({ hit, stdin, targets, ...parsed }) => parsed), ...scripts.flatMap(({ commands }) => commands)],
                    sequences: [...sequences, ...scripts.flatMap((script) => script.sequences)],
                }));
            },
        );
    return bind(await exited(['mise', 'which', 'ast-grep'], ''), (path) => scripted([path.trim(), ...SCAN], command, { cwd: placed(system.cwd, none, none), fed: false, nested: false }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Script };
export { parse };
