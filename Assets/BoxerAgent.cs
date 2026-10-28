using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BoxerAgent : Agent
{
    [Header("Training Targets")]
    public Transform targetOpponent; 
    
    [Header("Stats")]
    public float moveSpeed = 5f;
    public float punchRange = 1.5f;

    // NEW: Reference to the Animator (The Muscles)
    private Animator anim;
    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        // NEW: Find the Animator on the character
        anim = GetComponent<Animator>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset Position
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        this.transform.localRotation = Quaternion.identity;
        // Use Rigidbody.velocity (standard Unity API) and reset it on episode start
        rb.linearVelocity = Vector3.zero;

        // Reset Opponent
        targetOpponent.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetOpponent.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        // Observe the current Rigidbody velocity
        sensor.AddObservation(rb.linearVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Move the Rigidbody using MovePosition so physics is respected
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);

        //Update Animation Speed (for walking later)
        anim.SetFloat("Speed", move.magnitude);

        // PUNCH LOGIC
        float punchSignal = actions.ContinuousActions[2];
        
        // If the AI decides to punch (signal > 0.5)
        if (punchSignal > 0.5f)
        {
            // NEW: Tell the Animator to play the punch!
            anim.SetTrigger("Punch");
            
            ThrowPunch();
        }

        AddReward(-0.001f);
    }

    void ThrowPunch()
    {
        // Check distance to see if it connected
        float distance = Vector3.Distance(transform.position, targetOpponent.position);

        if (distance < punchRange)
        {
            AddReward(1.0f);
            EndEpisode(); 
        }
        else
        {
            AddReward(-0.05f); 
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }
}
