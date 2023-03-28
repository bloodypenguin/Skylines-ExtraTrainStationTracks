using System;
using System.Reflection;
using UnityEngine;

namespace ElevatedTrainStationTrack
{
    public class Initializer : AbstractInitializer
    {
        protected override void InitializeImpl()
        {
            CreatePrefab("Station Track Eleva", "Train Station Track Elevated",
                SetupElevatedPrefab); //for compatibility, never change this prefab's name
            CreatePrefab("Station Track Elevated (C)", "Train Station Track Elevated",
                new Action<NetInfo, bool>(SetupElevatedPrefab).Apply(true));
            CreatePrefab("Station Track Elevated (NP)", "Train Station Track Elevated",
                new Action<NetInfo>(SetupElevatedPrefab).Chain(Modifiers.RemoveElectricityPoles));
            CreatePrefab("Station Track Elevated (CNP)", "Train Station Track Elevated",
                new Action<NetInfo, bool>(SetupElevatedPrefab).Apply(true).Chain(Modifiers.RemoveElectricityPoles));

            CreatePrefab("Station Track Elevated Narrow", "Train Station Track Elevated",
                new Action<NetInfo>(SetupElevatedPrefab).Chain(Modifiers.MakePedestrianLanesNarrow));
            CreatePrefab("Station Track Elevated Narrow (C)", "Train Station Track Elevated",
                new Action<NetInfo, bool>(SetupElevatedPrefab).Apply(true).Chain(Modifiers.MakePedestrianLanesNarrow));
            CreatePrefab("Station Track Elevated Narrow (NP)", "Train Station Track Elevated",
                new Action<NetInfo>(SetupElevatedPrefab).Chain(Modifiers.RemoveElectricityPoles).Chain(Modifiers.MakePedestrianLanesNarrow));
            CreatePrefab("Station Track Elevated Narrow (CNP)", "Train Station Track Elevated",
                new Action<NetInfo, bool>(SetupElevatedPrefab).Apply(true).Chain(Modifiers.RemoveElectricityPoles).Chain(Modifiers.MakePedestrianLanesNarrow));

            CreatePrefab("Station Track Sunken", "Train Station Track",
                SetupSunkenPrefab); //for compatibility, never change this prefab's name
            CreatePrefab("Station Track Sunken (NP)", "Train Station Track",
                new Action<NetInfo>(SetupSunkenPrefab).Chain(Modifiers.RemoveElectricityPoles));
            CreatePrefab("Train Station Track (C)", "Train Station Track",
                Modifiers.CreatePavement);
            CreatePrefab("Train Station Track (NP)", "Train Station Track",
                Modifiers.RemoveElectricityPoles);
            CreatePrefab("Train Station Track (CNP)", "Train Station Track",
                new Action<NetInfo>(Modifiers.CreatePavement).Chain(Modifiers.RemoveElectricityPoles));

            CreatePrefab("Station Track Tunnel", "Metro Station Track",
                SetupTunnelPrefab); //for compatibility, never change this prefab's name
        }

        private static void SetupElevatedPrefab(NetInfo clonedPrefab)
        {
            SetupElevatedPrefab(clonedPrefab, false);
        }

        private static void SetupElevatedPrefab(NetInfo clonedPrefab, bool concrete)
        {
            var elevatedTrack = FindOriginalPrefab("Train Station Track Elevated");
            if (elevatedTrack == null || !concrete)
            {
                return;
            }
            clonedPrefab.m_segments[0].m_segmentMaterial = ModifyRailMaterial(elevatedTrack.m_segments[0].m_segmentMaterial);
            clonedPrefab.m_segments[0].m_material = ModifyRailMaterial(elevatedTrack.m_segments[0].m_material);
            clonedPrefab.m_segments[0].m_lodMaterial = ModifyRailMaterial(elevatedTrack.m_segments[0].m_lodMaterial);
            clonedPrefab.m_nodes[0].m_material = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_material);
            clonedPrefab.m_nodes[0].m_nodeMaterial = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_nodeMaterial);
            clonedPrefab.m_nodes[0].m_lodMaterial = ModifyRailMaterial(elevatedTrack.m_nodes[0].m_lodMaterial);
        }

        private static Material ModifyRailMaterial(Material material)
        {
            var newMaterial = new Material(material)
            {
                name = $"{material.name}-concrete",
                shader = Shader.Find("Custom/Net/RoadBridge")
            };
            return newMaterial;
        }
        
        public class UndergroundTrainStationTrackAI : MetroTrackTunnelAI
        {
            public override bool IsUnderground()
            {
                return false;
            }

            public override bool BuildUnderground()
            {
                return true;
            }
        }

        private static void SetupTunnelPrefab(NetInfo prefab)
        {
            var trainStationTrack = FindOriginalPrefab("Train Station Track");
            prefab.m_class = ScriptableObject.CreateInstance<ItemClass>();
            prefab.m_class.m_subService = ItemClass.SubService.PublicTransportTrain;
            prefab.m_class.m_service = ItemClass.Service.PublicTransport;
            prefab.m_class.m_layer = ItemClass.Layer.MetroTunnels | ItemClass.Layer.Default;
            prefab.m_canCollide = false;

            var metroAI = prefab.GetComponent<MetroTrackAI>();
            GameObject.DestroyImmediate(metroAI);
            var trackAI = prefab.gameObject.AddComponent<UndergroundTrainStationTrackAI>();
            prefab.m_netAI = trackAI;
            trackAI.m_createPassMilestone = trainStationTrack.GetComponent<TrainTrackAI>().m_createPassMilestone;
            trackAI.m_info = prefab;

            var field = typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(prefab, field.GetValue(trainStationTrack));

            prefab.m_averageVehicleLaneSpeed = trainStationTrack.m_averageVehicleLaneSpeed;
            prefab.m_vehicleTypes = VehicleInfo.VehicleType.Train;
            prefab.m_buildHeight = 0;

            foreach (var lane in prefab.m_lanes)
            {
                if (lane.m_vehicleType == VehicleInfo.VehicleType.None)
                {
                    lane.m_stopType = VehicleInfo.VehicleType.Train;
                }
                else
                {
                    lane.m_vehicleType = VehicleInfo.VehicleType.Train;
                }
            }

        }

        private static void SetupSunkenPrefab(NetInfo sunkenPrefab)
        {


            var stationAI = sunkenPrefab.GetComponent<TrainTrackAI>();
            stationAI.m_tunnelInfo = sunkenPrefab;

            sunkenPrefab.m_clipTerrain = false;

            sunkenPrefab.m_createGravel = false;
            sunkenPrefab.m_createPavement = false;
            sunkenPrefab.m_createRuining = false;

            sunkenPrefab.m_flattenTerrain = false;
            sunkenPrefab.m_followTerrain = false;

            sunkenPrefab.m_intersectClass = null;

            sunkenPrefab.m_maxHeight = -1;
            sunkenPrefab.m_minHeight = -3;

            sunkenPrefab.m_requireSurfaceMaps = false;
            sunkenPrefab.m_snapBuildingNodes = false;

            sunkenPrefab.m_placementStyle = ItemClass.Placement.Procedural;
            sunkenPrefab.m_useFixedHeight = true;
            sunkenPrefab.m_lowerTerrain = false;
            sunkenPrefab.m_availableIn = ItemClass.Availability.GameAndAsset;
        }
    }
}
