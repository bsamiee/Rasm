using System.Globalization;
using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using ClickHouse.Driver.ADO;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Npgsql;
using Rasm.Testing;
using StackExchange.Redis;
using Xunit;

namespace Rasm.Store.Testing;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Template<T>(string Digest, Func<T, IO<Unit>> Build);

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class StoreReason {
    public static readonly StoreReason Silent = new("holds no store session");
    public static readonly StoreReason Refused = new("was refused");
    public static readonly StoreReason Leaked = new("refused its drop with a connection open");

    public string Message { get; }
}

public sealed record StoreFault(object Subject, StoreReason Reason, Option<Error> Inner = default) : Expected(Reason.Message, 0, Inner) {
    public object Subject { get; } = Subject;
    public StoreReason Reason { get; } = Reason;

    public override string Message => Inner.Match(
        Some: cause => string.Create(CultureInfo.InvariantCulture, $"{Subject} {Reason.Message}: {cause.Message}"),
        None: () => string.Create(CultureInfo.InvariantCulture, $"{Subject} {Reason.Message}"));
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class StoreSession : IAsyncLifetime {

    // --- [SESSION]

    private sealed record Held(Uri Endpoint, GrpcChannel Channel, StoreService.StoreServiceClient Client, AsyncServerStreamingCall<SessionLease> Hold) {
        public static Held Open(ushort port, SessionRequest session) {
            Uri endpoint = new UriBuilder(Uri.UriSchemeHttp, IPAddress.Loopback.ToString(), port).Uri;
            GrpcChannel channel = GrpcChannel.ForAddress(endpoint);
            StoreService.StoreServiceClient client = new(channel);
            return new(endpoint, channel, client, client.HoldSession(session, cancellationToken: TestContext.Current.CancellationToken));
        }
    }

    private readonly SessionRequest session = new() { Session = Guid.CreateVersion7().ToString() };
    private readonly Fin<Held> held;

    private const string Published = "STORE_PORT";

    public StoreSession() : this(Optional(Environment.GetEnvironmentVariable(Published)).Bind(static port => parseUShort(port)).ToFin(Errors.ParseError(Published))) { }

    private StoreSession(Fin<ushort> port) => held = port.Map(open => Held.Open(open, session));

    public static StoreSession At(ushort port) => new(port);

    public async ValueTask InitializeAsync() =>
        await IO.lift(held).Bind(static open =>
                IO.liftAsync(env => open.Hold.ResponseStream.MoveNext(env.Token)).Bind(static leased => unless(leased, IO.fail<Unit>(Errors.Closed)).As())
                | @catch(K<IO, Unit> (error) => IO.fail<Unit>(new StoreFault(open.Endpoint, StoreReason.Silent, error))))
            .RunAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);

    public ValueTask DisposeAsync() {
        _ = held.Iter(static open => {
            open.Hold.Dispose();
            open.Channel.Dispose();
        });
        return ValueTask.CompletedTask;
    }

    private static IO<TReply> Call<TRequest, TReply>(Func<TRequest, CallOptions, AsyncUnaryCall<TReply>> rpc, TRequest request) where TRequest : IMessage =>
        IO.liftAsync(env => rpc(request, new CallOptions(cancellationToken: env.Token)).ResponseAsync)
        | @catch(static error => error.HasException<RpcException>(), K<IO, TReply> (error) => IO.fail<TReply>(new StoreFault(request, StoreReason.Refused, error)));

    private static IO<Unit> Ended<TReply>(Func<StoreRequest, CallOptions, AsyncUnaryCall<TReply>> rpc, string id) => Call(rpc, new StoreRequest { Id = id }).Map(static _ => unit);

    private static Func<Error, bool> Refused(StatusCode status) =>
        error => error.Filter<StoreFault>().Head is StoreFault { Inner: var inner } && inner.Bind(static cause => cause.Exception).Exists(thrown => thrown is RpcException refusal && refusal.StatusCode == status);

    // --- [KINDS]

    private sealed record Driver<TRecord, T>(Func<TRecord, string> Id, Func<TRecord, IO<T>> Open, Func<T, IO<Unit>> Close);

    private static readonly Driver<PostgresStore, NpgsqlDataSource> Postgreses = new(
        static store => store.Id,
        static store => IO.lift(() => NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder {
            Host = store.Host, Port = int.CreateChecked(store.Port), Database = store.Database, Username = store.Username, Password = store.Password,
            MaxPoolSize = int.CreateChecked(store.ConnectionLimit),
        })),
        Fixtures.Disposed);

    private static readonly Driver<ClickhouseStore, ClickHouseDataSource> Clickhouses = new(
        static store => store.Id,
        static store => IO.lift(() => new ClickHouseDataSource(new ClickHouseClientSettings {
            Host = store.Host, Port = ushort.CreateChecked(store.HttpPort), Database = store.Database, Username = store.Username, Password = store.Password,
        })),
        Fixtures.Disposed);

    private static readonly Driver<CacheStore, ConnectionMultiplexer> Caches = new(
        static store => store.Id,
        static store => IO.liftAsync(() => ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions {
            EndPoints = { { store.Host, int.CreateChecked(store.Port) } }, DefaultDatabase = int.CreateChecked(store.Database), User = store.Username, Password = store.Password,
            ChannelPrefix = RedisChannel.Literal(store.ChannelPrefix),
        })),
        Fixtures.Disposed);

    public IO<Fixture<NpgsqlDataSource>> Postgres(Option<Template<NpgsqlDataSource>> template = default) =>
        IO.lift(held).Bind(open => Taken(open.Client, Postgreses, template.Match(
            Some: built => Cloned(open.Client, Postgreses, built, open.Client.OpenPostgresTemplateAsync, open.Client.ClonePostgresTemplateAsync),
            None: () => Call(open.Client.CreatePostgresAsync, session))));

    public IO<Fixture<ClickHouseDataSource>> Clickhouse(Option<Template<ClickHouseDataSource>> template = default) =>
        IO.lift(held).Bind(open => Taken(open.Client, Clickhouses, template.Match(
            Some: built => Cloned(open.Client, Clickhouses, built, open.Client.OpenClickhouseTemplateAsync, open.Client.CloneClickhouseTemplateAsync),
            None: () => Call(open.Client.CreateClickhouseAsync, session))));

    public IO<Fixture<ConnectionMultiplexer>> Cache() => IO.lift(held).Bind(open => Taken(open.Client, Caches, Call(open.Client.CreateCacheAsync, session)));

    public IO<Fixture<(AmazonS3Client Client, string Bucket)>> Bucket() =>
        IO.lift(held).Bind(open => Taken(
            open.Client,
            new Driver<BucketStore, (AmazonS3Client Client, string Bucket)>(
                static store => store.Id,
                store => IO.lift(() => (
                    new AmazonS3Client(new Renewing(store, Call(open.Client.RenewBucketAsync, new StoreRequest { Id = store.Id })), new AmazonS3Config { ServiceURL = store.Endpoint, AuthenticationRegion = store.Region, ForcePathStyle = true }),
                    store.Bucket)),
                static bucket => IO.lift(bucket.Client.Dispose)),
            Call(open.Client.CreateBucketAsync, session)));

    private static IO<Fixture<T>> Taken<TRecord, T>(StoreService.StoreServiceClient client, Driver<TRecord, T> driver, IO<TRecord> created) where TRecord : IMessage =>
        from record in created
        let id = driver.Id(record)
        from store in driver.Open(record) | @catch(K<IO, T> (error) => Ended(client.DropStoreAsync, id) >> IO.fail<T>(error))
        select new Fixture<T>(store, result => driver.Close(store) >> (result is TestResult.Failed
            ? Ended(client.KeepStoreAsync, id) >> IO.lift(() => TestContext.Current.AddAttachment(id, JsonFormatter.Default.Format(record)))
            : Ended(client.DropStoreAsync, id) | @catch(Refused(StatusCode.FailedPrecondition), K<IO, Unit> (error) => IO.fail<Unit>(new StoreFault(id, StoreReason.Leaked, error)))));

    private IO<TRecord> Cloned<TRecord, T>(
        StoreService.StoreServiceClient client, Driver<TRecord, T> driver, Template<T> template,
        Func<TemplateRequest, CallOptions, AsyncUnaryCall<TRecord>> open, Func<TemplateRequest, CallOptions, AsyncUnaryCall<TRecord>> clone) {
        TemplateRequest digest = new() { Session = session.Session, Digest = template.Digest };
        IO<Unit> built = Call(open, digest).Bind(build =>
                (driver.Open(build).Bracket(Use: template.Build, Fin: driver.Close) >> Ended(client.SealTemplateAsync, driver.Id(build)))
                | @catch(K<IO, Unit> (error) => Ended(client.DropStoreAsync, driver.Id(build)) >> IO.fail<Unit>(error)))
            | @catch(Refused(StatusCode.AlreadyExists), unitIO);
        return (Call(clone, digest) | @catch(Refused(StatusCode.NotFound), K<IO, TRecord> (_) => built >> Call(clone, digest))).RetryWhile(Refused(StatusCode.NotFound));
    }

    private sealed class Renewing : RefreshingAWSCredentials {
        private readonly IO<BucketStore> renewed;

        public Renewing(BucketStore granted, IO<BucketStore> renewed) {
            this.renewed = renewed;
            currentState = Granted(granted);
        }

        protected override Task<CredentialsRefreshState> GenerateNewCredentialsAsync() => renewed.Map(Granted).RunAsync(CancellationToken.None).AsTask();

        private CredentialsRefreshState Granted(BucketStore store) =>
            new(new ImmutableCredentials(store.AccessKeyId, store.SecretAccessKey, store.SessionToken), _timeProvider.CorrectedUtcNow.AddSeconds(store.CredentialsTtlSeconds));
    }
}
