using System;

public class AutoRunParams
{
    public bool autoRun
    { get; private set; } = false;
    public bool autoClose
    { get; private set; } = false;

    public void ReadParameter()
    {
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            switch (arg)
            {
                case "-autoRun":
                    autoRun = true;
                    break;
                case "-autoClose":
                    autoClose = true;
                    break;
            }
        }
    }
}