import { Buffer } from 'node:buffer';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Effect, Equal, Function, flow, Match, Number, Option, Order, Predicate, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, Font, Face as HarfBuzzFace, Variation } from 'harfbuzzjs';
import { Color } from '#/color';
import { Layout } from '#/layout';
import { Metrics } from '#/metrics';
import { Names } from '#/names';
import type { Source } from '#/source';
import { Tables } from '#/tables';
import { TagSettings } from '#/tags';

// --- [MODELS] --------------------------------------------------------------------------

const Outlines = Schema.Literals(['glyf', 'CFF ', 'CFF2']);
const Collection = Schema.Array(Tables);
const OmittedOnEncode = <S extends Schema.Top>(schema: S) => schema.pipe(Schema.encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(() => 'Encoded face omits font data'), encode: SchemaGetter.omit() }));
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')(
    {
        ...Struct.Record(['postscriptName', 'fullName'], Schema.OptionFromOptionalKey(Schema.String)),
        style: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Subfamily name' })),
        ...Record.map({ family: Names.ids.family, version: Names.ids.version }, (ids) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description: `Name ID ${Object.values(ids).join(' else ')}, preferring English` }))),
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
        instances: Schema.Array(
            Schema.Struct({
                ...Record.map({ style: 'Subfamily name', postscriptName: 'fvar PostScript name else Adobe Technical Note 5902 name' }, (description) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description }))),
                variations: Schema.Record(Schema.String, Schema.Finite).annotate({ description: 'Axis values by tag' }),
            }),
        ).annotate({ description: 'Selectable by PostScript name' }),
        font: OmittedOnEncode(Schema.instanceOf(Font)),
        extenders: OmittedOnEncode(Schema.Array(Schema.Int).annotate({ description: 'JSTF arab extender glyphs' })),
    },
    { description: 'Face at one instance, names from instance matching coordinates, else Adobe Technical Note 5902 postscriptName' },
) {
    static readonly Instance = Schema.Struct({
        variations: TagSettings(Variation, 'HarfBuzz tag=value over selected instance, clamped to axis range, absent axes ignored'),
        size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size for opsz axis, overrides opsz variation' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    });
    static readonly open = Effect.fn('Face.open')(function* ({ data, postscriptName: selection }: typeof Source.Type, { variations, size }: typeof Face.Instance.Type = Face.Instance.make({})) {
        const container = yield* Effect.try({ try: () => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength)), catch: (cause) => TypographyError.make({ subject: `${data.byteLength} font bytes`, cause }) });
        const unsupported = ({ type }: { readonly type: string }) => Effect.fail(new Cause.IllegalArgumentError(`${type} unsupported`));
        const fonts = yield* Match.value(container).pipe(
            Match.whenOr({ type: 'WOFF' }, { type: 'WOFF2' }, unsupported),
            Match.when({ type: 'TTF' }, (single) => Effect.succeed([single])),
            Match.orElse(({ fonts: members }) => Effect.succeed(members)),
        );
        const blob = new Blob(data);
        const records = yield* Schema.decodeUnknownEffect(Collection)(fonts, { errors: 'all' });
        const faces = Array.zip(fonts, records).map(([parsed, decoded], index) => {
            const handle = new HarfBuzzFace(blob, index);
            const names = Names.of(handle);
            const axisInfos = Object.values(handle.getAxisInfos());
            const postscriptName = names.named('postscriptName');
            return {
                index,
                parsed,
                tables: decoded,
                handle,
                names,
                axisInfos,
                base: { names: { postscriptName, fullName: names.named('fullName'), style: names.named('style') }, keys: Option.toArray(postscriptName), variations: Record.fromIterableWith(axisInfos, ({ tag, default: fallback }) => [tag, fallback]) },
                instances: decoded.fvar.instance.map((record) => ({ ...names.instance(record), variations: Record.fromEntries(Array.zip(axisInfos.map(Struct.get('tag')), record.coord)) })),
            };
        });
        const rows = faces.flatMap((face) => [face.base, ...face.instances].map((row) => ({ face, ...row })));
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
            onSome: (wanted) => Effect.mapError(Option.match(namedRow, { onNone: () => generated(wanted), onSome: Effect.succeed }), () => Unmatched.make({ subject: 'face or instance', query: selection, candidates: Array.getSomes(rows.map(flow(Struct.get('names'), Struct.get('postscriptName')))) })),
        });
        const { tables } = chosen;
        const requested = { ...selectedVariations, ...Record.fromIterableWith([...variations, ...Option.toArray(Option.map(size, (points) => new Variation('opsz', points)))], ({ tag, value }) => [tag, value]) };
        const axes = chosen.axisInfos.map(({ tag, nameId, flags, min, default: fallback, max }) => ({
            tag,
            name: chosen.names.byId(nameId),
            min,
            default: fallback,
            max,
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(Record.get(requested, tag), () => fallback)),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const coordinates = Record.fromIterableWith(axes, ({ tag, value }) => [tag, Names.coordinate(value)]);
        const generatedName = yield* chosen.names.generated(Record.filter(coordinates, (value, tag) => Option.exists(Record.get(chosen.base.variations, tag), (fallback) => Names.coordinate(fallback) !== value)));
        const identity = pipe(
            Array.findFirst([...Option.toArray(namedRow), chosen.base, ...chosen.instances], (row) => Equal.equals(Record.map(row.variations, Names.coordinate), coordinates)),
            Option.map(Struct.get('names')),
            Option.getOrElse(() => ({ postscriptName: generatedName, fullName: Option.none<string>(), style: Option.none<string>() })),
        );
        const font = new Font(chosen.handle);
        font.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        return Face.make({
            ...identity,
            family: chosen.names.named('family'),
            version: chosen.names.named('version'),
            unitsPerEm: chosen.handle.upem,
            glyphCount: chosen.parsed.numGlyphs,
            coverage: chosen.handle.collectUnicodes().length,
            outlines: Array.findFirst(Outlines.literals, (tag) => Predicate.isNotUndefined(chosen.handle.referenceTable(tag))),
            embedding: Option.map(tables['OS/2'], ({ fsType: { noEmbedding, viewOnly, editable, noSubsetting, bitmapOnly } }) => ({
                permission: Option.match(Record.findFirst({ editable, preview: viewOnly, restricted: noEmbedding }, Function.identity), { onNone: () => 'installable' as const, onSome: ([permission]) => permission }),
                subsetting: !noSubsetting,
                outlines: !bitmapOnly,
            })),
            ...Record.map({ weight: 'usWeightClass', width: 'usWidthClass' } as const, (field) => Option.map(tables['OS/2'], Struct.get(field))),
            monospace: tables.post.isFixedPitch,
            metrics: Metrics.of({ font, parsed: chosen.parsed, tables, axes }),
            layout: Layout.of(chosen),
            color: Color.of(chosen),
            axes,
            instances: chosen.instances.map(({ names: { style, postscriptName }, variations: values }) => ({ style, postscriptName, variations: values })),
            font,
            extenders: tables.JSTF.pipe(
                Option.flatMap(({ scriptList }) => Array.findFirst(scriptList, ({ tag }) => tag === 'arab')),
                Option.flatMap(({ script }) => script.extenderGlyphs),
                Option.getOrElse(() => []),
            ),
        });
    });

    has(codePoint: number) {
        return this.font.nominalGlyph(codePoint) !== undefined;
    }
    get nuqta() {
        const beh = 0x6_28;
        return Option.flatMap(Option.fromUndefinedOr(this.font.nominalGlyph(beh)), (glyph) => {
            const [, contours] = Array.mapAccum(this.font.glyphToJson(glyph), 0, (contour, { type, values }) => [type === 'M' ? contour + 1 : contour, { contour, xs: Array.chunksOf(values, 2).map(([x = 0]) => x) }]);
            const extents = Object.values(Array.groupBy(contours, ({ contour }) => `${contour}`)).map((commands) => {
                const xs = commands.flatMap(Struct.get('xs'));
                return Math.max(...xs) - Math.min(...xs);
            });
            return Option.map(Option.liftPredicate(extents.filter(Number.isGreaterThan(0)), Array.isArrayNonEmpty), (positive) => Array.min(positive, Order.Number) / this.unitsPerEm);
        });
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class TypographyError extends Schema.TaggedError<TypographyError, { readonly brand: unique symbol }>()('TypographyError', { subject: Schema.String, cause: Schema.Defect() }) {
    override get message() {
        return `Unreadable ${this.subject}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face, TypographyError };
