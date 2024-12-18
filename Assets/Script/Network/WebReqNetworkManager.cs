using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

internal class WebReqNetworkManager : NetworkManager
{
    private const string baseUrl = "https://4862-220-90-159-239.ngrok-free.app";

    public async UniTask<string> WebRequestAsync(string mode, string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, mode))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();

                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log(request.downloadHandler.text);
                    return request.downloadHandler.text;
                }
                else
                {
                    //Debug.LogError(request.error);
                    throw new Exception(request.error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Request failed: {ex.Message}");
                throw;
            }
        }
    }
    public async UniTask<string> GetRequestAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            try
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();

                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    throw new Exception(request.error);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    public async UniTask DeleteRequestAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("User deleted successfully");
            }
            else
            {
                Debug.LogError($"Failed to delete data: {request.error}");
            }
        }
    }

    public override async UniTask DeleteUser(string UID)
    {
        try
        {
            await TryConnectServer();
            await DeleteRequestAsync($"{baseUrl}/user/{UID}");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override async UniTask<List<RankEntry>> GetRankingInfo(int difficulty)
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var res = await GetRequestAsync($"{baseUrl}/user/rank/{difficulty}");
            var json = "{\"entries\":" + res + "}";
            RankEntryList rankEntryList = JsonUtility.FromJson<RankEntryList>(json);
            return rankEntryList.entries;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            throw;
        }
    }

    public override async UniTask<User> GetUserInfo(string UID)
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var user = await GetRequestAsync($"{baseUrl}/user/{UID}");
            return JsonUtility.FromJson<User>(user);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override async UniTask<string> RegisterUser(string Username = "Default")
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var uid = await WebRequestAsync("POST", $"{baseUrl}/user", $"{{\"name\" : \"{Username}\"}}");
            return uid;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override async UniTaskVoid UpdateUserInfo(string UID, string username)
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var user = await GetUserInfo(UID);
            user.name = username;
            await WebRequestAsync("PUT", $"{baseUrl}/user/{UID}", JsonUtility.ToJson(user));
            PlayerPrefs.SetString("Username", username);
            Debug.Log($"Username successfully changed to {username}");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override async UniTaskVoid UpdateUserInfo(string UID, int difficulty, int score)
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var user = await GetUserInfo(UID);
            switch (difficulty)
            {
                case 3:
                    user.max_score_3 = score;
                    break;
                case 4:
                    user.max_score_4 = score;
                    break;
                case 5:
                    user.max_score_5 = score;
                    break;
                default:
                    throw new ArgumentException("Invalid difficulty level");
            }
            await WebRequestAsync("PUT", $"{baseUrl}/user/{UID}", JsonUtility.ToJson(user));
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }
    }

    public override async UniTask<bool> isServerOnline()
    {
        try
        {
            var res = await GetRequestAsync($"{baseUrl}/handshake");
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public override async UniTask TryConnectServer()
    {
        try
        {
            var res = await GetRequestAsync($"{baseUrl}/handshake");
            networkStatus = NetStatus.Available;
        }
        catch(Exception ex)
        {
            networkStatus = NetStatus.Unavailable;
            throw;
        }
    }

    public override async UniTaskVoid TryUpdateUserScore(string UID, int difficulty, int score)
    {
        try
        {
            if (networkStatus != NetStatus.Available)
            {
                throw new Exception("Network Unavailable");
            }
            var res = await GetUserInfo(UID);

            int targetDifficultyScore = difficulty switch
            {
                3 => res.max_score_3,
                4 => res.max_score_4,
                5 => res.max_score_5,
                _ => int.MaxValue,
            };

            if(targetDifficultyScore < score)
            {
                UpdateUserInfo(UID, difficulty, score).Forget();
            }
        }
        catch
        {
            throw;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WebReqNetworkManager))]
public class WebReqNetworkManagerEditor : Editor
{
    int input_diff;
    int input_score;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WebReqNetworkManager WRNM = (WebReqNetworkManager)target;

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Change G_UID member's score", EditorStyles.boldLabel);
        input_score = EditorGUILayout.IntField("Target score to change", input_score);
        input_diff = EditorGUILayout.IntSlider("Target difficulty to change", input_diff, 3, 5);
        if (GUILayout.Button("Apply!"))
        {
            WRNM.UpdateUserInfo(WRNM.g_UID, input_diff, input_score).Forget();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Dispose Game Cycle"))
        {
         
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
