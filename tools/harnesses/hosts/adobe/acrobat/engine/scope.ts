import { answerOf, pathed } from '#/hosts/adobe/acrobat/engine/answer';
import { root } from '#/hosts/adobe/acrobat/engine/script';
import { posix, recorded, refuse, type Session } from '#/hosts/adobe/acrobat/engine/session';
import { type Callable, enumerated, has, isCallable, isDoc, isMembers, keyed, kindOf, type Members, starts } from '#/hosts/adobe/acrobat/engine/value';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ACTIVATING: Record<string, { readonly names: readonly string[]; readonly prefixes: readonly string[] }> = {
    App: { names: ['launchURL', 'beep', 'goBack', 'goForward', 'addMenuItem', 'addSubMenu', 'addToolButton', 'hideMenuItem', 'hideToolbarButton', 'removeToolButton'], prefixes: ['mail'] },
    Doc: { names: ['bringToFront', 'getURL', 'submitForm'], prefixes: ['print', 'mail'] },
    Console: { names: ['show', 'hide', 'clear'], prefixes: [] },
    FullScreen: { names: ['isFullScreen'], prefixes: [] },
};
const RESTORED = ['App', 'FullScreen'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const activating = (kind: string, member: string) => {
    const listed = ACTIVATING[kind];
    return listed !== undefined && (has(listed.names, member) || listed.prefixes.some((prefix) => starts(member, prefix)));
};
const strayed = (session: Session, value: unknown): readonly string[] => {
    if (typeof value === 'string') {
        return starts(value, '/') && !starts(posix(session, value), `${session.request.case.precondition.sandbox}/`) ? [value] : [];
    }
    if (isMembers(value) && kindOf(value) === 'Object') {
        return Object.keys(value).reduce<readonly string[]>((found, key) => found.concat(strayed(session, value[key])), []);
    }
    return [];
};
const guarded = (session: Session, target: Members, kind: string, member: string, held: Callable) => {
    const key = `${kind}.${member}`;
    const signature = session.request.signatures[key];
    const answer = answerOf(kind, member);
    const reaching = pathed(kind, member, signature);
    const call = (args: readonly unknown[]) => held.apply(target, [...args]);
    return (...args: readonly unknown[]) => {
        if (args[0] === root.acrohelp) {
            return call(args);
        }
        if (activating(kind, member)) {
            return recorded(session, key);
        }
        const stray = reaching ? strayed(session, { ...args }) : [];
        if (stray.length > 0) {
            refuse(session, `${key} reaches ${stray.join(', ')} outside the case folder`);
        }
        return adopted(session, answer === undefined ? call(args) : answer({ session, key, signature: signature ?? [], target, args, call }), true);
    };
};
const scoped = (session: Session, target: Members, kind: string): Members =>
    Object.defineProperties(
        {},
        keyed(enumerated(target), (key) => ({
            enumerable: true,
            get: () => {
                const held = target[key];
                return isCallable(held) ? guarded(session, target, kind, key, held) : adopted(session, held, false);
            },
            set: (value: unknown) => {
                if (activating(kind, key)) {
                    recorded(session, `${kind}.${key}`);
                    return;
                }
                if (has(RESTORED, kind) && !session.writes.some((held) => held.target === target && held.key === key)) {
                    session.writes.push({ target, key, member: `${kind}.${key}`, before: target[key] });
                }
                // biome-ignore lint/style/noParameterAssign: the scope writes through to the native Acrobat object it wraps
                target[key] = value;
            },
        })),
    );
const adopted = (session: Session, value: unknown, made: boolean): unknown => {
    if (Array.isArray(value)) {
        return value.map((item) => adopted(session, item, made));
    }
    if (isDoc(value)) {
        if (made && !has(session.docs, value)) {
            session.docs.push(value);
        }
        return scoped(session, value, 'Doc');
    }
    return isMembers(value) && kindOf(value) === 'FullScreen' ? scoped(session, value, 'FullScreen') : value;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { adopted, scoped };
