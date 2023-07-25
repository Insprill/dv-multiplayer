using System;
using UnityEngine;

namespace Multiplayer.Utils;

public class ExecutionTimer
{
    private readonly float warnLimit = float.MaxValue;

    private bool isRunning;
    private float startTime;

    public ExecutionTimer()
    { }

    public ExecutionTimer(float warnLimit)
    {
        this.warnLimit = warnLimit;
    }

    public void Start()
    {
        if (isRunning) throw new InvalidOperationException("Timer is already running");
        isRunning = true;
        startTime = Time.realtimeSinceStartup;
    }

    public void Stop(Action<int> message)
    {
        float elapsedTime = Stop();
        if (elapsedTime > warnLimit)
            message.Invoke(Mathf.CeilToInt(elapsedTime * 1000));
    }

    public float Stop()
    {
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        if (!isRunning) throw new InvalidOperationException("Timer isn't running");
        isRunning = false;
        return elapsedTime;
    }
}
