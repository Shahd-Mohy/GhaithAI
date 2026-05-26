namespace GhaithAI.API.Helpers
{
    public static class FileUploadHelper
    {
        public static string GenerateFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            return $"{Guid.NewGuid()}{extension}";
        }
    }
}