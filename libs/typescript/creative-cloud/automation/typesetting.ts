import { Settings } from '@rasm/typography';
import { Option, Schema, Struct } from 'effect';
import { RpcGroup } from 'effect/rpc';
import { Reference, request } from '#/requests';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLURAL = new Intl.PluralRules('en');

// --- [MODELS] --------------------------------------------------------------------------

const Text = Schema.String.annotate({ description: 'Text or container specifier' });
const Run = Schema.Struct({
    index: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Insertion point index in target before run' }),
    length: Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Character count of run' }),
});
class Adjust extends Schema.Class<Adjust, { readonly brand: unique symbol }>('Adjust')({
    target: Text.annotate({ description: 'Text or container specifier, / for text defaults' }),
    deltas: Schema.Struct({
        kerning: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em after last base letter, before its marks, negative tightens' })),
        tracking: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em, negative tightens' })),
        baselineShift: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em of each style run point size, up positive' })),
        marks: Schema.optionalKey(Settings.Marks),
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
    restore: Schema.OptionFromOptionalKey(Run.annotate({ description: 'Characters returned to metrics kerning before rows write, manual kerning included' })),
    kerns: Schema.Array(
        Run.mapFields(
            Struct.assign({
                kerning: Schema.Finite.annotate({ description: 'Em at run index, negative tightens, 0 writes none' }),
                baselineShift: Schema.Finite.annotate({ description: 'Em of run point size, up positive, 0 writes none' }),
            }),
        ),
    ).annotate({ description: 'Written at insertion points and their runs, rows at nonzero manual kerning kept' }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Int.annotate({ description: 'Row count' }) }));

    get label(): string {
        const runs: { readonly [Rule in Intl.LDMLPluralRule]?: string } & { readonly other: string } = { one: 'run', other: 'runs' };
        return `${Option.isSome(this.restore) ? 'Restore metrics kerning, kern' : 'Kern'} ${this.kerns.length} ${runs[PLURAL.select(this.kerns.length)] ?? runs.other} of ${this.target}`;
    }
}
class Runs extends Schema.Class<Runs, { readonly brand: unique symbol }>('Runs')({ target: Adjust.fields.target }) {
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
            ).annotate({ description: 'Style ranges of container text in order, one empty range of text defaults' }),
        }),
    ).annotate({ description: 'Container holding target text or text defaults, with its style ranges' });
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
