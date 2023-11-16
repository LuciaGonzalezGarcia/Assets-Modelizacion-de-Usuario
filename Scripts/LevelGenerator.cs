using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor;

public class LevelGenerator : MonoBehaviour
{
    public ColumnInfo[] columns;
    public GameObject m_End;
    private float columnWidth = 0.625f;
    private int levelLength;
    public static Vector3 startingPosition = new Vector3(-8, 0, 0);
    private float countTime = 0f;
    public static bool inEnd = false;

    const float THRESHOLD = 0.95f;
    const float MAX_ATTEMPTS = 100;

    void Start()
    {
        //EN FUNCIÓN DEL ENGAGEMENT ESTIMADO DE LA PARTIDA ANTERIOR ASIGNAMOS UN TAMAÑO AL NIVEL.
        if (DataManeger.engagement == "No")
        {
            levelLength = 100;
        }
        else if (DataManeger.engagement == "Some")
        {
            levelLength = 150;
        }
        else if (DataManeger.engagement == "Yes")
        {
            levelLength = 200;
        }

        Debug.Log(levelLength);

        int[] currLevel = GetRandomLevel();
        float fitness = Fitness(currLevel);

        
        int steps = 0;
        while (Fitness(currLevel) < THRESHOLD && steps< MAX_ATTEMPTS)
        {
            steps += 1;
            List<int[]> neighbors = GetNeighborLevels(currLevel);

            foreach(int[] neighbor in neighbors)
            {
                float neighborFitness = Fitness(neighbor);
                if (neighborFitness > fitness)
                {
                    fitness = neighborFitness;
                    currLevel = neighbor;
                }
            }
        }
        Debug.Log("Fitness: " + fitness+" after "+steps+" steps");
        

        //Visualize level
        for (int i = 0; i < levelLength; i++)
        {
            GameObject gameObject = GameObject.Instantiate(columns[currLevel[i]].gameObject);
            gameObject.transform.position = startingPosition + Vector3.right * i * columnWidth;
        }

        m_End.transform.position = startingPosition + Vector3.right * (levelLength - 1) * columnWidth;

    }

    void Update()
    {
        if (!inEnd)
        {
            countTime += Time.deltaTime;

        }
        else
        {
            //Guardo el numero de saltos y el tiempo que he tardado
            DataManeger.Write(CharacterMovement.numJumps, countTime);
            inEnd = false;
            CharacterMovement.numJumps = 0;
            EditorApplication.isPlaying = false;
        }
        
    }

    private float Fitness(int[] _level)
    {
        float gaps = 0;
        float enemies = 0;
        float platforms = 0;
        float items = 0;
        float signs = 0;

        float scoreModifier = 0;

        for (int i = 0; i < _level.Length; i++)
        {
            if (columns[_level[i]].height == 0)
            {
                gaps += 1;
            }

            if (columns[_level[i]].hasEnemy)
            {
                enemies += 1f;
            }

            if (columns[_level[i]].hasPlatform)
            {
                if (columns[_level[i]].hasItem)
                {
                    items += 1;
                }
                else
                {
                    platforms += 1f;
                }
                    
            }

            if (columns[_level[i]].hasSign)
            {
                signs += 1f;
            }

            

            //El principio no gap, no enemy, no item
            if (i < 3 && (columns[_level[i]].height == 0 || columns[_level[i]].hasEnemy || columns[_level[i]].hasItem))
            {
                scoreModifier -= 0.1f;
            }

            //El final no gap, no enemy, no item
            if (i > _level.Length-5 && (columns[_level[i]].height == 0 || columns[_level[i]].hasEnemy || columns[_level[i]].hasItem))
            {
                scoreModifier -= 0.1f;
            }

            //EN FUNCIÓN DEL DATO ALMACENADO SOBRE LOS SALTOS QUE HA DADO EL JUGADOR EN LA PARTIDA ANTERIOR
            //HAZEMOS QUE SEA MAS O MENOS PLANO, MIENTRAS MENOS SALTOS DE, MENOS PLANO SERÁ.
            //Para hacerlo mas plano, penalizo que la columna actual y la anterior no tengan la misma altura
            if (i > 0 && columns[_level[i]].height != 0 && columns[_level[i]].height != columns[_level[i-1]].height)
            {
                scoreModifier -= 0.03f - DataManeger.jumps / 100;
            }

            //Para que no haya dos señales juntas
            if (i > 0 && columns[_level[i]].hasSign && !columns[_level[i-1]].hasSign)
            {
                scoreModifier += 0.1f;
            }

            //Premio que aparezca una señal al principio
            if (i < 10 && columns[_level[i]].hasSign)
            {
                scoreModifier += 0.1f;
            }

            //Para que no haya dos items juntos
            if (i > 0 && columns[_level[i]].hasItem && !columns[_level[i - 1]].hasItem)
            {
                scoreModifier += 0.1f;
            }

            //Premio que haya plataformas en los gap
            if (i > 0 && columns[_level[i]].hasPlatform && columns[_level[i]].height == 0)
            {
                scoreModifier += 0.01f;
            }

            //Penalizo que haya plataformas en los no gap
            if (i > 0 && columns[_level[i]].hasPlatform && columns[_level[i]].height != 0)
            {
                scoreModifier -= 0.02f;
            }
        }

        float score = 1.0f-Mathf.Abs(gaps - (float)levelLength / 3f)/((float)levelLength/3f);
        score += 1.0f - Mathf.Abs(enemies - (float)levelLength / 10f) / ((float)enemies / 10f);
        score += 1.0f - Mathf.Abs(platforms - (float)levelLength / 5f) / ((float)platforms / 5f);
        score += 1.0f - Mathf.Abs(items - (float)levelLength / 20f) / ((float)platforms / 20f);
        score += 1.0f - Mathf.Abs(signs - (float)levelLength / 40f) / ((float)platforms / 40f);

        score /= 5f;
        score += scoreModifier;

        return score;
    }
    
    private List<int[]> GetNeighborLevels(int[] level)
    {
        List<int[]> neighbors = new List<int[]>();
        for(int i = 0; i<level.Length; i++)
        {
            for(int j = 0; j<columns.Length; j++)
            {
                if (j != level[i])
                {
                    int[] levelClone = level.ToArray();
                    levelClone[i] = j;
                    neighbors.Add(levelClone);
                }
            }
        }

        for (int i = 0; i < neighbors.Count; i++)
        {
            int[] temp = neighbors[i];
            int randomIndex = Random.Range(i, neighbors.Count);
            neighbors[i] = neighbors[randomIndex];
            neighbors[randomIndex] = temp;
        }

        return neighbors;
    }

    private int[] GetRandomLevel()
    {
        int[] levelColumns = new int[levelLength];

        for (int i = 0; i < levelColumns.Length; i++)
        {
            levelColumns[i] = Random.Range(0, columns.Length);
        }

        return levelColumns;
    }
}
