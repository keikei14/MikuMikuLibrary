using MikuMikuLibrary.Databases;
using MikuMikuLibrary.Databases.PVPP;
using MikuMikuLibrary.IO.Sections.IO;

namespace MikuMikuLibrary.IO.Sections.Databases;

[Section("PVPP")]
public class PVPerformerParameterSection : BinaryFileSection<PVPerformerParameter>
{
    public override SectionFlags Flags => SectionFlags.None;

    public PVPerformerParameterSection(SectionMode mode, PVPerformerParameter data = null) : base(mode, data)
    {
    }
}