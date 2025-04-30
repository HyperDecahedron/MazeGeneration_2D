using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MazeCell2D : MonoBehaviour
{
    [SerializeField]
    public GameObject leftWall;

    [SerializeField]
    public GameObject rightWall;

    [SerializeField]
    public GameObject frontWall;

    [SerializeField]
    public GameObject backWall;

    [SerializeField]
    public GameObject tfCenter;

    [SerializeField]
    private GameObject unvistedBlock;

    [SerializeField]
    private GameObject visitedBlock;

    [SerializeField]
    private GameObject corruptedBlock;

    public bool IsVisited { get; private set; }

    public bool isCorrupted = false;

    public float fadeDuration = 3f;

    public void Visit()
    {
        IsVisited = true;
        unvistedBlock.SetActive(false);
    }

    public void UnVisit()
    {
        IsVisited = false;
    }

    public void ClearLeftWall()
    {
       leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        frontWall.SetActive(false);
    }


    public void ClearBackWall()
    {
       backWall.SetActive(false);
    }

    public void ShowLeftWall()
    {
        leftWall.SetActive(true);
    }

    public void ShowRightWall()
    {
        rightWall.SetActive(true);
    }

    public void ShowFrontWall()
    {
        frontWall.SetActive(true);
    }


    public void ShowBackWall()
    {
       backWall.SetActive(true);
    }

    public void ShowVisitedBlock()
    {
        visitedBlock.SetActive(true);
    }

    public void ShowCorruptedBlock()
    {
        StartCoroutine(FadeInTilemap());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ShowVisitedBlock();
    }

    private IEnumerator FadeInTilemap()
    {
        corruptedBlock.SetActive(true);

        TilemapRenderer tilemapRenderer = corruptedBlock.GetComponent<TilemapRenderer>();

        Color originalColor = tilemapRenderer.material.color;
        Color newColor = originalColor;
        newColor.a = 0f;
        tilemapRenderer.material.color = newColor;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            newColor.a = alpha;
            tilemapRenderer.material.color = newColor;
            yield return null;
        }

        // Ensure final alpha is set
        newColor.a = 1f;
        tilemapRenderer.material.color = newColor;
    }

}
