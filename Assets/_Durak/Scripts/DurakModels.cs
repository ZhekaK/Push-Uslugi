using System;
using System.Collections.Generic;

namespace PushPelmesh.Durak
{
    [Serializable]
    public class DurakRoomsResponse
    {
        public List<DurakRoomListItemDto> rooms = new List<DurakRoomListItemDto>();
    }

    [Serializable]
    public class DurakRoomResponse
    {
        public DurakRoomDto room;
    }

    [Serializable]
    public class DurakRoomListItemDto
    {
        public int id;
        public string name;
        public int maxPlayers;
        public int playerCount;
        public int cardCount;
        public string status;
        public bool hasPassword;
    }

    [Serializable]
    public class DurakRoomDto
    {
        public int id;
        public string name;
        public int maxPlayers;
        public int cardCount;
        public string status;
        public string trumpSuit;
        public string trumpCardCode;
        public int deckCount;
        public int myPlayerId;
        public int attackerPlayerId;
        public int defenderPlayerId;
        public bool isCreator;
        public bool canStart;
        public bool canAttack;
        public bool canDefend;
        public bool canTransfer;
        public bool canTake;
        public bool canPass;
        public string message;
        public List<DurakPlayerDto> players = new List<DurakPlayerDto>();
        public List<DurakCardDto> myHand = new List<DurakCardDto>();
        public List<DurakTableCardDto> table = new List<DurakTableCardDto>();
    }

    [Serializable]
    public class DurakPlayerDto
    {
        public int id;
        public int userId;
        public string displayName;
        public int handCount;
        public int turnOrder;
        public bool isBot;
        public bool isAttacker;
        public bool isDefender;
        public bool isOut;
    }

    [Serializable]
    public class DurakCardDto
    {
        public string code;
        public string rank;
        public string suit;
        public int value;
    }

    [Serializable]
    public class DurakTableCardDto
    {
        public DurakCardDto attack;
        public DurakCardDto defense;
    }

    [Serializable]
    public class CreateDurakRoomRequest
    {
        public string name;
        public string password;
        public int maxPlayers;
        public int cardCount;
    }

    [Serializable]
    public class JoinDurakRoomRequest
    {
        public string password;
    }

    [Serializable]
    public class DurakCardActionRequest
    {
        public string cardCode;
        public string attackCardCode;
    }
}
