﻿using UnityEngine.UI;
using UnityEngine;

public class FocusController : MonoBehaviour
{
    //public Text angle;
    private Transform cameraRig;

    private Transform verticalRig;

    [SerializeField, Range(1f, 10f)] protected float zoomSpeed = 7.5f, zoomDelta = 5f;

    [SerializeField, Range(1f, 10f)] protected float rotateSpeed = 7.5f, rotateDelta = 5f;
    [SerializeField, Range(1f, 10f)] protected float dragSpeed = 1.0f;
    [SerializeField, Range(0.1f, 30.0f)] protected float limitDistance = 5.0f;

    private Vector3 camPosition; // 相机的位置 --> 滚轮移动
    private Vector3 rigPosition; // CameraRig的位置，用于鼠标中间拖拽
    private Quaternion rigRotation; // CameraRig的旋转，用于水平旋转相机
    private Quaternion verticalRotation; // VerticalRig的旋转，用于垂直旋转相机

    public bool cursor_control_lock = false; // true则禁止一切鼠标控制
    private Vector3 cameraRigOriginPos; // 记录camera rig的初始位置
    private Quaternion rigOriginRot; // 记录camera rig的初始旋转
    private Quaternion vertOriginRot; // 记录Vertical Rig的初始旋转
    private Vector3 camOriginPos; // 记录camera group的位置

    protected void Awake()
    {
        cameraRig = transform.parent.transform.parent.transform;
        verticalRig = transform.parent.transform;

        camPosition = transform.localPosition;

        rigPosition = cameraRig.position;
        rigRotation = cameraRig.rotation;

        verticalRotation = verticalRig.localRotation;

        // origin record
        cameraRigOriginPos = cameraRig.position;
        rigOriginRot = cameraRig.rotation;
        vertOriginRot = verticalRotation;
        camOriginPos = camPosition;
    }

    protected void Update()
    {
        Zoom();
        Drag();
        Rotate();
        cameraRig.position = Vector3.Lerp(cameraRig.position, rigPosition, Time.deltaTime * zoomSpeed);
    }

    public void LocateThermoCouple(Vector3 target, float offset)
    {
        // 这样防止鼠标拖动的移动未完成时出现移动不到位的bug
        camPosition = transform.localPosition;
        rigPosition = cameraRig.position;
        verticalRotation = verticalRig.localRotation;
        rigRotation = cameraRig.rotation;

        Vector3 center = new Vector3(0, target.y, 0);
        rigPosition = center; // change CameraRig height
        float zDis = Vector3.Distance(center, target); // 计算目标热电偶和中心轴的水平距离
        camPosition = new Vector3(0, 0, -(zDis + offset)); // 计算 CameraGroup的位置，即加上偏移量

        Vector3 cam_pos = new Vector3(transform.position.x, target.y, transform.position.z); // 构造一个调整后的世界空间相机位置
        float h_angle = Vector3.SignedAngle(cam_pos - center, target - center, Vector3.up); // 计算相机水平需要旋转的角度
        float v_angle = verticalRig.rotation.eulerAngles.x; // 直接获得Verticalrig的绕x轴的角度，以便重置

        rigRotation *= Quaternion.AngleAxis(h_angle, Vector3.up); // 水平旋转
        verticalRotation *= Quaternion.AngleAxis(-v_angle, Vector3.right); // 垂直旋转
    }

    protected void Zoom()
    {
        if (!cursor_control_lock)
        {
            float amount = Input.GetAxis("Mouse ScrollWheel");
            float dis = Vector2.Distance(transform.localPosition, Vector3.zero);
            camPosition += new Vector3(0.0f, 0.0f, 1.0f) * zoomSpeed * amount;
            Vector3 inv_dir = (transform.localPosition).normalized;
            Vector3 target_inv_dir = camPosition;

            float dot = Vector3.Dot(inv_dir, target_inv_dir);
            if (dot < 0.0f)
            {
                camPosition = inv_dir * limitDistance;
            }
            else
            {
                if (target_inv_dir.magnitude <= limitDistance)
                {
                    camPosition = inv_dir * limitDistance;
                }
            }
            transform.localPosition = Vector3.Lerp(transform.localPosition, camPosition, Time.deltaTime * zoomDelta);
        }
    }

    protected void Rotate()
    {
        if (Input.GetMouseButton(1) && !cursor_control_lock)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;

            rigRotation *= Quaternion.AngleAxis(-mouseX, Vector3.up);

            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

            verticalRotation *= Quaternion.AngleAxis(-mouseY, Vector3.right);
        }
        else
        {
            float h = Input.GetAxis("Horizontal") * rotateSpeed;

            rigRotation *= Quaternion.AngleAxis(-h, Vector3.up);

            float v = Input.GetAxis("Vertical") * rotateSpeed;

            verticalRotation *= Quaternion.AngleAxis(v, Vector3.right);
        }
        verticalRig.localRotation = Quaternion.Slerp(verticalRig.localRotation, verticalRotation, Time.deltaTime * rotateDelta); // 旋转vertical rig
        cameraRig.rotation = Quaternion.Slerp(cameraRig.rotation, rigRotation, Time.deltaTime * rotateDelta); // 旋转camera rig

        // 显示当前相机的角度
        // TODO 这一步的角度显示有范围问题，仍需调整
        //angle.text = "水平视角:" + (Mathf.RoundToInt(cameraRig.rotation.eulerAngles.y - 90.0f)).ToString()
        //            + " 垂直视角:" + (Mathf.RoundToInt(verticalRig.localRotation.eulerAngles.x)).ToString();
    }

    protected void Drag()
    {
        if (Input.GetMouseButton(2) && !cursor_control_lock)
        {
            float mouseX = Input.GetAxis("Mouse X") * dragSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * dragSpeed;

            rigPosition += (-mouseX * cameraRig.right - mouseY * cameraRig.up) * Time.deltaTime * dragSpeed;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            rigPosition = cameraRigOriginPos; // 重置相机位置
        }
    }

    public void FullyReset()
    {
        rigPosition = cameraRigOriginPos;
        rigRotation = rigOriginRot;
        verticalRotation = vertOriginRot;
        camPosition = camOriginPos;
    }

    public void ResetCameraPos()
    {
        rigPosition = cameraRigOriginPos;
    }

    public void ChangeCameraRigHeight(float h)
    {
        cameraRigOriginPos = new Vector3(0, h, 0);
        rigPosition = cameraRigOriginPos;
    }

    public float GetCurrentCameraHeight()
    {
        return rigPosition.y;
    }

    public void DisableEnableMouseControl()
    {
        cursor_control_lock = !cursor_control_lock;
    }
}