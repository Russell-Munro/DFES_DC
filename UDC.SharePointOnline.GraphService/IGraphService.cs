using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    public interface IGraphService
    {
        Task<IEnumerable<Dictionary<string, object>>> GetListsAsync();

        Task<Dictionary<string, object>> GetListAsync(Guid id);

        Task<IEnumerable<Dictionary<string, object>>> GetDocumentsAsync(Guid listId, IEnumerable<string> fields);

        Task<IEnumerable<Dictionary<string, object>>> GetTermSetsAsync();

        Task<IEnumerable<Dictionary<string, object>>> GetTermsAsync(Guid termSetId);
    }
}
