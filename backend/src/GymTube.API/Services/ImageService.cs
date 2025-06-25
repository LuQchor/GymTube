using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GymTube.API.Services
{
    public class ImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(IConfiguration configuration)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var cloudName = cloudinaryConfig["CloudName"];
            var apiKey = cloudinaryConfig["ApiKey"];
            var apiSecret = cloudinaryConfig["ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing. Please check appsettings.json");
            }

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        public async Task<string?> SaveProfileImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            // Provjeri tip datoteke
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return null;

            // Provjeri veličinu (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return null;

            try
            {
                // Upload na Cloudinary
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = "gymtube/profile-images",
                    Transformation = new Transformation()
                        .Width(400)
                        .Height(400)
                        .Crop("fill")
                        .Gravity("face")
                        .Quality("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                return result.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                // Handle upload error
                throw new Exception($"Failed to upload image to Cloudinary: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProfileImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                // Izvuci public ID iz URL-a
                var publicId = ExtractPublicIdFromUrl(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                // Handle deletion error
                throw new Exception($"Failed to delete image from Cloudinary: {ex.Message}");
            }
        }

        private string? ExtractPublicIdFromUrl(string url)
        {
            try
            {
                // Cloudinary URL format: https://res.cloudinary.com/cloud-name/image/upload/v1234567890/folder/filename.jpg
                var uri = new Uri(url);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                // Traži "upload" segment i uzmi sve nakon njega
                var uploadIndex = Array.IndexOf(pathSegments, "upload");
                if (uploadIndex >= 0 && uploadIndex + 2 < pathSegments.Length)
                {
                    // Preskoči version segment (v1234567890) i uzmi ostatak
                    var publicIdSegments = pathSegments.Skip(uploadIndex + 2).ToArray();
                    return string.Join("/", publicIdSegments).Replace(".jpg", "").Replace(".png", "").Replace(".gif", "");
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
} 