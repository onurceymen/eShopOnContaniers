using IdentityService.Api.Application.Models;

namespace IdentityService.Api.Application.Services
{
    public interface IIdentityServices
    {
        Task<LoginResponseModel> Login(LoginRequestModel requestModel);

    }
}
