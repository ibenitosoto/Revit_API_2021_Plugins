using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_2021_Plugins
{
    class ExternalDBApplication : IExternalDBApplication
    {
        static double pipeTotalLength = 0;
        static double trayTotalLength = 0;
        static double ductTotalLength = 0;

        static List<Element> pipes = new List<Element>();
        static List<Element> trays = new List<Element>();
        static List<Element> ducts = new List<Element>();

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            application.DocumentChanged -= ElementChangedEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {

            try
            {
                //Register Event
                application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);

            }
            catch (Exception e)
            {

                return ExternalDBApplicationResult.Failed;
            }

            return ExternalDBApplicationResult.Succeeded;
        }

 
      
        
        
        

        //React to every change in the Revit model
        public void ElementChangedEvent(object sender, DocumentChangedEventArgs args)
        {
            if (ElementsAreAdded(args))
            {
                Document doc = args.GetDocument();

                //List<BuiltInCategory> categoryList = new List<BuiltInCategory>();
                //categoryList.Add(BuiltInCategory.OST_PipeCurves);
                //categoryList.Add(BuiltInCategory.OST_CableTray);
                //categoryList.Add(BuiltInCategory.OST_DuctCurves);

                //ElementMulticategoryFilter categoryFilter = new ElementMulticategoryFilter(categoryList);

                //ICollection<ElementId> elementIDs = args.GetAddedElementIds(categoryFilter);

                //foreach (ElementId elementID in elementIDs)
                //{
                //    Element newElement = doc.GetElement(elementID);
                //}

                ElementFilter pipeFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
                ElementFilter trayFilter = new ElementCategoryFilter(BuiltInCategory.OST_CableTray);
                ElementFilter ductFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves);


                try
                {
                    ICollection<ElementId> pipeIDs = args.GetAddedElementIds(pipeFilter);
                    if (pipeIDs.Count > 0)
                    {
                        AddElementsToLists(pipeIDs, pipes, doc);
                    }
                    
                }
                catch (Exception)
                {
                }

                try
                {
                    ICollection<ElementId> trayIDs = args.GetAddedElementIds(trayFilter);
                    if (trayIDs.Count > 0)
                    {
                        AddElementsToLists(trayIDs, trays, doc);
                    }
                }
                catch (Exception)
                {
                }

                try
                {
                    ICollection<ElementId> ductIDs = args.GetAddedElementIds(ductFilter);
                    if (ductIDs.Count > 0)
                    {
                        AddElementsToLists(ductIDs, ducts, doc);
                    }
                }
                catch (Exception)
                {
                }



                //ElementId newPipeID = args.GetAddedElementIds(pipeFilter).First();
                //ElementId newTrayID = args.GetAddedElementIds(trayFilter).First();
                //ElementId newDuctID = args.GetAddedElementIds(ductFilter).First();

                //Element newPipe = doc.GetElement(newPipeID);
                //Element newTray = doc.GetElement(newTrayID);
                //Element newDuct = doc.GetElement(newDuctID);

            }


            UpdateTotalLengths();






            //string name = args.GetTransactionNames().First();


            //TaskDialog.Show("New pipe", "Length of pipe: " + string.Format("{0:F2}", pipeLength/1000) + "mm\n" + "Total length modelled: " + string.Format("{0:F2}", pipeTotalLength/1000) + "mm");
            TaskDialog.Show("Total lengths", "Length of pipes: " + string.Format("{0:F2}", pipeTotalLength / 1000) + "m\n" + "Length of trays: " + string.Format("{0:F2}", trayTotalLength / 1000) + "m\n" + "Length of ducts: " + string.Format("{0:F2}", ductTotalLength / 1000) + "m\n");

        }//end of element changed event handler method



        //React to every change in the Revit model
        //Identify if elements are created or deleted
        //If elements are created, get their categories and add them to the right lists
        //If elements are deleted, remove them from the list



        public bool ElementsAreAdded(DocumentChangedEventArgs args)
        {
            if (args.GetAddedElementIds() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ElementsAreDeleted(DocumentChangedEventArgs args)
        {
            if (args.GetDeletedElementIds() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public void AddElementsToLists(ICollection<ElementId> elementIDs, List<Element> elements ,Document doc)
        {
            foreach (ElementId elementID in elementIDs)
            {
                Element newElement = doc.GetElement(elementID);
                elements.Add(newElement);
            }
        }

        //public void AddElementsToLists(List<Element>elements)
        //{
        //    foreach (Element element in elements)
        //    {
        //        if ((BuiltInCategory)element.Category.Id.IntegerValue is BuiltInCategory.OST_PipeCurves)
        //        {
        //            pipes.Add(element);
        //        }
        //        else if ((BuiltInCategory)element.Category.Id.IntegerValue is BuiltInCategory.OST_CableTray)
        //        {
        //            trays.Add(element);
        //        }
        //        else if ((BuiltInCategory)element.Category.Id.IntegerValue is BuiltInCategory.OST_DuctCurves)
        //        {
        //            ducts.Add(element);
        //        }
        //    }
        //}

        public double GetTotalLength(List<Element>elements)
        {
            double length = 0;
            foreach (Element element in elements)
            {
                double lengthInMm = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                length += FootToMm(lengthInMm);
            }
            return length;
        }

        public void UpdateTotalLengths()
        {
            pipeTotalLength = GetTotalLength(pipes);
            trayTotalLength = GetTotalLength(trays);
            ductTotalLength = GetTotalLength(ducts);
        }

        const double _inchToMm = 25.4;
        const double _footToMm = 12 * _inchToMm;

        /// Convert a given length in millimetres to feet.
        public static double FootToMm(double length)
        {
            return length * _footToMm;
        }

    }//end of class

}//end of namespace
