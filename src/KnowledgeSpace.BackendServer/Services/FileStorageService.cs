using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace KnowledgeSpace.BackendServer.Services
{
    public class FileStorageService : IStorageService
    {
        private readonly string _userContentFolder;
        private const string USER_CONTENT_FOLDER_NAME = "user-attachments";

        public FileStorageService(IWebHostEnvironment WebHostEnvironment)
        {
            _userContentFolder = Path.Combine(WebHostEnvironment.WebRootPath, USER_CONTENT_FOLDER_NAME);
        }
        public string GetFileUrl(string fileName)
        {
            return $"/{USER_CONTENT_FOLDER_NAME}/{fileName}";
        }
        public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
        {
            if (!Directory.Exists(_userContentFolder))
                Directory.CreateDirectory(_userContentFolder);
            var filePath = Path.Combine(fileName, USER_CONTENT_FOLDER_NAME);
            using var output = new FileStream(filePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(fileName , USER_CONTENT_FOLDER_NAME);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}
