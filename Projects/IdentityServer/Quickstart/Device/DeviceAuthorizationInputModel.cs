using IdentityServer.Quickstart.Consent;

namespace IdentityServer.Quickstart.Device
{
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        public string UserCode { get; set; }
    }
}