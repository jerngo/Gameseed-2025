using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterClick : MonoBehaviour, IPointerClickHandler
{
    public CharacterSelector selector;

    [Tooltip("GameObject lain yang menyimpan gambar karakter ini (misalnya prefab, image dummy, dll)")]
    public Image characterImageSource;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selector != null && characterImageSource != null)
        {
            selector.SelectCharacter(gameObject, characterImageSource.sprite);
        }
    }
}
