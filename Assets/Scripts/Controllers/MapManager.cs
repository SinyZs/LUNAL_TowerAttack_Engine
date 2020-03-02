using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/* Controller Global du System de Map
 * Contient les references vers les Datas de la map (MapData) 
 * Contient les references vers les vues de la map
 */
public class MapManager : MonoBehaviour
{
    // Variable de Data.
    public MapData mapData;

    // Variables de vues & NavMesh.
    [Header("View Var")]
    public bool generateView = true;
    public GameObject prefabSurface;
    public GameObject navContainer;
    public GameObject prefabWall, prefabWater;
    public GameObject prefabEdgeHori, prefabEdgeVert;

    // Variables de debug.
    [Header("Debug Var")]
    [Range(0.0f,0.5f)]
    public float downScale = 0.1f;
    public bool showDebugGrid = true;
    public List<Vector3> debugLockRemoved;

    [ContextMenu("InitializeMap")]
    public void InitializeMap()
    {
        // ====== Intialization des Datas.
        // Squares total random
        // IntializeSquareGrid();
        // Square en fonction du pourcent des Lock
        InitializeSquareGridPercentWall();

        // Edges Hori
        InitializeEdgeGrid(ref mapData.edgesHori, mapData.width * (mapData.height + 1), mapData.percenteEdgeHori);
        // Edges Vert
        InitializeEdgeGrid(ref mapData.edgesVert, (mapData.width + 1) * mapData.height, mapData.percentEdgeVert);

        // Initialisation de la liste de debug
        debugLockRemoved = new List<Vector3>();
        // On appel la regle de validation des Edges VS Squares
        ProcessRuleSquareVSEdge(mapData.edgesHori, mapData.width, new Vector3(0, 0, -1));
        ProcessRuleSquareVSEdge(mapData.edgesVert, mapData.width + 1, new Vector3(-1, 0, 0));

        // ====== Creation de la View
        // Clean NavContainer
        DestroyAllChild(navContainer);

        if(generateView)
        {
            // Instantiation des view des squares en fonction des Datas.
            CreateSquaresView();

            // Intstantiation des views des edges en fonction des datas
            CreateEdgesView(ref mapData.edgesHori, prefabEdgeHori, mapData.width, new Vector3(0.5f, 0, 0));
            CreateEdgesView(ref mapData.edgesVert, prefabEdgeVert, mapData.width + 1, new Vector3(0, 0, 0.5f));

            // Creation de la surface en fonction des Datas.
            CreateSurface();
        }
    }

    private void DestroyAllChild(GameObject parent)
    {
        if (parent != null)
        {
            // Mode editeur pas : Destroy(parent);
            // Il faut utiliser : DestroyImmediate();
            for (int i = parent.transform.childCount; i > 0; i--)
            {
                DestroyImmediate(parent.transform.GetChild(0).gameObject);
            }
        }
    }

    private Vector3 GetPositionFromIndex(int i, int width)
    {
        Vector3 pos = Vector3.zero;
        // Calcul des coordonnées du square dans la grid à partir de l'index dand le tableau.
        // Calcul de la position en x => le reste de la division
        pos.x = i % width;
        // Calcul de la position en z => partie entière de la division  
        pos.z = i / width;
        return pos;
    }

    private bool CalculateRandomFromPercentInt(int percent)
    {
        return Random.Range(1, 101) <= percent;
    }

    private bool CalculateRandomFromPercent(float percent)
    {
        return Random.Range(0.1f, 100.0f) <= percent;
    }

    private SquareState RandomStateWithSkipOneValue(SquareState skippedValue)
    {
        int nbrValueInEnum = System.Enum.GetValues(typeof(SquareState)).Length;
        int val = Random.Range(0, nbrValueInEnum);

        // On ajout +1 par rapport à la value qui doit être evitée (skip)
        if (val >= (int)skippedValue)
        {
            val++;
        }
        return (SquareState)val;
    }
    #region SQUARES
    private void IntializeSquareGridRandom()
    {
        mapData.grid = new SquareData[mapData.width * mapData.height];
        // Parcours des square pour générer aléatoirement un state.
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            int valMin = (int)SquareState.Normal;
            int valMax = (int)SquareState.Special;
            mapData.grid[i].state = (SquareState)Random.Range(valMin, valMax + 1);
        }
    }

    private void InitializeSquareGridPercentWall()
    {
        mapData.grid = new SquareData[mapData.width * mapData.height];

        // Parcours des square pour générer aléatoirement un state.
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            mapData.grid[i].state = RandomStateWithSkipOneValue(SquareState.Lock);
        }
        
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            if(CalculateRandomFromPercentInt(mapData.percentSquareLock))
            {
                mapData.grid[i].state = SquareState.Lock;
            }
        }
    }

    // Permet de créer la vue des squares en fonction de leur state.
    private void CreateSquaresView()
    {
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            // Creation d'une prefab de vue en focntion du state.
            GameObject newSquareView = null;
            switch (mapData.grid[i].state)
            {
                case SquareState.Lock:
                    newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabWall);
                    break;
                case SquareState.Water:
                    newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabWater);
                    break;
            }

            // Placement de la vue.
            if (newSquareView != null)
            {
                newSquareView.transform.SetParent(navContainer.transform);
                Vector3 newPosSquare = GetPositionFromIndex(i, mapData.width);
                newPosSquare.x += 0.5f;
                newPosSquare.z += 0.5f;
                newSquareView.transform.position = newPosSquare;
            }
        }
    }

    private int GetIndexSquareFromPos(Vector3 pos)
    {
        // Test si la position sort des limites
        if (pos.x >= 0 && pos.z >= 0 && pos.x < mapData.width && pos.z < mapData.height)
        {
            return ((int)pos.z * mapData.width) + (int)pos.x;
        }
        // Pas de square à la position demandée => on retourne -1
        return -1;
    }
    #endregion SQUARES

    #region EDGES
    private void InitializeEdgeGrid(ref bool[] arrayEdges, int numberElement, int percentRandom)
    {
        arrayEdges = new bool[numberElement];

        // Generation Random des edges
        for (int i = 0; i < arrayEdges.Length; i++)
        {
            // Random 50 / 50
            // arrayEdges[i] = Random.Range(0, 2) == 0;

            // En fonction d'un pourcentage
            arrayEdges[i] = Random.Range(1, 101) <= percentRandom;
        }
    }

    private void ProcessRuleSquareVSEdge(bool[] arrayEdges, int width, Vector3 adderTestLock)
    {
        for (int i = 0; i < arrayEdges.Length; i++)
        {
            // Si Edge presente.
            if (arrayEdges[i])
            {
                // On recupere la position de la edge en fonction de l'index.
                Vector3 newPosSquare = GetPositionFromIndex(i, width);

                // On test à cette position s'il y a un Square Lock.
                ValidateIfNoLockSquare(newPosSquare);

                // Et on test aussi à la position "derrière" la edge.
                newPosSquare += adderTestLock;
                ValidateIfNoLockSquare(newPosSquare);
            }
        }
    }

    private void ValidateIfNoLockSquare(Vector3 newPosSquare)
    {

        // On recupere l'index du square en fonction de la position.
        int indexSquare = GetIndexSquareFromPos(newPosSquare);
        // Si le square existe.
        if (indexSquare != -1)
        {
            // On Test si il est lock.
            if (mapData.grid[indexSquare].state == SquareState.Lock)
            {
                // On Set son state.
                mapData.grid[indexSquare].state = SquareState.Normal;

                // On ajoute le debug
                debugLockRemoved.Add(newPosSquare);
            }
        }
    }

    // Permet de créer la vue des edges en fonction de leur existance ou non.
    private void CreateEdgesView(ref bool[] arrayEdges, GameObject prefabEdge, int width, Vector3 adderPosition)
    {
        for (int i = 0; i < arrayEdges.Length; i++)
        {
            // Si Edge presente.
            if (arrayEdges[i])
            {
                // Creation d'une instance de prefab de vue en fonction du state.
                GameObject newSquareView = (GameObject)PrefabUtility.InstantiatePrefab(prefabEdge);
                newSquareView.transform.SetParent(navContainer.transform);
                Vector3 newEdgePos = GetPositionFromIndex(i, width);
                newEdgePos += adderPosition;
                newSquareView.transform.position = newEdgePos;
            }
        }
    }
    #endregion EDGES

    #region SURFACE
    // Permet de créer automatiquement la surface du navmesh à partir des données de la map
    private void CreateSurface()
    {
        // Création de la vue en fonction des Datas
        // GameObject surface = Instantiate(prefabSurface);
        GameObject surface = (GameObject)PrefabUtility.InstantiatePrefab(prefabSurface);
        surface.transform.SetParent(navContainer.transform);

        // Calcul de la position de la surface.
        Vector3 posSurface = surface.transform.position;
        posSurface.x = mapData.width / 2;
        posSurface.z = mapData.height / 2;

        // Test si width impair pour ajout de declage sur la position.
        if (mapData.width % 2 != 0)
        {
            posSurface.x += 0.5f;
        }
        if (mapData.height % 2 != 0)
        {
            posSurface.z += 0.5f;
        }
        surface.transform.position = posSurface;

        // Calcul du scale de la surface.
        Vector3 scaleSurface = surface.transform.localScale;
        scaleSurface.x = mapData.width;
        scaleSurface.z = mapData.height;
        surface.transform.localScale = scaleSurface;

        // Bake NavMesh
        NavMeshSurface surfaceComponent = surface.GetComponent<NavMeshSurface>();
        surfaceComponent.BuildNavMesh();
    }
    #endregion SURFACE

    #region DEBUG DRAW GIZMO MAP
    private void OnDrawGizmos()
    {
        if(showDebugGrid)
        {
            ShowGizmoMapSquares();

            ShowGizmoMapEdges();
        }
    }

    private void ShowGizmoMapEdges()
    {
        Vector3 pos = Vector3.zero;
        Vector3 scaleHori = (Vector3.one) / 10;
        scaleHori.x = 0.8f;

        // Parcours des élements du tableau via un for.
        for (int i = 0; i < mapData.edgesHori.Length; i++)
        {
            if(mapData.edgesHori[i])
            {
                pos = GetPositionFromIndex(i, mapData.width);
                Gizmos.color = Color.red;
                pos.x += 0.5f;
                Gizmos.DrawCube(pos, scaleHori);
            }
        }

        Vector3 scaleVert = (Vector3.one) / 10;
        scaleVert.z = 0.8f;
        for (int i = 0; i < mapData.edgesVert.Length; i++)
        {
            if (mapData.edgesVert[i])
            {
                Gizmos.color = Color.blue;
                pos = GetPositionFromIndex(i, mapData.width + 1);
                pos.z += 0.5f;
                Gizmos.DrawCube(pos, scaleVert);
            }
        }

        foreach (Vector3 position in debugLockRemoved)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(position.x + 0.5f, 1.1f, position.z + 0.5f), (Vector3.one) / 10);
        }
    }

    private void ShowGizmoMapSquares()
    {
        Vector3 pos = Vector3.zero;
        // Parcours des élements du tableau via un for.
        for (int i = 0; i < mapData.grid.Length; i++)
        {
            pos = GetPositionFromIndex(i, mapData.width);
            Gizmos.color = GetColorFromState(mapData.grid[i].state);
            //Gizmos.DrawCube(pos, (Vector3.one) / 2);

            // Affichage 1ere ligne
            Vector3 posFrom = pos;
            posFrom.x += downScale;
            posFrom.z += downScale;
            Vector3 posTo = pos;
            posTo.x += 1 - downScale;
            posTo.z += downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 2eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += 1 - downScale;
            posFrom.z += downScale;
            posTo.x += 1 - downScale;
            posTo.z += 1 - downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 3eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += 1 - downScale;
            posFrom.z += 1 - downScale;
            posTo.x += downScale;
            posTo.z += 1 - downScale;
            Gizmos.DrawLine(posFrom, posTo);

            // Affichage 4eme ligne
            posFrom = pos;
            posTo = pos;
            posFrom.x += downScale;
            posFrom.z += 1 - downScale;
            posTo.x += downScale;
            posTo.z += downScale;
            Gizmos.DrawLine(posFrom, posTo);
        }
    }

    private Color GetColorFromState(SquareState state)
    {
        switch (state)
        {
            case SquareState.Lock:
                return Color.black;
            case SquareState.Grass:
                return Color.green;
            case SquareState.Water:
                return Color.blue;
            case SquareState.Special:
                return Color.magenta;
            default:
                return Color.white;
        }
    }
    #endregion DEBUG DRAW GIZMO MAP
}
