using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;
using System.Reflection;

namespace API_2021_Plugins
{
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Create Ribbon Tab
            application.CreateRibbonTab("KGE BIM");

            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData button1 = new PushButtonData("button1", "Quantifications", path, "API_2021_Plugins.KGE_Quantifications");
            PushButtonData button2 = new PushButtonData("button2", "Isometrics", path, "API_2021_Plugins.KGE_Isometrics");
            PushButtonData button3 = new PushButtonData("button3", "Lines From Pipes", path, "API_2021_Plugins.KGE_LinesFromPipes");

            RibbonPanel panel = application.CreateRibbonPanel("KGE BIM", "KGE SCRIPTS");

            panel.AddItem(button1);
            panel.AddItem(button2);
            panel.AddItem(button3);

            return Result.Succeeded;
        }
    }
}
