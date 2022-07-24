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
        public static ExternalCommandData cd;
        public static Autodesk.Revit.ApplicationServices.Application app;
        public static Document doc;
        public static UIDocument uidoc;
        public static UIApplication uiapp;

        public static string template;
        public static string folder;
        public static List<string> modelNames;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Show_KGE_CreateModels_WPF(commandData, ref message, elements);

            uiapp = commandData.Application;
            app = uiapp.Application;

            //const string templatePath = "C:/ProgramData/Autodesk/RVT 2020/Templates/Generic/Default_M_ENU.rte";

            return Result.Succeeded;
        }


        public void Show_KGE_CreateModels_WPF(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get WPF Interface
            KGE_CreateModels_WPF createModelsForm = new KGE_CreateModels_WPF(commandData);
            createModelsForm.ShowDialog();
        }

        public static void CreateModels(string templatePath, string folderPath, List<string> modelNamesList)
        {
            template = templatePath;
            folder = folderPath;
            modelNames = modelNamesList;

            foreach (string modelName in modelNamesList)
            {
                string completeModelPath = folder + "/" + modelName + ".rvt";
                doc = app.NewProjectDocument(templatePath);
                doc.SaveAs(completeModelPath);
            }

        }

    }
}
