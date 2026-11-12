import { Array, Effect, Function, Option, Order, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Buffer, BufferFlag, ClusterLevel, Direction, Feature, GlyphClass, GlyphFlag, otTagToLanguage, otTagToScript, shape } from 'harfbuzzjs';
import { Settings } from '#/settings';
import type { Face } from '#/shaping/face';
import { Span } from '#/shaping/word';

// --- [MODELS] --------------------------------------------------------------------------

const GlyphFlags = Schema.Literals(Struct.keys(Struct.omit(GlyphFlag, ['DEFINED'])));
const GlyphClasses = Schema.Literals(Struct.keys(GlyphClass));
const Bounds = Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Union of HarfBuzz glyph extents in em from run origin, y up' });
const FeatureString = Schema.String.pipe(
    Schema.decodeTo(Schema.instanceOf(Feature), {
        decode: SchemaGetter.transformEffect((given: string, options) => Effect.fromOption(Option.fromUndefinedOr(Feature.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Invalid HarfBuzz feature ${given}` }, given, options))),
        encode: SchemaGetter.String(),
    }),
);
class ShapedText extends Schema.Class<ShapedText, { readonly brand: unique symbol }>('ShapedText')({
    text: Schema.String.annotate({ description: 'Run text in settings numbering system' }),
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in text of first character glyph shows' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromOptionalKey(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite)).annotate({ description: 'Bounds from glyph origin, y up, height negative' })),
            scale: Schema.Finite.annotate({ description: 'Horizontal outline scale, 1 unscaled' }),
            class: GlyphClasses.annotate({ description: 'GDEF glyph class' }),
            flags: Schema.Array(GlyphFlags).annotate({ description: 'HarfBuzz glyph flags, UNSAFE_TO_CONCAT and SAFE_TO_INSERT_TATWEEL with their PRODUCE run flag' }),
        }),
    ).annotate({ description: 'Visual order, positions in em, marks moved by settings mark offset away from their base' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters with no glyph, default-ignorables excluded' }),
}) {
    static readonly Run = Schema.Struct({
        text: Schema.String.annotate({ description: 'Text in one script and direction, or context around item' }),
        item: Schema.OptionFromOptionalKey(Span.annotate({ description: 'UTF-16 range shaped, text around it giving context, whole text when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        features: Schema.Array(FeatureString)
            .annotate({ description: 'HarfBuzz features (tag, -tag, tag=value, tag[start:end] in UTF-16 offsets) after settings features' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
        language: Schema.OptionFromOptionalKey(Settings.Tag.annotate({ description: 'layout.languageSystems tag (SRB, TRK), script default when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        script: Schema.OptionFromOptionalKey(Settings.Tag.annotate({ description: 'layout.languageSystems script (cyrl, arab), detected from text when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        direction: Schema.OptionFromOptionalKey(Schema.Literals(Struct.keys(Struct.omit(Direction, ['INVALID']))).annotate({ description: 'Script direction when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        clusterLevel: Schema.Literals(Struct.keys(Struct.omit(ClusterLevel, ['DEFAULT'])))
            .annotate({ description: 'HarfBuzz cluster level, MONOTONE_CHARACTERS when absent' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed('MONOTONE_CHARACTERS')), Schema.withConstructorDefault(Effect.succeed('MONOTONE_CHARACTERS' as const))),
        flags: Schema.UniqueArray(Schema.Literals(Struct.keys(Struct.omit(BufferFlag, ['DEFAULT', 'DEFINED']))))
            .annotate({ description: 'HarfBuzz buffer flags (BOT, EOT, PRODUCE_UNSAFE_TO_CONCAT), none when absent' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    });
    static readonly enclosing = (boxes: readonly (typeof Bounds.Type)[]) =>
        Option.map(Option.liftPredicate(boxes, Array.isReadonlyArrayNonEmpty), (enclosed) => ({
            minX: Math.min(...enclosed.map(Struct.get('minX'))),
            minY: Math.min(...enclosed.map(Struct.get('minY'))),
            maxX: Math.max(...enclosed.map(Struct.get('maxX'))),
            maxY: Math.max(...enclosed.map(Struct.get('maxY'))),
        }));
    static readonly converted = (run: typeof ShapedText.Run.Type, settings: Settings): typeof ShapedText.Run.Type => {
        const offset = (at: number) => (at > run.text.length ? at : settings.digits(run.text.slice(0, at)).length);
        return {
            ...run,
            text: settings.digits(run.text),
            item: Option.map(run.item, ({ start, end }) => Span.make({ start: offset(start), end: offset(end) })),
            features: run.features.map(({ tag, value, start, end }) => new Feature(tag, value, offset(start), offset(end))),
        };
    };
    static readonly of = ({ font, em }: Pick<Face, 'font' | 'em'>, run: typeof ShapedText.Run.Type, settings = Settings.make({})) => {
        const { text, item, features, language, script, direction, clusterLevel, flags } = ShapedText.converted(run, settings);
        const buffer = new Buffer();
        const { start, length } = Option.getOrElse(item, () => Span.make({ start: 0, end: text.length }));
        buffer.addText(text, start, length);
        buffer.setClusterLevel(ClusterLevel[clusterLevel]);
        buffer.setFlags(Math.sumPrecise(Array.map(flags, (flag) => BufferFlag[flag])));
        Option.match(language, { onNone: Function.constVoid, onSome: (tag) => buffer.setLanguage(otTagToLanguage(tag)) });
        Option.match(script, { onNone: Function.constVoid, onSome: (tag) => buffer.setScript(otTagToScript(tag)) });
        Option.match(direction, { onNone: Function.constVoid, onSome: (key) => buffer.setDirection(Direction[key]) });
        buffer.guessSegmentProperties();
        const characters = buffer.getGlyphInfos();
        shape(font, buffer, [...Record.collect(settings.features, (tag, value) => new Feature(tag, value)), ...features]);
        const infos = buffer.getGlyphInfos();
        const unmapped = new Set(infos.filter(({ codepoint }) => codepoint === 0).map(Struct.get('cluster')));
        const shaped = Array.zipWith(infos, buffer.getGlyphPositions(), ({ codepoint, cluster, flags: glyphFlags }, position) => ({
            glyph: codepoint,
            cluster,
            ...Record.map(position, em),
            extents: Option.map(Option.fromUndefinedOr(font.glyphExtents(codepoint)), Record.map(em)),
            scale: 1,
            class: Array.getUnsafe(GlyphClasses.literals, font.face.getGlyphClass(codepoint)),
            flags: GlyphFlags.literals.filter((flag) => (glyphFlags & GlyphFlag[flag]) !== 0),
        }));
        const bases = Array.sortWith(
            shaped.filter((glyph) => glyph.class !== 'MARK'),
            Struct.get('cluster'),
            Order.flip(Order.Number),
        );
        const center = ({ yOffset, extents }: (typeof shaped)[number]) => Option.map(extents, ({ yBearing, height }) => yOffset + yBearing + height / 2);
        const below = (mark: (typeof shaped)[number]) =>
            Option.exists(
                Option.all([
                    center(mark),
                    Option.flatMap(
                        Array.findFirst(bases, ({ cluster }) => cluster <= mark.cluster),
                        center,
                    ),
                ]),
                ([own, base]) => own < base,
            );
        return ShapedText.make({
            text,
            glyphs: shaped.map((glyph) => (glyph.class === 'MARK' ? { ...glyph, xOffset: glyph.xOffset + settings.marks.x, yOffset: glyph.yOffset + (below(glyph) ? -settings.marks.y : settings.marks.y) } : glyph)),
            missing: Array.dedupe(characters.filter(({ cluster }) => unmapped.has(cluster)).map(({ codepoint }) => String.fromCodePoint(codepoint))),
        });
    };

    get advance() {
        return Math.hypot(Math.sumPrecise(this.glyphs.map(Struct.get('xAdvance'))), Math.sumPrecise(this.glyphs.map(Struct.get('yAdvance'))));
    }
    get extents(): Option.Option<typeof Bounds.Type> {
        const [, boxes] = Array.mapAccum(this.glyphs, { x: 0, y: 0 }, (pen, { xAdvance, yAdvance, xOffset, yOffset, extents }) => [
            { x: pen.x + xAdvance, y: pen.y + yAdvance },
            Option.map(
                Option.filter(extents, ({ width, height }) => width !== 0 || height !== 0),
                ({ xBearing, yBearing, width, height }) => {
                    const [left, top] = [pen.x + xOffset + xBearing, pen.y + yOffset + yBearing];
                    return { minX: Math.min(left, left + width), minY: top + height, maxX: Math.max(left, left + width), maxY: top };
                },
            ),
        ]);
        return ShapedText.enclosing(Array.getSomes(boxes));
    }

    box({ x, y }: { readonly x: number; readonly y: number }) {
        return Option.map(this.extents, ({ minX, minY, maxX, maxY }) => ({ minX: minX - x, minY: minY - y, maxX: maxX + x, maxY: maxY + y }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ShapedText };
