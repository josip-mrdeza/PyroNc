using System;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Configuration.Startup;
using Pyro.Nc.UI;
using UnityEngine;

namespace Pyro.Nc.Simulation
{
    public class CameraControl : InitializerRoot
    {
        [SerializeField]
        private bool doFocus = false;

        [SerializeField] private float focusLimit = 100f;
        [SerializeField] private float minFocusDistance = 5.0f;
        private float doubleClickTime = .15f;
        private float cooldown = 0;

        [Header("Undo - Only undoes the Focus Object - The keys must be pressed in order.")] [SerializeField]
        private KeyCode firstUndoKey = KeyCode.LeftControl;

        [SerializeField] private KeyCode secondUndoKey = KeyCode.Z;

        [Header("Movement")] [SerializeField] private float moveSpeed = 1.0f;
        [SerializeField] private float rotationSpeed = 10.0f;
        [SerializeField] private float zoomSpeed = 10.0f;

        //Cache last pos and rot be able to undo last focus object action.
        Quaternion prevRot = new Quaternion();
        Vector3 prevPos = new Vector3();

        [SerializeField]
        private string mouseY = "Mouse Y";

        [SerializeField]
        private string mouseX = "Mouse X";

        [SerializeField]
        private string zoomAxis = "Mouse ScrollWheel";

        [Header("Move Keys")] [SerializeField] private KeyCode forwardKey = KeyCode.W;
        [SerializeField] private KeyCode backKey = KeyCode.S;
        [SerializeField] private KeyCode leftKey = KeyCode.A;
        [SerializeField] private KeyCode rightKey = KeyCode.D;
        
        [SerializeField]
        private KeyCode flatMoveKey = KeyCode.LeftShift;
        
        [SerializeField]
        private KeyCode anchoredMoveKey = KeyCode.Mouse2;

        [SerializeField] private KeyCode anchoredRotateKey = KeyCode.Mouse1;

        public override void Initialize()
        {
            SavePosAndRot();
        }

        void Update()
        {
            if (!doFocus) return;

            //Double click for focus 
            if (cooldown > 0 && Input.GetKeyDown(KeyCode.Mouse0)) FocusObject();
            if (Input.GetKeyDown(KeyCode.Mouse0)) cooldown = doubleClickTime;

            //--------UNDO FOCUS---------
            if (Input.GetKey(firstUndoKey))
            {
                if (Input.GetKeyDown(secondUndoKey)) GoBackToLastPosition();
            }

            cooldown -= Time.deltaTime;
        }

        private void LateUpdate()
        {
            if (ViewHandler.Active)
            {
                return;
            }
            Vector3 move = Vector3.zero;

            //Move and rotate the camera

            var dt = Time.deltaTime;
            var ms = (moveSpeed * dt);
            if (Input.GetKey(forwardKey)) move += Vector3.forward * ms;
            if (Input.GetKey(backKey)) move += Vector3.back * ms;
            if (Input.GetKey(leftKey)) move += Vector3.left * ms;
            if (Input.GetKey(rightKey)) move += Vector3.right * ms;

            //By far the simplest solution I could come up with for moving only on the Horizontal plane - no rotation, just cache y
            if (Input.GetKey(flatMoveKey))
            {
                float origY = transform.position.y;

                transform.Translate(move);
                transform.position = new Vector3(transform.position.x, origY, transform.position.z);

                return;
            }

            float mouseMoveY = Input.GetAxis(mouseY);
            float mouseMoveX = Input.GetAxis(mouseX);

            //Move the camera when anchored
            if (Input.GetKey(anchoredMoveKey))
            {
                move += Vector3.up * mouseMoveY * -moveSpeed;
                move += Vector3.right * mouseMoveX * -moveSpeed;
            }

            //Rotate the camera when anchored
            if (Input.GetKey(anchoredRotateKey))
            {
                transform.RotateAround(transform.position, transform.right, mouseMoveY * -rotationSpeed);
                transform.RotateAround(transform.position, Vector3.up, mouseMoveX * rotationSpeed);
            }

            transform.Translate(move);

            //Scroll to zoom
            float mouseScroll = Input.GetAxis(zoomAxis);
            transform.Translate(Vector3.forward * mouseScroll * zoomSpeed);
        }

        private void FocusObject()
        {
            //To be able to undo
            SavePosAndRot();

            //If we double-clicked an object in the scene, go to its position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, focusLimit))
            {
                GameObject target = hit.collider.gameObject;
                Vector3 targetPos = target.transform.position;
                Vector3 targetSize = hit.collider.bounds.size;

                transform.position = targetPos + GetOffset(targetPos, targetSize);

                transform.LookAt(target.transform);
            }
        }

        private void SavePosAndRot()
        {
            prevRot = transform.rotation;
            prevPos = transform.position;
        }

        private void GoBackToLastPosition()
        {
            transform.position = prevPos;
            transform.rotation = prevRot;
        }

        private Vector3 GetOffset(Vector3 targetPos, Vector3 targetSize)
        {
            Vector3 dirToTarget = targetPos - transform.position;

            float focusDistance = Mathf.Max(targetSize.x, targetSize.z);
            focusDistance = Mathf.Clamp(focusDistance, minFocusDistance, focusDistance);

            return -dirToTarget.normalized * focusDistance;
        }
    }
}