using DV.Logic.Job;
using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Jobs;
using System;

namespace Multiplayer.Patches.Jobs;

[HarmonyPatch(typeof(Task))]
public static class TaskPatch
{
    [HarmonyPatch(nameof(Task.SetState))]
    [HarmonyPrefix]
    public static void SetStatePrefix(Task __instance, TaskState newState)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;

        if (newState == TaskState.InProgress)
            return;


        //Multiplayer.LogDebug(()=>$"Task.SetState() called for jobId: {__instance.Job.ID}, taskType: {__instance.InstanceTaskType}, newState: {newState}");
        if(!NetworkedTask.TryGet(__instance, out var networkedTask))
        {
            Multiplayer.LogError($"Task.SetState() could not find NetworkedTask for jobId: {__instance.Job.ID}, taskType: {__instance.InstanceTaskType}");
            return;
        }

        networkedTask.SetState(newState);
    }

    [HarmonyPatch(nameof(Task.SetJobBelonging))]
    [HarmonyPostfix]
    public static void SetJobBelongingPostfix(Task __instance, Job Job)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;

        NetworkedJob.EnqueueTask(__instance, Job);
    }
}
