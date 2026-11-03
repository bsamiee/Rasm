import { all, bind, fault, map, none, type Option, ok, type Result, some, toArray } from '../composition.ts';
import { declared, type Invocation, invocations, operands, placed, staged, unquoted, waits, within } from './invocation.ts';
import type { System } from './policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Word {
    readonly text: string;
    readonly raw: string;
    readonly span: Span;
    readonly unbounded: boolean;
}
interface Command {
    readonly words: readonly Word[];
    readonly invocations: readonly Invocation[];
    readonly parts: readonly { readonly assigned: readonly Word[]; readonly words: readonly Word[] }[];
    readonly extent: Span;
    readonly cwd: Option<string>;
    readonly fed: boolean;
    readonly piping: boolean;
    readonly substituted: boolean;
    readonly looped: boolean;
    readonly origin: Origin;
    readonly written: readonly string[];
}
interface Script {
    readonly commands: readonly Command[];
    readonly jobs: readonly (Span & { readonly origin: Origin })[];
    readonly assignments: readonly { readonly name: string; readonly origin: Origin }[];
}
interface Capture {
    readonly text: string;
    readonly range: { readonly byteOffset: Span };
}
interface Context {
    readonly cwd: Option<string>;
    readonly fed: boolean;
    readonly looped: boolean;
    readonly origin: Origin;
    readonly visited: ReadonlySet<string>;
}

interface Binding {
    readonly name: string;
    readonly end: number;
    readonly scope: Capture | undefined;
    readonly value: Option<string>;
}

type Origin = 'call' | 'body' | 'file';
type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture; readonly SCOPE?: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'heredoc' | 'herestring' | 'written' | 'statement'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'assignment' | 'rebinding' | 'expansion'; readonly metaVariables: { readonly single: { readonly SCOPE?: Capture } } }
        | { readonly ruleId: 'chained'; readonly metaVariables: { readonly single: { readonly CHAIN: Capture } } }
        | { readonly ruleId: 'fed' | 'piping' | 'substituted' | 'looped' | 'unbounded' | 'background' }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = /\.(?:sh|bash|zsh)$/u;
const WORD = /^(?!\d*[<>](?!\()|&>)./su;
const STACK = /^(?:-|[+-]\d+)$/u;
const NAME = /^[A-Za-z_]\w*/u;
const LITERAL = /^(?:[\w./~@%+:,=-]+|'[\w./~@%+:,=-]+'|"[\w./~@%+:,=-]+"|"?\$!"?)$/u;
const REFERENCE = /^\$\{?(?<name>\w+)\}?$/u;
const REFERENCES = /\$(?:(?<bare>[A-Za-z_]\w*)|\{(?<braced>[A-Za-z_]\w*)\})/gu;
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
    guarded: {
        any: [{ inside: { stopBy: { matches: 'scope' }, kind: 'if_statement, case_statement, while_statement, for_statement, c_style_for_statement, function_definition' } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list' } }],
    },
    chained: {
        nthChild: { position: 1, reverse: true },
        not: { inside: { stopBy: { matches: 'scope' }, kind: 'if_statement, case_statement, while_statement, for_statement, c_style_for_statement, function_definition' } },
        all: [{ inside: { kind: 'list' } }, { inside: { stopBy: 'end', kind: 'list', not: { any: [{ inside: { kind: 'list' } }, { regex: String.raw`\|\|` }] }, pattern: '$CHAIN' } }],
    },
};
const SCANNER = [
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
            id: 'written',
            utils: OWNER,
            rule: { kind: VALUE, inside: { kind: 'file_redirect', field: 'destination', regex: '^(?:1?>|&>)', matches: 'owner', inside: { stopBy: { kind: 'redirected_statement' }, kind: 'redirected_statement', has: { kind: 'heredoc_redirect' } } } },
        },
        { id: 'statement', rule: { kind: 'redirected_statement', has: { field: 'body', kind: 'command', pattern: '$BODY' } } },
        { id: 'assignment', utils: SCOPE, rule: { kind: 'variable_assignment', not: { any: [{ inside: { kind: 'command, c_style_for_statement' } }, { matches: 'guarded' }] }, matches: 'scoped' } },
        { id: 'chained', utils: SCOPE, rule: { kind: 'variable_assignment', matches: 'chained' } },
        {
            id: 'rebinding',
            utils: SCOPE,
            rule: {
                any: [
                    { kind: 'variable_assignment', matches: 'guarded', not: { any: [{ inside: { kind: 'command' } }, { matches: 'chained' }] } },
                    { kind: 'variable_name', inside: { kind: 'for_statement, unset_command' } },
                    { kind: 'word', regex: String.raw`^[A-Za-z_]\w*$`, inside: { kind: 'command', has: { kind: 'command_name', regex: '^(?:read|readarray|mapfile|getopts|printf)$' } } },
                ],
                matches: 'scoped',
            },
        },
        { id: 'expansion', utils: SCOPE, rule: { kind: 'simple_expansion, expansion', regex: String.raw`^\$(?:[A-Za-z_]\w*|\{[A-Za-z_]\w*\})$`, matches: 'scoped' } },
        { id: 'looped', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'while_statement, for_statement, c_style_for_statement' } } },
        { id: 'substituted', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'process_substitution' } } },
        {
            id: 'piping',
            rule: {
                kind: 'command',
                any: [{ inside: { kind: 'pipeline' }, not: { nthChild: { position: 1, reverse: true } } }, { inside: { kind: 'redirected_statement', not: { nthChild: { position: 1, reverse: true } }, inside: { kind: 'pipeline' } } }],
            },
        },
        {
            id: 'fed',
            utils: {
                input: { any: [{ kind: 'heredoc_redirect' }, { kind: 'herestring_redirect' }, { kind: 'file_redirect', regex: '^0?<' }] },
                tail: { any: [{ inside: { kind: 'redirected_statement', field: 'body', has: { matches: 'input' } } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list, pipeline, negated_command', matches: 'tail' } }] },
                piped: {
                    any: [{ inside: { kind: 'pipeline' }, not: { nthChild: 1 } }, { nthChild: 1, inside: { kind: 'list, pipeline, negated_command', matches: 'piped' } }, { inside: { kind: 'pipeline', inside: { kind: 'heredoc_redirect' } } }],
                },
                stage: { any: [{ matches: 'tail' }, { matches: 'piped' }] },
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

const spliced = (source: Uint8Array, base: number, edits: readonly (Span & { readonly text: string })[]): string => {
    const decoder = new TextDecoder();
    const sorted = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const between = (start: number, end?: number): string => decoder.decode(source.subarray(start - base, end === undefined ? undefined : end - base));
    return [...sorted.map(({ start, text }, index) => `${between(sorted[index - 1]?.end ?? base, start)}${text}`), between(sorted.at(-1)?.end ?? base)].join('');
};

const inside = (outer: Capture | undefined, { range: { byteOffset } }: Capture): boolean => outer === undefined || (byteOffset.start >= outer.range.byteOffset.start && byteOffset.end <= outer.range.byteOffset.end);

const resolver = (hits: readonly Hit[], source: Uint8Array): ((capture: Capture) => string) => {
    const lookup = (known: readonly Binding[], name: string | undefined, at: Capture): Option<string> => {
        const [bound, ...more] = known.filter((binding) => binding.name === name && inside(binding.scope, at));
        return bound !== undefined && more.length === 0 && bound.end <= at.range.byteOffset.start ? bound.value : none;
    };
    const bindings = hits.reduce<readonly Binding[]>((bound, hit) => {
        if (hit.ruleId !== 'assignment' && hit.ruleId !== 'rebinding' && hit.ruleId !== 'chained') {
            return bound;
        }
        const [name] = NAME.exec(hit.text) ?? [];
        if (name === undefined) {
            return bound;
        }
        const values = [...hit.text.slice(name.length + 1).matchAll(REFERENCES)].map((match) => ({ match, value: lookup(bound, match.groups?.bare ?? match.groups?.braced, hit) }));
        const value = values.reduce((text, { match, value: found }) => (found.kind === 'some' ? text.replace(match[0], found.value) : text), hit.text.slice(name.length + 1));
        const literal = hit.ruleId !== 'rebinding' && hit.text[name.length] === '=' && values.every(({ value: found }) => found.kind === 'some') && LITERAL.test(value);
        return bound.concat({ name, end: hit.range.byteOffset.end, scope: hit.ruleId === 'chained' ? hit.metaVariables.single.CHAIN : hit.metaVariables.single.SCOPE, value: literal ? some(unquoted(value)) : none });
    }, []);
    const references = hits.flatMap((hit) => {
        const value = hit.ruleId === 'expansion' ? lookup(bindings, REFERENCE.exec(hit.text)?.groups?.name, hit) : none;
        return value.kind === 'some' ? [{ ...hit.range.byteOffset, text: value.value }] : [];
    });
    return ({ range: { byteOffset } }) =>
        unquoted(
            spliced(
                source.subarray(byteOffset.start, byteOffset.end),
                byteOffset.start,
                references.filter(({ start, end }) => start >= byteOffset.start && end <= byteOffset.end),
            ),
        );
};

const parse = (system: System, command: string): Promise<Result<Script>> => {
    const destination = (chain: readonly Invocation[]): readonly (string | undefined)[] =>
        chain
            .filter(([program]) => declared(program).chdir !== undefined)
            .slice(0, 1)
            .map((mover) => [...operands(mover).inputs, ...(declared(mover[0]).chdir ?? [])][0]);
    const sourced = (invocation: Invocation, head: Word | undefined): readonly string[] => {
        const { shell } = declared(invocation[0]);
        const { options, inputs } = operands(invocation);
        if (shell !== undefined && shell !== 'joined') {
            return options.some((name) => shell.includes(name)) ? [] : inputs.slice(0, 1);
        }
        return head !== undefined && !head.unbounded && head.text.includes('/') && SCRIPT.test(head.text) ? [head.text] : [];
    };
    const scripted = async (source: string, context: Context): Promise<Result<Script>> =>
        bind(
            bind(await exited(system, [system.scanner, ...SCANNER], { cwd: system.plugin, stdin: source }), (output) => decoded<readonly Hit[]>(system.scanner, output)),
            async (hits) => {
                const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
                const unbounded = new Set(sorted.flatMap((hit) => (hit.ruleId === 'unbounded' ? [hit.range.byteOffset.start] : [])));
                const resolved = resolver(sorted, new TextEncoder().encode(source));
                const owned = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' || hit.ruleId === 'written' || hit.ruleId === 'statement' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
                    .values()
                    .flatMap((own) => {
                        const hit = own.find((other) => other.ruleId === 'command');
                        if (hit === undefined) {
                            return [];
                        }
                        const has = (rule: Hit['ruleId']): boolean => own.some(({ ruleId }) => ruleId === rule);
                        const words = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')]
                            .filter(({ text }) => WORD.test(text))
                            .map((capture) => ({ text: resolved(capture), raw: capture.text, span: capture.range.byteOffset, unbounded: unbounded.has(capture.range.byteOffset.start) }));
                        const chain = invocations(words.map(({ text }) => text));
                        return [
                            {
                                hit,
                                words,
                                invocations: chain,
                                parts: staged(words, chain),
                                extent: (own.find((other) => other.ruleId === 'statement') ?? hit).range.byteOffset,
                                stdin: own
                                    .filter((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring')
                                    .slice(-1)
                                    .map((given) => (given.ruleId === 'herestring' ? resolved(given) : given.text)),
                                targets: destination(chain),
                                fed: context.fed || has('fed'),
                                piping: has('piping'),
                                substituted: has('substituted'),
                                looped: context.looped || has('looped') || chain.some(([program]) => declared(program).repeats === true),
                                origin: context.origin,
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
                        .reduce((folder, target) => (target === undefined || target.includes('$') || STACK.test(target) ? none : placed(target, folder, system.home)), context.cwd),
                }));
                const inner = all(await Promise.all(located.flatMap(({ invocations: chain, parts, stdin, cwd, fed, looped }) => descend(chain, parts.at(-1)?.words[0], stdin, { cwd, fed, looped, origin: context.origin, visited: context.visited }))));
                const names = sorted.flatMap((hit) => (hit.ruleId === 'assignment' || hit.ruleId === 'rebinding' || hit.ruleId === 'chained' ? (NAME.exec(hit.text)?.slice(0, 1) ?? []) : []));
                return map(inner, (scripts) => ({
                    commands: [...located.map(({ hit, stdin, targets, ...parsed }) => parsed), ...scripts.flatMap(({ commands }) => commands)],
                    jobs: [...sorted.flatMap((hit) => (hit.ruleId === 'background' ? [{ ...hit.range.byteOffset, origin: context.origin }] : [])), ...scripts.flatMap(({ jobs }) => jobs)],
                    assignments: [...names.map((name) => ({ name, origin: context.origin })), ...scripts.flatMap(({ assignments }) => assignments)],
                }));
            },
        );
    const tracked = async (file: string): Promise<boolean> =>
        (
            await Promise.all(
                toArray(system.root)
                    .filter((root) => within(file, root))
                    .map((root) => exited(system, ['git', 'ls-files', '--error-unmatch', '--', file], { cwd: root, stdin: '' })),
            )
        ).some(({ kind }) => kind === 'ok');
    const filed = async (file: string, context: Context): Promise<Result<Script>> => {
        const [known, text] = await Promise.all([tracked(file), system.read(file).then(some, () => none)]);
        return known || text.kind === 'none' ? ok({ commands: [], jobs: [], assignments: [] }) : scripted(text.value, { ...context, origin: 'file', visited: new Set([...context.visited, file]) });
    };
    const descend = (chain: readonly Invocation[], head: Word | undefined, stdin: readonly string[], context: Context): readonly Promise<Result<Script>>[] => {
        const last = chain.slice(-1);
        const bodies = [...last, ...last.flatMap((invocation) => invocations(operands(invocation).inner))].flatMap((invocation) => [...operands(invocation).bodies, ...(declared(invocation[0]).shell !== undefined && waits(invocation) ? stdin : [])]);
        const files = last
            .flatMap((invocation) => sourced(invocation, head))
            .flatMap((path) => toArray(placed(path, context.cwd, system.home)))
            .filter((file) => !context.visited.has(file));
        return [...bodies.map((body) => scripted(body, { ...context, origin: context.origin === 'file' ? 'file' : 'body' })), ...files.map((file) => filed(file, context))];
    };
    return scripted(command, { cwd: placed(system.cwd, none, none), fed: false, looped: false, origin: 'call', visited: new Set() });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Command, type Origin, parse, printed, type Script, spliced, type Word };
