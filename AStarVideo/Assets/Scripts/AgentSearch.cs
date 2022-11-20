using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class AgentSearch : MonoBehaviour
{
    private Dictionary<Vector3, Vector3> previousMap = new Dictionary<Vector3, Vector3>();
    private PriorityQueue<(Vector3, float, Vector3), float> frontier = new PriorityQueue<(Vector3, float, Vector3), float>();
    private Vector3 origin;

    private bool found = false;
    List<Vector3> builtPath;
    Rigidbody2D body;
    int currentStep = 0;

    public GameObject goal;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        origin = transform.position;
        frontier.Enqueue((origin, 0.0f, origin), 0.0f);
        Time.fixedDeltaTime = 0.05f;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        int steps = 40;
        if (!found) {
            Vector3 current  = 
            transform.position = oneStep();
            if (isTouchingGoal(current)) {
                found = true;
                builtPath = reconstructPath(current);
                transform.position = origin;
                Time.fixedDeltaTime = 0.01f;
            }
        } else {
            if (currentStep == 0) {
                if (builtPath.Count == 0) {
                    body.velocity = Vector2.zero;
                    return;
                } else {
                    Vector2 next = convert(builtPath[0]);
                    builtPath.RemoveAt(0);
                    Vector2 current = convert(transform.position);
                    body.velocity = (next - current) * (1.0f/(steps * Time.fixedDeltaTime));
                }
            }
            currentStep = (currentStep + 1) % steps;
        }
        
    }

    Vector3 oneStep() {
        while(true) {
            if (frontier.Count == 0) {
                return origin;
            }
            (Vector3 current, float distance, Vector3 previous) = frontier.Dequeue();
            if (!previousMap.ContainsKey(current)) {
                previousMap.Add(current, previous);
                foreach ((Vector3 successor, float relativeDistance) in successors(current)) {
                    if (!previousMap.ContainsKey(successor)) {
                        frontier.Enqueue((successor, distance + relativeDistance, current), 
                            distance + relativeDistance + heuristicOne(successor));
                    }
                }
                return current;
            }
        }
    }

    private List<(Vector3, float)> successors(Vector3 position) {
        (Vector2, float)[] directions = {(Vector2.up, 1.0f), (Vector2.left, 1.0f), (Vector2.right, 1.0f), (Vector2.down, 1.0f)};
        List<(Vector3, float)> legalDirections = new  List<(Vector3, float)>();
        foreach ((Vector2 successor, float relativeDistance) in directions) {
            if (validSuccesor(convert(position), successor)) {
                legalDirections.Add((position + convert(successor), relativeDistance));
            }
        }
        return legalDirections;
    }

    private bool isTouchingGoal(Vector3 position) {
        return hitGoal(castBox(convert(position), Vector2.zero));
    }

    private bool validSuccesor (Vector2 from, Vector2 dir) {
        RaycastHit2D testCollision = castBox(from, dir);
        if (testCollision.transform == null) {
            return true;
        } 
        return hitGoal(testCollision);
    }

    private RaycastHit2D castBox(Vector2 from, Vector2 dir) {
        return Physics2D.BoxCast(from, new Vector2(1.0f, 1.0f), 0.0f, dir, dir.magnitude);
    }

    private bool hitGoal(RaycastHit2D hit) {
        if (hit.transform != null && hit.transform.CompareTag("Goal")) {
            return true;
        }
        return false;
    }

    private Vector3 convert(Vector2 twodee) {
        return new Vector3(twodee.x, twodee.y, 0.0f);
    }

    private Vector2 convert(Vector3 threedee) {
        return new Vector2(threedee.x, threedee.y);
    }

    private List<Vector3> reconstructPath(Vector3 from) {
        Vector3 current = from;
        List<Vector3> path = new List<Vector3>();
        while (true) {
            if (current == previousMap[current]) {
                break;
            }
            path.Add(current);
            current = previousMap[current];
        }
        path.Reverse();
        return path;
    }

    private float heuristicOne(Vector3 compare) {
        return Mathf.Abs(goal.transform.position.x - compare.x) + Mathf.Abs(goal.transform.position.y - compare.y);
    }
}
