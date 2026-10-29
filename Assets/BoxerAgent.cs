using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BoxerAgent : Agent
{
    [Header("Opponent & Ring Target")]
    public Transform targetOpponent;
    public BoxerAgent opponentBoxerAgent;

    [Header("Stats")]
    public float moveSpeed = 4.5f;
    public float turnSpeed = 200f;
    public float punchRange = 1.6f;
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    [Header("Combat Cooldowns & State")]
    public float jabDamage = 15f;
    public float comboDamage = 25f;
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    public bool isBlocking { get; private set; } = false;

    private Animator anim;
    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        if (targetOpponent != null && opponentBoxerAgent == null)
        {
            opponentBoxerAgent = targetOpponent.GetComponent<BoxerAgent>();
        }
    }

    public override void OnEpisodeBegin()
    {
        currentHealth = maxHealth;
        isBlocking = false;
        nextAttackTime = 0f;

        // Reset Physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Spawn position & facing
        // If targetOpponent exists, spawn symmetrically
        if (targetOpponent != null)
        {
            float side = transform.position.x <= targetOpponent.position.x ? -1f : 1f;
            transform.localPosition = new Vector3(side * Random.Range(2f, 3.5f), 0.5f, Random.Range(-1.5f, 1.5f));
            Vector3 lookDir = (targetOpponent.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        else
        {
            transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f));
            transform.localRotation = Quaternion.identity;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (targetOpponent == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(currentHealth / maxHealth);
            sensor.AddObservation(isBlocking ? 1f : 0f);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(rb != null ? rb.linearVelocity : Vector3.zero);
            return;
        }

        // Relative vector to opponent in local coordinates
        Vector3 toOpponent = targetOpponent.position - transform.position;
        Vector3 localToOpponent = transform.InverseTransformDirection(toOpponent);

        sensor.AddObservation(localToOpponent); // (x, y, z relative)
        sensor.AddObservation(toOpponent.magnitude); // Distance

        // Angle between self forward and opponent
        float angleToOpponent = Vector3.SignedAngle(transform.forward, toOpponent, Vector3.up) / 180f;
        sensor.AddObservation(angleToOpponent);

        // Self and Opponent Combat States
        sensor.AddObservation(currentHealth / maxHealth);
        sensor.AddObservation(isBlocking ? 1f : 0f);

        if (opponentBoxerAgent != null)
        {
            sensor.AddObservation(opponentBoxerAgent.currentHealth / opponentBoxerAgent.maxHealth);
            sensor.AddObservation(opponentBoxerAgent.isBlocking ? 1f : 0f);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        sensor.AddObservation(rb != null ? transform.InverseTransformDirection(rb.linearVelocity) : Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous actions: [0] = moveX (strafe), [1] = moveZ (forward/back), [2] = rotate (turn)
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotate = actions.ContinuousActions[2];

        // Discrete actions: [0] = Combat Action (0: None, 1: Lead Jab, 2: Jab Cross Combo, 3: Guard/Block)
        int combatAction = actions.DiscreteActions.Length > 0 ? actions.DiscreteActions[0] : 0;

        // Rotation
        transform.Rotate(Vector3.up, rotate * turnSpeed * Time.deltaTime);

        // Movement (slower while blocking)
        float speedMultiplier = isBlocking ? 0.4f : 1.0f;
        Vector3 move = (transform.forward * moveZ + transform.right * moveX) * moveSpeed * speedMultiplier * Time.deltaTime;
        if (rb != null)
        {
            rb.MovePosition(rb.position + move);
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", move.magnitude);
        }

        // Process Combat Action
        HandleCombatAction(combatAction);

        // Small step existence penalty to encourage decisive action
        AddReward(-0.0005f);

        // Reward facing the opponent
        if (targetOpponent != null)
        {
            Vector3 toOpp = (targetOpponent.position - transform.position).normalized;
            float facingDot = Vector3.Dot(transform.forward, toOpp);
            if (facingDot > 0.8f)
            {
                AddReward(0.0005f);
            }
        }
    }

    private void HandleCombatAction(int action)
    {
        switch (action)
        {
            case 0: // Idle / release block
                isBlocking = false;
                break;
            case 1: // Lead Jab
                isBlocking = false;
                ExecutePunch("LeadJab", jabDamage, 0.4f);
                break;
            case 2: // Jab Cross Combo
                isBlocking = false;
                ExecutePunch("JabCross", comboDamage, 0.7f);
                break;
            case 3: // Block / Guard
                isBlocking = true;
                break;
        }
    }

    private void ExecutePunch(string triggerName, float damage, float cooldown)
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + cooldown;

        if (anim != null)
        {
            anim.SetTrigger(triggerName);
        }

        if (targetOpponent == null) return;

        float distance = Vector3.Distance(transform.position, targetOpponent.position);
        Vector3 toOpponent = (targetOpponent.position - transform.position).normalized;
        float facingDot = Vector3.Dot(transform.forward, toOpponent);

        // Hit connects if within range and facing the opponent
        if (distance <= punchRange && facingDot > 0.6f)
        {
            AddReward(0.8f);

            if (opponentBoxerAgent != null)
            {
                opponentBoxerAgent.TakeHit(damage, this);
            }
        }
        else
        {
            // Whiff penalty
            AddReward(-0.05f);
        }
    }

    public void TakeHit(float incomingDamage, BoxerAgent attacker)
    {
        float actualDamage = isBlocking ? incomingDamage * 0.25f : incomingDamage;

        currentHealth -= actualDamage;

        if (isBlocking)
        {
            AddReward(0.15f); // Reward good defensive blocking
            if (attacker != null)
            {
                attacker.AddReward(-0.1f); // Attacker penalized for hitting guard
            }
        }
        else
        {
            AddReward(-0.4f); // Penalty for taking clean hit
            if (anim != null)
            {
                anim.SetTrigger("TakingPunch");
            }
        }

        if (currentHealth <= 0f)
        {
            // Knockout
            AddReward(-1.0f);
            if (attacker != null)
            {
                attacker.AddReward(1.5f);
                attacker.EndEpisode();
            }
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal"); // Strafe
        continuous[1] = Input.GetAxis("Vertical");   // Forward/back
        
        // Mouse or Q/E rotation
        float turn = 0f;
        if (Input.GetKey(KeyCode.Q)) turn -= 1f;
        if (Input.GetKey(KeyCode.E)) turn += 1f;
        continuous[2] = turn;

        var discrete = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.J))
        {
            discrete[0] = 1; // Lead Jab
        }
        else if (Input.GetKey(KeyCode.K))
        {
            discrete[0] = 2; // Jab Cross
        }
        else if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.B))
        {
            discrete[0] = 3; // Block
        }
        else
        {
            discrete[0] = 0; // None
        }
    }
}
