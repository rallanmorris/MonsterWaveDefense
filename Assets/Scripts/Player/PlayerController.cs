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
    private Vector2 lookTopDownInput;
    Vector3 velocity;
    Vector3 raycastHitPoint = Vector3.zero;
    Vector3 fRayHitPoint = Vector3.zero;
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

            //transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        Ray forwardRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(forwardRay, out RaycastHit fRaycastHit, 999f, aimColliderLayerMask))
        {
            fRayHitPoint = fRaycastHit.point;
        }

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray reticleRay = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(reticleRay, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
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

            if (fireGun && !isFiring)
            {
                Vector3 aimDir = (raycastHitPoint - spawnBulletPosition.position).normalized;
                Instantiate(bulletPF, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                isFiring = true;
                fireGun = false;
            }

        }
        else
        {
            if (fireGun && !isFiring)
            {
                Vector3 aimDir = (fRayHitPoint - spawnBulletPosition.position);
                Instantiate(bulletPF, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
                isFiring = true;
                fireGun = false;
            }
        }
        

        if(jumpButtonDown && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    public void Movement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void AimTopDown(InputAction.CallbackContext context)
    {
        Vector2 up = new Vector2(0, 1);
        Vector2 down = new Vector2(0, -1);
        Vector2 left = new Vector2(-1, 0);
        Vector2 right = new Vector2(1, 0);

        //Gets dpad input
        lookTopDownInput = context.ReadValue<Vector2>();

        //Get Reticle input
        Vector3 worldAimTarget = raycastHitPoint;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = transform.forward;

        //Set aim direction
        if(lookTopDownInput.Equals(up))
        {
            aimDirection = (worldAimTarget - transform.position).normalized;
        }
        else if (lookTopDownInput.Equals(down))
        {
            aimDirection = (worldAimTarget - transform.position).normalized;
            aimDirection = -aimDirection;
        }
        else if (lookTopDownInput.Equals(left))
        {
            aimDirection = Vector3.Cross((worldAimTarget - transform.position).normalized, transform.up).normalized;
        }
        else if (lookTopDownInput.Equals(right))
        {
            aimDirection = Vector3.Cross((worldAimTarget - transform.position).normalized, transform.up).normalized;
            aimDirection = -aimDirection;
        }
        else
        {
            aimDirection = transform.forward;
        }

        //Set forward to aim direction
        transform.forward = aimDirection;
        
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
