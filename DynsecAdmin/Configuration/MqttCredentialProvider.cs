using System.Security.Claims;
using System.Text;
using MQTTnet.Client;

namespace DynsecAdmin.Configuration;

public class MqttCredentialProvider : IMqttClientCredentialsProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public MqttCredentialProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? GetUserName(MqttClientOptions clientOptions)
    {
        var user = _contextAccessor.HttpContext?.User;
        var identity = user?.Identity;
        if (identity is null) return null;
        if (!identity.IsAuthenticated) return null;

        return user.FindFirstValue(ClaimTypes.Upn);
    }

    public byte[]? GetPassword(MqttClientOptions clientOptions)
    {
        var user = _contextAccessor.HttpContext?.User;
        var identity = user?.Identity;
        if (identity is null) return null;
        if (!identity.IsAuthenticated) return null;

        return Encoding.UTF8.GetBytes(user.FindFirstValue(ClaimTypes.Hash));
    }
}