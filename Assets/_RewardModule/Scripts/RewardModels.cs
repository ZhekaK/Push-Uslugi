using System;
using System.Collections.Generic;

namespace PushPelmesh.RewardModule
{
    [Serializable]
    public class RewardRecordsResponse
    {
        public List<RewardRecordDto> records = new List<RewardRecordDto>();
    }

    [Serializable]
    public class RewardRecordDto
    {
        public int id;
        public int kind;
        public string kindName;
        public string fullName;
        public string eventType;
        public string eventName;
        public string place;
        public string createdAt;
    }
}
