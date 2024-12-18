using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface IRankingNetHandler
{
    public UniTask<bool> isServerOnline();
    public UniTask<User> GetUserInfo(string UID);
    public UniTask<List<RankEntry>> GetRankingInfo(int difficulty);
    public UniTask<string> RegisterUser(string Username);
    public UniTask DeleteUser(string UID);
    public UniTaskVoid UpdateUserInfo(string UID, string username);
    public UniTaskVoid UpdateUserInfo(string UID, int difficulty, int score);
    public UniTaskVoid TryUpdateUserScore(string UID, int difficulty, int score);
}

[Serializable]
public class User
{
    public string uid;
    public string name;
    public int max_score_3;
    public int max_score_4;
    public int max_score_5;

    public override string ToString()
    {
        return $"{uid} : {name}";
    }
}

[Serializable]
public class RankEntry
{
    public string uid;
    public string name;
    public int max_score;
    public int rank;

    public RankEntry()
    {
        uid = null;
        name = null;
        max_score = 0;
        rank = -1;
    }
}

[Serializable]
public class RankEntryList
{
    public List<RankEntry> entries;
}