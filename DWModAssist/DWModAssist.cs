using HarmonyLib;
using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace DWModAssist
{
    public class DWModAssist : ModBehaviour
    {
        public static DWModAssist ModInstance;

        private static DreamArrivalPoint.Location[] zones =
        {
            DreamArrivalPoint.Location.Zone1,
            DreamArrivalPoint.Location.Zone2,
            DreamArrivalPoint.Location.Zone3,
            DreamArrivalPoint.Location.Zone4
        };
        private static RelativeLocationData[] 
            locationsZone1 =
            {
                new RelativeLocationData(new Vector3(0, 10.65f, 0), new Quaternion(0, 0, 0, 1), Vector3.zero), //DreamFireHouse
                new RelativeLocationData(new Vector3(-61, 10.1f, 38.5f), new Quaternion(0, -0.4f, 0, 0.9f), Vector3.zero), //RaftProjector
                new RelativeLocationData(new Vector3(5, 13.62f, 110), new Quaternion(0, 0.4f, 0, -0.9f), Vector3.zero), //Bridge
                new RelativeLocationData(new Vector3(-115, 11.6f, 137), new Quaternion(0, 0.7f, 0, 0.7f), Vector3.zero), //Village
                new RelativeLocationData(new Vector3(55, 11.35f, 170), new Quaternion(0, 0.9f, 0, 0.5f), Vector3.zero), //PartyOutside
                new RelativeLocationData(new Vector3(117, 19.75f, 167), new Quaternion(0, 0.5f, 0, 0.9f), Vector3.zero) //ArchiveElevator
            }, 
            locationsZone2 =
            {
                new RelativeLocationData(new Vector3(0, 10.6f, 0), new Quaternion(0, 0, 0, 1), Vector3.zero), //DreamFireHouse
                new RelativeLocationData(new Vector3(-38, -2.4f, 2), new Quaternion(0, 0.6f, 0, 0.8f), Vector3.zero), //RaftProjector
                new RelativeLocationData(new Vector3(48.5f, 21.7f, 153), new Quaternion(0, 0.6f, 0, 0.8f), Vector3.zero), //LightsProjector
                new RelativeLocationData(new Vector3(5.5f, 30.4f, 0.5f), new Quaternion(0, 0.7f, 0, -0.7f), Vector3.zero), //SecretTowerRoom
                new RelativeLocationData(new Vector3(59, -2.4f, 182), new Quaternion(0, 0.5f, 0, 0.9f), Vector3.zero), //BurntHouseRaft
                new RelativeLocationData(new Vector3(-7.3f, -20.4f, 105), new Quaternion(0, 0.1f, 0, -1), Vector3.zero), //Underground
                new RelativeLocationData(new Vector3(-35, -56.4f, 216), new Quaternion(0, 0.1f, 0, -1), Vector3.zero) //ArchiveElevator
            }, 
            locationsZone3 =
            {
                new RelativeLocationData(new Vector3(0, 10.6f, 0), new Quaternion(0, 0, 0, 1), Vector3.zero), //DreamFireHouse
                new RelativeLocationData(new Vector3(25, -80.4f, 72.5f), new Quaternion(0, 0.9f, 0, -0.4f), Vector3.zero), //RaftProjector
                new RelativeLocationData(new Vector3(-55, 16, 71.5f), new Quaternion(0, 0.7f, 0, 0.7f), Vector3.zero), //Stage
                new RelativeLocationData(new Vector3(-86.5f, -6.4f, 85), new Quaternion(0, 0.9f, 0, 0.4f), Vector3.zero), //TheatreBalcony
                new RelativeLocationData(new Vector3(-27, -15.96f, 90.5f), new Quaternion(0, 0.9f, 0, 0.4f), Vector3.zero), //Ballroom
                new RelativeLocationData(new Vector3(2, -18.4f, 11), new Quaternion(0, 1, 0, 0), Vector3.zero) //ArchiveElevator
            }, 
            locationsZone4 =
            {
                new RelativeLocationData(new Vector3(0, 0.6f, -2), new Quaternion(0, 0, 0, 1), Vector3.zero), //DreamFire
                new RelativeLocationData(new Vector3(10.5f, -17.25f, 62.5f), new Quaternion(0, 0.9f, 0, 0.4f), Vector3.zero), //RaftProjector
                new RelativeLocationData(new Vector3(23, -308.7f, 0), new Quaternion(0, -0.7f, 0, 0.7f), Vector3.zero), //VaultOutside
                new RelativeLocationData(new Vector3(76, -308.6f, 64), new Quaternion(0, 0.9f, 0, -0.4f), Vector3.zero), //LockProjector1
                new RelativeLocationData(new Vector3(99.5f, -312.77f, 0), new Quaternion(0, -0.7f, 0, 0.7f), Vector3.zero), //LockProjector2
                new RelativeLocationData(new Vector3(77.5f, -308.27f, -65), new Quaternion(0, -0.4f, 0, 0.9f), Vector3.zero), //LockProjector3
                new RelativeLocationData(new Vector3(-77, -377.1f, 0), new Quaternion(0, 0.7f, 0, 0.7f), Vector3.zero) //PrisonerCell
            };
        private static RelativeLocationData[][] locationsCollection = { locationsZone1, locationsZone2, locationsZone3, locationsZone4 };

        public static (string optionName, UnityAction action)[]
            zone1AlterStates =
            {
                ("Open Raft Dock", OpenZone1Dock),
                ("Create All Bridges", MakeZone1Bridges),
                ("Extinguish Fire >:c", ExtinguishZone1Fire)
            },
            zone2AlterStates =
            {
                ("Open Raft Dock", OpenZone2Dock),
                ("Extinguish Lights", ExtinguishZone2Lights)
            },
            zone3AlterStates =
            {
                ("Open Raft Dock", OpenZone3Dock),
                ("Create All Bridges", MakeZone3Bridges),
                ("Extinguish Lights", ExtinguishZone3Lights)
            },
            zone4AlterStates =
            {
                ("Open Sealed Vault", OpenVault)
            };

        private static GameObject itemDropSocket;
        private static DreamLanternItem lantern;

        private static DreamCampfire zone1Fire;
        private static CageElevator zone3Elevator;
        private static PrisonCellElevator cellevator;
        private static SarcophagusController vaultController;
        private static OWTriggerVolume
            zone2Undercity,
            zone2UndercityAirMemorial,
            zone2UndercityAirElevator,
            zone3Interior,
            zone3Depths,
            zone3Courtyard,
            zone4FireChamber,
            zone4PrisonCell,
            zone4PrisonCellAir;
        private static DreamObjectProjector
            zone1DoorProjector,
            zone2DockProjector,
            zone1BridgeProjector,
            zone3BridgeProjectorOutside,
            zone3BridgeProjectorInside,
            zone2LightsProjector,
            zone3LightsProjector,
            lock1Projector,
            lock2Projector,
            lock3Projector;

        private static ShipCockpitController cockpitController;
        public static Campfire LastUsedCampfire;
        public static NomaiRemoteCameraPlatform LastUsedProjectionPool;
        public static SlideProjector LastUsedSlideProjector;
        public static Peephole LastUsedPeephole;
        public static PlayerAttachPoint LastAttachedPoint;


        private void Awake()
        {
            ModInstance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => 
            {
                if (loadScene != OWScene.SolarSystem) return;
                FindReferences();
                ModUI.CreateMenu();
            };

            ModHelper.Menus.PauseMenu.OnInit += () =>
            {
                if (LoadManager.GetCurrentScene() != OWScene.SolarSystem) return;
                var warpButton = ModHelper.Menus.PauseMenu.OptionsButton.Duplicate("DW MODDING / DEBUG ASSIST");
                warpButton.OnClick += ModUI.OpenMenu;
            };
        }

        private void FindReferences()
        {
            itemDropSocket = new("ItemDropSocket");
            itemDropSocket.transform.SetParent(GameObject.Find("Sector_DreamWorld").transform);
            lantern = GameObject.Find("Prefab_IP_DreamLanternItem_2").GetComponent<DreamLanternItem>();

            zone1Fire = Locator.GetDreamCampfire(zones[(int)DestinationZone.Zone1 - 1]);
            zone3Elevator = GameObject.Find("Elevator_Raft/Prefab_IP_DW_CageElevator").GetComponent<CageElevator>();
            cellevator = FindObjectOfType<PrisonCellElevator>();
            vaultController = FindObjectOfType<SarcophagusController>();

            var volumes = FindObjectsOfType<OWTriggerVolume>();
            foreach (var volume in volumes)
            {
                var name = volume.gameObject.name;
                if (name == "SectorTrigger_Undercity") zone2Undercity = volume;
                else if (name == "AirVolumePit" && volume.gameObject.transform.parent.gameObject.name == "Undercity") zone2UndercityAirMemorial = volume;
                else if (name == "AirVolume" && volume.gameObject.transform.parent.gameObject.name == "Undercity") zone2UndercityAirElevator = volume;
                else if (name == "HotelInteriorVolume") zone3Interior = volume;
                else if (name == "HotelDepthsVolume") zone3Depths = volume;
                else if (name == "HotelCourtyardVolume") zone3Courtyard = volume;
                else if (name == "DreamPrisonVolume") zone4FireChamber = volume;
                else if (name == "Sector_PrisonCell") zone4PrisonCell = volume;
                else if (name == "WaterOverrideVolume") zone4PrisonCellAir = volume;
            }

            var projectors = FindObjectsOfType<DreamObjectProjector>();
            foreach (var projector in projectors)
            {
                var name = projector.gameObject.name;
                if (name == "Prefab_IP_DreamObjectProjector (2)" && projector.gameObject.transform.parent.gameObject.name == "Tunnel") zone1DoorProjector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector" && projector.gameObject.transform.parent.gameObject.name == "RaftDockProjector") zone2DockProjector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector (1)") zone1BridgeProjector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector_Bridge") zone3BridgeProjectorOutside = projector;
                else if (name == "Prefab_IP_DreamObjectProjector" && projector.gameObject.transform.parent.gameObject.name == "Lobby") zone3BridgeProjectorInside = projector;
                else if (name == "Prefab_IP_DreamObjectProjector" && projector.gameObject.transform.parent.gameObject.name == "Interactibles_DreamZone_2") zone2LightsProjector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector_Hotel") zone3LightsProjector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector (4)") lock1Projector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector (3)") lock2Projector = projector;
                else if (name == "Prefab_IP_DreamObjectProjector (2)" && projector.gameObject.transform.parent.gameObject.name == "Interactibles_Island_C") lock3Projector = projector;
            }

            cockpitController = FindObjectOfType<ShipCockpitController>();
        }


        //-----WARP-----
        public void EngageWarp(DestinationZone destinationZone, bool sleepAtSafeFire, bool enterByDeath, int locationIndex) 
        {
            var zoneIndex = (int)destinationZone - 1;
            var campfireIndex = sleepAtSafeFire ? (int)DestinationZone.Zone3 - 1 : zoneIndex;
            StartCoroutine(WarpToPlace(zoneIndex, campfireIndex, locationIndex, enterByDeath)); 
        }

        private void GiveLantern(Vector3 worldDestinationPosition)
        {
            itemDropSocket.transform.position = worldDestinationPosition;

            var itemTool = Locator.GetToolModeSwapper().GetItemCarryTool();
            if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.None)
            {
                if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.Item) Locator.GetToolModeSwapper().UnequipTool();
                else if (itemTool.GetHeldItemType() == ItemType.DreamLantern) return;
                else itemTool.DropItemInstantly(Locator.GetDreamWorldController()._dreamWorldSector, itemDropSocket.transform);
            }
            itemTool.PickUpItemInstantly(lantern);
        }

        private IEnumerator ResetPlayerState()
        {
            if (LastUsedProjectionPool != null && LastUsedProjectionPool.IsPlatformActive())
            {
                LastUsedProjectionPool.OnLeaveBounds();
                while (PlayerState.UsingNomaiRemoteCamera()) yield return null;
            }
            if (LastUsedCampfire != null)
            {
                LastUsedCampfire.StopRoasting();
                LastUsedCampfire.StopSleeping();
            }
            if (PlayerState.AtFlightConsole()) cockpitController.ExitFlightConsole();
            if (PlayerState.IsViewingProjector()) LastUsedSlideProjector.CancelInteraction();
            if (PlayerState.IsPeeping()) LastUsedPeephole.Unpeep();
            if (PlayerState.IsAttached()) LastAttachedPoint.DetachPlayer();
            Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().BreakLock();
            OWInput.ChangeInputMode(InputMode.Character);

            PlayerState._isResurrected = false;
            Locator.GetDreamWorldController().ExitDreamWorld();
            while (Locator.GetDreamWorldController().IsInDream()) yield return null;
        }

        private IEnumerator WarpToPlace(int zoneIndex, int campfireIndex, int locationIndex, bool enterByDeath)
        {
            var campfire = Locator.GetDreamCampfire(zones[campfireIndex]);
            var arrivalPoint = Locator.GetDreamArrivalPoint(zones[zoneIndex]);
            var relativeLocationData = locationsCollection[zoneIndex][locationIndex];

            GiveLantern(arrivalPoint.transform.TransformPoint(relativeLocationData.localPosition - new Vector3(0, 0.5f, 0)));
            yield return StartCoroutine(ResetPlayerState());

            Locator.GetDreamWorldController().EnterDreamWorld(campfire, arrivalPoint, relativeLocationData);
            yield return new WaitForFixedUpdate();
            Locator.GetDreamWorldController()._relativeSleepLocation.localPosition = locationsCollection[campfireIndex][0].localPosition;

            PlayerState._isResurrected = enterByDeath;

            foreach (var volume in arrivalPoint._entrywayVolumes) volume.RemoveAllObjectsFromVolume();
            List<OWTriggerVolume> volumes = new();
            switch (zoneIndex + 1)
            {
                case (int)DestinationZone.Zone1:
                    break;
                case (int)DestinationZone.Zone2:
                    if (locationIndex is (int)LocationZone2.Underground or (int)LocationZone2.ArchiveElevator)
                    {
                        volumes.Add(zone2Undercity);
                        volumes.Add(locationIndex == (int)LocationZone2.Underground ? zone2UndercityAirMemorial : zone2UndercityAirElevator);
                    }
                    break;
                case (int)DestinationZone.Zone3:
                    if (locationIndex is (int)LocationZone3.TheatreBalcony)
                    {
                        volumes.Add(zone3Depths);
                        volumes.Add(zone3Courtyard);
                    }
                    if (locationIndex is (int)LocationZone3.TheatreBalcony or (int)LocationZone3.Ballroom)
                    {
                        volumes.Add(zone3Interior);
                    }
                    break;
                case (int)DestinationZone.Zone4:
                    if (locationIndex is (int)LocationZone4.DreamFire)
                    {
                        volumes.Add(zone4FireChamber);
                    }
                    if (locationIndex is (int)LocationZone4.PrisonerCell)
                    {
                        volumes.Add(zone4PrisonCell);
                        volumes.Add(zone4PrisonCellAir);
                        cellevator.CallToBottomFloor();
                        cellevator.TryOpenDoor();
                    }
                    break;
                default: break;
            }
            foreach (var volume in volumes)
            {
                volume.AddObjectToVolume(Locator.GetPlayerDetector().gameObject);
                volume.AddObjectToVolume(Locator.GetPlayerCameraDetector().gameObject);
                volume.AddObjectToVolume(lantern.GetFluidDetector().gameObject);
            }
        }


        //-----ALTER ZONE STATE-----
        public static void OpenZone1Dock() { zone1DoorProjector.SetLit(false); }

        public static void OpenZone2Dock() { zone2DockProjector.SetLit(true); }

        public static void OpenZone3Dock() { zone3Elevator.GoToFloor(0); }

        public static void MakeZone1Bridges() { zone1BridgeProjector.SetLit(true); }

        public static void MakeZone3Bridges()
        {
            zone3BridgeProjectorOutside.SetLit(true);
            zone3BridgeProjectorInside.SetLit(true);
        }

        public static void ExtinguishZone1Fire() { zone1Fire.OnEnterCustomCollider(); }

        public static void ExtinguishZone2Lights() { zone2LightsProjector.SetLit(false); }

        public static void ExtinguishZone3Lights() { zone3LightsProjector.SetLit(false); }

        public static void OpenVault()
        {
            lock1Projector.SetLit(false);
            lock2Projector.SetLit(false);
            lock3Projector.SetLit(false);
            vaultController.OnPressInteract();
        }


        /*private void Update() //For Debug
        {
            if (OWInput.IsNewlyPressed(InputLibrary.interactSecondary)) 
            { 
                foreach (var sector in Locator.GetPlayerSectorDetector()._sectorList) 
                    ModHelper.Console.WriteLine($"{sector.gameObject.name}"); 
            }
        }*/
    }
}