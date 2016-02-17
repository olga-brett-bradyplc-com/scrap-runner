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
    [CreateAction("ContainerHistory")]
    [EditAction("ContainerHistory")]
    [DeleteAction("ContainerHistory")]
    public class ContainerHistoryRecordType :
    ChangeableRecordType<ContainerHistory, string, ContainerHistoryValidator, ContainerHistoryDeletionValidator>
    {
        public override void ConfigureMapper()
        {
            var mapping = Mapper.CreateMap<ContainerHistory, ContainerHistory>();
        }

        public override ContainerHistory GetIdentityObject(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return new ContainerHistory
            {
                ContainerNumber = identityValues[0],
                ContainerSeqNumber = int.Parse(identityValues[1])
            };
        }

        public override Expression<Func<ContainerHistory, bool>> GetIdentityPredicate(ContainerHistory item)
        {
            return x => x.ContainerNumber == item.ContainerNumber &&
                        x.ContainerSeqNumber == item.ContainerSeqNumber;
        }

        public override Expression<Func<ContainerHistory, bool>> GetIdentityPredicate(string id)
        {
            var identityValues = TypeMetadataInternal.GetIdentityValues(id);
            return x => x.ContainerNumber == identityValues[0] &&
                        x.ContainerSeqNumber == int.Parse(identityValues[1]);
        }

    }

}
