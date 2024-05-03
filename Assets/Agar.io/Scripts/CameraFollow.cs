using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;
public class CameraFollow : NetworkBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    public float checkorthoSizeTime = 0.05f;
    public Transform playerTransform;
    public float offset = 2f;
    public GameObject VC;


    private void Start()
    {
        //UIController.instance.VirtualCamera.GetComponent<CinemachineVirtualCamera>();
        //virtualCamera = VC.GetComponent<CinemachineVirtualCamera>();
        VC = UIController.instance.VirtualCamera;
        virtualCamera = VC.GetComponent<CinemachineVirtualCamera>();
        if (isLocalPlayer)
        {
            virtualCamera.Follow = this.transform;

        }
        virtualCamera.m_Lens.OrthographicSize = 150;

    }

    private void LateUpdate()
    {
        AdjustCamera();
    }


    public void AdjustCamera()
    {


        StartCoroutine(AdjustOrthoSize());

    }

    public IEnumerator AdjustOrthoSize()
    {
        yield return new WaitForSecondsRealtime(checkorthoSizeTime);



        if (playerTransform != null || virtualCamera != null)
        {

            float previousSize = virtualCamera.m_Lens.OrthographicSize;
            float targetOrthoSize = playerTransform.localScale.x + offset;
            if (previousSize != targetOrthoSize)
            {
                //Debug.Log($"<color=aqua>Changing camera size from {previousSize} to {targetOrthoSize}</color>");
            }
            //virtualCamera.m_Lens.OrthographicSize = targetOrthoSize;
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(previousSize, targetOrthoSize, Time.deltaTime * 5f);
        }

    }













}
