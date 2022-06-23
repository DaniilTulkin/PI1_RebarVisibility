using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PI1_RebarVisibility
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Type> classList = new List<Type>();
            classList.Add(typeof(Rebar));
            classList.Add(typeof(AreaReinforcement));
            ElementMulticlassFilter classFilter = new ElementMulticlassFilter(classList);

            var rebars = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WherePasses(classFilter)
                .ToList();

            using (Transaction t = new Transaction(doc, "Видимость арматуры"))
            {
                t.Start();

                foreach (var el in rebars)
                {
                    SetVisibility(el, doc);
                }

                t.Commit();
            }

            return Result.Succeeded;
        }

        public void SetVisibility(Element el, Document doc)
        {
            View3D view3D = doc.GetElement(doc.ActiveView.Id) as View3D;

            if (el is Rebar)
            {
                Rebar rebar = el as Rebar;

                bool isSolidInView = rebar.IsSolidInView(view3D);
                rebar.SetSolidInView(view3D, !isSolidInView);
            }
            else if (el is AreaReinforcement)
            {
                AreaReinforcement areaReinforcement = el as AreaReinforcement;
                
                bool isSolidInView = areaReinforcement.IsSolidInView(view3D);
                areaReinforcement.SetSolidInView(view3D, !isSolidInView);
            }
        }

        public static string GetPath()
        {
            return typeof(Command).Namespace + "." + nameof(Command);
        }
    }    
}
