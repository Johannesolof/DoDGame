using UnityEngine;

public class SpawnDice : MonoBehaviour
{

    public GameObject[] Dice;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);
            Debug.Log("This hit at " + hit.point);

            if (Dice.Length != 0)
            {
                var ins = (GameObject)Instantiate(Dice[Random.Range(0, Dice.Length)], ray.origin + ray.direction * 2, Quaternion.identity);
                var rg = ins.GetComponent<Rigidbody>();
                rg.velocity = ray.direction * 12;
                const float rotValue = 5f;
                rg.angularVelocity = new Vector3(Random.Range(-rotValue, rotValue), Random.Range(-rotValue, rotValue), Random.Range(-rotValue, rotValue));
            }
        }
    }
}
