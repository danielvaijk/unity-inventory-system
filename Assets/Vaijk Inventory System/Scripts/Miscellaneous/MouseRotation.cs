using UnityEngine;
using System.Collections;

public class MouseRotation : MonoBehaviour
{
    public RotationAxis rotationAxis = RotationAxis.XY;

    public float sensitivityX;
    public float sensitivityY;

    public float minimumY;
    public float maximumY;

    private float rotationY = 0f;
    private float rotationX = 0f;

    public enum RotationAxis { XY, X, Y };

    private void Update ()
    {
        if (rotationAxis == RotationAxis.XY)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0f);
        }
        else if (rotationAxis == RotationAxis.X)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rotationX, 0f);
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0f);
        }
    }
}