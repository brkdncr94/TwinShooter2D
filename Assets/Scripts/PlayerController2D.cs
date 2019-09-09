using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour {

    public GameObject cam;
    public float speed;
    public float maxSpeed;
    public float knockbackIntensity;
    public string horizontalButton;
    public string fireButton;
    public Text hitCounterText;

    private Rigidbody2D rb;
    private Transform left_child;
    private Transform right_child;
    private LineRenderer left_laser;
    private LineRenderer right_laser;
    private Vector2 intersectPoint;
    private int obstaclesHit;
    

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();


        left_child = transform.GetChild(0);
        right_child = transform.GetChild(1);

        left_laser = left_child.GetComponent<LineRenderer>();
        right_laser = right_child.GetComponent<LineRenderer>();
        left_laser.material = new Material(Shader.Find("Unlit/Color"));
        right_laser.material = new Material(Shader.Find("Unlit/Color"));
        left_laser.material.color = Color.magenta;
        right_laser.material.color = Color.yellow;

        obstaclesHit = 0;
        SetHitCounter();
    }

    private void SetHitCounter()
    {
        hitCounterText.text = "Speed Handicap: " + obstaclesHit.ToString();
    }

    private void Shake()
    {
        // call the ScreenShake function from the CameraController
        cam.GetComponent<CameraController>().CamShake(0.8f, 1f);
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis(horizontalButton);
        //float moveVertical = Input.GetAxis("Vertical");

        //Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0f);
        Vector3 movement = new Vector3(moveHorizontal, 1.0f, 0f);

        if (obstaclesHit >= 0)
        {
            rb.AddForce(movement * speed / (1 + (obstaclesHit / 4)));
            if (rb.velocity.magnitude > (maxSpeed / (1 + (obstaclesHit / 20))))
            {
                rb.velocity = rb.velocity.normalized * (maxSpeed / (1 + (obstaclesHit / 20)));
            }
        }
        else
        {
            rb.AddForce(movement * speed * (1 - (obstaclesHit / 4)));
            if (rb.velocity.magnitude > (maxSpeed * (1 - (obstaclesHit / 20))))
            {
                rb.velocity = rb.velocity.normalized * (maxSpeed * (1 - (obstaclesHit / 20)));
            }
        }
    }

    private void LateUpdate()
    {
        if (Input.GetButtonDown(fireButton))
        {
            if (IsIntersecting())
            {
                Vector2 knockbackDir = (rb.position - intersectPoint).normalized;
                rb.AddForce(knockbackDir * knockbackIntensity);
                DestroyObstacle(intersectPoint, 0.5f);
            }
            else
            {
                Vector2 knockbackDir = Vector2.down;
                rb.AddForce(knockbackDir * knockbackIntensity);
            }
        }

    }

    private void DestroyObstacle(Vector2 center, float radius)
    {
        Collider2D hitCollider = Physics2D.OverlapCircle(center, radius);
        if (hitCollider != null && hitCollider.CompareTag("Obstacle"))
        {
            ParticleSystem ps = hitCollider.GetComponent<ParticleSystem>();
            hitCollider.GetComponent<Collider2D>().enabled = false;
            if (ps != null)
            {
                ps.Play();
            }
            Destroy(hitCollider.gameObject, ps.main.duration);
            cam.GetComponent<CameraController>().CamShake(1.5f, 1f);
            obstaclesHit = obstaclesHit - 1;
            SetHitCounter();
        }
    }


    private bool IsIntersecting()
    {
        // TAKEN FROM http://www.habrador.com/tutorials/math/5-line-line-intersection/

        bool isIntersecting = false;

        //3d -> 2d
        Vector3 leftOrig = left_laser.GetPosition(0);
        Vector3 leftEnd = left_laser.GetPosition(1);
        Vector3 rightOrig = right_laser.GetPosition(0);
        Vector3 rightEnd = right_laser.GetPosition(1);

        Vector2 l1_start = new Vector2(leftOrig.x, leftOrig.y);
        Vector2 l1_end = new Vector2(leftEnd.x, leftEnd.y);

        Vector2 l2_start = new Vector2(rightOrig.x, rightOrig.y);
        Vector2 l2_end = new Vector2(rightEnd.x, rightEnd.y);

        //Direction of the lines
        Vector2 l1_dir = (l1_end - l1_start).normalized;
        Vector2 l2_dir = (l2_end - l2_start).normalized;

        //If we know the direction we can get the normal vector to each line
        Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
        Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


        //Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
        //The normal vector is the A, B
        float A = l1_normal.x;
        float B = l1_normal.y;

        float C = l2_normal.x;
        float D = l2_normal.y;

        //To get k we just use one point on the line
        float k1 = (A * l1_start.x) + (B * l1_start.y);
        float k2 = (C * l2_start.x) + (D * l2_start.y);


        //Step 2: are the lines parallel? -> no solutions
        if (IsParallel(l1_normal, l2_normal))
        {
            Debug.Log("The lines are parallel so no solutions!");

            return isIntersecting;
        }


        //Step 3: calculate the intersection point -> one solution
        float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
        float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

        intersectPoint = new Vector2(x_intersect, y_intersect);


        //Step 4: but we have line segments so we have to check if the intersection point is within the segment
        if (IsBetween(l1_start, l1_end, intersectPoint) && IsBetween(l2_start, l2_end, intersectPoint))
        {
            Debug.Log("We have an intersection point!");

            Debug.Log("(x,y) = " + intersectPoint);

            isIntersecting = true;
        }

        return isIntersecting;
    }

    //Are 2 vectors parallel?
    private bool IsParallel(Vector2 v1, Vector2 v2)
    {
        // TAKEN FROM http://www.habrador.com/tutorials/math/5-line-line-intersection/

        //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
        if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
        {
            return true;
        }

        return false;
    }

    //Is a point c between 2 other points a and b?
    private bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
    {
        // TAKEN FROM http://www.habrador.com/tutorials/math/5-line-line-intersection/

        bool isBetween = false;

        //Entire line segment
        Vector2 ab = b - a;
        //The intersection and the first point
        Vector2 ac = c - a;

        //Need to check 2 things: 
        //1. If the vectors are pointing in the same direction = if the dot product is positive
        //2. If the length of the vector between the intersection and the first point is smaller than the entire line
        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
        {
            isBetween = true;
        }

        return isBetween;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Shake();
            obstaclesHit = obstaclesHit + 1;
            SetHitCounter();
            rb.velocity = new Vector3(0f, 0f, 0f);
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Finish"))
        {
            GameObject.FindGameObjectWithTag("Manager").GetComponent<ManageGame>().gameFinished = true;
            rb.velocity = new Vector3(0f, 0f, 0f);
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("hit a wall");
            Shake();
            obstaclesHit = obstaclesHit + 1;
            SetHitCounter();
            rb.velocity = new Vector3(0f, 0f, 0f);
            transform.Translate(0f, -2.5f, 0f);
        }
    }
}
