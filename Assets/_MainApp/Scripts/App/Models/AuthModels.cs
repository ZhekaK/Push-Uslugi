using System;

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
        public int id;
        public string type;

        public string firstName;
        public string middleName;
        public string lastName;
        public string birthDate;

        public string createdAt;
        public string lastLoginAt;

        public float weightKg;
    }
}