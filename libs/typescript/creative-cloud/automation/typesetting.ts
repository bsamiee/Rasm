import { Settings } from '@rasm/typography/settings';
import { Option, Schema, Struct } from 'effect';
import { RpcGroup } from 'effect/rpc';
import { Reference, request } from '#/requests';

// --- [MODELS] --------------------------------------------------------------------------

const Text = Schema.String.annotate({ description: 'Text or container specifier' });
class Adjust extends Schema.Class<Adjust, { readonly brand: unique symbol }>('Adjust')({
    target: Text.annotate({ description: 'Text or container specifier, / for text defaults' }),
    deltas: Schema.Struct({
        kerning: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em after last base letter, before its marks, negative tightens' })),
        tracking: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em, negative tightens' })),
        baselineShift: Schema.optionalKey(Schema.Finite.annotate({ description: 'Points, up positive' })),
        marks: Schema.optionalKey(Settings.fields.marks.schema),
    })
        .check(Schema.isMinProperties(1))
        .annotate({ description: 'Added to each style run value' }),
}) {
    static readonly Result = Reference.Written;

    get label(): string {
        return `Adjust ${Struct.keys(this.deltas).join(', ')} of ${this.target}`;
    }
}
class Kerns extends Schema.Class<Kerns, { readonly brand: unique symbol }>('Kerns')({
    target: Text,
    kerns: Schema.Array(
        Schema.Struct({
            index: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Insertion point index in target' }),
            kerning: Schema.Finite.annotate({ description: 'Em, negative tightens' }),
        }),
    ).annotate({ description: 'Written at insertion points, nonzero manual kerning kept' }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Int.annotate({ description: 'Insertion point count' }) }));

    get label(): string {
        return `Kern ${this.kerns.length} insertion points of ${this.target}`;
    }
}
class Runs extends Schema.Class<Runs, { readonly brand: unique symbol }>('Runs')({ target: Text }) {
    static readonly Result = Reference.mapFields(
        Struct.assign({
            start: Schema.Int.annotate({ description: 'Target offset in container text' }),
            length: Schema.Int.annotate({ description: 'Target length' }),
            runs: Schema.Array(
                Schema.Struct({
                    text: Schema.String.annotate({ description: 'Contents, one line feed per special character' }),
                    font: Schema.String.annotate({ description: 'Applied font name' }),
                    axes: Schema.Array(Schema.Finite).annotate({ description: 'Design axis values in fvar order, empty for static faces' }),
                }),
            ).annotate({ description: 'Style ranges of container text in order' }),
        }),
    ).annotate({ description: 'Container holding target text, with its style ranges' });
}
class Typeset extends Schema.Class<Typeset, { readonly brand: unique symbol }>('Typeset')({
    target: Adjust.fields.target,
    settings: Schema.OptionFromOptionalKey(Settings.Partial.check(Schema.isMinProperties(1))),
}) {
    static readonly Result = Settings.annotate({ description: 'Target settings, first run of mixed text' });

    get label(): string {
        return Option.match(this.settings, { onNone: () => `Read settings of ${this.target}`, onSome: (settings) => `Set ${Struct.keys(settings).join(', ')} of ${this.target}` });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Typesetting = RpcGroup.make(request('adjust', Adjust), request('kerns', Kerns), request('runs', Runs), request('typeset', Typeset));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Adjust, Kerns, Runs, Typeset, Typesetting };
