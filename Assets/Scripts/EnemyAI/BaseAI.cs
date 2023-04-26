using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class BaseAI : MonoBehaviour
{
    [SerializeField] Perception playerPerception;
    [SerializeField] Perception allyPerception;

    [SerializeField] float aiRange = 0.0f;

    BAIStateMachine stateMachine = new BAIStateMachine();

    RefValue<float> distanceToPlayer = new RefValue<float>();
    RefValue<bool> isAgitated = new RefValue<bool>();
    RefValue<float> agentHealth = new RefValue<float>();

    public GameObject obsession;
    public NavMeshAgent navMeshAgent;

    public Animator animator;

    private void Start()
    {
        obsession = GameManager.Instance.playerLocations[Random.Range(0, GameManager.Instance.playerLocations.Count - 1)];
        navMeshAgent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();

        stateMachine.AddState(new AttackState(typeof(AttackState).Name, this));
        stateMachine.AddState(new IdleState(typeof(IdleState).Name, this));
        stateMachine.AddState(new MoveState(typeof(MoveState).Name, this));
        stateMachine.AddState(new WanderState(typeof(WanderState).Name, this));
        stateMachine.AddState(new DeathState(typeof(DeathState).Name, this));
        //stateMachine.AddState(new FleeState(typeof(FleeState).Name, this));

        stateMachine.AddTransition(typeof(IdleState).Name, new Transition(new Condition[]
            { new Condition<bool>(isAgitated, Predicate.Equal, true)}), typeof(MoveState).Name);
        stateMachine.AddTransition(typeof(IdleState).Name, new Transition(new Condition[]
            { new Condition<float>(distanceToPlayer, Predicate.LessOrEqual, aiRange)}), typeof(AttackState).Name);

        stateMachine.AddTransition(typeof(MoveState).Name, new Transition(new Condition[]
            { new Condition<float>(distanceToPlayer, Predicate.LessOrEqual, aiRange)}), typeof(AttackState).Name);

        stateMachine.AddTransition(typeof(AttackState).Name, new Transition(new Condition[]
            { new Condition<float>(distanceToPlayer, Predicate.Greater, aiRange)}), typeof(MoveState).Name);

        //All States to the Death State
        stateMachine.AddTransition(typeof(AttackState).Name, new Transition(
            new Condition<float>(agentHealth, Predicate.LessOrEqual, 0.0f)), typeof(DeathState).Name);
        stateMachine.AddTransition(typeof(IdleState).Name, new Transition(
            new Condition<float>(agentHealth, Predicate.LessOrEqual, 0.0f)), typeof(DeathState).Name);
        stateMachine.AddTransition(typeof(MoveState).Name, new Transition(
            new Condition<float>(agentHealth, Predicate.LessOrEqual, 0.0f)), typeof(DeathState).Name);
        stateMachine.AddTransition(typeof(WanderState).Name, new Transition(
            new Condition<float>(agentHealth, Predicate.LessOrEqual, 0.0f)), typeof(DeathState).Name);

        stateMachine.SetState(stateMachine.GetState(typeof(IdleState).Name));

        isAgitated.value = true;
    }

    private void Update()
    {
        distanceToPlayer.value = (obsession.transform.position - transform.position).magnitude;

        agentHealth.value = GetComponent<Health>().health;
        Debug.Log(agentHealth.value);

        stateMachine.OnUpdate();
    }
}
