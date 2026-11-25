import { Cache, Context, Effect, Equal, FileSystem, Function, Match, Option, Record } from 'effect';
import { ChildProcessSpawner, type CommandLine } from 'effect/process';
import { BinTestOperator, createSourceIndex, ErrorCode, type Node, ParseError, RedirOperator, ShSyntaxError, walk, type Word } from 'sh-syntax';
import type { Processor } from 'sh-syntax/sync';
import { Place } from '#/analysis/invocation';
import { builtins, walked } from '#/analysis/walk';
import { Field, Origin, Role, Scope } from '#/evaluation/value';
import { catalog } from '#/programs/catalog';
import type { Usage } from '#/programs/usage';
import { constructed } from '#/syntax/construction';
import { type Dialect, probed } from '#/syntax/dialect';
import { type Edit, Editor } from '#/syntax/edit';
import { Script, ShellError } from '#/syntax/script';

// --- [OPERATIONS] ----------------------------------------------------------------------

const braceable = (word: Word) => word.parts.some((part) => part.type === 'Lit' && part.value.includes('{'));
const placed = (parent: Node | null, prop: string | null): Role =>
    Match.value({ parent, prop }).pipe(
        Match.when({ parent: { type: 'Redirect', op: Match.is(RedirOperator.Hdoc, RedirOperator.DashHdoc) }, prop: 'hdoc' }, ({ parent: redirect }) => Role.Heredoc({ expands: redirect.word?.parts.every((part) => part.type === 'Lit' && !part.value.includes('\\')) ?? false })),
        Match.when({ parent: { type: 'Redirect', op: Match.is(RedirOperator.Hdoc, RedirOperator.DashHdoc, RedirOperator.WordHdoc) } }, () => Role.Operand()),
        Match.when({ parent: { type: 'Redirect' } }, () => Role.Redirect()),
        Match.when({ parent: { type: 'Assign' }, prop: 'value' }, () => Role.Assignment()),
        Match.when({ parent: { type: 'CaseItem' } }, () => Role.Pattern()),
        Match.when({ parent: { type: 'BinaryTest', op: Match.is(BinTestOperator.TsMatch, BinTestOperator.TsMatchShort, BinTestOperator.TsNoMatch) }, prop: 'y' }, () => Role.Pattern()),
        Match.when({ parent: { type: Match.is('CaseClause', 'BinaryTest', 'UnaryTest', 'TestClause', 'ParenTest') } }, () => Role.Operand()),
        Match.orElse(() => Role.Argument()),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class Shell extends Context.Service<Shell>()('@rasm/shell/shell', {
    make: Effect.fnUntraced(function* (facts: { readonly processor: Processor; readonly programs?: readonly (typeof Usage.Encoded)[] }) {
        const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
        const programs = yield* catalog(facts.programs);
        const interpreters = yield* Cache.make({
            capacity: Number.MAX_SAFE_INTEGER,
            lookup: ({ binary, environment }: { readonly binary: string; readonly environment: Readonly<Record<string, string>> }) => Effect.option(Effect.provideService(probed({ shell: binary, environment }, builtins, parsed), ChildProcessSpawner.ChildProcessSpawner, spawner)),
        });
        const interpreter = (binary: string, environment: Readonly<Record<string, string>>) => Cache.get(interpreters, { binary, environment });
        const parsed = (source: string, dialect: Dialect) =>
            Effect.try({
                try: () => facts.processor(source, { variant: dialect.variant }),
                catch: (cause) =>
                    Match.value(cause).pipe(
                        Match.when(Match.instanceOf(ParseError), (error) => ShellError.of({ _tag: 'Syntax', offset: Option.map(Option.fromUndefinedOr(error.pos), ({ offset }) => createSourceIndex(source)(offset)), incomplete: error.incomplete }, dialect.binary)(error)),
                        Match.when(
                            (error: unknown) => error instanceof ShSyntaxError && error.code === ErrorCode.Language,
                            (error) => ShellError.of({ _tag: 'Dialect' }, dialect.binary)(error),
                        ),
                        Match.orElse((error) => ShellError.of({ _tag: 'Host' }, dialect.binary)(error)),
                    ),
            });
        const started = Effect.fnUntraced(function* (dialect: Dialect, at: { readonly cwd?: string; readonly operands?: readonly string[] }) {
            const logical = Record.get(dialect.environment, 'PWD');
            const [physical, resolved] = yield* Effect.all([Effect.option(Effect.flatMap(Effect.fromOption(Option.fromUndefinedOr(at.cwd)), fs.realPath)), Effect.option(Effect.flatMap(Effect.fromOption(logical), fs.realPath))]);
            const pwd = Option.orElse(
                Option.filter(logical, () => Option.isSome(physical) && Equal.equals(physical, resolved)),
                () => physical,
            );
            return { place: Place.Local({ cwd: Option.match(pwd, { onNone: () => Field.unknown(Origin.Variable({ name: 'PWD' })), onSome: Field.known }) }), scope: Scope.started({ dialect, operands: at.operands ?? [], pwd }) };
        });
        const scripted = Effect.fnUntraced(function* (source: string, dialect: Dialect, start: Pick<Script, 'place' | 'scope'>, redacted: Script['redacted'] = []): Effect.fn.Return<Script, ShellError> {
            const syntax = yield* parsed(source, dialect);
            const words: (readonly [Word, Role])[] = [];
            walk(syntax, (node, parent, prop) => {
                Equal.byReferenceUnsafe(node);
                if (node.type === 'Word') {
                    words.push([node, placed(parent, prop)]);
                }
            });
            const sites = yield* Effect.forEach(
                words,
                Effect.fnUntraced(function* ([word, role]) {
                    const { word: alternated, changed } = dialect.options.braceexpand && braceable(word) ? yield* Effect.try({ try: () => facts.processor.splitBraces(word), catch: ShellError.of({ _tag: 'Host' }, dialect.binary) }) : { word, changed: false };
                    return [word, { role, braces: Option.liftPredicate(alternated, Function.constant(changed)) }] as const;
                }),
            );
            const invocations = yield* Effect.cached(Effect.suspend(() => walked(script, { parse: (text, held) => scripted(text, held, { place: script.place, scope: Scope.started({ dialect: held, operands: [], pwd: Option.none() }) }), programs, interpreter })));
            const script: Script = new Script({ source, dialect, syntax, located: createSourceIndex(source), place: start.place, scope: start.scope, sites: new Map(sites), redacted, invocations });
            return script;
        });
        return {
            dialect: Effect.fn('Shell.dialect')(function* (at: Parameters<typeof probed>[0]) {
                return yield* Effect.provideService(probed(at, builtins, parsed), ChildProcessSpawner.ChildProcessSpawner, spawner);
            }),
            read: Effect.fn('Shell.read')(function* (source: string, at: { readonly dialect: Dialect; readonly cwd?: string; readonly operands?: readonly string[] }) {
                return yield* scripted(source, at.dialect, yield* started(at.dialect, at));
            }),
            edit: Effect.fn('Shell.edit')(function* (script: Script, edits: (editor: Editor, within: (nested: Script) => Editor) => readonly Edit[]) {
                const editor = new Editor(script);
                const { source, redacted } = yield* Effect.fromResult(editor.apply(...edits(editor, (nested) => new Editor(nested))));
                return yield* scripted(source, script.dialect, { place: script.place, scope: script.scope }, redacted);
            }),
            script:
                (dialect: Dialect) =>
                (strings: TemplateStringsArray, ...holes: readonly CommandLine.TemplateExpression[]) =>
                    Effect.gen(function* () {
                        const { source, redacted } = yield* constructed((text) => parsed(text, dialect), dialect, strings, holes, programs);
                        return yield* scripted(source, dialect, yield* started(dialect, {}), redacted);
                    }).pipe(Effect.withSpan('Shell.script')),
        } as const;
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shell };
