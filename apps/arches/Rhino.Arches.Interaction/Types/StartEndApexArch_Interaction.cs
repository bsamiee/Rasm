
using Rhino.Arches.Core;
using Rhino.Arches.Core.Extensions;

using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Visualization;
using Rhino.Display;
using Rhino.Geometry;

namespace Rhino.Arches.Interaction.Types
{
    public static partial class ArchInteractionTypes
    {

        public static RhinoArchBase Get_StartEndApexArch(Point3d? startPt,
                       Func<DrawThreePointResult, RhinoArchBase> buildArch)
        {

            void onEndPointCallback(DrawEventArgs e, DrawTwoPointResult result)
            {
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);

            }

            ApexCallbackResult onApexPointCallback(DrawEventArgs e, DrawThreePointResult result)
            {
                var arch = buildArch(result);
                if (arch is null)
                    return null;
                arch?.Visualize(e);
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);
                return new ApexCallbackResult(arch.GetMinimumPossibleRise(), arch.GetMaximumPossibleRise());
            }

            var startPoint = Point3d.Unset;
            bool isBothSides = false;
            bool isFlip = false;

            bool startPointWasProvided = false;

            if (startPt == null)
            {
                var result = RunStartLoop(ref isBothSides);

                if (result is null)
                {
                    return null;
                }

                startPoint = result.Value;
            }
            else
            {
                startPoint = startPt.Value;
                startPointWasProvided = true;
            }

            var endResult = RunEndLoop(startPoint, startPointWasProvided,
                isTwoPointArch: false, ref isBothSides, ref isFlip,
                onEndPointCallback: onEndPointCallback);

            if (endResult is null)
            { return null; }

            var twoDpResult = new DrawTwoPointResult(endResult.StartPt, endResult.EndPt, isBothSides, isFlip,
                endResult.SelectedOptionName);

            var threePtResult = RunApexLoop(twoDpResult, onApexPointCallback);

            if (threePtResult is null)
                return null;

            return buildArch?.Invoke(threePtResult);

        }

    }
}
