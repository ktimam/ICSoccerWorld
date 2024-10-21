using EdjCase.ICP.BLS;

public class BypassedBlsCryptography : IBlsCryptography
{
    /// <inheritdoc/>
    public bool VerifySignature(byte[] publicKey, byte[] messageHash, byte[] signature)
    {
        return true;
    }
}