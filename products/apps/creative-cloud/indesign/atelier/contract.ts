import { HostSession, Requests, request, Written } from '@rasm/atelier-protocol';
import { Called, Inspection, MenuActions, Search, Slice, Value } from '@rasm/indesign';
import { Reference, Specifier } from '@rasm/object-model';
import { Settings, Source } from '@rasm/typography';
import { Effect, Option, Record, Schema, Struct } from 'effect';
import { RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLURAL = new Intl.PluralRules('en');

// --- [MODELS] --------------------------------------------------------------------------

// --- [REQUESTS]

class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.Path.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.Every,
}) {
    static readonly Result = Inspection;
}
class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')({ target: Specifier.Path, values: Value.Fields }) {
    static readonly Result = Written;

    get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${this.target.leaf}`;
    }
}
class Call extends Schema.Class<Call, { readonly brand: unique symbol }>('Call')({
    target: Specifier.Path.annotate({ description: 'Object, or owner of collection' }),
    collection: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Collection field of target, its method called in place of the object method' })),
    method: Schema.String.annotate({ description: 'Method name (add, remove, place, duplicate, move, save, close, exportFile)' }),
    arguments: Schema.Array(Value)
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])))
        .annotate({ description: 'Positional arguments' }),
    properties: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Fields written onto returned object' })),
}) {
    static readonly Result = Called;

    get label(): string {
        return `Call ${[...Option.toArray(this.collection), this.method].join(' ')} of ${this.target.leaf}`;
    }
}
class FindChange extends Schema.Class<FindChange, { readonly brand: unique symbol }>('FindChange')({
    mode: Search,
    find: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find preference fields (pattern, format criteria)' })),
    change: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Change preference fields for every match, absent to find only' })),
    options: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find/change option fields' })),
    scope: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Document, story, text, or page item to search, current document when absent' })),
    slice: Slice.Every,
}) {
    static readonly Result = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Absent without change' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Schema.String, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Matches with text before changes, empty for Color' }),
    });

    get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}`;
    }
}
class FontSource extends Schema.Class<FontSource, { readonly brand: unique symbol }>('FontSource')({
    name: Schema.String.annotate({ description: 'Font, PostScript, or full name, family name selects its Regular or first face' }),
    text: Schema.String.annotate({ description: 'Subset characters for unreadable font files' }),
    file: Schema.String,
}) {
    static readonly Result = Schema.Struct({ font: Inspection.View.cases.Font, source: Schema.OptionFromOptionalKey(Source.mapFields(Struct.omit(['data'])).mapFields(Struct.assign({ file: Schema.String.annotate({ description: 'Installed face file, or subset written to the requested file' }) }))) });
}
class Invoke extends Schema.Class<Invoke, { readonly brand: unique symbol }>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }) {
    static readonly Result = MenuActions.value;

    get label(): string {
        return `Invoke ${this.action}`;
    }
}
class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Area in any case' })) }) {
    static readonly Result = MenuActions;
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Inspection.Issues }));
}

// --- [TYPESETTING]

const Run = Schema.Struct({ index: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Insertion point index in target before run' }), length: Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Character count of run' }) });
class Adjust extends Schema.Class<Adjust, { readonly brand: unique symbol }>('Adjust')({
    target: Schema.String.annotate({ description: 'Text or container specifier, / for text defaults' }),
    deltas: Schema.Struct({
        kerning: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em after last base letter, before its marks, negative tightens' })),
        tracking: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em, negative tightens' })),
        baselineShift: Schema.optionalKey(Schema.Finite.annotate({ description: 'Em of each style run point size, up positive' })),
        marks: Schema.optionalKey(Settings.fields.marks.schema),
    })
        .check(Schema.isMinProperties(1))
        .annotate({ description: 'Added to each style run value' }),
}) {
    static readonly Result = Written;

    get label(): string {
        return `Adjust ${Struct.keys(this.deltas).join(', ')} of ${this.target}`;
    }
}
class Kerns extends Schema.Class<Kerns, { readonly brand: unique symbol }>('Kerns')({
    target: Adjust.fields.target,
    restore: Schema.OptionFromOptionalKey(Run.annotate({ description: 'Characters returned to metrics kerning before rows write, manual kerning included' })),
    kerns: Schema.Array(Run.mapFields(Struct.assign({ kerning: Schema.Finite.annotate({ description: 'Em at run index, negative tightens, 0 writes none' }), baselineShift: Schema.Finite.annotate({ description: 'Em of run point size, up positive, 0 writes none' }) }))).annotate({
        description: 'Written at insertion points and their runs, rows at nonzero manual kerning kept',
    }),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ written: Schema.Int.annotate({ description: 'Row count' }) }));

    get label(): string {
        const runs: Readonly<Partial<Record<Intl.LDMLPluralRule, string>>> & { readonly other: string } = { one: 'run', other: 'runs' };
        return `${Option.isSome(this.restore) ? 'Restore metrics kerning, kern' : 'Kern'} ${this.kerns.length} ${runs[PLURAL.select(this.kerns.length)] ?? runs.other} of ${this.target}`;
    }
}
class Runs extends Schema.Class<Runs, { readonly brand: unique symbol }>('Runs')({ target: Adjust.fields.target }) {
    static readonly Result = Reference.mapFields(
        Struct.assign({
            start: Schema.Int.annotate({ description: 'Target offset in container text' }),
            length: Schema.Int.annotate({ description: 'Target length' }),
            runs: Schema.Array(
                Schema.Struct({ text: Schema.String.annotate({ description: 'Contents, one line feed per special character' }), font: Schema.String.annotate({ description: 'Applied font name' }), axes: Schema.Array(Schema.Finite).annotate({ description: 'Design axis values in fvar order, empty for static faces' }) }),
            ).annotate({ description: 'Style ranges of container text in order, one empty range of text defaults' }),
        }),
    ).annotate({ description: 'Container holding target text or text defaults, with its style ranges' });
}
class Typeset extends Schema.Class<Typeset, { readonly brand: unique symbol }>('Typeset')({ target: Adjust.fields.target, settings: Schema.OptionFromOptionalKey(Settings.Partial.check(Schema.isMinProperties(1))) }) {
    static readonly Result = Settings.annotate({ description: 'Target settings, first run of mixed text' });

    get label(): string {
        return Option.match(this.settings, { onNone: () => `Read settings of ${this.target}`, onSome: (settings) => `Set ${Struct.keys(settings).join(', ')} of ${this.target}` });
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const InDesign = Requests.add(request('inspect', Inspect), request('change', Change), request('call', Call), request('find_change', FindChange), request('font', FontSource), request('menu', Menu), request('invoke', Invoke), request('preflight', Preflight))
    .merge(RpcGroup.make(request('adjust', Adjust), request('kerns', Kerns), request('runs', Runs), request('typeset', Typeset)))
    .middleware(HostSession);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Adjust, InDesign, Kerns, Typeset };
