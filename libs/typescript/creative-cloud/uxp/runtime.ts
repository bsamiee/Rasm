import { storage } from 'adobe:uxp';
import { TextDecoder, TextEncoder } from '@exodus/bytes/encoding-lite.js';
import { Clock, clockWith } from 'effect/Clock';
import { Service } from 'effect/Context';
import { type Duration, toNanosUnsafe } from 'effect/Duration';
import { map, promise, succeed, sync } from 'effect/Effect';
import { effect } from 'effect/Layer';

// --- [SERVICES] ------------------------------------------------------------------------

class Temporary extends Service<Temporary, string>()('@rasm/uxp/runtime/Temporary') {
    static readonly layer = effect(this)(
        map(
            promise(() => storage.localFileSystem.getTemporaryFolder()),
            ({ nativePath }) => nativePath,
        ),
    );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

Object.assign(globalThis, { TextDecoder, TextEncoder });

const HostClock = effect(Clock)(
    clockWith((base) => {
        const monotonicTimeNanosUnsafe = (): bigint => toNanosUnsafe(performance.now());
        const origin = toNanosUnsafe(base.currentTimeMillisUnsafe()) - monotonicTimeNanosUnsafe();
        const currentTimeNanosUnsafe = (): bigint => origin + monotonicTimeNanosUnsafe();
        return succeed({
            currentTimeMillisUnsafe: () => base.currentTimeMillisUnsafe(),
            currentTimeMillis: base.currentTimeMillis,
            currentTimeNanosUnsafe,
            currentTimeNanos: sync(currentTimeNanosUnsafe),
            monotonicTimeNanosUnsafe,
            monotonicTimeNanos: sync(monotonicTimeNanosUnsafe),
            sleep: (duration: Duration) => base.sleep(duration),
        });
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostClock, Temporary };
