namespace Blogsphere.Notification.Service.Configurations;

public class BlobStorageOption
{
    public const string OptionName = "BlobStorage";
    public string ServiceUrl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string BucketName { get; set; }
}