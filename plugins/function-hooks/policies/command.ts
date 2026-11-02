import { all, bind, fault, map, none, type Option, ok, type Result, some } from '../composition.ts';
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
    readonly looped: boolean;
    readonly nested: boolean;
    readonly written: readonly string[];
}
interface Script {
    readonly commands: readonly Command[];
    readonly background: boolean;
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: Span };
}

type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture; readonly SCOPE?: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'heredoc' | 'herestring' | 'written'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'assignment' | 'expansion'; readonly metaVariables: { readonly single: { readonly SCOPE?: Capture } } }
        | { readonly ruleId: 'fed' | 'looped' | 'unbounded' | 'background' }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORD = /^(?!\d*[<>](?!\()|&>)./su;
const STACK = /^(?:-|[+-]\d+)$/u;
const LITERAL = /^(?:[\w./~@%+:,=-]*|'[\w./~@%+:,=-]*'|"[\w./~@%+:,=-]*")$/u;
const REFERENCE = /^\$\{?(?<name>\w+)\}?$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const exited = (system: System, argv: Invocation, init: { readonly cwd: string; readonly stdin: string }): Promise<Result<string>> =>
    system.run(argv, init).then(
        ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault({ kind: 'exited', program: argv[0], code: exitCode, output: stderr.trim() })),
        (cause: unknown) => fault({ kind: 'thrown', program: argv[0], cause }),
    );

const decoded = <A>(program: string, output: string): Result<A> => {
    try {
        return ok(JSON.parse(output));
    } catch (cause) {
        return fault({ kind: 'thrown', program, cause });
    }
};

const printed = async <A>(system: System, argv: Invocation, cwd: string): Promise<Result<A>> => bind(await exited(system, argv, { cwd, stdin: '' }), (output) => decoded<A>(argv[0], output));

const parse = async (system: System, command: string): Promise<Result<Script>> => {
    const scripted = async (scan: Invocation, source: string, context: Pick<Command, 'cwd' | 'fed' | 'looped' | 'nested'>): Promise<Result<Script>> =>
        bind(
            bind(await exited(system, scan, { cwd: system.plugin, stdin: source }), (output) => decoded<readonly Hit[]>(scan[0], output)),
            async (hits) => {
                const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
                const inside = (outer: Capture | undefined, { range: { byteOffset } }: Capture): boolean => outer === undefined || (byteOffset.start >= outer.range.byteOffset.start && byteOffset.end <= outer.range.byteOffset.end);
                const unbounded = new Set(sorted.flatMap((hit) => (hit.ruleId === 'unbounded' ? [hit.range.byteOffset.start] : [])));
                const bindings = sorted.flatMap((hit) => {
                    if (hit.ruleId !== 'assignment') {
                        return [];
                    }
                    const cut = hit.text.indexOf('=');
                    const value = hit.text.slice(cut + 1);
                    return [{ hit, name: cut < 0 ? hit.text : hit.text.slice(0, cut), value: cut >= 0 && LITERAL.test(value) ? some(unquoted(value)) : none }];
                });
                const references = sorted.flatMap((hit) => {
                    const name = hit.ruleId === 'expansion' ? REFERENCE.exec(hit.text)?.groups?.name : undefined;
                    const bound = bindings.findLast((binding) => binding.name === name && binding.hit.range.byteOffset.end <= hit.range.byteOffset.start && inside(binding.hit.metaVariables.single.SCOPE, hit));
                    return bound?.value.kind === 'some' ? [{ span: hit.range.byteOffset, value: bound.value.value }] : [];
                });
                const resolved = ({ text, range: { byteOffset } }: Capture): string => {
                    const parts = references.filter(({ span }) => span.start >= byteOffset.start && span.end <= byteOffset.end);
                    const bytes = new TextEncoder().encode(text);
                    const slice = (start: number, end?: number) => new TextDecoder().decode(bytes.subarray(start - byteOffset.start, end === undefined ? undefined : end - byteOffset.start));
                    return unquoted([...parts.map(({ span, value }, index) => `${slice(parts[index - 1]?.span.end ?? byteOffset.start, span.start)}${value}`), slice(parts.at(-1)?.span.end ?? byteOffset.start)].join(''));
                };
                const owned = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' || hit.ruleId === 'written' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
                    .values()
                    .flatMap((own) => {
                        const hit = own.find((other) => other.ruleId === 'command');
                        if (hit === undefined) {
                            return [];
                        }
                        const words = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')]
                            .filter(({ text }) => WORD.test(text))
                            .map((capture) => ({ text: resolved(capture), span: capture.range.byteOffset, unbounded: unbounded.has(capture.range.byteOffset.start) }));
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
                                looped: context.looped || own.some((other) => other.ruleId === 'looped'),
                                nested: context.nested,
                                written: own.flatMap((other) => (other.ruleId === 'written' ? [resolved(other)] : [])),
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
                const inner = all(
                    await Promise.all(
                        located.flatMap(({ invocations: chain, stdin, cwd, fed, looped }) => {
                            const last = chain.slice(-1);
                            return [...last, ...last.flatMap((invocation) => invocations(operands(invocation).inner))]
                                .flatMap((invocation) => [...operands(invocation).bodies, ...(declared(invocation[0]).shell !== undefined && waits(invocation) ? stdin : [])])
                                .map((body) => scripted(scan, body, { cwd, fed, looped, nested: true }));
                        }),
                    ),
                );
                return map(inner, (scripts) => ({
                    commands: [...located.map(({ hit, stdin, targets, ...parsed }) => parsed), ...scripts.flatMap(({ commands }) => commands)],
                    background: sorted.some(({ ruleId }) => ruleId === 'background') || scripts.some(({ background }) => background),
                }));
            },
        );
    const value = 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number';
    const owner = {
        owner: { any: [{ inside: { kind: 'command', pattern: '$BODY' } }, { inside: { kind: 'heredoc_redirect', inside: { kind: 'redirected_statement', matches: 'statement' } } }, { inside: { kind: 'redirected_statement', matches: 'statement' } }] },
        statement: { has: { field: 'body', matches: 'last' } },
        last: {
            any: [
                { kind: 'command', pattern: '$BODY' },
                { kind: 'list, pipeline, negated_command', has: { matches: 'last', nthChild: { position: 1, reverse: true } } },
            ],
        },
    };
    const scope = {
        scope: { kind: 'program, subshell, command_substitution, process_substitution, pipeline' },
        scoped: { any: [{ inside: { stopBy: { matches: 'scope' }, kind: 'program' } }, { inside: { stopBy: { matches: 'scope' }, matches: 'scope', not: { kind: 'program' }, pattern: '$SCOPE' } }] },
    };
    const scanner = [
        'scan',
        '--stdin',
        '--config',
        '/dev/null',
        '--json=compact',
        '--inline-rules',
        [
            { id: 'command', utils: scope, rule: { kind: 'command', any: [{ pattern: '$CMD $$$ARGS' }, { pattern: '$CMD' }], matches: 'scoped' } },
            { id: 'operand', utils: owner, rule: { kind: value, follows: { kind: value }, inside: { kind: 'file_redirect', matches: 'owner' } } },
            { id: 'heredoc', utils: owner, rule: { kind: 'heredoc_body', inside: { kind: 'heredoc_redirect', matches: 'owner' } } },
            { id: 'herestring', utils: owner, rule: { kind: value, inside: { kind: 'herestring_redirect', matches: 'owner' } } },
            { id: 'written', utils: owner, rule: { kind: value, inside: { kind: 'file_redirect', field: 'destination', regex: '^(?:1?>|&>)', matches: 'owner', inside: { kind: 'redirected_statement', has: { kind: 'heredoc_redirect' } } } } },
            {
                id: 'assignment',
                utils: scope,
                rule: { any: [{ kind: 'variable_assignment', not: { inside: { kind: 'command, c_style_for_statement' } } }, { kind: 'variable_name', inside: { kind: 'for_statement', field: 'variable' } }], matches: 'scoped' },
            },
            { id: 'expansion', utils: scope, rule: { kind: 'simple_expansion, expansion', regex: String.raw`^\$(?:[A-Za-z_]\w*|\{[A-Za-z_]\w*\})$`, matches: 'scoped' } },
            { id: 'looped', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'while_statement, for_statement, c_style_for_statement' } } },
            {
                id: 'fed',
                utils: {
                    input: { any: [{ kind: 'heredoc_redirect' }, { kind: 'herestring_redirect' }, { kind: 'file_redirect', regex: '^0?<' }] },
                    tail: { any: [{ inside: { kind: 'redirected_statement', field: 'body', has: { matches: 'input' } } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list, pipeline, negated_command', matches: 'tail' } }] },
                    stage: {
                        any: [
                            { matches: 'tail' },
                            { inside: { kind: 'pipeline' }, not: { nthChild: 1 } },
                            { nthChild: 1, inside: { kind: 'list, pipeline, negated_command', matches: 'stage' } },
                            { inside: { kind: 'pipeline', inside: { kind: 'heredoc_redirect' } } },
                        ],
                    },
                },
                rule: { kind: 'command', any: [{ has: { matches: 'input' } }, { matches: 'stage' }, { inside: { stopBy: 'end', not: { kind: 'list, pipeline, negated_command' }, matches: 'stage' } }] },
            },
            { id: 'background', rule: { pattern: '&' } },
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
    return bind(await exited(['mise', 'which', 'ast-grep'], ''), (path) => scripted([path.trim(), ...scanner], command, { cwd: placed(system.cwd, none, none), fed: false, looped: false, nested: false }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { parse, printed, type Script };
