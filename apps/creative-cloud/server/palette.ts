// --- [IMPORTS] -------------------------------------------------------------------------

import { utf8fromString } from '@exodus/bytes/utf8.js';
import { utf16fromString, utf16toString } from '@exodus/bytes/utf16.js';
import {
    addCodecSizePrefix,
    type Codec,
    combineCodec,
    containsBytes,
    createDecoderThatConsumesEntireByteArray,
    Endian,
    getArrayCodec,
    getConstantCodec,
    getDiscriminatedUnionCodec,
    getF32Codec,
    getHiddenPrefixCodec,
    getLiteralUnionCodec,
    getStructCodec,
    getTupleCodec,
    getU16Codec,
    getU32Codec,
    getUnionCodec,
    getUnitCodec,
    type NumberCodec,
    transformCodec,
} from '@solana/codecs';
import { Array, Effect, FileSystem, Function, Option, Result, Schema, Struct } from 'effect';
import { AbsolutePath, CHANNELS, Ink } from './values.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _percent: Schema.Finite = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: CHANNELS.percent }));
const _name: Schema.NonEmptyString = Schema.NonEmptyString.check(Schema.makeFilter((value) => value.isWellFormed() && !value.includes('\0'))).annotate({
    description: 'Exact nonempty Unicode name without null characters.',
});

const Spot: Schema.Struct<{
    readonly model: Schema.Literal<'Spot'>;
    readonly name: Schema.NonEmptyString;
    readonly colorType: Schema.Literals<readonly ['PROCESS', 'SPOT']>;
    readonly tint: Schema.Number;
    readonly ink: typeof Ink;
}> = Schema.Struct({
    model: Schema.Literal('Spot'),
    name: _name,
    colorType: Schema.Literals(['PROCESS', 'SPOT']),
    tint: _percent,
    ink: Ink,
});

const Color: Schema.Union<readonly [...(typeof Ink)['members'], typeof Spot]> = Schema.Union([...Ink.members, Spot]);

const Swatch: Schema.Union<readonly [Schema.Struct<{ readonly name: Schema.NonEmptyString; readonly color: typeof Ink }>, Schema.Struct<{ readonly color: typeof Spot }>]> = Schema.Union([
    Schema.Struct({ name: _name, color: Ink }),
    Schema.Struct({ color: Spot }),
]);
const _swatches: Schema.$Array<typeof Swatch> = Schema.Array(Swatch);

const Palette: Schema.Struct<{
    readonly root: Schema.$Array<typeof Swatch>;
    readonly groups: Schema.$Array<Schema.Struct<{ readonly name: Schema.NonEmptyString; readonly swatches: Schema.$Array<typeof Swatch> }>>;
}> = Schema.Struct({ root: _swatches, groups: Schema.Array(Schema.Struct({ name: _name, swatches: _swatches })) });

// --- [EXCHANGE] ------------------------------------------------------------------------

const PaletteError: Schema.TaggedUnion<{
    readonly unsupportedTint: Schema.TaggedStruct<'unsupportedTint', { readonly colors: Schema.NonEmptyArray<Schema.String> }>;
    readonly invalidData: Schema.TaggedStruct<'invalidData', { readonly cause: Schema.Defect }>;
    readonly invalidGroup: Schema.TaggedStruct<'invalidGroup', { readonly block: Schema.Int }>;
    readonly fileFailure: Schema.TaggedStruct<'fileFailure', { readonly path: typeof AbsolutePath; readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    unsupportedTint: { colors: Schema.NonEmptyArray(Schema.String) },
    invalidData: { cause: Schema.Defect() },
    invalidGroup: { block: Schema.Int },
    fileFailure: { path: AbsolutePath, cause: Schema.Defect() },
});

const _u16 = getU16Codec({ endian: Endian.Big });
const _u32 = getU32Codec({ endian: Endian.Big });
const _float = transformCodec(getF32Codec({ endian: Endian.Big }), (value: number) => Schema.encodeSync(Schema.Finite)(Math.fround(value)));
const _terminated = getArrayCodec(_u16, { size: { __kind: 'sentinel', sentinel: _u16.encode(0) } });
const bounded = <From, To extends From>(codec: Codec<From, To>, prefix: NumberCodec): Codec<From, To> =>
    addCodecSizePrefix(combineCodec(codec, createDecoderThatConsumesEntireByteArray(codec)), prefix);
const _unicode = transformCodec(
    bounded(
        _terminated,
        transformCodec(
            _u16,
            (bytes: number | bigint) => Number(bytes) / _u16.fixedSize,
            (units) => units * _u16.fixedSize,
        ),
    ),
    (name: string) => Array.fromIterable(utf16fromString(Schema.decodeUnknownSync(_name)(name))),
    (units) => Schema.decodeUnknownSync(_name)(utf16toString(Uint16Array.from(units))),
);
const _models = [
    { schema: Ink.members[0], signature: utf8fromString('RGB '), scale: Function.constant(CHANNELS.rgb) },
    { schema: Ink.members[1], signature: utf8fromString('CMYK'), scale: Function.constant(CHANNELS.percent) },
    { schema: Ink.members[2], signature: utf8fromString('Gray'), scale: Function.constant(1) },
    { schema: Ink.members[3], signature: utf8fromString('LAB '), scale: (index: number): number => (index === 0 ? CHANNELS.percent : 1) },
] as const;
const _ink = getUnionCodec(
    Array.map(_models, ({ schema, signature, scale }) =>
        transformCodec(
            getHiddenPrefixCodec(getArrayCodec(_float, { size: schema.fields.values.elements.length }), [getConstantCodec(signature)]),
            (ink: (typeof Ink)['Type']) => Array.map(Schema.encodeSync(Ink)(ink).values, (value, index) => value / scale(index)),
            (values) => Schema.decodeUnknownSync(Ink)({ model: schema.fields.model.literal, values: Array.map(values, (value, index) => value * scale(index)) }),
        ),
    ),
    (ink) => _models.findIndex(({ schema }) => schema.fields.model.literal === ink.model),
    (bytes, offset) => _models.findIndex(({ signature }) => containsBytes(bytes, signature, offset)),
);
const _swatch = transformCodec(
    getStructCodec([
        ['name', _unicode],
        ['ink', _ink],
        ['kind', getLiteralUnionCodec(['PROCESS', 'SPOT', 'normal'], { size: _u16 })],
    ]),
    (swatch: (typeof Swatch)['Type']) =>
        'name' in swatch ? { name: swatch.name, ink: swatch.color, kind: 'normal' as const } : { name: swatch.color.name, ink: swatch.color.ink, kind: swatch.color.colorType },
    ({ name, ink, kind }): (typeof Swatch)['Type'] => (kind === 'normal' ? { name, color: ink } : { color: { model: 'Spot', name, colorType: kind, tint: CHANNELS.percent, ink } }),
);
const _tags = { color: 0x00_01, start: 0xc0_01, end: 0xc0_02 } as const;
const _variants = [
    [_tags.color, bounded(getStructCodec([['swatch', _swatch]]), _u32)],
    [_tags.start, bounded(getStructCodec([['name', _unicode]]), _u32)],
    [_tags.end, bounded(getUnitCodec(), _u32)],
] as const;
const _block = getDiscriminatedUnionCodec(_variants, {
    discriminator: 'kind',
    size: transformCodec(
        _u16,
        (index: number | bigint) => Array.getUnsafe(_variants, Number(index))[0],
        (tag) => _variants.findIndex(([kind]) => kind === tag),
    ),
});
const _ase = getHiddenPrefixCodec(getArrayCodec(_block, { size: _u32, requireSizePrefix: true }), [
    getConstantCodec(utf8fromString('ASEF')),
    getConstantCodec(getTupleCodec([_u16, _u16]).encode([1, 0])),
]);

const encodeAse = (palette: (typeof Palette)['Type']): Effect.Effect<Uint8Array, (typeof PaletteError)['Type']> => {
    const tinted = Array.filterMap(Array.flatten([palette.root, ...Array.map(palette.groups, Struct.get('swatches'))]), (swatch) =>
        swatch.color.model === 'Spot' && swatch.color.tint !== CHANNELS.percent ? Result.succeed(swatch.color.name) : Result.failVoid,
    );
    if (Array.isArrayNonEmpty(tinted)) {
        return Effect.fail(PaletteError.cases.unsupportedTint.make({ colors: tinted }));
    }
    return Effect.try({
        try: () =>
            Uint8Array.from(
                _ase.encode([
                    ...Array.map(palette.root, (swatch) => ({ kind: _tags.color, swatch })),
                    ...Array.flatMap(palette.groups, ({ name, swatches }) => [{ kind: _tags.start, name }, ...Array.map(swatches, (swatch) => ({ kind: _tags.color, swatch })), { kind: _tags.end }]),
                ]),
            ),
        catch: (cause) => PaletteError.cases.invalidData.make({ cause }),
    });
};

const decodeAse: (bytes: Uint8Array) => Effect.Effect<(typeof Palette)['Type'], (typeof PaletteError)['Type']> = Effect.fnUntraced(function* (bytes) {
    const blocks = yield* Effect.try({
        try: () => createDecoderThatConsumesEntireByteArray(_ase).decode(bytes),
        catch: (cause) => PaletteError.cases.invalidData.make({ cause }),
    });
    const root: (typeof Swatch)['Type'][] = [];
    const groups: (typeof Palette)['Type']['groups'][number][] = [];
    const unclosed = yield* Effect.reduce(blocks, Option.none<{ readonly name: string; readonly swatches: (typeof Swatch)['Type'][] }>, (group, block, index) => {
        if (block.kind === _tags.color) {
            Option.match(group, { onNone: () => root.push(block.swatch), onSome: (open) => open.swatches.push(block.swatch) });
            return Effect.succeed(group);
        }
        if (block.kind === _tags.start) {
            return Option.match(group, {
                onNone: () => Effect.succeed(Option.some({ name: block.name, swatches: [] })),
                onSome: () => Effect.fail(PaletteError.cases.invalidGroup.make({ block: index })),
            });
        }
        return Effect.map(
            Effect.fromOption(group, () => PaletteError.cases.invalidGroup.make({ block: index })),
            (open) => {
                groups.push(open);
                return Option.none();
            },
        );
    });
    return yield* Option.match(unclosed, {
        onNone: () => Effect.succeed({ root, groups }),
        onSome: () => Effect.fail(PaletteError.cases.invalidGroup.make({ block: blocks.length })),
    });
});

// --- [FILES] --------------------------------------------------------------------------

const PaletteFile: Schema.Struct<{ readonly path: typeof AbsolutePath; readonly format: Schema.Literal<'ase'>; readonly palette: typeof Palette }> = Schema.Struct({
    path: AbsolutePath,
    format: Schema.Literal('ase'),
    palette: Palette,
});

const read: (path: AbsolutePath) => Effect.Effect<(typeof PaletteFile)['Type'], (typeof PaletteError)['Type'], FileSystem.FileSystem> = Effect.fnUntraced(function* (path) {
    const fs = yield* FileSystem.FileSystem;
    const bytes = yield* fs.readFile(path).pipe(Effect.mapError((cause) => PaletteError.cases.fileFailure.make({ path, cause })));
    return { path, format: 'ase', palette: yield* decodeAse(bytes) };
});

const compile: (palette: (typeof Palette)['Type'], path: AbsolutePath) => Effect.Effect<(typeof PaletteFile)['Type'], (typeof PaletteError)['Type'], FileSystem.FileSystem> = Effect.fnUntraced(
    function* (palette, path) {
        const bytes = yield* encodeAse(palette);
        const fs = yield* FileSystem.FileSystem;
        yield* fs.writeFile(path, bytes).pipe(Effect.mapError((cause) => PaletteError.cases.fileFailure.make({ path, cause })));
        return yield* read(path);
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Color, compile, decodeAse, encodeAse, Palette, PaletteError, PaletteFile, read, Spot, Swatch };
