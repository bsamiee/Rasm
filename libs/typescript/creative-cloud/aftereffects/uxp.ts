import { Containment } from '@rasm/object-model/host';
import { HostError } from '@rasm/object-model/model';
import type { Render } from '@rasm/object-model/rpcs';
import { Unmatched } from '@rasm/search/unmatched';
import { Callable, HostObject, Uxp } from '@rasm/uxp/plugin';
import type { File } from 'adobe:uxp';
import { Array, Context, Effect, Layer, Option, Predicate, Record, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Class = HostObject({ name: Schema.NonEmptyString }, (value): value is abstract new () => object => Predicate.isFunction(value));
const Identified = Schema.Struct({ id: Schema.Int });
const Placed = Schema.Struct({ containingComp: Identified });
const Project = HostObject({ items: Uxp.Collection, itemByID: Callable, layerByID: Callable, toolType: Schema.mutableKey(Schema.Finite) });
const Composition = HostObject({ ...Identified.fields, layers: Uxp.Collection, time: Schema.Finite, resolutionFactor: Schema.Tuple([Schema.Int, Schema.Int]), saveFrameToPng: Callable });
const Deferred = HostObject({ wait: Callable });
const Application = HostObject({ project: Project, beginSuppressDialogs: Callable, endSuppressDialogs: Callable, beginUndoGroup: Callable, endUndoGroup: Callable });
const Module = Schema.Struct({
    app: Application,
    CompItem: Class,
    FolderItem: Class,
    FootageItem: Class,
    ToolType: Schema.Record(Schema.String, Schema.Finite),
});

// --- [SERVICES] ------------------------------------------------------------------------

class AfterEffects extends Context.Service<AfterEffects>()('@rasm/aftereffects/uxp/AfterEffects', {
    make: Effect.map(Uxp.require('aftereffects', Module), ({ app, CompItem, FolderItem, FootageItem, ToolType }) => ({
        app,
        CompItem,
        ToolType,
        tools: new Map(Array.map(Record.toEntries(ToolType), ([id, toolType]) => [toolType, { id, label: id }] as const)),
        containment: {
            root: app,
            kinds: {
                project: Containment.kind({ property: ({ project }: typeof Application.Type) => project }),
                item: Containment.kind({
                    items: ({ items }: typeof Project.Type) => items,
                    base: 1,
                    key: Identified,
                    by: { id: (project, id) => project.itemByID(id) },
                    is: Schema.is(Identified.check(Schema.makeFilter((item) => item instanceof CompItem || item instanceof FolderItem || item instanceof FootageItem)))<object>,
                    owner: () => app.project,
                }),
                layer: Containment.kind({
                    items: ({ layers }: typeof Composition.Type) => layers,
                    base: 1,
                    key: Identified,
                    by: { id: (composition, id) => Option.getOrUndefined(Option.liftPredicate(app.project.layerByID(id), (layer) => Schema.is(Placed)(layer) && layer.containingComp.id === composition.id)) },
                    is: Schema.is(Schema.Struct({ ...Identified.fields, containingComp: Schema.instanceOf(CompItem) }))<object>,
                    owner: ({ containingComp }) => containingComp,
                }),
            },
        } satisfies Containment,
    })),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const bracket = (subject: string, { begin, end }: Record.ReadonlyRecord<'begin' | 'end', (app: typeof Application.Type) => unknown>) =>
    AfterEffects.use(({ app }) => Effect.acquireRelease(Effect.try({ try: () => begin(app), catch: HostError.refused(subject) }), () => Effect.orDie(Effect.try({ try: () => end(app), catch: HostError.refused(subject) }))));
const suppressDialogs = bracket('dialog suppression', { begin: (app) => app.beginSuppressDialogs(), end: (app) => app.endSuppressDialogs(false) });
const undoGroup = (label: string) => bracket(`undo group ${label}`, { begin: (app) => app.beginUndoGroup(label), end: (app) => app.endUndoGroup() });
const execute = (code: string, label: string) => AfterEffects.use(({ containment }) => Uxp.execute(containment, code, (call) => Effect.scoped(Effect.andThen(undoGroup(label), call))));
const toolbar = {
    current: AfterEffects.use(({ app: { project }, tools, ToolType }) => Effect.fromOption(Option.fromUndefinedOr(tools.get(project.toolType)), () => Unmatched.of({ subject: 'tool type', query: Option.some(`${project.toolType}`), candidates: Record.keys(ToolType) }))),
    listed: AfterEffects.useSync(({ tools }) => Array.fromIterable(tools.values())),
    select: (id: string) =>
        AfterEffects.use(({ app: { project }, ToolType }) =>
            Effect.fromOption(Record.get(ToolType, id), () => Unmatched.of({ subject: 'tool type', query: Option.some(id), candidates: Record.keys(ToolType) })).pipe(
                Effect.flatMap((toolType) => Effect.try({ try: () => Object.assign(project, { toolType } satisfies Partial<typeof project>), catch: HostError.refused(`tool type ${id}`) })),
            ),
        ),
    changes: Uxp.ticks,
};
const render = Effect.fnUntraced(function* ({ target, label }: Render, written: File) {
    const { containment, CompItem } = yield* AfterEffects;
    yield* undoGroup(label);
    const found = yield* Uxp.locate(containment, target);
    const composition = yield* Effect.mapError(Schema.decodeUnknownEffect(Composition)(found), () => Unmatched.of({ subject: CompItem.name, query: Option.some(target.path) }));
    yield* Uxp.preserve(composition, { resolutionFactor: [1, 1] });
    const deferred = yield* Effect.try({ try: (): unknown => composition.saveFrameToPng(composition.time, written.nativePath), catch: HostError.refused(`saveFrameToPng to ${written.nativePath}`) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Deferred)));
    yield* Effect.try({ try: () => deferred.wait(), catch: HostError.refused(`wait of saveFrameToPng to ${written.nativePath}`) });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { AfterEffects, execute, render, suppressDialogs, toolbar, undoGroup };
