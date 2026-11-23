using System.Data;
using CsCheck;
using Microsoft.Data.Sqlite;
using Rasm.Testing;
using Xunit;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(Fixtures))]

namespace Rasm.Testing.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class FixturesTests(Fixtures fixtures) {

    // --- [SETTLEMENT]

    public static TheoryData<TestResult> Results { get; } = [.. Enum.GetValues<TestResult>()];

    [Fact]
    public Task FinishSettlesEachStoreOnceNewestFirstAndKeepsEveryFailure() {
        IXunitTest test = Assert.IsType<IXunitTest>(TestContext.Current.Test, exactMatch: false);
        return Gen.Bool.Array[0, 8].SampleAsync(async refusals => {
            Atom<Seq<(int Fixture, TestResult Result)>> settled = Atom(Seq<(int Fixture, TestResult Result)>());
            Fixtures own = new();
            await own.OnTestStartingAsync(test);
            foreach ((int fixture, bool refuses) in refusals.Index())
                Assert.Equal(fixture, await own.TakeAsync(IO.pure(new Fixture<int>(fixture, result =>
                    IO.lift(() => settled.Swap(prior => prior.Add((fixture, result)))).Bind(_ => when(refuses, IO.fail<Unit>(Error.New(fixture + 1, "refused"))))))));
            Seq<int> newest = toSeq(Enumerable.Range(0, refusals.Length).Reverse());
            Assert.Equal(
                newest.Filter(fixture => refusals[fixture]),
                Optional(await Record.ExceptionAsync(() => own.OnTestFinishedAsync(test).AsTask())).ToSeq()
                    .Bind(static thrown => toSeq(Assert.IsType<ErrorException>(thrown, exactMatch: false).ToError().AsIterable().Map(static refusal => refusal.Code - 1))));
            await own.OnTestFinishedAsync(test);
            Assert.Equal(newest.Map(static fixture => (fixture, TestResult.NotRun)), settled.Value);
        });
    }

    [Theory]
    [MemberData(nameof(Results))]
    public async Task FolderSettlementDeletesTheFolderWhateverTheResult(TestResult result) {
        Fixture<DirectoryInfo> opened = await Fixtures.Folder.RunAsync(TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(opened.Value.FullName, "held.txt"), nameof(FolderSettlementDeletesTheFolderWhateverTheResult), TestContext.Current.CancellationToken);
        await opened.Settle(result).RunAsync(TestContext.Current.CancellationToken);
        Assert.False(Directory.Exists(opened.Value.FullName));
    }

    public static TheoryData<TestResult> Unfailed { get; } = [.. Enum.GetValues<TestResult>().Where(static result => result is not TestResult.Failed)];

    [Fact]
    public async Task FailedSqliteSettlementClosesTheConnectionAndAttachesTheDatabase() {
        Fixture<SqliteConnection> opened = await SettledAsync(TestResult.Failed);
        string kept = Path.Combine((await fixtures.TakeAsync(Fixtures.Folder)).FullName, "kept.sqlite3");
        await File.WriteAllBytesAsync(kept, Assert.Single(toSeq(TestContext.Current.Attachments?.Values)).AsByteArray().ByteArray, TestContext.Current.CancellationToken);
        SqliteConnection reader = await fixtures.TakeAsync(Fixtures.Sqlite);
        await Fixtures.ScalarAsync(() => new SqliteCommand("attach database $kept as kept", reader) { Parameters = { new SqliteParameter("$kept", kept) } });
        Assert.Equal(ConnectionState.Closed, opened.Value.State);
        Assert.Equal("kept", await Fixtures.ScalarAsync(() => new SqliteCommand("select label from kept.held", reader)));
    }

    [Theory]
    [MemberData(nameof(Unfailed))]
    public async Task UnfailedSqliteSettlementClosesTheConnectionAndAttachesNothing(TestResult result) {
        Fixture<SqliteConnection> opened = await SettledAsync(result);
        Assert.Equal(ConnectionState.Closed, opened.Value.State);
        Assert.Empty(toSeq(TestContext.Current.Attachments?.Keys));
    }

    private static async Task<Fixture<SqliteConnection>> SettledAsync(TestResult result) {
        Fixture<SqliteConnection> opened = await Fixtures.Sqlite.RunAsync(TestContext.Current.CancellationToken);
        await Fixtures.ScalarAsync(() => new SqliteCommand("create table held (label text); insert into held (label) values ('kept')", opened.Value));
        await opened.Settle(result).RunAsync(TestContext.Current.CancellationToken);
        return opened;
    }

    // --- [SQLITE]

    [Fact]
    public async Task SqliteRanksStoredVectorsByDistance() {
        SqliteConnection database = await fixtures.TakeAsync(Fixtures.Sqlite);
        await Fixtures.ScalarAsync(() => new SqliteCommand("create virtual table held using vec0(embedding float[2]); insert into held(rowid, embedding) values (1, '[0, 0]'), (2, '[3, 4]'), (3, '[1, 1]')", database));
        Assert.Equal("3,1", await Fixtures.ScalarAsync(() => new SqliteCommand("select group_concat(rowid order by distance) from held where embedding match '[1, 1]' and k = 2", database)));
    }
}
