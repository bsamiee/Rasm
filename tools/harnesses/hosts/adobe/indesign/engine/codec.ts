import { ownKeys } from '@rasm/extendscript/extendscript';
import { codec, type Host, messageOf } from '#/hosts/adobe/extendscript';

// --- [OPERATIONS] ----------------------------------------------------------------------

let owners: Host = {};
const enumerated = (enumerations: Host): void => {
    owners = {};
    for (const enumeration of ownKeys(enumerations)) {
        for (const member of enumerations[enumeration]) {
            owners[member] = owners[member] || [];
            owners[member].push(enumeration);
        }
    }
};
const candidates = (path: string): Host[] => {
    const dot = path.indexOf('.');
    const name = path.slice(dot + 1);
    const found: Host[] = [];
    const listed: Host = dot >= 0 ? [path.slice(0, dot)] : Object.prototype.hasOwnProperty.call(owners, name) ? owners[name] : [];
    for (const owner of listed) {
        const held = $.global[owner];
        if (held !== undefined && held !== null && held[name] !== undefined) {
            found.push([owner, held[name]]);
        }
    }
    return found;
};
const qualified = (value: Host): string => {
    const name = String(value);
    const holding: string[] = [];
    for (const row of candidates(name)) {
        if (Number(row[1]) === Number(value)) {
            holding.push(row[0]);
        }
    }
    return holding.length === 1 ? `enum:${holding[0]}.${name}` : `enum:${name}`;
};
const resolved = (path: string): Host => {
    const found = candidates(path);
    if (found.length === 0) {
        throw new Error(`enum:${path} names no enumerator`);
    }
    for (const row of found) {
        if (Number(row[1]) !== Number(found[0][1])) {
            throw new Error(`enum:${path} names enumerators of ${(owners[path] || []).join(', ')} with different codes`);
        }
    }
    return found[0][1];
};
const coded = codec({
    named: (value: Host): Host => {
        if (value.constructor && value.constructor.name === 'Enumerator') {
            return qualified(value);
        }
        if (typeof value.toSpecifier !== 'function') {
            return null;
        }
        try {
            return `object:${value.toSpecifier()}`;
        } catch (error) {
            return `unreadable:${messageOf(error)}`;
        }
    },
    enumerator: resolved,
    located: (path: Host): Host => $.global.resolve(path),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { coded, enumerated };
