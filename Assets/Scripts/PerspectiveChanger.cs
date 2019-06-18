﻿using UnityEngine;
using UnityEngine.Events;
using VRTK;

/// <summary>
/// This component allows teleporting to a target game object and have the player replace them,
/// with custom blink transition times to improve the feel of the POV change.
/// </summary>
public class PerspectiveChanger : MonoBehaviour
{
    [Tooltip("The target location the player will be teleported to on entering the sphere")]
    [SerializeField] Transform target;

    [Tooltip("The custom blink distance to use for the teleport to allow for a longer blink when teleporting to a nearby location")]
    [SerializeField, Range(0f, 32.9f)] float blinkDistance = 20;

    [Tooltip("The custom blink transition time to use for the teleport")]
    [SerializeField, Range(0, 10)] float blinkTransition = 2;

    [Tooltip("The scene objects to be scaled on the perspective change")]
    [SerializeField] Transform sceneObjects;

    [Tooltip("The scale to change the objects to be when the perspective change occurs")]
    [SerializeField] float newSceneScale;

    [Tooltip("The offset to teleport by")]
    [SerializeField] Vector3 offset;

    [SerializeField] UnityEvent afterTeleport;

    [SerializeField] bool scalePosition;

    [SerializeField] bool scaleCamera;

    [SerializeField] VRTK_BasicTeleport teleporter;

    public void DoTeleport()
    {
        bool scaleRoom = sceneObjects && newSceneScale > 0;

        var teleportPosition = (target.position * (scaleRoom && scalePosition ? newSceneScale : 1)) + offset;

        var teleporter = this.teleporter ?? FindObjectOfType<VRTK_BasicTeleport>();
        var originalBlinkDelay = teleporter.distanceBlinkDelay;
        var originalBlinkTransition = teleporter.blinkTransitionSpeed;

        teleporter.distanceBlinkDelay = blinkDistance;
        teleporter.blinkTransitionSpeed = blinkTransition;

        if (scaleRoom)
        {
            sceneObjects.localScale = new Vector3(newSceneScale, newSceneScale, newSceneScale);
        }

        float rotationY;

        if (VRTK_DeviceFinder.GetHeadsetTypeAsString() == "simulator")
        {
            // The simulator Y rotation is done by the play area and not the headset transform...
            rotationY = Vector3.SignedAngle(Vector3.forward, target.forward, Vector3.up);
        }
        else
        {
            rotationY = Vector3.SignedAngle(VRTK_DeviceFinder.HeadsetTransform().forward, target.forward, Vector3.up);
        }

        teleporter.Teleport(target, teleportPosition, Quaternion.Euler(0, rotationY, 0));

        if (scaleCamera)
        {
            VRTK_SDKManager.instance.transform.localScale = new Vector3(newSceneScale, newSceneScale, newSceneScale);
        }

        afterTeleport?.Invoke();

        Destroy(target.gameObject);

        teleporter.distanceBlinkDelay = originalBlinkDelay;
        teleporter.blinkTransitionSpeed = originalBlinkTransition;
    }
}