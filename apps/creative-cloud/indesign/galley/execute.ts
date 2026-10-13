import indesign from 'adobe:indesign';
import { HIGH_RESOLUTION } from '@rasm/ai/vision';
import { type AnyNode, getLineInfo, parse } from 'acorn';
import { fullAncestor } from 'acorn-walk';
import { Array, Cache, Deferred, Effect, flow, Match, MutableRef, Option, Predicate, Record, Result, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import MagicString from 'magic-string';
import { type Enter, type Execute, GalleyError, type Refusal, Specifier, Target } from './contract.ts';
import { planned } from './document.ts';
import { Callable, collection, encoded, host, LAST_STEP, Model, missing, plurals, prepared, read, record, resolve, scope, type Temporary, thrown, unwired, wired, write } from './dom.ts';
import { grep } from './find.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LINE_BREAK = /\r\n?|[\n\p{Zl}\p{Zp}]/u;

// --- [SERVICES] ------------------------------------------------------------------------

const entry = MutableRef.make(Option.none<() => void>());
const phase = MutableRef.make<Rpc.Success<typeof Enter>>('Idle');

// --- [OPERATIONS] ----------------------------------------------------------------------

const enter = Effect.sync(() => {
    const seen = MutableRef.get(phase);
    Option.map(MutableRef.getAndSet(entry, Option.none()), (run) => run());
    return seen;
});
const nested = <A>(step: () => A): Effect.Effect<A, GalleyError> => {
    const { app, ScriptLanguage } = indesign;
    const holds = 2048;
    const ran = MutableRef.make(Option.none<Result.Result<A, GalleyError>>());
    const run = (): void => {
        MutableRef.set(phase, 'Waiting');
        MutableRef.set(ran, Option.some(Result.try({ try: step, catch: thrown })));
        MutableRef.set(phase, 'Idle');
    };
    return Effect.try({
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
    }).pipe(Effect.tap(Effect.yieldNow), Effect.repeat({ until: Option.isSome, times: holds }), Effect.flatMap(Effect.fromOption(() => new GalleyError({ reason: { _tag: 'Unentered' } }))), Effect.flatMap(Effect.fromResult));
};
const ready = Effect.filterOrFail(
    Effect.sync(() => indesign.app.modalState),
    (modal) => !modal,
    () => new GalleyError({ reason: { _tag: 'Blocked' } }),
);
const rendering = Effect.fnUntraced(function* (preview: Rpc.Payload<typeof Execute>['preview']) {
    return yield* Effect.transposeOption(Option.map(Option.filter(preview, Schema.is(Target)), (target) => planned({ target, tokens: HIGH_RESOLUTION.tokens })));
});
const scripted = Effect.fn('scripted')(function* <A, R>(
    { description, preview }: Pick<Rpc.Payload<typeof Execute>, 'description' | 'preview'>,
    job: { readonly program: 'Set' | 'Find' | 'Queries' | 'Render'; readonly args: readonly unknown[]; readonly refusals: readonly (typeof Refusal.Type)[]; readonly notes: readonly string[]; readonly settle: (result: unknown) => Effect.Effect<A, GalleyError, R> },
) {
    const { app, ScriptLanguage, UndoModes } = indesign;
    const source = `(function (given) {
    var name = given[0], preview = given[1], rollback = given[2], jobs = decoded(given[3]), opened = {}, failure = null, refused = [], noted = [], outs = [], done = false, settings = app.scriptPreferences, kept = [settings.userInteractionLevel, settings.enableRedraw, settings.measurementUnit], at;
    function record(value) { return value !== null && typeof value === 'object' && value.constructor === Object; }
    function host(value) { return value !== null && typeof value === 'object' && typeof value.toSpecifier === 'function' && typeof value.everyItem !== 'function'; }
    function decoded(value) {
        var made, index;
        if (!(value instanceof Array)) return value;
        made = value[0] === '[' ? [] : {};
        for (index = 1; index < value.length; index++) if (value[0] === '[') made.push(decoded(value[index])); else { made[value[index]] = decoded(value[index + 1]); index++; }
        return made;
    }
    function wired(value) {
        var made, field, index;
        if (value instanceof Array) { made = ['[']; for (index = 0; index < value.length; index++) made.push(wired(value[index])); return made; }
        if (!record(value)) return host(value) ? value.toSpecifier() : value;
        made = ['{']; for (field in value) made.push(field, wired(value[field]));
        return made;
    }
    function key(value) {
        var parts = [], field, index;
        if (value instanceof Array) { for (index = 0; index < value.length; index++) parts.push(key(value[index])); return '[' + parts.join(',') + ']'; }
        if (record(value)) { for (field in value) parts.push(field + '=' + key(value[field])); return '{' + parts.join(',') + '}'; }
        return host(value) ? value.toSpecifier() : typeof value + ':' + String(value);
    }
    function located(value) {
        if (typeof value !== 'string') return [];
        try { return [resolve(value)]; } catch (missed) { return []; }
    }
    function put(owner, field, value, path, target, candidates) {
        var order = located(value), raw = order.length, failed = null, index;
        order.push(value);
        for (index = 0; index < candidates.length; index++) order.push($.global[candidates[index][0]][candidates[index][1]]);
        for (index = 0; index < order.length; index++) {
            try { owner[field] = order[index]; if (index > raw) noted.push([path, value, String(order[index])]); return; } catch (error) { if (index === raw) failed = error; }
        }
        refused.push([host(target) ? target.toSpecifier() : null, path, failed.name, failed.message]);
    }
    function owned(root, path) { for (var index = 0; index < path.length; index++) root = root[path[index]]; return root; }
    function written(rows, root) { for (var index = 0; index < rows.length; index++) put(owned(root, rows[index][0]), rows[index][1], rows[index][3], rows[index][2], root, rows[index][4]); }
    function shaped(owner, fields) {
        var made = {}, field, held;
        for (field in fields) { held = owner[field]; made[field] = record(fields[field]) && host(held) ? shaped(held, fields[field]) : held; }
        return made;
    }
    function properties(roles) { return [roles[0].properties, roles[1].properties, roles[2].properties]; }
    function clear(names) { for (var index = 0; index < 3; index++) app[names[index]] = NothingEnum.NOTHING; }
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
        Set: function (members, owner, field, rows, properties) {
            var fields = properties, adding = owner !== null, list = [], made = [], given = {}, index, each, row, member, before, after;
            for (index = 0; adding && index < rows[0].length; index++) { row = rows[0][index]; given[row[1]] = located(row[3]).concat([row[3]])[0]; }
            if (adding) list = [resolve(owner)[field].add(given)];
            for (index = 0; !adding && index < members.length; index++) list.push(resolve(members[index]));
            for (index = 0; index < list.length; index++) {
                member = list[index];
                before = shaped(member, fields);
                for (each = 0; each < rows[index].length; each++) {
                    row = rows[index][each];
                    if (!adding || key(before[row[1]]) !== key(given[row[1]])) put(owned(member, row[0]), row[1], row[3], row[2], member, row[4]);
                }
                after = shaped(member, fields);
                for (each in fields) made.push([member.toSpecifier(), each, adding ? null : wired(before[each]), wired(after[each])]);
            }
            return made;
        },
        Find: function (names, search, methods, targets, query, rows, changing, style, save, limit, reach) {
            var roles = [app[names[0]], app[names[1]], app[names[2]]], saved = properties(roles), cleared, held = [], hits, matches = [], paragraphs = [], seen = {}, altered = 0, index, field, hit, story, frames, page, first, expression, rule;
            clear(names);
            cleared = properties(roles);
            for (index = 0; index < 3; index++) held.push(differing(saved[index], cleared[index]));
            try {
                if (query !== null) app.loadFindChangeQuery(query, SearchModes[search]);
                for (index = 0; index < 3; index++) written(rows[index], roles[index]);
                if (style !== null) {
                    expression = roles[0].findWhat;
                    if (typeof expression !== 'string') { failure = ['Missing', 'findWhat of ' + names[0], 'findWhat']; return null; }
                    put(roles[0], 'appliedParagraphStyle', style[0], 'appliedParagraphStyle', roles[0], []);
                    for (index = 0; index < style[2].length; index++) {
                        rule = resolve(style[2][index]).nestedGrepStyles.add();
                        put(rule, 'grepExpression', expression, 'grepExpression', rule, []);
                        put(rule, 'appliedCharacterStyle', style[1], 'appliedCharacterStyle', rule, []);
                    }
                }
                if (refused.length > 0) return null;
                hits = searched(targets, methods[0]);
                for (index = 0; index < hits.length && (limit === null || index < limit); index++) {
                    hit = hits[index];
                    story = 'parentStory' in hit ? hit.parentStory : null;
                    frames = 'parentTextFrames' in hit ? hit.parentTextFrames : [hit];
                    page = frames.length > 0 && 'parentPage' in frames[0] ? frames[0].parentPage : null;
                    matches.push([hit.toSpecifier(), 'contents' in hit ? hit.contents : null, story === null ? null : story.characters.itemByRange(Math.max(0, hit.index - reach), Math.min(story.characters.length - 1, hit.index + hit.length + reach - 1)).contents, page === null ? null : page.name, 'appliedParagraphStyle' in hit ? wired(hit.appliedParagraphStyle) : null]);
                    if (changing && 'paragraphs' in hit) {
                        first = hit.paragraphs.item(0);
                        if (!seen[first.toSpecifier()]) { seen[first.toSpecifier()] = true; paragraphs.push([first.toSpecifier(), first.contents, first]); }
                    }
                }
                if (changing) altered = searched(targets, methods[1]).length;
                for (index = 0; index < paragraphs.length; index++) paragraphs[index][2] = paragraphs[index][2].contents;
                if (!preview && save !== null) app.saveFindChangeQuery(save, SearchModes[search]);
                return [wired(differing(roles[0].properties, cleared[0])), changing ? wired(differing(roles[1].properties, cleared[1])) : null, hits.length, altered, matches, paragraphs];
            } finally {
                clear(names);
                for (index = 0; index < 3; index++) for (field in held[index]) roles[index][field] = held[index][field];
            }
        },
        Queries: function (rows) {
            var made = [], index, roles, cleared;
            for (index = 0; index < rows.length; index++) {
                roles = [app[rows[index][0]], app[rows[index][1]], app[rows[index][2]]];
                clear(rows[index]);
                cleared = properties(roles);
                app.loadFindChangeQuery(rows[index][4], SearchModes[rows[index][3]]);
                made.push([wired(differing(roles[0].properties, cleared[0])), wired(differing(roles[1].properties, cleared[1])), wired(differing(roles[2].properties, cleared[2]))]);
            }
            return made;
        },
        Render: function (file, specifier, resolution, range, spreading, region) {
            var source = resolve(specifier), omitted = [], chosen = [], seen = {}, pages, page, master, own, from, frame, corner, leading, list, index, each, item;
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
                    for (each = list.length - 1; each >= 0; each--) if (list[each].constructor.name !== 'Guide') chosen.push([list[each], frame]);
                }
                list = source.pageItems.everyItem().getElements();
                for (each = list.length - 1; each >= 0; each--) chosen.push([list[each], region]);
                own = source.parent;
                source = app.documents.add(false);
                source.documentPreferences.properties = { facingPages: false, pageWidth: region.right - region.left, pageHeight: region.bottom - region.top };
                for (index = 1; index < own.layers.length; index++) source.layers.add();
                corner = box(source.pages.item(0));
                for (index = 0; index < chosen.length; index++) {
                    item = chosen[index][0];
                    frame = chosen[index][1];
                    if (seen[item.id] || !(item.itemLayer.visible && item.itemLayer.printable && item.visible && !item.nonprinting) || !touches(frame, item)) continue;
                    seen[item.id] = true;
                    try { item.duplicate(source.layers.item(item.itemLayer.index), [corner.left - frame.left, corner.top - frame.top]); } catch (error) { omitted.push([item.toSpecifier(), item.constructor.name, item.name, error.name, error.message]); }
                }
                range = '+1';
            }
            app.jpegExportPreferences.properties = { jpegQuality: JPEGOptionsQuality.HIGH, jpegColorSpace: JpegColorSpaceEnum.RGB, antiAlias: true, useDocumentBleeds: false, jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE, pageString: range, exportingSpread: spreading, exportResolution: resolution };
            source.exportFile(ExportFormat.JPG, new File(file));
            return omitted;
        }
    };
    for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
    settings.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
    settings.enableRedraw = false;
    settings.measurementUnit = MeasurementUnits.POINTS;
    try {
        app.doScript(function () {
            try { for (var step = 0; step < jobs.length && failure === null && refused.length === 0; step++) outs.push(programs[jobs[step][0]].apply(null, jobs[step][1])); } catch (error) { failure = ['Thrown', error.name, error.message]; }
            done = true;
            if (rollback || failure !== null || refused.length > 0) throw 0;
        }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT, name);
    } catch (error) {
        if (!done) throw error;
        for (at = app.documents.length - 1; at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.NO);
    } finally {
        settings.userInteractionLevel = kept[0];
        settings.enableRedraw = kept[1];
        settings.measurementUnit = kept[2];
    }
    return [failure, refused, noted, outs];
})(arguments)`;
    const Reply = Schema.Tuple([
        Schema.NullOr(Schema.Union([Schema.Tuple([Schema.Literal('Thrown'), Schema.String, Schema.String]), Schema.Tuple([Schema.Literal('Missing'), Schema.String, Schema.String])])),
        Schema.Array(Schema.Tuple([Schema.NullOr(Specifier), Schema.String, Schema.String, Schema.String])),
        Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])),
        Schema.Array(Schema.Unknown),
    ]);
    const error = (name: string, message: string): Error => Object.assign(new Error(message), { name });
    const shot = yield* rendering(preview);
    const jobs = [job, ...Option.toArray(shot)];
    yield* ready;
    const reply = yield* Effect.try({ try: (): unknown => app.doScript(source, ScriptLanguage.JAVASCRIPT, [description, Option.isSome(preview), Option.isSome(preview) || job.refusals.length > 0, wired(Array.map(jobs, ({ program, args }) => [program, args]))], UndoModes.ENTIRE_SCRIPT, description), catch: thrown });
    const [failure, refused, noted, [out, image]] = yield* Effect.orDie(Schema.decodeUnknownEffect(Reply)(reply));
    return yield* Array.match([...job.refusals, ...Array.map(refused, ([at, field, name, message]): typeof Refusal.Type => ({ _tag: 'Refused', target: Option.fromNullishOr(at), field, cause: error(name, message) }))], {
        onNonEmpty: (fields) => Effect.fail(new GalleyError({ reason: { _tag: 'Rejected', fields } })),
        onEmpty: () =>
            failure === null
                ? Effect.map(Effect.all([job.settle(out), Effect.transposeOption(Option.map(shot, (render) => render.settle(image)))]), ([value, seen]) => [value, [...job.notes, ...Array.map(noted, ([path, given, accepted]) => `${path} = ${JSON.stringify(given)} assigned ${accepted}`)], seen] as const)
                : Effect.fail(failure[0] === 'Thrown' ? thrown(error(failure[1], failure[2])) : missing(failure[1], failure[2], [])),
    });
});
const transact = Effect.fn('transact')(function* <A, R>({ description, preview }: Pick<Rpc.Payload<typeof Execute>, 'description' | 'preview'>, body: Effect.Effect<A, GalleyError, R>) {
    const { app, SaveOptions, ScriptLanguage, UndoModes } = indesign;
    yield* ready;
    yield* scope;
    const work = Effect.all([body, Effect.flatMap(rendering(preview), (shot) => Effect.transposeOption(Option.map(shot, (job) => Effect.map(scripted({ description, preview: Option.some(true) }, job), ([image]) => image))))]);
    const [services, settled, opened] = [yield* Effect.context<R | Model | Temporary>(), yield* Deferred.make<Effect.Success<typeof work>, GalleyError>(), app.documents.everyItem().id];
    yield* Effect.catch(
        nested(() =>
            app.doScript(
                () =>
                    Effect.andThen(
                        Effect.sync(() => MutableRef.set(phase, 'Running')),
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
const execute = ({ body, ...transaction }: Rpc.Payload<typeof Execute>): Effect.Effect<Rpc.Success<typeof Execute>, GalleyError, Model | Temporary> =>
    Match.valueTags(body, {
        Script: Effect.fnUntraced(function* ({ code }) {
            const [notes, runSync] = [new Set<string>(), Effect.runSyncWith(yield* Effect.context<Model>())];
            const applied = (): Rpc.Success<typeof Execute>['corrections'] => Option.liftPredicate(Array.fromIterable(notes), Array.isArrayNonEmpty);
            const failed = (cause: unknown, line: Option.Option<number>, kept: boolean): GalleyError => new GalleyError({ reason: { _tag: 'Thrown', cause, line, source: Option.flatMap(line, (at) => Array.get(code.split(LINE_BREAK), at - 1)), corrections: applied(), kept } });
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
                    const names = Array.filter(Array.dedupe(Array.flatMap(visits, ({ node }) => (node.type === 'Identifier' ? [node.name] : []))), (name) => Object.hasOwn(indesign, name) && !(name in globalThis));
                    const awaiting = Array.some(visits, ({ node, up }) => (node.type === 'AwaitExpression' || (node.type === 'ForOfStatement' && node.await)) && !Array.some(up, (above) => above.type === 'FunctionDeclaration' || above.type === 'FunctionExpression' || above.type === 'ArrowFunctionExpression'));
                    const [given, text] = [{ ...bound, ...Record.fromIterableWith(names, (name): [string, unknown] => [name, Reflect.get(indesign, name)]) }, spliced.prepend('{\n').append('\n}').toString()];
                    const Compiled: new (...parameters: readonly string[]) => (...values: readonly unknown[]) => unknown = Object.getPrototypeOf(awaiting ? async () => undefined : () => undefined).constructor;
                    const run = new Compiled(...Object.keys(given), text);
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
                    (value): Effect.Effect<PromiseLike<string | undefined> | string | undefined, GalleyError> =>
                        Predicate.isPromiseLike(value) ? Option.match(transaction.preview, { onNone: () => Effect.succeed(value.then((settled) => encoded(settled, true))), onSome: () => Effect.fail(new GalleyError({ reason: { _tag: 'Unsettled' } })) }) : Effect.succeed(encoded(value, true)),
                ),
            );
            const json = yield* Effect.tryPromise({ try: async () => await pending, catch: (cause) => failed(cause, located(cause), true) });
            const result = yield* Effect.orDie(Schema.decodeUnknownEffect(Schema.UndefinedOr(Schema.fromJsonString(Schema.Json)))(json));
            return { result: Option.fromUndefinedOr(result), corrections: applied(), preview };
        }),
        Set: Effect.fnUntraced(function* ({ target, properties, add }) {
            const model = yield* Model;
            const checks = grep(Option.liftPredicate(indesign.app.findGrepPreferences.findWhat, Predicate.isString));
            const into = add
                ? Option.some(
                      yield* Effect.gen(function* () {
                          const owners = Array.ensure(yield* resolve(target.replace(LAST_STEP, '') || '/'));
                          const held = yield* Effect.forEach(owners, (parent) =>
                              Effect.map(Effect.option(Cache.get(model, parent.constructorName)), (shape) =>
                                  Array.flatMap(Option.toArray(shape), ({ fields }) => Array.flatMap(Record.toEntries(fields), ([accessor, { type }]) => (type !== undefined && plurals.has(type) ? [{ owner: parent, name: accessor, items: parent[accessor] }] : []))),
                              ),
                          );
                          const found = yield* Effect.fromOption(
                              Array.findFirst(Array.flatten(held), (each) => (collection(each.items) && Schema.is(Schema.Struct({ add: Callable }))(each.items) && each.items.everyItem()?.toSpecifier() === target ? Option.some({ ...each, items: each.items }) : Option.none())),
                              () => missing(target, target, []),
                          );
                          const element = Option.flatMap(yield* Effect.option(Cache.get(model, found.items.constructorName)), ({ methods }) => Option.flatMapNullishOr(Record.get(methods, 'item'), ({ returns }) => returns?.type));
                          return { owner: found.owner, name: found.name, type: Option.getOrElse(element, () => '') };
                      }),
                  )
                : Option.none();
            const members = Option.isSome(into) ? [] : Array.ensure(yield* resolve(target));
            const screened = yield* Option.match(into, { onSome: ({ type }) => Effect.map(prepared(type, properties, checks), Array.of), onNone: () => Effect.forEach(members, (member) => prepared(member, properties, checks)) });
            const [rows, corrections, preview] = yield* scripted(transaction, {
                program: 'Set',
                args: [Array.map(members, (member) => member.toSpecifier()), ...Option.match(into, { onNone: () => [null, null], onSome: ({ owner, name }) => [owner.toSpecifier(), name] }), Array.map(screened, Struct.get('rows')), properties],
                refusals: Array.flatMap(screened, Struct.get('refusals')),
                notes: Array.flatMap(screened, Struct.get('notes')),
                settle: (result) =>
                    Effect.flatMap(Effect.orDie(Schema.decodeUnknownEffect(Schema.Array(Schema.Tuple([Schema.Unknown, Schema.String, Schema.Unknown, Schema.Unknown])))(result)), (written) =>
                        read(Schema.Array(Schema.Json), () => Array.map(written, ([member, field, before, after]) => ({ target: member, field, before: unwired(before), after: unwired(after) }))),
                    ),
            });
            return { result: Option.some(rows), corrections: Option.liftPredicate(corrections, Array.isReadonlyArrayNonEmpty), preview };
        }),
    }).pipe(Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { enter, execute, scripted };
