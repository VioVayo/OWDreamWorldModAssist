using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DWModAssist
{
    public class ModUI
    {
        private static GameObject refButton, refSelector, refToggle;
        private static SubmitActionCloseMenu closePauseMenuAction;

        private static PopupMenu menu;
        private static OptionsSelectorElement zoneSelector;
        private static ToggleElement safeFireToggle, deathToggle;
        private static List<MenuOption> menuOptions = new();

        private static DestinationZone selectedZone = DestinationZone.Zone1;
        private static Dictionary<DestinationZone, ZoneSubMenu> subMenus = new()
        {
            { DestinationZone.Zone1, null },
            { DestinationZone.Zone2, null },
            { DestinationZone.Zone3, null },
            { DestinationZone.Zone4, null }
        };

        public class ZoneSubMenu
        {
            public GameObject GameObject;
            public Type LocationEnumType;
            public OptionsSelectorElement Selector;
            public ZoneSubMenu(GameObject gameObject, Type locationEnumType, OptionsSelectorElement selector)
            {
                GameObject = gameObject;
                LocationEnumType = locationEnumType;
                Selector = selector;
            }

            public void SetSubMenuEnabled(bool enable) { GameObject.SetActive(enable); }
            public int GetSelectedLocationIndex() => (int)Enum.Parse(LocationEnumType, Selector.GetSelectedOption());
        }


        public static void OpenMenu() 
        {
            menu.EnableMenu(true);
            UpdateNavigation();
            zoneSelector._selectable.Select();
        }

        public static void CloseMenu() 
        {
            menu.EnableMenu(false);
            closePauseMenuAction.Submit();
        }

        public static void CreateMenu()
        {
            closePauseMenuAction = Resources.FindObjectsOfTypeAll<SubmitActionCloseMenu>().First(obj => obj.gameObject.name == "Button-Unpause");

            if (menu != null) return;
            menuOptions.Clear();

            var refMenu = GameObject.Find("PopupCanvas").transform.Find("TwoButton-Popup").gameObject;
            var menuObj = GameObject.Instantiate(refMenu, refMenu.transform.parent);

            refButton = menuObj.transform.Find("PopupBlock/PopupElements/Buttons/UIElement-ButtonConfirm").gameObject;
            refSelector = Resources.FindObjectsOfTypeAll<OptionsSelectorElement>().First(obj => obj.gameObject.name == "UIElement-ButtonImages").gameObject;
            refToggle = Resources.FindObjectsOfTypeAll<ToggleElement>().First(obj => obj.gameObject.name == "UIElement-ToggleVibration").gameObject;

            menuObj.transform.Find("PopupBlock/BackingImage").gameObject.GetComponent<RectTransform>().sizeDelta = new(0, 100);
            menuObj.transform.Find("PopupBlock/BorderImage").gameObject.GetComponent<RectTransform>().sizeDelta = new(0, 100);
            menuObj.transform.Find("PopupBlock").gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 120);
            var menuLayout = GameObject.Instantiate(DWModAssist.ModInstance.ModHelper.Assets.LoadBundle("Menu/menu_layout").LoadAsset<GameObject>("Assets/MenuLayout.prefab"), menuObj.transform.Find("PopupBlock"));

            Transform headOptions = null, warpButtonSocket = null, cancelButtonSocket = null, zone1SubMenu = null, zone2SubMenu = null, zone3SubMenu = null, zone4SubMenu = null;
            var transforms = menuLayout.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms)
            {
                if (transform.gameObject.name == "HeaderSocket") headOptions = transform;
                else if (transform.gameObject.name == "WarpButtonSocket") warpButtonSocket = transform;
                else if (transform.gameObject.name == "CancelButtonSocket") cancelButtonSocket = transform;
                else if (transform.gameObject.name == "Zone1") zone1SubMenu = transform;
                else if (transform.gameObject.name == "Zone2") zone2SubMenu = transform;
                else if (transform.gameObject.name == "Zone3") zone3SubMenu = transform;
                else if (transform.gameObject.name == "Zone4") zone4SubMenu = transform;
            }

            zoneSelector = AddSelector<DestinationZone>(headOptions, "Select a Zone");
            zoneSelector.OnValueChanged += OnNewZoneSelected;
            safeFireToggle = AddToggle(headOptions, "Sleep at Safe Fire", 1);
            deathToggle = AddToggle(headOptions, "Enter by Death", 0);

            subMenus[DestinationZone.Zone1] = SetupZoneMenu<LocationZone1>(zone1SubMenu, DWModAssist.zone1AlterStates);
            subMenus[DestinationZone.Zone2] = SetupZoneMenu<LocationZone2>(zone2SubMenu, DWModAssist.zone2AlterStates);
            subMenus[DestinationZone.Zone3] = SetupZoneMenu<LocationZone3>(zone3SubMenu, DWModAssist.zone3AlterStates);
            subMenus[DestinationZone.Zone4] = SetupZoneMenu<LocationZone4>(zone4SubMenu, DWModAssist.zone4AlterStates);

            var warpButton = AddButton(warpButtonSocket, "GO TO DREAM", OnWarp);
            var cancelButton = AddButton(cancelButtonSocket, "CANCEL", OnCancel);

            GameObject.Destroy(menuObj.transform.Find("PopupBlock/PopupElements").gameObject);
            foreach (var localiser in menuObj.GetComponentsInChildren<LocalizedText>()) GameObject.Destroy(localiser);

            subMenus[DestinationZone.Zone2].SetSubMenuEnabled(false);
            subMenus[DestinationZone.Zone3].SetSubMenuEnabled(false);
            subMenus[DestinationZone.Zone4].SetSubMenuEnabled(false);

            menu = menuObj.GetComponent<PopupMenu>();
            menu._okAction = warpButton.GetComponent<SubmitAction>();
            menu._cancelAction = cancelButton.GetComponent<SubmitAction>();
            menu.InitializeMenu();
        }


        private static void OnNewZoneSelected(int irrelevant)
        {
            subMenus[selectedZone].SetSubMenuEnabled(false);
            selectedZone = (DestinationZone)Enum.Parse(typeof(DestinationZone), zoneSelector.GetSelectedOption());
            subMenus[selectedZone].SetSubMenuEnabled(true);
            UpdateNavigation();
        }

        private static void UpdateNavigation()
        {
            Menu.SetVerticalNavigation(menu, menuOptions.Where(obj => obj.gameObject.activeInHierarchy).ToArray());
        }

        private static void OnWarp() 
        { 
            CloseMenu();
            DWModAssist.ModInstance.EngageWarp(selectedZone, safeFireToggle.GetValueAsBool(), deathToggle.GetValueAsBool(), subMenus[selectedZone].GetSelectedLocationIndex());
        }

        private static void OnCancel() 
        { 
            CloseMenu(); 
        }


        private static ZoneSubMenu SetupZoneMenu<T>(Transform transform, (string optionName, UnityAction action)[] alterStates) where T : Enum
        {
            var selector = AddSelector<T>(transform.Find("Header/LocationSelectorSocket"), "Arrival Location");
            transform.gameObject.GetComponentsInChildren<Transform>().First(obj => obj.gameObject.name == "LineBreak_Dots").gameObject.SetActive(false);
            foreach (var alterState in alterStates) AddButton(transform, alterState.Item1, alterState.Item2);
            return new(transform.gameObject, typeof(T), selector);
        }

        private static OptionsSelectorElement AddSelector<T>(Transform transform, string text) where T : Enum
        {
            var options = Enum.GetNames(typeof(T));
            var selector = GameObject.Instantiate(refSelector, transform).GetComponent<OptionsSelectorElement>();
            selector.Initialize(0, options);
            selector._label.text = text;
            selector.gameObject.transform.Find("HorizontalLayoutGroup").GetComponent<HorizontalLayoutGroup>().spacing = -175;
            menuOptions.Add(selector);
            return selector;
        }

        private static ToggleElement AddToggle(Transform transform, string text, int value)
        {
            var toggle = GameObject.Instantiate(refToggle, transform).GetComponent<ToggleElement>();
            toggle.Initialize(value);
            toggle.SetDisplayText(text);
            toggle.gameObject.transform.Find("HorizontalLayoutGroup").GetComponent<HorizontalLayoutGroup>().spacing = -175;
            menuOptions.Add(toggle);
            return toggle;
        }

        private static GameObject AddButton(Transform transform, string text, UnityAction action)
        {
            var button = GameObject.Instantiate(refButton, transform);
            button.GetComponent<ButtonWithHotkeyImageElement>().SetPrompt(new ScreenPrompt(text));
            button.GetComponent<UIStyleApplier>()._buttonItem = true;
            button.GetComponent<Button>().onClick.AddListener(action);
            menuOptions.Add(button.AddComponent<MenuOption>());
            return button;
        }
    }
}
