using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlagin
{
    [Transaction(TransactionMode.Manual)]//если нужно внести изменения в модель
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            //для реализации метода PickObject
            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Выберите группу объектов"); //получили ссылку на выбранный пользователем объект
            Element element = doc.GetElement(reference);// из ссылки получаем объект типа Element
            Group group = element as Group;//преобразуем объект из базового типа Element в тип Group

            //выбираем точку, в которую хотим скопировать объект
            XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

            //вставка группы в указанную точку

            Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы объектов");

            doc.Create.PlaceGroup(point, group.GroupType);

            transaction.Commit();
            return Result.Succeeded;
        }
    }
}
