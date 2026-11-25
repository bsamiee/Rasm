import { fileURLToPath } from 'node:url';
import { runInNewContext } from 'node:vm';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { lower, originalLine, readCoreOmv } from '@rasm/extendscript/node';
import { inlineScript } from '@rasm/extendscript/vite';
import { parse } from 'acorn';
import { Array, Cause, Effect, FileSystem, Formatter, Layer, Match, Option, Path, PlatformError, Predicate, Ref, Schema, Struct } from 'effect';
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
const unspawned = (requested: Ref.Ref<readonly string[]>, refused: PlatformError.PlatformError) => ChildProcessSpawner.make((command) => Effect.andThen(Ref.update(requested, Array.appendAll(command._tag === 'StandardCommand' ? [[command.command, ...command.args.filter(Predicate.isString)].join(' ')] : [])), refused));

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
            const { code, map } = yield* lower("const first: number = 1; const second = first + 1; throw new Error('marked ' + second);", lower.extendscript, 'typescript');
            const line = code.slice(0, code.indexOf('marked')).split('\n').length;
            const original = yield* originalLine(map);
            assert.notStrictEqual(line, 1);
            assert.deepStrictEqual([Option.map(original(line), Struct.get('line')), original(code.split('\n').length + 1)], [Option.some(1), Option.none()]);
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
});
