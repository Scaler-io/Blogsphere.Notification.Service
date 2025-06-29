namespace Blogsphere.Notification.Service.Configurations;

public sealed class EmailTemplates
{
    public const string OptionName = "EmailTemplates";
    public string UserInvite { get; set; }
    public string AuthCodeSent { get; set; }
    public string PasswordResetInstructionSent { get; set; }
}