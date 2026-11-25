import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { UxpManifest } from '@rasm/uxp';
import { pluginConfig } from '@rasm/uxp/vite';
import { Array, Cause, Effect, FileSystem, Formatter, Path, Predicate, Result, Schema, Struct } from 'effect';
import sharp from 'sharp';
import { build, mergeConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = { toolbar: 23, pluginList: 24 } as const;
const SCALES = [1, 2] as const;
const TONES = { lightest: '0,0,0', light: '0,0,0', medium: '255,255,255', dark: '255,255,255', darkest: '255,255,255' } as const;
const STATED = { manifestVersion: 5, id: 'app.rasm.test', name: 'Test', version: '0.1.0', host: { app: 'PS' }, requiredPermissions: {} } as const;
const COMMAND = { id: 'reset', label: 'Reset', type: 'command' } as const;
const PANEL = { id: 'typesetting', label: 'Typesetting', type: 'panel' } as const;
const GLYPH = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 23 23" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M4.25 11.5H18.75"/></svg>';
const ADDONS = Array.cartesianWith(Array.cartesian([5, 6] as const, [{}, { enableAddon: false }, { enableAddon: true }]), [{}, { addon: { name: 'Typesetting' } }], ([manifestVersion, requiredPermissions], addon) => ({
    manifestVersion,
    requiredPermissions,
    addon,
    accepted: !('addon' in addon) || (manifestVersion === 6 && 'enableAddon' in requiredPermissions && requiredPermissions.enableAddon),
}));

// --- [MODELS] --------------------------------------------------------------------------

const Side = Schema.Union([Schema.Literals([23, 24]), Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 64 }))]);
const Row = Schema.Struct({ path: Schema.String, scale: Schema.Array(Schema.Int), theme: Schema.NonEmptyArray(Schema.Literals(Struct.keys(TONES))), species: Schema.Tuple([Schema.Literals(['toolbar', 'pluginList'])]) });
const Written = Schema.fromJsonString(
    Schema.Struct({
        main: Schema.String,
        host: Schema.Struct({ minVersion: Schema.SemVerFromString }),
        icons: Schema.Array(Row),
        entrypoints: Schema.Tuple([Schema.Struct({ id: Schema.String, icons: Schema.optionalKey(Schema.Array(Row)) }), Schema.Struct({ id: Schema.String, icons: Schema.Array(Row) })]),
        requiredPermissions: Schema.Struct({ network: Schema.Struct({ domains: Schema.Array(Schema.String) }) }),
        featureFlags: Schema.Struct({ unhandledRejection: Schema.Boolean }),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const built = Effect.fnUntraced(function* (manifest: object, main: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    yield* fs.makeDirectory(path.join(folder, 'icons'));
    yield* Effect.all([fs.writeFileString(path.join(folder, 'manifest.json'), yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown))(manifest)), fs.writeFileString(path.join(folder, 'main.ts'), main), fs.writeFileString(path.join(folder, 'icons', `${PANEL.id}.svg`), GLYPH)], { discard: true });
    const out = path.join(folder, 'out');
    yield* Effect.tryPromise({
        try: () => build(mergeConfig(pluginConfig(), { configFile: false, root: folder, logLevel: 'silent', build: { outDir: out } })),
        catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)),
    });
    return { out, text: yield* fs.readFileString(path.join(out, UxpManifest.file)) };
});
const tones = (data: Uint8Array) => Array.dedupe(Array.filterMap(Array.chunksOf(Array.fromIterable(data), 4), ([red, green, blue, alpha = 0]) => (alpha > 0 ? Result.succeed(`${red},${green},${blue}`) : Result.failVoid)));
const scaled =
    (folder: string, join: (...parts: string[]) => string) =>
    ({ path: file, theme, species: [species] }: typeof Row.Type) =>
        Array.map(SCALES, (scale) => ({ file: join(folder, file.replace(/\.png$/u, `@${scale}x.png`)), side: ADOBE[species] * scale, tone: Array.dedupe(Array.map(theme, (each) => TONES[each])) }));
const drawn = (file: string) => Effect.promise(() => sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true }));
const misdrawn = (out: string, rows: readonly (typeof Row.Type)[]) =>
    Effect.flatMap(Path.Path, (path) => Effect.forEach(Array.flatMap(rows, scaled(out, path.join)), ({ file, side, tone }) => Effect.map(drawn(file), ({ data, info: { width, height } }) => ({ file, drawn: [width, height, tones(data)], expected: [side, side, tone] }))));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(UxpManifest, () => {
    it.each([
        { species: ['toolbar'], width: 23, height: 23, accepted: true },
        { species: ['toolbar'], width: 23, height: 24, accepted: false },
        { species: ['pluginList'], width: 24, height: 24, accepted: true },
        { species: ['pluginList'], width: 24, height: 23, accepted: false },
        { species: ['toolbar', 'pluginList'], width: 23, height: 23, accepted: false },
        { species: ['generic'], width: 48, height: 48, accepted: true },
    ] as const)('decides a $species icon at $width x $height as accepted $accepted', ({ species, width, height, accepted }) => {
        assert.strictEqual(Result.isSuccess(Schema.decodeResult(UxpManifest)({ ...STATED, entrypoints: [], icons: [{ width, height, path: 'icons/plugin.png', species }] })), accepted);
    });

    it.prop('accepts an icon exactly when every species it lists takes its width and height', { width: Side, height: Side, species: Schema.NonEmptyArray(Schema.Literals(['generic', 'toolbar', 'pluginList'])) }, ({ width, height, species }) => {
        const decoded = Schema.decodeResult(UxpManifest)({ ...STATED, entrypoints: [], icons: [{ width, height, path: 'icons/plugin.png', species }] });
        assert.strictEqual(
            Result.isSuccess(decoded),
            species.every((kind) => kind === 'generic' || (width === ADOBE[kind] && height === ADOBE[kind])),
        );
    });

    it.each(ADDONS)('decides addon $addon under manifest version $manifestVersion with permissions $requiredPermissions as accepted $accepted', ({ manifestVersion, requiredPermissions, addon, accepted }) => {
        const decoded = Schema.decodeResult(UxpManifest)({ ...STATED, manifestVersion, entrypoints: [], requiredPermissions, ...addon });
        assert.strictEqual(Result.isSuccess(decoded), accepted);
    });
});

it.layer(layer, { excludeTestServices: true })('uxp', (methods) => {
    methods.effect(
        'writes the entry file, a host floor, both icon roles at 1x and 2x in black for light and white for dark themes, the link origin beside stated domains, and the rejection flag',
        Effect.fnUntraced(function* () {
            const { out, text } = yield* built({ ...STATED, entrypoints: [COMMAND, PANEL], requiredPermissions: { network: { domains: ['https://example.com'] } } }, 'export {};\n');
            const written = yield* Schema.decodeEffect(Written)(text);
            const [command, panel] = written.entrypoints;
            const rows = [...written.icons, ...panel.icons];
            yield* Schema.decodeEffect(UxpManifest.json)(text);
            assert.strictEqual(written.main, 'main.js');
            assert.isFalse(Predicate.hasProperty(command, 'icons'));
            assert.deepStrictEqual(Array.dedupe(Array.map(rows, ({ species: [species], scale }) => `${species} ${scale.join(',')}`)), ['pluginList 1,2', 'toolbar 1,2']);
            assert.sameMembers(Array.flatMap(written.icons, Struct.get('theme')), Struct.keys(TONES));
            assert.sameMembers(Array.flatMap(panel.icons, Struct.get('theme')), Struct.keys(TONES));
            for (const { file, drawn: read, expected } of yield* misdrawn(out, rows)) {
                assert.deepStrictEqual(read, expected, file);
            }
            assert.deepStrictEqual(written.requiredPermissions.network.domains, ['https://example.com', new URL(UxpManifest.link).origin]);
            assert.isTrue(written.featureFlags.unhandledRejection);
        }, Effect.scoped),
    );

    methods.effect(
        'refuses an entry importing a Node builtin the UXP host lacks, naming the import and its importer',
        Effect.fnUntraced(function* () {
            const { message } = yield* Effect.flip(built({ ...STATED, entrypoints: [] }, "import { readFileSync } from 'node:fs';\nexport const read = readFileSync;\n"));
            assert.match(message, /main\.ts imports node:fs/u);
        }, Effect.scoped),
    );
});
