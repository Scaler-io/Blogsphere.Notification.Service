namespace Blogsphere.Notification.Service.Models.Notification
{
    public class TemplateFields(string key, string value)
    {
        public string Key { get; set; } = key;
        public string Value { get; set; } = value;
    }
}