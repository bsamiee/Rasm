// --- [TYPES] ---------------------------------------------------------------------------

interface Enumeration {
    readonly name: string;
    readonly constants: readonly string[];
}

// --- [TABLE] ---------------------------------------------------------------------------

const reflected = (dom: object): readonly Enumeration[] =>
    Object.getOwnPropertyNames(dom).flatMap((name) => {
        const value: unknown = Reflect.get(dom, name);
        if (typeof value !== 'object' || value === null || value.constructor.name !== 'Enumeration') {
            return [];
        }
        const spellings = Object.getOwnPropertyNames(Object.getPrototypeOf(value)).filter((constant) => constant === constant.toUpperCase());
        return [{ name, constants: spellings.map((spelling) => String(Reflect.get(value, spelling))) }];
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Enumeration };
export { reflected };
