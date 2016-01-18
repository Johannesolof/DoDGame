using System.Collections.Generic;
using UnityEngine;

public class d8_Dice : BaseDice
{

    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0.82f, 0.58f, 0f)},
            new Normal {Value = 2, Direction = new Vector3(-0.82f, 0.58f, 0f)},
            new Normal {Value = 3, Direction = new Vector3(0f, 0.58f, -0.82f)},
            new Normal {Value = 4, Direction = new Vector3(0f, 0.58f, 0.82f)},
            new Normal {Value = 5, Direction = new Vector3(0f, -0.58f, -0.82f)},
            new Normal {Value = 6, Direction = new Vector3(0f, -0.58f, 0.82f)},
            new Normal {Value = 7, Direction = new Vector3(0.82f, -0.58f, 0f)},
            new Normal {Value = 8, Direction = new Vector3(-0.82f, -0.58f, 0f)},
        };
    }
}