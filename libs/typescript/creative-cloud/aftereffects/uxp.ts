import { HostError, type Render } from '@rasm/object-model';
import { type Containment, read } from '@rasm/object-model/host';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Effect, Option, Predicate, type Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Method = Schema.declare((value: unknown): value is (...args: readonly unknown[]) => unknown => Predicate.isFunction(value));
const Class = Schema.declare((value: unknown): value is abstract new (...args: readonly never[]) => object => Predicate.isFunction(value));
class Application extends Schema.Opaque<Application, unknown>()(Schema.Struct({ beginSuppressDialogs: Method, endSuppressDialogs: Method, beginUndoGroup: Method, endUndoGroup: Method })) {}
const Module = Schema.Struct({
    app: Schema.declare(Schema.is(Application)),
    Project: Class,
    CompItem: Class,
    FolderItem: Class,
    FootageItem: Class,
}).annotate({ description: 'Application instance and DOM constructors require("aftereffects") holds' });
const Frame = Schema.declare(Schema.is(Schema.Struct({ time: Schema.Finite, resolutionFactor: Schema.Tuple([Schema.Int, Schema.Int]), saveFrameToPng: Method }))).annotate({ description: 'Composition by the members render reads and calls' });
const Deferred = Schema.declare(Schema.is(Schema.Struct({ wait: Method }))).annotate({ description: 'DeferredCall saveFrameToPng returns, settled through wait()' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = Effect.flatMap(Uxp.required('aftereffects'), Schema.decodeUnknownEffect(Module));
const containment = Effect.map(hosted, ({ app, Project, CompItem, FolderItem, FootageItem }): Containment => ({
    root: app,
    type: (value) => value.constructor.name,
    kinds: {
        project: { is: (value) => value instanceof Project },
        item: { collection: 'items', base: 1, keys: ['id'], is: (value) => value instanceof CompItem || value instanceof FolderItem || value instanceof FootageItem, owner: () => read(app, 'project') },
        layer: { collection: 'layers', base: 1, keys: ['id'], is: (value) => read(value, 'containingComp') instanceof CompItem && Predicate.isNumber(read(value, 'id')), owner: (value) => read(value, 'containingComp') },
    },
}));
const paired = (subject: string, { begin, end }: Record.ReadonlyRecord<'begin' | 'end', (app: Application) => unknown>) =>
    Effect.flatMap(hosted, ({ app }) => Effect.acquireRelease(Effect.try({ try: () => begin(app), catch: HostError.refused(subject) }), () => Effect.orDie(Effect.try({ try: () => end(app), catch: HostError.refused(subject) }))));
const session = paired('dialog suppression', { begin: (app) => app.beginSuppressDialogs(), end: (app) => app.endSuppressDialogs(false) });
const undo = (label: string) => paired(`undo group ${label}`, { begin: (app) => app.beginUndoGroup(label), end: (app) => app.endUndoGroup() });
const execute = (code: string, label: string) => Effect.flatMap(containment, (held) => Uxp.executed(held, code, (call) => Effect.scoped(Effect.andThen(Effect.all([session, undo(label)]), call))));
const render = Effect.fnUntraced(function* ({ target, label }: Render, written: File) {
    const [held, { CompItem }] = yield* Effect.all([containment, hosted, session, undo(label)]);
    const found = yield* Uxp.located(held, target);
    const composition = yield* Effect.mapError(Schema.decodeUnknownEffect(Frame)(found instanceof CompItem ? found : undefined), () => Unmatched.of({ subject: CompItem.name, query: Option.some(target.path) }));
    yield* Uxp.preserve(composition, { resolutionFactor: [1, 1] });
    const deferred = yield* Effect.try({ try: (): unknown => composition.saveFrameToPng(composition.time, written.nativePath), catch: HostError.refused(`saveFrameToPng to ${written.nativePath}`) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Deferred)));
    yield* Effect.try({ try: () => deferred.wait(), catch: HostError.refused(`wait of saveFrameToPng to ${written.nativePath}`) });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { containment, execute, render, session, undo };
