using System.Collections.Generic;
using UnityEngine;

public class d20_Dice : BaseDice
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0f, -1f, 0f)},
            new Normal {Value = 2, Direction = new Vector3(0.58f, 0.75f, 0.33f)},
            new Normal {Value = 3, Direction = new Vector3(-0.36f, -0.33f, -0.87f)},
            new Normal {Value = 4, Direction = new Vector3(-0.36f, 0.33f, 0.87f)},
            new Normal {Value = 5, Direction = new Vector3(0.58f, -0.33f, 0.75f)},
            new Normal {Value = 6, Direction = new Vector3(-0.93f, 0.33f, -0.13f)},
            new Normal {Value = 7, Direction = new Vector3(0.58f, -0.75f, -0.33f)},
            new Normal {Value = 8, Direction = new Vector3(0f, 0.74f, -0.67f)},
            new Normal {Value = 9, Direction = new Vector3(-0.93f, -0.33f, -0.13f)},
            new Normal {Value = 10, Direction = new Vector3(0.58f, 0.33f, -0.75f)},
            new Normal {Value = 11, Direction = new Vector3(-0.58f, -0.33f, 0.75f)},
            new Normal {Value = 12, Direction = new Vector3(0.93f, 0.33f, -0.13f)},
            new Normal {Value = 13, Direction = new Vector3(0f, -0.74f, 0.67f)},
            new Normal {Value = 14, Direction = new Vector3(-0.58f, 0.75f, 0.33f)},
            new Normal {Value = 15, Direction = new Vector3(0.93f, -0.33f, 0.13f)},
            new Normal {Value = 16, Direction = new Vector3(-0.58f, 0.33f, -0.75f)},
            new Normal {Value = 17, Direction = new Vector3(0.36f, -0.33f, -0.87f)},
            new Normal {Value = 18, Direction = new Vector3(0.36f, 0.33f, 0.87f)},
            new Normal {Value = 19, Direction = new Vector3(-0.58f, -0.75f, -0.33f)},
            new Normal {Value = 20, Direction = new Vector3(0f, 1f, 0f)},
        };
    }
}