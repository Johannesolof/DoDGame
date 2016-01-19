using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnDice : MonoBehaviour
{

    public GameObject[] Dice;
    private List<BaseDice> _instances;

    // Use this for initialization
    void Start()
    {
        _instances = new List<BaseDice>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Dice.Length != 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

			var ins = (GameObject)Instantiate(Dice[Random.Range(0, Dice.Length)], ray.origin + ray.direction * 2, Random.rotationUniform);
            var rg = ins.GetComponent<Rigidbody>();
            rg.velocity = ray.direction * 12;
            const float rotValue = 5f;
            rg.angularVelocity = new Vector3(Random.Range(-rotValue, rotValue), Random.Range(-rotValue, rotValue), Random.Range(-rotValue, rotValue));
            _instances.Add(ins.GetComponent<BaseDice>());
        }
        if (Input.GetMouseButtonDown(1))
        {
            foreach (var instance in _instances)
            {
               instance.Destroy();
            }
            _instances.Clear();
        }
    }

    void OnGUI()
    {
        if (!_instances.Any())
            return;
        StringBuilder sb = new StringBuilder();
        var sum = 0;
        foreach (var instance in _instances)
        {
            if(instance.Closest.Value == -1)
                continue;
            sum += instance.Closest.Value;
            sb.Append(instance.Closest.Value);
            sb.Append(", ");
        }
        if (sb.Length < 1)
            return;
        sb.Remove(sb.Length - 2, 2);
        sb.Append(" | ").Append(sum);

        GUI.Label(new Rect(100, 50, 200, 100), sb.ToString());
    }
}
