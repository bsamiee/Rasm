using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Rasm.Testing;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Fixture<T>(T Value, Func<TestResult, IO<Unit>> Settle);

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class Fixtures : INotifyTestLifecycleAsync {

    // --- [SCOPES]

    private readonly ConditionalWeakTable<ITest, Atom<Seq<Func<TestResult, IO<Unit>>>>> scopes = [];

    public ValueTask OnTestStartingAsync(IXunitTest test) {
        scopes.Add(test, Atom(Seq<Func<TestResult, IO<Unit>>>()));
        return ValueTask.CompletedTask;
    }

    public ValueTask<T> TakeAsync<T>(IO<Fixture<T>> open) =>
        (from scope in IO.lift(() =>
             TestContext.Current.Test is { } test && scopes.TryGetValue(test, out Atom<Seq<Func<TestResult, IO<Unit>>>>? held) ? held : throw new InvalidOperationException(typeof(T).FullName))
         from opened in open
         from taken in IO.lift(() => scope.Swap(settles => opened.Settle.Cons(settles)))
         select opened.Value).RunAsync(TestContext.Current.CancellationToken);

    public async ValueTask OnTestFinishedAsync(IXunitTest test) =>
        await (from settles in IO.lift(() => scopes.Remove(test, out Atom<Seq<Func<TestResult, IO<Unit>>>>? scope) ? scope.Value : [])
               from parts in settles.Map<K<IO, Unit>>(static settle => settle(TestContext.Current.TestState?.Result ?? TestResult.NotRun)).PartitionFallible().As()
               from settled in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails)))
               select settled).RunAsync(CancellationToken.None).ConfigureAwait(false);

    // --- [LOCAL]

    public static IO<Fixture<SqliteConnection>> Sqlite { get; } =
        from connection in IO.lift(static () => new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = ":memory:" }.ConnectionString))
        from opened in IO.lift(() => {
            connection.LoadVector();
            connection.Open();
        }) | @catch(K<IO, Unit> (error) => IO.lift(connection.Dispose) >> IO.fail<Unit>(error))
        select new Fixture<SqliteConnection>(connection, result => when(result is TestResult.Failed,
            IO.lift(Fin<(nint Image, long Size)> () => connection.Handle is { } handle && raw.sqlite3_serialize(handle, connection.Database, out long size, 0) is var image and not 0
                ? (image, size)
                : Error.New(new InsufficientMemoryException()))
            .Bracket(
                Use: static serialized => IO.lift(() => {
                    byte[] copy = GC.AllocateUninitializedArray<byte>(int.CreateChecked(serialized.Size));
                    Marshal.Copy(serialized.Image, copy, 0, copy.Length);
                    TestContext.Current.AddAttachment($"{Guid.CreateVersion7()}.sqlite3", copy);
                }),
                Fin: static serialized => IO.lift(() => raw.sqlite3_free(serialized.Image)))).As().Finally(IO.lift(connection.Dispose)));

    // --- [COMMANDS]

    public static async Task<object?> ScalarAsync(Func<DbCommand> command) {
        DbCommand asked = command();
        await using (asked.ConfigureAwait(false))
            return await asked.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
    }
}
