using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapEditor : Editor
{
    private MapManager m_CurrentMapManager;
    private bool m_ShowMapView = false;
    //Edit Square
    private bool m_IsInSquareEdit = false;
    private SquareState m_SquareStateModeEdit = SquareState.Normal;
    //Edit Brush
    private bool m_IsInEditBrush = false;
    private BrushState m_BrushStateModeEdit = BrushState.One;

    //Edit Edges
    private bool m_IsInEdgesEdit = false;
    private bool m_IsEdgesActivate = false;
    //Edit EdgesHorizontal
    private bool m_EditEdgesHor = false;
    //Edit EdgesVerticale
    private bool m_EditEdgesVert = false;



    public void OnEnable()
    {
        m_CurrentMapManager = (MapManager)target;
        LoadEditorState();
    }
    public void OnDisable()
    {
        SaveEditorState();
    }
    private void SaveEditorState()
    {
        EditorPrefs.SetBool(nameof(m_IsInSquareEdit), m_IsInSquareEdit);
        EditorPrefs.SetInt(nameof(m_SquareStateModeEdit), (int)m_SquareStateModeEdit);
        EditorPrefs.SetBool(nameof(m_IsInEditBrush), m_IsInEditBrush);
        EditorPrefs.SetInt(nameof(m_BrushStateModeEdit), (int)m_BrushStateModeEdit);
        EditorPrefs.SetBool(nameof(m_IsInEdgesEdit), m_IsInEdgesEdit);
        EditorPrefs.SetBool(nameof(m_ShowMapView), m_ShowMapView);
    }
    void LoadEditorState()
    {
        m_IsInSquareEdit = EditorPrefs.GetBool(nameof(m_IsInSquareEdit));
        m_SquareStateModeEdit = (SquareState)EditorPrefs.GetInt(nameof(m_SquareStateModeEdit));
        m_IsInEditBrush = EditorPrefs.GetBool(nameof(m_IsInEditBrush));
        m_BrushStateModeEdit = (BrushState)EditorPrefs.GetInt(nameof(m_BrushStateModeEdit));
        m_IsInEdgesEdit = EditorPrefs.GetBool(nameof(m_IsInEdgesEdit));
        m_ShowMapView = EditorPrefs.GetBool(nameof(m_ShowMapView));

    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label("=========== Map Editor =========", EditorStyles.boldLabel);
        if (GUILayout.Button("Initialize Map Randomly"))
        {
            m_CurrentMapManager.InitializeMapRandomly();
        }

        GUILayout.Space(10);
        m_ShowMapView = GUILayout.Toggle(m_ShowMapView, "Show View");
        m_CurrentMapManager.navContainer.SetActive(m_ShowMapView);

        if (GUILayout.Button("Initialize Map Empty"))
        {
            m_CurrentMapManager.InitializeEmptyMap();
        }

        GUILayout.Label("=========== Edit Square =========", EditorStyles.boldLabel);
        m_IsInSquareEdit = GUILayout.Toggle(m_IsInSquareEdit, " Edit Mode");
        if ((m_IsInSquareEdit) && (!m_IsInEdgesEdit))
        {
            m_SquareStateModeEdit = (SquareState)EditorGUILayout.EnumPopup(m_SquareStateModeEdit);
        }
        else
        {
            m_IsInSquareEdit = false;

            m_IsInEditBrush = GUILayout.Toggle(m_IsInEditBrush, "Edit Brush");
            if (m_IsInEditBrush)
            {
                m_BrushStateModeEdit = (BrushState)EditorGUILayout.EnumPopup(m_BrushStateModeEdit);
            }

            GUILayout.Label("=========== Edit Edges =========", EditorStyles.boldLabel);
            m_IsInEdgesEdit = GUILayout.Toggle(m_IsInEdgesEdit, "Edit Modes");


            if (m_IsInEdgesEdit)
            {
                m_IsEdgesActivate = GUILayout.Toggle(m_IsEdgesActivate, "Edit Edges Statu");
            }

            if ((m_IsInEdgesEdit) && (m_IsInEdgesEdit))
            {
                m_EditEdgesHor = GUILayout.Toggle(m_EditEdgesHor, "Edit Edges Hor");
                m_EditEdgesVert = GUILayout.Toggle(m_EditEdgesVert, "Edit Edges Vert");
            }

            GUILayout.Label("=========== Map Properties =========");
            base.OnInspectorGUI();

        }

        {
            //Edit Squares
            if (m_IsInSquareEdit)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;

                Vector3 posIntersection = CalculateInteractionPositionPlan();

                Vector3 posInt = new Vector3((int)posIntersection.x, 0, (int)posIntersection.z);


                if (posIntersection.x >= 0 && posIntersection.x < m_CurrentMapManager.mapData.width
                    && posIntersection.z >= 0 && posIntersection.z < m_CurrentMapManager.mapData.height)
                {
                    DisplayGizmosSquare(posInt);
                    EditSquareState(posInt);

                }

                if (m_SquareStateModeEdit == SquareState.Lock)
                {
                    EditSquareState(posInt);
                }
                if (m_BrushStateModeEdit == BrushState.Line)
                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 0));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(1, 0, 0));


                }
                if (m_BrushStateModeEdit == BrushState.Square)
                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 0));
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 1));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(1, 0, 0));
                    EditSquareState(posInt + new Vector3(0, 0, 1));
                    EditSquareState(posInt + new Vector3(1, 0, 1));


                }
                if (m_BrushStateModeEdit == BrushState.Cross)

                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 0));
                    DisplayGizmosSquare(posInt + new Vector3(-1, 0, 0));
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, -1));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(1, 0, 0));
                    EditSquareState(posInt + new Vector3(-1, 0, 0));
                    EditSquareState(posInt + new Vector3(0, 0, 1));
                    EditSquareState(posInt + new Vector3(0, 0, -1));

                }
                if (m_BrushStateModeEdit == BrushState.L)
                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 0));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(0, 0, 1));
                    EditSquareState(posInt + new Vector3(1, 0, 0));

                }
                if (m_BrushStateModeEdit == BrushState.T)
                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(-1, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 1));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(0, 0, 1));
                    EditSquareState(posInt + new Vector3(0, 0, 1));
                    EditSquareState(posInt + new Vector3(-1, 0, 1));
                    EditSquareState(posInt + new Vector3(1, 0, 1));

                }
                if (m_BrushStateModeEdit == BrushState.Heart)
                {
                    DisplayGizmosSquare(posInt);
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(-1, 0, 1));
                    DisplayGizmosSquare(posInt + new Vector3(2, 0, 2));
                    DisplayGizmosSquare(posInt + new Vector3(-2, 0, 2));
                    DisplayGizmosSquare(posInt + new Vector3(2, 0, 3));
                    DisplayGizmosSquare(posInt + new Vector3(-2, 0, 3));
                    DisplayGizmosSquare(posInt + new Vector3(1, 0, 4));
                    DisplayGizmosSquare(posInt + new Vector3(-1, 0, 4));
                    DisplayGizmosSquare(posInt + new Vector3(0, 0, 3));
                    EditSquareState(posInt);
                    EditSquareState(posInt + new Vector3(1, 0, 1));
                    EditSquareState(posInt + new Vector3(-1, 0, 1));
                    EditSquareState(posInt + new Vector3(2, 0, 2));
                    EditSquareState(posInt + new Vector3(-2, 0, 2));
                    EditSquareState(posInt + new Vector3(2, 0, 3));
                    EditSquareState(posInt + new Vector3(-2, 0, 3));
                    EditSquareState(posInt + new Vector3(1, 0, 4));
                    EditSquareState(posInt + new Vector3(-1, 0, 4));
                    EditSquareState(posInt + new Vector3(0, 0, 3));

                }

            }

            //edit Edges 
            if (m_IsInEdgesEdit)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Tools.current = Tool.None;
                Vector3 posIntersection = CalculateInteractionPositionPlan();
                Vector3 posInt = new Vector3((int)posIntersection.x, 0, (int)posIntersection.z);
                bool[] edgesVerti = m_CurrentMapManager.mapData.edgesVert;
                int verti = m_CurrentMapManager.mapData.width + 1;
                bool[] edgesHori = m_CurrentMapManager.mapData.edgesHori;
                int hori = m_CurrentMapManager.mapData.width;

                //Edit EdgesVertical.
                if (posIntersection.x >= 0 && posIntersection.x < m_CurrentMapManager.mapData.width + 1
                   && posIntersection.z >= 0 && posIntersection.z < m_CurrentMapManager.mapData.height)
                {
                    if (m_EditEdgesVert && m_IsInEditBrush && !m_EditEdgesHor && !m_IsEdgesActivate)
                    {
                        DisplayGizmosEdgesVert(posInt);
                        EditEdgesVertiState(posInt);
                        m_CurrentMapManager.ProcessRuleSquareVSEdge(edgesVerti, verti, new Vector3(-1, 0, 0));
                    }
                    if ((m_IsEdgesActivate) && (m_EditEdgesVert))
                    {
                        DisplayGizmosEdgesVert(posInt);
                        DeleteEdgesVertState(posInt);
                    }
                }

                //Edit EdgesHorizontale
                if (posIntersection.x >= 0 && posIntersection.x < m_CurrentMapManager.mapData.width
                    && posIntersection.z >= 0 && posIntersection.z < m_CurrentMapManager.mapData.height + 1)
                {
                    if (((m_EditEdgesHor) && (!m_EditEdgesVert) && (!m_IsEdgesActivate)))
                    {
                        DisplayGizmosEdgesHor(posInt);
                        EditEdgesHoriState(posInt);
                        m_CurrentMapManager.ProcessRuleSquareVSEdge(edgesHori, hori, new Vector3(0, 0, -1));
                    }
                    if ((m_IsEdgesActivate) && (m_EditEdgesHor))
                    {
                        DisplayGizmosEdgesHor(posInt);
                        DeleteEdgesHoriState(posInt);
                    }
                }
            }

            SceneView.RepaintAll();
        }


        void EditEdgesHoriState(Vector3 posInt)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        int index = m_CurrentMapManager.GetIndexEdgesHoriFromPos(posInt);
                        m_CurrentMapManager.mapData.edgesHori[index] = true;
                        break;
                    case EventType.MouseUp:
                        m_CurrentMapManager.CreateMapView();
                        break;
                }
            }
        }
        void DeleteEdgesHoriState(Vector3 posInt)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        int index = m_CurrentMapManager.GetIndexEdgesHoriFromPos(posInt);
                        m_CurrentMapManager.mapData.edgesHori[index] = false;
                        break;
                    case EventType.MouseUp:
                        m_CurrentMapManager.CreateMapView();
                        break;
                }
            }
        }

        void EditEdgesVertiState(Vector3 posInt)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        int index = m_CurrentMapManager.GetIndexEdgesVertiFromPos(posInt);
                        m_CurrentMapManager.mapData.edgesVert[index] = true;

                        break;
                    case EventType.MouseUp:
                        m_CurrentMapManager.CreateMapView();
                        break;
                }
            }

        }
        void DeleteEdgesVertState(Vector3 posInt)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        int index = m_CurrentMapManager.GetIndexEdgesVertiFromPos(posInt);
                        m_CurrentMapManager.mapData.edgesVert[index] = false;
                        break;
                    case EventType.MouseUp:
                        m_CurrentMapManager.CreateMapView();
                        break;
                }
            }

        }
        void EditSquareState(Vector3 posInt)
        {
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        int index = m_CurrentMapManager.GetIndexSquareFromPos(posInt);
                        m_CurrentMapManager.mapData.grid[index].state = m_SquareStateModeEdit;
                        DeleteEdgesVertState(posInt);
                        DeleteEdgesVertState(posInt + new Vector3(1, 0, 0));
                        DeleteEdgesHoriState(posInt);
                        DeleteEdgesHoriState(posInt + new Vector3(0, 0, 1));
                        break;
                    case EventType.MouseUp:
                        m_CurrentMapManager.CreateMapView();
                        break;
                }
            }
        }

        Vector3 CalculateInteractionPositionPlan()
        {
            Vector2 pos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(pos);

            Plane plan = new Plane(Vector3.up, Vector3.zero);

            if (plan.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return (Vector3.zero);

        }
        void DisplayGizmosSquare(Vector3 ponInt)
        {
            Handles.color = m_CurrentMapManager.GetColorFromState(m_SquareStateModeEdit);
            Vector3 sizeWireSqaure = Vector3.one;
            ponInt.x += 0.5f;
            ponInt.z += 0.5f;
            sizeWireSqaure.y = 0.25f;
            Handles.DrawWireCube(ponInt, sizeWireSqaure);

        }
        void DisplayGizmosEdgesVert(Vector3 ponInt)
        {
            Handles.color = Color.cyan;
            Vector3 sizeWireEdge = Vector3.one;
            ponInt.x += 0f;
            ponInt.z += 0.5f;
            sizeWireEdge.x = 0.25f;
            Handles.DrawWireCube(ponInt, sizeWireEdge);

        }
        void DisplayGizmosEdgesHor(Vector3 ponInt)
        {
            Handles.color = Color.green;
            Vector3 sizeWireEdge = Vector3.one;
            ponInt.x += 0.5F;
            ponInt.z += 0;
            sizeWireEdge.z = 0.25f;
            Handles.DrawWireCube(ponInt, sizeWireEdge);

        }

    }
}