using AutoMapper;
using Brady.ScrapRunner.DataService.Validators;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Core.Abstract;
using BWF.DataServices.Core.Concrete.ChangeSets;
using BWF.DataServices.Metadata;
using BWF.DataServices.Metadata.Attributes.Actions;
using BWF.DataServices.Support.NHibernate.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.RecordTypes
{

    [CreateAction("ContainerMaster")]
    [EditAction("ContainerMaster")]
    [DeleteAction("ContainerMaster")]
    public class ContainerMasterRecordType :
        ChangeableRecordType<ContainerMaster, string, ContainerMasterValidator, ContainerMasterDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<ContainerMaster, ContainerMaster>();
        }

        public override ContainerMaster GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new ContainerMaster
            {
                ContainerNumber = identityValues[0]
            };
        }

        public override Expression<Func<ContainerMaster, bool>> GetIdentityPredicate(ContainerMaster item)
        {
            return x => x.ContainerNumber == item.ContainerNumber;
        }

        public override Expression<Func<ContainerMaster, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ContainerNumber == identityValues[0];
        }

    }
}