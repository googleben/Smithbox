﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum RegionType : int
        {
            EntryPoint = 1,
            EnvMapPoint = 2,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            EnvMapEffectBox = 17,
            WindPlacement = 18,
            MufflingBox = 28,
            MufflingPortal = 29,
            SoundOverride = 30,
            Patrol = 32,
            FeMapDisplay = 33,
            OperationalArea = 35,
            AiInformationSharing = 36,
            AiTarget = 37,
            WwiseEnvironmentSound = 39,
            NaviGeneration = 45,
            TopdownView = 46,
            CharacterFollowing = 47,
            NavmeshCostControl = 49,
            ArenaControl = 50,
            ArenaAppearance = 51,
            GarageCamera = 52,
            JumpEdgeRestriction = 53,
            CutscenePlayback = 54,
            FallPreventionWallRemoval = 55,
            BigJump = 56,
            Other = -1,
        }

        /// <summary>
        /// Points and volumes used to trigger various effects.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            private int ParamVersion;

            /// <summary>
            /// Unknown
            /// </summary>
            public List<Region.EntryPoint> EntryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvMapPoint> EnvMapPoints { get; set; }

            /// <summary>
            /// Areas where a sound will play.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// Points for particle effects to play at.
            /// </summary>
            public List<Region.SFX> SFX { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvMapEffectBox> EnvMapEffectBoxes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindPlacement> WindPlacements { get; set; }

            /// <summary>
            /// Areas where sound is muffled.
            /// </summary>
            public List<Region.MufflingBox> MufflingBoxes { get; set; }

            /// <summary>
            /// Entrances to muffling boxes.
            /// </summary>
            public List<Region.MufflingPortal> MufflingPortals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.SoundOverride> SoundOverrides { get; set; }

            /// <summary>
            /// Points that describe an NPC patrol path.
            /// </summary>
            public List<Region.Patrol> Patrols { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FeMapDisplay> FeMapDisplays { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.OperationalArea> OperationalAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AiInformationSharing> AiInformationSharings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AiTarget> AiTargets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WwiseEnvironmentSound> WwiseEnvironmentSounds { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NaviGeneration> NaviGenerations { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.TopdownView> TopdownViews { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.CharacterFollowing> CharacterFollowings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NavmeshCostControl> NavmeshCostControls { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ArenaControl> ArenaControls { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ArenaAppearance> ArenaAppearances { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.GarageCamera> GarageCameras { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.JumpEdgeRestriction> JumpEdgeRestrictions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.CutscenePlayback> CutscenePlaybacks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FallPreventionWallRemoval> FallPreventionWallRemovals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.BigJump> BigJumps { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(52, "POINT_PARAM_ST")
            {
                ParamVersion = base.Version;

                EntryPoints = new List<Region.EntryPoint>(); // 1
                EnvMapPoints = new List<Region.EnvMapPoint>(); // 2
                Sounds = new List<Region.Sound>(); // 4
                SFX = new List<Region.SFX>(); // 5
                WindSFX = new List<Region.WindSFX>(); // 6
                EnvMapEffectBoxes = new List<Region.EnvMapEffectBox>(); // 17
                WindPlacements = new List<Region.WindPlacement>(); // 18
                MufflingBoxes = new List<Region.MufflingBox>(); // 28
                MufflingPortals = new List<Region.MufflingPortal>(); // 29
                SoundOverrides = new List<Region.SoundOverride>(); // 30
                Patrols = new List<Region.Patrol>(); // 32
                FeMapDisplays = new List<Region.FeMapDisplay>(); // 33
                OperationalAreas = new List<Region.OperationalArea>(); // 35
                AiInformationSharings = new List<Region.AiInformationSharing>(); // 36
                AiTargets = new List<Region.AiTarget>(); // 37
                WwiseEnvironmentSounds = new List<Region.WwiseEnvironmentSound>(); // 39
                NaviGenerations = new List<Region.NaviGeneration>();  // 45
                TopdownViews = new List<Region.TopdownView>(); // 46
                CharacterFollowings = new List<Region.CharacterFollowing>(); // 47
                NavmeshCostControls = new List<Region.NavmeshCostControl>(); // 49
                ArenaControls = new List<Region.ArenaControl>(); // 50
                ArenaAppearances = new List<Region.ArenaAppearance>(); // 51
                GarageCameras = new List<Region.GarageCamera>(); // 52
                JumpEdgeRestrictions = new List<Region.JumpEdgeRestriction>(); // 53
                CutscenePlaybacks = new List<Region.CutscenePlayback>(); // 54
                FallPreventionWallRemovals = new List<Region.FallPreventionWallRemoval>(); // 55
                BigJumps = new List<Region.BigJump>(); // 56
                Others = new List<Region.Other>(); // -1
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    // 1
                    case Region.EntryPoint r:
                        EntryPoints.Add(r);
                        break;
                    // 2
                    case Region.EnvMapPoint r:
                        EnvMapPoints.Add(r);
                        break;
                    // 4
                    case Region.Sound r:
                        Sounds.Add(r);
                        break;
                    // 5
                    case Region.SFX r:
                        SFX.Add(r);
                        break;
                    // 6
                    case Region.WindSFX r:
                        WindSFX.Add(r);
                        break;
                    // 17
                    case Region.EnvMapEffectBox r:
                        EnvMapEffectBoxes.Add(r);
                        break;
                    // 18
                    case Region.WindPlacement r:
                        WindPlacements.Add(r);
                        break;
                    // 28
                    case Region.MufflingBox r:
                        MufflingBoxes.Add(r);
                        break;
                    // 29
                    case Region.MufflingPortal r:
                        MufflingPortals.Add(r);
                        break;
                    // 30
                    case Region.SoundOverride r:
                        SoundOverrides.Add(r);
                        break;
                    // 32
                    case Region.Patrol r:
                        Patrols.Add(r);
                        break;
                    // 33
                    case Region.FeMapDisplay r:
                        FeMapDisplays.Add(r);
                        break;
                    // 35
                    case Region.OperationalArea r:
                        OperationalAreas.Add(r);
                        break;
                    // 36
                    case Region.AiInformationSharing r:
                        AiInformationSharings.Add(r);
                        break;
                    // 37
                    case Region.AiTarget r:
                        AiTargets.Add(r);
                        break;
                    // 39
                    case Region.WwiseEnvironmentSound r:
                        WwiseEnvironmentSounds.Add(r);
                        break;
                    // 45
                    case Region.NaviGeneration r:
                        NaviGenerations.Add(r);
                        break;
                    // 46
                    case Region.TopdownView r:
                        TopdownViews.Add(r);
                        break;
                    // 47
                    case Region.CharacterFollowing r:
                        CharacterFollowings.Add(r);
                        break;
                    // 49
                    case Region.NavmeshCostControl r:
                        NavmeshCostControls.Add(r);
                        break;
                    // 50
                    case Region.ArenaControl r:
                        ArenaControls.Add(r);
                        break;
                    // 51
                    case Region.ArenaAppearance r:
                        ArenaAppearances.Add(r);
                        break;
                    // 52
                    case Region.GarageCamera r:
                        GarageCameras.Add(r);
                        break;
                    // 53
                    case Region.JumpEdgeRestriction r:
                        JumpEdgeRestrictions.Add(r);
                        break;
                    // 54
                    case Region.CutscenePlayback r:
                        CutscenePlaybacks.Add(r);
                        break;
                    // 55
                    case Region.FallPreventionWallRemoval r:
                        FallPreventionWallRemovals.Add(r);
                        break;
                    // 56
                    case Region.BigJump r:
                        BigJumps.Add(r);
                        break;
                    // -1
                    case Region.Other r:
                        Others.Add(r);
                        break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => Add((Region)item);

            /// <summary>
            /// Returns every region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries()
            {
                return SFUtil.ConcatAll<Region>(
                    EntryPoints, 
                    EnvMapPoints, 
                    Sounds, 
                    SFX, 
                    WindSFX,
                    EnvMapEffectBoxes,
                    WindPlacements,
                    MufflingBoxes,
                    MufflingPortals, 
                    SoundOverrides, 
                    Patrols,
                    FeMapDisplays, 
                    OperationalAreas,
                    AiInformationSharings,
                    AiTargets, 
                    WwiseEnvironmentSounds,
                    NaviGenerations, 
                    TopdownViews, 
                    CharacterFollowings, 
                    NavmeshCostControls, 
                    ArenaControls,
                    ArenaAppearances, 
                    GarageCameras, 
                    JumpEdgeRestrictions, 
                    CutscenePlaybacks, 
                    FallPreventionWallRemovals, 
                    BigJumps,
                    Others
                );
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    // 1
                    case RegionType.EntryPoint:
                        return EntryPoints.EchoAdd(new Region.EntryPoint(br));

                    // 2
                    case RegionType.EnvMapPoint:
                        return EnvMapPoints.EchoAdd(new Region.EnvMapPoint(br, ParamVersion));

                    // 4
                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    // 5
                    case RegionType.SFX:
                        return SFX.EchoAdd(new Region.SFX(br));

                    // 6
                    case RegionType.WindSFX:
                        return WindSFX.EchoAdd(new Region.WindSFX(br));

                    // 17
                    case RegionType.EnvMapEffectBox:
                        return EnvMapEffectBoxes.EchoAdd(new Region.EnvMapEffectBox(br));

                    // 18
                    case RegionType.WindPlacement:
                        return WindPlacements.EchoAdd(new Region.WindPlacement(br, offsetLength));

                    // 28
                    case RegionType.MufflingBox:
                        return MufflingBoxes.EchoAdd(new Region.MufflingBox(br));

                    // 29
                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    // 30
                    case RegionType.SoundOverride:
                        return SoundOverrides.EchoAdd(new Region.SoundOverride(br));

                    // 32
                    case RegionType.Patrol:
                        return Patrols.EchoAdd(new Region.Patrol(br));

                    // 33
                    case RegionType.FeMapDisplay:
                        return FeMapDisplays.EchoAdd(new Region.FeMapDisplay(br));

                    // 35
                    case RegionType.OperationalArea:
                        return OperationalAreas.EchoAdd(new Region.OperationalArea(br));

                    // 36
                    case RegionType.AiInformationSharing:
                        return AiInformationSharings.EchoAdd(new Region.AiInformationSharing(br, offsetLength));

                    // 37
                    case RegionType.AiTarget:
                        return AiTargets.EchoAdd(new Region.AiTarget(br));

                    // 39
                    case RegionType.WwiseEnvironmentSound:
                        return WwiseEnvironmentSounds.EchoAdd(new Region.WwiseEnvironmentSound(br));

                    // 45
                    case RegionType.NaviGeneration:
                        return NaviGenerations.EchoAdd(new Region.NaviGeneration(br, offsetLength));

                    // 46
                    case RegionType.TopdownView:
                        return TopdownViews.EchoAdd(new Region.TopdownView(br, offsetLength));

                    // 47
                    case RegionType.CharacterFollowing:
                        return CharacterFollowings.EchoAdd(new Region.CharacterFollowing(br));

                    // 49
                    case RegionType.NavmeshCostControl:
                        return NavmeshCostControls.EchoAdd(new Region.NavmeshCostControl(br));

                    // 50
                    case RegionType.ArenaControl:
                        return ArenaControls.EchoAdd(new Region.ArenaControl(br));

                    // 51
                    case RegionType.ArenaAppearance:
                        return ArenaAppearances.EchoAdd(new Region.ArenaAppearance(br));

                    // 52
                    case RegionType.GarageCamera:
                        return GarageCameras.EchoAdd(new Region.GarageCamera(br));

                    // 53
                    case RegionType.JumpEdgeRestriction:
                        return JumpEdgeRestrictions.EchoAdd(new Region.JumpEdgeRestriction(br, offsetLength));

                    // 54
                    case RegionType.CutscenePlayback:
                        return CutscenePlaybacks.EchoAdd(new Region.CutscenePlayback(br));

                    // 55
                    case RegionType.FallPreventionWallRemoval:
                        return FallPreventionWallRemovals.EchoAdd(new Region.FallPreventionWallRemoval(br));

                    // 56
                    case RegionType.BigJump:
                        return BigJumps.EchoAdd(new Region.BigJump(br));

                    // -1
                    case RegionType.Other:
                        return Others.EchoAdd(new Region.Other(br, offsetLength));

                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }
        }

        /// <summary>
        /// A point or volume that triggers some sort of interaction.
        /// </summary>
        public abstract class Region : Entry, IMsbRegion
        {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            // Index among points of the same type
            public int TypeIndex { get; set; }

            /// <summary>
            /// The shape of the region.
            /// </summary>
            public MSB.Shape Shape { get; set; }

            /// <summary>
            /// The location of the region.
            /// </summary>
            [PositionProperty]
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotiation of the region, in degrees.
            /// </summary>
            [RotationProperty]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long parentListOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long childListOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk78 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk7C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> ParentListIndices { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> ChildListIndices { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long NameOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long FormOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long CommonOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long TypeOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long Struct98Offset { get; set; }


            /// <summary>
            /// Unknown.
            /// </summary>
            /// /// <summary>
            /// If specified, the region is only active when the part is loaded.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string ActivationPartName { get; set; }
            public int ActivationPartIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkC08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EntityGroupID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkC10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkC28 { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                Unk2C = -1;
                ParentListIndices = new List<short>();
                ChildListIndices = new List<short>();
                Unk78 = -1;
                Unk7C = -1;
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                DeepCopyTo(region);
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Region region) { }

            private protected Region(BinaryReaderEx br)
            {
                long start = br.Position;
                NameOffset = br.ReadInt64();
                br.AssertInt32((int)Type);
                TypeIndex = br.ReadInt32();
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                Shape = MSB.Shape.Create(shapeType);
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk2C = br.ReadInt32();

                parentListOffset = br.ReadInt64();
                childListOffset = br.ReadInt64();

                Unk78 = br.ReadInt32();
                Unk7C = br.ReadInt32();

                FormOffset = br.ReadInt64();
                CommonOffset = br.ReadInt64();
                TypeOffset = br.ReadInt64();
                Struct98Offset = br.ReadInt64();

                // Name
                Name = br.GetUTF16(start + NameOffset);

                // Point Indices 30
                br.Position = start + parentListOffset;
                short countA = br.ReadInt16();
                ParentListIndices = new List<short>(br.ReadInt16s(countA));

                // Point Indices 38
                br.Position = start + childListOffset;
                short countB = br.ReadInt16();
                ChildListIndices = new List<short>(br.ReadInt16s(countB));

                // Shape
                if (Shape.HasShapeData && FormOffset != 0L)
                {
                    br.Position = start + FormOffset;
                    Shape.ReadShapeData(br);
                }

                // Common
                br.Position = start + CommonOffset;

                ActivationPartIndex = br.ReadInt32();
                EntityID = br.ReadUInt32();
                UnkC08 = br.ReadSByte();
                br.AssertByte(new byte[1]);
                br.AssertInt16((short)-1);
                EntityGroupID = br.ReadInt32();
                UnkC10 = br.ReadInt32();
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                UnkC28 = br.ReadInt32();
                br.AssertInt32(new int[1]);

                // Type
                if (HasTypeData && TypeOffset != 0L)
                {
                    br.Position = start + TypeOffset;
                    ReadTypeData(br);
                }

                if(Type == RegionType.Other)
                {
                    OtherTypeData = null;

                    if (TypeOffset > 0)
                    {
                        long otherSize = Struct98Offset - TypeOffset;

                        if (otherSize > 0)
                            OtherTypeData = br.ReadBytes((int)otherSize);
                    }
                }

                // Struct98 Offset
                br.Position = start + Struct98Offset;
                br.AssertInt32(-1);
                br.AssertInt32(new int[1]);
                br.AssertInt32(-1);
            }

            private byte[] OtherTypeData;

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32((int) Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2C);

                bw.ReserveInt64("IndexListOffset30");
                bw.ReserveInt64("IndexListOffset38");

                bw.WriteInt32(Unk78);
                bw.WriteInt32(Unk7C);

                bw.ReserveInt64("FormOffset");
                bw.ReserveInt64("CommonOffset");
                bw.ReserveInt64("TypeOffset");
                bw.ReserveInt64("Struct98Offset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt64("IndexListOffset30", bw.Position - start);
                bw.WriteInt16((short)ParentListIndices.Count);
                bw.WriteInt16s(ParentListIndices);
                bw.Pad(4);

                bw.FillInt64("IndexListOffset38", bw.Position - start);
                bw.WriteInt16((short)ChildListIndices.Count);
                bw.WriteInt16s(ChildListIndices);
                bw.Pad(8);

                if (Shape.HasShapeData && FormOffset != 0L)
                {
                    bw.FillInt64("FormOffset", bw.Position - start);
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillInt64("FormOffset", 0L);
                }

                bw.FillInt64("CommonOffset", bw.Position - start);
                bw.WriteInt32(ActivationPartIndex);
                bw.WriteUInt32(EntityID);
                bw.WriteSByte(UnkC08);
                bw.WriteByte((byte)0);
                bw.WriteInt16((short)-1);
                bw.WriteInt32(EntityGroupID);
                bw.WriteInt32(UnkC10);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(UnkC28);
                bw.WriteInt32(0);

                if (Type >= RegionType.MufflingBox || Type == RegionType.Other)
                {
                    bw.Pad(8);
                }

                if (HasTypeData && TypeOffset != 0L)
                {
                    bw.FillInt64("TypeOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeOffset", 0L);
                }

                if(Type == RegionType.Other)
                {
                    if(OtherTypeData != null)
                    {
                        bw.WriteBytes(OtherTypeData);
                    }
                }

                if (HasTypeDataPadding())
                {
                    bw.Pad(8);
                }

                bw.FillInt64("Struct98Offset", bw.Position - start);
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.WriteInt32(-1);

                if (HasStruct98Padding())
                {
                    bw.WriteInt32(0);
                }
            }

            private bool HasTypeDataPadding()
            {
                switch (Type)
                {
                    case RegionType.EntryPoint:
                    case RegionType.EnvMapPoint:
                    case RegionType.Sound:
                    case RegionType.SFX:
                    case RegionType.WindSFX:
                    case RegionType.EnvMapEffectBox:
                    case RegionType.WindPlacement:
                    case RegionType.MufflingBox:
                    case RegionType.MufflingPortal:
                    case RegionType.SoundOverride:
                    case RegionType.Patrol:
                    case RegionType.FeMapDisplay:
                        return true;

                    case RegionType.OperationalArea:
                        return false;

                    case RegionType.AiInformationSharing:
                    case RegionType.AiTarget:
                        return true;

                    case RegionType.WwiseEnvironmentSound:
                        return false;

                    case RegionType.NaviGeneration:
                    case RegionType.TopdownView:
                    case RegionType.CharacterFollowing:
                        return true;

                    case RegionType.NavmeshCostControl:
                    case RegionType.ArenaControl:
                    case RegionType.ArenaAppearance:
                        return false;

                    case RegionType.GarageCamera:
                    case RegionType.JumpEdgeRestriction:
                    case RegionType.CutscenePlayback:
                    case RegionType.FallPreventionWallRemoval:
                    case RegionType.BigJump:
                    case RegionType.Other:
                        return true;

                    default:
                        return false;
                }
            }

            // Which types have 4 bytes of padding at the end
            private bool HasStruct98Padding()
            {
                switch(Type)
                {
                    case RegionType.EntryPoint:
                    case RegionType.EnvMapPoint:
                    case RegionType.Sound:
                    case RegionType.SFX:
                    case RegionType.WindSFX:
                    case RegionType.EnvMapEffectBox:
                    case RegionType.WindPlacement:
                    case RegionType.MufflingBox:
                    case RegionType.MufflingPortal:
                    case RegionType.SoundOverride:
                    case RegionType.Patrol:
                    case RegionType.FeMapDisplay:
                        return true;

                    case RegionType.OperationalArea:
                        return false;

                    case RegionType.AiInformationSharing:
                    case RegionType.AiTarget:
                        return true;

                    case RegionType.WwiseEnvironmentSound:
                        return false;

                    case RegionType.NaviGeneration:
                    case RegionType.TopdownView:
                    case RegionType.CharacterFollowing:
                        return true;

                    case RegionType.NavmeshCostControl:
                    case RegionType.ArenaControl:
                    case RegionType.ArenaAppearance:
                        return false;

                    case RegionType.GarageCamera:
                    case RegionType.JumpEdgeRestriction:
                    case RegionType.CutscenePlayback:
                    case RegionType.FallPreventionWallRemoval:
                    case RegionType.BigJump:
                    case RegionType.Other:
                        return true;

                    default: 
                        return false;
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(Entries entries)
            {
                ActivationPartName = MSB.FindName(entries.Parts, ActivationPartIndex);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetNames(entries.Regions);
            }

            internal virtual void GetIndices(Entries entries)
            {
                ActivationPartIndex = MSB.FindIndex(this, entries.Parts, ActivationPartName);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetIndices(entries.Regions);
            }

            public RegionType GetRegionType()
            {
                return Type;
            }

            /// <summary>
            /// Returns the type, shape type, and name of the region as a string.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"{Type} {Shape.Type} {Name}";
            }

            /// <summary>
            /// A point where a player can invade your world.
            /// </summary>
            public class EntryPoint : Region
            {
                private protected override RegionType Type => RegionType.EntryPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Not sure what this does.
                /// </summary>
                public int Priority { get; set; }

                /// <summary>
                /// Creates an InvasionPoint with default values.
                /// </summary>
                public EntryPoint() : base($"{nameof(Region)}: {nameof(EntryPoint)}") { }

                internal EntryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Priority = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Priority);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvMapPoint : Region
            {
                private int version;
                private protected override RegionType Type => RegionType.EnvMapPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Creates an EnvMapPoint with default values.
                /// </summary>
                public EnvMapPoint() : base($"{nameof(Region)}: {nameof(EnvMapPoint)}")
                {
                    UnkT00 = 1000f;
                    UnkT04 = 4;
                    UnkT0C = (byte)1;
                    UnkT0D = (byte)1;
                    UnkT0F = (byte)1;
                    UnkT10 = 1f;
                }

                internal EnvMapPoint(BinaryReaderEx br, int _version) : base(br) 
                {
                    version = _version;
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var point = (EnvMapPoint)region;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT0C = br.ReadByte();
                    UnkT0D = br.ReadByte();
                    br.AssertByte((byte)1);
                    UnkT0F = br.ReadByte();
                    UnkT10 = br.ReadSingle();
                    br.AssertSingle(1f);
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();

                    if (version < 52)
                        return;

                    UnkT20 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteByte(UnkT0C);
                    bw.WriteByte(UnkT0D);
                    bw.WriteByte((byte)1);
                    bw.WriteByte(UnkT0F);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(1f);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);

                    if (version < 52)
                        return;

                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An area where a sound plays.
            /// </summary>
            public class Sound : Region
            {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The category of the sound.
                /// </summary>
                public int SoundType { get; set; }

                /// <summary>
                /// The ID of the sound.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// References to other regions used to build a composite shape.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] ChildRegionNames { get; private set; }
                public int[] ChildRegionIndices;

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}")
                {
                    ChildRegionNames = new string[16];
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var sound = (Sound)region;
                    sound.ChildRegionNames = (string[])ChildRegionNames.Clone();
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    ChildRegionIndices = br.ReadInt32s(16);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    ChildRegionNames = MSB.FindNames(entries.Regions, ChildRegionIndices);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    ChildRegionIndices = MSB.FindIndices(this, entries.Regions, ChildRegionNames);
                }
            }

            /// <summary>
            /// A point where a particle effect can play.
            /// </summary>
            public class SFX : Region
            {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The ID of the particle effect FFX.
                /// </summary>
                [MSBAliasEnum(AliasEnumType = "PARTICLES")]
                public int EffectID { get; set; }

                /// <summary>
                /// If true, the effect is off by default until enabled by event scripts.
                /// </summary>
                public int StartDisabled { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    StartDisabled = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(StartDisabled);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindSFX : Region
            {
                private protected override RegionType Type => RegionType.WindSFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the effect FFX.
                /// </summary>
                [MSBAliasEnum(AliasEnumType = "PARTICLES")]
                public int EffectID { get; set; }

                /// <summary>
                /// Reference to a WindArea region.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string WindAreaName { get; set; }
                public int WindAreaIndex;

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    WindAreaIndex = br.ReadInt32();
                    br.AssertSingle(-1f);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(-1f);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    WindAreaName = MSB.FindName(entries.Regions, WindAreaIndex);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    WindAreaIndex = MSB.FindIndex(this, entries.Regions, WindAreaName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvMapEffectBox : Region
            {
                private protected override RegionType Type => RegionType.EnvMapEffectBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Distance from camera required before enabling envmap. 0 = always enabled.
                /// </summary>
                public float EnableDist { get; set; }

                /// <summary>
                /// Distance it takes for an envmap to fully transition into view.
                /// </summary>
                public float TransitionDist { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0B { get; set; }

                /// <summary>
                /// Strength of specular light in region.
                /// </summary>
                public float SpecularLightMult { get; set; }

                /// <summary>
                /// Strength of direct light emitting from EnvironmentMapPoint.
                /// </summary>
                public float PointLightMult { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT2C { get; set; }

                /// <summary>
                /// Affects lighting with other fields when true. Possibly normalizes light when false.
                /// </summary>
                public byte IsModifyLight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT32 { get; set; }

                /// <summary>
                /// Creates an EnvMapEffectBox with default values.
                /// </summary>
                public EnvMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvMapEffectBox)}") 
                {
                    SpecularLightMult = 1f;
                    PointLightMult = 1f;
                    IsModifyLight = (byte)1;
                    UnkT30 = (short)-1;
                    UnkT32 = (short)-1;
                }

                internal EnvMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnableDist = br.ReadSingle();
                    TransitionDist = br.ReadSingle();
                    br.AssertByte(new byte[1]);
                    UnkT09 = br.ReadByte();
                    UnkT0A = br.ReadByte();
                    UnkT0B = br.ReadByte();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    SpecularLightMult = br.ReadSingle();
                    PointLightMult = br.ReadSingle();
                    UnkT2C = br.ReadInt16();
                    IsModifyLight = br.ReadByte();
                    UnkT2F = br.ReadByte();
                    UnkT30 = br.ReadInt16();
                    UnkT32 = br.ReadInt16();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(EnableDist);
                    bw.WriteSingle(TransitionDist);
                    bw.WriteByte((byte)0);
                    bw.WriteByte(UnkT09);
                    bw.WriteByte(UnkT0A);
                    bw.WriteByte(UnkT0B);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(SpecularLightMult);
                    bw.WriteSingle(PointLightMult);
                    bw.WriteInt16(UnkT2C);
                    bw.WriteByte(IsModifyLight);
                    bw.WriteByte(UnkT2F);
                    bw.WriteInt16(UnkT30);
                    bw.WriteInt16(UnkT32);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindPlacement : Region
            {
                private protected override RegionType Type => RegionType.WindPlacement;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public WindPlacement() : base($"{nameof(Region)}: {nameof(WindPlacement)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal WindPlacement(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// An area where sound is muffled.
            /// </summary>
            public class MufflingBox : Region
            {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT34 { get; set; }

                /// <summary>
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadSingle();
                    UnkT28 = br.ReadSingle();
                    UnkT2C = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    UnkT34 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                    br.AssertSingle(-1f);
                    br.AssertSingle(-1f);
                    br.AssertSingle(-1f);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
                    bw.WriteInt32(UnkT20);
                    bw.WriteSingle(UnkT24);
                    bw.WriteSingle(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT34);
                    bw.WriteInt32(0);
                    bw.WriteSingle(-1f);
                    bw.WriteSingle(-1f);
                    bw.WriteSingle(-1f);
                }
            }

            /// <summary>
            /// An entrance to a muffling box.
            /// </summary>
            public class MufflingPortal : Region
            {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a MufflingPortal with default values.
                /// </summary>
                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SoundOverride : Region
            {
                private protected override RegionType Type => RegionType.SoundOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT03 { get; set; }

                /// <summary>
                /// Creates a SoundOverride with default values.
                /// </summary>
                public SoundOverride() : base($"{nameof(Region)}: {nameof(SoundOverride)}") { }

                internal SoundOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    UnkT03 = br.ReadSByte();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte(UnkT00);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteSByte(UnkT03);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A point along an NPC patrol path.
            /// </summary>
            public class Patrol : Region
            {
                private protected override RegionType Type => RegionType.Patrol;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT04 { get; set; }

                /// <summary>
                /// Creates a Patrol with default values.
                /// </summary>
                public Patrol() : base($"{nameof(Region)}: {nameof(Patrol)}") { }

                internal Patrol(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSByte(UnkT04);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class FeMapDisplay : Region
            {
                private protected override RegionType Type => RegionType.FeMapDisplay;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a MapPoint with default values.
                /// </summary>
                public FeMapDisplay() : base($"{nameof(Region)}: {nameof(FeMapDisplay)}") { }

                internal FeMapDisplay(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadByte();
                    UnkT03 = br.ReadByte();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteByte(UnkT02);
                    bw.WriteByte(UnkT03);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class OperationalArea : Region
            {
                private protected override RegionType Type => RegionType.OperationalArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a OperationalArea with default values.
                /// </summary>
                public OperationalArea() : base($"{nameof(Region)}: {nameof(OperationalArea)}") { }

                internal OperationalArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(UnkT18);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AiInformationSharing : Region
            {
                private protected override RegionType Type => RegionType.AiInformationSharing;
                private protected override bool HasTypeData => true;

                private byte[] Bytes { get; set; }

                private long Length { get; set; }
                public AiInformationSharing() : base($"{nameof(Region)}: {nameof(AiInformationSharing)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal AiInformationSharing(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AiTarget : Region
            {
                private protected override RegionType Type => RegionType.AiTarget;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a AiTarget with default values.
                /// </summary>
                public AiTarget() : base($"{nameof(Region)}: {nameof(AiTarget)}") { }

                internal AiTarget(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16((short)0);
                    bw.WriteInt32(UnkT04);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WwiseEnvironmentSound : Region
            {
                private protected override RegionType Type => RegionType.WwiseEnvironmentSound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown. May be: WwiseValuetoStrParam_RuntimeReflectTextType or WwiseValuetoStrParam_Material
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Creates an WwiseEnvironmentSound with default values.
                /// </summary>
                public WwiseEnvironmentSound() : base($"{nameof(Region)}: {nameof(WwiseEnvironmentSound)}") { }

                internal WwiseEnvironmentSound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16((short)0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NaviGeneration : Region
            {
                private protected override RegionType Type => RegionType.NaviGeneration;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public NaviGeneration() : base($"{nameof(Region)}: {nameof(NaviGeneration)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal NaviGeneration(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class TopdownView : Region
            {
                private protected override RegionType Type => RegionType.TopdownView;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public TopdownView() : base($"{nameof(Region)}: {nameof(TopdownView)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal TopdownView(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class CharacterFollowing : Region
            {
                private protected override RegionType Type => RegionType.CharacterFollowing;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Creates an CharacterFollowing with default values.
                /// </summary>
                public CharacterFollowing() : base($"{nameof(Region)}: {nameof(CharacterFollowing)}") { }

                internal CharacterFollowing(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    UnkT1C = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteSingle(UnkT1C);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NavmeshCostControl : Region
            {
                private protected override RegionType Type => RegionType.NavmeshCostControl;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown. Probably Navigation Weighting?
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates an NavmeshCostControl with default values.
                /// </summary>
                public NavmeshCostControl() : base($"{nameof(Region)}: {nameof(NavmeshCostControl)}") { }

                internal NavmeshCostControl(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ArenaControl : Region
            {
                private protected override RegionType Type => RegionType.ArenaControl;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an ArenaControl with default values.
                /// </summary>
                public ArenaControl() : base($"{nameof(Region)}: {nameof(ArenaControl)}") { }

                internal ArenaControl(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ArenaAppearance : Region
            {
                private protected override RegionType Type => RegionType.ArenaAppearance;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an ArenaAppearance with default values.
                /// </summary>
                public ArenaAppearance() : base($"{nameof(Region)}: {nameof(ArenaAppearance)}") { }

                internal ArenaAppearance(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class GarageCamera : Region
            {
                private protected override RegionType Type => RegionType.GarageCamera;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Creates an GarageCamera with default values.
                /// </summary>
                public GarageCamera() : base($"{nameof(Region)}: {nameof(GarageCamera)}") 
                {
                    UnkT00 = -1f;
                }

                internal GarageCamera(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class JumpEdgeRestriction : Region
            {
                private protected override RegionType Type => RegionType.JumpEdgeRestriction;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public JumpEdgeRestriction() : base($"{nameof(Region)}: {nameof(JumpEdgeRestriction)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal JumpEdgeRestriction(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class CutscenePlayback : Region
            {
                private protected override RegionType Type => RegionType.CutscenePlayback;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates an CutscenePlayback with default values.
                /// </summary>
                public CutscenePlayback() : base($"{nameof(Region)}: {nameof(CutscenePlayback)}") { }

                internal CutscenePlayback(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class FallPreventionWallRemoval : Region
            {
                private protected override RegionType Type => RegionType.FallPreventionWallRemoval;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an FallPreventionWallRemoval with default values.
                /// </summary>
                public FallPreventionWallRemoval() : base($"{nameof(Region)}: {nameof(FallPreventionWallRemoval)}") { }

                internal FallPreventionWallRemoval(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class BigJump : Region
            {
                private protected override RegionType Type => RegionType.BigJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "JumpSpecifyAltParam")]
                public int JumpSpecifyAltParamID { get; set; }

                /// <summary>
                /// Creates an BigJump with default values.
                /// </summary>
                public BigJump() : base($"{nameof(Region)}: {nameof(BigJump)}") { }

                internal BigJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpSpecifyAltParamID = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(JumpSpecifyAltParamID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Other.
            /// </summary>
            public class Other : Region
            {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") 
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Other(BinaryReaderEx br, long _length) : base(br) 
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }
        }
    }
}
