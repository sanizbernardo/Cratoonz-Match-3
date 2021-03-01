using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;
    private Rigidbody2D rb;

    private static GameObject selected;

    public string color;

    public int i;
    public int j;
    [SerializeField]
    private int targetHeight;

    [SerializeField]
    private bool falling = false;

    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (falling && Mathf.Abs(transform.position.y - targetHeight) < 0.05f) {
            falling = false;
            rb.gravityScale = 0.0f;

            transform.position = new Vector2(i, j);
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            gameManager.DropHasLanded();
        }
    }

    public void SetCoordinates(int first, int second) {
        i = first;
        j = second;
    }

    private void OnMouseDown()
    {
        if (selected)
        {
            if (selected == this)
            {
                return;
            }

            if (Vector2.Distance(transform.position, selected.transform.position) < 1.1f)
            {
                gameManager.Swap(this, selected.GetComponent<Drop>());
                selected = null;
            }
            else
            {
                selected = this.gameObject;
            }
        }
        else
        {
            selected = this.gameObject;
        }
    }

    public void Fall(int target) {
        targetHeight = target;
        j = target;

        rb.gravityScale = 1.0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        falling = true;
    }
}
