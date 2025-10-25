using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MainScript : MonoBehaviour
{

    public GameObject testObject;     // the stimulus capsule

    int phase = 1;                    // 1 = show stimulus, 2 = get response
    int trial = 1, ntrials = 40;      // trial counter
    float theta = 0, startTime = 0;   // stimulus orientation, trial start time
    string filename;                  // data file

    void Start()
    {
        // seed random number generator from clock
        int rngseed = (int)System.DateTime.Now.Ticks;
        Random.InitState(rngseed);

        // create data filename and write header
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        filename = $"data_{timestamp}.txt";
        using (StreamWriter writer = new StreamWriter(filename, true))
            writer.WriteLine("#trial,orientation,responseRight,rt");
    }

    void Update()
    {
        if (phase == 1)  // show stimulus for new trial
        {
            // show capsule at a random orientation
            theta = Random.Range(-30f, 30f);
            testObject.transform.rotation = Quaternion.Euler(0f, 0f, -theta);

            // move to next phase; record start time
            phase = 2;
            startTime = Time.time;
        }
        else if (phase == 2)  // get subject's response
        {
            // check keypresses
            bool responseLeft = Input.GetKeyDown(KeyCode.Alpha1);
            bool responseRight = Input.GetKeyDown(KeyCode.Alpha2);
            bool responseScreenCapture = Input.GetKeyDown(KeyCode.S);
            bool responseQuit = Input.GetKeyDown(KeyCode.Alpha0);

            // save screen capture
            if (responseScreenCapture)
            {
                string directory = Directory.GetCurrentDirectory();
                ScreenCapture.CaptureScreenshot(directory + "/screenshot.png");
            }

            // quit experiment early
            if (responseQuit)
                Quit();

            // left/right response
            if (responseRight || responseLeft)
            {
                // find response time
                float rt = Time.time - startTime;

                // save trial information
                string dataline = $"{trial},{theta},{responseRight},{rt}";
                using (StreamWriter writer = new StreamWriter(filename, true))
                    writer.WriteLine(dataline);

                // increase trial counter; quit if done
                trial = trial + 1;
                if (trial > ntrials)
                    Quit();

                // move to next phase
                phase = 1;
            }
        }
    }
    
    void Quit()
    {
#if UNITY_EDITOR
        // this is how to quit when running the experiment from the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // this is how to quit when running the experiment as a built project
        UnityEngine.Application.Quit();
#endif
     }

}
