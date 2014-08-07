using UnityEngine;
using System.Collections;

public class Example : MonoBehaviour
{
    public string gameName;
    public string version = "0.1.0";
    public bool gatherSystemInfo = true;
    public bool logExceptions = true;
    public bool logInEditor = false;

    private int playerDeaths = 0;

    private const string GOOGLE_ID = "UA-12345678-9";

    // For distguishing platform
    private const string WIN = "win-";
    private const string MAC = "osx-";
    private const string WEB = "web-";
    private const string EDITOR = "editor-";
    private const string UNKNOWN = "unknown-";

    void Awake()
    {
        if (Application.isEditor && !logInEditor)
        {
            // Don't initialize then nothing will be logged.
            return;
        }

        // Construct version. Since everything is done via http requests, we'll put the platform in the version.
        string constructedVersion;

        if (Application.isEditor)
        {
            constructedVersion = EDITOR;
        }
        else if (Application.isWebPlayer)
        {
            constructedVersion = WEB;
        }
        else
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer : 
                    constructedVersion = WIN;
                    break;

                case RuntimePlatform.OSXPlayer :
                    constructedVersion = MAC;
                    break;

                default:
                    constructedVersion = UNKNOWN;
                    break;
            }
        }

        constructedVersion += version;

        UniversalAnalytics.Initialize(GOOGLE_ID, gameName, constructedVersion);
        UniversalAnalytics.autoHandleExceptionLogging = logExceptions;
        UniversalAnalytics.gatherSystemInformation = gatherSystemInfo;

        UniversalAnalytics.LogEvent("Game", "Lifetime", "Start");
    }

    void PlayerDied()
    {
        playerDeaths++;
    }

    void PlayerWon()
    {
        UniversalAnalytics.LogEvent("Game", "Event", "Won (time)", (int)Time.timeSinceLevelLoad);
    }

    void OnApplicationQuit()
    {
        // OnApplicationQuit may not be invoked in time in the web player!
        UniversalAnalytics.LogEvent("Game", "Player Stats", "Deaths", playerDeaths);
        UniversalAnalytics.LogTiming("Game", "Lifetime", "End", (int)(Time.realtimeSinceStartup * 1000));
    }
}
