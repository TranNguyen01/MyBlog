using System.IO;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface IMinIOService
    {
        Task PutProject(string bucketName, Stream fileStream, string fileName, string contentType);
        Task<MemoryStream> GetObject(string bucket, string objectName);
        Task<string> PresignedGetObject(string bucket, string objectName);
    }
}
