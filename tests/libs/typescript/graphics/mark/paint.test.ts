import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { paint } from '@rasm/mark';
import { Array, Cause, Effect, FileSystem, Path } from 'effect';
import sharp from 'sharp';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DRAWINGS = {
    small: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor"><rect width="16" height="8"/></svg>',
    master: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><rect width="12" height="24"/></svg>',
    accented: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><rect width="12" height="24"/><rect class="accent" x="12" width="12" height="24"/></svg>',
    filled: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#00ff00"><rect width="12" height="24"/></svg>',
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = Effect.fnUntraced(function* (names: Array.NonEmptyReadonlyArray<keyof typeof DRAWINGS>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    return yield* Effect.forEach(names, (name) => Effect.as(fs.writeFileString(path.join(folder, `${name}.svg`), DRAWINGS[name]), path.join(folder, `${name}.svg`)));
});
const decoded = (image: Uint8Array) =>
    Effect.map(Effect.tryPromise({ try: () => sharp(image).ensureAlpha().raw().toBuffer({ resolveWithObject: true }), catch: (cause) => new Cause.UnknownError(cause, 'sharp decodes no image from the painted bytes') }), ({ data, info: { width } }) => {
        const lit = [...data.keys()].filter((index) => index % 4 === 3 && (data[index] ?? 0) > 0).map((index) => (index - 3) / 4);
        const [across, down] = [Array.map(lit, (index) => index % width), Array.map(lit, (index) => Math.floor(index / width))];
        return { bounds: [Math.min(...across), Math.min(...down), Math.max(...across), Math.max(...down)], at: (x: number, y: number) => [...data.subarray((y * width + x) * 4, (y * width + x) * 4 + 4)] };
    });
const header = (image: Uint8Array) => {
    const view = new DataView(image.buffer, image.byteOffset, image.byteLength);
    return [view.getUint32(16), view.getUint32(20)];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('paint', (methods) => {
    const STYLE = 'svg { color: #8f7af9; }\n.accent { fill: #339af0; }\n';

    methods.effect.each([
        { names: ['small', 'master'], width: 16, padding: undefined, bounds: [0, 0, 15, 7] },
        { names: ['small', 'master'], width: 32, padding: undefined, bounds: [0, 0, 31, 15] },
        { names: ['small', 'master'], width: 48, padding: undefined, bounds: [0, 0, 23, 47] },
        { names: ['master', 'small'], width: 16, padding: undefined, bounds: [0, 0, 15, 7] },
        { names: ['master'], width: 16, padding: undefined, bounds: [0, 0, 7, 15] },
        { names: ['small', 'master'], width: 128, padding: 16, bounds: [40, 16, 87, 111] },
        { names: ['master'], width: 24, padding: 4, bounds: [8, 4, 15, 19] },
    ] as const)(
        'renders $names at $width px with padding $padding to artwork bounds $bounds',
        Effect.fnUntraced(function* ({ names, width, padding, bounds }) {
            const drawings = yield* written(names);
            const image = yield* paint(drawings, STYLE, { width, padding });
            const shown = yield* decoded(image);
            assert.deepStrictEqual([header(image), shown.bounds], [[width, width], bounds]);
        }, Effect.scoped),
    );

    methods.effect.each([
        { name: 'master', drawn: 'currentColor', style: STYLE, at: [3, 24], pixel: [0x8f, 0x7a, 0xf9, 0xff] },
        { name: 'accented', drawn: 'class accent', style: STYLE, at: [36, 24], pixel: [0x33, 0x9a, 0xf0, 0xff] },
        { name: 'filled', drawn: 'its own fill', style: STYLE, at: [3, 24], pixel: [0x00, 0xff, 0x00, 0xff] },
        { name: 'master', drawn: 'currentColor', style: 'svg { color: #ffffff; opacity: 0.6; }', at: [3, 24], pixel: [0xff, 0xff, 0xff, 0x99] },
    ] as const)(
        'paints a part drawn in $drawn under $style as $pixel',
        Effect.fnUntraced(function* ({ name, style, at: [x, y], pixel }) {
            const drawings = yield* written([name]);
            const shown = yield* decoded(yield* paint(drawings, style, { width: 48 }));
            assert.deepStrictEqual(shown.at(x, y), [...pixel]);
        }, Effect.scoped),
    );

    methods.effect(
        'writes the scalable form from the largest drawing under its 24-unit view box with the stylesheet colors resolved',
        Effect.fnUntraced(function* () {
            const drawings = yield* written(['small', 'accented']);
            const image = yield* paint(drawings, STYLE);
            const shown = yield* decoded(image);
            assert.deepStrictEqual([/^<svg[^>]*\sviewBox="(?<box>[^"]*)"/u.exec(new TextDecoder().decode(image))?.groups?.['box'], shown.bounds, shown.at(3, 12), shown.at(18, 12)], ['0 0 24 24', [0, 0, 23, 23], [0x8f, 0x7a, 0xf9, 0xff], [0x33, 0x9a, 0xf0, 0xff]]);
        }, Effect.scoped),
    );
});
