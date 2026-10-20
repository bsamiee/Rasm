import { Buffer } from 'node:buffer';
import { hash } from 'node:crypto';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Equal, flow, Match, Number, Option, Order, Predicate, pipe, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, ClusterLevel, ColorPaletteFlags, Direction, Feature, Font, Buffer as HarfBuzzBuffer, Face as HarfBuzzFace, MetricsTag, otTagToLanguage, otTagToScript, shape, Variation } from 'harfbuzzjs';
import type { Source } from './source.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAG_LENGTH = 4;
const LEADING_ZERO = /^(?<sign>-?)0\./u;

// --- [MODELS] --------------------------------------------------------------------------

const Settings = <A extends Feature | Variation>(host: { new (tag: string, value?: number): A; fromString: (given: string) => A | undefined }, description: string) =>
    Schema.Array(
        Schema.String.annotate({ description }).pipe(
            Schema.decodeTo(Schema.instanceOf(host), {
                decode: SchemaGetter.transformEffect((given: string, options) => Effect.fromOption(Option.fromNullishOr(host.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Not a setting in HarfBuzz syntax: ${given}` }, given, options))),
                encode: SchemaGetter.transform((setting: A) => setting.toString()),
            }),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Tag = Schema.String.check(Schema.isMaxLength(TAG_LENGTH)).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform((tag: string) => tag.padEnd(TAG_LENGTH)), encode: SchemaGetter.transform((tag: string) => tag.trimEnd()) }));
const Instance = Schema.Struct({
    variations: Settings(Variation, 'Variation setting in HarfBuzz syntax, tag=value, applied over the instance the selection names; tags the face lacks are ignored and values clamp to the axis'),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis takes it over any opsz setting, clamped to its range' })),
});
const Run = Schema.Struct({
    text: Schema.String.annotate({ description: 'One script and direction run to shape and check for glyphs; omitted text returns no glyph geometry' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    features: Settings(Feature, 'OpenType feature setting in HarfBuzz syntax: tag, +tag, -tag, tag=value, or tag[start:end] over UTF-16 offsets, the cluster values of the shaped glyphs'),
    language: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'OpenType language system tag languageSystems reports, such as SRB or TRK; absent shapes with the script default' })),
    script: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'OpenType script tag languageSystems reports, such as cyrl or arab; absent takes the script of the text' })),
    direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Absent takes the direction of the script' })),
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
const Extents = Schema.Struct(Struct.Record(['ascender', 'descender', 'lineGap'], Schema.Finite));
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke centre, negative below the baseline' }), thickness: Schema.Finite });
const Outlines = Schema.Literals(['glyf', 'CFF ', 'CFF2']);
const Descriptor = Schema.TemplateLiteralParser([Schema.String.check(Schema.isStringFinite()).pipe(Schema.decodeTo(Schema.Finite, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((value: number) => `${value}`.replace(LEADING_ZERO, '$<sign>.')) })), Tag]).pipe(
    Schema.decodeTo(Schema.Tuple([Schema.String, Schema.Finite]), { decode: SchemaGetter.transform(([value, tag]) => [tag, value] as const), encode: SchemaGetter.transform(([tag, value]) => [value, tag] as const) }),
);
const Generated = Schema.TemplateLiteralParser([Schema.String.check(Schema.isPattern(/^[^_]+$/u)), '_', Schema.String.pipe(Schema.decodeTo(Schema.Array(Descriptor), { decode: SchemaGetter.split({ separator: '_' }), encode: SchemaGetter.transform(Array.join('_')) }))]);
const Variations = Schema.Array(
    Schema.Struct({
        fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ instance: [] }))),
    }),
);
const Tables = Schema.Struct({
    directory: Schema.Struct({ tables: Schema.Record(Schema.String, Schema.Unknown) }),
    post: Schema.Struct({ isFixedPitch: Schema.Finite }),
    'OS/2': Schema.OptionFromOptionalKey(
        Schema.Struct({
            usWeightClass: Schema.Int,
            usWidthClass: Schema.Int,
            fsType: Schema.Struct(Struct.Record(['noEmbedding', 'viewOnly', 'editable', 'noSubsetting', 'bitmapOnly'], Schema.Boolean)),
            ...Struct.Record(['typoAscender', 'typoDescender', 'typoLineGap'], Schema.OptionFromOptionalKey(Schema.Finite)),
            fsSelection: Schema.Struct({ useTypoMetrics: Schema.Boolean }),
        }),
    ),
    'CFF ': Schema.OptionFromOptionalKey(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
});
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
        extents: Schema.Struct({
            hhea: Extents.annotate({ description: 'Horizontal header ascender, descender (negative), and line gap' }),
            typo: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'OS/2 typographic ascender, descender, and line gap at this instance; absent when the table states none' })),
            win: Schema.OptionFromOptionalKey(Extents.mapFields(Struct.omit(['lineGap'])).annotate({ description: 'OS/2 Windows clipping ascent and descent at this instance, descent positive; absent when the table states none' })),
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
                ...Record.map({ style: 'Subfamily name of the instance', postscriptName: 'PostScript name of the instance: the one fvar names, else the one Adobe Technical Note 5902 generates from the variations prefix and the subfamily' }, (description) =>
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
    },
    { description: 'One face at one instance; postscriptName, fullName, and style name the default or named instance at its coordinates, else postscriptName is the Adobe Technical Note 5902 name of the axis values and the other two are absent' },
) {
    static readonly open = Effect.fn('open')(function* ({ data, postscriptName: selection }: typeof Source.Type, { variations, size }: typeof Instance.Type) {
        const container = yield* Effect.try(() => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength))).pipe(
            Effect.filterOrFail(
                ({ type }) => type !== 'WOFF' && type !== 'WOFF2',
                ({ type }) => new Cause.IllegalArgumentError(`${type} webfont containers must be decoded to OpenType or TrueType font bytes`),
            ),
        );
        const nameIds = { family: 1, subfamily: 2, fullName: 4, version: 5, postscriptName: 6, typographicFamily: 16, typographicSubfamily: 17, variationsPrefix: 25 } as const;
        const defaultLanguageSystem = 0xff_ff;
        const fractionBits = 16;
        const nameLength = 127;
        const layouts = Face.fields.languageSystems.value.fields.table.literals;
        const blob = new Blob(data);
        const fonts = 'fonts' in container ? container.fonts : [container];
        const strip = (name: string) => name.replaceAll(/[^A-Za-z0-9]/gu, '');
        const hyphenated = (name: string) => name.replaceAll(' ', '-');
        const fixed = (value: number) => Math.round(value * 2 ** fractionBits);
        const decimal = (value: number) => {
            const target = fixed(value);
            return Array.min(
                Array.map(Array.range(0, fractionBits), (places) => Number.round(value, places)),
                Order.mapInput(Order.Boolean, (rounded) => fixed(rounded) !== target),
            );
        };
        const faces = Array.zip(fonts, yield* Schema.decodeUnknownEffect(Variations)(fonts, { errors: 'all' })).map(([font, { fvar }], index) => {
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
                font,
                handle,
                named,
                names,
                infos,
                prefix,
                base: { names, keys: Option.toArray(names.postscriptName), variations: defaults, coordinates: Record.map(defaults, decimal) },
                instances: fvar.instance.map(({ nameID, coord, postscriptNameID }) => {
                    const style = named(nameID);
                    const postscriptName = Option.firstSomeOf([Option.flatMap(postscriptNameID, named), Option.map(Option.all([prefix, Option.map(style, strip)]), Array.join('-'))]);
                    const values = Record.fromEntries(Array.zip(infos.map(Struct.get('tag')), coord));
                    return {
                        names: { postscriptName, fullName: Option.map(Option.all([names.family, style]), Array.join(' ')), style },
                        keys: Array.getSomes([postscriptName, Option.map(Option.all([names.postscriptName, Option.map(style, hyphenated)]), Array.join('_'))]),
                        variations: values,
                        coordinates: Record.map(values, decimal),
                    };
                }),
            };
        });
        const rows = faces.flatMap((face) => [face.base, ...face.instances].map((row) => ({ face, ...row })));
        const listed = Option.flatMap(selection, (wanted) => Array.findFirst(rows, ({ keys }) => Array.contains(keys, wanted)));
        const generated = Effect.gen(function* () {
            const [prefix, , settings] = yield* Effect.flatMap(Effect.fromOption(selection), Schema.decodeUnknownEffect(Generated));
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
        const tables = yield* Schema.decodeUnknownEffect(Tables)(chosen.font);
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
        const coordinates = Record.fromIterableWith(axes, ({ tag, value }) => [tag, decimal(value)]);
        const encoded = Option.flatMap(chosen.prefix, (prefix) => Option.map(Schema.encodeOption(Generated)([prefix, '_', Record.toEntries(Record.filter(coordinates, (value, tag) => value !== chosen.base.coordinates[tag]))]), (name) => (name.length > nameLength ? `${prefix}-${hash('sha1', name)}...` : name)));
        const identity = pipe(
            Array.findFirst([...Option.toArray(listed), chosen.base, ...chosen.instances], (row) => Equal.equals(row.coordinates, coordinates)),
            Option.map(Struct.get('names')),
            Option.getOrElse(() => ({ postscriptName: encoded, fullName: Option.none<string>(), style: Option.none<string>() })),
        );
        const shaper = new Font(chosen.handle);
        shaper.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        const em = (units: number) => units / chosen.handle.upem;
        const metric = (tag: MetricsTag) => Option.map(Option.fromUndefinedOr(shaper.getMetricPosition(tag)), em);
        const drawn = Option.filter(({ thickness }: typeof Stroke.Type) => thickness !== 0);
        const stroke = ([offset, width]: readonly [MetricsTag, MetricsTag]) => drawn(Option.map(Option.all({ position: metric(offset), thickness: metric(width) }), ({ position, thickness }) => ({ position: position - thickness / 2, thickness })));
        return Face.make({
            ...chosen.names,
            ...identity,
            unitsPerEm: chosen.handle.upem,
            glyphCount: chosen.font.numGlyphs,
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
            extents: {
                hhea: Record.map({ ascender: chosen.font.ascent, descender: chosen.font.descent, lineGap: chosen.font.lineGap }, em),
                typo: tables['OS/2'].pipe(
                    Option.flatMap(({ typoAscender: ascender, typoDescender: descender, typoLineGap: lineGap }) => Option.all({ ascender, descender, lineGap })),
                    Option.map(({ ascender, descender, lineGap }) =>
                        Record.map({ ascender: ascender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_ASCENDER), descender: descender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_DESCENDER), lineGap: lineGap + shaper.getMetricVariation(MetricsTag.HORIZONTAL_LINE_GAP) }, em),
                    ),
                ),
                win: Option.all({ ascender: metric(MetricsTag.HORIZONTAL_CLIPPING_ASCENT), descender: metric(MetricsTag.HORIZONTAL_CLIPPING_DESCENT) }),
                vertical: Option.all({ ascender: metric(MetricsTag.VERTICAL_ASCENDER), descender: metric(MetricsTag.VERTICAL_DESCENDER), lineGap: metric(MetricsTag.VERTICAL_LINE_GAP) }),
                useTypoMetrics: Option.exists(tables['OS/2'], ({ fsSelection }) => fsSelection.useTypoMetrics),
            },
            ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag) => Option.filter(metric(tag), Predicate.isTruthy)),
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
            instances: chosen.instances.map(({ names: { style, postscriptName }, variations: values }) => ({ style, postscriptName, variations: values })),
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

    shape({ text, features, language, script, direction }: typeof Run.Type) {
        const em = (units: number) => units / this.unitsPerEm;
        const buffer = new HarfBuzzBuffer();
        buffer.addText(text);
        buffer.setClusterLevel(ClusterLevel.MONOTONE_CHARACTERS);
        Option.map(language, (tag) => buffer.setLanguage(otTagToLanguage(tag)));
        Option.map(script, (tag) => buffer.setScript(otTagToScript(tag)));
        Option.map(direction, (key) => buffer.setDirection(Direction[key]));
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
