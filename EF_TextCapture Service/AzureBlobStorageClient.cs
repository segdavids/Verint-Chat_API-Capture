using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;

namespace EF_TextCapture_Service
{
    public class AzureBlobStorageClient
    {
        public static async Task UploadBloab(string filename, string filepath, string converId)
        {
            var connectionstring = ConfigurationManager.ConnectionStrings["AzureBlob"].ConnectionString;
            string containername = "blobcontainer";
            var blobServiceClient = new BlobServiceClient(connectionstring);
            var blobcontainerclient = blobServiceClient.GetBlobContainerClient(containername);
            var blobclient = blobcontainerclient.GetBlobClient(filename);
            Program.logerror("Uploading started for Conversation_Id: " + converId + "");
            var stream = File.OpenRead(filepath);
            await blobclient.UploadAsync(stream);
            Program.logerror("Upload done for Conversation_Id: " + converId + "");
            stream.Close();
            if (File.Exists(filepath))
            {
                // If file found, delete it    
                File.Delete(filepath);
                Program.logerror("Upload Stream closed and cleared for Conversation_Id: " + converId + "");


            }
        }
    }
}
