using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(ARTrackedImageManager))]

public class ImageTracking : MonoBehaviour
{
    [SerializeField]
    private GameObject[] placeablePrefabs;

    [SerializeField]
    private AudioClip[] audioClips;

    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private HashSet<string> updatedPrefabNames = new HashSet<string>();
    private ARTrackedImageManager trackedImageManager;

    private void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        foreach (GameObject prefab in placeablePrefabs)
        {
            GameObject newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.name = prefab.name;
            spawnedPrefabs.Add(prefab.name, newPrefab);
        }
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
            
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            updatedPrefabNames.Remove(trackedImage.name);
            spawnedPrefabs[trackedImage.name].SetActive(false);
        }
        if (eventArgs.removed.Count > 0 && eventArgs.updated.Count == 0 && eventArgs.added.Count == 0)
        {
            updatedPrefabNames.Clear();
        }
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 position = trackedImage.transform.position;

        if (!spawnedPrefabs.ContainsKey(name))
        {
            Debug.LogWarning("Prefab not found for " + name);
            return;
        }

        GameObject prefab = spawnedPrefabs[name];
        prefab.transform.position = position;
        prefab.SetActive(true);

        // Sadece bir kere güncellenen prefab'ı set'e ekle
        if (updatedPrefabNames.Add(name))
        {
            PlayAudioClip(name);
        }


        foreach (GameObject go in spawnedPrefabs.Values)
        {
            if (go.name != name)
            {
                go.SetActive(false);
            }
        }
    }

   private void PlayAudioClip(string clipName)
{
    if (Array.Exists(audioClips, clip => clip.name == clipName) && spawnedPrefabs.ContainsKey(clipName))
    {
        GameObject prefab = spawnedPrefabs[clipName];

        // Get or add AudioSource component
        AudioSource audioSource = prefab.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Create a new AudioSource component if not found
            audioSource = prefab.AddComponent<AudioSource>();
        }

        // Stop any previous audio playback and set the new AudioClip
        audioSource.Stop();
        audioSource.clip = Array.Find(audioClips, clip => clip.name == clipName);

        // Play the audio
        audioSource.Play();
    }
}


}