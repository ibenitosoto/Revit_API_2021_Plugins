using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class KGE_CreateModels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            const string templatePath = "C:/ProgramData/Autodesk/RVT 2020/Templates/Generic/Default_M_ENU.rte";

            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            Document doc = app.NewProjectDocument(templatePath);

            doc.SaveAs("C:/test/test.rvt");

            return Result.Failed;
        }
    }
}
