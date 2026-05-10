using System;
using System.Threading.Tasks;
using PushPelmesh.App.Auth;
using PushPelmesh.App.Models;
using UnityEngine;

namespace PushPelmesh.App.Api
{
    public class ApiTestBehaviour : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                Debug.Log("API test started");

                AuthResponse authResponse = await AuthService.LoginAsGuestAsync();

                Debug.Log($"Login success");
                Debug.Log($"Token: {authResponse.token}");
                Debug.Log($"User id: {authResponse.user.id}");
                Debug.Log($"User type: {authResponse.user.type}");
                Debug.Log($"User display name: {authResponse.user.displayName}");

                UserProfileResponse profile = await AuthService.GetProfileAsync();

                Debug.Log("Profile loaded from database");
                Debug.Log($"Id: {profile.id}");
                Debug.Log($"Type: {profile.type}");
                Debug.Log($"First name: {profile.firstName}");
                Debug.Log($"Middle name: {profile.middleName}");
                Debug.Log($"Last name: {profile.lastName}");
                Debug.Log($"Birth date: {profile.birthDate}");
                Debug.Log($"Created at: {profile.createdAt}");
                Debug.Log($"Last login at: {profile.lastLoginAt}");
            }
            catch (Exception exception)
            {
                Debug.LogError("API test failed");
                Debug.LogError(exception.Message);
            }
        }
    }
}