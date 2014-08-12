using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

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
        }
    }

    //
    // Test Screen Views
    //

    public void TestSceenView()
    {
        UniversalAnalytics.LogScreenView("ScreenViewTest");
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
    
[CustomEditor(typeof(TestScript))]
public class TestScriptEditor : Editor
{
    private List<MethodInfo> testMethods = new List<MethodInfo>();
    private string[] names;
    private int index;
    private TestScript ts;

    void OnEnable()
    {
        System.Type type = target.GetType();

        // Get members.
        MethodInfo[] methods = type.GetMethods();

        foreach(var method in methods)
        {
            // Only care if it starts with the word "Test".
            if (method.Name.Substring(0, 4).Equals("Test"))
            {
                testMethods.Add(method);
            } 
        }

        ts = (TestScript)target; 
        names = new string[testMethods.Count + 1];
        index = testMethods.Count;

        for (int i = 0; i < testMethods.Count; i++)
        {
            names[i] = testMethods[i].Name;

            if(ts.delegateName != null && ts.delegateName == names[i])
            {
                index = i;
            }
        }

        // Delegates are not a serialized type so set it up if defined;
        if (index != testMethods.Count)
        {
            ts.testCase = (TestScript.TestDelegate)System.Delegate.CreateDelegate(
                typeof(TestScript.TestDelegate), 
                ts, 
                testMethods[index]);
        }

        names[testMethods.Count] = "None";
    }

    public override void OnInspectorGUI()
    {
        index = EditorGUILayout.Popup(index, names);

        if (index != testMethods.Count)
        {
            ts.delegateName = names[index];
            ts.testCase = (TestScript.TestDelegate)System.Delegate.CreateDelegate(
                typeof(TestScript.TestDelegate),
                ts,
                testMethods[index]);
        }
        else
        {
            ts.delegateName = null;
            ts.testCase = null;
        }

        EditorGUILayout.Separator();
    }
}
    
}
