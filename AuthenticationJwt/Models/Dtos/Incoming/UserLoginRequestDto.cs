﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationJwt.Models.Dtos.Incoming
{
    public class UserLoginRequestDto
    {
        [Required]
        public string Email { get; set;}
        [Required]
        public string Password { get; set;}
    }
}
