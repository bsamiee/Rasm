using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;

namespace Rasm.Telemetry;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class LogTarget {
    public static readonly LogTarget File = new("file", static (file, name, record) => new ConcurrentFileTarget {
        FileName = Layout.FromLiteral(Path.Join(file.Directory, $"{name}.log")),
        ArchiveFileName = Layout.FromLiteral(Path.Join(file.Directory, $"{name}.{{#}}.log")),
        ArchiveNumbering = ArchiveNumberingMode.Rolling,
        ArchiveAboveSize = file.Size,
        ArchiveEvery = file.Age,
        MaxArchiveFiles = file.Files,
        ConcurrentWrites = true,
        KeepFileOpen = true,
        Layout = record,
    });
    public static readonly LogTarget Stderr = new("stderr", static (_, _, record) => new ConsoleTarget { StdErr = true, Layout = record });

    [UseDelegateFromConstructor]
    internal partial Target Open(LogFile file, string name, Layout record);
}

public sealed record LogFile(LogTarget Target, string Directory, long Size, FileArchiveEveryPeriod Age, int Files) {
    public static LogFile Of(string name) => new(LogTarget.File, UserDirectory(name), 8L << 20, FileArchiveEveryPeriod.Day, 7);

    public static string UserDirectory(string name) =>
        OperatingSystem.IsMacOS() ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Logs", name)
        : OperatingSystem.IsWindows() ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name, "Logs")
        : Path.Join(Environment.GetEnvironmentVariable("XDG_STATE_HOME") ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "state"), name, "log");
}

public sealed record SettingRejection(string Setting, string Value, string Accepted);

public sealed record LogSettings(LogFile File, Seq<LoggerFilterRule> Rules, Seq<SettingRejection> Rejected) {

    // --- [ROWS]

    private const string LevelSetting = "LOG_LEVEL";
    private const string FilterSetting = "LOG_FILTER";

    private static readonly Seq<(string Setting, Func<string, LogFile, Option<LogFile>> Parsed, string Accepted)> Rows = [
        ("LOG_TARGET", static (text, file) => LogTarget.TryGet(text, out LogTarget? target) ? Some(file with { Target = target }) : None, string.Join(", ", LogTarget.Items.Select(static target => target.Key))),
        ("LOG_DIR", static (text, file) => Path.IsPathFullyQualified(text) ? Some(file with { Directory = text }) : None, "a fully qualified folder path"),
        ("LOG_SIZE", static (text, file) => long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long size) && size > 0 ? Some(file with { Size = size }) : None, "a positive byte count"),
        ("LOG_AGE", static (text, file) => Enum.TryParse(text, ignoreCase: true, out FileArchiveEveryPeriod age) && Enum.IsDefined(age) ? Some(file with { Age = age }) : None, string.Join(", ", Enum.GetNames<FileArchiveEveryPeriod>())),
        ("LOG_FILES", static (text, file) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int files) && files > 0 ? Some(file with { Files = files }) : None, "a positive file count"),
    ];

    private static readonly FrozenDictionary<string, LogLevel> Levels =
        NLog.LogLevel.AllLevels.ToFrozenDictionary(static level => level.Name, static level => (LogLevel)level.Ordinal, StringComparer.OrdinalIgnoreCase);

    private static readonly string LevelNames = string.Join(", ", Levels.Keys);

    // --- [READS]

    public static LogSettings Read(IConfiguration settings, string name, Seq<string> roots) {
        (LogFile file, Seq<SettingRejection> refused) = Rows.Fold((File: LogFile.Of(name), Refused: Seq<SettingRejection>()), (read, row) =>
            Optional(settings[row.Setting]).Match(
                Some: text => row.Parsed(text, read.File).Match(Some: file => (file, read.Refused), None: () => (read.File, read.Refused.Add(new SettingRejection(row.Setting, text, row.Accepted)))),
                None: () => read));
        Seq<(SettingRejection Asked, Option<LoggerFilterRule> Rule)> leveled = (
            (from text in Optional(settings[LevelSetting]).ToSeq()
             from root in roots
             select Leveled(LevelSetting, text, root, text, LevelNames))
            + (from text in Optional(settings[FilterSetting]).ToSeq()
               from pair in toSeq(text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
               select pair.Split('=', StringSplitOptions.TrimEntries) is [var scope, var level]
                   ? Leveled(FilterSetting, pair, scope, level, $"scope=level pairs with level {LevelNames}")
                   : (new SettingRejection(FilterSetting, pair, $"scope=level pairs with level {LevelNames}"), None))).Strict();
        return new(
            file,
            roots.Map(static root => new LoggerFilterRule(providerName: null, root, LogLevel.Information, filter: null)) + leveled.Choose(static row => row.Rule),
            (refused + leveled.Filter(static row => row.Rule.IsNone).Map(static row => row.Asked)).Map(rejection => rejection with { Setting = ProcessEnvironment.Key(Some(name), rejection.Setting) }));
    }

    private static (SettingRejection Asked, Option<LoggerFilterRule> Rule) Leveled(string setting, string text, string scope, string level, string accepted) =>
        (new SettingRejection(setting, text, accepted), Levels.TryGetValue(level, out LogLevel least) ? Some(new LoggerFilterRule(providerName: null, scope, least, filter: null)) : None);
}

// --- [SERVICES] ------------------------------------------------------------------------

public sealed partial class JsonLinesLoggerProvider : ILoggerProvider {

    // --- [STATE]

    private readonly NLog.LogFactory factory = new() { AutoShutdown = false };
    private readonly ConcurrentDictionary<SettingRejection, Unit> reported = new();
    private readonly NLogLoggerProvider provider;
    private readonly IDisposable reload;

    internal JsonLinesLoggerProvider(Func<LogSettings> settings, Func<IChangeToken> changed, Func<Seq<SettingRejection>> rejected, string name, Option<string> version) {
        provider = new NLogLoggerProvider(new NLogProviderOptions { CaptureEventId = EventIdCaptureType.None, IncludeScopes = false }, factory);
        ILogger logger = provider.CreateLogger(typeof(JsonLinesLoggerProvider).ToString());
        Action opened = () => {
            LogSettings read = settings();
            LoggingConfiguration configuration = new(factory);
            configuration.AddRuleForAllLevels(read.File.Target.Open(read.File, name, Record(name, version)));
            factory.Configuration = configuration;
            _ = (read.Rejected + rejected()).Filter(rejection => reported.TryAdd(rejection, unit))
                .Iter(rejection => Rejected(logger, rejection.Setting, rejection.Value, rejection.Accepted));
        };
        opened();
        reload = ChangeToken.OnChange(changed, opened);
    }

    // --- [LOGGERS]

    public ILogger CreateLogger(string categoryName) => provider.CreateLogger(categoryName);

    public void Dispose() {
        reload.Dispose();
        provider.Dispose();
        factory.Dispose();
    }

    // --- [RECORDS]

    private static JsonLayout Record(string name, Option<string> version) =>
        new() {
            SuppressSpaces = true,
            Attributes = {
                new JsonAttribute("timestamp", "${date:universalTime=true:format=yyyy-MM-ddTHH\\:mm\\:ss.ffffffZ}"),
                new JsonAttribute("severity_text", "${level:uppercase=true}"),
                new JsonAttribute("severity_number", Layout<int>.FromMethod(static entry => (entry.Level.Ordinal * 4) + 1), encode: false),
                new JsonAttribute("body", "${message:raw=true}"),
                new JsonAttribute("scope", "${logger}"),
                new JsonAttribute("trace_id", Layout<string?>.FromMethod(static _ => Activity.Current?.TraceId.ToHexString())),
                new JsonAttribute("span_id", Layout<string?>.FromMethod(static _ => Activity.Current?.SpanId.ToHexString())),
                new JsonAttribute("trace_flags", Layout<string?>.FromMethod(static _ => Activity.Current is { } activity ? ((byte)activity.ActivityTraceFlags).ToString("x2", CultureInfo.InvariantCulture) : null)),
                new JsonAttribute("attributes", new JsonLayout {
                    SuppressSpaces = true,
                    RenderEmptyObject = false,
                    IncludeEventProperties = true,
                    ExcludeProperties = { "CodeFunctionName", "ErrorType", "ErrorMessage", "exception.type", "exception.message", "exception.stacktrace" },
                    Attributes = {
                        new JsonAttribute("code.function.name", "${event-properties:CodeFunctionName}"),
                        new JsonAttribute("error.type", "${event-properties:ErrorType}"),
                        new JsonAttribute("error.message", "${event-properties:ErrorMessage}"),
                        new JsonAttribute("exception.type", "${exception:format=Type}"),
                        new JsonAttribute("exception.message", "${exception:format=Message}"),
                        new JsonAttribute("exception.stacktrace", "${when:when=level>=LogLevel.Error:inner=${exception:format=ToString}}"),
                    },
                }, encode: false),
                new JsonAttribute("resource", (
                    Seq(new JsonAttribute("service.name", Layout.FromLiteral(name)))
                    + version.Map(static held => new JsonAttribute("service.version", Layout.FromLiteral(held))).ToSeq()
                    + Seq(new JsonAttribute("process.pid", "${processid}", encode: false)))
                    .Fold(new JsonLayout { SuppressSpaces = true }, static (resource, attribute) => {
                        resource.Attributes.Add(attribute);
                        return resource;
                    }), encode: false),
            },
        };

    [LoggerMessage(Level = LogLevel.Warning, Message = "Setting refused, default stands: {variable}={value}, accepts {accepted}")]
    private static partial void Rejected(ILogger logger, string variable, string value, string accepted);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class JsonLinesLoggerExtensions {
    extension(ILoggingBuilder builder) {
        public ILoggingBuilder AddJsonLines(IConfiguration settings, string name, Option<string> version, Seq<string> roots) {
            _ = builder.Services
                .AddSingleton<IOptionsChangeTokenSource<LoggerFilterOptions>>(new ConfigurationChangeTokenSource<LoggerFilterOptions>(settings))
                .Configure<LoggerFilterOptions>(filters => {
                    filters.MinLevel = LogLevel.Warning;
                    _ = LogSettings.Read(settings, name, roots).Rules.Iter(filters.Rules.Add);
                });
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, JsonLinesLoggerProvider>(services => new JsonLinesLoggerProvider(
                () => LogSettings.Read(settings, name, roots), settings.GetReloadToken, () => toSeq(services.GetServices<SettingRejection>()), name, version)));
            return builder;
        }
    }
}
