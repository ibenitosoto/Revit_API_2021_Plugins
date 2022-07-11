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
using API_2021_Plugins;

namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class KGE_ModelTracker : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //API_2021_Plugins.ExternalApplication.instance.Show_KGE_ModelTracker_WPF(commandData, ref message, elements);
            Show_KGE_ModelTracker_WPF(commandData, ref message, elements);


            return Result.Succeeded;
        }

        public void Show_KGE_ModelTracker_WPF(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Get WPF Interface
            KGE_ModelTracker_WPF modelTrackerWPF = new KGE_ModelTracker_WPF(doc);
            modelTrackerWPF.ShowDialog();
        }
    }

}