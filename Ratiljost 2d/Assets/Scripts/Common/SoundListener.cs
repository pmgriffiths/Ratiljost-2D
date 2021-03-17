using UnityEngine;

/// <summary>
/// Class for objects that want to be notified when the sound clip
/// starts and finishes
/// </summary>
public abstract class SoundListener : MonoBehaviour
{
    /// <summary>
    /// Notifies the class that a clip has started playing
    /// </summary>
    /// <param name="clipLength">length of clip in seconds</param>
    public abstract void NotifySoundStart(float clipLength);

    public abstract void NotifySoundEnd();
}

