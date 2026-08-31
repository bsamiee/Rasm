namespace Lab.T05;

internal static class Logging {
    public static Logger Create(TypesToRenderAsString renderAsString) =>
        new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message:j}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
            .Destructure.UsingThinktectureRuntimeExtensions(renderAsString)
            .CreateLogger();

    public static Logger CreateBounded(int maximumDepth) =>
        new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message:j}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
            .Destructure.UsingThinktectureRuntimeExtensions()
            .Destructure.ToMaximumDepth(maximumDepth)
            .CreateLogger();
}
