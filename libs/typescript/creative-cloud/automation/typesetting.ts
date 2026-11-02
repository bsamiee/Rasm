import { Settings } from '@rasm/typography/settings';
import { Array, Match, Option, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { error, Reference } from '#/requests';

// --- [MODELS] --------------------------------------------------------------------------

class Adjust extends Schema.Class<Adjust, { readonly brand: unique symbol }>('Adjust')({
    target: Schema.String.annotate({ description: 'Text, story, or text frame specifier, / for text defaults' }),
    deltas: Schema.NonEmptyArray(
        Schema.TaggedUnion({
            Kerning: { em: Schema.Finite.annotate({ description: 'Kerning delta in em after last base letter, before its marks' }) },
            Tracking: { em: Schema.Finite.annotate({ description: 'Tracking delta in em' }) },
            BaselineShift: { points: Schema.Finite.annotate({ description: 'Baseline shift delta in points, up positive' }) },
            Marks: Struct.Record(['x', 'y'], Schema.Finite.annotate({ description: 'Mark offset delta in em, y up' })),
        }),
    ).annotate({ description: 'Deltas added to current values in order' }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ applied: Schema.Array(Schema.String) }));

    get label(): string {
        return `Adjust ${Array.dedupe(Array.map(this.deltas, Struct.get('_tag'))).join(', ')} of ${this.target}`;
    }
}
class Kerns extends Schema.Class<Kerns, { readonly brand: unique symbol }>('Kerns')({
    target: Adjust.fields.target,
    kerns: Schema.Union([
        Schema.Literal('clear').annotate({ description: 'Metric kerning restored across target' }),
        Schema.Array(
            Schema.Struct({
                index: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Insertion point index within target' }),
                em: Schema.Finite.annotate({ description: 'Kerning in em' }),
            }),
        ).annotate({ description: 'Kerning written at insertion points, manual kerning kept' }),
    ]),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Int }));

    get label(): string {
        return Match.value(this.kerns).pipe(
            Match.when('clear', () => `Clear kerning of ${this.target}`),
            Match.orElse((kerns) => `Kern ${kerns.length} insertion points of ${this.target}`),
        );
    }
}
class Typeset extends Schema.Class<Typeset, { readonly brand: unique symbol }>('Typeset')({
    target: Adjust.fields.target,
    settings: Schema.OptionFromOptionalKey(
        Settings.mapFields((fields) => Struct.map(Struct.evolve(fields, { direction: (field) => Schema.OptionFromNullOr(field.schema.from.schema), numberingSystem: (field) => Schema.OptionFromNullOr(field.schema.from.schema) }), Schema.optionalKey))
            .check(Schema.isMinProperties(1))
            .annotate({
                description: 'Fields written before read, absent reads alone, null clears direction or numbering system',
            }),
    ),
}) {
    static readonly Result = Settings;

    get label(): string {
        return Option.match(this.settings, { onNone: () => `Read settings of ${this.target}`, onSome: (given) => `Set ${Struct.keys(given).join(', ')} of ${this.target}` });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Typesetting = RpcGroup.make(Rpc.make('adjust', { payload: Adjust, success: Adjust.Result, error }), Rpc.make('kerns', { payload: Kerns, success: Kerns.Result, error }), Rpc.make('typeset', { payload: Typeset, success: Typeset.Result, error }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Adjust, Kerns, Typeset, Typesetting };
