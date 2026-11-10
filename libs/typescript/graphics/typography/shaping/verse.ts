import { Array, Effect, Schema, Struct } from 'effect';
import { Settings } from '#/settings';
import { Justification } from '#/shaping/justification';

// --- [MODELS] --------------------------------------------------------------------------

const Length = Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0));
class Verse extends Schema.Class<Verse, { readonly brand: unique symbol }>('Verse')({
    width: Schema.Finite.annotate({ description: 'Hemistich width in em, the widest hemistich at desired spacing within the room the setting leaves' }),
    hemistichs: Schema.NonEmptyArray(
        Schema.Struct({
            line: Justification,
            row: Schema.Int.annotate({ description: 'Row the hemistich sets on, Columns pairs sharing one baseline' }),
            indents: Schema.Struct(Struct.Record(['start', 'end'], Schema.Finite)).annotate({ description: 'Em from the measure start and end edges' }),
        }),
    ),
}) {
    static readonly Setting = Schema.TaggedUnion({
        Columns: { gap: Length.annotate({ description: 'Em between the hemistich columns' }) },
        Staggered: {
            indent: Length.annotate({ description: 'Em a hemistich indents from its own side, the other side taking the measure less width and indent' }),
            sides: Schema.NonEmptyArray(Schema.Literals(['start', 'center', 'end']))
                .annotate({ description: 'Side each hemistich takes in stanza order, cycling, center ignoring indent' })
                .pipe(Schema.withConstructorDefault(Effect.succeed(['start', 'end'] as const))),
        },
    }).annotate({ description: 'Centered hemistich column pairs, even hemistichs at the start side, or one hemistich per row at cycling sides' });
    static readonly of = (hemistichs: Array.NonEmptyReadonlyArray<Parameters<typeof Justification.of>[0]>, measure: number, setting: typeof Verse.Setting.Type) => {
        const widest = Math.max(...Array.map(hemistichs, Justification.natural));
        const { width, at } = Verse.Setting.match(setting, {
            Columns: ({ gap }) => {
                const fitted = Math.min(widest, (measure - gap) / 2);
                const margin = (measure - 2 * fitted - gap) / 2;
                return { width: fitted, at: (index: number) => ({ row: Math.floor(index / 2), indents: index % 2 === 0 ? { start: margin, end: measure - margin - fitted } : { start: margin + fitted + gap, end: margin } }) };
            },
            Staggered: ({ indent, sides }) => {
                const fitted = Math.min(widest, measure - (sides.every((side) => side === 'center') ? 0 : indent));
                const far = measure - fitted - indent;
                const indents = { start: { start: indent, end: far }, center: { start: (measure - fitted) / 2, end: (measure - fitted) / 2 }, end: { start: far, end: indent } };
                return { width: fitted, at: (index: number) => ({ row: index, indents: indents[Array.getUnsafe(sides, index % sides.length)] }) };
            },
        });
        return Verse.make({
            width,
            hemistichs: Array.map(hemistichs, (runs, index) => ({
                line: Justification.of(
                    Array.map(runs, (run) => ({ ...run, settings: Settings.make({ ...run.settings, alignment: Settings.fields.alignment.schema.cases.Justified.make({}) }) })),
                    width,
                ),
                ...at(index),
            })),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Verse };
