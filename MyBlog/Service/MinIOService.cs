using Minio;
using MyBlog.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MotoBlog.Services
{
    public class MinIOService : IMinIOService
    {
        private MinioClient _minioClient;
        public MinIOService()
        {
            _minioClient = new MinioClient()
            .WithEndpoint("127.0.0.1:9000/")
            .WithCredentials("NIJX6yfgXEiKsoEq",
            "EjRyapLWxpnf3bRtFwR5ahPcNI3uFbpd")
            .Build();
        }

        public async Task PutProject(string bucketName, Stream fileStream, string fileName, string contentType)
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
            {
                // if bucket not Exists,make bucket
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }
            var filename = Guid.NewGuid();
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(fileStream)
                .WithContentType(contentType)
                .WithObjectSize(fileStream.Length)

                );
        }


        public async Task<MemoryStream> GetObject(string bucket, string objectName)
        {

            try
            {
                MemoryStream destination = new MemoryStream();

                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(bucket)
                                                    .WithObject(objectName);
                await _minioClient.StatObjectAsync(statObjectArgs);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                                                  .WithBucket(bucket)
                                                  .WithObject(objectName)
                                                  .WithCallbackStream((stream) =>
                                                  {
                                                      stream.CopyTo(destination);
                                                  });
                await _minioClient.GetObjectAsync(getObjectArgs);
                destination.Position = 0;
                return destination;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<string> PresignedGetObject(string bucket, string objectName)
        {
            StatObjectArgs statObjectArgs = new StatObjectArgs()
                                    .WithBucket(bucket)
                                    .WithObject(objectName);

            await _minioClient.StatObjectAsync(statObjectArgs);

            PresignedGetObjectArgs presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithExpiry(60);
            return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);

        }
    }
}
