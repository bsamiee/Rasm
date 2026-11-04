import fs from 'adobe:fs';
import indesign, { app, type ScriptLanguage } from 'adobe:indesign';
import uxp from 'adobe:uxp';
import { Array, Function, Match, Option, Predicate, Record } from 'effect';
import { activeOf, type LIVE } from '#/state';
import { compiled, field, type Node, names, type Session, scope } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

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

// --- [OPERATIONS] ----------------------------------------------------------------------

const shadow = <T extends object>(target: T, get: (key: PropertyKey, value: unknown) => unknown, set: (key: PropertyKey, value: unknown) => boolean = (key, value) => Reflect.set(target, key, value)) =>
    new Proxy(target, {
        get: (held, key) => get(key, Reflect.get(held, key)),
        set: (_, key, value) => set(key, value),
    });
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
        return Object.assign(
            made,
            { destroy: () => Object.assign(made, { isValid: false }) },
            Record.fromIterableWith(CONTAINERS, (child) => [child, collection(child)]),
        );
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
                            const member = Record.get(VALUED, kind);
                            Option.match(Option.all([member, Record.get(filled, `${kind}#${index}`)]), { onNone: Function.constVoid, onSome: ([held, value]) => Reflect.set(row, held, value) });
                            return session.codec.encoded({ kind, index, label: row.staticLabel, ...Record.getSomes({ value: Option.map(member, (held) => row[held]) }) });
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
    const missing = () => app.documents.itemByID(-1);
    const at = (index: number) => open().at(index) ?? missing();
    const position = (doc: unknown) => open().findIndex((held) => field(held, 'id') === field(doc, 'id'));
    return {
        get length() {
            return open().length;
        },
        count: () => open().length,
        add: (...args: unknown[]) => {
            const made: unknown = Reflect.apply(app.documents.add, app.documents, Predicate.isObject(args[0]) ? [true, undefined, args[0]] : args);
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
        return compiled(keys, `return function () {${code}\n};`)(...values)(...extra);
    };
    const ours = (doc: unknown) => session.documents.some((held) => field(held, 'isValid') === true && field(held, 'id') === field(doc, 'id'));
    const owning = () => Option.filter(activeOf(), ours);
    const windowed = (listed: readonly { readonly parent: unknown }[]) => listed.filter((window) => ours(window.parent));
    const live: { readonly [K in (typeof LIVE)[number]]: () => unknown } = {
        activeDocument: () => Option.getOrElse(owning(), () => session.documents.findLast((doc) => field(doc, 'isValid') === true && field(doc, 'constructorName') === 'Document')),
        activeWindow: () => Option.getOrUndefined(Option.map(owning(), () => app.activeWindow)),
        selection: () => Option.match(owning(), { onNone: () => [], onSome: () => app.selection }),
        layoutWindows: () => windowed(app.layoutWindows.everyItem().getElements()),
        windows: () => windowed(app.windows.everyItem().getElements()),
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
                return Reflect.apply(app.doScript, app, [source, language, withArguments, ...rest]);
            }
            if (language !== undefined && !indesign.ScriptLanguage.UXPSCRIPT.equals(language)) {
                throw new Error(`doScript in ${String(language)} runs outside the harness shadow`);
            }
            const code = Predicate.isString(source) ? source : String(fs.readFileSync(String(field(source, 'nativePath') ?? source), { encoding: 'utf-8' }));
            return evaluate(code, Array.isArray(withArguments) ? withArguments : []);
        },
        open: (file: unknown, showing: unknown, options: unknown) => {
            const opened: unknown = Reflect.apply(app.open, app, [file, showing ?? true, options]);
            session.documents.push(...Array.ensure(opened));
            return opened;
        },
        ...(session.item.precondition.menus === true ? menus(session) : {}),
    };
    const lives = (key: PropertyKey): key is keyof typeof live => Predicate.isString(key) && Object.hasOwn(live, key);
    const owns = (key: PropertyKey): key is string => Predicate.isString(key) && Object.hasOwn(own, key);
    const fake = shadow(app, (key, value) =>
        Match.value({ key, value }).pipe(
            Match.when({ key: lives }, ({ key: held }) => live[held]()),
            Match.when({ key: owns }, ({ key: held }) => own[held]),
            Match.when({ value: Predicate.isFunction }, ({ value: held }) => held.bind(app)),
            Match.orElse(() => value),
        ),
    );
    const shadowed = shadow(indesign, (key, value) => (key === 'app' ? fake : value));
    const script = { args: session.item._tag === 'Probe' ? [] : session.item.args.map(String), setResult: (value: unknown) => Object.assign(session, { value: Option.some(value) }), executionContext: field(field(uxp, 'script'), 'executionContext') };
    const storage = field(uxp, 'storage');
    const files = shadow(new Object(field(storage, 'localFileSystem')), (member, value) => (member === 'getFileForOpening' || member === 'getFileForSaving' || member === 'getFolder' ? picked(session, String(member)) : value));
    const stored = shadow(new Object(storage), (member, held) => (member === 'localFileSystem' ? files : held));
    const host = shadow(uxp, (key, value) =>
        Match.value(key).pipe(
            Match.when('script', () => script),
            Match.when('shell', () => ({ openPath: (path: string) => session.record.activations.push(`openPath ${path}`), openExternal: (url: string) => session.record.activations.push(`openExternal ${url}`) })),
            Match.when('storage', () => stored),
            Match.orElse(() => value),
        ),
    );
    const modules: Record.ReadonlyRecord<string, unknown> = { indesign: shadowed, uxp: host };
    // biome-ignore lint/style/noCommonJs: originals require other host modules through the UXP require the shadow forwards
    const required = (name: string): unknown => Option.getOrElse(Record.get(modules, name), () => require(name));
    return { fake, evaluate, release: prototypes(session) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { installed };
