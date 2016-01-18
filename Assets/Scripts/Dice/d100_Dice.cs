using System.Collections.Generic;
using UnityEngine;

public class d100_Dice : BaseDice
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 10, Direction = new Vector3(0.45f, 0.65f, 0.61f)},
            new Normal {Value = 20, Direction = new Vector3(-0.72f, -0.65f, 0.23f)},
            new Normal {Value = 30, Direction = new Vector3(0f, 0.65f, 0.76f)},
            new Normal {Value = 40, Direction = new Vector3(0.45f, -0.65f, -0.61f)},
            new Normal {Value = 50, Direction = new Vector3(-0.44f, 0.65f, 0.62f)},
            new Normal {Value = 60, Direction = new Vector3(0f, -0.65f, 0.76f)},
            new Normal {Value = 70, Direction = new Vector3(0.72f, 0.65f, -0.24f)},
            new Normal {Value = 80, Direction = new Vector3(-0.45f, -0.65f, -0.61f)},
            new Normal {Value = 90, Direction = new Vector3(-0.73f, 0.65f, -0.22f)},
            new Normal {Value = 0, Direction = new Vector3(0.72f, -0.65f, 0.24f)},
        };
    }
}