import type { Application, InDesignDOMObject, Text } from 'adobe:indesign';
import { Field, HostError, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Cache, Effect, Match, Option, pipe, Record, Result, Schema, Struct } from 'effect';
import { DocumentView, LayerView, LinkView, PageView, SpreadView, SwatchView } from '#/documents/document';
import { StoryView, StyleView, TextView } from '#/documents/text';
import { unavailable, undeclared } from '#/dom/dictionary';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';
import { read, Slice } from '#/dom/value';
import { FontView } from '#/requests/font';
import { PreflightProcessView } from '#/requests/preflight';

// --- [MODELS] --------------------------------------------------------------------------

class ApplicationView extends Schema.TaggedClass<ApplicationView, { readonly brand: unique symbol }>()('Application', {
    ...Reference.fields,
    version: Schema.String,
    tool: Schema.String,
    panels: Schema.Array(Schema.String),
    documents: Schema.Array(DocumentView.Row),
    current: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Schema.String,
            window: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Absent for a document open without window' })),
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Schema.String), overrides: Schema.OptionFromOptionalKey(TextView.fields.overrides) }))),
            overset: Schema.Array(Schema.String),
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ).annotate({ description: 'Single open document, else document of the active window' }),
}) {
    static readonly owns = (model: typeof Model.Service) => model.owning<Application>('menuActions');
    static readonly of = Effect.fn('ApplicationView.of')(function* (app: Application) {
        const { owning } = yield* Model;
        const overridable = owning<Text>('textHasOverrides');
        const rows = yield* Effect.forEach(app.documents.everyItem().getElements(), DocumentView.row);
        const opened = yield* Effect.option(Model.document);
        const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
            pipe(
                Option.liftPredicate(item, overridable),
                Option.map(TextView.overrides),
                Effect.transposeOption,
                Effect.map((overrides) => ({
                    ...reference(item),
                    contents: Option.fromUndefinedOr('contents' in item ? item.contents : undefined),
                    parentStory: 'parentStory' in item ? Option.some(item.parentStory.toSpecifier()) : Option.none(),
                    overrides,
                })),
            ),
        );
        const panels = Array.getSomes(
            Array.makeBy(app.panels.length, (index) =>
                Option.map(
                    Option.filter(Option.fromUndefinedOr(app.panels.item(index)), ({ visible }) => visible),
                    Struct.get('name'),
                ),
            ),
        );
        const window = Option.liftThrowable(() => app.activeWindow.toSpecifier())();
        return yield* read(ApplicationView, app.toSpecifier(), () => ({
            _tag: 'Application',
            ...reference(app),
            version: app.version,
            tool: app.toolBoxTools.currentTool,
            panels,
            documents: rows,
            current: Option.map(opened, (document) => ({
                document: document.toSpecifier(),
                window,
                selection,
                overset: DocumentView.overset(document),
                undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
            })),
        }));
    });
}
const View = Schema.Union([ApplicationView, DocumentView, PageView, SpreadView, LayerView, StyleView, SwatchView, LinkView, StoryView, TextView, FontView, PreflightProcessView]);
const Read = Field.mapFields(Struct.omit(['unavailable'])).pipe(Schema.fieldsAssign({ value: Schema.Result(Schema.OptionFromNullOr(Schema.Json), Schema.Defect()) }));
class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.Path.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.field,
}) {
    static readonly Object = Schema.TaggedStruct('Object', {
        ...Reference.fields,
        view: Schema.OptionFromOptionalKey(View),
        fields: Schema.Array(Read),
        collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Schema.String })),
    });
    static readonly Items = Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Schema.Array(Read) }))) });
    static readonly Result = Schema.Union([Inspect.Object, Inspect.Items]);
    static readonly handle = Effect.fn('Inspect.handle')(function* ({ target, fields, slice }: Inspect) {
        const [found, { host }] = yield* Effect.all([Specifier.resolve(target), Model]);
        return yield* host(found)
            ? object(found, fields, slice)
            : Effect.flatMap(
                  Effect.forEach(slice.of(found), (item) => Effect.map(values(item, Option.getOrElse(fields, Array.empty)), (entries) => ({ ...reference(item), fields: entries }))),
                  (items) => read(Inspect.Items, target.path, () => ({ _tag: 'Items', specifier: target.path, total: found.length, items })),
              );
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const getters = (found: InDesignDOMObject<unknown>) => Array.map(Uxp.accessors(Object.getPrototypeOf(found)).getters, Struct.get('name')).filter((name) => name !== 'properties');
const values = Effect.fn('values')(function* (found: InDesignDOMObject<unknown>, wanted: readonly string[]) {
    const { rows } = yield* Model;
    const { fields } = yield* Cache.get(rows, found.constructorName);
    const readable = getters(found);
    return yield* Effect.forEach(
        wanted,
        Effect.fnUntraced(function* (name) {
            yield* readable.includes(name) ? Effect.void : Effect.fail(Unmatched.make({ subject: `field of ${found.toSpecifier()}`, query: Option.some(name), candidates: readable }));
            const subject = `${found.toSpecifier()} ${name}`;
            const row = yield* Option.match(Record.get(fields, name), { onNone: () => undeclared(Field, name), onSome: Effect.succeed });
            const refusal = Effect.transposeOption(Option.map(unavailable(found.constructorName, name, Option.some(found), false), (because) => HostError.make({ reason: { _tag: 'Unavailable', because }, subject })));
            const held = Option.contains(row.type, 'File') ? Model.file(found, name) : Effect.map(Effect.try({ try: (): unknown => Reflect.get(found, name), catch: HostError.refused(subject) }), Option.fromUndefinedOr);
            const value = yield* Effect.result(Effect.andThen(refusal, held));
            return { ...Struct.omit(row, ['unavailable']), value };
        }),
    );
});
const object = Effect.fn('object')(function* (found: InDesignDOMObject<unknown>, fields: Option.Option<readonly string[]>, slice: Slice) {
    const model = yield* Model;
    const { fields: own } = yield* Cache.get(model.rows, found.constructorName);
    const view = yield* Effect.transposeOption(
        Match.value<unknown>(found).pipe(
            Match.when(ApplicationView.owns(model), ApplicationView.of),
            Match.when(DocumentView.owns(model), DocumentView.of),
            Match.when(PageView.owns(model), PageView.of),
            Match.when(SpreadView.owns(model), SpreadView.of),
            Match.when(LayerView.owns(model), LayerView.of),
            Match.when(StyleView.owns(model), StyleView.of),
            Match.when(SwatchView.owns(model), SwatchView.of),
            Match.when(LinkView.owns(model), LinkView.of),
            Match.when(StoryView.owns(model), StoryView.of),
            Match.when(TextView.owns(model), (text) => TextView.of(text, slice)),
            Match.when(FontView.owns(model), FontView.of),
            Match.when(PreflightProcessView.owns(model), PreflightProcessView.of),
            Match.option,
        ),
    );
    const entries = yield* values(
        found,
        Option.getOrElse(fields, () => (Option.isSome(view) ? Record.keys(Record.filter(own, model.plural)) : getters(found))),
    );
    const [collections, scalars] = Array.partition(entries, (entry) =>
        pipe(
            Result.getSuccess(entry.value),
            Option.flatten,
            Option.filter(model.collection),
            Option.map((items) => ({ name: entry.name, type: items.constructor.name, count: items.length, specifier: items.everyItem().toSpecifier() })),
            Result.fromOption(() => entry),
        ),
    );
    return Inspect.Object.make({ ...reference(found), view, fields: yield* read(Schema.Array(Read), found.toSpecifier(), () => scalars), collections });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Inspect };
