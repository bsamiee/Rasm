import { Array, Option, Record, Schema, Struct } from 'effect';
import { Buffer, ClusterLevel, Direction, Feature, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import type { Face } from '#/face';
import { Settings, Tag } from '#/tags';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({
    text: Schema.String.annotate({ description: 'Text in one script and direction' }),
    features: Settings(Feature, 'HarfBuzz feature (tag, -tag, tag=value, tag[start:end] in UTF-16 offsets)'),
    language: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems tag (SRB, TRK), script default when absent' })),
    script: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'layout.languageSystems script (cyrl, arab), detected from text when absent' })),
    direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Script direction when absent' })),
});
class Shaped extends Schema.Class<Shaped, { readonly brand: unique symbol }>('Shaped')({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset of first character glyph shows' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Bounds from glyph origin, y up, height negative' })),
        }),
    ).annotate({ description: 'Visual order, positions in em' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters with no glyph, default-ignorables excluded' }),
}) {
    static readonly of = (face: Face, { text, features, language, script, direction }: typeof Run.Type) => {
        const em = (units: number) => units / face.unitsPerEm;
        const buffer = new Buffer();
        buffer.addText(text);
        buffer.setClusterLevel(ClusterLevel.MONOTONE_CHARACTERS);
        Option.map(language, (tag) => buffer.setLanguage(otTagToLanguage(tag)));
        Option.map(script, (tag) => buffer.setScript(otTagToScript(tag)));
        Option.map(direction, (key) => buffer.setDirection(Direction[key]));
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(face.font, buffer, [...features]);
        const glyphs = buffer.getGlyphInfos();
        const unmapped = new Set(glyphs.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        return Shaped.make({
            glyphs: Array.zipWith(glyphs, buffer.getGlyphPositions(), ({ codepoint, cluster }, position) => ({ glyph: codepoint, cluster, ...Record.map(position, em), extents: Option.map(Option.fromUndefinedOr(face.font.glyphExtents(codepoint)), Record.map(em)) })),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Run, Shaped };
