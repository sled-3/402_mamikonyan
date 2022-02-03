using System;

namespace ObjectStorage.Model
{
    public class Foto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public byte[] Image { get; set; }

        public string Hash { get; set; }

    }
}
