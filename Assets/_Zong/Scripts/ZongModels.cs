using System;
using System.Collections.Generic;

namespace PushPelmesh.Zong
{
    [Serializable]
    public class ZongRoomsResponse
    {
        public List<ZongRoomListItemDto> rooms = new List<ZongRoomListItemDto>();
    }

    [Serializable]
    public class ZongRoomResponse
    {
        public ZongRoomDto room;
    }

    [Serializable]
    public class ZongRoomListItemDto
    {
        public int id;
        public string name;
        public int maxPlayers;
        public int playerCount;
        public int targetScore;
        public string status;
        public bool hasPassword;
    }

    [Serializable]
    public class ZongRoomDto
    {
        public int id;
        public string name;
        public int maxPlayers;
        public int targetScore;
        public string status;
        public int currentTurnPlayerId;
        public int winnerPlayerId;
        public int myPlayerId;
        public bool isCreator;
        public bool isMyTurn;
        public bool canStart;
        public bool canRoll;
        public bool canBank;
        public int currentTurnScore;
        public int remainingDice;
        public string lastDice;
        public int lastRollScore;
        public string lastRollMessage;
        public List<ZongPlayerDto> players = new List<ZongPlayerDto>();
    }

    [Serializable]
    public class ZongPlayerDto
    {
        public int id;
        public int userId;
        public string displayName;
        public int score;
        public int initialRoll;
        public int turnOrder;
        public bool isCurrentTurn;
        public bool isWinner;
    }

    [Serializable]
    public class CreateZongRoomRequest
    {
        public string name;
        public string password;
        public int maxPlayers;
        public int targetScore;
    }

    [Serializable]
    public class JoinZongRoomRequest
    {
        public string password;
    }
}
