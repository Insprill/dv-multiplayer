using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DV.Logic.Job;
using UnityEngine;

namespace Multiplayer.Components.Networking.World;

public class NetworkedStation : MonoBehaviour
{
    private List<Job> jobs;
    private StationController stationController;

    public void AddJob(Job job)
    {
        Multiplayer.Log("NetworkedStation.AddJob()");
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
            //stationController.logicStation.JobAddedToStation += OnJobAddedToStation;
            NetworkLifecycle.Instance.OnTick += Server_OnTick;
        }

        Multiplayer.Log("NetworkedStation.Awake() done");
    }

   
   //private void OnJobAddedToStation()
   // {
   //     Multiplayer.Log("OnJobAddedToStation");

   //     UpdateCarPlates(job.tasks);
                
   // }

    public static void UpdateCarPlates(List<DV.Logic.Job.Task> tasks,string jobId)
    {
        Task task = tasks.First();
        List<Car> cars = null;

        if (task is WarehouseTask)
        {
            Multiplayer.Log("NetworkedStation.UpdateCarPlates() WarehouseTask");
            cars = ((WarehouseTask)task).cars;
        }else if (task is TransportTask)
        {
            Multiplayer.Log("NetworkedStation.UpdateCarPlates() TransportTask");
            cars = ((TransportTask)task).cars;
        }else if(task is SequentialTasks)
        {
            Multiplayer.Log("NetworkedStation.UpdateCarPlates() SequentialTasks");
            List<Task> seqTask = [((SequentialTasks)task).currentTask.Value];

            Multiplayer.Log("NetworkedStation.UpdateCarPlates() Calling UpdateCarPlates()");
            //drill down
            UpdateCarPlates(seqTask, jobId);
        }else if(task is ParallelTasks)
        {
            //not implemented
            Multiplayer.Log("NetworkedStation.UpdateCarPlates() ParallelTasks - not implemented");
        }

        /*
         * This section could be optimised/refactored
         */
        if (cars != null)
        {
            Multiplayer.Log("NetworkedStation.UpdateCarPlates() Cars count: " + cars.Count);

            foreach (Trainset trainset in Trainset.allSets)
            {
                foreach(TrainCar traincar in trainset.cars)
                {
                    foreach(Car car in cars)
                    {
                        Multiplayer.Log("NetworkedStation.UpdateCarPlates() car.ID: " + car.ID + " traincar.ID: " + traincar.ID);
                        if (car.ID == traincar.ID)
                        {
                            Multiplayer.Log("NetworkedStation.UpdateCarPlates() Calling  traincar.UpdateJobIdOnCarPlates()");
                            traincar.UpdateJobIdOnCarPlates(jobId);
                        }
                    }
                    
                }
            }

        }

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
