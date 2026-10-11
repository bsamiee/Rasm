using Rasm.Rhino;
using Rasm.Rhino.UI.Assets;

namespace Darkroom;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class DarkroomGlyph : IGlyph {
    public const int MarkPoints = 16;

    public static readonly DarkroomGlyph Mark = new("mark");
}
