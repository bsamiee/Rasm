import { Array, Effect, Number, Option, Record, Schema, Struct } from 'effect';
import { Buffer, BufferFlag, ClusterLevel, Direction, Feature, GlyphClass, GlyphFlag, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import { Settings } from '#/composition/settings';
import type { Face } from '#/face';
import { Tag, TagSettings } from '#/tags';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DEFAULT_CLUSTER_LEVEL = 'MONOTONE_CHARACTERS' as const;
const DIRECTIONS = { rtl: 'RTL', ltr: 'LTR' } as const satisfies Record<typeof Settings.Type.direction extends Option.Option<infer D> ? D : never, keyof typeof Direction>;

// --- [MODELS] --------------------------------------------------------------------------

const GlyphFlags = Schema.Literals(Struct.keys(Struct.omit(GlyphFlag, ['DEFINED'])));
const GlyphClasses = Schema.Literals(Struct.keys(GlyphClass));
class ShapedText extends Schema.Class<ShapedText, { readonly brand: unique symbol }>('ShapedText')({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset of first character glyph shows' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Bounds from glyph origin, y up, height negative' })),
            class: GlyphClasses.annotate({ description: 'GDEF glyph class' }),
            flags: Schema.Array(GlyphFlags).annotate({ description: 'HarfBuzz glyph flags, UNSAFE_TO_CONCAT and SAFE_TO_INSERT_TATWEEL with their PRODUCE run flag' }),
        }),
    ).annotate({ description: 'Visual order, positions in em' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters with no glyph, default-ignorables excluded' }),
}) {
    static readonly Run = Schema.Struct({
        text: Schema.String.annotate({ description: 'Text in one script and direction' }),
        features: TagSettings(Feature, 'HarfBuzz feature (tag, -tag, tag=value, tag[start:end] in UTF-16 offsets)'),
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
    static readonly of = (face: Face, { text, features, language, script, direction, clusterLevel, flags }: typeof ShapedText.Run.Type, settings = Settings.make({})) => {
        const em = (units: number) => units / face.unitsPerEm;
        const buffer = new Buffer();
        buffer.addText(text);
        buffer.setClusterLevel(ClusterLevel[clusterLevel]);
        buffer.setFlags(Number.sumAll(Array.map(flags, (flag) => BufferFlag[flag])));
        Option.map(language, (tag) => buffer.setLanguage(otTagToLanguage(tag)));
        Option.map(script, (tag) => buffer.setScript(otTagToScript(tag)));
        Option.map(
            Option.orElse(direction, () => Option.map(settings.direction, (key) => DIRECTIONS[key])),
            (key) => buffer.setDirection(Direction[key]),
        );
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(face.font, buffer, [...features, ...Record.collect(settings.features, (tag, value) => new Feature(tag, value))]);
        const glyphs = buffer.getGlyphInfos();
        const unmapped = new Set(glyphs.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        return ShapedText.make({
            glyphs: Array.zipWith(glyphs, buffer.getGlyphPositions(), ({ codepoint, cluster, flags: glyphFlags }, position) => ({
                glyph: codepoint,
                cluster,
                ...Record.map(position, em),
                extents: Option.map(Option.fromUndefinedOr(face.font.glyphExtents(codepoint)), Record.map(em)),
                class: Array.getUnsafe(GlyphClasses.literals, face.font.face.getGlyphClass(codepoint)),
                flags: GlyphFlags.literals.filter((flag) => (glyphFlags & GlyphFlag[flag]) !== 0),
            })),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        });
    };

    get advance() {
        return Number.sumAll(this.glyphs.map(Struct.get('xAdvance')));
    }
    offsetMarks({ x, y }: { readonly x: number; readonly y: number }) {
        return ShapedText.make({ glyphs: this.glyphs.map((glyph) => (glyph.class === 'MARK' ? { ...glyph, xOffset: glyph.xOffset + x, yOffset: glyph.yOffset + y } : glyph)), missing: this.missing });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ShapedText };
