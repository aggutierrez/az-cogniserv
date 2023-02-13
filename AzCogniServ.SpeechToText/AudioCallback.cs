using Microsoft.CognitiveServices.Speech.Audio;

namespace AzCogniServ.SpeechToText;

public sealed class AudioCallback : PullAudioInputStreamCallback
{
    private readonly FileStream stream;

    public AudioCallback(string fileName)
    {
        stream = File.OpenRead(fileName);
    }
    
    public override int Read(byte[] dataBuffer, uint size)
    {
        return stream.Read(dataBuffer, 0, (int)size);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        stream.Dispose();
    }
}