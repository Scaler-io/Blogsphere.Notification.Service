namespace Blogsphere.Notification.Service.Models.Constants;

public class AuthCodeEmailBody
{
    public const string SelfResetPassword = "We received a request to reset your Blogsphere account password. To complete the process, please use the verification code below:";
    public const string Disable2FA = "We received a request to disable two-factor authentication (2FA) for your Blogsphere account. To complete the process, please use the verification code below:";
    public const string SignIn = "We received a request to sign in to your Blogsphere account. To complete the sign-in process, please use the verification code below:";
}
