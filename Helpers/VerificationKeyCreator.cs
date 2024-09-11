namespace AYS.Helpers
{
    public static class VerificationKeyCreator
    {
        public static string CreateVerificationKey()
        {
            byte[] randomBytes = new byte[8];
            Random rng = new Random();
            rng.NextBytes(randomBytes);

            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}