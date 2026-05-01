namespace MedAppApi
{
    public class MedTable
    {
        public int ID               { get; set; }
        public string Description   { get; set; }
        public DateTime MedDate     { get; set; }
        public Boolean  am          { get; set; }
        public Boolean  pm          { get; set; }
    }
}
