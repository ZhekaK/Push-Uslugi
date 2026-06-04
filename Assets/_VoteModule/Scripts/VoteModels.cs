using System;
using System.Collections.Generic;

namespace PushPelmesh.VoteModule
{
    [Serializable]
    public class VotePollsResponse
    {
        public List<VotePollDto> polls = new List<VotePollDto>();
    }

    [Serializable]
    public class VotePollResponse
    {
        public VotePollDto poll;
    }

    [Serializable]
    public class VotePollDto
    {
        public int id;
        public string title;
        public string description;
        public string endDate;
        public string createdByUser;
        public string createdAt;
        public bool isClosed;
        public bool hasVoted;
        public bool canVote;
        public int selectedOptionId;
        public bool isMultipleChoice;
        public bool allowMultipleChoices;
        public int totalVotes;
        public List<int> selectedOptionIds = new List<int>();
        public List<string> audienceGroups = new List<string>();
        public List<VoteOptionDto> options = new List<VoteOptionDto>();

        public bool AllowsMultipleChoices => isMultipleChoice || allowMultipleChoices;
    }

    [Serializable]
    public class VoteOptionDto
    {
        public int id;
        public string text;
        public int votes;
        public float percent;
        public bool isSelected;
    }

    [Serializable]
    public class CreateVotePollRequest
    {
        public string title;
        public string description;
        public string endDate;
        public bool isMultipleChoice;
        public bool allowMultipleChoices;
        public List<string> options = new List<string>();
        public List<string> audienceGroups = new List<string>();
    }

    [Serializable]
    public class VotePollVoteRequest
    {
        public int optionId;
        public List<int> optionIds = new List<int>();
    }
}
