using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    public interface IGraphService
    {
        Task<Dictionary<string, object>> GetItemAsync(string itemId);
        Task<IList<Dictionary<string, object>>> GetChildrenAsync(string itemId = null);
    }
}

