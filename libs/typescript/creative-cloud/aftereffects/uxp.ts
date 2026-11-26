import { HostError, type Render } from '@rasm/object-model';
import type { Containment } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Callable, Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Class = Schema.declare((value: unknown): value is abstract new (...args: readonly never[]) => object => Predicate.isFunction(value));
const Identified = Schema.Struct({ id: Schema.Int }).annotate({ description: 'Item or layer keyed by its id' });
const Project = Schema.Struct({ items: Uxp.Collection }).annotate({ description: 'Project by its items, 1-based' });
const Composition = Schema.Struct({ layers: Uxp.Collection }).annotate({ description: 'Composition by its layers, 1-based' });
const Contained = Identified.mapFields(Struct.assign({ containingComp: Schema.declare(Predicate.isObject) })).annotate({ description: 'Layer by its id and the composition holding it' });
class Application extends Schema.Opaque<Application, unknown>()(Schema.Struct({ project: Schema.declare(Schema.is(Project)), beginSuppressDialogs: Callable, endSuppressDialogs: Callable, beginUndoGroup: Callable, endUndoGroup: Callable })) {}
const Module = Schema.Struct({
    app: Schema.declare(Schema.is(Application)),
    Project: Class,
    CompItem: Class,
    FolderItem: Class,
    FootageItem: Class,
}).annotate({ description: 'Application instance and DOM constructors require("aftereffects") holds' });
const Tooled = Schema.Struct({
    app: Schema.Struct({ project: Schema.declare(Schema.is(Schema.Struct({ toolType: Schema.mutableKey(Schema.Finite) }))).annotate({ description: 'Project of the application, its tool type read and written in place' }) }),
    ToolType: Schema.Record(Schema.String, Schema.Finite).annotate({ description: 'Tool enumeration, each tool name keying its value' }),
}).annotate({ description: 'Project of the application holding its tool type, and the enumeration naming it, require("aftereffects") holds' });
const Frame = Schema.declare(Schema.is(Schema.Struct({ time: Schema.Finite, resolutionFactor: Schema.Tuple([Schema.Int, Schema.Int]), saveFrameToPng: Callable }))).annotate({ description: 'Composition by the members render reads and calls' });
const Deferred = Schema.declare(Schema.is(Schema.Struct({ wait: Callable }))).annotate({ description: 'DeferredCall saveFrameToPng returns, settled through wait()' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = Uxp.require('aftereffects');
const hosted = Effect.flatMap(loaded, Schema.decodeUnknownEffect(Module));
const containment = Effect.map(
    hosted,
    (aftereffects): Containment => ({
        root: aftereffects.app,
        kinds: {
            project: { property: ({ project }: Application) => project, is: (value) => value instanceof aftereffects.Project },
            item: { items: ({ items }: typeof Project.Type) => items, base: 1, key: Identified, is: (value) => value instanceof aftereffects.CompItem || value instanceof aftereffects.FolderItem || value instanceof aftereffects.FootageItem, owner: () => aftereffects.app.project },
            layer: { items: ({ layers }: typeof Composition.Type) => layers, base: 1, key: Identified, is: (value) => Option.exists(Schema.decodeUnknownOption(Contained)(value), ({ containingComp }) => containingComp instanceof aftereffects.CompItem), owner: ({ containingComp }: typeof Contained.Type) => containingComp },
        },
    }),
);
const paired = (subject: string, { begin, end }: Record.ReadonlyRecord<'begin' | 'end', (app: Application) => unknown>) =>
    Effect.flatMap(hosted, ({ app }) => Effect.acquireRelease(Effect.try({ try: () => begin(app), catch: HostError.refused(subject) }), () => Effect.orDie(Effect.try({ try: () => end(app), catch: HostError.refused(subject) }))));
const suppressDialogs = paired('dialog suppression', { begin: (app) => app.beginSuppressDialogs(), end: (app) => app.endSuppressDialogs(false) });
const undoGroup = (label: string) => paired(`undo group ${label}`, { begin: (app) => app.beginUndoGroup(label), end: (app) => app.endUndoGroup() });
const execute = (code: string, label: string) => Effect.flatMap(containment, (held) => Uxp.execute(held, code, (call) => Effect.scoped(Effect.andThen(Effect.all([suppressDialogs, undoGroup(label)]), call))));
const tooled = Effect.flatMap(loaded, Schema.decodeUnknownEffect(Tooled));
const toolbar = {
    current: tooled.pipe(
        Effect.flatMap(({ app: { project }, ToolType }) =>
            Effect.fromOption(
                Record.findFirst(ToolType, (value) => value === project.toolType),
                () => Unmatched.of({ subject: 'tool type', query: Option.some(`${project.toolType}`), candidates: Record.keys(ToolType) }),
            ),
        ),
        Effect.map(([id]) => ({ id, label: id })),
    ),
    listed: Effect.map(tooled, ({ ToolType }) => Array.map(Record.keys(ToolType), (id) => ({ id, label: id }))),
    select: (id: string) =>
        tooled.pipe(
            Effect.flatMap(({ app: { project }, ToolType }) =>
                Effect.map(
                    Effect.fromOption(Record.get(ToolType, id), () => Unmatched.of({ subject: 'tool type', query: Option.some(id), candidates: Record.keys(ToolType) })),
                    (value) => ({ project, value }),
                ),
            ),
            Effect.flatMap(({ project, value }) => Effect.try({ try: () => Object.assign(project, { toolType: value } satisfies Partial<typeof project>), catch: HostError.refused(`tool type ${id}`) })),
        ),
};
const render = Effect.fnUntraced(function* ({ target, label }: Render, written: File) {
    const [held, { CompItem }] = yield* Effect.all([containment, hosted, suppressDialogs, undoGroup(label)]);
    const found = yield* Uxp.locate(held, target);
    const composition = yield* Effect.mapError(Schema.decodeUnknownEffect(Frame)(found instanceof CompItem ? found : undefined), () => Unmatched.of({ subject: CompItem.name, query: Option.some(target.path) }));
    yield* Uxp.preserve(composition, { resolutionFactor: [1, 1] });
    const deferred = yield* Effect.try({ try: (): unknown => composition.saveFrameToPng(composition.time, written.nativePath), catch: HostError.refused(`saveFrameToPng to ${written.nativePath}`) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Deferred)));
    yield* Effect.try({ try: () => deferred.wait(), catch: HostError.refused(`wait of saveFrameToPng to ${written.nativePath}`) });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { containment, execute, render, suppressDialogs, toolbar, undoGroup };
