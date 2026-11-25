import type indesign from 'adobe:indesign';
import type { InDesignDOMObject } from 'adobe:indesign';

// --- [OPERATIONS] ----------------------------------------------------------------------

const stepped = <A>({ app, ScriptLanguage, UndoModes, UserInteractionLevels }: Pick<typeof indesign, 'app' | 'ScriptLanguage' | 'UndoModes' | 'UserInteractionLevels'>, body: () => A, label: string): A => {
    const { scriptPreferences } = app;
    const level = scriptPreferences.userInteractionLevel;
    const settled: ({ readonly ok: true; readonly value: A } | { readonly ok: false; readonly error: unknown })[] = [];
    Object.assign(scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT });
    try {
        app.doScript(
            () => {
                try {
                    settled.push({ ok: true, value: body() });
                } catch (error) {
                    settled.push({ ok: false, error });
                    throw error;
                }
            },
            ScriptLanguage.UXPSCRIPT,
            [],
            UndoModes.ENTIRE_SCRIPT,
            label,
        );
    } catch (error) {
        settled.push({ ok: false, error });
    } finally {
        Object.assign(scriptPreferences, { userInteractionLevel: level });
    }
    const [outcome = { ok: false, error: new Error('doScript returned before its function ran') } as const] = settled;
    if (!outcome.ok) {
        throw outcome.error;
    }
    return outcome.value;
};
const referenced = (value: object) => {
    const held: Partial<Pick<InDesignDOMObject<unknown>, 'constructorName' | 'isValid' | 'toSpecifier'>> & { readonly name?: unknown } = value;
    const specifier = held.toSpecifier?.();
    return specifier === undefined || held.constructorName === undefined || held.isValid === undefined ? undefined : { type: held.constructorName, specifier, valid: held.isValid, ...(held.isValid && typeof held.name === 'string' ? { name: held.name } : {}) };
};
const ScriptHost = { module: 'indesign', around: String(stepped), reference: String(referenced) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ScriptHost };
