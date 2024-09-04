using Microsoft.AspNetCore.Identity;

namespace EchoSphere.UsersApi.Models;

internal sealed class UserProfile : IdentityUser<Guid>
{
}