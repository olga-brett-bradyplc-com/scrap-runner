using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Brady.ScrapRunner.DataService.Mappings
{
 
    public class TripTypeBasicDetailsMap : ClassMapping<TripTypeBasicDetails>
    {
        public TripTypeBasicDetailsMap()
        {
            Table("TripTypeBasicDetails");

            Id(x => x.SeqNo, m =>
            {
                m.UnsavedValue(0);
                m.Generator(Generators.Identity);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("SeqNo");
                m.Insert(false);
                m.Update(false);
                m.Generated(PropertyGeneration.Never);
            });

            Property(x => x.TripTypeCode);
            Property(x => x.ContainerType);
            Property(x => x.ContainerSize);
            Property(x => x.FirstCTRTime);
            Property(x => x.SecondCTRTime);
        }
    }
}
