// ...existing code...
using System.Collections.Generic;
using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
    public GameObject roadSection;
    public float sectionLength = 0f; // 0 = calcular automáticamente
    public Transform spawnPoint;     // opcional: punto inicial en escena

    [Tooltip("Este valor ya no se aplica automáticamente. Mantener en (0,0,0) si no se usa.")]
    public Vector3 prefabRotationOffset = Vector3.zero;

    [Tooltip("Cuántas secciones generar cada vez que se active el trigger.")]
    public int spawnBatch = 2;

    [Tooltip("Transform del jugador (opcional). Si está vacío se buscará la tag 'Player'.")]
    public Transform player;

    [Tooltip("Tiempo (s) que esperar antes de destruir una sección pasada.")]
    public float destructionDelay = 2f;

    private GameObject lastSpawned;
    private Transform container;
    private List<GameObject> spawnedSections = new List<GameObject>();
    private float halfLength;

    private void Start()
    {
        // contenedor para mantener la jerarquía limpia
        GameObject c = new GameObject("RoadSections");
        container = c.transform;

        // intentar calcular sectionLength automáticamente desde el prefab
        if (sectionLength <= 0f && roadSection != null)
        {
            var rend = roadSection.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                sectionLength = rend.bounds.size.z;
            }
            else
            {
                var col = roadSection.GetComponentInChildren<Collider>();
                if (col != null) sectionLength = col.bounds.size.z;
            }

            if (sectionLength <= 0f) sectionLength = 30f; // fallback
        }

        halfLength = sectionLength * 0.5f;

        // Si hay una instancia del prefab ya en la escena (misma name que el prefab), úsala como primera sección
        if (roadSection != null)
        {
            GameObject existing = GameObject.Find(roadSection.name);
            if (existing != null)
            {
                existing.transform.SetParent(container);
                lastSpawned = existing; // no cambiamos su rotación
                spawnedSections.Add(existing);
            }
        }

        if (spawnBatch < 1) spawnBatch = 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Trigger")) return;
        if (roadSection == null) return;

        // asegurarse de tener referencia al jugador
        if (player == null)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        // crear spawnBatch secciones, una después de la otra (cada una usa forward de la anterior)
        for (int i = 0; i < spawnBatch; i++)
        {
            Vector3 spawnPos;
            if (lastSpawned != null)
            {
                spawnPos = lastSpawned.transform.position + lastSpawned.transform.forward * sectionLength;
            }
            else if (spawnPoint != null)
            {
                spawnPos = spawnPoint.position;
            }
            else
            {
                spawnPos = transform.position + transform.forward * sectionLength;
            }

            // mantener la rotación de la última sección (si existe), o la del prefab/spawnPoint si no
            Quaternion spawnRot = lastSpawned != null ? lastSpawned.transform.rotation :
                                  (spawnPoint != null ? spawnPoint.rotation : roadSection.transform.rotation);

            GameObject go = Instantiate(roadSection, spawnPos, spawnRot, container);
            lastSpawned = go;
            spawnedSections.Add(go);
        }

        // limpiar secciones que el jugador ya pasó (excepto la sección en la que está atravesando)
        CleanupPassedSections();
    }

    private void CleanupPassedSections()
    {
        if (player == null) return;
        if (spawnedSections.Count == 0) return;

        // revisar siempre la más antigua (índice 0). si la pasó, destruir con pequeño delay y continuar.
        while (spawnedSections.Count > 0)
        {
            GameObject first = spawnedSections[0];
            if (first == null)
            {
                spawnedSections.RemoveAt(0);
                continue;
            }

            Vector3 toPlayer = player.position - first.transform.position;
            float localZ = Vector3.Dot(toPlayer, first.transform.forward);

            // si el jugador está "dentro" de la sección (entre -halfLength y +halfLength), no la borres
            bool playerInside = localZ > -halfLength && localZ < halfLength;

            // si el jugador ya la pasó completamente (localZ > halfLength) se puede programar su borrado
            if (!playerInside && localZ > halfLength)
            {
                spawnedSections.RemoveAt(0);
                Destroy(first, Mathf.Max(0f, destructionDelay));
                continue;
            }

            // si no se cumple la condición de borrado, salir (las siguientes son más nuevas)
            break;
        }
    }
}
// ...existing code...