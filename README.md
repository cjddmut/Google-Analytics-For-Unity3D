# Universal Analytics For Unity3D (UAUnity)
============================

UAUnity provides a script interface to logging data for Google's [Universal Analytics](https://developers.google.com/analytics/).

============================

####Obtain!####
[Releases](https://github.com/cjddmut/Google-Universal-Analytics-For-Unity3D/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/cjddmut/Google-Universal-Analytics-For-Unity3D/archive/develop.zip) and copy the files in Assets to your project's Assets folder.

[Planned Features](https://github.com/cjddmut/Google-Universal-Analytics-For-Unity3D/wiki/Development-Roadmap)

## Setup

First you'll have to [set up an account](https://support.google.com/analytics/answer/2817075?hl=en) with Google's Universal Analytics. Be sure to set up the property that you are tacking as a mobile app instead of a website, this way you can have an application name. Once you have a tracking id and application name, you are ready to start using UAUnity!

Make sure UniversalAnalytics.cs exists somewhere in your project and invoke UniversalAnalytics.Initialize(trackingId, appName) at least once during runtime before any logging.

Easy :).

## Interacting with Universal Analytics

### General ###

```csharp
void UniversalAnalytics.Initialize(
    string trackingId, 
    string applicationName, 
    string applicationVersion = "", 
    string clientId = null)
```

Initialize sets up all the data UAUnity will need to know for every log call. **This function must be called!**

**string tackingId** - This is your Universal Analytics tracking id, of the form UA-12345678-9.

**string applicationName** - The name of your application as set for Universal Analytics.

**string applicationVersion** - The version of your application, format can be anything you want.

**string clientId** - The identification of the installed application. NOTE! This is not the user id (that isn't supported yet)! This is the id of specific installed application. On installation you could generate a GUID and pass that here every time. If no value is specified then a GUID is generated resulting in effectively a play session id. This value is ignored for web player which uses a session id.

```csharp
bool UniversalAnalytics.initialized
```

Read only. Can query to see if UAUnity has already been initialized.

```csharp
bool gatherSystemInformation
```

If set to true then will gather some data that is automatically supported by Universal Analytics. This data is screen resolution, viewport size, and system language. If more data is desired (such as platform information) then you'll have to set up your own data points and log them.

```csharp
bool autoHandleExceptionLogging
```

If this is set to true then UAUnity will attach itself to log information inside Unity. While listening, if it detects an exception then it will log an exception with as much description that it can to Universal Analytics. This is useful for see what and how many exceptions happen in the wild.

### Events ###

```csharp
void UniversalAnalytics.LogEvent(
    string category, 
    string action, 
    string label, 
    int value = 0)
    
void UniversalAnalytics.LogEvent(
    string category, 
    string action, 
    int value = 0)
```

Log Universal Analytics events. 
https://developers.google.com/analytics/devguides/collection/analyticsjs/events

### Timing ###

```csharp
void LogTiming(string category, string variableName, string label, int timeInMS)
void LogTiming(string category, string variableName, int timeInMS)
```

Log Universal Analytics timing events. 
https://developers.google.com/analytics/devguides/collection/analyticsjs/user-timings

### Dimensions and Metrics ###

```csharp
void AddDimension(int index, string value)
void AddMetric(int index, int value)
```

Add Universal Analytics dimensions and metrics to the next log.
https://developers.google.com/analytics/devguides/platform/customdimsmets-overview

### Exceptions ###

```csharp
void LogException(string desc, bool isFatal = false)
```

Log Universal Analytics exceptions. 
https://developers.google.com/analytics/devguides/collection/analyticsjs/exceptions

## Example

```csharp
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

        // Construct version. Since everything is done via http requests, we'll put 
        // the platform in the version.
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
        UniversalAnalytics.LogEvent(
            "Game", 
            "Event", 
            "Won (time)", 
            (int)Time.timeSinceLevelLoad);
    }

    void OnApplicationQuit()
    {
        // OnApplicationQuit may not be invoked in time in the web player!
        UniversalAnalytics.LogEvent("Game", "Player Stats", "Deaths", playerDeaths);
        UniversalAnalytics.LogTiming(
            "Game", 
            "Lifetime", 
            "End", 
            (int)(Time.realtimeSinceStartup * 1000));
    }
}
```

## FAQ

**Can I use normal Universal Analytics with my website along side UAUnity for the web plugin?**
Sure can! UAUnity will piggy back on the analytics information that the website has set up and work along with it.

**What about along side Google's older analytics tool, Google Analytics?**
This will most likely not work. Since UAUnity tries to piggy back on an already queried property set up by the website, it will attempt to use the new interface instead of the old one. Since Google Analytics is being deprecated, I did not worry about making sure this case works. If this conflict turns out to be a common problem then it will be fixed. So let me know if it is a problem!

**What platforms will UAUnity work on?**
UAUnity has been tested on Windows, OSX, and the web player. There is nothing preventing it from working with other platforms so it may very well work on more.

**Hey! I'm all hooked up and I'm not seeing any data!**
It can take up to 24 hours for data to be gathered and show itself in the Universal Analytics' dashboard. Though if you check Real-Time > Events you can see how many events have registered in the last 30 minutes. Checking there is a way to verify if it's working.

It's also possible you've found a broken case, which case please open up an [issue](https://github.com/cjddmut/UniversalAnalyticsForUnity3D/issues). 
