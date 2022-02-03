using System;

namespace ObjectStorage.Model
{
    public class YoloObject
    {
        public int Id { get; set; }

        public string ClassName { get; set; }

        public byte[] Image { get; set; }

        public string Hash { get; set; }

        public float X1 { get; set; }

        public float Y1 { get; set; }

        public float X2 { get; set; }

        public float Y2 { get; set; }

        public float Confidence { get; set; }

        public int FotoId { get; set; }

    }
}
