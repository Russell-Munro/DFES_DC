using GraphQL.Types;
using UDC.Common.Data.Models;

namespace DataConnectorUI.GraphQL.Types
{
    public class SyncContainerType : ObjectGraphType<SyncContainer>
    {
        public SyncContainerType()
        {
            Field(x => x.Id, type: typeof(StringGraphType));
            Field(x => x.parentId, type: typeof(StringGraphType));
            Field(x => x.Name, type: typeof(StringGraphType));
            Field(x => x.Parent, type: typeof(SyncContainerType));
            Field(x => x.SyncContainers, type: typeof(ListGraphType<SyncContainerType>));
        }
    }
}