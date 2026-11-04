import { Buffer } from 'node:buffer';
import { Unmatched } from '@rasm/search';
import { Array, Effect, Equal, Function, flow, Match, Number, Option, Order, Predicate, Record, Schema, SchemaGetter, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, Font, Variation } from 'harfbuzzjs';
import { Tag } from '#/internal/tag';
import { Color } from '#/shaping/internal/color';
import { Layout } from '#/shaping/internal/layout';
import { Member } from '#/shaping/internal/member';
import { Metrics } from '#/shaping/internal/metrics';
import { Names } from '#/shaping/internal/names';
import { Profile } from '#/shaping/internal/profile';
import { Tables } from '#/shaping/internal/tables';
import type { Source } from '#/source';
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
    static readonly TATWEEL = 0x6_40;
    static readonly Instance = Schema.Struct({
        variations: Schema.Record(Tag, Schema.Finite)
            .annotate({ description: 'Axis values by tag over selected instance, clamped to axis range, absent axes ignored' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.succeed({}))),
        size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size for opsz axis, overrides opsz variation' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    });
    static readonly open = Effect.fn('Face.open')(function* ({ data, postscriptName: selection }: typeof Source.Type, { variations, size }: typeof Face.Instance.Type = Face.Instance.make({})) {
        const container = yield* Effect.try({ try: () => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength)), catch: TypographyError.unreadable(Option.getOrElse(selection, () => 'default face')) });
        const fonts = yield* Match.value(container).pipe(
            Match.whenOr({ type: 'WOFF' }, { type: 'WOFF2' }, ({ type }) => Effect.fail(TypographyError.unsupported(`${type} container`))),
            Match.when({ type: 'TTF' }, (single) => Effect.succeed([single])),
            Match.orElse((collection) => Effect.succeed(collection.fonts)),
        );
        const blob = new Blob(data);
        const fvars = yield* Schema.decodeUnknownEffect(MemberFvars)(fonts, { errors: 'all' });
        const members = Array.zip(fonts, fvars).map(([sfnt, { fvar }], index) => Member.of(blob, sfnt, fvar.instance, index));
        const rows = members.flatMap(Struct.get('rows'));
        const namedRow = Option.flatMap(selection, (wanted) => Array.findFirst(rows, flow(Struct.get('keys'), Array.containsWith(Unmatched.named)(wanted))));
        const { member: chosen, variations: selectedVariations } = yield* Option.match(selection, {
            onNone: () =>
                Effect.map(
                    Unmatched.single(members, 'face in collection', Option.none(), ({ base, index }) => [Option.getOrElse(base.names.postscriptName, Function.constant(`face ${index}`))]),
                    (member) => ({ member, variations: member.base.variations }),
                ),
            onSome: (wanted) =>
                Effect.fromOption(namedRow).pipe(
                    Effect.catch(() => Member.generated(members, wanted)),
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
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(Record.get(requested, tag), Function.constant(fallback))),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const coordinates = Record.fromIterableWith(axes, ({ tag, value }) => [tag, Names.spelled(value)]);
        const changed = Record.fromIterableWith(
            axes.filter(({ value, default: fallback }) => Names.spelled(value) !== Names.spelled(fallback)),
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
            metrics: Metrics.of({ font, tables, axes }),
            layout: yield* Layout.of({ ...chosen, tables }),
            color: Color.of(chosen),
            axes,
            namedInstances: chosen.namedInstances.map(({ names: { style, postscriptName }, variations: values }) => ({ style, postscriptName, variations: values })),
            extenders: tables.JSTF.scriptList.flatMap(({ tag, script }) => (tag === 'arab' ? Option.getOrElse(script.extenderGlyphs, Array.empty) : [])),
            font,
        });
    });

    glyph(codePoint: number) {
        return Option.fromUndefinedOr(this.font.nominalGlyph(codePoint));
    }
    draws(text: string) {
        return Array.fromIterable(text).every((character) => Option.isSome(this.glyph(character.codePointAt(0) ?? 0)));
    }
    advance(codePoint: number) {
        return Option.map(this.glyph(codePoint), (glyph) => this.em(this.font.glyphHAdvance(glyph)));
    }
    get em() {
        return (units: number) => units / this.unitsPerEm;
    }
    instance(variations: Record.ReadonlyRecord<string, number>) {
        const font = this.font.subFont();
        font.setVariations(this.axes.map(({ tag, value }) => new Variation(tag, Option.getOrElse(Record.get(variations, tag), Function.constant(value)))));
        return font;
    }
    height(codePoint: number) {
        return Option.flatMap(this.glyph(codePoint), (glyph) => Option.map(Option.fromUndefinedOr(this.font.glyphExtents(glyph)), ({ yBearing }) => this.em(yBearing)));
    }
    sizeRatio(other: Face, metric: { readonly [K in keyof Metrics]: Metrics[K] extends Option.Option<number> ? K : never }[keyof Metrics], codePoint = 0x6_27) {
        return Option.map(Option.all([this.height(codePoint), Option.filter(other.metrics[metric], Number.isGreaterThan(0))]), ([own, matched]) => own / matched);
    }
    get tatweel() {
        return Array.findFirst([...this.extenders.map((glyph) => this.em(this.font.glyphHAdvance(glyph))), ...Option.toArray(this.advance(Face.TATWEEL))], Number.isGreaterThan(0));
    }
    get nuqta() {
        const beh = 0x6_28;
        const widths = Option.map(this.glyph(beh), (glyph) =>
            Profile.outline(this.font.glyphToJson(glyph))
                .map((contour) => contour.flat().map(Struct.get('x')))
                .map((xs) => Math.max(...xs) - Math.min(...xs))
                .filter(Number.isGreaterThan(0)),
        );
        return Option.map(Option.flatMap(widths, Option.liftPredicate(Array.isArrayNonEmpty)), flow(Array.min(Order.Number), this.em));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face };
