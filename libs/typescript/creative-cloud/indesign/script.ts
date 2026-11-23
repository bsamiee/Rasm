import type indesign from 'adobe:indesign';
import type { InDesignDOMObject } from 'adobe:indesign';

// --- [OPERATIONS] ----------------------------------------------------------------------

const stepped = <A>({ app, ScriptLanguage, UndoModes, UserInteractionLevels }: Pick<typeof indesign, 'app' | 'ScriptLanguage' | 'UndoModes' | 'UserInteractionLevels'>, body: () => A, label: string): A => {
    const { scriptPreferences } = app;
    const level = scriptPreferences.userInteractionLevel;
    const returned: { readonly value: A }[] = [];
    const thrown: unknown[] = [];
    Object.assign(scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT });
    try {
        app.doScript(
            () => {
                try {
                    returned.push({ value: body() });
                } catch (error) {
                    thrown.push(error);
                    throw error;
                }
            },
            ScriptLanguage.UXPSCRIPT,
            [],
            UndoModes.ENTIRE_SCRIPT,
            label,
        );
    } catch (error) {
        throw thrown.length === 0 ? error : thrown[0];
    } finally {
        Object.assign(scriptPreferences, { userInteractionLevel: level });
    }
    const [settled] = returned;
    if (settled === undefined) {
        throw new Error('doScript returned before its function ran');
    }
    return settled.value;
};
const referenced = (value: object) => {
    const held: Partial<Pick<InDesignDOMObject<unknown>, 'constructorName' | 'isValid' | 'toSpecifier'>> & { readonly name?: unknown } = value;
    const specifier = held.toSpecifier?.();
    return specifier === undefined || held.constructorName === undefined || held.isValid === undefined ? undefined : { type: held.constructorName, specifier, valid: held.isValid, ...(held.isValid && typeof held.name === 'string' ? { name: held.name } : {}) };
};
const ScriptHost = { module: 'indesign', around: String(stepped), reference: String(referenced) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ScriptHost };
