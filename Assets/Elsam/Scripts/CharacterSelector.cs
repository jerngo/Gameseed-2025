using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("Slot Preview")]
    public Image slot1Preview;
    public Image slot2Preview;

    private GameObject[] selectedCharacters = new GameObject[2];
    private Sprite[] selectedSprites = new Sprite[2];

    private int selectedSlot = 0;

    public void SelectCharacter(GameObject character, Sprite characterSprite)
    {
        // Jika karakter sudah dipilih di slot 1
        if (selectedCharacters[0] == character)
        {
            if (BothSlotsFilled())
            {
                SwapSlots();
                UpdatePreviewImages();
                Debug.Log("Tukar posisi!");
            }
            else
            {
                Debug.Log("Sudah dipilih di Slot 1.");
            }
            return;
        }

        // Jika karakter sudah dipilih di slot 2
        if (selectedCharacters[1] == character)
        {
            if (BothSlotsFilled())
            {
                SwapSlots();
                UpdatePreviewImages();
                Debug.Log("Tukar posisi!");
            }
            else
            {
                Debug.Log("Sudah dipilih di Slot 2.");
            }
            return;
        }

        // Pilih karakter baru
        selectedCharacters[selectedSlot] = character;
        selectedSprites[selectedSlot] = characterSprite;

        Debug.Log("Slot " + (selectedSlot + 1) + " memilih: " + character.name);

        UpdatePreviewImage(selectedSlot, characterSprite);
        selectedSlot = (selectedSlot + 1) % 2;
    }

    private bool BothSlotsFilled()
    {
        return selectedCharacters[0] != null && selectedCharacters[1] != null;
    }

    private void SwapSlots()
    {
        (selectedCharacters[0], selectedCharacters[1]) = (selectedCharacters[1], selectedCharacters[0]);
        (selectedSprites[0], selectedSprites[1]) = (selectedSprites[1], selectedSprites[0]);
    }

    private void UpdatePreviewImages()
    {
        UpdatePreviewImage(0, selectedSprites[0]);
        UpdatePreviewImage(1, selectedSprites[1]);
    }

    private void UpdatePreviewImage(int slot, Sprite sprite)
    {
        if (slot == 0 && slot1Preview != null)
            slot1Preview.sprite = sprite;
        else if (slot == 1 && slot2Preview != null)
            slot2Preview.sprite = sprite;
    }

    public GameObject GetSelectedCharacter(int slot)
    {
        return selectedCharacters[slot];
    }
}
