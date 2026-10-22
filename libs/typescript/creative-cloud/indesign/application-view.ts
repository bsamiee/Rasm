import { Array, Effect, Option, pipe, Schema, Struct } from 'effect';
import { DocumentView } from '#/documents/document-view';
import { read } from '#/dom/encoding';
import { active } from '#/dom/location';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/reference';
import { TextView } from '#/text/text-view';

// --- [MODELS] --------------------------------------------------------------------------

class ApplicationView extends Schema.TaggedClass<ApplicationView, { readonly brand: unique symbol }>()('Application', {
    version: Schema.String,
    tool: Schema.String,
    panels: Schema.Array(Schema.String),
    documents: Schema.Array(DocumentView.Row),
    active: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Specifier,
            window: Specifier,
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Specifier), overrides: Schema.OptionFromOptionalKey(TextView.fields.overrides) }))),
            overset: Schema.Array(Specifier),
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ),
}) {
    static readonly current = Effect.gen(function* () {
        const { app } = yield* Model;
        const rows = yield* Effect.forEach(app.documents.everyItem().getElements(), DocumentView.row);
        const opened = yield* Effect.option(active);
        const selection = yield* Effect.forEach(Option.match(opened, { onNone: Array.empty, onSome: () => app.selection }), (item) =>
            pipe(
                Option.liftPredicate(item, (each) => 'textHasOverrides' in each),
                Option.map(TextView.overrides),
                Effect.transposeOption,
                Effect.map((overrides) => ({
                    ...Reference.of(item),
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
        return yield* read(ApplicationView, 'application', () => ({
            _tag: 'Application',
            version: app.version,
            tool: app.toolBoxTools.currentTool,
            panels,
            documents: rows,
            active: Option.map(opened, (document) => ({
                document: document.toSpecifier(),
                window: app.activeWindow.toSpecifier(),
                selection,
                overset: DocumentView.overset(document),
                undo: { mode: app.activeScriptUndoMode, name: document.undoName, history: document.undoHistory },
            })),
        }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationView };
