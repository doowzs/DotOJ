using System;
using System.Collections.Generic;

namespace Shared.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime Issued { get; set; }
        public DateTime Expires { get; set; }
    }
}