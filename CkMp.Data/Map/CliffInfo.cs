namespace CkMp.Data.Map
{
    /// <summary>
    /// Texture coordinates for a cliff tile. Referenced by the per tile cliff index (BlendTexture3).
    /// </summary>
    public class CliffInfo
    {
        public const int Size = 4 + 8 * 4 + 2;

        public int TileIndex { get; set; }
        public float U0 { get; set; }
        public float V0 { get; set; }
        public float U1 { get; set; }
        public float V1 { get; set; }
        public float U2 { get; set; }
        public float V2 { get; set; }
        public float U3 { get; set; }
        public float V3 { get; set; }
        public bool Flip { get; set; }
        public bool Mutant { get; set; }
    }
}
