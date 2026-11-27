import { fileURLToPath } from 'node:url';
import { runInNewContext } from 'node:vm';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { lower, originalLine, readCoreOmv } from '@rasm/extendscript/node';
import { inlineScript } from '@rasm/extendscript/vite';
import { parse } from 'acorn';
import { Array, Cause, Effect, FileSystem, Formatter, Layer, Match, Option, Path, PlatformError, Predicate, Ref, Schema, Sink, Stream, Struct } from 'effect';
import { Zip } from 'effect/encoding';
import { ChildProcessSpawner } from 'effect/process';
import { build } from 'vite';

// --- [TYPES] ---------------------------------------------------------------------------

type Expression = string | { readonly op: '||' | '&&'; readonly left: Expression; readonly right: Expression } | { readonly test: Expression; readonly consequent: Expression; readonly alternate: Expression };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ES3 = { ecmaVersion: 3, allowReserved: 'never', preserveParens: true } as const;
const BOUND = { a: 'ink', b: 'paper', c: 'bleed', d: 'slug' } as const;
const BINDINGS = Struct.keys(BOUND).reduce<readonly Readonly<Record<string, 0 | string>>[]>(
    (rows, key) =>
        rows.flatMap((row) => [
            { ...row, [key]: 0 },
            { ...row, [key]: BOUND[key] },
        ]),
    [{}],
);
const MODERN = [
    { named: 'optional chaining and nullish defaults', code: 'var width = frame?.bounds?.[2] ?? 0;' },
    { named: 'arrow functions and template text', code: `const label = (page) => \`page \${page.name}\`;` },
    { named: 'destructuring and spread', code: 'const { name, ...rest } = layer; const all = [...rest.items, name];' },
    { named: 'classes and methods', code: 'class Swatch { constructor(name) { this.name = name; } get label() { return this.name; } }' },
    { named: 'for-of loops and block bindings', code: 'for (const item of items) { let seen = item; }' },
    { named: 'reserved words as member names', code: 'preferences.default.delete(style.class, style.new);' },
    { named: 'a reserved member name after a comment holding a dot', code: 'var mode = preset./* from a.b */default;' },
    { named: 'reserved words as property keys', code: 'var row = { default: 1, new: 2, in: 3 };' },
    { named: 'reserved words ending a grouped operand', code: 'var mode = override || preset && preset.default;' },
    { named: 'exponents', code: 'var area = side ** 2;' },
] as const;
const PAYLOAD = 'payloads/AdobeExtendScriptToolkit_4_LS22/AdobeExtendScriptToolkit_4_LS22.zip';
const DECLARATIONS = ['Assets2/xplat/CommonOMV/javascript.xml', 'Assets2/xplat/CommonOMV/scriptui.xml'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Expression: Schema.Codec<Expression> = Schema.Union([
    Schema.Literals(['a', 'b', 'c', 'd']),
    Schema.Struct({ op: Schema.Literals(['||', '&&']), left: Schema.suspend(() => Expression), right: Schema.suspend(() => Expression) }),
    Schema.Struct({ test: Schema.suspend(() => Expression), consequent: Schema.suspend(() => Expression), alternate: Schema.suspend(() => Expression) }),
]);
const Inlined = Schema.Struct({ output: Schema.NonEmptyArray(Schema.Struct({ code: Schema.String })) });
const InlineModule = Schema.Struct({ default: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const rank = Match.type<Expression>().pipe(
    Match.when(Predicate.isString, () => 4),
    Match.when({ test: Match.any }, () => 1),
    Match.when({ op: '||' }, () => 2),
    Match.orElse(() => 3),
);
const wrapped = (child: Expression, least: number): string => (rank(child) >= least ? printed(child) : `(${printed(child)})`);
const printed: (expression: Expression) => string = Match.type<Expression>().pipe(
    Match.when(Predicate.isString, (name) => name),
    Match.when({ test: Match.any }, ({ test, consequent, alternate }) => `${wrapped(test, 2)} ? ${wrapped(consequent, 1)} : ${wrapped(alternate, 1)}`),
    Match.when({ op: Match.any }, (logical) => `${wrapped(logical.left, rank(logical))} ${logical.op} ${wrapped(logical.right, rank(logical) + 1)}`),
    Match.exhaustive,
);
const evaluated = (code: string) => Array.map(BINDINGS, (bound): unknown => runInNewContext(`var x;\n${code}\nx;`, { ...bound }));
const commandLine = (command: Parameters<ChildProcessSpawner.ChildProcessSpawner['Service']['spawn']>[0]) => (command._tag === 'StandardCommand' ? [command.command, ...command.args.filter(Predicate.isString)] : []);
const exited = ChildProcessSpawner.makeHandle({
    pid: ChildProcessSpawner.ProcessId(1),
    exitCode: Effect.succeed(ChildProcessSpawner.ExitCode(0)),
    isRunning: Effect.succeed(false),
    kill: () => Effect.void,
    stdin: Sink.drain,
    stdout: Stream.empty,
    stderr: Stream.empty,
    all: Stream.empty,
    getInputFd: () => Sink.drain,
    getOutputFd: () => Stream.empty,
    unref: Effect.succeed(Effect.void),
});
const mounting = (requested: Ref.Ref<readonly string[]>, mount: (at: string) => Effect.Effect<void, PlatformError.PlatformError>) =>
    ChildProcessSpawner.make((command) =>
        Effect.gen(function* () {
            const given = commandLine(command);
            yield* Ref.update(requested, Array.append(given.join(' ')));
            yield* Effect.transposeOption(Option.map(Array.get(given, given.indexOf('--mountPoint') + 1).pipe(Option.filter(() => given.includes('attach'))), mount));
            return exited;
        }),
    );
const unspawned = (requested: Ref.Ref<readonly string[]>, refused: PlatformError.PlatformError) => ChildProcessSpawner.make((command) => Effect.andThen(Ref.update(requested, Array.append(commandLine(command).join(' '))), refused));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(lower, () => {
    it.effect.each(MODERN)(
        'lowers $named to a script a strict ES3 parser reads',
        Effect.fnUntraced(function* ({ code }) {
            const { code: lowered } = yield* lower(code, lower.extendscript);
            yield* Effect.try({ try: () => parse(lowered, ES3), catch: (cause) => new Cause.UnknownError(cause, lowered) });
        }),
    );

    it.effect.each([
        { named: 'an && right of ||', code: 'x = a || b && c;', grouped: 'x = a || (b && c);\n' },
        { named: 'a ternary in the alternate of a ternary', code: 'x = a ? b : c ? d : e;', grouped: 'x = a ? b : (c ? d : e);\n' },
        { named: 'a ternary in the consequent of a ternary', code: 'x = a ? b ? c : d : e;', grouped: 'x = a ? (b ? c : d) : e;\n' },
    ])(
        'parenthesizes $named, which ExtendScript groups otherwise',
        Effect.fnUntraced(function* ({ code, grouped }) {
            assert.strictEqual((yield* lower(code, lower.extendscript)).code, grouped);
        }),
    );

    it.effect.prop(
        'keeps what every || && ?: expression evaluates to',
        { expression: Expression },
        Effect.fnUntraced(function* ({ expression }) {
            const source = `x = ${printed(expression)};`;
            const { code } = yield* lower(source, lower.extendscript);
            assert.deepStrictEqual(evaluated(code), evaluated(source));
        }),
    );

    it.effect(
        'maps a lowered line back to the TypeScript line it came from, and a line past the lowered code to none',
        Effect.fnUntraced(function* () {
            const { code, map } = yield* lower("const first: number = 1; const second = first + 1;\nthrow new Error('marked ' + second);", lower.extendscript, 'typescript');
            const line = code.slice(0, code.indexOf('marked')).split('\n').length;
            const original = yield* originalLine(map);
            assert.notStrictEqual(line, 2);
            assert.deepStrictEqual([original(line), original(code.split('\n').length + 1)], [Option.some(2), Option.none()]);
        }),
    );
});

describe(inlineScript, () => {
    it.effect(
        'builds the host script as a strict ES3 script whose writer answers JSON',
        Effect.fnUntraced(function* () {
            const host = fileURLToPath(import.meta.resolve('@rasm/extendscript/host'));
            const built = yield* Effect.tryPromise({
                try: () => build({ configFile: false, logLevel: 'silent', plugins: [inlineScript(host, 'host')], build: { write: false, rolldownOptions: { input: `${host}?inline`, preserveEntrySignatures: 'exports-only', output: { format: 'es' } } } }),
                catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)),
            });
            const { code } = Array.headNonEmpty((yield* Schema.decodeUnknownEffect(Inlined)(built)).output);
            const script = yield* Effect.promise((): Promise<unknown> => import(`data:text/javascript,${encodeURIComponent(code)}`)).pipe(Effect.flatMap(Schema.decodeUnknownEffect(InlineModule)));
            yield* Effect.try({ try: () => parse(script.default, ES3), catch: (cause) => new Cause.UnknownError(cause, script.default) });
            assert.strictEqual(runInNewContext(`${script.default}\nhost.stringify({ ink: [1, "two"] });`), '{"ink":[1,"two"]}');
        }),
    );
});

it.layer(Layer.mergeAll(layer, layerDigest))('core declarations', (methods) => {
    methods.effect(
        'refuses cached files whose hash differs from the pinned one, attaches the image again, and fails with the error the attach raised',
        Effect.fnUntraced(function* () {
            const [fs, path, requested] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Ref.make<readonly string[]>([])]);
            const [folder, refused] = [yield* fs.makeTempDirectoryScoped(), PlatformError.systemError({ _tag: 'NotFound', module: 'ChildProcess', method: 'spawn' })];
            yield* Effect.forEach(['javascript.xml', 'scriptui.xml'], (name) => fs.writeFileString(path.join(folder, name), '<dictionary/>'), { discard: true });
            const failed = yield* Effect.flip(Effect.provideService(readCoreOmv(folder), ChildProcessSpawner.ChildProcessSpawner, unspawned(requested, refused)));
            assert.deepStrictEqual([failed, Array.map(yield* Ref.get(requested), (argv) => argv.endsWith('.dmg'))], [refused, [true]]);
        }, Effect.scoped),
    );

    methods.effect(
        'reads each declaration file out of the payload archive of the image it attached, refuses one whose hash differs from the pinned one, and ejects the image',
        Effect.fnUntraced(function* () {
            const [fs, path, requested] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Ref.make<readonly string[]>([])]);
            const folder = yield* fs.makeTempDirectoryScoped();
            const mount = (at: string) =>
                Effect.andThen(fs.makeDirectory(path.dirname(path.join(at, PAYLOAD)), { recursive: true }), Stream.fromIterable(Array.map(DECLARATIONS, (member) => Zip.file(member, '<dictionary/>'))).pipe(Stream.pipeThroughChannel(Zip.encode()), Stream.run(fs.sink(path.join(at, PAYLOAD))), Effect.orDie));
            const failed = yield* Effect.flip(Effect.provideService(readCoreOmv(folder), ChildProcessSpawner.ChildProcessSpawner, mounting(requested, mount)));
            const [attach, eject] = yield* Ref.get(requested);
            assert.instanceOf(failed, Cause.UnknownError);
            assert.deepStrictEqual(
                [failed.message, attach?.startsWith('diskutil image attach --nobrowse --mountPoint '), eject === `diskutil eject ${attach?.split(' ')[5]}`],
                ['Assets2/xplat/CommonOMV/javascript.xml hashes to sha256 e601f35a78e2601ef14a428f8185660da1890f5e7cdb5dccb08e0aa2c85f281f in place of fe1dbf4641b8caafe332915cdebf454e165c030d7529692629138a807596cab7', true, true],
            );
        }, Effect.scoped),
    );
});
