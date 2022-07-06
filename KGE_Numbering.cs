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

namespace API_2021_Plugins
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class KGE_Numbering : IExternalCommand
    {
        static Dictionary<ElementId, Element> pipesDict = new Dictionary<ElementId, Element>();
        static Dictionary<ElementId, List<XYZ>> endpointsDict = new Dictionary<ElementId, List<XYZ>>();
        static Dictionary<ElementId, List<double>> distancesDict = new Dictionary<ElementId, List<double>>();
        static ElementId closestPipeId = null;
        static XYZ closestEndpoint = null;
        static XYZ otherEndpoint = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            UIApplication uiApp = commandData.Application;
            //Document doc = uiApp.ActiveUIDocument.Document;
            Application app = uiApp.Application;

            //Create Filters
            ElementCategoryFilter pipesFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);

            //Name of the numbering parameter
            string numberingParameterName = "Identifier";

            //Collect pipes
            IList<Element> pipes = new FilteredElementCollector(doc).WherePasses(pipesFilter).WhereElementIsNotElementType().ToElements();

            //list with elements belonging to the same system as the picked one
            //List<Element> allElementsInSystem = new List<Element>();

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
                    TaskDialog.Show("Picked Object", $"Element selected: {pickedElement.Category.Name} with ID number {pickedElementId}");

                    string systemAbb = pickedElement.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

                    var allPipesInSystem = from pipe in pipes
                                              where pipe.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString() == systemAbb
                                              select pipe;

                    foreach (Element pipe in allPipesInSystem)
                    {
                        //add pipe to the pipes dictionary
                        pipesDict.Add(pipe.Id, pipe);

                        //get pipe curve and endpoints
                        LocationCurve LC = pipe.Location as LocationCurve;
                        XYZ R1 = null;
                        XYZ R2 = null;
                        R1 = LC.Curve.GetEndPoint(0);
                        R2 = LC.Curve.GetEndPoint(1);

                        //create list with both endpoints
                        List<XYZ> pipeEndpoints = new List<XYZ>();
                        pipeEndpoints.Add(R1);
                        pipeEndpoints.Add(R2);

                        //add endpoint list to the endpoints dictionary
                        endpointsDict.Add(pipe.Id, pipeEndpoints);

                    }//end of foreach


                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start("Pipe numbering");

                        //take picked pipe as current pipe
                        Element currentPipe = pickedElement;

                        //first pipe number
                        int counter = 1;

                        //set identifier parameter with number 001
                        currentPipe.LookupParameter(numberingParameterName).Set(AssignNumber(counter, systemAbb));

                        //choose one of the 2 endpoints as current endpoint
                        List<XYZ> currentEndpointList = endpointsDict[currentPipe.Id];
                        XYZ currentEndpoint = currentEndpointList[0];

                        //delete picked pipe from both dictionaries
                        pipesDict.Remove(currentPipe.Id);
                        endpointsDict.Remove(currentPipe.Id);


                        //start loop through all pipes in pipes dictionary
                        while (pipesDict.Count > 0)
                        {
                            //increment the counter
                            counter++;

                            int pipesLeft = pipesDict.Count;

                            //general list with all distances, no nested lists
                            List<double> flattenedDistances = new List<double>();

                            //calculate distances to the rest of endpoints in the dictionary
                            foreach (var keyValuePair in endpointsDict)
                            {
                                List<double> distancesList = new List<double>();
                                
                                foreach (XYZ endpoint in keyValuePair.Value)
                                {
                                    double distance = 0;
                                    distance = endpoint.DistanceTo(currentEndpoint);

                                    //add it to the distance pair list to build the dictionary
                                    distancesList.Add(distance);

                                    //add to the flattened list to later check the shortest distance
                                    flattenedDistances.Add(distance);
                                }

                                distancesDict.Add(keyValuePair.Key, distancesList);
                            }


                            //find the closest endpoint (minimum value from calculated distances)
                            double minDistance = flattenedDistances.Min();

                            //get the key (element id) to which that endpoint belongs to
                            var closestPipeIdVar = from keyValuePair in distancesDict
                                              where keyValuePair.Value.Contains(minDistance)
                                              select keyValuePair.Key;

                            closestPipeId = closestPipeIdVar.FirstOrDefault();

        

                            //with the element id, find the closest pipe to the current one
                            if (closestPipeId != null)
                            {
                                Element closestPipe = pipesDict[closestPipeId];

                                //get endpoints of closest pipe
                                List<XYZ> closestEndpointPair = endpointsDict[closestPipeId];

                                foreach (XYZ endpoint in closestEndpointPair)
                                {
                                    if (endpoint.DistanceTo(currentEndpoint) == minDistance)
                                    {
                                        closestEndpoint = endpoint;
                                    }

                                    else
                                    {
                                        otherEndpoint = endpoint;
                                    }
                                    
                                }

          
                                //set closest pipe as current pipe
                                currentPipe = closestPipe;

                                //set other endpoint as current endpoint
                                currentEndpoint = otherEndpoint;

                                //set identifier parameter with number 00X
                                currentPipe.LookupParameter(numberingParameterName).Set(AssignNumber(counter, systemAbb));

                                //delete current pipe from all 3 dictionaries
                                pipesDict.Remove(currentPipe.Id);
                                endpointsDict.Remove(currentPipe.Id);
                                distancesDict.Clear();

           
                            }

                            else
                            {
                                break;
                            }

                        }

                        transaction.Commit();


                    }//end of transaction


                }//end of if statement
            }
            catch (Exception e)
            {
                message = e.Message;
                TaskDialog.Show("error", $"{pipesDict.Count()} pipes left to number");
                return Result.Failed;
            }

            return Result.Succeeded;
  

        }//end of Result Execute method


        private string AssignNumber(int counter, string systemAbb)
        {
            if (counter.ToString().Count() == 1)
            {
                return "KGE-" + systemAbb + "-00" + counter.ToString();
            }

            else if (counter.ToString().Count() == 2)
            {
                return "KGE-" + systemAbb + "-0" + counter.ToString();
            }

            else
            {
                return "KGE-" + systemAbb + "-" + counter.ToString();
            }
        }
    }
}
