using UnityEngine;
using UnityEditor;
using System.IO;

public class HeightmapGenerator : EditorWindow
{
    private int resolution = 513; // 513 on Unityn Terrainille optimaalinen oletusresoluutio
    private string fileName = "GeneratedHeightmap";

    [MenuItem("Tools/Heightmap Generator")]
    public static void ShowWindow()
    {
        GetWindow<HeightmapGenerator>("Heightmap Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generoi RAW Heightmap valitusta objektista", EditorStyles.boldLabel);

        resolution = EditorGUILayout.IntField("Resoluutio (px)", resolution);
        fileName = EditorGUILayout.TextField("Tiedoston nimi", fileName);

        if (GUILayout.Button("Generoi ja tallenna RAW-muodossa"))
        {
            GenerateHeightmapRAW();
        }
    }

    private void GenerateHeightmapRAW()
    {
        // Varmistetaan, ett‰ jokin objekti on valittuna sceness‰
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Virhe", "Valitse ensin objekti Sceness‰!", "OK");
            return;
        }

        // Haetaan objektin MeshCollider raycastia varten
        MeshCollider meshCollider = selectedObject.GetComponent<MeshCollider>();
        bool addedCollider = false;

        if (meshCollider == null)
        {
            meshCollider = selectedObject.AddComponent<MeshCollider>();
            addedCollider = true;
        }

        // Lasketaan objektin rajat (Bounds) maailmankoordinaateissa
        Bounds bounds = meshCollider.bounds;

        // Luodaan v‰liaikainen taulukko korkeusarvoille
        float[,] heightValues = new float[resolution, resolution];

        // K‰yd‰‰n l‰pi jokainen piste resoluution mukaan
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                // Lasketaan maailman koordinaatit objektin alueella
                float normalizedX = (float)x / (resolution - 1);
                float normalizedY = (float)y / (resolution - 1);

                float worldX = Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedX);
                float worldZ = Mathf.Lerp(bounds.min.z, bounds.max.z, normalizedY);

                // Aloituspiste reilusti objektin yl‰puolelta alasp‰in
                Vector3 rayOrigin = new Vector3(worldX, bounds.max.y + 10f, worldZ);
                Ray ray = new Ray(rayOrigin, Vector3.down);
                RaycastHit hit;

                if (meshCollider.Raycast(ray, out hit, bounds.size.y + 20f))
                {
                    // Lasketaan suhteellinen korkeus v‰lill‰ 0.0 - 1.0
                    float normalizedHeight = (hit.point.y - bounds.min.y) / bounds.size.y;
                    heightValues[x, y] = Mathf.Clamp01(normalizedHeight);
                }
                else
                {
                    // Jos s‰de ei osu kohteeseen, asetetaan pohjaksi nolla (musta)
                    heightValues[x, y] = 0f;
                }
            }
        }

        // Poistetaan v‰liaikaisesti lis‰tty collider, jos sellainen tehtiin
        if (addedCollider)
        {
            DestroyImmediate(meshCollider);
        }

        // Luodaan tavutaulukko 16-bittiselle RAW-datalle (jokainen pikseli tarvitsee 2 tavua)
        byte[] rawBytes = new byte[resolution * resolution * 2];
        int byteIndex = 0;

        // Unityn Terrain lukee RAW-datan riveitt‰in (Y-akseli ulompi, X-akseli sisempi lenkki)
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Muutetaan 0.0 - 1.0 liukuluku 16-bittiseksi kokonaisluvuksi (0 - 65535)
                ushort shortVal = (ushort)(heightValues[x, y] * 65535f);

                // Jaetaan 16-bittinen luku kahdeksi tavuksi (Little Endian / Windows)
                rawBytes[byteIndex++] = (byte)(shortVal & 0xFF);
                rawBytes[byteIndex++] = (byte)((shortVal >> 8) & 0xFF);
            }
        }

        // Tallennetaan .raw-tiedostona suoraan projektin Assets-kansioon
        string path = Path.Combine(Application.dataPath, fileName + ".raw");
        File.WriteAllBytes(path, rawBytes);

        // P‰ivitet‰‰n Unityn projektin‰kym‰, jotta tiedosto ilmestyy sinne heti
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Valmis!", "Heightmap tallennettu onnistuneesti nimell‰:\n" + fileName + ".raw Assets-kansioon.", "OK");
    }
}
