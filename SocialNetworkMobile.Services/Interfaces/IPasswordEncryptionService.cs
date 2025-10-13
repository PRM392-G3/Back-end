namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IPasswordEncryptionService
    {
        string EncryptPassword(string plainPassword);
        bool VerifyPassword(string plainPassword, string hashedPassword);
    }
}
