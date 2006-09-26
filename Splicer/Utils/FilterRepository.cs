using System;
using DirectShowLib;

namespace Splicer.Utils
{
    public static class FilterRepository
    {
        public static readonly Guid GVQuickTimeFilterId = new Guid("CB83D662-BEFE-4dbf-830C-25E52627C1C3");

        public static IBaseFilter AddGVQuickTimeFilter(IGraphBuilder graphBuilder, string name)
        {
            if (FilterGraphTools.IsThisComObjectInstalled(GVQuickTimeFilterId))
            {
                return FilterGraphTools.AddFilterFromClsid(graphBuilder, GVQuickTimeFilterId, name);
            }
            else
            {
                throw new ArgumentNullException("The GVQuickTime filter is not currently installed");
            }
        }
    }
}