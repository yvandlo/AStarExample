using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;

public class Node : MonoBehaviour
{
    private Dictionary<Vector3, Vector3> previousMap = new Dictionary<Vector3, Vector3>();
    // Priority Queue from https://github.com/FyiurAmron
    private PriorityQueue<(Vector3,float, Vector3), float> path = new PriorityQueue<(Vector3, float, Vector3), float>(); 
    private Dictionary<Vector3, float> reached = new Dictionary<Vector3, float>();
    private Rigidbody2D body;
    private Vector3 origin;
    private bool found = false;
    private List<Vector3> builtPath;
    private int traversalStep = 0;
    public GameObject toReach;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        origin = transform.position;
        path.Enqueue((origin, 0.0f, origin), 0.0f);
        Time.fixedDeltaTime = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        int steps = 10;
        if (!found) {
            Vector3 takeStep = oneStep();
            if (isDesired(takeStep)) {
                found = true;
                builtPath = buildList(takeStep);
                foreach (Vector3 vthree in builtPath) {
                    Debug.Log(vthree);
                }
                transform.position = origin;
            }
        } else {
            if (traversalStep == 0) {
                if (builtPath.Count == 0) {
                    body.velocity = new Vector2(0.0f, 0.0f);
                    return;
                }
                Vector2 next = convert(builtPath[0]);
                builtPath.RemoveAt(0);
                Vector2 current = convert(transform.position);
                body.velocity = (next - current) * (1.0f/(steps * Time.fixedDeltaTime));
                
            }
            traversalStep = (traversalStep + 1) % steps;
        }
        
    }

    private Vector3 oneStep() {
        while(true) {
            if (path.Count == 0) {
                return transform.position;
            }
            (Vector3 current, float distanceTo, Vector3 previous) = path.Dequeue();
            transform.position = current;
            if (!reached.ContainsKey(current)) {
                reached.Add(current, distanceTo);
                previousMap.Add(current, previous);
                foreach ((Vector3 succ, float relDistance) in succesors(current)) {
                    if (!reached.ContainsKey(succ)) {
                        float totalDistance = distanceTo + relDistance;
                        path.Enqueue((succ, totalDistance, current), totalDistance + heurestic(succ));
                    }
                }
                break;
            }
            return current;
        }
        return transform.position;
    }

    private List<(Vector3, float)> succesors(Vector3 from) {
        List<(Vector3, float)> toret = new List<(Vector3, float)>();
        Vector2 as2d = convert(from);
        (Vector2, float)[] directions = {(new Vector2(-1.0f, 0.0f), 1.0f), 
                                             (new Vector2(1.0f, 0.0f), 1.0f), 
                                             (new Vector2(0.0f, -1.0f), 1.0f),
                                             (new Vector2(0.0f, 1.0f), 1.0f)};
        foreach ((Vector2 succ, float relDistance) in directions) {
            if (inDir(as2d, succ)) {
                toret.Add((convert(as2d + succ), relDistance));
            }
        }
       
        return toret;
    }

    private Vector2 convert(Vector3 threed) {
        return new Vector2(threed.x, threed.y);
    }

    private Vector3 convert(Vector2 twod) {
        return new Vector3(twod.x, twod.y, 0.0f);
    }

    private bool isDesired(Vector3 totest) {
        Vector2 as2d = convert(totest);
        return hitGoal(castBox(as2d, new Vector2(0.0f, 0.0f)));
    }

    private RaycastHit2D castBox(Vector2 from, Vector2 direction) {
        //credit to https://gist.github.com/SolidAlloy
        return Physics2D.BoxCast(from, new Vector2(1.0f, 1.0f), 0.0f, direction, direction.magnitude);
    }

    private bool inDir(Vector2 from, Vector2 direction) {
        RaycastHit2D hit = castBox(from, direction);
        if (hit.transform == null) {
            return true;
        }
        return hitGoal(hit);
    }

    private bool hitGoal(RaycastHit2D hit) {
        if (hit.transform != null && hit.transform.CompareTag("Goal")) {
            return true;
            
        }
        return false;
    }

    float heurestic(Vector3 testPosition) {
       return Mathf.Abs(toReach.transform.position.x - testPosition.x) + Mathf.Abs(toReach.transform.position.y - testPosition.y) ;
    }

    private List<Vector3> buildList(Vector3 backfrom) {
        List<Vector3> built = new List<Vector3>();
        Vector3 current = backfrom;
        while(true) {
            if (current == previousMap[current]) {
                break;
            }
            built.Add(current);
            current = previousMap[current];
        }
        built.Reverse();
        return built;
    }
}
