using Brady.ScrapRunner.Domain.Process;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// A (trivial) TripInfoProcess mapping via Employee query to NHibernate.
    /// </summary>
    public class TripInfoProcessMap : ClassMapping<TripInfoProcess>
    {
        public TripInfoProcessMap()
        {
            Table("EmployeeMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
            });

        }
    }
}