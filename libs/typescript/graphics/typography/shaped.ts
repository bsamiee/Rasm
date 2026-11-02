import { Array, Effect, Number, Option, Record, Schema, Struct } from 'effect';
import { Buffer, BufferFlag, ClusterLevel, Direction, Feature, GlyphClass, GlyphFlag, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import type { Face } from '#/face';
import { Tag, TagSettings } from '#/tags';

// --- [MODELS] --------------------------------------------------------------------------

const GlyphFlags = Schema.Literals(Struct.keys(Struct.omit(GlyphFlag, ['DEFINED'])));
const GlyphClasses = Schema.Literals(Struct.keys(GlyphClass));
class Shaped extends Schema.Class<Shaped, { readonly brand: unique symbol }>('Shaped')({
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
        language: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems tag (SRB, TRK), script default when absent' })),
        script: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems script (cyrl, arab), detected from text when absent' })),
        direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Script direction when absent' })),
        clusterLevel: Schema.Literals(Struct.keys(Struct.omit(ClusterLevel, ['DEFAULT'])))
            .annotate({ description: 'HarfBuzz cluster level, MONOTONE_CHARACTERS when absent' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed('MONOTONE_CHARACTERS'))),
        flags: Schema.UniqueArray(Schema.Literals(Struct.keys(Struct.omit(BufferFlag, ['DEFAULT', 'DEFINED']))))
            .annotate({ description: 'HarfBuzz buffer flags (BOT, EOT, PRODUCE_UNSAFE_TO_CONCAT), none when absent' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    });
    static readonly of = (face: Face, { text, features, language, script, direction, clusterLevel, flags }: typeof Shaped.Run.Type) => {
        const em = (units: number) => units / face.unitsPerEm;
        const buffer = new Buffer();
        buffer.addText(text);
        buffer.setClusterLevel(ClusterLevel[clusterLevel]);
        buffer.setFlags(Number.sumAll(Array.map(flags, (flag) => BufferFlag[flag])));
        Option.map(language, (tag) => buffer.setLanguage(otTagToLanguage(tag)));
        Option.map(script, (tag) => buffer.setScript(otTagToScript(tag)));
        Option.map(direction, (key) => buffer.setDirection(Direction[key]));
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(face.font, buffer, [...features]);
        const glyphs = buffer.getGlyphInfos();
        const unmapped = new Set(glyphs.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        return Shaped.make({
            glyphs: Array.zipWith(glyphs, buffer.getGlyphPositions(), ({ codepoint, cluster, flags: set }, position) => ({
                glyph: codepoint,
                cluster,
                ...Record.map(position, em),
                extents: Option.map(Option.fromUndefinedOr(face.font.glyphExtents(codepoint)), Record.map(em)),
                class: Option.getOrElse(
                    Array.findFirst(GlyphClasses.literals, (key) => GlyphClass[key] === face.font.face.getGlyphClass(codepoint)),
                    () => 'UNCLASSIFIED' as const,
                ),
                flags: GlyphFlags.literals.filter((flag) => (set & GlyphFlag[flag]) !== 0),
            })),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        });
    };

    get advance() {
        return Number.sumAll(this.glyphs.map(Struct.get('xAdvance')));
    }
    offsetMarks({ x, y }: { readonly x: number; readonly y: number }) {
        return Shaped.make({ glyphs: this.glyphs.map((glyph) => (glyph.class === 'MARK' ? { ...glyph, xOffset: glyph.xOffset + x, yOffset: glyph.yOffset + y } : glyph)), missing: this.missing });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shaped };
