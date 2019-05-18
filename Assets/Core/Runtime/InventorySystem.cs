﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MC.Core
{
    public class InventorySystem : MonoBehaviour
    {
        public static int max_bottom_inventory_count = 10;

        //Inventory UI
        [System.Serializable]
        public class UILayout
        {
            public class ItemUI
            {
                public Image itemIcon;

                public GameObject instance;

                public Text itemCount;

                public GameObject selectedIcon;
            }

            public GameObject ItemTemplate;

            public List<ItemUI> itemInstance = new List<ItemUI>();

            public void Init()
            {
                for (var i = 0; i < max_bottom_inventory_count; i++)
                {
                    var currentIndex = i;

                    var instance = Instantiate(ItemTemplate, ItemTemplate.transform.parent, true);

                    var uiItem = new ItemUI()
                    {
                        instance = instance,
                        itemIcon = instance.transform.Find("ItemIcon").GetComponent<Image>(),
                        itemCount = instance.transform.Find("Num").GetComponent<Text>(),
                        selectedIcon = instance.transform.Find("Selected").gameObject
                    };
                    itemInstance.Add(uiItem);

                    var iconUI = instance.AddComponent<InventoryIconUI>();
                    iconUI.Init(i, uiItem.itemIcon, InventoryIconType.Inv);

                    uiItem.itemIcon.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ControlEvents.OnClickInventoryByID?.Invoke(currentIndex);
                    });
                }

                ItemTemplate.gameObject.SetActive(false);
            }
        }

        public UILayout layout = new UILayout();

        //玩家拥有物品的储存
        //Slot ID 插槽  <= max_inventory_count 显示在底部
        public List<InventoryStorage> inventoryStorageList = new List<InventoryStorage>();

        //当前选用的Inventory
        public int currentSelectID = 0;

        private void Start()
        {
            layout.Init();

            ControlEvents.OnClickInventoryByID += id =>
            {
                SelectInventoryByID(id);
            };

            InventoryIconUI.OnSwapItem += (a, b, type) =>
            {
                //Inventory 中交换物品
                if (type == SwapType.InvToInv)
                {
                    var itemA = inventoryStorageList.Find(val => val.slotID == a);
                    var itemB = inventoryStorageList.Find(val => val.slotID == b);

                    if (itemA != null)
                    {
                        itemA.slotID = b;
                    }

                    if (itemB != null)
                    {
                        itemB.slotID = a;
                    }

                    UpdateInvetoryUI();
                }

                //Inventory Craft 交换物品
                if (type == SwapType.InvToCraft)
                {
                    var itemA = inventoryStorageList.Find(val => val.slotID == a);
                    var itemB = CraftSystem.Instance.craftInventoryList.Find(val => val.slotID == b);

                    if (itemA != null)
                    {
                        itemA.slotID = b;

                        inventoryStorageList.Remove(itemA);
                        CraftSystem.Instance.craftInventoryList.Add(itemA);
                    }

                    if (itemB != null)
                    {
                        itemB.slotID = a;

                        inventoryStorageList.Add(itemB);
                        CraftSystem.Instance.craftInventoryList.Remove(itemB);

                    }

                    UpdateInvetoryUI();
                    CraftSystem.Instance.UpdateInvetoryUI();
                }

                //Craft Inventory 交换物品
                if (type == SwapType.CraftToInv)
                {
                    var itemA = CraftSystem.Instance.craftInventoryList.Find(val => val.slotID == a);
                    var itemB = inventoryStorageList.Find(val => val.slotID == b);

                    if (itemA != null)
                    {
                        itemA.slotID = b;

                        inventoryStorageList.Add(itemA);
                        CraftSystem.Instance.craftInventoryList.Remove(itemA);
                    }

                    if (itemB != null)
                    {
                        itemB.slotID = a;

                        inventoryStorageList.Remove(itemB);
                        CraftSystem.Instance.craftInventoryList.Add(itemB);
                    }

                    UpdateInvetoryUI();
                    CraftSystem.Instance.UpdateInvetoryUI();
                }

                //Craft 中交换物品
                if (type == SwapType.CraftToCraft)
                {
                    var itemA = CraftSystem.Instance.craftInventoryList.Find(val => val.slotID == a);
                    var itemB = CraftSystem.Instance.craftInventoryList.Find(val => val.slotID == b);

                    if (itemA != null)
                    {
                        itemA.slotID = b;
                    }

                    if (itemB != null)
                    {
                        itemB.slotID = a;
                    }

                    CraftSystem.Instance.UpdateInvetoryUI();
                }
                //Craft 生成物体
                if (type == SwapType.CraftedToInv)
                {
                    var isTargetEmpty = inventoryStorageList.Find(val => val.slotID == b) == null;

                    //目标插槽空
                    if (isTargetEmpty)
                    {
                        foreach (var usedItem in CraftSystem.Instance.craftInventoryList)
                        {
                            usedItem.count -= 1;
                        }

                        //深复制防止多引用
                        var craftedInventory = CraftSystem.Instance.craftedInventory.Clone();
                        craftedInventory.slotID = b;

                        inventoryStorageList.Add(craftedInventory);

                        CleanUpInventory();

                        UpdateInvetoryUI();
                        CraftSystem.Instance.UpdateInvetoryUI();
                    }
                }
            };

            UpdateInvetoryUI();
        }

        private void CleanUpInventory()
        {
            //清除无效的物品
            for (int i = inventoryStorageList.Count - 1; i >= 0; i--)
            {
                var item = inventoryStorageList[i];

                if (item.count <= 0)
                {
                    inventoryStorageList.Remove(item);
                }
            }

            for (int i = CraftSystem.Instance.craftInventoryList.Count - 1; i >= 0; i--)
            {
                var item = CraftSystem.Instance.craftInventoryList[i];

                if (item.count <= 0)
                {
                    CraftSystem.Instance.craftInventoryList.Remove(item);
                }
            }
        }

        private void UpdateInvetoryUI()
        {
            //更新底部插槽UI
            for (var i = 0; i < max_bottom_inventory_count; i++)
            {
                var item = inventoryStorageList.Find(val => val?.slotID == i);

                if (item != null)
                {
                    layout.itemInstance[i].itemIcon.sprite = item.inventory.inventoryIcon;
                    layout.itemInstance[i].itemCount.text = item.count.ToString();
                }
                else
                {
                    layout.itemInstance[i].itemIcon.sprite = null;
                    layout.itemInstance[i].itemCount.text = "0";
                }

                if (currentSelectID == i)
                {
                    layout.itemInstance[i].selectedIcon.SetActive(true);
                }
                else
                {
                    layout.itemInstance[i].selectedIcon.SetActive(false);
                }
            }
        }

        public void SelectInventoryByID(int id)
        {
            currentSelectID = id;
            UpdateInvetoryUI();
        }
    }

}
