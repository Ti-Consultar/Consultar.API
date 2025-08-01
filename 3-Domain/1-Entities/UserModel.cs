﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Domain._1_Entities
{
    public class UserModel
    {
        public UserModel() { }

        public UserModel(string name, string email, string contact ,string role ,string password)
        {
            Name = name;
            Email = email;
            Password = password;
            Role = role;
            Contact = contact;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Contact { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        // Relacionamento Muitos-para-Muitos com Companies
        public List<CompanyUserModel>? CompanyUsers { get; set; }
    }
}
