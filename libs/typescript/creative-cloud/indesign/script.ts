import type * as indesign from 'adobe:indesign';

// --- [OPERATIONS] ----------------------------------------------------------------------

const interactionless = <A>({ app: { scriptPreferences }, UserInteractionLevels }: Pick<typeof indesign, 'app' | 'UserInteractionLevels'>, body: () => A): A => {
    const level = scriptPreferences.userInteractionLevel;
    Object.assign(scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT });
    try {
        return body();
    } finally {
        Object.assign(scriptPreferences, { userInteractionLevel: level });
    }
};
const ScriptHost = { module: 'indesign', around: String(interactionless) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ScriptHost };
