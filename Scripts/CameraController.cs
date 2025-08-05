using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rotación")]
    public float sensibilidadRotacion = 3.0f;
    public float minY = -60f;
    public float maxY = 60f;

    [Header("Zoom")]
    public float sensibilidadZoom = 10f;
    public float minZoom = 10f;
    public float maxZoom = 60f;

    [Header("Movimiento RTS")]
    public float velocidadMovimiento = 20f;
    public float bordePantalla = 10f; // píxeles desde el borde
    public float limiteX = 100f;
    public float limiteZ = 100f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private Camera camara;

    void Start()
    {
        camara = GetComponent<Camera>();
        if (camara == null)
            camara = Camera.main;
        Vector3 rot = transform.eulerAngles;
        rotacionX = rot.y;
        rotacionY = rot.x;
    }

    void Update()
    {
        // Rotación con botón central del mouse (rueda)
        if (Input.GetMouseButton(2))
        {
            rotacionX += Input.GetAxis("Mouse X") * sensibilidadRotacion;
            rotacionY -= Input.GetAxis("Mouse Y") * sensibilidadRotacion;
            rotacionY = Mathf.Clamp(rotacionY, minY, maxY);
            transform.rotation = Quaternion.Euler(rotacionY, rotacionX, 0f);
        }

        // Zoom con la rueda del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float nuevoZoom = camara.fieldOfView - scroll * sensibilidadZoom;
            camara.fieldOfView = Mathf.Clamp(nuevoZoom, minZoom, maxZoom);
        }

        // Movimiento tipo RTS solo con flechas y mouse en bordes
        Vector3 direccion = Vector3.zero;
        // Con flechas
        if (Input.GetKey(KeyCode.UpArrow))
            direccion += transform.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            direccion -= transform.forward;
        if (Input.GetKey(KeyCode.RightArrow))
            direccion += transform.right;
        if (Input.GetKey(KeyCode.LeftArrow))
            direccion -= transform.right;
        // Con mouse en bordes
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x >= 0 && mousePos.x < bordePantalla)
            direccion -= transform.right;
        if (mousePos.x <= Screen.width && mousePos.x > Screen.width - bordePantalla)
            direccion += transform.right;
        if (mousePos.y >= 0 && mousePos.y < bordePantalla)
            direccion -= transform.forward;
        if (mousePos.y <= Screen.height && mousePos.y > Screen.height - bordePantalla)
            direccion += transform.forward;
        // Movimiento
        direccion.y = 0f;
        Vector3 nuevaPos = transform.position + direccion.normalized * velocidadMovimiento * Time.deltaTime;
        // Limitar dentro de los bordes
        nuevaPos.x = Mathf.Clamp(nuevaPos.x, -limiteX, limiteX);
        nuevaPos.z = Mathf.Clamp(nuevaPos.z, -limiteZ, limiteZ);
        transform.position = nuevaPos;
    }
} 