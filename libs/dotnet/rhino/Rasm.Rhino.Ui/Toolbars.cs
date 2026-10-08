using Rasm.Rhino.Document;
using Rhino;
using Rhino.UI;
using Riok.Mapperly.Abstractions;


namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ToolbarGroupState(Guid Id, Option<string> Name, bool Visible, bool Docked);

public sealed record ToolbarFileState(Guid Id, string Name, string Path, Seq<ToolbarGroupState> Groups, Seq<(Guid Id, string Name)> Toolbars);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ToolbarMapper {
    [MapProperty(nameof(ToolbarGroup.IsDocked), nameof(ToolbarGroupState.Docked))]
    internal static partial ToolbarGroupState ToState(ToolbarGroup group);
}

public static class Toolbars {
    // --- [FILES]
    public static IO<ToolbarFile> OpenFile(string path) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.Open(path), nameof(ToolbarFileCollection.Open)));

    public static IO<ToolbarFile> FindById(Guid id) =>
        IO.lift(() => Files().Find(file => file.Id == id).ToFin(new Missing(nameof(ToolbarFileCollection))));

    public static IO<ToolbarFile> FindByPath(string path) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.FindByPath(path), nameof(ToolbarFileCollection.FindByPath)));

    public static IO<ToolbarFile> FindByName(string name, bool ignoreCase) =>
        IO.lift(() => Missing.Unless(RhinoApp.ToolbarFiles.FindByName(name, ignoreCase), nameof(ToolbarFileCollection.FindByName)));

    public static IO<Unit> Close(ToolbarFile file, bool prompt) =>
        IO.lift(() => Refused.Unless(file.Close(prompt), nameof(ToolbarFile.Close)));

    public static IO<Unit> Save(ToolbarFile file) =>
        IO.lift(() => Refused.Unless(file.Save(), nameof(ToolbarFile.Save)));

    public static IO<Unit> SaveAs(ToolbarFile file, string path) =>
        IO.lift(() => Refused.Unless(file.SaveAs(path), nameof(ToolbarFile.SaveAs)));

    public static IO<Seq<ToolbarFileState>> Read() =>
        IO.lift(static () => Files().Traverse(Snapshot).As());

    private static Seq<ToolbarFile> Files() =>
        Answers.Present(RhinoApp.ToolbarFiles);

    private static Fin<ToolbarFileState> Snapshot(ToolbarFile file) =>
        (toSeq(Range(0, file.GroupCount)).Traverse(index => Missing.Unless(file.GetGroup(index), nameof(ToolbarFile.GetGroup))).As(),
            toSeq(Range(0, file.ToolbarCount)).Traverse(index => Missing.Unless(file.GetToolbar(index), nameof(ToolbarFile.GetToolbar))).As())
        .Apply((groups, toolbars) => new ToolbarFileState(
            file.Id,
            file.Name,
            file.Path,
            groups.Map(static row => ToolbarMapper.ToState(row)).Strict(),
            toolbars.Map(static toolbar => (toolbar.Id, toolbar.Name)).Strict()))
        .As();

    // --- [MENUS]
    public static IO<Unit> RegisterMenuItem(Guid file, Guid menu, Guid item, Func<RuiUpdateUi, IO<Unit>> update, Action<Error> reject) =>
        IO.lift(() => Refused.Unless(
            RuiUpdateUi.RegisterMenuItem(file, menu, item, (_, ui) => _ = Answers.Answer(update(ui), reject, unit)),
            nameof(RuiUpdateUi.RegisterMenuItem)));
}
