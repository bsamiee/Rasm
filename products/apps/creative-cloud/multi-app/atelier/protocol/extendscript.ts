import { declaration, illegalArgument, located, reply, within } from '@rasm/extendscript/host';
import { type Containment, isRecord, parse, read, reference, walk } from '@rasm/object-model/host';
import type { Schema } from 'effect';
import type { RpcGroup } from 'effect/rpc';
import type { Rasterize, Requests } from '#/requests';

// --- [TYPES] ---------------------------------------------------------------------------

type Envelope<R = RpcGroup.Rpcs<typeof Requests>> = R extends { readonly _tag: infer Tag; readonly payloadSchema: infer Payload extends Schema.Top } ? { readonly tag: Tag; readonly label: string; readonly payload: Payload['Encoded'] } : never;

interface Hooks {
    readonly global: unknown;
    readonly containment: Containment;
    readonly base?: number;
    readonly session: () => () => void;
    readonly undo?: (label: string) => () => void;
    readonly open: (path: string) => object;
    readonly rasterize: (found: object, payload: typeof Rasterize.Encoded, path: string) => void;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const callable = (value: unknown): value is () => unknown => typeof value === 'function';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = ({ global, containment, base = 0, session, undo, open, rasterize }: Hooks) => {
    const encoder = { reference: (value: object) => reference(containment, value) };
    const resolve = (target: string): object => {
        const path = parse(target);
        const found = path === undefined ? undefined : walk(containment, path);
        if (isRecord(found) && read(found, '_tag') === 'Unmatched') {
            throw found;
        }
        if (!isRecord(found)) {
            throw illegalArgument(`${target} is no object specifier`);
        }
        return found;
    };
    return (request: Envelope): string => {
        const answer = (): unknown => {
            switch (request.tag) {
                case 'execute_script': {
                    // oxlint-disable-next-line no-new-func, typescript/no-implied-eval, typescript/no-unsafe-call -- execute_script compiles caller source to run in the host
                    const body: unknown = new Function('alert', 'confirm', 'prompt', 'resolve', 'code', 'return eval(code)')(
                        () => undefined,
                        () => false,
                        () => null,
                        resolve,
                        request.payload.code,
                    );
                    if (!callable(body)) {
                        throw illegalArgument(`${request.tag} code evaluates to ${typeof body}, not a function`);
                    }
                    return { result: body() };
                }
                case 'open': {
                    return open(located(request.payload.file));
                }
                case 'rasterize': {
                    rasterize(resolve(request.payload.target), request.payload, located(request.payload.file));
                    return null;
                }
                case 'object_model': {
                    return declaration(global, containment.root, base, request.payload.name);
                }
                default: {
                    return request satisfies never;
                }
            }
        };
        const grouped = undo === undefined || request.tag === 'open' || request.tag === 'object_model' ? answer : () => within(() => undo(request.label), answer);
        return reply(
            () => within(session, grouped),
            encoder,
            (thrown) => request.tag === 'execute_script' && read(thrown, 'source') === request.payload.code,
        );
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
