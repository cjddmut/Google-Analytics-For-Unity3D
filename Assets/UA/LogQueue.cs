using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UA
{
    public class LogQueue : MonoBehaviour
    {
        private LinkedList<Log> logs = new LinkedList<Log>();

        public class Log
        {
            public WWWForm postData;
            public int frame;
            public float time;
        }

        // Update is called once per frame
        void Update()
        {
            if (logs.Count > 0)
            {
                // Push out the next set of logs based off when we received them. This hopefully prevents an huge amount going out at once.
                int frameToPush = logs.First.Value.frame;
                int curFrame = frameToPush;

                if (UniversalAnalytics.logToConsole)
                {
                    Debug.Log("UA: Sending queued messages.");
                }

                while (curFrame == frameToPush && logs.Count > 0)
                {
                    Log log = logs.First.Value;
                    log.postData.AddField("qt", (int)(Time.realtimeSinceStartup - log.time) * 1000);
                    StartCoroutine(SendWebData(log.postData, log.time));
                    logs.RemoveFirst();

                    if (logs.Count > 0)
                    {
                        curFrame = logs.First.Value.frame;
                    }
                }
            }
        }

        private void Queue(WWWForm data, int frame, float time)
        {
            Log log = new Log();
            log.postData = data;
            log.frame = frame;
            log.time = time;
            logs.AddLast(log);
        }

        public void SendData(WWWForm data)
        {
            StartCoroutine(SendWebData(data, Time.realtimeSinceStartup));
        }

        private IEnumerator SendWebData(WWWForm data, float time)
        {
            WWW www = new WWW(UniversalAnalytics.UA_COLLECT_URL, data);
            int frame = Time.frameCount;

            yield return www;
           
           if (www.error != null)
           {
               if (UniversalAnalytics.logToConsole)
               {
                   Debug.Log("UA: WWW request failed, queueing log.");
               }

               Queue(data, frame, time);
           }
        }
    }
}
