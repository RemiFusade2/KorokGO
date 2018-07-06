using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMapOSM : MonoBehaviour
{
    public static CameraMapOSM Instance;

    [Header("References in scene")]
    public Transform player;
    public Transform cameraTransitingStartTransform;
    public Transform cameraTransitingEndTransform;
    private Transform mapItemTargetTransform;

    [Header("Camera attributes")]
    public float touchSensivity;
    public float transitionTime;
    public AnimationCurve transitionCurve;

    [Header("Camera relative to player")]
    public float heightDeltaWithPlayer;
    public float projectionDistanceDeltaWithPlayer;

    [Header("Camera status")]
    public bool cameraCanRotate;
    public bool cameraFollowsPlayer;
    public bool cameraIsInTransition;

    #region Smooth rotation

    private float currentAngle;
    private float targetAngle;

    #endregion

    // Use this for initialization
    void Start()
    {
        CameraMapOSM.Instance = this;
        currentAngle = 0; // North
        targetAngle = 0;
        cameraCanRotate = true;
        cameraFollowsPlayer = true;
        cameraIsInTransition = false;

        SetCameraTransitingEndTransformToPlayerTransform();
    }

    private void SetCameraTransitingEndTransformToPlayerTransform()
    {
        // Translation
        float forwardDistance = -Mathf.Cos(currentAngle * Mathf.Deg2Rad) * projectionDistanceDeltaWithPlayer;
        float rightDistance = -Mathf.Sin(currentAngle * Mathf.Deg2Rad) * projectionDistanceDeltaWithPlayer;
        cameraTransitingEndTransform.position = player.transform.position + new Vector3(rightDistance, heightDeltaWithPlayer, forwardDistance);

        // Rotation
        cameraTransitingEndTransform.LookAt(player, Vector3.up);
    }

    private bool isSwiping;
    private float angleWhenSwipeBegan;
    private Vector2 touchPositionWhenSwipeBegan;
    private bool touchIsAbovePlayer;

    // Update is called once per frame
    void Update()
    {
        if (cameraCanRotate)
        {
            // Input
            if ((Application.platform == RuntimePlatform.Android && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) ||
                    (Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButtonDown(0)) ||
                    (Application.platform == RuntimePlatform.WindowsPlayer && Input.GetMouseButtonDown(0))
                )
            {
                // swipe starts
                if ((Application.platform == RuntimePlatform.Android && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) ||
                        Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    try
                    {
                        isSwiping = true;
                        angleWhenSwipeBegan = currentAngle;
                        Vector2 touchPosition = (Application.platform == RuntimePlatform.Android) ? Input.GetTouch(0).position : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                        Vector3 playerPositionInScreen = this.GetComponent<Camera>().WorldToScreenPoint(player.position);
                        if (touchPosition.y > playerPositionInScreen.y)
                        {
                            // touch is above player position
                            touchIsAbovePlayer = true;
                        }
                        else
                        {
                            // touch is under player position
                            touchIsAbovePlayer = false;
                        }
                        touchPositionWhenSwipeBegan = touchPosition;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("CameraMapOSM Got exception during Update(): " + ex.Message);
                    }
                }
            }
            if (isSwiping)
            {
                try
                {
                    Vector2 touchPosition = (Application.platform == RuntimePlatform.Android) ? Input.GetTouch(0).position : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    float mouseScrollPixelsToCameraAngleDegrees = touchSensivity;
                    Vector3 playerPositionInScreen = this.GetComponent<Camera>().WorldToScreenPoint(player.position);
                    if (touchPosition.y > playerPositionInScreen.y)
                    {
                        // touch is above player position
                        if (!touchIsAbovePlayer)
                        {
                            // touch change position relative to player
                            touchPositionWhenSwipeBegan = touchPosition;
                            angleWhenSwipeBegan = currentAngle;
                            touchIsAbovePlayer = true;
                        }
                        targetAngle = angleWhenSwipeBegan - (touchPosition.x - touchPositionWhenSwipeBegan.x) * mouseScrollPixelsToCameraAngleDegrees;
                    }
                    else
                    {
                        // touch is under player position
                        if (touchIsAbovePlayer)
                        {
                            // touch change position relative to player
                            touchPositionWhenSwipeBegan = touchPosition;
                            angleWhenSwipeBegan = currentAngle;
                            touchIsAbovePlayer = false;
                        }
                        targetAngle = angleWhenSwipeBegan + (touchPosition.x - touchPositionWhenSwipeBegan.x) * mouseScrollPixelsToCameraAngleDegrees;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("CameraMapOSM Got exception during Update(): " + ex.Message);
                }
            }
            if ((Application.platform == RuntimePlatform.Android && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) ||
                    (Application.platform == RuntimePlatform.WindowsEditor && Input.GetMouseButtonUp(0)) ||
                    (Application.platform == RuntimePlatform.WindowsPlayer && Input.GetMouseButtonUp(0))
                )
            {
                if ((Application.platform == RuntimePlatform.Android && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) ||
                        Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    isSwiping = false;
                }
            }
        }

        if (cameraFollowsPlayer)
        {
            SetCameraTransitingEndTransformToPlayerTransform();
            SmoothCurrentAngleTowardsTargetAngle();
            this.transform.position = cameraTransitingEndTransform.position;
            this.transform.rotation = cameraTransitingEndTransform.rotation;
        }
    }

    private void SmoothCurrentAngleTowardsTargetAngle()
    {
        currentAngle = targetAngle;
    }

    public void CameraBackToPlayer()
    {
        cameraTransitingStartTransform.position = cameraTransitingEndTransform.position;
        cameraTransitingStartTransform.rotation = cameraTransitingEndTransform.rotation;
        SetCameraTransitingEndTransformToPlayerTransform();
        cameraIsInTransition = true;
    }
}
