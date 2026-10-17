import indesign from 'adobe:indesign';
import type { Phase } from '@rasm/mcp/bridge';
import { HIGH_RESOLUTION } from '@rasm/mcp/vision';
import type { Temporary } from '@rasm/uxp/runtime';
import { type AnyNode, getLineInfo, parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Cache, Deferred, Effect, flow, identity, Match, MutableRef, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import MagicString from 'magic-string';
import { type Execute, InDesignError, missing, Refusal, type Specifier, Target, thrown } from './contract.ts';
import { planned } from './document.ts';
import { addable, collection, encoded, exported, host, LAST_STEP, Model, noted, plurals, prepared, read, record, resolve, write } from './dom.ts';
import { grep } from './find.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [MODELS] --------------------------------------------------------------------------

const Reply = Schema.Struct({ failure: Schema.NullOr(InDesignError.fields.reason), refused: Schema.Array(Refusal), noted: Schema.Array(Schema.Struct(Struct.Record(['name', 'given', 'accepted'], Schema.String))), ...Struct.Record(['out', 'image'], Schema.optionalKey(Schema.Unknown)) });

// --- [SERVICES] ------------------------------------------------------------------------

const entry = MutableRef.make(Option.none<() => void>());
const phase = MutableRef.make<typeof Phase.Type>('Idle');

// --- [OPERATIONS] ----------------------------------------------------------------------

const unwired = (value: unknown): unknown =>
    Match.value(value).pipe(
        Match.when(Schema.is(Schema.Array(Schema.Unknown)), ([tag, ...items]) => (tag === '[' ? Array.map(items, unwired) : Object.fromEntries(Array.map(Array.chunksOf(items, 2), ([field, held]) => [String(field), unwired(held)])))),
        Match.orElse(identity),
    );
const enter = Effect.sync((): typeof Phase.Type =>
    Option.match(MutableRef.getAndSet(entry, Option.none()), {
        onNone: () => MutableRef.get(phase),
        onSome: (run) => {
            run();
            return 'Entered';
        },
    }),
);
const nested = <A>(step: () => A): Effect.Effect<A, InDesignError> => {
    const { app, ScriptLanguage } = indesign;
    const holds = 2048;
    const ran = MutableRef.make(Option.none<Result.Result<A, InDesignError>>());
    const run = (): void => {
        MutableRef.set(phase, 'Held');
        MutableRef.set(ran, Option.some(Result.try({ try: step, catch: thrown })));
    };
    return Effect.sync(() => MutableRef.set(phase, 'Missed')).pipe(
        Effect.andThen(
            Effect.try({
                try: () => {
                    MutableRef.set(entry, Option.some(run));
                    try {
                        app.doScript('$.sleep(5)', ScriptLanguage.JAVASCRIPT);
                    } finally {
                        MutableRef.set(entry, Option.none());
                    }
                    return MutableRef.get(ran);
                },
                catch: thrown,
            }).pipe(Effect.tap(Effect.yieldNow), Effect.repeat({ until: Option.isSome, times: holds })),
        ),
        Effect.flatMap(Effect.fromOption(() => InDesignError.make({ reason: { _tag: 'Unentered' } }))),
        Effect.flatMap(Effect.fromResult),
        Effect.ensuring(Effect.sync(() => MutableRef.set(phase, 'Idle'))),
    );
};
const ready = Effect.filterOrFail(
    Effect.sync(() => indesign.app.modalState),
    (modal) => !modal,
    () => InDesignError.make({ reason: { _tag: 'Blocked' } }),
);
const rendering = (preview: Rpc.Payload<typeof Execute>['preview']): Option.Option<ReturnType<typeof planned>> => Option.map(Option.filter(preview, Schema.is(Target)), (target) => planned({ target, tokens: HIGH_RESOLUTION.tokens }));
const scripted = Effect.fn('scripted')(function* <A, R, R2>(
    { description, preview }: Pick<Rpc.Payload<typeof Execute>, 'description' | 'preview'>,
    planning: Effect.Effect<
        { readonly program: 'Set' | 'Find' | 'Queries' | 'Render'; readonly args: Record.ReadonlyRecord<string, unknown>; readonly refusals: readonly (typeof Refusal.Type)[]; readonly notes: readonly string[]; readonly settle: (result: unknown) => Effect.Effect<A, InDesignError, R> },
        InDesignError,
        R2
    >,
) {
    const source = `(function (wire) {
    var batch = eval('(' + wire + ')'), opened = {}, failure = null, refused = [], noted = [], out = null, image = null, reply = null, settings = app.scriptPreferences, wanted = { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS }, kept = {}, at;
    function record(value) { return value !== null && typeof value === 'object' && value.constructor === Object; }
    function host(value) { return value !== null && typeof value === 'object' && typeof value.toSpecifier === 'function' && typeof value.everyItem !== 'function'; }
    function mapped(list, made) { var out = [], index; for (index = 0; index < list.length; index++) out.push(made(list[index])); return out; }
    function wired(value) {
        var made = ['{'], field;
        if (value instanceof Array) return ['['].concat(mapped(value, wired));
        if (!record(value)) return host(value) ? value.toSpecifier() : value;
        for (field in value) if (value[field] !== undefined) made.push(field, wired(value[field]));
        return made;
    }
    function key(value) {
        var parts = [], field;
        if (value instanceof Array) return '[' + mapped(value, key).join(',') + ']';
        if (record(value)) { for (field in value) parts.push(field + '=' + key(value[field])); return '{' + parts.join(',') + '}'; }
        return host(value) ? value.toSpecifier() : typeof value + ':' + String(value);
    }
    function located(value) {
        if (typeof value !== 'string') return [];
        try { return [resolve(value)]; } catch (missed) { return []; }
    }
    function put(owner, field, value, name, target, candidates) {
        var order = located(value), raw = order.length, failed = null, index;
        order.push(value);
        for (index = 0; index < candidates.length; index++) order.push($.global[candidates[index].enumeration][candidates[index].member]);
        for (index = 0; index < order.length; index++) {
            try { owner[field] = order[index]; if (index > raw) noted.push({ name: name, given: value, accepted: String(order[index]) }); return; } catch (error) { if (index === raw) failed = error; }
        }
        refused.push({ _tag: 'Refused', target: host(target) ? target.toSpecifier() : undefined, field: name, cause: { name: failed.name, message: failed.message } });
    }
    function owned(root, path) { for (var index = 0; index < path.length; index++) root = root[path[index]]; return root; }
    function written(rows, root) { for (var index = 0; index < rows.length; index++) put(owned(root, rows[index].path), rows[index].field, rows[index].value, rows[index].name, root, rows[index].candidates); }
    function shaped(owner, fields) {
        var made = {}, field, held;
        for (field in fields) { held = owner[field]; made[field] = record(fields[field]) && host(held) ? shaped(held, fields[field]) : held; }
        return made;
    }
    function roles(names) { return mapped(names, function (name) { return app[name]; }); }
    function properties(held) { return mapped(held, function (role) { return role.properties; }); }
    function clear(names) { for (var index = 0; index < names.length; index++) app[names[index]] = NothingEnum.NOTHING; }
    function differing(current, base) {
        var made = {}, field;
        for (field in current) if (key(current[field]) !== key(base[field])) made[field] = current[field];
        return made;
    }
    function searched(targets, method) {
        var found = [], index, list, each;
        for (index = 0; index < targets.length; index++) { list = resolve(targets[index])[method](); for (each = 0; each < list.length; each++) if (host(list[each])) found.push(list[each]); }
        return found;
    }
    function box(item) {
        var ends = [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR], points = [], index;
        if (item.parent.constructor.name === 'Character') item.parent.parentStory.recompose();
        for (index = 0; index < ends.length; index++) points.push(item.resolve([ends[index], BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES)[0]);
        return { top: Math.min(points[0][1], points[1][1]), left: Math.min(points[0][0], points[1][0]), bottom: Math.max(points[0][1], points[1][1]), right: Math.max(points[0][0], points[1][0]) };
    }
    function touches(frame, item) {
        var parts = [item].concat(item.allPageItems), index, edge;
        for (index = 0; index < parts.length; index++) { edge = box(parts[index]); if (edge.left < frame.right && edge.right > frame.left && edge.top < frame.bottom && edge.bottom > frame.top) return true; }
        return false;
    }
    var programs = {
        Set: function (job) {
            var made = [], index, each, entry, row, member, before, after, given;
            for (index = 0; index < job.targets.length; index++) {
                entry = job.targets[index];
                given = {};
                for (each = 0; entry.owner !== undefined && each < entry.rows.length; each++) { row = entry.rows[each]; if (row.candidates.length === 0) given[row.field] = located(row.value).concat([row.value])[0]; }
                member = entry.owner === undefined ? resolve(entry.specifier) : resolve(entry.owner)[entry.field].add(given);
                before = shaped(member, job.properties);
                for (each = 0; each < entry.rows.length; each++) {
                    row = entry.rows[each];
                    if (entry.owner === undefined || key(before[row.field]) !== key(given[row.field])) put(owned(member, row.path), row.field, row.value, row.name, member, row.candidates);
                }
                after = shaped(member, job.properties);
                for (each in job.properties) made.push({ target: member.toSpecifier(), field: each, before: entry.owner === undefined ? before[each] : null, after: after[each] });
            }
            return made;
        },
        Find: function (job) {
            var held = roles(job.names), saved = properties(held), cleared, prior = [], hits, matches = [], changes = [], seen = {}, altered = 0, index, field, hit, story, frames, page, first, expression, rule;
            clear(job.names);
            cleared = properties(held);
            for (index = 0; index < held.length; index++) prior.push(differing(saved[index], cleared[index]));
            try {
                if (job.query !== null) app.loadFindChangeQuery(job.query, SearchModes[job.search]);
                for (index = 0; index < held.length; index++) written(job.rows[index], held[index]);
                if (job.style !== null) {
                    expression = held[0].findWhat;
                    if (typeof expression !== 'string') { failure = { _tag: 'Missing', subject: 'findWhat of ' + job.names[0], query: 'findWhat', candidates: [] }; return null; }
                    put(held[0], 'appliedParagraphStyle', job.style.paragraph, 'appliedParagraphStyle', held[0], []);
                    for (index = 0; index < job.style.owners.length; index++) {
                        rule = resolve(job.style.owners[index]).nestedGrepStyles.add();
                        put(rule, 'grepExpression', expression, 'grepExpression', rule, []);
                        put(rule, 'appliedCharacterStyle', job.style.character, 'appliedCharacterStyle', rule, []);
                    }
                }
                if (refused.length > 0) return null;
                hits = searched(job.targets, job.methods[0]);
                for (index = 0; index < hits.length && (job.limit === null || index < job.limit); index++) {
                    hit = hits[index];
                    story = 'parentStory' in hit && 'length' in hit ? hit.parentStory : null;
                    frames = 'parentTextFrames' in hit ? hit.parentTextFrames : [hit];
                    page = frames.length > 0 && 'parentPage' in frames[0] ? frames[0].parentPage : null;
                    matches.push({ specifier: hit.toSpecifier(), text: 'contents' in hit ? hit.contents : undefined, context: story === null ? undefined : story.characters.itemByRange(Math.max(0, hit.index - job.reach), Math.min(story.characters.length - 1, hit.index + hit.length + job.reach - 1)).contents, page: page === null ? undefined : page.name, paragraphStyle: 'appliedParagraphStyle' in hit ? hit.appliedParagraphStyle : undefined });
                    if (job.changing && 'paragraphs' in hit) {
                        first = hit.paragraphs.item(0);
                        if (!seen[first.toSpecifier()]) { seen[first.toSpecifier()] = true; changes.push({ specifier: first.toSpecifier(), before: first.contents, after: first }); }
                    }
                }
                if (job.changing) altered = searched(job.targets, job.methods[1]).length;
                for (index = 0; index < changes.length; index++) changes[index].after = changes[index].after.contents;
                if (!batch.rollback && job.save !== null) app.saveFindChangeQuery(job.save, SearchModes[job.search]);
                return { find: differing(held[0].properties, cleared[0]), change: job.changing ? differing(held[1].properties, cleared[1]) : undefined, count: { matches: hits.length, changes: altered }, matches: matches, changes: changes };
            } finally {
                clear(job.names);
                for (index = 0; index < held.length; index++) for (field in prior[index]) held[index][field] = prior[index][field];
            }
        },
        Queries: function (job) {
            var made = [], index, query, held, cleared;
            for (index = 0; index < job.queries.length; index++) {
                query = job.queries[index];
                held = roles(query.names);
                clear(query.names);
                cleared = properties(held);
                app.loadFindChangeQuery(query.name, SearchModes[query.search]);
                made.push({ name: query.name, mode: query.mode, origin: query.origin, find: differing(held[0].properties, cleared[0]), change: differing(held[1].properties, cleared[1]), options: differing(held[2].properties, cleared[2]) });
            }
            return made;
        },
        Render: function (job) {
            var source = resolve(job.source), region = job.region, omitted = [], chosen = [], seen = {}, pages, page, master, own, from, frame, corner, leading, list, index, each, item;
            function spine(collection) { var sides = collection.everyItem().side, count = 0, index; for (index = 0; index < sides.length; index++) if (sides[index] == leading) count++; return count; }
            if (region !== null) {
                pages = source.pages.everyItem().getElements();
                leading = source.parent.documentPreferences.pageBinding == PageBindingOptions.RIGHT_TO_LEFT ? PageSideOptions.RIGHT_HAND : PageSideOptions.LEFT_HAND;
                for (index = pages.length - 1; index >= 0; index--) {
                    page = pages[index];
                    master = page.appliedMaster;
                    if (master === null) continue;
                    own = box(page);
                    from = box(master.pages.item(Math.max(0, Math.min(page.index - spine(source.pages) + spine(master.pages), master.pages.length - 1))));
                    frame = { top: region.top + from.top - own.top, left: region.left + from.left - own.left, bottom: region.bottom + from.top - own.top, right: region.right + from.left - own.left };
                    list = page.masterPageItems;
                    for (each = list.length - 1; each >= 0; each--) if (list[each].constructor.name !== 'Guide') chosen.push({ item: list[each], frame: frame });
                }
                list = source.pageItems.everyItem().getElements();
                for (each = list.length - 1; each >= 0; each--) chosen.push({ item: list[each], frame: region });
                own = source.parent;
                source = app.documents.add(false);
                source.documentPreferences.properties = { facingPages: false, pageWidth: region.right - region.left, pageHeight: region.bottom - region.top };
                for (index = 1; index < own.layers.length; index++) source.layers.add();
                corner = box(source.pages.item(0));
                for (index = 0; index < chosen.length; index++) {
                    item = chosen[index].item;
                    frame = chosen[index].frame;
                    if (seen[item.id] || !(item.itemLayer.visible && item.itemLayer.printable && item.visible && !item.nonprinting) || !touches(frame, item)) continue;
                    seen[item.id] = true;
                    try { item.duplicate(source.layers.item(item.itemLayer.index), [corner.left - frame.left, corner.top - frame.top]); } catch (error) { omitted.push({ type: item.constructor.name, specifier: item.toSpecifier(), name: item.name, cause: { name: error.name, message: error.message } }); }
                }
            }
            app.jpegExportPreferences.properties = { jpegQuality: JPEGOptionsQuality.HIGH, jpegColorSpace: JpegColorSpaceEnum.RGB, antiAlias: true, useDocumentBleeds: false, jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE, pageString: region === null ? job.range : '+1', exportingSpread: job.spreading, exportResolution: job.resolution };
            source.exportFile(ExportFormat.JPG, new File(job.file));
            return omitted.length > 0 ? omitted : undefined;
        }
    };
    for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
    for (at in wanted) { kept[at] = settings[at]; settings[at] = wanted[at]; }
    try {
        app.doScript(function () {
            try {
                out = programs[batch.job.program](batch.job.args);
                if (failure === null && refused.length === 0 && batch.render !== null) image = programs.Render(batch.render);
            } catch (error) { failure = { _tag: 'Script', cause: { name: error.name, message: error.message }, kept: false }; }
            reply = wired({ failure: failure, refused: refused, noted: noted, out: out, image: image });
            if (batch.rollback || failure !== null || refused.length > 0) throw 0;
        }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT, batch.name);
    } catch (error) {
        if (reply === null) throw error;
        for (at = app.documents.length - 1; at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.NO);
    } finally {
        for (at in kept) settings[at] = kept[at];
    }
    return reply;
})(arguments[0])`;
    const [job, shot] = yield* Effect.all([planning, Effect.transposeOption(rendering(preview))]);
    yield* ready;
    const wire = yield* Effect.orDie(Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Unknown))({ name: description, rollback: Option.isSome(preview) || job.refusals.length > 0, job: Struct.pick(job, ['program', 'args']), render: Option.getOrNull(Option.map(shot, Struct.get('args'))) }));
    const reply = yield* Effect.try({
        try: (): unknown => indesign.app.doScript(source, indesign.ScriptLanguage.JAVASCRIPT, [wire.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')], indesign.UndoModes.ENTIRE_SCRIPT, description),
        catch: thrown,
    });
    const { failure, refused, noted: assigned, out, image } = yield* Effect.orDie(Schema.decodeUnknownEffect(Reply)(unwired(reply)));
    return yield* Array.match([...job.refusals, ...refused], {
        onNonEmpty: (fields) => Effect.fail(InDesignError.make({ reason: { _tag: 'Rejected', fields } })),
        onEmpty: () =>
            failure === null
                ? Effect.map(Effect.all({ result: job.settle(out), preview: Effect.transposeOption(Option.map(shot, (render) => render.settle(image))) }), (settled) => ({
                      ...settled,
                      corrections: Option.liftPredicate([...job.notes, ...Array.map(assigned, ({ name, given, accepted }) => noted(name, given, accepted))], Array.isArrayNonEmpty),
                  }))
                : Effect.fail(InDesignError.make({ reason: failure })),
    });
});
const transact = Effect.fn('transact')(function* <A, R>({ description, preview }: Pick<Rpc.Payload<typeof Execute>, 'description' | 'preview'>, body: Effect.Effect<A, InDesignError, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    yield* ready;
    const work = Effect.all([body, Effect.transposeOption(Option.map(rendering(preview), (job) => Effect.map(scripted({ description, preview: Option.some(true) }, job), Struct.get('result'))))]);
    const [services, settled, opened] = [yield* Effect.context<R | Model | Temporary>(), yield* Deferred.make<Effect.Success<typeof work>, InDesignError>(), app.documents.everyItem().id];
    yield* Effect.catch(
        nested(() =>
            app.doScript(
                () =>
                    Effect.andThen(
                        Effect.sync(() => MutableRef.set(phase, 'Entered')),
                        Effect.exit(work),
                    ).pipe(
                        Effect.tap((exit) => Deferred.done(settled, exit)),
                        Effect.flatMap((exit) => (Option.isSome(preview) ? Effect.interrupt : exit)),
                        Effect.runSyncWith(services),
                    ),
                ScriptLanguage.UXPSCRIPT,
                [],
                UndoModes.ENTIRE_SCRIPT,
                description,
            ),
        ),
        (error) =>
            Effect.andThen(
                Deferred.fail(settled, error),
                Effect.sync(() => Array.forEach(Array.difference(app.documents.everyItem().id, opened), (id) => app.documents.itemByID(id).close(SaveOptions.NO))),
            ),
    );
    return yield* Deferred.await(settled);
});
const execute = ({ body, ...transaction }: Rpc.Payload<typeof Execute>): Effect.Effect<Rpc.Success<typeof Execute>, InDesignError, Model | Temporary> =>
    Match.valueTags(body, {
        Script: Effect.fnUntraced(function* ({ code }) {
            const [notes, runSync] = [new Set<string>(), Effect.runSyncWith(yield* Effect.context<Model>())];
            const applied = (): Rpc.Success<typeof Execute>['corrections'] => Option.liftPredicate(Array.fromIterable(notes), Array.isArrayNonEmpty);
            const failed = (cause: unknown, line: Option.Option<number>, kept: boolean): InDesignError => InDesignError.make({ reason: { _tag: 'Script', cause, line, source: Option.flatMap(line, (at) => Array.get(code.split(LINE_BREAK), at - 1)), corrections: applied(), kept } });
            const corrected = (at: string, target: object, values: Record.ReadonlyRecord<string, unknown>): void => Array.forEach(runSync(write(target, values, grep(Option.liftPredicate(indesign.app.findGrepPreferences.findWhat, Predicate.isString)))), (text) => notes.add(`${at}.${text}`));
            const bound = {
                __item: (at: string, target: { readonly [name: PropertyKey]: unknown }, key: PropertyKey): unknown => {
                    if (collection(target) && !Predicate.isSymbol(key) && !(key in target)) {
                        notes.add(at);
                        return target.item(key);
                    }
                    return target[key];
                },
                __set: (at: string, target: object, key: PropertyKey, value: unknown): unknown => {
                    if (Predicate.isSymbol(key) || !host(target)) {
                        Reflect.set(target, key, value);
                    } else {
                        corrected(at, target, key === 'properties' && record(value) ? value : { [key]: value });
                    }
                    return value;
                },
                __add: (at: string, target: { readonly add: (...values: readonly unknown[]) => unknown }, ...values: readonly unknown[]): unknown => {
                    const created = target.add(...values);
                    if (host(created)) {
                        Array.forEach(Array.filter(values, record), (properties) => corrected(at, created, properties));
                    }
                    return created;
                },
                resolve: flow(resolve, runSync),
            };
            const compiled = yield* Effect.try({
                try: () => {
                    const [visits, spliced]: [{ readonly node: AnyNode; readonly up: readonly AnyNode[] }[], MagicString] = [[], new MagicString(code)];
                    fullAncestor(parse(code, { ecmaVersion: 2024, preserveParens: true, allowReturnOutsideFunction: true, allowAwaitOutsideFunction: Option.isNone(transaction.preview) }), (node, _state, ancestors) => {
                        visits.push({ node, up: Array.filter(Array.drop(Array.reverse(ancestors), 1), (above) => above.type !== 'ParenthesizedExpression') });
                    });
                    const source = (node: AnyNode): string => code.slice(node.start, node.end);
                    const line = (at: number): number => getLineInfo(code, at).line;
                    const bare = (node: AnyNode): AnyNode => (node.type === 'ParenthesizedExpression' ? bare(node.expression) : node);
                    const optional = (node: AnyNode): boolean => (node.type === 'MemberExpression' && (node.optional || optional(node.object))) || (node.type === 'CallExpression' && (node.optional || optional(node.callee)));
                    const written = (node: AnyNode, parent: AnyNode | undefined, grandparent: AnyNode | undefined): boolean =>
                        (parent?.type === 'AssignmentExpression' && bare(parent.left) === node) ||
                        ((parent?.type === 'CallExpression' || parent?.type === 'NewExpression') && bare(parent.callee) === node) ||
                        (parent?.type === 'TaggedTemplateExpression' && bare(parent.tag) === node) ||
                        ((parent?.type === 'ForInStatement' || parent?.type === 'ForOfStatement') && bare(parent.left) === node) ||
                        (parent?.type === 'AssignmentPattern' && parent.left === node) ||
                        (parent?.type === 'UnaryExpression' && parent.operator === 'delete') ||
                        parent?.type === 'UpdateExpression' ||
                        parent?.type === 'ArrayPattern' ||
                        parent?.type === 'RestElement' ||
                        grandparent?.type === 'ObjectPattern';
                    const wrap = (node: AnyNode, call: keyof typeof bound, note: string, swaps: readonly (readonly [from: number, to: number, text: string])[]): void => {
                        spliced.prependRight(node.start, `${call}(${JSON.stringify(`line ${line(node.start)}: ${note}`)}, `);
                        Array.forEach(swaps, ([start, end, put]) => spliced.update(start, end, `${put}${'\n'.repeat(line(end) - line(start))}`));
                        spliced.appendLeft(node.end, ')');
                    };
                    Array.forEach(visits, ({ node, up: [parent, grandparent] }) => {
                        if (node.type === 'MemberExpression' && node.computed && node.object.type !== 'Super' && !optional(node) && !written(node, parent, grandparent)) {
                            wrap(node, '__item', `${source(node)} read as ${source(node.object)}.item(${source(node.property)})`, [
                                [node.object.end, node.property.start, ', '],
                                [node.property.end, node.end, ''],
                            ]);
                        }
                        if (node.type === 'AssignmentExpression' && node.operator === '=' && node.left.type === 'MemberExpression' && node.left.object.type !== 'Super' && node.left.property.type !== 'PrivateIdentifier') {
                            const { object, property, computed } = node.left;
                            wrap(
                                node,
                                '__set',
                                source(object),
                                computed
                                    ? [
                                          [object.end, property.start, ', '],
                                          [property.end, node.right.start, ', '],
                                      ]
                                    : [[object.end, node.right.start, `, ${JSON.stringify(source(property))}, `]],
                            );
                        }
                        if (node.type === 'CallExpression' && node.callee.type === 'MemberExpression' && !node.callee.computed && node.callee.object.type !== 'Super' && node.callee.property.type === 'Identifier' && node.callee.property.name === 'add' && !optional(node)) {
                            const from = node.callee.object.end;
                            wrap(
                                node,
                                '__add',
                                `${source(node.callee)}(…)`,
                                Array.match(node.arguments, {
                                    onEmpty: () => [[from, node.end, '']],
                                    onNonEmpty: (passed) => [
                                        [from, Array.headNonEmpty(passed).start, ', '],
                                        [Array.lastNonEmpty(passed).end, node.end, ''],
                                    ],
                                }),
                            );
                        }
                    });
                    const used = new Set(Array.flatMap(visits, ({ node }) => (node.type === 'Identifier' ? [node.name] : [])));
                    const awaiting = Array.some(visits, ({ node, up }) => (node.type === 'AwaitExpression' || (node.type === 'ForOfStatement' && node.await)) && !Array.some(up, (above) => above.type === 'FunctionDeclaration' || above.type === 'FunctionExpression' || above.type === 'ArrowFunctionExpression'));
                    const [given, text] = [{ ...bound, ...Object.fromEntries(Array.filter(exported, ([name]) => used.has(name) && !(name in globalThis))) }, spliced.prepend('{\n').append('\n}').toString()];
                    const run = Reflect.construct(Function, [...Object.keys(given), awaiting ? `return (async () => ${text})();` : text]);
                    return { offset: run.toString().slice(0, run.toString().indexOf(text)).split('\n').length, run: (): unknown => run(...Object.values(given)) };
                },
                catch: (cause) =>
                    failed(
                        cause,
                        Option.map(Schema.decodeUnknownOption(Schema.Struct({ loc: Schema.Struct({ line: Schema.Int }) }))(cause), ({ loc }) => loc.line),
                        false,
                    ),
            });
            const located = (cause: unknown): Option.Option<number> => {
                const prepare = Error.prepareStackTrace;
                Error.prepareStackTrace = (_error, frames): number | null =>
                    Option.getOrNull(
                        Option.flatMapNullishOr(
                            Array.findFirst(frames, (frame) => frame.isEval()),
                            (frame) => frame.getLineNumber(),
                        ),
                    );
                try {
                    return Option.map(Option.liftPredicate(Predicate.hasProperty(cause, 'stack') ? cause.stack : undefined, Predicate.isNumber), (line) => line - compiled.offset);
                } finally {
                    Error.prepareStackTrace = prepare;
                }
            };
            const [pending, preview] = yield* transact(
                transaction,
                Effect.flatMap(
                    Effect.try({ try: compiled.run, catch: (cause) => failed(cause, located(cause), false) }),
                    (value): Effect.Effect<PromiseLike<string | undefined> | string | undefined, InDesignError> =>
                        Predicate.isPromiseLike(value) ? Option.match(transaction.preview, { onNone: () => Effect.succeed(value.then((settled) => encoded(settled, true))), onSome: () => Effect.fail(InDesignError.make({ reason: { _tag: 'Unsettled' } })) }) : Effect.succeed(encoded(value, true)),
                ),
            );
            const json = yield* Effect.tryPromise({ try: () => Promise.resolve(pending), catch: (cause) => failed(cause, located(cause), true) });
            const result = yield* Effect.orDie(Schema.decodeEffect(Schema.UndefinedOr(Schema.fromJsonString(Schema.Json)))(json));
            return { result: Option.fromUndefinedOr(result), corrections: applied(), preview };
        }),
        Set: Effect.fnUntraced(function* ({ target, properties, add }) {
            const model = yield* Model;
            const checks = grep(Option.liftPredicate(indesign.app.findGrepPreferences.findWhat, Predicate.isString));
            const owned = Effect.gen(function* () {
                const owners = Array.ensure(yield* resolve(target.replace(LAST_STEP, '') || '/'));
                const held = yield* Effect.forEach(owners, (parent) =>
                    Effect.map(Effect.option(Cache.get(model, parent.constructorName)), (shape) =>
                        Array.flatMap(Option.toArray(shape), ({ fields }) => Array.flatMap(Record.toEntries(fields), ([accessor, { type }]) => (type !== undefined && plurals.has(type) ? [{ owner: parent, name: accessor, items: parent[accessor] }] : []))),
                    ),
                );
                const found = yield* Effect.fromOption(
                    Array.findFirst(Array.flatten(held), (each) => (addable(each.items) && each.items.everyItem()?.toSpecifier() === target ? Option.some({ ...each, items: each.items }) : Option.none())),
                    () => missing(target, target, []),
                );
                const element = Option.flatMap(yield* Effect.option(Cache.get(model, found.items.constructorName)), ({ methods }) => Option.flatMapNullishOr(Record.get(methods, 'item'), ({ returns }) => returns?.type));
                return [
                    {
                        owner: found.owner.toSpecifier(),
                        field: found.name,
                        ...(yield* prepared(
                            Option.getOrElse(element, () => ''),
                            properties,
                            checks,
                        )),
                    },
                ];
            });
            const listed = Effect.flatMap(resolve(target), (found) => Effect.forEach(Array.ensure(found), (member) => Effect.map(prepared(member, properties, checks), (fields) => ({ specifier: member.toSpecifier(), ...fields }))));
            const screened: readonly (Effect.Success<ReturnType<typeof prepared>> & ({ readonly specifier: typeof Specifier.Type } | { readonly owner: typeof Specifier.Type; readonly field: string }))[] = yield* add ? owned : listed;
            return yield* scripted(
                transaction,
                Effect.succeed({
                    program: 'Set' as const,
                    args: { targets: Array.map(screened, Struct.omit(['refusals', 'notes'])), properties },
                    refusals: Array.flatMap(screened, Struct.get('refusals')),
                    notes: Array.flatMap(screened, Struct.get('notes')),
                    settle: (result) => Effect.asSome(read(Schema.Array(Schema.Json), () => result)),
                }),
            );
        }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { enter, execute, scripted };
