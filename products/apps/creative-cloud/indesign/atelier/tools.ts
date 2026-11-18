import { Adjust, Called, Inspection, Search, Slice, Value } from '@rasm/indesign';
import { Failure, Reference, Specifier, Written } from '@rasm/object-model';
import { Effect, Option, Record, Schema } from 'effect';
import { Tool, Toolkit } from 'effect/ai';

// --- [MODELS] --------------------------------------------------------------------------

class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.Path.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.Every,
}) {}
class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')({ target: Specifier.Path, values: Value.Fields }) {
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
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Inspection.Issues }));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const InDesign = Toolkit.make(
    Tool.make('read', { description: 'Reads object fields and typed view, or collection members, / for application. File fields read as paths', parameters: Inspect, success: Inspection, failure: Failure }).annotate(Tool.Readonly, true),
    Tool.make('change', { description: 'Writes object fields in one undo step, failures name every refused field by path', parameters: Change, success: Written, failure: Failure }),
    Tool.make('call', { description: 'Calls object or collection method in one undo step, atelier://object-model/indesign/{name} lists its parameters', parameters: Call, success: Called, failure: Failure }),
    Tool.make('find_change', { description: 'Finds or changes matches in one undo step and restores search preferences, values take change forms', parameters: FindChange, success: FindChange.Result, failure: Failure }),
    Tool.make('adjust', { description: 'Adds kerning, tracking, baseline shift or mark offsets to target text in one undo step, / for text defaults', parameters: Adjust, success: Adjust.Result, failure: Failure }),
    Tool.make('preflight', { description: 'Reads preflight rows per document, the finished live results when no profile is given, each read waiting for the preflight process every live check restarts', parameters: Preflight, success: Preflight.Result, failure: Failure }).annotate(Tool.Readonly, true),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { InDesign };
