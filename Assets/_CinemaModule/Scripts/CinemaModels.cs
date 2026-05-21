using System;
using System.Collections.Generic;

namespace PushPelmesh.CinemaModule
{
    [Serializable]
    public class CinemaMoviesResponse
    {
        public List<CinemaMovieDto> movies = new List<CinemaMovieDto>();
    }

    [Serializable]
    public class CinemaMovieDto
    {
        public int id;
        public string title;
        public float rating;
        public string watchedAt;
        public string url;
        public string createdAt;
    }

    [Serializable]
    public class CreateCinemaMovieRequest
    {
        public string Title;
        public float Rating;
        public string WatchedAt;
        public string Url;
    }
}
