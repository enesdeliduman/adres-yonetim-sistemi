using Microsoft.AspNetCore.Mvc;

namespace AYS.Helpers
{
public class ConfigurationServices
{
    private readonly IConfiguration _configuration;

    public ConfigurationServices(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetContactEmail()
    {
        return _configuration["Contact:Email"];
    }

    public string Name()
    {
        return _configuration["Contact:Name"];
    }
}

}