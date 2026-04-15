using Microsoft.AspNetCore.Http;

namespace ColafHotel.Helpers;

public static class FileUploadHelper
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public static async Task<string?> SaveImageAsync(
        IFormFile? file,
        string webRootPath,
        string relativeFolder,
        string? existingFile = null)
    {
        if (file is null || file.Length == 0)
        {
            return existingFile;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only JPG, JPEG, PNG, WEBP, and GIF files are allowed.");
        }

        var absoluteFolder = Path.Combine(webRootPath, relativeFolder);
        Directory.CreateDirectory(absoluteFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, fileName);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        if (!string.IsNullOrWhiteSpace(existingFile))
        {
            var oldAbsolutePath = Path.Combine(webRootPath, existingFile.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(oldAbsolutePath))
            {
                File.Delete(oldAbsolutePath);
            }
        }

        return $"/{relativeFolder.Replace("\\", "/")}/{fileName}";
    }
}
