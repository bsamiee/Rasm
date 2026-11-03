import { Digits } from '@rasm/search/digits';
import { Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Tag } from '#/tag';

// --- [MODELS] --------------------------------------------------------------------------

const Range = (value: number, description: string) => {
    const range = Schema.Struct(Struct.Record(['min', 'desired', 'max'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(value)))))
        .check(Schema.makeFilter(({ min, desired, max }) => min <= desired && desired <= max, { description: 'min <= desired <= max' }))
        .annotate({ description });
    return range.pipe(Schema.withConstructorDefault(Effect.sync(() => range.make({}))));
};
const Direction = Schema.Literals(['LTR', 'RTL']).annotate({ description: 'Paragraph direction as HarfBuzz direction key, the bidi base level' });
const Elongation = {
    lower: Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Lower elongation bound in nuqta, each chosen place stretching at least this far' }),
    upper: Schema.Int.annotate({ description: 'Upper elongation bound in nuqta, each chosen place stretching at most this far' }),
    fractional: Schema.Boolean.annotate({ description: 'Elongation below one tatweel width' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
};
const Kashida = Schema.TaggedUnion({ None: {}, Tatweel: Elongation, Stretched: Elongation })
    .check(Schema.makeFilter((kashida) => kashida._tag === 'None' || kashida.lower <= kashida.upper, { description: 'lower <= upper' }))
    .annotate({ description: 'Elongation by tatweel characters or by jalt alternates then tatweels' });
const Feature = Schema.Int.check(Schema.isGreaterThanOrEqualTo(0));
const Marks = Schema.Struct(Struct.Record(['x', 'y'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(0))))).annotate({ description: 'Mark offset in em, y up' });
const Spacing = Schema.Struct({
    word: Range(1, 'Ratios of space advance'),
    letter: Range(0, 'Ratios of space advance added between letters'),
    glyph: Range(1, 'Ratios of glyph width'),
});
class Settings extends Schema.Class<Settings, { readonly brand: unique symbol }>('Settings')(
    {
        direction: Schema.OptionFromOptionalKey(Direction).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        numberingSystem: Schema.OptionFromOptionalKey(Digits.System.annotate({ description: 'Digits shaped text takes, source digits when unset' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        kashida: Kashida.pipe(Schema.withConstructorDefault(Effect.sync(() => Kashida.cases.None.make({})))),
        marks: Marks.pipe(Schema.withConstructorDefault(Effect.sync(() => Marks.make({})))),
        features: Schema.Record(Tag, Feature)
            .annotate({ description: 'OpenType feature values by tag, 0 disabling a default feature, above 1 selecting that alternate' })
            .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        spacing: Spacing.pipe(Schema.withConstructorDefault(Effect.sync(() => Spacing.make({})))),
    },
    { description: 'Typesetting of one paragraph or run' },
) {
    static readonly Partial = Settings.mapFields((fields) =>
        Struct.map(Schema.optionalKey)({
            ...fields,
            direction: Schema.OptionFromNullOr(Direction),
            numberingSystem: Schema.OptionFromNullOr(Digits.System),
            marks: Schema.Struct(Struct.map(Marks.fields, Schema.optionalKey)),
            features: Schema.Record(Tag, Schema.NullOr(Feature)),
            spacing: Schema.Struct(Record.map(Spacing.fields, (range) => Schema.optionalKey(Schema.Struct(Struct.map(range.schema.fields, Schema.optionalKey))))),
        }),
    ).annotate({ description: 'Absent keys, mark axes, spacing bounds and feature tags stay unchanged, null unsets direction, numbering system or a feature tag' });
    static readonly merge = (settings: Settings, change: typeof Settings.Partial.Type) =>
        Settings.makeEffect({
            ...settings,
            ...Struct.evolve(change, {
                marks: (marks) => ({ ...settings.marks, ...marks }),
                features: (features) => Record.filter({ ...settings.features, ...features }, Predicate.isNotNull),
                spacing: (spacing = {}) => Record.map(settings.spacing, (range, axis) => ({ ...range, ...spacing[axis] })),
            }),
        }).pipe(Effect.mapError((issue) => new Schema.SchemaError(issue)));

    digits(text: string) {
        return Option.match(Option.flatMap(this.numberingSystem, Digits.of), { onNone: () => text, onSome: (digits) => digits.format(text) });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Settings };
