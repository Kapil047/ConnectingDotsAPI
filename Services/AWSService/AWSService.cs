using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ConnectingDotsAPI.DBModels;
using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.CacheService;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Mail;
using System.Security.Cryptography.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConnectingDotsAPI.Services.AwsService
{
    public class AWSService(ICacheService cacheService, ConnectingDotsDbContext db) : IAWSService
    {
        private readonly ICacheService cacheService = cacheService;
        private readonly ConnectingDotsDbContext db = db;
        //private readonly string accessKey = "AKIAR7NQIN52G7D36PKM";
        //private readonly string secretKey = "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";
        //private readonly string bucketName = "ensuenotech-test";


        public async Task<FileModel.FileDetails> UploadBlob(string fileName, byte[] data, bool regenerateName)
        {
            try
            {
                // Set up the AWS credentials and region
                var credentials = new Amazon.Runtime.BasicAWSCredentials(AccessKey(), SecretKey());
                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Region()) // Change to your desired region
                };

                // Create an instance of the S3 client
                using var client = new AmazonS3Client(credentials, config);
                // Create a PutObjectRequest
                var request = new PutObjectRequest
                {
                    BucketName = Bucket(),
                    Key = fileName,
                    InputStream = new MemoryStream(data),
                };
                var fileInfo = new FileInfo(fileName);
                switch (fileInfo.Extension.Replace(".", "").ToLower())
                {
                    case "jpg":
                    case "jpeg":
                        request.ContentType = "image/jpeg";
                        break;
                    case "gif":
                        request.ContentType = "image/gif";
                        break;
                    case "png":
                        request.ContentType = "image/png";
                        break;
                    case "bmp":
                        request.ContentType = "image/bmp";
                        break;
                    case "pdf":
                        request.ContentType = "application/pdf";
                        break;
                    case "doc":
                    case "docx":
                        request.ContentType = "application/vnd.ms-word";
                        break;
                    case "xls":
                    case "xlsx":
                        request.ContentType = "application/vnd.ms-excel";
                        break;
                }
                if (regenerateName)
                {
                    request.Key = Guid.NewGuid().ToString() + fileName.Remove(0, fileName.LastIndexOf("."));
                }
                // Upload the byte array
                var _ = await client.PutObjectAsync(request);
                var url = $"https://{Bucket()}.s3.{Region()}.amazonaws.com/{request.Key}";


                return new FileModel.FileDetails { Blob = request.Key, FileName = fileName, Url = url };

            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"Error uploading byte array: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error: {ex.Message}");
            }
        }
        private string AccessKey()
        {
            var key = $"AWSAccessKey";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = db.Settings.FirstOrDefault(x => x.Name == "aws.accesskey") ?? throw new Exception("AWS ACCESS KEY MISSING");
            cacheService.SetValue(key, value.Value, 60);
            return value.Value;
        }
        private string SecretKey()
        {
            var key = $"AWSSecretKey";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = db.Settings.FirstOrDefault(x => x.Name == "aws.secretkey") ?? throw new Exception("AWS SECRET KEY MISSING");
            cacheService.SetValue(key, value.Value, 60);
            return value.Value;
        }
        private string Region()
        {
            var key = $"AWSRegion";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = db.Settings.FirstOrDefault(x => x.Name == "aws.region") ?? throw new Exception("AWS REGION MISSING");
            cacheService.SetValue(key, value.Value, 60);
            return value.Value;
        }
        private string Bucket()
        {
            var key = $"AWSBucket";
            var cacheValue = cacheService.GetValue(key);
            if (!string.IsNullOrEmpty(cacheValue))
                return cacheValue;
            var value = db.Settings.FirstOrDefault(x => x.Name == "aws.bucket") ?? throw new Exception("AWS BUCKET MISSING");
            cacheService.SetValue(key, value.Value, 60);
            return value.Value;
        }
    }
}
