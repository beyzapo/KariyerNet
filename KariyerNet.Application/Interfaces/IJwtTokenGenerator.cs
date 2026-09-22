using System;
using System.Collections.Generic;
using System.Text;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }   
}
