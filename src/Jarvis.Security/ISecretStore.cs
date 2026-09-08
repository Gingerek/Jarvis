namespace Jarvis.Security;

public interface ISecretStore
{
    string? Read(string name);
    void Write(string name, string secret);
    void Delete(string name);
}
