using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class NetworkManager : SingletonBehaviour<NetworkManager>, IRankingNetHandler
{
    public NetStatus NetworkStatus => networkStatus;
    [SerializeField] protected NetStatus networkStatus = NetStatus.Unidentified;
    [SerializeField] public string g_UID;
    public abstract UniTask DeleteUser(string UID);
    public abstract UniTask<List<RankEntry>> GetRankingInfo(int difficulty);
    public abstract UniTask<User> GetUserInfo(string UID);
    public abstract UniTask<bool> isServerOnline();
    public abstract UniTask<string> RegisterUser(string Username);
    public abstract UniTaskVoid UpdateUserInfo(string UID, string username);
    public abstract UniTaskVoid UpdateUserInfo(string UID, int difficulty, int score);
    public abstract UniTaskVoid TryUpdateUserScore(string UID, int difficulty, int score);
    public abstract UniTask TryConnectServer();

    public UniTask<RankEntry> FindUserRank(int difficulty) => FindUserRank(g_UID, difficulty);
    public async UniTask<RankEntry> FindUserRank(string UID, int difficulty)
    {
        try
        {
            var ranking = await GetRankingInfo(difficulty);
            return (ranking.Find(m => m.uid == UID) ?? new RankEntry());
        }
        catch
        {
            throw;
        }
    }
}

public enum NetStatus
{
    Unidentified,
    Unavailable,
    Available,
}
