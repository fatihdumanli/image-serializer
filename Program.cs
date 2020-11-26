using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace image_serializer
{
    class Program
    {
        static async Task Main(string[] args)
        {
           
            MemoryStream ms = new MemoryStream();
            var input = File.OpenWrite(@"C:\\Users\\Fatih\\source\\repos\\image-serializer\\file2.jpg");
            
            const int chunkSize = 506; 
            using (var file = File.OpenRead(@"C:\\Users\\Fatih\\source\\repos\\image-serializer\\25thnov2020TestImg.txt"))
            {
                int bytesRead;
                var buffer = new byte[chunkSize];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
            }

            BlobContainerClient container = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=fatihteststorage;AccountKey=N01DqDtMWIjzyGiSf2e3mBNJcqvGqBnM7lD6SSKjIT6/WbqT8ecMng5IYyH9WsST3iLbuoEGjHHACR9zoO51Uw==;EndpointSuffix=core.windows.net", "main");


            var blockBlobClient = container.GetBlockBlobClient("myImage.png");
            int blockSize = 50 * 1024;
            int offset = 0;
            int counter = 0;
            List<string> blockIds = new List<string>();


            using(var fs = File.OpenRead(@"C:\\Users\\Fatih\\source\\repos\\image-serializer\\test-image.png"))
            {
                var bytesRemaining = fs.Length;

                do
                {
                    var dataToRead = Math.Min(bytesRemaining, blockSize);
                    byte[] data = new byte[dataToRead];

                    var dataRead = fs.Read(data, offset, (int)dataToRead);
                    bytesRemaining -= dataRead;

                    if (dataRead > 0)
                    {
                        var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(counter.ToString("d6")));
                        blockBlobClient.StageBlock(blockId, new MemoryStream(data));
                        Console.WriteLine(string.Format("Block {0} uploaded successfully.", counter.ToString("d6")));
                        blockIds.Add(blockId);
                        counter++;
                    }
                } while (bytesRemaining > 0);

                var headers = new BlobHttpHeaders()
                {
                    ContentType = "image/png"
                };
                blockBlobClient.CommitBlockList(blockIds, headers);
            }


            try
            {
                // Get a reference to a blob
                BlobClient blob = container.GetBlobClient("test.png");

                ms.Position = 0;
                // Open the file and upload its data
                await blob.UploadAsync(ms);


                // Verify we uploaded some content
                BlobProperties properties = await blob.GetPropertiesAsync();
            }

            catch(Exception e)
            {

            }
            finally
            {
                // Clean up after the test when we're finished
            }

        }
    }
}
