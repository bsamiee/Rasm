using Rasm.UI.Rendering;
using SkiaSharp;

namespace Rasm.UI.Motion;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Expiry(Duration Lifetime, IO<Unit> Elapsed);

internal sealed record Countdown(Duration Left, Option<Duration> Since) {
    internal static Countdown Expired { get; } = new(Duration.Zero, None);

    internal Option<Duration> Deadline => Since.Map(since => since + Left);

    internal bool Elapsed(Duration now) => Deadline.Exists(deadline => deadline <= now);

    internal Countdown Held(bool held, Duration now) =>
        Since.Match(
            Some: since => held && Left - (now - since) > Duration.Zero ? new Countdown(Left - (now - since), None) : this,
            None: () => !held && Left > Duration.Zero ? this with { Since = Some(now) } : this);

    internal static Countdown Started(Duration left, Duration now) => new(left, Some(now));
}

internal sealed record Timers {
    internal static Timers Idle { get; } = new();

    internal HashMap<Seq<Key>, Countdown> Expiries { get; init; }
    internal HashMap<Seq<Key>, Countdown> Hovers { get; init; }

    internal Option<Duration> Due => (toSeq(Expiries.Values) + toSeq(Hovers.Values)).Choose(static countdown => countdown.Deadline).Min<Seq, Duration>();

    internal Timers Rearmed(Scene scene, Option<SKPoint> pointer, HashMap<Seq<Key>, Duration> hovering, Duration now) =>
        this with {
            Expiries = toHashMap(scene.Order.Choose(layout => layout.View.Expires.Map(expiry => (layout.Path,
                Expiries.Find(layout.Path).IfNone(() => Countdown.Started(expiry.Lifetime, now))
                    .Held(held: pointer.Exists(at => scene.Visible(layout.Path).Exists(shown => shown.Contains(at))), now))))),
            Hovers = hovering.Map((path, delay) => Hovers.Find(path).IfNone(() => Countdown.Started(delay, now))),
        };

    internal (Timers Timers, Seq<IO<Unit>> Elapsed, HashSet<Seq<Key>> Opened) Tick(Scene scene, Duration now) {
        HashMap<Seq<Key>, Countdown> expired = Expiries.Filter(countdown => countdown.Elapsed(now));
        HashMap<Seq<Key>, Countdown> opened = Hovers.Filter(countdown => countdown.Elapsed(now));
        return (this with { Expiries = Expiries.AddOrUpdateRange(expired.Map(static _ => Countdown.Expired).AsIterable()), Hovers = Hovers.RemoveRange(opened.Keys) },
            expired.Keys.ToSeq().Choose(path => scene.Index.Find(path).Bind(static layout => layout.View.Expires)).Map(static expiry => expiry.Elapsed),
            toHashSet(opened.Keys));
    }
}
