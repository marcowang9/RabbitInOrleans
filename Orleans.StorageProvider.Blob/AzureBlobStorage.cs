namespace Orleans.StorageProvider.Blob
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Newtonsoft.Json;
    using Orleans.Providers;
    using Orleans.Runtime;
    using System.Collections.Generic;
    using Orleans.Runtime.Configuration;
    using Orleans.Storage;
    using System.Runtime.Serialization.Formatters;
    //copy from: https://github.com/dotnet/orleans/blob/master/src/OrleansAzureUtils/Providers/Storage/AzureBlobStorage.cs
    public class AzureBlobStorage : IStorageProvider
    {
        private JsonSerializerSettings jsonSettings;

        private CloudBlobContainer container;

        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Log = providerRuntime.GetLogger("Storage.AzureBlobStorage");

            try
            {
                this.Name = name;
                this.jsonSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    Formatting = Formatting.None
                };
                jsonSettings.ContractResolver = new PrivateContractResolver();

                if (!config.Properties.ContainsKey("DataConnectionString")) throw new BadProviderConfigException("The DataConnectionString setting has not been configured in the cloud role. Please add a DataConnectionString setting with a valid Azure Storage connection string.");

                var account = CloudStorageAccount.Parse(config.Properties["DataConnectionString"]);
                var blobClient = account.CreateCloudBlobClient();
                var containerName = config.Properties.ContainsKey("ContainerName") ? config.Properties["ContainerName"] : "grainstate";
                container = blobClient.GetContainerReference(containerName);
                await container.CreateIfNotExistsAsync().ConfigureAwait(false);

                Log.Info((int)AzureProviderErrorCode.AzureBlobProvider_InitProvider, "Init: Name={0} ServiceId={1} {2}", name, providerRuntime.ServiceId.ToString(), string.Join(" ", FormatPropertyMessage(config)));
                Log.Info((int)AzureProviderErrorCode.AzureBlobProvider_ParamConnectionString, "AzureBlobStorage Provider is using DataConnectionString: {0}", ConfigUtilities.PrintDataConnectionInfo(config.Properties["DataConnectionString"]));
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.AzureBlobProvider_InitProvider, ex.ToString(), ex);
                throw;
            }
        }

        IEnumerable<string> FormatPropertyMessage(IProviderConfiguration config)
        {
            foreach (var property in new string[] { "ContainerName", "SerializeTypeNames", "PreserveReferencesHandling", "UseFullAssemblyNames", "IndentJSON" })
            {
                if (!config.Properties.ContainsKey(property)) continue;
                yield return string.Format("{0}={1}", property, config.Properties[property]);
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainId, IGrainState grainState)
        {
            var blobName = GetBlobName(grainType, grainId);
            if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_Storage_Reading, "Reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);

            try
            {
                var blob = container.GetBlockBlobReference(blobName);

                string json;

                try
                {
                    json = await blob.DownloadTextAsync().ConfigureAwait(false);
                }
                catch (StorageException exception)
                {
                    var errorCode = exception.RequestInformation.ExtendedErrorInformation.ErrorCode;
                    if (errorCode == BlobErrorCodeStrings.BlobNotFound)
                    {
                        if (this.Log.IsVerbose2) this.Log.Verbose2((int)AzureProviderErrorCode.AzureBlobProvider_BlobNotFound, "BlobNotFound reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
                        return;
                    }
                    if (errorCode == BlobErrorCodeStrings.ContainerNotFound)
                    {
                        if (this.Log.IsVerbose2) this.Log.Verbose2((int)AzureProviderErrorCode.AzureBlobProvider_ContainerNotFound, "ContainerNotFound reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
                        return;
                    }

                    throw;
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    if (this.Log.IsVerbose2) this.Log.Verbose2((int)AzureProviderErrorCode.AzureBlobProvider_BlobEmpty, "BlobEmpty reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
                    return;
                }
                
                var data = JsonConvert.DeserializeObject(json, grainState.GetType(), jsonSettings);
                var dict = ((GrainState)data).AsDictionary();
                grainState.SetAll(dict);
                grainState.Etag = blob.Properties.ETag;

                if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_Storage_DataRead, "Read: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.AzureBlobProvider_ReadError,
                    string.Format("Error reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4} Exception={5}", grainType, grainId, grainState.Etag, blobName, container.Name, ex.Message),
                    ex);
            }
        }

        private static string GetBlobName(string grainType, GrainReference grainId)
        {
            return string.Format("{0}-{1}.json", grainType, grainId.ToKeyString());
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainId, IGrainState grainState)
        {
            var blobName = GetBlobName(grainType, grainId);
            try
            {
                if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_Storage_Writing, "Writing: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);

                var grainStateDictionary = grainState.AsDictionary();
                var json = JsonConvert.SerializeObject(grainStateDictionary, jsonSettings);

                var blob = container.GetBlockBlobReference(blobName);
                blob.Properties.ContentType = "application/json";

                var containerNotFound = false;
                try
                {
                    await blob.UploadTextAsync(
                            json,
                            Encoding.UTF8,
                            AccessCondition.GenerateIfMatchCondition(grainState.Etag),
                            null,
                            null).ConfigureAwait(false);
                }
                catch (StorageException exception)
                {
                    var errorCode = exception.RequestInformation.ExtendedErrorInformation.ErrorCode;
                    containerNotFound = errorCode == BlobErrorCodeStrings.ContainerNotFound;
                }
                if (containerNotFound)
                {
                    // if the container does not exist, create it, and make another attempt
                    if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_ContainerNotFound, "Creating container: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
                    await container.CreateIfNotExistsAsync().ConfigureAwait(false);

                    await blob.UploadTextAsync(
                        json,
                        Encoding.UTF8,
                        AccessCondition.GenerateIfMatchCondition(grainState.Etag),
                        null,
                        null).ConfigureAwait(false);
                }

                grainState.Etag = blob.Properties.ETag;

                if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_Storage_DataRead, "Written: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.AzureBlobProvider_WriteError,
                    string.Format("Error writing: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4} Exception={5}", grainType, grainId, grainState.Etag, blobName, container.Name, ex.Message),
                    ex);
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainId, IGrainState grainState)
        {
            var blobName = GetBlobName(grainType, grainId);
            try
            {
                if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_ClearingData, "Clearing: GrainType={0} Grainid={1} ETag={2} BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);

                var blob = container.GetBlockBlobReference(blobName);
                await blob.DeleteIfExistsAsync(
                        DeleteSnapshotsOption.None,
                        AccessCondition.GenerateIfMatchCondition(grainState.Etag),
                        null,
                        null).ConfigureAwait(false);
                grainState.Etag = blob.Properties.ETag;

                if (this.Log.IsVerbose3) this.Log.Verbose3((int)AzureProviderErrorCode.AzureBlobProvider_Cleared, "Cleared: GrainType={0} Grainid={1} ETag={2} BlobName={3} in Container={4}", grainType, grainId, grainState.Etag, blobName, container.Name);
            }
            catch (Exception ex)
            {
                Log.Error((int)AzureProviderErrorCode.AzureBlobProvider_ClearError,
                  string.Format("Error clearing: GrainType={0} Grainid={1} ETag={2} BlobName={3} in Container={4} Exception={5}", grainType, grainId, grainState.Etag, blobName, container.Name, ex.Message),
                  ex);
            }
        }

        public Logger Log { get; private set; }

        public string Name { get; private set; }

        public Task Close()
        {
            return TaskDone.Done;
        }
    }
}
