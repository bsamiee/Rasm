import { Array, Option, Schema, Struct } from 'effect';
import type { Settings } from '#/settings.ts';
import type { ShapedText } from '#/shaping/shaped-text.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Paragraph {
    readonly lines: readonly Pick<ShapedText, 'extents'>[];
    readonly settings: Pick<Settings, 'leading' | 'lineHeight'>;
}

// --- [MODELS] --------------------------------------------------------------------------

class TextColumn extends Schema.Class<TextColumn, { readonly brand: unique symbol }>('TextColumn')({
    baselines: Schema.Array(Schema.Array(Schema.Finite)).annotate({ description: 'Baseline depth in em below the column top per paragraph line, the first at its glyph extents top and each next one line height and paragraph space below' }),
    height: Schema.Finite.annotate({ description: 'Depth in em of the last line glyph extents bottom, paragraph space dropped at both column edges' }),
}) {
    static readonly of = (paragraphs: readonly Paragraph[]) => {
        const [bottom, baselines] = Array.mapAccum(paragraphs, Option.none<{ readonly baseline: number; readonly descent: number; readonly space: number }>(), (above, { lines, settings }) => {
            const [last, placed] = Array.mapAccum(
                lines,
                Option.map(above, (edge) => ({ ...edge, space: edge.space + settings.leading.before })),
                (previous, { extents }) => {
                    const [ascent, descent] = Option.match(extents, { onNone: () => [0, 0] as const, onSome: ({ maxY, minY }) => [maxY, -minY] as const });
                    const baseline = Option.match(previous, { onNone: () => ascent, onSome: (edge) => edge.baseline + edge.space + settings.lineHeight(1, { descent: edge.descent, ascent }) });
                    return [Option.some({ baseline, descent, space: 0 }), baseline] as const;
                },
            );
            return [Option.map(last, (edge) => ({ ...edge, space: edge.space + settings.leading.after })), placed] as const;
        });
        return TextColumn.make({ baselines, height: Option.match(bottom, { onNone: () => 0, onSome: ({ baseline, descent }) => baseline + descent }) });
    };
    static readonly rows = (rows: readonly Array.NonEmptyReadonlyArray<TextColumn>[]) => {
        const [height, tops] = Array.mapAccum(rows, 0, (top, cells) => {
            const tallest = Math.max(...cells.map(Struct.get('height')));
            return [top + tallest, { top, height: tallest }] as const;
        });
        return { rows: tops, height };
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { TextColumn };
