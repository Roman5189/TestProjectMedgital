using RuntimeHandle;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SpawnController : MonoBehaviour
{
    public Camera cam;
    public GameObject pointPrefab;
    public GameObject spherePrefab;
    public InputActionAsset mapActions;
    public UIDocument uIDocument;
    public RuntimeTransformHandle runtimeTransformHandle;

    private InputAction mouseButtonLeft;
    private InputAction mouseButtonRight;
    private VisualElement visualElement;

    private Label textRadius;
    private Button spawnStart;
    private Button cancel;
    private Button move;
    private Button rotate;
    private Button blue,yellow,green,red;
    private Slider slider;

    private Transform spawnObj;
    private Transform spawnPoint;   
    private Material matSpawnObj;
    private float cameraAxisX;
    private float cameraAxisY;
    private float cameraAxisZ;
    [HideInInspector]
    public bool spawnMode;
    public static bool openSpawnManager;
    private void Awake()
    {
        ConfigurateButtons();
        uIDocument.gameObject.SetActive(false);
        uIDocument.enabled = true;
    }
    private void ConfigurateButtons() 
    {   
        mouseButtonLeft = mapActions.FindAction("MouseClickLeft");
        mouseButtonRight = mapActions.FindAction("MouseClickRight");

        mouseButtonLeft.performed += context => SpawnObject();
        mouseButtonRight.performed += context => UndoSpawn();

        visualElement = uIDocument.rootVisualElement;

        textRadius = visualElement.Q<Label>("Radius");

        spawnStart = visualElement.Q<Button>("Spawn");
        cancel = visualElement.Q<Button>("Cancel");
        move = visualElement.Q<Button>("Move");
        rotate = visualElement.Q<Button>("Rotate");

        blue = visualElement.Q<Button>("Blue");
        yellow = visualElement.Q<Button>("Yellow");
        green = visualElement.Q<Button>("Green");
        red = visualElement.Q<Button>("Red");

        slider = visualElement.Q<Slider>("Transparency");

        spawnStart.clicked += () => SpawnMode(true);
        cancel.clicked += () => SpawnMode(false);
        move.clicked += () => ChangeHandle(0);
        rotate.clicked += () => ChangeHandle(1);

        blue.clicked += () => ChangeColor(Color.blue);
        yellow.clicked += () => ChangeColor(Color.yellow);
        green.clicked += () => ChangeColor(Color.green);
        red.clicked += () => ChangeColor(Color.red);

        EnableButtons(false);
    }

    public static void OpenSpawnManager(bool state) 
    {
        openSpawnManager = true;
    }

    public void SetCamera(Camera camera) 
    {
        cam = camera;
        runtimeTransformHandle.handleCamera = cam;
    }
    private void OnEnable()
    {
        mouseButtonLeft.Enable();
        mouseButtonRight.Enable();
    }

    private void OnDisable()
    {
        mouseButtonLeft.Disable();
        mouseButtonRight.Disable();
    }
    private void SpawnMode(bool state) 
    {
        spawnMode = state;
        spawnObj = null;
        openSpawnManager = state;
        uIDocument.gameObject.SetActive(state);
        if (!state) runtimeTransformHandle.Clear();
        if (state) ConfigurateButtons();
    }

    private void UndoSpawn() 
    {
        if (spawnPoint)
            Destroy(spawnPoint.gameObject);
    }
    private void ChangeHandle(int state) 
    {
        runtimeTransformHandle.SwitchsHandle(state);
    }
    private void ChangeColor(Color color) 
    {
        spawnObj.GetComponent<Renderer>().material.color = color;
    }
    private void ChangeTransparency(float transparency)
    {
        Color color= spawnObj.GetComponent<Renderer>().material.color;
        spawnObj.GetComponent<Renderer>().material.color = new Vector4(color.r,color.g,color.b,transparency);
    }
    private void EnableButtons(bool state) 
    {
        move.SetEnabled(state);
        rotate.SetEnabled(state);

        blue.SetEnabled(state);
        yellow.SetEnabled(state);
        green.SetEnabled(state);
        red.SetEnabled(state);

        slider.SetEnabled(state);
    }
    private void RenderLine(Transform startPoint) 
    {
        if (!cam || cam.name == "Camera")
            return;
        
        var mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //Vector3 targetPoint = new Vector3(mousePos.x, mousePos.y, cameraAxisZ);
        Vector3 targetPoint = PointPos();
        startPoint.GetComponent<LineRenderer>().SetPosition(0, startPoint.position);
        startPoint.GetComponent<LineRenderer>().SetPosition(1, targetPoint);
        textRadius.text = Vector3.Distance(startPoint.position, targetPoint).ToString();
    }

    Vector3 PointPos() 
    {
        var mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (cam.transform.eulerAngles.y == 180)
        {
            cameraAxisX = mousePos.x;
            cameraAxisZ = mousePos.z - 1;

        }
        if (cam.transform.eulerAngles.y == 270) 
        {
            cameraAxisX = mousePos.x - 1;
            cameraAxisZ = mousePos.z;
        }

        if (cam.transform.eulerAngles.y == 0) 
        {
            cameraAxisX = mousePos.x;
            cameraAxisZ = mousePos.z + 1;
        }

        if (cam.transform.eulerAngles.x != 90)
            cameraAxisY = mousePos.y;
        if (cam.transform.eulerAngles.x == 90)
        {
            cameraAxisY = mousePos.y - 1f;
            cameraAxisZ = mousePos.z;
        }


        if (cam.transform.eulerAngles.y == 270)
            cameraAxisX = mousePos.x - 1;

        return new Vector3(cameraAxisX, cameraAxisY, cameraAxisZ);
    }
    public void SpawnObject()
    {
        //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //for old Input System

        if (cam && !spawnObj && cam.name != "Camera")
        {
            //var mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (!spawnPoint && spawnMode)
            {
                spawnPoint = Instantiate(pointPrefab, PointPos(), Quaternion.Euler(0, 0, 0)).transform;
                return;
            }
            if (spawnPoint)
            {
                spawnObj = Instantiate(spherePrefab, spawnPoint.position, Quaternion.Euler(0, 0, 0)).transform;
                float radius = Vector3.Distance(spawnPoint.position, PointPos());
                spawnObj.transform.localScale = Vector3.one * radius * 2;
                matSpawnObj = spawnObj.GetComponent<Renderer>().material;
                runtimeTransformHandle.handleCamera = cam;
                runtimeTransformHandle.target = spawnObj.transform;
                runtimeTransformHandle.enabled = true;
                //spawnMode = false;
                Destroy(spawnPoint.gameObject);
                EnableButtons(true);
                return;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnPoint)
            RenderLine(spawnPoint);

        if (spawnObj && (float)System.Math.Round(matSpawnObj.color.a, 1) != (float)System.Math.Round(slider.value / 100, 1))
            ChangeTransparency(slider.value / 100);

        if (openSpawnManager && !spawnMode)
            SpawnMode(true);
    }
}
