namespace WebApi.Security
{
    public sealed class SymmetricParameters
    {
        public byte[] SecretKey { get; set; }
        public byte[] Iv { get; set; }
    }
}