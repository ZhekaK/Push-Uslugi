using System.Collections.Generic;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.CinemaModule
{
    public static class CinemaApi
    {
        private const string MoviesPath = "/api/cinema/movies";

        public static async Task<List<CinemaMovieDto>> GetMoviesAsync()
        {
            string json = await ApiClient.GetAsync(MoviesPath, withAuth: true);
            CinemaMoviesResponse response = JsonUtility.FromJson<CinemaMoviesResponse>(json);

            if (response == null || response.movies == null)
                return new List<CinemaMovieDto>();

            return response.movies;
        }

        public static async Task<CinemaMovieDto> CreateMovieAsync(CreateCinemaMovieRequest request)
        {
            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(MoviesPath, json, withAuth: true);
            return JsonUtility.FromJson<CinemaMovieDto>(responseJson);
        }
    }
}
