using SoulsFormats;

namespace StudioCore.Core;

public enum ProjectType
{
    Undefined = 0,
    DES = 1, // Demon's Souls
    DS1 = 2, // Dark Souls: Prepare to Die
    DS1R = 3, // Dark Souls: Remastered
    DS2S = 4, // Dark Souls II: Scholar of the First Sin
    DS3 = 5, // Dark Souls III
    BB = 6, // Bloodborne
    SDT = 7, // Sekiro: Shadows Die Twice
    ER = 8, // Elden Ring
    AC6 = 9, // Armored Core VI: Fires of Rubicon
    DS2 = 10 // Dark Souls II
    
}

public static class ProjectTypeMethods
{
    public static BHD5.Game? AsBhdGame(this ProjectType p)
    {
        return p switch
        {
            ProjectType.DS1 => BHD5.Game.DarkSouls1,
            ProjectType.DS1R => BHD5.Game.DarkSouls1,
            ProjectType.DS2 => BHD5.Game.DarkSouls2,
            ProjectType.DS2S => BHD5.Game.DarkSouls2,
            ProjectType.DS3 => BHD5.Game.DarkSouls3,
            ProjectType.SDT => BHD5.Game.DarkSouls3,
            ProjectType.ER => BHD5.Game.EldenRing,
            _ => null
        };
    }
}