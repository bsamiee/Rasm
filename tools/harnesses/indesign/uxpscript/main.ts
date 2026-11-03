import fs from 'adobe:fs';
import indesign, { type ScriptLanguage } from 'adobe:indesign';
import uxp from 'adobe:uxp';
import { Encoded, type fields, type HostCase, Outcome, Recorded, Reply } from '@rasm/harnesses/indesign/contract';
import { bury, JsonObject, type Reference, revive } from '@rasm/harnesses/values';
import { Equal, identity, Match, Option, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Node {
    [key: string]: unknown;
}
type Item = typeof HostCase.Type;
type Batch = typeof Encoded.Type;
type Records = { -readonly [K in keyof typeof fields]: (typeof fields)[K]['Type'][number][] };
interface Codec {
    readonly encoded: (value: unknown) => Schema.Json;
    readonly record: (value: Record.ReadonlyRecord<string, unknown>) => typeof JsonObject.Type;
    readonly revived: (value: Schema.Json) => unknown;
}
interface Session {
    readonly item: Item;
    readonly batch: Batch;
    readonly codec: Codec;
    readonly documents: unknown[];
    readonly confirms: boolean[];
    readonly prompts: (string | null)[];
    readonly files: (string | null)[];
    readonly actions: Node[];
    readonly dialogs: NonNullable<Item['dialogs']>[number][];
    readonly record: Records;
    value: Option.Option<unknown>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const STACK_LINE = /<anonymous>:(?<line>\d+):\d+/gu;
const LIVE = ['activeDocument', 'activeWindow', 'layoutWindows', 'windows', 'selection'] as const;
const VALUED: Record.ReadonlyRecord<string, string> = {
    textEditboxes: 'editContents',
    measurementEditboxes: 'editValue',
    integerEditboxes: 'editValue',
    realEditboxes: 'editValue',
    percentEditboxes: 'editValue',
    angleEditboxes: 'editValue',
    comboboxes: 'editContents',
    integerComboboxes: 'editValue',
    realComboboxes: 'editValue',
    percentComboboxes: 'editValue',
    angleComboboxes: 'editValue',
    measurementComboboxes: 'editValue',
    dropdowns: 'selectedIndex',
    checkboxControls: 'checkedState',
    radiobuttonGroups: 'selectedButton',
    enablingGroups: 'checkedState',
};
const CONTAINERS = ['dialogColumns', 'dialogRows', 'borderPanels', 'enablingGroups', 'radiobuttonGroups', 'staticTexts', 'radiobuttonControls', ...Object.keys(VALUED)];

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.Struct({ script: Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }) });
const same = Schema.toEquivalence(Schema.Json);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VALUES]
const { app } = indesign;
const names = Object.getOwnPropertyNames(indesign).filter((name) => name !== 'app');
const field = (target: unknown, key: PropertyKey): unknown => Reflect.get(new Object(target), key);
const message = (error: unknown) => (Predicate.hasProperty(error, 'message') ? String(error.message) : String(error));
const invoked = (target: unknown, key: string, args: readonly unknown[]): unknown => {
    const method = field(target, key);
    if (!Predicate.isFunction(method)) {
        throw new TypeError(`${key} is not a function`);
    }
    return Reflect.apply(method, target, args);
};
const items = (value: unknown): readonly unknown[] => (Array.isArray(value) ? value : []);
const strings = (value: unknown): readonly string[] => Option.getOrElse(Schema.decodeUnknownOption(Schema.Array(Schema.String))(value), () => []);
const elements = (list: unknown) => items(invoked(invoked(list, 'everyItem', []), 'getElements', []));
const called = (made: unknown, args: readonly unknown[]): unknown => Reflect.apply(Option.getOrThrow(Option.liftPredicate(made, Predicate.isFunction)), undefined, args);
const enumerator = (value: unknown): value is object => value instanceof indesign.Enumerator;
const equal = (left: unknown, right: unknown) => enumerator(left) && enumerator(right) && invoked(left, 'equals', [right]) === true;
const members = (enumeration: unknown) => Object.getOwnPropertyNames(Object.getPrototypeOf(enumeration)).filter((key) => enumerator(field(enumeration, key)) && String(field(enumeration, key)) === key);
const codecOf = (enumerations: Batch['enumerations']): Codec => {
    const owners = new Map<string, readonly string[]>();
    Object.entries(enumerations).forEach(([enumeration, listed]) => listed.forEach((member) => owners.set(member, [...(owners.get(member) ?? []), enumeration])));
    const qualified = (held: object) => {
        const name = String(invoked(held, 'toString', []));
        const holding = (owners.get(name) ?? []).filter((enumeration) => equal(field(field(indesign, enumeration), name), held));
        return Option.match(
            Option.filter(Option.fromUndefinedOr(holding[0]), () => holding.length === 1),
            { onNone: () => `enum:${name}`, onSome: (owner) => `enum:${owner}.${name}` },
        );
    };
    const named = (held: object): Option.Option<string> =>
        Match.value(held).pipe(
            Match.when(enumerator, qualified),
            Match.when(
                (value: object) => Predicate.isFunction(field(value, 'toSpecifier')),
                (value) => `object:${String(invoked(value, 'toSpecifier', []))}`,
            ),
            Match.option,
        );
    const resolved = (path: string): unknown => {
        const dot = path.indexOf('.');
        const candidates = dot < 0 ? (owners.get(path) ?? []).map((enumeration) => field(field(indesign, enumeration), path)).filter(enumerator) : [field(field(indesign, path.slice(0, dot)), path.slice(dot + 1))].filter(enumerator);
        const [first] = candidates;
        return Option.getOrThrowWith(
            Option.filter(Option.fromUndefinedOr(first), (held) => candidates.every((other) => equal(other, held))),
            () => new Error(`enum:${path} names ${first === undefined ? 'no enumerator' : `enumerators of ${(owners.get(path) ?? []).join(', ')} with different codes`}`),
        );
    };
    const located = ({ kind, path }: Reference): unknown =>
        Match.value(kind).pipe(
            Match.when('enum', () => resolved(path)),
            Match.when('object', () => app.doScript('resolve(arguments[0])', indesign.ScriptLanguage.JAVASCRIPT, [path])),
            Match.orElse(() => ({ $: `${kind}:${path}` })),
        );
    const encoded = (value: unknown) => bury(value, named);
    return { encoded, record: Record.map(encoded), revived: (value) => revive(value, located) };
};
const compiled = (parameters: readonly string[], code: string) => {
    // biome-ignore lint/nursery/noImpliedEval: case data and originals compile inside the host
    const made = new Function(...parameters, code);
    return (...args: readonly unknown[]): unknown => Reflect.apply(made, undefined, args);
};
const lines = (error: unknown) => Array.from(String(field(error, 'stack') ?? '').matchAll(STACK_LINE), (found) => Number(found.groups?.line));
const HEAD = (() => {
    try {
        called(compiled([], 'return function () {throw new Error();\n};')(), []);
        return 0;
    } catch (error) {
        return (lines(error)[0] ?? 1) - 1;
    }
})();
const scope = (overrides: Node) => {
    const keys = [...names.filter((name) => !Object.hasOwn(overrides, name)), ...Object.keys(overrides)];
    return { keys, values: keys.map((key) => (Object.hasOwn(overrides, key) ? overrides[key] : Reflect.get(indesign, key))) };
};
const shadow = <T extends object>(target: T, get: (key: PropertyKey, value: unknown) => unknown, set: (key: PropertyKey, value: unknown) => boolean = (key, value) => Reflect.set(target, key, value)) =>
    new Proxy(target, {
        get: (held, key) => get(key, Reflect.get(held, key)),
        set: (_, key, value) => set(key, value),
    });

// --- [STATE]
const owned = (value: unknown) => Predicate.isObject(value) && Predicate.isFunction(field(value, 'everyItem'));
const keyOf = (list: unknown): Option.Option<string> =>
    Option.fromUndefinedOr(
        ['id', 'name'].find((key) => {
            try {
                return Array.isArray(field(invoked(list, 'everyItem', []), key));
            } catch {
                return false;
            }
        }),
    );
const preferences = (target: unknown) => Object.keys(Object.getOwnPropertyDescriptors(Object.getPrototypeOf(target))).filter((name) => name !== 'parent' && name !== 'properties' && !Array.from<string>(LIVE).includes(name));
const pinned = <A>(work: () => A): A => {
    const unit = app.scriptPreferences.measurementUnit;
    app.scriptPreferences.measurementUnit = indesign.MeasurementUnits.POINTS;
    try {
        return work();
    } finally {
        app.scriptPreferences.measurementUnit = unit;
    }
};
const survey = () => {
    const settings = app.colorSettings;
    return {
        enumerations: Object.fromEntries(names.filter((name) => Reflect.get(indesign, name) instanceof indesign.Enumeration).map((name) => [name, members(Reflect.get(indesign, name))])),
        cmyk: settings.workingSpaceCMYKList,
        rgb: settings.workingSpaceRGBList,
        languages: Result.getOrThrow(Schema.decodeUnknownResult(Schema.Array(Schema.String))(field(invoked(app.languagesWithVendors, 'everyItem', []), 'name'))),
        host: { locale: String(app.locale), featureSet: String(app.featureSet), dom: String(app.scriptPreferences.version) },
    };
};
const scanned = () => {
    const preferred: string[] = [];
    const collections: { readonly name: string; readonly key: string }[] = [];
    const unread: string[] = [];
    const found = { preferences: preferred, collections, unread };
    preferences(app)
        .filter((name) => name !== 'scriptPreferences')
        .forEach((name) => {
            try {
                const value = field(app, name);
                if (owned(value)) {
                    const every = invoked(value, 'everyItem', []);
                    const closes = Predicate.isFunction(field(every, 'close')) || !Predicate.isFunction(field(every, 'remove'));
                    Option.match(keyOf(value), { onNone: () => (closes ? undefined : found.unread.push(`${name}: items hold neither id nor name`)), onSome: (key) => (closes ? undefined : found.collections.push({ name, key })) });
                } else if (Predicate.isObject(value) && !Array.isArray(value) && !('id' in value) && Predicate.isObject(field(value, 'properties'))) {
                    found.preferences.push(name);
                }
            } catch (error) {
                found.unread.push(`${name}: ${message(error)}`);
            }
        });
    return { ...found, preferences: [...found.preferences, 'scriptPreferences'] };
};
const bound = (value: Schema.Json) => Predicate.isObject(value) && !Array.isArray(value) && String(field(value, '$')).startsWith('object:/document');
const snapshot = (found: ReturnType<typeof scanned>, { encoded }: Codec) =>
    pinned(() => ({
        preferences: found.preferences.map((name) => {
            const target = field(app, name);
            const saved = Object.entries(new Object(field(target, 'properties'))).filter(([key, value]) => key !== 'measurementUnit' && !bound(encoded(value)));
            return { name, target, saved, encoded: new Map(saved.map(([key, value]) => [key, encoded(value)])) };
        }),
        collections: found.collections.map(({ name, key }) => ({ name, key, list: field(app, name), known: new Set(items(field(invoked(field(app, name), 'everyItem', []), key))) })),
    }));
const differences = (held: ReturnType<typeof snapshot>, { encoded }: Codec) =>
    pinned(() => [
        ...held.preferences.flatMap(({ name, target, saved, encoded: was }) => {
            const current = field(target, 'properties');
            return saved
                .filter(([key]) => !(bound(encoded(field(current, key))) || Equal.equals(encoded(field(current, key)), was.get(key))))
                .map(([key, before]) => ({ member: `${name}.${key}`, before, after: field(current, key), undo: () => (name === 'scriptPreferences' ? Reflect.set(new Object(target), key, before) : undefined) }));
        }),
        ...held.collections.flatMap(({ name, key, list, known }) =>
            items(field(invoked(list, 'everyItem', []), key))
                .filter((id) => !known.has(id))
                .map((id) => {
                    const item = invoked(list, key === 'id' ? 'itemByID' : 'itemByName', [id]);
                    return { member: name, before: undefined, after: item, undo: () => invoked(item, 'remove', []) };
                }),
        ),
    ]);
const restore = (held: ReturnType<typeof snapshot>, codec: Codec): readonly string[] =>
    pinned(() => [
        ...differences(held, codec).flatMap(({ member, undo }) => {
            try {
                undo();
                return [];
            } catch (error) {
                return [`${member}: ${message(error)}`];
            }
        }),
        ...differences(held, codec).map(({ member }) => member),
    ]);
const unchanged = (was: PropertyDescriptor | undefined, now: PropertyDescriptor | undefined) => was !== undefined && now !== undefined && was.value === now.value && was.get === now.get && was.set === now.set;
const reset = (owner: object, key: PropertyKey, was: PropertyDescriptor | undefined): readonly string[] => {
    try {
        return (was === undefined ? Reflect.deleteProperty(owner, key) : Reflect.defineProperty(owner, key, was)) ? [] : [`global ${String(key)}`];
    } catch (error) {
        return [`global ${String(key)}: ${message(error)}`];
    }
};
const globals = () => {
    const owners: object[] = [globalThis, Object.prototype, Array.prototype, String.prototype, Number.prototype, Boolean.prototype, Function.prototype, Promise.prototype, Error.prototype];
    const held = owners.map((owner) => ({ owner, descriptors: new Map(Reflect.ownKeys(owner).map((key) => [key, Object.getOwnPropertyDescriptor(owner, key)] as const)) }));
    return (): readonly string[] => held.flatMap(({ owner, descriptors }) => Reflect.ownKeys(owner).flatMap((key) => (unchanged(descriptors.get(key), Object.getOwnPropertyDescriptor(owner, key)) ? [] : reset(owner, key, descriptors.get(key)))));
};

// --- [SHADOWS]
const patched = (rows: readonly (readonly [object, PropertyKey, unknown])[]) => {
    const held = rows.map(([owner, key]) => [owner, key, Object.getOwnPropertyDescriptor(owner, key)] as const);
    rows.forEach(([owner, key, value]) => Reflect.defineProperty(owner, key, { configurable: true, writable: true, value }));
    return () => held.forEach(([owner, key, descriptor]) => (descriptor === undefined ? Reflect.deleteProperty(owner, key) : Reflect.defineProperty(owner, key, descriptor)));
};
const prototypes = (session: Session) => {
    const scheduled = (name: string) => () => {
        throw new Error(`${name} schedules work that outlives the answer`);
    };
    const printed = (...parts: readonly unknown[]) => session.record.console.push(parts.map(String).join(' '));
    const reaching = ['show', 'revealInSystem', 'revealInBridge'] as const;
    const scheduling = ['queueMicrotask', 'setTimeout', 'setInterval', 'setImmediate', 'requestAnimationFrame'] as const;
    const activating = names.flatMap((name) => {
        const prototype = field(Reflect.get(indesign, name), 'prototype');
        return Predicate.isObject(prototype) ? reaching.filter((member) => Predicate.isFunction(Object.getOwnPropertyDescriptor(prototype, member)?.value)).map((member) => [prototype, member, () => session.record.activations.push(member)] as const) : [];
    });
    const printers = Object.keys(console).filter((name) => Predicate.isFunction(Reflect.get(console, name)));
    return patched([...activating, ...scheduling.map((name) => [globalThis, name, scheduled(name)] as const), ...printers.map((name) => [console, name, printed] as const)]);
};
const dialogs = (session: Session): unknown[] => {
    const list: unknown[] = [];
    const node = (kind: string, owner: unknown, flat: Node[]): Node => {
        const made: Node = { parent: owner, isValid: true, kind, staticLabel: '', editContents: '', editValue: 0, selectedIndex: 0, stringList: [], checkedState: kind === 'enablingGroups', selectedButton: 0, minWidth: 0 };
        const collection = (child: string) => {
            const listed: unknown[] = [];
            return Object.assign(listed, {
                add: (properties: Node = {}) => {
                    const added = Object.assign(node(child, made, flat), properties);
                    Object.assign(made, child === 'radiobuttonControls' && properties.checkedState === true ? { selectedButton: listed.length } : {});
                    listed.push(added);
                    flat.push(added);
                    return added;
                },
                item: (index: number) => listed[index],
                firstItem: () => listed[0],
                lastItem: () => listed.at(-1),
            });
        };
        return Object.assign(made, { destroy: () => Object.assign(made, { isValid: false }) }, Object.fromEntries(CONTAINERS.map((child) => [child, collection(child)])));
    };
    return Object.assign(list, {
        add: (properties: Node = {}) => {
            const flat: Node[] = [];
            const dialog = Object.assign(node('dialog', app, flat), { name: '', canCancel: true }, properties);
            Object.assign(dialog, {
                show: () => {
                    const answer = session.dialogs.shift() ?? {};
                    const filled = answer.fields ?? {};
                    const counts = new Map<string, number>();
                    session.record.dialogs.push({
                        name: String(dialog.name),
                        controls: flat.map((row) => {
                            const kind = String(row.kind);
                            const index = counts.get(kind) ?? 0;
                            counts.set(kind, index + 1);
                            const member = VALUED[kind];
                            Option.map(Record.get(filled, `${kind}#${index}`), (value) => (member === undefined ? undefined : Reflect.set(row, member, value)));
                            return session.codec.encoded({ kind, index, label: row.staticLabel, ...(member === undefined ? {} : { value: row[member] }) });
                        }),
                    });
                    return answer.answer ?? true;
                },
            });
            list.push(dialog);
            return dialog;
        },
        item: (index: number) => list[index],
    });
};
const documents = (session: Session) => {
    const open = () => session.documents.filter((doc) => field(doc, 'isValid') === true && field(doc, 'constructorName') === 'Document');
    const missing = () => invoked(app.documents, 'itemByID', [-1]);
    const at = (index: number) => open().at(index) ?? missing();
    const position = (doc: unknown) => open().findIndex((held) => field(held, 'id') === field(doc, 'id'));
    return {
        get length() {
            return open().length;
        },
        count: () => open().length,
        add: (...args: unknown[]) => {
            const made = invoked(app.documents, 'add', Predicate.isObject(args[0]) ? [true, undefined, args[0]] : args);
            session.documents.push(made);
            return made;
        },
        item: (key: unknown) => (Predicate.isNumber(key) ? at(key) : (open().find((doc) => field(doc, 'name') === key) ?? missing())),
        itemByName: (name: string) => open().find((doc) => field(doc, 'name') === name) ?? missing(),
        itemByID: (id: number) => open().find((doc) => field(doc, 'id') === id) ?? missing(),
        itemByRange: (from: unknown, to: unknown) => {
            const [start = 0, end = 0] = [from, to].map((key) => (Predicate.isNumber(key) ? key : position(key)));
            return open().slice(Math.min(start, end), Math.max(start, end) + 1);
        },
        everyItem: () => shadow({ getElements: () => open() } satisfies Node, (key, value) => (value === undefined ? open().map((doc) => field(doc, key)) : value)),
        anyItem: () => at(0),
        firstItem: () => at(0),
        lastItem: () => at(-1),
        middleItem: () => at(Math.floor(open().length / 2)),
        previousItem: (doc: unknown) => at(position(doc) - 1),
        nextItem: (doc: unknown) => (position(doc) < 0 ? missing() : at(position(doc) + 1)),
    };
};
const menus = (session: Session) => {
    const menu: Node = { isValid: true };
    Object.assign(menu, {
        submenus: { item: () => menu, itemByName: () => menu },
        menuItems: { item: (name: string) => ({ name, isValid: true }), itemByName: (name: string) => ({ name, isValid: true }), add: (action: unknown) => session.record.menus.push({ added: session.codec.encoded(field(action, 'title')) }) },
        menuSeparators: { add: () => ({}) },
    });
    return {
        scriptMenuActions: {
            add: (title: string) => {
                const listeners: Node = {};
                const action: Node = { title, name: title, enabled: true, checked: false, isValid: true, listeners, addEventListener: (type: string, handler: unknown) => Object.assign(listeners, { [type]: handler }), remove: () => Object.assign(action, { isValid: false }) };
                session.actions.push(action);
                return action;
            },
            itemByName: () => ({ isValid: false }),
            everyItem: () => ({ getElements: () => [...session.actions] }),
        },
        menus: { item: () => menu, itemByName: () => menu },
    };
};
const picked = (session: Session, kind: string) => () => {
    session.record.files.push(kind);
    const answer = session.files.shift() ?? null;
    throw new Error(`${kind} picker answers through a promise that outlives the answer${answer === null ? '' : ` (${answer})`}`);
};
const installed = (session: Session) => {
    const queued = <A>(queue: A[], fallback: A): A => queue.shift() ?? fallback;
    const answering: Node = {
        alert: (text: unknown, title: unknown) => session.record.alerts.push(title === undefined ? { text: String(text) } : { text: String(text), title: String(title) }),
        confirm: (text: unknown) => {
            session.record.confirms.push(String(text));
            return queued(session.confirms, true);
        },
        prompt: (text: unknown, initial: string | null) => {
            session.record.prompts.push(String(text));
            return queued(session.prompts, initial);
        },
    };
    const evaluate = (code: string, extra: readonly unknown[]): unknown => {
        const { keys, values } = scope({ ...answering, app: fake, require: required });
        return called(compiled(keys, `return function () {${code}\n};`)(...values), extra);
    };
    const ours = (doc: unknown) => session.documents.some((held) => field(held, 'isValid') === true && field(held, 'id') === field(doc, 'id'));
    const activeOf = () => Option.filter(Result.getSuccess(Result.try(() => app.activeDocument)), ours);
    const windowed = (list: unknown) => elements(list).filter((window) => ours(field(window, 'parent')));
    const live: { readonly [K in (typeof LIVE)[number]]: () => unknown } = {
        activeDocument: () => Option.getOrElse(activeOf(), () => session.documents.findLast((doc) => field(doc, 'isValid') === true && field(doc, 'constructorName') === 'Document')),
        activeWindow: () => Option.getOrUndefined(Option.map(activeOf(), () => app.activeWindow)),
        selection: () => Option.match(activeOf(), { onNone: () => [], onSome: () => app.selection }),
        layoutWindows: () => windowed(app.layoutWindows),
        windows: () => windowed(app.windows),
    };
    const own: Node = {
        dialogs: dialogs(session),
        documents: documents(session),
        scriptPreferences: shadow(
            app.scriptPreferences,
            (_, value) => (Predicate.isFunction(value) ? value.bind(app.scriptPreferences) : value),
            (key, value) => key === 'userInteractionLevel' || Reflect.set(app.scriptPreferences, key, value),
        ),
        activate: () => session.record.activations.push('activate'),
        quit: () => session.record.activations.push('quit'),
        doScript: (source: unknown, language: ScriptLanguage | undefined, withArguments: unknown, ...rest: readonly unknown[]) => {
            if (Predicate.isFunction(source)) {
                return invoked(app, 'doScript', [source, language, withArguments, ...rest]);
            }
            if (language !== undefined && !indesign.ScriptLanguage.UXPSCRIPT.equals(language)) {
                throw new Error(`doScript in ${String(language)} runs outside the harness shadow`);
            }
            const code = Predicate.isString(source) ? source : String(fs.readFileSync(String(field(source, 'nativePath') ?? source), { encoding: 'utf-8' }));
            return evaluate(code, Array.isArray(withArguments) ? withArguments : []);
        },
        open: (file: unknown, showing: unknown, options: unknown) => {
            const opened = invoked(app, 'open', [file, showing ?? true, options]);
            session.documents.push(...(Array.isArray(opened) ? opened : [opened]));
            return opened;
        },
        ...(session.item.precondition.menus === true ? menus(session) : {}),
    };
    const lives = (key: PropertyKey): key is keyof typeof live => Predicate.isString(key) && Object.hasOwn(live, key);
    const owns = (key: PropertyKey): key is string => Predicate.isString(key) && Object.hasOwn(own, key);
    const fake = shadow(app, (key, value) =>
        Match.value(key).pipe(
            Match.when(lives, (held) => live[held]()),
            Match.when(owns, (held) => own[held]),
            Match.orElse(() => (Predicate.isFunction(value) ? value.bind(app) : value)),
        ),
    );
    const shadowed = shadow(indesign, (key, value) => (key === 'app' ? fake : value));
    const script = { args: session.item._tag === 'Probe' ? [] : session.item.args.map(String), setResult: (value: unknown) => Object.assign(session, { value: Option.some(value) }), executionContext: field(field(uxp, 'script'), 'executionContext') };
    const storage = field(uxp, 'storage');
    const files = shadow(new Object(field(storage, 'localFileSystem')), (member, value) => (member === 'getFileForOpening' || member === 'getFileForSaving' || member === 'getFolder' ? picked(session, String(member)) : value));
    const host = shadow(uxp, (key, value) =>
        Match.value(key).pipe(
            Match.when('script', () => script),
            Match.when('shell', () => ({ openPath: (path: string) => session.record.activations.push(`openPath ${path}`), openExternal: (url: string) => session.record.activations.push(`openExternal ${url}`) })),
            Match.when('storage', () => shadow(new Object(storage), (member, held) => (member === 'localFileSystem' ? files : held))),
            Match.orElse(() => value),
        ),
    );
    const modules: Record.ReadonlyRecord<string, unknown> = { indesign: shadowed, uxp: host };
    // biome-ignore lint/style/noCommonJs: originals require other host modules through the UXP require the shadow forwards
    const required = (name: string): unknown => Option.getOrElse(Record.get(modules, name), () => require(name));
    return { fake, evaluate, release: prototypes(session) };
};

// --- [PRECONDITION]
const given = (fake: unknown, expression: string, doc: unknown, made: Node, statement = false) => {
    const { keys, values } = scope({ app: fake, doc, made });
    return compiled(keys, statement ? expression : `return (${expression});`)(...values);
};
const assign = (fake: unknown, target: string, value: unknown, doc: unknown, made: Node) => {
    const at = target.indexOf('[*]');
    const count = at < 0 ? 1 : Number(field(given(fake, target.slice(0, at), doc, made), 'length'));
    Array.from({ length: count }, (_, index) => target.replaceAll('[*]', `[${index}]`)).forEach((path) => Reflect.set(new Object(given(fake, path.slice(0, path.lastIndexOf('.')), doc, made)), path.slice(path.lastIndexOf('.') + 1), value));
};
const differing = (codec: Codec, target: unknown, wanted: Schema.Json, path: string): readonly string[] => {
    const holds = Match.value(wanted).pipe(
        Match.when(Schema.is(Schema.Struct({ $: Schema.TemplateLiteral(['enum:', Schema.String]) })), (tag) => equal(target, codec.revived(tag))),
        Match.when(Schema.is(Schema.Struct({ $: Schema.String })), () => same(codec.encoded(target), wanted)),
        Match.when(Schema.is(JsonObject), () => true),
        Match.when(Predicate.isString, (name) => same(codec.encoded(target), wanted) || field(target, 'name') === name),
        Match.orElse(() => same(codec.encoded(target), wanted)),
    );
    const nested = Schema.is(JsonObject)(wanted) && !Predicate.isString(wanted.$) ? Object.entries(wanted).flatMap(([key, value]) => differing(codec, field(target, key), value, `${path}.${key}`)) : [];
    return holds ? nested : [`${path} reads ${JSON.stringify(codec.encoded(target))} where the precondition asks ${JSON.stringify(wanted)}`];
};
const opened = (session: Session): readonly unknown[] => {
    const { units, documents: rows, fonts = [] } = session.item.precondition;
    Reflect.set(app.scriptPreferences, 'measurementUnit', session.codec.revived(units.script));
    const docs = rows.map((row, index) => {
        const doc = 'open' in row ? invoked(app, 'open', [row.open, true]) : invoked(app.documents, 'add', [true, undefined, session.codec.revived(row.properties)]);
        session.documents.push(doc);
        invoked(doc, 'insertLabel', [session.batch.label, session.batch.records]);
        if ('open' in row) {
            Object.assign(new Object(field(doc, 'viewPreferences')), { horizontalMeasurementUnits: session.codec.revived(units.horizontal), verticalMeasurementUnits: session.codec.revived(units.vertical) });
        }
        const wrong = 'open' in row ? [] : differing(session.codec, doc, row.properties, `document ${index}`);
        if (wrong.length > 0) {
            throw new Error(wrong.join(', '));
        }
        return doc;
    });
    const missing = fonts.flatMap((name) => {
        const font = invoked(app.fonts, 'itemByName', [name]);
        const status = field(font, 'isValid') === true ? field(font, 'status') : undefined;
        return equal(status, indesign.FontStatus.INSTALLED) ? [] : [`${name} ${String(status ?? 'absent')}`];
    });
    if (missing.length > 0) {
        throw new Error(`fonts not installed: ${missing.join(', ')}`);
    }
    if (docs.length > 1) {
        Reflect.set(app, 'activeDocument', docs[0]);
    }
    return docs;
};
const state = (codec: Codec, doc: unknown) => {
    const nested = (name: string) =>
        pipe(
            Result.try(() => field(doc, name)),
            Result.getSuccess,
            Option.filter((value) => Predicate.isObject(value) && !owned(value) && !('id' in value) && Predicate.isObject(field(value, 'properties'))),
            Option.map((value) => [name, field(value, 'properties')] as const),
        );
    return codec.encoded({
        name: field(doc, 'name'),
        ...Object.fromEntries(preferences(doc).flatMap((name) => Option.toArray(nested(name)))),
        cmykProfile: field(doc, 'cmykProfile'),
        rgbProfile: field(doc, 'rgbProfile'),
        appliedLanguage: field(field(field(doc, 'textDefaults'), 'appliedLanguage'), 'name'),
        fonts: { names: field(invoked(field(doc, 'fonts'), 'everyItem', []), 'name'), status: field(invoked(field(doc, 'fonts'), 'everyItem', []), 'status') },
        links: { names: field(invoked(field(doc, 'links'), 'everyItem', []), 'name'), status: field(invoked(field(doc, 'links'), 'everyItem', []), 'status') },
    });
};
const reads = (session: Session, fake: unknown, docs: readonly unknown[], made: Node) => {
    const [doc] = docs;
    const unit = app.scriptPreferences.measurementUnit;
    const frames = docs.filter((held) => field(held, 'isValid') === true).map((held) => [held, field(field(held, 'viewPreferences'), 'rulerOrigin'), field(held, 'zeroPoint')] as const);
    app.scriptPreferences.measurementUnit = indesign.MeasurementUnits.POINTS;
    frames.forEach(([held]) => {
        Object.assign(new Object(field(held, 'viewPreferences')), { rulerOrigin: indesign.RulerOrigin.PAGE_ORIGIN });
        Reflect.set(new Object(held), 'zeroPoint', [0, 0]);
    });
    try {
        const evaluate = (expression: string) => given(fake, expression, doc, made);
        const rows = Record.collect(session.item.read, (key, expression) => {
            try {
                return { state: { [key]: evaluate(expression) }, failed: {} };
            } catch (error) {
                return { state: {}, failed: { [key]: message(error) } };
            }
        });
        return { state: session.codec.record(Object.assign({}, ...rows.map((row) => row.state))), failed: Record.map(Object.assign({}, ...rows.map((row) => row.failed)), String) };
    } finally {
        frames.forEach(([held, origin, zero]) => {
            Object.assign(new Object(field(held, 'viewPreferences')), { rulerOrigin: origin });
            Reflect.set(new Object(held), 'zeroPoint', zero);
        });
        app.scriptPreferences.measurementUnit = unit;
    }
};
const prepare = (fake: unknown, doc: unknown, session: Session, made: Node) => {
    const { labels = {}, set = [], add = [], steps = [], select = [] } = session.item.precondition;
    Object.entries(labels).forEach(([label, value]) => invoked(doc, 'insertLabel', [label, value]));
    set.forEach(([target, value]) => assign(fake, target, session.codec.revived(value), doc, made));
    add.forEach((row) => Object.assign(made, { [row.as]: invoked(field(given(fake, row.to, doc, made), row.kind), 'add', [session.codec.revived(row.properties ?? {})]) }));
    steps.forEach((step) => given(fake, step, doc, made, true));
    const picks = select.map((pick) => given(fake, pick, doc, made)).filter(Predicate.isObject);
    if (picks.length > 0) {
        app.select(picks);
    }
};
const exported = (session: Session, docs: readonly unknown[], moment: string) =>
    session.item.precondition.idml === true ? docs.forEach((doc, index) => (field(doc, 'isValid') === true ? invoked(doc, 'exportFile', [indesign.ExportFormat.INDESIGN_MARKUP, `${session.item.folder}/${index}.${moment}.idml`]) : undefined)) : undefined;

// --- [RUN]
const ran = (session: Session, { fake, evaluate }: ReturnType<typeof installed>, docs: readonly unknown[], made: Node) => {
    const { item } = session;
    const thrown = (error: unknown) => {
        const stack = lines(error).map((line) => ({ line: line - HEAD }));
        return { _tag: 'Thrown' as const, message: `${String(field(error, 'name') ?? 'Error')}: ${message(error)}`, ...(stack.length === 0 ? {} : { line: stack[0]?.line ?? 0, stack }) };
    };
    try {
        app.scriptPreferences.userInteractionLevel = item.interact ? indesign.UserInteractionLevels.INTERACT_WITH_ALL : indesign.UserInteractionLevels.NEVER_INTERACT;
        const value = (() => {
            try {
                return Match.valueTags(item, {
                    Probe: ({ evaluate: expression }) => given(fake, expression, docs[0], made),
                    Call: ({ source, call, args }) => evaluate(`${source?.code ?? ''}\n;return (${call}).apply(null, arguments);`, args.map(session.codec.revived)),
                    Run: ({ source, args }) => evaluate(source.code, args.map(session.codec.revived)),
                });
            } finally {
                app.scriptPreferences.userInteractionLevel = indesign.UserInteractionLevels.NEVER_INTERACT;
            }
        })();
        if (Predicate.isPromiseLike(value)) {
            return { completion: { _tag: 'Failed' as const, reason: 'original answered a thenable that settles after the answer, its rejection in logs' } };
        }
        session.actions.forEach((action) => {
            const handler = field(field(action, 'listeners'), 'onInvoke');
            session.record.menus.push({ title: String(action.title), invoked: Predicate.isFunction(handler) });
            if (Predicate.isFunction(handler)) {
                Reflect.apply(handler, action, [{ eventType: 'onInvoke' }]);
            }
        });
        return { completion: { _tag: 'Ran' as const }, value: session.codec.encoded(item._tag === 'Run' ? Option.getOrUndefined(session.value) : value) };
    } catch (error) {
        return { completion: thrown(error) };
    }
};
const outcomeOf = (item: Item, batch: Batch, found: ReturnType<typeof scanned>, codec: Codec): typeof Outcome.Type => {
    const session: Session = {
        item,
        batch,
        codec,
        documents: [],
        confirms: [...(item.confirms ?? [])],
        prompts: [...(item.prompts ?? [])],
        files: [...(item.files ?? [])],
        actions: [],
        record: { alerts: [], confirms: [], prompts: [], windows: [], dialogs: [], files: [], menus: [], activations: [], console: [], writes: [], undo: [], opened: [] },
        dialogs: [...(item.dialogs ?? [])],
        value: Option.none(),
    };
    const unit = app.scriptPreferences.measurementUnit;
    const active = Option.getOrUndefined(Result.getSuccess(Result.try(() => field(app.activeDocument, 'id'))));
    const held = snapshot(found, codec);
    const unglobal = globals();
    const leaked: string[] = [];
    const made: Node = {};
    const attempt = (label: string, work: () => void) => {
        try {
            work();
        } catch (error) {
            leaked.push(`${label}: ${message(error)}`);
        }
    };
    const shadows = installed(session);
    const result = ((): Omit<typeof Outcome.Type, 'name' | 'answered' | 'leaked' | keyof typeof fields> => {
        try {
            const docs = opened(session);
            session.record.opened = docs.map((doc) => state(codec, doc));
            prepare(shadows.fake, docs[0], session, made);
            const before = reads(session, shadows.fake, docs, made).state;
            const inner: { done?: ReturnType<typeof ran>; after?: ReturnType<typeof reads> } = {};
            const rolled = Result.try(() =>
                app.doScript(
                    () => {
                        exported(session, docs, 'before');
                        const history = docs.map((doc) => strings(field(doc, 'undoHistory')));
                        inner.done = ran(session, shadows, docs, made);
                        session.record.undo = docs.flatMap((doc, index) => (field(doc, 'isValid') === true ? strings(field(doc, 'undoHistory')).slice(0, strings(field(doc, 'undoHistory')).length - (history[index]?.length ?? 0)) : []));
                        inner.after = reads(session, shadows.fake, docs, made);
                        session.record.writes = differences(held, codec).map(({ member, before: previous, after: now }) => (previous === undefined ? { member, after: codec.encoded(now) } : { member, before: codec.encoded(previous), after: codec.encoded(now) }));
                        exported(session, docs, 'after');
                        throw new Error('rollback');
                    },
                    indesign.ScriptLanguage.UXPSCRIPT,
                    [],
                    indesign.UndoModes.ENTIRE_SCRIPT,
                    item.name,
                ),
            );
            const { done, after } = Option.getOrThrowWith(
                Option.zipWith(Option.fromUndefinedOr(inner.done), Option.fromUndefinedOr(inner.after), (finished, read) => ({ done: finished, after: read })),
                () => Result.match(rolled, { onFailure: identity, onSuccess: () => new Error('case did not finish') }),
            );
            return { ...done, before, read: after.state, readFailed: after.failed };
        } catch (error) {
            return { completion: { _tag: 'Failed', reason: message(error) } };
        }
    })();
    attempt('shadows', shadows.release);
    session.documents.forEach((doc) => attempt('close', () => (field(doc, 'isValid') === true ? invoked(doc, 'close', field(doc, 'constructorName') === 'Library' ? [] : [indesign.SaveOptions.NO]) : undefined)));
    leaked.push(...session.documents.filter((doc) => field(doc, 'isValid') === true).map((doc) => `document ${String(field(doc, 'name'))} stays open after close`));
    attempt('scriptPreferences', () => Object.assign(app.scriptPreferences, { measurementUnit: unit }));
    attempt('restore', () => leaked.push(...restore(held, codec)));
    attempt('globals', () => leaked.push(...unglobal()));
    attempt('activeDocument', () => (active !== undefined && app.documents.length > 0 && field(app.activeDocument, 'id') !== active ? Object.assign(app, { activeDocument: invoked(app.documents, 'itemByID', [active]) }) : undefined));
    return Outcome.make({ name: item.name, ...result, ...session.record, leaked });
};
const respond = ({ script: host }: typeof Host.Type) => {
    const written = Schema.encodeResult(Recorded);
    const answered = (batch: Batch): typeof Reply.Type => {
        const level = app.scriptPreferences.userInteractionLevel;
        app.scriptPreferences.userInteractionLevel = indesign.UserInteractionLevels.NEVER_INTERACT;
        try {
            elements(app.documents)
                .filter((doc) => !app.modalState && invoked(doc, 'extractLabel', [batch.label]) !== '')
                .forEach((doc) => invoked(doc, 'close', [indesign.SaveOptions.NO]));
            const found = scanned();
            const coder = codecOf(batch.enumerations);
            const settings = app.colorSettings;
            const header = {
                application: app.name,
                version: app.version,
                state: { modal: app.modalState, cmyk: settings.workingSpaceCMYK, rgb: settings.workingSpaceRGB, defaults: coder.record(Object.fromEntries(found.preferences.map((name) => [name, field(field(app, name), 'properties')]))), unread: found.unread },
                ...(batch.survey ? { survey: survey() } : {}),
            };
            const appended = (line: string) => fs.writeFileSync(batch.records, `${String(fs.readFileSync(batch.records, { encoding: 'utf-8' }))}${line}\n`);
            batch.cases.forEach((item, index) => {
                appended(Result.getOrThrow(written({ _tag: 'Started', index })));
                appended(Result.getOrThrow(Result.orElse(written({ _tag: 'Finished', outcome: outcomeOf(item, batch, found, coder) }), ({ message: reason }) => written({ _tag: 'Finished', outcome: Outcome.make({ name: item.name, completion: { _tag: 'Failed', reason } }) }))));
            });
            return header;
        } catch (error) {
            return { failed: message(error) };
        } finally {
            app.scriptPreferences.userInteractionLevel = level;
        }
    };
    host.setResult(Result.getOrThrow(Schema.encodeResult(Reply)(Result.match(Schema.decodeUnknownResult(Encoded)(host.args[0]), { onFailure: ({ message: failed }) => ({ failed }), onSuccess: answered }))));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

Result.match(Schema.decodeUnknownResult(Host)(uxp), { onFailure: () => undefined, onSuccess: respond });
