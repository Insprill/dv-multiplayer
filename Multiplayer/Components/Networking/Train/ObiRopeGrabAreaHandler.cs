using DV.CabControls;
using DV.Interaction;
using DV.Simulation.Controllers;

namespace Multiplayer.Components.Networking.Train;

internal class ObiRopeGrabAreaHandler : ControlImplBase
{
    WhistleRopeController ropeController;
    ObiRopeGrabArea[] grabAreas;
    TrainCar trainCar;

    // Match LeverBase - this won't be used anyway
    public override InteractionHandPoses GenericHandPoses { get; } = new InteractionHandPoses(HandPose.PreGrab, HandPose.PreGrab, HandPose.Grab);

    protected void Awake()
    {
        grabAreas = GetComponentsInChildren<ObiRopeGrabArea>();
        trainCar = TrainCar.Resolve(this.gameObject);

        ropeController = GetComponentInParent<WhistleRopeController>();
        Multiplayer.LogDebug(() => $"ropeController was {(ropeController != null ? "" : "not ")}found");
    }

    public override void AcceptSetValue(float newValue)
    {
        Multiplayer.LogDebug(() => $"AcceptSetValue({newValue}) on rope grab area on {trainCar?.ID}");
    }

    public override void ForceEndInteraction()
    {
        // May need something here to drop the rope
        Multiplayer.LogDebug(() => $"Force rope end interaction on {trainCar?.ID}");
        foreach (var grabArea in grabAreas)
            grabArea?.EndGrab();
    }

    public override bool IsGrabbed()
    {
        Multiplayer.LogDebug(() => $"Rope grabbed on {trainCar?.ID}");

        if (grabAreas == null || grabAreas.Length == 0)
            return false;

        bool canGrab = true;
        //return !grabArea.CanGrab();
        foreach (var grabArea in grabAreas)
            if (!grabArea.CanGrab())
            {
                canGrab = false;
                break;
            }

        return !canGrab;
    }

    internal void OnGrabbed()
    {
        Multiplayer.LogDebug(() => $"Rope grabbed on {trainCar?.ID}");
        FireGrabbed();
    }

    internal void OnUngrabbed()
    {
        Multiplayer.LogDebug(() => $"Rope ungrabbed on {trainCar?.ID}");
        FireUngrabbed();
    }

    public override void OnInteractionAllowedChanged(bool value)
    {
        base.OnInteractionAllowedChanged(value);

        if (ropeController != null)
            ropeController.enabled = value;

        foreach (var grabArea in grabAreas)
            if (grabArea != null)
                grabArea.grabCollider.enabled = value;
    }
}
