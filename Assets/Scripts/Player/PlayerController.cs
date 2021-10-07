using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] float speed = 6f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] Transform playerCamera;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float jumpHeight = 3f;
    [SerializeField] GameObject regCam;
    [SerializeField] GameObject aimCam;
    [SerializeField] GameObject aimBars;
    [SerializeField] LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] Transform debugTransform;
    [SerializeField] Transform bulletPF;
    [SerializeField] Transform spawnBulletPosition;

    private float turnSmoothVelocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    Vector3 velocity;
    Vector3 raycastHitPoint = Vector3.zero;
    bool isGrounded;
    bool jumpButtonDown = false;
    bool isFiring;
    bool fireGun;
    bool isAiming;

    private void Awake()
    {
        jumpButtonDown = false;
        isFiring = false;
        fireGun = false;
        isAiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        raycastHitPoint = Vector3.zero;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = moveInput.x;
        float vertical = moveInput.y;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray aimRay = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(aimRay, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            //debugTransform.position = raycastHit.point;
            raycastHitPoint = raycastHit.point;
        }

        if (isAiming)
        {
            Vector3 worldAimTarget = raycastHitPoint;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            /*
            float horizontalLook = lookInput.x;
            float verticalLook = lookInput.y;
            Vector3 directionLook = new Vector3(horizontalLook, 0f, verticalLook).normalized;

            float targetAngle = Mathf.Atan2(directionLook.x, directionLook.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.01f);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            **/
        }
        
        if(jumpButtonDown && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (fireGun && !isFiring)
        {
            Vector3 aimDir = (raycastHitPoint - spawnBulletPosition.position).normalized;
            Instantiate(bulletPF, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
            isFiring = true;
            fireGun = false;
        }
    }

    public void Movement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > .1f)
            jumpButtonDown = true;
        else
            jumpButtonDown = false;
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > .1f)
        {
            regCam.SetActive(false);
            aimCam.SetActive(true);
            aimBars.SetActive(true);
            isAiming = true;
        }
        else
        {
            regCam.SetActive(true);
            aimCam.SetActive(false);
            aimBars.SetActive(false);
            isAiming = false;
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > .1f && !isFiring)
        {
            fireGun = true;
        }
        else if(context.ReadValue<float>() < .01f && isFiring)
            isFiring = false;
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
