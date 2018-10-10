namespace Archiver.Models
{
    public class Chunk
    {
        public Chunk()
        {
        }

        public Chunk(int id, byte[] bytes)
        {
            Id = id;
            Bytes = bytes;
        }

        public int Id { get; set; }

        public byte[] Bytes { get; set; }
    }
}
