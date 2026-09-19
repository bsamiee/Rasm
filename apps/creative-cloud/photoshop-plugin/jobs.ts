// --- [IMPORTS] -------------------------------------------------------------------------

import { action, app, constants, core, type Document, type ExecutionContext, type GetPixelsResult, type ImagingBounds2, imaging, type Layer, type Size, type SolidColor } from 'adobe:photoshop';
import { active, evaluate, json, opened, read, thrown, written } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import type { Job } from '@rasm/creative-cloud-server/frames';
import { dpi, type PixelBudget, pixels, points } from '@rasm/creative-cloud-server/images';
import { type Body, PRESET_CLASSES, type Reply, type TypeStyle } from '@rasm/creative-cloud-server/photoshop/jobs';
import { CHANNELS } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Exit, flow, Iterable, Match, MutableHashMap, Option, Predicate, Record, Result, Schema, Struct, Tuple } from 'effect';

// --- [TABLES] --------------------------------------------------------------------------

const _PHOTOSHOP_ERRORS = { userCancelled: -128, modalHeld: 9 } as const;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROFILE = 'sRGB IEC61966-2.1';
const _COMPONENTS = 3;

// --- [MODELS] --------------------------------------------------------------------------

const _json: (input: unknown) => Option.Option<Schema.Json> = Schema.decodeUnknownOption(Schema.Json);

// --- [SCOPE] ---------------------------------------------------------------------------

const _suspended = <A>(context: ExecutionContext, work: Effect.Effect<A, HostRejection>, suspendHistory: Job['suspendHistory']): Effect.Effect<A, HostRejection> =>
    Effect.acquireUseRelease(
        Effect.transposeOption(
            Option.map(suspendHistory, ({ documentId, name }) => Effect.tryPromise({ try: () => context.hostControl.suspendHistory({ documentID: documentId, name }), catch: thrown })),
        ),
        () =>
            work.pipe(
                Effect.mapError((cause) => (context.isCancelled ? HostRejection.cases.userCancelled.make({}) : cause)),
                Effect.filterOrFail(
                    () => !context.isCancelled,
                    () => HostRejection.cases.userCancelled.make({}),
                ),
            ),
        (suspension, exit) =>
            Effect.asVoid(Effect.transposeOption(Option.map(suspension, (held) => Effect.tryPromise({ try: () => context.hostControl.resumeHistory(held, Exit.isSuccess(exit)), catch: thrown })))),
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
    maximum: number,
    depth: number,
    parentId: Option.Option<number>,
): Iterable<{ readonly layer: Layer; readonly depth: number; readonly parentId: Option.Option<number>; readonly children: number }> =>
    Iterable.flatMap(layers, (layer) => {
        const children = Option.getOrElse(Option.fromNullishOr(layer.layers), Array.empty);
        return Iterable.appendAll(Iterable.of({ layer, depth, parentId, children: children.length }), depth < maximum ? _flat(children, maximum, depth + 1, Option.some(layer.id)) : Iterable.empty());
    });

const _layer = (document: Document, id: number): Effect.Effect<Layer, HostRejection> =>
    Effect.map(
        Effect.fromOption(
            Iterable.findFirst(_flat(document.layers, Number.POSITIVE_INFINITY, 0, Option.none()), ({ layer }) => layer.id === id),
            () => HostRejection.cases.itemNotFound.make({ itemId: id }),
        ),
        Struct.get('layer'),
    );

// --- [DESIGN] --------------------------------------------------------------------------

const _color = (ink: Option.Option.Value<TypeStyle['color']>): SolidColor => {
    const color = new app.SolidColor();
    Match.value(ink).pipe(
        Match.discriminator('model')('RGB', ({ values: [red, green, blue] }) => Object.assign(color.rgb, { red, green, blue })),
        Match.discriminator('model')('CMYK', ({ values: [cyan, magenta, yellow, black] }) => Object.assign(color.cmyk, { cyan, magenta, yellow, black })),
        Match.discriminator('model')('LAB', ({ values: [l, a, b] }) => Object.assign(color.lab, { l, a, b })),
        Match.discriminator('model')('GRAY', ({ values: [white] }) => Object.assign(color.gray, { gray: (1 - white) * CHANNELS.percent })),
        Match.exhaustive,
    );
    return color;
};

const _styled = (layer: Layer, { character, paragraph, color }: TypeStyle): Effect.Effect<TypeStyle, HostRejection> =>
    Effect.try({
        try: () => {
            const text = layer.textItem;
            Object.assign(text.characterStyle, character, Record.getSomes({ color: Option.map(color, _color) }));
            Object.assign(text.paragraphStyle, paragraph);
            return {
                character: Struct.pick(text.characterStyle, Struct.keys(character)),
                paragraph: Struct.pick(text.paragraphStyle, Struct.keys(paragraph)),
                color: Option.map(color, (ink) =>
                    Match.value(ink).pipe(
                        Match.discriminator('model')('RGB', () => ({
                            model: 'RGB' as const,
                            values: [text.characterStyle.color.rgb.red, text.characterStyle.color.rgb.green, text.characterStyle.color.rgb.blue] as const,
                        })),
                        Match.discriminator('model')('CMYK', () => ({
                            model: 'CMYK' as const,
                            values: [text.characterStyle.color.cmyk.cyan, text.characterStyle.color.cmyk.magenta, text.characterStyle.color.cmyk.yellow, text.characterStyle.color.cmyk.black] as const,
                        })),
                        Match.discriminator('model')('LAB', () => ({
                            model: 'LAB' as const,
                            values: [text.characterStyle.color.lab.l, text.characterStyle.color.lab.a, text.characterStyle.color.lab.b] as const,
                        })),
                        Match.discriminator('model')('GRAY', () => ({ model: 'GRAY' as const, values: [1 - text.characterStyle.color.gray.gray / CHANNELS.percent] as const })),
                        Match.exhaustive,
                    ),
                ),
            };
        },
        catch: thrown,
    });

const applyTypeStyles = ({ documentId, styles, targets }: Body<'applyTypeStyles'>): Effect.Effect<Reply<'applyTypeStyles'>, HostRejection> =>
    Effect.gen(function* () {
        const document = yield* _found(documentId);
        const resolved = yield* Effect.validate(targets, ({ layerId, style }) =>
            Effect.all({
                layer: _layer(document, layerId).pipe(
                    Effect.filterOrFail(
                        (layer) => layer.kind === constants.LayerKind.TEXT,
                        () => HostRejection.cases.malformedParams.make({ cause: { layerId, expected: constants.LayerKind.TEXT } }),
                    ),
                ),
                values: Effect.fromOption(Record.get(styles, style), () => HostRejection.cases.malformedParams.make({ cause: { style } })),
                style: Effect.succeed(style),
            }),
        ).pipe(Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })));
        const layers = yield* Effect.forEach(resolved, ({ layer, style, values }) => Effect.map(_styled(layer, values), (readback) => ({ layerId: layer.id, style, values: readback })));
        return { kind: 'typeStyles', documentId, layers };
    });

const composeLayers: (input: Body<'composeLayers'>) => Effect.Effect<Reply<'composeLayers'>, HostRejection> = Effect.fn(function* ({
    documentId,
    layers,
    styles,
}: Body<'composeLayers'>): Effect.fn.Return<Reply<'composeLayers'>, HostRejection> {
    const document = yield* _found(documentId);
    const resolved = yield* Effect.validate(layers, (row) =>
        Match.value(row.content).pipe(
            Match.discriminator('kind')('source', (content) =>
                Effect.map(
                    Effect.flatMap(_found(content.documentId), (source) => _layer(source, content.layerId)),
                    (source) => ({ ...row, content: { ...content, source } }),
                ),
            ),
            Match.discriminator('kind')('text', (content) =>
                Effect.map(
                    Effect.fromOption(Record.get(styles, content.style), () => HostRejection.cases.malformedParams.make({ cause: { style: content.style } })),
                    (values) => ({ ...row, content: { ...content, values } }),
                ),
            ),
            Match.orElse((content) => Effect.succeed({ ...row, content })),
        ),
    ).pipe(Effect.mapError((cause) => HostRejection.cases.malformedParams.make({ cause })));
    const parents = MutableHashMap.empty<number, Layer>();
    const siblings = MutableHashMap.empty<Option.Option<number>, Layer>();
    const created = yield* Effect.forEach(
        resolved,
        Effect.fn(function* (row: (typeof resolved)[number], index: number): Effect.fn.Return<Layer, HostRejection> {
            const options = Struct.pick(row, ['name', 'opacity', 'blendMode']);
            const result = yield* Effect.tryPromise({
                try: () =>
                    Match.value(row.content).pipe(
                        Match.discriminator('kind')('group', ({ color }) => document.createLayerGroup({ ...options, color })),
                        Match.discriminator('kind')('pixel', ({ color }) => document.createPixelLayer({ ...options, color })),
                        Match.discriminator('kind')('text', ({ color, contents, position }) => document.createTextLayer({ ...options, color, contents, position })),
                        Match.discriminator('kind')('source', ({ source }) => source.duplicate(document, constants.ElementPlacement.PLACEATBEGINNING, row.name)),
                        Match.exhaustive,
                    ),
                catch: thrown,
            });
            const layer = yield* Effect.fromOption(Option.fromNullishOr(result), () => HostRejection.cases.resultNotJson.make({ cause: row }));
            const preceding = MutableHashMap.get(siblings, row.parent);
            const group = Option.flatMap(row.parent, (parent) => MutableHashMap.get(parents, parent));
            const anchor = Option.orElse(preceding, () => group);
            const placement = Option.isSome(preceding) ? constants.ElementPlacement.PLACEAFTER : constants.ElementPlacement.PLACEINSIDE;
            yield* Effect.try({
                try: () => {
                    Object.assign(layer, { allLocked: false, pixelsLocked: false, positionLocked: false, transparentPixelsLocked: false });
                    const [first] = document.layers;
                    if (Option.isSome(anchor)) {
                        layer.move(anchor.value, placement);
                    } else if (first && first.id !== layer.id) {
                        layer.move(first, constants.ElementPlacement.PLACEBEFORE);
                    }
                    Object.assign(layer, Struct.pick(row, ['name', 'opacity', 'blendMode', 'visible']));
                },
                catch: thrown,
            });
            if (row.content.kind === 'text') {
                yield* _styled(layer, row.content.values);
            }
            MutableHashMap.set(siblings, row.parent, layer);
            MutableHashMap.set(parents, index, layer);
            return layer;
        }),
    );
    yield* Effect.try({
        try: () =>
            Array.forEach(Array.reverse(Array.zip(created, layers)), ([layer, row]) => {
                if (layer.isClippingMask !== row.clipped) {
                    layer.isClippingMask = row.clipped;
                }
                layer.allLocked = row.locked;
            }),
        catch: thrown,
    });
    const { results } = yield* batchPlay({
        descriptors: Array.map(layers, (_, index) => ({
            _obj: 'get',
            _target: [{ _property: 'color' }, { _ref: 'layer', _id: Array.getUnsafe(created, index).id }, { _ref: 'document', _id: documentId }],
        })),
        continueOnError: false,
        immediateRedraw: false,
    });
    const colors = yield* Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ color: Schema.Struct({ _enum: Schema.Literal('color'), _value: Schema.Enum(constants.LabelColors) }) })))(
        results,
    ).pipe(Effect.mapError((cause) => HostRejection.cases.resultNotJson.make({ cause })));
    return yield* Effect.try({
        try: () => ({
            kind: 'layers' as const,
            documentId,
            layers: Array.map(created, (layer, index) => ({
                layerId: layer.id,
                parentId: Option.map(Option.fromNullishOr(layer.parent), Struct.get('id')),
                name: layer.name,
                kind: layer.kind,
                color: Array.getUnsafe(colors, index).color._value,
                visible: layer.visible,
                locked: layer.locked,
                clipped: layer.isClippingMask,
                opacity: layer.opacity,
                blendMode: layer.blendMode,
            })),
        }),
        catch: thrown,
    });
});

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
            try: () => imaging.getSelection({ documentID: open.id, sourceBounds: bounds, targetSize: _fit(open.resolution, budget, bounds) }),
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
        (mask) => Effect.sync(() => mask.imageData.dispose()),
    );

const _composite = (open: Document, budget: PixelBudget, layerId: Option.Option<number>, sourceBounds: ImagingBounds2): Effect.Effect<GetPixelsResult, HostRejection> =>
    Effect.tryPromise({
        try: () =>
            imaging.getPixels({
                documentID: open.id,
                ...Record.getSomes({ layerID: layerId }),
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
            const selectedLayer = yield* target.kind === 'layer'
                ? Effect.map(
                      Effect.fromOption(
                          Iterable.findFirst(_flat(open.layers, Number.POSITIVE_INFINITY, 0, Option.none()), ({ layer }) => layer.id === target.layerId),
                          () => HostRejection.cases.itemNotFound.make({ itemId: target.layerId }),
                      ),
                      ({ layer }) => Option.some(layer),
                  )
                : Effect.succeed(Option.none<Layer>());
            const source = Option.match(selectedLayer, {
                onNone: () => ({ left: 0, top: 0, right: open.width, bottom: open.height }),
                onSome: Struct.get('boundsNoEffects'),
            });
            const bounds = Option.match(region, {
                onNone: () => source,
                onSome: ([x0, y0, x1, y1]) => ({
                    left: Math.max(Math.round(x0 * open.width), source.left),
                    top: Math.max(Math.round(y0 * open.height), source.top),
                    right: Math.min(Math.round(x1 * open.width), source.right),
                    bottom: Math.min(Math.round(y1 * open.height), source.bottom),
                }),
            });
            yield* Effect.fromOption(
                Option.liftPredicate(bounds, ({ left, top, right, bottom }) => left < right && top < bottom),
                () => HostRejection.cases.malformedParams.make({ cause: bounds }),
            );
            return yield* target.kind === 'selection' ? _mask(open, budget, bounds) : _composite(open, budget, Option.map(selectedLayer, Struct.get('id')), bounds);
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
        ({ imageData }) => Effect.sync(() => imageData.dispose()),
    );

const getDocument = ({ documentId, limit, cursor, depth }: Body<'getDocument'>): Effect.Effect<Reply<'getDocument'>, HostRejection> =>
    Effect.map(Option.match(documentId, { onNone: () => Effect.succeed(active(app)), onSome: flow(_found, Effect.map(Option.some)) }), (open) => {
        const rows = Option.map(open, (selected) => Array.fromIterable(_flat(selected.layers, depth, 0, Option.none())));
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

export { applyTypeStyles, batchPlay, composeLayers, execute, getDocument, getPreferences, listPresets, modal, runAction, setPreferences, snapshot };
