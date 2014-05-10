using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS.AppDomine.DomineModel;

namespace TS.AppDomine.Abstract
{
    interface ITimeSheetGenerator
    {
        /// <summary>
        /// Генерирует табель для переданных работников
        /// </summary>
        /// <param name="employees">Работники для которых надо сгенерировать табель, в слцчае null генерируется для всех работников отдела</param>
        void GenerateTimeSheet(IEnumerable<Employee> employees = null);
        /// <summary>
        /// Сохраняет табель в хранилище
        /// </summary>
        /// <param name="saveAsFake">Если передать true то табель юудет сохранён как ложный</param>
        /// <returns></returns>
        BaseTimeSheet Save(bool saveAsFake);
    }
}
