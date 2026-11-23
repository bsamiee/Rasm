import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { paint } from '@rasm/mark';
import { Array, Cause, Effect, FileSystem, Path, Schema } from 'effect';
import sharp from 'sharp';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DRAWINGS = {
    accented: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><rect width="12" height="24"/><rect class="accent" x="12" width="12" height="24"/></svg>',
    filled: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#00ff00"><rect width="12" height="24"/></svg>',
    master: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><rect width="12" height="24"/></svg>',
    small: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="currentColor"><rect width="16" height="8"/></svg>',
} as const;
const FILLS = { 12: [0xff, 0x00, 0x00], 16: [0x00, 0xff, 0x00], 20: [0x00, 0x00, 0xff], 24: [0xff, 0xff, 0x00], 32: [0x00, 0xff, 0xff] } as const;
const TONE = { color: { mode: 'rgb', r: 0x8f / 0xff, g: 0x7a / 0xff, b: 0xf9 / 0xff }, parts: { accent: { fill: { mode: 'rgb', r: 0x33 / 0xff, g: 0x9a / 0xff, b: 0xf0 / 0xff } } } } as const;
const limit = { timeout: 120_000, arbitrary: { runs: 16, concurrency: 8 } };

// --- [MODELS] --------------------------------------------------------------------------

const Between = (minimum: number, maximum: number) => Schema.Int.check(Schema.isBetween({ minimum, maximum }));
const Side = Schema.Literals([12, 16, 20, 24, 32]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = Effect.fnUntraced(function* (sources: Array.NonEmptyReadonlyArray<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* fs.makeTempDirectoryScoped();
    return yield* Effect.forEach(sources, (source, index) => Effect.as(fs.writeFileString(path.join(folder, `${String(index)}.svg`), source), path.join(folder, `${String(index)}.svg`)));
});
const filled = (side: keyof typeof FILLS) => `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${String(side)} ${String(side)}" fill="rgb(${FILLS[side].join(',')})"><rect width="${String(side)}" height="${String(side)}"/></svg>`;
const decoded = (image: Uint8Array) =>
    Effect.map(Effect.tryPromise({ try: () => sharp(image).ensureAlpha().raw().toBuffer({ resolveWithObject: true }), catch: (cause) => new Cause.UnknownError(cause, 'sharp decodes no image from the painted bytes') }), ({ data, info: { width, height } }) => {
        const covered = Array.map(
            Array.filter(Array.fromIterable(data.keys()), (index) => index % 4 === 3 && (data[index] ?? 0) >= 0x80),
            (index) => (index - 3) / 4,
        );
        const [across, down] = [Array.map(covered, (index) => index % width), Array.map(covered, (index) => Math.floor(index / width))];
        const [left, top, right, bottom] = [Math.min(...across), Math.min(...down), Math.max(...across), Math.max(...down)];
        return { size: [width, height], extent: [right - left + 1, bottom - top + 1] as const, margins: [left, top, width - 1 - right, height - 1 - bottom] as const, at: (x: number, y: number) => [...data.subarray((y * width + x) * 4, (y * width + x) * 4 + 4)] };
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('paint', (methods) => {
    methods.effect.prop(
        'fits the artwork of a padded mark centered in the width less both paddings along its longer side',
        { left: Between(0, 10), right: Between(14, 24), top: Between(0, 10), bottom: Between(14, 24), width: Between(40, 160), padding: Between(1, 8) },
        Effect.fnUntraced(function* ({ left, right, top, bottom, width, padding }) {
            const [drawing] = yield* written([`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><rect x="${String(left)}" y="${String(top)}" width="${String(right - left)}" height="${String(bottom - top)}"/></svg>`]);
            const { size, extent, margins } = yield* Effect.flatMap(paint([drawing], TONE, { width, padding }), decoded);
            assert.deepStrictEqual(size, [width, width]);
            assert.closeTo(Math.max(...extent), width - 2 * padding, 1);
            assert.closeTo(margins[0], margins[2], 1);
            assert.closeTo(margins[1], margins[3], 1);
        }, Effect.scoped),
        limit,
    );

    methods.effect.prop(
        'scales an unpadded mark by the largest whole factor of its canvas the width holds, centered',
        { side: Side, beyond: Between(0, 128) },
        Effect.fnUntraced(function* ({ side, beyond }) {
            const width = side + beyond;
            const { size, extent, margins } = yield* Effect.flatMap(
                Effect.flatMap(written([filled(side)]), (drawings) => paint(drawings, TONE, { width })),
                decoded,
            );
            assert.deepStrictEqual(size, [width, width]);
            assert.closeTo(extent[0], Math.floor(width / side) * side, 1);
            assert.closeTo(extent[1], Math.floor(width / side) * side, 1);
            assert.closeTo(margins[0], margins[2], 1);
            assert.closeTo(margins[1], margins[3], 1);
        }, Effect.scoped),
        limit,
    );

    methods.effect.each([
        { sides: [16, 24], width: 32, padding: undefined, chosen: 16, covers: 'twice its side over one side of the larger' },
        { sides: [16, 24], width: 48, padding: undefined, chosen: 24, covers: 'the width as the smaller does, the larger winning the tie' },
        { sides: [12, 16, 24], width: 40, padding: undefined, chosen: 12, covers: '36 pixels over 32 and 24' },
        { sides: [16, 24], width: 8, padding: undefined, chosen: 16, covers: 'nothing at a whole factor, the smallest scaled down' },
        { sides: [24, 32], width: 36, padding: 8, chosen: 24, covers: 'nothing within the width less both paddings, the smallest scaled into it' },
    ] as const)(
        'draws $sides at $width px with padding $padding from the $chosen px drawing, which covers $covers',
        Effect.fnUntraced(function* ({ sides, width, padding, chosen }) {
            const shown = yield* Effect.flatMap(
                Effect.flatMap(written(Array.map(sides, filled)), (drawings) => paint(drawings, TONE, { width, padding })),
                decoded,
            );
            assert.deepStrictEqual(shown.at(width / 2, width / 2), [...FILLS[chosen], 0xff]);
        }, Effect.scoped),
    );

    methods.effect(
        'paints a drawing wider than the width as a PNG the width across, keeping its aspect',
        Effect.fnUntraced(function* () {
            const image = yield* Effect.flatMap(written(['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 16" fill="currentColor"><rect width="32" height="16"/></svg>']), (drawings) => paint(drawings, TONE, { width: 16 }));
            assert.deepStrictEqual(
                [[...image.subarray(0, 8)], (yield* decoded(image)).size],
                [
                    [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a],
                    [16, 8],
                ],
            );
        }, Effect.scoped),
    );

    methods.effect.each([
        { name: 'master', drawn: 'currentColor', tone: TONE, at: [3, 24], pixel: [0x8f, 0x7a, 0xf9, 0xff] },
        { name: 'accented', drawn: 'class accent', tone: TONE, at: [36, 24], pixel: [0x33, 0x9a, 0xf0, 0xff] },
        { name: 'filled', drawn: 'its own fill', tone: TONE, at: [3, 24], pixel: [0x00, 0xff, 0x00, 0xff] },
        { name: 'master', drawn: 'currentColor', tone: { color: { mode: 'rgb', r: 1, g: 1, b: 1, alpha: 0.6 } }, at: [3, 24], pixel: [0xff, 0xff, 0xff, 0x99] },
    ] as const)(
        'paints a part drawn in $drawn under tone $tone.color as $pixel',
        Effect.fnUntraced(function* ({ name, tone, at: [x, y], pixel }) {
            const shown = yield* Effect.flatMap(
                Effect.flatMap(written([DRAWINGS[name]]), (drawings) => paint(drawings, tone, { width: 48 })),
                decoded,
            );
            assert.deepStrictEqual(shown.at(x, y), [...pixel]);
        }, Effect.scoped),
    );

    methods.effect(
        'writes the scalable form from the largest drawing at its own size with the tone colors resolved',
        Effect.fnUntraced(function* () {
            const image = yield* Effect.flatMap(written([DRAWINGS.small, DRAWINGS.accented]), (drawings) => paint(drawings, TONE));
            const shown = yield* decoded(image);
            assert.deepStrictEqual([new TextDecoder().decode(image.subarray(0, 4)), shown.size, shown.at(3, 12), shown.at(18, 12)], ['<svg', [24, 24], [0x8f, 0x7a, 0xf9, 0xff], [0x33, 0x9a, 0xf0, 0xff]]);
        }, Effect.scoped),
    );
});
