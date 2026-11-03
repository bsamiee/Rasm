import { Buffer } from 'node:buffer';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Effect, Equal, Function, flow, Match, Number, Option, Order, Predicate, Record, Schema, SchemaGetter, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, Font, Face as HarfBuzzFace, Variation } from 'harfbuzzjs';
import { Color } from '#/color';
import { Layout } from '#/layout';
import { Metrics } from '#/metrics';
import { Names } from '#/names';
import type { Source } from '#/source';
import { Tables } from '#/tables';
import { Tag } from '#/tag';
import { TypographyError } from '#/typography-error';

// --- [MODELS] --------------------------------------------------------------------------

const Outlines = Schema.Literals(['glyf', 'CFF ', 'CFF2']);
const MemberFvars = Schema.Array(Tables.mapFields(Struct.pick(['fvar'])));
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')(
    {
        ...Record.map(Struct.omit(Names.ids, ['prefix']), (ids) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description: `Name ID ${Object.values(ids).join(' else ')}, preferring English` }))),
        unitsPerEm: Schema.Int.annotate({ description: 'Font units per em, other lengths in em' }),
        glyphCount: Schema.Int.annotate({ description: 'Includes .notdef' }),
        coverage: Schema.Int.annotate({ description: 'Unicode code points mapped to glyphs' }),
        outlines: Schema.OptionFromOptionalKey(Outlines.annotate({ description: 'Absent for bitmap-only face' })),
        embedding: Schema.OptionFromOptionalKey(
            Schema.Struct({ permission: Schema.Literals(['installable', 'editable', 'preview', 'restricted']), subsetting: Schema.Boolean, outlines: Schema.Boolean }).annotate({
                description: 'OS/2 fsType, least restrictive permission among set bits',
            }),
        ),
        ...Struct.Record(['weight', 'width'], Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'OS/2 class at default instance' }))),
        monospace: Schema.Boolean.annotate({ description: 'post table isFixedPitch' }),
        metrics: Metrics,
        layout: Layout,
        color: Color,
        axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite), hidden: Schema.Boolean })).annotate({
            description: 'fvar order with applied value',
        }),
        namedInstances: Schema.Array(
            Schema.Struct({
                ...Record.map({ style: 'fvar subfamilyNameID name', postscriptName: 'fvar postScriptNameID name else Adobe Technical Note 5902 name' }, (description) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description }))),
                variations: Schema.Record(Schema.String, Schema.Finite).annotate({ description: 'Axis values by tag' }),
            }),
        ).annotate({ description: 'fvar named instances, selectable by PostScript name' }),
        extenders: Schema.Array(Schema.Int).annotate({ description: 'JSTF arab extender glyphs' }),
        font: Schema.instanceOf(Font).pipe(Schema.encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(() => 'Encoded face omits font data'), encode: SchemaGetter.omit() })),
    },
    { description: 'Face at one instance, names from named instance matching coordinates, else Adobe Technical Note 5902 postscriptName' },
) {
    static readonly Instance = Schema.Struct({
        variations: Schema.Record(Tag, Schema.Finite)
            .annotate({ description: 'Axis values by tag over selected instance, clamped to axis range, absent axes ignored' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.succeed({}))),
        size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size for opsz axis, overrides opsz variation' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    });
    static readonly open = Effect.fn('Face.open')(function* ({ data, postscriptName: selection }: typeof Source.Type, { variations, size }: typeof Face.Instance.Type = Face.Instance.make({})) {
        const container = yield* Effect.try({ try: () => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength)), catch: (cause) => TypographyError.make({ subject: Option.getOrElse(selection, () => 'default face'), cause }) });
        const unsupported = ({ type }: { readonly type: string }) => Effect.fail(new Cause.IllegalArgumentError(`${type} unsupported`));
        const fonts = yield* Match.value(container).pipe(
            Match.whenOr({ type: 'WOFF' }, { type: 'WOFF2' }, unsupported),
            Match.when({ type: 'TTF' }, (single) => Effect.succeed([single])),
            Match.orElse(({ fonts: members }) => Effect.succeed(members)),
        );
        const blob = new Blob(data);
        const fvars = yield* Schema.decodeUnknownEffect(MemberFvars)(fonts, { errors: 'all' });
        const faces = Array.zip(fonts, fvars).map(([sfnt, { fvar }], index) => {
            const handle = new HarfBuzzFace(blob, index);
            const names = Names.of(handle);
            const axisInfos = Object.values(handle.getAxisInfos());
            const postscriptName = names.find('postscriptName');
            return {
                index,
                sfnt,
                handle,
                names,
                axisInfos,
                base: { names: { postscriptName, fullName: names.find('fullName'), style: names.find('style') }, keys: Option.toArray(postscriptName), variations: Record.fromIterableWith(axisInfos, ({ tag, default: fallback }) => [tag, fallback]) },
                namedInstances: fvar.instance.map((record) => ({ ...names.namedInstance(record), variations: Record.fromEntries(Array.zip(axisInfos.map(Struct.get('tag')), record.coord)) })),
            };
        });
        const rows = faces.flatMap((face) => [face.base, ...face.namedInstances].map((row) => ({ face, ...row })));
        const namedRow = Option.flatMap(selection, (wanted) => Array.findFirst(rows, ({ keys }) => Array.containsWith(Unmatched.named)(keys, wanted)));
        const generated = Effect.fnUntraced(function* (wanted: string) {
            const { prefix, variations: decoded } = yield* Schema.decodeUnknownEffect(Names.Generated)(wanted);
            const face = yield* Effect.fromOption(Array.findFirst(faces, (each) => Option.contains(each.names.prefix, prefix) && Array.isReadonlyArrayEmpty(Array.difference(Record.keys(decoded), Record.keys(each.base.variations)))));
            return { face, variations: { ...face.base.variations, ...decoded } };
        });
        const { face: chosen, variations: selectedVariations } = yield* Option.match(selection, {
            onNone: () =>
                Effect.map(
                    Unmatched.single(faces, 'face in collection', Option.none(), ({ base, index }) => [Option.getOrElse(base.names.postscriptName, () => `face ${index}`)]),
                    (face) => ({ face, variations: face.base.variations }),
                ),
            onSome: (wanted) =>
                Effect.fromOption(namedRow).pipe(
                    Effect.catch(() => generated(wanted)),
                    Effect.mapError(() => Unmatched.make({ subject: 'face or instance', query: selection, candidates: Array.getSomes(rows.map(flow(Struct.get('names'), Struct.get('postscriptName')))) })),
                ),
        });
        const tables = yield* Schema.decodeUnknownEffect(Tables)(chosen.sfnt, { errors: 'all' });
        const requested = { ...selectedVariations, ...variations, ...Record.getSomes({ opsz: size }) };
        const axes = chosen.axisInfos.map(({ tag, nameId, flags, min, default: fallback, max }) => ({
            tag,
            name: chosen.names.findById(nameId),
            min,
            default: fallback,
            max,
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(Record.get(requested, tag), () => fallback)),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const coordinates = Record.fromIterableWith(axes, ({ tag, value }) => [tag, Names.spelled(value)]);
        const changed = Record.fromIterableWith(
            axes.filter(({ tag }) => Option.exists(Record.get(chosen.base.variations, tag), (fallback) => Names.spelled(fallback) !== coordinates[tag])),
            ({ tag, value }) => [tag, value],
        );
        const identity = yield* Option.match(
            Array.findFirst([...Option.toArray(namedRow), chosen.base, ...chosen.namedInstances], (row) => Equal.equals(Record.map(row.variations, Names.spelled), coordinates)),
            {
                onNone: () => Effect.map(chosen.names.generated(changed), (postscriptName) => ({ postscriptName, fullName: Option.none<string>(), style: Option.none<string>() })),
                onSome: ({ names }) => Effect.succeed(names),
            },
        );
        const font = new Font(chosen.handle);
        font.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        return Face.make({
            ...identity,
            family: chosen.names.find('family'),
            version: chosen.names.find('version'),
            unitsPerEm: chosen.handle.upem,
            glyphCount: chosen.sfnt.numGlyphs,
            coverage: chosen.handle.collectUnicodes().length,
            outlines: Array.findFirst(Outlines.literals, (tag) => Predicate.isNotUndefined(chosen.handle.referenceTable(tag))),
            embedding: Option.map(tables['OS/2'], ({ fsType: { noEmbedding, viewOnly, editable, noSubsetting, bitmapOnly } }) => ({
                permission: Option.match(Record.findFirst({ editable, preview: viewOnly, restricted: noEmbedding }, Function.identity), { onNone: () => 'installable' as const, onSome: ([permission]) => permission }),
                subsetting: !noSubsetting,
                outlines: !bitmapOnly,
            })),
            ...Record.map({ weight: 'usWeightClass', width: 'usWidthClass' } as const, (field) => Option.map(tables['OS/2'], Struct.get(field))),
            monospace: tables.post.isFixedPitch,
            metrics: Metrics.of({ font, sfnt: chosen.sfnt, tables, axes }),
            layout: Layout.of(chosen),
            color: Color.of(chosen),
            axes,
            namedInstances: chosen.namedInstances.map(({ names: { style, postscriptName }, variations: values }) => ({ style, postscriptName, variations: values })),
            extenders: tables.JSTF.scriptList.flatMap(({ tag, script }) => (tag === 'arab' ? Option.getOrElse(script.extenderGlyphs, Array.empty) : [])),
            font,
        });
    });

    has(codePoint: number) {
        return this.font.nominalGlyph(codePoint) !== undefined;
    }
    get nuqta() {
        const beh = 0x6_28;
        return Option.flatMap(Option.fromUndefinedOr(this.font.nominalGlyph(beh)), (glyph) => {
            const [, ends] = Array.mapAccum(this.font.glyphToJson(glyph), 0, (contour, { type, values }) => {
                const current = type === 'M' ? contour + 1 : contour;
                return [current, Option.map(Option.fromUndefinedOr(values.at(-2)), (x) => ({ contour: current, x }))] as const;
            });
            const extents = Object.values(Array.groupBy(Array.getSomes(ends), ({ contour }) => `${contour}`)).map((points) => {
                const xs = points.map(Struct.get('x'));
                return Math.max(...xs) - Math.min(...xs);
            });
            return Option.map(Option.liftPredicate(extents.filter(Number.isGreaterThan(0)), Array.isArrayNonEmpty), (positive) => Array.min(positive, Order.Number) / this.unitsPerEm);
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face };
