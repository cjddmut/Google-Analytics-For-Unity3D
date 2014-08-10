using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class UniversalAnalytics
{
    private static string accountId;
    private static string appName;
    private static string appVersion;
    private static string cid;

    private static string screenResolution;
    private static string viewportSize;
    private static string systemLanguage;

    private static Dictionary<int, string> dimensions = new Dictionary<int, string>();
    private static Dictionary<int, int> metrics = new Dictionary<int, int>();

    private const string UA_COLLECT_URL = "http://www.google-analytics.com/collect";
    private const int UA_EXCEPTION_DESC_LIMIT = 150;

    /*
     * Call this before trying to log any events! Just needs to be called once while the game is running, but repeated
     * calls will not hurt it if placed in some Start() that happens on every scene load.
     * 
     * If Initialize() is not invoked then all functions will fail silently. This is so you can not log in
     * certain cases (like say while in the editor) but still just call the log functions in the scripts.
     * 
     *     googleAccountId    - The tracking Id that is collecting the data.
     *     applicationName    - Name of the application on the UA website.
     *     applicationVersion - Application version.
     *     clientId           - A unique id for the client, if one is not supplied then a GUID is generated.
     *                          Note! This is supposed to be an id for the installation, not the user. User Id
     *                          is currently not supported. 
     *                          Ignored for web player.
     * */
    public static void Initialize(
        string trackingId, 
        string applicationName, 
        string applicationVersion = "", 
        string clientId = null)
    {
        if (initialized)
        {
            return;
        }

        accountId = trackingId;
        appName = applicationName;
        appVersion = applicationVersion;

        if (!Application.isWebPlayer && clientId == null)
        {
            // Generate a unique id for this client session.
            cid = System.Guid.NewGuid().ToString();
        }

        if (Application.isWebPlayer)
        {
            // Since we do not have a Crossdomain.xml file on the google server, we'll have to use eval functions
            // to send information to google.
            Application.ExternalEval(@"
                if (typeof ga == 'undefined') {
                    console.log('creating analytics');
                    (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
                        (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
                        m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
                        })(window,document,'script','//www.google-analytics.com/analytics.js','ga');
                }

                ga('create', '" + trackingId + @"', 'auto', {'name': 'unityUATracker'});
                ga('unityUATracker.set', 'appName', '" + WebMakeStringSafe(appName) + @"');
                ga('unityUATracker.set', 'appVersion', '" + WebMakeStringSafe(appVersion) + @"');
                ");
        }

        initialized = true;
    }

    public static void LogScreenView(string screenName)
    {
        if (!initialized)
        {
            return;
        }

        if (logToConsole)
        {
            Debug.Log("UA: ScreenView (" + screenName + ")");
        }

        if (Application.isWebPlayer)
        {
            Application.ExternalEval("ga('unityUATracker.send', 'screenview', {'screenName': '" + WebMakeStringSafe(screenName) + "'});");
        }
        else
        {
            // Everything else!
            WWWForm data = new WWWForm();
            data.AddField("t", "screenview");
            data.AddField("cd", screenName);
            SendData(data);
        }
    }

    /*
     * https://developers.google.com/analytics/devguides/collection/analyticsjs/events
     * */
    public static void LogEvent(string category, string action, string label, int value = 0)
    {
        if (!initialized)
        {
            return;
        }

        if (value < 0)
        {
            Debug.LogWarning("UA: Event value must be non-negative for logging events.");
            return;
        }

        if (logToConsole)
        {
            Debug.Log("UA: Event (" + category + ", " + action + ", " + label + ", " + value + ")");
        }

        if (Application.isWebPlayer)
        {
            Application.ExternalEval(@"
                ga('unityUATracker.send', {
                    'hitType': 'event',
                    'eventCategory': '" + WebMakeStringSafe(category) + @"',
                    'eventAction': '" + WebMakeStringSafe(action) + @"',
                    'eventLabel': '" + WebMakeStringSafe(label) + @"',
                    'eventValue': " + value + @"
                    " + WebConstructExtraArgs() + @"
                    });
                ");
        }
        else
        {
            // Everything else!
            WWWForm data = new WWWForm();
            data.AddField("t", "event");
            data.AddField("ec", category);
            data.AddField("ea", action);
            data.AddField("el", label);
            data.AddField("ev", value.ToString());

            SendData(data);
        }
    }

    public static void LogEvent(string category, string action, int value = 0)
    {
        LogEvent(category, action, "", value);
    }


    /*
     * https://developers.google.com/analytics/devguides/collection/analyticsjs/user-timings
     * */
    public static void LogTiming(string category, string variableName, string label, int timeInMS)
    {
        if (!initialized)
        {
            return;
        }

        if (logToConsole)
        {
            Debug.Log("UA: Timing (" + category + ", " + variableName + ", " + label + ", " + timeInMS + ")");
        }

        if (Application.isWebPlayer)
        {
            Application.ExternalEval(@"
                ga('unityUATracker.send', {
                    'hitType': 'timing',
                    'timingCategory': '" + WebMakeStringSafe(category) + @"',
                    'timingVar': '" + WebMakeStringSafe(variableName) + @"',
                    'timingLabel': '" + WebMakeStringSafe(label) + @"',
                    'timingValue': " + timeInMS + @"
                    " + WebConstructExtraArgs() + @"
                    });
                ");
        }
        else
        {
            WWWForm data = new WWWForm();
            data.AddField("t", "timing");
            data.AddField("utc", category);
            data.AddField("utv", variableName);
            data.AddField("utl", label);
            data.AddField("utt", timeInMS);

            SendData(data);
        }
    }

    public static void LogTiming(string category, string variableName, int timeInMS)
    {
        LogTiming(category, variableName, "", timeInMS);
    }

    /*
     * https://developers.google.com/analytics/devguides/collection/analyticsjs/exceptions
     * */
    public static void LogException(string desc, bool isFatal = false)
    {
        if (!initialized)
        {
            return;
        }
        
        // We are limited to 150 characters. The post form is UTF8 so just count length.
        if (desc.Length > UA_EXCEPTION_DESC_LIMIT)
        {
            Debug.LogWarning("UA: Exception description surpasses 150 in length, truncating.");
            desc = desc.Substring(0, UA_EXCEPTION_DESC_LIMIT);
        }

        if (logToConsole)
        {
            Debug.Log("UA: Exception (" + desc + ", " + isFatal + ")");
        }

        if (Application.isWebPlayer)
        {
            Application.ExternalEval(@"
                ga('unityUATracker.send', 'exception', {
                    'exDescription': '" + WebMakeStringSafe(desc) + @"',
                    'exFatal': " + (isFatal ? "true" : "false") + @"
                    " + WebConstructExtraArgs() + @"
                    });
                ");

            //Application.ExternalEval(desc.Replace("'", "\\'"));
        }
        else
        {
            WWWForm data = new WWWForm();
            data.AddField("t", "exception");
            data.AddField("exd", desc);
            data.AddField("exf", isFatal ? 1 : 0);

            SendData(data);
        }
    }

    /*
     * https://developers.google.com/analytics/devguides/platform/customdimsmets-overview
     * */
    public static void AddDimension(int index, string value)
    {
        if (!initialized)
        {
            return;
        }

        if (index < 1 || index > 200)
        {
            Debug.LogWarning("UA: Dimension index has to be between 1 and 200.");
            return;
        }

        dimensions[index] = value;
    }

    /*
     * https://developers.google.com/analytics/devguides/platform/customdimsmets-overview
     * */
    public static void AddMetric(int index, int value)
    {
        if (!initialized)
        {
            return;
        }

        if (index < 1 || index > 200)
        {
            Debug.LogWarning("UA: Metric index has to be between 1 and 200.");
            return;
        }

        metrics[index] = value;
    }

    private static void SendData(WWWForm data)
    {
        LogDimensionsAndMetrics();

        // Internet connectivity?
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            // Suck :(.
            Debug.LogWarning("UA: Internet is not reachable!");
            dimensions.Clear();
            metrics.Clear();
            return;
        }

        // Add default values.
        data.AddField("v", "1");
        data.AddField("tid", accountId);
        data.AddField("cid", cid);
        data.AddField("an", appName);
        data.AddField("av", appVersion);

        if (gatherSystemInformation)
        {
            data.AddField("sr", screenResolution);
            data.AddField("vp", viewportSize);
            data.AddField("ul", systemLanguage);
        }

        foreach (KeyValuePair<int, string> kv in dimensions)
        {
            data.AddField("cd" + kv.Key, kv.Value);
        }

        dimensions.Clear();

        foreach (KeyValuePair<int, int> kv in metrics)
        {
            data.AddField("cm" + kv.Key, kv.Value);
        }

        metrics.Clear();

        if (_uid != null && _uid != "")
        {
            data.AddField("uid", _uid);
        }

        new WWW(UA_COLLECT_URL, data);
    }

    private static void LogDimensionsAndMetrics()
    {
        if (logToConsole)
        {
            StringBuilder msg = new StringBuilder("UA: Adding Dimensions: {");

            if (dimensions.Count > 0)
            {
                foreach (KeyValuePair<int, string> kv in dimensions)
                {
                    msg.Append(kv.Key + ": " + kv.Value + ", ");
                }

                msg.Remove(msg.Length - 2, 2);
                msg.Append("}");
                Debug.Log(msg.ToString());
            }

            if (metrics.Count > 0)
            {
                msg = new StringBuilder("UA: Adding Metrics: {");

                foreach (KeyValuePair<int, int> kv in metrics)
                {
                    msg.Append(kv.Key + ": " + kv.Value + ", ");
                }

                msg.Remove(msg.Length - 2, 2);
                msg.Append("}");
                Debug.Log(msg.ToString());
            }
        }
    }

    private static string WebMakeStringSafe(string s)
    {
        s = s.Replace("'", "\'");
        s = s.Replace("\n", "");
        s = s.Trim();

        return s;
    }

    private static string WebConstructExtraArgs()
    {
        StringBuilder strb = new StringBuilder("");

        // If there are any additional args then start off with ',' since the caller has not placed it!
        foreach (KeyValuePair<int, string> kv in dimensions)
        {
            strb.Append(",'dimension" + kv.Key + "': '" + WebMakeStringSafe(kv.Value) + "'");
        }

        dimensions.Clear();

        foreach (KeyValuePair<int, int> kv in metrics)
        {
            strb.Append(",'metric" + kv.Key + "': " + kv.Value);
        }

        metrics.Clear();

        return strb.ToString();
    }

    static void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            // Unfortunately we can only log up to 150 bytes which I find unfortunate...so cram it and hope.
            LogException(logString + stackTrace);
        }
    }

    /*
     * If this is set to true then UniversalAnalytics will attach to log messages and listen for an
     * exception log and will then send an exception data point to UA.
     * */
    public static bool autoHandleExceptionLogging 
    { 
        private get
        {
            return _autoExceptionLog;
        } 
        set
        {
            if (!initialized)
            {
                return;
            }

            if (value && value != _autoExceptionLog)
            {
                Application.RegisterLogCallback(HandleException);
            }
            else if (!value && value != _autoExceptionLog)
            {
                Application.RegisterLogCallback(null);
            }

            _autoExceptionLog = value;
        }
    }
    private static bool _autoExceptionLog = false;

    /*
     * If this is set to true then all logs will contain some system information.
     * */
    public static bool gatherSystemInformation
    {
        get
        {
            return _gatherSystemInfo;
        }
        set
        {
            _gatherSystemInfo = value;

            if (!initialized)
            {
                return;
            }

            if (_gatherSystemInfo)
            {
                screenResolution = Screen.width + "x" + Screen.height;
                viewportSize = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
                systemLanguage = Application.systemLanguage.ToString();

                if (Application.isWebPlayer)
                {
                    Application.ExternalEval(@"
                        ga('unityUATracker.set', 'screenResolution', '" + screenResolution + @"');
                        ga('unityUATracker.set', 'viewportSize', '" + viewportSize + @"');
                        ga('unityUATracker.set', 'language', '" + systemLanguage + @"');
                        ");
                }
            }
        }
    }
    private static bool _gatherSystemInfo;

    public static bool logToConsole { get; set; }

    /*
     * If Universal Analytics has been initialized.
     * */
    public static bool initialized { get; private set; }

    /*
     * The id of the user, could be a login name. Set to null or an empty string to disable.
     * */
    public static string userId 
    { 
        get
        {
            return _uid;
        }

        set
        {
            _uid = value;

            if (!initialized)
            {
                return;
            }

            if (Application.isWebPlayer)
            {
                if (_uid == null)
                {
                    // TODO: I don't know if this actually unsets it...
                    Application.ExternalEval("ga('unityUATracker.set', 'userId', '');");
                }
                else
                {
                    Application.ExternalEval("ga('unityUATracker.set', 'userId', '" + WebMakeStringSafe(_uid) + "');");
                }
            }
        }
    }
    private static string _uid;

}