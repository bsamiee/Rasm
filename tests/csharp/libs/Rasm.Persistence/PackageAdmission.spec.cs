using Rasm.TestKit;

namespace Rasm.Persistence.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class PackageAdmissionLaws {
    [Fact]
    public void PersistenceStorePackagesAreCentralAndManagedSpecsCarryNoRuntimeProofPackages() {
        PackageAdmission.ApiCatalogues(
            relativeDirectory: "libs/csharp/Rasm.Persistence/.api",
            "api-arrow.md",
            "api-npgsql-nts.md",
            "api-redis.md",
            "api-thinktecture-ef.md");

        ProjectAdmission app = PackageAdmission.Project(relativePath: "libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj");
        app.IncludesOnlyProjects("../Rasm/Rasm.csproj", "../Rasm.Element/Rasm.Element.csproj", "../Rasm.AppHost/Rasm.AppHost.csproj");
        app.IncludesOnlyPackages(
            "Generator.Equals",
            "QuikGraph",
            "Riok.Mapperly",
            "AWSSDK.KeyManagementService",
            "Azure.Security.KeyVault.Keys",
            "ClickHouse.Driver",
            "CloudNative.CloudEvents",
            "CloudNative.CloudEvents.Kafka",
            "CloudNative.CloudEvents.SystemTextJson",
            "Confluent.Kafka",
            "DeltaLake.Net",
            "DotPulsar",
            "DuckDB.NET.Data.Full",
            "EFCore.NamingConventions",
            "Google.Cloud.Kms.V1",
            "linq2db.EntityFrameworkCore",
            "Microsoft.Data.Sqlite",
            "Microsoft.EntityFrameworkCore.Design",
            "Microsoft.EntityFrameworkCore.Sqlite",
            "NetTopologySuite.IO.GeoJSON4STJ",
            "NetTopologySuite.IO.GeoPackage",
            "NodaTime",
            "NodaTime.Serialization.SystemTextJson",
            "Npgsql",
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite",
            "Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime",
            "Npgsql.NetTopologySuite",
            "Npgsql.OpenTelemetry",
            "Pgvector",
            "Pgvector.EntityFrameworkCore",
            "pocketken.H3",
            "Qdrant.Client",
            "ScyllaDBCSharpDriver",
            "SQLitePCLRaw.bundle_e_sqlite3",
            "System.Numerics.Tensors",
            "Thinktecture.Runtime.Extensions.EntityFrameworkCore10",
            "LightningDB",
            "rocksdb",
            "AWSSDK.S3",
            "Azure.Storage.Blobs",
            "CommunityToolkit.HighPerformance",
            "Google.Cloud.Storage.V1",
            "Microsoft.Extensions.Caching.StackExchangeRedis",
            "Minio",
            "StackExchange.Redis",
            "Apache.Arrow",
            "Apache.Arrow.Adbc",
            "Apache.Arrow.Adbc.Drivers.Apache",
            "Apache.Arrow.Adbc.Drivers.BigQuery",
            "Apache.Arrow.Compression",
            "Apache.Arrow.Flight",
            "FastCDC.Net",
            "FlowtideDotNet.Substrait",
            "ParquetSharp",
            "Ara3D.BimOpenSchema",
            "Ara3D.BimOpenSchema.IO",
            "Chr.Avro",
            "Chr.Avro.Binary",
            "Chr.Avro.Confluent",
            "Confluent.SchemaRegistry",
            "Confluent.SchemaRegistry.Serdes.Avro",
            "Confluent.SchemaRegistry.Serdes.Json",
            "Confluent.SchemaRegistry.Serdes.Protobuf",
            "NATS.Net",
            "RabbitMQ.Client",
            "K4os.Compression.LZ4",
            "Marten",
            "MessagePack",
            "MessagePackAnalyzer",
            "Microsoft.AspNetCore.JsonPatch.SystemTextJson",
            "Microsoft.Extensions.Caching.Hybrid",
            "Microsoft.Extensions.Compliance.Redaction",
            "MiniExcel",
            "MPXJ.Net",
            "Sep",
            "System.Formats.Cbor",
            "System.IO.Hashing",
            "Thinktecture.Runtime.Extensions.Json",
            "Thinktecture.Runtime.Extensions.MessagePack",
            "ZstdSharp.Port",
            "PollinationSDK",
            "Speckle.Objects",
            "Speckle.Sdk");
        app.PackageReferenceHasAttribute(packageName: "Microsoft.EntityFrameworkCore.Design", attributeName: "PrivateAssets", expectedValue: "all");
        app.PackageReferenceHasAttribute(packageName: "MessagePackAnalyzer", attributeName: "PrivateAssets", expectedValue: "all");

        ProjectAdmission tests = PackageAdmission.Project(relativePath: "tests/csharp/libs/Rasm.Persistence/Rasm.Persistence.Tests.csproj");
        tests.IncludesNoPackages();
    }
}
