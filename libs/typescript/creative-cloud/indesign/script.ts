import type indesign from 'adobe:indesign';

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
const ScriptHost = { module: 'indesign', around: String(stepped) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ScriptHost };
