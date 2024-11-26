using MikuMikuLibrary.IO;
using MikuMikuLibrary.IO.Common;
using MikuMikuLibrary.IO.Sections;

namespace MikuMikuLibrary.Databases.PVPP;

public enum PerformerId : sbyte
{
    PVPP_PERFORMER_PNONE = -1,
    PVPP_PERFORMER_1P = 0x00,
    PVPP_PERFORMER_2P = 0x01,
    PVPP_PERFORMER_3P = 0x02,
    PVPP_PERFORMER_4P = 0x03,
}

public enum CharacterIndex : sbyte
{
    PVPP_CHARA_NONE = -1,
    PVPP_CHARA_MIKU = 0x00,
    PVPP_CHARA_RIN = 0x01,
    PVPP_CHARA_LEN = 0x02,
    PVPP_CHARA_LUKA = 0x03,
    PVPP_CHARA_NERU = 0x04,
    PVPP_CHARA_HAKU = 0x05,
    PVPP_CHARA_KAITO = 0x06,
    PVPP_CHARA_MEIKO = 0x07,
    PVPP_CHARA_SAKINE = 0x08,
    PVPP_CHARA_TETO = 0x09,
    PVPP_CHARA_EXTRA = 0x0A,
};

public enum CharacterItemType : sbyte
{
    PVPP_CHARA_ITEM_NONE = -1,
    PVPP_CHARA_ITEM_OBJ = 0x00,
    PVPP_CHARA_ITEM_OBJ_HRC = 0x01,
};

public class StringWithId
{
    public string Name { get; set; }
    public Int64 Id { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        Int64 offset = reader.ReadOffset();
        Id = reader.ReadInt64();
        Name = reader.ReadStringAtOffset(offset, StringBinaryFormat.NullTerminated);
    }

    public StringWithId()
    {
        Name = "";
        Id = 0;
    }
}

public class GlitterEntry
{
    public string Name { get; set; }
    public string UnkString { get; set; }
    public sbyte unk { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        Name = reader.ReadStringAtOffset(reader.ReadOffset(), StringBinaryFormat.NullTerminated);
        UnkString = reader.ReadStringAtOffset(reader.ReadOffset(), StringBinaryFormat.NullTerminated);
        unk = reader.ReadSByte();
    }

    public GlitterEntry()
    {
        Name = "";
        UnkString = null;
        unk = 0;
    }
}

public class SongEffect
{
    public PerformerId ParentId { get; set; }
    public List<StringWithId> Auth3dEntries { get; set; }
    public List<GlitterEntry> GlitterEntries { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        sbyte auth3dEntriesCount = reader.ReadSByte();
        Auth3dEntries.Capacity = auth3dEntriesCount;
        sbyte glitterEntriesCount = reader.ReadSByte();
        GlitterEntries.Capacity = glitterEntriesCount;

        ParentId = (PerformerId)reader.ReadSByte();
        reader.SeekCurrent(5);

        // Auth3d
        reader.ReadAtOffset(reader.ReadOffset(), () =>
        {
            for (int i = 0; i < auth3dEntriesCount; i++)
            {
                var entry = new StringWithId();
                entry.Read(reader);
                Auth3dEntries.Add(entry);
            }
        });

        // Glitter
        reader.ReadAtOffset(reader.ReadOffset(), () =>
        {
            for (int i = 0; i < glitterEntriesCount; i++)
            {
                GlitterEntry glitterEntry = new GlitterEntry();
                glitterEntry.Read(reader);
                GlitterEntries.Add(glitterEntry);
            }
        });
    }

    public SongEffect()
    {
        ParentId = PerformerId.PVPP_PERFORMER_PNONE;
        Auth3dEntries = new List<StringWithId>();
        GlitterEntries = new List<GlitterEntry>();
    }
}

public class MotionEntry
{
    public StringWithId MotionName { get; set; }
    public Int64 NextMotionOffset { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        Int64 motionNameOffset = reader.ReadOffset();
        NextMotionOffset = reader.ReadOffset();

        reader.ReadAtOffset(motionNameOffset, () =>
        {
            var motionEntry = new StringWithId();
            motionEntry.Read(reader);
            MotionName = motionEntry;
        });
    }

    public MotionEntry()
    {
        MotionName = new StringWithId();
        NextMotionOffset = 0;
    }
}

public class CharaEffEntry
{
    public class CharaEffAuth3d
    {
        public sbyte u00 { get; set; }
        public sbyte u01 { get; set; }
        public sbyte u02 { get; set; }
        public sbyte u03 { get; set; }
        public sbyte u04 { get; set; }
        public sbyte u05 { get; set; }
        public sbyte u06 { get; set; }
        public sbyte u07 { get; set; }
        public StringWithId CharaEffAuth3dEntry { get; set; }
        public StringWithId SourceAuth3dEntry { get; set; }

        public void Read(EndianBinaryReader reader)
        {
            u00 = reader.ReadSByte();
            u01 = reader.ReadSByte();
            u02 = reader.ReadSByte();
            u03 = reader.ReadSByte();
            u04 = reader.ReadSByte();
            u05 = reader.ReadSByte();
            u06 = reader.ReadSByte();
            u07 = reader.ReadSByte();
            Int64 CharaEffAuth3dEntryOffset = reader.ReadOffset();
            Int64 SourceAuth3dEntryOffset = reader.ReadOffset();

            reader.ReadAtOffset(CharaEffAuth3dEntryOffset, () =>
            {
                var a3dentry = new StringWithId();
                a3dentry.Read(reader);
                CharaEffAuth3dEntry = a3dentry;
            });

            reader.ReadAtOffset(SourceAuth3dEntryOffset, () =>
            {
                var sourceA3dentry = new StringWithId();
                sourceA3dentry.Read(reader);
                SourceAuth3dEntry = sourceA3dentry;
            });
        }

        public CharaEffAuth3d()
        {
            u00 = 0;
            u01 = 0;
            u02 = 0;
            u03 = 0;
            u04 = 0;
            u05 = 0;
            u06 = 0;
            u07 = 0;
            CharaEffAuth3dEntry = new StringWithId();
            SourceAuth3dEntry = new StringWithId();
        }
    }

    public CharacterIndex BaseCharacter { get; set; }
    public PerformerId ParentPerformerId { get; set; }
    public List<CharaEffAuth3d> CharaEffAuth3DEntries { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        BaseCharacter = (CharacterIndex)reader.ReadSByte();
        sbyte auth3dCount = reader.ReadSByte();
        CharaEffAuth3DEntries.Capacity = auth3dCount;
        ParentPerformerId = (PerformerId)reader.ReadSByte();
        Int64 a3doffset = reader.ReadOffset();
        reader.ReadAtOffset(a3doffset, () =>
        {
            for (int i = 0; i < auth3dCount; i++)
            {
                var a3dentry = new CharaEffAuth3d();
                a3dentry.Read(reader);
                CharaEffAuth3DEntries.Add(a3dentry);
            }
        });
    }

    public CharaEffEntry()
    {
        BaseCharacter = CharacterIndex.PVPP_CHARA_MIKU;
        ParentPerformerId = PerformerId.PVPP_PERFORMER_PNONE;
        CharaEffAuth3DEntries = new List<CharaEffAuth3d>();
    }
}

public class ItemEntry
{
    public CharacterItemType ItemType { get; set; }
    public sbyte u131 { get; set; }
    public sbyte u132 { get; set; }
    public sbyte u133 { get; set; }
    public int u134_137 { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float w { get; set; }
    public StringWithId Auth3dEntry { get; set; }
    public string Node { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        ItemType = (CharacterItemType)reader.ReadSByte();
        u131 = reader.ReadSByte();
        u132 = reader.ReadSByte();
        u133 = reader.ReadSByte();
        u134_137 = reader.ReadInt32();
        Int64 auth3dsOffset = reader.ReadOffset();
        Int64 nodeOffset = reader.ReadOffset();
        x = reader.ReadSingle();
        y = reader.ReadSingle();
        z = reader.ReadSingle();
        w = reader.ReadSingle();

        reader.ReadAtOffset(auth3dsOffset, () =>
        {
            var a3dentry = new StringWithId();
            a3dentry.Read(reader);
            Auth3dEntry = a3dentry;
        });

        reader.ReadAtOffset(nodeOffset, () =>
        {
            Node = reader.ReadString();
        });
    }

    public ItemEntry()
    {
        ItemType = CharacterItemType.PVPP_CHARA_ITEM_NONE;
        u131 = 0;
        u132 = 0;
        u133 = 0;
        u134_137 = 0;
        x = 0f;
        y = 0f;
        z = 0f;
        w = 0f;
        Auth3dEntry = new StringWithId();
        Node = "";
    }
}

public class CharaEntry
{
    public List<CharaEffEntry> CharaEffEntries { get; set; }
    public List<MotionEntry> MotionEntries { get; set; }
    public List<StringWithId> Auth3dEntries { get; set; }
    public List<ItemEntry> ItemEntries { get; set; }
    public List<GlitterEntry> GlitterEntries { get; set; }

    public void Read(EndianBinaryReader reader)
    {
        sbyte u00 = reader.ReadSByte(); // always 7 for some reason
        sbyte u01 = reader.ReadSByte(); // seems related to effchr
        sbyte motionCount = reader.ReadSByte();
        sbyte auth3dCount = reader.ReadSByte();
        sbyte itemCount = reader.ReadSByte();
        sbyte u05 = reader.ReadSByte();
        sbyte glitterCount = reader.ReadSByte();
        sbyte u07 = reader.ReadSByte();

        Int64 chrEffOffset = reader.ReadOffset();
        Int64 motionOffset = reader.ReadOffset();
        Int64 auth3dOffset = reader.ReadOffset();
        Int64 itemOffset = reader.ReadOffset();
        Int64 glitterOffset = reader.ReadOffset();
        Int64 o30 = reader.ReadOffset();

        // chreff
        reader.ReadAtOffset(chrEffOffset, () =>
        {
            var charaEffEntry = new CharaEffEntry();
            charaEffEntry.Read(reader);
            CharaEffEntries.Add(charaEffEntry);
        });

        // motion
        reader.ReadAtOffset(motionOffset, () =>
        {
            for (int i = 0; i < motionCount; i++)
            {
                var motionEntry = new MotionEntry();
                motionEntry.Read(reader);
                MotionEntries.Add(motionEntry);
            }
        });

        // a3d
        reader.ReadAtOffset(auth3dOffset, () =>
        {
            for (int i = 0; i < auth3dCount; i++)
            {
                var auth3dEntry = new StringWithId();
                auth3dEntry.Read(reader);
                Auth3dEntries.Add(auth3dEntry);
            }
        });

        // item
        reader.ReadAtOffset(itemOffset, () =>
        {
            for (int i = 0; i < itemCount; i++)
            {
                var itemEntry = new ItemEntry();
                itemEntry.Read(reader);
                ItemEntries.Add(itemEntry);
            }
        });

        // glitter
        reader.ReadAtOffset(glitterOffset, () =>
        {
            for (int i = 0; i < glitterCount; i++)
            {
                var glitterEntry = new GlitterEntry();
                glitterEntry.Read(reader);
                GlitterEntries.Add(glitterEntry);
            }
        });
    }

    public CharaEntry()
    {
        CharaEffEntries = new List<CharaEffEntry>();
        MotionEntries = new List<MotionEntry>();
        Auth3dEntries = new List<StringWithId>();
        ItemEntries = new List<ItemEntry>();
        GlitterEntries = new List<GlitterEntry>();
    }
}

public class PVPerformerParameter : BinaryFile
{
    public override BinaryFileFlags Flags => BinaryFileFlags.Load | BinaryFileFlags.HasSectionFormat;
    
    public List<CharaEntry> CharaEntries { get; set; }
    public List<SongEffect> SongEffects { get; set; }
    public uint u20 {  get; set; }
    public sbyte u25 { get; set; }
    public sbyte u26 { get; set; }
    public sbyte u27 { get; set; }
    public sbyte u40 { get; set; }
    public sbyte u42 { get; set; }
    public sbyte u43 { get; set; }
    public int u44_47 { get; set; }

    public override void Read(EndianBinaryReader reader, ISection section = null)
    {
        u20 = reader.ReadUInt32();
        sbyte charaEntryCount = reader.ReadSByte();
        CharaEntries.Capacity = charaEntryCount;
        u25 = reader.ReadSByte();
        u26 = reader.ReadSByte();
        u27 = reader.ReadSByte();
        Int64 songEffectSectionOffset = reader.ReadOffset();
        Int64 charaEntriesOffset = reader.ReadOffset();
        Int64 unkOffset = reader.ReadOffset();

        reader.ReadAtOffset(songEffectSectionOffset, () =>
        {
            u40 = reader.ReadSByte();
            sbyte songEffectCount = reader.ReadSByte();
            SongEffects.Capacity = songEffectCount;
            u42 = reader.ReadSByte();
            u43 = reader.ReadSByte();
            u44_47 = reader.ReadInt32();
            Int64 songEffectOffset = reader.ReadOffset();
            reader.ReadAtOffset(songEffectOffset, () =>
            {
                for (int i = 0; i < songEffectCount; i++)
                {
                    var songEffect = new SongEffect();
                    songEffect.Read(reader);
                    SongEffects.Add(songEffect);
                }
            });
        });

        reader.ReadAtOffset(charaEntriesOffset, () =>
        {
            for (int i = 0; i < charaEntryCount; i++)
            {
                var charaEntry = new CharaEntry();
                charaEntry.Read(reader);
                CharaEntries.Add(charaEntry);
            }
        });
    }

    public override void Write(EndianBinaryWriter writer, ISection section = null)
    {
        throw new NotImplementedException();
    }

    public PVPerformerParameter()
    {
        CharaEntries = new List<CharaEntry>();
        SongEffects = new List<SongEffect>();
    }
}
