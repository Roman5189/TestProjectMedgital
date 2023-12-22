using RuntimeHandle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SpawnController : MonoBehaviour
{    
    public GameObject pointPrefab;
    public GameObject spherePrefab;
    public InputActionAsset mapActions;
    public UIDocument uIDocument;
    public RuntimeTransformHandle runtimeTransformHandle;

    private InputAction mouseButtonLeft;
    private InputAction mouseButtonRight;
    private VisualElement visualElement;

    private Label textRadius;
    private Button spawn;
    private Button move;
    private Button rotate;
    private Button blue,yellow,green,red;
    private Slider slider;

    private Transform spawnObj;
    private Transform spawnPoint;   
    private Material matSpawnObj;
    private bool spawnMode;

    void Awake()
    {
        ConfigurateButtons();
    }

    private void ConfigurateButtons() 
    {   
        mouseButtonLeft = mapActions.FindAction("MouseClickLeft");
        mouseButtonRight = mapActions.FindAction("MouseClickRight");

        mouseButtonLeft.performed += context => SpawnObject();
        mouseButtonRight.performed += context => CancelSpawnMode();

        visualElement = uIDocument.rootVisualElement;

        textRadius = visualElement.Q<Label>("Radius");

        spawn = visualElement.Q<Button>("Spawn");
        move = visualElement.Q<Button>("Move");
        rotate = visualElement.Q<Button>("Rotate");

        blue = visualElement.Q<Button>("Blue");
        yellow = visualElement.Q<Button>("Yellow");
        green = visualElement.Q<Button>("Green");
        red = visualElement.Q<Button>("Red");

        slider = visualElement.Q<Slider>("Transparency");

        spawn.clicked += () => SpawnMode();
        move.clicked += () => ChangeHandle(0);
        rotate.clicked += () => ChangeHandle(1);

        blue.clicked += () => ChangeColor(Color.blue);
        yellow.clicked += () => ChangeColor(Color.yellow);
        green.clicked += () => ChangeColor(Color.green);
        red.clicked += () => ChangeColor(Color.red);

        EnableButtons(false);
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
    private void SpawnMode() 
    {
        spawnMode = true;
        spawn.SetEnabled(false);
    }  
    private void CancelSpawnMode() 
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
        var mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 targetPoint = new Vector3(mousePos.x, mousePos.y, 0);
        startPoint.GetComponent<LineRenderer>().SetPosition(0, startPoint.position);
        startPoint.GetComponent<LineRenderer>().SetPosition(1, targetPoint);
        textRadius.text = Vector3.Distance(startPoint.position, targetPoint).ToString();
    }
    private void SpawnObject()
    {
        //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //for old Input System
        var mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (!spawnPoint && spawnMode)
        {
            spawnPoint = Instantiate(pointPrefab, new Vector3(mousePos.x, mousePos.y, 0), Quaternion.Euler(0, 0, 0)).transform;
            return;
        }
        if (spawnPoint)
        {
            spawnObj = Instantiate(spherePrefab, spawnPoint.position, Quaternion.Euler(0, 0, 0)).transform;
            float radius = Vector3.Distance(spawnPoint.position, new Vector3(mousePos.x, mousePos.y, 0));
            spawnObj.transform.localScale = Vector3.one * radius * 2;
            matSpawnObj = spawnObj.GetComponent<Renderer>().material;
            runtimeTransformHandle.target = spawnObj.transform;
            runtimeTransformHandle.enabled = true;
            spawnMode = false;            
            Destroy(spawnPoint.gameObject);
            EnableButtons(true);
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnPoint)
            RenderLine(spawnPoint);

        if (spawnObj && (float)System.Math.Round(matSpawnObj.color.a, 1) != (float)System.Math.Round(slider.value / 100, 1))
            ChangeTransparency(slider.value / 100);
    }
}
