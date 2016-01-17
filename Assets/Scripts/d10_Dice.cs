using System.Collections.Generic;
using UnityEngine;

public class d10_Dice : BaseDice
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0.45f, 0.65f, 0.61f)},
            new Normal {Value = 2, Direction = new Vector3(-0.72f, -0.65f, 0.23f)},
            new Normal {Value = 3, Direction = new Vector3(0f, 0.65f, 0.76f)},
            new Normal {Value = 4, Direction = new Vector3(0.45f, -0.65f, -0.61f)},
            new Normal {Value = 5, Direction = new Vector3(-0.44f, 0.65f, 0.62f)},
            new Normal {Value = 6, Direction = new Vector3(0f, -0.65f, 0.76f)},
            new Normal {Value = 7, Direction = new Vector3(0.72f, 0.65f, -0.24f)},
            new Normal {Value = 8, Direction = new Vector3(-0.45f, -0.65f, -0.61f)},
            new Normal {Value = 9, Direction = new Vector3(-0.73f, 0.65f, -0.22f)},
            new Normal {Value = 0, Direction = new Vector3(0.72f, -0.65f, 0.24f)},
        };
    }
}