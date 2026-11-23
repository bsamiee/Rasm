import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { UxpManifest } from '@rasm/uxp';
import { uxp } from '@rasm/uxp/vite';
import { Array, Cause, Effect, FileSystem, Formatter, Path, Result, Schema } from 'effect';
import sharp from 'sharp';
import { build, mergeConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PANEL = { id: 'typesetting', label: 'Typesetting', type: 'panel' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const manifest = (app: string, icons: readonly object[], entrypoints: readonly object[] = []) => JSON.stringify({ manifestVersion: 5, id: 'app.rasm.test', name: 'Test', version: '0.1.0', main: 'main.js', host: { app }, icons, entrypoints, requiredPermissions: {} });
const built = Effect.fnUntraced(function* (source: string, glyphs: readonly string[], mark?: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    yield* fs.makeDirectory(path.join(folder, 'icons'));
    yield* Effect.all(
        [
            fs.writeFileString(path.join(folder, 'manifest.json'), source),
            fs.writeFileString(path.join(folder, 'main.ts'), 'export {};\n'),
            ...(mark === undefined ? [] : [fs.writeFileString(path.join(folder, 'mark.svg'), mark)]),
            ...glyphs.map((id) => fs.writeFileString(path.join(folder, 'icons', `${id}.svg`), '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 23 23" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M4.25 11.5H18.75"/></svg>')),
        ],
        {
            discard: true,
        },
    );
    const out = path.join(folder, 'out');
    const config = yield* Effect.tryPromise({ try: () => uxp(folder, mark === undefined ? {} : { mark: path.join(folder, 'mark.svg') })({ command: 'build', mode: 'production' }), catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)) });
    yield* Effect.tryPromise({ try: () => build(mergeConfig(config, { configFile: false, root: folder, logLevel: 'silent', build: { outDir: out } })), catch: (cause) => new Cause.UnknownError(cause, Formatter.format(cause)) });
    return (file: string) =>
        Effect.map(Effect.tryPromise({ try: () => sharp(path.join(out, file)).ensureAlpha().raw().toBuffer({ resolveWithObject: true }), catch: (cause) => new Cause.UnknownError(cause, `sharp decodes no image at ${file}`) }), ({ data, info: { width, height } }) => {
            const lit = [...data.keys()].filter((index) => index % 4 === 3 && (data[index] ?? 0) > 0).map((index) => (index - 3) / 4);
            const [across, down] = [Array.map(lit, (index) => index % width), Array.map(lit, (index) => Math.floor(index / width))];
            const strongest = lit.reduce((best, index) => ((data[index * 4 + 3] ?? 0) > (data[best * 4 + 3] ?? 0) ? index : best), lit[0] ?? 0);
            return { size: [width, height], bounds: [Math.min(...across), Math.min(...down), Math.max(...across), Math.max(...down)], strongest: [...data.subarray(strongest * 4, strongest * 4 + 4)] };
        });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('UxpManifest', (methods) => {
    methods.effect.each([
        { species: ['toolbar'], side: 23, refusal: undefined },
        { species: ['toolbar'], side: 24, refusal: 'toolbar takes 23 x 23' },
        { species: ['pluginList'], side: 24, refusal: undefined },
        { species: ['pluginList'], side: 23, refusal: 'pluginList takes 24 x 24' },
        { species: ['generic'], side: 48, refusal: undefined },
        { species: ['toolbar', 'pluginList'], side: 23, refusal: 'pluginList takes 24 x 24' },
    ] as const)('decodes a $species icon at $side px with refusal $refusal', ({ species, side, refusal }) =>
        Effect.sync(() => {
            const decoded = Schema.decodeResult(UxpManifest.json)(manifest('PS', [{ width: side, height: side, path: 'icons/plugin.png', species }]));
            assert.deepStrictEqual(Result.match(decoded, { onSuccess: () => undefined, onFailure: ({ message }) => (refusal !== undefined && message.includes(refusal) ? refusal : message) }), refusal);
        }),
    );
});

it.layer(layer, { excludeTestServices: true })('uxp', (methods) => {
    methods.effect(
        'writes the plug-in list icon from the library mark at 24 and 48 px, its strokes on the 16 px live area, white at Photoshop glyph opacity',
        Effect.fnUntraced(function* () {
            const read = yield* built(manifest('PS', [{ width: 24, height: 24, path: 'icons/plugin-dark.png', scale: [1, 2], theme: ['dark', 'darkest'], species: ['pluginList'] }]), []);
            const [single, double] = yield* Effect.all([read('icons/plugin-dark@1x.png'), read('icons/plugin-dark@2x.png')]);
            assert.deepStrictEqual(
                [single.size, single.bounds, single.strongest, double.size, double.bounds],
                [
                    [24, 24],
                    [4, 4, 19, 19],
                    [255, 255, 255, 204],
                    [48, 48],
                    [8, 8, 39, 39],
                ],
            );
        }, Effect.scoped),
        { timeout: 60_000 },
    );

    methods.effect(
        'writes the plug-in list icon from an app mark in place of the library mark, its 20-unit canvas centred at 1 and 2 px per unit',
        Effect.fnUntraced(function* () {
            const read = yield* built(manifest('ID', [{ width: 24, height: 24, path: 'icons/plugin-light.png', scale: [1, 2], theme: ['light'], species: ['pluginList'] }]), [], '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor"><rect x="1" y="3" width="18" height="14"/></svg>');
            const [single, double] = yield* Effect.all([read('icons/plugin-light@1x.png'), read('icons/plugin-light@2x.png')]);
            assert.deepStrictEqual(
                [single.bounds, double.bounds],
                [
                    [3, 5, 20, 18],
                    [6, 10, 41, 37],
                ],
            );
        }, Effect.scoped),
        { timeout: 60_000 },
    );

    methods.effect(
        'writes the panel icon from its glyph at 23 and 46 px, black at InDesign glyph opacity',
        Effect.fnUntraced(function* () {
            const read = yield* built(manifest('ID', [], [{ ...PANEL, icons: [{ width: 23, height: 23, path: 'icons/typesetting-light.png', scale: [1, 2], theme: ['lightest', 'light'], species: ['toolbar'] }] }]), ['typesetting']);
            const [single, double] = yield* Effect.all([read('icons/typesetting-light@1x.png'), read('icons/typesetting-light@2x.png')]);
            assert.deepStrictEqual(
                [single.size, single.strongest, double.size],
                [
                    [23, 23],
                    [0, 0, 0, 166],
                    [46, 46],
                ],
            );
        }, Effect.scoped),
        { timeout: 60_000 },
    );

    methods.effect.each([
        { name: 'a panel without its glyph', source: manifest('ID', [], [{ ...PANEL, icons: [{ width: 23, height: 23, path: 'icons/typesetting-dark.png', scale: [1], theme: ['dark'], species: ['toolbar'] }] }]), refusal: 'icons/typesetting.svg' },
        { name: 'an icon row across tones', source: manifest('PS', [{ width: 24, height: 24, path: 'icons/plugin.png', scale: [1], theme: ['light', 'dark'], species: ['pluginList'] }]), refusal: 'themes of one tone' },
        { name: 'an icon row with no theme', source: manifest('PS', [{ width: 24, height: 24, path: 'icons/plugin.png', scale: [1], species: ['pluginList'] }]), refusal: 'icons/plugin.png' },
    ])(
        'refuses $name',
        Effect.fnUntraced(function* ({ source, refusal }) {
            const { message } = yield* Effect.flip(built(source, []));
            assert.include(message, refusal);
        }, Effect.scoped),
        { timeout: 60_000 },
    );
});
