using System.Collections;
using System.Collections.Generic;
using DV.Logic.Job;
using UnityEngine;

namespace Multiplayer.Components.Networking.World;

public class NetworkedStation : MonoBehaviour
{
    private List<Job> jobs;
    private StationController stationController;

    public void AddJob(Job job)
    {
        jobs.Add(job);
    }


    private void Awake()
    {
        Multiplayer.Log("NetworkedStation.Awake()");

        stationController = GetComponent<StationController>();
        StartCoroutine(WaitForLogicStation());
    }

    private IEnumerator WaitForLogicStation()
    {
        while (stationController.logicStation == null)
            yield return null;

        StationComponentLookup.Instance.RegisterStation(stationController);

        jobs = new List<Job>(stationController.logicStation.availableJobs);

        if (NetworkLifecycle.Instance.IsHost())
        {
            stationController.logicStation.JobAddedToStation += OnJobAddedToStation;
            NetworkLifecycle.Instance.OnTick += Server_OnTick;
        }

        Multiplayer.Log("NetworkedStation.Awake() done");
    }

    private void OnJobAddedToStation()
    {
        foreach (var job in stationController.logicStation.availableJobs)
        {
            jobs.Add(job);
        }

        Multiplayer.Log("NetworkedStation.OnJobAddedToStation()");
    }

    private void Server_OnTick(uint tick)
    {
        if (jobs.Count == 0)
            return;

        NetworkLifecycle.Instance.Server.SendJobsToAll(jobs.ToArray(), stationController.logicStation.ID);
        jobs.Clear();

        Multiplayer.Log("NetworkedStation.Server_OnTick()");
    }
}
