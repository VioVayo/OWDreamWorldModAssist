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

        private static Menu menu;
        private static OptionsSelectorElement zoneSelector;
        private static ToggleElement safeFireToggle, deathToggle;
        private static List<MenuOption> menuOptions = new();

        private static DreamZone selectedZone = DreamZone.Zone1;
        private static Dictionary<DreamZone, ZoneSubMenu> subMenus;

        private static (string optionName, UnityAction action)[]
            zone1AlterStates =
            {
                ("Open Raft Dock", DWModAssist.OpenZone1Dock),
                ("Create All Bridges", DWModAssist.MakeZone1Bridges),
                ("Extinguish Fire >:c", DWModAssist.ExtinguishZone1Fire)
            },
            zone2AlterStates =
            {
                ("Open Raft Dock", DWModAssist.OpenZone2Dock),
                ("Extinguish Lights", DWModAssist.ExtinguishZone2Lights)
            },
            zone3AlterStates =
            {
                ("Open Raft Dock", DWModAssist.OpenZone3Dock),
                ("Create All Bridges", DWModAssist.MakeZone3Bridges),
                ("Extinguish Lights", DWModAssist.ExtinguishZone3Lights)
            },
            zone4AlterStates =
            {
                ("Open Sealed Vault", DWModAssist.OpenVault)
            };

        private class ZoneSubMenu
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
            public Enum GetSelectedLocation() => (Enum)Enum.Parse(LocationEnumType, Selector.GetSelectedOption());
        }


        public static void SetUpMenu()
        {
            CreateMenu();
            closePauseMenuAction = Resources.FindObjectsOfTypeAll<SubmitActionCloseMenu>().First(obj => obj.gameObject.name == "Button-Unpause");

            //Add button to pause menu
            var refButton = GameObject.Find("PauseMenuBlock").transform.Find("PauseMenuItems/PauseMenuItemsLayout/Button-Options");
            var warpButton = GameObject.Instantiate(refButton, refButton.transform.parent);
            warpButton.transform.SetSiblingIndex(refButton.transform.GetSiblingIndex() + 1);
            warpButton.GetComponent<Button>().onClick.AddListener(ModUI.OpenMenu);
            warpButton.GetComponent<UIStyleApplier>()._textItems[0].text = "DW MODDING / DEBUG ASSIST";
            GameObject.Destroy(warpButton.GetComponentInChildren<LocalizedText>());
            GameObject.Destroy(warpButton.GetComponent<SubmitActionMenu>());
        }

        private static void CreateMenu()
        {
            if (menu != null) return;
            menuOptions.Clear();

            var refMenu = GameObject.Find("PopupCanvas").transform.Find("TwoButton-Popup").gameObject;
            var menuObj = GameObject.Instantiate(refMenu, refMenu.transform.parent);

            refButton = menuObj.transform.Find("PopupBlock/PopupElements/Buttons/UIElement-ButtonConfirm").gameObject;
            var optionsCanvas = GameObject.Find("OptionsCanvas");
            refSelector = optionsCanvas.GetComponentsInChildren<Transform>(true).FirstOrDefault(obj => obj.gameObject.name == "UIElement-ButtonImages").gameObject;
            refToggle = optionsCanvas.GetComponentsInChildren<Transform>(true).FirstOrDefault(obj => obj.gameObject.name == "UIElement-ToggleVibration").gameObject;

            menuObj.transform.Find("PopupBlock/BackingImage").gameObject.GetComponent<RectTransform>().sizeDelta = new(0, 100);
            menuObj.transform.Find("PopupBlock/BorderImage").gameObject.GetComponent<RectTransform>().sizeDelta = new(0, 100);
            menuObj.transform.Find("PopupBlock").gameObject.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 120);

            //The Transform hierarchy is saved as a prefab asset, so instead of coding all the button positions the hard way we're going to use that and attach the menu elements to placeholder GOs as children
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

            //Make sure to add the MenuOptions in vertical navigation order
            zoneSelector = AddSelector<DreamZone>(headOptions, "Select a Zone");
            zoneSelector.OnValueChanged += OnNewZoneSelected;
            safeFireToggle = AddToggle(headOptions, "Sleep at Safe Fire", 1);
            deathToggle = AddToggle(headOptions, "Enter by Death", 0);
            subMenus = new()
            {
                { DreamZone.Zone1, SetUpZoneMenu<LocationZone1>(zone1SubMenu, zone1AlterStates) },
                { DreamZone.Zone2, SetUpZoneMenu<LocationZone2>(zone2SubMenu, zone2AlterStates) },
                { DreamZone.Zone3, SetUpZoneMenu<LocationZone3>(zone3SubMenu, zone3AlterStates) },
                { DreamZone.Zone4, SetUpZoneMenu<LocationZone4>(zone4SubMenu, zone4AlterStates) }
            };
            AddButton(warpButtonSocket, "GO TO DREAM", OnWarp, false);
            AddButton(cancelButtonSocket, "CLOSE MENU", OnCancel, false);

            //LocalizedText changes the buttons to use default text, it's very annoying and has to go, this needs to happen while all the submenu GOs are still active
            GameObject.Destroy(menuObj.transform.Find("PopupBlock/PopupElements").gameObject);
            foreach (var localiser in menuObj.GetComponentsInChildren<LocalizedText>()) GameObject.Destroy(localiser);

            subMenus[DreamZone.Zone2].SetSubMenuEnabled(false);
            subMenus[DreamZone.Zone3].SetSubMenuEnabled(false);
            subMenus[DreamZone.Zone4].SetSubMenuEnabled(false);

            //Our reference menu GO comes with a PopupMenu component, but it does some unwanted stuff so we'll get rid of it and add a new Menu component
            GameObject.Destroy(menuObj.GetComponent<PopupMenu>());
            menu = menuObj.AddComponent<Menu>();
            menu._menuActivationRoot = menuObj; //avoid nullrefs
            menu._menuOptions = new MenuOption[0];
            menu.gameObject.AddComponent<GraphicRaycaster>(); //make options mouse-selectable
        }


        private static void OpenMenu()
        {
            menu.EnableMenu(true);
            UpdateNavigation();
            Locator.GetMenuInputModule().SelectOnNextUpdate(zoneSelector._selectable);
        }

        private static void CloseMenu()
        {
            menu.EnableMenu(false);
            closePauseMenuAction.Submit();
        }

        private static void OnNewZoneSelected(int _)
        {
            subMenus[selectedZone].SetSubMenuEnabled(false);
            selectedZone = (DreamZone)Enum.Parse(typeof(DreamZone), zoneSelector.GetSelectedOption());
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
            DWModAssist.ModInstance.EngageWarp(selectedZone, subMenus[selectedZone].GetSelectedLocation(), safeFireToggle.GetValueAsBool(), deathToggle.GetValueAsBool());
        }

        private static void OnCancel() 
        { 
            CloseMenu(); 
        }


        private static ZoneSubMenu SetUpZoneMenu<T>(Transform transform, (string optionName, UnityAction action)[] alterStates) where T : Enum
        {
            var selector = AddSelector<T>(transform.Find("Header/LocationSelectorSocket"), "Arrival Location");
            transform.gameObject.GetComponentsInChildren<Transform>().First(obj => obj.gameObject.name == "LineBreak_Dots").gameObject.SetActive(false);
            foreach (var alterState in alterStates) AddButton(transform, alterState.Item1, alterState.Item2, true);
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

        private static GameObject AddButton(Transform transform, string text, UnityAction action, bool addSound)
        {
            var button = GameObject.Instantiate(refButton, transform);
            button.GetComponent<ButtonWithHotkeyImageElement>().SetPrompt(new ScreenPrompt(text));
            button.GetComponent<UIStyleApplier>()._buttonItem = true;
            button.GetComponent<Button>().onClick.AddListener(action);
            if (addSound) button.GetComponent<Button>().onClick.AddListener(() => { Locator.GetMenuAudioController().PlayOptionToggle(); }); 
            //can't subscribe PlayOptionToggle() directly at this time bc Locator hasn't done its thing yet
            menuOptions.Add(button.AddComponent<MenuOption>());
            return button;
        }
    }
}
