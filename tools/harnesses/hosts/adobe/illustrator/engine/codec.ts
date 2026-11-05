import { attempt, readProperty } from '@rasm/extendscript';
import { codec, type Host, holds, RealFile, RealFolder, realApp, realGlobal } from '#/hosts/adobe/extendscript';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENUMERATOR = /^([A-Za-z_$][\w$]*)\.([A-Za-z_$][\w$]*)$/;
const REFERENCE = /^([A-Za-z]+):(.+)$/;
const IDENTITY: readonly string[] = ['__proto__', 'parent', 'typename'];
const homes: Host = {};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [REFLECTION]
const kindOf = (value: Host): Host => {
    const typename = readProperty(value, 'typename');
    return typeof typename === 'string' ? typename : null;
};
const reflects = (value: Host, name: string): boolean => Boolean(attempt(() => value.reflect.find.call(value.reflect, name)));
const collected = (value: Host): boolean => typeof readProperty(value, 'getByName') === 'function' && typeof readProperty(value, 'length') === 'number';
const classed = (info: Host): boolean => {
    const first = info.dataType.charAt(0);
    return first !== first.toLowerCase() && info.dataType !== 'Object';
};
const properties = (owner: Host, visit: (property: Host, value: Host) => void, wanted: (property: Host) => boolean = () => true, unread: (property: Host, error: Host) => void = () => {}): void => {
    for (const property of owner.reflect.properties) {
        if (holds(IDENTITY, property.name) || !wanted(property)) {
            continue;
        }
        let value: Host;
        try {
            value = owner[property.name];
        } catch (error) {
            unread(property, error);
            continue;
        }
        visit(property, value);
    }
};

// --- [REFERENCES]
const enumerator = (value: Host): Host => {
    const parts: Host = ENUMERATOR.exec(String(value));
    return parts && attempt(() => realGlobal[parts[1]][parts[2]]) === value ? `enum:${parts[1]}.${parts[2]}` : null;
};
const home = (value: Host, above: string, kind: string): Host => {
    const key = `${above}>${kind}`;
    if (homes[key] !== undefined) {
        return homes[key];
    }
    const { parent } = value;
    for (const property of parent.reflect.properties) {
        const held = classed(property) ? attempt(() => parent[property.name]) : undefined;
        if (collected(held) && attempt(() => held.getByName(value.name).typename === kind) === true) {
            homes[key] = property.name;
            return property.name;
        }
    }
    return null;
};
const anchored: Host = { Application: () => 'app', Document: (value: Host) => `Document:${value.name}` };
const specifier = (value: Host): Host => {
    const kind = kindOf(value);
    if (kind === null) {
        return null;
    }
    if (anchored.hasOwnProperty(kind)) {
        return anchored[kind](value);
    }
    const uuid = readProperty(value, 'uuid');
    if (uuid !== undefined) {
        return `${kind}:${uuid}`;
    }
    if (!(reflects(value, 'parent') && reflects(value, 'name'))) {
        return null;
    }
    const above = specifier(value.parent);
    const collection = above === null ? null : home(value, above, kind);
    return collection === null ? null : `${kind}:${above}/${collection}/${encodeURIComponent(value.name)}`;
};
const located = (reference: string): Host => {
    const parts: Host = REFERENCE.exec(reference);
    if (reference === 'app') {
        return realApp;
    }
    if (!parts) {
        throw new Error(`object:${reference} reads neither Document:<name>, <typename>:<uuid>, nor <typename>:<container>/<collection>/<name>`);
    }
    if (parts[1] === 'Document') {
        return realApp.documents.getByName(parts[2]);
    }
    const at = parts[2].lastIndexOf('/');
    if (at >= 0) {
        const head = parts[2].slice(0, at);
        const from = head.lastIndexOf('/');
        return located(head.slice(0, from))[head.slice(from + 1)].getByName(decodeURIComponent(parts[2].slice(at + 1)));
    }
    for (const open of realApp.documents) {
        const item = attempt(() => open.getPageItemFromUuid(parts[2]));
        if (item !== undefined) {
            return item;
        }
    }
    throw new Error(`object:${reference} names no item of an open document`);
};

// --- [VALUES]
const expanded = (value: Host, path: Host): Host => {
    const row: Host = { typename: kindOf(value) };
    const below = path.concat([row.typename]);
    properties(value, (property, held) => {
        if (!collected(held)) {
            row[property.name] = coded.kept(held, below);
        }
    });
    return row;
};
const hosted = (value: Host, path: Host): Host => {
    if (value instanceof RealFile || value instanceof RealFolder) {
        return value.fsName;
    }
    if (collected(value)) {
        const listed: Host[] = [];
        for (const entry of value) {
            listed.push(coded.kept(entry, path));
        }
        return listed;
    }
    const held = specifier(value);
    if (held !== null) {
        return { $: `object:${held}` };
    }
    const kind = kindOf(value);
    return kind !== null && !holds(path, kind) ? expanded(value, path) : null;
};
const coded = codec({ named: enumerator, plain: hosted, located, constructed: true });

// --- [EXPORTS] -------------------------------------------------------------------------

export { classed, coded, collected, expanded, kindOf, properties, reflects };
