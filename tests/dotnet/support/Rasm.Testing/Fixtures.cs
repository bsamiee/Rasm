using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.Buffered;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Rasm.Testing;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Fixture<T>(T Value, Func<TestResult, IO<Unit>> Settle);

// --- [SERVICES] ------------------------------------------------------------------------

public sealed partial class Fixtures : INotifyTestLifecycleAsync {

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

    private sealed record Composed(ImmutableDictionary<string, JsonElement> Services) {
        internal sealed record Service(string Image, ImmutableArray<string>? Command = null);
    }

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
    [JsonSerializable(typeof(Composed))]
    [JsonSerializable(typeof(Composed.Service))]
    private sealed partial class Compose : JsonSerializerContext;

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

    public static IO<Fixture<TContainer>> Container<TContainer>(Func<TContainer> build) where TContainer : IContainer =>
        from container in IO.lift(build)
        from started in IO.liftAsync(env => container.StartAsync(env.Token).ToUnit()) | @catch(K<IO, Unit> (error) => Disposed(container) >> IO.fail<Unit>(error))
        select new Fixture<TContainer>(container, result => when(result is TestResult.Failed, IO.liftAsync(env => container.GetLogsAsync(ct: env.Token)).Map(logs => {
            TestContext.Current.AddAttachment($"{container.Name}.stdout", logs.Stdout);
            TestContext.Current.AddAttachment($"{container.Name}.stderr", logs.Stderr);
            return unit;
        })).As().Finally(Disposed(container)));

    public static IO<(string Image, Seq<string> Command)> Defined(string compose, string service) =>
        from resolved in IO.liftAsync(env => Cli.Wrap("docker").WithArguments(["compose", "--file", compose, "--profile", "*", "config", "--format", "json"]).ExecuteBufferedAsync(env.Token).Task)
        from composed in IO.lift(Optional(JsonSerializer.Deserialize(resolved.StandardOutput, Compose.Default.Composed)).ToFin(Errors.ParseError(compose)))
        from defined in IO.lift(Optional(composed.Services[service].Deserialize(Compose.Default.Service)).ToFin(Errors.ParseError(service)))
        select (defined.Image, toSeq(defined.Command ?? []));

    public static IO<Unit> Disposed(IAsyncDisposable resource) => IO.liftAsync(() => resource.DisposeAsync().AsTask().ToUnit());

    // --- [COMMANDS]

    public static async Task<object?> ScalarAsync(Func<DbCommand> command) {
        DbCommand asked = command();
        await using (asked.ConfigureAwait(false))
            return await asked.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
    }
}
