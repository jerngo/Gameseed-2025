using UnityEngine;

public class BallShadowProjector : MonoBehaviour
{
    public GameObject markerPrefab; // Prefab X Image (pastikan ini sudah di Instantiate sebelumnya)
    private GameObject markerInstance;

    public LayerMask groundLayer;

    public float minScale = 0.5f;
    public float maxScale = 2f;

    public float maxHeight = 10f; // ketinggian di mana skala mencapai min

    private void Start()
    {
        // Spawn marker di awal
        markerInstance = Instantiate(markerPrefab);
    }

    public float offset = 0.1f;
    private void Update()
    {
        RaycastHit hit;

        // Raycast ke bawah dari posisi bola
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, groundLayer))
        {
            // Tempatkan marker di tanah
            markerInstance.transform.position = hit.point + Vector3.up * offset; // sedikit naik agar tidak tembus

            // Hitung tinggi dari bola ke tanah
            float height = Vector3.Distance(transform.position, hit.point);

            // Interpolasi ukuran antara maxScale dan minScale
            float t = Mathf.Clamp01(height / maxHeight);
            float scale = Mathf.Lerp(maxScale, minScale, t); // makin tinggi, makin kecil

            markerInstance.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            //markerInstance.SetActive(false); // kalau tidak kena tanah, sembunyikan
        }

        // Panjang ray disesuaikan, misalnya 100 unit ke bawah
        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down;
        float rayLength = 100f;

        // Gambar debug ray (hanya terlihat di Scene view)
        Debug.DrawRay(origin, direction * rayLength, Color.red);

        if (Physics.Raycast(origin, direction, out hit, rayLength, groundLayer))
        {
            // Log posisi hit dan objek yang dikenai
            Debug.Log("Ray hit: " + hit.collider.name + " at " + hit.point);

            // ... (lanjutan logika marker seperti di jawaban sebelumnya)
        }
    }
}

