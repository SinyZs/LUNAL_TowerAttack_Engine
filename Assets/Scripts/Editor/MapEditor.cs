
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SquareBrush
{
    One,
    Line,
    Cube,
    L
}

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    // Ref instance editée
    private MapManager m_CurrentMapTarget = null;

    // Variable Square Edit Mode
    private bool m_IsInEditSquareMode = false;
    private SquareState m_CurrentEditSquareState = SquareState.Normal;
    private SquareBrush m_CurrentSquareBrush = SquareBrush.Line;

    // Variable Brush
    private int m_CurrentBrushOrientation = 0;
    private readonly Vector3[] brushArrayLine = new Vector3[] { Vector3.right };
    private readonly Vector3[] brushArrayCube = new Vector3[] { Vector3.right, Vector3.forward, new Vector3(1, 0, 1)};
    private readonly Vector3[] brushArrayL = new Vector3[] { Vector3.right, Vector3.forward, new Vector3(0, 0, 2) };    
    
    // Variable Edge Edit Mode
    private bool m_IsInEditEdgeMode = false;
    private bool m_IsAddOrRemoveEdge = true;

    // Map Properties
    private bool m_ShowMapView = true;

    // Global edit constraint
    private bool m_CanEditMouseAndKeyConstraints = true;

    private void OnEnable()
    {
        // Recupération de l'instance editée
        m_CurrentMapTarget = (MapManager)target;

        // Recupération de l'état de la vue de la map
        if (m_CurrentMapTarget.navContainer != null)
        {
            m_ShowMapView = m_CurrentMapTarget.navContainer.activeSelf;
        }

        // Chargement des états précédents de l'editor
        LoadLastEditorState();
    }

    private void OnDisable()
    {
        SaveCurrentEditorState();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("============ MAP EDITOR =============", EditorStyles.boldLabel);
        // Affichage Bouton Initialize Map And Generate.
        if (GUILayout.Button("Initialize Map Randomly"))
        {
            m_CurrentMapTarget.InitializeMapRandomly();
            SetObjectDirty(m_CurrentMapTarget);
        }

        // Affichage Bouton de Reset de la map.
        if (GUILayout.Button("Initialize Empty Map"))
        {
            m_CurrentMapTarget.InitiliazeEmptyMap();
            SetObjectDirty(m_CurrentMapTarget);
        }

        GUILayout.Space(10);
        m_ShowMapView = GUILayout.Toggle(m_ShowMapView, "Show View");
        if (m_CurrentMapTarget.navContainer != null)
        {
            m_CurrentMapTarget.navContainer.SetActive(m_ShowMapView);
        }

        GUILayout.Label("EDIT SQUARE", EditorStyles.boldLabel);
        m_IsInEditSquareMode = GUILayout.Toggle(m_IsInEditSquareMode, "Edit Square Mode");
        if(m_IsInEditSquareMode)
        {
            m_IsInEditEdgeMode = false;

            // Affichage pop up d'edition du state d'un square de la map
            m_CurrentEditSquareState = (SquareState)EditorGUILayout.EnumPopup(m_CurrentEditSquareState);

            // Affichage pop up brush
            m_CurrentSquareBrush = (SquareBrush)EditorGUILayout.EnumPopup(m_CurrentSquareBrush);
            GUILayout.Label("Brush orientation 'R' : " + m_CurrentBrushOrientation);
        }

        GUILayout.Label("EDIT EDGE", EditorStyles.boldLabel);
        m_IsInEditEdgeMode = GUILayout.Toggle(m_IsInEditEdgeMode, "Edit Edge Mode");
        if(m_IsInEditEdgeMode)
        {
            m_IsInEditSquareMode = false;
            m_IsAddOrRemoveEdge = GUILayout.Toggle(m_IsAddOrRemoveEdge, m_IsAddOrRemoveEdge ? "Add Edge" : "Remove Edge");
        }

        GUILayout.Label("============ MAP PROPERTIES =============", EditorStyles.boldLabel);
        base.OnInspectorGUI();

    }

    // Ici on affiche dans la Scene les elements necessaire.
    // Ici on recupère les inputs qui ont été fait dans la Scene.
    private void OnSceneGUI()
    {
        // On valide si on peut editer
        UpdateGlobalEditState();

        // Si on peut editer
        if (m_CanEditMouseAndKeyConstraints)
        {
            // Si on est en edit square mode
            if (m_IsInEditSquareMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // On verifie si le mode d'edition change
                UpdateEditModeSquareState();

                // On verifie si l'orientation change
                UpdateBrushOrientation();

                // Calculate Interact Coordonnee
                Vector3 intersectPos = CalculateInteractPosition();
                intersectPos.y = 0;

                Vector3 intersectPosInt = GetRoundPos(intersectPos);

                //Debug.Log("Scene GUI is painted");
                DisplayGizmoEditSquareInScene(intersectPosInt);

                EditCurrentSquareState(intersectPosInt);
            }
            else if(m_IsInEditEdgeMode)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                // Calculate Interact Coordonnee
                Vector3 intersectPos = CalculateInteractPosition();

                int indexEdge = DisplayGizmoEditEdgeInScene(intersectPos);

                EditCurrentEdge(indexEdge, intersectPos);
            }
        }
    }

    #region GENERIQUE METHODE
    private Vector3 GetRoundPos(Vector3 intersectPos)
    {
        Vector3 intersectPosInt = Vector3.zero;
        intersectPosInt.x = Mathf.FloorToInt(intersectPos.x);
        intersectPosInt.z = Mathf.FloorToInt(intersectPos.z);
        return intersectPosInt;
    }

    private Vector3 CalculateInteractPosition()
    {
        Vector3 mousePosition = Event.current.mousePosition;

        // recupération d'un Ray (rayon) à partir de la position de la mouse sur l'ecran
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Creation d'un plan dans l'espace
        // Il n'y a pas de plan créer dans la scene
        Plane hPlane = new Plane(Vector3.up, Vector3.zero);

        // On envoi le rayon par rapport à la scene
        if (hPlane.Raycast(ray, out float distance))
        {
            // get the hit point:
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private bool TestIfPositionIsInLimit(Vector3 position)
    {
        return position.x >= 0 && position.z >= 0 && position.x < m_CurrentMapTarget.mapData.width && position.z < m_CurrentMapTarget.mapData.height;
    }

    private bool EditInputTriggered()
    {
        if (Event.current.button == 0)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    return true;
            }
        }
        return false;
    }

    private void UpdateBrushOrientation()
    {
        if (Event.current.keyCode == KeyCode.R)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                m_CurrentBrushOrientation += 90;
                // Lorsque la valuer sera egal 360 ou plus 
                // On redemarre de 0;
                m_CurrentBrushOrientation %= 360;
            }
        }
    }
    #endregion GENERIQUE METHODE

    #region DISPLAY EDIT SQUARE GIZMO
    private void DisplayGizmoEditSquareInScene(Vector3 intersectPosInt)
    {
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPosInt))
        {
            DrawEditSquareGizmo(intersectPosInt);

            switch(m_CurrentSquareBrush)
            {
                case SquareBrush.Line:
                    DisplayGizmoShape(intersectPosInt, brushArrayLine);
                    break;
                case SquareBrush.Cube:
                    DisplayGizmoShape(intersectPosInt, brushArrayCube);
                    break;
                case SquareBrush.L:
                    DisplayGizmoShape(intersectPosInt, brushArrayL);
                    break;
            }
        }
        SceneView.RepaintAll();
    }

    private void DisplayGizmoShape(Vector3 intersectPos, Vector3[] allFigurePos)
    {
        foreach (Vector3 pos in allFigurePos)
        {
            // On oriente en fonction de l'orientation de la brush
            Vector3 posBrushOriented = SetByBrushOrientation(pos);

            // On affiche le gizmo
            DrawEditSquareGizmo(intersectPos + posBrushOriented);
        }
    }

    private void DrawEditSquareGizmo(Vector3 intersectPos)
    {
        Handles.color = m_CurrentMapTarget.GetColorFromState(m_CurrentEditSquareState);

        intersectPos.x += 0.5f;
        intersectPos.z += 0.5f;
        Vector3 scaleGizmo = Vector3.one;
        scaleGizmo.y = 0.2f;
        Handles.DrawWireCube(intersectPos, scaleGizmo);
    }
    
    // Methode permettant de changer le mode
    // d'edition des squares en appuyant sur la touche E
    // On passe au suivant tant que c possible.
    // Sinon on recommence de la première valeur d'enum
    private void UpdateEditModeSquareState()
    {
        if (Event.current.keyCode == KeyCode.E)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                m_CurrentEditSquareState += 1; 
                // On redemarre de 0 si on depasse le nombre de valeurs de l'enum;
                int newVal = (int)m_CurrentEditSquareState % System.Enum.GetNames(typeof(SquareState)).Length;
                m_CurrentEditSquareState = (SquareState)newVal;
            }
        }
    }
    #endregion DISPLAY EDIT SQUARE GIZMO

    #region EDIT SQUARE
    private void EditCurrentSquareState(Vector3 intersectPos)
    {
        if (EditInputTriggered())
        {
            // On edit le square de base
            m_CurrentMapTarget.SetSquareState(intersectPos, m_CurrentEditSquareState);

            // On edit les autres squares en fonction de la brush
            ProcessBrushEdit(intersectPos);

            // On update la vue de la map
            m_CurrentMapTarget.CreateMapViewFromData();

            // On force la Scene Unity en état "Editée"
            SetObjectDirty(m_CurrentMapTarget);
        }
    }

    private void ProcessBrushEdit(Vector3 intersectPos)
    {
        switch (m_CurrentSquareBrush)
        {
            case SquareBrush.Line:
                EditSquareByShape(intersectPos, brushArrayLine);
                break;
            case SquareBrush.Cube:
                EditSquareByShape(intersectPos, brushArrayCube);
                break;
            case SquareBrush.L:
                EditSquareByShape(intersectPos, brushArrayL);
                break;
        }
    }

    private void EditSquareByShape(Vector3 intersectPos,  Vector3[] allFigurePos)
    {
        foreach(Vector3 pos in allFigurePos)
        {
            // On oriente en fonction de l'orientation de la brush
            Vector3 posBrushOriented = SetByBrushOrientation(pos);

            // On affiche le gizmo
            m_CurrentMapTarget.SetSquareState(intersectPos + posBrushOriented, m_CurrentEditSquareState);
        }
    }

    private Vector3 SetByBrushOrientation(Vector3 toOrient)
    {
        return Quaternion.Euler(0, m_CurrentBrushOrientation, 0) * toOrient;
    }

    // Skip Update si en train d'appuyer sur Alt
    private void UpdateGlobalEditState()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = false;
                }
                break;
            case EventType.KeyUp:
                if (e.keyCode == KeyCode.LeftAlt)
                {
                    m_CanEditMouseAndKeyConstraints = true;
                }
                break;
        }
    }
    #endregion

    #region DISPLAY EDIT EDGE GIZMO
    private int DisplayGizmoEditEdgeInScene(Vector3 intersectPos)
    {
        int edgeIndex = -1;
        // Affichage du gizmo uniquement si on est dans la grille
        if (TestIfPositionIsInLimit(intersectPos))
        {
            edgeIndex = DrawEdgeSelectedGizmo(intersectPos);
        }
        SceneView.RepaintAll();
        return edgeIndex;
    }

    // Trouver sur internet ici : https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    // Fonction permettant de tester si un point est dans un triangle
    private bool IsInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, v1, v2);
        d2 = Sign(pt, v2, v3);
        d3 = Sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    // On recupère la position par rapport au square
    // On divise en 4 triangles le Square et on test si le point d'intersection est dans un des triangles
    // Si triangle du bas => edge du bas => return 0
    // Si triangle de gauche => edge de gauche => return 1
    // Si triangle du haut => edge du haut => return 2
    // Si triangle de droite => edge de droite => return 3
    // On retourne un int en fonction de quel edge on edit 
    private int DrawEdgeSelectedGizmo(Vector3 intersectPos)
    {
        // On recupère la partie decimal de la position.
        // Ceci pour pouvoir tester la position de la souris 
        // dans un square à la position (0,0).
        float xDecimal = intersectPos.x - (int)intersectPos.x;
        float zDecimal = intersectPos.z - (int)intersectPos.z;

        // On recupère la position en int
        Vector3 posEdge = Vector3.zero;
        posEdge.x = (int)intersectPos.x;
        posEdge.z = (int)intersectPos.z;

        // On declare une variable int pour recupérer
        // quelle edge on edit : bas (0), gauche (1), haut (2), droite (3) 
        int edgeOrientation = 0;
        Vector3 scaleWireSquare = Vector3.one / 10;

        // Edge du bas
        if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 0.5f;
            scaleWireSquare.x = 1;
        }
        // Edge de gauche
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.z += 0.5f;
            edgeOrientation = 1;
            scaleWireSquare.z = 1;
        }
        // Edge de droite
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 1.0f;
            posEdge.z += 0.5f;
            edgeOrientation = 3;
            scaleWireSquare.z = 1;
        }
        // Edge du haut
        else if (IsInTriangle(new Vector2(xDecimal, zDecimal), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f)))
        {
            posEdge.x += 0.5f;
            posEdge.z += 1f;
            edgeOrientation = 2;
            scaleWireSquare.x = 1;
        }

        Handles.color = m_IsAddOrRemoveEdge ? Color.cyan : Color.red;
        Handles.DrawWireCube(posEdge, scaleWireSquare);
        return edgeOrientation;
    }

    // Edition des dats l'edge montrée par le gizmos
    // EdgeOrientation est l'edge du square  editée (bas, gauche, haut, droite)
    // IntersectPos est la position du square
    private void EditCurrentEdge(int edgeOrientation, Vector3 intersectPos)
    {
        if (EditInputTriggered())
        {
            // Edit les datas de l'edge en fontion de
            // son orientation
            switch(edgeOrientation)
            {
                // Edges Hori
                case 0:
                    m_CurrentMapTarget.SetEdgeData(true, intersectPos, m_IsAddOrRemoveEdge);
                    break;
                case 2:
                    m_CurrentMapTarget.SetEdgeData(true, intersectPos + new Vector3(0, 0, 1), m_IsAddOrRemoveEdge);
                    break;
                // Edges Vert
                case 1:
                    m_CurrentMapTarget.SetEdgeData(false, intersectPos, m_IsAddOrRemoveEdge);
                    break;
                case 3:
                    m_CurrentMapTarget.SetEdgeData(false, intersectPos + new Vector3(1, 0, 0), m_IsAddOrRemoveEdge);
                    break;
            }

            // On update la vue de la map
            m_CurrentMapTarget.CreateMapViewFromData();

            // On force la Scene Unity en état "Editée"
            SetObjectDirty(m_CurrentMapTarget);
        }
    }
    #endregion

    #region LOAD / SAVE EDITOR STATE
    private void SetObjectDirty(Object objectDirty)
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(objectDirty);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void LoadLastEditorState()
    {
        m_IsInEditSquareMode = EditorPrefs.GetBool(nameof(m_IsInEditSquareMode));
        m_CurrentEditSquareState = (SquareState)EditorPrefs.GetInt(nameof(m_CurrentEditSquareState));
        m_CurrentSquareBrush = (SquareBrush)EditorPrefs.GetInt(nameof(m_CurrentSquareBrush));
        m_IsInEditEdgeMode = EditorPrefs.GetBool(nameof(m_IsInEditEdgeMode));
        m_IsAddOrRemoveEdge = EditorPrefs.GetBool(nameof(m_IsAddOrRemoveEdge));
    }
    private void SaveCurrentEditorState()
    {
        EditorPrefs.SetBool(nameof(m_IsInEditSquareMode), m_IsInEditSquareMode);
        EditorPrefs.SetInt(nameof(m_CurrentEditSquareState), (int)m_CurrentEditSquareState);
        EditorPrefs.SetInt(nameof(m_CurrentSquareBrush), (int)m_CurrentSquareBrush);

        EditorPrefs.SetBool(nameof(m_IsInEditEdgeMode), m_IsInEditEdgeMode);
        EditorPrefs.SetBool(nameof(m_IsAddOrRemoveEdge), m_IsAddOrRemoveEdge);
    }
    #endregion
}