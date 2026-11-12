using CsCheck;
using Microsoft.Data.Sqlite;
using Rasm.Testing;
using Xunit;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(Fixtures))]

namespace Rasm.Testing.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class FixturesTests(Fixtures fixtures) {

    // --- [SQLITE]

    [Fact]
    public async Task SqliteRanksStoredVectorsByDistance() {
        SqliteConnection database = await fixtures.TakeAsync(Fixtures.Sqlite);
        _ = await Fixtures.ScalarAsync(() => new SqliteCommand("create virtual table held using vec0(embedding float[2]); insert into held(rowid, embedding) values (1, '[0, 0]'), (2, '[3, 4]'), (3, '[1, 1]')", database));
        Assert.Equal("3,1", await Fixtures.ScalarAsync(() => new SqliteCommand("select group_concat(rowid order by distance) from held where embedding match '[1, 1]' and k = 2", database)));
    }

    [Fact]
    public async Task SqliteDatabasesStayApart() {
        SqliteConnection first = await fixtures.TakeAsync(Fixtures.Sqlite);
        SqliteConnection second = await fixtures.TakeAsync(Fixtures.Sqlite);
        _ = await Fixtures.ScalarAsync(() => new SqliteCommand("create table held (label text)", first));
        Assert.Equal(0L, await Fixtures.ScalarAsync(() => new SqliteCommand("select count(*) from sqlite_schema", second)));
    }

    [Fact]
    public async Task FailedSqliteSettlementAttachesTheDatabase() {
        Fixture<SqliteConnection> opened = await Fixtures.Sqlite.RunAsync(TestContext.Current.CancellationToken);
        _ = await Fixtures.ScalarAsync(() => new SqliteCommand("create table held (label text); insert into held (label) values ('kept')", opened.Value));
        _ = await opened.Settle(TestResult.Failed).RunAsync(TestContext.Current.CancellationToken);
        string kept = Path.GetTempFileName();
        await File.WriteAllBytesAsync(kept, Assert.Single(toSeq(TestContext.Current.Attachments?.Values)).AsByteArray().ByteArray, TestContext.Current.CancellationToken);
        SqliteConnection reader = await fixtures.TakeAsync(Fixtures.Sqlite);
        _ = await Fixtures.ScalarAsync(() => new SqliteCommand("attach database $kept as kept", reader) { Parameters = { new SqliteParameter("$kept", kept) } });
        Assert.Equal("kept", await Fixtures.ScalarAsync(() => new SqliteCommand("select label from kept.held", reader)));
        File.Delete(kept);
    }

    // --- [SETTLEMENT]

    [Fact]
    public Task FinishSettlesEachStoreOnceNewestFirstAndKeepsEveryFailure() {
        IXunitTest test = Assert.IsType<IXunitTest>(TestContext.Current.Test, exactMatch: false);
        return Gen.Bool.Array[0, 8].SampleAsync(async refusals => {
            Atom<Seq<(int Store, TestResult Result)>> settled = Atom(Seq<(int Store, TestResult Result)>());
            Fixtures own = new();
            await own.OnTestStartingAsync(test).ConfigureAwait(false);
            foreach ((int store, bool refuses) in refusals.Index())
                Assert.Equal(store, await own.TakeAsync(IO.pure(new Fixture<int>(store, result =>
                    IO.lift(() => settled.Swap(prior => prior.Add((store, result)))).Bind(_ => when(refuses, IO.fail<Unit>(Error.New(store + 1, "refused"))))))).ConfigureAwait(false));
            Seq<int> newest = toSeq(Enumerable.Range(0, refusals.Length).Reverse());
            Assert.Equal(
                newest.Filter(store => refusals[store]),
                Optional(await Record.ExceptionAsync(() => own.OnTestFinishedAsync(test).AsTask()).ConfigureAwait(false)).ToSeq()
                    .Bind(static thrown => toSeq(Assert.IsType<ErrorException>(thrown, exactMatch: false).ToError().AsIterable().Map(static refusal => refusal.Code - 1))));
            await own.OnTestFinishedAsync(test).ConfigureAwait(false);
            Assert.Equal(newest.Map(static store => (store, TestResult.NotRun)), settled.Value);
        });
    }

    [Fact]
    public async Task TakeOutsideTheFixtureScopeFailsNamingTheStore() {
        Fixtures unscoped = new();
        Assert.Equal(
            typeof(SqliteConnection).FullName,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => unscoped.TakeAsync(Fixtures.Sqlite).AsTask())).Message);
    }
}
