using System;

[Serializable]
public class SaveEnvelope
{
    public int Version = 1;
    public string EncodedPayload;
    public string Checksum;
}
