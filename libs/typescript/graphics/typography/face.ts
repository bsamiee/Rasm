import { Buffer } from 'node:buffer';
import { Unmatched } from '@rasm/search/unmatched.ts';
import { Array, Cause, Effect, Equal, flow, Match, Number, Option, Order, Predicate, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, Font, Face as HarfBuzzFace, Variation } from 'harfbuzzjs';
import { Color } from './color.ts';
import { coordinate, GeneratedName, type Instance, InstanceRecords, instanceName } from './instance.ts';
import { Layout } from './layout.ts';
import { Metrics } from './metrics.ts';
import type { Source } from './source.ts';
import { Tables } from './tables.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Outlines = Schema.Literals(['glyf', 'CFF ', 'CFF2']);
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')(
    {
        ...Record.map({ postscriptName: 'PostScript name', fullName: 'Full name', style: 'Subfamily name' }, (description) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description }))),
        ...Record.map({ family: 'Typographic family (name ID 16), else the family (name ID 1)', version: 'Version string (name ID 5)' }, (description) =>
            Schema.OptionFromOptionalKey(Schema.String.annotate({ description: `${description} in English, else in the first language the name table states it; absent when it states none` })),
        ),
        unitsPerEm: Schema.Int.annotate({ description: 'Font units per em; every length the face states is in em' }),
        glyphCount: Schema.Int.annotate({ description: 'Glyphs in the face, .notdef included' }),
        coverage: Schema.Int.annotate({ description: 'Count of Unicode code points the face maps to glyphs' }),
        outlines: Schema.OptionFromOptionalKey(Outlines.annotate({ description: 'Table holding the glyph outlines: TrueType, CFF, or CFF2; absent for a face of bitmaps alone' })),
        embedding: Schema.OptionFromOptionalKey(
            Schema.Struct({ permission: Schema.Literals(['installable', 'editable', 'preview', 'restricted']), subsetting: Schema.Boolean, outlines: Schema.Boolean }).annotate({
                description: 'OS/2 fsType: the usage permission, the least restrictive one when several bits are set, and whether subsets and outline embedding are allowed; absent when the face has no OS/2 table',
            }),
        ),
        ...Record.map({ weight: 'OS/2 weight class at the default instance', width: 'OS/2 width class at the default instance' }, (description) => Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: `${description}; absent when the face has no OS/2 table` }))),
        monospace: Schema.Boolean.annotate({ description: 'The face declares fixed-width spacing' }),
        metrics: Metrics,
        layout: Layout,
        color: Color,
        axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite), hidden: Schema.Boolean })).annotate({
            description: 'Variation axes in fvar order with the value this instance applies; hidden axes are not meant for user interfaces',
        }),
        instances: Schema.Array(
            Schema.Struct({
                ...Record.map({ style: 'Subfamily name of the instance', postscriptName: 'PostScript name of the instance: the one fvar names, else the one Adobe Technical Note 5902 generates from the variations prefix and the subfamily' }, (description) =>
                    Schema.OptionFromOptionalKey(Schema.String.annotate({ description })),
                ),
                variations: Schema.Record(Schema.String, Schema.Finite).annotate({ description: 'Axis values of the instance by axis tag' }),
            }),
        ).annotate({ description: 'Named instances; a selection takes their PostScript names' }),
        font: Schema.instanceOf(Font).pipe(Schema.encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(() => 'A face opens from font bytes, not from its encoded form'), encode: SchemaGetter.omit() })),
    },
    { description: 'One face at one instance; postscriptName, fullName, and style name the default or named instance at its coordinates, else postscriptName is the Adobe Technical Note 5902 name of the axis values and the other two are absent' },
) {
    static readonly open = Effect.fn('Face.open')(function* ({ data, postscriptName: selection }: typeof Source.Type, { variations, size }: typeof Instance.Type) {
        const container = yield* Effect.try(() => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength))).pipe(
            Effect.filterOrFail(
                ({ type }) => type !== 'WOFF' && type !== 'WOFF2',
                ({ type }) => new Cause.IllegalArgumentError(`${type} webfont containers must be decoded to OpenType or TrueType font bytes`),
            ),
        );
        const nameIds = { family: 1, subfamily: 2, fullName: 4, version: 5, postscriptName: 6, typographicFamily: 16, typographicSubfamily: 17, variationsPrefix: 25 } as const;
        const blob = new Blob(data);
        const fonts = 'fonts' in container ? container.fonts : [container];
        const strip = (name: string) => name.replaceAll(/[^A-Za-z0-9]/gu, '');
        const hyphenated = (name: string) => name.replaceAll(' ', '-');
        const faces = Array.zip(fonts, yield* Schema.decodeUnknownEffect(InstanceRecords)(fonts, { errors: 'all' })).map(([parsed, { fvar }], index) => {
            const handle = new HarfBuzzFace(blob, index);
            const records = pipe(
                handle.listNames(),
                Array.sortWith(({ language }) => language !== 'en', Order.Boolean),
                Array.groupBy(({ nameId }) => `${nameId}`),
                Record.map(([{ nameId, language }]) => handle.getName(nameId, language)),
                Record.filter(Predicate.isTruthy),
            );
            const named = (id: number | undefined) => Option.flatMap(Option.fromUndefinedOr(id), (each) => Record.get(records, `${each}`));
            const names = {
                ...Record.map(Struct.pick(nameIds, ['postscriptName', 'fullName', 'version']), named),
                family: Option.orElse(named(nameIds.typographicFamily), () => named(nameIds.family)),
                style: Option.orElse(named(nameIds.typographicSubfamily), () => named(nameIds.subfamily)),
            };
            const prefix = Option.orElse(named(nameIds.variationsPrefix), () => Option.map(names.family, strip));
            const infos = Object.values(handle.getAxisInfos());
            const defaults = Record.fromIterableWith(infos, ({ tag, default: fallback }) => [tag, fallback]);
            return {
                parsed,
                handle,
                named,
                names,
                infos,
                prefix,
                base: { names, keys: Option.toArray(names.postscriptName), variations: defaults, coordinates: Record.map(defaults, coordinate) },
                instances: fvar.instance.map(({ nameID, coord, postscriptNameID }) => {
                    const style = named(nameID);
                    const postscriptName = Option.firstSomeOf([Option.flatMap(postscriptNameID, named), Option.map(Option.all([prefix, Option.map(style, strip)]), Array.join('-'))]);
                    const values = Record.fromEntries(Array.zip(infos.map(Struct.get('tag')), coord));
                    return {
                        names: { postscriptName, fullName: Option.map(Option.all([names.family, style]), Array.join(' ')), style },
                        keys: Array.getSomes([postscriptName, Option.map(Option.all([names.postscriptName, Option.map(style, hyphenated)]), Array.join('_'))]),
                        variations: values,
                        coordinates: Record.map(values, coordinate),
                    };
                }),
            };
        });
        const rows = faces.flatMap((face) => [face.base, ...face.instances].map((row) => ({ face, ...row })));
        const listed = Option.flatMap(selection, (wanted) => Array.findFirst(rows, ({ keys }) => Array.contains(keys, wanted)));
        const generated = Effect.gen(function* () {
            const [prefix, , settings] = yield* Effect.flatMap(Effect.fromOption(selection), Schema.decodeUnknownEffect(GeneratedName));
            const decoded = Record.fromEntries(settings);
            const face = yield* Effect.fromOption(Array.findFirst(faces, (each) => Option.contains(each.prefix, prefix) && Array.isReadonlyArrayEmpty(Array.difference(Record.keys(decoded), Record.keys(each.base.variations)))));
            return { face, variations: { ...face.base.variations, ...decoded } };
        });
        const { face: chosen, variations: given } = yield* Option.match(selection, {
            onNone: () =>
                Effect.map(
                    Unmatched.single('face in the collection', Option.none())(faces, ({ names }) => Option.toArray(names.postscriptName)),
                    (face) => ({ face, variations: face.base.variations }),
                ),
            onSome: () => Effect.mapError(Option.match(listed, { onNone: () => generated, onSome: Effect.succeed }), () => Unmatched.make({ subject: 'face or instance by PostScript name', query: selection, candidates: Array.getSomes(rows.map(flow(Struct.get('names'), Struct.get('postscriptName')))) })),
        });
        const tables = yield* Schema.decodeUnknownEffect(Tables)(chosen.parsed);
        const requested = { ...given, ...Record.fromIterableWith([...variations, ...Option.toArray(Option.map(size, (points) => new Variation('opsz', points)))], ({ tag, value }) => [tag, value]) };
        const axes = chosen.infos.map(({ tag, nameId, flags, min, default: fallback, max }) => ({
            tag,
            name: chosen.named(nameId),
            min,
            default: fallback,
            max,
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(Record.get(requested, tag), () => fallback)),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const coordinates = Record.fromIterableWith(axes, ({ tag, value }) => [tag, coordinate(value)]);
        const encoded = Option.flatMap(chosen.prefix, (prefix) =>
            instanceName(
                prefix,
                Record.filter(coordinates, (value, tag) => value !== chosen.base.coordinates[tag]),
            ),
        );
        const identity = pipe(
            Array.findFirst([...Option.toArray(listed), chosen.base, ...chosen.instances], (row) => Equal.equals(row.coordinates, coordinates)),
            Option.map(Struct.get('names')),
            Option.getOrElse(() => ({ postscriptName: encoded, fullName: Option.none<string>(), style: Option.none<string>() })),
        );
        const font = new Font(chosen.handle);
        font.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        return Face.make({
            ...chosen.names,
            ...identity,
            unitsPerEm: chosen.handle.upem,
            glyphCount: chosen.parsed.numGlyphs,
            coverage: chosen.handle.collectUnicodes().length,
            outlines: Array.findFirst(Outlines.literals, (tag) => Record.has(tables.directory.tables, tag)),
            embedding: Option.map(tables['OS/2'], ({ fsType: { noEmbedding, viewOnly, editable, noSubsetting, bitmapOnly } }) => ({
                permission: Match.value({ editable, viewOnly, noEmbedding }).pipe(
                    Match.when({ editable: true }, () => 'editable' as const),
                    Match.when({ viewOnly: true }, () => 'preview' as const),
                    Match.when({ noEmbedding: true }, () => 'restricted' as const),
                    Match.orElse(() => 'installable' as const),
                ),
                subsetting: !noSubsetting,
                outlines: !bitmapOnly,
            })),
            weight: Option.map(tables['OS/2'], Struct.get('usWeightClass')),
            width: Option.map(tables['OS/2'], Struct.get('usWidthClass')),
            monospace: tables.post.isFixedPitch !== 0,
            metrics: Metrics.of({ font, tables, axes }),
            layout: Layout.of(chosen),
            color: Color.of(chosen),
            axes,
            instances: chosen.instances.map(({ names: { style, postscriptName }, variations: values }) => ({ style, postscriptName, variations: values })),
            font,
        });
    });

    outline(glyph: number) {
        return this.font.glyphToJson(glyph).map(({ type, values }) => ({ type, values: values.map((value) => value / this.unitsPerEm) }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face };
