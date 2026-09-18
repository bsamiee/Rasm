// --- [IMPORTS] -------------------------------------------------------------------------

import { action, app, core, type Document, type ExecutionContext, type GetPixelsResult, type ImagingBounds2, imaging, type Layer, type Size } from 'adobe:photoshop';
import { active, evaluate, json, opened, read, thrown, written } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { HistoryState, type Job } from '@rasm/creative-cloud-server/frames';
import { dpi, type PixelBudget, pixels, points } from '@rasm/creative-cloud-server/images';
import { type Body, PRESET_CLASSES, type Reply } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Exit, flow, Match, Option, Predicate, pipe, Record, Result, Schema, Struct, Tuple } from 'effect';

// --- [TABLES] --------------------------------------------------------------------------

const _PHOTOSHOP_ERRORS = { userCancelled: -128, modalHeld: 9 } as const;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROFILE = 'sRGB IEC61966-2.1';
const _COMPONENTS = 3;

// --- [MODELS] --------------------------------------------------------------------------

const _ids = Schema.encodeSync(
    Schema.Struct({ documentId: Schema.Number, layerId: Schema.OptionFromOptionalKey(Schema.Number) }).pipe(Schema.encodeKeys({ documentId: 'documentID', layerId: 'layerID' })),
);

const _history = Schema.encodeSync(HistoryState.pipe(Schema.encodeKeys({ documentId: 'documentID' })));

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);

// --- [SCOPE] ---------------------------------------------------------------------------

const _suspended = <A>(context: ExecutionContext, work: Effect.Effect<A, HostRejection>, suspendHistory: Job['suspendHistory']): Effect.Effect<A, HostRejection> =>
    Effect.acquireUseRelease(
        Effect.transposeOption(Option.map(suspendHistory, (held) => Effect.tryPromise({ try: () => context.hostControl.suspendHistory(_history(held)), catch: thrown }))),
        () => work,
        (suspension, exit) =>
            Effect.asVoid(Effect.transposeOption(Option.map(suspension, (held) => Effect.tryPromise({ try: () => context.hostControl.resumeHistory(held, Exit.isSuccess(exit)), catch: thrown })))),
    ).pipe(
        Effect.filterOrFail(
            () => !context.isCancelled,
            () => HostRejection.cases.userCancelled.make({}),
        ),
    );

const modal = <A>(work: Effect.Effect<A, HostRejection>, commandName: string, suspendHistory: Job['suspendHistory']): Effect.Effect<A, HostRejection> =>
    Effect.flatMap(
        Effect.tryPromise({
            try: () => core.executeAsModal((context) => Effect.runPromise(Effect.result(_suspended(context, work, suspendHistory))), { commandName }),
            catch: (cause) =>
                Match.value(cause).pipe(
                    Match.when({ number: _PHOTOSHOP_ERRORS.modalHeld }, () => HostRejection.cases.modalDenied.make({ holder: Option.none() })),
                    Match.orElse(thrown),
                ),
        }),
        Effect.fromResult,
    );

// --- [DOCUMENTS] -----------------------------------------------------------------------

const _found = (id: number): Effect.Effect<Document, HostRejection> =>
    Effect.fromOption(
        Array.findFirst(app.documents, (open) => open.id === id),
        () => HostRejection.cases.documentNotFound.make({ documentId: id }),
    );

const _flat = (
    layers: readonly Layer[],
    depth: number,
    parentId: Option.Option<number>,
): readonly { readonly layer: Layer; readonly depth: number; readonly parentId: Option.Option<number>; readonly children: number }[] =>
    Array.flatMap(layers, (layer) =>
        pipe(Option.getOrElse(Option.fromNullishOr(layer.layers), Array.empty), (children) => [
            { layer, depth, parentId, children: children.length },
            ..._flat(children, depth + 1, Option.some(layer.id)),
        ]),
    );

// --- [PIXELS] --------------------------------------------------------------------------

const _fit = (resolution: number, budget: PixelBudget, { left, top, right, bottom }: ImagingBounds2): Required<Size> => {
    const widthPt = points(right - left, resolution);
    const heightPt = points(bottom - top, resolution);
    const resolved = dpi(budget, widthPt, heightPt);
    return { width: Math.min(right - left, pixels(widthPt, resolved)), height: Math.min(bottom - top, pixels(heightPt, resolved)) };
};

const _mask = (open: Document, budget: PixelBudget, bounds: ImagingBounds2): Effect.Effect<GetPixelsResult, HostRejection> =>
    Effect.acquireUseRelease(
        Effect.tryPromise({
            try: () => imaging.getSelection({ ..._ids({ documentId: open.id, layerId: Option.none() }), sourceBounds: bounds, targetSize: _fit(open.resolution, budget, bounds) }),
            catch: thrown,
        }),
        (mask) =>
            Effect.tryPromise({
                try: async () => {
                    const gray = await mask.imageData.getData({ chunky: true });
                    const rgb = Match.value(gray).pipe(
                        Match.when(Match.instanceOf(Uint8Array), ({ length }) => new Uint8Array(length * _COMPONENTS)),
                        Match.when(Match.instanceOf(Uint16Array), ({ length }) => new Uint16Array(length * _COMPONENTS)),
                        Match.orElse(({ length }) => new Float32Array(length * _COMPONENTS)),
                    );
                    gray.forEach((value, index) => {
                        rgb.fill(value, index * _COMPONENTS, (index + 1) * _COMPONENTS);
                    });
                    return {
                        ...mask,
                        level: 0,
                        imageData: await imaging.createImageDataFromBuffer(rgb, {
                            width: mask.imageData.width,
                            height: mask.imageData.height,
                            components: _COMPONENTS,
                            chunky: true,
                            colorSpace: 'RGB',
                            colorProfile: _PROFILE,
                        }),
                    };
                },
                catch: thrown,
            }),
        (mask) => Effect.promise(() => mask.imageData.dispose()),
    );

const _composite = (open: Document, budget: PixelBudget, layerId: Option.Option<number>, sourceBounds: ImagingBounds2): Effect.Effect<GetPixelsResult, HostRejection> =>
    Effect.tryPromise({
        try: () =>
            imaging.getPixels({
                ..._ids({ documentId: open.id, layerId }),
                sourceBounds,
                targetSize: _fit(open.resolution, budget, sourceBounds),
                colorSpace: 'RGB',
                colorProfile: _PROFILE,
                componentSize: 8,
                applyAlpha: true,
            }),
        catch: thrown,
    });

// --- [HANDLERS] ------------------------------------------------------------------------

const execute = (body: Body<'execute'>): Effect.Effect<Reply<'execute'>, HostRejection> => evaluate(body.code);

const batchPlay = ({ descriptors, continueOnError, immediateRedraw }: Body<'batchPlay'>): Effect.Effect<Reply<'batchPlay'>, HostRejection> =>
    Effect.tryPromise({ try: () => action.batchPlay([...descriptors], { continueOnError, immediateRedraw }), catch: thrown }).pipe(
        Effect.bindTo('results'),
        Effect.let('failed', ({ results }) =>
            Array.map(
                Array.filter(
                    Array.map(results, (descriptor, index) => ({ ...descriptor, index })),
                    Schema.is(Schema.Struct({ _obj: Schema.Literal('error'), index: Schema.Int, message: Schema.String, result: Schema.Number })),
                ),
                Struct.pick(['index', 'result', 'message']),
            ),
        ),
        Effect.filterOrFail(
            ({ failed }) => Array.every(failed, ({ result }) => result !== _PHOTOSHOP_ERRORS.userCancelled),
            () => HostRejection.cases.userCancelled.make({}),
        ),
        Effect.tap(({ failed }) =>
            Effect.transposeOption(
                Option.map(
                    Option.filter(Array.head(failed), () => !continueOnError),
                    flow(HostRejection.cases.descriptorFailed.make, Effect.fail),
                ),
            ),
        ),
        Effect.bind('rendered', ({ results }) => Effect.forEach(results, json)),
        Effect.map(({ rendered, failed }) => ({ kind: 'descriptors' as const, results: rendered, failed })),
    );

const snapshot = ({ documentId, target, region, budget }: Body<'snapshot'>): Effect.Effect<Reply<'snapshot'>, HostRejection> =>
    Effect.acquireUseRelease(
        Effect.gen(function* () {
            const open = yield* Option.match(documentId, { onNone: () => opened(app), onSome: _found });
            const bounds = Option.match(region, {
                onNone: () => ({ left: 0, top: 0, right: open.width, bottom: open.height }),
                onSome: ([x0, y0, x1, y1]) => ({ left: Math.round(x0 * open.width), top: Math.round(y0 * open.height), right: Math.round(x1 * open.width), bottom: Math.round(y1 * open.height) }),
            });
            return yield* Match.value(target).pipe(
                Match.discriminatorsExhaustive('kind')({
                    layer: ({ layerId }) =>
                        Effect.flatMap(
                            Effect.fromOption(
                                Array.findFirst(_flat(open.layers, 0, Option.none()), ({ layer }) => layer.id === layerId),
                                () => HostRejection.cases.itemNotFound.make({ itemId: layerId }),
                            ),
                            ({ layer }) => _composite(open, budget, Option.some(layer.id), layer.boundsNoEffects),
                        ),
                    document: () => _composite(open, budget, Option.none(), bounds),
                    selection: () => _mask(open, budget, bounds),
                }),
            );
        }),
        ({ imageData, sourceBounds, level }) =>
            Effect.map(
                Effect.filterOrFail(Effect.tryPromise({ try: () => imaging.encodeImageData({ imageData, base64: true }), catch: thrown }), Predicate.isString, (encoded) =>
                    HostRejection.cases.resultNotJson.make({ cause: encoded }),
                ),
                (base64) => ({
                    kind: 'jpeg' as const,
                    base64,
                    widthPx: imageData.width,
                    heightPx: imageData.height,
                    level,
                    scale: 2 ** -level,
                    sourceBounds: { left: sourceBounds.left * 2 ** level, top: sourceBounds.top * 2 ** level, right: sourceBounds.right * 2 ** level, bottom: sourceBounds.bottom * 2 ** level },
                    colorProfile: imageData.colorProfile,
                }),
            ),
        ({ imageData }) => Effect.promise(() => imageData.dispose()),
    );

const getDocument = ({ documentId, limit, cursor, depth }: Body<'getDocument'>): Effect.Effect<Reply<'getDocument'>, HostRejection> =>
    Effect.map(Option.match(documentId, { onNone: () => Effect.succeed(active(app)), onSome: flow(_found, Effect.map(Option.some)) }), (open) => {
        const rows = Option.map(
            open,
            flow(
                (selected) => _flat(selected.layers, 0, Option.none()),
                Array.filter((placed) => placed.depth <= depth),
            ),
        );
        const count = Option.map(rows, Array.length);
        const paged = Option.map(
            rows,
            flow(
                Array.drop(cursor),
                Array.take(limit),
                Array.map(({ layer, ...placed }) => ({ id: layer.id, name: layer.name, kind: layer.kind, visible: layer.visible, ...placed })),
            ),
        );
        const layerCursor = Option.as(
            Option.filter(count, (total) => cursor + limit < total),
            cursor + limit,
        );
        return {
            kind: 'document' as const,
            documents: Array.map(app.documents, (document) => ({ id: document.id, name: document.name, path: document.path, saved: document.saved })),
            active: Option.map(Option.all({ selected: open, layers: paged, layerCount: count }), ({ selected, layers, layerCount }) => ({
                id: selected.id,
                mode: selected.mode,
                bitsPerChannel: selected.bitsPerChannel,
                colorProfileName: selected.colorProfileName,
                width: selected.width,
                height: selected.height,
                resolution: selected.resolution,
                layers,
                layerCount,
                layerCursor,
            })),
        };
    });

const getPreferences = ({ sections }: Body<'getPreferences'>): Effect.Effect<Reply<'getPreferences'>> =>
    Effect.sync(() =>
        Array.flatMap(sections, (section) =>
            Array.cartesian(Array.of(section), Record.toEntries(Record.remove(Object.getOwnPropertyDescriptors<object>(Object.getPrototypeOf(app.preferences[section])), 'typename'))),
        ),
    ).pipe(
        Effect.map(Array.filter(([, [, descriptor]]) => Predicate.isFunction(descriptor.get))),
        Effect.map(
            Array.map(([section, [key]]) =>
                Result.mapBoth(read(_json)(app.preferences[section], key), { onSuccess: (value) => ({ section, entry: Tuple.make(key, value) }), onFailure: (cause) => ({ section, key, cause }) }),
            ),
        ),
        Effect.map(Array.separate),
        Effect.map(([unreadable, rows]) => ({
            kind: 'preferences' as const,
            values: Record.map(Array.groupBy<(typeof rows)[number], string>(rows, Struct.get('section')), flow(Array.map(Struct.get('entry')), Record.fromEntries)),
            unreadable,
        })),
    );

const setPreferences = ({ values }: Body<'setPreferences'>): Effect.Effect<Reply<'setPreferences'>, HostRejection> =>
    Effect.sync(() =>
        Array.separate(
            Array.map(values, ({ section, key, value }) =>
                Result.mapBoth(written(_json)(app.preferences[section], key, value, value), {
                    onSuccess: (changed) => ({ section, key, ...changed }),
                    onFailure: (reason) => ({ section, key, reason }),
                }),
            ),
        ),
    ).pipe(
        Effect.flatMap(([rejected, applied]) =>
            Option.match(
                Option.filter(
                    Array.findFirst(rejected, (row) => row.section === 'notifications' && Predicate.isTagged(row.reason, 'threw')),
                    () => app.preferences.notifications.quietMode,
                ),
                {
                    onNone: () => Effect.succeed({ kind: 'applied' as const, applied, rejected }),
                    onSome: (row) => Effect.fail(HostRejection.cases.preferenceLocked.make(Struct.pick(row, ['section', 'key']))),
                },
            ),
        ),
    );

const listPresets = ({ kind }: Body<'listPresets'>): Effect.Effect<Reply<'listPresets'>, HostRejection> =>
    Effect.gen(function* () {
        const answer = yield* Effect.tryPromise({
            try: () =>
                action.batchPlay(
                    [
                        {
                            _obj: 'get',
                            _target: [
                                { _ref: 'property', _property: 'presetManager' },
                                { _ref: 'application', _enum: 'ordinal', _value: 'targetEnum' },
                            ],
                        },
                    ],
                    {},
                ),
            catch: thrown,
        });
        const [store] = yield* Effect.mapError(
            Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ presetManager: Schema.Array(Schema.Struct({ _obj: Schema.String, name: Schema.Array(Schema.String) })) })]))(answer),
            (cause) => HostRejection.cases.resultNotJson.make({ cause }),
        );
        const groups = Record.fromEntries(Array.map(store.presetManager, (row, groupIndex) => Tuple.make(row._obj, { groupIndex, names: row.name })));
        const group = yield* Effect.fromOption(Record.get(groups, PRESET_CLASSES[kind]), () => HostRejection.cases.resultNotJson.make({ cause: { kind, groups: Record.keys(groups) } }));
        return { kind: 'presets' as const, ...group, count: group.names.length };
    });

const runAction = ({ set, action: name }: Body<'runAction'>): Effect.Effect<Reply<'runAction'>, HostRejection> =>
    Effect.flatMap(
        Effect.fromOption(
            Option.flatMap(
                Array.findFirst(app.actionTree, (candidate) => candidate.name === set),
                flow(
                    Struct.get('actions'),
                    Array.findFirst((candidate) => candidate.name === name),
                ),
            ),
            () => HostRejection.cases.actionNotFound.make({ set, action: name }),
        ),
        (found) => Effect.as(Effect.tryPromise({ try: () => found.play(), catch: thrown }), null),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { batchPlay, execute, getDocument, getPreferences, listPresets, modal, runAction, setPreferences, snapshot };
