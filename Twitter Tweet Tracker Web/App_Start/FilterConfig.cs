using System.Web;
using System.Web.Mvc;

namespace Twitter_Tweet_Tracker_Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
