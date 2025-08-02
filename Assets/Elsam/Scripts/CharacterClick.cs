using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterClick : MonoBehaviour, IPointerClickHandler
{
    public CharacterSelector selector;

    [Tooltip("Gambar karakter untuk UI preview")]
    public Image characterImageSource;

    [Tooltip("Prefab karakter 3D untuk preview")]
    public GameObject character3DPrefab;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selector != null && characterImageSource != null && character3DPrefab != null)
        {
            selector.SelectCharacter(gameObject, characterImageSource.sprite, character3DPrefab);
        }
    }
}
