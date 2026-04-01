using NuciDAL.DataObjects;

namespace NuciCraft.API.DataAccess.DataObjects
{
    public class NuciCraftEntityBase : EntityBase
    {
        public string CreatedDT { get; set; }

        public string UpdatedDT { get; set; }
    }
}
