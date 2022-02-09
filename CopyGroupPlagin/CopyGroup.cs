using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                GroupPickedFilter groupPickedFilter = new GroupPickedFilter(); // создаем экземпляр класса, чтобы передать его вторым аргументом в PickObject
                //для реализации метода PickObject
                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, groupPickedFilter, "Выберите группу объектов"); //получили ссылку на выбранный пользователем объект
                Element element = doc.GetElement(reference);// из ссылки получаем объект типа Element
                Group group = element as Group;//преобразуем объект из базового типа Element в тип Group
                XYZ groupCenter = GetElementCenter(group);
                Room room = GetRoomByPoint(doc, groupCenter);//комната, в кот. находится выбранная группа
                XYZ roomCenter = GetElementCenter(room);// находим центр этой комнаты
                XYZ offset = groupCenter - roomCenter;//смещение центра группы относительно центра группы

                //выбираем точку, в которую хотим скопировать объект
                XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

                Room roomInsert = GetRoomByPoint(doc, point);//комната, в кот. находится указанная точка
                XYZ roomInsertCenter = GetElementCenter(roomInsert);// находим центр этой комнаты
                XYZ poinInsert = roomInsertCenter + offset;

                //вставка группы в указанную точку
                Transaction transaction = new Transaction(doc);
                transaction.Start("Копирование группы объектов");

                doc.Create.PlaceGroup(poinInsert, group.GroupType);

                transaction.Commit();
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)// исключения по нажатию Esc
            {
                return Result.Cancelled;
            }
            catch(Exception ex)// все остальные исключения
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        //метод для определения центра объекта по BoundingBox
        public XYZ GetElementCenter(Element element)
        {
           BoundingBoxXYZ bounding= element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }

        //метод определяет комнату по точке
        public Room GetRoomByPoint(Document doc, XYZ point)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);//отбираем все комнаты
            foreach (Element e in collector)
            {
                Room room = e as Room;//приводим элемент к нужному типу
                if (room!=null)//если преобразование прошло успешно
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
            }
            return null;
        }
    }
    //фильтр для выбора именно групп
    public class GroupPickedFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                return true;
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false; //т.к. ссылки нас не интересуют
        }
    }
}
