using UnityEngine;
using System.Collections;

public class CopyZone : MonoBehaviour
{
    public enum ZoneType { Alis, Binis }

    [Header("Bölge Tipi")]
    public ZoneType alanTipi;

    [Header("Referanslar")]
    public AudioSource copySound;
    public GameObject  paperPrefab;
    public Transform   playerHandPoint;
    public Transform   lookTarget;
    public string      kagitTagi = "Paper";

    [Header("Görev Entegrasyonu")]
    [Tooltip("Bu işlem tamamlanınca hangi görev bitsin? (QuestManager listesindeki isimle birebir aynı olmalı)")]
    public string questToComplete = "";

    private bool isWorking = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isWorking) return;

        if (alanTipi == ZoneType.Alis)
            StartCoroutine(CopyProcess(other.gameObject));
        else if (alanTipi == ZoneType.Binis)
            StartCoroutine(DropProcess(other.gameObject));
    }

    // ── FOTOKOPI ÇEKME ────────────────────────────────────────
    IEnumerator CopyProcess(GameObject player)
    {
        isWorking = true;

        if (lookTarget != null) player.transform.LookAt(lookTarget);

        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isCopying", true);

        if (copySound != null) copySound.Play();

        yield return new WaitForSeconds(2f);

        // Kağıdı el noktasına yerleştir
        if (paperPrefab != null && playerHandPoint != null)
        {
            GameObject paper = Instantiate(paperPrefab, playerHandPoint.position, playerHandPoint.rotation);
            paper.transform.SetParent(playerHandPoint);
            paper.tag = kagitTagi;
        }

        if (anim != null) anim.SetBool("isCopying", false);

        // Görevi tamamla
        CompleteQuest();

        isWorking = false;
    }

    // ── FOTOKOPİ TESLİM ETME ──────────────────────────────────
    IEnumerator DropProcess(GameObject player)
    {
        // Elde kağıt var mı?
        if (playerHandPoint == null) yield break;

        GameObject heldPaper = null;
        for (int i = 0; i < playerHandPoint.childCount; i++)
        {
            if (playerHandPoint.GetChild(i).CompareTag(kagitTagi))
            {
                heldPaper = playerHandPoint.GetChild(i).gameObject;
                break;
            }
        }

        if (heldPaper == null) yield break; // Elde kağıt yoksa işlem yapma

        isWorking = true;

        if (lookTarget != null) player.transform.LookAt(lookTarget);

        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isCopying", true);

        if (copySound != null) copySound.Play();

        yield return new WaitForSeconds(2f);

        Destroy(heldPaper);

        if (anim != null) anim.SetBool("isCopying", false);

        // Görevi tamamla
        CompleteQuest();

        isWorking = false;
    }

    void CompleteQuest()
    {
        if (!string.IsNullOrEmpty(questToComplete) && QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(questToComplete);
    }
}
