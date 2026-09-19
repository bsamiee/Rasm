// --- [IMPORTS] -------------------------------------------------------------------------

import type { Schema } from 'effect';
import type { HostRejection } from '../errors.ts';
import type { Action } from './actions.ts';
import type { Color, Field, Fields, Operation } from './scripts.ts';

// --- [HOST] ----------------------------------------------------------------------------

const PAGE_ROTATIONS = { upright: 0, clockwise: 90, inverted: 180, counterclockwise: 270 } as const;

interface DocumentRequest {
    readonly quad: readonly [number, number, number, number, number, number, number, number];
    readonly range: { readonly kind: 'all' } | { readonly kind: 'pages'; readonly pages: readonly number[] } | { readonly kind: 'span'; readonly start: number; readonly end: number };
    readonly target: { readonly kind: 'page'; readonly page: number } | { readonly kind: 'label'; readonly label: string };
    readonly pageBoxes: {
        readonly op: 'pageBoxes';
        readonly pages: DocumentRequest['range'];
        readonly boxes: readonly {
            readonly box: 'Trim' | 'Bleed' | 'Art' | 'Crop' | 'Media';
            readonly set: { readonly kind: 'rect'; readonly rect: readonly [number, number, number, number] } | { readonly kind: 'inset'; readonly inset: number } | { readonly kind: 'remove' };
        }[];
    };
    readonly pageLabels: {
        readonly op: 'pageLabels';
        readonly request: {
            readonly kind: 'sections';
            readonly sections: readonly { readonly page: number; readonly style: 'D' | 'R' | 'r' | 'A' | 'a'; readonly prefix: string; readonly start: number }[];
        };
    };
    readonly organize: {
        readonly op: 'organize';
        readonly operations: readonly (
            | { readonly op: 'replace'; readonly at: DocumentRequest['target']; readonly from: { readonly path: string; readonly start: number; readonly end: number } }
            | { readonly op: 'extract'; readonly pages: DocumentRequest['range']; readonly to: string }
            | { readonly op: 'move'; readonly page: number; readonly after: number }
            | { readonly op: 'rotate'; readonly pages: DocumentRequest['range']; readonly degrees: (typeof PAGE_ROTATIONS)[keyof typeof PAGE_ROTATIONS] }
            | { readonly op: 'delete'; readonly pages: DocumentRequest['range'] }
            | { readonly op: 'blank'; readonly after: number; readonly width: number; readonly height: number }
        )[];
    };
    readonly metadata: { readonly op: 'metadata'; readonly info: readonly (readonly [string, string])[]; readonly xmp?: string };
    readonly layers: {
        readonly op: 'layers';
        readonly layers: readonly { readonly name: string; readonly state?: boolean; readonly initState?: boolean; readonly locked?: boolean; readonly intent?: 'view' | 'design' }[];
        readonly order?: readonly string[];
    };
    readonly attachments: { readonly op: 'attachments'; readonly attachments: readonly { readonly name: string; readonly path: string }[]; readonly remove: readonly string[] };
    readonly scan: { readonly op: 'scan'; readonly pages: DocumentRequest['range'] };
    readonly comments: {
        readonly op: 'comments';
        readonly request: { readonly kind: 'list' } | { readonly kind: 'export'; readonly path: string; readonly fields: boolean } | { readonly kind: 'import'; readonly path: string };
    };
    readonly links: {
        readonly op: 'links';
        readonly matches: readonly {
            readonly page: number;
            readonly text: string;
            readonly quads: readonly DocumentRequest['quad'][];
            readonly target: DocumentRequest['target'] | { readonly kind: 'url'; readonly url: string };
            readonly border: boolean;
        }[];
    };
    readonly redact: {
        readonly op: 'redact';
        readonly matches: readonly { readonly page: number; readonly wordIndex: number; readonly text: string; readonly quads: readonly DocumentRequest['quad'][] }[];
        readonly keepMarks: boolean;
        readonly overlay?: { readonly text: string; readonly repeat: boolean; readonly alignment: 0 | 1 | 2; readonly fill: (typeof Color)['Encoded'] };
    };
    readonly stamp: {
        readonly op: 'stamp';
        readonly appearance: string;
        readonly pages: DocumentRequest['range'];
        readonly rect: readonly [number, number, number, number];
        readonly opacity?: number;
        readonly rotation?: number;
        readonly flatten: boolean;
    };
    readonly export: { readonly op: 'export'; readonly converter: string; readonly output: string };
    readonly encrypt: { readonly op: 'encrypt'; readonly policy: string };
}

interface DocumentResult {
    readonly pageBoxes: {
        readonly kind: 'pageBoxes';
        readonly applied: readonly {
            readonly page: number;
            readonly box: DocumentRequest['pageBoxes']['boxes'][number]['box'];
            readonly before: readonly number[];
            readonly after: readonly number[];
        }[];
    };
    readonly pageLabels: { readonly kind: 'pageLabels'; readonly labels: readonly string[] };
    readonly organize: { readonly kind: 'organized'; readonly applied: readonly number[]; readonly numPages: number; readonly labels: readonly string[]; readonly rotations: readonly number[] };
    readonly metadata: { readonly kind: 'metadata'; readonly info: Readonly<Record<string, Schema.Json>>; readonly xmp: string; readonly xmpLength: number };
    readonly layers: {
        readonly kind: 'layers';
        readonly applied: readonly {
            readonly name: string;
            readonly from: { readonly state: boolean; readonly initState: boolean; readonly locked: boolean };
            readonly to: { readonly state: boolean; readonly initState: boolean; readonly locked: boolean };
        }[];
        readonly absent: readonly string[];
    };
    readonly attachments: { readonly kind: 'attachments'; readonly dataObjects: readonly { readonly name: string; readonly size: number; readonly mimeType: string }[] };
    readonly scan: {
        readonly kind: 'text';
        readonly pages: readonly {
            readonly page: number;
            readonly label: string;
            readonly words: readonly { readonly wordIndex: number; readonly word: string; readonly quads: readonly DocumentRequest['quad'][] }[];
        }[];
    };
    readonly comments:
        | {
              readonly kind: 'comments';
              readonly rows: readonly {
                  readonly name: string;
                  readonly type: string;
                  readonly page: number;
                  readonly author: string;
                  readonly contents: string;
                  readonly modDate: string;
                  readonly rect: readonly number[];
                  readonly inReplyTo?: string;
              }[];
          }
        | { readonly kind: 'commentsExported'; readonly path: string; readonly count: number }
        | { readonly kind: 'commentsImported'; readonly before: number; readonly after: number };
    readonly links: {
        readonly kind: 'links';
        readonly added: readonly {
            readonly page: number;
            readonly text: string;
            readonly target: DocumentRequest['links']['matches'][number]['target'];
            readonly rectangles: readonly (readonly number[])[];
        }[];
    };
    readonly redact: { readonly kind: 'redacted'; readonly marks: DocumentRequest['redact']['matches']; readonly applied: boolean };
    readonly stamp: { readonly kind: 'stamped'; readonly annots: readonly { readonly page: number; readonly name: string; readonly appearance: string }[] };
    readonly export: { readonly kind: 'exported'; readonly path: string; readonly converter: string } | { readonly kind: 'converterAbsent'; readonly available: readonly string[] };
    readonly encrypt:
        | { readonly kind: 'encrypted'; readonly policyId: string; readonly securityHandler: string }
        | { readonly kind: 'policyAbsent'; readonly available: readonly string[] }
        | { readonly kind: 'encryptionRefused'; readonly code: 1 | 2; readonly message: string };
    readonly auditTrail: {
        readonly name: string;
        readonly creator: string;
        readonly creatorVersion: string;
        readonly formatVersion: string;
        readonly results: string;
        readonly description: string;
        readonly executedDate: string;
        readonly fingerprint: string;
    };
}

type AcrobatField = { -readonly [K in keyof (typeof Field)['Encoded']]-?: (typeof Field)['Encoded'][K] } & {
    comb: boolean;
    doNotSpellCheck: boolean;
    doNotScroll: boolean;
    richText: boolean;
    radiosInUnison: boolean;
    commitOnSelChange: boolean;
    editable: boolean;
    multipleSelection: boolean;
    getItemAt: (index: number, exported: boolean) => string;
    setItems: (items: readonly (readonly [string, string])[]) => void;
    setAction: (trigger: string, code: string) => void;
    defaultIsChecked: (widget: number, checked: boolean) => void;
    buttonSetCaption: (caption: string) => void;
};

interface AcrobatDocument {
    readonly path: string;
    readonly documentFileName: string;
    readonly numPages: number;
    readonly numFields: number;
    readonly dirty: boolean;
    readonly securityHandler: string;
    // biome-ignore lint/style/useNamingConvention: Adobe Data exposes this exact read-only native property; replies map it to mimeType.
    readonly dataObjects: readonly { readonly name: string; readonly size: number; readonly MIMEType: string }[] | null;
    info: Record<string, Schema.Json>;
    metadata: string;
    readonly bookmarkRoot: { createChild: (label: string, script: string, index: number) => void };
    getNthFieldName: (index: number) => string;
    getField: (name: string) => AcrobatField | null;
    addField: (name: string, type: (typeof Field)['Type']['type'], page: number, rect: readonly number[]) => AcrobatField;
    calculateNow: () => void;
    saveAs: (options: { readonly cPath: string; readonly cConvID?: string; readonly bCopy?: boolean }) => void;
    closeDoc: (discard: boolean) => void;
    insertPages: (options: { readonly nPage: number; readonly cPath: string; readonly nStart?: number; readonly nEnd?: number }) => void;
    deletePages: (start: number, end?: number) => void;
    getPageBox: (box: DocumentRequest['pageBoxes']['boxes'][number]['box'], page: number) => readonly [number, number, number, number];
    setPageBoxes: (box: DocumentRequest['pageBoxes']['boxes'][number]['box'], start: number, end: number, rect?: readonly number[]) => void;
    getPageLabel: (page: number) => string;
    setPageLabels: (page: number, label?: readonly [string, string, number]) => void;
    getPageRotation: (page: number) => number;
    setPageRotations: (start: number, end: number, degrees: number) => void;
    replacePages: (page: number, path: string, start: number, end: number) => void;
    extractPages: () => AcrobatDocument;
    movePage: (page: number, after: number) => void;
    newPage: (after: number, width: number, height: number) => void;
    getPageNumWords: (page: number) => number;
    getPageNthWord: (page: number, word: number, strip: boolean) => string;
    getPageNthWordQuads: (page: number, word: number) => readonly DocumentRequest['quad'][] | null;
    getOCGs: () => readonly AcrobatOCG[] | null;
    setOCGOrder: (order: readonly AcrobatOCG[]) => void;
    importDataObject: (options: { readonly cName: string; readonly cDIPath: string }) => boolean;
    removeDataObject: (name: string) => void;
    syncAnnotScan: () => void;
    getAnnots: () => readonly AcrobatAnnotation[] | null;
    exportAsXFDF: (options: { readonly cPath: string; readonly bAnnotations: true; readonly bAllFields: boolean; readonly aFields?: readonly string[] }) => void;
    importAnXFDF: (path: string) => void;
    addLink: (page: number, rect: readonly number[]) => { borderWidth: number; setAction: (script: string) => void };
    addAnnot: (properties: {
        readonly type: 'Redact' | 'Stamp';
        readonly page: number;
        readonly rect?: readonly number[];
        readonly quads?: readonly DocumentRequest['quad'][];
        readonly AP?: string;
        readonly opacity?: number;
        readonly rotate?: number;
        readonly overlayText?: string;
        readonly repeat?: boolean;
        readonly alignment?: 0 | 1 | 2;
        readonly fillColor?: (typeof Color)['Encoded'];
    }) => AcrobatAnnotation;
    encryptUsingPolicy: (options: {
        readonly oPolicy: { readonly policyId: string };
        readonly bUI: false;
    }) => { readonly errorCode: 0; readonly policyApplied: { readonly policyId: string } } | { readonly errorCode: 1; readonly errorText: string } | { readonly errorCode: 2 };
    setPageTabOrder: (page: number, order: 'rows' | 'columns' | 'structure') => void;
    preflight: (
        profile: object,
        omitFixups: boolean,
    ) => { readonly numErrors: number; readonly numWarnings: number; readonly numInfos: number; readonly numFixed: number; readonly numNotFixed: number; report: () => string };
    getPreflightAuditTrail: () =>
        | Readonly<
              Record<
                  | 'profile_name'
                  | 'profile_creator'
                  | 'profile_creator_version'
                  | 'profile_format_version'
                  | 'preflight_results'
                  | 'preflight_results_description'
                  | 'preflight_executed_date'
                  | 'profile_fingerprint',
                  string
              >
          >
        | undefined;
    getColorConvertAction: () => {
        matchAttributesAny: number;
        matchSpaceTypeAny: number;
        matchIntent: number;
        action: number;
        convertProfile: string;
        convertIntent: number;
        embed: boolean;
        preserveBlack: boolean;
        useBlackPointCompensation: boolean;
        readonly constants: {
            readonly spaceFlags: Readonly<Record<'AlternateSpace', number>>;
            readonly renderingIntents: Readonly<Record<'Any' | 'RelativeColorimetric', number>>;
            readonly actions: Readonly<Record<'Convert', number>>;
        };
    };
    colorConvertPage: (page: number, actions: readonly ReturnType<AcrobatDocument['getColorConvertAction']>[], inkAliases: readonly unknown[]) => void;
    embedOutputIntent: (profile: string) => void;
    flattenPages: (options: { readonly nStart?: number; readonly nEnd?: number; readonly nNonPrint: number }) => void;
    addWatermarkFromText: (options: {
        readonly cText: string;
        readonly cFont?: string;
        readonly nFontSize?: number;
        readonly aColor?: (typeof Field)['Encoded']['fillColor'];
        readonly nOpacity?: number;
        readonly nRotation?: number;
        readonly nStart?: number;
        readonly nEnd?: number;
    }) => void;
    applyRedactions: (options?: { readonly aMarks: readonly AcrobatAnnotation[]; readonly bKeepMarks: boolean; readonly bShowConfirmation: false }) => boolean;
}

interface AcrobatOCG {
    readonly name: string;
    state: boolean;
    initState: boolean;
    locked: boolean;
    readonly constants: { readonly intents: Readonly<Record<'view' | 'design', string>> };
    setIntent: (intent: readonly string[]) => void;
}

interface AcrobatAnnotation {
    readonly name: string;
    readonly type: string;
    readonly page: number;
    readonly author: string;
    readonly contents: string;
    readonly modDate: Date;
    readonly rect: readonly number[];
    readonly inReplyTo?: string;
    readonly AP: string;
}

interface AcrobatMenu {
    readonly cName: string;
    readonly oChildren?: readonly AcrobatMenu[] | null;
}

declare const app: {
    readonly activeDocs: readonly AcrobatDocument[];
    readonly viewerVersion: number;
    readonly viewerType: string;
    readonly language: string;
    readonly platform: string;
    readonly fromPDFConverters: readonly string[];
    openDoc: (options: { readonly cPath: string; readonly bHidden: boolean }) => AcrobatDocument;
    newDoc: () => AcrobatDocument;
    listMenuItems: () => readonly AcrobatMenu[];
    execMenuItem: (name: string, document?: AcrobatDocument) => void;
};

declare const Preflight: {
    getNumProfiles: () => number;
    getNthProfile: (index: number) => { readonly name: string; readonly description: string; readonly hasFixups: boolean; readonly hasChecks: boolean };
    getProfileByName: (name: string) => ReturnType<typeof Preflight.getNthProfile> | null | undefined;
    createComplianceProfile: (name: string) => ReturnType<typeof Preflight.getNthProfile> | null | undefined;
    getNumLibraries: () => number;
    getNthLibrary: (index: number) => { readonly name: string; readonly description: string; readonly locked: boolean };
    getLibraryByName: (name: string) => ReturnType<typeof Preflight.getNthLibrary> | null | undefined;
    getDefaultLibrary: () => ReturnType<typeof Preflight.getNthLibrary>;
    setDefaultLibrary: (library: ReturnType<typeof Preflight.getNthLibrary>) => boolean;
};

declare const security: {
    getSecurityPolicies: () => readonly { readonly name: string; readonly policyId: string; readonly handler: string }[] | null;
};

declare class Matrix2D {
    fromRotated: (document: AcrobatDocument, page: number) => Matrix2D;
    invert: () => Matrix2D;
    transform: (quad: DocumentRequest['quad']) => DocumentRequest['quad'];
}

// --- [FIELD CAPABILITIES] --------------------------------------------------------------

const _WIDGET = ['readonly', 'textSize', 'lineWidth', 'strokeColor', 'fillColor', 'borderStyle'] as const;
const FIELD_TYPES: Readonly<
    Record<
        'text' | 'button' | 'combobox' | 'listbox' | 'checkbox' | 'radiobutton' | 'signature',
        {
            readonly read: readonly Exclude<keyof (typeof Field)['Encoded'], 'name' | 'type' | 'page' | 'rect' | 'items'>[];
            readonly defaults: readonly FieldWrite[];
            readonly configure: readonly Extract<keyof (typeof Fields)['Encoded'][number], 'format' | 'items' | 'caption' | 'mouseUp'>[];
        }
    >
> = {
    text: { configure: ['format'], read: [..._WIDGET, 'value', 'defaultValue', 'required', 'textFont', 'calcOrderIndex', 'multiline', 'charLimit', 'alignment'], defaults: [] },
    button: { configure: ['caption', 'mouseUp'], read: [..._WIDGET, 'textFont'], defaults: [] },
    combobox: {
        configure: ['format', 'items'],
        read: [..._WIDGET, 'value', 'defaultValue', 'required', 'textFont', 'calcOrderIndex', 'numItems'],
        defaults: [
            ['commitOnSelChange', true],
            ['editable', false],
        ],
    },
    listbox: {
        configure: ['items'],
        read: [..._WIDGET, 'value', 'defaultValue', 'required', 'textFont', 'numItems'],
        defaults: [
            ['commitOnSelChange', true],
            ['multipleSelection', false],
        ],
    },
    checkbox: {
        configure: [],
        read: [..._WIDGET, 'value', 'defaultValue', 'required', 'style', 'exportValues'],
        defaults: [
            ['exportValues', ['Yes']],
            ['style', 'check'],
        ],
    },
    radiobutton: {
        configure: [],
        read: [..._WIDGET, 'value', 'defaultValue', 'required', 'style', 'exportValues'],
        defaults: [
            ['style', 'circle'],
            ['radiosInUnison', false],
        ],
    },
    signature: { configure: [], read: [..._WIDGET, 'value', 'required'], defaults: [] },
} as const;

type FieldWrite = { [Key in keyof AcrobatField]: AcrobatField[Key] extends Schema.Json ? readonly [Key, AcrobatField[Key]] : never }[keyof AcrobatField];

// --- [BOUNDARY] ------------------------------------------------------------------------

function literal(value: unknown): string {
    const encoded = JSON.stringify(value);
    if (encoded === undefined) {
        return _reject({ _tag: 'resultNotJson', cause: { type: typeof value } });
    }
    const hexadecimal = 16;
    const codeUnitDigits = 4;
    // biome-ignore lint/nursery/useUnicodeRegex: AppleScript needs ASCII JSON; escaping each UTF-16 code unit preserves surrogate pairs.
    return encoded.replace(/[\u007f-\uffff]/g, (unit) => `\\u${`0000${unit.charCodeAt(0).toString(hexadecimal)}`.slice(-codeUnitDigits)}`);
}

function run(work: () => unknown): string {
    try {
        const value = work();
        return literal({ _tag: 'Success', success: value === undefined ? null : value });
    } catch (error: unknown) {
        const fields = error && typeof error === 'object' ? error : {};
        const failure =
            'rejection' in fields
                ? fields.rejection
                : {
                      _tag: 'scriptThrew',
                      name: 'name' in fields && typeof fields.name === 'string' ? fields.name : 'ThrownValue',
                      message: 'message' in fields && typeof fields.message === 'string' ? fields.message : String(error),
                      line: 'lineNumber' in fields ? fields.lineNumber : undefined,
                      fileName: 'fileName' in fields ? fields.fileName : undefined,
                  };
        return literal({ _tag: 'Failure', failure });
    }
}

function document(device: string, path: string): AcrobatDocument {
    const open = app.activeDocs.reduce<AcrobatDocument | undefined>((found, candidate) => (found === undefined && candidate.path === device ? candidate : found), undefined);
    return open === undefined ? _reject({ _tag: 'documentNotOpen', path }) : open;
}

function state(): {
    readonly kind: 'state';
    readonly viewerVersion: number;
    readonly viewerType: string;
    readonly language: string;
    readonly platform: string;
    readonly documents: readonly { readonly path: string; readonly fileName: string; readonly numPages: number; readonly numFields: number; readonly dirty: boolean }[];
    readonly converters: readonly string[];
} {
    return {
        kind: 'state',
        viewerVersion: app.viewerVersion,
        viewerType: app.viewerType,
        language: app.language,
        platform: app.platform,
        documents: app.activeDocs.map((open) => ({ path: open.path, fileName: open.documentFileName, numPages: open.numPages, numFields: open.numFields, dirty: open.dirty })),
        converters: app.fromPDFConverters,
    };
}

function openDocument(path: string, hidden: boolean): { readonly kind: 'opened'; readonly path: string; readonly fileName: string; readonly numPages: number } {
    const open = app.openDoc({ cPath: path, bHidden: hidden });
    return { kind: 'opened', path: open.path, fileName: open.documentFileName, numPages: open.numPages };
}

function _menus(items: readonly AcrobatMenu[] | null | undefined, depth: number): { readonly cName: string; readonly depth: number }[] {
    if (items === undefined || items === null) {
        return [];
    }
    return items.reduce<ReturnType<typeof _menus>>((rows, item) => rows.concat({ cName: item.cName, depth }, _menus(item.oChildren, depth + 1)), []);
}

function menu(): { readonly kind: 'menu'; readonly items: ReturnType<typeof _menus> } {
    return { kind: 'menu', items: _menus(app.listMenuItems(), 0) };
}

function execMenuItem(this: AcrobatDocument, name: string, scoped: boolean): { readonly kind: 'executed'; readonly name: string } {
    if (scoped) {
        app.execMenuItem(name, this);
    } else {
        app.execMenuItem(name);
    }
    return { kind: 'executed', name };
}

function _field(field: AcrobatField): Record<string, Schema.Json> {
    const row: Record<string, Schema.Json> = { name: field.name, type: field.type, page: field.page, rect: field.rect };
    for (const property of FIELD_TYPES[field.type].read) {
        row[property] = field[property];
    }
    if (field.type === 'combobox' || field.type === 'listbox') {
        const items: { readonly label: string; readonly export: string }[] = [];
        for (let index = 0; index < field.numItems; index += 1) {
            items.push({ label: field.getItemAt(index, false), export: field.getItemAt(index, true) });
        }
        row['items'] = items;
    }
    return row;
}

function getFields(this: AcrobatDocument, names: readonly string[] | null): { readonly kind: 'fields'; readonly fields: readonly Record<string, Schema.Json>[]; readonly absent: readonly string[] } {
    const selected: string[] = [];
    for (let index = 0, count = names === null ? this.numFields : 0; index < count; index += 1) {
        selected.push(this.getNthFieldName(index));
    }
    const found = (names === null ? selected : names).map((name) => ({ name, field: this.getField(name) }));
    return {
        kind: 'fields',
        fields: found.filter((row): row is { readonly name: string; readonly field: AcrobatField } => row.field !== null).map(({ field }) => _field(field)),
        absent: found.filter(({ field }) => field === null).map(({ name }) => name),
    };
}

function setFields<Key extends keyof AcrobatField>(
    this: AcrobatDocument,
    specs: readonly (Omit<(typeof Fields)['Encoded'][number], 'format' | 'readOnly' | 'required' | 'defaultValue' | 'mouseUp' | 'exportValues'> & {
        readonly unsupported: Readonly<Record<keyof typeof FIELD_TYPES, readonly string[]>>;
        readonly writes: Readonly<Record<keyof typeof FIELD_TYPES, readonly (readonly [Key, AcrobatField[Key]])[]>>;
        readonly overrides: readonly (readonly [Key, AcrobatField[Key]])[];
        readonly actions: readonly (readonly [string, string])[];
        readonly operands: readonly string[];
        readonly mouseUp?: string;
    })[],
    save: string | null,
): {
    readonly kind: 'fieldsApplied';
    readonly applied: readonly { readonly name: string; readonly from: Record<string, Schema.Json> | null; readonly to: Record<string, Schema.Json> }[];
    readonly rejected: readonly {
        readonly name: string;
        readonly reason: { readonly _tag: 'fieldAbsent' } | { readonly _tag: 'fieldTypeConflict'; readonly type: string; readonly properties: readonly string[] };
    }[];
} {
    const outcomes = specs.map((spec) => {
        const found = this.getField(spec.name);
        const target = found === null ? spec.create : { type: found.type, field: found };
        if (target === undefined) {
            return { name: spec.name, reason: { _tag: 'fieldAbsent' as const } };
        }
        const unsupported = spec.unsupported[target.type];
        if (unsupported.length > 0) {
            return { name: spec.name, reason: { _tag: 'fieldTypeConflict' as const, type: target.type, properties: unsupported } };
        }
        const from = found === null ? null : _field(found);
        const field = 'field' in target ? target.field : this.addField(spec.name, target.type, target.page, target.rect);
        for (const [key, value] of spec.writes[target.type]) {
            field[key] = value;
        }
        for (const [trigger, script] of spec.actions) {
            field.setAction(trigger, script);
        }
        if (spec.operands.length > 0) {
            const computable = spec.operands
                .map((name) => this.getField(name))
                .filter((operand): operand is AcrobatField => operand !== null && (operand.type === 'text' || operand.type === 'combobox'));
            field.calcOrderIndex = computable.reduce((highest, operand) => Math.max(highest, operand.calcOrderIndex + 1), 0);
        }
        if (field.type === 'checkbox') {
            field.defaultIsChecked(0, false);
        }
        if ((field.type === 'combobox' || field.type === 'listbox') && spec.items !== undefined) {
            field.setItems(spec.items.map((item) => [item.label, item.export]));
        }
        if (field.type === 'button' && spec.caption !== undefined) {
            field.buttonSetCaption(spec.caption);
        }
        if (field.type === 'button' && spec.mouseUp !== undefined) {
            field.setAction('MouseUp', spec.mouseUp);
        }
        for (const [key, value] of spec.overrides) {
            field[key] = value;
        }
        return { name: spec.name, from, field };
    });
    this.calculateNow();
    const applied = outcomes.filter((outcome) => outcome.field !== undefined).map((outcome) => ({ name: outcome.name, from: outcome.from, to: _field(outcome.field) }));
    const rejected = outcomes.filter((outcome) => outcome.reason !== undefined);
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'fieldsApplied', applied, rejected };
}

function tabOrder(
    this: AcrobatDocument,
    selectedPages: readonly number[] | null,
    order: 'rows' | 'columns' | 'structure',
    save: string | null,
): { readonly kind: 'tabOrder'; readonly applied: readonly number[] } {
    const all: number[] = [];
    for (let page = 0, count = selectedPages === null ? this.numPages : 0; page < count; page += 1) {
        all.push(page);
    }
    const selected = selectedPages === null ? all : selectedPages;
    const applied = selected.map((page) => {
        this.setPageTabOrder(page, order);
        return page;
    });
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'tabOrder', applied };
}

function _library<A>(name: string | null, work: (selected: ReturnType<typeof Preflight.getNthLibrary>, original: ReturnType<typeof Preflight.getNthLibrary>) => A): A {
    const original = Preflight.getDefaultLibrary();
    const selected = name === null ? original : Preflight.getLibraryByName(name);
    if (selected === null || selected === undefined) {
        return _reject({ _tag: 'malformedParams', cause: { library: name } });
    }
    const changed = selected.name !== original.name;
    let outcome: { readonly _tag: 'Success'; readonly value: A } | { readonly _tag: 'Failure'; readonly cause: unknown };
    try {
        if (changed && !Preflight.setDefaultLibrary(selected)) {
            return _reject({ _tag: 'malformedParams', cause: { library: name, reason: 'librarySelectionRefused' } });
        }
        outcome = { _tag: 'Success', value: work(selected, original) };
    } catch (cause: unknown) {
        outcome = { _tag: 'Failure', cause };
    }
    if (changed && !Preflight.setDefaultLibrary(original)) {
        return _reject({ _tag: 'malformedParams', cause: { library: original.name, reason: 'libraryRestorationRefused', operationError: outcome._tag === 'Failure' ? String(outcome.cause) : null } });
    }
    if (outcome._tag === 'Failure') {
        throw outcome.cause;
    }
    return outcome.value;
}

function runPreflight(
    this: AcrobatDocument,
    method: 'getProfileByName' | 'createComplianceProfile',
    profile: string,
    fixups: boolean,
    report: boolean,
    save: string | null,
    library: string | null,
): {
    readonly numErrors: number;
    readonly numWarnings: number;
    readonly numInfos: number;
    readonly numFixed: number;
    readonly numNotFixed: number;
    readonly report?: string;
    readonly auditTrail?: DocumentResult['auditTrail'];
} {
    return _library(library, () => {
        const selected = Preflight[method](profile);
        if (selected === undefined || selected === null) {
            return _reject({ _tag: 'profileAbsent', profile });
        }
        const result = this.preflight(selected, !fixups);
        const audit = this.getPreflightAuditTrail();
        const value = {
            numErrors: result.numErrors,
            numWarnings: result.numWarnings,
            numInfos: result.numInfos,
            numFixed: result.numFixed,
            numNotFixed: result.numNotFixed,
            ...(report ? { report: result.report() } : {}),
            ...(audit === undefined
                ? {}
                : {
                      auditTrail: {
                          name: audit.profile_name,
                          creator: audit.profile_creator,
                          creatorVersion: audit.profile_creator_version,
                          formatVersion: audit.profile_format_version,
                          results: audit.preflight_results,
                          description: audit.preflight_results_description,
                          executedDate: audit.preflight_executed_date,
                          fingerprint: audit.profile_fingerprint,
                      },
                  }),
        };
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return value;
    });
}

function listPreflight(library: string | null): {
    readonly kind: 'preflightLibraries';
    readonly libraries: readonly (ReturnType<typeof Preflight.getNthLibrary> & { readonly isDefault: boolean })[];
    readonly profiles: readonly ReturnType<typeof Preflight.getNthProfile>[];
    readonly defaultLibrary: string;
    readonly selectedLibrary: string;
} {
    return _library(library, (selected, original) => {
        const libraries: (ReturnType<typeof Preflight.getNthLibrary> & { readonly isDefault: boolean })[] = [];
        for (let index = 0, count = Preflight.getNumLibraries(); index < count; index += 1) {
            const { name, description, locked } = Preflight.getNthLibrary(index);
            libraries.push({ name, description, locked, isDefault: name === original.name });
        }
        const profiles: ReturnType<typeof Preflight.getNthProfile>[] = [];
        for (let index = 0, count = Preflight.getNumProfiles(); index < count; index += 1) {
            const { name, description, hasFixups, hasChecks } = Preflight.getNthProfile(index);
            profiles.push({ name, description, hasFixups, hasChecks });
        }
        return { kind: 'preflightLibraries', libraries, profiles, defaultLibrary: original.name, selectedLibrary: selected.name };
    });
}

function autotag(this: AcrobatDocument, profile: string, command: string): { readonly before: number } {
    const before = runPreflight.call(this, 'getProfileByName', profile, false, false, null, null).numErrors;
    app.execMenuItem(command, this);
    return { before };
}

function tagged(
    this: AcrobatDocument,
    profile: string,
    before: number,
    save: string | null,
): { readonly kind: 'tagged'; readonly numErrors: { readonly before: number; readonly after: number }; readonly dirty: boolean } {
    const after = runPreflight.call(this, 'getProfileByName', profile, false, false, null, null).numErrors;
    const { dirty } = this;
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'tagged', numErrors: { before, after }, dirty };
}

function printProduction(
    this: AcrobatDocument,
    operations: readonly (
        | Extract<(typeof Operation)['Encoded'], { readonly op: 'colorConvertPage' }>
        | {
              [Method in Exclude<(typeof Operation)['Encoded']['op'], 'colorConvertPage'>]: { readonly op: Method; readonly args: Parameters<AcrobatDocument[Method]> };
          }[Exclude<(typeof Operation)['Encoded']['op'], 'colorConvertPage'>]
    )[],
    targets: Readonly<Record<'digital' | 'print', { readonly profile: string; readonly preserveBlack: boolean }>>,
    save: string | null,
): {
    readonly kind: 'production';
    readonly applied: readonly number[];
    readonly rejected: readonly { readonly opIndex: number; readonly reason: { readonly name: string; readonly message: string } }[];
} {
    const applied: number[] = [];
    const rejected: { readonly opIndex: number; readonly reason: { readonly name: string; readonly message: string } }[] = [];
    operations.forEach((operation, opIndex) => {
        try {
            switch (operation.op) {
                case 'colorConvertPage': {
                    const conversion = this.getColorConvertAction();
                    conversion.matchAttributesAny = -1;
                    // biome-ignore lint/suspicious/noBitwiseOperators: Adobe matchSpaceTypeAny is a native 32-bit bitmap; arithmetic complement changes signed-mask semantics.
                    conversion.matchSpaceTypeAny = ~conversion.constants.spaceFlags.AlternateSpace;
                    conversion.matchIntent = conversion.constants.renderingIntents.Any;
                    conversion.action = conversion.constants.actions.Convert;
                    conversion.convertProfile = targets[operation.target].profile;
                    conversion.convertIntent = conversion.constants.renderingIntents.RelativeColorimetric;
                    conversion.embed = true;
                    conversion.preserveBlack = targets[operation.target].preserveBlack;
                    conversion.useBlackPointCompensation = true;
                    this.colorConvertPage(operation.page, [conversion], []);
                    break;
                }
                case 'embedOutputIntent':
                    this.embedOutputIntent(...operation.args);
                    break;
                case 'flattenPages':
                    this.flattenPages(...operation.args);
                    break;
                case 'addWatermarkFromText':
                    this.addWatermarkFromText(...operation.args);
                    break;
                case 'applyRedactions':
                    this.applyRedactions(...operation.args);
                    break;
            }
            applied.push(opIndex);
        } catch (error: unknown) {
            rejected.push({ opIndex, reason: { name: error instanceof Error ? error.name : 'ThrownValue', message: error instanceof Error ? error.message : String(error) } });
        }
    });
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'production', applied, rejected };
}

function combine(
    sources: readonly { readonly path: string; readonly range: Omit<Parameters<AcrobatDocument['insertPages']>[0], 'nPage' | 'cPath'>; readonly label?: string }[],
    output: string,
):
    | { readonly kind: 'nothingInserted'; readonly skipped: readonly { readonly path: string; readonly reason: { readonly _tag: 'insertRefused'; readonly message: string } }[] }
    | {
          readonly kind: 'combined';
          readonly path: string;
          readonly numPages: number;
          readonly bookmarks: number;
          readonly skipped: readonly { readonly path: string; readonly reason: { readonly _tag: 'insertRefused'; readonly message: string } }[];
      } {
    const open = app.newDoc();
    try {
        const inserted = sources.map((source) => {
            const first = open.numPages - 1;
            try {
                open.insertPages({ nPage: first, cPath: source.path, ...source.range });
                return { label: source.label, page: first };
            } catch (error: unknown) {
                return { path: source.path, reason: { _tag: 'insertRefused' as const, message: error instanceof Error ? error.message : String(error) } };
            }
        });
        const skipped = inserted.filter((item) => item.reason !== undefined);
        const marks = inserted.filter((item) => item.page !== undefined);
        if (marks.length === 0) {
            return { kind: 'nothingInserted', skipped };
        }
        open.deletePages(0);
        const bookmarks = marks.filter((mark): mark is { readonly label: string; readonly page: number } => mark.label !== undefined);
        bookmarks.forEach(({ label, page }, index) => {
            open.bookmarkRoot.createChild(label, `this.pageNum = ${page}`, index);
        });
        open.saveAs({ cPath: output });
        return { kind: 'combined', path: output, numPages: open.numPages, bookmarks: bookmarks.length, skipped };
    } finally {
        open.closeDoc(true);
    }
}

function action(this: AcrobatDocument, step: Extract<(typeof Action)['Encoded']['groups'][number]['steps'][number], { readonly op: 'command' | 'preflight' | 'save' }>, output: string): void {
    switch (step.op) {
        case 'command':
            app.execMenuItem(step.name, this);
            break;
        case 'preflight':
            runPreflight.call(this, 'getProfileByName', step.profile, step.fixups, false, null, null);
            break;
        case 'save':
            this.saveAs({ cPath: output });
            break;
    }
}

function runAction(source: string, work: (this: AcrobatDocument) => unknown): void {
    const existing = app.activeDocs.slice();
    const open = app.openDoc({ cPath: source, bHidden: true });
    const owned = !existing.some((candidate) => candidate === open);
    try {
        work.call(open);
    } finally {
        if (owned) {
            open.closeDoc(true);
        }
    }
}

// --- [DOCUMENT OPERATIONS] -------------------------------------------------------------

function _pages(this: AcrobatDocument, range: DocumentRequest['range']): number[] {
    if (range.kind === 'span' && (range.start < 0 || range.start > range.end || range.end >= this.numPages)) {
        return _reject({ _tag: 'malformedParams', cause: { range, numPages: this.numPages } });
    }
    const selected: number[] = [];
    const start = range.kind === 'span' ? range.start : 0;
    const end = range.kind === 'span' ? range.end : this.numPages - 1;
    if (range.kind === 'pages') {
        selected.push(...range.pages.filter((page, index, values) => values.indexOf(page) === index));
    } else {
        for (let page = start; page <= end; page += 1) {
            selected.push(page);
        }
    }
    if (selected.length === 0 || selected.some((page) => page < 0 || page >= this.numPages)) {
        return _reject({ _tag: 'malformedParams', cause: { range, numPages: this.numPages } });
    }
    return selected;
}

function _page(this: AcrobatDocument, target: DocumentRequest['target']): number {
    if (target.kind === 'page') {
        _pages.call(this, { kind: 'pages', pages: [target.page] });
        return target.page;
    }
    const matching = _pages.call(this, { kind: 'all' }).filter((page) => this.getPageLabel(page) === target.label);
    const [selected] = matching;
    return selected !== undefined && matching.length === 1 ? selected : _reject({ _tag: 'malformedParams', cause: { label: target.label, matching } });
}

function pages(this: AcrobatDocument, request: DocumentRequest['pageBoxes' | 'pageLabels' | 'organize'], save: string | null): DocumentResult['pageBoxes' | 'pageLabels' | 'organize'] {
    if (request.op === 'pageBoxes') {
        const changes = _pages.call(this, request.pages).reduce<
            {
                readonly page: number;
                readonly box: DocumentRequest['pageBoxes']['boxes'][number]['box'];
                readonly before: readonly number[];
                readonly after: readonly [number, number, number, number] | null;
            }[]
        >(
            (all, page) =>
                all.concat(
                    request.boxes.map(({ box, set }) => {
                        const before = this.getPageBox(box, page);
                        const media = this.getPageBox('Media', page);
                        const inset: readonly [number, number, number, number] | null =
                            set.kind === 'inset' ? [media[0] + set.inset, media[1] - set.inset, media[2] - set.inset, media[3] + set.inset] : null;
                        const after = set.kind === 'rect' ? set.rect : inset;
                        return { page, box, before, after };
                    }),
                ),
            [],
        );
        const invalid = changes.filter(({ after }) => after !== null && (after[0] >= after[2] || after[1] <= after[3]));
        if (invalid.length > 0) {
            return _reject({ _tag: 'malformedParams', cause: invalid });
        }
        for (const { page, box, after } of changes) {
            if (after === null) {
                this.setPageBoxes(box, page, page);
            } else {
                this.setPageBoxes(box, page, page, after);
            }
        }
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'pageBoxes', applied: changes.map(({ page, box, before }) => ({ page, box, before, after: this.getPageBox(box, page) })) };
    }
    if (request.op === 'pageLabels') {
        _pages.call(this, { kind: 'pages', pages: request.request.sections.map(({ page }) => page) });
        for (const section of request.request.sections) {
            this.setPageLabels(section.page, [section.style, section.prefix, section.start]);
        }
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'pageLabels', labels: _pages.call(this, { kind: 'all' }).map((page) => this.getPageLabel(page)) };
    }
    const applied = request.operations.map((operation, index) => {
        switch (operation.op) {
            case 'replace':
                this.replacePages(_page.call(this, operation.at), operation.from.path, operation.from.start, operation.from.end);
                break;
            case 'move':
                _pages.call(this, { kind: 'pages', pages: operation.after === -1 ? [operation.page] : [operation.page, operation.after] });
                this.movePage(operation.page, operation.after);
                break;
            case 'blank':
                if (operation.after !== -1) {
                    _pages.call(this, { kind: 'pages', pages: [operation.after] });
                }
                this.newPage(operation.after + 1, operation.width, operation.height);
                break;
            case 'rotate':
                for (const page of _pages.call(this, operation.pages)) {
                    this.setPageRotations(page, page, operation.degrees);
                }
                break;
            case 'delete': {
                const selected = _pages.call(this, operation.pages).sort((left, right) => right - left);
                if (selected.length === this.numPages) {
                    return _reject({ _tag: 'malformedParams', cause: { operation, reason: 'emptyDocument' } });
                }
                for (const page of selected) {
                    this.deletePages(page);
                }
                break;
            }
            case 'extract': {
                const selected = _pages.call(this, operation.pages);
                const omitted = _pages
                    .call(this, { kind: 'all' })
                    .filter((page) => selected.indexOf(page) === -1)
                    .reverse();
                const extracted = this.extractPages();
                try {
                    for (const page of omitted) {
                        extracted.deletePages(page);
                    }
                    extracted.saveAs({ cPath: operation.to });
                } finally {
                    extracted.closeDoc(true);
                }
                break;
            }
        }
        return index;
    });
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    const selected = _pages.call(this, { kind: 'all' });
    return { kind: 'organized', applied, numPages: this.numPages, labels: selected.map((page) => this.getPageLabel(page)), rotations: selected.map((page) => this.getPageRotation(page)) };
}

function resources(this: AcrobatDocument, request: DocumentRequest['metadata' | 'layers' | 'attachments'], save: string | null): DocumentResult['metadata' | 'layers' | 'attachments'] {
    if (request.op === 'metadata') {
        if (request.xmp !== undefined) {
            this.metadata = request.xmp;
        }
        for (const [key, value] of request.info) {
            this.info[key] = value;
        }
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'metadata', info: this.info, xmp: this.metadata, xmpLength: this.metadata.length };
    }
    if (request.op === 'attachments') {
        for (const name of request.remove) {
            this.removeDataObject(name);
        }
        for (const attachment of request.attachments) {
            this.importDataObject({ cName: attachment.name, cDIPath: attachment.path });
        }
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        const objects = this.dataObjects;
        return { kind: 'attachments', dataObjects: objects === null ? [] : objects.map(({ name, size, MIMEType }) => ({ name, size, mimeType: MIMEType })) };
    }
    const found = this.getOCGs();
    const layers = found === null ? [] : found;
    const missingOrder = request.order === undefined ? [] : request.order.filter((name) => !layers.some((layer) => layer.name === name));
    if (missingOrder.length > 0) {
        return _reject({ _tag: 'malformedParams', cause: { order: missingOrder } });
    }
    const applied: DocumentResult['layers']['applied'][number][] = [];
    const changes = request.layers.reduce<{ readonly layer: AcrobatOCG; readonly desired: DocumentRequest['layers']['layers'][number] }[]>(
        (all, desired) => all.concat(layers.filter(({ name }) => name === desired.name).map((layer) => ({ layer, desired }))),
        [],
    );
    for (const { layer, desired } of changes) {
        const from = { state: layer.state, initState: layer.initState, locked: layer.locked };
        if (desired.state !== undefined) {
            layer.state = desired.state;
        }
        if (desired.initState !== undefined) {
            layer.initState = desired.initState;
        }
        if (desired.locked !== undefined) {
            layer.locked = desired.locked;
        }
        if (desired.intent !== undefined) {
            layer.setIntent([layer.constants.intents[desired.intent]]);
        }
        applied.push({ name: layer.name, from, to: { state: layer.state, initState: layer.initState, locked: layer.locked } });
    }
    if (request.order !== undefined) {
        const { order } = request;
        this.setOCGOrder(
            order.reduce<AcrobatOCG[]>((ordered, name) => ordered.concat(layers.filter((layer) => layer.name === name)), []).concat(layers.filter((layer) => order.indexOf(layer.name) === -1)),
        );
    }
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'layers', applied, absent: request.layers.filter(({ name }) => !layers.some((layer) => layer.name === name)).map(({ name }) => name) };
}

function scan(this: AcrobatDocument, range: DocumentRequest['range']): DocumentResult['scan'] {
    return {
        kind: 'text',
        pages: _pages.call(this, range).map((page) => {
            const words: DocumentResult['scan']['pages'][number]['words'][number][] = [];
            for (let wordIndex = 0, count = this.getPageNumWords(page); wordIndex < count; wordIndex += 1) {
                const quads = this.getPageNthWordQuads(page, wordIndex);
                words.push({ wordIndex, word: this.getPageNthWord(page, wordIndex, false), quads: quads === null ? [] : quads });
            }
            return { page, label: this.getPageLabel(page), words };
        }),
    };
}

function annotations(this: AcrobatDocument, request: DocumentRequest['comments' | 'stamp' | 'redact' | 'links'], save: string | null): DocumentResult['comments' | 'stamp' | 'redact' | 'links'] {
    if (request.op === 'comments') {
        this.syncAnnotScan();
        const found = this.getAnnots();
        const comments = found === null ? [] : found;
        if (request.request.kind === 'list') {
            return {
                kind: 'comments',
                rows: comments.map(({ name, type, page, author, contents, modDate, rect, inReplyTo }) => ({
                    name,
                    type,
                    page,
                    author,
                    contents,
                    modDate: modDate.toISOString(),
                    rect,
                    ...(inReplyTo === undefined ? {} : { inReplyTo }),
                })),
            };
        }
        if (request.request.kind === 'export') {
            this.exportAsXFDF({ cPath: request.request.path, bAnnotations: true, bAllFields: request.request.fields, ...(request.request.fields ? {} : { aFields: [] }) });
            return { kind: 'commentsExported', path: request.request.path, count: comments.length };
        }
        this.importAnXFDF(request.request.path);
        this.syncAnnotScan();
        const imported = this.getAnnots();
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'commentsImported', before: comments.length, after: imported === null ? 0 : imported.length };
    }
    if (request.op === 'stamp') {
        const annots = _pages.call(this, request.pages).map((page) =>
            this.addAnnot({
                type: 'Stamp',
                page,
                rect: request.rect,
                // biome-ignore lint/style/useNamingConvention: Adobe addAnnot requires AP for the native stamp appearance name.
                AP: request.appearance,
                ...(request.opacity === undefined ? {} : { opacity: request.opacity }),
                ...(request.rotation === undefined ? {} : { rotate: request.rotation }),
            }),
        );
        const result = annots.map(({ page, name, AP }) => ({ page, name, appearance: AP }));
        if (request.flatten) {
            for (const { page } of result) {
                this.flattenPages({ nStart: page, nEnd: page, nNonPrint: 1 });
            }
        }
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'stamped', annots: result };
    }
    if (request.matches.length > 0) {
        _pages.call(this, { kind: 'pages', pages: request.matches.map(({ page }) => page) });
    }
    if (request.op === 'redact') {
        const marks = request.matches.map(({ page, quads }) =>
            this.addAnnot({
                type: 'Redact',
                page,
                quads,
                ...(request.overlay === undefined ? {} : { overlayText: request.overlay.text, repeat: request.overlay.repeat, alignment: request.overlay.alignment, fillColor: request.overlay.fill }),
            }),
        );
        const applied = marks.length > 0 && this.applyRedactions({ aMarks: marks, bKeepMarks: request.keepMarks, bShowConfirmation: false });
        if (save !== null) {
            this.saveAs({ cPath: save });
        }
        return { kind: 'redacted', marks: request.matches, applied };
    }
    const links = request.matches.map((match) => ({
        ...match,
        script: match.target.kind === 'url' ? `app.launchURL(${literal(match.target.url)}, true)` : `this.pageNum = ${_page.call(this, match.target)}`,
    }));
    const added = links.map(({ page, text, target, quads, border, script }) => {
        const transform = new Matrix2D().fromRotated(this, page).invert();
        const rectangles = quads
            .map((quad) => transform.transform(quad))
            .map((quad) => [
                Math.min(quad[0], quad[2], quad[4], quad[6]),
                Math.max(quad[1], quad[3], quad[5], quad[7]),
                Math.max(quad[0], quad[2], quad[4], quad[6]),
                Math.min(quad[1], quad[3], quad[5], quad[7]),
            ]);
        for (const rectangle of rectangles) {
            const link = this.addLink(page, rectangle);
            link.borderWidth = border ? 1 : 0;
            link.setAction(script);
        }
        return { page, text, target, rectangles };
    });
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'links', added };
}

function delivery(this: AcrobatDocument, request: DocumentRequest['export' | 'encrypt'], save: string | null): DocumentResult['export' | 'encrypt'] {
    if (request.op === 'export') {
        if (app.fromPDFConverters.indexOf(request.converter) === -1) {
            return { kind: 'converterAbsent', available: app.fromPDFConverters };
        }
        this.saveAs({ cPath: request.output, cConvID: request.converter, bCopy: true });
        return { kind: 'exported', path: request.output, converter: request.converter };
    }
    const found = security.getSecurityPolicies();
    const policies = found === null ? [] : found;
    const [policy] = policies.filter((candidate) => candidate.name === request.policy);
    if (policy === undefined) {
        return { kind: 'policyAbsent', available: policies.map(({ name }) => name) };
    }
    const result = this.encryptUsingPolicy({ oPolicy: policy, bUI: false });
    if (result.errorCode !== 0) {
        return { kind: 'encryptionRefused', code: result.errorCode, message: result.errorCode === 1 ? result.errorText : 'Encryption cancelled' };
    }
    if (save !== null) {
        this.saveAs({ cPath: save });
    }
    return { kind: 'encrypted', policyId: result.policyApplied.policyId, securityHandler: this.securityHandler };
}

function _reject(rejection: (typeof HostRejection)['Encoded']): never {
    const error: Error & { rejection?: (typeof HostRejection)['Encoded'] } = new Error(rejection._tag);
    error.rejection = rejection;
    throw error;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export type { AcrobatField, DocumentRequest, DocumentResult, FieldWrite };
export {
    action,
    annotations,
    autotag,
    combine,
    delivery,
    document,
    execMenuItem,
    FIELD_TYPES,
    getFields,
    listPreflight,
    literal,
    menu,
    openDocument,
    PAGE_ROTATIONS,
    pages,
    printProduction,
    resources,
    run,
    runAction,
    runPreflight,
    scan,
    setFields,
    state,
    tabOrder,
    tagged,
};
