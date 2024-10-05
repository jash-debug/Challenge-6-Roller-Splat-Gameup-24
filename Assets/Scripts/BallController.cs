using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 15;
    private AudioSource playerAudio;
    public ParticleSystem dirtParticle;

    private bool isTravelling;
    private Vector3 travelDirection;
    private Vector3 nextCollisionPosition;

    public int minSwipeRecognition = 500;
    private Vector2 swipePositionLastFrame;
    private Vector2 swipePositionCurrentFrame;
    private Vector2 CurrentFrame;
    public AudioClip movingBall;

    private Color solveColor;

    // Start is called before the first frame update
    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        solveColor = Random.ColorHSV(0.5f, 1);
        GetComponent<MeshRenderer>().material.color = solveColor;
    }

    // Update is called once per frame
    void Update() { }

    private void FixedUpdate()
    {
        if (isTravelling)
        {
            rb.velocity = speed * travelDirection;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), 0.05f);
        foreach (var hitCollider in hitColliders)
        {
            GroundScript ground = hitCollider.transform.GetComponent<GroundScript>();
            if (ground && !ground.isColored)
            {
                ground.ChangeColor(solveColor);
            }
        }

        if (nextCollisionPosition != Vector3.zero)
        {
            if (Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTravelling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
            }
        }

        if (isTravelling) return;

        if (Input.GetMouseButton(0))
        {
            swipePositionCurrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if (swipePositionLastFrame != Vector2.zero)
            {
                CurrentFrame = swipePositionCurrentFrame - swipePositionLastFrame;

                if (CurrentFrame.sqrMagnitude < minSwipeRecognition)
                {
                    return;
                }

                CurrentFrame.Normalize();

                // Up/down
                if (CurrentFrame.x > -0.5f && CurrentFrame.x < 0.5)
                {
                    // Go up
                    Vector3 direction = CurrentFrame.y > 0 ? Vector3.forward : Vector3.back;
                    SetDestination(direction);
                    playerAudio.PlayOneShot(movingBall, 1.0f);
                    EmitDirtParticles(direction); // Emit particles in the opposite direction
                }

                // Left/right
                if (CurrentFrame.y > -0.5f && CurrentFrame.y < 0.5)
                {
                    Vector3 direction = CurrentFrame.x > 0 ? Vector3.right : Vector3.left;
                    SetDestination(direction);
                    playerAudio.PlayOneShot(movingBall, 1.0f);
                    EmitDirtParticles(direction); // Emit particles in the opposite direction
                }
            }
            swipePositionLastFrame = swipePositionCurrentFrame;
        }

        if (Input.GetMouseButtonUp(0))
        {
            swipePositionLastFrame = Vector2.zero;
            CurrentFrame = Vector2.zero;
        }
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTravelling = true;
    }

    private void EmitDirtParticles(Vector3 direction)
    {
        // Calculate the opposite direction for the particles
        Vector3 particleDirection = -direction;

        // Set the rotation to face the direction of emission
        dirtParticle.transform.rotation = Quaternion.LookRotation(particleDirection);

        // Play the dirt particle effect
        dirtParticle.Play();
    }
}
