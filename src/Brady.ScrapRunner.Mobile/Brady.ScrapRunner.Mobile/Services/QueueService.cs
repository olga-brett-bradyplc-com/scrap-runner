namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Interfaces;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.Metadata.Models;
    using Interfaces;
    using Models;
    using MvvmCross.Platform;
    using Newtonsoft.Json;

    public class QueueService : IQueueService
    {
        private readonly IRepository<QueueItemModel> _repository;
        private readonly IConnectionService _connectionService;

        public QueueService(
            IRepository<QueueItemModel> repository, 
            IConnectionService connectionService)
        {
            _repository = repository;
            _connectionService = connectionService;
        }

        public Task EnqueueItemAsync(QueueItemModel queueItem)
        {
            return _repository.InsertAsync(queueItem);
        }

        public async Task ProcessQueueAsync()
        {
            while (await _repository.AsQueryable().CountAsync() > 0)
            {
                var queueItem = await _repository.AsQueryable().FirstOrDefaultAsync();
                if (queueItem.Verb == QueueItemVerb.Delete)
                {
                    // Delete can't be used with a ChangeSet for now.
                    // A ChangeSet requires the type and id.
                    // The IDataServiceClient.DeleteAsync methods only provide an id.
                    dynamic deleteId = JsonConvert.DeserializeObject(queueItem.SerializedId,
                        Type.GetType(queueItem.IdType));
                    var deleteResult = await _connectionService.GetConnection().DeleteAsync<dynamic>(
                        deleteId, queueItem.DataService);
                    var deleteChangeResult = deleteResult as ChangeResult;
                    if (deleteChangeResult != null && deleteChangeResult.WasSuccessful)
                        Mvx.Trace($"Deleted {queueItem.SerializedId}");
                    else
                        Mvx.Warning($"Failed to delete {queueItem.SerializedId} ({deleteChangeResult?.Failure.Summary})");
                    await _repository.DeleteAsync(queueItem);
                }
                else
                {
                    // Create and Update can be batched together as one ChangeSet.
                    var queueItems = await _repository.ToListAsync(q => q.RecordType == queueItem.RecordType &&
                        q.Verb == QueueItemVerb.Create || q.Verb == QueueItemVerb.Update);

                    var objectType = Type.GetType(queueItem.RecordType);
                    var idType = Type.GetType(queueItem.IdType);
                    var changeSet = MakeChangeSetForType(idType, objectType);
                    var noRoleIds = Enumerable.Empty<long>().ToList();
                    var createReference = 0L;
                    var updateReferences = new List<object>();

                    foreach (var remainingQueueItem in queueItems)
                    {
                        var deserializedObject = JsonConvert.DeserializeObject(queueItem.SerializedRecord, objectType);
                        switch (remainingQueueItem.Verb)
                        {
                            case QueueItemVerb.Create:
                                Mvx.Trace($"Adding ChangeSet create {createReference}");
                                changeSet.AddCreate(createReference++, deserializedObject, noRoleIds, noRoleIds);
                                break;
                            case QueueItemVerb.Update:
                                var deserializedId = JsonConvert.DeserializeObject(queueItem.SerializedId, idType);
                                updateReferences.Add(deserializedId);
                                Mvx.Trace($"Adding ChangeSet update {deserializedId}");
                                changeSet.AddUpdate(deserializedId, deserializedObject);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    var changeSetResult = await _connectionService.GetConnection().ProcessChangeSetAsync(
                        changeSet, queueItem.DataService);
                    if (WasChangeSetSuccessful(changeSetResult, createReference, updateReferences))
                        Mvx.Trace($"Successfuly processed change set {queueItem.IdType} {queueItem.RecordType}");
                    else
                        Mvx.Warning($"Failed processing change set {queueItem.IdType} {queueItem.RecordType}");
                    foreach (var itemToDelete in queueItems)
                    {
                        await _repository.DeleteAsync(itemToDelete);
                    }
                }
            }
        }

        public async Task<bool> IsEmptyAsync()
        {
                return await _repository.AsQueryable().CountAsync() > 0;
        }

        private IChangeSet MakeChangeSetForType(Type idType, Type itemType)
        {
            var genericChangeSetType = typeof(ChangeSet<,>);
            Type[] typeArgs = { idType, itemType };
            var typedChangeSetType = genericChangeSetType.MakeGenericType(typeArgs);
            var changeSet = Activator.CreateInstance(typedChangeSetType);
            return (IChangeSet)changeSet;
        }


        private bool WasChangeSetSuccessful(IChangeSetResult changeSetResult, long createReference, IEnumerable<object> updateIds)
        {
            for (var reference = 0L; reference < createReference; reference++)
            {
                var failure = changeSetResult.GetFailedCreateForRef(reference);
                if (failure != null)
                {
                    Mvx.Warning($"ChangeSet create for type {failure.Type} failed ({failure.Summary})");
                    return false;
                }
            }
            foreach (var updateId in updateIds)
            {
                var failure = changeSetResult.GetFailedUpdateForId(updateId);
                if (failure != null)
                {
                    Mvx.Warning($"ChangeSet update for type {failure.Type} failed ({failure.Summary})");
                    return false;
                }
            }
            return true;
        }
    }
}