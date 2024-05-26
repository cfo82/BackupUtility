namespace Data.IntegTest;

using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// Verifies that the empty hash is always the same.
/// </summary>
[TestClass]
public class EmptyHashTest
{
    private const string _emptyFileHash = "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e";

    /// <summary>
    /// vERIFIES THAT THE HASH OF AN EMPTY FILE IS AS EXPECTED.
    /// </summary>
    [TestMethod]
    public void VerifyEmptyHash()
    {
        using var stream = new MemoryStream(new byte[0]);
        using var sha = SHA512.Create();
        byte[] checksum = sha.ComputeHash(stream);
        var fullHash = BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();

        Assert.AreEqual(_emptyFileHash, fullHash);
    }
}
