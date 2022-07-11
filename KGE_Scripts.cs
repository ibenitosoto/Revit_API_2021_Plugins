using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class KGE_Scripts
    {
        public static Element PickObject(ExternalCommandData commandData)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get UI App
            UIApplication uiApp = commandData.Application;

            Document doc = uiApp.ActiveUIDocument.Document;
            Application app = uiApp.Application;

            try
            {
                //pick object
                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //element Id
                ElementId pickedElementId = pickedObj.ElementId;

                //element
                Element pickedElement = doc.GetElement(pickedElementId);

                if (pickedElement != null)
                {
                    //TaskDialog.Show("Picked Object", $"Element selected: {pickedElement.Category.Name} with ID: {pickedElementId}");
                    ExternalApplication.pickedObject = pickedElement;
                  
                    return pickedElement;

                }

                else
                {
                    TaskDialog.Show("error", $"Error. Contact Ignacio Benito Soto");
                    return null;
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("error", $"Error. Contact Ignacio Benito Soto");
                return null;
            }

          


        }//end

    }//end of class


}