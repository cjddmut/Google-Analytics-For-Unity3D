using UnityEngine;

namespace UAUnity
{

[RequireComponent(typeof(BoxCollider))]
public class TestScript : MonoBehaviour
{
    public delegate void TestDelegate();
    public TestDelegate testCase;
    public string delegateName;

    public string applicationVersion;

    // Analytics information.
    private const string TRACKING_ID = "UA-53559036-3";
    private const string APPLICATION_NAME = "UniversalAnalyticsForUnity";

    void Awake()
    {
        if (testCase == null && delegateName != null && delegateName != "")
        {
            // Set up.
            testCase = (TestScript.TestDelegate)System.Delegate.CreateDelegate(
                typeof(TestScript.TestDelegate), 
                this, 
                GetType().GetMethod(delegateName));
        }

        if (!UniversalAnalytics.initialized)
        {
            UniversalAnalytics.Initialize(TRACKING_ID, APPLICATION_NAME);
            UniversalAnalytics.gatherSystemInformation = true;
            UniversalAnalytics.autoHandleExceptionLogging = true;
            UniversalAnalytics.logToConsole = true;
            UniversalAnalytics.queueLogs = true;
        }
    }

    //
    // Test Screen Views
    //

    public void TestSceenView()
    {
        UniversalAnalytics.LogScreenView("ScreenViewTest");
    }

    public void TestSceenView2()
    {
        UniversalAnalytics.LogScreenView("ScreenViewTest2");
    }

    //
    // Test Sessions
    //

    public void TestSessions()
    {
        UniversalAnalytics.StartSessionOnNextHit();
        UniversalAnalytics.LogScreenView("SessionTestScreenViewStart");
        Invoke("EndSession", 5);
    }

    void EndSession()
    {
        UniversalAnalytics.EndSessionOnNextHit();
        UniversalAnalytics.LogScreenView("SessionTestScreenViewEnd");
    }

    //
    // Test Persistent Metrics/Dimensions
    //

    public void TestUserDimensionMetric()
    {
        UniversalAnalytics.userId = "TestUserDimensionMetric User";
        UniversalAnalytics.SetDimension(3, "UserDimension");
        UniversalAnalytics.SetMetric(3, 3);
        UniversalAnalytics.LogEvent("UAUnity", "TestUserDimensionMetric", "With Persisted DM");
        UniversalAnalytics.LogScreenView("TestUserDimensionMetric Screen View");

        // Does this even work?
        UniversalAnalytics.UnsetDimension(3);
        UniversalAnalytics.UnsetMetric(3);

        UniversalAnalytics.LogEvent("UAUnity", "TestUserDimensionMetric", "Unset Persisted DM");
        UniversalAnalytics.userId = "";
    }

    public void TestSessionDimensionMetric()
    {
        UniversalAnalytics.StartSessionOnNextHit();
        UniversalAnalytics.SetDimension(2, "SessionDimension");
        UniversalAnalytics.SetMetric(2, 3);
        UniversalAnalytics.LogEvent("UAUnity", "TestSessionDimensionMetric", "With Persisted DM");
        UniversalAnalytics.LogScreenView("TestSessionDimensionMetric Screen View Start");

        Invoke("EndSessionDimensionMetric", 5);
    }

    void EndSessionDimensionMetric()
    {
        UniversalAnalytics.EndSessionOnNextHit();

        // Does this even work?
        UniversalAnalytics.UnsetDimension(2);
        UniversalAnalytics.UnsetMetric(2);

        UniversalAnalytics.LogEvent("UAUnity", "TestSessionDimensionMetric", "Unset Persisted DM");
        UniversalAnalytics.LogScreenView("TestSessionDimensionMetric Screen View End");
    }

    //
    // Test User IDs
    //
    public void TestUserIds()
    {
        UniversalAnalytics.userId = "TestUserIds User";
        UniversalAnalytics.LogScreenView("TestUserIds Screen View With User");
        UniversalAnalytics.userId = "";
        UniversalAnalytics.LogScreenView("TestUserIds Screen View Without User");
    }

    void OnMouseDown()
    {
        if (testCase != null)
        {
            testCase();
        }
    }
}
}
