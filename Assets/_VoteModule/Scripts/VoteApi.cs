using System.Collections.Generic;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.VoteModule
{
    public static class VoteApi
    {
        private const string BasePath = "/api/votes/polls";

        public static async Task<VotePollsResponse> GetPollsAsync()
        {
            string json = await ApiClient.GetAsync(BasePath, withAuth: true);
            VotePollsResponse response = JsonUtility.FromJson<VotePollsResponse>(json);
            return response ?? new VotePollsResponse();
        }

        public static async Task<VotePollDto> GetPollAsync(int pollId)
        {
            string json = await ApiClient.GetAsync(BasePath + "/" + pollId, withAuth: true);
            VotePollResponse response = JsonUtility.FromJson<VotePollResponse>(json);
            return response != null ? response.poll : null;
        }

        public static async Task<VotePollDto> CreatePollAsync(CreateVotePollRequest request)
        {
            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath, json, withAuth: true);
            VotePollResponse response = JsonUtility.FromJson<VotePollResponse>(responseJson);
            return response != null ? response.poll : null;
        }

        public static async Task<VotePollDto> VoteAsync(int pollId, int optionId)
        {
            VotePollVoteRequest request = new VotePollVoteRequest
            {
                optionId = optionId
            };

            request.optionIds.Add(optionId);

            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + pollId + "/vote", json, withAuth: true);
            VotePollResponse response = JsonUtility.FromJson<VotePollResponse>(responseJson);
            return response != null ? response.poll : null;
        }

        public static async Task<VotePollDto> VoteAsync(int pollId, List<int> optionIds)
        {
            VotePollVoteRequest request = new VotePollVoteRequest();

            if (optionIds != null)
            {
                for (int i = 0; i < optionIds.Count; i++)
                {
                    if (optionIds[i] <= 0 || request.optionIds.Contains(optionIds[i]))
                        continue;

                    request.optionIds.Add(optionIds[i]);
                }
            }

            request.optionId = request.optionIds.Count > 0 ? request.optionIds[0] : 0;

            string json = JsonUtility.ToJson(request);
            string responseJson = await ApiClient.PostJsonAsync(BasePath + "/" + pollId + "/vote", json, withAuth: true);
            VotePollResponse response = JsonUtility.FromJson<VotePollResponse>(responseJson);
            return response != null ? response.poll : null;
        }
    }
}
