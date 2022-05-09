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
        static List<ElementId> allElementIDs = new List<ElementId>();

        static Dictionary<ElementId,double> lengthDict = new Dictionary<ElementId,double>();
        static Dictionary<ElementId, BuiltInCategory> categoryDict = new Dictionary<ElementId, BuiltInCategory>();

        ElementFilter pipeFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
        ElementFilter trayFilter = new ElementCategoryFilter(BuiltInCategory.OST_CableTray);
        ElementFilter ductFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves);



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
            catch (Exception)
            {

                return ExternalDBApplicationResult.Failed;
            }

            return ExternalDBApplicationResult.Succeeded;
        }

 
      
        
        
        

        //React to every change in the Revit model
        public void ElementChangedEvent(object sender, DocumentChangedEventArgs args)
        {
            string transactionName = args.GetTransactionNames().FirstOrDefault();

            Document doc = args.GetDocument();

            UndoOperation undoOperation = args.Operation;
            if (args.Operation is UndoOperation.TransactionUndone)
            {
                TotalLength(lengthDict, categoryDict);
                DisplayLengthUpdate();
            }


            //Identify if elements are created or deleted
            //If elements are created, get their categories and add them to the right lists
            if (PipeAdded(args, transactionName))

            {
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

                //filtering elements to split them by their categories
                try
                {
                    AddElementsToLists(pipes, doc, args, pipeFilter);
                    //UpdateTotalLengths();
                    //UpdateLengths();
                    TotalLength(lengthDict, categoryDict);
                    DisplayLengthUpdate();
                }
                catch (Exception)
                {
                    //ignore
                }

                //ElementId newPipeID = args.GetAddedElementIds(pipeFilter).First();
                //ElementId newTrayID = args.GetAddedElementIds(trayFilter).First();
                //ElementId newDuctID = args.GetAddedElementIds(ductFilter).First();

                //Element newPipe = doc.GetElement(newPipeID);
                //Element newTray = doc.GetElement(newTrayID);
                //Element newDuct = doc.GetElement(newDuctID);

            }
            else if (TrayAdded(args, transactionName))
            {
                try
                {
                    AddElementsToLists(trays, doc, args, trayFilter);
                    UpdateTotalLengths();
                    DisplayLengthUpdate();
                }
                catch (Exception)
                {
                }

            }
            else if (DuctAdded(args, transactionName))
            {
                try
                {
                    AddElementsToLists(ducts, doc, args, ductFilter);
                    UpdateTotalLengths();
                    DisplayLengthUpdate();
                }
                catch (Exception)
                {
                }

            }

            //If elements are deleted, remove them from the list
            else if (SelectionDeleted(args, transactionName))
            {
                //try
                //{
                //    //ids of all deleted elements
                //    ICollection<ElementId> deletedIDs = args.GetDeletedElementIds();

                //    ////new list that will store all deleted elements
                //    //List<Element> deletedElements = new List<Element>();

                //    ////add all deleted elements to the new list
                //    //foreach (ElementId id in deletedIDs)
                //    //{
                //    //    deletedElements.Add(doc.GetElement(id));

                //    //}

                //    //foreach (ElementId id in deletedIDs)
                //    //{
                //    //    if (allElementIDs.Contains(id))
                //    //    {
                //    //        try
                //    //        {
                //    //            foreach (Element pipe in pipes)
                //    //            {
                //    //                if (id == pipe.Id)
                //    //                {
                //    //                    pipes.Remove(pipe);
                //    //                }
                //    //            }

                //    //            foreach (Element tray in trays)
                //    //            {
                //    //                if (id == tray.Id)
                //    //                {
                //    //                    trays.Remove(tray);
                //    //                }
                //    //            }

                //    //        }
                //    //        catch (Exception)
                //    //        {
                //    //        }

                //    //    }

                //    //}

                //    //foreach (Element deletedElement in deletedElements)
                //    //{

                //    //    if ((BuiltInCategory)deletedElement.Category.Id.IntegerValue is BuiltInCategory.OST_PipeCurves)
                //    //    {
                //    //        pipes.Remove(deletedElement);
                //    //    }

                //    //    else if ((BuiltInCategory)deletedElement.Category.Id.IntegerValue is BuiltInCategory.OST_CableTray)
                //    //    {
                //    //        trays.Remove(deletedElement);
                //    //    }

                //    //    else if ((BuiltInCategory)deletedElement.Category.Id.IntegerValue is BuiltInCategory.OST_DuctCurves)
                //    //    {
                //    //        ducts.Remove(deletedElement);
                //    //    }
                //    //}

                //}//end of try

                //catch (Exception)
                //{
                //}

                //dictionary test

                //ids of all deleted elements
                ICollection<ElementId> deletedIDs = args.GetDeletedElementIds();

                foreach (ElementId deletedID in deletedIDs)
                {
                    if (lengthDict.ContainsKey(deletedID))
                    {
                        lengthDict.Remove(deletedID);
                        categoryDict.Remove(deletedID);
                    }
                }


                //UpdateTotalLengths();
                //UpdateLengths();
                TotalLength(lengthDict, categoryDict);
                DisplayLengthUpdate();

            }





        }//end of element changed event handler method

        
        
        



        public bool PipeAdded(DocumentChangedEventArgs args, string transactionName)
        {
            if (transactionName == "Pipe")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TrayAdded(DocumentChangedEventArgs args, string transactionName)
        {
            if (transactionName == "Cable Tray")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DuctAdded(DocumentChangedEventArgs args, string transactionName)
        {
            if (transactionName == "Duct")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SelectionDeleted(DocumentChangedEventArgs args, string transactionName)
        {
            if (transactionName == "Delete Selection")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public bool ElementsAreAdded(DocumentChangedEventArgs args)
        //{
        //    if (args.GetAddedElementIds() != null)    
        //    //if (args.GetAddedElementIds().Count > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public bool ElementsAreDeleted(DocumentChangedEventArgs args)
        //{
        //    if (args.GetDeletedElementIds() != null)
        //    //if (args.GetDeletedElementIds().Count > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        
        public void AddElementsToLists(List<Element> elements ,Document doc, DocumentChangedEventArgs args, ElementFilter filter)
        {
            ICollection<ElementId> elementIDs = args.GetAddedElementIds(filter);

            foreach (ElementId elementID in elementIDs)
            {
                Element newElement = doc.GetElement(elementID);
                elements.Add(newElement);
                allElementIDs.Add(elementID);

                //dictionary test
                double newElementLength = newElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                lengthDict.Add(elementID, newElementLength);
                categoryDict.Add(elementID, (BuiltInCategory)newElement.Category.Id.IntegerValue);
            }
        }

        //public void RemoveElementsFromLists(ICollection<ElementId> elementIDs, List<Element> elements, Document doc)
        //{
        //    foreach (ElementId elementID in elementIDs)
        //    {
        //        Element deletedElement = doc.GetElement(elementID);
        //        elements.Remove(deletedElement);
        //    }
        //}

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
                double lengthInFeet = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                length += FootToMm(lengthInFeet);
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

        public void DisplayLengthUpdate()
        {
            //TaskDialog.Show("New pipe", "Length of pipe: " + string.Format("{0:F2}", pipeLength/1000) + "mm\n" + "Total length modelled: " + string.Format("{0:F2}", pipeTotalLength/1000) + "mm");
            TaskDialog.Show("Total lengths", "Length of pipes: " + string.Format("{0:F2}", pipeTotalLength / 1000) + "m\n" + "Length of trays: " + string.Format("{0:F2}", trayTotalLength / 1000) + "m\n" + "Length of ducts: " + string.Format("{0:F2}", ductTotalLength / 1000) + "m\n");
        }


        public void TotalLength(Dictionary<ElementId, double> lengthDict, Dictionary<ElementId, BuiltInCategory> categoryDict)
        {
            double lengthInMm = 0;
            double lengthInFeet = 0;

            pipeTotalLength = 0;
            trayTotalLength = 0;
            ductTotalLength = 0;

            foreach (var key in lengthDict)
            {
                lengthInFeet += key.Value;
                lengthInMm += FootToMm(lengthInFeet);

                if (categoryDict[key.Key] is BuiltInCategory.OST_PipeCurves)
                {
                    pipeTotalLength += lengthInMm;
                }

                else if (categoryDict[key.Key] is BuiltInCategory.OST_CableTray)
                {
                    trayTotalLength += lengthInMm;
                }

                else if (categoryDict[key.Key] is BuiltInCategory.OST_DuctCurves)
                {
                    ductTotalLength += lengthInMm;
                }

            }
            //return lengthInMm;
        }



        //public void UpdateLengths()
        //{
        //    pipeTotalLength = TotalLength(lengthDict);
        //    //trayTotalLength = TotalLength(trays);
        //    //ductTotalLength = TotalLength(ducts);
        //}









    }//end of class

}//end of namespace
