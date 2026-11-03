import { Option, Result } from '../monads.ts';
import { type Invocation, invocationParts, invocations, isInside, operands, programRow, readsStdin, resolvePath, unquote } from './invocation.ts';
import type { System } from './policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Word {
    readonly text: string;
    readonly raw: string;
    readonly literal: string;
    readonly span: Span;
    readonly expands: boolean;
}
interface Command {
    readonly words: readonly Word[];
    readonly invocations: readonly Invocation[];
    readonly parts: readonly { readonly assigned: readonly Word[]; readonly words: readonly Word[] }[];
    readonly extent: Span;
    readonly cwd: Option<string>;
    readonly hasInput: boolean;
    readonly pipesOutput: boolean;
    readonly inSubstitution: boolean;
    readonly inLoop: boolean;
    readonly inBusyLoop: boolean;
    readonly origin: Origin;
    readonly heredocFiles: readonly string[];
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

interface Binding {
    readonly hit: Hit;
    readonly name: string;
    readonly scope: Span | undefined;
    readonly value: Option<string>;
    readonly literal: Option<string>;
}

type Origin = 'call' | 'body' | 'file';
type Context = Pick<Command, 'cwd' | 'hasInput' | 'inLoop' | 'origin'> & { readonly visited: ReadonlySet<string> };
type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture; readonly SCOPE?: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'heredoc' | 'herestring' | 'heredoc-file' | 'statement'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'assignment' | 'dynamic-assignment' | 'expansion'; readonly metaVariables: { readonly single: { readonly SCOPE?: Capture } } }
        | { readonly ruleId: 'chain-assignment'; readonly metaVariables: { readonly single: { readonly CHAIN: Capture } } }
        | { readonly ruleId: 'has-input' | 'pipes-output' | 'in-substitution' | 'in-loop' | 'in-busy-loop' | 'expands' | 'background' }
    );

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = /\.(?:sh|bash|zsh)$/u;
const WORD = /^(?!\d*[<>](?!\()|&>)./su;
const DIR_STACK = /^(?:-|[+-]\d+)$/u;
const NAME = /^[A-Za-z_]\w*/u;
const LITERAL = /^(?:[\w./~@%+:,=-]+|'[\w./~@%+:,=-]+'|"[\w./~@%+:,=-]+"|"?\$!"?)$/u;
const REFERENCE = /^\$\{?(?<name>\w+)\}?$/u;
const REFERENCES = /\$(?:(?<bare>[A-Za-z_]\w*)|\{(?<braced>[A-Za-z_]\w*)\})/gu;
const UTILS = {
    value: { kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number' },
    owner: { any: [{ inside: { kind: 'command', pattern: '$BODY' } }, { inside: { kind: 'heredoc_redirect', inside: { kind: 'redirected_statement', matches: 'statement' } } }, { inside: { kind: 'redirected_statement', matches: 'statement' } }] },
    statement: { has: { field: 'body', matches: 'last' } },
    last: {
        any: [
            { kind: 'command', pattern: '$BODY' },
            { kind: 'list, pipeline, negated_command', has: { matches: 'last', nthChild: { position: 1, reverse: true } } },
        ],
    },
    scope: { kind: 'program, subshell, command_substitution, process_substitution, pipeline' },
    scoped: { any: [{ inside: { stopBy: { matches: 'scope' }, kind: 'program' } }, { inside: { stopBy: { matches: 'scope' }, matches: 'scope', not: { kind: 'program' }, pattern: '$SCOPE' } }] },
    control: { kind: 'if_statement, case_statement, while_statement, for_statement, c_style_for_statement, function_definition' },
    guarded: { any: [{ inside: { stopBy: { matches: 'scope' }, matches: 'control' } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list' } }] },
    chained: {
        nthChild: { position: 1, reverse: true },
        not: { inside: { stopBy: { matches: 'scope' }, matches: 'control' } },
        all: [{ inside: { kind: 'list' } }, { inside: { stopBy: 'end', kind: 'list', not: { any: [{ inside: { kind: 'list' } }, { regex: String.raw`\|\|` }] }, pattern: '$CHAIN' } }],
    },
    input: { any: [{ kind: 'heredoc_redirect' }, { kind: 'herestring_redirect' }, { kind: 'file_redirect', regex: '^0?<' }] },
    tail: { any: [{ inside: { kind: 'redirected_statement', field: 'body', has: { matches: 'input' } } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list, pipeline, negated_command', matches: 'tail' } }] },
    piped: { any: [{ inside: { kind: 'pipeline' }, not: { nthChild: 1 } }, { nthChild: 1, inside: { kind: 'list, pipeline, negated_command', matches: 'piped' } }, { inside: { kind: 'pipeline', inside: { kind: 'heredoc_redirect' } } }] },
    stage: { any: [{ matches: 'tail' }, { matches: 'piped' }] },
};
const SCAN_ARGUMENTS = [
    'scan',
    '--stdin',
    '--config',
    '/dev/null',
    '--json=compact',
    '--inline-rules',
    [
        { id: 'command', rule: { kind: 'command', any: [{ pattern: '$CMD $$$ARGS' }, { pattern: '$CMD' }], matches: 'scoped' } },
        { id: 'operand', rule: { matches: 'value', follows: { matches: 'value' }, inside: { kind: 'file_redirect', matches: 'owner' } } },
        { id: 'heredoc', rule: { kind: 'heredoc_body', inside: { kind: 'heredoc_redirect', matches: 'owner' } } },
        { id: 'herestring', rule: { matches: 'value', inside: { kind: 'herestring_redirect', matches: 'owner' } } },
        { id: 'heredoc-file', rule: { matches: 'value', inside: { kind: 'file_redirect', field: 'destination', regex: '^(?:1?>|&>)', matches: 'owner', inside: { stopBy: { kind: 'redirected_statement' }, kind: 'redirected_statement', has: { kind: 'heredoc_redirect' } } } } },
        { id: 'statement', rule: { kind: 'redirected_statement', has: { field: 'body', kind: 'command', pattern: '$BODY' } } },
        { id: 'assignment', rule: { kind: 'variable_assignment', not: { any: [{ inside: { kind: 'command, c_style_for_statement' } }, { matches: 'guarded' }] }, matches: 'scoped' } },
        { id: 'chain-assignment', rule: { kind: 'variable_assignment', matches: 'chained' } },
        {
            id: 'dynamic-assignment',
            rule: {
                any: [
                    { kind: 'variable_assignment', matches: 'guarded', not: { any: [{ inside: { kind: 'command' } }, { matches: 'chained' }] } },
                    { kind: 'variable_name', inside: { kind: 'for_statement, unset_command' } },
                    { kind: 'word', regex: String.raw`^[A-Za-z_]\w*$`, inside: { kind: 'command', has: { kind: 'command_name', regex: '^(?:read|readarray|mapfile|getopts|printf)$' } } },
                ],
                matches: 'scoped',
            },
        },
        { id: 'expansion', rule: { kind: 'simple_expansion, expansion', regex: String.raw`^\$(?:[A-Za-z_]\w*|\{[A-Za-z_]\w*\})$`, matches: 'scoped' } },
        { id: 'in-loop', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'while_statement, for_statement, c_style_for_statement' } } },
        { id: 'in-busy-loop', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'while_statement', has: { field: 'body', kind: 'do_group', not: { has: { stopBy: 'end', kind: 'command', not: { regex: '^(?::|true)$' } } } } } } },
        { id: 'in-substitution', rule: { kind: 'command', inside: { stopBy: 'end', kind: 'process_substitution' } } },
        {
            id: 'pipes-output',
            rule: {
                kind: 'command',
                any: [{ inside: { kind: 'pipeline' }, not: { nthChild: { position: 1, reverse: true } } }, { inside: { kind: 'redirected_statement', not: { nthChild: { position: 1, reverse: true } }, inside: { kind: 'pipeline' } } }],
            },
        },
        { id: 'has-input', rule: { kind: 'command', any: [{ has: { matches: 'input' } }, { matches: 'stage' }, { inside: { stopBy: 'end', not: { kind: 'list, pipeline, negated_command' }, matches: 'stage' } }] } },
        { id: 'background', rule: { pattern: '&' } },
        {
            id: 'expands',
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
        .map((rule) => JSON.stringify({ language: 'bash', utils: UTILS, ...rule }))
        .join('\n---\n'),
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]

const runStdout = (system: System, argv: Invocation, init: { readonly cwd: string; readonly stdin: string }): Promise<Result<string>> =>
    system.run(argv, init).then(
        ({ exitCode, stdout, stderr }) => (exitCode === 0 ? Result.ok(stdout) : Result.fault({ kind: 'exited', program: argv[0], code: exitCode, output: stderr.trim() })),
        (cause: unknown) => Result.fault({ kind: 'thrown', program: argv[0], cause }),
    );

const runJson = async <A>(system: System, argv: Invocation, cwd: string, stdin = ''): Promise<Result<A>> =>
    (await runStdout(system, argv, { cwd, stdin })).bind((output): Result<A> => {
        try {
            return Result.ok(JSON.parse(output));
        } catch (cause) {
            return Result.fault({ kind: 'thrown', program: argv[0], cause });
        }
    });

// --- [TEXT]

const applyEdits = (source: Uint8Array, base: number, edits: readonly (Span & { readonly text: string })[]): string => {
    const decoder = new TextDecoder();
    const sorted = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const between = (start: number, end?: number): string => decoder.decode(source.subarray(start - base, end === undefined ? undefined : end - base));
    return [...sorted.map(({ start, text }, index) => `${between(sorted[index - 1]?.end ?? base, start)}${text}`), between(sorted.at(-1)?.end ?? base)].join('');
};

const contains = (outer: Span | undefined, { start, end }: Span): boolean => outer === undefined || (start >= outer.start && end <= outer.end);

const resolver = (expansions: readonly Hit[], declarations: readonly Omit<Binding, 'value' | 'literal'>[], source: Uint8Array, environment: ReadonlyMap<string, string>) => {
    const lookup = (known: readonly Binding[], name: string | undefined, at: Span, scoped: boolean): Option<string> => {
        const [bound, ...more] = known.filter((binding) => binding.name === name && (!scoped || contains(binding.scope, at)));
        if (name !== undefined && environment.has(name)) {
            return Option.fromNullable(environment.get(name));
        }
        if (bound === undefined || more.length > 0 || bound.hit.range.byteOffset.end > at.start) {
            return Option.none;
        }
        return scoped ? bound.value : bound.literal;
    };
    const bindingValue = (bound: readonly Binding[], { hit, name }: Omit<Binding, 'value' | 'literal'>, scoped: boolean): Option<string> => {
        const text = hit.text.slice(name.length + 1);
        const found = (reference: string | undefined): readonly string[] => lookup(bound, reference, hit.range.byteOffset, scoped).toArray();
        const complete = [...text.matchAll(REFERENCES)].every((match) => found(match.groups?.bare ?? match.groups?.braced).length > 0);
        const value = text.replace(REFERENCES, (_match, bare: string | undefined, braced: string | undefined) => found(bare ?? braced).join(''));
        return hit.ruleId !== 'dynamic-assignment' && hit.text[name.length] === '=' && complete && LITERAL.test(value) ? Option.some(unquote(value)) : Option.none;
    };
    const bindings = declarations.reduce<readonly Binding[]>((bound, declaration) => bound.concat({ ...declaration, value: bindingValue(bound, declaration, true), literal: bindingValue(bound, declaration, false) }), []);
    const substitute = (scoped: boolean) => {
        const references = expansions.flatMap((hit) => Array.from(lookup(bindings, REFERENCE.exec(hit.text)?.groups?.name, hit.range.byteOffset, scoped).toArray(), (text) => ({ ...hit.range.byteOffset, text })));
        return ({ range: { byteOffset } }: Capture): string => {
            const text = applyEdits(
                source.subarray(byteOffset.start, byteOffset.end),
                byteOffset.start,
                references.filter((reference) => contains(byteOffset, reference)),
            );
            return scoped ? unquote(text) : text;
        };
    };
    return { resolved: substitute(true), literal: substitute(false) };
};

// --- [PARSE]

const parseSource = async (system: System, source: string, context: Context): Promise<Result<Script>> => (await runJson<readonly Hit[]>(system, [system.scanner, ...SCAN_ARGUMENTS], system.plugin, source)).bind((hits) => hitScript(system, source, context, hits));

const hitScript = async (system: System, source: string, context: Context, hits: readonly Hit[]): Promise<Result<Script>> => {
    const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start);
    const expands = new Set(sorted.flatMap((hit) => (hit.ruleId === 'expands' ? [hit.range.byteOffset.start] : [])));
    const expansions = sorted.filter(({ ruleId }) => ruleId === 'expansion');
    const declarations = sorted.flatMap((hit) =>
        hit.ruleId === 'assignment' || hit.ruleId === 'dynamic-assignment' || hit.ruleId === 'chain-assignment' ? (NAME.exec(hit.text)?.slice(0, 1) ?? []).map((name) => ({ hit, name, scope: (hit.ruleId === 'chain-assignment' ? hit.metaVariables.single.CHAIN : hit.metaVariables.single.SCOPE)?.range.byteOffset })) : [],
    );
    const assigned = new Set([...declarations.map(({ name }) => name), 'PWD', 'OLDPWD', 'SHLVL', '_']);
    const referenced = [...new Set([...expansions, ...declarations.map(({ hit }) => hit)].flatMap(({ text }) => [...text.matchAll(REFERENCES)].flatMap((match) => Option.fromNullable(match.groups?.bare ?? match.groups?.braced).toArray())))].filter((name) => !assigned.has(name));
    const environment = new Map((await Promise.all(referenced.map(async (name) => (await system.environment(name)).toArray().map((value) => [name, value] as const)))).flat());
    const { resolved, literal } = resolver(expansions, declarations, new TextEncoder().encode(source), environment);
    const entries = Map.groupBy(sorted, (hit) => (hit.ruleId === 'operand' || hit.ruleId === 'heredoc' || hit.ruleId === 'herestring' || hit.ruleId === 'heredoc-file' || hit.ruleId === 'statement' ? hit.metaVariables.single.BODY : hit).range.byteOffset.start)
        .values()
        .flatMap((own) => {
            const hit = own.find((other) => other.ruleId === 'command');
            if (hit === undefined) {
                return [];
            }
            const rules = new Set(own.map(({ ruleId }) => ruleId));
            const words = [hit.metaVariables.single.CMD, ...(hit.metaVariables.multi.ARGS ?? []), ...own.filter((other) => other.ruleId === 'operand')]
                .filter(({ text }) => WORD.test(text))
                .map((capture) => ({ text: resolved(capture), raw: capture.text, literal: literal(capture), span: capture.range.byteOffset, expands: expands.has(capture.range.byteOffset.start) }));
            const chain = invocations(words.map(({ text }) => text));
            return [
                {
                    hit,
                    stdin: own
                        .filter((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring')
                        .slice(-1)
                        .map((redirect) => (redirect.ruleId === 'herestring' ? resolved(redirect) : redirect.text)),
                    cdTargets: [chain.find(([program]) => programRow(program).chdir !== undefined)].flatMap((cd) => (cd === undefined ? [] : [operands(cd).inputs[0] ?? programRow(cd[0]).chdir?.[0]])),
                    command: {
                        words,
                        invocations: chain,
                        parts: invocationParts(words, chain),
                        extent: (own.find((other) => other.ruleId === 'statement') ?? hit).range.byteOffset,
                        hasInput: context.hasInput || rules.has('has-input'),
                        pipesOutput: rules.has('pipes-output'),
                        inSubstitution: rules.has('in-substitution'),
                        inLoop: context.inLoop || rules.has('in-loop') || chain.some(([program]) => programRow(program).repeats === true),
                        inBusyLoop: rules.has('in-busy-loop'),
                        origin: context.origin,
                        heredocFiles: own.flatMap((other) => (other.ruleId === 'heredoc-file' ? [resolved(other)] : [])),
                    },
                },
            ];
        })
        .toArray();
    const cwdEntries = entries.map((part) => ({
        stdin: part.stdin,
        command: {
            ...part.command,
            cwd: entries
                .filter(({ hit }) => hit.range.byteOffset.end <= part.hit.range.byteOffset.start && contains(hit.metaVariables.single.SCOPE?.range.byteOffset, part.hit.range.byteOffset))
                .flatMap(({ cdTargets }) => cdTargets)
                .reduce((folder, target) => (target === undefined || target.includes('$') || DIR_STACK.test(target) ? Option.none : resolvePath(target, folder, system.home)), context.cwd),
        },
    }));
    const nested = Result.all(await Promise.all(cwdEntries.flatMap(({ stdin, command: { invocations: chain, parts, cwd, hasInput, inLoop } }) => parseNested(system, chain, parts.at(-1)?.words[0], stdin, { ...context, cwd, hasInput, inLoop }))));
    return nested.map((scripts) => ({
        commands: [...cwdEntries.map(({ command }) => command), ...scripts.flatMap(({ commands }) => commands)],
        jobs: [...sorted.flatMap((hit) => (hit.ruleId === 'background' ? [{ ...hit.range.byteOffset, origin: context.origin }] : [])), ...scripts.flatMap(({ jobs }) => jobs)],
        assignments: [...declarations.map(({ name }) => ({ name, origin: context.origin })), ...scripts.flatMap(({ assignments }) => assignments)],
    }));
};

const parseFile = async (system: System, file: string, context: Context): Promise<Result<Script>> => {
    const [tracked, [text]] = await Promise.all([
        Promise.all(system.root.toArray().flatMap((root) => (isInside(file, root) ? [runStdout(system, ['git', 'ls-files', '--error-unmatch', '--', file], { cwd: root, stdin: '' })] : []))),
        system.read(file).then(
            (read) => [read],
            () => [],
        ),
    ]);
    return tracked.some((result) => result.toArray().length > 0) || text === undefined ? Result.ok({ commands: [], jobs: [], assignments: [] }) : parseSource(system, text, { ...context, origin: 'file', visited: new Set([...context.visited, file]) });
};

const parseNested = (system: System, chain: readonly Invocation[], head: Word | undefined, stdin: readonly string[], context: Context): readonly Promise<Result<Script>>[] =>
    chain.slice(-1).flatMap((last) => {
        const { shell } = programRow(last[0]);
        const { options, inputs, inner } = operands(last);
        const bodies = [last, ...invocations(inner)].flatMap((invocation) => [...operands(invocation).bodies, ...(programRow(invocation[0]).shell !== undefined && readsStdin(invocation) ? stdin : [])]);
        const script = [head].flatMap((word) => (word !== undefined && !word.expands && word.text.includes('/') && SCRIPT.test(word.text) ? [word.text] : []));
        const sourced = shell === undefined || shell === 'joined' ? script : inputs.slice(0, Number(!options.some((name) => shell.includes(name))));
        const files = sourced.flatMap((path) => resolvePath(path, context.cwd, system.home).toArray()).filter((file) => !context.visited.has(file));
        return [...bodies.map((body) => parseSource(system, body, { ...context, origin: context.origin === 'file' ? 'file' : 'body' })), ...files.map((file) => parseFile(system, file, context))];
    });

const parse = (system: System, command: string): Promise<Result<Script>> => parseSource(system, command, { cwd: resolvePath(system.cwd, Option.none, Option.none), hasInput: false, inLoop: false, origin: 'call', visited: new Set() });

// --- [EXPORTS] -------------------------------------------------------------------------

export { applyEdits, type Command, parse, runJson, type Script, type Word };
