using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AircraftController : MonoBehaviour
{
    [Header("綁定導演物件與障礙物陣列")]
    public GameDirector director;
    public Transform[] obstacles;

    [Header("三個音效 (.mp3 或 .wav)")]
    public AudioClip flySound;
    public AudioClip crashSound;
    public AudioClip passSound;

    [Header("飛行參數")]
    public float forwardSpeed = 6.0f;
    public float moveSpeed = 6.0f; // 上下左右移動的速度
    public float minHeight = 0.5f;
    public float maxHeight = 7.0f;
    public float minX = -5.0f;     // 左邊界
    public float maxX = 5.0f;      // 右邊界
    public float passOffsetZ = 2.0f;

    private int state = 0; // 0: 待命, 1: 飛行中, 2: 結束
    private bool[] passedFlags;
    private int passedCount = 0;

    private AudioSource flyAudio;
    private AudioSource sfxAudio;
    private Rigidbody rb;
    private Camera mainCam;
    private float camOffsetZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        flyAudio = gameObject.AddComponent<AudioSource>();
        flyAudio.playOnAwake = false;
        flyAudio.loop = true;

        sfxAudio = gameObject.AddComponent<AudioSource>();
        sfxAudio.playOnAwake = false;
        sfxAudio.loop = false;

        if (obstacles != null) passedFlags = new bool[obstacles.Length];

        mainCam = Camera.main;
        if (mainCam != null) camOffsetZ = mainCam.transform.position.z - transform.position.z;
    }

    void Update()
    {
        if (state == 0 && CheckStartInput())
        {
            state = 1;
            if (flySound != null)
            {
                flyAudio.clip = flySound;
                flyAudio.Play();
            }
            if (director != null) director.OnStartFly((obstacles != null) ? obstacles.Length : 0);
        }
        else if (state == 1)
        {
            Vector3 nextPos = transform.position;
            nextPos.z += forwardSpeed * Time.deltaTime; // 飛機自動往前飛

            // 取得玩家的上下左右輸入
            Vector2 moveInput = GetMoveInput();
            nextPos.x += moveInput.x * moveSpeed * Time.deltaTime;
            nextPos.y += moveInput.y * moveSpeed * Time.deltaTime;

            // 限制飛機不要飛出畫面邊界
            nextPos.x = Mathf.Clamp(nextPos.x, minX, maxX);
            nextPos.y = Mathf.Clamp(nextPos.y, minHeight, maxHeight);

            transform.position = nextPos;
            CheckPassedObstacles();
        }
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            Vector3 camPos = mainCam.transform.position;
            camPos.z = transform.position.z + camOffsetZ;
            mainCam.transform.position = camPos;
        }
    }

    private void CheckPassedObstacles()
    {
        if (obstacles == null || obstacles.Length == 0) return;
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (!passedFlags[i] && obstacles[i] != null)
            {
                if (transform.position.z > obstacles[i].position.z + passOffsetZ)
                {
                    passedFlags[i] = true;
                    passedCount++;
                    if (passSound != null) sfxAudio.PlayOneShot(passSound);
                    if (director != null) director.OnPassObstacle(passedCount, obstacles.Length);

                    if (passedCount >= obstacles.Length)
                    {
                        state = 2;
                        rb.isKinematic = true;
                        flyAudio.Stop();
                        if (director != null) director.OnWin(obstacles.Length);
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision) { if (state == 1 && IsObstacle(collision.transform)) HandleCrash(); }
    void OnTriggerEnter(Collider other) { if (state == 1 && IsObstacle(other.transform)) HandleCrash(); }

    private bool IsObstacle(Transform hitTransform)
    {
        if (obstacles == null || obstacles.Length == 0) return true;
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (obstacles[i] != null && (hitTransform == obstacles[i] || hitTransform.IsChildOf(obstacles[i]))) return true;
        }
        return false;
    }

    private void HandleCrash()
    {
        state = 2;
        rb.isKinematic = true;
        flyAudio.Stop();
        if (crashSound != null) sfxAudio.PlayOneShot(crashSound);
        if (director != null) director.OnCrash();
    }

    private bool CheckStartInput()
    {
#if ENABLE_INPUT_SYSTEM
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
#else
        return Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
#endif
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
        }
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            input = Touchscreen.current.primaryTouch.delta.ReadValue().normalized;
        }
#else
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved) input = t.deltaPosition.normalized;
        }
#endif
        return input;
    }
}