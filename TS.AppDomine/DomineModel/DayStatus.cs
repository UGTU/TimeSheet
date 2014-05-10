namespace TS.AppDomine.DomineModel
{
    public class DayStatus
    {
        public int Id { get; set; }
        public string SmallDayStatusName { get; set; }
        public string DayStatusName { get; set; }
        public bool IsVisible { get; set; }

        public override bool Equals(object obj)
        {
            //If parameter is bull or cannot be cast to Point return false.
            var p = obj != null ? obj as DayStatus : null;
            if (p == null)
            {
                return false;
            }
            // Return true if the fields match:
            //return (Id == p.Id) && (SmallDayStatusName == p.SmallDayStatusName) && (DayStatusName == p.DayStatusName);
            return Id == p.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
