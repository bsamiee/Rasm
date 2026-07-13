# [timeprovider-testing] — the deterministic clock under time-dependent gauges

`Microsoft.Extensions.TimeProvider.Testing` ships `FakeTimeProvider`, the controllable `TimeProvider` that makes every time-dependent proof deterministic: wall-clock reads, timestamps, and timers advance only when the spec says so. It injects wherever a SUT accepts `TimeProvider`, so retry schedules, cache expiry, debounce windows, and the AppUi proof engine's timed lanes prove without real sleeps.

## [01]-[PACKAGE_SURFACE]

- package: `Microsoft.Extensions.TimeProvider.Testing` `10.7.0`
- license: `MIT`
- namespace: `Microsoft.Extensions.Time.Testing`
- asset: `lib/net10.0/Microsoft.Extensions.TimeProvider.Testing.dll`
- rail: evidence — deterministic time for kit gauges and specs; injected per `IsTestProject`/`IsTestKitProject` with `PrivateAssets="all"`

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]           | [KIND] | [CAPABILITY]                                                                |
| :-----: | :----------------- | :----- | :-------------------------------------------------------------------------- |
|  [01]   | `FakeTimeProvider` | clock  | controllable `TimeProvider`: manual advance, auto-advance, timezone, timers |

## [03]-[ENTRYPOINTS]

The fence carries the full `CreateTimer` signature and the `FakeTimeProvider` shape.

| [INDEX] | [SURFACE]                                               | [KIND]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------ | :------ | :-------------------------------------------------------------- |
|  [01]   | `new FakeTimeProvider()`                                | ctor    | fixed epoch start                                               |
|  [02]   | `new FakeTimeProvider(DateTimeOffset startDateTime)`    | ctor    | explicit start instant                                          |
|  [03]   | `Advance(TimeSpan delta)`                               | control | move time forward; due timers fire on the advancing thread      |
|  [04]   | `SetUtcNow(DateTimeOffset value)`                       | control | jump the clock to an instant; crossed timers fire               |
|  [05]   | `AdjustTime(DateTimeOffset value)`                      | control | shift the clock without firing timers                           |
|  [06]   | `AutoAdvanceAmount { get; set; }`                       | policy  | every `GetUtcNow()` read advances by the amount; default 0      |
|  [07]   | `GetUtcNow()` / `GetTimestamp()` / `TimestampFrequency` | read    | deterministic reads; frequency fixed at `10000000`              |
|  [08]   | `SetLocalTimeZone(TimeZoneInfo localTimeZone)`          | policy  | set the test timezone                                           |
|  [09]   | `LocalTimeZone`                                         | read    | the current test timezone                                       |
|  [10]   | `CreateTimer(...)`                                      | timer   | fake `ITimer` driven purely by advances; signature in the fence |

```csharp signature
public class FakeTimeProvider : TimeProvider {
    public DateTimeOffset Start { get; }
    public TimeSpan AutoAdvanceAmount { get; set; }
    public void Advance(TimeSpan delta);
    public void SetUtcNow(DateTimeOffset value);
    public void AdjustTime(DateTimeOffset value);
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period);
}
```

## [04]-[IMPLEMENTATION_LAW]

[DETERMINISM]: time moves only through `Advance`/`SetUtcNow`/`AutoAdvanceAmount`; timer callbacks run synchronously on the advancing thread when their due time is crossed, so a timed proof is a pure function of the advance sequence — no sleeps, no race windows.

[STACKING]:
- `Rasm.TestKit`: the kit's time-dependent gauges take `TimeProvider` and receive `FakeTimeProvider` in specs; production code keeps `TimeProvider.System`.
- `xunit.v3` (`xunit-v3.md`): plain construction inside `[Fact]` bodies; no fixture requirement.
- `libs/csharp/Rasm.AppUi/.api/api-headless.md`: the AppUi proof engine drives its timed render and evidence lanes off this clock.

[LOCAL_ADMISSION]:
- A SUT reads time through an injected `TimeProvider`; a spec that sleeps, polls `DateTime.UtcNow`, or hand-rolls a fake clock is the named defect.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.TimeProvider.Testing`
- Owns: deterministic time and timer control inside C# specs and kit gauges.
- Accept: injected `FakeTimeProvider` with explicit advances; `AutoAdvanceAmount` for progress-dependent loops.
- Reject: `Thread.Sleep`/`Task.Delay` in proofs, ambient `DateTime.Now` reads, or a second fake-clock implementation.
