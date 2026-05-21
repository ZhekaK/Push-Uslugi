using System.Collections.Generic;
using System.Threading.Tasks;
using PushPelmesh.App.Api;
using UnityEngine;

namespace PushPelmesh.RewardModule
{
    public static class RewardApi
    {
        public static async Task<List<RewardRecordDto>> GetChampionshipsAsync()
        {
            return await GetRecordsAsync("/api/rewards/championships");
        }

        public static async Task<List<RewardRecordDto>> GetGovernmentAwardsAsync()
        {
            return await GetRecordsAsync("/api/rewards/government-awards");
        }

        private static async Task<List<RewardRecordDto>> GetRecordsAsync(string path)
        {
            string json = await ApiClient.GetAsync(path, withAuth: true);
            RewardRecordsResponse response = JsonUtility.FromJson<RewardRecordsResponse>(json);

            if (response == null || response.records == null)
                return new List<RewardRecordDto>();

            return response.records;
        }
    }
}
