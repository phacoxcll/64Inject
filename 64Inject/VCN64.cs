
namespace _64Inject
{
    public class VCN64
    {
        public readonly uint HashCRC32;
        public readonly string Title;
        public readonly string Info;

        public VCN64(uint hashCRC32, string info, string title)
        {
            HashCRC32 = hashCRC32;
            Title = title;
            Info = info;
        }

        public override string ToString()
        {
            return Title + "\nCRC32: " + HashCRC32.ToString("X8") + " " + Info;
        }

        public static readonly VCN64 DonkeyKong64U  = new VCN64(0xFB245F10, "SVN: 1680 TIME: 2015/01/20 14:12:06", "Super Mario 64 (USA/EUR/JPN)/Donkey Kong 64 (USA/EUR)");
        public static readonly VCN64 DonkeyKong64J  = new VCN64(0x8EF60284, "SVN: 1690 TIME: 2015/01/27 16:08:00", "Donkey Kong 64 (JPN)");
        public static readonly VCN64 Ocarina        = new VCN64(0xF042E451, "SVN: 1696 TIME: 2015/01/30 10:49:22", "The Legend of Zelda: Ocarina of Time (USA/EUR/JPN)");
        public static readonly VCN64 PaperMario     = new VCN64(0xAE933905, "SVN: 1743 TIME: 2015/03/05 15:06:57", "Paper Mario (USA/EUR/JPN)");
        public static readonly VCN64 Kirby64J       = new VCN64(0xCEB7A833, "SVN: 1778 TIME: 2015/03/19 16:15:32", "Kirby 64: The Crystal Shards (JPN)");
        public static readonly VCN64 Kirby64U       = new VCN64(0x7EB7B97D, "SVN: 1790 TIME: 2015/03/24 13:46:36", "Kirby 64: The Crystal Shards (USA/EUR)");
        public static readonly VCN64 MarioTennisJ   = new VCN64(0x17BCC968, "SVN: 1897 TIME: 2015/05/12 17:32:21", "Mario Tennis (JPN)/1080º Snowboarding (JPN)");
        public static readonly VCN64 MarioTennisU   = new VCN64(0x05F20995, "SVN: 1918 TIME: 2015/05/20 14:34:00", "Mario Tennis (USA/EUR)/1080º Snowboarding (USA/EUR)");
        public static readonly VCN64 MarioGolfJ     = new VCN64(0x8D3C196C, "SVN: 1946 TIME: 2015/06/09 11:00:28", "Mario Golf (JPN)");
        public static readonly VCN64 MarioGolfU     = new VCN64(0x307DCE21, "SVN: 1955 TIME: 2015/06/16 16:09:03", "Mario Golf (USA/EUR)");
        public static readonly VCN64 StarFox64      = new VCN64(0xF41BC127, "SVN: 1970 TIME: 2015/06/30 14:07:35", "Star Fox 64 (USA/EUR/JPN)");
        public static readonly VCN64 SinAndP        = new VCN64(0x36C0456E, "SVN: 1991 TIME: 2015/07/16 09:20:39", "Sin and Punishment (USA/EUR/JPN)");
        public static readonly VCN64 MarioKart64    = new VCN64(0x5559F831, "SVN: 2043 TIME: 2015/08/18 10:07:52", "Mario Kart 64 (USA/EUR/JPN)");
        public static readonly VCN64 YoshiStory     = new VCN64(0xD554D2E4, "SVN: 2079 TIME: 2015/09/15 16:19:11", "Yoshi's Story (USA/EUR/JPN)");
        public static readonly VCN64 WaveRace64J    = new VCN64(0x04F7D67F, "SVN: 2109 TIME: 2015/10/22 10:15:03", "Wave Race 64 (JPN)");
        public static readonly VCN64 WaveRace64U    = new VCN64(0xC376B949, "SVN: 2136 TIME: 2015/11/18 12:41:26", "Wave Race 64 (USA/EUR)");
        public static readonly VCN64 MajoraJ        = new VCN64(0xEE8855FF, "SVN: 2170 TIME: 2015/12/16 16:01:23", "The Legend of Zelda: Majora's Mask (JPN)");
        public static readonly VCN64 MajoraU        = new VCN64(0x71FC1731, "SVN: 2190 TIME: 2016/01/05 16:50:14", "The Legend of Zelda: Majora's Mask (USA/EUR)");
        public static readonly VCN64 PokemonSnap    = new VCN64(0x967E7DF0, "SVN: 2195 TIME: 2016/01/08 09:42:51", "Pokémon Snap (USA/EUR/JPN)");
        public static readonly VCN64 MarioParty2    = new VCN64(0xBE3CEC5F, "SVN: 2234 TIME: 2016/02/02 10:56:10", "Mario Party 2 (USA/EUR/JPN)");
        public static readonly VCN64 CustomRoboV2   = new VCN64(0x89F2BC09, "SVN: 2244 TIME: 2016/02/26 09:03:55", "Custom Robo V2 (JPN)");
        public static readonly VCN64 OgreBattle64   = new VCN64(0xFED1FB48, "SVN: 2395 TIME: 2016/08/30 13:57:02", "Ogre Battle 64: Person of Lordly Caliber (USA/EUR/JPN)");
        public static readonly VCN64 Excitebike64   = new VCN64(0x724C4F5D, "SVN: 2404 TIME: 2016/09/14 13:42:00", "Excitebike 64 (USA/EUR/JPN)");
        public static readonly VCN64 FZeroX         = new VCN64(0x2AF3C23B, "SVN: 2428 TIME: 2016/11/18 11:44:39", "F-Zero X (USA/EUR/JPN)/Bomberman 64 (USA/EUR/JPN)/Harvest Moon 64 (USA/EUR/JPN)");
    }
}
