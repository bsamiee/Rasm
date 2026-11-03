import { Settings } from '@rasm/typography/settings';
import { Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { error, Reference } from '#/requests';

// --- [MODELS] --------------------------------------------------------------------------

class Adjust extends Schema.Class<Adjust, { readonly brand: unique symbol }>('Adjust')({
    target: Schema.String.annotate({ description: 'Text or container specifier, / for text defaults' }),
    deltas: Schema.Struct({
        kerning: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em after last base letter, before its marks, negative tightens' })),
        tracking: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em, negative tightens' })),
        baselineShift: Schema.optionalKey(Schema.Finite.annotate({ description: 'Points, up positive' })),
        marks: Schema.optionalKey(
            Schema.Struct({
                x: Schema.Finite.annotate({ description: 'Horizontal offset in em' }),
                y: Schema.Finite.annotate({ description: 'Vertical offset in em, away from base positive' }),
            }),
        ),
    })
        .check(Schema.isMinProperties(1))
        .annotate({ description: 'Added to each style run value' }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String).annotate({ description: 'Member names' }) }));

    get label(): string {
        return `Adjust ${Struct.keys(this.deltas).join(', ')} of ${this.target}`;
    }
}
class Kerns extends Schema.Class<Kerns, { readonly brand: unique symbol }>('Kerns')({
    target: Adjust.fields.target,
    kerns: Schema.Array(
        Schema.Struct({
            index: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Insertion point index in target' }),
            em: Schema.Finite.annotate({ description: 'Negative tightens' }),
        }),
    ).annotate({ description: 'Written at insertion points, nonzero manual kerning kept' }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Int.annotate({ description: 'Insertion point count' }) }));

    get label(): string {
        return `Kern ${this.kerns.length} insertion points of ${this.target}`;
    }
}
class Typeset extends Schema.Class<Typeset, { readonly brand: unique symbol }>('Typeset')({
    target: Adjust.fields.target,
    settings: Schema.OptionFromOptionalKey(Settings.Partial.check(Schema.isMinProperties(1))),
}) {
    static readonly Result = Settings.annotate({ description: 'Target settings, first run of mixed text' });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Typesetting = RpcGroup.make(Rpc.make('adjust', { payload: Adjust, success: Adjust.Result, error }), Rpc.make('kerns', { payload: Kerns, success: Kerns.Result, error }), Rpc.make('typeset', { payload: Typeset, success: Typeset.Result, error }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Adjust, Kerns, Typeset, Typesetting };
