// Models/EmailTemplates/VerifyAccountModel.cs
namespace YourProjectName.Models.EmailTemplates
{
    public class VerifyAccountModel
    {
        public string Username { get; set; }
        public string VerificationLink { get; set; }
    }
}