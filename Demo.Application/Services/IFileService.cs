namespace Demo.Application.Services
{
    public interface IFileService
    {
        string Upsert(string rootFolder, string filePath, string base64FileContent);
        string UpsertImage(string rootFolder, string filePath, Stream fileStream);
        string ResizeImageJpeg(Stream stream, int w, int h, string rootFolder, string filePath);
        void Delete(string path);
    }
}
