using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

/* Network Data are sent as JSON files, they can contain:
 * - Information to fill the game main UI :
 *    - List of Invaders (all possible invaders)    UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 *    - List of Resources (all possible resources)   UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 * - Information to fill the map :
 *    - Encounters (around a location)  UPDATED AT FIRST CONNECTION + WHEN PLAYER MOVES
 *    - Leaderboards (for a given encounter)   UPDATED WHEN PLAYER CLICKS ON AN ENCOUNTER
 * - Information to start an encounter :
 *    - Encounter Patterns (all possible patterns)    UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 *    - Request to start an encounter       SENT ONCE BY THE PLAYER
 * - Information to join a multiplayer encounter :
 *    - Current state of the game       SENT BY A PLAYER IN-GAME TO A PLAYER WHO WANT TO JOIN
 *    - Current number of players       UPDATED AT START OF ENCOUNTER + EACH TIME A PLAYER JOINS
 * - Information during an encounter :
 *    - Update about a running encounter (information about other players, about invaders status)    SENT EVERY SECOND BY THE PLAYER + UPDATED REGULARLY DURING AN ENCOUNTER
 *    - Resolution of an encounter (information about score and resources won)       UPDATED AFTER AN ENCOUNTER IS WON/LOST
 * - Information to access shop and buy things :
 *    - Items/Upgrades (all possible upgrades)     UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 *    - Current amount of resources (for current player)     UPDATED AT CONNECTION WHEN PLAYER IS NOT UP TO DATE + AFTER EACH ENCOUNTER RESOLVED + AFTER EACH TRANSACTION + AFTER A PHOTO IS VALIDATED
 * - Information to access friends list :
 *    - Current list of friends (for current player)     UPDATED AT CONNECTION + WHEN PLAYER MAKES A FRIEND REQUEST
 * - Information about a picture :
 *    - The picture itself      SENT ONCE BY THE PLAYER
 *    - The status of a picture     UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 * - Information to access play store shop :
 *    - All current deals       UPDATED ONCE WHEN PLAYER IS NOT UP TO DATE
 *    - Request to make a deal      SENT BY THE PLAYER
 *    - 
 * */

// COMMUNICATION WITH ALFRED 

#region Info relative to error messages

[System.Serializable]
public class ErrorMessage
{
    public string JSONType;

    public string message;
    public int errorCode;

    public ErrorMessage()
    {
        this.JSONType = this.GetType().ToString();
    }

    public string ToJSON()
    {
        this.JSONType = this.GetType().ToString();
        return JsonUtility.ToJson(this);
    }
}

#endregion

// ENCAPSULATION FOR ANY NETWORK COMMUNICATION

#region Encapsulation

[System.Serializable]
public class NetworkData
{
    private const string jsonTypeField = "JSONType";
    private const int maxJsonTypeLength = 50;

    // contains the object from JSON
    private object networkObject;
    
    // JSON Methods
    public static NetworkData CreateFromJSON(string jsonString)
    {
        NetworkData networkData = new NetworkData();
        if (jsonString.Contains(jsonTypeField))
        {
            int index = jsonString.IndexOf(jsonTypeField);
            string JSONTypeName = jsonString.Substring(index + jsonTypeField.Length, Mathf.Min(maxJsonTypeLength, (jsonString.Length - index - jsonTypeField.Length))).Split('"')[2];
            Type JSONType = Type.GetType(JSONTypeName);
            if (JSONType != null)
            {
                networkData.networkObject = JsonUtility.FromJson(jsonString, JSONType);
            }
            else
            {
                Debug.LogError("CreateFromJSON() failed. Type of JSON doesn't exist.");
                return null;
            }
        }

        return networkData;
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this.networkObject);
    }

    public static NetworkData CreateNetworkData(object data)
    {
        NetworkData result = new NetworkData();
        result.networkObject = data;
        return result;
    }
    
    /// <summary>
    /// Return data according to type given in JSON file.
    /// </summary>
    /// <returns>Object of one of those types : NetworkAllStartInfo, or NetworkAllUpdateInfo, or NetworkAllAuthenticationInfo, or NetworkErrorMessage, or null</returns>
    public object GetData()
    {
        return networkObject;
    }
}

#endregion

