namespace Orleans.StorageProvider.Blob
{
    //copy from: https://github.com/dotnet/orleans/blob/master/src/OrleansAzureUtils/Providers/AzureProviderErrorCode.cs
    internal enum AzureProviderErrorCode
    {
        ProvidersBase = 200000,

        AzureBlobProviderBase = ProvidersBase + 300,
        AzureBlobProvider_BlobNotFound = AzureBlobProviderBase + 1,
        AzureBlobProvider_ContainerNotFound = AzureBlobProviderBase + 2,
        AzureBlobProvider_BlobEmpty = AzureBlobProviderBase + 3,
        AzureBlobProvider_ReadingData = AzureBlobProviderBase + 4,
        AzureBlobProvider_WritingData = AzureBlobProviderBase + 5,
        AzureBlobProvider_Storage_Reading = AzureBlobProviderBase + 6,
        AzureBlobProvider_Storage_Writing = AzureBlobProviderBase + 7,
        AzureBlobProvider_Storage_DataRead = AzureBlobProviderBase + 8,
        AzureBlobProvider_WriteError = AzureBlobProviderBase + 9,
        AzureBlobProvider_DeleteError = AzureBlobProviderBase + 10,
        AzureBlobProvider_InitProvider = AzureBlobProviderBase + 11,
        AzureBlobProvider_ParamConnectionString = AzureBlobProviderBase + 12,
        AzureBlobProvider_ReadError = AzureBlobProviderBase + 13,
        AzureBlobProvider_ClearError = AzureBlobProviderBase + 14,
        AzureBlobProvider_ClearingData = AzureBlobProviderBase + 15,
        AzureBlobProvider_Cleared = AzureBlobProviderBase + 16
    }
}
