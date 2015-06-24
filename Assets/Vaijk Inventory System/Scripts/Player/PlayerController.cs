using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float walkingSpeed;

    private void Update ()
    {
        float horizontalSpeed = Input.GetAxis("Horizontal");
        float verticalSpeed = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalSpeed, 0f, verticalSpeed);

        moveDirection = transform.TransformDirection(moveDirection);

        transform.position += moveDirection * walkingSpeed * Time.deltaTime;
    }
}