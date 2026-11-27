import { Array, Cache, Cause, Context, Effect, Equal, FileSystem, HashSet, Match, Option, Record } from 'effect';
import { ChildProcessSpawner, type CommandLine } from 'effect/process';
import { createSourceIndex, ErrorCode, type Node, ParseError, ShSyntaxError, type Word, type WordPart } from 'sh-syntax';
import type { Processor } from 'sh-syntax/sync';
import { Place } from '#/analysis/invocation';
import { builtins, interpret } from '#/analysis/walk';
import { Field, Origin, Scope } from '#/evaluation/value';
import { catalog } from '#/programs/catalog';
import type { Usage } from '#/programs/usage';
import { interpolate } from '#/syntax/construction';
import { type Dialect, probe } from '#/syntax/dialect';
import { type Edit, Editor } from '#/syntax/edit';
import { Fragment, index, Script, ShellError } from '#/syntax/script';

// --- [OPERATIONS] ----------------------------------------------------------------------

const isBraced = (node: Node): node is Word => node.type === 'Word' && node.parts.some((part) => part.type === 'Lit' && part.value.includes('{'));

// --- [SERVICES] ------------------------------------------------------------------------

class Shell extends Context.Service<Shell>()('@rasm/shell/shell', {
    make: Effect.fnUntraced(function* (facts: { readonly processor: Processor; readonly programs?: readonly (typeof Usage.Encoded)[] }) {
        const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
        const programs = yield* catalog(facts.programs);
        const read = (path: string) =>
            fs.readFileString(path).pipe(
                Effect.map(Option.liftPredicate((body: string) => !body.includes('\0'))),
                Effect.catch((error) => (error.reason._tag === 'NotFound' ? Effect.fail(new Cause.NoSuchElementError(path)) : Effect.succeedNone)),
            );
        const parse = (source: string, dialect: Dialect) =>
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
        const detect = (at: Parameters<typeof probe>[0]) => Effect.provideService(probe(at, builtins, parse), ChildProcessSpawner.ChildProcessSpawner, spawner);
        const dialects = yield* Cache.make({ capacity: Number.POSITIVE_INFINITY, lookup: (at: { readonly shell: string; readonly environment: Readonly<Record<string, string>> }) => Effect.option(detect(at)) });
        const locate = Effect.fnUntraced(function* (dialect: Dialect, at: { readonly cwd?: string; readonly operands?: readonly string[] }) {
            const logical = Record.get(dialect.environment, 'PWD');
            const [physical, resolved] = yield* Effect.all([Option.fromUndefinedOr(at.cwd).pipe(Effect.fromOption, Effect.flatMap(fs.realPath), Effect.option), logical.pipe(Effect.fromOption, Effect.flatMap(fs.realPath), Effect.option)]);
            const pwd = Option.orElse(
                Option.filter(logical, () => Option.isSome(physical) && Equal.equals(physical, resolved)),
                () => physical,
            );
            return { place: Place.Local({ cwd: Option.match(pwd, { onNone: () => Field.unknown(Origin.Variable({ name: 'PWD', origins: HashSet.empty() })), onSome: Field.known }) }), scope: Scope.start({ dialect, operands: at.operands ?? [], pwd }) };
        });
        const assemble = Effect.fnUntraced(function* (text: Fragment, dialect: Dialect, start: Pick<Script, 'place' | 'scope'>): Effect.fn.Return<Script, ShellError> {
            const tree = index(text.source, yield* parse(text.source, dialect));
            const braced = dialect.options.braceexpand ? tree.links.keys().filter(isBraced).toArray() : [];
            const split = yield* Effect.forEach(braced, (word) => Effect.try({ try: () => facts.processor.splitBraces(word), catch: ShellError.of({ _tag: 'Host' }, dialect.binary) }));
            const braces = new Map(Array.zip(braced, split).flatMap(([word, { word: alternated, changed }]) => (changed ? [[word, alternated] as const] : [])));
            const nested = (source: string, held: Dialect) => assemble(Fragment.of(source), held, { place: script.place, scope: Scope.start({ dialect: held, operands: [], pwd: Option.none() }) });
            const invocations = yield* Effect.cached(Effect.suspend(() => interpret(script, { parse: nested, programs, probe: (shell, environment) => Cache.get(dialects, { shell, environment }), read })));
            const script: Script = new Script({ text, dialect, place: start.place, scope: start.scope, tree, braces, invocations });
            return script;
        });
        return {
            dialect: Effect.fn('Shell.dialect')(detect),
            read: Effect.fn('Shell.read')(function* (source: string, at: { readonly dialect: Dialect; readonly cwd?: string; readonly operands?: readonly string[] }) {
                return yield* assemble(Fragment.of(source), at.dialect, yield* locate(at.dialect, at));
            }),
            edit: Effect.fn('Shell.edit')(function* (script: Script, edits: (editor: Editor, within: (nested: Script) => Editor) => readonly Edit[]) {
                const editor = new Editor(script);
                return yield* assemble(yield* Effect.fromResult(editor.apply(...edits(editor, (nested) => new Editor(nested)))), script.dialect, { place: script.place, scope: script.scope });
            }),
            script:
                (at: Dialect | Script) =>
                (strings: TemplateStringsArray, ...holes: readonly (CommandLine.TemplateExpression | Word | WordPart)[]) =>
                    Effect.gen(function* () {
                        const [dialect, origin] = at instanceof Script ? [at.dialect, Option.some(at)] : [at, Option.none<Script>()];
                        return yield* assemble(yield* interpolate({ parse: (source) => parse(source, dialect), programs, dialect, origin }, strings, holes), dialect, yield* locate(dialect, {}));
                    }).pipe(Effect.withSpan('Shell.script')),
        } as const;
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shell };
