import type { Schema } from 'effect';
import type { HostCase, Model, Outcome, Request } from '#/acrobat/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Item = typeof HostCase.Type;
type Members = Record<string, unknown>;
type Callable = (...args: readonly unknown[]) => unknown;
type Listed<T> = { -readonly [K in keyof T]-?: NonNullable<T[K]> extends readonly (infer A)[] ? A[] : never };
type Answers = Listed<Pick<Item, 'confirms' | 'prompts' | 'files' | 'windows'>>;
type Records = Listed<Pick<typeof Outcome.Encoded, 'alerts' | 'confirms' | 'prompts' | 'files' | 'windows' | 'console' | 'activations' | 'events' | 'opened' | 'leaked'>>;
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
    readonly records: Records;
    readonly answers: Answers;
    readonly writes: Write[];
    readonly docs: Doc[];
    readonly saved: string[];
    readonly timers: [string, unknown][];
    readonly refused: string[];
}
interface Fault {
    readonly name?: string;
    readonly message?: string;
    readonly extMessage?: string;
    readonly lineNumber?: number;
    readonly stack?: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = function (this: unknown, request: typeof Request.Type): string {
    const NUMBERS: Members = { undefined, NaN: Number.NaN, Infinity: Number.POSITIVE_INFINITY, '-Infinity': Number.NEGATIVE_INFINITY, '-0': -0 };
    const DATED = 'date:';
    const VALUED = ['Object', 'Array', 'Date', 'Error', 'Function', 'Boolean', 'Number', 'String', 'RegExp'];
    const BUTTONS = { ok: 1, cancel: 2, no: 3, yes: 4 } as const;
    const OK_CANCEL = 1;
    const SIGNED = {
        App: ['alert', 'response', 'browseForDoc', 'browseForMultipleDocs', 'execDialog', 'openDoc'],
        Doc: ['closeDoc', 'saveAs', 'insertPages', 'replacePages', 'importDataObject', 'importIcon', 'importAnFDF', 'importAnXFDF', 'importTextData', 'importXFAData', 'exportDataObject', 'exportAsFDF', 'exportAsXFDF', 'exportAsText', 'exportXFAData'],
        Util: ['readFileIntoStream'],
    };
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
    const pathy = (name: string) => name.slice(-PATH.length) === PATH;
    const activating = (kind: string, member: string) => {
        const listed = ACTIVATING[kind];
        return listed !== undefined && (listed.names.indexOf(member) >= 0 || listed.prefixes.some((prefix) => member.indexOf(prefix) === 0));
    };
    const isMembers = (value: unknown): value is Members => value !== null && typeof value === 'object';
    const isCallable = (value: unknown): value is Callable => typeof value === 'function';
    const kindOf = (value: unknown) => Object.prototype.toString.call(value).slice(8, -1);
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
    const counted = <T>(count: number, each: (index: number) => T): T[] => {
        const made: T[] = [];
        for (let index = 0; index < count; index += 1) {
            made.push(each(index));
        }
        return made;
    };
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
    const root = compile({}, 'return this')(undefined) as Root;
    const { app, util, acrohelp } = root;
    const shared = root.global;
    const base = ((): number => {
        try {
            compile({}, 'throw new Error("")')(undefined);
            return 0;
        } catch (error) {
            return (isMembers(error) && typeof error.lineNumber === 'number' ? error.lineNumber : 1) - 1;
        }
    })();
    const documents = String((app.getPath as Callable)('user', 'documents'));
    const volume = documents.indexOf(request.home) > 0 ? documents.slice(0, documents.indexOf(request.home)) : '';
    const posix = (path: string) => (volume !== '' && path.indexOf(`${volume}/`) === 0 ? path.slice(volume.length) : path);
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
        } catch (_) {
            return '';
        }
    };
    const encoded = (value: unknown, seen: readonly unknown[] = []): Schema.Json => {
        const kind = kindOf(value);
        if (value === undefined) {
            return { $: 'undefined' };
        }
        if (typeof value === 'number') {
            return numbered(value);
        }
        if (value === null || typeof value === 'string' || typeof value === 'boolean') {
            return value;
        }
        if (kind === 'Date') {
            const time = (value as Date).getTime();
            return { $: `${DATED}${Number.isNaN(time) ? 'NaN' : (value as Date).toISOString()}` };
        }
        if (!isMembers(value) || seen.indexOf(value) >= 0) {
            return typeof value === 'function' ? '<function>' : String(value);
        }
        const inner = seen.concat([value]);
        if (Array.isArray(value)) {
            return value.map((item) => encoded(item, inner));
        }
        if (VALUED.indexOf(kind) >= 0) {
            return kind === 'Object' ? keyed(Object.keys(value), (key) => encoded(value[key], inner)) : String(value);
        }
        if (kind === 'Annot' && isCallable(value.getProps)) {
            const props = value.getProps() as Members;
            return { $: `object:Annot${named(value)}`, ...keyed(Object.keys(props), (key) => encoded(props[key], inner)) };
        }
        return { $: `object:${kind}${kind === 'Doc' ? `/${(value as Doc).path}` : named(value)}` };
    };
    const tagged = (tag: string): unknown => {
        if (tag in NUMBERS) {
            return NUMBERS[tag];
        }
        // @effect-diagnostics-next-line globalDate:off
        return tag.indexOf(DATED) === 0 ? new Date(tag.slice(DATED.length)) : { $: tag };
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
        const fault: Fault = isMembers(error) ? error : { message: String(error) };
        const message = fault.name === 'HelpError' ? String(fault.extMessage) : `${fault.name ?? 'Error'}: ${fault.message ?? ''}`;
        const frames = String(fault.stack ?? '')
            .split('\n')
            .filter((line) => line.indexOf('@') >= 0)
            .map((line) => ({ name: line.slice(0, line.indexOf('@')), line: line.slice(line.lastIndexOf(':') + 1) }));
        const ending = frames.map(({ name }) => name).indexOf('anonymous');
        const stack = frames.slice(0, ending + 1).map(({ line }) => ({ line: Number(line) - offset }));
        return {
            _tag: 'Thrown' as const,
            message,
            ...(fault.lineNumber === undefined ? {} : { line: fault.lineNumber - offset }),
            ...(ending < 0 ? {} : { stack }),
        };
    };
    const recorded = (session: Session, member: string): undefined => {
        session.records.activations.push(member);
    };
    const refuse = (session: Session, reason: string) => {
        session.refused.push(reason);
        throw { refused: reason };
    };
    const signaturesOf = (targets: Record<string, Members>): Record<string, Signature> => {
        const made: Record<string, Signature> = {};
        Object.keys(SIGNED).forEach((kind) =>
            SIGNED[kind as keyof typeof SIGNED].forEach((member) => {
                try {
                    (targets[kind]?.[member] as Callable)(acrohelp);
                } catch (error) {
                    const fault: Fault = isMembers(error) ? error : {};
                    if (fault.name === 'HelpError') {
                        made[`${kind}.${member}`] = String(fault.extMessage)
                            .split('\n')
                            .filter((line) => line.indexOf(HELP) === 0)
                            .map((line) => {
                                const [name = '', type = ''] = line.slice(HELP.length).replace('[', '').replace(']', '').split(':');
                                return { name: name.trim(), type: type.trim(), optional: line.charAt(HELP.length) === '[' };
                            });
                    }
                }
            }),
        );
        return made;
    };
    const argued = (signature: Signature, args: readonly unknown[]) => {
        const [first] = args;
        const object = args.length === 1 && isMembers(first) && kindOf(first) === 'Object' && ['string', 'integer', 'number', 'boolean'].indexOf(signature[0]?.type ?? '') >= 0;
        return {
            values: keyed(
                signature.map(({ name }) => name),
                (name) => (object ? first[name] : args[signature.map((held) => held.name).indexOf(name)]),
            ),
            rebuilt: (name: string, value: unknown) => (object ? [{ ...first, [name]: value }] : counted(Math.max(args.length, signature.map((held) => held.name).indexOf(name) + 1), (index) => (signature[index]?.name === name ? value : args[index]))),
        };
    };
    const answered = (session: Session, signatures: Record<string, Signature>, kind: string, member: string, held: Callable, target: Members) => {
        const { records, answers } = session;
        const signature = signatures[`${kind}.${member}`] ?? [];
        const call = (args: readonly unknown[]) => held.apply(target, [...args]);
        const next = <T>(list: T[]) => list.splice(0, 1)[0];
        const picker = (args: readonly unknown[]) => {
            const { values, rebuilt } = argued(signature, args);
            const path = signature.map(({ name }) => name).filter(pathy)[0] ?? '';
            if (values[path] !== undefined) {
                return call(args);
            }
            records.files.push(`${kind}.${member}`);
            const file = next(answers.files);
            return typeof file === 'string' ? call(rebuilt(path, file)) : undefined;
        };
        const table: Record<string, Callable> = {
            'App.alert': (...args) => {
                const { cMsg, nType, cTitle, oCheckbox } = argued(signature, args).values;
                const checkbox = isMembers(oCheckbox) ? oCheckbox : undefined;
                records.alerts.push({ text: String(cMsg), ...(cTitle === undefined ? {} : { title: String(cTitle) }), ...(checkbox === undefined ? {} : { checkbox: String(checkbox.cMsg) }) });
                if (checkbox !== undefined) {
                    checkbox.bAfterValue = checkbox.bInitialValue === true;
                }
                if (nType === undefined || nType === 0) {
                    return BUTTONS.ok;
                }
                records.confirms.push(String(cMsg));
                const agreed = next(answers.confirms) === true;
                if (nType === OK_CANCEL) {
                    return agreed ? BUTTONS.ok : BUTTONS.cancel;
                }
                return agreed ? BUTTONS.yes : BUTTONS.no;
            },
            'App.response': (...args) => {
                records.prompts.push(String(argued(signature, args).values.cQuestion));
                const answer = next(answers.prompts);
                return answer === undefined ? null : answer;
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
                return next(answers.prompts) ?? null;
            },
            'App.popUpMenuEx': (...items) => {
                records.prompts.push(JSON.stringify(encoded(items)));
                const answer = next(answers.prompts);
                const found = (list: readonly unknown[]): unknown =>
                    list.map((entry) => (isMembers(entry) ? (entry.cName === answer ? (entry.cReturn ?? entry.cName) : found(Array.isArray(entry.oSubMenu) ? entry.oSubMenu : isMembers(entry.oSubMenu) ? [entry.oSubMenu] : [])) : null)).filter((hit) => hit !== null)[0] ?? null;
                return answer === undefined ? null : found(items);
            },
            'App.execDialog': (...args) => {
                const { monitor } = argued(signature, args).values;
                const handlers = isMembers(monitor) ? monitor : {};
                const answer = next(answers.windows) ?? {};
                const values: Members = {};
                const ended: string[] = [];
                const dialog = {
                    load: (loaded: unknown) =>
                        Object.keys(isMembers(loaded) ? loaded : {}).forEach((key) => {
                            values[key] = (loaded as Members)[key];
                        }),
                    store: () => ({ ...values, ...keyed(Object.keys(answer.fields ?? {}), (key) => revived(answer.fields?.[key])) }),
                    enable: () => undefined,
                    visible: () => undefined,
                    focus: () => undefined,
                    gotoPage: () => undefined,
                    end: (id: unknown) => {
                        ended.push(String(id ?? 'cancel'));
                    },
                };
                const called = (name: string) => (isCallable(handlers[name]) ? (handlers[name] as Callable).apply(handlers, [dialog]) : undefined);
                const pressed: string[] = [];
                called('initialize');
                (answer.presses ?? []).forEach((id) => {
                    if (ended.length > 0) {
                        return;
                    }
                    pressed.push(id);
                    if (id === 'ok' && called('validate') !== false) {
                        called('commit');
                        ended.push('ok');
                    } else if (id === 'cancel') {
                        ended.push('cancel');
                    } else if (id !== 'ok') {
                        called(id);
                    }
                });
                const result = ended[0] ?? 'cancel';
                called('destroy');
                records.windows.push({ description: encoded(handlers.description), pressed, result });
                return result;
            },
            'App.execMenuItem': (...args) => {
                const [item] = args;
                return request.modal.indexOf(String(item)) >= 0 ? recorded(session, `App.execMenuItem ${String(item)}`) : call(args);
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
                const doc = target as Doc;
                if (argued(signature, args).values.bNoSave !== true && doc.dirty === true) {
                    records.confirms.push(`Doc.closeDoc ${doc.path}`);
                    return next(answers.confirms) === true ? (doc.saveAs(doc.path), call([true])) : call([true]);
                }
                return call(args);
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
        return table[`${kind}.${member}`] ?? (PICKERS.indexOf(member) >= 0 ? (...args: readonly unknown[]) => picker(args) : undefined);
    };
    const scoped = (session: Session, signatures: Record<string, Signature>, target: Members, kind: string): Members => {
        const guarded = (member: string, held: Callable) => {
            const own = answered(session, signatures, kind, member, held, target);
            const pathed = signatures[`${kind}.${member}`]?.some(({ name }) => pathy(name)) === true;
            return (...args: readonly unknown[]) => {
                if (args[0] === acrohelp) {
                    return held.apply(target, [...args]);
                }
                if (activating(kind, member)) {
                    return recorded(session, `${kind}.${member}`);
                }
                const outside = (value: unknown): readonly string[] =>
                    typeof value === 'string' ? (value.indexOf('/') === 0 && posix(value).indexOf(`${session.item.folder}/`) !== 0 ? [value] : []) : isMembers(value) && kindOf(value) === 'Object' ? Object.keys(value).reduce<readonly string[]>((found, key) => found.concat(outside(value[key])), []) : [];
                const stray = pathed ? outside({ ...args }) : [];
                if (stray.length > 0) {
                    refuse(session, `${kind}.${member} reaches ${stray.join(', ')} outside the case folder`);
                }
                return adopted(session, signatures, own === undefined ? held.apply(target, [...args]) : own(...args), true);
            };
        };
        return Object.defineProperties(
            {},
            keyed(enumerated(target), (key) => ({
                enumerable: true,
                get: () => {
                    const held = target[key];
                    return isCallable(held) ? guarded(key, held) : adopted(session, signatures, held, false);
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
    };
    const adopted = (session: Session, signatures: Record<string, Signature>, value: unknown, made: boolean): unknown => {
        const kind = kindOf(value);
        if (Array.isArray(value)) {
            return value.map((item) => adopted(session, signatures, item, made));
        }
        if (kind === 'Doc' && made && session.docs.indexOf(value as Doc) < 0) {
            session.docs.push(value as Doc);
        }
        return kind === 'Doc' || kind === 'FullScreen' ? scoped(session, signatures, value as Members, kind) : value;
    };
    const spread = (expression: string, evaluate: (code: string) => unknown): unknown => {
        const at = expression.indexOf('[*]');
        if (at < 0) {
            return evaluate(expression);
        }
        const list = evaluate(expression.slice(0, at));
        const count = isMembers(list) && typeof list.length === 'number' ? list.length : 0;
        return counted(count, (index) => spread(`${expression.slice(0, at)}[${index}]${expression.slice(at + 3)}`, evaluate));
    };
    const reads = (item: Item, evaluate: (code: string) => unknown) => {
        const read: Record<string, Schema.Json> = {};
        const failed: Record<string, string> = {};
        Object.keys(item.read).forEach((key) => {
            try {
                const value = spread(item.read[key] ?? 'undefined', evaluate);
                if (key.indexOf('[*]') >= 0 && Array.isArray(value)) {
                    value.forEach((each, index) => {
                        read[key.split('[*]').join(`[${index}]`)] = encoded(each);
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
        const tree = (mark: Members): Schema.Json => ({ name: String(mark.name), children: (Array.isArray(mark.children) ? mark.children : []).map((child: Members) => tree(child)) });
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
            dataObjects: (Array.isArray(doc.dataObjects) ? doc.dataObjects : []).map((data: Members) => String(data.name)),
        };
    };
    const opened = (session: Session) => {
        const { precondition, folder } = session.item;
        const docs = precondition.documents.map((document) => {
            if ('open' in document) {
                return (app.openDoc as Callable)(document.open) as Doc;
            }
            const doc = (app.newDoc as Callable)(document.width, document.height) as Doc;
            counted(document.pages - 1, (page) => doc.newPage(page + 1, document.width, document.height));
            if (document.rotation !== undefined) {
                doc.setPageRotations(0, doc.numPages - 1, document.rotation);
            }
            Object.keys(document.boxes ?? {}).forEach((box) => doc.setPageBoxes(box, 0, doc.numPages - 1, document.boxes?.[box as keyof typeof document.boxes] ?? []));
            return doc;
        });
        docs.forEach((doc) => session.docs.push(doc));
        const [first] = docs;
        if (first !== undefined) {
            (precondition.fields ?? []).forEach(({ name, type, page, rect, properties = {} }) => {
                const field = first.addField(name, type, page, rect);
                Object.keys(properties).forEach((key) => {
                    field[key] = revived(properties[key]);
                });
            });
            (precondition.annots ?? []).forEach((properties) => first.addAnnot(revived(properties)));
            Object.keys(precondition.info ?? {}).forEach((key) => {
                first.info[key] = precondition.info?.[key];
            });
            if (precondition.metadata !== undefined) {
                first.metadata = precondition.metadata;
            }
        }
        const made = precondition.documents.map((document, index) => ('open' in document ? '' : `${folder}/${index}.pdf`));
        docs.forEach((doc, index) => {
            if (made[index] !== '') {
                doc.saveAs(made[index] ?? '');
            }
        });
        docs.forEach((doc) => session.records.opened.push(state(doc)));
        return { docs, made: made.filter((file) => file !== '') };
    };
    const fired = (session: Session, scope: Members, target: unknown, doc: Doc | undefined, code: string, event: NonNullable<Item['precondition']['event']>) => {
        const field = doc?.getField(event.field) ?? null;
        if (doc === undefined || field === null) {
            return refuse(session, `no field ${event.field} on the first document`);
        }
        compile(scope, code);
        const key = `rasm${now()}`;
        const holder: Members = { events: [], scope, target, thrown: undefined };
        doc[key] = holder;
        const names = Object.keys(scope);
        (field.setAction as Callable)(
            event.trigger,
            `try { (function (${names.join(', ')}) {\n${code}\n}).apply(this.${key}.target, [${names.map((name) => `this.${key}.scope.${name}`).join(', ')}]); } catch (error) { this.${key}.thrown = error; }\nthis.${key}.events.push({ type: event.type, name: event.name, rc: event.rc, value: event.value, change: event.change, willCommit: event.willCommit });`,
        );
        try {
            if (event.trigger === 'Calculate') {
                doc.calculateNow();
            } else {
                field.value = revived(event.value);
            }
        } finally {
            (field.setAction as Callable)(event.trigger, '');
            delete doc[key];
        }
        (holder.events as readonly unknown[]).forEach((each) => session.records.events.push(encoded(each)));
        if (holder.thrown !== undefined) {
            throw { action: holder.thrown };
        }
    };
    const restored = (session: Session, globals: Members) => {
        const { records } = session;
        session.timers.forEach(([clear, timer]) => (app[clear] as Callable)(timer));
        const writes = session.writes.map(({ target, key, member, before }) => {
            const after = encoded(target[key]);
            try {
                target[key] = before;
            } catch (error) {
                records.leaked.push(`${member}: ${thrown(error, base).message}`);
            }
            return { member, before: encoded(before), after };
        });
        session.docs
            .slice()
            .reverse()
            .forEach((doc) => {
                try {
                    doc.closeDoc(true);
                } catch (error) {
                    records.leaked.push(`close: ${thrown(error, base).message}`);
                }
            });
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
    const run = (signatures: Record<string, Signature>) => (item: Item) => {
        const start = now();
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
        const held: { made: readonly string[]; before?: ReturnType<typeof reads>; after?: ReturnType<typeof reads> } = { made: [] };
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
                const event = item.precondition.event;
                const result = (() => {
                    if (item._tag === 'Probe') {
                        return evaluate(item.evaluate);
                    }
                    if (item._tag === 'Run') {
                        return event === undefined ? evaluate(item.source.code, true) : fired(session, scope, target, docs[0], item.source.code, event);
                    }
                    const callee = evaluate(`${item.source?.code ?? ''}\nreturn (${item.call});`, true);
                    return isCallable(callee) ? callee.apply(target, item.args.map(revived)) : callee;
                })();
                held.after = reads(item, evaluate);
                return { completion: { _tag: 'Ran' as const }, value: encoded(result) };
            } catch (error) {
                if (isMembers(error) && typeof error.refused === 'string') {
                    return { completion: { _tag: 'Failed' as const, reason: error.refused } };
                }
                return isMembers(error) && 'action' in error ? { completion: thrown(error.action, 1) } : { completion: thrown(error, base) };
            }
        })();
        const writes = restored(session, globals);
        const completion = session.refused.length > 0 ? { completion: { _tag: 'Failed' as const, reason: session.refused.join('; ') } } : {};
        return {
            outcome: {
                name: item.name,
                ...ended,
                ...completion,
                ...(held.before === undefined ? {} : { before: held.before.read }),
                read: held.after?.read ?? {},
                readFailed: { ...held.before?.failed, ...held.after?.failed },
                answered: [],
                written: [],
                writes,
                ...session.records,
            },
            span: { start, end: now(), kept: held.made.filter((file) => session.saved.indexOf(file) < 0) },
        };
    };
    const viewer = () => ({
        type: String(app.viewerType),
        variation: String(app.viewerVariation),
        language: String(app.language),
        plugIns: (app.plugIns as readonly Members[]).map((plugIn) => ({ name: String(plugIn.name), version: Number(plugIn.version), loaded: plugIn.loaded === true, certified: plugIn.certified === true })),
    });
    const surveyed = () => {
        const doc = (app.newDoc as Callable)() as Doc;
        try {
            const field = doc.addField('survey', 'text', 0, [0, 20, 20, 0]);
            const annot = doc.addAnnot({ type: 'Text', page: 0, point: [10, 10] });
            const kinds = [root, app, util, root.console, shared, doc, field, annot, doc.bookmarkRoot];
            return { members: keyed(kinds.map(kindOf), (kind) => enumerated(kinds[kinds.map(kindOf).indexOf(kind)] ?? {})), signatures: signaturesOf({ App: app, Doc: doc, Util: util }) };
        } finally {
            doc.closeDoc(true);
        }
    };
    try {
        const model = request._tag === 'Survey' ? surveyed() : undefined;
        const ran = request.cases.map(run(model?.signatures ?? (request._tag === 'Cases' ? request.signatures : {})));
        return encodeURIComponent(JSON.stringify({ _tag: request._tag, viewer: viewer(), outcomes: ran.map(({ outcome }) => outcome), spans: ran.map(({ span }) => span), ...(model === undefined ? {} : { model }) }));
    } catch (error) {
        return encodeURIComponent(JSON.stringify({ _tag: 'Failed', reason: thrown(error, base).message }));
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { engine };
