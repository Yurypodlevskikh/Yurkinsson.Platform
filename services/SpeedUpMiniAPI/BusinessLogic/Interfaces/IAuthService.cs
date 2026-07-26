using BusinessLogic.DTOs;
using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        Task<(ApiResponseDto Response, int StatusCode)> RegisterUserAsync(RegisterUserRequest model);
    }
}
