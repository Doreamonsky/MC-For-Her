﻿using UnityEngine;
using UnityEngine.UI;

namespace MC.Core
{
    public struct InputData
    {
        public float x, y;
    }

    public class Player : MonoBehaviour
    {
        public System.Action<InputData> OnControllerInput;

        public float velocity = 10;

        public float jumpVelocity = 8.0f;

        public float gravity = -5f;

        public int blockID = 2;

        public bool isCreatorMode = false;

        public GameObject craftingUI;

        public InventorySystem inventorySystem;

        public Button openCraftingBtn, closeCraftingBtn;

        private Vector3 moveDirection;

        private CharacterController m_CharacterContoller;

        private CameraController m_CameraController;

        private float mobileX, mobileY;

        private bool isControllable = true;

        private bool isCrafting = false;

        private bool isMobile = false;

        private void Start()
        {
            isMobile = Util.IsMobile();

            m_CharacterContoller = GetComponent<CharacterController>();
            m_CameraController = GetComponent<CameraController>();

            craftingUI.SetActive(isCrafting);

            MouseLockModule.Instance.OnLock();

            OnControllerInput += data =>
            {
                if (!isControllable)
                {
                    return;
                }

                var delta = m_CameraController.m_Camera.transform.forward * data.y + m_CameraController.m_Camera.transform.right * data.x;

                if (!isCreatorMode)
                {
                    delta = Vector3.ProjectOnPlane(delta, Vector3.up);
                }

                m_CharacterContoller.Move(delta * Time.deltaTime * velocity);
            };

            openCraftingBtn.onClick.AddListener(() =>
            {
                ToggleCrafting();
            });

            closeCraftingBtn.onClick.AddListener(() =>
            {
                ToggleCrafting();
            });

            if (isMobile)
            {
                ControlEvents.OnClickScreen += pos =>
                {
                    CreateBlock(false, pos);
                };
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleCrafting();
            }

            if (!isControllable)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            if (isMobile)
            {
                OnControllerInput?.Invoke(new InputData()
                {
                    x = mobileX,
                    y = mobileY
                });
            }
            else
            {
                OnControllerInput?.Invoke(new InputData()
                {
                    x = Input.GetAxis("Horizontal"),
                    y = Input.GetAxis("Vertical")
                });

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    RemoveBlock();
                }

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    CreateBlock(true, Vector2.zero);
                }
            }


            if (!isCreatorMode)
            {
                m_CharacterContoller.Move(Vector3.up * gravity * Time.deltaTime);
            }

            for (var i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    ControlEvents.OnClickInventoryByID?.Invoke(i);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                ControlEvents.OnClickInventoryByID?.Invoke(9);
            }
        }

        private void Jump()
        {
            if (m_CharacterContoller.isGrounded)
            {
                m_CharacterContoller.Move(Vector3.up * jumpVelocity * Time.deltaTime);

            }
        }

        private void CreateBlock(bool isScreenCenter, Vector2 screenPos)
        {
            var rayScreen = isScreenCenter ? new Vector2(Screen.width, Screen.height) * 0.5f : screenPos;

            var cameraTrans = m_CameraController.m_Camera.transform;

            var ray = m_CameraController.m_Camera.ScreenPointToRay(rayScreen);

            var isHit = Physics.Raycast(ray, out RaycastHit rayHit, 10, 1 << LayerMask.NameToLayer("Block"));

            if (isHit)
            {
                var chunckPoint = rayHit.point + rayHit.normal * 0.5f;

                chunckPoint = new Vector3(Mathf.FloorToInt(chunckPoint.x), Mathf.FloorToInt(chunckPoint.y), Mathf.FloorToInt(chunckPoint.z));

                WorldManager.Instance.CreateBlock((int)chunckPoint.y, (int)chunckPoint.x, (int)chunckPoint.z, blockID);
            }
        }

        private void RemoveBlock()
        {
            var cameraTrans = m_CameraController.m_Camera.transform;

            var isHit = Physics.Raycast(cameraTrans.position, cameraTrans.forward, out RaycastHit rayHit, 5, 1 << LayerMask.NameToLayer("Block"));

            if (isHit)
            {
                var chunckPoint = rayHit.point - rayHit.normal * 0.5f;

                chunckPoint = new Vector3(Mathf.FloorToInt(chunckPoint.x), Mathf.FloorToInt(chunckPoint.y), Mathf.FloorToInt(chunckPoint.z));

                WorldManager.Instance.InteractBlock((int)chunckPoint.y, (int)chunckPoint.x, (int)chunckPoint.z);
            }
        }

        private void ToggleControl(bool state)
        {
            isControllable = state;
            m_CameraController.enabled = state;
        }

        private void ToggleCrafting()
        {
            isCrafting = !isCrafting;

            craftingUI.SetActive(isCrafting);

            if (isCrafting)
            {
                MouseLockModule.Instance.OnUnlock();

                ToggleControl(false);
            }
            else
            {
                MouseLockModule.Instance.OnLock();

                ToggleControl(true);
            }
        }

        public void StartForward()
        {
            mobileY = 1;
        }
        public void CancelForward()
        {
            mobileY = 0;
        }
        public void StartBack()
        {
            mobileY = -1;
        }
        public void CancelBack()
        {
            mobileY = 0;
        }
        public void StartRight()
        {
            mobileX = 1;
        }
        public void CancelRight()
        {
            mobileX = 0;
        }
        public void StartLeft()
        {
            mobileX = -1;
        }
        public void CancelLeft()
        {
            mobileX = 0;
        }
    }

}
