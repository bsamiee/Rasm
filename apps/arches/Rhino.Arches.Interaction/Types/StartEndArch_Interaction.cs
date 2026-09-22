using Rhino.Arches.Core;
using Rhino.Arches.Interaction.Faactory;
using Rhino.Arches.Interaction.Visualization;
using Rhino.Display;
using Rhino.Geometry;


namespace Rhino.Arches.Interaction.Types
{
    public static partial class ArchInteractionTypes
    {
        public static RhinoArchBase Get_StartEndArch(Point3d? startPt,
Func<DrawTwoPointResult, RhinoArchBase> buildArch)
        {
            void onEndPointCallback(DrawEventArgs e, DrawTwoPointResult result)
            {
                var arch = buildArch(result);
                arch?.Visualize(e);
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);

            }

            var startPoint = Point3d.Unset;
            bool isBothSides = false;
            bool isFlip = false;

            bool startPointWasProvided = false;

            if (startPt == null)
            {
                var result = RunStartLoop(ref isBothSides, null, null);

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

            var pD = RunEndLoop(startPoint, startPointWasProvided, isTwoPointArch: true,
                ref isBothSides, ref isFlip,
                onEndPointCallback: onEndPointCallback);

            if (pD is null)
            { return null; }

            var arch = buildArch?.Invoke(pD);

            return arch;
        }


        public static RhinoArchBase Get_StartEndArch(Point3d? startPt,
Func<DrawTwoPointResult,bool, RhinoArchBase> buildArch)
        {
            bool showPointed = false;
            void onEndPointCallback(DrawEventArgs e, DrawTwoPointResult result)
            {
                var arch = buildArch(result,showPointed);
                arch?.Visualize(e);
                e.VisualizeArchEndPoints(result.StartPt, result.EndPt);

            }

            var startPoint = Point3d.Unset;
            bool isBothSides = false;
            bool isFlip = false;

            bool startPointWasProvided = false;

            if (startPt == null)
            {
                var result = RunStartLoop(ref isBothSides, null, null);

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

            //var pD = RunEndLoop(startPoint, startPointWasProvided, isTwoPointArch: true,
            //    ref isBothSides, ref isFlip,
            //    onEndPointCallback: onEndPointCallback);

            var pD = RunEndLoop_FoilArch(startPoint, startPointWasProvided, isTwoPointArch: true,
    ref isBothSides, ref isFlip, ref showPointed,
    onEndPointCallback: onEndPointCallback);

            if (pD is null)
            { return null; }

            var arch = buildArch?.Invoke(pD,showPointed);

            return arch;
        }

    }
}
