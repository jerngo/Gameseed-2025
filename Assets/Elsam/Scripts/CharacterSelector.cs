using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI Preview")]
    public Image slot1Preview;
    public Image slot2Preview;

    [Header("3D Preview")]
    public Transform preview3DSlot1;
    public Transform preview3DSlot2;

    private GameObject[] selectedCharacters = new GameObject[2];

    private GameObject[] selectedPrefabCharacters = new GameObject[2];

    private Sprite[] selectedSprites = new Sprite[2];
    private GameObject[] character3DInstances = new GameObject[2];

    private int selectedSlot = 0;

    public GameSettings gameSettings;
    public GameObject[] PrefabChar;

    public void SelectCharacter(GameObject character, Sprite characterSprite, GameObject character3DPrefab, int characterId)
    {
        // Cek duplikasi
        if (selectedCharacters[0] == character)
        {
            if (BothSlotsFilled()) { SwapSlots(); UpdateAllPreviews(); }
            return;
        }
        if (selectedCharacters[1] == character)
        {
            if (BothSlotsFilled()) { SwapSlots(); UpdateAllPreviews(); }
            return;
        }

        // Simpan karakter
        selectedCharacters[selectedSlot] = character;
        selectedSprites[selectedSlot] = characterSprite;
        selectedPrefabCharacters[selectedSlot] = PrefabChar[characterId];

        // Hapus preview lama jika ada
        if (character3DInstances[selectedSlot] != null)
        {
            Destroy(character3DInstances[selectedSlot]);
        }

        // Instantiate prefab karakter ke preview slot yang sesuai
        Transform targetParent = selectedSlot == 0 ? preview3DSlot1 : preview3DSlot2;
        character3DInstances[selectedSlot] = Instantiate(character3DPrefab, targetParent);
        character3DInstances[selectedSlot].transform.localPosition = Vector3.zero;
        character3DInstances[selectedSlot].transform.localRotation = Quaternion.identity;

        // Update UI preview
        UpdatePreviewImage(selectedSlot, characterSprite);

        // Pindah ke slot berikut
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
        (character3DInstances[0], character3DInstances[1]) = (character3DInstances[1], character3DInstances[0]);

        (selectedPrefabCharacters[0], selectedPrefabCharacters[1]) = (selectedPrefabCharacters[1], selectedPrefabCharacters[0]);
        // Pindahkan posisi 3D instance juga
        character3DInstances[0].transform.SetParent(preview3DSlot1, false);
        character3DInstances[1].transform.SetParent(preview3DSlot2, false);
    }

    private void UpdateAllPreviews()
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

    public void ConfirmSelection() {
        gameSettings.PlayerCharacter1 = selectedPrefabCharacters[0];
        gameSettings.PlayerCharacter2 = selectedPrefabCharacters[1];

        List<GameObject> remainingEnemies = new List<GameObject>();

        foreach (GameObject prefab in PrefabChar)
        {
            if (prefab != selectedPrefabCharacters[0] && prefab != selectedPrefabCharacters[1])
            {
                remainingEnemies.Add(prefab);
            }
        }

        if (remainingEnemies.Count < 2)
        {
            Debug.LogError("Tidak cukup karakter untuk musuh. Pastikan Player tidak memilih karakter yang sama.");
            return;
        }

        gameSettings.EnemyCharacter1 = remainingEnemies[0];
        gameSettings.EnemyCharacter2 = remainingEnemies[1];

        StartCoroutine(GoToNextSceneIndex(2));
    }

    IEnumerator GoToNextSceneIndex(int index) {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
