using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSoundProfile", menuName = "AimTrainer/WeaponSoundProfile")]
public class WeaponSoundProfile : ScriptableObject
{
    [Tooltip("Le fichier .wav à jouer. Si vide, un son procédural est généré en fallback.")]
    public AudioClip clip;

    [Tooltip("Son procédural utilisé si aucun clip n'est assigné.")]
    public SoundGenerator.ProceduralSound proceduralFallback = SoundGenerator.ProceduralSound.Pistol;

    public AudioClip GetClip()
    {
        return clip != null ? clip : SoundGenerator.GenerateShootSound(proceduralFallback);
    }
}
