namespace Brady.ScrapRunner.Mobile.Helpers
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using BWF.DataServices.Domain.Interfaces;
    using BWF.DataServices.Domain.Models;
    using BWF.DataServices.PortableClients.Interfaces;

    public static class ChangeSetExtensions
    {
        public static async Task<IChangeSetResult> ProcessChangeSetAsync(
               this IDataServiceClient client,
               IChangeSet changeSet,
               string dataService)
        {
            var types = changeSet.GetType().GenericTypeArguments;
            var method = typeof(IDataServiceClient).GetTypeInfo().GetDeclaredMethod("ProcessChangeSetAsync");
            var genericMethod = method.MakeGenericMethod(types);
            object[] arguments = { changeSet, dataService };
            var result = await (dynamic)(genericMethod.Invoke(client, arguments));
            return result;
        }

        public static IChangeSet MakeChangeSetForType(Type idType, Type itemType)
        {
            var genericChangeSetType = typeof(ChangeSet<,>);
            Type[] typeArgs = { idType, itemType };
            var typedChangeSetType = genericChangeSetType.MakeGenericType(typeArgs);
            var changeSet = Activator.CreateInstance(typedChangeSetType);
            return (IChangeSet)changeSet;
        }
    }
}
