import { Array, Clock, Config, Duration, Effect, Option, Schedule, Schema } from 'effect';
import type { Routes } from '#/claude/routes';
import { Host } from '#/host';

// --- [MODELS] --------------------------------------------------------------------------

const Owner = Schema.fromJsonString(Schema.Struct({ front: Schema.String, typed: Schema.Finite, fullScreen: Schema.Boolean }));
const Assertions = Schema.fromJsonString(Schema.Struct({ data: Schema.Array(Schema.Struct({ storeAssertionRecords: Schema.optionalKey(Schema.Array(Schema.Unknown)) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const holds = Effect.gen(function* () {
    const host = yield* Host;
    const [own, { front, typed, fullScreen }, focused] = yield* Effect.all([
        host.environment('__CFBundleIdentifier'),
        Effect.flatMap(
            host.output([
                'osascript',
                '-l',
                'JavaScript',
                '-e',
                "ObjC.import('AppKit'); JSON.stringify({ front: $.NSWorkspace.sharedWorkspace.frontmostApplication.bundleIdentifier.js, typed: $.CGEventSourceSecondsSinceLastEventType($.kCGEventSourceStateHIDSystemState, $.kCGEventKeyDown), fullScreen: ($.NSApplication.sharedApplication.currentSystemPresentationOptions & $.NSApplicationPresentationFullScreen) !== 0 })",
            ]),
            Schema.decodeEffect(Owner),
        ),
        Effect.fromOption(host.home).pipe(
            Effect.flatMap((home) => host.fs.readFileString(host.path.join(home, 'Library/DoNotDisturb/DB/Assertions.json'))),
            Effect.flatMap(Schema.decodeEffect(Assertions)),
            Effect.map(({ data }) => data.some(({ storeAssertionRecords = [] }) => storeAssertionRecords.length > 0)),
            Effect.orElseSucceed(() => false),
        ),
    ]);
    return Option.contains(own, front) ? [] : [...(typed < Duration.toSeconds('20 seconds') ? ['typing'] : []), ...(fullScreen ? ['a full-screen app'] : []), ...(focused ? ['a Focus'] : [])];
});

const paused = Effect.fnUntraced(function* ({ since, first }: Routes['paused'][0]) {
    const [enabled, now] = yield* Effect.all([Effect.orElseSucceed(Config.Boolean('FUNCTION_HOOKS_QUESTION_GATE'), () => false), Clock.currentTimeMillis]);
    const standing = enabled && now - since < Duration.toMillis('15 minutes') ? yield* Effect.repeat(holds, { schedule: Schedule.spaced('5 seconds'), times: 3, until: Array.isReadonlyArrayEmpty }) : [];
    const waited = Duration.format(Duration.millis((yield* Clock.currentTimeMillis) - since));
    return { holds: standing, notes: standing.length > 0 || first.length === 0 ? [] : [`Question waited ${waited} on ${first.join(', ')}`] };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { paused };
