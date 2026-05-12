using System;
using System.Collections.Generic;

namespace PushPelmesh.App.Models
{
    [Serializable]
    public class LoginByKeyRequest
    {
        public string series;
        public string number;
    }

    [Serializable]
    public class AuthResponse
    {
        public string message;
        public string token;
        public UserDto user;
    }

    [Serializable]
    public class UserDto
    {
        public int id;
        public string type;
        public string displayName;
    }

    [Serializable]
    public class UpdateWeightRequest
    {
        public float weightKg;
    }

    [Serializable]
    public class UpdateWeightResponse
    {
        public string message;
        public float weightKg;
    }

    [Serializable]
    public class UserProfileResponse
    {
        public string type;
        public string series;
        public string number;

        public string firstName;
        public string middleName;
        public string lastName;
        public string birthDate;
        public string sex;
        public string giveDate;
        public string givePlace;

        public float weightKg;
    }

    [Serializable]
    public class UserRoleResponse
    {
        [Serializable]
        public class UserRoleDTO
        {
            public string number;
            public string postName;
            public string givePlace;
            public string startDate;
        }

        public List<UserRoleDTO> roles = new List<UserRoleDTO>();
    }
}