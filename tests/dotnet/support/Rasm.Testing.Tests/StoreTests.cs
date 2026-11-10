using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using ClickHouse.Driver.ADO;
using Npgsql;
using Rasm.Testing;
using StackExchange.Redis;
using Xunit;

[assembly: AssemblyFixture(typeof(Fixtures))]

namespace Rasm.Testing.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class StoreTests(Fixtures fixtures, StoreSession session) : IClassFixture<StoreSession> {

    // --- [KINDS]

    [Fact]
    public async Task PostgresServesItsLoginItsDatabaseAndItsConnectionLimit() {
        NpgsqlDataSource source = await fixtures.TakeAsync(session.Postgres());
        NpgsqlConnectionStringBuilder stated = new() { ConnectionString = source.ConnectionString };
        Assert.Equal(
            string.Join('/', stated.Username, stated.Database, stated.MaxPoolSize),
            await ScalarAsync(source.CreateCommand("select concat_ws('/', current_user, current_database(), rolconnlimit) from pg_roles where rolname = current_user")));
    }

    [Fact]
    public async Task BucketStoresAnObject() {
        (AmazonS3Client client, string bucket) = await fixtures.TakeAsync(session.Bucket());
        _ = await client.PutObjectAsync(new PutObjectRequest { BucketName = bucket, Key = "held", ContentBody = "stored" }, TestContext.Current.CancellationToken);
        using GetObjectResponse read = await client.GetObjectAsync(bucket, "held", TestContext.Current.CancellationToken);
        using StreamReader text = new(read.ResponseStream);
        Assert.Equal("stored", await text.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ClickhouseServesItsDatabase() {
        ClickHouseDataSource source = await fixtures.TakeAsync(session.Clickhouse());
        Assert.Equal(source.Settings.Database, await ScalarAsync(source.CreateCommand("select currentDatabase()")));
    }

    [Fact]
    public async Task CacheStoresAKeyAndDeliversOnItsChannelPrefix() {
        ConnectionMultiplexer cache = await fixtures.TakeAsync(session.Cache());
        Assert.True(await cache.GetDatabase().StringSetAsync("held", "stored"));
        Assert.Equal("stored", await cache.GetDatabase().StringGetAsync("held"));
        ChannelMessageQueue heard = await cache.GetSubscriber().SubscribeAsync(RedisChannel.Literal("events"));
        _ = await cache.GetSubscriber().PublishAsync(RedisChannel.Literal("events"), "sent");
        Assert.Equal("sent", (await heard.ReadAsync(TestContext.Current.CancellationToken)).Message);
    }

    // --- [TEMPLATES]

    [Fact]
    public async Task TemplateClonesHoldTheBuiltSchemaApart() {
        const string schema = "create table held (label text primary key); insert into held (label) values ('built')";
        Atom<int> builds = Atom(0);
        Template<NpgsqlDataSource> template = new(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{schema}{Guid.CreateVersion7()}"))),
            source => IO.lift(() => builds.Swap(static built => built + 1)).Bind(_ => IO.liftAsync(() => ScalarAsync(source.CreateCommand(schema)))).Map(static _ => unit));
        Seq<NpgsqlDataSource> clones = toSeq(await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => fixtures.TakeAsync(session.Postgres(template)).AsTask())));
        _ = await ScalarAsync(clones[0].CreateCommand("insert into held (label) values ('first')"));
        Assert.Equal(1, builds.Value);
        Assert.Equal(Seq<object?>(2L, 1L, 1L, 1L), await clones.TraverseM(static clone => IO.liftAsync(() => ScalarAsync(clone.CreateCommand("select count(*) from held")))).As().RunAsync(TestContext.Current.CancellationToken));
    }

    // --- [SETTLEMENT]

    [Fact]
    public async Task PassedSettlementDropsTheStore() {
        Fixture<NpgsqlDataSource> opened = await session.Postgres().RunAsync(TestContext.Current.CancellationToken);
        string apart = Apart(opened.Value);
        Assert.True(await Serves(apart).RunAsync(TestContext.Current.CancellationToken));
        _ = await opened.Settle(TestResult.Passed).RunAsync(TestContext.Current.CancellationToken);
        Assert.False(await Serves(apart).RunAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OpenConnectionRefusesTheDropUntilItCloses() {
        Fixture<NpgsqlDataSource> opened = await session.Postgres().RunAsync(TestContext.Current.CancellationToken);
        NpgsqlConnection leaked = await opened.Value.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await using (leaked.ConfigureAwait(false))
            Assert.True(await Record.ExceptionAsync(() => opened.Settle(TestResult.Passed).RunAsync(TestContext.Current.CancellationToken).AsTask())
                is ErrorException thrown && thrown.ToError().Filter<StoreFault>().Head is StoreFault { Reason: var reason } && reason == StoreReason.Leaked);
        _ = await opened.Settle(TestResult.Passed).RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task FailedSettlementKeepsTheStorePastItsSessionAndAttachesItsRecord() {
        StoreSession own = new();
        await using (own.ConfigureAwait(false)) {
            await own.InitializeAsync();
            Fixture<NpgsqlDataSource> kept = await own.Postgres().RunAsync(TestContext.Current.CancellationToken);
            Fixture<NpgsqlDataSource> left = await own.Postgres().RunAsync(TestContext.Current.CancellationToken);
            (string keptApart, string leftApart) = (Apart(kept.Value), Apart(left.Value));
            _ = await kept.Settle(TestResult.Failed).RunAsync(TestContext.Current.CancellationToken);
            _ = Assert.Single(toSeq(TestContext.Current.Attachments?.Keys));
            await left.Value.DisposeAsync();
            await own.DisposeAsync();
            Assert.False(await Serves(leftApart).RepeatUntil(Schedule.spaced(TimeSpan.FromSeconds(1)), static served => !served).RunAsync(TestContext.Current.CancellationToken));
            Assert.True(await Serves(keptApart).RunAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task SilentEndpointFailsNamingItself() {
        using TcpListener free = new(IPAddress.Loopback, 0);
        free.Start();
        ushort port = ushort.CreateChecked(((IPEndPoint)free.LocalEndpoint).Port);
        free.Stop();
        StoreSession silent = StoreSession.At(port);
        await using (silent.ConfigureAwait(false))
            Assert.True(await Record.ExceptionAsync(() => silent.InitializeAsync().AsTask())
                is ErrorException thrown && thrown.ToError().Filter<StoreFault>().Head is StoreFault { Reason: var reason, Subject: Uri { Port: var named } } && reason == StoreReason.Silent && named == port);
    }

    // --- [EXECUTION]

    internal static async Task<object?> ScalarAsync(DbCommand asked) {
        await using (asked.ConfigureAwait(false))
            return await asked.ExecuteScalarAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
    }

    private static string Apart(NpgsqlDataSource source) {
        using NpgsqlConnection pooled = source.CreateConnection();
        using NpgsqlConnection apart = pooled.CloneWith(new NpgsqlConnectionStringBuilder { ConnectionString = source.ConnectionString, Pooling = false }.ConnectionString);
        return apart.ConnectionString;
    }

    private static IO<bool> Serves(string apart) =>
        IO.liftAsync(async env => {
            NpgsqlConnection asked = new(apart);
            await using (asked.ConfigureAwait(false)) {
                await asked.OpenAsync(env.Token).ConfigureAwait(false);
                return true;
            }
        }) | @catch(static error => error.HasException<PostgresException>(), IO.pure(value: false));
}
