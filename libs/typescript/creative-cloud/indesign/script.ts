import type { Installation } from '@rasm/automation/applications';
import { Automation } from '@rasm/automation/automation';
import { Context, Effect } from 'effect';
import { RpcClient } from 'effect/rpc';
import { Contract } from '#/contract';

// --- [SERVICES] ------------------------------------------------------------------------

class Script extends Context.Service<Script>()('@rasm/indesign/script', {
    make: Effect.fnUntraced(function* (script: URL) {
        const automation = yield* Automation;
        return {
            hosts: Effect.map(automation.applications({ toOpen: script }), (urls) => (row: Installation) => urls.some((url) => url.href === row.url.href)),
            client: (pid: number) =>
                RpcClient.make(Contract, { flatten: true }).pipe(Effect.provide(Automation.protocol(pid, (message) => ({ command: 'do script', arguments: { direct: { _tag: 'file', url: script.href }, language: 'uxpscript', 'with arguments': [message] } }))), Effect.provideService(Automation, automation)),
        };
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Script };
