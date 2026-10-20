import { Buffer } from 'node:buffer';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Match, Number, Option, Order, Predicate, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, ClusterLevel, ColorPaletteFlags, Feature, Font, Buffer as HarfBuzzBuffer, Face as HarfBuzzFace, MetricsTag, shape, Variation } from 'harfbuzzjs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAG_LENGTH = 4;

// --- [MODELS] --------------------------------------------------------------------------

const Settings = <A extends Feature | Variation>(host: { new (tag: string, value?: number): A; fromString: (given: string) => A | undefined }, description: string) =>
    Schema.Array(
        Schema.String.annotate({ description }).pipe(
            Schema.decodeTo(Schema.instanceOf(host), {
                decode: SchemaGetter.transformEffect((given: string) => Effect.mapError(Effect.fromNullishOr(host.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Not a setting in HarfBuzz syntax: ${given}` }, given))),
                encode: SchemaGetter.transform((setting: A) => setting.toString()),
            }),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Instance = Schema.Struct({
    variations: Settings(Variation, 'Variation setting in HarfBuzz syntax, tag=value, applied over the instance the selection names; tags the face lacks are ignored and values clamp to the axis'),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis takes it over any opsz setting, clamped to its range' })),
});
const Run = Schema.Struct({
    text: Schema.String.annotate({ description: 'One script and direction run to shape and check for glyphs; omitted text returns no glyph geometry' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    features: Settings(Feature, 'OpenType feature setting in HarfBuzz syntax: tag, +tag, -tag, tag=value, or tag[start:end] over character positions'),
});
const Shaped = Schema.Struct({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID in the face' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in the input text of the first character the glyph shows; a ligature spans several characters' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Glyph bounds from its origin; positive y is up, height normally negative. Top is yOffset + yBearing above the pen baseline' })),
        }),
    ).annotate({ description: 'Shaped text in visual glyph order with default and requested features at this instance, positions in em; not application-composed text or document coordinates' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text the face shaped to no glyph; default-ignorable characters shape to invisible glyphs' }),
});
const Extents = Schema.Struct({ ascender: Schema.Finite, descender: Schema.Finite, lineGap: Schema.Finite });
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke centre, negative below the baseline' }), thickness: Schema.Finite });
const Outlines = Schema.Literals(['glyf', 'CFF ', 'CFF2']);
const Descriptor = Schema.TemplateLiteralParser([Schema.Finite, Schema.String.check(Schema.isMaxLength(TAG_LENGTH))])
    .pipe(
        Schema.decodeTo(Schema.Tuple([Schema.String, Schema.Finite]), {
            decode: SchemaGetter.transform(([value, tag]) => [tag.padEnd(TAG_LENGTH), value] as const),
            encode: SchemaGetter.transform(([tag, value]) => [value, tag.trimEnd()] as const),
        }),
    )
    .annotate({ description: 'Axis value descriptor of a generated instance PostScript name: the value, then the axis tag less trailing spaces (Adobe Technical Note 5902)' });
const Variations = Schema.Struct({
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.optional(Schema.Int) })) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ instance: [] }))),
});
const Tables = Schema.Struct({
    post: Schema.Struct({ isFixedPitch: Schema.Finite }),
    'OS/2': Schema.OptionFromOptionalKey(
        Schema.Struct({
            usWeightClass: Schema.Int,
            usWidthClass: Schema.Int,
            fsType: Schema.Struct(Struct.Record(['noEmbedding', 'viewOnly', 'editable', 'noSubsetting', 'bitmapOnly'], Schema.Boolean)),
            ...Struct.Record(['typoAscender', 'typoDescender', 'typoLineGap'], Schema.optional(Schema.Finite)),
            fsSelection: Schema.Struct({ useTypoMetrics: Schema.Boolean }),
        }),
    ),
    'CFF ': Schema.OptionFromOptionalKey(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
});
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')({
    ...Record.map({ postscriptName: 'PostScript name (name ID 6)', fullName: 'Full name (name ID 4)', family: 'Typographic family (name ID 16), else the family (name ID 1)', style: 'Typographic subfamily (name ID 17), else the subfamily (name ID 2)', version: 'Version string (name ID 5)' }, (description) =>
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
    extents: Schema.Struct({
        hhea: Extents.annotate({ description: 'Horizontal header ascender, descender (negative), and line gap' }),
        typo: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'OS/2 typographic ascender, descender, and line gap at this instance; absent when the table states none' })),
        win: Schema.OptionFromOptionalKey(Schema.Struct({ ascender: Schema.Finite, descender: Schema.Finite }).annotate({ description: 'OS/2 Windows clipping ascent and descent at this instance, descent positive; absent when the table states none' })),
        vertical: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'Vertical header ascender, descender, and line gap at this instance; absent when the face has no vhea table' })),
        useTypoMetrics: Schema.Boolean.annotate({ description: 'The face asks layout to take the typographic set over the header set' }),
    }).annotate({ description: 'Every ascent and descent the face states, in em' }),
    ...Record.map({ xHeight: 'OS/2 height of non-ascending lowercase letters at this instance', capHeight: 'OS/2 height of capital letters at this instance' }, (description) => Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: `${description}; absent when the table states none` }))),
    underline: Schema.Struct({
        post: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'The post table underline at this instance; absent when the face sets no stroke' })),
        cff: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'The CFF Top DICT underline of a CFF face; absent when the face is not CFF or sets no stroke' })),
    }).annotate({ description: 'Underline strokes the face states, in em' }),
    strikeout: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'OS/2 strikeout stroke at this instance; absent when the face sets no stroke' })),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Declared font-wide box at the default instance, in em' }),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean; the slnt axis value at this instance when the face has one' }),
    monospace: Schema.Boolean.annotate({ description: 'The face declares fixed-width spacing' }),
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
        description: 'GSUB and GPOS feature tags, plus mapped AAT tags and kern table kerning; a name identifies a stylistic set or character variant from any of its feature records',
    }),
    languageSystems: Schema.Array(Schema.Struct({ table: Schema.Literals(['GSUB', 'GPOS']), script: Schema.String, language: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Language system tag; absent for the script default' })), features: Schema.Array(Schema.String) })).annotate({
        description: 'Feature tags each GSUB and GPOS script and language system enables, for script- and language-specific forms such as locl and vert',
    }),
    axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite), hidden: Schema.Boolean })).annotate({
        description: 'Variation axes in fvar order with the value this instance applies; hidden axes are not meant for user interfaces',
    }),
    instances: Schema.Array(
        Schema.Struct({
            ...Record.map({ name: 'Subfamily name of the instance', postscriptName: 'PostScript name of the instance: the one fvar names, else the one Adobe Technical Note 5902 generates from the variations prefix and the subfamily' }, (description) =>
                Schema.OptionFromOptionalKey(Schema.String.annotate({ description })),
            ),
            variations: Schema.Record(Schema.String, Schema.Finite).annotate({ description: 'Axis values of the instance by axis tag' }),
        }),
    ).annotate({ description: 'Named instances; a selection takes their PostScript names' }),
    color: Schema.Struct({
        palettes: Schema.Array(
            Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), colors: Schema.Array(Schema.Struct({ ...Struct.Record(['red', 'green', 'blue', 'alpha'], Schema.Int), name: Schema.OptionFromOptionalKey(Schema.String) })), ...Struct.Record(['light', 'dark'], Schema.Boolean) }).annotate({
                description: 'CPAL palette of unpremultiplied 8-bit colors, and whether it suits a light or a dark background',
            }),
        ),
        ...Struct.Record(['layers', 'paint', 'png'], Schema.Boolean),
    }).annotate({ description: 'Color glyph data: CPAL palettes, COLRv0 layers, COLRv1 paint, and CBDT or sbix PNG images' }),
    shaper: Schema.instanceOf(Font).pipe(Schema.encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(() => 'A face opens from font bytes, not from its encoded form'), encode: SchemaGetter.omit() })),
}) {
    static readonly open = Effect.fn('open')(function* (data: Uint8Array, selection: Option.Option<string>, { variations, size }: typeof Instance.Type) {
        const container = yield* Effect.try(() => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength))).pipe(
            Effect.filterOrFail(
                ({ type }) => type !== 'WOFF' && type !== 'WOFF2',
                ({ type }) => new Cause.IllegalArgumentError(`${type} webfont containers must be decoded to OpenType or TrueType font bytes`),
            ),
        );
        const nameIds = { family: 1, subfamily: 2, fullName: 4, version: 5, postscriptName: 6, typographicFamily: 16, typographicSubfamily: 17, variationsPrefix: 25 } as const;
        const defaultLanguageSystem = 0xff_ff;
        const layouts = Face.fields.languageSystems.value.fields.table.literals;
        const blob = new Blob(data);
        const fonts = 'fonts' in container ? container.fonts : [container];
        const strip = (name: string) => name.replaceAll(/[^A-Za-z0-9]/gu, '');
        const hyphenated = (name: string) => name.replaceAll(' ', '-');
        const faces = Array.zip(fonts, yield* Effect.forEach(fonts, (each) => Schema.decodeUnknownEffect(Variations)(each))).map(([font, { fvar }], index) => {
            const handle = new HarfBuzzFace(blob, index);
            const names = Record.map(
                Array.groupBy(
                    Array.sortWith(handle.listNames(), ({ language }) => language !== 'en', Order.Boolean),
                    ({ nameId }) => `${nameId}`,
                ),
                ([{ nameId, language }]) => handle.getName(nameId, language),
            );
            const named = (id: number | undefined) =>
                Option.filter(
                    Option.flatMap(Option.fromUndefinedOr(id), (each) => Record.get(names, `${each}`)),
                    Predicate.isTruthy,
                );
            const infos = Object.values(handle.getAxisInfos());
            const prefix = Option.firstSomeOf([
                named(nameIds.variationsPrefix),
                Option.map(
                    Option.orElse(named(nameIds.typographicFamily), () => named(nameIds.family)),
                    strip,
                ),
            ]);
            return {
                font,
                handle,
                named,
                infos,
                prefix,
                instances: fvar.instance.map(({ nameID, coord, postscriptNameID }) => ({
                    name: named(nameID),
                    postscriptName: Option.firstSomeOf([named(postscriptNameID), Option.map(Option.all([prefix, Option.map(named(nameID), strip)]), Array.join('-'))]),
                    coreTextName: Option.map(Option.all([named(nameIds.postscriptName), Option.map(named(nameID), hyphenated)]), Array.join('_')),
                    variations: Record.fromEntries(Array.zip(infos.map(Struct.get('tag')), coord)),
                })),
            };
        });
        const rows = faces.flatMap((face) => [
            { face, postscriptName: face.named(nameIds.postscriptName), variations: Record.empty<string, number>() },
            ...face.instances.flatMap((instance) => [
                { face, ...instance },
                { face, ...instance, postscriptName: instance.coreTextName },
            ]),
        ]);
        const generated = (wanted: string) =>
            Array.findFirst(faces, (face) =>
                Option.map(
                    Option.flatMap(
                        Option.filter(face.prefix, (family) => wanted.startsWith(`${family}_`)),
                        (family) => Schema.decodeUnknownOption(Schema.Array(Descriptor))(wanted.slice(family.length + 1).split('_')),
                    ),
                    (settings) => ({ face, variations: Record.fromEntries(settings) }),
                ),
            );
        const candidates = Array.getSomes(rows.map(Struct.get('postscriptName')));
        const { face: chosen, variations: applied } = yield* Option.match(selection, {
            onNone: () =>
                Effect.map(
                    Unmatched.single('face in the collection', Option.none())(faces, ({ font }) => [font.postscriptName]),
                    (face) => ({ face, variations: Record.empty<string, number>() }),
                ),
            onSome: (wanted) => Effect.fromOption(Option.firstSomeOf([Array.findFirst(rows, ({ postscriptName }) => Option.contains(postscriptName, wanted)), generated(wanted)]), () => Unmatched.make({ subject: 'face or instance by PostScript name', query: selection, candidates })),
        });
        const tables = yield* Schema.decodeUnknownEffect(Tables)(chosen.font);
        const requested = { ...applied, ...Record.fromIterableWith([...variations, ...Option.toArray(Option.map(size, (points) => new Variation('opsz', points)))], ({ tag, value }) => [tag, value]) };
        const axes = chosen.infos.map(({ tag, nameId, flags, min, default: fallback, max }) => ({
            tag,
            name: chosen.named(nameId),
            min,
            default: fallback,
            max,
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(Record.get(requested, tag), () => fallback)),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const shaper = new Font(chosen.handle);
        shaper.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        const em = (units: number) => units / chosen.handle.upem;
        const metric = (tag: MetricsTag) => Option.fromUndefinedOr(shaper.getMetricPosition(tag));
        const drawn = Option.filter(({ thickness }: typeof Stroke.Type) => thickness !== 0);
        const stroke = ([offset, thickness]: readonly [MetricsTag, MetricsTag]) => drawn(Option.map(Option.all({ position: metric(offset), thickness: metric(thickness) }), ({ position, thickness: width }) => Record.map({ position: position - width / 2, thickness: width }, em)));
        return Face.make({
            ...Record.map(Struct.pick(nameIds, ['postscriptName', 'fullName', 'version']), chosen.named),
            family: Option.orElse(chosen.named(nameIds.typographicFamily), () => chosen.named(nameIds.family)),
            style: Option.orElse(chosen.named(nameIds.typographicSubfamily), () => chosen.named(nameIds.subfamily)),
            unitsPerEm: chosen.handle.upem,
            glyphCount: chosen.font.numGlyphs,
            coverage: chosen.handle.collectUnicodes().length,
            outlines: Array.findFirst(Outlines.literals, (tag) => Predicate.isNotUndefined(chosen.handle.referenceTable(tag))),
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
            extents: {
                hhea: Record.map({ ascender: chosen.font.ascent, descender: chosen.font.descent, lineGap: chosen.font.lineGap }, em),
                typo: Option.map(
                    Option.flatMap(tables['OS/2'], ({ typoAscender, typoDescender, typoLineGap }) => Option.all({ ascender: Option.fromUndefinedOr(typoAscender), descender: Option.fromUndefinedOr(typoDescender), lineGap: Option.fromUndefinedOr(typoLineGap) })),
                    ({ ascender, descender, lineGap }) => Record.map({ ascender: ascender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_ASCENDER), descender: descender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_DESCENDER), lineGap: lineGap + shaper.getMetricVariation(MetricsTag.HORIZONTAL_LINE_GAP) }, em),
                ),
                win: Option.map(Option.all({ ascender: metric(MetricsTag.HORIZONTAL_CLIPPING_ASCENT), descender: metric(MetricsTag.HORIZONTAL_CLIPPING_DESCENT) }), Record.map(em)),
                vertical: Option.map(Option.all({ ascender: metric(MetricsTag.VERTICAL_ASCENDER), descender: metric(MetricsTag.VERTICAL_DESCENDER), lineGap: metric(MetricsTag.VERTICAL_LINE_GAP) }), Record.map(em)),
                useTypoMetrics: Option.exists(tables['OS/2'], ({ fsSelection }) => fsSelection.useTypoMetrics),
            },
            ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag) => Option.map(Option.filter(metric(tag), Predicate.isTruthy), em)),
            underline: {
                post: stroke([MetricsTag.UNDERLINE_OFFSET, MetricsTag.UNDERLINE_SIZE]),
                cff: drawn(Option.map(tables['CFF '], ({ topDict: { UnderlinePosition, UnderlineThickness, FontMatrix } }) => ({ position: UnderlinePosition * FontMatrix[3], thickness: UnderlineThickness * FontMatrix[3] }))),
            },
            strikeout: stroke([MetricsTag.STRIKEOUT_OFFSET, MetricsTag.STRIKEOUT_SIZE]),
            bounds: Record.map(Struct.pick(chosen.font.bbox, ['minX', 'minY', 'maxX', 'maxY']), em),
            italicAngle: Option.match(
                Array.findFirst(axes, ({ tag }) => tag === 'slnt'),
                { onNone: () => chosen.font.italicAngle, onSome: Struct.get('value') },
            ),
            monospace: tables.post.isFixedPitch !== 0,
            features: Record.toEntries(
                Array.groupBy([...layouts.flatMap((table) => chosen.handle.getTableFeatureTags(table).map((tag, at) => ({ tag, name: chosen.named(chosen.handle.getFeatureNameIds(table, at)?.uiLabelNameId) }))), ...chosen.font.availableFeatures.map((tag) => ({ tag, name: Option.none<string>() }))], Struct.get('tag')),
            ).map(([tag, records]) => ({ tag, name: Option.firstSomeOf(records.map(Struct.get('name'))) })),
            languageSystems: layouts
                .flatMap((table) => chosen.handle.getTableScriptTags(table).map((script, at) => ({ table, script, at })))
                .flatMap(({ table, script, at }) => [
                    { table, script, language: Option.none<string>(), features: chosen.handle.getLanguageFeatureTags(table, at, defaultLanguageSystem) },
                    ...chosen.handle.getScriptLanguageTags(table, at).map((language, index) => ({ table, script, language: Option.some(language), features: chosen.handle.getLanguageFeatureTags(table, at, index) })),
                ]),
            axes,
            instances: chosen.instances.map(Struct.pick(['name', 'postscriptName', 'variations'])),
            color: {
                palettes: chosen.handle.getColorPalettes().map(({ colors, nameId, flags }) => ({
                    name: chosen.named(nameId),
                    colors: colors.map(({ nameId: label, ...channels }) => ({ ...channels, name: chosen.named(label) })),
                    ...Record.map({ light: ColorPaletteFlags.USABLE_WITH_LIGHT_BACKGROUND, dark: ColorPaletteFlags.USABLE_WITH_DARK_BACKGROUND }, (flag) => (flags & flag) !== 0),
                })),
                layers: chosen.handle.hasColorLayers(),
                paint: chosen.handle.hasColorPaint(),
                png: chosen.handle.hasColorPng(),
            },
            shaper,
        });
    });

    shape({ text, features }: typeof Run.Type) {
        const em = (units: number) => units / this.unitsPerEm;
        const buffer = new HarfBuzzBuffer();
        buffer.addText(text);
        buffer.setClusterLevel(ClusterLevel.MONOTONE_CHARACTERS);
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(this.shaper, buffer, [...features]);
        const glyphs = buffer.getGlyphInfos();
        const unmapped = new Set(glyphs.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        return {
            glyphs: Array.zipWith(glyphs, buffer.getGlyphPositions(), ({ codepoint, cluster }, position) => ({ glyph: codepoint, cluster, ...Record.map(position, em), extents: Option.map(Option.fromUndefinedOr(this.shaper.glyphExtents(codepoint)), Record.map(em)) })),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        };
    }
    outline(glyph: number) {
        return this.shaper.glyphToJson(glyph).map(({ type, values }) => ({ type, values: values.map((value) => value / this.unitsPerEm) }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face, Instance, Run, Shaped };
