using UnityEngine;
using System.Collections;

public class NPCManager : MonoBehaviour
{
    public NPCMovement3D[] npcs; // Misal: NPC0 dan NPC1
    public Transform ball;

    private int currentIndex = 0;
    private int hitCount = 0;
    private bool smashMode = false;
    private NPCMovement3D lastHitter = null;

    void Start()
    {
        ActivateNPC(currentIndex);
    }

    public void OnNPCHit(NPCMovement3D npc)
    {
        if (npc != npcs[currentIndex]) return; // Hanya NPC aktif yang boleh memukul

        if (npc == lastHitter)
        {
            Debug.LogWarning("❌ NPC yang sama mencoba memukul dua kali berturut-turut!");
            return;
        }

        if (smashMode)
        {
            Debug.Log("💥 Smash selesai oleh " + npc.name);
            npc.isActive = false;
            smashMode = false;

            StartCoroutine(StartPassingAfterSmash());
            return;
        }

        lastHitter = npc;
        hitCount++;
        npc.isActive = false;

        Debug.Log("✅ Passing ke-" + hitCount + " oleh " + npc.name);

        if (hitCount >= 2)
        {
            hitCount = 0;
            lastHitter = null;
            currentIndex = 0;
            smashMode = true;

            Debug.Log("💥 Smash dimulai oleh " + npcs[currentIndex].name);

            npcs[currentIndex].SetTarget(ball);
            npcs[currentIndex].isActive = true;
            npcs[currentIndex].PrepareSmash();
        }
        else
        {
            currentIndex = (currentIndex + 1) % npcs.Length;
            ActivateNPC(currentIndex);
        }
    }

    private IEnumerator StartPassingAfterSmash()
    {
        yield return new WaitForSeconds(1f);

        hitCount = 0; // Hit pertama setelah smash
        lastHitter = npcs[1]; // Karena NPC 0 baru saja melakukan smash

        currentIndex = 0; // Passing dimulai oleh NPC berikutnya
        ActivateNPC(currentIndex);

        Debug.Log("🔁 Passing dimulai lagi oleh " + npcs[currentIndex].name);
    }

    void ActivateNPC(int index)
    {
        for (int i = 0; i < npcs.Length; i++)
        {
            npcs[i].isActive = false;
        }

        npcs[index].SetTarget(ball);
        npcs[index].isActive = true;
        Debug.Log("🎯 NPC aktif: " + npcs[index].name);
    }

    public GameObject GetOtherNPC(GameObject self)
    {
        foreach (var npc in npcs)
        {
            if (npc.gameObject != self)
            {
                return npc.gameObject;
            }
        }
        return null;
    }
}
