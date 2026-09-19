// --- [IMPORTS] -------------------------------------------------------------------------

import { BinaryView } from '@ot-builder/bin-util';
import { readOtMetadata } from '@ot-builder/io-bin-metadata';
import { readSfntView, writeSfntOtf } from '@ot-builder/io-bin-sfnt';
import { Fvar, Head, Maxp, Os2, Post } from '@ot-builder/ot-metadata';
import { Name } from '@ot-builder/ot-name';
import type { Sfnt } from '@ot-builder/ot-sfnt';
import { addDecoderSizePrefix, Endian, fixDecoderSize, getArrayDecoder, getBytesDecoder, getStructDecoder, getU16Decoder, getU32Decoder, getUtf8Decoder, transformDecoder } from '@solana/codecs';
import { Array, Boolean, Crypto, Effect, Encoding, FileSystem, HashMap, Match, Number, Option, Order, Path, Predicate, pipe, Record, Result, Schema, String, Struct } from 'effect';
import { fdir } from 'fdir';
import { unzlibSync } from 'fflate';
import fonteditor from 'fonteditor-core';
import { create } from 'fontkit';
import { Blob, Face, Font, MetricsTag, Variation } from 'harfbuzzjs';
import { CodePoint, CSS, FontFace, FontWeight, Measured, MetricsError } from './metrics.ts';

// --- [MODELS] --------------------------------------------------------------------------

type FontRequest = typeof FontRequest.Type;
type FontSelector = Pick<FontRequest, 'axes' | 'codePoints'> & (FontRequest | Pick<FontFace, 'postScriptName'> | Pick<FontFace, 'fullName'> | Pick<FontFace, 'digest' | 'index'>);

const FontRequest: Schema.Struct<
    Pick<typeof FontFace.fields, 'family' | 'width' | 'angle' | 'italic' | 'codePoints'> & {
        readonly weight: typeof FontWeight;
        readonly axes: Schema.$Record<Schema.String, Schema.Finite>;
    }
> = Schema.Struct({
    ...Struct.pick(FontFace.fields, ['family', 'width', 'angle', 'italic', 'codePoints']),
    weight: FontWeight,
    axes: Schema.Record(Schema.String, Schema.Finite),
});
const FontSelector: Schema.Codec<FontSelector, FontSelector> = Schema.Union([
    FontRequest,
    Schema.Struct({ postScriptName: FontFace.fields.postScriptName, axes: FontRequest.fields.axes, codePoints: FontRequest.fields.codePoints }),
    Schema.Struct({ fullName: FontFace.fields.fullName, axes: FontRequest.fields.axes, codePoints: FontRequest.fields.codePoints }),
    Schema.Struct({ ...Struct.pick(FontFace.fields, ['digest', 'index']), axes: FontRequest.fields.axes, codePoints: FontRequest.fields.codePoints }),
]);
const _Candidates: Schema.NonEmptyArray<typeof FontFace> = Schema.NonEmptyArray(FontFace);

const FontError: Schema.TaggedUnion<{
    readonly fontSourceUnreadable: Schema.TaggedStruct<'fontSourceUnreadable', { readonly path: Schema.String; readonly cause: Schema.Defect }>;
    readonly fontFaceUnreadable: Schema.TaggedStruct<'fontFaceUnreadable', { readonly path: Schema.String; readonly index: Schema.Int; readonly cause: Schema.Defect }>;
    readonly fontFormatUnsupported: Schema.TaggedStruct<'fontFormatUnsupported', { readonly path: Schema.String; readonly index: Schema.Int; readonly format: Schema.Literal<'bhed'> }>;
    readonly fontUnavailable: Schema.TaggedStruct<'fontUnavailable', { readonly request: typeof FontSelector }>;
    readonly fontAmbiguous: Schema.TaggedStruct<'fontAmbiguous', { readonly request: typeof FontSelector; readonly faces: typeof _Candidates }>;
    readonly fontWeightUnknown: Schema.TaggedStruct<'fontWeightUnknown', { readonly request: typeof FontSelector; readonly faces: typeof _Candidates }>;
    readonly fontAxisScaleUnsupported: Schema.TaggedStruct<'fontAxisScaleUnsupported', { readonly request: typeof FontSelector; readonly faces: typeof _Candidates }>;
}> = Schema.TaggedUnion({
    fontSourceUnreadable: { path: Schema.String, cause: Schema.Defect() },
    fontFaceUnreadable: { path: Schema.String, index: Schema.Int, cause: Schema.Defect() },
    fontFormatUnsupported: { path: Schema.String, index: Schema.Int, format: Schema.Literal('bhed') },
    fontUnavailable: { request: FontSelector },
    fontAmbiguous: { request: FontSelector, faces: _Candidates },
    fontWeightUnknown: { request: FontSelector, faces: _Candidates },
    fontAxisScaleUnsupported: { request: FontSelector, faces: _Candidates },
});

// --- [CONTAINERS] ----------------------------------------------------------------------

const _U16 = getU16Decoder({ endian: Endian.Big });
const _U32 = getU32Decoder({ endian: Endian.Big });
const _RESOURCE = { emptyCount: 0xff_ff, mapReservedBytes: 24, offsetRange: 0x01_00_00_00 } as const;
const _Count = transformDecoder(_U16, (value) => (value === _RESOURCE.emptyCount ? 0 : value + 1));
const _FORMATS = ['.otf', '.ttf', '.ttc', '.otc', '.dfont', '.woff', '.woff2'];

const _source: (
    file: string,
) => Effect.Effect<{ readonly digest: string; readonly members: readonly Result.Result<Sfnt, typeof FontError.Type>[] }, typeof FontError.Type, FileSystem.FileSystem | Crypto.Crypto> =
    Effect.fnUntraced(function* (file: string) {
        const fs = yield* FileSystem.FileSystem;
        const crypto = yield* Crypto.Crypto;
        const original = yield* fs.readFile(file).pipe(Effect.mapError((cause) => FontError.cases.fontSourceUnreadable.make({ path: file, cause })));
        const digest = Encoding.encodeHex(yield* crypto.digest('SHA-256', original).pipe(Effect.mapError((cause) => FontError.cases.fontSourceUnreadable.make({ path: file, cause }))));
        const containers = yield* Effect.tryPromise({
            try: async () => {
                const format = create(Buffer.from(original)).type;
                const data = await Match.value(format).pipe(
                    Match.when('WOFF2', async () => Buffer.from((await fonteditor.woff2.init()).decode(original.slice().buffer))),
                    Match.when('WOFF', () => Buffer.from(fonteditor.woff2ttf(original.slice().buffer, { inflate: (compressed) => Array.fromIterable(unzlibSync(Uint8Array.from(compressed))) }))),
                    Match.orElse(() => Buffer.from(original)),
                );
                const opened = create(data);
                let pieces: readonly Result.Result<{ readonly data: Buffer; readonly offset: number }, unknown>[];
                if (opened.type === 'DFont') {
                    const header = getStructDecoder([
                        ['data', _U32],
                        ['map', _U32],
                        ['dataLength', _U32],
                        ['mapLength', _U32],
                    ]).decode(data);
                    const resources = fixDecoderSize(getBytesDecoder(), header.dataLength).decode(data, header.data);
                    const map = fixDecoderSize(getBytesDecoder(), header.mapLength).decode(data, header.map);
                    const typeOffset = getStructDecoder([
                        ['reserved', fixDecoderSize(getBytesDecoder(), _RESOURCE.mapReservedBytes)],
                        ['types', _U16],
                    ]).decode(map).types;
                    const types = getArrayDecoder(
                        getStructDecoder([
                            ['tag', fixDecoderSize(getUtf8Decoder(), _U32.fixedSize)],
                            ['count', _Count],
                            ['offset', _U16],
                        ]),
                        { size: _Count, requireSizePrefix: true },
                    ).decode(map, typeOffset);
                    const references = Array.flatMap(types, (resource) =>
                        resource.tag === 'sfnt'
                            ? getArrayDecoder(
                                  getStructDecoder([
                                      ['id', _U16],
                                      ['name', _U16],
                                      ['attributesOffset', _U32],
                                      ['handle', _U32],
                                  ]),
                                  { size: resource.count },
                              ).decode(map, typeOffset + resource.offset)
                            : [],
                    );
                    pieces = Array.map(references, ({ attributesOffset }) =>
                        Math.trunc(attributesOffset / _RESOURCE.offsetRange) % 2 === 0
                            ? Result.try(() => ({ data: Buffer.from(addDecoderSizePrefix(getBytesDecoder(), _U32).decode(resources, attributesOffset % _RESOURCE.offsetRange)), offset: 0 }))
                            : Result.fail('Compressed font resource is unsupported'),
                    );
                } else {
                    const { offsets } =
                        opened.type === 'TTC'
                            ? getStructDecoder([
                                  ['signature', _U32],
                                  ['version', _U32],
                                  ['offsets', getArrayDecoder(_U32, { size: _U32, requireSizePrefix: true })],
                              ]).decode(data)
                            : { offsets: [0] };
                    pieces = Array.map(offsets, (offset) => Result.succeed({ data, offset }));
                }
                return pieces;
            },
            catch: (cause) => FontError.cases.fontSourceUnreadable.make({ path: file, cause }),
        });
        return {
            digest,
            members: Array.map(containers, (piece, index) =>
                piece.pipe(
                    Result.flatMap((member) => Result.try(() => readSfntView(new BinaryView(member.data, 0, member.offset)))),
                    Result.mapError((cause) => FontError.cases.fontFaceUnreadable.make({ path: file, index, cause })),
                ),
            ),
        };
    });

// --- [CATALOGUE] -----------------------------------------------------------------------

const _WIDTH_CLASS: Readonly<Record<number, number>> = { 1: 50, 2: 62.5, 3: 75, 4: 87.5, 5: CSS.width, 6: 112.5, 7: 125, 8: 150, 9: 200 };

const inspect: (
    file: string,
) => Effect.Effect<{ readonly faces: readonly FontFace[]; readonly rejected: readonly (typeof FontError.Type)[] }, typeof FontError.Type, FileSystem.FileSystem | Crypto.Crypto> = Effect.fnUntraced(
    function* (file: string) {
        const { digest, members } = yield* _source(file);
        const crypto = yield* Crypto.Crypto;
        const [rejected, faces] = yield* Effect.partition(
            members,
            Effect.fnUntraced(function* (member: (typeof members)[number], index: number) {
                const sfnt = yield* Effect.fromResult(member);
                if (sfnt.tables.has('bhed')) {
                    return yield* Effect.fail(FontError.cases.fontFormatUnsupported.make({ path: file, index, format: 'bhed' }));
                }
                const namesData = yield* Effect.fromOption(Option.fromUndefinedOr(sfnt.tables.get(Name.Tag)), () =>
                    FontError.cases.fontFaceUnreadable.make({ path: file, index, cause: 'Name table is absent' }),
                );
                const nameTableDigest = Encoding.encodeHex(
                    yield* crypto.digest('SHA-256', namesData).pipe(Effect.mapError((cause) => FontError.cases.fontFaceUnreadable.make({ path: file, index, cause }))),
                );
                const { head, post, os2, fvar, names, features, axes, codePoints, unitsPerEm } = yield* Effect.try({
                    try: () => {
                        const data = writeSfntOtf(sfnt);
                        const face = new Face(new Blob(data));
                        const font = create(data);
                        if ('fonts' in font) {
                            throw new TypeError('A standalone sfnt decoded as a collection');
                        }
                        return {
                            ...readOtMetadata(
                                { ...sfnt, tables: new Map(Array.filter(Array.fromIterable(sfnt.tables), ([tag]) => Array.contains([Head.Tag, Maxp.Tag, Post.Tag, Os2.Tag, Fvar.Tag], tag))) },
                                { fontMetadata: {} },
                            ),
                            names: Array.map(face.listNames(), (entry) => ({ ...entry, value: face.getName(entry.nameId, entry.language) })).filter(({ value }) => value.length > 0),
                            features: font.availableFeatures,
                            axes: face.getAxisInfos(),
                            codePoints: Array.fromIterable(face.collectUnicodes()),
                            unitsPerEm: face.upem,
                        };
                    },
                    catch: (cause) => FontError.cases.fontFaceUnreadable.make({ path: file, index, cause }),
                });
                const preferred = HashMap.fromIterable(
                    Array.map(
                        Array.sortWith(names, ({ language }) => language === 'en', Boolean.Order),
                        ({ nameId, value }) => [nameId, value],
                    ),
                );
                const variationFormat = Option.map(Option.fromNullishOr(fvar), () => (sfnt.tables.has('STAT') ? ('OpenType' as const) : ('TrueTypeGX' as const)));
                const macStyle = Record.map(Struct.pick(Head.MacStyle, ['Bold', 'Italic', 'Condensed', 'Extended']), (flag) => Math.trunc(head.macStyle / flag) % 2 !== 0);
                const weight = Option.getOrElse(Option.map(Option.fromNullishOr(os2), Struct.get('usWeightClass')), () => (macStyle.Bold ? CSS.weight.bold : CSS.weight.normal));
                const width = Option.match(Option.fromNullishOr(os2), {
                    onSome: ({ usWidthClass }) => _WIDTH_CLASS[usWidthClass],
                    onNone: () =>
                        Match.value(macStyle).pipe(
                            Match.when(
                                ({ Condensed }) => Condensed,
                                () => _WIDTH_CLASS[3],
                            ),
                            Match.when(
                                ({ Extended }) => Extended,
                                () => _WIDTH_CLASS[7],
                            ),
                            Match.orElse(() => CSS.width),
                        ),
                });
                return yield* Schema.decodeUnknownEffect(FontFace, { errors: 'all' })({
                    file,
                    digest,
                    nameTableDigest,
                    index,
                    postScriptName: Option.getOrNull(HashMap.get(preferred, Name.NameID.PostscriptName)),
                    family: Option.getOrNull(Option.orElse(HashMap.get(preferred, Name.NameID.PreferredFamily), () => HashMap.get(preferred, Name.NameID.LegacyFamily))),
                    style: Option.getOrNull(Option.orElse(HashMap.get(preferred, Name.NameID.PreferredSubfamily), () => HashMap.get(preferred, Name.NameID.LegacySubfamily))),
                    fullName: Option.getOrNull(HashMap.get(preferred, Name.NameID.FullFontName)),
                    version: Option.getOrNull(HashMap.get(preferred, Name.NameID.VersionString)),
                    variationFormat: Option.getOrNull(variationFormat),
                    unitsPerEm,
                    weight: !Option.contains(variationFormat, 'TrueTypeGX') && Schema.is(FontWeight)(weight) ? weight : null,
                    width,
                    angle: post?.italicAngle,
                    italic: Option.match(Option.fromNullishOr(os2), { onNone: () => macStyle.Italic, onSome: ({ fsSelection }) => Math.trunc(fsSelection / Os2.FsSelection.ITALIC) % 2 !== 0 }),
                    isMonospace: post?.isFixedPitch,
                    bbox: { minX: head.xMin, minY: head.yMin, maxX: head.xMax, maxY: head.yMax },
                    aliases: Record.map({ family: [Name.NameID.LegacyFamily, Name.NameID.PreferredFamily], fullName: [Name.NameID.FullFontName, Name.NameID.CompatibleFull] }, (ids) =>
                        Array.dedupe(
                            Array.map(
                                Array.filter(names, ({ nameId }) => Array.contains(ids, nameId)),
                                Struct.get('value'),
                            ),
                        ),
                    ),
                    features,
                    namedVariations: Array.map(Array.flatMap(Option.toArray(Option.fromNullishOr(fvar)), Struct.get('instances')), (instance) => ({
                        name: Option.getOrNull(HashMap.get(preferred, instance.subfamilyNameID)),
                        postScriptName: Option.getOrNull(Option.flatMap(Option.fromUndefinedOr(instance.postScriptNameID), (nameId) => HashMap.get(preferred, nameId))),
                        coordinates: Predicate.isNotNullish(instance.coordinates) ? Record.fromEntries(Array.fromIterable(instance.coordinates).map(([axis, value]) => [axis.tag, value])) : null,
                    })),
                    axes: Record.map(axes, (axis) => ({ ...Struct.pick(axis, ['axisIndex', 'min', 'default', 'max']), name: Option.getOrElse(HashMap.get(preferred, axis.nameId), () => '') })),
                    codePoints,
                }).pipe(Effect.mapError((cause) => FontError.cases.fontFaceUnreadable.make({ path: file, index, cause })));
            }),
        );
        return { faces, rejected };
    },
);

const catalogue: (sources: readonly string[]) => Effect.Effect<
    {
        readonly faces: readonly FontFace[];
        readonly rejected: readonly (typeof FontError.Type | typeof MetricsError.Type)[];
    },
    Array.NonEmptyArray<typeof FontError.Type>,
    FileSystem.FileSystem | Crypto.Crypto | Path.Path
> = Effect.fnUntraced(function* (sources: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const files = yield* Effect.validate(
        sources,
        Effect.fnUntraced(function* (location: string) {
            const info = yield* fs.stat(location).pipe(Effect.mapError((cause) => FontError.cases.fontSourceUnreadable.make({ path: location, cause })));
            if (info.type === 'File') {
                return [location];
            }
            if (info.type !== 'Directory') {
                return yield* Effect.fail(FontError.cases.fontSourceUnreadable.make({ path: location, cause: info.type }));
            }
            const discovered = yield* Effect.tryPromise({
                try: (signal) => new fdir().withSymlinks().withErrors().withAbortSignal(signal).crawl(location).withPromise(),
                catch: (cause) => FontError.cases.fontSourceUnreadable.make({ path: location, cause }),
            });
            return Array.filter(discovered, (file) => {
                const extension = path.extname(file).toLowerCase();
                return extension === '' || Array.contains(_FORMATS, extension);
            });
        }),
    );
    const [rejected, inspected] = yield* pipe(files, Array.flatten, Array.dedupe, Effect.partition(inspect));
    return {
        faces: Array.dedupeWith(Array.flatMap(inspected, Struct.get('faces')), (left, right) => left.digest === right.digest && left.index === right.index),
        rejected: [...rejected, ...Array.flatMap(inspected, Struct.get('rejected'))],
    };
});

// --- [MATCHING] ------------------------------------------------------------------------

const select = (
    faces: readonly FontFace[],
    request: FontSelector,
): Result.Result<
    {
        readonly face: FontFace;
        readonly coordinates: Readonly<Record<string, number>>;
    },
    typeof FontError.Type
> => {
    const eligible = Array.filter(
        faces,
        (face) =>
            Array.every(request.codePoints, (point) => Array.contains(face.codePoints, point)) &&
            Record.isSubrecord(
                request.axes,
                Record.intersection(request.axes, face.axes, (value, axis) => Number.clamp(value, { minimum: axis.min, maximum: axis.max })),
            ),
    );
    if (!('family' in request)) {
        const matching = Array.flatMap(eligible, (face) => {
            const instances = Array.filter(face.namedVariations, (instance) =>
                'postScriptName' in request ? Option.exists(instance.postScriptName, (name) => String.toLowerCase(name) === String.toLowerCase(request.postScriptName)) : false,
            );
            const identified = Match.value(request).pipe(
                Match.when({ postScriptName: Predicate.isString }, ({ postScriptName }) => String.toLowerCase(face.postScriptName) === String.toLowerCase(postScriptName)),
                Match.when({ fullName: Predicate.isString }, ({ fullName }) => Array.contains(Array.map([face.fullName, ...face.aliases.fullName], String.toLowerCase), String.toLowerCase(fullName))),
                Match.orElse(({ digest, index }) => face.digest === digest && face.index === index),
            );
            const candidates = identified ? [{ face, coordinates: request.axes }] : [];
            return Array.isArrayNonEmpty(instances) ? Array.map(instances, (instance) => ({ face, coordinates: { ...instance.coordinates, ...request.axes } })) : candidates;
        });
        return Option.match(Array.head(matching), {
            onNone: () => Result.fail(FontError.cases.fontUnavailable.make({ request })),
            onSome: (selected) =>
                matching.length > 1 && Array.isArrayNonEmpty(matching)
                    ? Result.fail(FontError.cases.fontAmbiguous.make({ request, faces: Array.map(matching, Struct.get('face')) }))
                    : Result.succeed(selected),
        });
    }
    const family = Array.filter(eligible, (face) => Array.some([face.family, ...face.aliases.family], (name) => String.toLowerCase(name) === String.toLowerCase(request.family)));
    const normal = !request.italic && request.angle === 0;
    const target = request.italic ? CSS.oblique : Math.abs(request.angle);
    const direction = request.angle > 0 && !request.italic ? -1 : 1;
    const slopes = Array.flatMap(family, (face: FontFace): readonly { readonly face: FontFace; readonly tag: 'ital' | 'slnt'; readonly value: number }[] => {
        const italic = Option.getOrElse(Record.get(face.axes, 'ital'), () => ({ min: face.italic ? 1 : 0, max: face.italic ? 1 : 0 }));
        const angle = Record.has(face.axes, 'ital') ? 0 : face.angle;
        const slant = Option.getOrElse(Record.get(face.axes, 'slnt'), () => ({ min: angle, max: angle }));
        return [
            ...(face.italic || Record.has(face.axes, 'ital')
                ? [{ face, tag: 'ital' as const, value: Number.clamp(request.italic || request.angle !== 0 ? 1 : 0, { minimum: italic.min, maximum: italic.max }) }]
                : []),
            ...(Record.has(face.axes, 'slnt') || !face.italic || (italic.min <= 0 && italic.max >= 0)
                ? [{ face, tag: 'slnt' as const, value: Number.clamp(request.italic ? -CSS.oblique : request.angle, { minimum: slant.min, maximum: slant.max }) }]
                : []),
        ];
    });
    const ranked = Array.map(slopes, ({ face, tag, value }) => {
        const legacy = Option.contains(face.variationFormat, 'TrueTypeGX');
        const width = Option.match(Record.get(face.axes, 'wdth'), {
            onNone: () => Option.some(face.width),
            onSome: (axis) => (legacy ? Option.none() : Option.some(Number.clamp(request.width, { minimum: axis.min, maximum: axis.max }))),
        });
        const weight = Option.orElse(
            Option.flatMap(Record.get(face.axes, 'wght'), (axis) => (legacy ? Option.none() : Option.some(Number.clamp(request.weight, { minimum: axis.min, maximum: axis.max })))),
            () => face.weight,
        );
        const slope = (tag === 'slnt' ? -value : value) * direction;
        const slanted = tag === 'slnt';
        const preferredSlope = slanted ? target : 1;
        const preferredSide = target >= CSS.oblique ? slope >= preferredSlope : slope <= preferredSlope;
        const regions = Match.value({ normal, italic: request.italic }).pipe(
            Match.when({ normal: true }, () => [slanted && slope >= 0, !slanted && slope >= 0, slanted, !slanted]),
            Match.when({ italic: true }, () => [!slanted && slope >= 1, !slanted && slope > 0, slanted && slope >= target, slanted && slope > 0, !slanted, slanted]),
            Match.orElse(() => [slanted && slope > 0 && preferredSide, slanted && slope > 0, !slanted && slope > 0 && preferredSide, !slanted && slope > 0, slanted, !slanted]),
        );
        const styleRank = [Array.takeWhile(regions, Boolean.not).length, Math.abs(slope - (!normal && slope > 0 ? preferredSlope : 0))];
        const traits: Readonly<Record<string, number>> = {
            ...Option.match(width, { onNone: () => ({}), onSome: (wdth) => ({ wdth }) }),
            ...Option.match(weight, { onNone: () => ({}), onSome: (wght) => ({ wght }) }),
            ...(tag === 'slnt'
                ? {
                      ital: Option.match(Record.get(face.axes, 'ital'), { onNone: () => (face.italic ? 1 : 0), onSome: (axis) => Number.clamp(0, { minimum: axis.min, maximum: axis.max }) }),
                      slnt: value,
                  }
                : { ital: value }),
        };
        const coordinates = { ...Record.filter(traits, (_, axis) => Record.has(face.axes, axis)), ...request.axes };
        const widthRank = Option.match(width, {
            // An unknown scale can match the requested width, but cannot rank better than an exact match.
            onNone: () => [0, 0],
            onSome: (cssWidth) => {
                const preferred = request.width <= CSS.width ? cssWidth <= request.width : cssWidth >= request.width;
                return [preferred ? 0 : 1, Math.abs(cssWidth - request.width)];
            },
        });
        return { face, coordinates, weight, rank: [...widthRank, ...styleRank] };
    });
    const candidates = Array.sortWith(ranked, Struct.get<(typeof ranked)[number], 'rank'>('rank'), Order.Array(Number.Order));
    const closest = Array.head(candidates);
    if (Option.isNone(closest)) {
        return Result.fail(FontError.cases.fontUnavailable.make({ request }));
    }
    const nearest = Array.filter(candidates, (candidate) => Order.Array(Number.Order)(candidate.rank, closest.value.rank) === 0);
    const unsupported = Array.filter(nearest, ({ face }) => Option.contains(face.variationFormat, 'TrueTypeGX') && (Record.has(face.axes, 'wdth') || Record.has(face.axes, 'wght')));
    if (Array.isArrayNonEmpty(unsupported)) {
        return Result.fail(FontError.cases.fontAxisScaleUnsupported.make({ request, faces: Array.map(unsupported, Struct.get('face')) }));
    }
    const [unknown, known] = Array.partition(nearest, (candidate) =>
        Result.fromOption(
            Option.map(candidate.weight, (weight) => ({ ...candidate, weight })),
            () => candidate.face,
        ),
    );
    if (Array.isArrayNonEmpty(unknown)) {
        return Result.fail(FontError.cases.fontWeightUnknown.make({ request, faces: unknown }));
    }
    const weighted = Array.map(known, ({ face, coordinates, weight }) => {
        const ranges = Match.value(request.weight).pipe(
            Match.when(
                (requestedWeight) => requestedWeight < CSS.weight.normal,
                () => [weight <= request.weight, true],
            ),
            Match.when(
                (requestedWeight) => requestedWeight <= CSS.weight.medium,
                () => [weight >= request.weight && weight <= CSS.weight.medium, weight < request.weight, true],
            ),
            Match.orElse(() => [weight >= request.weight, true]),
        );
        return {
            face,
            coordinates,
            rank: [Array.takeWhile(ranges, Boolean.not).length, Math.abs(weight - request.weight)],
        };
    });
    const ordered = Array.sortWith(weighted, Struct.get<(typeof weighted)[number], 'rank'>('rank'), Order.Array(Number.Order));
    return Option.match(Array.head(ordered), {
        onNone: () => Result.fail(FontError.cases.fontUnavailable.make({ request })),
        onSome: (first) => {
            const peers = Array.filter(ordered, (candidate) => Order.Array(Number.Order)(candidate.rank, first.rank) === 0);
            return Array.isArrayNonEmpty(peers) && peers.length > 1
                ? Result.fail(FontError.cases.fontAmbiguous.make({ request, faces: Array.map(peers, Struct.get('face')) }))
                : Result.succeed(Struct.pick(first, ['face', 'coordinates']));
        },
    });
};

const bolder = (
    faces: readonly FontFace[],
    request: FontRequest,
): Result.Result<
    {
        readonly face: FontFace;
        readonly coordinates: Readonly<Record<string, number>>;
    },
    typeof FontError.Type
> =>
    Result.gen(function* () {
        const base = yield* select(faces, request);
        const weight = yield* Result.fromOption(
            Option.orElse(Record.get(base.coordinates, 'wght'), () => base.face.weight),
            () => FontError.cases.fontWeightUnknown.make({ request, faces: [base.face] }),
        );
        const target = Match.value(weight).pipe(
            Match.when(
                (value) => value < CSS.relative.normal,
                () => CSS.weight.normal,
            ),
            Match.when(
                (value) => value < CSS.relative.bold,
                () => CSS.weight.bold,
            ),
            Match.when(
                (value) => value < CSS.weight.heavy,
                () => CSS.weight.heavy,
            ),
            Match.orElse(() => weight),
        );
        const emphasis = { ...request, weight: target, axes: Record.remove(request.axes, 'wght') };
        const selected = yield* select(faces, emphasis);
        const selectedWeight = yield* Result.fromOption(
            Option.orElse(Record.get(selected.coordinates, 'wght'), () => selected.face.weight),
            () => FontError.cases.fontWeightUnknown.make({ request: emphasis, faces: [selected.face] }),
        );
        if (selectedWeight <= weight) {
            return yield* Result.fail(FontError.cases.fontUnavailable.make({ request: emphasis }));
        }
        return selected;
    });

// --- [MEASUREMENT] ---------------------------------------------------------------------

const _METRICS = {
    ascent: MetricsTag.HORIZONTAL_ASCENDER,
    descent: MetricsTag.HORIZONTAL_DESCENDER,
    lineGap: MetricsTag.HORIZONTAL_LINE_GAP,
    capHeight: MetricsTag.CAP_HEIGHT,
    xHeight: MetricsTag.X_HEIGHT,
    underlinePosition: MetricsTag.UNDERLINE_OFFSET,
    underlineThickness: MetricsTag.UNDERLINE_SIZE,
    strikeoutPosition: MetricsTag.STRIKEOUT_OFFSET,
    strikeoutThickness: MetricsTag.STRIKEOUT_SIZE,
    subscriptXSize: MetricsTag.SUBSCRIPT_EM_X_SIZE,
    subscriptYSize: MetricsTag.SUBSCRIPT_EM_Y_SIZE,
    subscriptXOffset: MetricsTag.SUBSCRIPT_EM_X_OFFSET,
    subscriptYOffset: MetricsTag.SUBSCRIPT_EM_Y_OFFSET,
    superscriptXSize: MetricsTag.SUPERSCRIPT_EM_X_SIZE,
    superscriptYSize: MetricsTag.SUPERSCRIPT_EM_Y_SIZE,
    superscriptXOffset: MetricsTag.SUPERSCRIPT_EM_X_OFFSET,
    superscriptYOffset: MetricsTag.SUPERSCRIPT_EM_Y_OFFSET,
} as const satisfies Readonly<Record<keyof Measured['metrics'], number>>;
const _HEIGHT_GLYPHS = { capHeight: 0x48, xHeight: 0x78 } as const;

const metrics: (
    face: FontFace,
    coordinates: Readonly<Record<string, number>>,
    size: Option.Option<number>,
    codePoints: readonly number[],
) => Effect.Effect<Measured, typeof FontError.Type | typeof MetricsError.Type, FileSystem.FileSystem | Crypto.Crypto> = Effect.fnUntraced(function* (
    face: FontFace,
    coordinates: Readonly<Record<string, number>>,
    size: Option.Option<number>,
    codePoints: readonly number[],
) {
    const invalidCodePoints = Array.filter(codePoints, Predicate.not(Schema.is(CodePoint)));
    const points = Option.getOrElse(size, () => 1);
    const invalid = Record.keys(
        Record.filter(coordinates, (value, tag) => !Option.exists(Record.get(face.axes, tag), (axis) => globalThis.Number.isFinite(value) && value >= axis.min && value <= axis.max)),
    );
    if (Array.isArrayNonEmpty(invalidCodePoints)) {
        return yield* Effect.fail(MetricsError.cases.invalidCodePoints.make({ codePoints: invalidCodePoints }));
    }
    if (!globalThis.Number.isFinite(points) || points <= 0) {
        return yield* Effect.fail(MetricsError.cases.invalidFontFit.make({ height: points }));
    }
    if (Array.isArrayNonEmpty(invalid)) {
        return yield* Effect.fail(MetricsError.cases.invalidFontCoordinates.make({ file: face.file, axes: invalid }));
    }
    const source = yield* _source(face.file);
    if (source.digest !== face.digest) {
        return yield* Effect.fail(MetricsError.cases.fontChanged.make({ file: face.file }));
    }
    const member = yield* Effect.fromOption(Array.get(source.members, face.index), () =>
        FontError.cases.fontFaceUnreadable.make({ path: face.file, index: face.index, cause: 'Collection member is absent' }),
    );
    const sfnt = yield* Effect.fromResult(member);
    const measured = yield* Effect.try({
        try: () => {
            const optical = Option.flatMap(size, (value) => Option.map(Record.get(face.axes, 'opsz'), (axis) => ({ opsz: Number.clamp(value, { minimum: axis.min, maximum: axis.max }) })));
            const effective: Measured['coordinates'] = { ...Record.map(face.axes, Struct.get('default')), ...Option.getOrElse(optical, () => ({})), ...coordinates };
            const opened = new Face(new Blob(writeSfntOtf(sfnt)));
            const font = new Font(opened);
            font.setScale(opened.upem, opened.upem);
            font.setVariations(Array.map(Record.toEntries(effective), ([tag, value]) => new Variation(tag, value)));
            const scale = points / opened.upem;
            const dimensions = Record.map(_METRICS, (tag) => Option.map(Option.fromUndefinedOr(font.getMetricPosition(tag)), (value) => value * scale));
            const glyphs = HashMap.fromIterable(
                Array.flatMap(Array.dedupe([...codePoints, ...Record.values(_HEIGHT_GLYPHS)]), (point) =>
                    Option.fromUndefinedOr(font.nominalGlyph(point)).pipe(
                        Option.flatMap((glyph) => Option.fromUndefinedOr(font.glyphExtents(glyph))),
                        Option.map(Record.map(Number.multiply(scale))),
                        Option.map((extents) => [point, extents] as const),
                        Option.toArray,
                    ),
                ),
            );
            const heights = Record.map(_HEIGHT_GLYPHS, (point, field) =>
                Option.firstSomeOf([
                    Option.map(
                        Option.filter(dimensions[field], (value) => value > 0),
                        (value) => ({ value, source: 'OS/2' as const }),
                    ),
                    Option.map(HashMap.get(glyphs, point), ({ yBearing }) => ({ value: yBearing, source: 'glyph' as const })),
                ]),
            );
            return {
                ...face,
                size: points,
                coordinates: effective,
                designAxes: Array.map(
                    Array.sortWith(Record.toEntries(face.axes), ([, axis]) => axis.axisIndex, Number.Order),
                    ([tag]) => effective[tag],
                ),
                metrics: { ...dimensions, ...Record.map(heights, Option.map(Struct.get('value'))) },
                heightSources: Record.map(heights, Option.map(Struct.get('source'))),
                glyphs: HashMap.filter(glyphs, (_, point) => Array.contains(codePoints, point)),
            };
        },
        catch: (cause) => FontError.cases.fontFaceUnreadable.make({ path: face.file, index: face.index, cause }),
    });
    const missing = Array.filter(codePoints, (point) => !HashMap.has(measured.glyphs, point));
    if (Array.isArrayNonEmpty(missing)) {
        return yield* Effect.fail(MetricsError.cases.metricsMissing.make({ postScriptName: face.postScriptName, fields: Array.map(missing, globalThis.String) }));
    }
    return yield* Schema.decodeUnknownEffect(Schema.toType(Measured), { errors: 'all' })(measured).pipe(
        Effect.mapError((cause) => FontError.cases.fontFaceUnreadable.make({ path: face.file, index: face.index, cause })),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { bolder, catalogue, FontError, FontRequest, FontSelector, inspect, metrics, select };
