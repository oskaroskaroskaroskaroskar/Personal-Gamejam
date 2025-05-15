using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    AutoRunPlayer autoRunPlayer;

    public float jumpForce = 50f;
    private Rigidbody2D rb;
    public bool isGrounded;
    private float slideTimer;
    bool isSliding = false;

    private Animator animator;


    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public BoxCollider2D playerCollider;
    private Vector2 originalColliderSize;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private CapsuleCollider2D fallCheck;

    float lastTouchTime = 0f;
    float doubleTouchTime = 0.2f;
    bool doubleTouch = false;

    private bool megaMode = false;
    public bool MegaMode
    {  get { return megaMode; }
    set {megaMode = value; }
    }
    float megaModeCoolDown = 1f;
    float megaModeCoolDownTime = 0f;
    float megaModeDuration = 5f;
    float megaModeDurationTime = 0f;
    float scaleEnhancer = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        autoRunPlayer = GetComponent<AutoRunPlayer>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
        originalColliderSize.y = 0.64f;
        playerCollider.size = originalColliderSize;


        fallCheck = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float swipeThreshold = 50f; // Adjust for sensitivity

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (lastTouchTime < doubleTouchTime && touch.phase == TouchPhase.Began)
            {
                doubleTouch = true;
            } else
            {
                doubleTouch = false;
            }
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchTime = 0f; // Reset lastTouchTime
            }
               

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                Vector2 swipeDirection = endTouchPosition - startTouchPosition;

                if (Mathf.Abs(swipeDirection.y) > swipeThreshold && Mathf.Abs(swipeDirection.y) > Mathf.Abs(swipeDirection.x))
                {
                    if (swipeDirection.y > 0 && isGrounded)
                    {
                        // Swipe Up - Jump
                        Jump();
                    }
                    else if (swipeDirection.y < 0 && isGrounded && !isSliding)
                    {
                        // Swipe Down - Slide
                        Slide();
                    }
                }
            }

        if (isSliding == true)
        {
            animator.SetBool("isSliding", true);
        }
        else
        {
            animator.SetBool("isSliding", false);
        }

        }
            if(Input.GetKey(KeyCode.W) && isGrounded)
        {
            Jump();
        }
        if (Input.GetKey(KeyCode.S) && isGrounded && !isSliding)
        {
            Slide();
        }


        if (doubleTouch && !megaMode)
        {
            if (megaModeCoolDownTime > megaModeCoolDown)
            {
                EnableMegaMode();

            }
        }
        if (megaMode)
        {
            megaModeDurationTime += Time.deltaTime;
            if (megaModeDurationTime > megaModeDuration)
            {
                DisableMegaMode();
                megaModeCoolDownTime = 0f;
                megaModeDurationTime = 0f;
            }
        }
        else
        {
            megaModeCoolDownTime += Time.deltaTime;
        }
        lastTouchTime += Time.deltaTime;

    }
    void EnableMegaMode ()
    {
        Vector3 cameraPostion = Camera.main.transform.position;
        megaMode = true;
        transform.localScale = transform.localScale * scaleEnhancer;
        transform.position = new Vector2(transform.position.x-scaleEnhancer/2, transform.position.y +scaleEnhancer/2);
        Camera.main.transform.position = cameraPostion;

    }
    void DisableMegaMode()
    {
        Vector3 cameraPostion = Camera.main.transform.position;
        megaMode = false;
        transform.localScale = transform.localScale / scaleEnhancer;
        transform.position = new Vector2(transform.position.x + scaleEnhancer / 2, transform.position.y - scaleEnhancer / 2);
        Camera.main.transform.position = cameraPostion;
    }
    void Jump ()
    {
        Debug.Log("jump");
        rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        //rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        SoundManager.Instance.PlayPlayerSound("PlayerJump");
        
    }
    void Slide()
    {

        StartCoroutine(SlideCoroutine());
        SoundManager.Instance.PlayPlayerSound("PlayerSlide");
    }

    private System.Collections.IEnumerator SlideCoroutine()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);

        Vector2 slideSize = playerCollider.size;
        slideSize.y = 0.15f;
        playerCollider.size = slideSize;

        yield return new WaitForSeconds(2f);
       
        playerCollider.size = originalColliderSize;
        isSliding = false;
        animator.SetBool("isSliding", false);

        if (megaMode)
        {
            transform.position += new Vector3(0f, scaleEnhancer/2f,0);
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            AutoRunPlayer.moveSpeed = 0f;
        }

        if (collision.CompareTag("Restart"))
        {
            SoundManager.Instance.PlayEnemySound("dying");
            GameManager.Instance.LoadScene("SampleScene");
        }
      
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Obstacle" && megaMode)
        {
            Destroy(collision.gameObject);
        }
    }
}
