using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Ayarlar")]
    public string promptMessage = "Etkileşime Geç";

    [Header("Ne Olsun?")]
    public UnityEvent onInteract = new UnityEvent();

    // Alt sınıflar bu metodu override ederek dinamik metin döndürebilir
    public virtual string GetPromptMessage() => promptMessage;

    // Alt sınıflar false döndürerek etkileşimi geçici olarak devre dışı bırakabilir
    // (örn. TrashItem: eller doluyken gösterme)
    public virtual bool IsAvailable() => true;

    // Alt sınıflar override edebilir (örn. ek mantık eklemek için)
    public virtual void BaseInteract()
    {
        onInteract.Invoke();
    }
}