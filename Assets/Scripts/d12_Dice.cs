using System.Collections.Generic;
using UnityEngine;

public class d12_Dice : BaseDice
{

    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0f, 1f, 0f)},
            new Normal {Value = 2, Direction = new Vector3(0f, 0.45f, 0.89f)},
            new Normal {Value = 3, Direction = new Vector3(0.85f, 0.45f, 0.28f)},
            new Normal {Value = 4, Direction = new Vector3(-0.85f, 0.45f, 0.28f)},
            new Normal {Value = 5, Direction = new Vector3(0.53f, 0.45f, -0.72f)},
            new Normal {Value = 6, Direction = new Vector3(-0.53f, 0.45f, -0.72f)},
            new Normal {Value = 7, Direction = new Vector3(0.53f, -0.45f, 0.72f)},
            new Normal {Value = 8, Direction = new Vector3(-0.53f, -0.45f, 0.72f)},
            new Normal {Value = 9, Direction = new Vector3(0.85f, -0.45f, -0.28f)},
            new Normal {Value = 10, Direction = new Vector3(-0.85f, -0.45f, -0.28f)},
            new Normal {Value = 2, Direction = new Vector3(0f, -0.45f, -0.89f)},
            new Normal {Value = 1, Direction = new Vector3(0f, -1f, 0f)},
        };
    }
}