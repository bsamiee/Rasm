import { FAULTS, faultOf, root } from '#/hosts/adobe/acrobat/engine/script';
import { next, posix, recorded, type Session, type Signature } from '#/hosts/adobe/acrobat/engine/session';
import { counted, encoded, has, isCallable, isDoc, isMembers, keyed, kindOf, type Members, revived, starts } from '#/hosts/adobe/acrobat/engine/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly session: Session;
    readonly key: string;
    readonly signature: Signature;
    readonly target: Members;
    readonly args: readonly unknown[];
    readonly call: (args: readonly unknown[]) => unknown;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUTTONS = { ok: 1, cancel: 2, no: 3, yes: 4 } as const;
const ALERTS = [[BUTTONS.ok], [BUTTONS.ok, BUTTONS.cancel], [BUTTONS.yes, BUTTONS.no], [BUTTONS.yes, BUTTONS.no, BUTTONS.cancel]] as const;
const CANCELLED: null = null;
const HELP = '====> ';
const PATHED = /Path$/u;
const SCALARS = ['string', 'integer', 'number', 'boolean'];
const SIGNED: Record<string, readonly string[]> = {
    App: ['alert', 'response', 'browseForDoc', 'browseForMultipleDocs', 'execDialog', 'openDoc'],
    Doc: ['closeDoc', 'saveAs', 'insertPages', 'replacePages', 'importDataObject', 'importIcon', 'importAnFDF', 'importAnXFDF', 'importTextData', 'importXFAData', 'exportDataObject', 'exportAsFDF', 'exportAsXFDF', 'exportAsText', 'exportXFAData'],
    Util: ['readFileIntoStream'],
};
const PICKERS = ['importDataObject', 'importIcon', 'importAnFDF', 'importAnXFDF', 'importTextData', 'importXFAData', 'exportDataObject', 'readFileIntoStream'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const pathy = (name: string) => PATHED.test(name);
const argued = ({ signature, args }: Pick<Call, 'signature' | 'args'>) => {
    const names = signature.map(({ name }) => name);
    const [first] = args;
    const [leading] = signature;
    const object = args.length === 1 && isMembers(first) && kindOf(first) === 'Object' && leading !== undefined && has(SCALARS, leading.type);
    return {
        values: keyed(names, (name) => (object ? first[name] : args[names.indexOf(name)])),
        rebuilt: (name: string, value: unknown) => (object ? [{ ...first, [name]: value }] : counted(Math.max(args.length, names.indexOf(name) + 1), (index) => (names[index] === name ? value : args[index]))),
    };
};
const chosen = (entries: readonly unknown[], answer: string | null): readonly unknown[] => entries.filter(isMembers).reduce<readonly unknown[]>((hits, entry) => hits.concat(entry.cName === answer ? [entry.cReturn ?? entry.cName] : chosen(Array.isArray(entry.oSubMenu) ? entry.oSubMenu : [entry.oSubMenu], answer)), []);
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
    const press = (id: string): void => {
        if (id === 'cancel') {
            dialog.end(id);
        } else if (id !== 'ok') {
            called(id);
        } else if (called('validate') !== false) {
            called('commit');
            dialog.end(id);
        }
    };
    const pressed: string[] = [];
    called('initialize');
    presses.every((id) => {
        if (ended.length > 0) {
            return false;
        }
        pressed.push(id);
        press(id);
        return true;
    });
    const result = ended[0] ?? 'cancel';
    called('destroy');
    return { pressed, result };
};
const picked = ({ session, key, signature, args, call }: Call) => {
    const { values, rebuilt } = argued({ signature, args });
    const [path] = signature.map(({ name }) => name).filter(pathy);
    if (path === undefined || values[path] !== undefined) {
        return call(args);
    }
    session.records.files.push(key);
    const file = next(session.answers.files);
    return typeof file === 'string' ? call(rebuilt(path, file)) : undefined;
};
const browsed = ({ session }: Call, label: string) => {
    session.records.files.push(label);
    const file = next(session.answers.files);
    return typeof file === 'string' ? { cFS: 'DOS', cPath: `${session.volume}${file}` } : undefined;
};
const prompted = ({ session }: Call, question: string) => {
    session.records.prompts.push(question);
    return next(session.answers.prompts);
};
const timed =
    (clear: string) =>
    ({ session, args, call }: Call) => {
        const timer = call(args);
        session.timers.push([clear, timer]);
        return timer;
    };
const ANSWERS: Record<string, (call: Call) => unknown> = {
    'App.alert': (call) => {
        const { records, answers } = call.session;
        const { cMsg, nType, cTitle, oCheckbox } = argued(call).values;
        const buttons = ALERTS[Number(nType ?? 0)] ?? ALERTS[0];
        records.alerts.push({ text: String(cMsg), ...(cTitle === undefined ? {} : { title: String(cTitle) }), ...(isMembers(oCheckbox) ? { checkbox: String(oCheckbox.cMsg) } : {}) });
        if (isMembers(oCheckbox)) {
            oCheckbox.bAfterValue = oCheckbox.bInitialValue === true;
        }
        if (buttons.length === 1) {
            return buttons[0];
        }
        records.confirms.push(String(cMsg));
        return next(answers.confirms) === true ? buttons[0] : buttons[1];
    },
    'App.response': (call) => prompted(call, String(argued(call).values.cQuestion)) ?? CANCELLED,
    'App.browseForDoc': (call) => browsed(call, argued(call).values.bSave === true ? `${call.key} save` : call.key),
    'App.browseForMultipleDocs': (call) => {
        const doc = browsed(call, call.key);
        return doc === undefined ? undefined : [doc];
    },
    'App.popUpMenu': (call) => prompted(call, JSON.stringify(encoded(call.args))) ?? CANCELLED,
    'App.popUpMenuEx': (call) => {
        const answer = prompted(call, JSON.stringify(encoded(call.args)));
        return answer === undefined ? CANCELLED : (chosen(call.args, answer)[0] ?? CANCELLED);
    },
    'App.execDialog': (call) => {
        const { monitor } = argued(call).values;
        const handlers = isMembers(monitor) ? monitor : {};
        const answer = next(call.session.answers.windows) ?? {};
        const { pressed, result } = simulated(handlers, answer.fields ?? {}, answer.presses ?? []);
        call.session.records.windows.push({ description: encoded(handlers.description), pressed, result });
        return result;
    },
    'App.execMenuItem': ({ session, key, args, call }) => {
        const item = String(args[0]);
        return has(session.request.modal, item) ? recorded(session, `${key} ${item}`) : call(args);
    },
    'App.setTimeOut': timed('clearTimeOut'),
    'App.setInterval': timed('clearInterval'),
    'Doc.closeDoc': (call) => {
        const { session, target } = call;
        if (!isDoc(target) || argued(call).values.bNoSave === true || target.dirty !== true) {
            return call.call(call.args);
        }
        session.records.confirms.push(`${call.key} ${target.path}`);
        if (next(session.answers.confirms) === true) {
            target.saveAs(target.path);
        }
        return call.call([true]);
    },
    'Doc.saveAs': (call) => {
        call.session.saved.push(posix(call.session, String(argued(call).values.cPath)));
        return call.call(call.args);
    },
    'Doc.exportDataObject': (call) => (Number(argued(call).values.nLaunch ?? 0) > 0 ? recorded(call.session, `${call.key} nLaunch`) : picked(call)),
    'Console.println': ({ session, args }) => {
        session.records.console.push(String(args[0]));
    },
};
const answerOf = (kind: string, member: string) => ANSWERS[`${kind}.${member}`] ?? (has(PICKERS, member) ? picked : undefined);
const pathed = (kind: string, member: string, signature: Signature | undefined) => (signature === undefined ? has(SIGNED[kind] ?? [], member) : signature.some(({ name }) => pathy(name)));
const helped = (target: Members, member: string): Signature | undefined => {
    const held = target[member];
    if (!isCallable(held)) {
        return undefined;
    }
    try {
        held.apply(target, [root.acrohelp]);
        return undefined;
    } catch (error) {
        const fault = faultOf(error);
        return fault.name === FAULTS.help
            ? String(fault.extMessage)
                  .split('\n')
                  .filter((line) => starts(line, HELP) && line.indexOf(':') > HELP.length)
                  .map((line) => {
                      const parameter = line.slice(HELP.length).replace('[', '').replace(']', '');
                      const colon = parameter.indexOf(':');
                      return { name: parameter.slice(0, colon).trim(), type: parameter.slice(colon + 1).trim(), optional: line.charAt(HELP.length) === '[' };
                  })
            : undefined;
    }
};
const signaturesOf = (targets: Record<string, Members>) => {
    const made: Record<string, Signature> = {};
    Object.keys(SIGNED).forEach((kind) =>
        (SIGNED[kind] ?? []).forEach((member) => {
            const signature = helped(targets[kind] ?? {}, member);
            if (signature !== undefined) {
                made[`${kind}.${member}`] = signature;
            }
        }),
    );
    return made;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { answerOf, pathed, signaturesOf };
