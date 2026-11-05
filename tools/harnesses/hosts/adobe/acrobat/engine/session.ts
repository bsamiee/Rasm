import type { Contract, EngineRequest, Model } from '#/hosts/adobe/acrobat/contract';
import { FAULTS, raised, root, thrown } from '#/hosts/adobe/acrobat/engine/script';
import { type Doc, encoded, enumerated, invoked, keyed, type Members, starts } from '#/hosts/adobe/acrobat/engine/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Request = typeof EngineRequest.Type;
type Item = typeof Contract.Case.Type;
type Signature = (typeof Model.Type)['signatures'][string];
type Listed<T> = { -readonly [K in keyof T]-?: NonNullable<T[K]> extends readonly (infer A)[] ? A[] : never };
interface Session {
    readonly request: Extract<Request, { readonly _tag: 'Case' }>;
    readonly volume: string;
    readonly globals: Members;
    readonly records: Listed<Pick<typeof Contract.Outcome.Encoded, 'alerts' | 'confirms' | 'prompts' | 'files' | 'windows' | 'console' | 'activations' | 'events' | 'opened' | 'leaked'>>;
    readonly answers: Listed<Pick<Item, 'confirms' | 'prompts' | 'files' | 'windows'>>;
    readonly writes: { readonly target: Members; readonly key: string; readonly member: string; readonly before: unknown }[];
    readonly docs: Doc[];
    readonly saved: string[];
    readonly timers: [string, unknown][];
    readonly refused: string[];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const started = (request: Session['request']): Session => {
    const documents = String(invoked(root.app, 'getPath', ['user', 'documents']));
    const home = documents.indexOf(request.home);
    const { case: item } = request;
    return {
        request,
        volume: home > 0 ? documents.slice(0, home) : '',
        globals: keyed(enumerated(root.global), (key) => root.global[key]),
        records: { alerts: [], confirms: [], prompts: [], files: [], windows: [], console: [], activations: [], events: [], opened: [], leaked: [] },
        answers: { confirms: [...(item.confirms ?? [])], prompts: [...(item.prompts ?? [])], files: [...(item.files ?? [])], windows: [...(item.windows ?? [])] },
        writes: [],
        docs: [],
        saved: [],
        timers: [],
        refused: [],
    };
};
const next = <T>(list: T[]) => list.splice(0, 1)[0];
const posix = ({ volume }: Session, path: string) => (volume !== '' && starts(path, `${volume}/`) ? path.slice(volume.length) : path);
const recorded = (session: Session, member: string): undefined => {
    session.records.activations.push(member);
};
const refuse: (session: Session, reason: string) => never = (session, reason) => {
    session.refused.push(reason);
    throw raised(FAULTS.refused, reason, undefined);
};
const restored = (session: Session) => {
    const { records, globals } = session;
    const shared = root.global;
    const leaked = (what: string, undo: () => void) => {
        try {
            undo();
        } catch (error) {
            records.leaked.push(`${what}: ${thrown(error).message}`);
        }
    };
    session.timers.forEach(([clear, timer]) => leaked(`App.${clear}`, () => invoked(root.app, clear, [timer])));
    const writes = session.writes.map(({ target, key, member, before }) => {
        const after = encoded(target[key]);
        leaked(member, () => {
            target[key] = before;
        });
        return { member, before: encoded(before), after };
    });
    session.docs
        .slice()
        .reverse()
        .forEach((doc) => leaked('close', () => doc.closeDoc(true)));
    enumerated(shared)
        .filter((name) => !(name in globals) || shared[name] !== globals[name])
        .forEach((key) => {
            records.leaked.push(`global.${key}`);
            delete shared[key];
        });
    Object.keys(globals)
        .filter((name) => !(name in shared))
        .forEach((key) => {
            shared[key] = globals[key];
        });
    return writes;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Item, next, posix, type Request, recorded, refuse, restored, type Session, type Signature, started };
