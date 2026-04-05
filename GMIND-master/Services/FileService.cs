using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Gamingv1.Services
{
    /// <summary>
    /// Service for managing file uploads and deletions
    /// </summary>
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _imagesFolder = "uploads/images";
        private readonly string _gamesFolder = "uploads/games";
        private readonly string _eventsFolder = "uploads/events";
        private readonly string _usersFolder = "uploads/users";

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
            
            // Ensure directories exist
            EnsureDirectoryExists(_imagesFolder);
            EnsureDirectoryExists(_gamesFolder);
            EnsureDirectoryExists(_eventsFolder);
            EnsureDirectoryExists(_usersFolder);
        }

        private void EnsureDirectoryExists(string relativePath)
        {
            string fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        /// <summary>
        /// Upload a game image to the server
        /// </summary>
        public async Task<string> UploadGameImageAsync(IFormFile file)
        {
            return await UploadFileAsync(file, _gamesFolder);
        }

        /// <summary>
        /// Upload an event image to the server
        /// </summary>
        public async Task<string> UploadEventImageAsync(IFormFile file)
        {
            return await UploadFileAsync(file, _eventsFolder);
        }

        /// <summary>
        /// Upload a general image to the server
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            return await UploadFileAsync(file, _imagesFolder);
        }

        /// <summary>
        /// Upload a user profile image to the server
        /// </summary>
        public async Task<string> UploadUserImageAsync(IFormFile file)
        {
            string userImagesFolder = "uploads/users";
            EnsureDirectoryExists(userImagesFolder);
            return await UploadFileAsync(file, userImagesFolder);
        }

        /// <summary>
        /// Upload a file to the specified folder
        /// </summary>
        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(_environment.WebRootPath, folder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative URL
            return $"/{folder}/{fileName}";
        }

        /// <summary>
        /// Delete a file from the server
        /// </summary>
        public bool DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return false;
            }

            try
            {
                // Convert URL to physical path
                string relativePath = fileUrl.TrimStart('/');
                string fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Delete a file from the server
        /// </summary>
        public Task<bool> DeleteFileAsync(string relativePath)
        {
            try
            {
                // Remove the leading slash if present
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }

                string fullPath = Path.Combine(_environment.WebRootPath, relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
