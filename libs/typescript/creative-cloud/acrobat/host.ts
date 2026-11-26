// --- [TYPES] ---------------------------------------------------------------------------

type Host = Record<PropertyKey, unknown>;
type Call = (this: unknown, ...args: readonly unknown[]) => unknown;
type Answer = { readonly value: unknown } | { readonly refused: string } | undefined;
type Row = (args: readonly unknown[], owner: Host) => Answer;
interface Seen {
    readonly from: unknown[];
    readonly to: unknown[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const isObject = (value: unknown): value is Host => (typeof value === 'object' || typeof value === 'function') && value !== null;
const at = (args: readonly unknown[], index: number, name: string): unknown => {
    const [first] = args;
    return args.length === 1 && isObject(first) && !Array.isArray(first) ? first[name] : args[index];
};
const answer = (shown: boolean, value?: unknown): Answer => (shown ? { value } : undefined);
const refusal = (shown: boolean, refused: string): Answer => (shown ? { refused } : undefined);
const always =
    (value: unknown): Row =>
    () => ({ value });
const refused =
    (reason: string): Row =>
    () => ({ refused: reason });
const picked =
    (index: number, name: string, value?: unknown): Row =>
    (args) =>
        answer(at(args, index, name) === undefined, value);
const required =
    (index: number, name: string): Row =>
    (args) =>
        refusal(at(args, index, name) === undefined, `opens a file picker without ${name}`);
const flagged =
    (index: number, name: string): Row =>
    (args) =>
        refusal(at(args, index, name) === true, `shows a dialog when ${name} is true`);
const composed =
    (...rows: readonly Row[]): Row =>
    (args, owner) =>
        rows.reduce<Answer>((found, row) => found ?? row(args, owner), undefined);

// --- [TABLES] --------------------------------------------------------------------------

const DIALOG_ITEMS: Readonly<Record<string, true>> = { SaveAs: true, Open: true };
const APPLICATION: Readonly<Record<string, Row>> = {
    alert: (args) => {
        const checkbox = at(args, 5, 'oCheckbox');
        if (isObject(checkbox)) {
            Object.defineProperty(checkbox, 'bAfterValue', { value: checkbox['bInitialValue'] === true, writable: true, enumerable: true, configurable: true });
        }
        return { value: [1, 2, 3, 2][Number(at(args, 2, 'nType') ?? 0)] ?? 1 };
    },
    response: always(null),
    browseForDoc: always(undefined),
    popUpMenu: always(null),
    popUpMenuEx: always(null),
    execDialog: always('cancel'),
    mailGetAddrs: always(undefined),
    mailMsg: (args) => answer(at(args, 0, 'bUI') !== false),
    execMenuItem: (args) => {
        const item = String(at(args, 0, 'cMenuItem'));
        return refusal(Object.prototype.hasOwnProperty.call(DIALOG_ITEMS, item), `${item} opens a dialog`);
    },
};
const OBJECTS: Readonly<Record<string, Row>> = {
    closeDoc: (args, owner) => refusal(at(args, 0, 'bNoSave') !== true && owner['dirty'] === true, `prompts to save the changes of ${String(owner['path'])}: save it first, or pass bNoSave true to discard them`),
    exportAsFDF: picked(4, 'cPath'),
    exportAsText: picked(2, 'cPath'),
    exportAsXFDF: picked(3, 'cPath'),
    exportXFAData: picked(0, 'cPath'),
    exportDataObject: composed(flagged(2, 'bAllowAuth'), picked(1, 'cDIPath')),
    importAnFDF: picked(0, 'cPath'),
    importAnXFDF: picked(0, 'cPath'),
    importXFAData: picked(0, 'cPath'),
    importSound: picked(1, 'cDIPath'),
    importIcon: picked(1, 'cDIPath', 1),
    importTextData: composed(picked(0, 'cPath', -1), picked(1, 'nRow', -2)),
    importDataObject: required(1, 'cDIPath'),
    addEmbeddedFile: required(0, 'cDIPath'),
    readFileIntoStream: required(0, 'cDIPath'),
    addAnnot: (args) => refusal(at(args, 0, 'type') === 'FileAttachment' && at(args, 0, 'cAttachmentPath') === undefined, 'opens a file picker for a FileAttachment without cAttachmentPath'),
    browseForFileToSubmit: (_args, owner) => answer(owner['fileSelect'] === true),
    buttonImportIcon: (args) => answer(at(args, 0, 'cPath') === undefined && at(args, 1, 'nPage') === undefined, 1),
    mailDoc: (args) => answer(at(args, 0, 'bUI') !== false),
    mailForm: (args) => answer(at(args, 0, 'bUI') !== false),
    mail: (args) => answer(at(args, 0, 'bUI') !== false),
    print: (args) => {
        const named = at(args, 8, 'printParams');
        const params = named === undefined && args.length === 1 ? args[0] : named;
        const constants = isObject(params) ? params['constants'] : undefined;
        const levels = isObject(constants) ? constants['interactionLevel'] : undefined;
        return answer(at(args, 0, 'bUI') !== false && (isObject(levels) && isObject(params) ? params['interactive'] === levels['full'] : true));
    },
    applyRedactions: (args) => answer(at(args, 2, 'bShowConfirmation') === true, false),
    signatureSign: (args) => answer(at(args, 3, 'bUI') === true || (typeof at(args, 2, 'nUI') === 'number' && at(args, 2, 'nUI') !== 0), false),
    saveAs: flagged(4, 'bPromptToOverwrite'),
    submitForm: flagged(12, 'cPassword'),
    certifyInvisibleSign: flagged(3, 'bUI'),
    timestampSign: flagged(2, 'bUI'),
    encryptForRecipients: flagged(2, 'bUI'),
    encryptUsingPolicy: flagged(3, 'bUI'),
    embedDocAsDataObject: flagged(3, 'bUI'),
    getDataObjectContents: flagged(1, 'bAllowAuth'),
    signatureValidate: flagged(1, 'bUI'),
    login: flagged(3, 'bUI'),
    newUser: flagged(4, 'bUI'),
    connect: flagged(1, 'bUI'),
    search: flagged(3, 'bUI'),
    getSecurityPolicies: flagged(0, 'bUI'),
    importFromFile: flagged(2, 'bUI'),
    importSettings: always(-1),
    check: always(false),
    checkText: refused('opens the spelling dialog'),
    customDictionaryExport: refused('opens a folder picker'),
    chooseRecipientsDialog: refused('opens the recipients dialog'),
    chooseSecurityPolicy: refused('opens the security policy dialog'),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const suppressDialogs = (root: Host) => {
    const start = Number((new Error('suppressDialogs') as Error & { readonly lineNumber?: number }).lineNumber);
    const [reals, standIns]: readonly [unknown[], unknown[]] = [[], []];
    const located = (error: unknown): unknown => {
        const stack = isObject(error) ? error['stack'] : undefined;
        const line = (typeof stack === 'string' ? stack.split('\n') : []).reduce<number | undefined>((found, frame) => {
            const number = Number(/:(\d+)(?::\d+)?$/.exec(frame)?.[1]);
            return found ?? (number < start ? number : undefined);
        }, undefined);
        if (isObject(error) && line !== undefined && error['line'] === undefined) {
            error['line'] = line;
        }
        return error;
    };
    const guarded = (run: () => unknown): unknown => {
        try {
            return run();
        } catch (error) {
            throw located(error);
        }
    };
    const paired = (real: unknown, standIn: unknown): unknown => {
        reals.push(real);
        standIns.push(standIn);
        return standIn;
    };
    const toHost = (value: unknown, seen: Seen): unknown => {
        const [held, copied] = [standIns.indexOf(value), seen.from.indexOf(value)];
        if (!isObject(value) || held !== -1 || copied !== -1) {
            return held === -1 ? (copied === -1 ? value : seen.to[copied]) : reals[held];
        }
        if (typeof value === 'function') {
            const body = value as Call;
            const bridge = function (this: unknown, ...args: readonly unknown[]): unknown {
                return toHost(body.apply(wrap(this), args.map(wrap)), { from: [], to: [] });
            };
            paired(bridge, body);
            return bridge;
        }
        const plain = Array.isArray(value) || Object.getPrototypeOf(value) === Object.prototype;
        const copy: Host = Array.isArray(value) ? ([] as unknown as Host) : {};
        seen.from.push(value);
        seen.to.push(plain ? copy : value);
        Object.keys(plain ? value : {}).forEach((key) => {
            copy[key] = toHost(value[key], seen);
        });
        return plain ? copy : value;
    };
    const callable = (real: Call, owner: Host | undefined, member: string): Call => {
        const table = owner === root['app'] ? APPLICATION : OBJECTS;
        const row = owner !== undefined && Object.prototype.hasOwnProperty.call(table, member) ? table[member] : undefined;
        const made = function (this: unknown, ...args: readonly unknown[]): unknown {
            const answered = row === undefined || owner === undefined ? undefined : row(args, owner);
            if (answered !== undefined && 'refused' in answered) {
                throw { _tag: 'IllegalArgument', message: `${member} ${answered.refused}` };
            }
            const seen: Seen = { from: [], to: [] };
            const hosted = args.map((arg) => toHost(arg, seen));
            const constructed = this instanceof made;
            const receiver = owner ?? toHost(this, seen);
            return answered === undefined ? wrap(guarded(() => (constructed ? new (Function.prototype.bind.apply(real, [undefined, ...hosted]) as new () => unknown)() : real.apply(receiver, hosted)))) : answered.value;
        };
        return made;
    };
    const wrap = (value: unknown): unknown => {
        const held = reals.indexOf(value);
        if (!isObject(value) || held !== -1 || Array.isArray(value)) {
            return held === -1 ? (Array.isArray(value) ? value.map(wrap) : value) : standIns[held];
        }
        if (typeof value === 'function') {
            return paired(value, callable(value, undefined, ''));
        }
        const real = value;
        const made: Host = {};
        const define = (name: string) => {
            if (!Object.prototype.hasOwnProperty.call(made, name)) {
                Object.defineProperty(made, name, {
                    enumerable: true,
                    configurable: true,
                    get: () => {
                        const read = guarded(() => real[name]);
                        return typeof read === 'function' ? callable(read as Call, real, name) : wrap(read);
                    },
                    set: (written: unknown) => {
                        guarded(() => {
                            real[name] = toHost(written, { from: [], to: [] });
                        });
                    },
                });
            }
        };
        paired(real, made);
        for (const name in real) {
            define(name);
        }
        for (let level: unknown = real; isObject(level) && level !== Object.prototype; level = Object.getPrototypeOf(level)) {
            Object.getOwnPropertyNames(level).forEach(define);
        }
        Object.keys(real === root['app'] ? APPLICATION : OBJECTS)
            .filter((name) => name in real)
            .forEach(define);
        return made;
    };
    const unwrap = (value: unknown): unknown => {
        const held = standIns.indexOf(value);
        return held === -1 ? value : reals[held];
    };
    return { root: wrap(root), wrap, unwrap };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { suppressDialogs };
