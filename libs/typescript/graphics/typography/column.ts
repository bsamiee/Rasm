import { Array, Option, Schema, Struct } from 'effect';
import type { Settings } from '#/settings';
import type { ShapedText } from '#/shaped-text';

// --- [TYPES] ---------------------------------------------------------------------------

interface Paragraph {
    readonly lines: readonly Pick<ShapedText, 'ink'>[];
    readonly settings: Pick<Settings, 'leading' | 'lineHeight'>;
}

// --- [MODELS] --------------------------------------------------------------------------

class Column extends Schema.Class<Column, { readonly brand: unique symbol }>('Column')({
    baselines: Schema.Array(Schema.Array(Schema.Finite)).annotate({ description: 'Baseline depth in em below the column top per paragraph line, the first at its ink ascent and each next one line height and paragraph space below' }),
    height: Schema.Finite.annotate({ description: 'Depth in em of the last line ink bottom, paragraph space dropped at both column edges' }),
}) {
    static readonly of = (paragraphs: readonly Paragraph[]) => {
        const [bottom, baselines] = Array.mapAccum(paragraphs, Option.none<{ readonly baseline: number; readonly descent: number; readonly space: number }>(), (above, { lines, settings }) => {
            const [last, placed] = Array.mapAccum(
                lines,
                Option.map(above, (edge) => ({ ...edge, space: edge.space + settings.leading.before })),
                (previous, { ink }) => {
                    const [ascent, descent] = Option.match(ink, { onNone: () => [0, 0] as const, onSome: ({ maxY, minY }) => [maxY, -minY] as const });
                    const baseline = Option.match(previous, { onNone: () => ascent, onSome: (edge) => edge.baseline + edge.space + settings.lineHeight(1, { descent: edge.descent, ascent }) });
                    return [Option.some({ baseline, descent, space: 0 }), baseline] as const;
                },
            );
            return [Option.map(last, (edge) => ({ ...edge, space: edge.space + settings.leading.after })), placed] as const;
        });
        return Column.make({ baselines, height: Option.match(bottom, { onNone: () => 0, onSome: ({ baseline, descent }) => baseline + descent }) });
    };
    static readonly rows = (rows: readonly Array.NonEmptyReadonlyArray<Column>[]) => {
        const [height, tops] = Array.mapAccum(rows, 0, (top, cells) => {
            const tallest = Math.max(...cells.map(Struct.get('height')));
            return [top + tallest, { top, height: tallest }] as const;
        });
        return { rows: tops, height };
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Column };
