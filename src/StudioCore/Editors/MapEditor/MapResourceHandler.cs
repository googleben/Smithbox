﻿using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Locators;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    public class MapResourceHandler
    {
        private Task task;

        private HashSet<ResourceDescriptor> LoadList_MapPiece_Model = new();
        private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
        private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
        private HashSet<ResourceDescriptor> LoadList_Collision = new();
        private HashSet<ResourceDescriptor> LoadList_Navmesh = new();

        private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
        private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
        private HashSet<ResourceDescriptor> LoadList_Map_Texture = new();
        private HashSet<ResourceDescriptor> LoadList_Other_Texture = new();

        private ResourceDescriptor MapResource = null;

        public string MapID;
        public string AdjustedMapID;
        public IMsb Msb;

        public MapResourceHandler(string mapId)
        {
            MapID = mapId;
            AdjustedMapID = MapLocator.GetAssetMapID(MapID);
        }

        public bool GetMapMSB()
        {
            MapResource = MapLocator.GetMapMSB(MapID);

            if(MapResource.AssetPath == null)
                return false;

            return true;
        }

        public void ReadMap()
        {
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                Msb = MSB3.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.SDT)
            {
                Msb = MSBS.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.ER)
            {
                Msb = MSBE.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.AC6)
            {
                Msb = MSB_AC6.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                Msb = MSB2.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.BB)
            {
                Msb = MSBB.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else if (Smithbox.ProjectType == ProjectType.DES)
            {
                Msb = MSBD.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
            else
            {
                Msb = MSB1.Read(Smithbox.FS.GetFile(MapResource.AssetPath).GetData());
            }
        }

        public void SetupHumanEnemySubstitute()
        {
            var chrId = CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID;

            var modelAsset = ModelLocator.GetChrModel(chrId);
            var textureAsset = TextureLocator.GetChrTextures(chrId);

            if (modelAsset.IsValid())
                LoadList_Character_Model.Add(modelAsset);

            if (textureAsset.IsValid())
                LoadList_Character_Texture.Add(textureAsset);
        }

        public void SetupModelLoadLists()
        {
            foreach (IMsbModel model in Msb.Models.GetEntries())
            {
                // MapPiece
                if (model.Name.StartsWith("m"))
                {
                    var modelAsset = ModelLocator.GetMapModel(AdjustedMapID, ModelLocator.MapModelNameToAssetName(AdjustedMapID, model.Name));

                    if (modelAsset.IsValid())
                        LoadList_MapPiece_Model.Add(modelAsset);
                }

                // Character
                if (model.Name.StartsWith("c"))
                {
                    var modelAsset = ModelLocator.GetChrModel(model.Name);

                    if (modelAsset.IsValid())
                        LoadList_Character_Model.Add(modelAsset);
                }

                // Object / Asset
                if (model.Name.StartsWith("o") || model.Name.StartsWith("AEG"))
                {
                    var modelAsset = ModelLocator.GetObjModel(model.Name);

                    if (modelAsset.IsValid())
                        LoadList_Asset_Model.Add(modelAsset);
                }

                // Collision
                if (model.Name.StartsWith("h"))
                {
                    var modelAsset = ModelLocator.GetMapCollisionModel(AdjustedMapID, ModelLocator.MapModelNameToAssetName(AdjustedMapID, model.Name), false);

                    if (modelAsset.IsValid())
                        LoadList_Collision.Add(modelAsset);
                }

                // Navmesh
                if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.DS1 or ProjectType.DS1R)
                {
                    if (model.Name.StartsWith("n"))
                    {
                        var modelAsset = ModelLocator.GetMapNVMModel(AdjustedMapID, ModelLocator.MapModelNameToAssetName(AdjustedMapID, model.Name));

                        if (modelAsset.IsValid())
                            LoadList_Navmesh.Add(modelAsset);
                    }
                }
            }
        }

        public void SetupTexturelLoadLists()
        {
            // Models
            foreach (IMsbModel model in Msb.Models.GetEntries())
            {
                // Character
                if (model.Name.StartsWith("c"))
                {
                    var textureAsset = TextureLocator.GetChrTextures(model.Name);

                    if (textureAsset.IsValid())
                        LoadList_Character_Texture.Add(textureAsset);
                }

                // Object
                if (model.Name.StartsWith("o"))
                {
                    var textureAsset = TextureLocator.GetObjTextureContainer(model.Name);

                    if (textureAsset.IsValid())
                        LoadList_Asset_Texture.Add(textureAsset);
                }
            }

            // AET
            /*
            if(Smithbox.ProjectType is ProjectType.ER)
            {
                var erMap = (MSBE)Msb;

                foreach(var entry in erMap.Parts.Assets)
                {
                    var name = entry.ModelName.ToLower().Replace("aeg", "aet");
                    var asset = ResourceTextureLocator.GetAetTexture(name);
                    LoadList_Other_Texture.Add(asset);
                }
            }
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                var acMap = (MSB_AC6)Msb;

                foreach (var entry in acMap.Parts.Assets)
                {
                    var name = entry.ModelName.ToLower().Replace("aeg", "aet");
                    var asset = ResourceTextureLocator.GetAetTexture(name);
                    LoadList_Other_Texture.Add(asset);
                }
            }
            */

            // AAT

            // SYSTEX

            // Map
            foreach (ResourceDescriptor asset in TextureLocator.GetMapTextures(AdjustedMapID))
            {
                if (asset.IsValid())
                    LoadList_Map_Texture.Add(asset);
            }
        }

        public void SetupModelMasks(MapContainer map)
        {
            foreach (Entity obj in map.Objects)
            {
                if (obj.WrappedObject is IMsbPart mp && mp.ModelName != null && mp.ModelName != "" &&
                    obj.RenderSceneMesh == null)
                {
                    int[]? masks = null;
                    if (obj is MsbEntity msbEnt)
                    {
                        masks = msbEnt.GetModelMasks();
                    }

                    Smithbox.EditorHandler.MapEditor.Universe.GetModelDrawable(map, obj, mp.ModelName, false, masks);
                }
            }
        }

        public void LoadLights(MapContainer map)
        {
            List<ResourceDescriptor> BTLs = MapLocator.GetMapBTLs(MapID);
            foreach (ResourceDescriptor btl_ad in BTLs)
            {
                BTL btl = LoadBTL(btl_ad);
                if (btl != null)
                {
                    map.LoadBTL(btl_ad, btl);
                }
            }
        }

        private BTL LoadBTL(ResourceDescriptor ad)
        {
            try
            {
                BTL btl;

                if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
                {
                    var bhdb = Smithbox.FS.ReadFile(ad.AssetPath).Value;
                    var bdtb = Smithbox.FS.ReadFile(ad.AssetPath[..^3] + "bdt").Value;
                    using var bdt = BXF4.Read(bhdb, bdtb);
                    BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));
                    if (file == null)
                    {
                        return null;
                    }

                    btl = BTL.Read(file.Bytes);
                }
                else
                {
                    btl = BTL.Read(Smithbox.FS.GetFile(ad.AssetPath).GetData());
                }

                return btl;
            }
            catch (InvalidDataException e)
            {
                TaskLogs.AddLog($"Failed to load {ad.AssetName}",
                    LogLevel.Error, TaskLogs.LogPriority.Normal, e);
                return null;
            }
        }

        public List<Task> LoadTextures(List<Task> tasks, MapContainer map)
        {
            if (!CFG.Current.Viewport_Enable_Texturing)
                return tasks;

            // Map
            var texJob = ResourceManager.CreateNewJob($@"{AdjustedMapID} Textures");

            foreach (ResourceDescriptor asset in LoadList_Map_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);

            // Character
            texJob = ResourceManager.CreateNewJob($@"Character Textures");

            foreach (ResourceDescriptor asset in LoadList_Character_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);

            // Asset
            texJob = ResourceManager.CreateNewJob($@"Asset Textures");

            foreach (ResourceDescriptor asset in LoadList_Asset_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);

            // Other Textures
            texJob = ResourceManager.CreateNewJob($@"Other Textures");

            foreach (ResourceDescriptor asset in LoadList_Other_Texture)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    texJob.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false, ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    texJob.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = texJob.Complete();
            tasks.Add(task);

            return tasks;
        }

        public List<Task> LoadModels(List<Task> tasks, MapContainer map)
        {
            // MapPieces
            var job = ResourceManager.CreateNewJob($@"MapPieces");

            foreach (ResourceDescriptor asset in LoadList_MapPiece_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);

            // Characters
            job = ResourceManager.CreateNewJob($@"Characters");

            foreach (ResourceDescriptor asset in LoadList_Character_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);

            // Objects
            job = ResourceManager.CreateNewJob($@"Assets");

            foreach (ResourceDescriptor asset in LoadList_Asset_Model)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                        ResourceManager.ResourceType.Flver);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            task = job.Complete();
            tasks.Add(task);

            // Collisions
            job = ResourceManager.CreateNewJob($@"Collisions");

            string archive = null;
            HashSet<string> collisionAssets = new();

            foreach (ResourceDescriptor asset in LoadList_Collision)
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    archive = asset.AssetArchiveVirtualPath;
                    collisionAssets.Add(asset.AssetVirtualPath);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }
            }

            if (archive != null)
            {
                job.AddLoadArchiveTask(archive, AccessLevel.AccessGPUOptimizedOnly, false, collisionAssets);
            }

            task = job.Complete();
            tasks.Add(task);

            // Navmesh
            job = ResourceManager.CreateNewJob($@"Navmesh");

            if (Smithbox.ProjectType != ProjectType.DS3)
            {
                foreach (ResourceDescriptor asset in LoadList_Navmesh)
                {
                    if (asset.AssetArchiveVirtualPath != null)
                    {
                        job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                            false);
                    }
                    else if (asset.AssetVirtualPath != null)
                    {
                        job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                    }
                }
            }
            else
            {
                ResourceDescriptor nav = ModelLocator.GetHavokNavmeshes(AdjustedMapID);
                job.AddLoadArchiveTask(nav.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                    ResourceManager.ResourceType.NavmeshHKX);
            }

            task = job.Complete();
            tasks.Add(task);

            return tasks;
        }

        public void SetupNavmesh(MapContainer map)
        {
            // DS3 Navmeshes
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                ResourceDescriptor nvaasset = MapLocator.GetMapNVA(AdjustedMapID);
                if (nvaasset.AssetPath != null)
                {
                    var nva = NVA.Read(Smithbox.FS.GetFile(nvaasset.AssetPath).GetData());
                    foreach (NVA.Navmesh currentNav in nva.Navmeshes)
                    {
                        // TODO2: set parent to MapOffset
                        MsbEntity n = new(map, currentNav, MsbEntity.MsbEntityType.Editor);
                        map.AddObject(n);
                        var navid = $@"n{currentNav.ModelID:D6}";
                        var navname = "n" + ModelLocator.MapModelNameToAssetName(AdjustedMapID, navid).Substring(1);

                        ResourceDescriptor nasset = ModelLocator.GetHavokNavmeshModel(AdjustedMapID, navname);

                        var mesh = MeshRenderableProxy.MeshRenderableFromHavokNavmeshResource(
                            Smithbox.EditorHandler.MapEditor.Universe._renderScene, nasset.AssetVirtualPath, ModelMarkerType.Other);
                        mesh.World = n.GetWorldMatrix();
                        mesh.SetSelectable(n);
                        mesh.DrawFilter = RenderFilter.Navmesh;
                        n.RenderSceneMesh = mesh;
                    }
                }
            }
        }
    }
}
