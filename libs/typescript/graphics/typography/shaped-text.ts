import { Array, Effect, Function, Number, Option, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Buffer, BufferFlag, ClusterLevel, Direction, Feature, GlyphClass, GlyphFlag, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import type { Face } from '#/face';
import { Settings } from '#/settings';
import { Tag } from '#/tag';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEFAULT_CLUSTER_LEVEL = 'MONOTONE_CHARACTERS' as const;

// --- [MODELS] --------------------------------------------------------------------------

const GlyphFlags = Schema.Literals(Struct.keys(Struct.omit(GlyphFlag, ['DEFINED'])));
const GlyphClasses = Schema.Literals(Struct.keys(GlyphClass));
const FeatureString = Schema.String.pipe(
    Schema.decodeTo(Schema.instanceOf(Feature), {
        decode: SchemaGetter.transformEffect((given: string, options) => Effect.fromOption(Option.fromUndefinedOr(Feature.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Invalid HarfBuzz feature ${given}` }, given, options))),
        encode: SchemaGetter.String(),
    }),
);
class ShapedText extends Schema.Class<ShapedText, { readonly brand: unique symbol }>('ShapedText')({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in run text of first character glyph shows' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Bounds from glyph origin, y up, height negative' })),
            class: GlyphClasses.annotate({ description: 'GDEF glyph class' }),
            flags: Schema.Array(GlyphFlags).annotate({ description: 'HarfBuzz glyph flags, UNSAFE_TO_CONCAT and SAFE_TO_INSERT_TATWEEL with their PRODUCE run flag' }),
        }),
    ).annotate({ description: 'Visual order, positions in em, marks moved by settings mark offset' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters with no glyph, default-ignorables excluded' }),
}) {
    static readonly Run = Schema.Struct({
        text: Schema.String.annotate({ description: 'Text in one script and direction, or context around item' }),
        item: Schema.OptionFromOptionalKey(
            Schema.Struct(Struct.Record(['start', 'end'], Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))))
                .check(Schema.makeFilter(({ start, end }) => start <= end, { description: 'start <= end' }))
                .annotate({ description: 'UTF-16 range shaped, text around it giving context, whole text when absent' }),
        ).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        features: Schema.Array(FeatureString)
            .annotate({ description: 'HarfBuzz features (tag, -tag, tag=value, tag[start:end] in UTF-16 offsets) after settings features' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
        language: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems tag (SRB, TRK), script default when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        script: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems script (cyrl, arab), detected from text when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Script direction when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        clusterLevel: Schema.Literals(Struct.keys(Struct.omit(ClusterLevel, ['DEFAULT'])))
            .annotate({ description: `HarfBuzz cluster level, ${DEFAULT_CLUSTER_LEVEL} when absent` })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed(DEFAULT_CLUSTER_LEVEL)), Schema.withConstructorDefault(Effect.succeed(DEFAULT_CLUSTER_LEVEL))),
        flags: Schema.UniqueArray(Schema.Literals(Struct.keys(Struct.omit(BufferFlag, ['DEFAULT', 'DEFINED']))))
            .annotate({ description: 'HarfBuzz buffer flags (BOT, EOT, PRODUCE_UNSAFE_TO_CONCAT), none when absent' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    });
    static readonly of = (face: Face, { text, item, features, language, script, direction, clusterLevel, flags }: typeof ShapedText.Run.Type, settings = Settings.make({})) => {
        const em = (units: number) => units / face.unitsPerEm;
        const buffer = new Buffer();
        const { start, end } = Option.getOrElse(item, () => ({ start: 0, end: text.length }));
        buffer.addText(settings.digits(text), start, end - start);
        buffer.setClusterLevel(ClusterLevel[clusterLevel]);
        buffer.setFlags(Number.sumAll(Array.map(flags, (flag) => BufferFlag[flag])));
        Option.match(language, { onNone: Function.constVoid, onSome: (tag) => buffer.setLanguage(otTagToLanguage(tag)) });
        Option.match(script, { onNone: Function.constVoid, onSome: (tag) => buffer.setScript(otTagToScript(tag)) });
        Option.match(direction, { onNone: Function.constVoid, onSome: (key) => buffer.setDirection(Direction[key]) });
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(face.font, buffer, [...Record.collect(settings.features, (tag, value) => new Feature(tag, value)), ...features]);
        const glyphs = buffer.getGlyphInfos();
        const unmapped = new Set(glyphs.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        return ShapedText.make({
            glyphs: Array.zipWith(glyphs, buffer.getGlyphPositions(), ({ codepoint, cluster, flags: glyphFlags }, position) => {
                const glyphClass = face.font.face.getGlyphClass(codepoint);
                const kind = Option.getOrElse(Array.findFirst(GlyphClasses.literals, (key) => GlyphClass[key] === glyphClass), () => 'UNCLASSIFIED' as const);
                const placed = Record.map(position, em);
                return {
                    glyph: codepoint,
                    cluster,
                    ...(kind === 'MARK' ? { ...placed, xOffset: placed.xOffset + settings.marks.x, yOffset: placed.yOffset + settings.marks.y } : placed),
                    extents: Option.map(Option.fromUndefinedOr(face.font.glyphExtents(codepoint)), Record.map(em)),
                    class: kind,
                    flags: GlyphFlags.literals.filter((flag) => (glyphFlags & GlyphFlag[flag]) !== 0),
                };
            }),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        });
    };

    get advance() {
        return Math.hypot(Number.sumAll(this.glyphs.map(Struct.get('xAdvance'))), Number.sumAll(this.glyphs.map(Struct.get('yAdvance'))));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ShapedText };
