﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace MC.Core
{
    public class PlotManager : MonoBehaviour
    {

        [System.Serializable]
        public class Task
        {
            public enum TaskType
            {
                InventoryCount
            }

            public TaskType taskType = TaskType.InventoryCount;

            public GameObject uiGuidance;

            public Toggle completeToggle;

            [Header("Inventory Count")]
            public string invName;
            public int invCount;

            [HideInInspector]
            public Player player;

            public void CompleteTask()
            {
                completeToggle.isOn = true;
                uiGuidance.transform.SetAsLastSibling();
            }

            public bool CheckComplete()
            {
                var isComplete = false;

                switch (taskType)
                {
                    case TaskType.InventoryCount:
                        var inv = player.inventorySystem.inventoryStorageList.Find(val => val.inventory.inventoryName == invName);

                        if (inv == null)
                        {
                            isComplete = false;
                        }
                        else
                        {
                            if (inv.count < invCount)
                            {
                                isComplete = false;
                            }
                            else
                            {
                                isComplete = true;
                            }
                        }
                        break;


                }

                return isComplete;
            }
        }

        public List<Task> tasks = new List<Task>();

        public GameObject player;

        public GameObject runtime;

        public GameObject timelinePrologue;

        public GameObject gamePlayGuide;

        public PlayableDirector directorPrologue, directorResourceCollected, directorSkeletonShown, directorEnding;

        public GameObject skeletonFightCheckPoint;

        public GameObject skeletonMaster;

        public Inventory gunWeapon;

        public bool skipTimeline = false;

        private Queue<Task> collectTasks = new Queue<Task>();

        private void Start()
        {
            timelinePrologue.SetActive(true);
            runtime.SetActive(false);
            gamePlayGuide.SetActive(false);

            foreach (var task in tasks)
            {
                task.player = player.GetComponent<Player>();

                if (task.taskType == Task.TaskType.InventoryCount)
                {
                    collectTasks.Enqueue(task);
                }
            }

            StartCoroutine(PlotUpdate());
        }

        private IEnumerator PlotUpdate()
        {
            //仅编辑器可选择跳过Timeline
#if UNITY_EDITOR
            if (!skipTimeline)
            {
#endif
                yield return new WaitForSeconds((float)directorPrologue.duration + (float)directorPrologue.initialTime);
#if UNITY_EDITOR
            }
#endif

            timelinePrologue.SetActive(false);

            runtime.SetActive(true);
            gamePlayGuide.SetActive(true);

            //判断资源是否收集完毕
            while (true)
            {
                if (collectTasks.Count <= 0)
                {
                    break;
                }

                var task = collectTasks.Dequeue();

                while (!task.CheckComplete())
                {
                    yield return new WaitForSeconds(1);
                }

                task.CompleteTask();
            }

            skeletonFightCheckPoint.SetActive(true);
            directorResourceCollected.Play();

            while (true)
            {
                var dir = Vector3.ProjectOnPlane(player.transform.position - skeletonFightCheckPoint.transform.position, Vector3.up);

                if (dir.magnitude > 10)
                {
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    break;
                }
            }

            skeletonMaster.SetActive(true);

            directorSkeletonShown.Play();

            while (skeletonMaster != null)
            {
                yield return new WaitForEndOfFrame();
            }

            runtime.SetActive(false);

            directorEnding.Play();

            yield return new WaitForSeconds((float)directorEnding.duration);

            runtime.SetActive(true);

            for (var i = 0; i < 10; i++)
            {
                var id = player.GetComponent<Player>().inventorySystem.inventoryStorageList.FindIndex(val => val.slotID == i);

                if (id == -1)
                {
                    player.GetComponent<Player>().inventorySystem.inventoryStorageList.Add(new InventoryStorage()
                    {
                        inventory = Instantiate(gunWeapon),
                        count = 1,
                        slotID = i
                    });

                    player.GetComponent<Player>().inventorySystem.UpdateInvetoryUI();

                    break;
                }
            }
        }
    }

}
