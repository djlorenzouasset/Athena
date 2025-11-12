using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace Athena.Models.API.Responses;

public class EpicAuth
{
    [J("access_token")] public string AccessToken;
    [J("expires_at")] public DateTime ExpiresAt;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(AccessToken) &&
            ExpiresAt > DateTime.UtcNow;
    }
}
