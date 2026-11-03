import type { Schema } from 'effect';
import type { HostCase, HostOutcome, Model, Request } from '#/acrobat/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Item = typeof HostCase.Type;
type Members = Record<string, unknown>;
type Callable = (...args: readonly unknown[]) => unknown;
type Listed<T> = { -readonly [K in keyof T]-?: NonNullable<T[K]> extends readonly (infer A)[] ? A[] : never };
type Signature = (typeof Model.Type)['signatures'][string];
interface Root extends Members {
    readonly app: Members;
    readonly global: Members;
    readonly util: Members;
    readonly console: Members;
    readonly acrohelp: unknown;
}
interface Doc extends Members {
    readonly path: string;
    readonly numPages: number;
    readonly numFields: number;
    readonly info: Members;
    readonly bookmarkRoot: Members;
    metadata: string;
    closeDoc: (noSave: boolean) => void;
    saveAs: (path: string) => void;
    newPage: (page: number, width: number, height: number) => void;
    setPageRotations: (start: number, end: number, rotation: number) => void;
    setPageBoxes: (box: string, start: number, end: number, rect: readonly number[]) => void;
    getPageBox: (box: string, page: number) => unknown;
    getPageRotation: (page: number) => number;
    getNthFieldName: (index: number) => string;
    getField: (name: string) => Members | null;
    addField: (name: string, type: string, page: number, rect: readonly number[]) => Members;
    addAnnot: (properties: unknown) => Members;
    getAnnots: () => readonly Members[] | null;
    getOCGs: () => readonly Members[] | null;
    calculateNow: () => void;
}
interface Write {
    readonly target: Members;
    readonly key: string;
    readonly member: string;
    readonly before: unknown;
}
interface Session {
    readonly item: Item;
    readonly records: Listed<Pick<typeof HostOutcome.Encoded, 'alerts' | 'confirms' | 'prompts' | 'files' | 'windows' | 'console' | 'activations' | 'events' | 'opened' | 'leaked'>>;
    readonly answers: Listed<Pick<Item, 'confirms' | 'prompts' | 'files' | 'windows'>>;
    readonly writes: Write[];
    readonly docs: Doc[];
    readonly saved: string[];
    readonly timers: [string, unknown][];
    readonly refused: string[];
}
interface Fault {
    readonly name: string;
    readonly message: string;
    readonly extMessage: string | undefined;
    readonly lineNumber: number | undefined;
    readonly stack: string | undefined;
}
interface Holder {
    readonly events: unknown[];
    readonly scope: Members;
    readonly target: unknown;
    thrown: unknown;
}
interface Reads {
    readonly read: Record<string, Schema.Json>;
    readonly failed: Record<string, string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = function (this: unknown, request: typeof Request.Type): string {
    const NUMBERS: Members = { undefined, NaN: Number.NaN, Infinity: Number.POSITIVE_INFINITY, '-Infinity': Number.NEGATIVE_INFINITY, '-0': -0 };
    const DATED = 'date:';
    const TAGGED = '[object ';
    const SPREAD = '[*]';
    const VALUED = ['Object', 'Array', 'Date', 'Error', 'Function', 'Boolean', 'Number', 'String', 'RegExp'];
    const SCALARS = ['string', 'integer', 'number', 'boolean'];
    const BUTTONS = { ok: 1, cancel: 2, no: 3, yes: 4 } as const;
    const OK_CANCEL = 1;
    const CANCELLED: null = null;
    const REFUSED = 'Refused';
    const ACTION = 'Action';
    const SURVEY_SIZE = 20;
    const SURVEY_RECT = [0, SURVEY_SIZE, SURVEY_SIZE, 0];
    const SURVEY_POINT = [SURVEY_SIZE / 2, SURVEY_SIZE / 2];
    const SIGNED: readonly (readonly [string, readonly string[]])[] = [
        ['App', ['alert', 'response', 'browseForDoc', 'browseForMultipleDocs', 'execDialog', 'openDoc']],
        ['Doc', ['closeDoc', 'saveAs', 'insertPages', 'replacePages', 'importDataObject', 'importIcon', 'importAnFDF', 'importAnXFDF', 'importTextData', 'importXFAData', 'exportDataObject', 'exportAsFDF', 'exportAsXFDF', 'exportAsText', 'exportXFAData']],
        ['Util', ['readFileIntoStream']],
    ];
    const PICKERS = ['importDataObject', 'importIcon', 'importAnFDF', 'importAnXFDF', 'importTextData', 'importXFAData', 'exportDataObject', 'readFileIntoStream'];
    const ACTIVATING: Record<string, { readonly names: readonly string[]; readonly prefixes: readonly string[] }> = {
        App: { names: ['launchURL', 'beep', 'goBack', 'goForward', 'addMenuItem', 'addSubMenu', 'addToolButton', 'hideMenuItem', 'hideToolbarButton', 'removeToolButton'], prefixes: ['mail'] },
        Doc: { names: ['bringToFront', 'getURL', 'submitForm'], prefixes: ['print', 'mail'] },
        Console: { names: ['show', 'hide', 'clear'], prefixes: [] },
    };
    const PATH = 'Path';
    const HELP = '====> ';
    // @effect-diagnostics-next-line globalDate:off
    const now = () => Date.now();
    const has = <T>(list: readonly T[], item: T) => list.some((held) => held === item);
    const starts = (text: string, prefix: string) => text.lastIndexOf(prefix, 0) === 0;
    const ends = (text: string, suffix: string) => text.length >= suffix.length && text.indexOf(suffix, text.length - suffix.length) === text.length - suffix.length;
    const pathy = (name: string) => ends(name, PATH);
    const activating = (kind: string, member: string) => {
        const listed = ACTIVATING[kind];
        return listed !== undefined && (has(listed.names, member) || listed.prefixes.some((prefix) => starts(member, prefix)));
    };
    const isMembers = (value: unknown): value is Members => value !== null && typeof value === 'object';
    const isCallable = (value: unknown): value is Callable => typeof value === 'function';
    const kindOf = (value: unknown) => Object.prototype.toString.call(value).slice(TAGGED.length, -1);
    const isDate = (value: unknown): value is Date => kindOf(value) === 'Date';
    const isDoc = (value: unknown): value is Doc => isMembers(value) && kindOf(value) === 'Doc';
    const isMemberList = (value: unknown): value is readonly Members[] => Array.isArray(value) && value.every(isMembers);
    const isListed = (value: unknown): value is Members & { readonly length: number } => isMembers(value) && typeof value.length === 'number';
    const shaped = <T>(value: unknown, is: (held: unknown) => held is T, what: string): T => {
        if (is(value)) {
            return value;
        }
        throw new Error(`${what} is ${kindOf(value)}`);
    };
    const invoked = (target: Members, name: string, args: readonly unknown[]) => shaped(target[name], isCallable, name).apply(target, [...args]);
    const enumerated = (target: Members) => {
        const names: string[] = [];
        // biome-ignore lint/suspicious/noForIn: native Acrobat objects enumerate their prototype members through for-in alone
        // biome-ignore lint/suspicious/useGuardForIn: native Acrobat objects enumerate their prototype members through for-in alone
        for (const key in target) {
            names.push(key);
        }
        return names;
    };
    const keyed = <T>(names: readonly string[], each: (key: string) => T): Record<string, T> => {
        const made: Record<string, T> = {};
        names.forEach((name) => {
            made[name] = each(name);
        });
        return made;
    };
    const counted = <T>(count: number, each: (index: number) => T): T[] => (count > 0 ? counted(count - 1, each).concat([each(count - 1)]) : []);
    const indexed = (text: string, at: number, index: number) => `${text.slice(0, at)}[${index}]${text.slice(at + SPREAD.length)}`;
    const compile = (scope: Members, body: string) => {
        const names = Object.keys(scope);
        // biome-ignore lint/nursery/noImpliedEval: case data and originals compile inside the host
        const made = new Function(...names, body);
        return (target: unknown): unknown =>
            made.apply(
                target,
                names.map((name) => scope[name]),
            );
    };
    const faultOf = (error: unknown): Fault => {
        const held = isMembers(error) ? error : { message: String(error) };
        return {
            name: typeof held.name === 'string' ? held.name : 'Error',
            message: typeof held.message === 'string' ? held.message : '',
            extMessage: typeof held.extMessage === 'string' ? held.extMessage : undefined,
            lineNumber: typeof held.lineNumber === 'number' ? held.lineNumber : undefined,
            stack: typeof held.stack === 'string' ? held.stack : undefined,
        };
    };
    const root = shaped(compile({}, 'return this')(undefined), (value): value is Root => isMembers(value) && isMembers(value.app) && isMembers(value.global) && isMembers(value.util) && isMembers(value.console), 'this');
    const { app, util, acrohelp } = root;
    const shared = root.global;
    const base = ((): number => {
        try {
            compile({}, 'throw new Error("")')(undefined);
            return 0;
        } catch (error) {
            return (faultOf(error).lineNumber ?? 1) - 1;
        }
    })();
    const documents = String(invoked(app, 'getPath', ['user', 'documents']));
    const home = documents.indexOf(request.home);
    const volume = home > 0 ? documents.slice(0, home) : '';
    const posix = (path: string) => (volume !== '' && starts(path, `${volume}/`) ? path.slice(volume.length) : path);
    const numbered = (value: number): Schema.Json => {
        if (Number.isNaN(value)) {
            return { $: 'NaN' };
        }
        if (!Number.isFinite(value)) {
            return { $: value > 0 ? 'Infinity' : '-Infinity' };
        }
        return value === 0 && 1 / value < 0 ? { $: '-0' } : value;
    };
    const named = (value: Members) => {
        try {
            return typeof value.name === 'string' ? `/${value.name}` : '';
        } catch {
            return '';
        }
    };
    const scalar = (value: unknown): Schema.Json => {
        if (value === undefined) {
            return { $: 'undefined' };
        }
        if (typeof value === 'number') {
            return numbered(value);
        }
        if (value === null || typeof value === 'string' || typeof value === 'boolean') {
            return value;
        }
        return typeof value === 'function' ? '<function>' : String(value);
    };
    const described = (value: Members, inner: readonly unknown[]): Schema.Json => {
        const kind = kindOf(value);
        if (kind === 'Object') {
            return keyed(Object.keys(value), (key) => encoded(value[key], inner));
        }
        if (has(VALUED, kind)) {
            return String(invoked(value, 'toString', []));
        }
        if (kind === 'Annot' && isCallable(value.getProps)) {
            const props = shaped(value.getProps(), isMembers, 'Annot.getProps');
            return { $: `object:Annot${named(value)}`, ...keyed(Object.keys(props), (key) => encoded(props[key], inner)) };
        }
        return { $: `object:${kind}${isDoc(value) ? `/${value.path}` : named(value)}` };
    };
    const encoded = (value: unknown, seen: readonly unknown[] = []): Schema.Json => {
        if (isDate(value)) {
            return { $: `${DATED}${Number.isNaN(value.getTime()) ? 'NaN' : value.toISOString()}` };
        }
        if (!isMembers(value) || has(seen, value)) {
            return scalar(value);
        }
        const inner = seen.concat([value]);
        return Array.isArray(value) ? value.map((item) => encoded(item, inner)) : described(value, inner);
    };
    const tagged = (tag: string): unknown => {
        if (tag in NUMBERS) {
            return NUMBERS[tag];
        }
        // @effect-diagnostics-next-line globalDate:off
        return starts(tag, DATED) ? new Date(tag.slice(DATED.length)) : { $: tag };
    };
    const revived = (value: unknown): unknown => {
        if (Array.isArray(value)) {
            return value.map(revived);
        }
        if (!isMembers(value)) {
            return value;
        }
        const names = Object.keys(value);
        return typeof value.$ === 'string' && names.length === 1 ? tagged(value.$) : keyed(names, (key) => revived(value[key]));
    };
    const thrown = (error: unknown, offset: number) => {
        const fault = faultOf(error);
        const message = fault.name === 'HelpError' ? String(fault.extMessage) : `${fault.name}: ${fault.message}`;
        const frames = (fault.stack ?? '')
            .split('\n')
            .filter((line) => line.split('@').length > 1)
            .map((line) => ({ name: line.slice(0, line.indexOf('@')), line: line.split(':').pop() }));
        const ending = frames.map(({ name }) => name).indexOf('anonymous');
        const stack = frames.slice(0, ending + 1).map(({ line }) => ({ line: Number(line) - offset }));
        return {
            _tag: 'Thrown' as const,
            message,
            ...(fault.lineNumber === undefined ? {} : { line: fault.lineNumber - offset }),
            ...(ending < 0 ? {} : { stack }),
        };
    };
    const raised = (name: string, message: string, cause: unknown) => {
        const error = new Error(message);
        error.name = name;
        error.cause = cause;
        return error;
    };
    const recorded = (session: Session, member: string): undefined => {
        session.records.activations.push(member);
    };
    const refuse: (session: Session, reason: string) => never = (session, reason) => {
        session.refused.push(reason);
        throw raised(REFUSED, reason, undefined);
    };
    const signaturesOf = (targets: Record<string, Members>): Record<string, Signature> => {
        const made: Record<string, Signature> = {};
        const pairs = SIGNED.reduce<readonly { readonly kind: string; readonly member: string }[]>((all, [kind, members]) => all.concat(members.map((member) => ({ kind, member }))), []);
        pairs.forEach(({ kind, member }) => {
            const target = targets[kind] ?? {};
            const held = target[member];
            if (!isCallable(held)) {
                return;
            }
            try {
                held.apply(target, [acrohelp]);
            } catch (error) {
                const fault = faultOf(error);
                if (fault.name !== 'HelpError') {
                    return;
                }
                made[`${kind}.${member}`] = String(fault.extMessage)
                    .split('\n')
                    .filter((line) => starts(line, HELP) && line.indexOf(':') > HELP.length)
                    .map((line) => {
                        const parameter = line.slice(HELP.length).replace('[', '').replace(']', '');
                        const colon = parameter.indexOf(':');
                        return { name: parameter.slice(0, colon).trim(), type: parameter.slice(colon + 1).trim(), optional: line.charAt(HELP.length) === '[' };
                    });
            }
        });
        return made;
    };
    const argued = (signature: Signature, args: readonly unknown[]) => {
        const names = signature.map(({ name }) => name);
        const [first] = args;
        const [leading] = signature;
        const object = args.length === 1 && isMembers(first) && kindOf(first) === 'Object' && leading !== undefined && has(SCALARS, leading.type);
        return {
            values: keyed(names, (name) => (object ? first[name] : args[names.indexOf(name)])),
            rebuilt: (name: string, value: unknown) => (object ? [{ ...first, [name]: value }] : counted(Math.max(args.length, names.indexOf(name) + 1), (index) => (names[index] === name ? value : args[index]))),
        };
    };
    const choice = (type: unknown, agreed: boolean) => {
        const [yes, no] = type === OK_CANCEL ? [BUTTONS.ok, BUTTONS.cancel] : [BUTTONS.yes, BUTTONS.no];
        return agreed ? yes : no;
    };
    const submenu = (value: unknown): readonly unknown[] => {
        if (Array.isArray(value)) {
            return value;
        }
        return isMembers(value) ? [value] : [];
    };
    const chosen = (entries: readonly unknown[], answer: string | null): readonly unknown[] =>
        entries.reduce<readonly unknown[]>((hits, entry) => {
            if (!isMembers(entry)) {
                return hits;
            }
            return entry.cName === answer ? hits.concat([entry.cReturn ?? entry.cName]) : hits.concat(chosen(submenu(entry.oSubMenu), answer));
        }, []);
    const press = (id: string, called: (name: string) => unknown, ended: string[]) => {
        if (id === 'ok') {
            if (called('validate') !== false) {
                called('commit');
                ended.push('ok');
            }
            return;
        }
        if (id === 'cancel') {
            ended.push('cancel');
            return;
        }
        called(id);
    };
    const simulated = (handlers: Members, fields: Members, presses: readonly string[]) => {
        const values: Members = {};
        const ended: string[] = [];
        const dialog = {
            load: (loaded: unknown) => {
                const held = isMembers(loaded) ? loaded : {};
                Object.keys(held).forEach((key) => {
                    values[key] = held[key];
                });
            },
            store: () => ({ ...values, ...keyed(Object.keys(fields), (key) => revived(fields[key])) }),
            enable: () => undefined,
            visible: () => undefined,
            focus: () => undefined,
            gotoPage: () => undefined,
            end: (id: unknown) => {
                ended.push(String(id ?? 'cancel'));
            },
        };
        const called = (name: string) => {
            const handler = handlers[name];
            return isCallable(handler) ? handler.apply(handlers, [dialog]) : undefined;
        };
        const pressed: string[] = [];
        called('initialize');
        presses.forEach((id) => {
            if (ended.length > 0) {
                return;
            }
            pressed.push(id);
            press(id, called, ended);
        });
        const result = ended[0] ?? 'cancel';
        called('destroy');
        return { pressed, result };
    };
    const answered = (session: Session, signatures: Record<string, Signature>, kind: string, member: string, held: Callable, target: Members) => {
        const { records, answers } = session;
        const signature = signatures[`${kind}.${member}`] ?? [];
        const call = (args: readonly unknown[]) => held.apply(target, [...args]);
        const next = <T>(list: T[]) => list.splice(0, 1)[0];
        const picker = (args: readonly unknown[]) => {
            const { values, rebuilt } = argued(signature, args);
            const [path] = signature.map(({ name }) => name).filter(pathy);
            if (path === undefined || values[path] !== undefined) {
                return call(args);
            }
            records.files.push(`${kind}.${member}`);
            const file = next(answers.files);
            return typeof file === 'string' ? call(rebuilt(path, file)) : undefined;
        };
        const table: Record<string, Callable> = {
            'App.alert': (...args) => {
                const { cMsg, nType, cTitle, oCheckbox } = argued(signature, args).values;
                const checkbox = isMembers(oCheckbox) ? oCheckbox : {};
                records.alerts.push({ text: String(cMsg), ...(cTitle === undefined ? {} : { title: String(cTitle) }), ...(isMembers(oCheckbox) ? { checkbox: String(oCheckbox.cMsg) } : {}) });
                checkbox.bAfterValue = checkbox.bInitialValue === true;
                if (nType === undefined || nType === 0) {
                    return BUTTONS.ok;
                }
                records.confirms.push(String(cMsg));
                return choice(nType, next(answers.confirms) === true);
            },
            'App.response': (...args) => {
                records.prompts.push(String(argued(signature, args).values.cQuestion));
                return next(answers.prompts) ?? CANCELLED;
            },
            'App.browseForDoc': (...args) => {
                records.files.push(argued(signature, args).values.bSave === true ? 'App.browseForDoc save' : 'App.browseForDoc');
                const file = next(answers.files);
                return typeof file === 'string' ? { cFS: 'DOS', cPath: `${volume}${file}` } : undefined;
            },
            'App.browseForMultipleDocs': () => {
                records.files.push('App.browseForMultipleDocs');
                const file = next(answers.files);
                return typeof file === 'string' ? [{ cFS: 'DOS', cPath: `${volume}${file}` }] : undefined;
            },
            'App.popUpMenu': (...items) => {
                records.prompts.push(JSON.stringify(encoded(items)));
                return next(answers.prompts) ?? CANCELLED;
            },
            'App.popUpMenuEx': (...items) => {
                records.prompts.push(JSON.stringify(encoded(items)));
                const answer = next(answers.prompts);
                return answer === undefined ? CANCELLED : (chosen(items, answer)[0] ?? CANCELLED);
            },
            'App.execDialog': (...args) => {
                const { monitor } = argued(signature, args).values;
                const handlers = isMembers(monitor) ? monitor : {};
                const answer = next(answers.windows) ?? {};
                const { pressed, result } = simulated(handlers, answer.fields ?? {}, answer.presses ?? []);
                records.windows.push({ description: encoded(handlers.description), pressed, result });
                return result;
            },
            'App.execMenuItem': (...args) => {
                const [item] = args;
                return has(request.modal, String(item)) ? recorded(session, `App.execMenuItem ${String(item)}`) : call(args);
            },
            'App.setTimeOut': (...args) => {
                const timer = call(args);
                session.timers.push(['clearTimeOut', timer]);
                return timer;
            },
            'App.setInterval': (...args) => {
                const timer = call(args);
                session.timers.push(['clearInterval', timer]);
                return timer;
            },
            'Doc.closeDoc': (...args) => {
                if (!isDoc(target) || argued(signature, args).values.bNoSave === true || target.dirty !== true) {
                    return call(args);
                }
                records.confirms.push(`Doc.closeDoc ${target.path}`);
                if (next(answers.confirms) === true) {
                    target.saveAs(target.path);
                }
                return call([true]);
            },
            'Doc.saveAs': (...args) => {
                session.saved.push(posix(String(argued(signature, args).values.cPath)));
                return call(args);
            },
            'Doc.exportDataObject': (...args) => {
                const launched = Number(argued(signature, args).values.nLaunch ?? 0) > 0;
                return launched ? recorded(session, 'Doc.exportDataObject nLaunch') : picker(args);
            },
            'Console.println': (...args) => {
                records.console.push(String(args[0]));
            },
        };
        return table[`${kind}.${member}`] ?? (has(PICKERS, member) ? (...args: readonly unknown[]) => picker(args) : undefined);
    };
    const strayed = (sandbox: string, value: unknown): readonly string[] => {
        if (typeof value === 'string') {
            return starts(value, '/') && !starts(posix(value), `${sandbox}/`) ? [value] : [];
        }
        if (isMembers(value) && kindOf(value) === 'Object') {
            return Object.keys(value).reduce<readonly string[]>((found, key) => found.concat(strayed(sandbox, value[key])), []);
        }
        return [];
    };
    const vetted = (session: Session, member: string, pathed: boolean, args: readonly unknown[]) => {
        const stray = pathed ? strayed(session.item.precondition.sandbox, { ...args }) : [];
        if (stray.length > 0) {
            refuse(session, `${member} reaches ${stray.join(', ')} outside the case folder`);
        }
    };
    const guarded = (session: Session, signatures: Record<string, Signature>, target: Members, kind: string, member: string, held: Callable) => {
        const own = answered(session, signatures, kind, member, held, target);
        const signature = signatures[`${kind}.${member}`];
        const pathed = signature === undefined ? SIGNED.some(([owner, members]) => owner === kind && has(members, member)) : signature.some(({ name }) => pathy(name));
        return (...args: readonly unknown[]) => {
            if (args[0] === acrohelp) {
                return held.apply(target, [...args]);
            }
            if (activating(kind, member)) {
                return recorded(session, `${kind}.${member}`);
            }
            vetted(session, `${kind}.${member}`, pathed, args);
            return adopted(session, signatures, own === undefined ? held.apply(target, [...args]) : own(...args), true);
        };
    };
    const scoped = (session: Session, signatures: Record<string, Signature>, target: Members, kind: string): Members =>
        Object.defineProperties(
            {},
            keyed(enumerated(target), (key) => ({
                enumerable: true,
                get: () => {
                    const held = target[key];
                    return isCallable(held) ? guarded(session, signatures, target, kind, key, held) : adopted(session, signatures, held, false);
                },
                set: (value: unknown) => {
                    if (kind === 'FullScreen' && key === 'isFullScreen') {
                        recorded(session, 'FullScreen.isFullScreen');
                        return;
                    }
                    if ((kind === 'App' || kind === 'FullScreen') && !session.writes.some((held) => held.target === target && held.key === key)) {
                        session.writes.push({ target, key, member: `${kind}.${key}`, before: target[key] });
                    }
                    // biome-ignore lint/style/noParameterAssign: the scope writes through to the native Acrobat object it wraps
                    target[key] = value;
                },
            })),
        );
    const adopted = (session: Session, signatures: Record<string, Signature>, value: unknown, made: boolean): unknown => {
        if (Array.isArray(value)) {
            return value.map((item) => adopted(session, signatures, item, made));
        }
        if (isDoc(value)) {
            if (made && !has(session.docs, value)) {
                session.docs.push(value);
            }
            return scoped(session, signatures, value, 'Doc');
        }
        return isMembers(value) && kindOf(value) === 'FullScreen' ? scoped(session, signatures, value, 'FullScreen') : value;
    };
    const spread = (expression: string, evaluate: (code: string) => unknown): unknown => {
        const at = expression.indexOf(SPREAD);
        if (at < 0) {
            return evaluate(expression);
        }
        const head = expression.slice(0, at);
        const list = evaluate(head);
        return counted(list === null || list === undefined ? 0 : shaped(list, isListed, head).length, (index) => spread(indexed(expression, at, index), evaluate));
    };
    const reads = (item: Item, evaluate: (code: string) => unknown): Reads => {
        const read: Record<string, Schema.Json> = {};
        const failed: Record<string, string> = {};
        Object.keys(item.read).forEach((key) => {
            try {
                const value = spread(item.read[key] ?? 'undefined', evaluate);
                const at = key.indexOf(SPREAD);
                if (at >= 0 && Array.isArray(value)) {
                    value.forEach((each, index) => {
                        read[indexed(key, at, index)] = encoded(each);
                    });
                } else {
                    read[key] = encoded(value);
                }
            } catch (error) {
                failed[key] = thrown(error, base).message;
            }
        });
        return { read, failed };
    };
    const state = (doc: Doc) => {
        const tree = (mark: Members): Schema.Json => ({ name: String(mark.name), children: (Array.isArray(mark.children) ? mark.children : []).filter(isMembers).map(tree) });
        return {
            path: posix(doc.path),
            pages: counted(doc.numPages, (page) => ({ rotation: doc.getPageRotation(page), boxes: keyed(request.boxes, (box) => encoded(doc.getPageBox(box, page))) })),
            info: keyed(enumerated(doc.info), (key) => encoded(doc.info[key])),
            metadata: doc.metadata,
            annots: (doc.getAnnots() ?? []).map((annot) => encoded(annot)),
            fields: counted(doc.numFields, (index) => {
                const name = doc.getNthFieldName(index);
                const field = doc.getField(name) ?? {};
                return { name, type: encoded(field.type), page: encoded(field.page), rect: encoded(field.rect) };
            }),
            ocgs: (doc.getOCGs() ?? []).map((ocg) => ({ name: String(ocg.name), state: ocg.state === true })),
            bookmarks: tree(doc.bookmarkRoot),
            dataObjects: (Array.isArray(doc.dataObjects) ? doc.dataObjects : []).filter(isMembers).map((data) => String(data.name)),
        };
    };
    const created = (document: Exclude<Item['precondition']['documents'][number], { readonly open: string }>) => {
        const doc = shaped(invoked(app, 'newDoc', [document.width, document.height]), isDoc, 'app.newDoc');
        counted(document.pages - 1, (page) => doc.newPage(page + 1, document.width, document.height));
        if (document.rotation !== undefined) {
            doc.setPageRotations(0, doc.numPages - 1, document.rotation);
        }
        request.boxes.forEach((box) => {
            const rect = document.boxes?.[box];
            if (rect !== undefined) {
                doc.setPageBoxes(box, 0, doc.numPages - 1, rect);
            }
        });
        return doc;
    };
    const furnished = (docs: readonly Doc[], precondition: Item['precondition']) => {
        const [doc] = docs;
        if (doc === undefined) {
            return;
        }
        (precondition.fields ?? []).forEach(({ name, type, page, rect, properties = {} }) => {
            const field = doc.addField(name, type, page, rect);
            Object.keys(properties).forEach((key) => {
                field[key] = revived(properties[key]);
            });
        });
        (precondition.annots ?? []).forEach((properties) => doc.addAnnot(revived(properties)));
        const { info, metadata } = precondition;
        Object.keys(info ?? {}).forEach((key) => {
            doc.info[key] = info?.[key];
        });
        if (metadata !== undefined) {
            doc.metadata = metadata;
        }
    };
    const opened = (session: Session) => {
        const { precondition } = session.item;
        const docs = precondition.documents.map((document) => ('open' in document ? shaped(invoked(app, 'openDoc', [document.open]), isDoc, 'app.openDoc') : created(document)));
        docs.forEach((doc) => session.docs.push(doc));
        furnished(docs, precondition);
        const made = precondition.documents.reduce<readonly string[]>((files, document, index) => {
            const doc = docs[index];
            if ('open' in document || doc === undefined) {
                return files;
            }
            const file = `${precondition.sandbox}/${index}.pdf`;
            doc.saveAs(file);
            return files.concat([file]);
        }, []);
        docs.forEach((doc) => session.records.opened.push(state(doc)));
        return { docs, made };
    };
    const fired = (session: Session, scope: Members, target: unknown, docs: readonly Doc[], code: string, event: NonNullable<Item['precondition']['event']>) => {
        const [doc] = docs;
        if (doc === undefined) {
            refuse(session, `no document holds field ${event.field}`);
        }
        const field = doc.getField(event.field);
        if (field === null) {
            refuse(session, `no field ${event.field} on the first document`);
        }
        compile(scope, code);
        const key = `rasm${now()}`;
        const holder: Holder = { events: [], scope, target, thrown: undefined };
        doc[key] = holder;
        const names = Object.keys(scope);
        invoked(field, 'setAction', [
            event.trigger,
            `try { (function (${names.join(', ')}) {\n${code}\n}).apply(this.${key}.target, [${names.map((name) => `this.${key}.scope.${name}`).join(', ')}]); } catch (error) { this.${key}.thrown = error; }\nthis.${key}.events.push({ type: event.type, name: event.name, rc: event.rc, value: event.value, change: event.change, willCommit: event.willCommit });`,
        ]);
        try {
            if (event.trigger === 'Calculate') {
                doc.calculateNow();
            } else {
                field.value = revived(event.value);
            }
        } finally {
            invoked(field, 'setAction', [event.trigger, '']);
            delete doc[key];
        }
        holder.events.forEach((each) => session.records.events.push(encoded(each)));
        if (holder.thrown !== undefined) {
            throw raised(ACTION, thrown(holder.thrown, 1).message, holder.thrown);
        }
    };
    const closed = (doc: Doc): readonly string[] => {
        try {
            doc.closeDoc(true);
            return [];
        } catch (error) {
            return [`close: ${thrown(error, base).message}`];
        }
    };
    const restored = (session: Session, globals: Members) => {
        const { records } = session;
        session.timers.forEach(([clear, timer]) => {
            try {
                invoked(app, clear, [timer]);
            } catch (error) {
                records.leaked.push(`App.${clear}: ${thrown(error, base).message}`);
            }
        });
        const writes = session.writes.map(({ target, key, member, before }) => {
            const after = encoded(target[key]);
            try {
                target[key] = before;
            } catch (error) {
                records.leaked.push(`${member}: ${thrown(error, base).message}`);
            }
            return { member, before: encoded(before), after };
        });
        session.docs.reduceRight<readonly string[]>((leaked, doc) => leaked.concat(closed(doc)), []).forEach((line) => records.leaked.push(line));
        enumerated(shared)
            .filter((name) => !(name in globals) || shared[name] !== globals[name])
            .forEach((key) => {
                records.leaked.push(`global.${key}`);
                delete shared[key];
            });
        Object.keys(globals)
            .filter((name) => !(name in shared))
            .forEach((key) => {
                shared[key] = globals[key];
            });
        return writes;
    };
    const evaluated = (session: Session, scope: Members, target: unknown, docs: readonly Doc[], item: Item, evaluate: (code: string, statement?: boolean) => unknown): unknown => {
        if (item._tag === 'Probe') {
            return evaluate(item.evaluate);
        }
        if (item._tag === 'Run') {
            const { event } = item.precondition;
            return event === undefined ? evaluate(item.source.code, true) : fired(session, scope, target, docs, item.source.code, event);
        }
        const callee = evaluate(`${item.source?.code ?? ''}\nreturn (${item.call});`, true);
        return isCallable(callee) ? callee.apply(target, item.args.map(revived)) : callee;
    };
    const caught = (error: unknown) => {
        const fault = faultOf(error);
        if (fault.name === REFUSED) {
            return { completion: { _tag: 'Failed' as const, reason: fault.message } };
        }
        return fault.name === ACTION && isMembers(error) ? { completion: thrown(error.cause, 1) } : { completion: thrown(error, base) };
    };
    const run = (signatures: Record<string, Signature>, item: Item) => {
        const session: Session = {
            item,
            records: { alerts: [], confirms: [], prompts: [], files: [], windows: [], console: [], activations: [], events: [], opened: [], leaked: [] },
            answers: { confirms: [...(item.confirms ?? [])], prompts: [...(item.prompts ?? [])], files: [...(item.files ?? [])], windows: [...(item.windows ?? [])] },
            writes: [],
            docs: [],
            saved: [],
            timers: [],
            refused: [],
        };
        const globals = keyed(enumerated(shared), (key) => shared[key]);
        const held: { made: readonly string[]; before?: Reads; after?: Reads } = { made: [] };
        const ended = (() => {
            try {
                const { docs, made } = opened(session);
                held.made = made;
                const scope: Members = {
                    app: scoped(session, signatures, app, 'App'),
                    util: scoped(session, signatures, util, 'Util'),
                    console: scoped(session, signatures, root.console, 'Console'),
                    global: Object.create(shared, { setPersistent: { value: (name: unknown) => recorded(session, `Global.setPersistent ${String(name)}`) } }),
                };
                const target = docs[0] === undefined ? this : adopted(session, signatures, docs[0], false);
                const evaluate = (code: string, statement = false) => compile(scope, statement ? code : `return (${code});`)(target);
                (item.precondition.steps ?? []).forEach((step) => evaluate(step, true));
                held.before = reads(item, evaluate);
                const result = evaluated(session, scope, target, docs, item, evaluate);
                held.after = reads(item, evaluate);
                return { completion: { _tag: 'Ran' as const }, value: encoded(result) };
            } catch (error) {
                return caught(error);
            }
        })();
        const writes = restored(session, globals);
        const completion = session.refused.length > 0 ? { completion: { _tag: 'Failed' as const, reason: session.refused.join('; ') } } : {};
        return {
            outcome: {
                name: item.name,
                ...ended,
                ...completion,
                ...(held.before === undefined ? {} : { before: held.before.read, beforeFailed: held.before.failed }),
                read: held.after?.read ?? {},
                readFailed: held.after?.failed ?? {},
                writes,
                ...session.records,
            },
            kept: held.made.filter((file) => !has(session.saved, file)),
        };
    };
    const viewer = () => ({
        type: String(app.viewerType),
        variation: String(app.viewerVariation),
        language: String(app.language),
        plugIns: shaped(app.plugIns, isMemberList, 'app.plugIns').map((plugIn) => ({ name: String(plugIn.name), version: Number(plugIn.version), loaded: plugIn.loaded === true, certified: plugIn.certified === true })),
    });
    const surveyed = () => {
        const doc = shaped(invoked(app, 'newDoc', []), isDoc, 'app.newDoc');
        try {
            const field = doc.addField('survey', 'text', 0, SURVEY_RECT);
            const annot = doc.addAnnot({ type: 'Text', page: 0, point: SURVEY_POINT });
            const kinds = [root, app, util, root.console, shared, doc, field, annot, doc.bookmarkRoot];
            return { members: keyed(kinds.map(kindOf), (kind) => enumerated(kinds[kinds.map(kindOf).indexOf(kind)] ?? {})), signatures: signaturesOf({ App: app, Doc: doc, Util: util }) };
        } finally {
            doc.closeDoc(true);
        }
    };
    try {
        return encodeURIComponent(JSON.stringify({ _tag: 'Success', success: request._tag === 'Survey' ? { viewer: viewer(), model: surveyed() } : run(request.signatures, request.case) }));
    } catch (error) {
        return encodeURIComponent(JSON.stringify({ _tag: 'Failure', failure: { reason: thrown(error, base).message } }));
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { engine };
