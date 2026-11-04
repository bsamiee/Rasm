import { Option, Result } from '../../monads.ts';
import type { System } from '../command-decision.ts';
import { type Invocation, invocationParts, invocations, isInside, operands, programRow, readsStdin, resolvePath, unquote } from './invocation.ts';

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
    readonly name: string;
    readonly capture: Capture;
    readonly scope: Scope | undefined;
    readonly context: Scope | undefined;
    readonly condition: Span | undefined;
    readonly dynamic: boolean;
    readonly value: Option<string>;
}
interface Scope {
    readonly hit: Extract<Hit, { readonly ruleId: 'scope' | 'local-scope' | 'function' }>;
    readonly parent: Scope | undefined;
}
interface Ownership {
    readonly scope: Scope | undefined;
    readonly control: Span | undefined;
    readonly chain: Span | undefined;
}

type Origin = 'call' | 'body' | 'file';
type Context = Pick<Command, 'cwd' | 'hasInput' | 'inLoop' | 'origin'> & { readonly visited: ReadonlySet<string>; readonly scanner: Promise<Invocation>; readonly scans: Map<string, Promise<Result<readonly Hit[]>>> };
type Hit = Capture &
    (
        | { readonly ruleId: 'command'; readonly metaVariables: { readonly single: { readonly CMD: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
        | { readonly ruleId: 'operand' | 'heredoc' | 'herestring' | 'heredoc-file' | 'statement'; readonly metaVariables: { readonly single: { readonly BODY: Capture } } }
        | { readonly ruleId: 'assignment' | 'dynamic-assignment' | 'expansion' | 'control' | 'chain' }
        | { readonly ruleId: 'scope' }
        | { readonly ruleId: 'local-scope'; readonly metaVariables: { readonly single: { readonly OPERATOR: Capture } } }
        | { readonly ruleId: 'function'; readonly metaVariables: { readonly single: { readonly NAME: Capture } } }
        | { readonly ruleId: 'declaration'; readonly metaVariables: { readonly single: { readonly KEYWORD: Capture }; readonly multi: { readonly ARGS?: readonly Capture[] } } }
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
const VALUE = { kind: 'word, string, raw_string, concatenation, simple_expansion, expansion, command_substitution, number' };
const SCOPE = { kind: 'program, subshell, command_substitution, process_substitution, pipeline', not: { has: { field: 'operator', regex: String.raw`^\$\{` } } };
const CONTROL = { kind: 'if_statement, case_statement, while_statement, for_statement, c_style_for_statement, negated_command' };
const FLOW = { any: [SCOPE, CONTROL, { kind: 'function_definition, variable_assignment' }] };
const UTILS = {
    owner: { any: [{ inside: { kind: 'command', pattern: '$BODY' } }, { inside: { kind: 'heredoc_redirect', inside: { kind: 'redirected_statement', matches: 'statement' } } }, { inside: { kind: 'redirected_statement', matches: 'statement' } }] },
    statement: { has: { field: 'body', matches: 'last' } },
    last: {
        any: [
            { kind: 'command', pattern: '$BODY' },
            { kind: 'list, pipeline, negated_command', has: { matches: 'last', nthChild: { position: 1, reverse: true } } },
        ],
    },
    list: { kind: 'list', not: { inside: { kind: 'list', stopBy: FLOW } } },
    input: { any: [{ kind: 'heredoc_redirect' }, { kind: 'herestring_redirect' }, { kind: 'file_redirect', regex: '^0?<' }] },
    tail: { any: [{ inside: { kind: 'redirected_statement', field: 'body', has: { matches: 'input' } } }, { nthChild: { position: 1, reverse: true }, inside: { kind: 'list, pipeline, negated_command', matches: 'tail' } }] },
    piped: { any: [{ inside: { kind: 'pipeline' }, not: { nthChild: 1 } }, { nthChild: 1, inside: { kind: 'list, pipeline, negated_command', matches: 'piped' } }, { inside: { kind: 'pipeline', inside: { kind: 'heredoc_redirect' } } }] },
    stage: { any: [{ matches: 'tail' }, { matches: 'piped' }] },
};
const RULES = [
    { id: 'command', rule: { kind: 'command', any: [{ pattern: '$CMD $$$ARGS' }, { pattern: '$CMD' }] } },
    { id: 'scope', rule: { all: [SCOPE, { not: { kind: 'program' } }] } },
    { id: 'local-scope', rule: { kind: 'command_substitution', has: { field: 'operator', regex: String.raw`^\$\{`, pattern: '$$OPERATOR' } } },
    { id: 'function', rule: { kind: 'function_definition', has: { field: 'name', pattern: '$NAME' } } },
    { id: 'control', rule: { any: [CONTROL, { matches: 'list' }] } },
    { id: 'chain', rule: { matches: 'list', not: { has: { stopBy: 'end', regex: String.raw`^\|\|$`, not: { pattern: '$_' } } } } },
    {
        id: 'declaration',
        rule: { kind: 'declaration_command, command', any: ['local', 'declare', 'typeset', 'export', 'readonly'].flatMap((keyword) => ['', 'builtin ', 'builtin -- '].map((prefix) => ({ pattern: `${prefix}${keyword} $$$ARGS` }))), has: { regex: '^(?:local|declare|typeset|export|readonly)$', pattern: '$$KEYWORD' } },
    },
    { id: 'operand', rule: { ...VALUE, follows: VALUE, inside: { kind: 'file_redirect', matches: 'owner' } } },
    { id: 'heredoc', rule: { kind: 'heredoc_body', inside: { kind: 'heredoc_redirect', matches: 'owner' } } },
    { id: 'herestring', rule: { ...VALUE, inside: { kind: 'herestring_redirect', matches: 'owner' } } },
    { id: 'heredoc-file', rule: { ...VALUE, inside: { kind: 'file_redirect', field: 'destination', regex: '^(?:1?>|&>)', matches: 'owner', inside: { stopBy: { kind: 'redirected_statement' }, kind: 'redirected_statement', has: { kind: 'heredoc_redirect' } } } } },
    { id: 'statement', rule: { kind: 'redirected_statement', has: { field: 'body', kind: 'command', pattern: '$BODY' } } },
    { id: 'assignment', rule: { kind: 'variable_assignment', not: { inside: { kind: 'command, c_style_for_statement, declaration_command' } } } },
    {
        id: 'dynamic-assignment',
        rule: {
            any: [
                { kind: 'variable_name', inside: { kind: 'for_statement, unset_command' } },
                { kind: 'word', regex: String.raw`^[A-Za-z_]\w*$`, inside: { kind: 'command', has: { kind: 'command_name', regex: '^(?:read|readarray|mapfile|getopts|printf)$' } } },
            ],
        },
    },
    { id: 'expansion', rule: { kind: 'simple_expansion, expansion', regex: String.raw`^\$(?:[A-Za-z_]\w*|\{[A-Za-z_]\w*\})$` } },
    ...[
        { id: 'in-loop', rule: { inside: { stopBy: 'end', kind: 'while_statement, for_statement, c_style_for_statement' } } },
        { id: 'in-busy-loop', rule: { inside: { stopBy: 'end', kind: 'while_statement', has: { field: 'body', kind: 'do_group', not: { has: { stopBy: 'end', kind: 'command', not: { regex: '^(?::|true)$' } } } } } } },
        { id: 'in-substitution', rule: { inside: { stopBy: 'end', kind: 'process_substitution' } } },
        { id: 'pipes-output', rule: { any: [{ inside: { kind: 'pipeline' }, not: { nthChild: { position: 1, reverse: true } } }, { inside: { kind: 'redirected_statement', not: { nthChild: { position: 1, reverse: true } }, inside: { kind: 'pipeline' } } }] } },
        { id: 'has-input', rule: { any: [{ has: { matches: 'input' } }, { matches: 'stage' }, { inside: { stopBy: 'end', not: { kind: 'list, pipeline, negated_command' }, matches: 'stage' } }] } },
    ].map(({ id, rule }) => ({ id, rule: { kind: 'command', not: { not: rule } } })),
    { id: 'background', rule: { pattern: '&' } },
    {
        id: 'expands',
        rule: {
            kind: 'simple_expansion, expansion, command_substitution, process_substitution, arithmetic_expansion, string, concatenation, word, raw_string',
            not: {
                not: {
                    inside: { kind: 'command, command_name' },
                    any: [
                        { kind: 'simple_expansion, expansion, command_substitution, process_substitution, arithmetic_expansion' },
                        { kind: 'string, concatenation', has: { stopBy: 'end', kind: 'simple_expansion, expansion, command_substitution, arithmetic_expansion' } },
                        { kind: 'word', regex: String.raw`[*?\[{]` },
                        { kind: 'concatenation', has: { kind: 'word', regex: String.raw`[*?\[{]` } },
                        { kind: 'word, string, raw_string', regex: String.raw`^["']?(?:~[^/"']*|\.{1,2}|\*+|\.\*)?(?:/+(?:\.{1,2}|\*+|\.\*)?)*["']?$` },
                    ],
                },
            },
        },
    },
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESSES]

const scanner = async ({ scanner: binary, plugin, read }: System): Promise<Invocation> => {
    const config = `${plugin}/../../.cache/ast-grep/bash.yml`;
    const [language] = Object.keys(JSON.parse(await read(config)).customLanguages);
    return [binary, 'scan', '--config', config, '--stdin', '--custom-languages', 'allow', '--json=compact', '--inline-rules', RULES.map((rule) => JSON.stringify({ language, utils: UTILS, ...rule })).join('\n---\n')];
};

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

const opens = (hit: Hit): hit is Scope['hit'] => hit.ruleId === 'scope' || hit.ruleId === 'local-scope' || hit.ruleId === 'function';

const lineage = (scope: Scope | undefined): readonly (Scope | undefined)[] => (scope === undefined ? [undefined] : [scope, ...lineage(scope.parent)]);

const enclosing = (scope: Scope | undefined, kind: Scope['hit']['ruleId']): Scope | undefined => lineage(scope).find((frame) => frame?.hit.ruleId !== kind);

const replies = (scope: Scope | undefined, name: string): boolean => scope?.hit.ruleId === 'local-scope' && scope.hit.metaVariables.single.OPERATOR.text.startsWith('${|') && name === 'REPLY';

const ownership = (hits: readonly Hit[]): ReadonlyMap<Hit, Ownership> =>
    hits.reduce<{ readonly active: readonly Hit[]; readonly scopes: Map<Hit, Scope>; readonly owners: Map<Hit, Ownership> }>(
        ({ active, scopes, owners }, hit) => {
            const open = active.filter((other) => contains(other.range.byteOffset, hit.range.byteOffset));
            const parent = open.findLast(opens);
            const scope = parent === undefined ? undefined : scopes.get(parent);
            const control = open.findLast((other) => other.ruleId === 'control' && contains(scope?.hit.range.byteOffset, other.range.byteOffset))?.range.byteOffset;
            const chain = open.findLast((other) => other.ruleId === 'chain' && contains(control, other.range.byteOffset) && contains(scope?.hit.range.byteOffset, other.range.byteOffset))?.range.byteOffset;
            return {
                active: opens(hit) || hit.ruleId === 'control' || hit.ruleId === 'chain' ? [...open, hit] : open,
                scopes: opens(hit) ? scopes.set(hit, { hit, parent: scope }) : scopes,
                owners: owners.set(hit, { scope, control, chain }),
            };
        },
        { active: [], scopes: new Map(), owners: new Map() },
    ).owners;

const resolver = (expansions: readonly Hit[], declarations: readonly Omit<Binding, 'value'>[], calls: readonly Hit[], source: Uint8Array, environment: ReadonlyMap<string, string>, owners: ReadonlyMap<Hit, Ownership>) => {
    const values = new Map<Scope | undefined, Map<string, Binding>>();
    const invalidated = new Map<Scope | undefined, number>();
    const lookup = (name: string | undefined, scope: Scope | undefined, at: Span): Option<string> => {
        if (name === undefined) {
            return Option.none;
        }
        const frames = lineage(scope);
        const owner = frames.findIndex((frame) => values.get(frame)?.has(name) === true || replies(frame, name));
        const bound = values.get(frames[owner])?.get(name);
        if (owner >= 0 && bound === undefined) {
            return Option.none;
        }
        const changed = invalidated.get(enclosing(scope, 'local-scope'));
        if (changed !== undefined && (bound === undefined || changed > bound.capture.range.byteOffset.end)) {
            return Option.none;
        }
        if (bound === undefined) {
            return Option.fromNullable(environment.get(name));
        }
        return contains(bound.condition, at) ? bound.value : Option.none;
    };
    const events = [
        ...expansions.map((hit) => ({ kind: 'expansion' as const, hit, at: hit.range.byteOffset.start })),
        ...declarations.map((declaration) => ({ kind: 'binding' as const, declaration, at: declaration.capture.range.byteOffset.end })),
        ...calls.map((hit) => ({ kind: 'call' as const, hit, at: hit.range.byteOffset.end })),
    ].toSorted((left, right) => left.at - right.at);
    const references = events.flatMap((event): readonly (Span & { readonly text: string })[] => {
        if (event.kind === 'expansion') {
            const { hit } = event;
            return lookup(REFERENCE.exec(hit.text)?.groups?.name, owners.get(hit)?.scope, hit.range.byteOffset)
                .map((resolved) => ({ ...hit.range.byteOffset, text: resolved }))
                .toArray();
        }
        if (event.kind === 'call') {
            invalidated.set(enclosing(owners.get(event.hit)?.scope, 'local-scope'), event.at);
            return [];
        }
        const { declaration } = event;
        const { name, capture, scope, dynamic } = declaration;
        const text = capture.text.slice(name.length + 1);
        const found = (reference: string | undefined) => lookup(reference, declaration.context, capture.range.byteOffset).toArray();
        const complete = [...text.matchAll(REFERENCES)].every((match) => found(match.groups?.bare ?? match.groups?.braced).length > 0);
        const value = text.replace(REFERENCES, (_match, bare: string | undefined, braced: string | undefined) => found(bare ?? braced).join(''));
        values.getOrInsertComputed(scope, () => new Map()).set(name, { ...declaration, value: !dynamic && capture.text[name.length] === '=' && complete && LITERAL.test(value) ? Option.some(unquote(value)) : Option.none });
        return [];
    });
    return ({ range: { byteOffset } }: Capture): string =>
        applyEdits(
            source.subarray(byteOffset.start, byteOffset.end),
            byteOffset.start,
            references.filter((reference) => contains(byteOffset, reference)),
        );
};

// --- [PARSE]

const parseSource = async (system: System, source: string, context: Context): Promise<Result<Script>> => (await context.scans.getOrInsertComputed(source, async (text) => runJson<readonly Hit[]>(system, await context.scanner, system.plugin, text))).bind((hits) => hitScript(system, source, context, hits));

const hitScript = async (system: System, source: string, context: Context, hits: readonly Hit[]): Promise<Result<Script>> => {
    const priority = (hit: Hit): number => Number(hit.ruleId !== 'control') + Number(hit.ruleId !== 'control' && hit.ruleId !== 'chain');
    const sorted = hits.toSorted((left, right) => left.range.byteOffset.start - right.range.byteOffset.start || right.range.byteOffset.end - left.range.byteOffset.end || priority(left) - priority(right));
    const owners = ownership(sorted);
    const expands = new Set(sorted.flatMap((hit) => (hit.ruleId === 'expands' ? [hit.range.byteOffset.start] : [])));
    const expansions = sorted.filter(({ ruleId }) => ruleId === 'expansion');
    const declared = new Map<Scope | undefined, Map<string, Span | undefined>>();
    const candidates = sorted
        .flatMap((hit) => {
            const owner = owners.get(hit);
            const scope = owner?.scope;
            const process = enclosing(scope, 'local-scope');
            const assigned = hit.ruleId === 'assignment' || hit.ruleId === 'dynamic-assignment' ? [hit] : [];
            const captures = hit.ruleId === 'declaration' ? (hit.metaVariables.multi.ARGS ?? []) : assigned;
            const keyword = hit.ruleId === 'declaration' ? hit.metaVariables.single.KEYWORD.text : '';
            const options = hit.ruleId === 'declaration' ? operands([keyword, ...captures.map(({ text }) => text)]).options : [];
            const localContext = enclosing(scope, 'scope');
            const local = keyword === 'local' || (['declare', 'typeset'].includes(keyword) && localContext !== undefined && !options.includes('-g'));
            const query = options.some((option) => option === '-p' || option === '-f' || option === '-F');
            const dynamic = hit.ruleId === 'dynamic-assignment' || (owner?.control !== undefined && owner.chain === undefined) || options.includes('-n');
            return query || (keyword === 'local' && localContext === undefined) ? [] : captures.map((capture) => ({ capture, owner, scope, process, local, options, dynamic }));
        })
        .toSorted((left, right) => left.capture.range.byteOffset.end - right.capture.range.byteOffset.end);
    const declarations = candidates.flatMap(({ capture, owner, scope, process, local, options, dynamic }): readonly Omit<Binding, 'value'>[] => {
        const name = NAME.exec(capture.text)?.[0];
        if (name === undefined) {
            return [];
        }
        const destination = lineage(scope).find((frame) => frame === process || declared.get(frame)?.has(name) === true || replies(frame, name));
        const assigned = options.includes('-g') ? process : destination;
        const target = local ? scope : assigned;
        const uncertain = !local && target !== process && !contains(declared.get(target)?.get(name), capture.range.byteOffset);
        const names = declared.getOrInsertComputed(target, () => new Map());
        names.getOrInsert(name, owner?.control === undefined ? undefined : (owner.chain ?? capture.range.byteOffset));
        const binding = { capture, name, scope: target, context: scope, condition: owner?.chain, dynamic };
        return [binding, ...(uncertain ? [{ ...binding, scope: process, dynamic: true }] : [])];
    });
    const assigned = new Set([...declarations.filter(({ scope, condition, dynamic }) => scope === undefined && condition === undefined && !dynamic).map(({ name }) => name), 'PWD', 'OLDPWD', 'SHLVL', '_']);
    const referenced = [...new Set([...expansions, ...declarations.map(({ capture }) => capture)].flatMap(({ text }) => [...text.matchAll(REFERENCES)].flatMap((match) => Option.fromNullable(match.groups?.bare ?? match.groups?.braced).toArray())))].filter((name) => !assigned.has(name));
    const environment = new Map((await Promise.all(referenced.map(async (name) => (await system.environment(name)).toArray().map((value) => [name, value] as const)))).flat());
    const functions = new Set(sorted.flatMap((hit) => (hit.ruleId === 'function' ? [hit.metaVariables.single.NAME.text] : [])));
    const calls = sorted.filter((hit) => {
        if (hit.ruleId !== 'command') {
            return false;
        }
        const head = unquote(hit.metaVariables.single.CMD.text);
        return (functions.has(head) || ['eval', 'source', '.', 'builtin', 'command'].includes(head)) && invocations([head, ...(hit.metaVariables.multi.ARGS ?? []).map(({ text }) => unquote(text))]).some(([program]) => functions.has(program) || program === 'eval' || program === 'source' || program === '.');
    });
    const effects = new Set(calls);
    const literal = resolver(expansions, declarations, calls, new TextEncoder().encode(source), environment, owners);
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
                .map((capture) => {
                    const spelled = literal(capture);
                    return { text: unquote(spelled), raw: capture.text, literal: spelled, span: capture.range.byteOffset, expands: expands.has(capture.range.byteOffset.start) };
                });
            const chain = invocations(words.map(({ text }) => text));
            return [
                {
                    hit,
                    stdin: own
                        .filter((other) => other.ruleId === 'heredoc' || other.ruleId === 'herestring')
                        .slice(-1)
                        .map((redirect) => (redirect.ruleId === 'herestring' ? unquote(literal(redirect)) : redirect.text)),
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
                        heredocFiles: own.flatMap((other) => (other.ruleId === 'heredoc-file' ? [unquote(literal(other))] : [])),
                    },
                },
            ];
        })
        .toArray();
    const directories = new Map<Scope | undefined, { readonly value: Option<string>; readonly condition: Span | undefined }>([[undefined, { value: context.cwd, condition: undefined }]]);
    const directoryAt = (scope: Scope | undefined, at: Span): Option<string> => {
        const frames = lineage(scope);
        const known = frames.findIndex((frame) => directories.has(frame));
        const stored = directories.get(frames[known]);
        return frames
            .slice(0, known)
            .toReversed()
            .reduce(
                (value, frame) => {
                    const inherited = frame?.hit.ruleId === 'function' ? Option.none : value;
                    directories.set(frame, { value: inherited, condition: undefined });
                    return inherited;
                },
                stored !== undefined && contains(stored.condition, at) ? stored.value : Option.none,
            );
    };
    const entered = (folder: Option<string>, target: string | undefined): Option<string> => (target === undefined || target.includes('$') || DIR_STACK.test(target) ? Option.none : resolvePath(target, folder, system.home));
    const timeline = entries
        .flatMap((part) => [
            { part, at: part.hit.range.byteOffset.start, effect: false },
            { part, at: part.command.extent.end, effect: true },
        ])
        .toSorted((left, right) => left.at - right.at || Number(right.effect) - Number(left.effect));
    const cwdEntries = timeline.flatMap(({ part, effect }): readonly { readonly stdin: readonly string[]; readonly command: Command }[] => {
        const owner = owners.get(part.hit);
        const scope = enclosing(owner?.scope, 'local-scope');
        const cwd = directoryAt(scope, part.hit.range.byteOffset);
        const current = part.command.invocations.every(([program]) => programRow(program).chdir !== undefined || program === 'command' || program === 'builtin');
        const unknown = effects.has(part.hit);
        if (!effect) {
            return [{ stdin: part.stdin, command: { ...part.command, cwd } }];
        }
        if ((current && part.cdTargets.length > 0) || unknown) {
            directories.set(scope, { value: unknown || owner?.chain === undefined ? Option.none : part.cdTargets.reduce(entered, cwd), condition: owner?.chain });
        }
        return [];
    });
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
        const { options, inputs, inner, executesSource } = operands(last);
        if (!executesSource) {
            return [];
        }
        const bodies = [last, ...invocations(inner)].flatMap((invocation) => [...operands(invocation).bodies, ...(programRow(invocation[0]).shell !== undefined && readsStdin(invocation) ? stdin : [])]);
        const script = [head].flatMap((word) => (word !== undefined && !word.expands && word.text.includes('/') && SCRIPT.test(word.text) ? [word.text] : []));
        const sourced = shell === undefined || shell === 'joined' ? script : inputs.slice(0, Number(!options.some((name) => shell.includes(name))));
        const files = sourced.flatMap((path) => resolvePath(path, context.cwd, system.home).toArray()).filter((file) => !context.visited.has(file));
        return [...bodies.map((body) => parseSource(system, body, { ...context, origin: context.origin === 'file' ? 'file' : 'body' })), ...files.map((file) => parseFile(system, file, context))];
    });

const parse = (system: System, command: string): Promise<Result<Script>> => parseSource(system, command, { cwd: resolvePath(system.cwd, Option.none, Option.none), hasInput: false, inLoop: false, origin: 'call', visited: new Set(), scanner: scanner(system), scans: new Map() });

// --- [EXPORTS] -------------------------------------------------------------------------

export { applyEdits, type Command, parse, runJson, type Script, type Word };
