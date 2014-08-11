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

    public void TestSceenView()
    {
        UniversalAnalytics.LogScreenView("ScreenView1");
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
