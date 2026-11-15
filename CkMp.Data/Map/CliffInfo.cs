namespace CkMp.Data.Map
{
    /// <summary>
    /// Custom texture mapping of a cliff tile, written by WorldBuilder when cliff textures are adjusted.
    /// Tiles reference these through <see cref="Tile.BlendTexture3"/> (1-based, 0 = none).
    /// </summary>
    public class CliffInfo
    {
        public const int Size = 4 + 8 * 4 + 1 + 1;

        public int TileIndex { get; set; }

        public float U0 { get; set; }
        public float V0 { get; set; }
        public float U1 { get; set; }
        public float V1 { get; set; }
        public float U2 { get; set; }
        public float V2 { get; set; }
        public float U3 { get; set; }
        public float V3 { get; set; }

        public byte Flip { get; set; }
        public byte Mutant { get; set; }
    }
}
