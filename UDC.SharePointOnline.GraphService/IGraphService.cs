using System.IO;
using System.Threading.Tasks;

namespace Equ.SharePoint.GraphService
{
    public interface IGraphService
    {
        Task<bool> ExistsAsync(string path);
        Task DeleteAsync(string path);
        Task<Stream> DownloadAsync(string path);
        Task UploadAsync(string path, Stream stream);
    }
}
