import { Array, Option, Record, Schema, Struct } from 'effect';
import { Buffer, ClusterLevel, Direction, Feature, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import type { Face } from './face.ts';
import { Settings, Tag } from './tags.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({
    text: Schema.String.annotate({ description: 'One script and direction run to shape and check for glyphs' }),
    features: Settings(Feature, 'OpenType feature setting in HarfBuzz syntax: tag, +tag, -tag, tag=value, or tag[start:end] over UTF-16 offsets, the cluster values of the shaped glyphs'),
    language: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'OpenType language system tag layout.languageSystems reports, such as SRB or TRK; absent shapes with the script default' })),
    script: Schema.OptionFromOptionalKey(Tag.annotate({ description: 'OpenType script tag layout.languageSystems reports, such as cyrl or arab; absent takes the script of the text' })),
    direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Absent takes the direction of the script' })),
});
class Shaped extends Schema.Class<Shaped, { readonly brand: unique symbol }>('Shaped')({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID in the face' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in the input text of the first character the glyph shows; a ligature spans several characters' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Glyph bounds from its origin; positive y is up, height normally negative. Top is yOffset + yBearing above the pen baseline' })),
        }),
    ).annotate({ description: 'Shaped text in visual glyph order with default and requested features at this instance, positions in em' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text the face shaped to no glyph; default-ignorable characters shape to invisible glyphs' }),
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
