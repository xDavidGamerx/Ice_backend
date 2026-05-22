namespace IceBackend.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
        bool IsLegacyHash(string hash);
        string HashLegacy(string password);
    }
}
