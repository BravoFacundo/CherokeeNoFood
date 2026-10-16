using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// In-game HUD: enemy bars and coins. Moved as-is from UIManager.
public class HUDManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<GameObject> bars = new();
    [SerializeField] private List<GameObject> coins = new();
    [SerializeField] private GameObject enemyBar;

    [Header("References")]
    [SerializeField] private Transform enemyBarParent;
    [SerializeField] private Transform popupsParent;
    [SerializeField] private List<RectTransform> coinTransforms = new List<RectTransform>();

    [Header("Prefabs")]
    [SerializeField] private GameObject popupNewLevelPrefab;
    [SerializeField] private GameObject enemyBarPrefab;
    [SerializeField] private GameObject enemyCoinPrefab;

    [Header("Lists")]
    [SerializeField] private List<GameObject> enemiesList;
    [SerializeField] private List<GameObject> bossList;
    [SerializeField] private int totalSpawnedEnemies;
    // totalSpawnedEnemies = enemiesList.Count + bossList.Count
    [SerializeField] private int totalBars; //Cada 5 enemigos de enemiesList suma 1, cada 1 de bossList suma 1
    [SerializeField] private int totalEnemies;
    [SerializeField] private int totalBosses;
    // if totalBosses == totalBars || totalBosses == 0 || totalBars == 1 { No Order Rule }

    // if totalBars == 2 && totalBosses == 1 { Boss on 1st place }

    // if totalBars == 3 && totalBosses == 1 { Boss on 2nd place }
    // if totalBars == 3 && totalBosses == 2 { Boss on 1st and 3rd place }

    // if totalBars == 4 && totalBosses == 1 { Not Posible }
    // if totalBars == 4 && totalBosses == 2 { Boss on 2nd and 3rd place }
    // if totalBars == 4 && totalBosses == 3 { Not Recommended, but Boss on 1st, 3rd and 4th place }

    public void InstantiateBars(int enemyCount)
    {
        float enemiesBarYpos = enemyBarPrefab.GetComponent<RectTransform>().anchoredPosition.y;
        float enemiesBarWidth = enemyBarPrefab.GetComponent<RectTransform>().rect.width - 12;

        coinTransforms = enemyBarPrefab.GetComponentsInChildren<RectTransform>().ToList();
        coinTransforms.RemoveAt(0);

        int barsNeeded = CalculateEnemyBars(enemyCount);
        float barPosX = CalculateBarStartPosX(barsNeeded, enemiesBarWidth);

        Utilities.DestroyChildElements(enemyBarParent.gameObject);
        for (int i = 0; i < barsNeeded; i++)
        {
            GameObject newBar = Instantiate(enemyBarPrefab, enemyBarParent);

            RectTransform newBarRT = newBar.GetComponent<RectTransform>();
            newBarRT.anchoredPosition = new Vector2(barPosX, enemiesBarYpos);
            barPosX += Mathf.Abs(enemiesBarWidth / 2) * 2;

            for (int j = 0; j < 5; j++)
            {
                Destroy(newBar.transform.GetChild(j).gameObject);
                GameObject newCoin = Instantiate(enemyCoinPrefab, newBar.transform);
                RectTransform newCoinRT = newCoin.GetComponent<RectTransform>();
                newCoinRT.anchoredPosition = coinTransforms[j].anchoredPosition;
            }
        }
    }

    private int CalculateEnemyBars(int enemyCount)
    {
        if (enemyCount <= 0) return 0;
        int barsNeeded = enemyCount / 5;
        if (enemyCount % 5 != 0) barsNeeded++;
        return barsNeeded;
    }
    private float CalculateBarStartPosX(float barsNeeded, float enemiesBarWidth)
    {
        if (barsNeeded == 0) return 0;
        float barPosX = (enemiesBarWidth / 2) * (barsNeeded - 1);
        return barPosX * -1;
    }
}
